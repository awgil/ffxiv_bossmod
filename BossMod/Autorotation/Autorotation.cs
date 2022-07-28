using Dalamud;
using Dalamud.Hooking;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // typically 'casting an action' causes the following sequence of events:
    // - immediately after sending ActionRequest message, client 'speculatively' starts CD (including GCD)
    // - ~50-100ms later client receives bundle (typically one, but sometimes messages can be spread over two frames!) with ActorControlSelf[Cooldown], ActorControl[Gain/LoseEffect], AbilityN, ActorGauge, StatusEffectList
    //   new statuses have large negative duration (e.g. -30 when ST is applied) - theory: it means 'show as X, don't reduce' - TODO test?..
    // - ~600ms later client receives EventResult with normal durations
    //
    // during this 'unconfirmed' window we might be considering wrong move to be the next-best one (e.g. imagine we've just started long IR cd and don't see the effect yet - next-best might be infuriate)
    // but I don't think this matters in practice, as presumably client forbids queueing any actions while there are pending requests
    // I don't know what happens if there is no confirmation for a long time (due to ping or packet loss)
    //
    // reject scenario:
    // a relatively easy way to repro it is doing no-movement rotation, then enabling moves when PR is up and 3 charges are up; next onslaught after PR seems to be often rejected
    // it seems that game will not send another request after reject until 500ms passed since prev request
    //
    // IMPORTANT: it seems that game uses *client-side* cooldown to determine when next request can happen, here's an example:
    // - 04:51.508: request Upheaval
    // - 04:51.635: confirm Upheaval (ACS[Cooldown] = 30s)
    // - 05:21.516: request Upheaval (30.008 since prev request, 29.881 since prev response)
    // - 05:21.609: confirm Upheaval (29.974 since prev response)
    //
    // here's a list of things we do now:
    // 1. we use cooldowns as reported by ActionManager API rather than parse network messages. This (1) allows us to not rely on randomized opcodes, (2) allows us not to handle things like CD resets on wipes, actor resets on zone changes, etc.
    // 2. we convert large negative status durations to their expected values
    // 3. when there are pending actions, we don't update internal state, leaving same next-best recommendation
    class Autorotation : IDisposable
    {
        private Network _network;
        private AutorotationConfig _config;
        private BossModuleManager _bossmods;
        private WindowManager.Window? _ui;
        private CommonActions? _classActions;

        private List<Network.PendingAction> _pendingActions = new();
        private ActorCastEvent? _completedCast = null;
        private DateTime _animLockEnd;
        private float _animLockDelay = 0.1f; // smoothed delay between client request and response
        private float _animLockDelaySmoothing = 0.8f; // TODO tweak

        private InputOverride _inputOverride;
        private DateTime _inputPendingUnblock;

        private delegate bool UseActionDelegate(ulong self, ActionType actionType, uint actionID, ulong targetID, uint a4, uint a5, uint a6, ulong a7);
        private Hook<UseActionDelegate> _useActionHook;
        private unsafe float* _comboTimeLeft = null;
        private unsafe uint* _comboLastMove = null;

        private IntPtr _gtQueuePatch;

        public AutorotationConfig Config => _config;
        public BossModuleManager Bossmods => _bossmods;
        public WorldState WorldState => _bossmods.WorldState;
        public CommonActions? ClassActions => _classActions;
        public List<Actor> PotentialTargets = new();
        public float AnimLock => MathF.Max((float)(_animLockEnd - DateTime.Now).TotalSeconds, 0);
        public float AnimLockDelay => _animLockDelay;
        public unsafe float ComboTimeLeft => *_comboTimeLeft;
        public unsafe uint ComboLastMove => *_comboLastMove;
        public bool DisableReplacement = false; // used when action selection is done by AI, so that replacement doesn't interfere

        public unsafe Autorotation(Network network, BossModuleManager bossmods, InputOverride inputOverride)
        {
            _network = network;
            _config = Service.Config.Get<AutorotationConfig>();
            _bossmods = bossmods;
            _inputOverride = inputOverride;

            _network.EventActionRequest += OnNetworkActionRequest;
            _network.EventActionRequestGT += OnNetworkActionRequest;
            _network.EventActorCast += OnNetworkActionCastStart;
            _network.EventActionEffect += OnNetworkActionEffect;
            _network.EventActorControlCancelCast += OnNetworkActionCancel;
            _network.EventActorControlSelfActionRejected += OnNetworkActionReject;

            IntPtr comboPtr = Service.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 80 7E 21 00", 0x19E);
            _comboTimeLeft = (float*)comboPtr;
            _comboLastMove = (uint*)(comboPtr + 0x4);

            var useActionAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 64 B1 01");
            _useActionHook = Hook<UseActionDelegate>.FromAddress(useActionAddress, UseActionDetour);
            _useActionHook.Enable();

            _gtQueuePatch = Service.SigScanner.ScanModule("74 24 41 81 FE F5 0D 00 00");
            SafeMemory.WriteBytes(_gtQueuePatch, new byte[] { 0xEB });
        }

        public void Dispose()
        {
            _network.EventActionRequest -= OnNetworkActionRequest;
            _network.EventActionRequestGT -= OnNetworkActionRequest;
            _network.EventActorCast -= OnNetworkActionCastStart;
            _network.EventActionEffect -= OnNetworkActionEffect;
            _network.EventActorControlCancelCast -= OnNetworkActionCancel;
            _network.EventActorControlSelfActionRejected -= OnNetworkActionReject;

            _useActionHook.Dispose();
            SafeMemory.WriteBytes(_gtQueuePatch, new byte[] { 0x74 });
        }

        public void UpdatePotentialTargets()
        {
            PotentialTargets.Clear();
            PotentialTargets.AddRange(WorldState.Actors.Where(a => a.Type == ActorType.Enemy && a.IsTargetable && !a.IsAlly && !a.IsDead && a.InCombat));
        }

        public void Update(Actor? target)
        {
            var player = WorldState.Party.Player();
            Type? classType = null;
            if (_config.Enabled && player != null)
            {
                classType = player.Class switch
                {
                    Class.WAR => typeof(WARActions),
                    Class.GLA or Class.PLD => Service.ClientState.LocalPlayer?.Level < 40 ? typeof(PLDActions) : null,
                    Class.CNJ or Class.WHM => typeof(WHMActions),
                    Class.PGL or Class.MNK => typeof(MNKActions),
                    Class.LNC or Class.DRG => Service.ClientState.LocalPlayer?.Level < 40 ? typeof(DRGActions) : null,
                    Class.BRD or Class.ARC => Service.ClientState.LocalPlayer?.Level < 40 ? typeof(BRDActions) : null,
                    Class.THM or Class.BLM => Service.ClientState.LocalPlayer?.Level < 40 ? typeof(BLMActions) : null,
                    Class.ACN or Class.SMN => Service.ClientState.LocalPlayer?.Level < 40 ? typeof(SMNActions) : null,
                    _ => null
                };
            }

            if (_classActions?.GetType() != classType || _classActions?.Player != player)
            {
                _classActions = classType != null ? (CommonActions?)Activator.CreateInstance(classType, this, player) : null;
            }

            if (_classActions != null)
            {
                if (_completedCast != null)
                {
                    _classActions.CastSucceeded(_completedCast);
                }
                else if (_pendingActions.Count == 0)
                {
                    bool moving = _inputOverride.IsMoving(); // TODO: reconsider
                    _classActions.Update(target, moving);
                }
            }

            _completedCast = null;

            // unblock 'speculative' movement locks
            if (_inputPendingUnblock != new DateTime() && _inputPendingUnblock < DateTime.Now)
            {
                _inputOverride.UnblockMovement();
                _inputPendingUnblock = new();
            }

            bool showUI = _classActions != null && _config.ShowUI;
            if (showUI && _ui == null)
            {
                _ui = WindowManager.CreateWindow("Autorotation", () => _classActions?.DrawOverlay(), () => { }, () => true);
                _ui.SizeHint = new(100, 100);
                _ui.MinSize = new(100, 100);
                _ui.Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            }
            else if (!showUI && _ui != null)
            {
                WindowManager.CloseWindow(_ui);
                _ui = null;
            }
        }

        public IEnumerable<Actor> PotentialTargetsInRange(WPos center, float radius)
        {
            return PotentialTargets.Where(a => (a.Position - center).LengthSq() <= (a.HitboxRadius + radius) * (a.HitboxRadius + radius));
        }

        public IEnumerable<Actor> PotentialTargetsInRangeFromPlayer(float radius)
        {
            var player = WorldState.Party.Player();
            return player != null ? PotentialTargetsInRange(player.Position, radius) : Enumerable.Empty<Actor>();
        }

        private void OnNetworkActionRequest(object? sender, Network.PendingAction action)
        {
            if (_pendingActions.Count > 0)
            {
                Log($"New action request ({PendingActionString(action)}) while {_pendingActions.Count} are pending (first = {PendingActionString(_pendingActions[0])})", true);
            }
            Log($"++ {PendingActionString(action)}");
            _pendingActions.Add(action);
            _animLockEnd = DateTime.Now.AddSeconds(0.5); // note: for casted spells, it is actually 0.1 after cast end - we update it if we receive cast-start message

            if (_inputPendingUnblock > DateTime.Now)
            {
                // we have speculative movement block; if we've actually requested casted action, extend it until cast ends, otherwise cancel it
                if (!action.Action.IsCasted())
                    _inputOverride.UnblockMovement();
                _inputPendingUnblock = new();
            }
        }

        private void OnNetworkActionCastStart(object? sender, (ulong actorID, ActionID action, float castTime, ulong targetID) args)
        {
            if (args.actorID != WorldState.Party.Player()?.InstanceID)
                return; // not a player cast

            // update animation lock end for casted spells: it is 0.1 after cast end instead of 0.5 after request
            _animLockEnd = _animLockEnd.AddSeconds(args.castTime - 0.4f);
        }

        private void OnNetworkActionEffect(object? sender, (ulong actorID, ActorCastEvent cast) args)
        {
            if (args.cast.SourceSequence == 0 || args.actorID != WorldState.Party.Player()?.InstanceID)
                return; // non-player-initiated

            var pa = new Network.PendingAction() { Action = args.cast.Action, TargetID = args.cast.MainTargetID, Sequence = args.cast.SourceSequence };
            int index = _pendingActions.FindIndex(a => a.Sequence == args.cast.SourceSequence);
            if (index == -1)
            {
                Log($"Unexpected action-effect ({PendingActionString(pa)}): currently {_pendingActions.Count} are pending", true);
                _pendingActions.Clear();
            }
            else if (index > 0)
            {
                Log($"Unexpected action-effect ({PendingActionString(pa)}): index={index}, first={PendingActionString(_pendingActions[0])}, count={_pendingActions.Count}", true);
            }
            else if (_pendingActions[0].Action != args.cast.Action)
            {
                Log($"Request/response action mismatch: requested {PendingActionString(_pendingActions[0])}, got {PendingActionString(pa)}", true);
            }
            _pendingActions.RemoveRange(0, index + 1);
            Log($"-+ {PendingActionString(pa)}, lock={args.cast.AnimationLockTime:f3}");
            _completedCast = args.cast;

            var now = DateTime.Now;
            var delay = (float)(now.AddSeconds(0.5) - _animLockEnd).TotalSeconds; // TODO: this isn't correct for casted spells...
            _animLockDelay = delay * (1 - _animLockDelaySmoothing) + _animLockDelay * _animLockDelaySmoothing;
            _animLockEnd = now.AddSeconds(args.cast.AnimationLockTime);

            // unblock input unconditionally on successful cast (I assume there are no instances where we need to immediately start next GCD?)
            _inputOverride.UnblockMovement();
        }

        private void OnNetworkActionCancel(object? sender, (ulong actorID, uint actionID) args)
        {
            if (args.actorID != WorldState.Party.Player()?.InstanceID)
                return; // non-player-initiated

            int index = _pendingActions.FindIndex(a => a.Action.ID == args.actionID);
            if (index == -1)
            {
                Log($"Unexpected action-cancel ({args.actionID}): currently {_pendingActions.Count} are pending", true);
                _pendingActions.Clear();
            }
            else
            {
                if (index > 0)
                {
                    Log($"Unexpected action-cancel ({PendingActionString(_pendingActions[index])}): index={index}, first={PendingActionString(_pendingActions[0])}, count={_pendingActions.Count}", true);
                }
                Log($"-- {PendingActionString(_pendingActions[index])}");
                _pendingActions.RemoveRange(0, index + 1);
            }

            // clear animation lock (TODO: or should it be set to 0.1? I think if you spam cast while running, next cast will start <0.1s after interrupt...)
            _animLockEnd = new();

            // keep movement locked for a slight duration after interrupted cast, in case player restarts it
            _inputPendingUnblock = DateTime.Now.AddSeconds(0.2f);
        }

        private void OnNetworkActionReject(object? sender, (ulong actorID, uint actionID, uint sourceSequence) args)
        {
            int index = args.sourceSequence != 0
                ? _pendingActions.FindIndex(a => a.Sequence == args.sourceSequence)
                : _pendingActions.FindIndex(a => a.Action.ID == args.actionID);
            if (index == -1)
            {
                Log($"Unexpected action-reject (#{args.sourceSequence} '{args.actionID}'): currently {_pendingActions.Count} are pending", true);
                _pendingActions.Clear();
            }
            else
            {
                if (index > 0)
                {
                    Log($"Unexpected action-reject ({PendingActionString(_pendingActions[index])}): index={index}, first={PendingActionString(_pendingActions[0])}, count={_pendingActions.Count}", true);
                }
                if (_pendingActions[index].Action.ID != args.actionID)
                {
                    Log($"Request/reject action mismatch: requested {PendingActionString(_pendingActions[index])}, got {args.actionID}", true);
                }
                Log($"!! {PendingActionString(_pendingActions[index])}");
                _pendingActions.RemoveRange(0, index + 1);
            }

            // TODO: should we clear animation lock here?..

            // unblock input unconditionally on reject (TODO: investigate more why it can happen)
            _inputOverride.UnblockMovement();
        }

        private string PendingActionString(Network.PendingAction a)
        {
            return $"#{a.Sequence} {a.Action} @ {Utils.ObjectString(a.TargetID)}";
        }

        private void Log(string message, bool warning = false)
        {
            if (warning || _config.Logging)
                Service.Log($"[AR] {message}");
        }

        private unsafe bool UseActionDetour(ulong self, ActionType actionType, uint actionID, ulong targetID, uint a4, uint a5, uint a6, ulong a7)
        {
            // when spamming e.g. HS, every click (~0.2 sec) this function is called; aid=HS, a4=a5=a6=a7==0, returns True
            // ~0.3s before GCD/animlock end, it starts returning False - probably meaning "next action is already queued"?
            // right when GCD ends, it is called internally (by queue mechanism I assume) with aid=adjusted-id, a5=1, a4=a6=a7==0, returns True
            // a5==1 means "forced"?
            // a4==0 for spells, 65535 for item used from hotbar, some value (e.g. 6) for item used from inventory; it is the same as a4 in UseActionLocation
            var action = new ActionID(actionType, actionID);
            //Service.Log($"UA: {action} @ {targetID:X}: {a4} {a5} {a6} {a7}");
            if (_classActions != null && !DisableReplacement)
            {
                (action, targetID) = _classActions.ReplaceActionAndTarget(action, targetID, a5 != 0);
                if (a4 == 0 && action.Type == ActionType.Item)
                    a4 = 65535;
            }

            // if we're spamming casted action, we have to block movement a bit before cast starts, otherwise it would be interrupted
            // if we block movement now, we might or might not get actual action request when GCD ends; if we do, we'll extend lock until cast ends, otherwise (e.g. if we got out of range) we'll remove lock after slight delay
            if (_config.PreventMovingWhileCasting && !_inputOverride.IsBlocked() && action.IsCasted())
            {
                var gcd = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetRecastGroupDetail(57);
                var gcdLeft = gcd->Total - gcd->Elapsed;
                if (gcdLeft < 0.3f)
                {
                    _inputOverride.BlockMovement();
                    _inputPendingUnblock = DateTime.Now.AddSeconds(gcdLeft + 0.1f);
                }
            }

            bool ret = _useActionHook.Original(self, action.Type, action.ID, targetID, a4, a5, a6, a7);

            if (_config.GTMode != AutorotationConfig.GroundTargetingMode.Manual && action.IsGroundTargeted())
            {
                // hack to cast ground-targeted immediately
                var am = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
                if (_config.GTMode == AutorotationConfig.GroundTargetingMode.AtTarget)
                    *(ulong*)((IntPtr)am + 0x98) = targetID;
                *(byte*)((IntPtr)am + 0xB8) = 1;
            }

            return ret;
        }
    }
}
