namespace BossMod.Endwalker.Savage.P6SHegemone;

class Exocleaver(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ExocleaverAOE2))
{
    public bool FirstDone { get; private set; }
    private AOEShapeCone _cone = new(30, 15.Degrees());
    private List<Angle> _directions = new();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts > 0)
            yield break;

        // TODO: timing
        var offset = (FirstDone ? 30 : 0).Degrees();
        foreach (var dir in _directions)
            yield return new(_cone, Module.PrimaryActor.Position, dir + offset);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ExocleaverAOE1)
            _directions.Add(caster.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ExocleaverAOE1)
            FirstDone = true;
    }
}
