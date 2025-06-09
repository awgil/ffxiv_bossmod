namespace BossMod.Shadowbringers.Alliance.A31KnaveOfHearts;

public enum OID : uint
{
    Boss = 0x31B8, // R30.000, x1
    Helper = 0x233C, // R0.500, x15, Helper type
    CopiedKnave = 0x322C, // R30.000, x2
    Spheroid = 0x322D, // R1.000, x15
    Energy = 0x322E, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 24668, // Boss->player, no cast, single-target
    Roar = 24245, // Boss->self, 5.0s cast, range 80 circle
    ColossalImpactCastFast = 24228, // Boss/CopiedKnave->self, 7.0s cast, single-target
    ColossalImpact1 = 24229, // Helper->self, 8.0s cast, range 61 width 20 rect
    ColossalImpact2 = 24230, // Helper->self, 8.0s cast, range 61 width 20 rect
    ColossalImpact3 = 24231, // Boss/CopiedKnave->self, 8.0s cast, range 61 width 20 rect
    ColossalImpactCastSlow = 23805, // Boss->self, 8.5s cast, single-target
    ColossalImpactSlow1 = 24774, // Helper->self, 9.5s cast, range 61 width 20 rect
    ColossalImpactSlow2 = 24775, // Helper->self, 9.5s cast, range 61 width 20 rect
    ColossalImpactSlow3 = 24776, // Boss->self, 9.5s cast, range 61 width 20 rect
    MagicArtilleryBetaCast = 24242, // Boss->self, 3.0s cast, single-target
    MagicArtilleryBeta = 24243, // Helper->player, 5.0s cast, range 3 circle
    Replicate = 24233, // Boss->self, 3.0s cast, single-target
    StackingTheDeckClone = 24816, // CopiedKnave->self, 6.0s cast, single-target
    StackingTheDeck = 23801, // Boss->self, 6.0s cast, single-target
    Spheroids = 24232, // Boss->self, 4.0s cast, single-target
    KnavishBullets = 24237, // Spheroid->self, no cast, single-target
    MagicArtilleryAlphaCast = 24234, // Boss->self, 3.0s cast, single-target
    MagicArtilleryAlpha = 24235, // Helper->players, 6.0s cast, range 5 circle
    Burst = 24244, // Energy->player, no cast, single-target
    LightLeapCast = 24238, // Boss->self, 7.0s cast, single-target
    LightLeap = 24239, // Helper->location, 8.5s cast, range 40 circle
    BlockSpawn = 24240, // Helper->self, 4.0s cast, range 8 width 8 rect
    Lunge = 24241, // Boss/CopiedKnave->self, 8.0s cast, range 61 width 60 rect
    MagicBarrage = 24236, // Spheroid->self, 6.0s cast, range 61 width 5 rect
}

public enum TetherID : uint
{
    CopiedTether = 152, // CopiedKnave->Boss
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
    Order1 = 278, // Boss->self
    Order2 = 279, // CopiedKnave->self
    Order3 = 280, // CopiedKnave->self
    Spread = 169, // player->self
}

class ColossalImpact(BossModule module) : Components.GroupedAOEs(module, [AID.ColossalImpact1, AID.ColossalImpact2, AID.ColossalImpact3, AID.ColossalImpactSlow1, AID.ColossalImpactSlow2, AID.ColossalImpactSlow3], new AOEShapeRect(61, 10));

class MagicArtilleryAlpha(BossModule module) : Components.SpreadFromCastTargets(module, AID.MagicArtilleryAlpha, 5);
class LightLeap(BossModule module) : Components.StandardAOEs(module, AID.LightLeap, new AOEShapeCircle(25));
class Roar(BossModule module) : Components.RaidwideCast(module, AID.Roar);
class MagicArtilleryBeta(BossModule module) : Components.BaitAwayCast(module, AID.MagicArtilleryBeta, new AOEShapeCircle(3), centerAtTarget: true, endsOnCastEvent: true);
class Blocks(BossModule module) : Components.GenericAOEs(module, AID.BlockSpawn)
{
    private readonly Dictionary<ulong, List<WPos>> _blocks = [];
    private readonly List<Actor> _pendingBlocks = [];

    public IEnumerable<WPos> BlockCenters => _blocks.SelectMany(b => b.Value);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in _pendingBlocks)
            yield return new AOEInstance(new AOEShapeRect(4, 4, 4), p.Position, default, Module.CastFinishAt(p.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _pendingBlocks.Add(caster);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EA1A1)
        {
            if (state == 0x00010002)
            {
                _blocks[actor.InstanceID] = [.. _pendingBlocks.Select(p => p.Position)];
                _pendingBlocks.Clear();
            }
            if (state == 0x00040008)
                _blocks.Remove(actor.InstanceID);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var b in BlockCenters)
            Arena.AddRect(b, new WDir(0, 1), 4, 4, 4, ArenaColor.Border, 2 * Arena.Config.ThicknessScale);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var b in BlockCenters)
            hints.TemporaryObstacles.Add(ShapeContains.Rect(b, new WDir(0, 1), 4.5f, 4.5f, 4.5f));
    }
}
class Lunge(BossModule module) : Components.Knockback(module, AID.Lunge)
{
    private readonly List<Actor> _casters = [];
    private readonly Blocks _blocks = module.FindComponent<Blocks>()!;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(c.Position, CalcDistance(actor, c.Rotation), Module.CastFinishAt(c.CastInfo), Direction: c.Rotation, Kind: Kind.DirForward);
    }

    private float CalcDistance(Actor actor, Angle direction)
    {
        foreach (var b in _blocks.BlockCenters)
        {
            var dir = direction.ToDirection();
            var dist = Intersect.RaySegment(actor.Position - b, dir, dir.OrthoL() * 4, dir.OrthoR() * 4);
            if (dist < float.MaxValue)
                return dist - 4;
        }
        return 60;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_casters.FirstOrDefault() is { } source && !IsImmune(slot, Module.CastFinishAt(source.CastInfo)))
        {
            var safeRects = ShapeContains.Union([.. _blocks.BlockCenters.Select(c => ShapeContains.Rect(c, source.Rotation + 180.Degrees(), 60, 0, 4))]);
            hints.AddForbiddenZone(p => !safeRects(p), Module.CastFinishAt(source.CastInfo));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _casters.Remove(caster);
        }
    }
}
class MagicBarrage(BossModule module) : Components.StandardAOEs(module, AID.MagicBarrage, new AOEShapeRect(61, 2.5f), maxCasts: 6);
class Energy(BossModule module) : Components.PersistentVoidzone(module, 2, m => m.Enemies(OID.Energy).Where(e => !e.IsDead), 10);

class A31KnaveOfHeartsStates : StateMachineBuilder
{
    public A31KnaveOfHeartsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<ColossalImpact>()
            .ActivateOnEnter<MagicArtilleryAlpha>()
            .ActivateOnEnter<LightLeap>()
            .ActivateOnEnter<MagicArtilleryBeta>()
            .ActivateOnEnter<Energy>()
            .ActivateOnEnter<Blocks>()
            .ActivateOnEnter<Lunge>()
            .ActivateOnEnter<MagicBarrage>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9955)]
public class A31KnaveOfHearts(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -724.40625f), new ArenaBoundsSquare(30));

