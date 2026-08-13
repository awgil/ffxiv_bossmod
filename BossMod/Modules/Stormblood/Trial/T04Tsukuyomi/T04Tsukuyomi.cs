namespace BossMod.Stormblood.Trial.T04Tsukuyomi;

public enum OID : uint
{
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x?, EventObj type : Helpers during spectres
    Tsukuyomi = 0x2210, // R3.250, x?
    Specter = 0x18D6, // R0.500, x?, mixed types : Helper
    DancingFan = 0x2241, // R1.600, x?
    MidnightHaze = 0x2242, // R1.000, x? : Cloud adds
    SpecterOfZenos = 0x2247, // R1.152, x?
    SpecterOfGosetsu = 0x2248, // R0.600, x?
    Yotsuyu = 0x2249, // R0.600, x?
    SpecterOfThePatriarch = 0x2244, // R0.600, x?
    SpecterOfTheMatriarch = 0x2245, // R0.600, x?
    SpecterOfTheEmpire = 0x224B, // R0.600, x?
    SpecterOfTheHomeland = 0x224A, // R0.600, x?
    SpecterOfAsahi = 0x2246, // R0.600, x?
    Moonlight = 0x2278, // R1.000, x? : The waxing/waning moon objects
    Yotsuyu1 = 0xFA453, // R0.500, x?, EventNpc type
}

public enum AID : uint
{
    _AutoAttack_Attack = 11523, // Tsukuyomi->player, no cast, single-target
    TormentUntoDeath = 11235, // Tsukuyomi->self/player, 4.0s cast, range 15+R ?-degree cone
    TormentUntoDeath1 = 11955, // Tsukuyomi->self/player, 4.0s cast, range 15+R ?-degree cone
    ZashikiAsobi = 11244, // Tsukuyomi->self, 4.0s cast, single-target
    Nightfall = 11237, // Tsukuyomi->self, 4.0s cast, single-target
    TsukiNoMaiogi = 11245, // DancingFan->self, 5.0s cast, range 10 circle
    SteelOfTheUnderworld = 11239, // Tsukuyomi->self, 3.0s cast, range 40+R 90.000-degree cone
    _Weaponskill_ = 11200, // Tsukuyomi->self, no cast, single-target
    Reprimand = 11234, // Tsukuyomi->self, 4.0s cast, range 100 circle
    MidnightHaze = 11240, // Tsukuyomi->location, 4.0s cast, single-target
    MidnightHaze1 = 11241, // Tsukuyomi->location, no cast, single-target
    Nightfall1 = 11236, // Tsukuyomi->self, 4.0s cast, single-target
    LeadOfUnderworldMark = 11441, // Specter->player, no cast, single-target
    LeadOfTheUnderworld = 11238, // Tsukuyomi->self/players, 5.0s cast, range 40+R width 8 rect
    _Weaponskill_1 = 11471, // Tsukuyomi->self, no cast, single-target
    Nightbloom = 11246, // Tsukuyomi->self, 6.0s cast, range 100 circle
    _AutoAttack_Attack1 = 11542, // SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfTheEmpire->player/Yotsuyu, no cast, single-target
    _AutoAttack_Attack2 = 11543, // SpecterOfTheHomeland->player, no cast, single-target
    _AutoAttack_Attack3 = 11858, // SpecterOfAsahi->Yotsuyu, no cast, single-target
    Concentrativity = 11247, // SpecterOfZenos->self, no cast, range 100 circle
    UnmovingTroika = 11435, // SpecterOfZenos->self, no cast, range 9+R ?-degree cone
    UnmovingTroika1 = 11436, // Specter->self, 1.7s cast, range 9+R ?-degree cone
    UnmovingTroika2 = 11437, // Specter->self, 2.1s cast, range 9+R ?-degree cone
    _Weaponskill_2 = 11210, // SpecterOfZenos->self, 3.0s cast, single-target
    _Weaponskill_3 = 11211, // SpecterOfGosetsu->location, no cast, width 8 rect charge
    Dispersivity = 11248, // Specter->self, no cast, range 100 circle
    _Weaponskill_4 = 11478, // SpecterOfGosetsu->location, no cast, ???
    Nightbloom1 = 11438, // Yotsuyu->self, no cast, single-target
    Nightbloom2 = 11440, // Specter->self, 4.0s cast, range 60 circle
    Selenomancy = 11249, // Tsukuyomi->self, 4.0s cast, single-target
    LunarHalo = 11379, // Moonlight->self, 4.0s cast, range ?-15 donut
    Antitwilight = 11256, // Tsukuyomi->self, 5.0s cast, range 100 circle
    _Weaponskill_5 = 11261, // Tsukuyomi->self, no cast, single-target
    DanceOfTheDead = 11551, // Specter->self, no cast, single-target
    DanceOfTheDead1 = 11897, // Tsukuyomi->self, no cast, range 100 circle
    Lunacy = 11259, // Tsukuyomi->players, 5.0s cast, range 6 circle
    Lunacy1 = 11260, // Tsukuyomi->players, no cast, range 6 circle
    DarkBlade = 11257, // Tsukuyomi->self, 3.0s cast, range 40+R 210.000-degree cone
    AutoAttack_Attack4 = 870, // Tsukuyomi->player, no cast, single-target
    BrightBlade = 11258, // Tsukuyomi->self, 3.0s cast, range 40+R 210.000-degree cone
    ToAshes = 11243, // 2242->self, 20.0s cast, range 100 circle
}

