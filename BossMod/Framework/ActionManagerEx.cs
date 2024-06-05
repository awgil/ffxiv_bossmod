using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using CSActionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;

namespace BossMod;

// extensions and utilities for interacting with game's ActionManager singleton
// handles following features:
// 1. automatic action execution (provided by autorotation or ai modules, if enabled); does nothing if no automatic actions are provided
// 2. effective animation lock reduction (a-la xivalex)
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
unsafe sealed class ActionManagerEx : IDisposable
{
    public static ActionManagerEx? Instance;

    public ActionID CastSpell => new(ActionType.Spell, _inst->CastSpellId);
    public ActionID CastAction => new((ActionType)_inst->CastActionType, _inst->CastActionId);
    public float CastTimeRemaining => _inst->CastSpellId != 0 ? _inst->CastTimeTotal - _inst->CastTimeElapsed : 0;
    public float ComboTimeLeft => _inst->Combo.Timer;
    public uint ComboLastMove => _inst->Combo.Action;
    public ActionID QueuedAction => new((ActionType)_inst->QueuedActionType, _inst->QueuedActionId);

    public float EffectiveAnimationLock => _inst->AnimationLock + CastTimeRemaining; // animation lock starts ticking down only when cast ends, so this is the minimal time until next action can be requested

    public Event<ClientActionRequest> ActionRequested = new();
    public Event<ulong, ActorCastEvent> ActionEffectReceived = new();

    public AnimationLockTweak AnimLockTweak = new();
    public InputOverride InputOverride = new();
    public ActionManagerConfig Config = Service.Config.Get<ActionManagerConfig>();
    public CommonActions.NextAction AutoQueue;
    public bool MoveMightInterruptCast { get; private set; } // if true, moving now might cause cast interruption (for current or queued cast)
    private readonly ActionManager* _inst = ActionManager.Instance();
    private float _lastReqInitialAnimLock;
    private int _lastReqSequence = -1;
    private float _useActionInPast; // if >0 while using an action, cooldown/anim lock will be reduced by this amount as if action was used a bit in the past
    private (Angle pre, Angle post)? _restoreRotation; // if not null, we'll try restoring rotation to pre while it is equal to post
    private int _restoreCntr;

    private readonly HookAddress<ActionManager.Delegates.Update> _updateHook;
    private readonly HookAddress<ActionManager.Delegates.UseActionLocation> _useActionLocationHook;
    private readonly HookAddress<PublicContentBozja.Delegates.UseFromHolster> _useBozjaFromHolsterDirectorHook;
    private readonly HookAddress<ActionEffectHandler.Delegates.Receive> _processPacketActionEffectHook;

    public ActionManagerEx()
    {
        Service.Log($"[AMEx] ActionManager singleton address = 0x{(ulong)_inst:X}");
        _updateHook = new(ActionManager.Addresses.Update, UpdateDetour);
        _useActionLocationHook = new(ActionManager.Addresses.UseActionLocation, UseActionLocationDetour);
        _useBozjaFromHolsterDirectorHook = new(PublicContentBozja.Addresses.UseFromHolster, UseBozjaFromHolsterDirectorDetour);
        _processPacketActionEffectHook = new(ActionEffectHandler.Addresses.Receive, ProcessPacketActionEffectDetour);
    }

    public void Dispose()
    {
        _processPacketActionEffectHook.Dispose();
        _useBozjaFromHolsterDirectorHook.Dispose();
        _useActionLocationHook.Dispose();
        _updateHook.Dispose();
        InputOverride.Dispose();
    }

    public Vector3? GetWorldPosUnderCursor()
    {
        Vector3 res = new();
        return _inst->GetGroundPositionForCursor(&res) ? res : null;
    }

    public void FaceTarget(Vector3 position, ulong unkObjID = 0xE0000000) => _inst->AutoFaceTargetPosition(&position, unkObjID);
    public void FaceDirection(WDir direction)
    {
        var player = Service.ClientState.LocalPlayer;
        if (player != null)
            FaceTarget(player.Position + new Vector3(direction.X, 0, direction.Z));
    }

