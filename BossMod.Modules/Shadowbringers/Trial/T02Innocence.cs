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
    SoulAndBody1 = 16049, // Helper->self, 3.0s cast, rotating wave; the pair is one ribbon
    SoulAndBody2 = 16050, // Helper->self, 3.0s cast, opposite half of the same wave
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
    BeatificVision = 16071, // BossP2->self, 5.0s cast, range 45 width 40 rect, line proximity from the rim across the arena
    ReprobationLong = 16075, // Helper->self, 1.5s cast, range 42 width 4 rect
    Shadowreaver = 16106, // BossP2->self, 5.0s cast, range 40 circle
    ExaltedPlumes = 16114, // Helper->self, no cast, range 40 circle
    SoulAndBodyInstant1 = 16121, // Helper->self, no cast, same wave advanced +15°
    SoulAndBodyInstant2 = 16122, // Helper->self, no cast, opposite half of that step
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

public enum TetherID : uint
{
    HolySword = 88, // ForgivenVenery2->ForgivenShame2
}

class Enthrall(BossModule module) : Components.CastGaze(module, AID.Enthrall)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var eye in ActiveEyes(slot, actor))
            hints.ForbiddenDirections.Add((Angle.FromDirection(eye.Position - actor.Position), 120.Degrees(), eye.Activation));
    }
}
class Realmrazer(BossModule module) : Components.RaidwideCastDelay(module, AID.Realmrazer, AID.RealmrazerAOE, 0.5f);
class DaybreakAOE(BossModule module) : Components.StandardAOEs(module, AID.DaybreakAOE, 6);
class ScoldsBridle(BossModule module) : Components.RaidwideCast(module, AID.ScoldsBridle);
class HolySword(BossModule module) : Components.SingleTargetCast(module, AID.HolySword);
class RighteousBolt(BossModule module) : Components.BaitAwayCast(module, AID.RighteousBolt, new AOEShapeCircle(3), centerAtTarget: true, endsOnCastEvent: true);
class SoulAndBody : Components.GenericRotatingAOE
{
    private static readonly AOEShapeCustom _shape = BuildShape();

    public SoulAndBody(BossModule module) : base(module)
    {
        FutureRisky = false;
    }

    private static AOEShapeCustom BuildShape()
    {
        var clipper = new PolygonClipper();
        var gap = clipper.Difference(new(CurveApprox.Circle(16f, 0.25f)), new(CurveApprox.Circle(11.5f, 0.25f)));
        return new(clipper.Difference(new(Ribbon()), new(gap)));
    }

    private static List<WDir> Ribbon()
    {
        const int n = 48;
        const float amp = 2f, period = 30f, halfWidth = 3f;
        var k = 2 * MathF.PI / period;
        var left = new List<WDir>(n + 1);
        var right = new List<WDir>(n + 1);
        for (var i = 0; i <= n; ++i)
        {
            var z = -22f + 44f * i / n;
            var x = amp * MathF.Sin(k * z);
            var dx = amp * k * MathF.Cos(k * z);
            var len = MathF.Sqrt(dx * dx + 1);
            var ox = halfWidth / len;
            var oz = halfWidth * -dx / len;
            left.Add(new(x + ox, z + oz));
            right.Add(new(x - ox, z - oz));
        }
        right.Reverse();
        left.AddRange(right);
        return left;
    }

