namespace BossMod.Stormblood.Dungeon.D06AlaMhigo.D062AulusMalAsina;

public enum OID : uint
{
    Boss = 0x1BA6,
    Helper = 0x233C,
    AulusMalAsina = 0x0, // R0.500, x0-5, None type
    AulusMalAsina2 = 0x18D6, // R0.500, x2-7
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1ea50f = 0x1EA50F, // R2.000, x1-2, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x3, EventObj type
    PrototypeBit = 0x1BA7, // R0.600, x0-5 (spawn during fight)
    EmptyVessel = 0x1BAF, // R0.500, x0 (spawn during fight)
    PrototypeBit2 = 0x1BA8, // R0.600, x0 (spawn during fight)
    N = 0x1DD8, // R3.000, x0 (spawn during fight)

}

public enum AID : uint
{
    Attack = 9304, // Boss->player, no cast, single-target
    ManaBurst = 8271, // Boss->self, 3.0s cast, range 40 circle
    OrderToCharge = 8279, // Boss->self, 3.0s cast, single-target
    Ability = 8275, // 1BA7/1BA8->location, no cast, ???
    OrderToFire = 8280, // Boss->self, 3.0s cast, single-target
    AetherochemicalGrenado = 8282, // 1BA7->location, 3.5s cast, range 8 circle
    IntegratedAetheromodulator = 8283, // 1BA7->self, 3.0s cast, range 15+R circle
    MagitekDisruptor = 8272, // Boss->self, 3.0s cast, range 40 circle
    Weaponskill1 = 8274, // 18D6->player, no cast, single-target
    Weaponskill2 = 8273, // 18D6->self, no cast, single-target
    Mindjack = 8270, // Boss->self, 3.0s cast, ???
    MagitekRay = 8276, // 1BA8->self, 3.0s cast, range 45+R width 2 rect
    Weaponskill3 = 8278, // 18D6->1BAF, no cast, single-target
    Demimagicks = 8286, // 18D6->player, 5.0s cast, range 5 circle
    Demimagicks2 = 8285, // Boss->self, 5.0s cast, single-target
}

public enum IconID : uint
{
    Icon96 = 96, // player
}

public enum TetherID : uint
{
    MindjackTether = 45, // 1BAF->player
}

class AetherochemicalGrenado(BossModule module) : Components.StandardAOEs(module, AID.AetherochemicalGrenado, 8);
class IntegratedAetheromodulator(BossModule module) : Components.GenericAOEs(module, AID.IntegratedAetheromodulator)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoes.Add(new AOEInstance(new AOEShapeDonut(11.5f, 18.5f), caster.Position, Activation: WorldState.FutureTime(5)));
    }

    public override void Update()
    {
        if (_aoes.Count > 0)
            _aoes.RemoveAll(x => DateTime.Now >= x.Activation);
    }
}
class MagitekRay(BossModule module) : Components.StandardAOEs(module, AID.MagitekRay, new AOEShapeRect(45, 2, 45));
class Demimagicks(BossModule module) : Components.SpreadFromCastTargets(module, AID.Demimagicks, 5);
class Mindjack(BossModule module) : Components.Chains(module, (uint)TetherID.MindjackTether, AID.Mindjack, 1, false);

class D062AulusMalAsinaStates : StateMachineBuilder
{
    public D062AulusMalAsinaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<IntegratedAetheromodulator>()
            .ActivateOnEnter<MagitekRay>()
            .ActivateOnEnter<Demimagicks>()
            .ActivateOnEnter<Mindjack>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 247, NameID = 6038)]
public class D062AulusMalAsina(WorldState ws, Actor primary) : BossModule(ws, primary, new(250, -70), new ArenaBoundsCircle(20));
