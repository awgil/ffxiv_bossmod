namespace BossMod.Stormblood.Raid.O1NAlteRoite;

public enum OID : uint
{
    Boss = 0x1A6F, // R5.600
    Helper = 0x18D6, // R0.500
    BallOfFire = 0x1A71, // R1.000
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WyrmTail = 9174, // Boss->player, no cast, single-target tankbuster
    Flame = 9181, // Boss->self, no cast, raidwide, spawns fire orbs
    Roar = 9180, // Boss->self, 4.0s cast, range 100 circle
    Burn = 9173, // BallOfFire->self, 1.0s cast, range 8 circle
    BreathWing = 9182, // Boss->self, 5.0s cast, knockback 8 along facing
    TwinBolt = 9175, // Boss->self, 5.0s cast, tether 21 from target to closest player
    TwinBoltHit = 9176, // Helper->player, no cast, single-target
    Clamp = 9186, // Boss->self, 3.0s cast, range 9+R width 10 rect
    FlashFreeze = 9183, // Boss->self, no cast, applies thin ice
    Levinbolt = 9177, // Boss->self, 5.0s cast, spread
    LevinboltAOE = 9178, // Helper->self, no cast, range 6 circle
    Blaze = 9185, // Boss->player, 5.0s cast, range 6 circle stack
    TheClassicalElements = 9184, // Boss->self, 5.0s cast, elemental combo
    Turbulence = 9603, // Helper->self, no cast, range 5 circle, tornado pulses
    Downburst = 7896, // Boss->self, 5.0s cast, tornado in the center
    DownburstDeath = 7897, // Helper->self, no cast, range 5 circle
    DownburstKnockback = 7898, // Helper->self, no cast, range 21, knockback 14
    Charybdis = 9179, // Boss->self, 5.0s cast, range 100 circle, HP to 1
}

public enum SID : uint
{
    ThinIce = 911, // none->player, extra/10 = slide distance
}

public enum IconID : uint
{
    Spread = 108, // player
    Stack = 62, // player
}

public enum TetherID : uint
{
    TwinBolt = 21, // player->player, boss target to closest player
}

class Roar(BossModule module) : Components.RaidwideCast(module, AID.Roar);
class Charybdis(BossModule module) : Components.RaidwideCast(module, AID.Charybdis, "HP to 1");
class Clamp(BossModule module) : Components.StandardAOEs(module, AID.Clamp, new AOEShapeRect(14.6f, 5));
class Blaze(BossModule module) : Components.StackWithCastTargets(module, AID.Blaze, 6);

class BreathWing(BossModule module) : Components.KnockbackFromCastTarget(module, AID.BreathWing, 8, kind: Kind.DirForward)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casters.Count > 0)
            hints.Add("Knockback - get close!");
    }
}

class Levinbolt(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, AID.LevinboltAOE, 6, 6.3f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.LevinboltAOE)
            return;
        var before = Spreads.Count;
        Spreads.RemoveAll(s => spell.Targets.Any(t => t.ID == s.Target.InstanceID));
        if (Spreads.Count != before)
            ++NumFinishedSpreads;
    }
}

class TwinBolt(BossModule module) : Components.CastHint(module, AID.TwinBolt, "Tankbuster on closest player")
{
    private readonly List<(Actor Source, Actor Target)> _tethers = [];
    private DateTime _activation;

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.TwinBolt || WorldState.Actors.Find(tether.Target) is not { } target)
            return;
        _tethers.Add((source, target));
        if (_activation == default)
            _activation = WorldState.FutureTime(5.7f);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID != (uint)TetherID.TwinBolt)
            return;
        _tethers.RemoveAll(t => t.Source == source);
        if (_tethers.Count == 0 && Casters.Count == 0)
            _activation = default;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action == WatchedAction)
            _activation = Module.CastFinishAt(spell, 0.7f);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (_tethers.Count == 0 && Casters.Count == 0)
            _activation = default;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var (source, target) = Pair();
        if (source == null)
            return;
        if (actor == source || actor == target && actor.Role == Role.Tank)
            hints.Add("Tankbuster!", false);
        else if (actor == target)
            hints.Add("Buster on you!");
        else if (_tethers.Count == 0 && actor.Role != Role.Tank && actor.Position.InCircle(source.Position, 8))
            hints.Add("Away from tank!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var (source, target) = Pair();
        if (_activation == default || source == null)
            return;
        if (Raid.TryFindSlot(source, out var sourceSlot))
            hints.AddPredictedDamage(new BitMask().WithBit(sourceSlot), _activation, AIHints.PredictedDamageType.Tankbuster);
        if (target != null && Raid.TryFindSlot(target, out var targetSlot))
            hints.AddPredictedDamage(new BitMask().WithBit(targetSlot), _activation, AIHints.PredictedDamageType.Tankbuster);
        if (_tethers.Count == 0 && actor.Role != Role.Tank)
            hints.AddForbiddenZone(ShapeDistance.Circle(source.Position, 8), _activation);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var (source, target) = Pair();
        return player == source || player == target ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (source, target) in _tethers)
        {
            var color = source.Role == Role.Tank && target.Role == Role.Tank ? ArenaColor.Safe : ArenaColor.Danger;
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(source.Position, target.Position, 0xFF000000, 2);
            Arena.AddLine(source.Position, target.Position, color);
        }
    }

    private (Actor? source, Actor? target) Pair()
    {
        if (_tethers.Count > 0)
            return _tethers[0];
        if (Casters.Count == 0)
            return (null, null);

        var source = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        if (source == null)
            return (null, null);

        Actor? target = null;
        var best = float.MaxValue;
        foreach (var p in Raid.WithoutSlot().Exclude(source))
        {
            var dist = (p.Position - source.Position).LengthSq();
            if (dist < best)
            {
                best = dist;
                target = p;
            }
        }
        return (source, target);
    }
}

