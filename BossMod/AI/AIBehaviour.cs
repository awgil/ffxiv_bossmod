using BossMod.Autorotation;
using BossMod.Pathfinding;
using Dalamud.Interface.Utility;
using ImGuiNET;

namespace BossMod.AI;

public record struct Targeting(AIHints.Enemy Target, float PreferredRange = 3, Positional PreferredPosition = Positional.Any, bool PreferTanking = false);

// constantly follow master
sealed class AIBehaviour(AIController ctrl, RotationModuleManager autorot, Preset? aiPreset) : IDisposable
{
    public WorldState WorldState => autorot.Bossmods.WorldState;
    public Preset? AIPreset = aiPreset;
    public float ForceMovementIn { get; private set; } = float.MaxValue; // TODO: reconsider
    private readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly NavigationDecision.Context _naviCtx = new();
    private NavigationDecision _naviDecision;
    private bool _afkMode;
    private bool _followMaster; // if true, our navigation target is master rather than primary target - this happens e.g. in outdoor or in dungeons during gathering trash
    private WPos _masterPrevPos;
    private WPos _masterMovementStart;
    private DateTime _masterLastMoved;

    public void Dispose()
    {
    }

    public void Execute(Actor player, Actor master)
    {
        ForceMovementIn = float.MaxValue;
        if (player.IsDead || ctrl.InCutscene)
            return;

        // keep master in focus
        if (_config.FocusTargetMaster)
            FocusMaster(master);

        _afkMode = !master.InCombat && (WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 10;
        bool gazeImminent = autorot.Hints.ForbiddenDirections.Count > 0 && autorot.Hints.ForbiddenDirections[0].activation <= WorldState.FutureTime(0.5f);
        bool pyreticImminent = autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic && autorot.Hints.ImminentSpecialMode.activation <= WorldState.FutureTime(1);
        bool forbidActions = _config.ForbidActions || _afkMode || gazeImminent || pyreticImminent || autorot.Preset != null && autorot.Preset != AIPreset;

        Targeting target = new();
        if (!forbidActions)
        {
            target = SelectPrimaryTarget(player, master);
            if (target.Target != null || TargetIsForbidden(player.TargetID))
                autorot.Hints.ForcedTarget ??= target.Target?.Actor;
            AdjustTargetPositional(player, ref target);
        }

        if (_config.OverridePositional)
        {
            target.PreferredPosition = _config.DesiredPositional;
            target.PreferTanking = _config.DesiredPositional != Positional.Any;
        }
        if (_config.OverrideRange)
            target.PreferredRange = _config.MaxDistanceToTarget;

        _followMaster = master != player && (autorot.Bossmods.ActiveModule?.StateMachine.ActiveState == null || _config.FollowDuringActiveBossModule) && (!master.InCombat || _config.FollowDuringCombat || (_masterPrevPos - _masterMovementStart).LengthSq() > 100) && (player.InCombat || _config.FollowOutOfCombat);

        // note: if there are pending knockbacks, don't update navigation decision to avoid fucking up positioning
        if (!WorldState.PendingEffects.PendingKnockbacks(player.InstanceID))
        {
            _naviDecision = BuildNavigationDecision(player, master, ref target);
            // there is a difference between having a small positive leeway and having a negative one for pathfinding, prefer to keep positive
            _naviDecision.LeewaySeconds = Math.Max(0, _naviDecision.LeewaySeconds - 0.1f);
        }

        bool masterIsMoving = TrackMasterMovement(master);
        bool moveWithMaster = masterIsMoving && (master == player || _followMaster);
        ForceMovementIn = moveWithMaster || gazeImminent || pyreticImminent ? 0 : _naviDecision.LeewaySeconds;

        // note: that there is a 1-frame delay if target and/or strategy changes - we don't really care?..
        if (!forbidActions)
        {
            autorot.Preset = target.Target != null ? AIPreset : null;
        }

        UpdateMovement(player, master, target, gazeImminent || pyreticImminent, !forbidActions ? autorot.Hints.ActionsToExecute : null);
    }

    // returns null if we're to be idle, otherwise target to attack
    private Targeting SelectPrimaryTarget(Actor player, Actor master)
    {
        if (autorot.Hints.InteractWithTarget is Actor interact)
            return new Targeting(new AIHints.Enemy(interact, false), 3);

        // we prefer not to switch targets unnecessarily, so start with current target - it could've been selected manually or by AI on previous frames
        // if current target is not among valid targets, clear it - this opens way for future target selection heuristics
        var targetId = autorot.Hints.ForcedTarget?.InstanceID ?? player.TargetID;
        var target = autorot.Hints.PriorityTargets.FirstOrDefault(e => e.Actor.InstanceID == targetId);

        // if we don't have a valid target yet, use some heuristics to select some 'ok' target to attack
        // try assisting master, otherwise (if player is own master, or if master has no valid target) just select closest valid target
        target ??= master != player ? autorot.Hints.PriorityTargets.FirstOrDefault(t => master.TargetID == t.Actor.InstanceID) : null;
        target ??= autorot.Hints.PriorityTargets.MinBy(e => (e.Actor.Position - player.Position).LengthSq());

        // if the previous line returned no target, there aren't any priority targets at all - give up
        if (target == null)
            return new();

        // TODO: rethink all this... ai module should set forced target if it wants to switch... figure out positioning and stuff
        // now give class module a chance to improve targeting
        // typically it would switch targets for multidotting, or to hit more targets with AOE
        // in case of ties, it should prefer to return original target - this would prevent useless switches
        var targeting = new Targeting(target!, autorot.Hints.RecommendedRangeToTarget - 0.1f);

        var pos = autorot.Hints.RecommendedPositional;
        if (pos.Target != null && targeting.Target.Actor == pos.Target)
            targeting.PreferredPosition = pos.Pos;

        return /*autorot.SelectTargetForAI(targeting) ??*/ targeting;
    }

    private void AdjustTargetPositional(Actor player, ref Targeting targeting)
    {
        if (targeting.Target == null || targeting.PreferredPosition == Positional.Any)
            return; // nothing to adjust

        if (targeting.PreferredPosition == Positional.Front)
        {
            // 'front' is tank-specific positional; no point doing anything if we're not tanking target
            if (targeting.Target.Actor.TargetID != player.InstanceID)
                targeting.PreferredPosition = Positional.Any;
            return;
        }

        // if target-of-target is player, don't try flanking, it's probably impossible... - unless target is currently casting (TODO: reconsider?)
        // skip if targeting a dummy, they don't rotate
        if (targeting.Target.Actor.TargetID == player.InstanceID && targeting.Target.Actor.CastInfo == null && targeting.Target.Actor.OID != 0x385)
            targeting.PreferredPosition = Positional.Any;
    }

    private NavigationDecision BuildNavigationDecision(Actor player, Actor master, ref Targeting targeting)
    {
        if (_config.ForbidMovement)
            return new() { LeewaySeconds = float.MaxValue };
        if (_followMaster)
            return NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player, master.Position, _config.OverrideRange ? _config.MaxDistanceToSlot : 1, new(), Positional.Any);
        if (targeting.Target == null)
            return NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player, null, 0, new(), Positional.Any);

