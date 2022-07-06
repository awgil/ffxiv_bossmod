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
                var action = _autorot.ClassActions?.CalculateBestAction(self, assistTarget) ?? new();
                if (action.Action)
                {
                    bool allowMovement = true;
                    if (action.ReadyIn < 0.2f)
                    {
                        _useAction.Execute(action.Action, action.Target.InstanceID);
                        allowMovement = action.Action.Type == ActionType.Spell ? (Service.LuminaRow<Lumina.Excel.GeneratedSheets.Action>(action.Action.ID)?.Cast100ms ?? 0) == 0 : true;
                    }

                    if (allowMovement)
                    {
                        _navi.TargetPos = action.PositionHint;
                        _navi.TargetRot = (action.PositionHint - self.Position).Normalized();
                    }
                    else
                    {
                        _navi.TargetPos = null;
                        _navi.TargetRot = null;
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
    }
}
