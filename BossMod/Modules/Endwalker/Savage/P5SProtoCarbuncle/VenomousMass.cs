namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class VenomousMass(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.VenomousMassAOE))
{
    private Actor? _target;

    private const float _radius = 6;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target != null && _target != actor && actor.Position.InCircle(_target.Position, _radius))
            hints.Add("GTFO from tankbuster!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target != null)
            Arena.AddCircle(_target.Position, _radius, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.VenomousMass)
            _target = WorldState.Actors.Find(caster.TargetID);
    }
}
