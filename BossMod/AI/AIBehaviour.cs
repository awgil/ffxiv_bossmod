using BossMod.Autorotation;
using BossMod.Pathfinding;
using Dalamud.Bindings.ImGui;

namespace BossMod.AI;

public record struct Targeting(AIHints.Enemy Target, float PreferredRange = 3, Positional PreferredPosition = Positional.Any, bool PreferTanking = false);

// constantly follow master
sealed class AIBehaviour(AIController ctrl, RotationModuleManager autorot) : IDisposable
{
    public WorldState WorldState => autorot.Bossmods.WorldState;
    public float ForceMovementIn { get; private set; } = float.MaxValue; // TODO: reconsider
    private readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly NavigationDecision.Context _naviCtx = new();
    private NavigationDecision _naviDecision;
    private bool _afkMode;
    private bool _followMaster; // if true, our navigation target is master rather than primary target - this happens e.g. in outdoor or in dungeons during gathering trash
    private WPos _masterPrevPos;
    private DateTime _masterLastMoved;
    private DateTime _navStartTime; // if current time is < this, navigation won't start

    public void Dispose()
    {
    }

    public void Execute(Actor player, Actor master)
    {
        ForceMovementIn = float.MaxValue;
        if (player.IsDead)
            return;

        // keep master in focus
        if (_config.FocusTargetMaster)
            FocusMaster(master);

        _afkMode = master != player && !master.InCombat && (WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 10;
        bool gazeImminent = autorot.Hints.ForbiddenDirections.Count > 0 && autorot.Hints.ForbiddenDirections[0].activation <= WorldState.FutureTime(0.5f);
        bool pyreticImminent = autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Pyretic && autorot.Hints.ImminentSpecialMode.activation <= WorldState.FutureTime(1);
        bool misdirectionMode = autorot.Hints.ImminentSpecialMode.mode == AIHints.SpecialMode.Misdirection && autorot.Hints.ImminentSpecialMode.activation <= WorldState.CurrentTime;
        bool forbidTargeting = _config.ForbidActions || _afkMode || gazeImminent || pyreticImminent;
        bool hadNavi = _naviDecision.Destination != null;

        Targeting target = new();
        if (!forbidTargeting)
        {
            target = SelectPrimaryTarget(player, master);
            if (target.Target != null || TargetIsForbidden(player.TargetID))
                autorot.Hints.ForcedTarget ??= target.Target?.Actor;
            AdjustTargetPositional(player, ref target);
        }

        _followMaster = master != player;

        // note: if there are pending knockbacks, don't update navigation decision to avoid fucking up positioning
        if (player.PendingKnockbacks.Count == 0)
        {
            _naviDecision = BuildNavigationDecision(player, master, ref target);
            // there is a difference between having a small positive leeway and having a negative one for pathfinding, prefer to keep positive
            _naviDecision.LeewaySeconds = Math.Max(0, _naviDecision.LeewaySeconds - 0.1f);
        }

        bool masterIsMoving = TrackMasterMovement(master);
        bool moveWithMaster = masterIsMoving && _followMaster && master != player;
        ForceMovementIn = moveWithMaster || gazeImminent || pyreticImminent ? 0 : _naviDecision.LeewaySeconds;

        if (_config.MoveDelay > 0 && !hadNavi && _naviDecision.Destination != null)
            _navStartTime = WorldState.FutureTime(_config.MoveDelay);

        UpdateMovement(player, master, target, gazeImminent || pyreticImminent, misdirectionMode ? autorot.Hints.MisdirectionThreshold : default, !forbidTargeting ? autorot.Hints.ActionsToExecute : null);
    }

    // returns null if we're to be idle, otherwise target to attack
    private Targeting SelectPrimaryTarget(Actor player, Actor master)
    {
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
        var targeting = new Targeting(target!, player.Role is Role.Melee or Role.Tank ? 2.9f : 24.5f);

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

        Actor? forceDestination = null;
        float forceDestinationRange = 2;
        if (_followMaster)
            forceDestination = master;
        else if (autorot.Hints.InteractWithTarget is Actor tar)
        {
            forceDestination = tar;
            forceDestinationRange = 3.5f;
        }

        if (forceDestination != null && autorot.Hints.PathfindMapBounds.Contains(forceDestination.Position - autorot.Hints.PathfindMapCenter))
        {
            autorot.Hints.GoalZones.Clear();
            autorot.Hints.GoalZones.Add(autorot.Hints.GoalSingleTarget(forceDestination, forceDestinationRange));
            return NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player);
        }

        // TODO: remove this once all rotation modules are fixed
        if (autorot.Hints.GoalZones.Count == 0 && targeting.Target != null)
            autorot.Hints.GoalZones.Add(autorot.Hints.GoalSingleTarget(targeting.Target.Actor, targeting.PreferredPosition, targeting.PreferredRange));

        return NavigationDecision.Build(_naviCtx, WorldState, autorot.Hints, player);
    }

