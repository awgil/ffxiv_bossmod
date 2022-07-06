using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using System;

namespace BossMod.AI
{
    // constantly follow master
    class BehaviourFollow : Behaviour
    {
        private WorldState _ws;
        private Navigation _navi;
        private UseAction _useAction;
        private Autorotation _autorot;

        public BehaviourFollow(WorldState ws, Navigation navi, UseAction useAction, Autorotation autorot)
        {
            _ws = ws;
            _navi = navi;
            _useAction = useAction;
            _autorot = autorot;
        }

        public override bool Execute(Actor master)
        {
            var self = _ws.Party.Player();
            if (self == null)
            {
                _navi.TargetPos = null;
                _navi.TargetRot = null;
                return true;
            }

            if (self.CastInfo != null)
            {
                // don't interrupt cast...
                _navi.TargetPos = null;
                _navi.TargetRot = null;
                return true;
            }

            // TODO: in-aoe check => gtfo
            Actor? assistTarget = null;
            if (master.InCombat)
            {
                // attack master's target
                var masterTarget = _ws.Actors.Find(master.TargetID);
                if (masterTarget?.Type == ActorType.Enemy)
                {
                    assistTarget = masterTarget;

                    // TODO: reconsider (this is currently done to ensure 'state' is calculated for correct target...)
                    if (Service.TargetManager.Target?.ObjectId != masterTarget.InstanceID)
                        Service.TargetManager.SetTarget(Service.ObjectTable.SearchById((uint)masterTarget.InstanceID));
                }
            }

            if (assistTarget != null)
            {
                // in combat => assist
                var action = _autorot.ClassActions?.CalculateBestAction(self, assistTarget) ?? new();
                if (action.Action)
                {
                    // TODO: improve movement logic; currently we always attempt to move to melee range, this is good for classes that have point-blank aoes
                    bool moveCloser = true;
                    if (action.ReadyIn < 0.2f)
                    {
                        _useAction.Execute(action.Action, action.Target.InstanceID);
                        moveCloser = action.Action.Type == ActionType.Spell ? (Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(action.Action.ID)?.Cast100ms ?? 0) == 0 : true;
                    }

                    _navi.TargetPos = null;
                    if (moveCloser)
                    {
                        // max melee is 3 + src.hitbox + target.hitbox, max range is -0.5 that to allow for some navigation errors
                        var maxMelee = 3 + self.HitboxRadius + action.Target.HitboxRadius;
                        _navi.TargetPos = action.Positional switch
                        {
                            CommonActions.Positional.Flank => MoveToFlank(self, action.Target, maxMelee, 0.5f),
                            CommonActions.Positional.Rear => MoveToRear(self, action.Target, maxMelee, 0.5f),
                            _ => MoveToMelee(self, action.Target, maxMelee, 0.5f)
                        };
                    }
                    _navi.TargetRot = _navi.TargetPos != null ? (_navi.TargetPos.Value - self.Position).Normalized() : null;
                    return true;
                }
            }

            if (master != self)
            {
                // out of combat => follow master, if any
                var targetPos = master.Position;
                var selfPos = self.Position;
                var toTarget = targetPos - selfPos;
                if (toTarget.LengthSq() > 4)
                {
                    _navi.TargetPos = targetPos;
                    _navi.TargetRot = toTarget.Normalized();
                }
                else
                {
                    _navi.TargetPos = null;
                    _navi.TargetRot = null;
                }

                // sprint
                if (toTarget.LengthSq() > 400 && !self.InCombat)
                {
                    var sprint = new ActionID(ActionType.Spell, 3);
                    if (_useAction.Cooldown(sprint) < 0.5f)
                        _useAction.Execute(sprint, self.InstanceID);
                }

                //var cameraFacing = _navi.CameraFacing;
                //var dot = cameraFacing.Dot(_navi.TargetRot.Value);
                //if (dot < -0.707107f)
                //    _navi.TargetRot = -_navi.TargetRot.Value;
                //else if (dot < 0.707107f)
                //    _navi.TargetRot = cameraFacing.OrthoL().Dot(_navi.TargetRot.Value) > 0 ? _navi.TargetRot.Value.OrthoR() : _navi.TargetRot.Value.OrthoL();
            }
            else
            {
                _navi.TargetPos = null;
                _navi.TargetRot = null;
            }
            return true;
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
