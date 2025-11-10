using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using System.Runtime.InteropServices;
using CSActionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;

namespace BossMod;

// extensions and utilities for interacting with game's ActionManager singleton
// handles following features:
// 1. automatic action execution (provided by autorotation or ai modules, if enabled); does nothing if no automatic actions are provided
// 2. effective animation lock reduction (a-la xivalex)
// 3. framerate-dependent cooldown reduction
// 4. slidecast assistant aka movement block
//    cast is interrupted if player moves when remaining cast time is greater than ~0.5s (moving during that window without interrupting is known as slidecasting)
//    this feature blocks WSAD input to prevent movement while this would interrupt a cast, allowing slidecasting efficiently while just holding movement button
//    other ways of moving (eg LMB+RMB, jumping etc) are not blocked, allowing for emergency movement even while the feature is active
//    movement is blocked a bit before cast start and unblocked as soon as action effect packet is received
// 5. preserving character facing direction
// 6. ground-targeted action queueing
//    ground-targeted actions can't be queued, making using them efficiently tricky
//    this feature allows queueing them, plus provides options to execute them automatically either at target's position or at cursor's position
// 7. auto cancel cast utility
// TODO: should not be public!
public sealed unsafe class ActionManagerEx : IDisposable
{
    public ActionID CastSpell => new(ActionType.Spell, _inst->CastSpellId);
    public ActionID CastAction => new((ActionType)_inst->CastActionType, _inst->CastActionId);
    public float CastTimeRemaining => _inst->CastSpellId != 0 ? _inst->CastTimeTotal - _inst->CastTimeElapsed : 0;
    public float ComboTimeLeft => _inst->Combo.Timer;
    public uint ComboLastMove => _inst->Combo.Action;
    public ActionID QueuedAction => new((ActionType)_inst->QueuedActionType, _inst->QueuedActionId);

    public float EffectiveAnimationLock => _inst->AnimationLock + CastTimeRemaining; // animation lock starts ticking down only when cast ends, so this is the minimal time until next action can be requested
    public float AnimationLockDelayEstimate => _animLockTweak.DelayEstimate;

    public Event<ClientActionRequest> ActionRequestExecuted = new();
    public Event<ulong, ActorCastEvent> ActionEffectReceived = new();

    public ActionTweaksConfig Config = Service.Config.Get<ActionTweaksConfig>();
    public ActionQueue.Entry AutoQueue { get; private set; }
    public bool MoveMightInterruptCast { get; private set; } // if true, moving now might cause cast interruption (for current or queued cast)
    private readonly ActionManager* _inst = ActionManager.Instance();
    private readonly WorldState _ws;
    private readonly AIHints _hints;
    private readonly MovementOverride _movement;
    private readonly ManualActionQueueTweak _manualQueue;
    private readonly AnimationLockTweak _animLockTweak = new();
    private readonly CooldownDelayTweak _cooldownTweak = new();
    private readonly CancelCastTweak _cancelCastTweak;
    private readonly AutoDismountTweak _dismountTweak;
    private readonly SmartRotationTweak _smartRotationTweak;
    private readonly OutOfCombatActionsTweak _oocActionsTweak;
    private readonly AutoAutosTweak _autoAutosTweak;

    private readonly HookAddress<ActionManager.Delegates.Update> _updateHook;
    private readonly HookAddress<ActionManager.Delegates.UseAction> _useActionHook;
    private readonly HookAddress<ActionManager.Delegates.UseActionLocation> _useActionLocationHook;
    private readonly HookAddress<PublicContentBozja.Delegates.UseFromHolster> _useBozjaFromHolsterDirectorHook;
    private readonly HookAddress<InstanceContentDeepDungeon.Delegates.UsePomander> _usePomanderHook;
    private readonly HookAddress<InstanceContentDeepDungeon.Delegates.UseStone> _useStoneHook;
    private readonly HookAddress<ActionEffectHandler.Delegates.Receive> _processPacketActionEffectHook;
    private readonly HookAddress<AutoAttackState.Delegates.SetImpl> _setAutoAttackStateHook;

    private delegate void ExecuteCommandGTDelegate(uint commandId, Vector3* position, uint param1, uint param2, uint param3, uint param4);
    private readonly ExecuteCommandGTDelegate _executeCommandGT;
    private DateTime _nextAllowedExecuteCommand;

    private readonly unsafe delegate* unmanaged<TargetSystem*, TargetSystem*> _autoSelectTarget;

