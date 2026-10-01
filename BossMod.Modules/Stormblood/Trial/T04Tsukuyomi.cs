namespace BossMod.Stormblood.Trial.T04Tsukuyomi;

public enum OID : uint
{
    Boss = 0x2210, // R3.250
    Helper = 0x18D6, // R0.500
    DancingFan = 0x2241, // R1.600
    MidnightHaze = 0x2242, // R1.000
    SpecterOfThePatriarch = 0x2244, // R0.600
    SpecterOfTheMatriarch = 0x2245, // R0.600
    SpecterOfAsahi = 0x2246, // R0.600
    SpecterOfZenos = 0x2247, // R1.152
    SpecterOfGosetsu = 0x2248, // R0.600
    Yotsuyu = 0x2249, // R0.600
    SpecterOfTheHomeland = 0x224A, // R0.600
    SpecterOfTheEmpire = 0x224B, // R0.600
    Moonlight = 0x2278, // R1.000
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttackNight = 11523, // Boss->player, no cast, single-target
    SpecterAuto = 11542, // SpecterOfThePatriarch/Matriarch/Empire->player, no cast, single-target
    HomelandAuto = 11543, // SpecterOfTheHomeland->player, no cast, single-target
    AsahiAuto = 11858, // SpecterOfAsahi->player, no cast, single-target

    GosetsuSpecial = 11211, // SpecterOfGosetsu->self, no cast, single-target
    Reprimand = 11234, // Boss->self, 4.0s cast, range 100 circle
    TormentUntoDeath = 11235, // Boss->player, 4.0s cast, range 15+R 90-degree cone tankbuster
    Nightfall = 11236, // Boss->self, 4.0s cast, single-target
    NightfallRepeat = 11237, // Boss->self, 4.0s cast, single-target
    LeadOfTheUnderworld = 11238, // Boss->player, 5.0s cast, range 40+R width 8 rect
    SteelOfTheUnderworld = 11239, // Boss->self, 3.0s cast, range 40+R 90-degree cone
    MidnightHaze = 11240, // Boss->location, 4.0s cast, single-target
    MidnightHazeSpawn = 11241, // Boss->self, no cast, single-target
    ToAshes = 11243, // MidnightHaze->self, 20.0s cast, range 100 circle
    ZashikiAsobi = 11244, // Boss->self, 4.0s cast, single-target
    TsukiNoMaiogi = 11245, // DancingFan->self, 5.0s cast, range 10 circle
    Nightbloom = 11246, // Boss->self, 6.0s cast, range 100 circle
    Concentrativity = 11247, // SpecterOfZenos->self, no cast, range 100 circle
    Dispersivity = 11248, // Helper->self, no cast, range 100 circle
    Selenomancy = 11249, // Boss->self, 4.0s cast, single-target
    Antitwilight = 11256, // Boss->self, 5.0s cast, range 100 circle
    DarkBlade = 11257, // Boss->self, 3.0s cast, range 40+R 210-degree cone
    BrightBlade = 11258, // Boss->self, 3.0s cast, range 40+R 210-degree cone
    Lunacy = 11259, // Boss->player, 5.0s cast, range 6 circle stack
    LunacyRepeat = 11260, // Boss->player, no cast, range 6 circle
    LunarHalo = 11379, // Moonlight->self, 4.0s cast, range 2-15 donut
    UnmovingTroika = 11435, // SpecterOfZenos->self, no cast, range 9+R cone
    UnmovingTroikaSecond = 11436, // Helper->self, 1.7s cast, range 9+R cone
    UnmovingTroikaThird = 11437, // Helper->self, 2.1s cast, range 9+R cone
    NightbloomYotsuyu = 11438, // Yotsuyu->self, no cast, single-target
    NightbloomAdds = 11440, // Helper->self, 4.0s cast, range 60 circle
    LeadOfTheUnderworldMarker = 11441, // Helper->player, no cast, single-target
    DanceOfTheDead = 11551, // Helper->self, no cast, single-target
    DanceOfTheDeadRaidwide = 11897, // Boss->self, no cast, range 100 circle
    TormentUntoDeathRepeat = 11955, // Boss->player, 4.0s cast, range 15+R 90-degree cone tankbuster
}

public enum SID : uint
{
    VulnerabilityUp = 202,
    DownForTheCount = 783,
    Haunt1 = 1542,
    Grudge = 1573,
    Stun = 149,
    Haunt2 = 1543,
    Moonshadowed = 1539,
    Moonlit = 1538,
    Doom = 210,
    Bleeding = 642,
    BloodMoon = 1537,
}

