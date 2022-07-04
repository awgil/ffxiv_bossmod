using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;

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
                }
            }

            if (assistTarget != null)
            {
                // in combat => assist
                var action = SelectAttackAction(self);
                if (action)
                {
                    float range = _useAction.Range(action);
                    var toTarget = assistTarget.Position - self.Position;
                    if (toTarget.LengthSq() > range * range)
                    {
                        _navi.TargetPos = assistTarget.Position;
                        _navi.TargetRot = toTarget.Normalized();
                    }
                    else
                    {
                        _navi.TargetPos = null;
                        _navi.TargetRot = null;
                        _useAction.Execute(action, assistTarget.InstanceID);
                    }
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

        // TODO: this needs huge improvement...
        private ActionID SelectAttackAction(Actor player)
        {
            // TODO: aoe if profitable...
            return _autorot.ClassActions?.BaseAbility ?? new();
        }
    }
}
