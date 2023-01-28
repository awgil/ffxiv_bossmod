using System.Linq;
using System;

namespace BossMod.Endwalker.Ultimate.TOP
{
    class P1BallisticImpact : Components.LocationTargetedAOEs
    {
        public P1BallisticImpact() : base(ActionID.MakeSpell(AID.BallisticImpact), 5) { }
    }

    class P1FlameThrowerFirst : Components.SelfTargetedAOEs
    {
        public P1FlameThrowerFirst() : base(ActionID.MakeSpell(AID.FlameThrowerFirst), new AOEShapeCone(65, 30.Degrees())) { }
    }

    class P1FlameThrowerRest : Components.SelfTargetedAOEs
    {
        public P1FlameThrowerRest() : base(ActionID.MakeSpell(AID.FlameThrowerRest), new AOEShapeCone(65, 30.Degrees())) { }
    }

    class P1Pantokrator : P1CommonAssignments
    {
        public int NumSpreadsDone { get; private set; }
        public int NumStacksDone { get; private set; }

        private static float _spreadRadius = 5;
        private static AOEShapeRect _stackShape = new(50, 3);

        protected override (GroupAssignmentUnique assignment, bool global) Assignments()
        {
            var config = Service.Config.Get<TOPConfig>();
            return (config.P1PantokratorAssignments, config.P1PantokratorGlobalPriority);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            var ps = PlayerStates[slot];
            if (ps.Order == 0)
                return;

            var stackOrder = NextStackOrder();
            if (ps.Order == NextSpreadOrder())
            {
                hints.Add("Spread!", module.Raid.WithoutSlot().InRadiusExcluding(actor, _spreadRadius).Any());
            }
            else if (ps.Order != stackOrder)
            {
                var stackTargetSlot = Array.FindIndex(PlayerStates, s => s.Order == stackOrder && s.Group == ps.Group);
                var stackTarget = module.Raid[stackTargetSlot];
                if (stackTarget != null && !_stackShape.Check(actor.Position, module.PrimaryActor.Position, Angle.FromDirection(stackTarget.Position - module.PrimaryActor.Position)))
                    hints.Add("Stack!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var spreadOrder = NextSpreadOrder();
            var stackOrder = NextStackOrder();

            foreach (var (i, p) in module.Raid.WithSlot(true))
            {
                var order = PlayerStates[i].Order;
                if (order == spreadOrder)
                {
                    arena.AddCircle(p.Position, _spreadRadius, i == pcSlot ? ArenaColor.Safe : ArenaColor.Danger);
                }
                else if (order == stackOrder)
                {
                    _stackShape.Outline(arena, module.PrimaryActor.Position, Angle.FromDirection(p.Position - module.PrimaryActor.Position), i == pcSlot ? ArenaColor.Safe : ArenaColor.Danger);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.GuidedMissileKyrios:
                    ++NumSpreadsDone;
                    break;
                case AID.CondensedWaveCannonKyrios:
                    ++NumStacksDone;
                    break;
            }
        }

        private int NextSpreadOrder(int skip = 0)
        {
            var index = NumSpreadsDone + skip * 2;
            return index < 8 ? (index >> 1) + 1 : 0;
        }

        private int NextStackOrder(int skip = 0)
        {
            var index = NumStacksDone + skip * 2;
            return index < 8 ? (index >> 1) + (index < 4 ? 3 : -1) : 0;
        }
    }

    class P1DiffuseWaveCannonKyrios : Components.GenericBaitAway
    {
        private static AOEShape _shape = new AOEShapeCone(60, 60.Degrees()); // TODO: verify angle

        public P1DiffuseWaveCannonKyrios() : base(ActionID.MakeSpell(AID.DiffuseWaveCannonKyrios)) { }

        public override void Init(BossModule module)
        {
            ForbiddenPlayers = module.Raid.WithSlot().WhereActor(a => a.Role != Role.Tank).Mask();
        }

        public override void Update(BossModule module)
        {
            CurrentBaits.Clear();
            CurrentBaits.AddRange(module.Raid.WithoutSlot().SortedByRange(module.PrimaryActor.Position).TakeLast(2).Select(t => (module.PrimaryActor, t, _shape)));
        }
    }

    class P1WaveCannonKyrios : Components.GenericBaitAway
    {
        private static AOEShapeRect _shape = new(50, 3);

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.WaveCannonKyrios)
            {
                ++NumCasts;
                CurrentBaits.Clear();
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if (iconID == (uint)IconID.WaveCannonKyrios)
                CurrentBaits.Add((module.PrimaryActor, actor, _shape));
        }
    }
}
