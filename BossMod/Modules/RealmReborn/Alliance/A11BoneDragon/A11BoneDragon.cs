namespace BossMod.RealmReborn.Alliance.A11BoneDragon;

public enum OID : uint
{
    Boss = 0x92B, // R5.000, x1
    Helper = 0x92C, // R0.500, x1
    Platinal = 0x92D, // R1.000, x0 (spawn during fight)
    RottingEye = 0x982, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    DarkWave = 736, // Boss->self, no cast, range 6+R circle
    AutoAttack = 1461, // Boss/_Gen_Platinal->player, no cast, single-target
    DarkThorn = 745, // Boss->location, no cast, range 6 circle
    MiasmaBreath = 735, // Boss->self, no cast, range 8+R ?-degree cone
    HellSlash = 341, // _Gen_Platinal->player, no cast, single-target
    Apocalypse = 749, // _Gen_BoneDragon->location, 3.0s cast, range 6 circle
    Stone = 970, // _Gen_RottingEye->player, 1.0s cast, single-target
    EvilEye = 750, // Boss->self, 3.0s cast, range 100+R 120-degree cone
}

class MiasmaBreath(BossModule module) : Components.Cleave(module, AID.MiasmaBreath, new AOEShapeCone(13, 45.Degrees()), activeWhileCasting: false);
class Apocalypse(BossModule module) : Components.StandardAOEs(module, AID.Apocalypse, 6);
class EvilEye(BossModule module) : Components.StandardAOEs(module, AID.EvilEye, new AOEShapeCone(105, 60.Degrees()));

class Platinal(BossModule module) : Components.Adds(module, (uint)OID.Platinal, 1);

class Poison : Components.GenericAOEs
{
    private readonly AOEShapeCustom _poisonShape;

    private bool _active;

    public Poison(BossModule module) : base(module)
    {
        var circle = CurveApprox.Circle(49.4f, 1 / 90f);
        var clipper = new PolygonClipper();
        var op1 = new PolygonClipper.Operand(circle);

        var op2 = clipper.Difference(op1, new(CurveApprox.Circle(8, 1 / 90f)));

        var laneOffset = new WDir(0, 31.87f);
        for (var i = 0; i < 8; i++)
        {
            var deg = (45 * i).Degrees();
            var rect = CurveApprox.Rect(laneOffset.Rotate(deg), deg.ToDirection().OrthoR() * 3.9f, deg.ToDirection() * 17.675f);
            op2 = clipper.Difference(new(op2), new(rect));
        }

        _poisonShape = new(op2);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            yield return new(_poisonShape, Arena.Center, default, DateTime.MaxValue);
    }

    public override void OnLegacyMapEffect(byte seq, byte param, byte[] data)
    {
        if (data[0] == 3 && data[2] == 0x10)
            _active = true;

        if (data[0] == 3 && data[2] == 0x80)
            _active = false;
    }
}
class BossDeathTracker(BossModule module) : BossComponent(module)
{
    public bool Dead { get; private set; }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000001 && param1 == 0xB4)
            Dead = true;
    }
}

class A11BoneDragonStates : StateMachineBuilder
{
    public A11BoneDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Apocalypse>()
            .ActivateOnEnter<BossDeathTracker>()
            .ActivateOnEnter<MiasmaBreath>()
            .ActivateOnEnter<EvilEye>()
            .ActivateOnEnter<Platinal>()
            .ActivateOnEnter<Poison>()
            .Raw.Update = () => Module.FindComponent<BossDeathTracker>()!.Dead || Module.PrimaryActor.IsDestroyed;
    }
}

// inner circle radius = 8
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 706, DevOnly = true)]
public class A11BoneDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-451.2f, 23.93f), new ArenaBoundsCircle(49.4f, MapResolution: 1));
