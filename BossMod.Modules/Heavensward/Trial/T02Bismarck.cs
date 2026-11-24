namespace BossMod.Heavensward.Trial.T02Bismarck;

public enum OID : uint
{
    Boss = 0xEF8, // R6.000
    Helper = 0xEF9, // R0.500
    VaporBubble = 0xEFA, // R1.200
    LanMaiiVundu = 0xEFC, // R1.500
    VukMaiiVundu = 0xEFD, // R1.300
    SoSanuwa = 0xEFE, // R3.500
    FunnelCloud = 0xEFF, // R1.000
    UlSanuwa = 0xF00, // R3.500
    MagitekFieldGenerator = 0xF60, // R1.000
    ChitinCarapace = 0x1147, // R5.000, Part
    Corona = 0x1148, // R5.000, Part
    Dragonkiller = 0x12EB, // R1.000
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
    MagickedBubble = 4017, // Boss->self, no cast, single-target
    WhaleSong = 4019, // Boss->self, no cast, single-target
    Windcaller = 4021, // Boss->self, 3.0s cast, single-target
    WaterCannon = 4024, // VaporBubble->self, no cast, range 3 circle
    Morrowmotes = 4026, // LanMaiiVundu/VukMaiiVundu->self, no cast, single-target
    PowerfulGust = 4029, // VukMaiiVundu->player, 1.0s cast, single-target
    DryFin = 4031, // SoSanuwa->self, no cast, range 9+R cone
    WetFin = 4033, // UlSanuwa->self, no cast, range 9+R cone
    Shrug = 4038, // Helper->player, no cast, single-target
    BismarckHit4039 = 4039, // Boss->self, no cast, single-target
    BismarckHit4040 = 4040, // Boss->self, no cast, single-target
    BismarckHit4041 = 4041, // Boss->self, no cast, single-target
    HardsilverChain = 4042, // Dragonkiller->self, no cast, single-target
    FunnelCloudHit = 4765, // Helper->self, no cast, range 4 circle
    GeneratorHit = 4778, // MagitekFieldGenerator->self, no cast, single-target
    ThunderheadVisual = 4906, // Boss->self, 3.0s cast, single-target, visual
    CetaceanRage = 4918, // Boss->self, 6.0s cast, range 50 circle
    BreachBlast = 4925, // Boss->self, no cast, range 50 circle
    HowlingWing = 4928, // LanMaiiVundu->self, 2.5s cast, range 30 width 6 rect
    ExtremeWind = 4929, // VukMaiiVundu->location, 2.5s cast, range 5 circle
    BaleenBomb = 4932, // Helper->location, 3.5s cast, range 5 circle
    VacuumWave = 4933, // Helper->self, no cast, range 100 circle
    AtmosphericDisruption = 4934, // Helper->self, no cast
    Thunderhead = 4935, // Helper->location, 3.0s cast, range 5 circle
    LightningBolt = 4936, // Helper->location, 4.0s cast, range 4 circle
    BreachBlastRepeat = 5062, // Helper->self, no cast, range 50 circle
    AtmosphericDisruptionLand = 5064, // Helper->player, no cast, single-target
}

public enum SID : uint
{
    VulnerabilityDown = 350, // none->SoSanuwa/UlSanuwa
    Whaleback = 719, // none->player
}

public enum TetherID : uint
{
    SanuwaLink = 35, // SoSanuwa->UlSanuwa
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
    private readonly Dictionary<ulong, ulong> _gunners = [];

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
        var party = Raid.WithoutSlot().Where(p => !p.IsDead).ToList();
        var cannons = Cannons.OrderBy(c => c.InstanceID).ToList();
        foreach (var stale in _gunners.Keys.Where(k => !cannons.Any(c => c.InstanceID == k)).ToList())
            _gunners.Remove(stale);
        if (cannons.Count > 0)
        {
            if (party.Count <= 1)
            {
                hints.InteractWithTarget = cannons.MinBy(actor.DistanceToHitbox);
                if (cannons.Count == 1)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
                return;
            }

            foreach (var cannon in cannons)
            {
                if (_gunners.TryGetValue(cannon.InstanceID, out var id) && party.Any(p => p.InstanceID == id))
                    continue;
                var gunner = party.Where(p => !_gunners.ContainsValue(p.InstanceID)).MinBy(cannon.DistanceToHitbox);
                if (gunner != null)
                    _gunners[cannon.InstanceID] = gunner.InstanceID;
            }
            if (cannons.FirstOrDefault(c => _gunners.GetValueOrDefault(c.InstanceID) == actor.InstanceID) is { } mine)
            {
                hints.InteractWithTarget = mine;
                hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), actor, ActionQueue.Priority.High);
            }
        }
        else if (party.Count <= 1 && Usable(OID.FieldGeneratorSwitch).FirstOrDefault() is { } generator)
        {
            hints.InteractWithTarget = generator;
        }
    }
}