public enum SID : uint
{
    VulnerabilityUp = 202, // DancingFan/Boss->player, extra=0x1/0x2
    DownForTheCount = 783, // Boss->player, extra=0xEC7
    Haunt1 = 1542, // none->SpecterOfZenos/SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfTheEmpire/SpecterOfTheHomeland/SpecterOfAsahi, extra=0x0
    Grudge = 1573, // none->Yotsuyu, extra=0x0
    Stun = 149, // SpecterOfZenos->player, extra=0x0
    Haunt2 = 1543, // none->SpecterOfGosetsu, extra=0x0
    Moonshadowed = 1539, // none->player, extra=0x1/0x2/0x3/0x4
    Moonlit = 1538, // none->player, extra=0x1/0x2/0x3/0x4/0x5
    Doom = 210, // none->player, extra=0x0
    Bleeding = 642, // none->player, extra=0x0
    BloodMoon = 1537, // Boss->Boss, extra=0x58
}

public enum IconID : uint
{
    LunacyStack = 305, // player : Circle stack that hits 3x.
    TormentBait = 230, // player : tankbuster? the yellow with hazard stripes
}

public enum TetherID : uint
{
    MidnightHazeTether = 12, // MidnightHaze->MidnightHaze
    SpecterTether = 17, // SpecterOfThePatriarch/SpecterOfTheMatriarch/SpecterOfAsahi->Yotsuyu
}

sealed class TormentUntoDeath(BossModule module) : Components.BaitAwayCast(module, (uint)AID.TormentUntoDeath,
    new AOEShapeCone(15f, 37.5f.Degrees()), tankbuster: true);

sealed class TsukuNoMaiogi(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TsukiNoMaiogi, 10f, 7);

sealed class SteelOfTheUnderworld(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SteelOfTheUnderworld, new AOEShapeCone(70f, 45f.Degrees()));

sealed class Reprimand(BossModule module) : Components.RaidwideCast(module, (uint)AID.Reprimand);

sealed class MidnightHaze(BossModule module) : Components.Adds(module, (uint)OID.MidnightHaze, 1);

sealed class LeadOfTheUnderworld(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.LeadOfUnderworldMark, (uint)AID.LeadOfTheUnderworld, 5d, 70f, 4f, 8, 8);

/*
 * Nightbloom happens and the arena takes the bloody smoke look, same bounds. Multiple adds come out to be killed.
 */
sealed class Nightbloom(BossModule module) : Components.RaidwideCast(module, (uint)AID.Nightbloom);

sealed class Spectres(BossModule module) : Components.AddsMulti(module,
[
    (uint)OID.SpecterOfThePatriarch, (uint)OID.SpecterOfTheMatriarch, (uint)OID.SpecterOfTheEmpire,
    (uint)OID.SpecterOfTheHomeland, (uint)OID.SpecterOfAsahi, (uint)OID.SpecterOfZenos
], 1);

/*
 * After the spectres are dead she casts nightbloom again and arena appearance updates to the garden scene.  Same bounds.
 */
sealed class Nightbloom2(BossModule module) : Components.RaidwideCast(module, (uint)AID.Nightbloom2);

// Selenomancy triggers the floor change in arena to waxing/waning moon.

