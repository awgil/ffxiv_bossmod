namespace BossMod.RealmReborn.Alliance.A21Scylla;

public enum OID : uint
{
    Boss = 0xBC1, // R3.750, x1
    Helper = 0x1B2, // R0.500
    StaffOfEldering = 0xBC2, // R1.000, spawns in sets of 3, either on the outer ring (~32y out) or on the inner hexagon (~16y out)
    ShiveringSoul = 0xBC4, // R1.000, ice, tethered to a player
    SmolderingSoul = 0xBC5, // R1.000, fire, tethered to a player
    ShudderingSoul = 0xBC6, // R1.000, aether, tethered to a player
    Puddle = 0x1E950D, // R0.500, EventObj, left where a smoldering soul thaws a player, shields from ancient flare
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AncientFlare = 2317, // Boss->self, 10.0s cast, range 81 circle, raidwide
    Unholy = 2319, // Boss->location, no cast, range 81 circle, raidwide ~7s after souls spawn
    Topple = 2321, // Boss->self, 3.0s cast, range 3+R circle
    Smolder = 2322, // SmolderingSoul->self, no cast, range 2+R circle, on contact with its target; thaws Deep Freeze
    Shiver = 2323, // ShiveringSoul->self, no cast, single-target, on contact with its target; applies Deep Freeze
    Shudder = 2324, // ShudderingSoul->self, no cast, range 81 circle, raidwide if the soul is not drained within ~25s
    Firewalker = 2329, // Boss->self, no cast, range 5+R 90-degree cone
    SearingChain = 2330, // StaffOfEldering->self, 3.0s cast, range 60+R width 4 rect
    InfiniteAnguish = 2331, // StaffOfEldering->location, 3.0s cast, range 6-12 donut
}

public enum SID : uint
{
    Heavy = 14, // none->ShiveringSoul, extra=0x32
    DeepFreeze = 3479, // ShiveringSoul->player, extra=0x1, 30s stun
}

public enum IconID : uint
{
    ShudderingSoul = 24, // player
    SmolderingSoul = 25, // player
    ShiveringSoul = 26, // player
}

public enum TetherID : uint
{
    SmolderingSoul = 5, // SmolderingSoul->player
    ShudderingSoul = 6, // ShudderingSoul->player
    ShiveringSoul = 8, // ShiveringSoul->player
}

class AncientFlare(BossModule module) : Components.RaidwideCast(module, AID.AncientFlare);
class Unholy(BossModule module) : Components.CastCounter(module, AID.Unholy);
class Topple(BossModule module) : Components.StandardAOEs(module, AID.Topple, new AOEShapeCircle(6.75f));
class SearingChain(BossModule module) : Components.StandardAOEs(module, AID.SearingChain, new AOEShapeRect(61, 2));
class InfiniteAnguish(BossModule module) : Components.StandardAOEs(module, AID.InfiniteAnguish, new AOEShapeDonut(6, 12));

class Firewalker(BossModule module) : Components.Cleave(module, AID.Firewalker, new AOEShapeCone(8.75f, 45.Degrees()))
{
    private const float Warning = 3;
    private static readonly float[] Delays = [36.4f, 31.4f, 40.3f];

    private bool Due => NextExpected != default && WorldState.CurrentTime >= NextExpected.AddSeconds(-Warning);

    public override void Update()
    {
        if (NextExpected == default)
            NextExpected = WorldState.FutureTime(Delays[0]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            NextExpected = WorldState.FutureTime(Delays[Math.Min(NumCasts, Delays.Length - 1)]);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Due)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Due)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Due)
            base.DrawArenaForeground(pcSlot, pc);
    }
}

class Puddles(BossModule module) : BossComponent(module)
{
    private const float Radius = 4;
    private DateTime _flare;

    private List<Actor> Active => [.. Module.Enemies(OID.Puddle).Where(p => !p.IsDestroyed && p.EventState != 7)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AncientFlare)
            _flare = Module.CastFinishAt(spell);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AncientFlare)
            _flare = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_flare != default && Active is { Count: > 0 } puddles)
            hints.Add("Stand in a puddle!", !puddles.Any(p => actor.Position.InCircle(p.Position, Radius)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_flare != default && Active is { Count: > 0 } puddles)
            hints.AddForbiddenZone(ShapeDistance.Intersection([.. puddles.Select(p => ShapeDistance.InvertedCircle(p.Position, Radius))]), _flare);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_flare != default)
            foreach (var p in Active)
                Arena.ZoneCircle(p.Position, Radius, ArenaColor.SafeFromAOE);
    }
}

class Souls(BossModule module) : BossComponent(module)
{
    private static readonly WPos[] DrainSpots = [.. Enumerable.Range(0, 3).Select(i => A21Scylla.ArenaCenter + 27.5f * (120f * i).Degrees().ToDirection())];
    private const float DrainRadius = 2.5f;
    private const float SmolderRadius = 4;
    private const float ShiverRadius = 3;
    private const float ShudderDelay = 25;

    private readonly List<(Actor Soul, TetherID Kind, Actor Target, DateTime Explosion)> _souls = [];

