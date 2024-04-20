namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class FlameAndSulphur(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeFlameExpand = new(46, 5);
    private static readonly AOEShapeRect _shapeFlameSplit = new(46, 2.5f);
    private static readonly AOEShapeCircle _shapeRockExpand = new(11);
    private static readonly AOEShapeDonut _shapeRockSplit = new(6, 16);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BrazenBalladExpanding:
                foreach (var a in Module.Enemies(OID.FlameAndSulphurFlame))
                    _aoes.Add(new(_shapeFlameExpand, a.Position, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                foreach (var a in Module.Enemies(OID.FlameAndSulphurRock))
                    _aoes.Add(new(_shapeRockExpand, a.Position, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                break;
            case AID.BrazenBalladSplitting:
                foreach (var a in Module.Enemies(OID.FlameAndSulphurFlame))
                {
                    var offset = a.Rotation.ToDirection().OrthoL() * 7.5f;
                    _aoes.Add(new(_shapeFlameSplit, a.Position + offset, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                    _aoes.Add(new(_shapeFlameSplit, a.Position - offset, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                }
                foreach (var a in Module.Enemies(OID.FlameAndSulphurRock))
                    _aoes.Add(new(_shapeRockSplit, a.Position, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NFireSpreadExpand or AID.NFireSpreadSplit or AID.NFallingRockExpand or AID.NFallingRockSplit
            or AID.SFireSpreadExpand or AID.SFireSpreadSplit or AID.SFallingRockExpand or AID.SFallingRockSplit)
        {
            ++NumCasts;
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}