        var adjRange = targeting.PreferredRange + player.HitboxRadius + targeting.Target.Actor.HitboxRadius;
        if (targeting.PreferTanking)
        {
            // see whether we need to move target
            // TODO: think more about keeping uptime while tanking, this is tricky...
            var desiredToTarget = targeting.Target.Actor.Position - targeting.Target.DesiredPosition;
            if (desiredToTarget.LengthSq() > 4 /*&& (_autorot.ClassActions?.GetState().GCD ?? 0) > 0.5f*/)
            {
                var dest = autorot.Hints.ClampToBounds(targeting.Target.DesiredPosition - adjRange * desiredToTarget.Normalized());
                return NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player, dest, 0.5f, new(), Positional.Any);
            }
        }

        var adjRotation = targeting.PreferTanking ? targeting.Target.DesiredRotation : targeting.Target.Actor.Rotation;
        return NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player, targeting.Target.Actor.Position, adjRange, adjRotation, targeting.PreferredPosition);
    }

    private void FocusMaster(Actor master)
    {
        bool masterChanged = Service.TargetManager.FocusTarget?.EntityId != master.InstanceID;
        if (masterChanged)
        {
            ctrl.SetFocusTarget(master);
            _masterPrevPos = _masterMovementStart = master.Position;
            _masterLastMoved = WorldState.CurrentTime.AddSeconds(-1);
        }
    }

    private bool TrackMasterMovement(Actor master)
    {
        // keep track of master movement
        // idea is that if master is moving forward (e.g. running in outdoor or pulling trashpacks in dungeon), we want to closely follow and not stop to cast
        bool masterIsMoving = true;
        if (master.Position != _masterPrevPos)
        {
            _masterLastMoved = WorldState.CurrentTime;
            _masterPrevPos = master.Position;
        }
        else if ((WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 0.5f)
        {
            // master has stopped, consider previous movement finished
            _masterMovementStart = _masterPrevPos;
            masterIsMoving = false;
        }
        // else: don't consider master to have stopped moving unless he's standing still for some small time

        return masterIsMoving;
    }

    private void UpdateMovement(Actor player, Actor master, Targeting target, bool gazeOrPyreticImminent, ActionQueue? queueForSprint)
    {
        if (gazeOrPyreticImminent)
        {
            // gaze or pyretic imminent, drop any movement - we should have moved to safe zone already...
            ctrl.NaviTargetPos = null;
            ctrl.NaviTargetVertical = null;
            ctrl.ForceCancelCast = true;
        }
        else
        {
            var toDest = _naviDecision.Destination != null ? _naviDecision.Destination.Value - player.Position : new();
            var distSq = toDest.LengthSq();
            ctrl.NaviTargetPos = _naviDecision.Destination;
            ctrl.NaviTargetVertical = master != player ? master.PosRot.Y : null;
            ctrl.AllowInterruptingCastByMovement = player.CastInfo != null && _naviDecision.LeewaySeconds <= player.CastInfo.RemainingTime - 0.5;
            ctrl.ForceCancelCast = false;
            ctrl.WantJump = distSq >= 0.01f && autorot.Bossmods.ActiveModule?.StateMachine.ActiveState != null && autorot.Bossmods.ActiveModule.NeedToJump(player.Position, toDest.Normalized());

            //var cameraFacing = _ctrl.CameraFacing;
            //var dot = cameraFacing.Dot(_ctrl.TargetRot.Value);
            //if (dot < -0.707107f)
            //    _ctrl.TargetRot = -_ctrl.TargetRot.Value;
            //else if (dot < 0.707107f)
            //    _ctrl.TargetRot = cameraFacing.OrthoL().Dot(_ctrl.TargetRot.Value) > 0 ? _ctrl.TargetRot.Value.OrthoR() : _ctrl.TargetRot.Value.OrthoL();

            // sprint, if not in combat and far enough away from destination
            if (player.InCombat ? _naviDecision.LeewaySeconds <= 0 && distSq > 25 : player != master && distSq > 400)
            {
                queueForSprint?.Push(ActionDefinitions.IDSprint, player, ActionQueue.Priority.Minimal + 100);
            }
        }
    }

    public void DrawDebug()
    {
        ImGui.Checkbox("Forbid actions", ref _config.ForbidActions);
        ImGui.SameLine();
        ImGui.Checkbox("Forbid movement", ref _config.ForbidMovement);
        ImGui.SameLine();
        ImGui.Checkbox("Show extra options", ref _config.ShowExtraUIOptions);
        if (_config.ShowExtraUIOptions)
        {
            ImGui.Checkbox("Follow during combat", ref _config.FollowDuringCombat);
            ImGui.SameLine();
            ImGui.Checkbox("Follow during active boss module", ref _config.FollowDuringActiveBossModule);
            ImGui.Spacing();
            ImGui.Checkbox("Follow out of combat", ref _config.FollowOutOfCombat);
            ImGui.SameLine();
            ImGui.Checkbox("Follow target", ref _config.FollowTarget);
            ImGui.SameLine();
            ImGui.Checkbox("Override follow range", ref _config.OverrideRange);
            ImGui.PushItemWidth(75 * ImGuiHelpers.GlobalScale);
            ImGui.InputFloat("Follow slot range", ref _config.MaxDistanceToSlot);
            ImGui.SameLine();
            ImGui.InputFloat("Follow target range", ref _config.MaxDistanceToTarget);
            ImGui.PopItemWidth();
        }
        var player = WorldState.Party.Player();
        var dist = _naviDecision.Destination != null && player != null ? (_naviDecision.Destination.Value - player.Position).Length() : 0;
        ImGui.TextUnformatted($"Max-cast={MathF.Min(ForceMovementIn, 1000):f3}, afk={_afkMode}, follow={_followMaster}, \n{_naviDecision.Destination} (d={dist:f3}), master standing for {Math.Clamp((WorldState.CurrentTime - _masterLastMoved).TotalSeconds, 0, 1000):f1}");
    }

    private bool TargetIsForbidden(ulong actorId) => autorot.Hints.ForbiddenTargets.Any(e => e.Actor.InstanceID == actorId);
}
