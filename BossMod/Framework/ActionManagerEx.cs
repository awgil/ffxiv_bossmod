using Dalamud;
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
    class ActionManagerEx : IDisposable
    {
        public static ActionManagerEx? Instance;
        public const int NumCooldownGroups = 80;

        public float AnimationLockDelaySmoothing = 0.8f; // TODO tweak
        public float AnimationLockDelayAverage { get; private set; } = 0.1f; // smoothed delay between client request and server response
        public float AnimationLockDelayMax => Config.RemoveAnimationLockDelay ? 0 : float.MaxValue; // this caps max delay a-la xivalexander (TODO: make tweakable?)
        public unsafe float AnimationLock => Utils.ReadField<float>(_inst, 8);

        public unsafe uint CastSpellID => Utils.ReadField<uint>(_inst, 0x24);
        public ActionID CastSpell => new(ActionType.Spell, CastActionID);
        public unsafe ActionType CastActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x28);
        public unsafe uint CastActionID => Utils.ReadField<uint>(_inst, 0x2C);
        public ActionID CastAction => new(CastActionType, CastActionID);
        public unsafe float CastTimeElapsed => Utils.ReadField<float>(_inst, 0x30);
        public unsafe float CastTimeTotal => Utils.ReadField<float>(_inst, 0x34);
        public float CastTimeRemaining => CastSpellID != 0 ? CastTimeTotal - CastTimeElapsed : 0;
        public unsafe ulong CastTargetID => Utils.ReadField<ulong>(_inst, 0x38);
        public unsafe Vector3 CastTargetPos => Utils.ReadField<Vector3>(_inst, 0x40);

        public unsafe float ComboTimeLeft => Utils.ReadField<float>(_inst, 0x60);
        public unsafe uint ComboLastMove => Utils.ReadField<uint>(_inst, 0x64);

        public unsafe bool QueueActive => Utils.ReadField<bool>(_inst, 0x68);
        public unsafe ActionType QueueActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x6C);
        public unsafe uint QueueActionID => Utils.ReadField<uint>(_inst, 0x70);
        public ActionID QueueAction => new(QueueActionType, QueueActionID);
        public unsafe ulong QueueTargetID => Utils.ReadField<ulong>(_inst, 0x78);
        public unsafe uint QueueCallType => Utils.ReadField<uint>(_inst, 0x80);
        public unsafe uint QueueComboRouteID => Utils.ReadField<uint>(_inst, 0x84);

        public unsafe uint GTActionID => Utils.ReadField<uint>(_inst, 0x88);
        public unsafe ActionType GTActionType => (ActionType)Utils.ReadField<uint>(_inst, 0x8C);
        public ActionID GTAction => new(GTActionType, GTActionID);
        public unsafe uint GTSpellID => Utils.ReadField<uint>(_inst, 0x90);
        public ActionID GTSpell => new(ActionType.Spell, GTSpellID);
        public unsafe uint GTUnkArg => Utils.ReadField<uint>(_inst, 0x94);
        public unsafe ulong GTUnkObj => Utils.ReadField<ulong>(_inst, 0x98);
        public unsafe byte GT_uA0 => Utils.ReadField<byte>(_inst, 0xA0);
        public unsafe byte GT_uB8 => Utils.ReadField<byte>(_inst, 0xB8);
        public unsafe uint GT_uBC => Utils.ReadField<byte>(_inst, 0xBC);

        public unsafe ushort LastUsedActionSequence => Utils.ReadField<ushort>(_inst, 0x110);

        public float EffectiveAnimationLock => AnimationLock + CastTimeRemaining; // animation lock starts ticking down only when cast ends
        public float EffectiveAnimationLockDelay => AnimationLockDelayMax <= 0.5f ? AnimationLockDelayMax : MathF.Min(AnimationLockDelayAverage, 0.1f); // this is a conservative estimate

        public event EventHandler<ClientActionRequest>? ActionRequested;

        public InputOverride InputOverride;
        public ActionManagerConfig Config;
        public CommonActions.NextAction AutoQueue; // TODO: consider using native 'queue' fields for this?
        public bool MoveMightInterruptCast { get; private set; } // if true, moving now might cause cast interruption (for current or queued cast)
        private unsafe ActionManager* _inst;
        private float _lastReqInitialAnimLock;
        private ushort _lastReqSequence;
        private float _useActionInPast; // if >0 while using an action, cooldown/anim lock will be reduced by this amount as if action was used a bit in the past
        private (Angle pre, Angle post)? _restoreRotation; // if not null, we'll try restoring rotation to pre while it is equal to post
        private int _restoreCntr;

        private unsafe delegate bool GetGroundTargetPositionDelegate(ActionManager* self, Vector3* outPos);
        private GetGroundTargetPositionDelegate _getGroundTargetPositionFunc;

        private unsafe delegate void FaceTargetDelegate(ActionManager* self, Vector3* position, ulong targetID);
        private FaceTargetDelegate _faceTargetFunc;

        private unsafe delegate void UpdateDelegate(ActionManager* self);
        private Hook<UpdateDelegate> _updateHook;

        private unsafe delegate bool UseActionLocationDelegate(ActionManager* self, ActionType actionType, uint actionID, ulong targetID, Vector3* targetPos, uint itemLocation);
        private Hook<UseActionLocationDelegate> _useActionLocationHook;

        private unsafe delegate void ProcessActionEffectPacketDelegate(uint casterID, void* casterObj, Vector3* targetPos, Protocol.Server_ActionEffectHeader* header, ulong* effects, ulong* targets);
        private Hook<ProcessActionEffectPacketDelegate> _processActionEffectPacketHook;

        private IntPtr _gtQueuePatch; // instruction inside UseAction: conditional jump that disallows queueing for ground-targeted actions
        private bool _gtQueuePatchEnabled;
        public bool AllowGTQueueing
        {
            get => _gtQueuePatchEnabled;
            set
            {
                if (_gtQueuePatchEnabled != value)
                {
                    SafeMemory.WriteBytes(_gtQueuePatch, new byte[] { value ? (byte)0xEB : (byte)0x74 });
                    _gtQueuePatchEnabled = value;
                }
            }
        }

        public unsafe ActionManagerEx()
        {
            InputOverride = new();
            Config = Service.Config.Get<ActionManagerConfig>();

            _inst = ActionManager.Instance();
            Service.Log($"[AMEx] ActionManager singleton address = 0x{(ulong)_inst:X}");

            var getGroundTargetPositionAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 44 8B 84 24 80 00 00 00 33 C0");
            Service.Log($"[AMEx] GetGroundTargetPosition address = 0x{getGroundTargetPositionAddress:X}");
            _getGroundTargetPositionFunc = Marshal.GetDelegateForFunctionPointer<GetGroundTargetPositionDelegate>(getGroundTargetPositionAddress);

            var faceTargetAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 81 FE FB 1C 00 00 74 ?? 81 FE 53 5F 00 00 74 ?? 81 FE 6F 73 00 00");
            Service.Log($"[AMEx] FaceTarget address = 0x{faceTargetAddress:X}");
            _faceTargetFunc = Marshal.GetDelegateForFunctionPointer<FaceTargetDelegate>(faceTargetAddress);

            var updateAddress = Service.SigScanner.ScanText("48 8B C4 48 89 58 20 57 48 81 EC 90 00 00 00 48 8B 3D ?? ?? ?? ?? 48 8B D9 48 85 FF 0F 84 ?? ?? ?? ?? 48 89 68 08 48 8B CF 48 89 70 10 4C 89 70 18 0F 29 70 E8 44 0F 29 48 B8 44 0F 29 50 A8");
            Service.Log($"[AMEx] Update address = 0x{updateAddress:X}");
            _updateHook = Hook<UpdateDelegate>.FromAddress(updateAddress, UpdateDetour);
            _updateHook.Enable();

            var useActionLocationAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 3C 01 0F 85 ?? ?? ?? ?? EB 46");
            Service.Log($"[AMEx] UseActionLocation address = 0x{useActionLocationAddress:X}");
            _useActionLocationHook = Hook<UseActionLocationDelegate>.FromAddress(useActionLocationAddress, UseActionLocationDetour);
            _useActionLocationHook.Enable();

            var processActionEffectPacketAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 4C 24 68 48 33 CC E8 ?? ?? ?? ?? 4C 8D 5C 24 70 49 8B 5B 20 49 8B 73 28 49 8B E3 5F C3");
            Service.Log($"[AMEx] ProcessActionEffectPacket address = 0x{processActionEffectPacketAddress:X}");
            _processActionEffectPacketHook = Hook<ProcessActionEffectPacketDelegate>.FromAddress(processActionEffectPacketAddress, ProcessActionEffectPacketDetour);
            _processActionEffectPacketHook.Enable();

            _gtQueuePatch = Service.SigScanner.ScanModule("74 24 41 81 FE F5 0D 00 00");
            Service.Log($"[AMEx] GT queue check address = 0x{_gtQueuePatch:X}");
            AllowGTQueueing = true;
        }

        public void Dispose()
        {
            AllowGTQueueing = false;
            _processActionEffectPacketHook.Dispose();
            _useActionLocationHook.Dispose();
            _updateHook.Dispose();
            InputOverride.Dispose();
        }

        public unsafe Vector3? GetWorldPosUnderCursor()
        {
            Vector3 res = new();
            return _getGroundTargetPositionFunc(_inst, &res) ? res : null;
        }

        public unsafe void FaceTarget(Vector3 position, ulong unkObjID = GameObject.InvalidGameObjectId)
        {
            _faceTargetFunc(_inst, &position, unkObjID);
        }
        public void FaceDirection(WDir direction)
        {
            var player = Service.ClientState.LocalPlayer;
            if (player != null)
                FaceTarget(player.Position + new Vector3(direction.X, 0, direction.Z));
        }

        public unsafe void GetCooldowns(float[] cooldowns)
        {
            var rg = _inst->GetRecastGroupDetail(0);
            for (int i = 0; i < NumCooldownGroups; ++i)
            {
                cooldowns[i] = rg->Total - rg->Elapsed;
                ++rg;
            }
        }

        public unsafe float GCD()
        {
            var gcd = _inst->GetRecastGroupDetail(CommonDefinitions.GCDGroup);
            return gcd->Total - gcd->Elapsed;
        }

        public unsafe uint GetAdjustedActionID(uint actionID) => _inst->GetAdjustedActionId(actionID);

        public unsafe uint GetActionStatus(ActionID action, ulong target, bool checkRecastActive = true, bool checkCastingActive = true, uint* outOptExtraInfo = null)
        {
            return _inst->GetActionStatus((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, (long)target, checkRecastActive, checkCastingActive, outOptExtraInfo);
        }

        // returns time in ms
        public unsafe int GetAdjustedCastTime(ActionID action, bool skipHasteAdjustment = true, byte* outOptProcState = null)
            => ActionManager.GetAdjustedCastTime((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, (byte)(skipHasteAdjustment ? 1 : 0), outOptProcState);

        public unsafe bool IsRecastTimerActive(ActionID action)
            => _inst->IsRecastTimerActive((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID);

        public unsafe int GetRecastGroup(ActionID action)
            => _inst->GetRecastGroup((int)action.Type, action.ID);

        public unsafe bool UseAction(ActionID action, ulong targetID, uint itemLocation, uint callType, uint comboRouteID, bool* outOptGTModeStarted)
        {
            return _inst->UseAction((FFXIVClientStructs.FFXIV.Client.Game.ActionType)action.Type, action.ID, (long)targetID, itemLocation, callType, comboRouteID, outOptGTModeStarted);
        }

        // skips queueing etc
        public unsafe bool UseActionRaw(ActionID action, ulong targetID = GameObject.InvalidGameObjectId, Vector3 targetPos = new(), uint itemLocation = 0)
        {
            return UseActionLocationDetour(_inst, action.Type, action.ID, targetID, &targetPos, itemLocation);
        }

        private unsafe void UpdateDetour(ActionManager* self)
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

        private unsafe bool UseActionLocationDetour(ActionManager* self, ActionType actionType, uint actionID, ulong targetID, Vector3* targetPos, uint itemLocation)
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
                ActionRequested?.Invoke(this, new() {
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

        private unsafe void ProcessActionEffectPacketDetour(uint casterID, void* casterObj, Vector3* targetPos, Protocol.Server_ActionEffectHeader* header, ulong* effects, ulong* targets)
        {
            var prevAnimLock = AnimationLock;
            _processActionEffectPacketHook.Original(casterID, casterObj, targetPos, header, effects, targets);
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
                        animLockReduction = Math.Min(adjDelay - AnimationLockDelayMax, currAnimLock);
                        adjDelay -= animLockReduction;
                        Utils.WriteField(_inst, 8, currAnimLock - animLockReduction);
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
    }
}
