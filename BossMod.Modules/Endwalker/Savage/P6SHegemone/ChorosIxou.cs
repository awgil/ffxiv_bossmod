namespace BossMod.Endwalker.Savage.P6SHegemone;

class ChorosIxou(BossModule module) : Components.GenericAOEs(module)
{
    public bool FirstDone { get; private set; }
    public bool SecondDone { get; private set; }
    private readonly AOEShapeCone _cone = new(40, 45.Degrees());
    private readonly List<Angle> _directions = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (SecondDone)
            yield break;

        // TODO: timing
        var offset = (FirstDone ? 90 : 0).Degrees();
        foreach (var dir in _directions)
            yield return new(_cone, Module.PrimaryActor.Position, dir + offset);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChorosIxouFSFrontAOE or AID.ChorosIxouSFSidesAOE)
            _directions.Add(caster.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ChorosIxouFSFrontAOE or AID.ChorosIxouSFSidesAOE)
            FirstDone = true;
        else if ((AID)spell.Action.ID is AID.ChorosIxouFSSidesAOE or AID.ChorosIxouSFFrontAOE)
            SecondDone = true;
    }
}
