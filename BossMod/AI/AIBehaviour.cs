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

        public Actor? UpdateTargeting(Actor player, Actor master)
        {
            if (_autoTarget)
            {
                return TargetingSelectBest(player);
            }
            else
            {
                return TargetingAssistMaster(player, master);
            }
        }

        private Actor? TargetingSelectBest(Actor player)
        {
            var healTarget = TargetingHeal(player);
            if (healTarget != null)
                return healTarget;

            var selectedTarget = _autorot.WorldState.Actors.Find(player.TargetID);
            if (selectedTarget?.Type == ActorType.Enemy)
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
            if (masterTarget?.Type != ActorType.Enemy)
                return null; // master has no target or targets non-enemy => just follow

            // TODO: check potential targets, if anyone is casting - queue interrupt or stun (unless planned), otherwise gtfo from aoe
            // TODO: mitigates/self heals, unless planned, if low hp

            var healTarget = TargetingHeal(player);
            if (healTarget != null)
                return healTarget;

            // assist master
            return masterTarget;
        }

        private Actor? TargetingHeal(Actor player)
        {
            if (player.Class != Class.ACN && player.Role != Role.Healer)
                return null;

            Actor? healTarget = null;
            float healPrio = 0;
            foreach (var p in _autorot.WorldState.Party.WithoutSlot(true))
            {
                if (p.IsDead || p.Statuses.Any(s => Utils.StatusIsRemovable(s.ID)))
                {
                    if (healPrio == 0)
                        healTarget = p; // raise/esuna is lowest prio
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

        public void Execute(Actor player, Actor master, Actor? primaryTarget)
        {
            // keep master in focus
            bool masterChanged = Service.TargetManager.FocusTarget?.ObjectId != master.InstanceID;
            if (masterChanged)
            {
                _ctrl.SetFocusTarget(master.InstanceID);
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

            var action = _passive ? new() : _autorot.ClassActions?.CalculateBestAction(player, primaryTarget, _instantCastsOnly) ?? new();
            var selfTargeted = IsSelfTargeted(action.Action);
            _ctrl.PlannedAction = action.Action;
            _ctrl.PlannedActionTarget = selfTargeted ? player : action.Target;
            if (!selfTargeted)
            {
                // note: if target-of-target is player, don't try flanking, it's probably impossible... - unless target is currently casting
                var positional = action.Positional;
                if (action.Target != null && action.Target.TargetID == player.InstanceID && action.Target.CastInfo == null)
                    positional = CommonActions.Positional.Any;
                _avoidAOE.SetDesired(action.Target?.Position, action.Target?.Rotation ?? new(), _ctrl.Range(action.Action) + player.HitboxRadius + action.Target?.HitboxRadius ?? 0, positional);
            }

            var dest = _avoidAOE.Update(player);
            //if (action.Action && action.Target != null)
            //{
            //    // TODO: improve movement logic; currently we always attempt to move to melee range, this is good for classes that have point-blank aoes
            //    _ctrl.NaviTargetPos = null;
            //    if (action.Target.InstanceID != player.InstanceID)
            //    {
            //        // max melee is 3 + src.hitbox + target.hitbox, max range is -0.5 that to allow for some navigation errors
            //        // note: if target-of-target is player, don't try flanking, it's probably impossible...
            //        var maxMelee = 3 + player.HitboxRadius + action.Target.HitboxRadius;
            //        _ctrl.NaviTargetPos = action.Target.TargetID != player.InstanceID ? action.Positional switch
            //        {
            //            CommonActions.Positional.Flank => MoveToFlank(player, action.Target, maxMelee, 0.5f),
            //            CommonActions.Positional.Rear => MoveToRear(player, action.Target, maxMelee, 0.5f),
            //            _ => MoveToMelee(player, action.Target, maxMelee, 0.5f)
            //        } : MoveToMelee(player, action.Target, maxMelee, 0.5f);
            //    }
            //    _ctrl.NaviTargetRot = _ctrl.NaviTargetPos != null ? (_ctrl.NaviTargetPos.Value - player.Position).Normalized() : null;
            //    return;
            //}

            if (dest == null && !action.Action && master != player)
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
                    _ctrl.PlannedAction = new ActionID(ActionType.Spell, 3);
                    _ctrl.PlannedActionTarget = player;
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

        //private static WPos? MoveToFlank(Actor player, Actor target, float maxRange, float safetyThreshold)
        //{
        //    var maxRangeSafe = maxRange - safetyThreshold;
        //    var toPlayer = player.Position - target.Position;
        //    var distance = toPlayer.Length();
        //    toPlayer /= distance;
        //    var targetDir = target.Rotation.ToDirection();
        //    var cosAngle = targetDir.Dot(toPlayer);
        //    if (Math.Abs(cosAngle) > 0.707106f)
        //    {
        //        // not on flank
        //        bool goToLeft = targetDir.OrthoL().Dot(toPlayer) > 0;
        //        bool isInFront = cosAngle > 0;
        //        var segOrigin = target.Position + (goToLeft ? 1.4142f : -1.4142f) * safetyThreshold * targetDir.OrthoL();
        //        var segDir = (target.Rotation + (goToLeft ? 1 : -1) * (isInFront ? 45 : 135).Degrees()).ToDirection();
        //        // law of cosines, a = sqrt(2) * thr, gamma = 135 degrees, c = maxRange => c^ = a^2 + b^2 - 2abcos(gamma) = 2*thr^2 + b^2 + 4b*thr = (b+2*thr)^2 - 2*thr^2 => b = sqrt(c^2 + 2*thr^2) - 2*thr
        //        var segMax = MathF.Sqrt(maxRangeSafe * maxRangeSafe + 2 * safetyThreshold * safetyThreshold) - 2 * safetyThreshold;
        //        var segProj = Math.Clamp((player.Position - segOrigin).Dot(segDir), 0, segMax);
        //        return segOrigin + segProj * segDir;
        //    }
        //    else if (distance > maxRange)
        //    {
        //        // on flank, but too far
        //        return target.Position + toPlayer * maxRangeSafe;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //private static WPos? MoveToRear(Actor player, Actor target, float maxRange, float safetyThreshold)
        //{
        //    var maxRangeSafe = maxRange - safetyThreshold;
        //    var toPlayer = player.Position - target.Position;
        //    var distance = toPlayer.Length();
        //    toPlayer /= distance;
        //    var targetDir = target.Rotation.ToDirection();
        //    var cosAngle = targetDir.Dot(toPlayer);
        //    if (cosAngle > -0.707107f)
        //    {
        //        // not on rear
        //        bool goToLeft = targetDir.OrthoL().Dot(toPlayer) > 0;
        //        var segOrigin = target.Position - 1.4142f * safetyThreshold * targetDir;
        //        var segDir = (target.Rotation + (goToLeft ? 1 : -1) * 135.Degrees()).ToDirection();
        //        // law of cosines, a = sqrt(2) * thr, gamma = 135 degrees, c = maxRange => c^ = a^2 + b^2 - 2abcos(gamma) = 2*thr^2 + b^2 + 4b*thr = (b+2*thr)^2 - 2*thr^2 => b = sqrt(c^2 + 2*thr^2) - 2*thr
        //        var segMax = MathF.Sqrt(maxRangeSafe * maxRangeSafe + 2 * safetyThreshold * safetyThreshold) - 2 * safetyThreshold;
        //        var segProj = Math.Clamp((player.Position - segOrigin).Dot(segDir), 0, segMax);
        //        return segOrigin + segProj * segDir;
        //    }
        //    else if (distance > maxRange)
        //    {
        //        // on rear, but too far
        //        return target.Position + toPlayer * maxRangeSafe;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //private static WPos? MoveToMelee(Actor player, Actor target, float maxRange, float safetyThreshold)
        //{
        //    var maxRangeSafe = maxRange - safetyThreshold;
        //    var toPlayer = player.Position - target.Position;
        //    var distance = toPlayer.Length();
        //    if (distance > maxRange)
        //    {
        //        return target.Position + toPlayer / distance * maxRangeSafe;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        private bool IsSelfTargeted(ActionID action)
        {
            if (!action)
                return false;
            if (action.Type != ActionType.Spell)
                return true;
            var data = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(action.ID);
            return data != null && !data.CanTargetFriendly && !data.CanTargetHostile && !data.CanTargetParty;
        }
    }
}
