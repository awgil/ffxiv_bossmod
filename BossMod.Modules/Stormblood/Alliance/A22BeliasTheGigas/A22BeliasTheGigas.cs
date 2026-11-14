namespace BossMod.Stormblood.Alliance.A22BeliasTheGigas;

public enum OID : uint
{
    Boss = 0x22A4, // R5.100, x2
    _Gen_BeliasTheGigas = 0x18D6, // R0.500, x24, Helper type
    _Gen_ = 0x22A6, // R1.500, x0 (spawn during fight)
    _Gen_Gigas = 0x22A5, // R3.400, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_Fire = 11483, // Boss->player, 4.0s cast, single-target
    _Weaponskill_FireIV = 11484, // Boss->self, 5.0s cast, range 90 circle
    _Weaponskill_TimeEruption = 11486, // Boss->self, 3.0s cast, single-target
    _Weaponskill_TimeEruption1 = 11487, // 18D6->self, 5.0s cast, range 20 width 20 rect
    _Weaponskill_TimeEruption2 = 11488, // 18D6->self, 8.0s cast, range 20 width 20 rect
    _Weaponskill_CrimsonCyclone = 11490, // Boss->location, 5.0s cast, single-target
    _Weaponskill_CrimsonCyclone1 = 11491, // 18D6->self, no cast, range 40+R width 20 rect
    _Weaponskill_CrimsonCyclone2 = 11492, // 18D6->self, no cast, range 60+R width 20 rect
    _Weaponskill_CrimsonCyclone3 = 11615, // Boss->self, no cast, range 70+R width 20 rect
    _Weaponskill_TheHandOfTime = 11493, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Eruption = 11485, // 18D6->location, 3.0s cast, range 8 circle
    _AutoAttack_Attack1 = 870, // 22A5->player, no cast, single-target
    _Weaponskill_Hellfire = 11496, // Boss->self, no cast, range 60 circle
    _Weaponskill_TimeBomb = 11494, // Boss->self, 3.0s cast, single-target
    _Weaponskill_TimeBomb1 = 11495, // 18D6->self, 2.0s cast, range 60 90-degree cone
}

public enum SID : uint
{
    _Gen_Invincibility = 1570, // none->player, extra=0x0
    _Gen_VulnerabilityUp = 202, // 18D6/Boss->player, extra=0x1/0x2/0x3/0x4
    _Gen_Slow = 1509, // 18D6/Boss->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_Slow1 = 1568, // none->player, extra=0x3/0x2/0x1
    _Gen_Burns = 530, // none->player, extra=0x3/0x2/0x1
    _Gen_VulnerabilityDown = 350, // none->22A5, extra=0x0
    _Gen_TemporalDisplacement = 1119, // none->player, extra=0x0
    _Gen_TemporalBarrier = 1571, // none->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_tank_lockon01i = 198, // player->self
    _Gen_Icon_m0376trg_fire3_a0p = 97, // player->self
    _Gen_Icon_m0406trg_a0n = 102, // player->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_fire001f = 5, // 22A6->player
    _Gen_Tether_chn_m0480_hfchain1x = 78, // 22A6->player
    _Gen_Tether_chn_subbly_mgc01f = 14, // 22A5->22A5
}

class Fire(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_Fire);
class FireIV(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_FireIV);

class TimeEruption(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_TimeEruption1, AID._Weaponskill_TimeEruption2], new AOEShapeRect(20, 10))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).TakeSpan(a => a.Activation, TimeSpan.FromSeconds(1));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (IDs.Contains(spell.Action))
            Casters.SortBy(c => Module.CastFinishAt(c.CastInfo));
    }
}

class CrimsonCyclone(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<Actor> _adds = [];
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Index().Select(p => p.Item with { Risky = p.Index < 2, Color = p.Index < 2 ? ArenaColor.Danger : ArenaColor.AOE });

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Boss && id == 0x1E43)
            _adds.Add(actor);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EA9A4)
        {
            var ix = _adds.FindIndex(a => actor.Position.InRect(a.Position, a.Rotation, 100, 0, 10));
            if (ix >= 0)
            {
                var toArrow = actor.Position - _adds[ix].Position;
                _predicted.Add(new(new AOEShapeRect(toArrow.Length() + 10, 10), _adds[ix].Position, toArrow.ToAngle(), WorldState.FutureTime(6.4f)));
                _predicted.Add(new(new AOEShapeRect(50, 10, 10), actor.Position, actor.Rotation, WorldState.FutureTime(9.5f)));
                _predicted.SortBy(p => p.Activation);
                _adds.RemoveAt(ix);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_CrimsonCyclone1 or AID._Weaponskill_CrimsonCyclone2 or AID._Weaponskill_CrimsonCyclone3 && _predicted.Count > 0)
        {
            NumCasts++;
            _predicted.RemoveAt(0);
        }
    }
}

class Eruption(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Eruption, 8);

class HandOfTime(BossModule module) : BossComponent(module)
{
    // completely guessing the range of these, gotta do a bunch more runs to find out
    const float StretchRadius = 12f;

