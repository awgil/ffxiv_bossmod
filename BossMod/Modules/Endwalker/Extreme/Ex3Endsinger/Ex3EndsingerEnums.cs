namespace BossMod.Endwalker.Extreme.Ex3Endsigner;

public enum OID : uint
{
    Boss = 0x38BF,
    Shinryu = 0x38C0, // x1
    SmallHead = 0x38C1, // x12
    FieryStar = 0x38C2, // x2, and more spawn for short time during fight
    AzureStar = 0x38C3, // x2, and more spawn for short time during fight
    DarkStar = 0x38C4, // x1
    Helper = 0x233C, // x16, and more spawn for short time during fight
};

public enum AID : uint
{
    AutoAttack = 28661,
    Telos = 28718, // Boss->self, 5s cast, raidwide
    Hubris = 28716, // Boss->self, 5s cast, tankbuster visual
    HubrisAOE = 28717, // Helper->target, 5s cast, range 6 aoe

    ElegeiaUnforgotten = 28662, // Boss->self, 5s cast
    Elegeia = 28663, // Helper->self, no cast, raidwide
    FieryStarVisual = 28664, // FieryStar->self, no cast, visual (red planet)
    AzureStarVisual = 28665, // AzureStar->self, no cast, visual (blue planet)
    RubistellarCollision = 28666, // Helper->self, 7s cast, range 30 aoe
    CaerustellarCollision = 28667, // Helper->self, 7s cast, knockback 25
    DiairesisElegeia = 28668, // SmallHead->self, 5s cast, range 20 180-degree cone

    Katasterismoi = 28713, // Boss->self, 3s cast, visual
    KatasterismoiExplosion = 28714, // Helper->self, no cast, range 4 aoe if tower is soaked
    KatasterismoiMassiveExposion = 28715, // Helper->self, no cast, range 40 aoe if failed
    GripOfDespair = 28701, // Boss->self, 5s cast, visual

    ElenchosSides = 28704, // Boss->self, 6s cast, visual
    ElenchosSidesAOE = 28705, // Helper->self, 6s cast, 6.5 half-width rect
    ElenchosCenter = 28706, // Boss->self, 6s cast, 7 half-width rect

    Eironeia = 28719, // Boss->self, 5s cast, visual
    EironeiaAOE = 28720, // Helper->target (healer), 5s cast, range 6 stack

    Fatalism = 28669, // Boss->self, 3s cast, visual
    FatalismFieryStar1 = 28670, // FieryStar->self, no cast, caster is in center, but faces direction of first explosion
    FatalismFieryStar2 = 28671, // FieryStar->self, no cast, caster is in center, but faces direction of second explosion
    FatalismAzureStar1 = 28672, // AzureStar->self, no cast, caster is in center, but faces direction of first explosion
    FatalismAzureStar2 = 28673, // AzureStar->self, no cast, caster is in center, but faces direction of second explosion
    FatalismRubistellarCollisionVisual = 28674, // FieryStar->self, no cast, visual
    FatalismRubistallarCollisionAOE = 28675, // Helper->self, 2s cast, range 30 aoe
    FatalismCaerustellarCollisionVisual = 28676, // AzureStar->self, no cast, visual
    FatalismCaerustallarCollisionAOE = 28677, // Helper->self, 2s cast, knockback 25

    TwinsongAporrhoia = 28679, // Boss->self, 3s cast, visual
    DiairesisTwinsong = 28678, // SmallHead->self, 5s cast, range 20 180-degree cone
    AporrhoiaUnforgotten = 28680, // Boss->self, 5s cast, visual
    NecroticFluid = 28681, // SmallHead->self, 5s cast, range 15 aoe
    WaveOfNausea = 28682, // SmallHead->self, 5s cast, range 15 donut aoe, ? inner
    TheologicalFatalism = 28683, // Boss->self, 3s cast, visual
    FatalismNecroticFluidVisual = 28684, // SmallHead->self, no cast, visual
    FatalistWaveOfNauseaVisual = 28685, // SmallHead->self, no cast, visual
    FatalismDiairesisVisual = 28686, // SmallHead->self, no cast, visual
    FatalismNecroticFluid = 28687, // SmallHead->self, 1s cast, range 15 aoe
    FatalismWaveOfNausea = 28688, // SmallHead->self, 1s cast, range 15 donut aoe, ? inner
    FatalismDiairesis = 28689, // SmallHead->self, 1s cast, range 20 180-degree cone

    DespairUnforgotten = 28690, // Boss->self, 3s cast, visual
    WaveOfNauseaDespair = 28691, // Helper->target, 6s cast, range 15 donut aoe, 6 inner?
    Befoulment = 28692, // Helper->target, 6s cast, range 10 aoe
    NoFuture = 28693, // Helper->target, 6s cast, range 40 aoe with ? falloff
    Benevolence = 28694, // Helper->target, 6s cast, range 6 stack
    FatalismWaveOfNauseaDespair = 28695, // Helper->target, 2s cast
    FatalismBefoulment = 28696, // Helper->target, 2s cast
    FatalismNoFuture = 28697, // Helper->target, 2s cast
    FatalismBenevolence = 28698, // Helper->target, 2s cast

    Telomania = 29379, // Boss->self, 5s cast, visual
    TelomaniaHit = 29380, // Helper->self, no cast, raidwide
    TelomaniaLast = 29381, // Helper->self, no cast, heavy raidwide with bleed

    EndsongAporrhoia = 29361, // Boss->self, 3s cast, visual
    Endsong = 28699, // Boss->self, 3s cast, visual
    EndsongHead = 28700, // SmallHead->self, 2s cast, range 15 aoe

    Enrage = 28721, // Boss->self, 5s cast, visual
    EnrageAOE = 28722, // Helper->self, 5s cast
};

public enum SID : uint
{
    None = 0,
    RewindTwinsong = 2056, // 0x178 = 1 ring, 0x179 = 2 rings, 0x17A = 3 rings
    RewindDespair = 2397, // 0x17C = 1 ring, 0x17D = 2 rings, 0x17E = 3 rings
    EchoesOfNausea = 2989, // donut marker
    EchoesOfBefoulment = 2990, // spread marker
    EchoesOfFuture = 2991, // flare marker
    EchoesOfBenevolence = 2992, // stack marker
    GripOfDespair = 2993,
}

public enum TetherID : uint
{
    None = 0,
    GripOfDespair = 163, // player->player
    Fatalism = 180, // Helper->Boss
    EndsongNext = 181, // SmallHead->SmallHead
    EndsongFirst = 189, // SmallHead->Boss, source will be center of aoe
}

public enum IconID : uint
{
    None = 0,
    Eironeia = 161,
    EchoesNausea = 322,
    RewindNausea = 323,
    EchoesBefoulment = 328,
    RewindBefoulment = 324,
    EchoesFuture = 327,
    RewindFuture = 325,
    EchoesBenevolence = 318,
    RewindBenevolence = 221,
    GripOfDespair = 326,
    Hubris = 344,
}
