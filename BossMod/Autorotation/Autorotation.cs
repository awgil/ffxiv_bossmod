using Dalamud.Hooking;
using ImGuiNET;
using System;
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
        private AutorotationConfig _config;
        private BossModuleManager _bossmods;
        private AutoHints _autoHints;
        private WindowManager.Window? _ui;
        private CommonActions? _classActions;

        private unsafe delegate bool UseActionDelegate(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* self, ActionType actionType, uint actionID, ulong targetID, uint itemLocation, uint callType, uint comboRouteID, bool* outOptGTModeStarted);
        private Hook<UseActionDelegate> _useActionHook;

        public AutorotationConfig Config => _config;
        public BossModuleManager Bossmods => _bossmods;
        public WorldState WorldState => _bossmods.WorldState;
        public CommonActions? ClassActions => _classActions;
        public float[] Cooldowns = new float[ActionManagerEx.NumCooldownGroups];

        public Actor? PrimaryTarget; // this is usually a normal (hard) target, but AI can override; typically used for damage abilities
        public Actor? SecondaryTarget; // this is usually a mouseover, but AI can override; typically used for heal and utility abilities
        public AIHints Hints = new();
        public float EffAnimLock => ActionManagerEx.Instance!.EffectiveAnimationLock;
        public float AnimLockDelay => ActionManagerEx.Instance!.EffectiveAnimationLockDelay;

        private static ActionID IDSprintGeneral = new(ActionType.General, 4);

        public unsafe Autorotation(BossModuleManager bossmods)
        {
            _config = Service.Config.Get<AutorotationConfig>();
            _bossmods = bossmods;
            _autoHints = new(bossmods.WorldState);

            ActionManagerEx.Instance!.ActionRequested += OnActionRequested;
            WorldState.Actors.CastEvent += OnCastEvent;

            var useActionAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 64 B1 01");
            _useActionHook = Hook<UseActionDelegate>.FromAddress(useActionAddress, UseActionDetour);
            _useActionHook.Enable();
        }

        public void Dispose()
        {
            ActionManagerEx.Instance!.ActionRequested -= OnActionRequested;
            WorldState.Actors.CastEvent -= OnCastEvent;

            _useActionHook.Dispose();
            _classActions?.Dispose();
            _autoHints.Dispose();
        }

        public void Update()
        {
            var player = WorldState.Party.Player();
            PrimaryTarget = WorldState.Actors.Find(player?.TargetID ?? 0);
            SecondaryTarget = WorldState.Actors.Find(Mouseover.Instance?.Object?.ObjectId ?? 0);

            Hints.Clear();
            if (player != null)
            {
                var playerAssignment = Service.Config.Get<PartyRolesConfig>()[WorldState.Party.ContentIDs[PartyState.PlayerSlot]];
                var activeModule = Bossmods.ActiveModule?.StateMachine.ActivePhase != null ? Bossmods.ActiveModule : null;
                Hints.FillPotentialTargets(WorldState, playerAssignment == PartyRolesConfig.Assignment.MT || playerAssignment == PartyRolesConfig.Assignment.OT && !WorldState.Party.WithoutSlot().Any(p => p != player && p.Role == Role.Tank));
                Hints.FillPlannedActions(Bossmods.ActiveModule, PartyState.PlayerSlot, player); // note that we might fill some actions even if module is not active yet (prepull)
                if (activeModule != null)
                    activeModule.CalculateAIHints(PartyState.PlayerSlot, player, playerAssignment, Hints);
                else
                    _autoHints.CalculateAIHints(Hints, player.Position);
            }
            Hints.Normalize();

            // TODO: this should be part of worldstate update for player
            ActionManagerEx.Instance!.GetCooldowns(Cooldowns);

            Type? classType = null;
            if (_config.Enabled && player != null)
            {
                classType = player.Class switch
                {
                    Class.WAR => typeof(WAR.Actions),
                    Class.PLD => Service.ClientState.LocalPlayer?.Level <= 60 ? typeof(PLD.Actions) : null,
                    Class.MNK => Service.ClientState.LocalPlayer?.Level <= 60 ? typeof(MNK.Actions) : null,
                    Class.DRG => typeof(DRG.Actions),
                    Class.BRD => typeof(BRD.Actions),
                    Class.BLM => Service.ClientState.LocalPlayer?.Level <= 60 ? typeof(BLM.Actions) : null,
                    Class.SMN => Service.ClientState.LocalPlayer?.Level <= 30 ? typeof(SMN.Actions) : null,
                    Class.WHM => typeof(WHM.Actions),
                    Class.SCH => Service.ClientState.LocalPlayer?.Level <= 60 ? typeof(SCH.Actions) : null,
                    _ => null
                };
            }

            if (_classActions?.GetType() != classType || _classActions?.Player != player)
            {
                _classActions?.Dispose();
                _classActions = classType != null ? (CommonActions?)Activator.CreateInstance(classType, this, player) : null;
            }

            _classActions?.Update();
            ActionManagerEx.Instance!.AutoQueue = _classActions?.CalculateNextAction() ?? default;

            bool showUI = _classActions != null && _config.ShowUI;
            if (showUI && _ui == null)
            {
                _ui = WindowManager.CreateWindow("Autorotation", DrawOverlay, () => { }, () => true);
                _ui.SizeHint = new(100, 100);
                _ui.MinSize = new(100, 100);
                _ui.Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            }
            else if (!showUI && _ui != null)
            {
                WindowManager.CloseWindow(_ui);
                _ui = null;
            }

            if (_config.ShowPositionals && PrimaryTarget != null && _classActions != null && _classActions.AutoAction != CommonActions.AutoActionNone && !PrimaryTarget.Omnidirectional)
            {
                var strategy = _classActions.GetStrategy();
                var color = PositionalColor(strategy);
                switch (strategy.NextPositional)
                {
                    case Positional.Flank:
                        Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation + 90.Degrees(), 45.Degrees(), color);
                        Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation - 90.Degrees(), 45.Degrees(), color);
                        break;
                    case Positional.Rear:
                        Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation + 180.Degrees(), 45.Degrees(), color);
                        break;
                }
            }
        }

        private void DrawOverlay()
        {
            if (_classActions == null)
                return;
            var next = ActionManagerEx.Instance!.AutoQueue;
            var state = _classActions.GetState();
            var strategy = _classActions.GetStrategy();
            ImGui.TextUnformatted($"[{_classActions.AutoAction}] Next: {next.Action} ({next.Source})");
            if (_classActions.AutoAction != CommonActions.AutoActionNone && strategy.NextPositional != Positional.Any)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, PositionalColor(strategy));
                ImGui.TextUnformatted(strategy.NextPositional.ToString());
                ImGui.PopStyleColor();
                ImGui.SameLine();
            }
            ImGui.TextUnformatted(strategy.ToString());
            ImGui.TextUnformatted($"Raidbuffs: {state.RaidBuffsLeft:f2}s left, next in {strategy.RaidBuffsIn:f2}s");
            ImGui.TextUnformatted($"Downtime: {strategy.FightEndIn:f2}s, pos-lock: {strategy.PositionLockIn:f2}");
            ImGui.TextUnformatted($"GCD={Cooldowns[CommonDefinitions.GCDGroup]:f3}, AnimLock={EffAnimLock:f3}+{AnimLockDelay:f3}");
        }

        private void OnActionRequested(object? sender, ClientActionRequest request)
        {
            _classActions?.NotifyActionExecuted(request);
        }

        private void OnCastEvent(object? sender, (Actor actor, ActorCastEvent cast) args)
        {
            if (args.cast.SourceSequence != 0 && args.actor == WorldState.Party.Player())
                _classActions?.NotifyActionSucceeded(args.cast);
        }

        private uint PositionalColor(CommonRotation.Strategy strategy)
        {
            return strategy.NextPositionalImminent
                ? (strategy.NextPositionalCorrect ? 0xff00ff00 : 0xff0000ff)
                : (strategy.NextPositionalCorrect ? 0xffffffff : 0xff00ffff);
        }

        // note: current implementation introduces slight input lag (on button press, next autorotation update will pick state updates, which will be executed on next action manager update)
        private unsafe bool UseActionDetour(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* self, ActionType actionType, uint actionID, ulong targetID, uint itemLocation, uint callType, uint comboRouteID, bool* outOptGTModeStarted)
        {
            // when spamming e.g. HS, every click (~0.2 sec) this function is called; aid=HS, a4=a5=a6=a7==0, returns True
            // 0.5s before CD end, action becomes queued (this function returns True); while anything is queued, further calls return False
            // callType is 0 for normal calls, 1 if called by queue mechanism, 2 if called from macro, 3 if combo (in such case comboRouteID is ActionComboRoute row id)
            // right when GCD ends, it is called internally by queue mechanism with aid=adjusted-id, a5=1, a4=a6=a7==0, returns True
            // itemLocation==0 for spells, 65535 for item used from hotbar, some value (bagID<<8 | slotID) for item used from inventory; it is the same as a4 in UseActionLocation
            //Service.Log($"UA: {action} @ {targetID:X}: {a4} {a5} {a6} {a7}");
            if (callType != 0 || _classActions == null)
            {
                // pass to hooked function transparently
                return _useActionHook.Original(self, actionType, actionID, targetID, itemLocation, callType, comboRouteID, outOptGTModeStarted);
            }

            var action = new ActionID(actionType, actionID);
            if (action == IDSprintGeneral)
                action = CommonDefinitions.IDSprint;
            bool nullTarget = targetID == 0 || targetID == Dalamud.Game.ClientState.Objects.Types.GameObject.InvalidGameObjectId;
            var target = nullTarget ? null : WorldState.Actors.Find(targetID);
            if (target == null && !nullTarget || !_classActions.HandleUserActionRequest(action, target))
            {
                // unknown target (e.g. quest object) or unsupported action - pass to hooked function
                return _useActionHook.Original(self, actionType, actionID, targetID, itemLocation, callType, comboRouteID, outOptGTModeStarted);
            }

            return false;
        }
    }
}
