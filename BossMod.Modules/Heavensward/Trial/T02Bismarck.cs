namespace BossMod.Heavensward.Trial.T02Bismarck;

public enum OID : uint
{
    Boss = 0xEF8, // R6.000, Bismarck, untargetable all fight
    Helper = 0xEF9, // R0.500
    VaporBubble = 0xEFA, // R1.200
    LanMaiiVundu = 0xEFC, // R1.500
    VukMaiiVundu = 0xEFD, // R1.300
    SoSanuwa = 0xEFE, // R3.500
    FunnelCloud = 0xEFF, // R1.000, wanders across the deck
    UlSanuwa = 0xF00, // R3.500
    MagitekFieldGenerator = 0xF60, // R1.000
    ChitinCarapace = 0x1147, // R5.000, Part, first shell
    Corona = 0x1148, // R5.000, Part, second shell
    Dragonkiller = 0x12EB, // R1.000, armed cannon
    HelperB = 0x1214, // R0.500
    HelperC = 0x1303, // R1.000
    FieldGeneratorSwitch = 0x1E9A25, // EventObj
    DragonkillerNorth = 0x1E9A28, // EventObj
    DragonkillerSouth = 0x1E9A29, // EventObj
}

public enum AID : uint
{
    AutoAttackAdd = 872, // LanMaiiVundu/SoSanuwa/UlSanuwa->player, no cast, single-target
    BaleenBombVisual = 4011, // Boss->self, 3.0s cast, single-target, visual
    LightningBoltVisual = 4016, // Boss->self, 3.0s cast, single-target, visual
    MagickedBubble = 4017, // Boss->self, no cast, single-target, spawns Vapor Bubbles
    WhaleSong = 4019, // Boss->self, no cast, single-target
    Windcaller = 4021, // Boss->self, 3.0s cast, single-target, spawns Vuk'maii Vundu
    WaterCannon = 4024, // VaporBubble->self, no cast, range 3 circle
    Morrowmotes = 4026, // LanMaiiVundu/VukMaiiVundu->self, no cast, single-target
    PowerfulGust = 4029, // VukMaiiVundu->player, 1.0s cast, single-target
    DryFin = 4031, // SoSanuwa->self, no cast, range 9+R cone, hits the add's target
    WetFin = 4033, // UlSanuwa->self, no cast, range 9+R cone, hits the add's target
    Shrug = 4038, // Helper->player, no cast, single-target, throws everyone off the shell
    BismarckHit4039 = 4039, // Boss->self, no cast, single-target
    BismarckHit4040 = 4040, // Boss->self, no cast, single-target
    BismarckHit4041 = 4041, // Boss->self, no cast, single-target
    HardsilverChain = 4042, // Dragonkiller->self, no cast, single-target, harpoon fired
    FunnelCloudHit = 4765, // Helper->self, no cast, range 4 circle, players touching a Funnel Cloud
    GeneratorHit = 4778, // MagitekFieldGenerator->self, no cast, single-target
    ThunderheadVisual = 4906, // Boss->self, 3.0s cast, single-target, visual
    CetaceanRage = 4918, // Boss->self, 6.0s cast, range 50 circle
    BreachBlast = 4925, // Boss->self, no cast, range 50 circle
    HowlingWing = 4928, // LanMaiiVundu->self, 2.5s cast, range 30 width 6 rect
    ExtremeWind = 4929, // VukMaiiVundu->location, 2.5s cast, range 5 circle
    BaleenBomb = 4932, // Helper->location, 3.5s cast, range 5 circle
    VacuumWave = 4933, // Helper->self, no cast, range 100 circle, every ~6s during the Sanuwa phase
    AtmosphericDisruption = 4934, // Helper->self, no cast, every ~5s to players on the shell
    Thunderhead = 4935, // Helper->location, 3.0s cast, range 5 circle, placed on players
    LightningBolt = 4936, // Helper->location, 4.0s cast, range 4 circle, placed on players
    BreachBlastRepeat = 5062, // Helper->self, no cast, range 50 circle
    AtmosphericDisruptionLand = 5064, // Helper->player, no cast, single-target, on landing on the shell
}

