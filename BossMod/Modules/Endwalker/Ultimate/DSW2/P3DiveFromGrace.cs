using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // TODO: invent some better algorithm for assigning people to spots
    // currently we make some arbitrary decisions:
    // 1. raid stacks to the north
    // 2. if there are forward/backward jumps at given order, forward takes W spot, backward takes E spot (center takes S)
    // 3. otherwise, players with given order are sorted by party role and assigned spots in E-S-W order
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

        private class PlayerState
        {
            public int JumpOrder; // 0 if unassigned, otherwise [1,3]
            public int JumpDirection; // -1 for backward, +1 for forward
            public int AssignedSpot; // 0 if unassigned, 1 for 'backward' spot, 2 for 'center' spot, 3 for 'forward' spot
            //public string Hint = "";
            public WPos SafeSpot;
            public bool IsBaitingJump;
        }

        private class OrderState
        {
            public List<int> PlayerSlots = new();
            public bool HaveDirections = false;
            public bool HaveAssignments = false;
        }

        public State NextEvent { get; private set; }
        private int _eventProgress;
        private int _wheelsDone;
        private Actor? _boss;
        private PlayerState[] _playerStates = { new(), new(), new(), new(), new(), new(), new(), new() };
        private OrderState[] _orderState = { new(), new(), new() };
        private string _orderHint = "";
        private AOEShape? _nextAOE;
        private List<WPos> _predictedTowers = new();
        private List<Actor> _castingTowers = new();

        private static AOEShapeCircle _aoeGnash = new(8);
        private static AOEShapeDonut _aoeLash = new(8, 40);
        private static float _towerRadius = 5;
        private static float _towerOffset = 14;
        private static float _spotOffset = 7f;
        private static float _stepToBait = 1.2f;

        public override void Init(BossModule module)
        {
            _boss = module.Enemies(OID.BossP3).FirstOrDefault();
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (movementHints != null && _playerStates[slot].SafeSpot != new WPos())
            {
                movementHints.Add(actor.Position, _playerStates[slot].SafeSpot, ArenaColor.Safe);
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_orderHint.Length > 0)
                hints.Add(_orderHint);
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            _nextAOE?.Draw(arena, _boss);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_playerStates[pcSlot].SafeSpot != new WPos())
            {
                arena.AddCircle(_playerStates[pcSlot].SafeSpot, 2, ArenaColor.Safe);
            }

            // draw baited jumps
            foreach (var (slot, player) in module.Raid.WithSlot(true))
            {
                var state = _playerStates[slot];
                if (state.IsBaitingJump)
                {
                    var pos = player.Position + state.JumpDirection * player.Rotation.ToDirection() * _towerOffset;
                    arena.AddCircle(pos, _towerRadius, ArenaColor.Object);
                    if (slot == pcSlot)
                        arena.AddLine(pc.Position, pos, ArenaColor.Object);
                }
            }

            // draw active towers
            foreach (var t in _predictedTowers)
                arena.AddCircle(t, _towerRadius, ArenaColor.Danger);
            foreach (var t in _castingTowers)
                arena.AddCircle(t.Position, _towerRadius, ArenaColor.Danger);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
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

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.GnashAndLash:
                    _nextAOE = _aoeGnash;
                    break;
                case AID.LashAndGnash:
                    _nextAOE = _aoeLash;
                    break;
                case AID.Geirskogul:
                    if (NextEvent is State.Bait1 or State.Bait2 or State.Bait3)
                        AdvanceState(module);
                    break;
                case AID.DarkdragonDive:
                    _predictedTowers.Clear();
                    _castingTowers.Add(caster);
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DarkdragonDive:
                    _castingTowers.Remove(caster);
                    if (NextEvent is State.Towers2)
                        AdvanceState(module);
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.GnashingWheel:
                case AID.LashingWheel:
                    ++_wheelsDone;
                    _nextAOE = (_wheelsDone & 1) == 0 ? null : (_nextAOE == _aoeGnash ? _aoeLash : _aoeGnash);
                    if (NextEvent is State.InOut1 or State.Towers1InOut2 or State.InOut3 or State.Towers3InOut4)
                        AdvanceState(module);
                    break;
                case AID.DarkHighJump:
                    AddPredictedTower(module, spell.MainTargetID, 0);
                    if (NextEvent is State.Jump1Stack1 or State.Jump2 or State.Jump3Stack2)
                        AdvanceState(module);
                    break;
                case AID.DarkSpineshatterDive:
                    AddPredictedTower(module, spell.MainTargetID, 1);
                    if (NextEvent is State.Jump1Stack1 or State.Jump2 or State.Jump3Stack2)
                        AdvanceState(module);
                    break;
                case AID.DarkElusiveJump:
                    AddPredictedTower(module, spell.MainTargetID, -1);
                    if (NextEvent is State.Jump1Stack1 or State.Jump2 or State.Jump3Stack2)
                        AdvanceState(module);
                    break;
                case AID.Geirskogul:
                    if (NextEvent is State.Resolve)
                        AdvanceState(module);
                    break;
            }
        }

        private void AssignJumpOrder(BossModule module, Actor actor, int order)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                _playerStates[slot].JumpOrder = order;
                _orderState[order - 1].PlayerSlots.Add(slot);
            }
            if (NextEvent == State.AssignOrder && ++_eventProgress == 8)
            {
                AdvanceState(module);
            }
        }

        private void AssignJumpDirection(BossModule module, Actor actor, int direction)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
            {
                _playerStates[slot].JumpDirection = direction;
                if (direction != 0)
                    _orderState[_playerStates[slot].JumpOrder - 1].HaveDirections = true;
            }
            if (NextEvent == State.AssignDirection && ++_eventProgress == 8)
            {
                InitAssignments(module);
                AdvanceState(module);
            }
        }

        private WDir DirectionForStack() => new(0, -_spotOffset); // TODO: this is arbitrary
        private WDir DirectionForSpot(int spot, bool secondOrder)
        {
            var dir = spot switch
            {
                1 => DirectionForStack().OrthoR(), // TODO: this is arbitrary
                2 => -DirectionForStack(),
                3 => DirectionForStack().OrthoL(),
                _ => new()
            };
            if (spot != 0 && secondOrder)
                dir += DirectionForStack();
            return dir;
        }

        private void InitAssignments(BossModule module)
        {
            var roles = Service.Config.Get<PartyRolesConfig>().AssignmentsPerSlot(module.Raid);
            foreach (var order in _orderState)
            {
                if (order.HaveDirections)
                {
                    order.PlayerSlots.SortBy(e => _playerStates[e].JumpDirection);
                    AssignPlayerSequence(order);
                }
                else if (roles.Length > 0)
                {
                    order.PlayerSlots.SortBy(e => roles[e]); // TODO: this is completely arbitrary, but we need stable sort order...
                    AssignPlayerSequence(order);
                }
            }
            _orderHint = $"1: {HintForOrder(module, _orderState[0])}, 3: {HintForOrder(module, _orderState[2])}";
        }

        private void AssignPlayerSequence(OrderState state)
        {
            switch (state.PlayerSlots.Count)
            {
                case 2:
                    _playerStates[state.PlayerSlots[0]].AssignedSpot = 1;
                    _playerStates[state.PlayerSlots[1]].AssignedSpot = 3;
                    state.HaveAssignments = true;
                    break;
                case 3:
                    _playerStates[state.PlayerSlots[0]].AssignedSpot = 1;
                    _playerStates[state.PlayerSlots[1]].AssignedSpot = 2;
                    _playerStates[state.PlayerSlots[2]].AssignedSpot = 3;
                    state.HaveAssignments = true;
                    break;
            }
        }

        private string HintForOrder(BossModule module, OrderState state)
        {
            if (!state.HaveAssignments || state.PlayerSlots.Count < 2)
                return "???";
            var ab = module.Raid[state.PlayerSlots.First()]!;
            var af = module.Raid[state.PlayerSlots.Last()]!;
            return $"{ab.Name} / {af.Name}";
        }

        private void AdvanceState(BossModule module)
        {
            _eventProgress = 0;
            switch (NextEvent++)
            {
                case State.AssignOrder: // 2/3 can immediately go stack, 1's need to wait for arrows...
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder != 1)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForStack();
                        }
                    }
                    break;
                case State.AssignDirection: // 1's should now have assignments for jumps
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 1)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, false);
                            ps.IsBaitingJump = true;
                        }
                    }
                    break;
                case State.Jump1Stack1: // 1's should now return to stack, 3's should run to the towers, 2's can stay until in/out resolves
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 1)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForStack();
                            ps.IsBaitingJump = false;
                        }
                        else if (ps.JumpOrder == 3)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, false);
                        }
                    }
                    break;
                case State.Towers1InOut2: // 3's should step to bait, 2's can go bait jumps
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 3)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, false) * _stepToBait;
                        }
                        else if (ps.JumpOrder == 2)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, true);
                            ps.IsBaitingJump = true;
                        }
                    }
                    break;
                case State.Bait1: // 3's should return to stack
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 3)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForStack();
                        }
                    }
                    break;
                case State.Jump2: // 2's should return to stack, side 1's should run to the towers, 3's can go bait jumps
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 2)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForStack();
                            ps.IsBaitingJump = false;
                        }
                        else if (ps.JumpOrder == 1 && ps.AssignedSpot is 1 or 3)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, true);
                        }
                        else if (ps.JumpOrder == 3)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, false);
                            ps.IsBaitingJump = true;
                        }
                    }
                    break;
                case State.Towers2: // side 1's should step to bait
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 1 && ps.AssignedSpot is 1 or 3)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, true) * _stepToBait;
                        }
                    }
                    break;
                case State.Bait2: // side 1's should return to stack
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 1 && ps.AssignedSpot is 1 or 3)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForStack();
                        }
                    }
                    break;
                case State.Jump3Stack2: // 3's should return to stack (?), center 1 and 2's should run to the towers
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 3)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForStack();
                            ps.IsBaitingJump = false;
                        }
                        else if (ps.JumpOrder == 2 || ps.JumpOrder == 1 && ps.AssignedSpot == 2)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, false);
                        }
                    }
                    break;
                case State.Towers3InOut4: // center 1 and 2's should step to bait
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 2 || ps.JumpOrder == 1 && ps.AssignedSpot == 2)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForSpot(ps.AssignedSpot, false) * _stepToBait;
                        }
                    }
                    break;
                case State.Bait3: // center 1 and 2's should return to stack (?)
                    foreach (var ps in _playerStates)
                    {
                        if (ps.JumpOrder == 2 || ps.JumpOrder == 1 && ps.AssignedSpot == 2)
                        {
                            ps.SafeSpot = module.Bounds.Center + DirectionForStack();
                        }
                    }
                    break;
            }
        }

        private void AddPredictedTower(BossModule module, ulong actorID, int direction)
        {
            var actor = module.WorldState.Actors.Find(actorID);
            if (actor != null)
                _predictedTowers.Add(actor.Position + direction * actor.Rotation.ToDirection() * _towerOffset);
        }
    }
}