    private void FocusMaster(Actor master)
    {
        bool masterChanged = Service.TargetManager.FocusTarget?.EntityId != master.InstanceID;
        if (masterChanged)
        {
            ctrl.SetFocusTarget(master);
            _masterPrevPos = master.Position;
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
            masterIsMoving = false;
        }
        // else: don't consider master to have stopped moving unless he's standing still for some small time

        return masterIsMoving;
    }

    private void UpdateMovement(Actor player, Actor master, Targeting target, bool gazeOrPyreticImminent, Angle misdirectionAngle, ActionQueue? queueForSprint)
    {
        if (gazeOrPyreticImminent)
        {
            // gaze or pyretic imminent, drop any movement - we should have moved to safe zone already...
            ctrl.NaviTargetPos = null;
            ctrl.NaviTargetVertical = null;
            ctrl.ForceCancelCast = true;
        }
        else if (misdirectionAngle != default && _naviDecision.Destination != null)
        {
            var turn = (_naviDecision.Destination.Value - player.Position).OrthoL().Dot((_naviDecision.NextWaypoint ?? _naviDecision.Destination).Value - _naviDecision.Destination.Value);
            ctrl.NaviTargetPos = turn == 0 ? _naviDecision.Destination
                : player.Position + (_naviDecision.Destination.Value - player.Position).Rotate(turn > 0 ? -misdirectionAngle : misdirectionAngle);
            ctrl.AllowInterruptingCastByMovement = true;

            // debug
            //void drawLine(WPos from, WPos to, uint color) => Camera.Instance!.DrawWorldLine(new(from.X, player.PosRot.Y, from.Z), new(to.X, player.PosRot.Y, to.Z), color);
            //var toDest = _naviDecision.Destination.Value - player.Position;
            //drawLine(player.Position, _naviDecision.Destination.Value, 0xff00ff00);
            //drawLine(_naviDecision.Destination.Value, _naviDecision.Destination.Value + toDest.Normalized().OrthoL(), 0xff00ff00);
            //drawLine(player.Position, ctrl.NaviTargetPos.Value, 0xff00ffff);
        }
        else
        {
            var toDest = _naviDecision.Destination != null ? _naviDecision.Destination.Value - player.Position : new();
            var distSq = toDest.LengthSq();
            ctrl.NaviTargetPos = WorldState.CurrentTime >= _navStartTime ? _naviDecision.Destination : null;
            ctrl.NaviTargetVertical = master != player ? master.PosRot.Y : null;
            ctrl.AllowInterruptingCastByMovement = player.CastInfo != null && _naviDecision.LeewaySeconds <= player.CastInfo.RemainingTime - 0.5;
            ctrl.ForceCancelCast = false;

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
        ImGui.Checkbox("Disable autotarget", ref _config.ForbidActions);
        ImGui.SameLine();
        ImGui.Checkbox("Disable movement", ref _config.ForbidMovement);
    }

    private bool TargetIsForbidden(ulong actorId) => autorot.Hints.ForbiddenTargets.Any(e => e.Actor.InstanceID == actorId);
}
