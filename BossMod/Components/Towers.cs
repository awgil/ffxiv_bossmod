using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    public class GenericTowers : CastCounter
    {
        public struct Tower
        {
            public WPos Position;
            public float Radius;
            public int MinSoakers;
            public int MaxSoakers;
            public BitMask ForbiddenSoakers;

            public Tower(WPos position, float radius, int minSoakers = 1, int maxSoakers = 1, BitMask forbiddenSoakers = default)
            {
                Position = position;
                Radius = radius;
                MinSoakers = minSoakers;
                MaxSoakers = maxSoakers;
                ForbiddenSoakers = forbiddenSoakers;
            }

            public bool IsInside(WPos pos) => pos.InCircle(Position, Radius);
            public bool IsInside(Actor actor) => IsInside(actor.Position);
            public int NumInside(BossModule module) => module.Raid.WithSlot().ExcludedFromMask(ForbiddenSoakers).InRadius(Position, Radius).Count();
            public bool CorrectAmountInside(BossModule module) => NumInside(module) is var count && count >= MinSoakers && count <= MaxSoakers;
        }

        public List<Tower> Towers = new();

        // default tower styling
        public static void DrawTower(MiniArena arena, WPos pos, float radius, bool safe)
        {
            if (arena.Config.ShowOutlinesAndShadows)
                arena.AddCircle(pos, radius, 0xFF000000, 3);
            arena.AddCircle(pos, radius, safe ? ArenaColor.Safe : ArenaColor.Danger, 2);
        }

        public GenericTowers(ActionID aid = default) : base(aid) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (Towers.Any(t => t.ForbiddenSoakers[slot] && t.IsInside(actor)))
            {
                hints.Add("GTFO from tower!");
            }
            else if (Towers.FindIndex(t => !t.ForbiddenSoakers[slot] && t.IsInside(actor)) is var soakedIndex && soakedIndex >= 0) // note: this assumes towers don't overlap
            {
                var count = Towers[soakedIndex].NumInside(module);
                if (count < Towers[soakedIndex].MinSoakers)
                    hints.Add("Too few soakers in the tower!");
                else if (count > Towers[soakedIndex].MaxSoakers)
                    hints.Add("Too many soakers in the tower!");
            }
            else if (Towers.Any(t => !t.ForbiddenSoakers[slot] && !t.CorrectAmountInside(module)))
            {
                hints.Add("Soak the tower!");
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var t in Towers)
                DrawTower(arena, t.Position, t.Radius, !t.ForbiddenSoakers[pcSlot]);
        }
    }

    public class CastTowers : GenericTowers
    {
        public float Radius;
        public int MinSoakers;
        public int MaxSoakers;

        public CastTowers(ActionID aid, float radius, int minSoakers = 1, int maxSoakers = 1) : base(aid)
        {
            Radius = radius;
            MinSoakers = minSoakers;
            MaxSoakers = maxSoakers;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                Towers.Add(new(DeterminePosition(module, caster, spell), Radius, MinSoakers, MaxSoakers));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                var pos = DeterminePosition(module, caster, spell);
                Towers.RemoveAll(t => t.Position.AlmostEqual(pos, 1));
            }
        }

        private WPos DeterminePosition(BossModule module, Actor caster, ActorCastInfo spell) => spell.TargetID == caster.InstanceID ? caster.Position : module.WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
    }
}
