namespace BossMod.Shadowbringers.Trial.T02Innocence;

public enum OID : uint
{
    Boss = 0x28FB, // R2.600, Innocence P1
    Helper = 0x233C, // R0.500
    ForgivenShame = 0x28FC, // R0.960
    ForgivenVenery = 0x28FD, // R1.500
    BossP2 = 0x28FE, // R4.400, Innocence P2
    NailOfCondemnation = 0x2900, // R0.500
    ThornOfCondemnation = 0x2901, // R1.000
    SwordOfCondemnation = 0x2902, // R0.000
    InnocenceAdd = 0x2BED, // R2.800
    ForgivenShame2 = 0x2BEE, // R0.960
    ForgivenVenery2 = 0x2BEF, // R1.500
    MeteorTower = 0x1EAD40, // R0.500, EventObj
}

public enum AID : uint
{
    AutoAttack = 870, // ForgivenShame/Venery->player, no cast, single-target
    LightPillarMarker = 14588, // Helper->player, no cast, single-target
    AutoAttackBoss = 16016, // Boss->player, no cast, single-target
    ExaltedWing = 16019, // Helper->self, no cast, range 40 circle
    HeavenlyHost = 16021, // Boss->self, 3.0s cast, single-target
    GuidingLight = 16022, // Boss->self, 3.0s cast, single-target
    Sinsphere = 16023, // Helper->self, no cast, range 5 circle
    Sinburst = 16024, // Helper->self, no cast, range 40 circle
    Enthrall = 16025, // Boss->self, 4.0s cast, range 40 circle
    Realmrazer = 16026, // Boss->self, 5.0s cast, single-target
    RealmrazerAOE = 16027, // Helper->self, no cast, range 40 circle
    Daybreak = 16028, // Boss->self, 3.5s cast, single-target
    DaybreakAOE = 16029, // Helper->location, 3.5s cast, range 6 circle
    ScoldsBridle = 16030, // ForgivenShame->self, 6.0s cast, range 40 circle
    HolySword = 16031, // ForgivenVenery->player, 5.0s cast, single-target, tankbuster
    AutoAttackP2 = 16032, // BossP2->player, no cast, single-target
    RighteousBolt = 16035, // BossP2->player, 5.0s cast, single-target, tankbuster
    NailExplosion = 16041, // NailOfCondemnation->self, no cast
    SoulAndBody1 = 16049, // Helper->self, 3.0s cast, range 5-20 donut
    SoulAndBody2 = 16050, // Helper->self, 3.0s cast, range 5-20 donut
    HolyTrinity = 16051, // Helper->self, 3.0s cast, range 40 width 4 rect
    RightfulReprobation = 16053, // BossP2->self, 3.0s cast, single-target
    Reprobation = 16054, // ThornOfCondemnation->self, 3.0s cast, single-target
    ReprobationShort = 16055, // ThornOfCondemnation->self, 1.5s cast, single-target
    ReprobationLine = 16056, // Helper->self, 3.0s cast, range 21 width 4 rect
    GodRay = 16060, // BossP2->self, 4.5s cast, single-target
    GodRayRepeat = 16061, // BossP2->self, no cast
    GodRayCone = 16062, // Helper->self, 4.5s cast, range 5 100-degree cone
    GodRayDonut1 = 16063, // Helper->self, 3.5s cast, range 5-10 donut
    GodRayDonut2 = 16064, // Helper->self, 3.5s cast, range 10-20 donut
    FlamingSword = 16065, // SwordOfCondemnation->self, no cast, range 40 circle
    DropOfLightVisual = 16068, // BossP2->self, no cast, single-target
    DropOfLight = 16069, // Helper->self, no cast, range 10 circle
    LightPillar = 16070, // BossP2->self, no cast, range 40 width 6 rect
    BeatificVision = 16071, // BossP2->self, 5.0s cast, range 45 width 40 rect
    ReprobationLong = 16075, // Helper->self, 1.5s cast, range 42 width 4 rect
    Shadowreaver = 16106, // BossP2->self, 5.0s cast, range 40 circle
    ExaltedPlumes = 16114, // Helper->self, no cast, range 40 circle
    SoulAndBodyInstant1 = 16121, // Helper->self, no cast, range 5-20 donut
    SoulAndBodyInstant2 = 16122, // Helper->self, no cast, range 5-20 donut
    WingedReprobation = 16572, // BossP2->self, 3.0s cast, single-target
    HolySwordInstant = 17175, // ForgivenVenery->player, no cast, single-target
    Manacle = 18064, // ForgivenShame2->location, 3.5s cast, range 6 circle
    HolySwordAdd = 18065, // ForgivenVenery2->ForgivenShame2, 9.0s cast, single-target
    GuiltyVerdict = 18066, // ForgivenVenery2->self, no cast, range 50 circle
    LightPillarCast = 16190, // BossP2->self, 5.0s cast, single-target
}

public enum IconID : uint
{
    DropOfLight = 138, // player
}

