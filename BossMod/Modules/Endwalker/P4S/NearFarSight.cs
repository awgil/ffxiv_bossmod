using System.Linq;

namespace BossMod.P4S
{
    using static BossModule;

    // state related to nearsight & farsight mechanics
    class NearFarSight : Component
    {
        public enum State { Near, Far, Done }

        public State CurState { get; private set; }
        private P4S _module;
        private ulong _targets = 0;
        private ulong _inAOE = 0;

        private static float _aoeRadius = 5;

        public NearFarSight(P4S module, State state)
        {
            CurState = state;
            _module = module;
        }

        public override void Update()
        {
            _targets = _inAOE = 0;
            var boss = _module.Boss2();
            if (boss == null || CurState == State.Done)
                return;

            var playersByRange = _module.Raid.WithSlot().SortedByRange(boss.Position);
            foreach ((int i, var player) in CurState == State.Near ? playersByRange.Take(2) : playersByRange.TakeLast(2))
            {
                BitVector.SetVector64Bit(ref _targets, i);
                _inAOE |= _module.Raid.WithSlot().InRadiusExcluding(player, _aoeRadius).Mask();
            }
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_targets == 0)
                return;

            bool isTarget = BitVector.IsVector64BitSet(_targets, slot);
            bool shouldBeTarget = actor.Role == Role.Tank;
            bool isFailing = isTarget != shouldBeTarget;
            bool shouldBeNear = CurState == State.Near ? shouldBeTarget : !shouldBeTarget;
            hints.Add(shouldBeNear ? "Stay near boss" : "Stay on max melee", isFailing);
            if (BitVector.IsVector64BitSet(_inAOE, slot))
            {
                hints.Add("GTFO from tanks!");
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_targets == 0)
                return;

            foreach ((int i, var player) in _module.Raid.WithSlot())
            {
                if (BitVector.IsVector64BitSet(_targets, i))
                {
                    arena.Actor(player, arena.ColorDanger);
                    arena.AddCircle(player.Position, _aoeRadius, arena.ColorDanger);
                }
                else
                {
                    arena.Actor(player, BitVector.IsVector64BitSet(_inAOE, i) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                }
            }
        }

        public override void OnEventCast(CastEvent info)
        {
            if (info.IsSpell(AID.NearsightAOE) || info.IsSpell(AID.FarsightAOE))
                CurState = State.Done;
        }
    }
}
