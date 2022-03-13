using System.Linq;

namespace BossMod.Endwalker.P3S
{
    using static BossModule;

    // state relared to darkened fire add placement mechanic
    // adds should be neither too close (or they insta explode and wipe raid) nor too far (or during brightened fire someone wouldn't be able to hit two adds)
    class DarkenedFire : Component
    {
        private P3S _module;

        private static float _minRange = 11; // note: on one of our pulls adds at (94.14, 105.55) and (94.21, 94.69) (distance=10.860) linked and wiped us
        private static float _maxRange = 13; // brigthened fire aoe radius is 7, so this is x2 minus some room for positioning

        public DarkenedFire(P3S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            bool haveTooClose = false;
            int numInRange = 0;
            foreach (var player in _module.Raid.WithoutSlot().Where(player => CanBothBeTargets(player, actor)))
            {
                var distance = player.Position - actor.Position;
                haveTooClose |= GeometryUtils.PointInCircle(distance, _minRange);
                if (GeometryUtils.PointInCircle(distance, _maxRange))
                    ++numInRange;
            }

            if (haveTooClose)
            {
                hints.Add("Too close to other players!");
            }
            else if (numInRange < 2)
            {
                hints.Add("Too far from other players!");
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            // draw other potential targets, to simplify positioning
            bool healerOrTank = pc.Role == Role.Tank || pc.Role == Role.Healer;
            foreach (var player in _module.Raid.WithoutSlot().Where(player => CanBothBeTargets(player, pc)))
            {
                var distance = player.Position - pc.Position;
                bool tooClose = GeometryUtils.PointInCircle(distance, _minRange);
                bool inRange = GeometryUtils.PointInCircle(distance, _maxRange);
                arena.Actor(player, tooClose ? arena.ColorDanger : (inRange ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric));
            }

            // draw circles around pc
            arena.AddCircle(pc.Position, _minRange, arena.ColorDanger);
            arena.AddCircle(pc.Position, _maxRange, arena.ColorSafe);
        }

        private bool CanBothBeTargets(Actor one, Actor two)
        {
            // i'm quite sure it selects either 4 dds or 2 tanks + 2 healers
            return one != two && (one.Role == Role.Tank || one.Role == Role.Healer) == (two.Role == Role.Tank || two.Role == Role.Healer);
        }
    }
}
