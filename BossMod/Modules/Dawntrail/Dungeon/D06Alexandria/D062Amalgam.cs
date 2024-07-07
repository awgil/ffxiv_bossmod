namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D062Amalgam;

public enum OID : uint
{
    Boss = 0x416A, // R5.075, x1
    Helper = 0x233C, // R0.500, x28, 523 type
    AddBlock = 0x416B, // R1.200, x9
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Electrowave1 = 36337, // Boss->self, 5.0s cast, single-target
    Electrowave2 = 36338, // Helper->self, 5.0s cast, range 54 circle // Raidwide

    Disassembly = 36323, // Boss->self, 5.0s cast, range 40 circle // Half room cleave

    UnknownSpell = 36325, // Boss->self, no cast, single-target
    CentralizedCurrent = 36327, // Boss->self, 5.0s cast, range 90 width 15 rect // Central line AOE

    SplitCurrent1 = 36329, // Boss->self, 5.0s cast, single-target
    SplitCurrent2 = 36331, // Helper->self, 5.0s cast, range 90 width 25 rect // Side line AOE
    SplitCurrent3 = 36330, // Helper->self, 5.0s cast, range 90 width 25 rect // Side line AOE

    SupercellMatrix1 = 39136, // Helper->self, 10.1s cast, ???
    SupercellMatrix2 = 39138, // Helper->self, 7.8s cast, range 55 width 8 rect // Portal line AOEs

    StaticSpark = 36334, // Helper->player, 5.0s cast, range 6 circle // Spread
    Amalgamight = 36339, // Boss->player, 5.0s cast, single-target // Tankbuster
    Voltburst = 36336, // Helper->location, 4.0s cast, range 6 circle // 3x Baited AOEs

    Superbolt1 = 36332, // Boss->self, 5.0s cast, single-target
    Superbolt2 = 36333, // Helper->players, 5.0s cast, range 6 circle // Stack

    TernaryChargeVisual = 39253, // Boss->self, 4.0s cast, single-target
    TernaryChargeCenter = 39254, // Helper->location, 4.0s cast, range 10 circle
    TernaryChargeMid = 39255, // Helper->location, 6.0s cast, range 10-20 donut
    TernaryChargeOuter = 39256, // Helper->location, 8.0s cast, range 20-30 donut
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Boss/Helper->player, extra=0x1/0x2/0x3
    Paralysis = 3463, // Boss/Helper->player, extra=0x0
}

public enum IconID : uint
{
    Spreadmarker = 139, // player
    Tankbuster = 218, // player
    Stackmarker = 161, // player
}
class Electrowave2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave2));
class CentralizedCurrent(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CentralizedCurrent), new AOEShapeRect(90, 7.5f, 90));
class SplitCurrent2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SplitCurrent2), new AOEShapeRect(90, 30, -5, DirectionOffset: -90.Degrees()));
class SplitCurrent3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SplitCurrent3), new AOEShapeRect(90, 30, -5, DirectionOffset: 90.Degrees()));
class SupercellMatrix2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SupercellMatrix2), new AOEShapeRect(55, 4));
class StaticSpark(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.StaticSpark), 6);
class Amalgamight(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Amalgamight));
class Voltburst(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Voltburst), 6);
class Superbolt2(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Superbolt2), 6, 4);

class TernaryCharge(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.TernaryChargeVisual))
{
    private readonly List<Actor> _castersTernaryChargeCenter = [];
    private readonly List<Actor> _castersTernaryChargeMid = [];

    private static readonly AOEShape _shapeTernaryChargeCenter = new AOEShapeCircle(10);
    private static readonly AOEShape _shapeTernaryChargeMid = new AOEShapeDonut(10, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersTernaryChargeCenter.Count > 0)
            return _castersTernaryChargeCenter.Select(c => new AOEInstance(_shapeTernaryChargeCenter, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
        else
            return _castersTernaryChargeMid.Select(c => new AOEInstance(_shapeTernaryChargeMid, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.TernaryChargeCenter => _castersTernaryChargeCenter,
        AID.TernaryChargeMid => _castersTernaryChargeMid,
        _ => null
    };
}
class TernaryChargeOuter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TernaryChargeOuter), new AOEShapeDonut(20, 30));

class D062AmalgamStates : StateMachineBuilder
{
    public D062AmalgamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrowave2>()
            .ActivateOnEnter<CentralizedCurrent>()
            .ActivateOnEnter<SplitCurrent2>()
            .ActivateOnEnter<SplitCurrent3>()
            .ActivateOnEnter<SupercellMatrix2>()
            .ActivateOnEnter<StaticSpark>()
            .ActivateOnEnter<Amalgamight>()
            .ActivateOnEnter<Voltburst>()
            .ActivateOnEnter<Superbolt2>()
            .ActivateOnEnter<TernaryCharge>()
            .ActivateOnEnter<TernaryChargeOuter>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12864)]
public class D062Amalgam(WorldState ws, Actor primary) : BossModule(ws, primary, new(-533, -373), new ArenaBoundsSquare(20));
