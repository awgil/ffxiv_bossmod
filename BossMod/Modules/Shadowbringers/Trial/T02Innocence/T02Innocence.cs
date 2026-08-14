namespace BossMod.Shadowbringers.Trial.T02Innocence;

public enum OID : uint
{
    Helper = 0x233C,
    BossP2 = 0x28FE, // R4.400, x? : Phase 2 boss.
    Innocence = 0x28FB, // R2.600, x?  : Phase 1 boss.
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type
    Exit = 0x1E850B, // R0.500, x?, EventObj type
    ForgivenShame = 0x28FC, // R0.960-1.200, x?
    ForgivenVenery = 0x28FD, // R1.500-1.875, x?
    MeteorTower = 0x1EAD40, // R0.500, x?, EventObj type
    NailOfCondemnation = 0x2900, // R0.500, x?
    ThornOfCondemnation = 0x2901, // R1.000, x?
    ForgivenShame1 = 0x2BEE, // R0.960, x?
    ForgivenVenery1 = 0x2BEF, // R1.500-1.875, x?
    Innocence3 = 0x2BED, // R2.800, x? : Shows up once behind the scenes TODO
    SwordOfCondemnation = 0x2902, // R0.000, x?
}

public enum AID : uint
{
    _AutoAttack_ = 16016, // Innocence->player, no cast, single-target
    Realmrazer = 16026, // Innocence->self, 5.0s cast, single-target
    Realmrazer1 = 16027, // Helper->location, no cast, range 40 circle
    HeavenlyHost = 16021, // Innocence->self, 3.0s cast, single-target
    _AutoAttack_Attack = 870, // ForgivenShame/ForgivenVenery/ForgivenVenery1->player, no cast, single-target
    ScoldsBridle = 16030, // ForgivenShame->self, 6.0s cast, range 40 circle
    HolySword = 16031, // ForgivenVenery->player, 5.0s cast, single-target
    HolySword1 = 17175, // ForgivenVenery->player, no cast, single-target
    Daybreak = 16029, // Helper->location, 3.5s cast, range 6 circle
    Daybreak1 = 16028, // Innocence->self, 3.5s cast, single-target
    Enthrall = 16025, // Innocence->self, 4.0s cast, range 40 circle : Gaze
    Sinsphere = 16023, // Helper->self, no cast, range 5 circle
    _Ability_ = 16017, // Innocence->location, no cast, single-target
    GuidingLight = 16022, // Innocence->self, 3.0s cast, single-target
    GuidingLight1 = 16197, // Helper->ForgivenShame/ForgivenVenery, no cast, single-target
    _Weaponskill_ = 16018, // Innocence->self, no cast, single-target
    ExaltedWing = 16019, // Helper->self, no cast, range 40 circle
    _Weaponskill_1 = 16708, // Innocence->self, no cast, single-target
    ExaltedPlumes = 16114, // Helper->self, no cast, range 40 circle
    _Ability_1 = 16033, // Innocence1->self, no cast, single-target
    _Ability_2 = 16020, // Innocence->self, no cast, single-target
    _AutoAttack_1 = 16032, // Innocence1->player, no cast, single-target
    RighteousBolt = 16035, // Innocence1->player, 5.0s cast, single-target
    WingedReprobation = 16572, // Innocence1->self, 3.0s cast, single-target
    HolyTrinity = 16051, // Helper->self, 3.0s cast, range 40 width 4 rect
    NailSpawn = 16041, // NailOfCondemnation->self, no cast, single-target : Cast when nails appear in arena
    SoulAndBody = 16049, // Helper->self, 3.0s cast, range ?-20 donut
    SoulAndBody1 = 16050, // Helper->self, 3.0s cast, range ?-20 donut
    SoulAndBody2 = 16121, // Helper->self, no cast, range ?-20 donut
    SoulAndBody3 = 16122, // Helper->self, no cast, range ?-20 donut
    _Ability_4 = 16034, // Innocence1->location, no cast, single-target
    Reprobation = 16054, // ThornOfCondemnation->location, 3.0s cast, single-target
    Reprobation1 = 16056, // Helper->self, 3.0s cast, range 21 width 4 rect
    Reprobation2 = 16055, // ThornOfCondemnation->location, 1.5s cast, single-target
    Reprobation3 = 16075, // Helper->self, 1.5s cast, range 42 width 4 rect
    RightfulReprobation = 16053, // Innocence1->self, 3.0s cast, single-target
    Shadowreaver = 16106, // Innocence1->self, 5.0s cast, range 40 circle
    _Weaponskill_2 = 17072, // Innocence1->self, no cast, single-target
    _Weaponskill_3 = 17073, // SwordOfCondemnation->self, no cast, single-target
    Manacle = 18064, // ForgivenShame1->location, 3.5s cast, range 6 circle
    HolySword2 = 18065, // ForgivenVenery1->ForgivenShame1, 9.0s cast, single-target
    GuiltyVerdict = 18066, // ForgivenVenery1->self, no cast, range 50 circle
    FlamingSword = 16065, // SwordOfCondemnation->self, no cast, range 40 circle
    FlamingSword1 = 18184, // Helper->self, no cast, range 40 circle
    GodRayCone = 16062, // Helper->self, 4.5s cast, range 5 100.000-degree cone
    GodRay1 = 16060, // Innocence1->self, 4.5s cast, single-target
    GodRayMedium = 16063, // Helper->self, 3.5s cast, range 5-10 donut sector, 100 degree
    GodRayOuter = 16064, // Helper->self, 3.5s cast, range 10-20 donut sector, 100 degree
    GodRay4 = 16061, // Innocence1->self, no cast, single-target
    LightPillarMarker = 14588, // Helper->player, no cast, single-target
    LightPillar = 16190, // Innocence1->self, 5.0s cast, single-target
    LightPillar1 = 16070, // Innocence1->self, no cast, range 40 width 6 rect
    BeatificVision = 16071, // Innocence1->self, 5.0s cast, range 45 width 40 rect
    DropOfLight = 16068, // Innocence1->self, no cast, single-target
    DropOfLight1 = 16069, // Helper->player, no cast, range 10 circle
}

