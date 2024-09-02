namespace BossMod.Components;

// generic component for mechanics that require baiting some aoe (by proximity, by tether, etc) away from raid
// some players can be marked as 'forbidden' - if any of them is baiting, they are warned
// otherwise we show own bait as as outline (and warn if player is clipping someone) and other baits as filled (and warn if player is being clipped)
public class GenericBaitAway(BossModule module, ActionID aid = default, bool alwaysDrawOtherBaits = true, bool centerAtTarget = false) : CastCounter(module, aid)
{
    public record struct Bait(Actor Source, Actor Target, AOEShape Shape, DateTime Activation = default)
    {
        public readonly Angle Rotation => Source != Target ? Angle.FromDirection(Target.Position - Source.Position) : Source.Rotation;
    }

    public bool AlwaysDrawOtherBaits = alwaysDrawOtherBaits; // if false, other baits are drawn only if they are clipping a player
    public bool CenterAtTarget = centerAtTarget; // if true, aoe source is at target
    public bool AllowDeadTargets = true; // if false, baits with dead targets are ignored
    public bool EnableHints = true;
    public bool IgnoreOtherBaits; // if true, don't show hints/aoes for baits by others
    public PlayerPriority BaiterPriority = PlayerPriority.Interesting;
    public BitMask ForbiddenPlayers; // these players should avoid baiting
    public List<Bait> CurrentBaits = [];

    public IEnumerable<Bait> ActiveBaits => AllowDeadTargets ? CurrentBaits : CurrentBaits.Where(b => !b.Target.IsDead);
    public IEnumerable<Bait> ActiveBaitsOn(Actor target) => ActiveBaits.Where(b => b.Target == target);
    public IEnumerable<Bait> ActiveBaitsNotOn(Actor target) => ActiveBaits.Where(b => b.Target != target);
    public WPos BaitOrigin(Bait bait) => (CenterAtTarget ? bait.Target : bait.Source).Position;
    public bool IsClippedBy(Actor actor, Bait bait) => bait.Shape.Check(actor.Position, BaitOrigin(bait), bait.Rotation);
    public IEnumerable<Actor> PlayersClippedBy(Bait bait) => Raid.WithoutSlot().Exclude(bait.Target).InShape(bait.Shape, BaitOrigin(bait), bait.Rotation);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints)
            return;

        if (ForbiddenPlayers[slot])
        {
            if (ActiveBaitsOn(actor).Any())
                hints.Add("Avoid baiting!");
        }
        else
        {
            if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(b).Any()))
                hints.Add("Bait away from raid!");
        }

        if (!IgnoreOtherBaits && ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from baited aoe!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaits)
        {
            if (b.Target != actor)
            {
                hints.AddForbiddenZone(b.Shape, BaitOrigin(b), b.Rotation, b.Activation);
            }
            else
            {
                // add forbidden zones for as if all other players were baiting; this works fine only in common cases
                foreach (var p in Raid.WithoutSlot().Exclude(actor))
                {
                    if (CenterAtTarget)
                        hints.AddForbiddenZone(b.Shape, p.Position, b.Rotation, b.Activation);
                    else if (b.Source != b.Target)
                        hints.AddForbiddenZone(b.Shape, b.Source.Position, Angle.FromDirection(b.Target.Position - b.Source.Position), b.Activation);
                }
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => ActiveBaitsOn(player).Any() ? BaiterPriority : PlayerPriority.Irrelevant;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!IgnoreOtherBaits)
            foreach (var bait in ActiveBaitsNotOn(pc))
                if (AlwaysDrawOtherBaits || IsClippedBy(pc, bait))
                    bait.Shape.Draw(Arena, BaitOrigin(bait), bait.Rotation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaitsOn(pc))
        {
            bait.Shape.Outline(Arena, BaitOrigin(bait), bait.Rotation);
        }
    }
}

// bait on all players, requiring everyone to spread out
public class BaitAwayEveryone : GenericBaitAway
{
    public BaitAwayEveryone(BossModule module, Actor? source, AOEShape shape, ActionID aid = default) : base(module, aid)
    {
        AllowDeadTargets = false;
        if (source != null)
            CurrentBaits.AddRange(Raid.WithoutSlot(true).Select(p => new Bait(source, p, shape)));
    }
}

// component for mechanics requiring tether targets to bait their aoe away from raid
public class BaitAwayTethers(BossModule module, AOEShape shape, uint tetherID, ActionID aid = default) : GenericBaitAway(module, aid)
{
    public AOEShape Shape = shape;
    public uint TID = tetherID;
    public bool DrawTethers = true;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (DrawTethers)
        {
            foreach (var b in ActiveBaits)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                Arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            CurrentBaits.Add(new(enemy, player, Shape));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        var (player, enemy) = DetermineTetherSides(source, tether);
        if (player != null && enemy != null)
        {
            CurrentBaits.RemoveAll(b => b.Source == enemy && b.Target == player);
        }
    }

    // we support both player->enemy and enemy->player tethers
    private (Actor? player, Actor? enemy) DetermineTetherSides(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != TID)
            return (null, null);

        var target = WorldState.Actors.Find(tether.Target);
        if (target == null)
            return (null, null);

        var (player, enemy) = source.Type is ActorType.Player or ActorType.DutySupport ? (source, target) : (target, source);
        if (!(player.Type is ActorType.Player or ActorType.DutySupport) || enemy.Type == ActorType.Player)
        {
            ReportError($"Unexpected tether pair: {source.InstanceID:X} -> {target.InstanceID:X}");
            return (null, null);
        }

        return (player, enemy);
    }
}

// component for mechanics requiring icon targets to bait their aoe away from raid
public class BaitAwayIcon(BossModule module, AOEShape shape, uint iconID, ActionID aid = default, float activationDelay = 5.1f, bool centerAtTarget = false) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget)
{
    public AOEShape Shape = shape;
    public uint IID = iconID;
    public float ActivationDelay = activationDelay;

    public virtual Actor? BaitSource(Actor target) => Module.PrimaryActor;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == IID && BaitSource(actor) is var source && source != null)
            CurrentBaits.Add(new(source, actor, Shape, WorldState.FutureTime(ActivationDelay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}

// component for mechanics requiring cast targets to gtfo from raid (aoe tankbusters etc)
public class BaitAwayCast(BossModule module, ActionID aid, AOEShape shape, bool centerAtTarget = false, bool endsOnCastEvent = false) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget)
{
    public AOEShape Shape = shape;
    public bool EndsOnCastEvent = endsOnCastEvent;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, Shape, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && !EndsOnCastEvent)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction && EndsOnCastEvent)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}

// a variation of BaitAwayCast for charges that end at target
public class BaitAwayChargeCast(BossModule module, ActionID aid, float halfWidth) : GenericBaitAway(module, aid)
{
    public float HalfWidth = halfWidth;

    public override void Update()
    {
        foreach (ref var b in CurrentBaits.AsSpan())
        {
            if (b.Shape is AOEShapeRect shape)
            {
                var length = (b.Target.Position - b.Source.Position).Length();
                if (shape.LengthFront != length)
                {
                    b.Shape = shape with { LengthFront = length };
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, new AOEShapeRect(0, HalfWidth), Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}