class DeckArena(BossModule module) : BossComponent(module)
{
    public static readonly WPos Center = new(-9, -3.5f);
    public static readonly ArenaBoundsCustom DeckBounds = new(28, new([new RelPolygonWithHoles([
        new(-5.14f, -16), new(-1.56f, -16), new(-1.28f, -13.78f), new(2.45f, -11.2f), new(5.31f, -10.43f), new(6.47f, -8.7f), new(6.93f, -6.36f), new(7.44f, -6.15f),
        new(8.55f, -5.4f), new(8.9f, -4.1f), new(9.6f, -3.5f), new(10.5f, -2.4f), new(10.7f, -1.5f), new(10.6f, 0.5f), new(10.3f, 1.6f), new(10, 3.55f), new(7.22f, 15.44f), new(4.59f, 19.91f),
        new(2.48f, 20.7f), new(-2.07f, 24.24f), new(-7.32f, 24.5f), new(-12.81f, 22.67f), new(-13.5f, 21.75f), new(-15.88f, 21.75f), new(-19.71f, 19.56f), new(-20.22f, 16.17f),
        new(-22.25f, 12.38f), new(-22.25f, 9.54f), new(-21.76f, 9.06f), new(-22.01f, 5.66f), new(-21.01f, 3.42f), new(-21.26f, 1.49f), new(-20.48f, -3.13f), new(-19.19f, -6.5f),
        new(-18.23f, -7.68f), new(-17.99f, -10.67f), new(-16.85f, -12.37f), new(-13.59f, -14.28f), new(-12.48f, -14.05f), new(-11.57f, -15.43f), new(-6.97f, -16.78f)])]));
    public static readonly ArenaBoundsCustom ShellBounds = new(28, new([new RelPolygonWithHoles([
        new(13.11f, -19.5f), new(17.84f, -19.5f), new(20.15f, -18.63f), new(20.94f, -16.25f), new(19.59f, -16.25f), new(19.45f, -15.92f), new(21.37f, -15.71f), new(22.11f, -13.5f),
        new(21, -12.75f), new(22.25f, -12.75f), new(22.25f, -10.89f), new(15.92f, -12.89f), new(16.14f, -12.36f), new(19.4f, -10.49f), new(23.09f, -10.21f), new(23.8f, -7.9f), new(23.4f, -6.3f),
        new(23.05f, -5.15f), new(22.29f, -4.14f), new(23, -1.57f), new(23, 1), new(21.86f, 1), new(22.75f, 2.35f), new(22.75f, 3.67f), new(21.25f, 3.92f),
        new(22.06f, 5.16f), new(20.95f, 8.49f), new(18.43f, 11.84f), new(17.06f, 11.38f), new(16.78f, 9.73f), new(16.01f, 8.96f), new(14.62f, 8.68f), new(14.39f, 8),
        new(13.61f, 8), new(13.36f, 7), new(13.01f, 7), new(11.75f, 5.1f), new(11.08f, 4.89f), new(10.74f, 3.53f), new(11.21f, -0.52f), new(10.54f, -0.9f),
        new(10.78f, 1.25f), new(10, 1.77f), new(10, 3.55f), new(7.22f, 15.44f), new(4.59f, 19.91f), new(2.48f, 20.7f), new(-2.07f, 24.24f), new(-7.32f, 24.5f),
        new(-12.81f, 22.67f), new(-13.5f, 21.75f), new(-15.88f, 21.75f), new(-19.71f, 19.56f), new(-20.22f, 16.17f), new(-22.25f, 12.38f), new(-22.25f, 9.54f), new(-21.76f, 9.06f),
        new(-22.01f, 5.66f), new(-21.01f, 3.42f), new(-21.26f, 1.49f), new(-20.48f, -3.13f), new(-19.19f, -6.5f), new(-18.23f, -7.68f), new(-17.99f, -10.67f), new(-16.85f, -12.37f),
        new(-13.59f, -14.28f), new(-12.48f, -14.05f), new(-11.57f, -15.43f), new(-6.97f, -16.78f), new(-5.14f, -16), new(-1.56f, -16), new(-1.28f, -13.78f), new(2.45f, -11.2f),
        new(4.25f, -10.71f), new(4.25f, -11.65f), new(10.56f, -20.63f), new(6.47f, -8.7f), new(6.93f, -6.36f), new(7.44f, -6.15f), new(7.02f, -8.07f), new(5.92f, -10.05f), new(5.51f, -10.11f)], [76])]));

    public override void Update()
    {
        var shellUp = Module.Enemies(OID.ChitinCarapace).Any(s => s.IsTargetable) || Module.Enemies(OID.Corona).Any(s => s.IsTargetable);
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