public enum IconID : uint
{
    TankbusterIcon = 218, // player->self
    DropOfLightTarget = 138, // player->self

}

public enum TetherID : uint
{
    HolySwordTether = 88, // ForgivenVenery1->ForgivenShame1
}

sealed class CrownTrash(BossModule module) : Components.AddsMulti(module, T02Innocence.CrownMobs);

sealed class RealmRazer(BossModule module) : Components.RaidwideCast(module, (uint)AID.Realmrazer1);

sealed class RighteousBolt(BossModule module) : Components.BaitAwayCast(module, (uint)AID.RighteousBolt,
    new AOEShapeCircle(3f), centerAtTarget: true, tankbuster: true);

sealed class ScoldsBridle(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScoldsBridle);

sealed class DayBreak(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Daybreak, new AOEShapeCircle(6f));

// Causes confusion if you are hit by enthrall.
sealed class Enthrall(BossModule module) : Components.CastGaze(module, (uint)AID.Enthrall);

// create towers for the meteor; min 1 max 2 for four towers
sealed class SinSphere(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        // initial meteor towers.
        if (actor.OID == (uint)OID.MeteorTower)
        {
            Towers.Add(new(actor.Position, 5f, 1, 2));
        }
    }

    // A fallback tower removal in case all the towers were soaked
    public override void OnActorDestroyed(Actor actor)
    {
        base.OnActorDestroyed(actor);
        if (actor.OID == (uint)OID.MeteorTower)
        {
            Towers.Clear();
        }
    }
}

// Does pulsing raidwides to finish out phase 1
sealed class ExaltedRaidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ExaltedPlumes, (uint)AID.ExaltedWing]);

sealed class HolyTrinity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HolyTrinity, new AOEShapeRect(40f, 2f));

sealed class SoulAndBodyLongCast(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SoulAndBody, (uint)AID.SoulAndBody1], new AOEShapeDonutSector(15f, 20f, 20f.Degrees()));

/*
 * This is the wavy aoe that rotates across the arena.
 * Seems like the first round is 8 casts of instant and second round is 12 casts.
 */
sealed class SoulAndBody(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SoulAndBody, new AOEShapeDonutSector(15f, 20f, 20f.Degrees()), maxCasts: 8)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SoulAndBody or (uint)AID.SoulAndBody1)
            BespokeAOE(caster, spell);
    }

    public void BespokeAOE(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SoulAndBody or (uint)AID.SoulAndBody1)
        {
            var rotation = spell.Rotation;
            /*
             * spin up aoe's for the rotating aoe.  _genAOE is a magic number because we do not have a good way to predict
             * how many will be cast. Instead we make extra and then clear the Casters queue when the Thorn actor
             * that is in the Arena.Center position dies.
             */
            var _genAOE = 26;

            for (var i = 0; i < _genAOE; i++)
            {
                var rotAngFloat = 8f * i;
                WPos futureOrigin = WPos.RotateAroundOrigin(-rotAngFloat, Arena.Center, spell.LocXZ);
                Angle futureRot = rotation + rotAngFloat.Degrees();
                // The first long cast takes approx 2.7 seconds. The instants that follow every 0.6 seconds or so.
                DateTime _activation = WorldState.FutureTime(2.7f + (0.6f * i));

                Casters.Add(new(Shape, futureOrigin.Quantized(), futureRot, _activation,
                    actorID: caster.InstanceID,
                    shapeDistance: Shape.Distance(futureOrigin.Quantized(), futureRot)));
            }
            SortHelpers.SortAOEByActivation(Casters);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((spell.Action.ID is (uint)AID.SoulAndBody or (uint)AID.SoulAndBody1) && Casters.Count != 0)
            Casters.RemoveAll(cast => cast.Activation <= WorldState.CurrentTime && cast.Shape is AOEShapeDonutSector);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SoulAndBody2 && Casters.Count != 0)
        {
            Casters.RemoveAll(cast => cast.Activation <= WorldState.CurrentTime && cast.Shape is AOEShapeDonutSector);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.NailOfCondemnation && actor.Position == Arena.Center)
        {
            Casters.Clear();
        }
    }
}

