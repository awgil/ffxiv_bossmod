namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE212ManyMouthstoFeed;

public enum OID : uint
{
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    PelekysHelper = 0x233C, // R0.500, x40, Helper type
    Pelekys = 0x4BCA, // R7.000, x1
    Pelekys1 = 0x4BCC, // R0.500, x1
    Actor1ebfed = 0x1EBFED, // R0.500, x4, EventObj type
    Actor1ec007 = 0x1EC007, // R0.500, x1, EventObj type
    UnknownActor = 0x4BCD, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    UnknownWeaponskill = 47214, // Pelekys1->self, no cast, range ?-30 donut
    AutoAttack = 50850, // Pelekys->player, no cast, single-target
    AcridRain1 = 47231, // Pelekys->self, 5.0s cast, single-target
    AcridRain2 = 47232, // PelekysHelper->self, no cast, ???
    CentralGardening1 = 47218, // Pelekys->self, 5.0s cast, single-target
    CentralGardening2 = 47220, // PelekysHelper->self, 6.0s cast, range 52 width 10 rect
    SideGardening1 = 47219, // Pelekys->self, 5.0s cast, single-target
    SideGardening2 = 49729, // PelekysHelper->self, 6.0s cast, range 26 180.000-degree cone
    SideGardening3 = 47221, // PelekysHelper->self, 6.0s cast, range 26 180.000-degree cone
    NoxiousNectar = 49730, // Pelekys->self, 3.0s cast, single-target
    NoxiousNectar1 = 49885, // Pelekys->self, no cast, single-target
    Venom = 47216, // PelekysHelper->self, 4.8s cast, range 2 circle
    Venom1 = 47217, // PelekysHelper->self, no cast, range 2 circle
    NoxiousNectar2 = 47215, // Pelekys->self, no cast, single-target
    PollenLure = 47222, // Pelekys->self, 4.0s cast, single-target
    Devour = 47223, // Pelekys->self, 7.0s cast, range 10 circle
    PoisonHeart1 = 47229, // Pelekys->self, 4.0s cast, single-target
    PoisonHeart2 = 47230, // PelekysHelper->location, 3.0s cast, range 5 circle
    VenomMist1 = 47225, // Pelekys->self, 5.0s cast, single-target
    VenomMist2 = 50548, // PelekysHelper->self, 6.0s cast, range 30 90.000-degree cone
    VenomMist3 = 50547, // PelekysHelper->self, 6.0s cast, range 30 90.000-degree cone
    VenomMist4 = 50549, // PelekysHelper->self, 6.0s cast, range 30 90.000-degree cone
    VenomMist5 = 47227, // Pelekys->self, 5.0s cast, single-target
    VenomMist6 = 47228, // PelekysHelper->self, 6.0s cast, range 30 90.000-degree cone
}

public enum SID : uint
{
    Toxicosis = 4379, // PelekysHelper->player, extra=0x0
    VulnerabilityUp = 2347, // PelekysHelper/Pelekys->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA
    UnknownStatus1 = 2552, // none->Pelekys, extra=0x3F2/0x3F3
    Poison = 5425, // PelekysHelper->player, extra=0x0
    UnknownStatus2 = 2056, // none->UnknownActor, extra=0x3C2
    QuickerStep = 4799, // none->player, extra=0x0
}

[SkipLocalsInit]
sealed class CE212ManyMouthstoFeedStates : StateMachineBuilder
{
    public CE212ManyMouthstoFeedStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(CE212ManyMouthstoFeedStates),
    ConfigType = null, // replace null with typeof(ManyMouthstoFeedConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Pelekys,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14747u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE212ManyMouthstoFeed(WorldState ws, Actor primary) : BossModule(ws, primary, new(-870f, -560f), new ArenaBoundsCircle(30f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
