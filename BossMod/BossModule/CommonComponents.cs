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

        // generic component that is 'active' during specific primary target's cast
        // useful for simple bosses - outdoor, dungeons, etc.
        public class CastHint : BossModule.Component
        {
            protected BossModule _module;
            protected ActionID _action;
            protected string _hint;

            public bool Active => _module.PrimaryActor.CastInfo?.Action == _action;

            public CastHint(BossModule module, ActionID action, string hint)
            {
                _module = module;
                _action = action;
                _hint = hint;
            }

            public override void AddGlobalHints(BossModule.GlobalHints hints)
            {
                if (Active)
                    hints.Add(_hint);
            }
        }

        // generic avoidable aoe
        public class CastHintAvoidable : CastHint
        {
            protected AOEShape _shape;

            public CastHintAvoidable(BossModule module, ActionID action, AOEShape shape)
                : base(module, action, "Avoidable AOE")
            {
                _shape = shape;
            }

            public override void AddHints(int slot, Actor actor, BossModule.TextHints hints, BossModule.MovementHints? movementHints)
            {
                if (Active && _shape.Check(actor.Position, _module.PrimaryActor))
                    hints.Add("GTFO from aoe!");
            }

            public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
            {
                if (Active)
                    _shape.Draw(arena, _module.PrimaryActor);
            }
        }
    }
}