//Angle is an estimate
sealed class UnmovingTroika1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnmovingTroika1, new AOEShapeCone(39f, 37.5f.Degrees()));
//Angle is an estimate
sealed class UnmovingTroika2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnmovingTroika2, new AOEShapeCone(39f, 37.5f.Degrees()));

// radius are estimates
sealed class LunarHalo(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.LunarHalo, new AOEShapeDonut(6f, 14f));

/*
 * When the boss does the moon phases the arena is split into two halves.
 * Player will receive a stack of light or dark status every few seconds.
 * At 3 stacks ai will move to the other side of the arena to drop stacks.
 */
sealed class MoonStatus(BossModule module) : BossComponent(module)
{
    bool _westSideDanger;
    bool _eastSideDanger;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        // Check if we have 3 or more stacks. Check if player character is the affected target.
        if (status.ID is (uint)SID.Moonlit or (uint)SID.Moonshadowed && status.Extra >= 3 && actor.InstanceID == Raid.Player()!.InstanceID)
        {
            // This is assuming that moon splits are east/west always
            // if they can be north/south also this will need logic for that.
            // west.x < center.x < east.x  : where x is the east/west axis
            // figure if actor is on east or west side when building stacks
            if (Arena.Center.X > actor.Position.X)
                _westSideDanger = true;
            else if (Arena.Center.X < actor.Position.X)
                _eastSideDanger = true;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        // Check that status is dropping from the player character.
        if (status.ID is (uint)SID.Moonlit or (uint)SID.Moonshadowed && actor.InstanceID == Raid.Player()!.InstanceID)
        {
            _westSideDanger = false;
            _eastSideDanger = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var s in actor.Statuses)
        {
            if (s.ID is (uint)SID.Moonlit or (uint)SID.Moonshadowed)
            {
                if (_eastSideDanger)
                    hints.AddForbiddenZone(new SDRect(new WPos(Arena.Center.X + 20.5f, Arena.Center.Z), Arena.Center, 20f));
                else if (_westSideDanger)
                    hints.AddForbiddenZone(new SDRect(Arena.Center, new WPos(Arena.Center.X - 20.5f, Arena.Center.Z), 20f));
            }
        }
    }
}

// Anti-twilight is a raidwide that marks the end of the waxing/ waning phases.
sealed class Antitwilight(BossModule module) : Components.RaidwideCast(module, (uint)AID.Antitwilight);

sealed class Lunacy(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Lunacy, 6f, 8, 8);

sealed class BrightDarkBlade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BrightBlade, (uint)AID.DarkBlade], new AOEShapeCone(70f, 105f.Degrees()));

sealed class DanceOfTheDead(BossModule module) : Components.RaidwideCast(module, (uint)AID.DanceOfTheDead1);

sealed class ToAshes(BossModule module) : Components.RaidwideCast(module, (uint)AID.ToAshes);

[SkipLocalsInit]
sealed class TsukuyomiStates : StateMachineBuilder
{
    public TsukuyomiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            // Phase 1 simple aoe attacks
            .ActivateOnEnter<TsukuNoMaiogi>()
            .ActivateOnEnter<TormentUntoDeath>()
            .ActivateOnEnter<SteelOfTheUnderworld>()
            .ActivateOnEnter<Reprimand>()
            .ActivateOnEnter<MidnightHaze>()
            .ActivateOnEnter<LeadOfTheUnderworld>()
            // Phase 2 - multiple spectre adds
            .ActivateOnEnter<Nightbloom>()
            .ActivateOnEnter<Spectres>()
            // Phase 3 - the lunar stacks phase
            .ActivateOnEnter<UnmovingTroika1>()
            .ActivateOnEnter<UnmovingTroika2>()
            .ActivateOnEnter<Nightbloom2>()
            .ActivateOnEnter<LunarHalo>()
            .ActivateOnEnter<Antitwilight>()
            .ActivateOnEnter<MoonStatus>()
            .ActivateOnEnter<Lunacy>()
            // Final Phase with no moon status. Uses all the other 1st phase aoe
            .ActivateOnEnter<DanceOfTheDead>()
            .ActivateOnEnter<BrightDarkBlade>()
            .ActivateOnEnter<ToAshes>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(TsukuyomiStates),
    ConfigType = null, // replace null with typeof(TsukuyomiConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Tsukuyomi,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.Stormblood,
    Category = BossModuleInfo.Category.Trial,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 537u,
    NameID = 7225u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Tsukuyomi(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
