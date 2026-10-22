namespace BossMod.Dawntrail.Trial.T08Enuo;

public enum OID : uint
{
    Boss = 0x4DB9, // R?, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    BossVoidTether = 0x4DB8, // R5.000, x?
    YawningVoid = 0x4DBA, // R?, x0 (spawn during fight)
    NaughtHuntChaser = 0x4EB4, // R0.850, x0 (spawn during fight), chasing aoe
    VoidVacuum = 0x4DBB, // R0.850, x0 (spawn during fight)
    BeaconInTheDark = 0x4DBE, // R?, x0 (spawn during fight)
    UncastShadow = 0x4DBD, // R5.000, x0 (spawn during fight)
    LoomingShadow = 0x4DBC, // R12.500, x0 (spawn during fight)
    Zero = 0x4DBF, // R?, x0 (spawn during fight)
    Golbez = 0x4DC0, // R?, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 49936, // Boss->player, no cast, single-target
    Meteorain = 49971, // Boss->self, no cast, range 40 circle, raidwide
    Jump = 49927, // Boss->location, no cast, single-target
    NaughtGrowsCastVisual = 49931, // Boss->self, no cast, single-target, visual
    NaughtGrowsAOE = 49933, // YawningVoid->self, no cast, range 40 circle
    NaughtWakes = 49929, // Boss->self, no cast, single-target
    GazeOfTheVoidVisual = 49950, // Boss->self, 7.0+1.0s cast, single-target, visual
    GazeOfTheVoid1 = 49952, // Helper->self, 8.0s cast, single-target, visual
    GazeOfTheVoidCones = 49953, // Helper->self, 8.0s cast, range 40 45-degree cone
    GazeOfTheVoid2 = 49951, // Helper->self, 8.0s cast, single-target, visual
    DeepFreezeCast = 49965, // Boss->self, 5.0+1.0s cast, single-target, visual (flare bait)
    DeepFreeze = 49966, // Helper->players, 6.0s cast, range 8 circle
    NaughtHunts = 49939, // Boss->self, 6.0+1.0s cast, single-target, visual (chase)
    EndlessChaseFirst = 48474, // Void->self, 6.0s cast, range 6 circle
    EndlessChaseRest = 49940, // Void->location, no cast, range 6 circle
    NaughtHuntsAnother = 49941, // Boss->self, 5.0+1.0s cast, single-target, visual
    ShroudedHolyVisual = 49967, // Boss->self, 4.0+1.0s cast, single-target, visual (stack)
    ShroudedHolyStack = 49968, // Helper->players, 5.0s cast, range 6 circle
    MeltdownVisual = 49962, // Boss->self, 4.0+1.0s cast, single-target, visual
    MeltdownAOE = 49963, // Helper->location, 5.0s cast, range 5 circle
    MeltdownSpread = 49964, // Helper->players, 5.0s cast, range 5 circle
    VacuumVisual = 49942, // Boss->self, 2.0+1.0s cast, single-target, visual
    SilentTorrentVisual1 = 49944, // VoidVacuum->location, 3.5s cast, single-target, visual
    SilentTorrentVisual2 = 49943, // VoidVacuum->location, 3.5s cast, single-target, visual
    SilentTorrentVisual3 = 49945, // VoidVacuum->location, 3.5s cast, single-target, visual
    SilentTorrentMedium = 49947, // Helper->self, 4.0s cast, range 17-19 donut sector
    SilentTorrentLarge = 49948, // Helper->self, 4.0s cast, range 17-19 donut sector
    SilentTorrentSmall = 49946, // Helper->self, 4.0s cast, range 17-19 donut sector
    VacuumCircle = 49949, // VoidVacuum->self, 1.5s cast, range 7 circle
    AllForNaughtVisual = 49954, // Boss->self, 5.0s cast, single-target, visual
    LoomingEmptinessVisual = 49955, // LoomingShadow->self, 6.0s cast, single-target, visual
    LoomingEmptinessAOE = 49981, // Helper->self, 7.0s cast, range 100 circle
    EmptyShadowVisual = 49956, // UncastShadow->self, 7.0s cast, single-target, visual
    EmptyShadowAOE = 50667, // Helper->self, 8.0s cast, range 10 circle
    Nothingness = 49958, // UncastShadow->self, 3.0s cast, range 100 width 4 rect
    LightlessWorld = 49959, // Boss->self, 5.0s cast, single-target, visual (LOS behind Zero)
    LightlessWorld1 = 49960, // Helper->self, no cast, range 40 circle
    LightlessWorldAOE = 49961, // Helper->self, no cast, range 40 circle
    NaughtGrows = 49932, // Boss->self, 6.0+1.0s cast, single-target, visual
    NaughtGrowsAOESmall = 49934, // Helper->self, 7.0s cast, range 12 circle
    Almagest = 49928, // Boss->self, 5.0s cast, range 40 circle, raidwide
    DimensionZeroRect = 49969, // Boss->self, 5.0s cast, single-target, visual (line stack)
    DimensionZeroMarker = 49970, // Boss->self, no cast, range 60 width 8 rect
}

public enum IconID : uint
{
    DeepFreezeFlare = 327, // player
    EndlessChase = 172, // player
    ShroudedHolyStack = 161, // player
    MeltdownSpread = 558, // player
    DimensionZero = 719, // Boss->player
}

public enum TetherID : uint
{
    BossTether = 391, // tether helper->Boss
    NaughtHunts = 404, // Void->player
    NaughtHuntsPass = 405, // player->player
    LoomingShadow = 284, // LoomingShadow->player
    GolbezZero = 425, // Golbez->Zero
}