class Burn(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Orb, DateTime Activation)> _orbs = [];
    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_orbs.Count == 0)
            yield break;

        var soonest = _orbs.Min(o => o.Activation);
        foreach (var o in _orbs)
            yield return new(_shape, o.Orb.Position, default, o.Activation, Risky: o.Activation <= soonest.AddSeconds(0.5f));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BallOfFire)
            _orbs.Add((actor, WorldState.FutureTime(17.2f)));
    }

    public override void OnActorDestroyed(Actor actor) => _orbs.RemoveAll(o => o.Orb == actor);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.Burn)
            return;
        if (_orbs.RemoveAll(o => o.Orb == caster) > 0)
            ++NumCasts;
    }
}

class Downburst(BossModule module) : Components.GenericAOEs(module, AID.DownburstDeath, "Out of the tornado!")
{
    private DateTime _activation;
    private static readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(_shape, Arena.Center, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Downburst)
            _activation = Module.CastFinishAt(spell, 1.3f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.DownburstDeath)
            return;
        _activation = default;
        ++NumCasts;
    }
}

class DownburstKnockback(BossModule module) : Components.Knockback(module, AID.DownburstKnockback, ignoreImmunes: true, stopAtWall: true)
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Arena.Center, 14, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Downburst)
            _activation = Module.CastFinishAt(spell, 1.3f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.DownburstKnockback)
            return;
        _activation = default;
        ++NumCasts;
    }
}

class ThinIce(BossModule module) : Components.Knockback(module, ignoreImmunes: true)
{
    private static float SlideDistance(Actor actor) => actor.FindStatus(SID.ThinIce) is { } status ? status.Extra / 10f : 0;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        var dist = SlideDistance(actor);
        if (dist > 0)
            yield return new(actor.Position, dist, default, null, actor.Rotation, Kind.DirForward);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (!Module.InBounds(pos))
            return true;
        foreach (var comp in Module.Components.OfType<Components.GenericAOEs>())
            if (comp.ActiveAOEs(slot, actor).Any(a => a.Risky && a.Check(pos)))
                return true;
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (SlideDistance(actor) <= 0)
            return;
        hints.Add("Slippery floor", false);
        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var dist = SlideDistance(actor);
        if (dist <= 0)
            return;

        var aoes = Module.Components.OfType<Components.GenericAOEs>().SelectMany(c => c.ActiveAOEs(slot, actor)).Where(a => a.Risky).ToList();
        if (!aoes.Any(a => a.Check(actor.Position)))
            return;

        WDir bestDir = default;
        var bestScore = float.MinValue;
        for (var i = 0; i < 16; ++i)
        {
            var dir = (i * 22.5f).Degrees().ToDirection();
            var landing = actor.Position + dist * dir;
            if (!Module.InBounds(landing) || aoes.Any(a => a.Check(landing)))
                continue;

            var score = -(landing - Module.PrimaryActor.Position).LengthSq();
            if (score > bestScore)
            {
                bestScore = score;
                bestDir = dir;
            }
        }
        if (bestDir != default)
            hints.ForcedMovement = bestDir.ToVec3(actor.PosRot.Y);
    }
}

class O1NAlteRoiteStates : StateMachineBuilder
{
    public O1NAlteRoiteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Clamp>()
            .ActivateOnEnter<Blaze>()
            .ActivateOnEnter<BreathWing>()
            .ActivateOnEnter<Levinbolt>()
            .ActivateOnEnter<TwinBolt>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<Downburst>()
            .ActivateOnEnter<DownburstKnockback>()
            .ActivateOnEnter<ThinIce>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 252, NameID = 5629)]
public class O1NAlteRoite(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