public enum SID : uint
{
    VulnerabilityDown = 350, // none->SoSanuwa/UlSanuwa, extra=0x0, while the Sanuwas are linked
    Whaleback = 719, // none->player, extra=0x0, on Bismarck's back; the shells only take damage from players with it
}

public enum TetherID : uint
{
    SanuwaLink = 35, // SoSanuwa->UlSanuwa, when they come within ~15y of each other
    DragonkillerA = 39, // Dragonkiller->Boss
    DragonkillerB = 40, // Dragonkiller->Boss
}

class BaleenBomb(BossModule module) : Components.StandardAOEs(module, AID.BaleenBomb, 5);
class LightningBolt(BossModule module) : Components.StandardAOEs(module, AID.LightningBolt, 4);
class Thunderhead(BossModule module) : Components.StandardAOEs(module, AID.Thunderhead, 5);
class HowlingWing(BossModule module) : Components.StandardAOEs(module, AID.HowlingWing, new AOEShapeRect(30, 3));
class ExtremeWind(BossModule module) : Components.StandardAOEs(module, AID.ExtremeWind, 5);
class CetaceanRage(BossModule module) : Components.RaidwideCast(module, AID.CetaceanRage);
class PowerfulGust(BossModule module) : Components.SingleTargetCast(module, AID.PowerfulGust);
class DryFin(BossModule module) : Components.Cleave(module, AID.DryFin, new AOEShapeCone(12.5f, 45.Degrees()), (uint)OID.SoSanuwa);
class WetFin(BossModule module) : Components.Cleave(module, AID.WetFin, new AOEShapeCone(12.5f, 45.Degrees()), (uint)OID.UlSanuwa);

class SanuwaSeparation(BossModule module) : BossComponent(module)
{
    private const float MinDistance = 15;

    private Actor? Dry => Module.Enemies(OID.SoSanuwa).FirstOrDefault(a => !a.IsDead);
    private Actor? Wet => Module.Enemies(OID.UlSanuwa).FirstOrDefault(a => !a.IsDead);
    private bool Linked => Dry?.Tether.ID == (uint)TetherID.SanuwaLink;

    private Actor? OtherSanuwa(Actor tank)
    {
        if (Dry is not { } dry || Wet is not { } wet)
            return null;
        return dry.TargetID == tank.InstanceID ? wet : wet.TargetID == tank.InstanceID ? dry : null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (OtherSanuwa(actor) != null && (Linked || Dry!.Position.InCircle(Wet!.Position, MinDistance)))
            hints.Add("Pull the Sanuwas apart!");
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Dry != null && Wet != null)
            hints.Add(Linked ? "Sanuwas linked: they take less damage!" : "Keep the Sanuwas apart");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (OtherSanuwa(actor) is not { } other)
            return;
        hints.AddForbiddenZone(ShapeDistance.Circle(other.Position, MinDistance));
        if (hints.FindEnemy(other) is { } e)
        {
            e.Priority = AIHints.Enemy.PriorityUndesirable;
            e.ShouldBeTanked = false;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (OtherSanuwa(pc) is { } other)
            Arena.AddCircle(other.Position, MinDistance, ArenaColor.Danger);
    }
}

class FunnelCloud(BossModule module) : Components.GenericAOEs(module, AID.FunnelCloudHit)
{
    private static readonly AOEShapeCircle _shape = new(4.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Module.Enemies(OID.FunnelCloud).Select(c => new AOEInstance(_shape, c.Position));
}

class Whaleback(BossModule module) : BossComponent(module)
{
    private bool ShellUp => Module.Enemies(OID.ChitinCarapace).Any(s => s.IsTargetable) || Module.Enemies(OID.Corona).Any(s => s.IsTargetable);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ShellUp && actor.FindStatus(SID.Whaleback) == null)
            hints.Add("Get on Bismarck's back!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ShellUp && actor.FindStatus(SID.Whaleback) == null)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(new WPos(1.5f, -4.5f), new WPos(8.5f, -4.5f), 2.5f));
    }
}

