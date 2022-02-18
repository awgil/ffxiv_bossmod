using System.Collections.Generic;
using System.Linq;

namespace BossMod.P3S
{
    using static BossModule;

    // bird distance utility
    // when small birds die and large birds appear, they cast 26328, and if it hits any other large bird, they buff
    // when large birds die and sparkfledgeds appear, they cast 26329, and if it hits any other sparkfledged, they wipe the raid or something
    // so we show range helper for dead birds
    class BirdDistance : Component
    {
        private P3S _module;
        private List<WorldState.Actor> _watchedBirds;
        private ulong _birdsAtRisk = 0; // mask

        private static float _radius = 13;

        public BirdDistance(P3S module, List<WorldState.Actor> birds)
        {
            _module = module;
            _watchedBirds = birds;
        }

        public override void Update()
        {
            _birdsAtRisk = 0;
            for (int i = 0; i < _watchedBirds.Count; ++i)
            {
                var bird = _watchedBirds[i];
                if (!bird.IsDead && _watchedBirds.Where(other => other.IsDead).InRadius(bird.Position, _radius).Any())
                {
                    BitVector.SetVector64Bit(ref _birdsAtRisk, i);
                }
            }
        }

        public override void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints)
        {
            for (int i = 0; i < _watchedBirds.Count; ++i)
            {
                var bird = _watchedBirds[i];
                if (!bird.IsDead && bird.TargetID == actor.InstanceID && BitVector.IsVector64BitSet(_birdsAtRisk, i))
                {
                    hints.Add("Drag bird away!");
                    return;
                }
            }
        }

        public override void DrawArenaForeground(MiniArena arena)
        {
            // draw alive birds tanked by PC and circles around dead birds
            for (int i = 0; i < _watchedBirds.Count; ++i)
            {
                var bird = _watchedBirds[i];
                if (bird.IsDead)
                {
                    arena.AddCircle(bird.Position, _radius, arena.ColorDanger);
                }
                else if (bird.TargetID == _module.WorldState.PlayerActorID)
                {
                    arena.Actor(bird, BitVector.IsVector64BitSet(_birdsAtRisk, i) ? arena.ColorEnemy : arena.ColorPlayerGeneric);
                }
            }
        }
    }
}