    public void GetCooldown(ref Cooldown result, RecastDetail* data)
    {
        if (data->IsActive != 0)
        {
            result.Elapsed = data->Elapsed;
            result.Total = data->Total;
        }
        else
        {
            result.Elapsed = result.Total = 0;
        }
    }

    public void GetCooldowns(Span<Cooldown> cooldowns)
    {
        // [0,80) are stored in actionmanager, [80,81) are stored in director
        var rg = _inst->GetRecastGroupDetail(0);
        for (int i = 0; i < 80; ++i)
            GetCooldown(ref cooldowns[i], rg++);
        rg = _inst->GetRecastGroupDetail(80);
        if (rg != null)
        {
            for (int i = 80; i < 82; ++i)
                GetCooldown(ref cooldowns[i], rg++);
        }
        else
        {
            for (int i = 80; i < 82; ++i)
                cooldowns[i] = default;
        }
    }

    public float GCD()
    {
        var gcd = _inst->GetRecastGroupDetail(CommonDefinitions.GCDGroup);
        return gcd->Total - gcd->Elapsed;
    }

    public ActionID GetDutyAction(ushort slot)
    {
        var id = ActionManager.GetDutyActionId(slot);
        return id != 0 ? new(ActionType.Spell, id) : default;
    }
    public (ActionID, ActionID) GetDutyActions() => (GetDutyAction(0), GetDutyAction(1));

    public uint GetAdjustedActionID(uint actionID) => _inst->GetAdjustedActionId(actionID);

    public uint GetActionStatus(ActionID action, ulong target, bool checkRecastActive = true, bool checkCastingActive = true, uint* outOptExtraInfo = null)
    {
        if (action.Type is ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1)
            action = BozjaActionID.GetHolster(action.As<BozjaHolsterID>()); // see BozjaContentDirector.useFromHolster
        return _inst->GetActionStatus((CSActionType)action.Type, action.ID, target, checkRecastActive, checkCastingActive, outOptExtraInfo);
    }

    // returns time in ms
    public int GetAdjustedCastTime(ActionID action, bool applyProcs = true, ActionManager.CastTimeProc* outOptProc = null)
        => ActionManager.GetAdjustedCastTime((CSActionType)action.Type, action.ID, applyProcs, outOptProc);

    public bool IsRecastTimerActive(ActionID action)
        => _inst->IsRecastTimerActive((CSActionType)action.Type, action.ID);

    public int GetRecastGroup(ActionID action)
        => _inst->GetRecastGroup((int)action.Type, action.ID);

    // skips queueing etc
    private bool ExecuteAction(ActionID action, ulong targetId, Vector3 targetPos)
    {
        if (action.Type is ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1)
        {
            // fake action type - using action from bozja holster
            var state = PublicContentBozja.GetState(); // note: if it's non-null, the director instance can't be null too
            var holsterIndex = state != null ? state->HolsterActions.IndexOf((byte)action.ID) : -1;
            return holsterIndex >= 0 && PublicContentBozja.GetInstance()->UseFromHolster((uint)holsterIndex, action.Type == ActionType.BozjaHolsterSlot1 ? 1u : 0);
        }
        else
        {
            // real action type, just execute our UAL hook
            // note that for items extraParam should be 0xFFFF (since we want to use any item, not from first inventory slot)
            var extraParam = action.Type == ActionType.Item ? 0xFFFFu : 0;
            return _inst->UseActionLocation((CSActionType)action.Type, action.ID, targetId, &targetPos, extraParam);
        }
    }

