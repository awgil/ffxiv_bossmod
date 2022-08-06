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
        private AvoidAOE _avoidAOE;
        private bool _passive;
        private bool _autoTarget;
        private bool _instantCastsOnly;
        private WPos _masterPrevPos;
        private WPos _masterMovementStart;
        private DateTime _masterLastMoved;

        public AIBehaviour(AIController ctrl, Autorotation autorot)
        {
            _autorot = autorot;
            _ctrl = ctrl;
            _avoidAOE = new(autorot.Bossmods);
        }

        public void Dispose()
        {
            _avoidAOE.Dispose();
        }

        public void Execute(Actor player, Actor master)
        {
            var primaryTarget = _autoTarget ? TargetingSelectBest(player) : TargetingAssistMaster(player, master);
            if (primaryTarget != null)
            {
                _autorot.PrimaryTarget = primaryTarget;
                _ctrl.SetPrimaryTarget(primaryTarget);
            }

            var secondaryTarget = TargetingHeal(player);
            _autorot.SecondaryTarget = secondaryTarget;

            UpdateState(player, master);
            UpdateControl(player, master, primaryTarget, secondaryTarget);
        }

        private Actor? TargetingSelectBest(Actor player)
        {
            var selectedTarget = _autorot.WorldState.Actors.Find(player.TargetID);
            if (selectedTarget?.Type == ActorType.Enemy && !selectedTarget.IsAlly)
                return selectedTarget;

            // choose min-hp targets among those targeting me first
            var bestAttacker = _autorot.PotentialTargets.Where(t => t.TargetID == player.InstanceID).MinBy(t => t.HP.Cur);
            if (bestAttacker != null)
                return bestAttacker;

            // otherwise (if no one is attacking me), choose min-hp target (TODO: also consider distance?..)
            return _autorot.PotentialTargets.MinBy(t => t.HP.Cur);
        }

        private Actor? TargetingAssistMaster(Actor player, Actor master)
        {
            if (!master.InCombat)
                return null; // master not in combat => just follow

            var masterTarget = _autorot.WorldState.Actors.Find(master.TargetID);
            if (masterTarget?.Type != ActorType.Enemy || masterTarget.IsAlly)
                return null; // master has no target or targets non-enemy => just follow

            // TODO: check potential targets, if anyone is casting - queue interrupt or stun (unless planned), otherwise gtfo from aoe
            // TODO: mitigates/self heals, unless planned, if low hp

            // assist master
            return masterTarget;
        }

        private Actor? TargetingHeal(Actor player)
        {
            if (player.Class != Class.ACN && player.Role != Role.Healer)
                return null;

            Actor? healTarget = null;
            float healPrio = 0;
            foreach (var p in _autorot.WorldState.Party.WithoutSlot(false))
            {
                if (p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)))
                {
                    if (healPrio == 0)
                        healTarget = p; // esuna is lowest prio
                }
                else if (p.HP.Cur < p.HP.Max * 0.8f)
                {
                    var prio = 1 - (float)p.HP.Cur / p.HP.Max;
                    if (prio > healPrio)
                    {
                        healTarget = p;
                        healPrio = prio;
                    }
                }
            }
            return healTarget;
        }

        private void UpdateState(Actor player, Actor master)
        {
            // keep master in focus
            bool masterChanged = Service.TargetManager.FocusTarget?.ObjectId != master.InstanceID;
            if (masterChanged)
            {
                _ctrl.SetFocusTarget(master);
                _masterPrevPos = _masterMovementStart = master.Position;
                _masterLastMoved = new();
            }

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

            _instantCastsOnly = masterIsMoving && _autorot.Bossmods.ActiveModule?.StateMachine.ActiveState == null && (_masterPrevPos - _masterMovementStart).LengthSq() > 100
                || _ctrl.NaviTargetPos != null && (_ctrl.NaviTargetPos.Value - player.Position).LengthSq() > 1;
        }

        private void UpdateControl(Actor player, Actor master, Actor? primaryTarget, Actor? healTarget)
        {
            var strategy = AutoAction.None;
            if (_autorot.ClassActions != null && !_passive && !_ctrl.InCutscene)
            {
                strategy = healTarget != null
                    ? AutoAction.GCDHeal | AutoAction.OGCDHeal | AutoAction.AOEHeal
                    : AutoAction.GCDDamage | AutoAction.OGCDDamage | AutoAction.AOEDamage;
                if (_instantCastsOnly)
                    strategy |= AutoAction.NoCast;

                if (primaryTarget != null)
                {
                    // note: that there is a 1-frame delay if target and/or strategy changes - we don't really care?..
                    // note: if target-of-target is player, don't try flanking, it's probably impossible... - unless target is currently casting
                    var positional = _autorot.ClassActions.PreferredPosition;
                    if (primaryTarget.TargetID == player.InstanceID && primaryTarget.CastInfo == null)
                        positional = CommonActions.Positional.Any;
                    _avoidAOE.SetDesired(primaryTarget.Position, primaryTarget.Rotation, _autorot.ClassActions.PreferredRange + player.HitboxRadius + primaryTarget.HitboxRadius, positional);
                }
                else if (healTarget != null)
                {
                    _avoidAOE.SetDesired(healTarget.Position, healTarget.Rotation, _autorot.ClassActions.PreferredRange + player.HitboxRadius + healTarget.HitboxRadius);
                }
                else
                {
                    _avoidAOE.ClearDesired();
                }
            }
            else
            {
                _avoidAOE.ClearDesired();
            }

            _autorot.ClassActions?.UpdateAutoStrategy(strategy);
            var dest = _avoidAOE.Update(player);
            if (dest == null && strategy == AutoAction.None && master != player)
            {
                // if there is no planned action and no aoe avoidance, just follow master...
                var targetPos = master.Position;
                var playerPos = player.Position;
                var toTarget = targetPos - playerPos;
                if (toTarget.LengthSq() > 4)
                {
                    dest = targetPos;
                }

                // sprint
                if (toTarget.LengthSq() > 400 && !player.InCombat)
                {
                    _autorot.ClassActions?.HandleUserActionRequest(CommonDefinitions.IDSprint, player);
                }

                //var cameraFacing = _ctrl.CameraFacing;
                //var dot = cameraFacing.Dot(_ctrl.TargetRot.Value);
                //if (dot < -0.707107f)
                //    _ctrl.TargetRot = -_ctrl.TargetRot.Value;
                //else if (dot < 0.707107f)
                //    _ctrl.TargetRot = cameraFacing.OrthoL().Dot(_ctrl.TargetRot.Value) > 0 ? _ctrl.TargetRot.Value.OrthoR() : _ctrl.TargetRot.Value.OrthoL();
            }

            var toDest = dest != null ? dest.Value - player.Position : new();
            _ctrl.NaviTargetPos = dest;
            _ctrl.NaviTargetRot = toDest.LengthSq() >= 0.04f ? toDest.Normalized() : null;
        }

        public void DrawDebug()
        {
            ImGui.Checkbox("Passively follow", ref _passive);
            ImGui.Checkbox("Auto select target", ref _autoTarget);
            ImGui.TextUnformatted($"Only-instant={_instantCastsOnly}, master standing for {(_autorot.WorldState.CurrentTime - _masterLastMoved).TotalSeconds:f1}");
        }
    }
}