    public override void Update()
    {
        while (Sequences.Count > 0 && Sequences[0].NumRemainingCasts > 0 && Sequences[0].NextActivation <= WorldState.CurrentTime)
            AdvanceSequence(0, WorldState.CurrentTime);
        if (Sequences.Count > 0 && Sequences[0].NumRemainingCasts <= 0)
            Sequences.Clear();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.SoulAndBody1)
            return;
        var rot = (caster.Position - Arena.Center).ToAngle();
        if (Sequences.Count > 0)
        {
            var cur = Sequences[0].Rotation;
            if (cur.AlmostEqual(rot, 0.1f) || cur.AlmostEqual(rot + 180.Degrees(), 0.1f))
                return;
            Sequences.Clear();
        }
        Sequences.Add(new(_shape, Arena.Center, rot, 8.Degrees(), Module.CastFinishAt(spell), 0.6f, 26, 2));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.NailOfCondemnation && actor.Position.AlmostEqual(Arena.Center, 1))
            Sequences.Clear();
    }
}
class HolyTrinity(BossModule module) : Components.StandardAOEs(module, AID.HolyTrinity, new AOEShapeRect(40, 2));
class ReprobationLine(BossModule module) : Components.StandardAOEs(module, AID.ReprobationLine, new AOEShapeRect(21, 2));
class ReprobationLong(BossModule module) : Components.StandardAOEs(module, AID.ReprobationLong, new AOEShapeRect(42, 2));
class ReprobationLongPredicted(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _shape = new(21, 2);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ReprobationLine)
            _aoes.Add(new(_shape, spell.LocXZ, spell.Rotation, WorldState.FutureTime(9.5f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ReprobationLong)
            _aoes.Clear();
    }
}
class GodRayCone(BossModule module) : Components.StandardAOEs(module, AID.GodRayCone, new AOEShapeCone(5, 50.Degrees()));
class GodRayDonut1(BossModule module) : Components.StandardAOEs(module, AID.GodRayDonut1, new AOEShapeDonutSector(5, 10, 50.Degrees()));
class GodRayDonut2(BossModule module) : Components.StandardAOEs(module, AID.GodRayDonut2, new AOEShapeDonutSector(10, 20, 50.Degrees()));
class BeatificVision : Components.StandardAOEs
{
    public BeatificVision(BossModule module) : base(module, AID.BeatificVision, new AOEShapeRect(45, 20), warningText: "Side edge — away from the line!")
    {
        Risky = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var caster in Casters)
        {
            if (caster.CastInfo == null)
                continue;
            var activation = Module.CastFinishAt(caster.CastInfo);
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), activation);
            var dir = caster.CastInfo.Rotation.ToDirection();
            var side = dir.OrthoL() * Arena.Bounds.Radius;
            var a = Arena.Center + side;
            var b = Arena.Center - side;
            var far = (actor.Position - a).LengthSq() <= (actor.Position - b).LengthSq() ? a : b;
            hints.GoalZones.Add(AIHints.GoalProximity(far, 6, 80));
            hints.AddForbiddenZone(new AOEShapeRect(45, 6), caster.Position, caster.CastInfo.Rotation, activation);
        }
    }
}
class Shadowreaver(BossModule module) : Components.RaidwideCast(module, AID.Shadowreaver);
class Manacle(BossModule module) : Components.StandardAOEs(module, AID.Manacle, 6);
class HolySwordAdd(BossModule module) : Components.SingleTargetCast(module, AID.HolySwordAdd, "Kill tethered Shame");
class DropOfLight(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(10), (uint)IconID.DropOfLight, AID.DropOfLight, 5.1f, centerAtTarget: true);
class LightPillar(BossModule module) : Components.SimpleLineStack(module, 3, 40, AID.LightPillarMarker, AID.LightPillar, 4.7f);
class InnocenceAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.ForgivenShame, (uint)OID.ForgivenVenery, (uint)OID.ForgivenShame2, (uint)OID.ForgivenVenery2, (uint)OID.NailOfCondemnation, (uint)OID.SwordOfCondemnation], 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var venery in Module.Enemies(OID.ForgivenVenery2))
        {
            if (venery.Tether.ID == (uint)TetherID.HolySword && WorldState.Actors.Find(venery.Tether.Target) is { } tethered)
                hints.SetPriority(tethered, 3);
            if (venery.CastInfo?.IsSpell(AID.HolySwordAdd) == true && WorldState.Actors.Find(venery.CastInfo.TargetID) is { } castTarget)
                hints.SetPriority(castTarget, 3);
        }
    }
}

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

static class T02InnocenceComponents
{
    public static StateMachineBuilder.Phase ActivateP1(StateMachineBuilder.Phase p) => p
        .ActivateOnEnter<Enthrall>()
        .ActivateOnEnter<Realmrazer>()
        .ActivateOnEnter<DaybreakAOE>()
        .ActivateOnEnter<ScoldsBridle>()
        .ActivateOnEnter<HolySword>()
        .ActivateOnEnter<SinSphere>()
        .ActivateOnEnter<ExaltedWing>()
        .ActivateOnEnter<ExaltedPlumes>()
        .ActivateOnEnter<InnocenceAdds>();

    public static StateMachineBuilder.Phase ActivateP2(StateMachineBuilder.Phase p) => p
        .ActivateOnEnter<RighteousBolt>()
        .ActivateOnEnter<SoulAndBody>()
        .ActivateOnEnter<HolyTrinity>()
        .ActivateOnEnter<ReprobationLine>()
        .ActivateOnEnter<ReprobationLong>()
        .ActivateOnEnter<ReprobationLongPredicted>()
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

class T02InnocenceStates : StateMachineBuilder
{
    private readonly T02Innocence _module;

    public T02InnocenceStates(T02Innocence module) : base(module)
    {
        _module = module;
        T02InnocenceComponents.ActivateP1(SimplePhase(0, id => SimpleState(id, 10000, "P2"), "P1"))
            .Raw.Update = () => _module.BossP2()?.IsTargetable == true || Module.PrimaryActor.IsDeadOrDestroyed;
    }
}

class T02InnocenceP2States : StateMachineBuilder
{
    public T02InnocenceP2States(T02InnocenceP2 module) : base(module)
    {
        T02InnocenceComponents.ActivateP2(TrivialPhase());
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 666, NameID = 8353)]
public class T02Innocence(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20))
{
    public Actor? BossP2() => Enemies(OID.BossP2).FirstOrDefault(a => !a.IsDestroyed);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(BossP2(), ArenaColor.Enemy);
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 666, NameID = 8353, PrimaryActorOID = (uint)OID.BossP2, SortOrder = 2)]
public class T02InnocenceP2(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
