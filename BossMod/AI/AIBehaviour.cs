using BossMod.Pathfinding;
using ImGuiNET;
using System;
using System.Linq;

namespace BossMod.AI
{
    // constantly follow master
    class AIBehaviour : IDisposable
    {
        private Autorotation _autorot;
        private AIController _ctrl;
        private NavigationDecision _naviDecision;
        private bool _forbidMovement;
        private bool _forbidActions;
        private bool _afkMode;
        private bool _followMaster; // if true, our navigation target is master rather than primary target - this happens e.g. in outdoor or in dungeons during gathering trash
        private float _maxCastTime;
        private WPos _masterPrevPos;
        private WPos _masterMovementStart;
        private DateTime _masterLastMoved;

        public AIBehaviour(AIController ctrl, Autorotation autorot)
        {
            _autorot = autorot;
            _ctrl = ctrl;
        }

        public void Dispose()
        {
        }

        public void Execute(Actor player, Actor master)
        {
            if (_ctrl.InCutscene)
                return;

            // keep master in focus
            FocusMaster(master);

            _afkMode = !master.InCombat && (_autorot.WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 10;
            bool forbidActions = _forbidActions || _ctrl.IsMounted || _afkMode || _autorot.ClassActions == null || _autorot.ClassActions.AutoAction >= CommonActions.AutoActionFirstCustom;

            var target = forbidActions ? new() : SelectPrimaryTarget(player, master);
            if (!forbidActions)
                _autorot.PrimaryTarget = target.Target;
            if (target.Target != null)
                _ctrl.SetPrimaryTarget(target.Target);

            AdjustTargetPositional(player, ref target);

            _followMaster = master != player && _autorot.Bossmods.ActiveModule?.StateMachine.ActiveState == null && (!master.InCombat || (_masterPrevPos - _masterMovementStart).LengthSq() > 100);
            if (_forbidMovement)
                _naviDecision = new() { LeewaySeconds = float.MaxValue };
            else if (_followMaster)
                _naviDecision = NavigationDecision.Build(_autorot.WorldState, _autorot.Hints, player, master.Position, 2, new(), Positional.Any);
            else if (target.Target != null)
                _naviDecision = NavigationDecision.Build(_autorot.WorldState, _autorot.Hints, player, target.Target.Position, target.PreferredRange + player.HitboxRadius + target.Target.HitboxRadius, target.Target.Rotation, target.PreferredPosition);
            else
                _naviDecision = NavigationDecision.Build(_autorot.WorldState, _autorot.Hints, player, null, 0, new(), Positional.Any);

            bool masterIsMoving = TrackMasterMovement(master);
            bool moveWithMaster = masterIsMoving && (master == player || _followMaster);
            _maxCastTime = moveWithMaster || _ctrl.ForceFacing ? 0 : _naviDecision.LeewaySeconds;

            // note: that there is a 1-frame delay if target and/or strategy changes - we don't really care?..
            if (!forbidActions)
            {
                int actionStrategy = target.Target != null ? CommonActions.AutoActionAIFight : CommonActions.AutoActionAIIdle;
                _autorot.ClassActions?.UpdateAutoAction(actionStrategy, _maxCastTime);
            }

            UpdateMovement(player, master, target, !forbidActions);
        }

        // returns null if we're to be idle, otherwise target to attack
        private CommonActions.Targeting SelectPrimaryTarget(Actor player, Actor master)
        {
            if (!_autorot.Hints.PriorityTargets.Any() || !master.InCombat || _autorot.ClassActions == null)
                return new(); // there are no valid targets to attack, or we're not fighting - remain idle

            // we prefer not to switch targets unnecessarily, so start with current target - it could've been selected manually or by AI on previous frames
            var target = _autorot.PrimaryTarget;

            // if current target is not among valid targets, clear it - this opens way for future target selection heuristics
            if (target != null && !_autorot.Hints.PriorityTargetsActors.Contains(target))
                target = null;

            // if we don't have a valid target yet, use some heuristics to select some 'ok' target to attack
            // try assisting master, otherwise (if player is own master, or if master has no valid target) just select closest valid target
            target ??= master != player ? _autorot.Hints.PriorityTargetsActors.FirstOrDefault(t => master.TargetID == t.InstanceID) : null;
            target ??= _autorot.Hints.PriorityTargetsActors.Closest(player.Position);

            // now give class module a chance to improve targeting
            // typically it would switch targets for multidotting, or to hit more targets with AOE
            // in case of ties, it should prefer to return original target - this would prevent useless switches
            return _autorot.ClassActions.SelectBetterTarget(target!);
        }

        private void AdjustTargetPositional(Actor player, ref CommonActions.Targeting targeting)
        {
            if (targeting.Target == null || targeting.PreferredPosition == Positional.Any)
                return; // nothing to adjust

            // if target-of-target is player, don't try flanking, it's probably impossible... - unless target is currently casting (TODO: reconsider?)
            if (targeting.Target.TargetID == player.InstanceID && targeting.Target.CastInfo == null)
                targeting.PreferredPosition = Positional.Any;

            // TODO: check whether target ignores positionals...
        }

        private void FocusMaster(Actor master)
        {
            bool masterChanged = Service.TargetManager.FocusTarget?.ObjectId != master.InstanceID;
            if (masterChanged)
            {
                _ctrl.SetFocusTarget(master);
                _masterPrevPos = _masterMovementStart = master.Position;
                _masterLastMoved = _autorot.WorldState.CurrentTime.AddSeconds(-1);
            }
        }

        private bool TrackMasterMovement(Actor master)
        {
            // keep track of master movement
            // idea is that if master is moving forward (e.g. running in outdoor or pulling trashpacks in dungeon), we want to closely follow and not stop to cast
            bool masterIsMoving = true;
            if (master.Position != _masterPrevPos)
            {
                _masterLastMoved = _autorot.WorldState.CurrentTime;
                _masterPrevPos = master.Position;
            }
            else if ((_autorot.WorldState.CurrentTime - _masterLastMoved).TotalSeconds > 0.5f)
            {
                // master has stopped, consider previous movement finished
                _masterMovementStart = _masterPrevPos;
                masterIsMoving = false;
            }
            // else: don't consider master to have stopped moving unless he's standing still for some small time

            return masterIsMoving;
        }

        private void UpdateMovement(Actor player, Actor master, CommonActions.Targeting target, bool allowSprint)
        {
            var destRot = AvoidGaze.Update(player, target.Target?.Position, _autorot.Hints, _autorot.WorldState.CurrentTime.AddSeconds(0.5));
            if (destRot != null)
            {
                // rotation check imminent, drop any movement - we should have moved to safe zone already...
                _ctrl.NaviTargetPos = null;
                _ctrl.NaviTargetRot = destRot;
                _ctrl.NaviTargetVertical = null;
                _ctrl.ForceFacing = true;
            }
            else
            {
                var toDest = _naviDecision.Destination != null ? _naviDecision.Destination.Value - player.Position : new();
                var distSq = toDest.LengthSq();
                _ctrl.NaviTargetPos = _naviDecision.Destination;
                _ctrl.NaviTargetRot = distSq >= 0.04f ? toDest.Normalized() : null;
                _ctrl.NaviTargetVertical = master != player ? master.PosRot.Y : null;
                _ctrl.AllowInterruptingCastByMovement = player.CastInfo != null && _naviDecision.LeewaySeconds <= (player.CastInfo.FinishAt - _autorot.WorldState.CurrentTime).TotalSeconds - 0.5;
                _ctrl.ForceFacing = false;

                //var cameraFacing = _ctrl.CameraFacing;
                //var dot = cameraFacing.Dot(_ctrl.TargetRot.Value);
                //if (dot < -0.707107f)
                //    _ctrl.TargetRot = -_ctrl.TargetRot.Value;
                //else if (dot < 0.707107f)
                //    _ctrl.TargetRot = cameraFacing.OrthoL().Dot(_ctrl.TargetRot.Value) > 0 ? _ctrl.TargetRot.Value.OrthoR() : _ctrl.TargetRot.Value.OrthoL();

                // sprint, if not in combat and far enough away from destination
                if (allowSprint && !player.InCombat && player != master && distSq > 400)
                {
                    _autorot.ClassActions?.HandleUserActionRequest(CommonDefinitions.IDSprint, player);
                }
            }
        }

        public void DrawDebug()
        {
            ImGui.Checkbox("Forbid actions", ref _forbidActions);
            ImGui.SameLine();
            ImGui.Checkbox("Forbid movement", ref _forbidMovement);
            ImGui.TextUnformatted($"Max-cast={MathF.Min(_maxCastTime, 1000):f3}, afk={_afkMode}, follow={_followMaster}, algo={_naviDecision.DecisionType}, master standing for {Math.Clamp((_autorot.WorldState.CurrentTime - _masterLastMoved).TotalSeconds, 0, 1000):f1}");
        }
    }
}
