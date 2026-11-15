namespace BossMod.Endwalker.Raid.P3NPhoinix;

public enum OID : uint
{
    Boss = 0x353B, // R5.980, x1
    Helper = 0x233C, // R0.500, x45, Helper type
    Sparkfledged = 0x353C, // R1.500, x4
    Sunbird = 0x353E, // R1.800, x4
    SparkfledgedBig = 0x37FF, // R2.300, x4, Part type
    DarkenedFire = 0x3858, // R2.000, x4
    ActorFE36F = 0xFE36F, // R0.500, x1, EventNpc type
    ActorFE370 = 0xFE370, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1EA1A1 = 0x1EA1A1, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    ExperimentalFireplumeSingle = 26262, // Boss->self, 5.0s cast, single-target
    FireplumeSingle = 26263, // Helper->self, 6.0s cast, range 15 circle
    ExperimentalFireplumeMulti = 26264, // Boss->self, 5.0s cast, single-target
    FireplumeMulti = 26265, // Helper->self, 5.0s cast, range 10 circle
    ExperimentalCharplume = 26268, // Boss->self, 3.0s cast, single-target
    Charplume = 26269, // Helper->player, 5.0s cast, range 6 circle spread
    DevouringBrand = 26270, // Boss->self, 3.0s cast, single-target
    DevouringBrandFirst = 26271, // Helper->self, 3.0s cast, range 2 width 5 rect
    DevouringBrandRest = 26272, // Helper->self, no cast, range 4 width 5 rect
    DevouringBrandEnd = 26273, // Helper->self, no cast, range 40 width 10 cross
    BlazingRain = 26274, // Helper->self, no cast, range 60 circle
    FlamesOfUndeath = 26275, // Boss->self, no cast, range 60 circle
    JointPyre = 26277, // Sparkfledged->self, no cast, range 9 circle, hits other sparkfledged that are too close
    ForeCarve = 26279, // Sunbird->self, 4.0s cast, range 15 180-degree cone
    RearCarve = 26280, // Sunbird->self, 4.0s cast, range 15 180-degree cone
    DeadRebirth = 26281, // Boss->self, 10.0s cast, range 60 circle
    FledglingFlight = 26282, // Boss->self, 3.0s cast, single-target
    AshenEye = 26283, // Sparkfledged->self, 7.0s cast, range 60 90-degree cone
    Ability26284 = 26284, // Boss->self, no cast, single-target
    TrailOfCondemnation = 26285, // Boss->self, 3.5s cast, single-target
    TrailOfCondemnationAOE = 26287, // Helper->self, 4.5s cast, range 40 width 15 rect
    FlareOfCondemnation = 26288, // Helper->player, 5.0s cast, range 6 circle spread
    HeatOfCondemnation = 26290, // Boss->self, 5.0s cast, single-target
    HeatOfCondemnationAOE = 26291, // Helper->player, 6.0s cast, range 6 circle tankbuster
    RightCinderwing = 26292, // Boss->self, 5.0s cast, range 60 180-degree cone
    LeftCinderwing = 26293, // Boss->self, 5.0s cast, range 60 180-degree cone
    SearingBreeze = 26294, // Boss->self, 3.0s cast, single-target
    SearingBreezeAOE = 26295, // Helper->self, 3.0s cast, range 6 circle
    ScorchedExaltation = 26296, // Boss->self, 5.0s cast, range 60 circle
    AutoAttackSunbird = 27916, // Sunbird->player, no cast, single-target
    DevouringBrandCross = 28034, // Helper->self, 14.5s cast, range 40 width 10 cross
    AutoAttackSparkfledged = 28124, // SparkfledgedBig->player, no cast, single-target
    DarkenedFire = 28378, // Boss->self, 6.0s cast, single-target
    DarkenedBlaze = 28379, // DarkenedFire->self, 15.0s cast, range 60 circle
    BrightenedFire = 28380, // Boss->self, 5.0s cast, single-target
    BrightenedFireAOE = 28381, // Boss->players, no cast, range 7 circle, kills darkened fires it hits
    DeadRebirthEmpowered = 28382, // Boss->self, 10.0s cast, range 60 circle, replaces DeadRebirth after sparkfledged link up
}

