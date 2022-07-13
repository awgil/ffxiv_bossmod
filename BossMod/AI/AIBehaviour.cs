using ImGuiNET;
using System;
using System.Linq;

namespace BossMod.AI
{
    // constantly follow master
    class AIBehaviour
    {
        private WorldState _ws;
        private Autorotation _autorot;
        private AIController _ctrl;

        public AIBehaviour(WorldState ws, AIController ctrl, Autorotation autorot)
        {
            _ws = ws;
            _autorot = autorot;
            _ctrl = ctrl;
        }

        public Actor? UpdateTargeting(Actor player, Actor master)
        {
            if (!master.InCombat)
                return null; // master not in combat => just follow

            var masterTarget = _ws.Actors.Find(master.TargetID);
            if (masterTarget?.Type != ActorType.Enemy)
                return null; // master has no target or targets non-enemy => just follow

            // TODO: check potential targets, if anyone is casting - queue interrupt or stun (unless planned), otherwise gtfo from aoe
            // TODO: mitigates/self heals, unless planned, if low hp

            // TODO: target or party heals, if possible and not planned, esuna etc.
            if (player.Role == Role.Healer)
            {
                Actor? healTarget = null;
                float healPrio = 0;
                foreach (var p in _ws.Party.WithoutSlot(true))
                {
                    // TODO: esuna check
                    if (p.IsDead)
                    {
                        if (healPrio == 0)
                            healTarget = p; // raise is lowest prio
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

                if (healTarget != null)
                    return healTarget;
            }

            // assist master
            return masterTarget;
        }

        public void Execute(Actor player, Actor master, Actor? primaryTarget)
        {
            var action = _autorot.ClassActions?.CalculateBestAction(player, primaryTarget) ?? new();
            _ctrl.PlannedAction = action.Action;
            _ctrl.PlannedActionTarget = action.Target?.InstanceID ?? 0;
            if (action.Action && action.Target != null)
            {
                // TODO: improve movement logic; currently we always attempt to move to melee range, this is good for classes that have point-blank aoes
                _ctrl.NaviTargetPos = null;
                if (action.Target.InstanceID != player.InstanceID)
                {
                    // max melee is 3 + src.hitbox + target.hitbox, max range is -0.5 that to allow for some navigation errors
                    // note: if target-of-target is player, don't try flanking, it's probably impossible...
                    var maxMelee = 3 + player.HitboxRadius + action.Target.HitboxRadius;
                    _ctrl.NaviTargetPos = action.Target.TargetID != player.InstanceID ? action.Positional switch
                    {
                        CommonActions.Positional.Flank => MoveToFlank(player, action.Target, maxMelee, 0.5f),
                        CommonActions.Positional.Rear => MoveToRear(player, action.Target, maxMelee, 0.5f),
                        _ => MoveToMelee(player, action.Target, maxMelee, 0.5f)
                    } : MoveToMelee(player, action.Target, maxMelee, 0.5f);
                }
                _ctrl.NaviTargetRot = _ctrl.NaviTargetPos != null ? (_ctrl.NaviTargetPos.Value - player.Position).Normalized() : null;
                return;
            }

            // we're here if there is no planned action...
            if (master != player)
            {
                // follow master, if any
                var targetPos = master.Position;
                var playerPos = player.Position;
                var toTarget = targetPos - playerPos;
                if (toTarget.LengthSq() > 4)
                {
                    _ctrl.NaviTargetPos = targetPos;
                    _ctrl.NaviTargetRot = toTarget.Normalized();
                }
                else
                {
                    _ctrl.NaviTargetPos = null;
                    _ctrl.NaviTargetRot = null;
                }

                // sprint
                if (toTarget.LengthSq() > 400 && !player.InCombat)
                {
                    _ctrl.PlannedAction = new ActionID(ActionType.Spell, 3);
                    _ctrl.PlannedActionTarget = player.InstanceID;
                }

                //var cameraFacing = _ctrl.CameraFacing;
                //var dot = cameraFacing.Dot(_ctrl.TargetRot.Value);
                //if (dot < -0.707107f)
                //    _ctrl.TargetRot = -_ctrl.TargetRot.Value;
                //else if (dot < 0.707107f)
                //    _ctrl.TargetRot = cameraFacing.OrthoL().Dot(_ctrl.TargetRot.Value) > 0 ? _ctrl.TargetRot.Value.OrthoR() : _ctrl.TargetRot.Value.OrthoL();
            }
            else
            {
                _ctrl.NaviTargetPos = null;
                _ctrl.NaviTargetRot = null;
            }
        }

        public void DrawDebug()
        {
        }

        private static WPos? MoveToFlank(Actor player, Actor target, float maxRange, float safetyThreshold)
        {
            var maxRangeSafe = maxRange - safetyThreshold;
            var toPlayer = player.Position - target.Position;
            var distance = toPlayer.Length();
            toPlayer /= distance;
            var targetDir = target.Rotation.ToDirection();
            var cosAngle = targetDir.Dot(toPlayer);
            if (Math.Abs(cosAngle) > 0.707106f)
            {
                // not on flank
                bool goToLeft = targetDir.OrthoL().Dot(toPlayer) > 0;
                bool isInFront = cosAngle > 0;
                var segOrigin = target.Position + (goToLeft ? 1.4142f : -1.4142f) * safetyThreshold * targetDir.OrthoL();
                var segDir = (target.Rotation + (goToLeft ? 1 : -1) * (isInFront ? 45 : 135).Degrees()).ToDirection();
                // law of cosines, a = sqrt(2) * thr, gamma = 135 degrees, c = maxRange => c^ = a^2 + b^2 - 2abcos(gamma) = 2*thr^2 + b^2 + 4b*thr = (b+2*thr)^2 - 2*thr^2 => b = sqrt(c^2 + 2*thr^2) - 2*thr
                var segMax = MathF.Sqrt(maxRangeSafe * maxRangeSafe + 2 * safetyThreshold * safetyThreshold) - 2 * safetyThreshold;
                var segProj = Math.Clamp((player.Position - segOrigin).Dot(segDir), 0, segMax);
                return segOrigin + segProj * segDir;
            }
            else if (distance > maxRange)
            {
                // on flank, but too far
                return target.Position + toPlayer * maxRangeSafe;
            }
            else
            {
                return null;
            }
        }

        private static WPos? MoveToRear(Actor player, Actor target, float maxRange, float safetyThreshold)
        {
            var maxRangeSafe = maxRange - safetyThreshold;
            var toPlayer = player.Position - target.Position;
            var distance = toPlayer.Length();
            toPlayer /= distance;
            var targetDir = target.Rotation.ToDirection();
            var cosAngle = targetDir.Dot(toPlayer);
            if (cosAngle > -0.707107f)
            {
                // not on rear
                bool goToLeft = targetDir.OrthoL().Dot(toPlayer) > 0;
                var segOrigin = target.Position - 1.4142f * safetyThreshold * targetDir;
                var segDir = (target.Rotation + (goToLeft ? 1 : -1) * 135.Degrees()).ToDirection();
                // law of cosines, a = sqrt(2) * thr, gamma = 135 degrees, c = maxRange => c^ = a^2 + b^2 - 2abcos(gamma) = 2*thr^2 + b^2 + 4b*thr = (b+2*thr)^2 - 2*thr^2 => b = sqrt(c^2 + 2*thr^2) - 2*thr
                var segMax = MathF.Sqrt(maxRangeSafe * maxRangeSafe + 2 * safetyThreshold * safetyThreshold) - 2 * safetyThreshold;
                var segProj = Math.Clamp((player.Position - segOrigin).Dot(segDir), 0, segMax);
                return segOrigin + segProj * segDir;
            }
            else if (distance > maxRange)
            {
                // on rear, but too far
                return target.Position + toPlayer * maxRangeSafe;
            }
            else
            {
                return null;
            }
        }

        private static WPos? MoveToMelee(Actor player, Actor target, float maxRange, float safetyThreshold)
        {
            var maxRangeSafe = maxRange - safetyThreshold;
            var toPlayer = player.Position - target.Position;
            var distance = toPlayer.Length();
            if (distance > maxRange)
            {
                return target.Position + toPlayer / distance * maxRangeSafe;
            }
            else
            {
                return null;
            }
        }
    }
}
