namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class HandOfTheDestroyer : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new();

    private static readonly AOEShapeRect _shape = new(90, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HandOfTheDestroyerWrathAOE or AID.HandOfTheDestroyerJudgmentAOE)
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HandOfTheDestroyerWrathAOE or AID.HandOfTheDestroyerJudgmentAOE)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}

class BrokenWorld : Components.SelfTargetedAOEs
{
    public BrokenWorld() : base(ActionID.MakeSpell(AID.BrokenWorldAOE), new AOEShapeCircle(30)) { } // TODO: determine falloff
}

// this is not an official mechanic name - it refers to broken world + hand of the destroyer combo, which creates multiple small aoes
class BrokenShards : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new();

    private static readonly WPos[] _eastLocations = { new(-30.0f, 266.9f), new(-46.5f, 269.6f), new(-26.2f, 292.9f), new(-2.8f, 283.5f), new(-37.4f, 283.7f), new(1.6f, 271.5f), new(-18.8f, 278.8f), new(-12.3f, 298.3f), new(-34.1f, 250.5f) };
    private static readonly WPos[] _westLocations = { new(-6.9f, 268.0f), new(-0.2f, 285.0f), new(-25.6f, 298.5f), new(-34.2f, 283.5f), new(-11.6f, 293.5f), new(-46.1f, 270.5f), new(-18.1f, 279.0f), new(-40.3f, 290.5f), new(-2.1f, 252.0f) };
    private static readonly AOEShapeCircle _shape = new(20);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var locs = (AID)spell.Action.ID switch
        {
            AID.BrokenShardsE => _eastLocations,
            AID.BrokenShardsW => _westLocations,
            _ => null
        };
        if (locs != null)
            _aoes.AddRange(locs.Select(p => new AOEInstance(_shape, p, default, spell.NPCFinishAt)));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BrokenShardsAOE)
        {
            ++NumCasts;
            if (_aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 0.1f)) != 1)
                module.ReportError(this, $"Unexpected shard position: {caster.Position}");
        }
    }
}

class LightningStorm : Components.SpreadFromCastTargets
{
    public LightningStorm() : base(ActionID.MakeSpell(AID.LightningStorm), 5) { }
}
