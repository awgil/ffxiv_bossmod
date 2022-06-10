using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // role ID is [0,8] == 3 * order + dir, where order is [0,2], dir is [0, 2] (0 for backward, 1 for high, 2 for forward); 4 is not used
    class P3DiveFromGrace : BossModule.Component
    {
        public int WheelsDone { get; private set; }
        public int BaitsStarted { get; private set; }
        public int DivesDone { get; private set; }
        private Actor? _boss;
        private int[] _playerRoles = new int[8];
        private AOEShape? _nextAOE;

        private static AOEShapeCircle _aoeGnash = new(8);
        private static AOEShapeDonut _aoeLash = new(8, 40);

        public override void Init(BossModule module)
        {
            _boss = module.Enemies(OID.BossP3).FirstOrDefault();
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _nextAOE?.Draw(arena, _boss);
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.Jump1:
                    AssignJumpOrder(module, actor, 0);
                    break;
                case SID.Jump2:
                    AssignJumpOrder(module, actor, 1);
                    break;
                case SID.Jump3:
                    AssignJumpOrder(module, actor, 2);
                    break;
                case SID.JumpBackward:
                    AssignJumpDirection(module, actor, 0);
                    break;
                case SID.JumpCenter:
                    AssignJumpDirection(module, actor, 1);
                    break;
                case SID.JumpForward:
                    AssignJumpDirection(module, actor, 2);
                    break;
            }
        }

        public override void OnCastStarted(BossModule module, Actor actor)
        {
            if (!actor.CastInfo!.IsSpell())
                return;
            switch ((AID)actor.CastInfo.Action.ID)
            {
                case AID.GnashAndLash:
                    _nextAOE = _aoeGnash;
                    break;
                case AID.LashAndGnash:
                    _nextAOE = _aoeLash;
                    break;
                case AID.Geirskogul:
                    ++BaitsStarted;
                    break;
            }
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (!info.IsSpell())
                return;
            switch ((AID)info.Action.ID)
            {
                case AID.GnashingWheel:
                case AID.LashingWheel:
                    ++WheelsDone;
                    _nextAOE = (WheelsDone & 1) == 0 ? null : (_nextAOE == _aoeGnash ? _aoeLash : _aoeGnash);
                    break;
                case AID.DarkHighJump:
                case AID.DarkSpineshatterDive:
                case AID.DarkElusiveJump:
                    ++DivesDone;
                    break;
            }
        }

        private void AssignJumpOrder(BossModule module, Actor actor, int order)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerRoles[slot] = 3 * order;
        }

        private void AssignJumpDirection(BossModule module, Actor actor, int direction)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerRoles[slot] += direction;
        }
    }
}