class Reprimand(BossModule module) : Components.RaidwideCast(module, AID.Reprimand);
class Nightbloom(BossModule module) : Components.RaidwideCast(module, AID.Nightbloom);
class NightbloomAdds(BossModule module) : Components.RaidwideCast(module, AID.NightbloomAdds);
class Antitwilight(BossModule module) : Components.RaidwideCast(module, AID.Antitwilight);
class ToAshes(BossModule module) : Components.RaidwideCast(module, AID.ToAshes, "Kill Midnight Haze or raidwide");
class DanceOfTheDead(BossModule module) : Components.RaidwideInstant(module, AID.DanceOfTheDeadRaidwide, 0);

class TormentUntoDeath(BossModule module) : Components.GenericBaitAway(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCone _shape = new(18.25f, 45.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TormentUntoDeath or AID.TormentUntoDeathRepeat && WorldState.Actors.Find(spell.TargetID) is { } target)
            CurrentBaits.Add(new(caster, target, _shape, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TormentUntoDeath or AID.TormentUntoDeathRepeat)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}

class LeadOfTheUnderworld(BossModule module) : Components.SimpleLineStack(module, 4, 70, AID.LeadOfTheUnderworldMarker, AID.LeadOfTheUnderworld, 5);
class SteelOfTheUnderworld(BossModule module) : Components.StandardAOEs(module, AID.SteelOfTheUnderworld, new AOEShapeCone(70, 45.Degrees()));
class TsukiNoMaiogi(BossModule module) : Components.StandardAOEs(module, AID.TsukiNoMaiogi, 10, maxCasts: 7);
class DarkBladeBrightBlade(BossModule module) : Components.GroupedAOEs(module, [AID.DarkBlade, AID.BrightBlade], new AOEShapeCone(70, 105.Degrees()));
class Lunacy(BossModule module) : Components.StackWithCastTargets(module, AID.Lunacy, 6, 8, 8);
class LunarHalo(BossModule module) : Components.StandardAOEs(module, AID.LunarHalo, new AOEShapeDonut(2, 15));
class UnmovingTroikaSecond(BossModule module) : Components.StandardAOEs(module, AID.UnmovingTroikaSecond, new AOEShapeCone(39, 37.5f.Degrees()));
class UnmovingTroikaThird(BossModule module) : Components.StandardAOEs(module, AID.UnmovingTroikaThird, new AOEShapeCone(39, 37.5f.Degrees()));
class MidnightHazeAdds(BossModule module) : Components.Adds(module, (uint)OID.MidnightHaze, 2);
class SpecterAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.SpecterOfThePatriarch, (uint)OID.SpecterOfTheMatriarch, (uint)OID.SpecterOfAsahi, (uint)OID.SpecterOfZenos, (uint)OID.SpecterOfGosetsu, (uint)OID.SpecterOfTheHomeland, (uint)OID.SpecterOfTheEmpire], 1);

class MoonStatus(BossModule module) : BossComponent(module)
{
    // TODO: draw moonlit/moonshadowed safe sides instead of a text hint once arena half mapping is confirmed
    public override void AddGlobalHints(GlobalHints hints)
    {
        var pc = Raid.Player();
        if (pc != null && (pc.FindStatus(SID.Moonlit) is { Extra: >= 3 } || pc.FindStatus(SID.Moonshadowed) is { Extra: >= 3 }))
            hints.Add("Swap sides to drop moon stacks!");
    }
}

class T04TsukuyomiStates : StateMachineBuilder
{
    public T04TsukuyomiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TsukiNoMaiogi>()
            .ActivateOnEnter<TormentUntoDeath>()
            .ActivateOnEnter<SteelOfTheUnderworld>()
            .ActivateOnEnter<Reprimand>()
            .ActivateOnEnter<MidnightHazeAdds>()
            .ActivateOnEnter<LeadOfTheUnderworld>()
            .ActivateOnEnter<Nightbloom>()
            .ActivateOnEnter<SpecterAdds>()
            .ActivateOnEnter<UnmovingTroikaSecond>()
            .ActivateOnEnter<UnmovingTroikaThird>()
            .ActivateOnEnter<NightbloomAdds>()
            .ActivateOnEnter<LunarHalo>()
            .ActivateOnEnter<Antitwilight>()
            .ActivateOnEnter<MoonStatus>()
            .ActivateOnEnter<Lunacy>()
            .ActivateOnEnter<DanceOfTheDead>()
            .ActivateOnEnter<DarkBladeBrightBlade>()
            .ActivateOnEnter<ToAshes>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 537, NameID = 7225)] // TODO: clear Incomplete after moon-gauge safespots + specter timeline
public class T04Tsukuyomi(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
