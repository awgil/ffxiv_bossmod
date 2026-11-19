namespace BossMod.Endwalker.Dungeon.D07Smileton.D073BigCheese;

public enum OID : uint
{
    Boss = 0x34D3, // R6.000, x1
    Bomb = 0x3585, // R1.000, x2
    ExcavationBombDispenser = 0x38CF, // R1.000, x2
    ExcavationBomb = 0x3741, // R1.000, spawned by Dispense Explosives
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 26444, // Boss->player, no cast, single-target
    ExplosivesDistribution = 26446, // Boss->self, 3.0s cast, single-target, visual (bombs onto conveyors)
    IronKiss = 26445, // Bomb->location, 1.5s cast, range 16 circle
    RightDisassembler = 26447, // Boss->location, 8.0s cast, range 30 width 10 rect
    LeftDisassembler = 26448, // Boss->location, 8.0s cast, range 30 width 10 rect
    PiercingMissile = 26449, // Boss->player, 5.0s cast, single-target, tankbuster
    ElectricArc = 26451, // Boss->player, 5.0s cast, range 8 circle stack
    DispenseExplosives = 27696, // Boss->self, 3.0s cast, single-target, visual
    ExcavationBombVisual = 27700, // ExcavationBombDispenser->self, no cast, single-target, places an excavation bomb
    ExplosivePower = 27697, // Boss->self, 3.0s cast, single-target, detonates excavation bombs
    ViolentDischarge = 26450, // Boss->self, 5.0s cast, range 60 circle, raidwide
    LevelingMissile = 26452, // Boss->self, 5.0s cast, single-target, visual (spreads)
    LevelingMissileAOE = 26453, // Helper->player, 5.0s cast, range 6 circle spread
    Excavated = 27698, // ExcavationBombDispenser->self, no cast, range 8 circle
}

public enum IconID : uint
{
    PiercingMissile = 218, // player
    ElectricArc = 62, // player
}

class IronKiss(BossModule module) : Components.GenericAOEs(module, AID.IronKiss)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(16);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00800040 || index > 1)
            return;
        var activation = WorldState.FutureTime(8.1f);
        _aoes.Clear();
        _aoes.Add(new(_shape, index == 0 ? new(-31, -57) : new(-15, -57), default, activation));
        _aoes.Add(new(_shape, index == 0 ? new(-15, -31) : new(-31, -31), default, activation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action != WatchedAction)
            return;
        var i = _aoes.FindIndex(a => a.Origin.AlmostEqual(spell.LocXZ, 1));
        if (i < 0)
        {
            ReportError($"Unexpected bomb position {spell.LocXZ}");
            _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
        else
        {
            _aoes[i] = _aoes[i] with { Activation = Module.CastFinishAt(spell) };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            _aoes.RemoveAll(a => a.Origin.AlmostEqual(spell.TargetXZ, 1));
    }
}

class ExcavationBombs(BossModule module) : Components.GenericAOEs(module, AID.Excavated)
{
    private DateTime _detonation;

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => Module.Enemies(OID.ExcavationBomb).Where(b => !b.IsDestroyed).Select(b => new AOEInstance(_shape, b.Position, default, _detonation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ExplosivePower)
            _detonation = Module.CastFinishAt(spell, 1.2f);
    }
}

class Disassembler(BossModule module) : Components.GroupedAOEs(module, [AID.RightDisassembler, AID.LeftDisassembler], new AOEShapeRect(30, 5));
class PiercingMissile(BossModule module) : Components.SingleTargetCast(module, AID.PiercingMissile);
class ViolentDischarge(BossModule module) : Components.RaidwideCast(module, AID.ViolentDischarge);
class LevelingMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID.LevelingMissileAOE, 6);
class ElectricArc(BossModule module) : Components.StackWithCastTargets(module, AID.ElectricArc, 8);

class D073BigCheeseStates : StateMachineBuilder
{
    public D073BigCheeseStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IronKiss>()
            .ActivateOnEnter<ExcavationBombs>()
            .ActivateOnEnter<Disassembler>()
            .ActivateOnEnter<PiercingMissile>()
            .ActivateOnEnter<ElectricArc>()
            .ActivateOnEnter<ViolentDischarge>()
            .ActivateOnEnter<LevelingMissile>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 794, NameID = 10336)]
public class D073BigCheese(ModuleInit init) : BossModule(init, new(-22, -44), new ArenaBoundsRect(14.5f, 10));
