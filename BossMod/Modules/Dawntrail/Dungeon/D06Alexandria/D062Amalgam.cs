namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D062Amalgam;

public enum OID : uint
{
    Boss = 0x416A, // R5.075
    AddBlock = 0x416B, // R1.2
    Helper = 0x233C // R0.5
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    ElectrowaveVisual = 36337, // Boss->self, 5.0s cast, single-target
    Electrowave = 36338, // Helper->self, 5.0s cast, range 54 circle, raidwide
    Disassembly = 36323, // Boss->self, 5.0s cast, range 40 circle, raidwide

    CentralizedCurrent = 36327, // Boss->self, 5.0s cast, range 90 width 15 rect, central line AOE

    SplitCurrentVisual = 36329, // Boss->self, 5.0s cast, single-target
    SplitCurrent1 = 36331, // Helper->self, 5.0s cast, range 90 width 25 rect, side line AOE
    SplitCurrent2 = 36330, // Helper->self, 5.0s cast, range 90 width 25 rect, side line AOE

    SupercellMatrix1 = 39136, // Helper->self, 10.1s cast, triangle cone, 90-degrees, 40 range
    SupercellMatrix2 = 39138, // Helper->self, 7.8s cast, range 55 width 8 rect, portal line AOEs

    StaticSparkVisual = 36325, // Boss->self, no cast, single-target
    StaticSpark = 36334, // Helper->player, 5.0s cast, range 6 circle, Spread
    Amalgamight = 36339, // Boss->player, 5.0s cast, single-target, tankbuster
    Voltburst = 36336, // Helper->location, 4.0s cast, range 6 circle, 3x baited AOEs

    SuperboltVisual = 36332, // Boss->self, 5.0s cast, single-target
    Superbolt = 36333, // Helper->players, 5.0s cast, range 6 circle, stack

    TernaryChargeVisual = 39253, // Boss->self, 4.0s cast, single-target
    TernaryCharge1 = 39254, // Helper->location, 4.0s cast, range 10 circle
    TernaryCharge2 = 39255, // Helper->location, 6.0s cast, range 10-20 donut
    TernaryCharge3 = 39256, // Helper->location, 8.0s cast, range 20-30 donut
}

sealed class ElectrowaveArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrowave && Arena.Bounds.Radius > 21f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 23f)], [new Square(center, 20f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.5d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x27 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
            _aoe = [];
        }
    }
}

sealed class ElectrowaveDisassembly(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Electrowave, (uint)AID.Disassembly]);
sealed class CentralizedCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CentralizedCurrent, new AOEShapeRect(90f, 7.5f));

sealed class SplitCurrent(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SplitCurrent1, (uint)AID.SplitCurrent2], new AOEShapeRect(90f, 12.5f));
sealed class SupercellMatrix1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SupercellMatrix1, new AOEShapeTriCone(40f, 45.Degrees()));
sealed class SupercellMatrix2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SupercellMatrix2, new AOEShapeRect(55f, 4f));
sealed class StaticSpark(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.StaticSpark, 6f);
sealed class Amalgamight(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Amalgamight);
sealed class Voltburst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Voltburst, 6f);
sealed class Superbolt(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Superbolt, 6f, 4, 4);

sealed class TernaryCharge(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TernaryCharge1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.TernaryCharge1 => 0,
                (uint)AID.TernaryCharge2 => 1,
                (uint)AID.TernaryCharge3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class D062AmalgamStates : StateMachineBuilder
{
    public D062AmalgamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectrowaveArenaChange>()
            .ActivateOnEnter<ElectrowaveDisassembly>()
            .ActivateOnEnter<CentralizedCurrent>()
            .ActivateOnEnter<SplitCurrent>()
            .ActivateOnEnter<SupercellMatrix1>()
            .ActivateOnEnter<SupercellMatrix2>()
            .ActivateOnEnter<StaticSpark>()
            .ActivateOnEnter<Amalgamight>()
            .ActivateOnEnter<Voltburst>()
            .ActivateOnEnter<Superbolt>()
            .ActivateOnEnter<TernaryCharge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827u, NameID = 12864u)]
public sealed class D062Amalgam(WorldState ws, Actor primary) : BossModule(ws, primary, new(-533f, -373f), new ArenaBoundsSquare(23f));