    public ActionManagerEx(WorldState ws, AIHints hints, MovementOverride movement)
    {
        _ws = ws;
        _hints = hints;
        _movement = movement;
        _manualQueue = new(ws, hints);
        _cancelCastTweak = new(ws, hints);
        _dismountTweak = new(ws);
        _smartRotationTweak = new(ws, hints);
        _oocActionsTweak = new(ws);
        _autoAutosTweak = new(ws, hints);

        Service.Log($"[AMEx] ActionManager singleton address = 0x{(ulong)_inst:X}");
        _updateHook = new(ActionManager.Addresses.Update, UpdateDetour);
        _useActionHook = new(ActionManager.Addresses.UseAction, UseActionDetour);
        _useActionLocationHook = new(ActionManager.Addresses.UseActionLocation, UseActionLocationDetour);
        _useBozjaFromHolsterDirectorHook = new(PublicContentBozja.Addresses.UseFromHolster, UseBozjaFromHolsterDirectorDetour);
        _usePomanderHook = new(InstanceContentDeepDungeon.Addresses.UsePomander, UsePomanderDetour);
        _useStoneHook = new(InstanceContentDeepDungeon.Addresses.UseStone, UseStoneDetour);
        _processPacketActionEffectHook = new(ActionEffectHandler.Addresses.Receive, ProcessPacketActionEffectDetour);
        _setAutoAttackStateHook = new(AutoAttackState.Addresses.SetImpl, SetAutoAttackStateDetour);

        var executeCommandGTAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 3D 8B 93 ?? ?? ?? ??");
        Service.Log($"ExecuteCommandGT address: 0x{executeCommandGTAddress:X}");
        _executeCommandGT = Marshal.GetDelegateForFunctionPointer<ExecuteCommandGTDelegate>(executeCommandGTAddress);

        var selectTargetAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B CE E8 ?? ?? ?? ?? 48 3B C5");
        Service.Log($"SelectTarget address: 0x{selectTargetAddress:X}");
        _autoSelectTarget = (delegate* unmanaged<TargetSystem*, TargetSystem*>)selectTargetAddress;
    }

    public void Dispose()
    {
        _setAutoAttackStateHook.Dispose();
        _processPacketActionEffectHook.Dispose();
        _useStoneHook.Dispose();
        _usePomanderHook.Dispose();
        _useBozjaFromHolsterDirectorHook.Dispose();
        _useActionLocationHook.Dispose();
        _useActionHook.Dispose();
        _updateHook.Dispose();
        _oocActionsTweak.Dispose();
    }

    public void QueueManualActions()
    {
        _manualQueue.RemoveExpired();
        _manualQueue.FillQueue(_hints.ActionsToExecute);
    }

    // finish gathering candidate actions for this frame: sort by priority and select best action to execute
    public void FinishActionGather()
    {
        AutoQueue = default;
        var player = _ws.Party.Player();
        if (player == null)
            return;

        _oocActionsTweak.FillActions(player, _hints);
        AutoQueue = _hints.ActionsToExecute.FindBest(_ws, player, _ws.Client.Cooldowns, EffectiveAnimationLock, _hints, _animLockTweak.DelayEstimate, _dismountTweak.AutoDismountEnabled);
        if (AutoQueue.Delay > 0)
            AutoQueue = default;

        if (AutoQueue.Priority < ActionQueue.Priority.ManualEmergency)
        {
            if (Config.PyreticThreshold > 0 && _hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic && _hints.ImminentSpecialMode.activation < _ws.FutureTime(Config.PyreticThreshold))
                AutoQueue = default; // do not execute non-emergency actions when pyretic is imminent

            if (_hints.FindEnemy(AutoQueue.Target)?.Priority == AIHints.Enemy.PriorityForbidden)
                AutoQueue = default; // or if selected target is forbidden
        }
    }

    public Vector3? GetWorldPosUnderCursor()
    {
        Vector3 res = new();
        return _inst->GetGroundPositionForCursor(&res) ? res : null;
    }

