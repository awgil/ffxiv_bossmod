namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE21WithDiremiteAndMain;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x6
    Boss = 0x31CC, // R7.200, x1
    DimCrystal = 0x31CD, // R1.600, spawn during fight
    CorruptedCrystal = 0x31CE, // R1.600, spawn during fight
    SandSphere = 0x31CF, // R4.000, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    Shardfall = 24071, // Boss->self, 2.0s cast, single-target, visual
    CrystallineFracture = 24072, // CorruptedCrystal/DimCrystal->self, 3.0s cast, range 4 circle aoe (cast on spawn)
    ResonantFrequencyDim = 24073, // DimCrystal->self, 3.0s cast, range 6 circle, suicide, cast early if crystal is hit by shardstrike
    ResonantFrequencyCorrupted = 24074, // CorruptedCrystal->self, 3.0s cast, range 6 circle, suicide, cast early if crystal is hit by shardstrike
    ResonantFrequencyDimStinger = 24075, // DimCrystal->self, no cast, single-target, suicide, after aetherial stingers
    ResonantFrequencyCorruptedStinger = 24076, // CorruptedCrystal->self, no cast, single-target, suicide, after crystalline stingers
    CrystallineStingers = 24077, // Boss->self, 5.0s cast, range 60 circle, hide behind dim
    AetherialStingers = 24078, // Boss->self, 5.0s cast, range 60 circle, hide behind corrupted
    SandSphere = 24079, // Boss->self, 5.0s cast, single-target, visual
    Subduction = 24080, // SandSphere->self, 4.0s cast, range 8 circle aoe with knockback 10
    Earthbreaker = 24081, // Boss->self, 5.0s cast, single-target, visual
    EarthbreakerAOE1 = 24082, // Helper->self, 5.0s cast, range 10 circle
    EarthbreakerAOE2 = 24083, // Helper->self, 3.0s cast, range 10-20 donut
    EarthbreakerAOE3 = 24084, // Helper->self, 3.0s cast, range 20-30 donut

    CrystalNeedle = 24085, // Boss->player, 5.0s cast, single-target, tankbuster
    Shardstrike = 24086, // Boss->self, 2.0s cast, single-target, visual
    ShardstrikeAOE = 24087, // Helper->players, 5.0s cast, range 5 circle spread
    Hailfire = 24088, // Boss->self, 8.0s cast, single-target, visual
    HailfireAOE = 24089, // Boss->self, no cast, range 40 width 4 rect aoe
    Teleport = 24090, // Boss->location, no cast, single-target
}

public enum IconID : uint
{
    Shardstrike = 96, // player
    Hailfire1 = 79, // player
    Hailfire2 = 80, // player
    Hailfire3 = 81, // player
    Hailfire4 = 82, // player
}

class CrystallineFracture(BossModule module) : Components.StandardAOEs(module, AID.CrystallineFracture, new AOEShapeCircle(4));
class ResonantFrequencyDim(BossModule module) : Components.StandardAOEs(module, AID.ResonantFrequencyDim, new AOEShapeCircle(6));
class ResonantFrequencyCorrupted(BossModule module) : Components.StandardAOEs(module, AID.ResonantFrequencyCorrupted, new AOEShapeCircle(6));

class CrystallineStingers(BossModule module) : Components.CastLineOfSightAOE(module, AID.CrystallineStingers, 60, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.DimCrystal).Where(a => !a.IsDead);
}

class AetherialStingers(BossModule module) : Components.CastLineOfSightAOE(module, AID.AetherialStingers, 60, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.CorruptedCrystal).Where(a => !a.IsDead);
}

class Subduction(BossModule module) : Components.StandardAOEs(module, AID.Subduction, new AOEShapeCircle(8));

// next aoe starts casting slightly before previous, so use a custom component
class Earthbreaker(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, AOEShape shape)> _active = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _active.Take(1).Select(e => new AOEInstance(e.shape, e.caster.Position, e.caster.CastInfo!.Rotation, Module.CastFinishAt(e.caster.CastInfo)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.EarthbreakerAOE1 => new AOEShapeCircle(10),
            AID.EarthbreakerAOE2 => new AOEShapeDonut(10, 20),
            AID.EarthbreakerAOE3 => new AOEShapeDonut(20, 30),
            _ => null
        };
        if (shape != null)
            _active.Add((caster, shape));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        _active.RemoveAll(c => c.caster == caster);
    }
}

class CrystalNeedle(BossModule module) : Components.SingleTargetCast(module, AID.CrystalNeedle);
class Shardstrike(BossModule module) : Components.SpreadFromCastTargets(module, AID.ShardstrikeAOE, 5);

// TODO: this should probably be generalized
class Hailfire(BossModule module) : Components.GenericAOEs(module, AID.HailfireAOE)
{
    private readonly Actor?[] _targets = new Actor?[4];
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(40, 2);

    private Actor? NextTarget => NumCasts < _targets.Length ? _targets[NumCasts] : null;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NextTarget is var target && target != null && target != actor)
            yield return new(_shape, Module.PrimaryActor.Position, Angle.FromDirection(target.Position - Module.PrimaryActor.Position), _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (NextTarget == pc)
            _shape.Outline(Arena, Module.PrimaryActor.Position, Angle.FromDirection(pc.Position - Module.PrimaryActor.Position), ArenaColor.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && NumCasts < _targets.Length)
        {
            _targets[NumCasts] = null;
            _activation = WorldState.FutureTime(2.3f);
        }
        base.OnEventCast(caster, spell);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = (IconID)iconID switch
        {
            IconID.Hailfire1 => 0,
            IconID.Hailfire2 => 1,
            IconID.Hailfire3 => 2,
            IconID.Hailfire4 => 3,
            _ => -1
        };
        if (order >= 0)
        {
            NumCasts = 0;
            _targets[order] = actor;
            _activation = WorldState.FutureTime(8.2f);
        }
    }
}

class HedetetStates : StateMachineBuilder
{
    public HedetetStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrystallineFracture>()
            .ActivateOnEnter<ResonantFrequencyDim>()
            .ActivateOnEnter<ResonantFrequencyCorrupted>()
            .ActivateOnEnter<CrystallineStingers>()
            .ActivateOnEnter<AetherialStingers>()
            .ActivateOnEnter<Subduction>()
            .ActivateOnEnter<Earthbreaker>()
            .ActivateOnEnter<CrystalNeedle>()
            .ActivateOnEnter<Shardstrike>()
            .ActivateOnEnter<Hailfire>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 21)] // bnpcname=9969
public class Hedetet : BossModule
{
    private readonly IReadOnlyList<Actor> _dimCrystals;
    private readonly IReadOnlyList<Actor> _corruptedCrystals;

    public Hedetet(WorldState ws, Actor primary) : base(ws, primary, new(-220, 530), new ArenaBoundsCircle(30))
    {
        _dimCrystals = Enemies(OID.DimCrystal);
        _corruptedCrystals = Enemies(OID.CorruptedCrystal);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(_dimCrystals.Where(c => !c.IsDead), ArenaColor.Object, true);
        Arena.Actors(_corruptedCrystals.Where(c => !c.IsDead), ArenaColor.Object, true);
    }
}
