namespace BossMod.Dawntrail.Dungeon.D12Mistwake.D121TrenoCatoblepas;

public enum OID : uint
{
    Boss = 0x4841, // R4.500, x1
    Helper = 0x233C, // R0.500, x15, Helper type
    SmallRock = 0x4842,
    MediumRock = 0x4843,
    BigRock = 0x4851,
}

public enum AID : uint
{
    AutoAttack = 43328, // Boss->player, no cast, single-target
    Earthquake = 43327, // Boss->self, 5.0s cast, range 30 circle
    ThunderIICast = 43331, // Boss->self, 3.5+1.5s cast, single-target
    ThunderIIGround = 43332, // Helper->location, 5.0s cast, range 5 circle
    ThunderIIPlayer = 43333, // Helper->players, 5.0s cast, range 5 circle
    BedevilingLight = 43330, // Boss->self, 7.0s cast, range 30 circle
    ThunderIII = 43329, // Boss->player, 5.0s cast, range 4 circle
    RayOfLightningCast = 44825, // Boss->player, 6.0s cast, single-target
    RayOfLightning = 43334, // Boss->self, no cast, range 50 width 5 rect
    Petribreath = 43335, // Boss->self, 5.0s cast, range 30 120-degree cone
}

public enum IconID : uint
{
    Spread = 641, // player->self
    Tankbuster = 342, // player->self
    ShareLaser = 524, // Boss->player
}

class Earthquake(BossModule module) : Components.RaidwideCast(module, AID.Earthquake);
class Rocks(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor Rock, float Radius)> TheRocks = [];

    // small rock 1y
    // medium rock 1.5y
    // big rock 2
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (rock, radius) in TheRocks)
            hints.TemporaryObstacles.Add(ShapeContains.Circle(rock.Position, radius + 0.5f));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (rock, radius) in TheRocks)
            Arena.AddCircle(rock.Position, radius, ArenaColor.Object);
    }

    public override void OnActorCreated(Actor actor)
    {
        var radius = (OID)actor.OID switch
        {
            OID.SmallRock => 1,
            OID.MediumRock => 1.5f,
            OID.BigRock => 2,
            _ => 0
        };
        if (radius > 0)
            TheRocks.Add((actor, radius));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        TheRocks.RemoveAll(r => r.Rock == actor);
    }
}

class Petribreath(BossModule module) : Components.StandardAOEs(module, AID.Petribreath, new AOEShapeCone(30, 60.Degrees()));
class ThunderIIAOE(BossModule module) : Components.StandardAOEs(module, AID.ThunderIIGround, 5);
class ThunderIISpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.ThunderIIPlayer, 5)
{
    private readonly Rocks _rocksComponent = module.FindComponent<Rocks>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_rocksComponent.TheRocks.Count is > 0 and < 5 && Spreads.FirstOrNull(s => s.Target == actor) is { } s)
            foreach (var (rock, radius) in _rocksComponent.TheRocks)
                hints.AddForbiddenZone(ShapeContains.Circle(rock.Position, radius + 5), s.Activation);
    }
}
class ThunderIII(BossModule module) : Components.BaitAwayCast(module, AID.ThunderIII, new AOEShapeCircle(4), true, true);

class RayOfLightning(BossModule module) : Components.IconLineStack(module, 2.5f, 50, (uint)IconID.ShareLaser, AID.RayOfLightning, 6.2f);

class BedevilingLight(BossModule module) : Components.GenericLineOfSightAOE(module, AID.BedevilingLight, 50, false)
{
    private readonly Rocks _rocksComponent = module.FindComponent<Rocks>()!;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Modify(spell.LocXZ, _rocksComponent.TheRocks.Select(r => (r.Rock.Position, r.Radius)), Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Modify(null, []);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Origin != null && Visibility.Count == 0)
            hints.Add("Good luck, buddy", false);
        else
            base.AddHints(slot, actor, hints);
    }
}

class D121TrenoCatoblepasStates : StateMachineBuilder
{
    public D121TrenoCatoblepasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rocks>()
            .ActivateOnEnter<Earthquake>()
            .ActivateOnEnter<Petribreath>()
            .ActivateOnEnter<ThunderIIAOE>()
            .ActivateOnEnter<ThunderIISpread>()
            .ActivateOnEnter<ThunderIII>()
            .ActivateOnEnter<BedevilingLight>()
            .ActivateOnEnter<RayOfLightning>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1064, NameID = 14270)]
public class D121TrenoCatoblepas(WorldState ws, Actor primary) : BossModule(ws, primary, new(84, 370), new ArenaBoundsSquare(19.5f));