    readonly List<(Actor Nail, Actor Player)> Tethers = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_fire001f && WorldState.Actors.Find(tether.Target) is { } target)
            Tethers.Add((source, target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_m0480_hfchain1x)
            Tethers.RemoveAll(t => t.Nail == source);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (from, to) in Tethers)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(from.Position, to.Position, 0xFF000000, 2);
            Arena.AddLine(from.Position, to.Position, ArenaColor.Danger);

            if (to == pc)
                Arena.AddCircle(from.Position, StretchRadius, ArenaColor.Object);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var (from, to) in Tethers)
            if (to == actor)
                hints.Add("Stretch tether!", actor.Position.InCircle(from.Position, StretchRadius));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (from, to) in Tethers)
        {
            if (to == actor)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(from.Position, StretchRadius), DateTime.MaxValue);

                if (from.Position.AlmostEqual(Arena.Center, 12))
                {
                    var griefingDirection = (Module.Arena.Center - from.Position).ToAngle();
                    hints.AddForbiddenZone(ShapeDistance.Cone(from.Position, 100, griefingDirection, 70.Degrees()), DateTime.MaxValue);
                }
            }
            else
            {
                // touching tether gives a very annoying debuff
                hints.AddForbiddenZone(ShapeDistance.Rect(from.Position, to.Position, 1));
                hints.AddForbiddenZone(ShapeDistance.Rect(from.Position, to.Position, 3), DateTime.MaxValue);
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => Tethers.Any(t => t.Player == player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
}

class Gigas(BossModule module) : Components.Adds(module, (uint)OID._Gen_Gigas)
{
    private readonly List<(Actor From, Actor To)> _tethered = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == ObjectID))
            target.Priority = target.Actor.FindStatus(SID._Gen_VulnerabilityDown) != null ? AIHints.Enemy.PriorityPointless : 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var target in WorldState.Actors.Where(t => t.OID == ObjectID && t.TargetID == actor.InstanceID && t.FindStatus(SID._Gen_VulnerabilityDown) != null))
            hints.Add("Separate adds!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var (f, t) in _tethered)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(f.Position, t.Position, 0xFF000000, 2);
            Arena.AddLine(f.Position, t.Position, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_subbly_mgc01f && WorldState.Actors.Find(tether.Target) is { } target)
            _tethered.Add((source, target));
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        var tt = tether.Target;
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_subbly_mgc01f)
            _tethered.RemoveAll(t => t.From.InstanceID == source.InstanceID && t.To.InstanceID == tt);
    }
}

class Hellfire(BossModule module) : Components.RaidwideInstant(module, AID._Weaponskill_Hellfire, 10)
{
    bool AddsPhase;

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Boss && id == 0x0C84)
            AddsPhase = true;
    }

    public override void OnTargetable(Actor actor)
    {
        if ((OID)actor.OID == OID.Boss && AddsPhase)
        {
            AddsPhase = false;
            Activation = WorldState.FutureTime(5.1f);
        }
    }
}

class TimeBomb(BossModule module) : BossComponent(module)
{
    readonly List<(Actor, Angle)> Bombs = [];
    bool Draw;
    DateTime Activation;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EA9A5)
        {
            Bombs.Add((actor, 180.Degrees()));
            if (Activation == default)
                Activation = WorldState.FutureTime(12.9f);
        }
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        var ix = Bombs.FindIndex(b => b.Item1 == actor);
        if (ix >= 0)
        {
            Draw = true;
            var turn = state switch
            {
                0x800 or 0x880 => 180.Degrees(),
                0x810 => 90.Degrees(),
                0x820 => 0.Degrees(),
                0x840 => -90.Degrees(),
                _ => default
            };
            Bombs.Ref(ix).Item2 = turn;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Draw)
            foreach (var (bomb, turn) in Bombs)
                Arena.ZoneCone(bomb.Position, 0, 60, bomb.Rotation + turn, 45.Degrees(), ArenaColor.AOE);

        foreach (var (bomb, turn) in Bombs)
        {
            Arena.AddCircle(bomb.Position, 3, ArenaColor.Danger);
            Arena.AddLine(bomb.Position, bomb.Position + (bomb.Rotation + turn).ToDirection() * 2, ArenaColor.Danger);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (bomb, turn) in Bombs)
        {
            if (Activation < WorldState.FutureTime(5))
                hints.AddForbiddenZone(ShapeDistance.Cone(bomb.Position, 60, bomb.Rotation + turn, 45.Degrees()), Activation);

            var badDir = Arena.Center - bomb.Position;
            if ((bomb.Rotation + turn).Normalized().AlmostEqual(badDir.ToAngle(), 0.1f))
                hints.GoalZones.Add(AIHints.GoalSingleTarget(bomb.Position, 3, 0.5f));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_TimeBomb1)
        {
            Draw = false;
            Bombs.Clear();
            Activation = default;
        }
    }
}

class TimeBombCast(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_TimeBomb1, new AOEShapeCone(60, 45.Degrees()));

class A22BeliasTheGigasStates : StateMachineBuilder
{
    public A22BeliasTheGigasStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Fire>()
            .ActivateOnEnter<FireIV>()
            .ActivateOnEnter<TimeEruption>()
            .ActivateOnEnter<CrimsonCyclone>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<HandOfTime>()
            .ActivateOnEnter<Gigas>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<TimeBomb>()
            .ActivateOnEnter<TimeBombCast>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7223)]
public class A22BeliasTheGigas(ModuleInit init) : BossModule(init, new(-200, -541), new ArenaBoundsSquare(29.5f));

