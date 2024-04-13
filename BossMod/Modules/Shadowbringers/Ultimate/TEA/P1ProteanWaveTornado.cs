namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1ProteanWaveTornado : Components.GenericBaitAway
{
    private readonly IReadOnlyList<Actor> _liquidRage;

    private static readonly AOEShapeCone _shape = new(40, 15.Degrees());

    public P1ProteanWaveTornado(BossModule module, bool enableHints) : base(module, ActionID.MakeSpell(AID.ProteanWaveTornadoInvis))
    {
        _liquidRage = module.Enemies(OID.LiquidRage);
        EnableHints = enableHints;
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        foreach (var tornado in _liquidRage)
        {
            var target = Raid.WithoutSlot().Closest(tornado.Position);
            if (target != null)
                CurrentBaits.Add(new(tornado, target, _shape));
        }
    }
}

class P1ProteanWaveTornadoVisCast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ProteanWaveTornadoVis), new AOEShapeCone(40, 15.Degrees()));
class P1ProteanWaveTornadoVisBait(BossModule module) : P1ProteanWaveTornado(module, false);
class P1ProteanWaveTornadoInvis(BossModule module) : P1ProteanWaveTornado(module, true);
