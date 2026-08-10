namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE215WebofTerror;

public enum OID : uint
{
    HiddenTrap = 0x4D28, // R1.000, x16
    ArachneDaughter = 0x233C, // R0.500, x19 (spawn during fight), Helper type
    CrescentArachneHelper = 0x4DFC, // R1.000, x1
    CrescentBombadeel = 0x4E42, // R2.850, x10
    CrescentHellhound = 0x4E30, // R4.500, x10
    CrescentBlackguard = 0x4E2F, // R2.500, x2 (spawn during fight)
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    CrescentOpken = 0x4E13, // R1.690, x1 (spawn during fight)
    CrescentJester = 0x4E3F, // R3.060, x6
    Actor1ec09d = 0x1EC09D, // R0.500, x1, EventObj type
    CrescentArachne = 0x4DFA, // R6.500, x1
    ArachneDaughter1 = 0x4DFB, // R2.400, x0 (spawn during fight)
    CrescentSoblyn = 0x4E1A, // R2.200, x0 (spawn during fight)
}
public enum AID : uint
{
    UnknownAbility = 50365, // CrescentArachneHelper->self, no cast, range ?-30 donut
    AutoAttack = 50853, // CrescentArachne->player, no cast, single-target
    Implosion = 50366, // CrescentArachne->self, 5.0s cast, single-target
    Implosion1 = 50367, // ArachneDaughter->self, no cast, ???
    Summon = 50368, // CrescentArachne->self, 3.0s cast, single-target
    ArachnidWeb = 50369, // CrescentArachne->ArachneDaughter1, 3.0s cast, single-target
    ArachnidWeb1 = 50370, // ArachneDaughter1->ArachneDaughter1, no cast, single-target
    ArachnidFunnel = 50371, // CrescentArachne->ArachneDaughter1, 5.0s cast, width 20 rect charge
    ArachnidFunnel1 = 50372, // CrescentArachne->location, no cast, width 20 rect charge
    ArachnidFunnel2 = 50680, // ArachneDaughter->location, no cast, width 20 rect charge
    AutoAttack1 = 50635, // ArachneDaughter1->player, no cast, single-target
    Conformity = 50376, // CrescentArachne->self, 3.0s cast, range 50 45.000-degree cone
    QueensOrders = 50647, // CrescentArachne->self, 3.0s cast, single-target
    Conformity1 = 50377, // ArachneDaughter1->self, 3.0s cast, range 50 45.000-degree cone
    BedrockUplift = 50378, // CrescentArachne->self, 4.7s cast, single-target
    BedrockUplift1 = 50379, // ArachneDaughter->self, 5.0s cast, range 10 circle
    BedrockUplift2 = 50380, // ArachneDaughter->self, 7.0s cast, range 10-20 donut
    BedrockUplift3 = 50381, // ArachneDaughter->self, 9.0s cast, range 20-30 donut
    VenomEruption = 50375, // ArachneDaughter1->self, 12.0s cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 2347, // ArachneDaughter/CrescentArachne/ArachneDaughter1->player, extra=0x1/0x2/0x3/0x4/0x5/0x6
    UnknownSID = 2056, // none->ArachneDaughter1, extra=0x291
    QuickerStep = 4799, // none->player, extra=0x0

}
public enum TetherID : uint
{
    Tether_chn_m0280_net_1e1 = 420, // ArachneDaughter1->CrescentArachne
    Tether_chn_m0280_net_0e1 = 408, // ArachneDaughter1->ArachneDaughter1
}

[SkipLocalsInit]
sealed class CE215WebofTerrorStates : StateMachineBuilder
{
    public CE215WebofTerrorStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(CE215WebofTerrorStates),
    ConfigType = null, // replace null with typeof(WebofTerrorConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = typeof(TetherID), // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.CrescentArachne,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 55u,
    SortOrder = 7,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE215WebofTerror(WorldState ws, Actor primary) : BossModule(ws, primary, new(170f, -136f), new ArenaBoundsCircle(25f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}
