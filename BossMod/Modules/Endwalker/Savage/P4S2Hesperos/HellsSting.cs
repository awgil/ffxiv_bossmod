namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to hell's sting mechanic (part of curtain call sequence)
class HellsSting(BossModule module) : BossComponent(module)
{
    public int NumCasts { get; private set; }

    private readonly AOEShapeCone _cone = new(50, 15.Degrees());
    private readonly List<Angle> _directions = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumCasts >= _directions.Count * 2)
            return;

        if (ConeDirections().Any(x => actor.Position.InCone(Module.PrimaryActor.Position, x, _cone.HalfAngle)))
            hints.Add("GTFO from cone!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (NumCasts >= _directions.Count * 2)
            return;

        foreach (var dir in ConeDirections())
            _cone.Draw(Arena, Module.PrimaryActor.Position, dir);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HellsStingAOE1)
            _directions.Add(caster.Rotation);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HellsStingAOE1 or AID.HellsStingAOE2)
            ++NumCasts;
    }

    private IEnumerable<Angle> ConeDirections()
    {
        return NumCasts < _directions.Count ? _directions : _directions.Select(x => x + 22.5f.Degrees());
    }
}
