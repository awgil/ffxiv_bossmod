namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D062Amalgam;

public enum OID : uint
{
    Boss = 0x416A, // R5.075, x1
    Helper = 0x233C, // R0.500, x28, Helper type
    AddBlock = 0x416B, // R1.200, x9
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Electrowave = 36337, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    ElectrowaveAOE = 36338, // Helper->self, 5.0s cast, range 54 circle, raidwide
    Disassembly = 36323, // Boss->self, 5.0s cast, range 40 circle, raidwide
    SupercellMatrix = 39136, // Helper->self, 10.1s cast, ???
    SupercellMatrixRect = 39138, // Helper->self, 7.8s cast, range 55 width 8 rect
    SupercellMatrixEnd = 36325, // Boss->self, no cast, single-target, visual (???)
    CentralizedCurrent = 36327, // Boss->self, 5.0s cast, range 90 width 15 rect
    SplitCurrent = 36329, // Boss->self, 5.0s cast, single-target, visual (side cleaves)
    SplitCurrentAOE1 = 36330, // Helper->self, 5.0s cast, range 90 width 25 rect
    SplitCurrentAOE2 = 36331, // Helper->self, 5.0s cast, range 90 width 25 rect
    StaticSpark = 36334, // Helper->player, 5.0s cast, range 6 circle, spread
    Amalgamight = 36339, // Boss->player, 5.0s cast, single-target, tankbuster
    Voltburst = 36336, // Helper->location, 4.0s cast, range 6 circle puddle
    Superbolt = 36332, // Boss->self, 5.0s cast, single-target
    SuperboltAOE = 36333, // Helper->none, 5.0s cast, range 6 circle, stack
    TernaryCharge = 39253, // Boss->self, 4.0s cast, single-target
    TernaryChargeAOE1 = 39254, // Helper->location, 4.0s cast, range 10 circle
    TernaryChargeAOE2 = 39255, // Helper->location, 6.0s cast, range 10-20 donut
    TernaryChargeAOE3 = 39256, // Helper->location, 8.0s cast, range 20-30 donut
}

public enum IconID : uint
{
    StaticSpark = 139, // player
    Amalgamight = 218, // player
    Superbolt = 161, // player
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, AID.ElectrowaveAOE);
class Disassembly(BossModule module) : Components.RaidwideCast(module, AID.Disassembly);
class SupercellMatrix(BossModule module) : Components.StandardAOEs(module, AID.SupercellMatrix, new AOEShapeRect(28.2843f, 28.2843f));
class SupercellMatrixRect(BossModule module) : Components.StandardAOEs(module, AID.SupercellMatrixRect, new AOEShapeRect(55, 4));
class CentralizedCurrent(BossModule module) : Components.StandardAOEs(module, AID.CentralizedCurrent, new AOEShapeRect(45, 7.5f, 45));

class SplitCurrent(BossModule module) : Components.GenericAOEs(module, AID.SplitCurrent)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shape = new(45, 12.5f, 45);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var offset = (_shape.HalfWidth + 5) * spell.Rotation.ToDirection().OrthoL();
            _aoes.Add(new(_shape, caster.Position + offset, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.Add(new(_shape, caster.Position - offset, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _aoes.Clear();
    }
}

class StaticSpark(BossModule module) : Components.SpreadFromCastTargets(module, AID.StaticSpark, 6)
{
    private readonly SupercellMatrixRect? _supercell = module.FindComponent<SupercellMatrixRect>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // only spread after aoes are done
        if (_supercell == null || _supercell.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class Amalgamight(BossModule module) : Components.SingleTargetCast(module, AID.Amalgamight);
class Voltburst(BossModule module) : Components.StandardAOEs(module, AID.Voltburst, 6);
class Superbolt(BossModule module) : Components.StackWithCastTargets(module, AID.SuperboltAOE, 6, 4);

class TernaryCharge(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TernaryChargeAOE1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.TernaryChargeAOE1 => 0,
            AID.TernaryChargeAOE2 => 1,
            AID.TernaryChargeAOE3 => 2,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2)))
            ReportError($"Unexpected ring {order}");
    }
}

class D062AmalgamStates : StateMachineBuilder
{
    public D062AmalgamStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<Disassembly>()
            .ActivateOnEnter<SupercellMatrix>()
            .ActivateOnEnter<SupercellMatrixRect>()
            .ActivateOnEnter<CentralizedCurrent>()
            .ActivateOnEnter<SplitCurrent>()
            .ActivateOnEnter<StaticSpark>()
            .ActivateOnEnter<Amalgamight>()
            .ActivateOnEnter<Voltburst>()
            .ActivateOnEnter<Superbolt>()
            .ActivateOnEnter<TernaryCharge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12864)]
public class D062Amalgam(WorldState ws, Actor primary) : BossModule(ws, primary, new(-533, -373), new ArenaBoundsSquare(20));
