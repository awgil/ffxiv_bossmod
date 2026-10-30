namespace BossMod.Dawntrail.Dungeon.D08TenderValley.D082Anthracite;

public enum OID : uint
{
    Boss = 0x41BE,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872,

    AnthrabombLong = 36401,
    AnthrabombShort = 36402,
    AnthrabombSetup = 36542,
    AnthrabombMid = 36543,
    AnthrabombMidResolve = 36544,
    HotBlastLong = 36545,
    HotBlastLongResolve = 36546,
    Carniflagration = 36547,
    CarniflagrationVisual = 36548,
    AnthrabombFast = 36549,
    AnthrabombFastResolve = 36550,
    HotBlastShort = 36551,
    HotBlastShortResolve = 36552,
    AnthrabombSpread = 36553,
    BurningCoalsVisual = 36554,
    BurningCoals = 36555,
    CarbonaceousCombustion = 36556,
    CarbonaceousCombustionAOE = 36557,
    ChimneySmack = 38467,
    ChimneySmackTarget = 38468
}

class CarbonaceousCombustion(BossModule module) : Components.RaidwideCastDelay(module, AID.CarbonaceousCombustion, AID.CarbonaceousCombustionAOE, 0.5f);
class BurningCoals(BossModule module) : Components.StackWithCastTargets(module, AID.BurningCoals, 6, 4);
class ChimneySmack(BossModule module) : Components.SingleTargetCast(module, AID.ChimneySmackTarget);
class AnthrabombSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.AnthrabombSpread, 6);
class CarniflagrationHint(BossModule module) : Components.CastHint(module, AID.Carniflagration, "Bomb sequence + spreads");

class Anthrabombs(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(10);
    private static readonly AOEShapeRect _line = new(40, 3);
    private readonly List<(AID aid, ulong caster, AOEInstance aoe)> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Select(e => e.aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AnthrabombLong:
            case AID.AnthrabombShort:
            case AID.AnthrabombMid:
            case AID.AnthrabombFast:
                _aoes.Add(((AID)spell.Action.ID, caster.InstanceID, new(_circle, caster.Position, Activation: Module.CastFinishAt(spell))));
                break;
            case AID.HotBlastLong:
            case AID.HotBlastShort:
                _aoes.Add(((AID)spell.Action.ID, caster.InstanceID, new(_line, caster.Position, spell.Rotation, Module.CastFinishAt(spell))));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        var index = _aoes.FindIndex(e => e.aid == aid && e.caster == caster.InstanceID);
        if (index >= 0)
            _aoes.RemoveAt(index);
    }
}

class D082AnthraciteStates : StateMachineBuilder
{
    public D082AnthraciteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CarbonaceousCombustion>()
            .ActivateOnEnter<Anthrabombs>()
            .ActivateOnEnter<CarniflagrationHint>()
            .ActivateOnEnter<AnthrabombSpread>()
            .ActivateOnEnter<BurningCoals>()
            .ActivateOnEnter<ChimneySmack>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "CerQ", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12853)]
public class D082Anthracite(WorldState ws, Actor primary) : BossModule(ws, primary, new(-130, -57), new ArenaBoundsRect(15, 20));