public enum SID : uint
{
    JointPyre = 2550, // Sparkfledged->Sparkfledged, extra=number of links
}

public enum IconID : uint
{
    Charplume = 139, // player
    HeatOfCondemnation = 218, // player
    BrightenedFire1 = 268, // player
    BrightenedFire2 = 269, // player
    BrightenedFire3 = 270, // player
    BrightenedFire4 = 271, // player
}

class FireplumeSingle(BossModule module) : Components.StandardAOEs(module, AID.FireplumeSingle, 15);
class FireplumeMulti(BossModule module) : Components.StandardAOEs(module, AID.FireplumeMulti, 10);
class Charplume(BossModule module) : Components.SpreadFromCastTargets(module, AID.Charplume, 6);
class DevouringBrandFirst(BossModule module) : Components.StandardAOEs(module, AID.DevouringBrandFirst, new AOEShapeRect(2, 2.5f));
class DevouringBrandCross(BossModule module) : Components.StandardAOEs(module, AID.DevouringBrandCross, new AOEShapeCross(40, 5));
class ForeCarve(BossModule module) : Components.StandardAOEs(module, AID.ForeCarve, new AOEShapeCone(15, 90.Degrees()));
class RearCarve(BossModule module) : Components.StandardAOEs(module, AID.RearCarve, new AOEShapeCone(15, 90.Degrees()));
class DeadRebirth(BossModule module) : Components.RaidwideCast(module, AID.DeadRebirth);
class DeadRebirthEmpowered(BossModule module) : Components.RaidwideCast(module, AID.DeadRebirthEmpowered, "Raidwide (sparkfledged linked)");
class AshenEye(BossModule module) : Components.StandardAOEs(module, AID.AshenEye, new AOEShapeCone(60, 45.Degrees()));
class TrailOfCondemnation(BossModule module) : Components.StandardAOEs(module, AID.TrailOfCondemnationAOE, new AOEShapeRect(40, 7.5f));
class FlareOfCondemnation(BossModule module) : Components.SpreadFromCastTargets(module, AID.FlareOfCondemnation, 6);
class HeatOfCondemnation(BossModule module) : Components.BaitAwayCast(module, AID.HeatOfCondemnationAOE, new AOEShapeCircle(6), true);
class RightCinderwing(BossModule module) : Components.StandardAOEs(module, AID.RightCinderwing, new AOEShapeCone(60, 90.Degrees()));
class LeftCinderwing(BossModule module) : Components.StandardAOEs(module, AID.LeftCinderwing, new AOEShapeCone(60, 90.Degrees()));
class SearingBreezeAOE(BossModule module) : Components.StandardAOEs(module, AID.SearingBreezeAOE, 6);
class ScorchedExaltation(BossModule module) : Components.RaidwideCast(module, AID.ScorchedExaltation);
class DarkenedBlaze(BossModule module) : Components.RaidwideCast(module, AID.DarkenedBlaze, "Raidwide (kill darkened fires with brightened fire)");

class SunbirdSpread(BossModule module) : BossComponent(module)
{
    private const float LinkRange = 10.5f;
    private const float TargetSlack = 3;

    private readonly IReadOnlyList<Actor> _birds = module.Enemies(OID.Sunbird);

    private bool TooClose(Actor bird) => _birds.Any(o => o != bird && o.Position.InCircle(bird.Position, LinkRange));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_birds.Any(b => !b.IsDead && b.TargetID == actor.InstanceID && TooClose(b)))
            hints.Add("Drag bird away from other birds!");
        else if (_birds.Any(b => !b.IsDead && TooClose(b)))
            hints.Add("Kill birds apart!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bird in _birds.Where(b => !b.IsDead && b.TargetID == actor.InstanceID))
            foreach (var other in _birds.Where(o => o != bird && (o.IsDead || o.TargetID != actor.InstanceID)))
                hints.AddForbiddenZone(ShapeDistance.Circle(other.Position, LinkRange + TargetSlack));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bird in _birds)
            Arena.AddCircle(bird.Position, LinkRange / 2, TooClose(bird) ? ArenaColor.Danger : ArenaColor.Safe);
        Arena.Actors(_birds.Where(b => !b.IsDead), ArenaColor.Enemy);
    }
}

