namespace BossMod.Dawntrail.Hunt.RankA.Heshuala;

public enum OID : uint
{
    Boss = 0x416E, // R6.600, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    HigherPower1 = 39095, // Boss->self, 2.0s cast, single-target, visual (apply debuffs, 3 charges)
    HigherPower2 = 39096, // Boss->self, 2.0s cast, single-target, visual (apply debuffs, 4 charges)
    HigherPower3 = 39097, // Boss->self, 2.0s cast, single-target, visual (apply debuffs, 5 charges)
    HigherPower4 = 39098, // Boss->self, 2.0s cast, single-target, visual (apply debuffs, 6 charges)
    SpinshockCW = 39099, // Boss->self, 5.0s cast, range 50 90-degree cone
    SpinshockCCW = 39100, // Boss->self, 5.0s cast, range 50 90-degree cone
    SpinshockRest = 39101, // Boss->self, 0.7s cast, range 50 90-degree cone
    ShockingCross = 39102, // Boss->self, 1.7s cast, range 50 width 10 cross, stuns and makes lightning bolt deadly
    XMarksTheShock = 39103, // Boss->self, 1.7s cast, range 50 width 10 cross, rotated 45 degrees, stuns and makes lightning bolt deadly
    LightningBolt = 39104, // Boss->location, 2.0s cast, range 5 circle
    ElectricalOverload = 39105, // Boss->self, 4.0s cast, range 50 circle, raidwide
}

public enum SID : uint
{
    ShockingCross = 3977, // Boss->Boss, extra=0x0
    XMarksTheShock = 3978, // Boss->Boss, extra=0x0
    ElectricalCharge = 3979, // Boss->Boss, extra=0x6/0x5/0x4/0x3/0x2/0x1
    NumbingCurrent = 3980, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    RotateCW = 167, // Boss
    RotateCCW = 168, // Boss
}

class HigherPower(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private int _numCharges;
    private Angle _crossOffset;

    private static readonly AOEShapeCone _shapeSpinshock = new(50, 45.Degrees());
    private static readonly AOEShapeCross _shapeCross = new(50, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 1)
            yield return _aoes[1];
        if (_aoes.Count > 0)
            yield return _aoes[0] with { Color = ArenaColor.Danger };
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ShockingCross:
                _crossOffset = 0.Degrees();
                break;
            case SID.XMarksTheShock:
                _crossOffset = 45.Degrees();
                break;
            case SID.ElectricalCharge:
                _numCharges = status.Extra;
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var offset = (AID)spell.Action.ID switch
        {
            AID.SpinshockCW => -90.Degrees(),
            AID.SpinshockCCW => 90.Degrees(),
            _ => default
        };
        if (offset != default)
        {
            for (int i = 0; i < _numCharges; ++i)
                _aoes.Add(new(_shapeSpinshock, caster.Position, spell.Rotation + i * offset, Module.CastFinishAt(spell, i * 2.7f)));
            _aoes.Add(new(_shapeCross, caster.Position, spell.Rotation + _crossOffset, Module.CastFinishAt(spell, _numCharges * 2.7f + 3.7f)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SpinshockCW or AID.SpinshockCCW or AID.SpinshockRest or AID.ShockingCross or AID.XMarksTheShock && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, 5);
class ElectricalOverload(BossModule module) : Components.RaidwideCast(module, AID.ElectricalOverload);

class HeshualaStates : StateMachineBuilder
{
    public HeshualaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HigherPower>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<ElectricalOverload>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13157)]
public class Heshuala(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
