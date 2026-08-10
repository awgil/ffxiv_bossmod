namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D063Eliminator;

public enum OID : uint
{
    Boss = 0x41CE, // R6.001
    Elimbit = 0x41D0, // R2.0
    EliminationClaw = 0x41CF, // R2.0
    LightningGenerator = 0x41D1, // R3.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 36764, // Boss->player, no cast, single-target
    Teleport = 36763, // Boss->location, no cast, single-target

    Disruption = 36765, // Boss->self, 5.0s cast, range 60 circle, raidwide

    PartitionVisual1 = 39599, // Boss->self, 6.2+0.6s cast, single-target
    PartitionVisual2 = 39600, // Boss->self, 6.2+0.6s cast, single-target
    PartitionVisual3 = 36768, // Boss->self, 4.3+0.7s cast, single-target
    PartitionVisual4 = 36766, // Boss->self, no cast, single-target
    PartitionVisual5 = 36767, // Boss->self, no cast, single-target
    Partition1 = 39007, // Helper->self, 5.0s cast, range 40 180-degree cone
    Partition2 = 39238, // Helper->self, 7.0s cast, range 40 180-degree cone
    Partition3 = 39249, // Helper->self, 7.0s cast, range 40 180-degree cone

    Subroutine1 = 36781, // Boss->self, 3.0s cast, single-target, summons adds
    Subroutine2 = 36775, // Boss->self, 3.0s cast, single-target
    Subroutine3 = 36772, // Boss->self, 3.0s cast, single-target
    SpawnClaw = 36774, // Boss->self, no cast, single-target
    SpawnElimbit = 36777, // Boss->self, no cast, single-target
    SpawnClawAndElimbit = 36788, // Boss->self, no cast, single-target

    ReconfiguredPartition1 = 39248, // Boss->self, 1.2+5.6s cast, single-target
    ReconfiguredPartition2 = 39247, // Boss->self, 1.2+5.6s cast, single-target

    TerminateVisual = 36773, // EliminationClaw->self, 6.2+0.6s cast, single-target
    Terminate = 39615, // Helper->self, 7.0s cast, range 40 width 10 rect

    HaloOfDestructionVisual = 36776, // Elimbit->self, 6.4+0.4s cast, single-target
    HaloOfDestruction = 39616, // Helper->self, 7.0s cast, range 6-40 donut

    OverexposureMarker = 36778, // Helper->player, no cast, single-target
    OverexposureVisual = 36779, // Boss->self, 4.3+0.7s cast, single-target
    Overexposure = 36780, // Helper->self, no cast, range 40 width 6 rect, line stack

    Electray = 39243, // Helper->player, 5.0s cast, range 6 circle

    HoloArk1 = 36789, // Boss->self, no cast, single-target
    HoloArk2 = 36790, // Helper->self, no cast, range 60 circle
    ChargeLimitBreakBar = 36791, // LightningGenerator->Boss, no cast, single-target

    CompressionVisual = 36792, // EliminationClaw->location, 5.3s cast, single-target
    Compression = 36793, // Helper->self, 6.0s cast, range 6 circle

    Impact = 36794, // Helper->self, 6.0s cast, range 60 circle, knockback 15, away from source

    LightOfSalvationVisual = 36782, // Elimbit->self, 6.0s cast, single-target
    LightOfSalvationMarker = 36783, // Helper->player, 5.9s cast, single-target
    LightOfSalvation = 36784, // Helper->self, no cast, range 40 width 6 rect

    LightOfDevotionVisual = 36785, // EliminationClaw->self, 5.0s cast, single-target
    LightOfDevotionMarker = 36786, // Helper->player, no cast, single-target
    LightOfDevotion = 36787, // Helper->self, no cast, range 40 width 6 rect

    Elimination1 = 36795, // Boss->self, 4.0s cast, single-target
    Elimination2 = 36796, // Helper->self, no cast, range 60 circle

    Explosion = 39239 // Helper->self, 8.5s cast, range 50 width 8 rect
}

sealed class DisruptionArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Disruption && Arena.Bounds.Radius > 15f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 15.5f)], [new Square(center, 15f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.7d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x28 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(15f);
            _aoe = [];
        }
    }
}

sealed class Disruption(BossModule module) : Components.RaidwideCast(module, (uint)AID.Disruption);

sealed class Partition(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Partition1, (uint)AID.Partition2, (uint)AID.Partition3], new AOEShapeCone(40f, 90f.Degrees()));
sealed class Terminate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Terminate, new AOEShapeRect(40f, 5f));
sealed class HaloOfDestruction(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HaloOfDestruction, new AOEShapeDonut(6f, 40f));

sealed class Electray(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Electray, 6f)
{
    private readonly HaloOfDestruction _aoe1 = module.FindComponent<HaloOfDestruction>()!;
    private readonly Partition _aoe2 = module.FindComponent<Partition>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe1.Casters.Count != 0 || _aoe2.Casters.Count != 0)
        { }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
            if (Spreads.Count != 0)
            {
                hints.AddForbiddenZone(new SDCircle(Arena.Center - new WDir(default, 15f), 15f), Spreads.Ref(0).Activation);
            }
        }
    }
}

sealed class Explosion : Components.SimpleAOEs
{
    public Explosion(BossModule module) : base(module, (uint)AID.Explosion, new AOEShapeRect(50f, 4f))
    {
        MaxDangerColor = 2;
        MaxRisky = 4;
    }
}

sealed class Impact(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Impact, 15f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, c.Origin, 15f, 14f), c.Activation);
        }
    }
}

sealed class Compression(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Compression, 6f);

sealed class Overexposure(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.OverexposureMarker, (uint)AID.Overexposure, 5d, 40f, 3f);
sealed class LightOfDevotion(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.LightOfDevotionMarker, (uint)AID.LightOfDevotion, 5.5d, 40f, 3f)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x2F && state == 0x00080004u) // as soon as limit break phase ends the line stack gets cancelled
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class LightOfSalvation(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly Impact _kb = module.FindComponent<Impact>()!;
    private readonly AOEShapeRect rect = new(40f, 3f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightOfSalvationMarker)
        {
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.TargetID)!, rect, Module.CastFinishAt(spell, 0.2d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.LightOfSalvation)
        {
            CurrentBaits.Clear();
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x2F && state == 0x00080004u) // as soon as limit break phase ends the line stack gets cancelled
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Casters.Count != 0)
        { }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }
}

sealed class D063EliminatorStates : StateMachineBuilder
{
    public D063EliminatorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DisruptionArenaChange>()
            .ActivateOnEnter<Disruption>()
            .ActivateOnEnter<Partition>()
            .ActivateOnEnter<Terminate>()
            .ActivateOnEnter<HaloOfDestruction>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<Compression>()
            .ActivateOnEnter<LightOfDevotion>()
            .ActivateOnEnter<LightOfSalvation>()
            .ActivateOnEnter<Overexposure>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS), erdelf", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827u, NameID = 12729u)]
public sealed class D063Eliminator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-759f, -648f), new ArenaBoundsSquare(15.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.LightningGenerator), Colors.Object);
    }
}
