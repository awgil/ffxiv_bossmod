namespace BossMod.Endwalker.Alliance.A34Eulogia;

class FirstBlush(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos position, Angle rotation, DateTime activation, uint AID)> _castersunsorted = [];
    private List<(WPos position, Angle rotation, DateTime activation)> _casters = [];
    private static readonly AOEShapeRect _shape = new(120, 12.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(_shape, _casters[0].position, _casters[0].rotation, _casters[0].activation, ArenaColor.Danger);
        if (_casters.Count > 1)
            yield return new(_shape, _casters[1].position, _casters[1].rotation, _casters[1].activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FirstBlush1 or AID.FirstBlush2 or AID.FirstBlush3 or AID.FirstBlush4)
        {
            _castersunsorted.Add((caster.Position, spell.Rotation, spell.NPCFinishAt, spell.Action.ID)); //casters appear in random order in raw ops
            _casters = _castersunsorted.OrderBy(x => x.AID).Select(x => (x.position, x.rotation, x.activation)).ToList();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID is AID.FirstBlush1 or AID.FirstBlush2 or AID.FirstBlush3 or AID.FirstBlush4)
        {
            ++NumCasts;
            _casters.RemoveAt(0);
            _castersunsorted.Clear();
        }
    }
}
