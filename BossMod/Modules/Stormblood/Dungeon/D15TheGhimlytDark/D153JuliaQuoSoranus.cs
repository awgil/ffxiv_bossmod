namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D153JuliaQuoSoranus;

public enum OID : uint
{
    Boss = 0x25AF, // R0.600, x1
    AnniaQuoSoranus = 0x25B0, // R0.600, x1
    Helper = 0x233C, // R0.500, x35, Helper type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    CeruleumTank = 0x25B2, // R1.000, x0 (spawn during fight)
    SoranusDuo1 = 0x25B1, // R0.000, x1 (spawn during fight)
    SoranusDuo2 = 0x25B5, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    Ability1 = 14095, // AnniaQuoSoranus->location, no cast, single-target
    Ability2 = 14094, // Boss->location, no cast, single-target
    AglaeaBite = 14103, // AnniaQuoSoranus->CeruleumTank, no cast, single-target
    AngrySalamander = 14124, // AnniaQuoSoranus->self, 3.0s cast, range 45+R width 6 rect
    ArtificialBoostAnnia = 14128, // AnniaQuoSoranus->self, 3.0s cast, single-target
    ArtificialBoostJulia = 14127, // Boss->self, 3.0s cast, single-target
    ArtificialPlasma = 14119, // Boss->self, 3.0s cast, range 40+R circle
    AutoAttack = 870, // Boss->player, no cast, single-target
    Burst = 14106, // CeruleumTank->self, 2.0s cast, range 10+R circle
    CommenceAirStrike = 14102, // Boss->self, 3.0s cast, single-target
    CoveringFire = 14108, // Helper->player, 5.0s cast, range 8 circle
    Crosshatch = 14113, // SoranusDuo1->self, no cast, single-target
    CrosshatchHelper = 14114, // Helper->self, no cast, range 40+R width 4 rect
    Heirsbane = 14105, // Boss->CeruleumTank, 3.5s cast, single-target
    Helper1 = 14115, // Helper->self, 3.5s cast, range 20+R width 4 rect // Dash
    Helper2 = 14411, // Helper->self, 3.5s cast, range 35+R width 4 rect
    Helper3 = 14412, // Helper->self, 3.5s cast, range 39+R width 4 rect
    Helper4 = 14116, // Helper->self, 3.5s cast, range 40+R width 4 rect
    Helper5 = 14698, // Helper->self, 3.5s cast, range 39+R width 4 rect
    Helper6 = 14697, // Helper->self, 3.5s cast, range 35+R width 4 rect
    Helper7 = 14413, // Helper->self, 3.5s cast, range 29+R width 4 rect
    Helper8 = 14699, // Helper->self, 3.5s cast, range 40+R width 4 rect
    ImperialAuthorityAnnia = 14130, // AnniaQuoSoranus->self, 40.0s cast, range 80 circle
    ImperialAuthorityJulia = 14129, // Boss->self, 40.0s cast, range 80 circle
    Innocence = 14121, // Boss->player, 4.0s cast, single-target
    MissileImpact = 14126, // Helper->location, 3.0s cast, range 6 circle
    OrderToFire = 14125, // AnniaQuoSoranus->self, 3.0s cast, single-target
    OrderToSupport = 14107, // AnniaQuoSoranus->self, 3.0s cast, single-target
    Quaternity = 14729, // SoranusDuo2->self, 3.0s cast, range 25+R width 4 rect
    Roundhouse = 14104, // AnniaQuoSoranus->self, no cast, range 6+R circle
    TheOrder1 = 14099, // Boss->self, no cast, single-target
    TheOrder2 = 14778, // Boss->self, 3.0s cast, single-target
}

class D153JuliaQuoSoranusStates : StateMachineBuilder
{
    public D153JuliaQuoSoranusStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7857)]
public class D153JuliaQuoSoranus(WorldState ws, Actor primary) : BossModule(ws, primary, new(372, -265), new ArenaBoundsCircle(20));
