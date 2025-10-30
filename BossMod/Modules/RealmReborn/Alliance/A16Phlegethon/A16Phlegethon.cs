namespace BossMod.RealmReborn.Alliance.A16Phlegethon;

public enum OID : uint
{
    Boss = 0x938, // R5.000, x1
    Helper = 0x939, // R0.500, x6 (spawn during fight)
    IronClaws = 0x93A, // R2.000, x0 (spawn during fight)
    IronGiant = 0x984, // R3.000, x0 (spawn during fight)
}

public enum AID : uint
{
    GreatDivide = 1725, // Boss->self, no cast, range 5+R width 8 rect, charge on random player
    AutoAttack = 1461, // Boss/IronGiant->player, no cast, single-target
    VacuumSlashCast = 1733, // Boss->self, 3.0s cast, single-target
    VacuumSlash = 1736, // Helper->self, 3.0s cast, range 80+R 52-degree cone
    DeathGrip = 610, // IronClaws->player, no cast, single-target
    AutoAttackClaw = 1459, // IronClaws->player, no cast, single-target
    MegiddoFlameCast = 1735, // Boss->self, 3.0s cast, single-target
    MegiddoFlame1 = 1741, // Helper->location, 3.0s cast, range 3 circle
    MegiddoFlame2 = 1742, // Helper->location, 3.0s cast, range 4 circle
    MegiddoFlame3 = 1743, // Helper->location, 3.0s cast, range 5 circle
    MegiddoFlame4 = 1744, // Helper->location, 3.0s cast, range 6 circle
    Quake = 1745, // Boss->self, no cast, raidwide
    AbyssalSlashCast = 1734, // Boss->self, 3.0s cast, single-target
    AbyssalSlashSmall = 1737, // Helper->self, 3.0s cast, range 2.2-7.5 180-degree cone
    AbyssalSlashLarge = 1739, // Helper->self, 3.0s cast, range 12.5-17.5 180-degree cone
    AncientFlareCast = 1730, // Boss->self, 7.0s cast, single-target
    AncientFlare = 1748, // Boss->self, no cast, ???
    GrandSword = 1785, // IronGiant->self, no cast, range 12+R 120-degree cone
}

class VacuumSlash(BossModule module) : Components.StandardAOEs(module, AID.VacuumSlash, new AOEShapeCone(80, 26.Degrees()));

class MegiddoFlame1(BossModule module) : Components.StandardAOEs(module, AID.MegiddoFlame1, 3);
class MegiddoFlame2(BossModule module) : Components.StandardAOEs(module, AID.MegiddoFlame2, 4);
class MegiddoFlame3(BossModule module) : Components.StandardAOEs(module, AID.MegiddoFlame3, 5);
class MegiddoFlame4(BossModule module) : Components.StandardAOEs(module, AID.MegiddoFlame4, 6);

class AbyssalSlash1(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSlashSmall, new AOEShapeDonutSector(2.2f, 7.5f, 90.Degrees()));
class AbyssalSlash2(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSlashLarge, new AOEShapeDonutSector(12.5f, 17.5f, 90.Degrees()));

class Pads(BossModule module) : Components.GenericTowers(module)
{
    private readonly WPos[] _pads = [new(-148.65f, 191.975f), new(-110, 221.59f), new(-71.35f, 191.975f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AncientFlareCast)
        {
            foreach (var p in _pads)
                Towers.Add(new(p, 4.2f, minSoakers: 6, maxSoakers: 8, activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AncientFlare)
            Towers.Clear();
    }
}

class AncientFlareVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeCircle(32.45f), Arena.Center, Activation: _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AncientFlareCast)
        {
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1E8885 && state == 0x00110022)
            _activation = default;
    }
}

class DynamicArenaBorder(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            yield return new(new AOEShapeDonut(32.45f, 100), Arena.Center);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1E8894)
        {
            if (state == 0x00040008)
                _active = true;
            if (state == 0x00010002)
                _active = false;
        }
    }
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.IronGiant, OID.IronClaws]);

class A16PhlegethonStates : StateMachineBuilder
{
    public A16PhlegethonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VacuumSlash>()
            .ActivateOnEnter<DynamicArenaBorder>()
            .ActivateOnEnter<AncientFlareVoidzone>()
            .ActivateOnEnter<MegiddoFlame1>()
            .ActivateOnEnter<MegiddoFlame2>()
            .ActivateOnEnter<MegiddoFlame3>()
            .ActivateOnEnter<MegiddoFlame4>()
            .ActivateOnEnter<AbyssalSlash1>()
            .ActivateOnEnter<AbyssalSlash2>()
            .ActivateOnEnter<Pads>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 732)]
public class A16Phlegethon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-110, 181.6f), PhlegBounds)
{
    private static readonly ArenaBoundsCustom PhlegBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds()
    {
        var circle = CurveApprox.Circle(32.45f, 1 / 75f);
        var cone = CurveApprox.CircleSector(44.5f, 97.5f.Degrees(), -97.5f.Degrees(), 1 / 75f);

        return new(44.5f, new PolygonClipper().Union(new(circle), new(cone)), MapResolution: 1);
    }
}