class Interactables(BossModule module) : BossComponent(module)
{
    private IEnumerable<Actor> Usable(OID oid) => Module.Enemies(oid).Where(o => o.IsTargetable);
    private IEnumerable<Actor> Cannons => Usable(OID.DragonkillerNorth).Concat(Usable(OID.DragonkillerSouth));

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Usable(OID.FieldGeneratorSwitch).Any())
            hints.Add("Use the field generator!");
        if (Cannons.Any())
            hints.Add("Fire the Dragonkiller cannons!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Raid.WithoutSlot().Count() > 1)
            return;

        var cannons = Cannons.ToList();
        if (cannons.Count > 0)
        {
            hints.InteractWithTarget = cannons.MinBy(actor.DistanceToHitbox);
            if (cannons.Count == 1)
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
        }
        else if (Usable(OID.FieldGeneratorSwitch).FirstOrDefault() is { } generator)
        {
            hints.InteractWithTarget = generator;
        }
    }
}

class DeckArena(BossModule module) : BossComponent(module)
{
    private static readonly WPos[] _deck = [
        new(-17.5f, -18), new(-12.5f, -18), new(-11.5f, -14.5f), new(-10, -13.5f), new(-8, -12.5f), new(-4.5f, -11.5f), new(-4, -7.5f), new(-3.5f, -6),
        new(-0.5f, -3.5f), new(-0.5f, -1.5f), new(-2.5f, 0), new(-3.5f, 1.5f), new(-1, 4.5f), new(0, 6.5f), new(-2, 10), new(-8, 12),
        new(-10, 14), new(-11.5f, 14.5f), new(-11.5f, 18.5f), new(-17, 19.5f), new(-19, 18), new(-22, 16), new(-24, 13.5f), new(-26.5f, 11),
        new(-27, 7.5f), new(-28, 5), new(-28, 1.5f), new(-26.5f, 0), new(-26.5f, -7), new(-25.5f, -11.5f), new(-21, -12.5f), new(-20.5f, -14.5f),
        new(-19, -15.5f)];
    private static readonly WPos[] _shell = [new(-6, -10.5f), new(9.5f, -10.5f), new(9.5f, 1.5f), new(-6, 1.5f)];

    public static readonly WPos Center = new(-10, 0);
    public static readonly ArenaBoundsCustom DeckBounds = new(22, new(Rel(_deck)));
    public static readonly ArenaBoundsCustom ShellBounds = new(22, DeckBounds.Clipper.Union(new(Rel(_deck)), new(Rel(_shell))));

    private static List<WDir> Rel(WPos[] points) => [.. points.Select(p => p - Center)];

    public override void Update()
    {
        var shellUp = Module.Enemies(OID.Dragonkiller).Any(d => d.Tether.ID != 0);
        var bounds = shellUp ? ShellBounds : DeckBounds;
        if (!ReferenceEquals(Arena.Bounds, bounds))
            Arena.Bounds = bounds;
    }
}

class BismarckAdds(BossModule module) : Components.AddsMulti(module, [OID.LanMaiiVundu, OID.VukMaiiVundu, OID.SoSanuwa, OID.UlSanuwa, OID.VaporBubble, OID.ChitinCarapace, OID.Corona], 1);

class T02BismarckStates : StateMachineBuilder
{
    public T02BismarckStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeckArena>()
            .ActivateOnEnter<BaleenBomb>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<Thunderhead>()
            .ActivateOnEnter<HowlingWing>()
            .ActivateOnEnter<ExtremeWind>()
            .ActivateOnEnter<CetaceanRage>()
            .ActivateOnEnter<PowerfulGust>()
            .ActivateOnEnter<DryFin>()
            .ActivateOnEnter<WetFin>()
            .ActivateOnEnter<SanuwaSeparation>()
            .ActivateOnEnter<FunnelCloud>()
            .ActivateOnEnter<Interactables>()
            .ActivateOnEnter<Whaleback>()
            .ActivateOnEnter<BismarckAdds>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 88, NameID = 3649)]
public class T02Bismarck(ModuleInit init) : BossModule(init, DeckArena.Center, DeckArena.DeckBounds)
{
    protected override bool CheckPull() => Raid.WithoutSlot().Any(p => p.InCombat);
}