    private IEnumerable<Actor> Frozen => Raid.WithoutSlot().Where(p => p.FindStatus(SID.DeepFreeze) != null);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var kind = (TetherID)tether.ID;
        if (kind is TetherID.SmolderingSoul or TetherID.ShudderingSoul or TetherID.ShiveringSoul && WorldState.Actors.Find(tether.Target) is { } target)
        {
            var explosion = _souls.Find(s => s.Soul == source).Explosion;
            _souls.RemoveAll(s => s.Soul == source);
            _souls.Add((source, kind, target, explosion != default ? explosion : WorldState.FutureTime(ShudderDelay)));
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether) => _souls.RemoveAll(s => s.Soul == source);

    public override void Update() => _souls.RemoveAll(s => s.Soul.IsDestroyed || s.Soul.IsDead);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        foreach (var s in _souls.Where(s => s.Target == actor))
        {
            hints.Add(s.Kind switch
            {
                TetherID.ShudderingSoul => $"Drag the purple soul into a drain spot! ({Math.Max((s.Explosion - WorldState.CurrentTime).TotalSeconds, 0):f0}s)",
                TetherID.ShiveringSoul => "Kite the ice soul!",
                _ => Frozen.Any() ? "Bring the fire soul to a frozen player!" : "Kite the fire soul away from the raid!"
            });
        }

        if (actor.FindStatus(SID.DeepFreeze) == null && _souls.Any(s => s.Kind == TetherID.SmolderingSoul && s.Target != actor && actor.Position.InCircle(s.Soul.Position, SmolderRadius)))
            hints.Add("GTFO from fire soul!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var frozen = Frozen.ToList();
        foreach (var s in _souls)
        {
            if (s.Target == actor)
            {
                switch (s.Kind)
                {
                    case TetherID.ShudderingSoul:
                        hints.GoalZones.Add(AIHints.GoalSingleTarget(DrainSpots.MinBy(p => (p - actor.Position).LengthSq()), DrainRadius, 5));
                        break;
                    case TetherID.ShiveringSoul:
                        hints.AddForbiddenZone(ShapeDistance.Circle(s.Soul.Position, ShiverRadius));
                        break;
                    case TetherID.SmolderingSoul:
                        if (frozen.Count > 0)
                            hints.GoalZones.Add(AIHints.GoalSingleTarget(frozen.MinBy(p => (p.Position - actor.Position).LengthSq())!.Position, 1, 5));
                        break;
                }
            }
            else if (s.Kind == TetherID.SmolderingSoul && actor.FindStatus(SID.DeepFreeze) == null)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(s.Soul.Position, SmolderRadius));
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var s in _souls.Where(s => s.Kind == TetherID.SmolderingSoul && s.Target != pc))
            Arena.ZoneCircle(s.Soul.Position, SmolderRadius, ArenaColor.AOE);

        if (_souls.Any(s => s.Kind == TetherID.ShudderingSoul && s.Target == pc))
            foreach (var p in DrainSpots)
                Arena.ZoneCircle(p, DrainRadius, ArenaColor.SafeFromAOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var s in _souls)
        {
            Arena.Actor(s.Soul, s.Kind == TetherID.SmolderingSoul ? ArenaColor.Danger : ArenaColor.Object, true);
            if (s.Target != pc)
                continue;

            Arena.AddLine(s.Soul.Position, pc.Position, ArenaColor.Danger);
            if (s.Kind == TetherID.ShudderingSoul)
                Arena.AddCircle(DrainSpots.MinBy(p => (p - pc.Position).LengthSq()), DrainRadius, ArenaColor.Safe, 2);
            else if (s.Kind == TetherID.SmolderingSoul)
                foreach (var f in Frozen)
                    Arena.AddCircle(f.Position, 1, ArenaColor.Safe);
        }
    }
}

class Staves(BossModule module) : Components.AddsPointless(module, (uint)OID.StaffOfEldering);

class A21ScyllaStates : StateMachineBuilder
{
    private const float Late = 1000;

    public A21ScyllaStates(BossModule module) : base(module)
    {
        DeathPhase(0, Fight)
            .ActivateOnEnter<AncientFlare>()
            .ActivateOnEnter<Unholy>()
            .ActivateOnEnter<Puddles>()
            .ActivateOnEnter<Topple>()
            .ActivateOnEnter<Firewalker>()
            .ActivateOnEnter<SearingChain>()
            .ActivateOnEnter<InfiniteAnguish>()
            .ActivateOnEnter<Souls>()
            .ActivateOnEnter<Staves>();
    }

    private void Fight(uint id)
    {
        Unholy(id, 25.2f, 1);
        Unholy(id + 0x10000, 19.4f, 2);
        Unholy(id + 0x20000, 18.1f, 3);
        Unholy(id + 0x30000, 15.3f, 4);
        ComponentCondition<AncientFlare>(id + 0x40000, 21.1f, c => c.NumCasts >= 1, "Ancient flare", Late)
            .SetHint(StateMachine.StateHint.Raidwide);
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void Unholy(uint id, float delay, int count)
    {
        ComponentCondition<Unholy>(id, delay, c => c.NumCasts >= count, "Raidwide", Late)
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 102, NameID = 2809, SortOrder = 2)]
public class A21Scylla(ModuleInit init) : BossModule(init, ArenaCenter, new ArenaBoundsCircle(39))
{
    public static readonly WPos ArenaCenter = new(0, -192);
}
