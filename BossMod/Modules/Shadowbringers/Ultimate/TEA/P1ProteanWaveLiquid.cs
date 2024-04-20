namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1ProteanWaveLiquidVisBoss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ProteanWaveLiquidVisBoss), new AOEShapeCone(40, 15.Degrees()));
class P1ProteanWaveLiquidVisHelper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ProteanWaveLiquidVisHelper), new AOEShapeCone(40, 15.Degrees()));

// single protean ("shadow") that fires in the direction the boss is facing
class P1ProteanWaveLiquidInvisFixed(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ProteanWaveLiquidInvisBoss))
{
    private readonly Actor? _source = module.Enemies(OID.BossP1).FirstOrDefault();

    private static readonly AOEShapeCone _shape = new(40, 15.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_shape, _source.Position, _source.Rotation);
    }
}

// proteans baited on 4 closest targets
class P1ProteanWaveLiquidInvisBaited(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.ProteanWaveLiquidInvisHelper))
{
    private readonly Actor? _source = module.Enemies(OID.BossP1).FirstOrDefault();

    private static readonly AOEShapeCone _shape = new(40, 15.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null)
            foreach (var target in Raid.WithoutSlot().SortedByRange(_source.Position).Take(4))
                CurrentBaits.Add(new(_source, target, _shape));
    }
}