/*
 * Rightful Reprobation: Innocence moves to the center of the arena and sends out eight swords as Line AoEs.
 * After the attack, the swords will linger for a few seconds before returning to Innocence as Line AoEs.
 * Getting hit by either part of the attack inflicts Physical Vulnerability Up.
 */
sealed class Reprobation(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Reprobation1, new AOEShapeRect(21f, 2f));
sealed class ReprobationLong(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Reprobation3, new AOEShapeRect(42f, 2));

sealed class Shadowreaver(BossModule module) : Components.RaidwideCast(module, (uint)AID.Shadowreaver);

sealed class Manacle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Manacle, new AOEShapeCircle(6f));

sealed class PhaseTwoRaidwides(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GuiltyVerdict, (uint)AID.FlamingSword);

sealed class GodRayCone(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GodRayCone, new AOEShapeCone(5f, 50f.Degrees()));
sealed class GodRayMedium(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GodRayMedium, new AOEShapeDonutSector(5f, 10f, 50f.Degrees()));
sealed class GodRayOuter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GodRayOuter, new AOEShapeDonutSector(10f, 20f, 50f.Degrees()));

// stack rectangle
sealed class LightPillar(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.LightPillarMarker, aidResolve: (uint)AID.LightPillar1, 4.7d, 40f, 3f, 8, 8);

// Giant proximity aoe. Safe zone is an estimate.
sealed class BeatificVision(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.BeatificVision, new AOEShapeRect(45f, 15f));

sealed class DropOfLight(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(10f), (uint)IconID.DropOfLightTarget, (uint)AID.DropOfLight1, centerAtTarget: true);

[SkipLocalsInit]
sealed class InnocenceStates : StateMachineBuilder
{
    public InnocenceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrownTrash>()
            .ActivateOnEnter<RealmRazer>()
            .ActivateOnEnter<RighteousBolt>()
            .ActivateOnEnter<ScoldsBridle>()
            .ActivateOnEnter<DayBreak>()
            .ActivateOnEnter<Enthrall>()
            .ActivateOnEnter<SinSphere>()
            .ActivateOnEnter<ExaltedRaidwides>()
            // Phase 2
            .ActivateOnEnter<HolyTrinity>()
            .ActivateOnEnter<SoulAndBodyLongCast>()
            .ActivateOnEnter<SoulAndBody>()
            .ActivateOnEnter<Reprobation>()
            .ActivateOnEnter<ReprobationLong>()
            .ActivateOnEnter<Shadowreaver>()
            .ActivateOnEnter<Manacle>()
            .ActivateOnEnter<PhaseTwoRaidwides>()
            .ActivateOnEnter<GodRayCone>()
            .ActivateOnEnter<GodRayMedium>()
            .ActivateOnEnter<GodRayOuter>()
            .ActivateOnEnter<LightPillar>()
            .ActivateOnEnter<BeatificVision>()
            .ActivateOnEnter<DropOfLight>()
            // The first phase of the boss dies so we must make sure that the module stays active while BossP2 exists.
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies((uint)OID.BossP2).All(k => k.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(InnocenceStates),
    ConfigType = null, // replace null with typeof(InnocenceConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = typeof(TetherID),
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Innocence,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Shadowbringers,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 666u,
    NameID = 8353u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]

/*
 * The first stage of innocence is the fat innocence.  Phase 1 ends when that model
 * dies and is replaced by the muscular innocence (OID.BossP2)
 *
 * We account for that by pointing the module at _bossP2 when he comes up.
 * We also have a condition in the state machine that both primary actor and BossP2
 * actor are dead or destroyed before the fight is over.
 */
public sealed class T02Innocence(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    private Actor? _bossP2;

    // draw the adds and BossP2 as they come up.
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies((uint)OID.BossP2), Colors.Enemy);
        Arena.Actors(WorldState.Actors.Where(a => !a.IsAlly), Colors.Enemy);
    }

    // Update the module to stay active after initial primary actor 'dies'.
    protected override void UpdateModule()
    {
        _bossP2 ??= GetActor((uint)OID.BossP2);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }

    public static readonly uint[] CrownMobs =
    [
        (uint)OID.ForgivenShame,
        (uint)OID.ForgivenVenery,
        (uint)OID.NailOfCondemnation,
        (uint)OID.ForgivenShame1,
        (uint)OID.ForgivenVenery1,
        (uint)OID.SwordOfCondemnation
    ];
}
