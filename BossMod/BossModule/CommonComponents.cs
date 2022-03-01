using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public static class CommonComponents
    {
        // generic component that counts specified casts
        public class CastCounter : BossModule.Component
        {
            public int NumCasts { get; private set; } = 0;

            private ActionID _watchedCastID;

            public CastCounter(ActionID aid)
            {
                _watchedCastID = aid;
            }

            public override void OnEventCast(WorldState.CastResult info)
            {
                if (info.Action == _watchedCastID)
                {
                    ++NumCasts;
                }
            }
        }

        // generic 'shared tankbuster' component that shows radius around boss target
        // TODO: consider showing invuln/stack/gtfo hints...
        public class SharedTankbuster : BossModule.Component
        {
            private BossModule _module;
            private List<WorldState.Actor> _caster;
            private float _radius;

            public SharedTankbuster(BossModule module, List<WorldState.Actor> caster, float radius)
            {
                _module = module;
                _caster = caster;
                _radius = radius;
            }

            public override void DrawArenaForeground(MiniArena arena)
            {
                var targetID = _caster.FirstOrDefault()?.TargetID ?? 0;
                var target = targetID != 0 ? _module.WorldState.FindActor(targetID) : null;
                if (target != null)
                {
                    arena.Actor(target, arena.ColorPlayerGeneric);
                    arena.AddCircle(target.Position, _radius, arena.ColorDanger);
                }
            }
        }
    }
}