// TODO: no personal replay yet — verify IDs/shapes on first pull before clearing Incomplete
class Meteorain(BossModule module) : Components.RaidwideInstant(module, AID.Meteorain, 0);
class NaughtGrowsAOE(BossModule module) : Components.StandardAOEs(module, AID.NaughtGrowsAOE, new AOEShapeCircle(40));
class NaughtGrowsSmall(BossModule module) : Components.StandardAOEs(module, AID.NaughtGrowsAOESmall, 12);
class GazeOfTheVoid(BossModule module) : Components.StandardAOEs(module, AID.GazeOfTheVoidCones, new AOEShapeCone(40, 22.5f.Degrees()), maxCasts: 7);
class DeepFreeze(BossModule module) : Components.BaitAwayCast(module, AID.DeepFreeze, new AOEShapeCircle(8), centerAtTarget: true);
class ShroudedHoly(BossModule module) : Components.StackWithCastTargets(module, AID.ShroudedHolyStack, 6, 4);
class MeltdownSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.MeltdownSpread, 5);
class MeltdownAOE(BossModule module) : Components.StandardAOEs(module, AID.MeltdownAOE, 5);
class SilentTorrentSmall(BossModule module) : Components.StandardAOEs(module, AID.SilentTorrentSmall, new AOEShapeDonutSector(17, 19, 10.Degrees()));
class SilentTorrentMedium(BossModule module) : Components.StandardAOEs(module, AID.SilentTorrentMedium, new AOEShapeDonutSector(17, 19, 20.Degrees()));
class SilentTorrentLarge(BossModule module) : Components.StandardAOEs(module, AID.SilentTorrentLarge, new AOEShapeDonutSector(17, 19, 30.Degrees()));
class VacuumCircle(BossModule module) : Components.StandardAOEs(module, AID.VacuumCircle, 7);
class EmptyShadow(BossModule module) : Components.StandardAOEs(module, AID.EmptyShadowAOE, 10);
class LoomingEmptiness(BossModule module) : Components.StandardAOEs(module, AID.LoomingEmptinessAOE, 42);
class Nothingness(BossModule module) : Components.StandardAOEs(module, AID.Nothingness, new AOEShapeRect(100, 2));
class Almagest(BossModule module) : Components.RaidwideCast(module, AID.Almagest);
class DimensionZero(BossModule module) : Components.IconLineStack(module, 4, 60, (uint)IconID.DimensionZero, AID.DimensionZeroMarker, 5);

class EndlessChase(BossModule module) : Components.GenericChasingAOEs(module, AID.EndlessChaseRest)
{
    private const float Distance = 2.9f;
    private int _tethers;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.NaughtHunts || WorldState.Actors.Find(tether.Target) is not { } target)
            return;
        if (_tethers == 2)
            Chasers.Clear();
        Chasers.Add(new(new AOEShapeCircle(6), target, source.Position, 0, 13, WorldState.FutureTime(6), 0.7f, Distance));
        ++_tethers;
        if (_tethers == 4)
            _tethers = 0;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.NaughtHuntChaser)
            Chasers.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.EndlessChaseRest)
            return;
        if (Advance(spell.TargetXZ, Distance, WorldState.CurrentTime) == null)
            ReportError($"unexpected chase cast at {spell.TargetXZ}");
    }
}

class ArenaSwitcher(BossModule module) : BossComponent(module)
{
    public Actor? Beacon { get; private set; }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BeaconInTheDark)
            Beacon = actor;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0)
            return;
        switch (state)
        {
            case 0x00020001:
                Arena.Bounds = new ArenaBoundsSquare(80);
                break;
            case 0x00080004:
                Arena.Bounds = new ArenaBoundsCircle(20);
                break;
        }
    }
}

class LoomingShadowAdds(BossModule module) : Components.Adds(module, (uint)OID.LoomingShadow);
class UncastShadowAdds(BossModule module) : Components.Adds(module, (uint)OID.UncastShadow, 1);
class BeaconAdds(BossModule module) : Components.Adds(module, (uint)OID.BeaconInTheDark, 1, true);

class LightlessWorld(BossModule module) : Components.GenericLineOfSightAOE(module, AID.LightlessWorld, 40, false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LightlessWorld)
            Modify(caster.Position, Module.Enemies(OID.Zero).Select(z => (z.Position, z.HitboxRadius)), Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LightlessWorld1 or AID.LightlessWorldAOE)
            Modify(null, []);
    }
}

class T08EnuoStates : StateMachineBuilder
{
    public T08EnuoStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Meteorain>()
            .ActivateOnEnter<NaughtGrowsAOE>()
            .ActivateOnEnter<NaughtGrowsSmall>()
            .ActivateOnEnter<GazeOfTheVoid>()
            .ActivateOnEnter<DeepFreeze>()
            .ActivateOnEnter<EndlessChase>()
            .ActivateOnEnter<ShroudedHoly>()
            .ActivateOnEnter<MeltdownSpread>()
            .ActivateOnEnter<MeltdownAOE>()
            .ActivateOnEnter<SilentTorrentSmall>()
            .ActivateOnEnter<SilentTorrentMedium>()
            .ActivateOnEnter<SilentTorrentLarge>()
            .ActivateOnEnter<VacuumCircle>()
            .ActivateOnEnter<ArenaSwitcher>()
            .ActivateOnEnter<LoomingShadowAdds>()
            .ActivateOnEnter<UncastShadowAdds>()
            .ActivateOnEnter<BeaconAdds>()
            .ActivateOnEnter<EmptyShadow>()
            .ActivateOnEnter<LoomingEmptiness>()
            .ActivateOnEnter<Nothingness>()
            .ActivateOnEnter<LightlessWorld>()
            .ActivateOnEnter<Almagest>()
            .ActivateOnEnter<DimensionZero>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1115, NameID = 14749, Incomplete = true)]
public class T08Enuo(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