class BrightenedFire(BossModule module) : Components.CastCounter(module, AID.BrightenedFireAOE)
{
    private const float Radius = 7;
    private readonly int[] _order = new int[PartyState.MaxPartySize];
    private readonly IReadOnlyList<Actor> _fires = module.Enemies(OID.DarkenedFire);

    private IEnumerable<Actor> LiveFires => _fires.Where(f => !f.IsDead);
    private bool Pending(int slot) => _order[slot] > NumCasts;
    private int NumHit(WPos pos) => LiveFires.Count(f => f.Position.InCircle(pos, Radius + f.HitboxRadius));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Pending(slot))
        {
            var hit = NumHit(actor.Position);
            if (hit == 0)
                hints.Add($"Fire #{_order[slot]}: get darkened fires into your circle!");
            else if (hit == 1 && LiveFires.Count() > 1)
                hints.Add($"Fire #{_order[slot]}: stand between two darkened fires!", false);
        }
        if (Raid.WithSlot().Exclude(actor).Any(ip => Pending(ip.Item1) && actor.Position.InCircle(ip.Item2.Position, Radius)))
            hints.Add("GTFO from brightened fire!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Pending(slot))
            hints.GoalZones.Add(AIHints.GoalAOECircle([.. LiveFires.Select(f => (f.Position, f.HitboxRadius))], Radius));
        foreach (var (i, p) in Raid.WithSlot().Exclude(actor))
            if (Pending(i))
                hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, Radius));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NumCasts >= 4 || !_order.Any(o => o > 0))
            return;
        Arena.Actors(LiveFires, ArenaColor.Enemy);
        if (Pending(pcSlot))
            Arena.AddCircle(pc.Position, Radius, NumHit(pc.Position) > 0 ? ArenaColor.Safe : ArenaColor.Danger);
        foreach (var (i, p) in Raid.WithSlot().Exclude(pc))
            if (Pending(i))
                Arena.AddCircle(p.Position, Radius, ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID is >= IconID.BrightenedFire1 and <= IconID.BrightenedFire4 && Raid.TryFindSlot(actor, out var slot))
            _order[slot] = (int)iconID - (int)IconID.BrightenedFire1 + 1;
    }
}

class P3NPhoinixStates : StateMachineBuilder
{
    public P3NPhoinixStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FireplumeSingle>()
            .ActivateOnEnter<FireplumeMulti>()
            .ActivateOnEnter<Charplume>()
            .ActivateOnEnter<DevouringBrandFirst>()
            .ActivateOnEnter<DevouringBrandCross>()
            .ActivateOnEnter<ForeCarve>()
            .ActivateOnEnter<RearCarve>()
            .ActivateOnEnter<DeadRebirth>()
            .ActivateOnEnter<DeadRebirthEmpowered>()
            .ActivateOnEnter<AshenEye>()
            .ActivateOnEnter<TrailOfCondemnation>()
            .ActivateOnEnter<FlareOfCondemnation>()
            .ActivateOnEnter<HeatOfCondemnation>()
            .ActivateOnEnter<RightCinderwing>()
            .ActivateOnEnter<LeftCinderwing>()
            .ActivateOnEnter<SearingBreezeAOE>()
            .ActivateOnEnter<ScorchedExaltation>()
            .ActivateOnEnter<DarkenedBlaze>()
            .ActivateOnEnter<SunbirdSpread>()
            .ActivateOnEnter<BrightenedFire>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 806, NameID = 10720)]
public class P3NPhoinix(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
