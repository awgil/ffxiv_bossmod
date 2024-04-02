namespace BossMod.Endwalker.Savage.P6SHegemone;

class Exocleaver : Components.GenericAOEs
{
    public bool FirstDone { get; private set; }
    private AOEShapeCone _cone = new(30, 15.Degrees());
    private List<Angle> _directions = new();

    public Exocleaver() : base(ActionID.MakeSpell(AID.ExocleaverAOE2)) { }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (NumCasts > 0)
            yield break;

        // TODO: timing
        var offset = (FirstDone ? 30 : 0).Degrees();
        foreach (var dir in _directions)
            yield return new(_cone, module.PrimaryActor.Position, dir + offset);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ExocleaverAOE1)
            _directions.Add(caster.Rotation);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ExocleaverAOE1)
            FirstDone = true;
    }
}
