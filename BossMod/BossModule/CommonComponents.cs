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

            public override void OnEventCast(BossModule module, CastEvent info)
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
            private float _radius;

            public SharedTankbuster(float radius)
            {
                _radius = radius;
            }

            public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                var target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
                if (target != null)
                {
                    arena.Actor(target, arena.ColorPlayerGeneric);
                    arena.AddCircle(target.Position, _radius, arena.ColorDanger);
                }
            }
        }

        // generic component that is 'active' during specific primary target's cast
        // useful for simple bosses - outdoor, dungeons, etc.
        public class CastHint : BossModule.Component
        {
            protected ActionID _action;
            protected string _hint;

            public CastHint(ActionID action, string hint)
            {
                _action = action;
                _hint = hint;
            }

            public bool Active(BossModule module) => module.PrimaryActor.CastInfo?.Action == _action;

            public override void AddGlobalHints(BossModule module, BossModule.GlobalHints hints)
            {
                if (Active(module))
                    hints.Add(_hint);
            }
        }

        // generic avoidable aoe
        public class CastHintAvoidable : CastHint
        {
            protected AOEShape _shape;

            public CastHintAvoidable(ActionID action, AOEShape shape)
                : base(action, "Avoidable AOE")
            {
                _shape = shape;
            }

            public override void AddHints(BossModule module, int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
            {
                if (Active(module) && _shape.Check(actor.Position, module.PrimaryActor))
                    hints.Add("GTFO from aoe!");
            }

            public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
            {
                if (Active(module))
                    _shape.Draw(arena, module.PrimaryActor);
            }
        }
    }
}
