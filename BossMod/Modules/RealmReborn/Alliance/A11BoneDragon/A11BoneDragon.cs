#if DEBUG
namespace BossMod.RealmReborn.Alliance.A11BoneDragon;

public enum OID : uint
{
    Boss = 0x92B,
    _Gen_BoneDragon = 0x92C, // R0.500, x1
    _Gen_Platinal = 0x92D, // R1.000, x0 (spawn during fight)
    _Gen_RottingEye = 0x982, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_DarkWave = 736, // Boss->self, no cast, range 6+R circle
    _AutoAttack_Attack = 1461, // Boss/_Gen_Platinal->player, no cast, single-target
    _Weaponskill_DarkThorn = 745, // Boss->location, no cast, range 6 circle
    _Weaponskill_MiasmaBreath = 735, // Boss->self, no cast, range 8+R ?-degree cone
    _Weaponskill_HellSlash = 341, // _Gen_Platinal->player, no cast, single-target
    _Weaponskill_Apocalypse = 749, // _Gen_BoneDragon->location, 3.0s cast, range 6 circle
    _Spell_Stone = 970, // _Gen_RottingEye->player, 1.0s cast, single-target
    _Spell_EvilEye = 750, // Boss->self, 3.0s cast, range 100+R 120-degree cone
}

class A11BoneDragonStates : StateMachineBuilder
{
    public A11BoneDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Apocalypse>()
            .ActivateOnEnter<EvilEye>()
            .ActivateOnEnter<Platinal>()
            .ActivateOnEnter<Poison>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed;
    }
}

class Apocalypse(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Apocalypse, 6);
class EvilEye(BossModule module) : Components.StandardAOEs(module, AID._Spell_EvilEye, new AOEShapeCone(100, 60.Degrees()));

class Platinal(BossModule module) : Components.Adds(module, (uint)OID._Gen_Platinal, 1);

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

// inner circle radius = 8
[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 706)]
public class A11BoneDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-451.2f, 23.93f), new ArenaBoundsCircle(49.4f, MapResolution: 1));
#endif
