namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D112BloodyHeadsman;

public enum OID : uint
{
    Boss = 0x4890, // R2.720, x1
    PaleHeadsman = 0x4891, // R2.720, x1
    RavenousHeadsman = 0x4892, // R2.720, x1
    PestilentHeadsman = 0x4893, // R2.720, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    SwordOfJustice = 0x4894, // R1.000, x0 (spawn during fight)
    Unk1 = 0x4A27, // R1.000, x4
    Hellmaker = 0x48D2, // R2.500, x0 (spawn during fight)
    HoodedHeadsman = 0x49E1, // R1.000, x1
}

public enum AID : uint
{
    LawlessPursuit = 43577, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->self, 3.0s cast, single-target
    HeadSplittingRoarCast = 43578, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->self, 5.0s cast, single-target
    HeadSplittingRoar = 43579, // HoodedHeadsman->self, 5.5s cast, range 60 circle
    Jump = 43580, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->location, no cast, single-target
    ShacklesOfFateCast = 43581, // PaleHeadsman/PestilentHeadsman/RavenousHeadsman/Boss->player, 4.0s cast, single-target
    ShacklesOfFateStun = 43582, // Helper->player, 1.0s cast, single-target
    DismembermentCast = 43586, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->self, 3.0s cast, single-target
    AutoAttack3 = 43600, // RavenousHeadsman/PestilentHeadsman->player, no cast, single-target
    AutoAttack2 = 43599, // Boss->player, no cast, single-target
    AutoAttack1 = 43598, // PaleHeadsman->player, no cast, single-target
    Dismemberment = 43587, // Helper->self, 6.0s cast, range 16 width 4 rect
    PealOfJudgmentCast = 43593, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->self, 3.0s cast, single-target
    PealOfJudgment = 43594, // Helper->self, no cast, range 2 width 4 rect
    ExecutionWheel = 43596, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->self, 3.5s cast, range ?-9 donut
    FlayingFlailCast = 43591, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->self, 3.0s cast, single-target
    FlayingFlail = 43592, // Helper->location, 5.0s cast, range 5 circle
    DeathPenalty = 43588, // Boss->player, 5.0s cast, single-target
    RelentlessTorment = 43589, // PaleHeadsman->player, 5.0s cast, single-target
    RelentlessTormentRepeat = 43590, // Helper->player, no cast, single-target
    SerialTorture = 43597, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->self, 3.0s cast, single-target
    ChoppingBlock = 43595, // RavenousHeadsman/Boss/PestilentHeadsman/PaleHeadsman->self, 3.5s cast, range 6 circle
    WillBreaker = 44856, // PaleHeadsman->player, 7.0s cast, single-target
}

public enum SID : uint
{
    CellBlockA = 4542, // none->player, extra=0x0
    CellBlockB = 4543, // none->player, extra=0x0
    CellBlockC = 4544, // none->player, extra=0x0
    CellBlockD = 4545, // none->player, extra=0x0
    GuardOnDutyA = 4546, // none->Boss, extra=0x0
    GuardOnDutyB = 4547, // none->PaleHeadsman, extra=0x0
    GuardOnDutyC = 4548, // none->RavenousHeadsman, extra=0x0
    GuardOnDutyD = 4549, // none->PestilentHeadsman, extra=0x0
    Activate = 2552, // none->PaleHeadsman/RavenousHeadsman/PestilentHeadsman/Boss/SwordOfJustice, extra=0x394/0x3A3
    Bind = 3457, // Helper->player, extra=0x0
    SwordGlow = 2234, // none->SwordOfJustice, extra=0xA
    Burns = 3065, // none->player, extra=0x0
    Doom = 4594, // Boss->player, extra=0x0
    FugitiveFromJustice = 4550, // none->player, extra=0x0
}

class HeadSplittingRoar(BossModule module) : Components.RaidwideCast(module, AID.HeadSplittingRoar);
class Dismemberment(BossModule module) : Components.StandardAOEs(module, AID.Dismemberment, new AOEShapeRect(16, 2), maxCasts: 8);
class FlayingFlail(BossModule module) : Components.StandardAOEs(module, AID.FlayingFlail, 5);
class ChoppingBlock(BossModule module) : Components.StandardAOEs(module, AID.ChoppingBlock, 6);
class RelentlessTorment(BossModule module) : Components.SingleTargetCast(module, AID.RelentlessTorment);

// TODO: figure out what the inner radius is when duty recorder works again
class ExecutionWheel(BossModule module) : Components.StandardAOEs(module, AID.ExecutionWheel, new AOEShapeDonut(4, 9));

class WillBreaker(BossModule module) : Components.CastInterruptHint(module, AID.WillBreaker);

class Shackles(BossModule module) : Components.GenericInvincible(module)
{
    private readonly List<Actor> _shackles = [];

    private readonly int[] _cellBlocks = Utils.MakeArray(4, -1);
    private readonly Actor?[] _guards = new Actor?[4];

    private static readonly WPos[] _cells = [
        new(60, -268),
        new(60, -248),
        new(42.5f, -258),
        new(77.5f, -258)
    ];

