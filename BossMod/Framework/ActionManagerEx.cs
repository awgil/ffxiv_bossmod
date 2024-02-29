using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BossMod
{
    // extensions and utilities for interacting with game's ActionManager singleton
    // handles following features:
    // 1. automatic action execution (provided by autorotation or ai modules, if enabled); does nothing if no automatic actions are provided
    // 2. effective animation lock reduction (a-la xivalex)
    //    game handles instants and casted actions differently:
    //    * instants: on action request (e.g. on the frame the action button is pressed), animation lock is set to 0.5 (or 0.35 for some specific actions); it then ticks down every frame
    //      some time later (ping + server latency, typically 50-100ms if ping is good), we receive action effect packet - the packet contains action's animation lock (typically 0.6)
    //      the game then updates animation lock (now equal to 0.5 minus time since request) to the packet data
    //      so the 'effective' animation lock between action request and animation lock end is equal to action's animation lock + delay between request and response
    //      this feature reduces effective animation lock by either removing extra delay completely or clamping it to specified maximal value
    //    * casts: on action request animation lock is not set (remains equal to 0), remaining cast time is set to action's cast time; remaining cast time then ticks down every frame
    //      some time later (cast time minus approximately 0.5s, aka slidecast window), we receive action effect packet - the packet contains action's animation lock (typically 0.1)
    //      the game then updates animation lock (still 0) to the packet data - however, since animation lock isn't ticking down while cast is in progress, there is no extra delay
    //      this feature does nothing for casts, since they already work correctly
    // 3. framerate-dependent cooldown reduction
    //    imagine game is running at exactly 100fps (10ms frame time), and action is queued when remaining cooldown is 5ms
    //    on next frame (+10ms), cooldown will be reduced and clamped to 0, action will be executed and it's cooldown set to X ms - so next time it can be pressed at X+10 ms
    //    if we were running with infinite fps, cooldown would be reduced to 0 and action would be executed slightly (5ms) earlier
    //    we can't fix that easily, but at least we can fix the cooldown after action execution - so that next time it can be pressed at X+5ms
    //    we do that by reducing actual cooldown by difference between previously-remaining cooldown and frame delta, if action is executed at first opportunity
    // 4. slidecast assistant aka movement block
    //    cast is interrupted if player moves when remaining cast time is greater than ~0.5s (moving during that window without interrupting is known as slidecasting)
    //    this feature blocks WSAD input to prevent movement while this would interrupt a cast, allowing slidecasting efficiently while just holding movement button
    //    other ways of moving (eg LMB+RMB, jumping etc) are not blocked, allowing for emergency movement even while the feature is active
    //    movement is blocked a bit before cast start and unblocked as soon as action effect packet is received
    // 5. preserving character facing direction
    //    when any action is executed, character is automatically rotated to face the target (this can be disabled in-game, but it would simply block an action if not facing target instead)
    //    this makes maintaining uptime during gaze mechanics unnecessarily complicated (requiring either moving or rotating mouse back-and-forth in non-legacy camera mode)
    //    this feature remembers original rotation before executing an action and then attempts to restore it
    //    just like any 'manual' way, it is not 100% reliable:
    //    * client rate-limits rotation updates, so even for instant casts there is a short window of time (~0.1s) following action execution when character faces a target on server
    //    * for movement-affecting abilities (jumps, charges, etc) rotation can't be restored until animation ends
    //    * for casted abilities, rotation isn't restored until slidecast window starts, as otherwise cast is interrupted
    // 6. ground-targeted action queueing
    //    ground-targeted actions can't be queued, making using them efficiently tricky
    //    this feature allows queueing them, plus provides options to execute them automatically either at target's position or at cursor's position
    unsafe class ActionManagerEx : IDisposable
    {
        public static ActionManagerEx? Instance;
        public const int NumCooldownGroups = 80;

        public float AnimationLockDelaySmoothing = 0.8f; // TODO tweak
        public float AnimationLockDelayAverage { get; private set; } = 0.1f; // smoothed delay between client request and server response
        public float AnimationLockDelayMax => Config.RemoveAnimationLockDelay ? 0 : float.MaxValue; // this caps max delay a-la xivalexander (TODO: make tweakable?)
        public float AnimationLock => Utils.ReadField<float>(_inst, 8);

        public uint CastSpellID => Utils.ReadField<uint>(_inst, 0x24);
        public ActionID CastSpell => new(ActionType.Spell, CastActionID);
        public ActionType CastActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x28);
        public uint CastActionID => Utils.ReadField<uint>(_inst, 0x2C);
        public ActionID CastAction => new(CastActionType, CastActionID);
        public float CastTimeElapsed => Utils.ReadField<float>(_inst, 0x30);
        public float CastTimeTotal => Utils.ReadField<float>(_inst, 0x34);
        public float CastTimeRemaining => CastSpellID != 0 ? CastTimeTotal - CastTimeElapsed : 0;
        public ulong CastTargetID => Utils.ReadField<ulong>(_inst, 0x38);
        public Vector3 CastTargetPos => Utils.ReadField<Vector3>(_inst, 0x40);

        public float ComboTimeLeft => Utils.ReadField<float>(_inst, 0x60);
        public uint ComboLastMove => Utils.ReadField<uint>(_inst, 0x64);

        public bool QueueActive => Utils.ReadField<bool>(_inst, 0x68);
        public ActionType QueueActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x6C);
        public uint QueueActionID => Utils.ReadField<uint>(_inst, 0x70);
        public ActionID QueueAction => new(QueueActionType, QueueActionID);
        public ulong QueueTargetID => Utils.ReadField<ulong>(_inst, 0x78);
        public uint QueueCallType => Utils.ReadField<uint>(_inst, 0x80);
        public uint QueueComboRouteID => Utils.ReadField<uint>(_inst, 0x84);

        public uint GTActionID => Utils.ReadField<uint>(_inst, 0x88);
        public ActionType GTActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x8C);
        public ActionID GTAction => new(GTActionType, GTActionID);
        public uint GTSpellID => Utils.ReadField<uint>(_inst, 0x90);
        public ActionID GTSpell => new(ActionType.Spell, GTSpellID);
        public uint GTUnkArg => Utils.ReadField<uint>(_inst, 0x94);
        public ulong GTUnkObj => Utils.ReadField<ulong>(_inst, 0x98);
        public byte GT_uA0 => Utils.ReadField<byte>(_inst, 0xA0);
        public byte GT_uB8 => Utils.ReadField<byte>(_inst, 0xB8);
        public uint GT_uBC => Utils.ReadField<byte>(_inst, 0xBC);

        public ushort LastUsedActionSequence => Utils.ReadField<ushort>(_inst, 0x110);

        public float EffectiveAnimationLock => AnimationLock + CastTimeRemaining; // animation lock starts ticking down only when cast ends
        public float EffectiveAnimationLockDelay => AnimationLockDelayMax <= 0.5f ? AnimationLockDelayMax : MathF.Min(AnimationLockDelayAverage, 0.1f); // this is a conservative estimate

        public event Action<ClientActionRequest>? ActionRequested;

        public delegate void ActionEffectReceivedDelegate(ulong sourceID, ActorCastEvent info);
        public event ActionEffectReceivedDelegate? ActionEffectReceived;

        public delegate void EffectResultReceivedDelegate(ulong targetID, uint seq, int targetIndex);
        public event EffectResultReceivedDelegate? EffectResultReceived;

        public InputOverride InputOverride;
        public ActionManagerConfig Config;
        public CommonActions.NextAction AutoQueue; // TODO: consider using native 'queue' fields for this?
        public bool MoveMightInterruptCast { get; private set; } // if true, moving now might cause cast interruption (for current or queued cast)
        private ActionManager* _inst;
        private float _lastReqInitialAnimLock;
        private ushort _lastReqSequence;
        private float _useActionInPast; // if >0 while using an action, cooldown/anim lock will be reduced by this amount as if action was used a bit in the past
        private (Angle pre, Angle post)? _restoreRotation; // if not null, we'll try restoring rotation to pre while it is equal to post
        private int _restoreCntr;

        private delegate bool GetGroundTargetPositionDelegate(ActionManager* self, Vector3* outPos);
        private GetGroundTargetPositionDelegate _getGroundTargetPositionFunc;

        private delegate void FaceTargetDelegate(ActionManager* self, Vector3* position, ulong targetID);
        private FaceTargetDelegate _faceTargetFunc;

        private delegate void UpdateDelegate(ActionManager* self);
        private Hook<UpdateDelegate> _updateHook;

        private delegate bool UseActionLocationDelegate(ActionManager* self, ActionType actionType, uint actionID, ulong targetID, Vector3* targetPos, uint itemLocation);
        private Hook<UseActionLocationDelegate> _useActionLocationHook;

        private delegate void ProcessPacketActionEffectDelegate(uint casterID, FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* casterObj, Vector3* targetPos, Network.ServerIPC.ActionEffectHeader* header, ulong* effects, ulong* targets);
        private Hook<ProcessPacketActionEffectDelegate> _processPacketActionEffectHook;

        private delegate void ProcessPacketEffectResultDelegate(uint targetID, byte* packet, byte replaying);
        private Hook<ProcessPacketEffectResultDelegate> _processPacketEffectResultHook;
        private Hook<ProcessPacketEffectResultDelegate> _processPacketEffectResultBasicHook;

        // it's a static function of StatusManager really
        private delegate bool CancelStatusDelegate(uint statusId, uint sourceId);
        private CancelStatusDelegate _cancelStatusFunc;

        public ActionManagerEx()
        {
            InputOverride = new();
            Config = Service.Config.Get<ActionManagerConfig>();

            _inst = ActionManager.Instance();
            Service.Log($"[AMEx] ActionManager singleton address = 0x{(ulong)_inst:X}");

            var getGroundTargetPositionAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 44 8B 84 24 80 00 00 00 33 C0");
            _getGroundTargetPositionFunc = Marshal.GetDelegateForFunctionPointer<GetGroundTargetPositionDelegate>(getGroundTargetPositionAddress);
            Service.Log($"[AMEx] GetGroundTargetPosition address = 0x{getGroundTargetPositionAddress:X}");

            var faceTargetAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 81 FE FB 1C 00 00 74 ?? 81 FE 53 5F 00 00 74 ?? 81 FE 6F 73 00 00");
            _faceTargetFunc = Marshal.GetDelegateForFunctionPointer<FaceTargetDelegate>(faceTargetAddress);
            Service.Log($"[AMEx] FaceTarget address = 0x{faceTargetAddress:X}");

            _updateHook = Service.Hook.HookFromSignature<UpdateDelegate>("48 8B C4 48 89 58 20 57 48 81 EC", UpdateDetour);
            _updateHook.Enable();
            Service.Log($"[AMEx] Update address = 0x{_updateHook.Address:X}");

            _useActionLocationHook = Service.Hook.HookFromSignature<UseActionLocationDelegate>("E8 ?? ?? ?? ?? 3C 01 0F 85 ?? ?? ?? ?? EB 46", UseActionLocationDetour);
            _useActionLocationHook.Enable();
            Service.Log($"[AMEx] UseActionLocation address = 0x{_useActionLocationHook.Address:X}");

            _processPacketActionEffectHook = Service.Hook.HookFromSignature<ProcessPacketActionEffectDelegate>("E8 ?? ?? ?? ?? 48 8B 4C 24 68 48 33 CC E8 ?? ?? ?? ?? 4C 8D 5C 24 70 49 8B 5B 20 49 8B 73 28 49 8B E3 5F C3", ProcessPacketActionEffectDetour);
            _processPacketActionEffectHook.Enable();
            Service.Log($"[AMEx] ProcessPacketActionEffect address = 0x{_processPacketActionEffectHook.Address:X}");

            _processPacketEffectResultHook = Service.Hook.HookFromSignature<ProcessPacketEffectResultDelegate>("48 8B C4 44 88 40 18 89 48 08", ProcessPacketEffectResultDetour);
            _processPacketEffectResultHook.Enable();
            Service.Log($"[AMEx] ProcessPacketEffectResult address = 0x{_processPacketEffectResultHook.Address:X}");

            _processPacketEffectResultBasicHook = Service.Hook.HookFromSignature<ProcessPacketEffectResultDelegate>("40 53 41 54 41 55 48 83 EC 40", ProcessPacketEffectResultBasicDetour);
            _processPacketEffectResultBasicHook.Enable();
            Service.Log($"[AMEx] ProcessPacketEffectResultBasic address = 0x{_processPacketEffectResultBasicHook.Address:X}");

            var cancelStatusAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 84 C0 75 2C 48 8B 07");
            _cancelStatusFunc = Marshal.GetDelegateForFunctionPointer<CancelStatusDelegate>(cancelStatusAddress);
            Service.Log($"[AMEx] CancelStatus address = 0x{cancelStatusAddress:X}");
        }

        public void Dispose()
        {
            _processPacketEffectResultBasicHook.Dispose();
            _processPacketEffectResultHook.Dispose();
            _processPacketActionEffectHook.Dispose();
            _useActionLocationHook.Dispose();
            _updateHook.Dispose();
            InputOverride.Dispose();
        }

        public Vector3? GetWorldPosUnderCursor()
        {
            Vector3 res = new();
            return _getGroundTargetPositionFunc(_inst, &res) ? res : null;
        }

        public void FaceTarget(Vector3 position, ulong unkObjID = GameObject.InvalidGameObjectId)
        {
            _faceTargetFunc(_inst, &position, unkObjID);
        }
        public void FaceDirection(WDir direction)
        {
            var player = Service.ClientState.LocalPlayer;
            if (player != null)
                FaceTarget(player.Position + new Vector3(direction.X, 0, direction.Z));
        }

        public void GetCooldowns(float[] cooldowns)
        {
            var rg = _inst->GetRecastGroupDetail(0);
            for (int i = 0; i < NumCooldownGroups; ++i)
            {
                cooldowns[i] = rg->Total - rg->Elapsed;
                ++rg;
            }
        }

        public float GCD()
        {
            var gcd = _inst->GetRecastGroupDetail(CommonDefinitions.GCDGroup);
            return gcd->Total - gcd->Elapsed;
        }

        public uint GetAdjustedActionID(uint actionID) => _inst->GetAdjustedActionId(actionID);

        public uint GetActionStatus(ActionID action, ulong target, bool checkRecastActive = true, bool checkCastingActive = true, uint* outOptExtraInfo = null)
        {
            return _inst->GetActionStatus((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, target, checkRecastActive, checkCastingActive, outOptExtraInfo);
        }

        // returns time in ms
        public int GetAdjustedCastTime(ActionID action, bool skipHasteAdjustment = true, byte* outOptProcState = null)
            => ActionManager.GetAdjustedCastTime((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, (byte)(skipHasteAdjustment ? 1 : 0), outOptProcState);

        public bool IsRecastTimerActive(ActionID action)
            => _inst->IsRecastTimerActive((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID);

        public int GetRecastGroup(ActionID action)
            => _inst->GetRecastGroup((int)action.Type, action.ID);

        public bool UseAction(ActionID action, ulong targetID, uint itemLocation, uint callType, uint comboRouteID, bool* outOptGTModeStarted)
        {
            return _inst->UseAction((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, targetID, itemLocation, callType, comboRouteID, outOptGTModeStarted);
        }

        // skips queueing etc
        public bool UseActionRaw(ActionID action, ulong targetID = GameObject.InvalidGameObjectId, Vector3 targetPos = new(), uint itemLocation = 0)
        {
            return UseActionLocationDetour(_inst, action.Type, action.ID, targetID, &targetPos, itemLocation);
        }

        // does all the sanity checks (that status is on actor, is a buff that can be canceled, etc.)
        // on success, the status manager is updated immediately, meaning that no rate limiting is needed
        // if sourceId is not specified, removes first status with matching id
        public bool CancelStatus(uint statusId, uint sourceId = GameObject.InvalidGameObjectId)
        {
            var res = _cancelStatusFunc(statusId, sourceId);
            Service.Log($"[AMEx] Canceling status {statusId} from {sourceId:X} -> {res}");
            return res;
        }

        private void UpdateDetour(ActionManager* self)
        {
            var dt = Framework.Instance()->FrameDeltaTime;
            var imminentAction = QueueActive ? QueueAction : AutoQueue.Action;
            var imminentActionAdj = imminentAction.Type == ActionType.Spell ? new(ActionType.Spell, GetAdjustedActionID(imminentAction.ID)) : imminentAction;
            var imminentRecast = imminentActionAdj ? _inst->GetRecastGroupDetail(GetRecastGroup(imminentActionAdj)) : null;
            if (imminentRecast != null && Config.RemoveCooldownDelay)
            {
                var cooldownOverflow = imminentRecast->IsActive != 0 ? imminentRecast->Elapsed + dt - imminentRecast->Total : dt;
                var animlockOverflow = dt - AnimationLock;
                _useActionInPast = Math.Min(cooldownOverflow, animlockOverflow);
                if (_useActionInPast >= dt)
                    _useActionInPast = 0; // nothing prevented us from casting it before, so do not adjust anything...
                else if (_useActionInPast > 0.1f)
                    _useActionInPast = 0.1f; // upper limit for time adjustment
            }

            _updateHook.Original(self);

            // check whether movement is safe; block movement if not and if desired
            MoveMightInterruptCast &= CastTimeRemaining > 0; // previous cast could have ended without action effect
            MoveMightInterruptCast |= imminentActionAdj && CastTimeRemaining <= 0 && AnimationLock < 0.1f && GetAdjustedCastTime(imminentActionAdj) > 0 && GCD() < 0.1f; // if we're not casting, but will start soon, moving might interrupt future cast
            bool blockMovement = Config.PreventMovingWhileCasting && MoveMightInterruptCast;

            // restore rotation logic; note that movement abilities (like charge) can take multiple frames until they allow changing facing
            if (_restoreRotation != null && !MoveMightInterruptCast)
            {
                var curRot = (Service.ClientState.LocalPlayer?.Rotation ?? 0).Radians();
                //Service.Log($"[AMEx] Restore rotation: {curRot.Rad}: {_restoreRotation.Value.post.Rad}->{_restoreRotation.Value.pre.Rad}");
                if (_restoreRotation.Value.post.AlmostEqual(curRot, 0.01f))
                    FaceDirection(_restoreRotation.Value.pre.ToDirection());
                else if (--_restoreCntr == 0)
                    _restoreRotation = null;
            }

            // note: if we cancel movement and start casting immediately, it will be canceled some time later - instead prefer to delay for one frame
            if (EffectiveAnimationLock <= 0 && AutoQueue.Action && !IsRecastTimerActive(AutoQueue.Action) && !(blockMovement && InputOverride.IsMoving()))
            {
                // extra safety checks (should no longer be needed, but leaving them for now)
                // hack for sprint support
                // normally general action -> spell conversion is done by UseAction before calling UseActionRaw
                // calling UseActionRaw directly is not good: it would call StartCooldown, which would in turn call GetRecastTime, which always returns 5s for general actions
                // this leads to incorrect sprint cooldown (5s instead of 60s), which is just bad
                // for spells, call GetAdjustedActionId - even though it is typically done correctly by autorotation modules
                var actionAdj = AutoQueue.Action.Type == ActionType.Spell ? new(ActionType.Spell, GetAdjustedActionID(AutoQueue.Action.ID)) : AutoQueue.Action;
                if (actionAdj != AutoQueue.Action)
                    Service.Log($"[AMEx] Something didn't perform action adjustment correctly: replacing {AutoQueue.Action} with {actionAdj}");

                var targetID = AutoQueue.Target?.InstanceID ?? GameObject.InvalidGameObjectId;
                var status = GetActionStatus(actionAdj, targetID);
                if (status == 0)
                {
                    var res = UseActionRaw(actionAdj, targetID, AutoQueue.TargetPos, AutoQueue.Action.Type == ActionType.Item ? 65535u : 0);
                    //Service.Log($"[AMEx] Auto-execute {AutoQueue.Source} action {AutoQueue.Action} (=> {actionAdj}) @ {targetID:X} {Utils.Vec3String(AutoQueue.TargetPos)} => {res}");
                }
                else
                {
                    Service.Log($"[AMEx] Can't execute {AutoQueue.Source} action {AutoQueue.Action} (=> {actionAdj}) @ {targetID:X}: status {status} '{Service.LuminaRow<Lumina.Excel.GeneratedSheets.LogMessage>(status)?.Text}'");
                    blockMovement = false;
                }
            }

            _useActionInPast = 0; // clear any potential adjustments

            if (blockMovement)
                InputOverride.BlockMovement();
            else
                InputOverride.UnblockMovement();
        }

        private bool UseActionLocationDetour(ActionManager* self, ActionType actionType, uint actionID, ulong targetID, Vector3* targetPos, uint itemLocation)
        {
            var pc = Service.ClientState.LocalPlayer;
            var prevSeq = LastUsedActionSequence;
            var prevRot = pc?.Rotation ?? 0;
            bool ret = _useActionLocationHook.Original(self, actionType, actionID, targetID, targetPos, itemLocation);
            var currSeq = LastUsedActionSequence;
            var currRot = pc?.Rotation ?? 0;
            if (currSeq != prevSeq)
            {
                _lastReqInitialAnimLock = AnimationLock;
                _lastReqSequence = currSeq;
                MoveMightInterruptCast = CastTimeRemaining > 0;
                if (prevRot != currRot && Config.RestoreRotation)
                {
                    _restoreRotation = (prevRot.Radians(), currRot.Radians());
                    _restoreCntr = 2; // not sure why - but sometimes after successfully restoring rotation it is snapped back on next frame; TODO investigate
                    //Service.Log($"[AMEx] Restore start: {currRot} -> {prevRot}");
                }

                var action = new ActionID(actionType, actionID);
                var recast = _inst->GetRecastGroupDetail(GetRecastGroup(action));
                if (_useActionInPast > 0 && recast != null)
                {
                    if (CastTimeRemaining > 0)
                        Utils.WriteField(_inst, 0x30, CastTimeElapsed + _useActionInPast);
                    else
                        Utils.WriteField(_inst, 8, Math.Max(0, AnimationLock - _useActionInPast));
                    recast->Elapsed += _useActionInPast;
                }

                var recastElapsed = recast != null ? recast->Elapsed : 0;
                var recastTotal = recast != null ? recast->Total : 0;
                Service.Log($"[AMEx] UAL #{currSeq} ({action} @ {targetID:X} / {Utils.Vec3String(*targetPos)} {(ret ? "succeeded" : "failed?")}, ALock={AnimationLock:f3}, CTR={CastTimeRemaining:f3}, CD={recastElapsed:f3}/{recastTotal:f3}, GCD={GCD():f3}");
                ActionRequested?.Invoke(new() {
                    Action = action,
                    TargetID = targetID,
                    TargetPos = *targetPos,
                    SourceSequence = currSeq,
                    InitialAnimationLock = AnimationLock,
                    InitialCastTimeElapsed = CastSpellID != 0 ? CastTimeElapsed : 0,
                    InitialCastTimeTotal = CastSpellID != 0 ? CastTimeTotal : 0,
                    InitialRecastElapsed = recastElapsed,
                    InitialRecastTotal = recastTotal,
                });
            }
            return ret;
        }

        private void ProcessPacketActionEffectDetour(uint casterID, FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* casterObj, Vector3* targetPos, Network.ServerIPC.ActionEffectHeader* header, ulong* effects, ulong* targets)
        {
            var packetAnimLock = header->animationLockTime;
            if (ActionEffectReceived != null)
            {
                // note: there's a slight difference with dispatching event from here rather than from packet processing (ActionEffectN) functions
                // 1. action id is already unscrambled
                // 2. this function won't be called if caster object doesn't exist
                // the last point is deemed to be minor enough for us to not care, as it simplifies things (no need to hook 5 functions)
                var info = new ActorCastEvent
                {
                    Action = new(header->actionType, header->actionId),
                    MainTargetID = header->animationTargetId,
                    AnimationLockTime = header->animationLockTime,
                    MaxTargets = header->NumTargets,
                    TargetPos = *targetPos,
                    SourceSequence = header->SourceSequence,
                    GlobalSequence = header->globalEffectCounter,
                };
                for (int i = 0; i < header->NumTargets; ++i)
                {
                    var target = new ActorCastEvent.Target();
                    target.ID = targets[i];
                    for (int j = 0; j < 8; ++j)
                        target.Effects[j] = effects[i * 8 + j];
                    info.Targets.Add(target);
                }
                ActionEffectReceived.Invoke(casterID, info);
            }

            var prevAnimLock = AnimationLock;
            _processPacketActionEffectHook.Original(casterID, casterObj, targetPos, header, effects, targets);
            var currAnimLock = AnimationLock;

            if (header->SourceSequence == 0 || casterID != Service.ClientState.LocalPlayer?.ObjectId)
            {
                // non-player-initiated
                if (currAnimLock != prevAnimLock)
                    Service.Log($"[AMEx] Animation lock updated by non-player-initiated action: {casterID:X} {new ActionID(header->actionType, header->actionId)} {prevAnimLock:f3} -> {currAnimLock:f3}");
                return;
            }

            MoveMightInterruptCast = false; // slidecast window start
            InputOverride.UnblockMovement(); // unblock input unconditionally on successful cast (I assume there are no instances where we need to immediately start next GCD?)

            float animLockDelay = _lastReqInitialAnimLock - prevAnimLock;
            float animLockReduction = 0;

            // animation lock delay update
            if (_lastReqSequence == header->SourceSequence)
            {
                if (_lastReqInitialAnimLock > 0)
                {
                    float adjDelay = animLockDelay;
                    if (adjDelay > AnimationLockDelayMax)
                    {
                        // sanity check for plugin conflicts
                        if (header->animationLockTime != packetAnimLock || packetAnimLock % 0.01 is >= 0.0005f and <= 0.0095f)
                        {
                            Service.Log($"[AMEx] Unexpected animation lock {packetAnimLock:f} -> {header->animationLockTime:f}, disabling anim lock tweak feature");
                            Config.RemoveAnimationLockDelay = false;
                        }
                        else
                        {
                            animLockReduction = Math.Min(adjDelay - AnimationLockDelayMax, currAnimLock);
                            adjDelay -= animLockReduction;
                            Utils.WriteField(_inst, 8, currAnimLock - animLockReduction);
                        }
                    }
                    AnimationLockDelayAverage = adjDelay * (1 - AnimationLockDelaySmoothing) + AnimationLockDelayAverage * AnimationLockDelaySmoothing;
                }
            }
            else if (currAnimLock != prevAnimLock)
            {
                Service.Log($"[AMEx] Animation lock updated by action with unexpected sequence ID #{header->SourceSequence}: {prevAnimLock:f3} -> {currAnimLock:f3}");
            }

            Service.Log($"[AMEx] AEP #{header->SourceSequence} {prevAnimLock:f3} -> ALock={currAnimLock:f3} (delayed by {animLockDelay:f3}-{animLockReduction:f3}), CTR={CastTimeRemaining:f3}, GCD={GCD():f3}");
        }

        private void ProcessPacketEffectResultDetour(uint targetID, byte* packet, byte replaying)
        {
            if (EffectResultReceived != null)
            {
                var count = packet[0];
                var p = (Network.ServerIPC.EffectResultEntry*)(packet + 4);
                for (int i = 0; i < count; ++i)
                {
                    EffectResultReceived.Invoke(targetID, p->RelatedActionSequence, p->RelatedTargetIndex);
                    ++p;
                }
            }
            _processPacketEffectResultHook.Original(targetID, packet, replaying);
        }

        private void ProcessPacketEffectResultBasicDetour(uint targetID, byte* packet, byte replaying)
        {
            if (EffectResultReceived != null)
            {
                var count = packet[0];
                var p = (Network.ServerIPC.EffectResultBasicEntry*)(packet + 4);
                for (int i = 0; i < count; ++i)
                {
                    EffectResultReceived.Invoke(targetID, p->RelatedActionSequence, p->RelatedTargetIndex);
                    ++p;
                }
            }
            _processPacketEffectResultBasicHook.Original(targetID, packet, replaying);
        }
    }
}
