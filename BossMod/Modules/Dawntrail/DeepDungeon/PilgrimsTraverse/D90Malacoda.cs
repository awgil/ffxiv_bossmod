namespace BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D90Malacoda;

public enum OID : uint
{
    Boss = 0x48F5, // R4.500, x1
    Helper = 0x233C, // R0.500, x26, Helper type
    ArcaneCylinder = 0x48F6, // R4.000, x12
    ArcaneCylinderHelper = 0x48F7, // R1.000, x8
}

public enum AID : uint
{
    AutoAttack = 45130, // Boss->player, no cast, single-target
    Backhand1 = 44250, // Boss->self, 5.0s cast, range 30 ?-degree cone
    Backhand2 = 44251, // Boss->self, 5.0s cast, range 30 ?-degree cone
    Appear = 44256, // ArcaneCylinder->location, no cast, single-target
    ForeHindFolly = 44258, // Boss->self, 7.0+0.6s cast, single-target
    TwinWingedTreachery = 44259, // Boss->self, 7.0+0.6s cast, single-target
    DevilsQuarter = 44262, // Helper->self, 7.6s cast, range 35 90-degree cone
    ArcaneBeaconSlow = 44257, // ArcaneCylinder->self, 7.6s cast, range 50 width 10 rect
    ArcaneBeaconFast = 43796, // ArcaneCylinder->self, 5.6s cast, range 50 width 10 rect
    Skinflayer = 44266, // Boss->self, 5.0s cast, distance 30 knockback
    HotIronCast = 44267, // Boss->self, 3.0s cast, single-target
    HotIron = 44268, // Helper->location, 3.0s cast, range 6 circle
}

class Backhand(BossModule module) : Components.GroupedAOEs(module, [AID.Backhand1, AID.Backhand2], new AOEShapeCone(30, 135.Degrees()));
class DevilsQuarter(BossModule module) : Components.StandardAOEs(module, AID.DevilsQuarter, new AOEShapeCone(35, 45.Degrees()));
class ArcaneBeacon(BossModule module) : Components.GroupedAOEs(module, [AID.ArcaneBeaconSlow, AID.ArcaneBeaconFast], new AOEShapeRect(50, 5));
class HotIron(BossModule module) : Components.StandardAOEs(module, AID.HotIron, 6);
class Skinflayer(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Skinflayer, 30, kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
                hints.AddForbiddenZone(ShapeContains.Rect(src.Origin + src.Direction.ToDirection() * 10, src.Direction, 50, 0, 25), src.Activation);
    }
}

class D90MalacodaStates : StateMachineBuilder
{
    public D90MalacodaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Backhand>()
            .ActivateOnEnter<DevilsQuarter>()
            .ActivateOnEnter<ArcaneBeacon>()
            .ActivateOnEnter<HotIron>()
            .ActivateOnEnter<Skinflayer>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1040, NameID = 14090)]
public class D90Malacoda(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsRect(20, 20, 45.Degrees(), 28.3f));

