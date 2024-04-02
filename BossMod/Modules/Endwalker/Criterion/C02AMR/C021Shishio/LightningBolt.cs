namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class LightningBolt : Components.LocationTargetedAOEs
{
    public LightningBolt(AID aid) : base(ActionID.MakeSpell(aid), 6) { }
}
class NLightningBolt : LightningBolt { public NLightningBolt() : base(AID.NLightningBoltAOE) { } }
class SLightningBolt : LightningBolt { public SLightningBolt() : base(AID.SLightningBoltAOE) { } }

class CloudToCloud : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new();

    private static readonly AOEShapeRect _shape1 = new(100, 1);
    private static readonly AOEShapeRect _shape2 = new(100, 3);
    private static readonly AOEShapeRect _shape3 = new(100, 6);

    public bool Active => _aoes.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            var deadline = _aoes[0].Activation.AddSeconds(1.4f);
            foreach (var aoe in _aoes.TakeWhile(aoe => aoe.Activation <= deadline))
                yield return aoe;
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action);
        if (shape != null)
            _aoes.Add(new(shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (ShapeForAction(spell.Action) != null)
        {
            ++NumCasts;
            var numRemoved = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            if (numRemoved != 1)
                module.ReportError(this, $"Unexpected number of matching aoes: {numRemoved}");
        }
    }

    private AOEShapeRect? ShapeForAction(ActionID action) => (AID)action.ID switch
    {
        AID.NCloudToCloud1 or AID.SCloudToCloud1 => _shape1,
        AID.NCloudToCloud2 or AID.SCloudToCloud2 => _shape2,
        AID.NCloudToCloud3 or AID.SCloudToCloud3 => _shape3,
        _ => null
    };
}