    private void UpdateDetour(ActionManager* self)
    {
        var dt = Framework.Instance()->FrameDeltaTime;
        var imminentAction = _inst->ActionQueued ? QueuedAction : AutoQueue.Action;
        var imminentActionAdj = imminentAction.Type == ActionType.Spell ? new(ActionType.Spell, GetAdjustedActionID(imminentAction.ID)) : imminentAction;
        var imminentRecast = imminentActionAdj ? _inst->GetRecastGroupDetail(GetRecastGroup(imminentActionAdj)) : null;
        if (Config.RemoveCooldownDelay)
        {
            var cooldownOverflow = imminentRecast != null && imminentRecast->IsActive != 0 ? imminentRecast->Elapsed + dt - imminentRecast->Total : dt;
            var animlockOverflow = dt - _inst->AnimationLock;
            _useActionInPast = Math.Min(cooldownOverflow, animlockOverflow);
            if (_useActionInPast >= dt)
                _useActionInPast = 0; // nothing prevented us from casting it before, so do not adjust anything...
            else if (_useActionInPast > 0.1f)
                _useActionInPast = 0.1f; // upper limit for time adjustment
        }

        _updateHook.Original(self);

        // check whether movement is safe; block movement if not and if desired
        MoveMightInterruptCast &= CastTimeRemaining > 0; // previous cast could have ended without action effect
        MoveMightInterruptCast |= imminentActionAdj && CastTimeRemaining <= 0 && _inst->AnimationLock < 0.1f && GetAdjustedCastTime(imminentActionAdj) > 0 && GCD() < 0.1f; // if we're not casting, but will start soon, moving might interrupt future cast
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

            var targetID = AutoQueue.Target?.InstanceID ?? 0xE0000000;
            var status = GetActionStatus(actionAdj, targetID);
            if (status == 0)
            {
                if (AutoQueue.FacingAngle != null)
                    FaceDirection(AutoQueue.FacingAngle.Value.ToDirection());

                var res = ExecuteAction(actionAdj, targetID, AutoQueue.TargetPos);
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

    private bool UseActionLocationDetour(ActionManager* self, CSActionType actionType, uint actionId, ulong targetId, Vector3* location, uint extraParam)
    {
        var pc = Service.ClientState.LocalPlayer;
        var prevSeq = _inst->LastUsedActionSequence;
        var prevRot = pc?.Rotation ?? 0;
        bool ret = _useActionLocationHook.Original(self, actionType, actionId, targetId, location, extraParam);
        var currSeq = _inst->LastUsedActionSequence;
        var currRot = pc?.Rotation ?? 0;
        if (currSeq != prevSeq)
        {
            HandleActionRequest(new((ActionType)actionType, actionId), currSeq, targetId, *location, prevRot, currRot);
        }
        return ret;
    }

    private bool UseBozjaFromHolsterDirectorDetour(PublicContentBozja* self, uint holsterIndex, uint slot)
    {
        var pc = Service.ClientState.LocalPlayer;
        var prevRot = pc?.Rotation ?? 0;
        var res = _useBozjaFromHolsterDirectorHook.Original(self, holsterIndex, slot);
        if (res)
        {
            var currRot = pc?.Rotation ?? 0;
            var entry = (BozjaHolsterID)self->State.HolsterActions[(int)holsterIndex];
            HandleActionRequest(ActionID.MakeBozjaHolster(entry, (int)slot), 0, 0xE0000000, default, prevRot, currRot);
        }
        return res;
    }

    private void ProcessPacketActionEffectDetour(uint casterID, Character* casterObj, Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects, GameObjectId* targets)
    {
        // notify listeners about the event
        // note: there's a slight difference with dispatching event from here rather than from packet processing (ActionEffectN) functions
        // 1. action id is already unscrambled
        // 2. this function won't be called if caster object doesn't exist
        // the last point is deemed to be minor enough for us to not care, as it simplifies things (no need to hook 5 functions)
        var info = new ActorCastEvent
        {
            Action = new ActionID((ActionType)header->ActionType, header->ActionId),
            MainTargetID = header->AnimationTargetId,
            AnimationLockTime = header->AnimationLock,
            MaxTargets = header->NumTargets,
            TargetPos = *targetPos,
            SourceSequence = header->SourceSequence,
            GlobalSequence = header->GlobalSequence,
        };
        var rawEffects = (ulong*)effects;
        for (int i = 0; i < header->NumTargets; ++i)
        {
            var targetEffects = new ActionEffects();
            for (int j = 0; j < ActionEffects.MaxCount; ++j)
                targetEffects[j] = rawEffects[i * 8 + j];
            info.Targets.Add(new(targets[i], targetEffects));
        }
        ActionEffectReceived.Fire(casterID, info);

        // call the hooked function and observe the effects
        var packetAnimLock = header->AnimationLock;
        var prevAnimLock = _inst->AnimationLock;
        _processPacketActionEffectHook.Original(casterID, casterObj, targetPos, header, effects, targets);
        var currAnimLock = _inst->AnimationLock;

        if (casterID != UIState.Instance()->PlayerState.EntityId || header->SourceSequence == 0 && _lastReqSequence != 0)
        {
            // this action is either executed by non-player, or is non-player-initiated
            // TODO: reconsider the condition:
            // - some actions with SourceSequence != 0 are special-cased in code (NIN's ten/chi/jin) and apparently don't trigger anim-lock, verify
            // - actions can have 'force anim lock' flag set, and then trigger anim-lock despite SourceSequence == 0, verify e.g. bozja holster actions
            // - auto is the most common cast with SourceSequence == 0; can it happen while waiting for reholster response?..
            // - do we want to do non-anim-lock related things (eg unblock movement override) when we get action with 'force anim lock' flag?
            if (currAnimLock != prevAnimLock)
                Service.Log($"[AMEx] Animation lock updated by non-player-initiated action: #{header->SourceSequence} {casterID:X} {info.Action} {prevAnimLock:f3} -> {currAnimLock:f3}");
            return;
        }

        MoveMightInterruptCast = false; // slidecast window start
        InputOverride.UnblockMovement(); // unblock input unconditionally on successful cast (I assume there are no instances where we need to immediately start next GCD?)

        // animation lock delay update
        float animLockDelay = _lastReqInitialAnimLock - prevAnimLock;
        float animLockReduction = 0;
        if (_lastReqSequence == header->SourceSequence)
        {
            if (_lastReqInitialAnimLock > 0)
            {
                AnimLockTweak.SanityCheck(packetAnimLock, header->AnimationLock);
                animLockReduction = AnimLockTweak.Apply(_inst->AnimationLock, animLockDelay);
                _inst->AnimationLock -= animLockReduction;
            }
        }
        else if (currAnimLock != prevAnimLock)
        {
            Service.Log($"[AMEx] Animation lock updated by action with unexpected sequence ID #{header->SourceSequence}: {prevAnimLock:f3} -> {currAnimLock:f3}");
        }

        Service.Log($"[AMEx] AEP #{header->SourceSequence} {prevAnimLock:f3} {info.Action} -> ALock={currAnimLock:f3} (delayed by {animLockDelay:f3}-{animLockReduction:f3}), CTR={CastTimeRemaining:f3}, GCD={GCD():f3}");
        _lastReqSequence = -1;
    }

    private void HandleActionRequest(ActionID action, int seq, ulong targetID, Vector3 targetPos, float prevRot, float currRot)
    {
        _lastReqInitialAnimLock = _inst->AnimationLock;
        _lastReqSequence = seq;
        MoveMightInterruptCast = CastTimeRemaining > 0;
        if (prevRot != currRot && Config.RestoreRotation)
        {
            _restoreRotation = (prevRot.Radians(), currRot.Radians());
            _restoreCntr = 2; // not sure why - but sometimes after successfully restoring rotation it is snapped back on next frame; TODO investigate
            //Service.Log($"[AMEx] Restore start: {currRot} -> {prevRot}");
        }

        var recast = _inst->GetRecastGroupDetail(GetRecastGroup(action));
        if (_useActionInPast > 0)
        {
            if (CastTimeRemaining > 0)
                _inst->CastTimeElapsed += _useActionInPast;
            else
                _inst->AnimationLock = Math.Max(0, _inst->AnimationLock - _useActionInPast);

            if (recast != null)
                recast->Elapsed += _useActionInPast;
        }

        var recastElapsed = recast != null ? recast->Elapsed : 0;
        var recastTotal = recast != null ? recast->Total : 0;
        Service.Log($"[AMEx] UAL #{seq} {action} @ {targetID:X} / {Utils.Vec3String(targetPos)}, ALock={_inst->AnimationLock:f3}, CTR={CastTimeRemaining:f3}, CD={recastElapsed:f3}/{recastTotal:f3}, GCD={GCD():f3}");
        ActionRequested.Fire(new(action, targetID, targetPos, (uint)seq, _inst->AnimationLock, _inst->CastSpellId != 0 ? _inst->CastTimeElapsed : 0, _inst->CastSpellId != 0 ? _inst->CastTimeTotal : 0, recastElapsed, recastTotal));
    }
}
