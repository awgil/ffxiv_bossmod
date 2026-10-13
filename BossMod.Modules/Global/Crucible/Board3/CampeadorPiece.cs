namespace BossMod.Global.Crucible.CampeadorPiece;

public enum OID : uint
{
    Boss = 0x4CA5, // R2.000, x1
    Helper = 0x233C, // R0.500, x7, Helper type
    SabotenderPiece = 0x4CA6, // R0.800, x38
    FlowertenderPiece = 0x4CA7, // R1.600, x2
    SoldadoPiece = 0x4CA8, // R2.000, x4
    GuardiaPiece = 0x4CA9, // R2.400, x1
}

public enum AID : uint
{
    EmergencyOrder = 48578, // Boss->self, 5.0s cast, single-target
    AutoAttack = 49682, // SabotenderPiece/FlowertenderPiece/SoldadoPiece/GuardiaPiece/Boss->player, no cast, single-target
    HealingWater = 48579, // FlowertenderPiece->SabotenderPiece, 3.0s cast, range 6 circle, doesn't deal damage
    NeedlesCast = 48580, // SoldadoPiece->self, 8.0s cast, single-target
    Needles = 48581, // Helper->self, 8.0s cast, range 15 circle
    Cactguard = 48582, // GuardiaPiece->Boss, no cast, single-target
    SeedingNeedlesRaidwide = 48589, // Boss->self, 5.0s cast, range 100 circle
    SeedingNeedlesDonutCast = 48587, // Boss->self, 5.3+0.7s cast, single-target
    SeedingNeedlesDonut = 48588, // Helper->self, 6.0s cast, range 5-40 donut
}

public enum IconID : uint
{
    Order1 = 336, // 4CA8->self
    Order2 = 337, // 4CA8->self
    Order3 = 338, // 4CA8->self
    Order4 = 339, // 4CA8->self
}

public enum SID : uint
{
    Cover = 2412, // GuardiaPiece->GuardiaPiece, extra=0x14
    Covered = 2413, // GuardiaPiece->Boss, extra=0x0
}

class Flowertender(BossModule module) : Components.Adds(module, (uint)OID.FlowertenderPiece, 2);
class SabotenderPiece(BossModule module) : Components.AddsMulti(module, [OID.SabotenderPiece, OID.SoldadoPiece, OID.GuardiaPiece], 1);
class Needles(BossModule module) : Components.StandardAOEs(module, AID.Needles, 15, 3);
class SeedingNeedles(BossModule module) : Components.RaidwideCast(module, AID.SeedingNeedlesRaidwide);
class SeedingNeedlesDonut(BossModule module) : Components.StandardAOEs(module, AID.SeedingNeedlesDonut, new AOEShapeDonut(5, 40));

class CampeadorPieceStates : StateMachineBuilder
{
    public CampeadorPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flowertender>()
            .ActivateOnEnter<SabotenderPiece>()
            .ActivateOnEnter<Needles>()
            .ActivateOnEnter<SeedingNeedles>()
            .ActivateOnEnter<SeedingNeedlesDonut>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14587)]
public class CampeadorPiece(ModuleInit init) : BossModule(init, new(120, -420), new ArenaBoundsCircle(20));

