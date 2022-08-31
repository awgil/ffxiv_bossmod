namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    class VenomousMass : Components.CastCounter
    {
        private Actor? _target;

        private static float _radius = 6;

        public VenomousMass() : base(ActionID.MakeSpell(AID.VenomousMassAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_target != null && _target != actor && actor.Position.InCircle(_target.Position, _radius))
                hints.Add("GTFO from tankbuster!");
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (_target != null)
                arena.AddCircle(_target.Position, _radius, ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.VenomousMass)
                _target = module.WorldState.Actors.Find(caster.TargetID);
        }
    }
}