    public static int GetCell(WPos position) => Array.FindIndex(_cells, c => position.AlmostEqual(c, 8.5f));

    protected override IEnumerable<Actor> ForbiddenTargets(int slot, Actor actor)
    {
        var block = _cellBlocks[slot];

        if (block < 0)
            yield break;

        for (var i = 0; i < _guards.Length; i++)
        {
            if (i == block)
                continue;

            if (_guards[i] is { } g)
                yield return g;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var actor in _shackles)
            Arena.AddCircle(actor.Position, 8, ArenaColor.Border);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_cellBlocks[slot] >= 0 && _shackles.Count > 0 && _guards[_cellBlocks[slot]] is { } g)
            hints.TemporaryObstacles.Add(ShapeContains.Donut(g.Position, 7.5f, 100));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        // TODO: i don't know what spawns the cell vfx, but it's not this, helper objects also get the 2552 status when they activate
        if (status.ID == 2552 && _guards.Contains(actor))
            _shackles.Add(actor);

        if (status.ID is >= 4542 and <= 4545 && Raid.TryFindSlot(actor, out var slot))
            _cellBlocks[slot] = (int)status.ID - 4542;

        if (status.ID is >= 4546 and <= 4549)
        {
            _guards[status.ID - 4546] = actor;
        }
    }

    public override void Update()
    {
        _shackles.RemoveAll(s => s.IsDead);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is >= 4542 and <= 4545 && Raid.TryFindSlot(actor, out var slot))
            _cellBlocks[slot] = -1;

        if (status.ID is >= 4546 and <= 4549)
        {
            _shackles.Remove(actor);
            for (var i = 0; i < _guards.Length; i++)
                if (_guards[i] == actor)
                    _guards[i] = null;
        }
    }
}

class PealOfJudgment(BossModule module) : Components.GenericAOEs(module, AID.PealOfJudgment)
{
    private readonly List<(Actor Actor, DateTime Spawn, int cell)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(2, 2), c.Actor.Position, c.Actor.Rotation, c.Spawn));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var cell = Shackles.GetCell(actor.Position);

        foreach (var c in _casters)
        {
            // make sure other player's AOE doesn't bleed into our cell
            if (c.cell != cell)
                continue;

            hints.AddForbiddenZone(new AOEShapeRect(2, 2), c.Actor.Position, c.Actor.Rotation, c.Spawn);
            hints.AddForbiddenZone(new AOEShapeRect(8, 2), c.Actor.Position, c.Actor.Rotation, WorldState.FutureTime(2));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == 2552 && (OID)actor.OID == OID.SwordOfJustice)
            _casters.Add((actor, WorldState.FutureTime(2), Shackles.GetCell(actor.Position)));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _casters.RemoveAll(c => c.Actor == actor);
    }
}

class Hellmaker(BossModule module) : Components.Adds(module, (uint)OID.Hellmaker)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var inCell = actor.Statuses.Any(s => s.ID is >= 4542 and <= 4545);

        foreach (var add in ActiveActors)
        {
            var canAttack = !inCell || actor.Position.InCircle(add.Position, 18);
            hints.SetPriority(add, canAttack ? 1 : AIHints.Enemy.PriorityInvincible);
        }
    }
}

class Doom(BossModule module) : BossComponent(module)
{
    private Actor? _target;
    private Actor? _victim;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeathPenalty)
            _target = WorldState.Actors.Find(spell.TargetID);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
        {
            _target = null;
            _victim = actor;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Doom)
            _victim = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == actor)
            hints.Add("Prepare to cleanse!", false);

        if (_victim == actor && _victim.PendingDispels.Count == 0)
            hints.Add("Cleanse yourself!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_victim != null && Raid.TryFindSlot(_victim, out var v))
            hints.ShouldCleanse.Set(v);
    }
}

class D112BloodyHeadsmanStates : StateMachineBuilder
{
    public D112BloodyHeadsmanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeadSplittingRoar>()
            .ActivateOnEnter<Shackles>()
            .ActivateOnEnter<Dismemberment>()
            .ActivateOnEnter<PealOfJudgment>()
            .ActivateOnEnter<FlayingFlail>()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<ChoppingBlock>()
            .ActivateOnEnter<RelentlessTorment>()
            .ActivateOnEnter<ExecutionWheel>()
            .ActivateOnEnter<Hellmaker>()
            .ActivateOnEnter<WillBreaker>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && !Module.Enemies(OID.PaleHeadsman).Any(h => !h.IsDead) && !Module.Enemies(OID.RavenousHeadsman).Any(h => !h.IsDead) && !Module.Enemies(OID.PestilentHeadsman).Any(h => !h.IsDead);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028, NameID = 14047)]
public class D112BloodyHeadsman(WorldState ws, Actor primary) : BossModule(ws, primary, new(60, -258), new ArenaBoundsRect(29.5f, 20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.PaleHeadsman), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.RavenousHeadsman), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.PestilentHeadsman), ArenaColor.Enemy);
    }
}
