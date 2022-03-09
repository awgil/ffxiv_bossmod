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

            public override void OnEventCast(CastEvent info)
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
            private float _radius;

            public SharedTankbuster(BossModule module, float radius)
            {
                _module = module;
                _radius = radius;
            }

            public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
            {
                var target = _module.WorldState.Actors.Find(_module.PrimaryActor.TargetID);
                if (target != null)
                {
                    arena.Actor(target, arena.ColorPlayerGeneric);
                    arena.AddCircle(target.Position, _radius, arena.ColorDanger);
                }
            }
        }
    }
}