    public void FaceDirection(Angle direction)
    {
        Service.Log($"face direction requested: {direction}");
        var player = (Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (player != null)
        {
            var position = player->Position.ToSystem() + direction.ToDirection().ToVec3();
            _inst->AutoFaceTargetPosition(&position);

            var pm = (PlayerMove*)player;
            // if rotation interpolation is in progress, we have to reset desired rotation to avoid game rotating us away next frame
            pm->Move.Interpolation.DesiredRotation = direction.Rad;
        }
    }

    public void GetCooldown(ref Cooldown result, RecastDetail* data)
    {
        if (data->IsActive)
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
        // [0, 80) are in ActionManager
        var rg = _inst->GetRecastGroupDetail(0);
        var i = 0;
        for (; i < 80; ++i)
            GetCooldown(ref cooldowns[i], rg++);

        // 80, 81 are in DutyActionManager
        rg = _inst->GetRecastGroupDetail(80);
        if (rg != null)
        {
            for (; i < 82; ++i)
                GetCooldown(ref cooldowns[i], rg++);
        }
        else
        {
            for (; i < 82; ++i)
                cooldowns[i] = default;
        }

        // [82,87) are in MassivePcContentDirector
        rg = _inst->GetRecastGroupDetail(82);
        if (rg != null)
        {
            for (; i < 87; ++i)
                GetCooldown(ref cooldowns[i], rg++);
        }
        else
        {
            for (; i < 87; ++i)
                cooldowns[i] = default;
        }
    }

    public float GCD()
    {
        var gcd = _inst->GetRecastGroupDetail(ActionDefinitions.GCDGroup);
        return gcd->Total - gcd->Elapsed;
    }

    public ClientState.DutyAction GetDutyAction(ushort slot)
    {
        // TODO: 7.1: there are now 5 actions, but only 2 charges...
        var dm = DutyActionManager.GetInstanceIfReady();

        (byte cur, byte max) charges(ushort slot) => slot < 2 ? (dm->CurCharges[slot], dm->MaxCharges[slot]) : default;

        if (dm == null || !dm->ActionActive[0] || slot >= dm->NumValidSlots)
            return default;

        var (cur, max) = charges(slot);
        return new(new(ActionType.Spell, dm->ActionId[slot]), cur, max);
    }
    public ClientState.DutyAction[] GetDutyActions() => [GetDutyAction(0), GetDutyAction(1), GetDutyAction(2), GetDutyAction(3), GetDutyAction(4)];

    public uint GetAdjustedActionID(uint actionID) => _inst->GetAdjustedActionId(actionID);

    public uint GetSpellIdForAction(ActionID action) => ActionManager.GetSpellIdForAction((CSActionType)action.Type, action.ID);

    public uint GetActionStatus(ActionID action, ulong target, bool checkRecastActive = true, bool checkCastingActive = true, uint* outOptExtraInfo = null)
    {
        if (action.Type is ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1)
            action = BozjaActionID.GetHolster(action.As<BozjaHolsterID>()); // see BozjaContentDirector.useFromHolster

        // the spell that corresponds with pomanders/magicite can't directly be used by players, so actionstatus will always be 579
        if (action.Type is ActionType.Pomander or ActionType.Magicite)
            return 0;

        return _inst->GetActionStatus((CSActionType)action.Type, action.ID, target, checkRecastActive, checkCastingActive, outOptExtraInfo);
    }

    // returns time in ms
    public int GetAdjustedCastTime(ActionID action, bool applyProcs = true, ActionManager.CastTimeProc* outOptProc = null)
        => ActionManager.GetAdjustedCastTime((CSActionType)action.Type, action.ID, applyProcs, outOptProc);

    public int GetAdjustedRecastTime(ActionID action, bool applyClassMechanics = true) => ActionManager.GetAdjustedRecastTime((CSActionType)action.Type, action.ID, applyClassMechanics);

    public bool CanMoveWhileCasting(ActionID action)
    {
        var player = (Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var inCombat = player->InCombat;

        return action switch
        {
            { Type: ActionType.Mount } => true,

            // phys ranged PVP actions
            { Type: ActionType.Spell, ID: 29391 or 29402 } => true,

            // player actions that can be cast instantly out of combat (1 RPR and all PCT motifs)
            { Type: ActionType.Spell, ID: 24387 or 34689 or 34664 or 34665 or 34690 or 34668 or 34669 or 34691 or 34667 or 34666 } => !inCombat,

            _ => false
        };
    }

    public bool IsRecastTimerActive(ActionID action)
        => _inst->IsRecastTimerActive((CSActionType)action.Type, action.ID);

    public int GetRecastGroup(ActionID action)
        => _inst->GetRecastGroup((int)action.Type, action.ID);

    // see ActionEffectHandler.Receive - there are a few hardcoded actions here
    private bool ExpectAnimationLockUpdate(ActionEffectHandler.Header* header)
        => header->SourceSequence != 0 && !(header->ActionType == CSActionType.Action && (NIN.AID)header->ActionId is NIN.AID.Ten1 or NIN.AID.Chi1 or NIN.AID.Jin1 or NIN.AID.Ten2 or NIN.AID.Chi2 or NIN.AID.Jin2)
        || header->ForceAnimationLock;

    // perform some action transformations to simplify implementation of queueing; UseActionLocation expects some normalization to be already done
    private ActionID NormalizeActionForQueue(ActionID action)
    {
        switch (action.Type)
        {
            case ActionType.Spell:
                // for normal actions, we want to do adjustment immediately, before action is queued; there are several reasons for that:
                // 1. transformation can affect action targeting (eg MNK meditation/chakra); queue will check the properties of the queued action
                // 2. for classes that start at high level and then are synced down, the 'base' action can be some upgrade; the queue will ignore non-adjusted actions, assuming they aren't unlocked yet
                // note that when action is executed several frames later, the adjustment will be done again, in case something changes in the state
                return new(ActionType.Spell, GetAdjustedActionID(action.ID));
            case ActionType.General:
                // for general actions, we want to convert things we care about to spells; UseActionLocation will expect that to be done
                if (action == ActionDefinitions.IDGeneralLimitBreak)
                {
                    var lb = LimitBreakController.Instance();
                    var level = lb->BarUnits != 0 ? lb->CurrentUnits / lb->BarUnits : 0;
                    var id = level > 0 ? lb->GetActionId((Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value, (byte)(level - 1)) : 0;
                    return id != 0 ? new(ActionType.Spell, id) : action;
                }
                // special case for lunar sprint, copied from UseGeneralAction
                else if (action == ActionDefinitions.IDGeneralSprint && GameMain.Instance()->CurrentTerritoryIntendedUseId == 60)
                {
                    return new(ActionType.Spell, 43357);
                }
                else if (action == ActionDefinitions.IDGeneralSprint || action == ActionDefinitions.IDGeneralDuty1 || action == ActionDefinitions.IDGeneralDuty2)
                {
                    return new(ActionType.Spell, GetSpellIdForAction(action));
                }
                else
                {
                    return action;
                }
            default:
                return action;
        }
    }

    // skips queueing etc
    private bool ExecuteAction(ActionID action, ulong targetId, Vector3 targetPos)
    {
        switch (action.Type)
        {
            case ActionType.Spell:
                // for spells, execute our UAL hook
                // note that for 'summon carbuncle/eos/titan/ifrit/garuda' actions, extraParam can be used to select glamour; the function will return 0 for non-summon actions
                return _inst->UseActionLocation(CSActionType.Action, action.ID, targetId, &targetPos, ActionManager.GetExtraParamForSummonAction(action.ID));
            case ActionType.Item:
                // note that for items extraParam should be 0xFFFF (since we want to use any item, not from first inventory slot)
                return _inst->UseActionLocation(CSActionType.Item, action.ID, targetId, &targetPos, 0xFFFFu);
            case ActionType.General:
                // TODO: are there any general actions that require (or even work with) UAL?
                // 23 Dismount does not, haven't tested others
                return _useActionHook.Original(_inst, CSActionType.GeneralAction, action.ID, targetId, 0, ActionManager.UseActionMode.None, 0, null);
            case ActionType.PetAction:
                if (action.ID == 3)
                {
                    // pet action "Place" - uses location targeting but doesn't interact with UseActionLocation at all, meaning it requires its own send-packet function
                    var now = DateTime.Now;
                    if (_nextAllowedExecuteCommand > now)
                        return false;
                    _nextAllowedExecuteCommand = now.AddMilliseconds(100);
                    _executeCommandGT(1800, &targetPos, action.ID, 0, 0, 0);
                    return true;
                }
                else
                {
                    // all other pet actions can be used as normal through UA (not UAL)
                    // TODO: consider calling UsePetAction instead?..
                    return _useActionHook.Original(_inst, CSActionType.PetAction, action.ID, targetId, 0, ActionManager.UseActionMode.None, 0, null);
                }

            // fake action types
            case ActionType.BozjaHolsterSlot0:
            case ActionType.BozjaHolsterSlot1:
                return UseBozjaHolsterNative(action);
            case ActionType.Pomander:
                UsePomanderNative(action);
                return true;
            case ActionType.Magicite:
                UseStoneNative(action);
                return true;

            default:
                // fall back to UAL hook for everything not covered explicitly
                return _inst->UseActionLocation((CSActionType)action.Type, action.ID, targetId, &targetPos, 0);
        }
    }

    private Angle? CalculateDesiredOrientation(bool actionImminent)
    {
        if (actionImminent && AutoQueue.FacingAngle != null)
            return AutoQueue.FacingAngle; // explicit angle overrides all other concerns

        var player = (Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (player == null)
            return null;
        var current = player->Rotation.Radians();

        // gaze avoidance & targeting
        // note: to execute an oriented action (cast a spell or use instant), target has to be within 45 degrees of character orientation (reversed)
        // to finish a spell without interruption, by the beginning of the slide-cast window target has to be within 75 degrees of character orientation (empirical)
        var castInfo = player->GetCastInfo();
        // with <500ms remaining on cast timer, player can face and move wherever they want and still complete the cast successfully (slidecast)
        var isCasting = castInfo != null && castInfo->IsCasting && castInfo->CurrentCastTime + 0.5f < castInfo->TotalCastTime;
        var currentAction = isCasting ? new((ActionType)castInfo->ActionType, castInfo->ActionId) : actionImminent ? AutoQueue.Action : default;
        var currentTargetId = isCasting ? (ulong)castInfo->TargetId : (AutoQueue.Target?.InstanceID ?? 0xE0000000);
        var currentTargetSelf = currentTargetId == player->EntityId;
        var currentTargetObj = currentTargetSelf ? &player->GameObject : currentTargetId is not 0 and not 0xE0000000 ? GameObjectManager.Instance()->Objects.GetObjectByGameObjectId(currentTargetId) : null;
        WPos? currentTargetPos = currentTargetObj != null ? new WPos(currentTargetObj->Position.X, currentTargetObj->Position.Z) : null;
        var currentTargetLoc = isCasting ? new WPos(castInfo->TargetLocation.X, castInfo->TargetLocation.Z) : new(AutoQueue.TargetPos.XZ()); // note: this only matters for area-targeted spells, for which targetlocation in castinfo is set correctly
        var idealOrientation = currentAction ? _smartRotationTweak.GetSpellOrientation(GetSpellIdForAction(currentAction), new(player->Position.X, player->Position.Z), currentTargetSelf, currentTargetPos, currentTargetLoc) : null;
        // avoiding a gaze has a priority over restore
        return _smartRotationTweak.GetSafeRotation(current, idealOrientation, isCasting ? 75.Degrees() : 45.Degrees());
    }

    private void UpdateDetour(ActionManager* self)
    {
        var fwk = Framework.Instance();
        var dt = fwk->GameSpeedMultiplier * fwk->FrameDeltaTime;
        var imminentAction = _inst->ActionQueued ? QueuedAction : AutoQueue.Action;
        var imminentActionAdj = imminentAction.Type == ActionType.Spell ? new(ActionType.Spell, GetAdjustedActionID(imminentAction.ID)) : imminentAction;
        var imminentRecast = imminentActionAdj ? _inst->GetRecastGroupDetail(GetRecastGroup(imminentActionAdj)) : null;

        _cooldownTweak.StartAdjustment(_inst->AnimationLock, imminentRecast != null && imminentRecast->IsActive ? imminentRecast->Total - imminentRecast->Elapsed : 0, dt);
        _updateHook.Original(self);

        // check whether movement is safe; block movement if not and if desired
        MoveMightInterruptCast &= CastTimeRemaining > 0; // previous cast could have ended without action effect
        // if we're not casting, but will start soon, moving might interrupt future cast
        if (imminentActionAdj && CastTimeRemaining <= 0 && _inst->AnimationLock < 0.1f && GetAdjustedCastTime(imminentActionAdj) > 0 && !CanMoveWhileCasting(imminentActionAdj) && GCD() < 0.1f)
        {
            // check LoS on target; blocking movement can cause AI mode to get stuck behind a wall trying to cast a spell on an unreachable target forever
            MoveMightInterruptCast |= CheckActionLoS(imminentAction, _inst->ActionQueued ? _inst->QueuedTargetId : (AutoQueue.Target?.InstanceID ?? 0));
        }
        bool blockMovement = Config.PreventMovingWhileCasting && MoveMightInterruptCast && _ws.Party.Player()?.MountId == 0;
        blockMovement |= Config.PyreticThreshold > 0 && _hints.ImminentSpecialMode.mode is AIHints.SpecialMode.Pyretic or AIHints.SpecialMode.PyreticMove && _hints.ImminentSpecialMode.activation < _ws.FutureTime(Config.PyreticThreshold);

        // note: if we cancel movement and start casting immediately, it will be canceled some time later - instead prefer to delay for one frame
        bool actionImminent = EffectiveAnimationLock <= 0 && AutoQueue.Action && !IsRecastTimerActive(AutoQueue.Action) && !(blockMovement && _movement.IsMoving());
        var desiredRotation = CalculateDesiredOrientation(actionImminent);

        // execute rotation, if needed
        var autoRotateConfig = fwk->SystemConfig.GetConfigOption((uint)ConfigOption.AutoFaceTargetOnAction);
        var autoRotateOriginal = autoRotateConfig->Value.UInt;
        if (desiredRotation != null)
        {
            autoRotateConfig->Value.UInt = 1;
            FaceDirection(desiredRotation.Value);
        }

        if (actionImminent)
        {
            var actionAdj = NormalizeActionForQueue(AutoQueue.Action);
            var targetID = AutoQueue.Target?.InstanceID ?? 0xE0000000;
            var status = GetActionStatus(actionAdj, targetID);
            if (status == 0)
            {
                // disable in-game auto rotation, to prevent fucking up with our logic
                autoRotateConfig->Value.UInt = _smartRotationTweak.Enabled ? 0 : autoRotateOriginal;
                var res = ExecuteAction(actionAdj, targetID, AutoQueue.TargetPos);
                //Service.Log($"[AMEx] Auto-execute {AutoQueue.Source} action {AutoQueue.Action} (=> {actionAdj}) @ {targetID:X} {Utils.Vec3String(AutoQueue.TargetPos)} => {res}");
            }
            else if (_dismountTweak.IsMountPreventingAction(actionAdj))
            {
                Service.Log("[AMEx] Trying to dismount...");
                _hints.WantDismount |= _dismountTweak.AutoDismountEnabled;
            }
            else
            {
                Service.Log($"[AMEx] Can't execute prio {AutoQueue.Priority} action {AutoQueue.Action} (=> {actionAdj}) @ {targetID:X}: status {status} '{Service.LuminaRow<Lumina.Excel.Sheets.LogMessage>(status)?.Text}'");
                blockMovement = false;
            }
        }

        autoRotateConfig->Value.UInt = autoRotateOriginal;
        _cooldownTweak.StopAdjustment(); // clear any potential adjustments
        _movement.MovementBlocked = blockMovement;

        // TODO: what's the reason to do it in AM update, rather than plugin's executehints?..
        if (_ws.Party.Player()?.CastInfo != null && _cancelCastTweak.ShouldCancel(_ws.CurrentTime, _hints.ForceCancelCast))
            UIState.Instance()->Hotbar.CancelCast();

        var autosEnabled = UIState.Instance()->WeaponState.AutoAttackState.IsAutoAttacking;
        if (_autoAutosTweak.GetDesiredState(autosEnabled, _ws.Party.Player()?.TargetID ?? 0) != autosEnabled)
            _inst->UseAction(CSActionType.GeneralAction, 1);

        if (_hints.WantDismount && !_movement.FollowPathActive() && _dismountTweak.AllowDismount())
            _inst->UseAction(CSActionType.Action, 4);
    }

    // note: targetId is usually your current primary target (or 0xE0000000 if you don't target anyone), unless you do something like /ac XXX <f> etc
    private bool UseActionDetour(ActionManager* self, CSActionType actionType, uint actionId, ulong targetId, uint extraParam, ActionManager.UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted)
    {
        var action = new ActionID((ActionType)actionType, actionId);
        //Service.Log($"[AMEx] UA: {action} @ {targetId:X}: {extraParam} {mode} {comboRouteId}");
        action = NormalizeActionForQueue(action);
        var spellId = GetSpellIdForAction(action);

        var targetSystem = TargetSystem.Instance();

        // if mouseover mode is enabled AND target is a usual primary target AND current mouseover is valid target for action, then we override target to mouseover
        var primaryTarget = targetSystem->Target;
        var primaryTargetId = primaryTarget != null ? primaryTarget->GetGameObjectId() : 0xE0000000;
        bool targetOverridden = targetId != primaryTargetId;
        if (Config.PreferMouseover && !targetOverridden)
        {
            var mouseoverTarget = PronounModule.Instance()->UiMouseOverTarget;
            if (mouseoverTarget != null && ActionManager.CanUseActionOnTarget(spellId, mouseoverTarget))
            {
                targetId = mouseoverTarget->GetGameObjectId();
                targetOverridden = true;
            }
        }

        (ulong, Vector3?) getAreaTarget() => targetOverridden ? (targetId, null) :
            (Config.GTMode == ActionTweaksConfig.GroundTargetingMode.AtTarget ? targetId : 0xE0000000, Config.GTMode == ActionTweaksConfig.GroundTargetingMode.AtCursor ? GetWorldPosUnderCursor() : null);

        ulong findNearestTarget()
        {
            if (Framework.Instance()->SystemConfig.GetConfigOption((uint)ConfigOption.AutoNearestTarget)->Value.UInt == 1)
            {
                _autoSelectTarget(targetSystem);
                if (targetSystem->Target != null)
                    return targetSystem->Target->GetGameObjectId();
            }

            return 0xE0000000;
        }

        // note: only standard mode can be filtered
        // note: current implementation introduces slight input lag (on button press, next autorotation update will pick state updates, which will be executed on next action manager update)
        if (mode == ActionManager.UseActionMode.None && action.Type is ActionType.Spell or ActionType.Item && _manualQueue.Push(action, targetId, GetAdjustedCastTime(action) * 0.001f, !targetOverridden, getAreaTarget, findNearestTarget))
            return false;

        bool areaTargeted = false;
        var res = _useActionHook.Original(self, actionType, actionId, targetId, extraParam, mode, comboRouteId, &areaTargeted);
        if (outOptAreaTargeted != null)
            *outOptAreaTargeted = areaTargeted;
        if (areaTargeted && Config.GTMode == ActionTweaksConfig.GroundTargetingMode.AtCursor)
            self->AreaTargetingExecuteAtCursor = true;
        if (areaTargeted && Config.GTMode == ActionTweaksConfig.GroundTargetingMode.AtTarget)
            self->AreaTargetingExecuteAtObject = targetId;
        return res;
    }

    private bool UseActionLocationDetour(ActionManager* self, CSActionType actionType, uint actionId, ulong targetId, Vector3* location, uint extraParam, byte a7)
    {
        var targetSystem = TargetSystem.Instance();
        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var prevSeq = _inst->LastUsedActionSequence;
        var prevRot = player != null ? player->Rotation.Radians() : default;
        var hardTarget = targetSystem->Target;
        var preventAutos = _autoAutosTweak.ShouldPreventAutoActivation(ActionManager.GetSpellIdForAction(actionType, actionId));
        if (preventAutos)
            targetSystem->Target = null;
        bool ret = _useActionLocationHook.Original(self, actionType, actionId, targetId, location, extraParam, a7);
        if (preventAutos)
            targetSystem->Target = hardTarget;
        var currSeq = _inst->LastUsedActionSequence;
        var currRot = player != null ? player->Rotation.Radians() : default;
        if (currSeq != prevSeq)
        {
            HandleActionRequest(new((ActionType)actionType, actionId), currSeq, targetId, *location, prevRot, currRot);
        }
        return ret;
    }

    private Angle GetPlayerRotation()
    {
        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        return player != null ? player->Rotation.Radians() : default;
    }

    private bool UseBozjaFromHolsterDirectorDetour(PublicContentBozja* self, uint holsterIndex, uint slot)
    {
        var state = PublicContentBozja.GetState();
        if (state == null)
            return false;
        var action = new ActionID(slot == 0 ? ActionType.BozjaHolsterSlot0 : ActionType.BozjaHolsterSlot1, state->HolsterActions[(int)holsterIndex]);

        if (_manualQueue.Push(action, 0xE0000000, 0, false, () => (0, null), () => 0xE0000000))
            return true;

        return UseBozjaHolsterNative(action);
    }

    private bool UseBozjaHolsterNative(ActionID action)
    {
        var state = PublicContentBozja.GetState();
        var ix = state != null ? state->HolsterActions.IndexOf((byte)action.ID) : -1;
        var prevRot = GetPlayerRotation();
        if (ix >= 0 && _useBozjaFromHolsterDirectorHook.Original(PublicContentBozja.GetInstance(), (uint)ix, action.Type == ActionType.BozjaHolsterSlot1 ? 1u : 0))
        {
            _inst->AnimationLock = 2.1f;
            HandleActionRequest(action, 0, 0xE0000000, default, prevRot, GetPlayerRotation());
            return true;
        }

        return false;
    }

    private void UsePomanderDetour(InstanceContentDeepDungeon* self, uint slot)
    {
        var action = _ws.DeepDungeon.GetPomanderActionID((int)slot);
        if (_manualQueue.Push(action, 0xE0000000, 0, false, () => (0, null), () => 0xE0000000))
            return;

        UsePomanderNative(action);
    }

    private void UsePomanderNative(ActionID action)
    {
        if (_inst->AnimationLock > 0)
            return;

        var dd = EventFramework.Instance()->GetInstanceContentDeepDungeon();
        var slot = _ws.DeepDungeon.GetPomanderSlot((PomanderID)action.ID);
        if (dd != null && slot >= 0)
        {
            var prevRot = GetPlayerRotation();
            _usePomanderHook.Original(dd, (uint)slot);
            _inst->AnimationLock = 2.1f;
            HandleActionRequest(action, 0, 0xE0000000, default, prevRot, GetPlayerRotation());
        }
    }

    private void UseStoneDetour(InstanceContentDeepDungeon* self, uint slot)
    {
        var action = new ActionID(ActionType.Magicite, slot + 1);
        if (_manualQueue.Push(action, 0xE0000000, 0, false, () => (0, null), () => 0xE0000000))
            return;

        UseStoneNative(action);
    }

    private void UseStoneNative(ActionID action)
    {
        if (_inst->AnimationLock > 0)
            return;

        var dd = EventFramework.Instance()->GetInstanceContentDeepDungeon();
        if (dd != null)
        {
            var prevRot = GetPlayerRotation();
            _useStoneHook.Original(dd, action.ID - 1);
            _inst->AnimationLock = 2.1f;
            HandleActionRequest(action, 0, 0xE0000000, default, prevRot, GetPlayerRotation());
        }
    }

    private void ProcessPacketActionEffectDetour(uint casterID, Character* casterObj, Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects, GameObjectId* targets)
    {
        // notify listeners about the event
        // note: there's a slight difference with dispatching event from here rather than from packet processing (ActionEffectN) functions
        // 1. action id is already unscrambled
        // 2. this function won't be called if caster object doesn't exist
        // the last point is deemed to be minor enough for us to not care, as it simplifies things (no need to hook 5 functions)
        var info = new ActorCastEvent(new((ActionType)header->ActionType, header->ActionId), header->AnimationTargetId, header->AnimationLock, header->NumTargets, *targetPos,
            header->GlobalSequence, header->SourceSequence, Network.PacketDecoder.IntToFloatAngle(header->RotationInt));
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

        if (casterID != UIState.Instance()->PlayerState.EntityId || !ExpectAnimationLockUpdate(header))
        {
            // this action is either executed by non-player, or is non-player-initiated
            // TODO: reconsider the condition:
            // - do we want to do non-anim-lock related things (eg unblock movement override) when we get action with 'force anim lock' flag?
            if (currAnimLock != prevAnimLock)
                Service.Log($"[AMEx] Animation lock updated by non-player-initiated action: #{header->SourceSequence} {casterID:X} {info.Action} {prevAnimLock:f3} -> {currAnimLock:f3}");
            return;
        }

        MoveMightInterruptCast = false; // slidecast window start
        _movement.MovementBlocked = false; // unblock input unconditionally on successful cast (I assume there are no instances where we need to immediately start next GCD?)

        // animation lock delay update
        var animLockReduction = _animLockTweak.Apply(header->SourceSequence, prevAnimLock, _inst->AnimationLock, packetAnimLock, header->AnimationLock, out var animLockDelay);
        _inst->AnimationLock -= animLockReduction;
        Service.Log($"[AMEx] AEP #{header->SourceSequence} {prevAnimLock:f3} {info.Action} -> ALock={currAnimLock:f3} (delayed by {animLockDelay:f3}) -> {_inst->AnimationLock:f3}), Flags={header->Flags:X}, CTR={CastTimeRemaining:f3}, GCD={GCD():f3}");
    }

    private void HandleActionRequest(ActionID action, uint seq, ulong targetID, Vector3 targetPos, Angle prevRot, Angle currRot)
    {
        _manualQueue.Pop(action);
        _animLockTweak.RecordRequest(seq, _inst->AnimationLock);
        MoveMightInterruptCast = CastTimeRemaining > 0 && !CanMoveWhileCasting(action);

        var recast = _inst->GetRecastGroupDetail(GetRecastGroup(action));

        if (CastTimeRemaining > 0)
            _inst->CastTimeElapsed += _cooldownTweak.Adjustment;
        else
            _inst->AnimationLock = Math.Max(0, _inst->AnimationLock - _cooldownTweak.Adjustment);

        if (recast != null)
            recast->Elapsed += _cooldownTweak.Adjustment;

        var (castElapsed, castTotal) = _inst->CastSpellId != 0 ? (_inst->CastTimeElapsed, _inst->CastTimeTotal) : (0, 0);
        var (recastElapsed, recastTotal) = recast != null ? (recast->Elapsed, recast->Total) : (0, 0);
        Service.Log($"[AMEx] UAL #{seq} {action} @ {targetID:X} / {Utils.Vec3String(targetPos)}, ALock={_inst->AnimationLock:f3}, CTR={CastTimeRemaining:f3}, CD={recastElapsed:f3}/{recastTotal:f3}, GCD={GCD():f3}");
        ActionRequestExecuted.Fire(new(action, targetID, targetPos, seq, _inst->AnimationLock, castElapsed, castTotal, recastElapsed, recastTotal));
    }

    // note: we can't rely on worldstate target id, it might not be updated when this is called
    // TODO: current implementation means that we'll check desired state twice (once before making a decision to start autos, then again in the hook)
    private bool SetAutoAttackStateDetour(AutoAttackState* self, bool value, bool sendPacket, bool isInstant)
    {
        if (value && !_autoAutosTweak.GetDesiredState(true, TargetSystem.Instance()->GetTargetObjectId()))
        {
            Service.Log($"[AMEx] Prevented starting autoattacks");
            return true;
        }
        return _setAutoAttackStateHook.Original(self, value, sendPacket, isInstant);
    }

    // just the LoS portion of ActionManager::GetActionInRangeOrLoS (which also checks range, which we don't care about, and also checks facing angle, which we don't care about)
    private static bool CheckActionLoS(ActionID action, ulong targetID)
    {
        var row = action.Type == ActionType.Spell ? Service.LuminaRow<Lumina.Excel.Sheets.Action>(action.ID) : null;
        if (row == null)
            return true; // unknown action, assume nothing

        if (!row.Value.RequiresLineOfSight)
            return true;

        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var targetObj = GameObjectManager.Instance()->Objects.GetObjectByGameObjectId(targetID);
        if (targetObj == null || targetObj->EntityId == player->EntityId)
            return true;

        var playerPos = *player->GetPosition();
        var targetPos = *targetObj->GetPosition();

        playerPos.Y += 2;
        targetPos.Y += 2;

        var offset = targetPos - playerPos;
        var maxDist = offset.Magnitude;
        var direction = offset / maxDist;

        return !BGCollisionModule.RaycastMaterialFilter(playerPos, direction, out _, maxDist);
    }
}
