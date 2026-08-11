namespace BossMod.Shadowbringers.Dungeon.D02DohnMheg.D022Griaule;

public enum OID : uint
{
    Boss = 0x98E, // R3.180-8.268, x1
    PaintedRoot = 0xF08, // R1.480, x8 (spawn during fight)
    PaintedSapling = 0xEFB, // R0.900, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    Rake = 10355, // Boss->player, no cast, single-target
    Swinge = 8906, // Boss->self, 4.0s cast, range 50+R 60-degree cone
    Fodder = 8897, // Boss->self, 5.0s cast, single-target
    Tiiimbeeer = 8915, // Boss->self, 6.0s cast, range 50 circle
    FeedingTime = 8899, // PaintedSapling->player/Boss, no cast, single-target
    CoilingIvy = 8901, // Boss->self, 3.0s cast, single-target
}

public enum SID : uint
{
    GrowingPlayer = 383, // PaintedSapling->player, extra=0x1
    GrowingBoss = 390, // PaintedSapling->Boss, extra=0x2/0x4
    Fetters = 1153, // none->player, extra=0x0
    FullGrown = 391, // PaintedSapling->player, extra=0x1
}

public enum TetherID : uint
{
    FeedingTime = 84, // EFB->Boss/player
}

class Swinge(BossModule module) : Components.StandardAOEs(module, AID.Swinge, new AOEShapeCone(50, 30.Degrees()));

// these aren't "real" tethers, they're basically charge AOEs (except that you have to go closer to the middle of the rect to intercept it)
class FeedingTime(BossModule module) : BossComponent(module)
{
    readonly List<(Actor Source, Actor Target)> _tethers = [];

    DateTime _deadline;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.FeedingTime && WorldState.Actors.Find(tether.Target) is { } target)
        {
            if (_tethers.Count == 0)
                // Feeding Time is cast ~8.1s after the first tether appears, let's account for latency and tether jank etc
                _deadline = WorldState.FutureTime(7);

            _tethers.Add((source, target));
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.FeedingTime)
            _tethers.RemoveAll(t => t.Source == source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var hasTether = _tethers.Any(t => t.Target == pc);

        foreach (var (src, target) in _tethers)
        {
            if (target == pc)
                Arena.AddLine(src.Position, target.Position, ArenaColor.Safe);
            else if (target.IsAlly)
                Arena.AddLine(src.Position, target.Position, ArenaColor.Danger);
            else
                Arena.AddLine(src.Position, target.Position, ArenaColor.Danger, hasTether ? 1 : 2);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tethers.Any(t => !t.Target.IsAlly) && !_tethers.Any(t => t.Target == actor))
            hints.Add("Grab a tether!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        List<Func<WPos, float>> zones = [];

        foreach (var (src, target) in _tethers)
        {
            if (target == actor)
            {
                zones.Clear();
                // don't move away
                zones.Add(ShapeDistance.InvertedRect(src.Position, Module.PrimaryActor.Position, 2));
                break;
            }
            else if (!target.IsAlly)
                zones.Add(ShapeDistance.InvertedRect(src.Position, target.Position, 1));
        }

        if (zones.Count > 0)
        {
            hints.AddForbiddenZone(ShapeDistance.Intersection(zones), _deadline);

            // the boss gets the tether if you stand in its hitbox
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius), _deadline);
            // stay at least 2y away from saplings or something i guess
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 18), _deadline);
        }
    }
}

class Tiiimbeeer(BossModule module) : Components.RaidwideCast(module, AID.Tiiimbeeer);

class PaintedRoot(BossModule module) : Components.Adds(module, (uint)OID.PaintedRoot, 1, true);

class D022GriauleStates : StateMachineBuilder
{
    public D022GriauleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Swinge>()
            .ActivateOnEnter<FeedingTime>()
            .ActivateOnEnter<PaintedRoot>()
            .ActivateOnEnter<Tiiimbeeer>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 649, NameID = 8143)]
public class D022Griaule(WorldState ws, Actor primary) : BossModule(ws, primary, new(7.17f, -339.12f), new ArenaBoundsCircle(24.5f));