class Enthrall(BossModule module) : Components.CastGaze(module, AID.Enthrall);
class Realmrazer(BossModule module) : Components.RaidwideCastDelay(module, AID.Realmrazer, AID.RealmrazerAOE, 0.5f);
class DaybreakAOE(BossModule module) : Components.StandardAOEs(module, AID.DaybreakAOE, 6);
class ScoldsBridle(BossModule module) : Components.RaidwideCast(module, AID.ScoldsBridle);
class HolySword(BossModule module) : Components.SingleTargetCast(module, AID.HolySword);
class RighteousBolt(BossModule module) : Components.BaitAwayCast(module, AID.RighteousBolt, new AOEShapeCircle(3), centerAtTarget: true, endsOnCastEvent: true);
class SoulAndBody(BossModule module) : Components.GroupedAOEs(module, [AID.SoulAndBody1, AID.SoulAndBody2], new AOEShapeDonut(5, 20));
class HolyTrinity(BossModule module) : Components.StandardAOEs(module, AID.HolyTrinity, new AOEShapeRect(40, 2));
class ReprobationLine(BossModule module) : Components.StandardAOEs(module, AID.ReprobationLine, new AOEShapeRect(21, 2));
class ReprobationLong(BossModule module) : Components.StandardAOEs(module, AID.ReprobationLong, new AOEShapeRect(42, 2));
class GodRayCone(BossModule module) : Components.StandardAOEs(module, AID.GodRayCone, new AOEShapeCone(5, 50.Degrees()));
class GodRayDonut1(BossModule module) : Components.StandardAOEs(module, AID.GodRayDonut1, new AOEShapeDonutSector(5, 10, 50.Degrees()));
class GodRayDonut2(BossModule module) : Components.StandardAOEs(module, AID.GodRayDonut2, new AOEShapeDonutSector(10, 20, 50.Degrees()));
class BeatificVision(BossModule module) : Components.StandardAOEs(module, AID.BeatificVision, new AOEShapeRect(45, 15));
class Shadowreaver(BossModule module) : Components.RaidwideCast(module, AID.Shadowreaver);
class Manacle(BossModule module) : Components.StandardAOEs(module, AID.Manacle, 6);
class HolySwordAdd(BossModule module) : Components.SingleTargetCast(module, AID.HolySwordAdd, "Interrupt add");
class DropOfLight(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(10), (uint)IconID.DropOfLight, AID.DropOfLight, 5.1f, centerAtTarget: true);
class LightPillar(BossModule module) : Components.SimpleLineStack(module, 3, 40, AID.LightPillarMarker, AID.LightPillar, 4.7f);
class InnocenceAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.ForgivenShame, (uint)OID.ForgivenVenery, (uint)OID.ForgivenShame2, (uint)OID.ForgivenVenery2, (uint)OID.NailOfCondemnation, (uint)OID.SwordOfCondemnation], 1);

class SinSphere(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.MeteorTower)
            Towers.Add(new(actor.Position, 5, 1, 2));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        base.OnActorDestroyed(actor);
        if (actor.OID == (uint)OID.MeteorTower)
            Towers.Clear();
    }
}

class InstantRaidwide(BossModule module, AID aid) : Components.RaidwideInstant(module, aid, 0.1f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Activation = WorldState.FutureTime(Delay);
        base.OnEventCast(caster, spell);
    }
}
class ExaltedWing(BossModule module) : InstantRaidwide(module, AID.ExaltedWing);
class ExaltedPlumes(BossModule module) : InstantRaidwide(module, AID.ExaltedPlumes);
class GuiltyVerdict(BossModule module) : InstantRaidwide(module, AID.GuiltyVerdict);
class FlamingSwordRaidwide(BossModule module) : InstantRaidwide(module, AID.FlamingSword);

class T02InnocenceStates : StateMachineBuilder
{
    private readonly T02Innocence _module;

    public T02InnocenceStates(T02Innocence module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => _module.BossP2() != null || Module.PrimaryActor.IsDeadOrDestroyed;
        SimplePhase(1, Phase2, "P2")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDead ?? false);
    }

    private void Phase1(uint id)
    {
        SimpleState(id, 10000, "P2")
            .ActivateOnEnter<Enthrall>()
            .ActivateOnEnter<Realmrazer>()
            .ActivateOnEnter<DaybreakAOE>()
            .ActivateOnEnter<ScoldsBridle>()
            .ActivateOnEnter<HolySword>()
            .ActivateOnEnter<SinSphere>()
            .ActivateOnEnter<ExaltedWing>()
            .ActivateOnEnter<ExaltedPlumes>()
            .ActivateOnEnter<InnocenceAdds>();
    }

    private void Phase2(uint id)
    {
        SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<RighteousBolt>()
            .ActivateOnEnter<SoulAndBody>()
            .ActivateOnEnter<HolyTrinity>()
            .ActivateOnEnter<ReprobationLine>()
            .ActivateOnEnter<ReprobationLong>()
            .ActivateOnEnter<GodRayCone>()
            .ActivateOnEnter<GodRayDonut1>()
            .ActivateOnEnter<GodRayDonut2>()
            .ActivateOnEnter<BeatificVision>()
            .ActivateOnEnter<Shadowreaver>()
            .ActivateOnEnter<Manacle>()
            .ActivateOnEnter<HolySwordAdd>()
            .ActivateOnEnter<GuiltyVerdict>()
            .ActivateOnEnter<FlamingSwordRaidwide>()
            .ActivateOnEnter<DropOfLight>()
            .ActivateOnEnter<LightPillar>()
            .ActivateOnEnter<InnocenceAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 666, NameID = 8353)] // TODO: clear Incomplete after P1/P2 full replay verify
public class T02Innocence(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20))
{
    public Actor? BossP2() => Enemies(OID.BossP2).FirstOrDefault(a => !a.IsDestroyed);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(BossP2(), ArenaColor.Enemy);
    }
}
