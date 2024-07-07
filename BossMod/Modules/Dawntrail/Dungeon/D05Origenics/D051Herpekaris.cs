namespace BossMod.Dawntrail.Dungeon.D05Origenics.D051Herpekaris;

public enum OID : uint
{
    Boss = 0x4185, // R8.400, x1
    Helper = 0x233C, // R0.500, x32, 523 type

    Cahciua = 0x418F, // R0.960, x1
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1e9e3c = 0x1E9E3C, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    StridentShriek = 36519, // Boss->self, 5.0s cast, range 60 circle // Raidwide

    UnknownAbility1 = 36520, // Boss->location, no cast, single-target
    UnknownAbility2 = 36474, // Helper->player, no cast, single-target

    Vasoconstrictor = 36459, // Boss->self, 3.0+1.2s cast, single-target // Spawns poison
    PoisonHeart1 = 36460, // Helper->location, 4.2s cast, range 2 circle
    PoisonHeart2 = 37921, // Helper->player, 8.0s cast, range 5 circle // Spreadmarker

    PodBurst1 = 38518, // Helper->location, 5.0s cast, range 6 circle
    PodBurst2 = 38519, // Helper->location, 4.0s cast, range 6 circle

    Venomspill1 = 37451, // Boss->self, 5.0s cast, single-target
    Venomspill2 = 36452, // Boss->self, 5.0s cast, single-target
    Venomspill3 = 36453, // Boss->self, 4.0s cast, single-target
    Venomspill4 = 36454, // Boss->self, 4.0s cast, single-target

    WrithingRiot = 36463, // Boss->self, 9.0s cast, single-target
    WrithingRiotRightSweep = 36465, // Helper->self, 2.0s cast, range 25 210.000-degree cone
    WrithingRiotLeftSweep = 36466, // Helper->self, 2.0s cast, range 25 210.000-degree cone
    WrithingRiotRearSweep = 36467, // Helper->self, 2.0s cast, range 25 ?-degree cone

    RightSweep = 36469, // Boss->self, no cast, range 25 ?-degree cone
    LeftSweep = 36470, // Boss->self, no cast, range 25 ?-degree cone
    RearSweep = 36471, // Boss->self, no cast, range 25 ?-degree cone

    CollectiveAgony = 36473, // Boss->self/players, 5.5s cast, range 50 width 8 rect
    ConvulsiveCrush = 36518, // Boss->player, 5.0s cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/Boss->player, extra=0x1/0x2/0x3
    Toxicosis1 = 3081, // none->player, extra=0x0
    Toxicosis2 = 3082, // none->player, extra=0x0
}

public enum IconID : uint
{
    Icon345 = 345, // player
    Icon218 = 218, // player
}

class StridentShriek(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.StridentShriek));
class PoisonHeart2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PoisonHeart2), 5);
class PoisonHeart1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PoisonHeart1), 2);
class PodBurst1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PodBurst1), 6);
class PodBurst2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.PodBurst2), 6);
class WrithingRiotRightSweep(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WrithingRiotRightSweep), new AOEShapeCone(25, 105.Degrees()));
class WrithingRiotLeftSweep(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WrithingRiotLeftSweep), new AOEShapeCone(25, 105.Degrees()));
class WrithingRiotRearSweep(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WrithingRiotRearSweep), new AOEShapeCone(25, 45.Degrees()));

class D051HerpekarisStates : StateMachineBuilder
{
    public D051HerpekarisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StridentShriek>()
            .ActivateOnEnter<PoisonHeart2>()
            .ActivateOnEnter<PoisonHeart1>()
            .ActivateOnEnter<PodBurst1>()
            .ActivateOnEnter<PodBurst2>()
            .ActivateOnEnter<WrithingRiotRightSweep>()
            .ActivateOnEnter<WrithingRiotLeftSweep>()
            .ActivateOnEnter<WrithingRiotRearSweep>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825, NameID = 12741)]
public class D051Herpekaris(WorldState ws, Actor primary) : BossModule(ws, primary, new(-88, -180), new ArenaBoundsSquare(17.5f));
