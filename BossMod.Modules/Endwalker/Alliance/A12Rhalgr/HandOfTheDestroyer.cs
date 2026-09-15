namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class HandOfTheDestroyer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(90, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HandOfTheDestroyerWrathAOE or AID.HandOfTheDestroyerJudgmentAOE)
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HandOfTheDestroyerWrathAOE or AID.HandOfTheDestroyerJudgmentAOE)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}

class BrokenWorld(BossModule module) : Components.StandardAOEs(module, AID.BrokenWorldAOE, new AOEShapeCircle(30)); // TODO: determine falloff

// this is not an official mechanic name - it refers to broken world + hand of the destroyer combo, which creates multiple small aoes
class BrokenShards(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly WPos[] _eastLocations = [new(-30.0f, 266.9f), new(-46.5f, 269.6f), new(-26.2f, 292.9f), new(-2.8f, 283.5f), new(-37.4f, 283.7f), new(1.6f, 271.5f), new(-18.8f, 278.8f), new(-12.3f, 298.3f), new(-34.1f, 250.5f)];
    private static readonly WPos[] _westLocations = [new(-6.9f, 268.0f), new(-0.2f, 285.0f), new(-25.6f, 298.5f), new(-34.2f, 283.5f), new(-11.6f, 293.5f), new(-46.1f, 270.5f), new(-18.1f, 279.0f), new(-40.3f, 290.5f), new(-2.1f, 252.0f)];
    private static readonly AOEShapeCircle _shape = new(20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var locs = (AID)spell.Action.ID switch
        {
            AID.BrokenShardsE => _eastLocations,
            AID.BrokenShardsW => _westLocations,
            _ => null
        };
        if (locs != null)
            _aoes.AddRange(locs.Select(p => new AOEInstance(_shape, p, default, Module.CastFinishAt(spell))));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BrokenShardsAOE)
        {
            ++NumCasts;
            if (_aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 0.1f)) != 1)
                ReportError($"Unexpected shard position: {caster.Position}");
        }
    }
}

class LightningStorm(BossModule module) : Components.SpreadFromCastTargets(module, AID.LightningStorm, 5);
