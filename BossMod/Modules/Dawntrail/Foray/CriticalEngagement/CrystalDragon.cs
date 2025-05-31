namespace BossMod.Dawntrail.Foray.CriticalEngagement.CrystalDragon;

public enum OID : uint
{
    Boss = 0x4715, // R4.200, x1
    DraconicDouble = 0x4716, // R4.200, x2
    Crystal = 0x4717, // R1.700, x6
    Helper = 0x233C, // R0.500, x12, Helper type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    PrimalRoarCast = 41762, // Boss->self, 4.1+0.9s cast, single-target
    PrimalRoar = 41763, // Helper->self, 5.0s cast, ???
    FearsomeFacet = 41749, // Boss->self, 3.0s cast, single-target
    PrismaticWingOutCast = 41750, // DraconicDouble->self, 6.2+0.8s cast, single-target
    PrismaticWingInCast = 41751, // DraconicDouble->self, 6.0+1.0s cast, single-target
    PrismaticWingOut = 42766, // Helper->self, 7.0s cast, range 22 circle
    PrismaticWingIn = 42767, // Helper->self, 7.0s cast, range 5-31 donut
    MadeMagicOutCast = 41752, // Helper->self, 4.3+0.2s cast, single-target
    MadeMagicInCast = 41753, // Helper->self, 4.1+0.4s cast, single-target
    MadeMagicOut = 42768, // Helper->self, 4.5s cast, range 22 circle
    MadeMagicIn = 42769, // Helper->self, 4.5s cast, range 5-31 donut
    CrystalMirror = 41754, // Boss->self, 6.0s cast, single-target
    CrystalCall = 41748, // Boss->self, 3.0s cast, single-target
    Energy1 = 41758, // Helper->self, 4.0s cast, range 7 circle
    Energy2 = 42728, // Helper->self, 7.0s cast, range 7 circle
    Energy3 = 42732, // Helper->self, 10.0s cast, range 7 circle
    ChaosSmall1 = 41759, // Crystal->self, 4.0s cast, range 7-13 donut
    ChaosMedium1 = 41760, // Crystal->self, 4.0s cast, range 13-19 donut
    ChaosLarge1 = 41761, // Crystal->self, 4.0s cast, range 19-25 donut
    ChaosSmall2 = 42729, // Crystal->self, 7.0s cast, range 7-13 donut
    ChaosMedium2 = 42730, // Crystal->self, 7.0s cast, range 13-19 donut
    ChaosLarge2 = 42731, // Crystal->self, 7.0s cast, range 19-25 donut
    ChaosBoss2 = 41755, // Boss->self, 7.0s cast, single-target
    ChaosBoss1 = 41757, // Boss->self, no cast, single-target
    ChaosBoss3 = 41756, // Boss->self, 10.0s cast, single-target
    ChaosSmall3 = 42733, // Crystal->self, 10.0s cast, range 7-13 donut
    ChaosMedium3 = 42734, // Crystal->self, 10.0s cast, range 13-19 donut
    ChaosLarge3 = 42735, // Crystal->self, 10.0s cast, range 19-25 donut
}

public enum TetherID : uint
{
    Order1 = 314, // Crystal->Boss
    Order2 = 315, // Crystal->Boss
    Order3 = 316, // Crystal->Boss
}

class PrimalRoar(BossModule module) : Components.RaidwideCast(module, AID.PrimalRoarCast);
class PrismaticWingOut(BossModule module) : Components.StandardAOEs(module, AID.PrismaticWingOut, 22);
class MadeMagicOut(BossModule module) : Components.StandardAOEs(module, AID.MadeMagicOut, 22);
class PrismaticWingIn(BossModule module) : Components.StandardAOEs(module, AID.PrismaticWingIn, new AOEShapeDonut(5, 31));
class MadeMagicIn(BossModule module) : Components.StandardAOEs(module, AID.MadeMagicIn, new AOEShapeDonut(5, 31));

class CrystallizedEnergy(BossModule module) : Components.GroupedAOEs(module, [AID.Energy1, AID.Energy2, AID.Energy3], new AOEShapeCircle(7));
class CrystallizedChaos1(BossModule module) : Components.GroupedAOEs(module, [AID.ChaosSmall1, AID.ChaosSmall2, AID.ChaosSmall3], new AOEShapeDonut(7, 13));
class CrystallizedChaos2(BossModule module) : Components.GroupedAOEs(module, [AID.ChaosMedium1, AID.ChaosMedium2, AID.ChaosMedium3], new AOEShapeDonut(13, 19));
class CrystallizedChaos3(BossModule module) : Components.GroupedAOEs(module, [AID.ChaosLarge1, AID.ChaosLarge2, AID.ChaosLarge3], new AOEShapeDonut(19, 25));

class CrystalDragonStates : StateMachineBuilder
{
    public CrystalDragonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PrimalRoar>()
            .ActivateOnEnter<PrismaticWingOut>()
            .ActivateOnEnter<MadeMagicOut>()
            .ActivateOnEnter<PrismaticWingIn>()
            .ActivateOnEnter<MadeMagicIn>()
            .ActivateOnEnter<CrystallizedEnergy>()
            .ActivateOnEnter<CrystallizedChaos1>()
            .ActivateOnEnter<CrystallizedChaos2>()
            .ActivateOnEnter<CrystallizedChaos3>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13696)]
public class CrystalDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-414, 75), new ArenaBoundsCircle(24.5f))
{
    public override bool DrawAllPlayers => true;
}
