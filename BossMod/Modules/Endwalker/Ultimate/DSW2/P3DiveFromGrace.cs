using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P3DiveFromGrace : BossComponent
    {
        // current 'state' is next event that will happen
        // note that some mechanics happen with slight delay (~0.1-0.2s) - we merge them together to protect against potential reordering
        public enum State
        {
            AssignOrder,
            AssignDirection,
            Jump1Stack1,
            InOut1,
            Towers1InOut2,
            Bait1,
            Jump2,
            Towers2,
            Bait2,
            Jump3Stack2,
            InOut3,
            Towers3InOut4,
            Bait3,
            Resolve,
            Done
        }

        private struct PlayerState
        {
            public int JumpOrder; // 0 if unassigned, otherwise [1,3]
            public int JumpDirection; // -1 for backward, +1 for forward
        }

        public State NextEvent { get; private set; }
        private int _wheelsDone;
        private Actor? _boss;
        private PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
        private AOEShape? _nextAOE;

        private static AOEShapeCircle _aoeGnash = new(8);
        private static AOEShapeDonut _aoeLash = new(8, 40);

        public override void Init(BossModule module)
        {
            _boss = module.Enemies(OID.BossP3).FirstOrDefault();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            switch (_playerStates[slot].JumpOrder)
            {
                case 1:
                    if (NextEvent <= State.Jump1Stack1)
                        hints.Add("Bait jump -> return to raid, avoid in/out", false);
                    else if (NextEvent <= State.Jump2)
                        hints.Add("Wait for second (sides) or third (center) jump -> soak tower", false);
                    else if (NextEvent <= State.Towers2)
                        hints.Add("Side: soak tower; Center: wait for third jump", false);
                    else if (NextEvent <= State.Bait2)
                        hints.Add("Side: bait -> return to raid; Center: wait for third jump", false);
                    else if (NextEvent <= State.Jump3Stack2)
                        hints.Add("Center: wait for third jump -> soak tower", false);
                    else if (NextEvent <= State.Towers3InOut4)
                        hints.Add("Center: avoid in/out, soak tower", false);
                    else if (NextEvent <= State.Bait3)
                        hints.Add("Center: bait", false);
                    break;
                case 2:
                    if (NextEvent <= State.Towers1InOut2)
                        hints.Add("Wait, avoid in/out", false);
                    else if (NextEvent <= State.Jump2)
                        hints.Add("Bait jump", false);
                    else if (NextEvent <= State.Jump3Stack2)
                        hints.Add("Wait for third jump", false);
                    else if (NextEvent <= State.Towers3InOut4)
                        hints.Add("Avoid in/out, soak tower", false);
                    else if (NextEvent <= State.Bait3)
                        hints.Add("Bait", false);
                    break;
                case 3:
                    if (NextEvent <= State.Jump1Stack1)
                        hints.Add("Wait for first jump -> soak tower", false);
                    else if (NextEvent <= State.Towers1InOut2)
                        hints.Add("Avoid in/out, soak tower", false);
                    else if (NextEvent <= State.Bait1)
                        hints.Add("Bait", false);
                    else if (NextEvent <= State.Jump3Stack2)
                        hints.Add("Bait jump", false);
                    break;
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _nextAOE?.Draw(arena, _boss);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
                arena.Actor(player, ArenaColor.PlayerGeneric);
        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.Jump1:
                    AssignJumpOrder(module, actor, 1);
                    break;
                case SID.Jump2:
                    AssignJumpOrder(module, actor, 2);
                    break;
                case SID.Jump3:
                    AssignJumpOrder(module, actor, 3);
                    break;
                case SID.JumpBackward:
                    AssignJumpDirection(module, actor, -1);
                    break;
                case SID.JumpCenter:
                    AssignJumpDirection(module, actor, 0);
                    break;
                case SID.JumpForward:
                    AssignJumpDirection(module, actor, +1);
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
                    if (NextEvent is State.Bait1 or State.Bait2 or State.Bait3)
                        ++NextEvent;
                    break;
                case AID.DarkdragonDive:
                    if (NextEvent is State.Towers2)
                        ++NextEvent;
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
                    ++_wheelsDone;
                    _nextAOE = (_wheelsDone & 1) == 0 ? null : (_nextAOE == _aoeGnash ? _aoeLash : _aoeGnash);
                    if (NextEvent is State.InOut1 or State.Towers1InOut2 or State.InOut3 or State.Towers3InOut4)
                        ++NextEvent;
                    break;
                case AID.DarkHighJump:
                case AID.DarkSpineshatterDive:
                case AID.DarkElusiveJump:
                    if (NextEvent is State.Jump1Stack1 or State.Jump2 or State.Jump3Stack2)
                        ++NextEvent;
                    break;
                case AID.Geirskogul:
                    if (NextEvent is State.Resolve)
                        ++NextEvent;
                    break;
            }
        }

        private void AssignJumpOrder(BossModule module, Actor actor, int order)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerStates[slot].JumpOrder = order;
            if (NextEvent is State.AssignOrder)
                ++NextEvent;
        }

        private void AssignJumpDirection(BossModule module, Actor actor, int direction)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _playerStates[slot].JumpDirection = direction;
            if (NextEvent is State.AssignDirection)
                ++NextEvent;
        }
    }
}
