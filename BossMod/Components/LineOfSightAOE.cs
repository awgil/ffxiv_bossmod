namespace BossMod.Components;

// generic component that shows line-of-sight cones for arbitrary origin and blocking shapes
[SkipLocalsInit]
public abstract class GenericLineOfSightAOE(BossModule module, uint aid, float maxRange, bool blockersImpassable = false, bool rect = false, bool safeInsideHitbox = true) : GenericAOEs(module, aid, "Hide behind obstacle!")
{
    public DateTime NextExplosion;
    public readonly bool BlockersImpassable = blockersImpassable;
    public readonly bool SafeInsideHitbox = safeInsideHitbox;
    public readonly float MaxRange = maxRange;
    public readonly bool Rect = rect; // if the AOE is a rectangle instead of a circle
    public BitMask IgnoredPlayers;
    public WPos? Origin; // inactive if null
    public readonly List<(WPos Center, float Radius)> Blockers = [];
    public readonly List<(float Distance, Angle Dir, Angle HalfWidth)> Visibility = [];
    public readonly List<AOEInstance> Safezones = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Safezones.Count != 0 && !IgnoredPlayers[slot] ? CollectionsMarshal.AsSpan(Safezones)[..1] : [];

    public void Modify(WPos? origin, IEnumerable<(WPos Center, float Radius)> blockers, DateTime nextExplosion = default)
    {
        NextExplosion = nextExplosion;
        Origin = origin;
        Blockers.Clear();
        Blockers.AddRange(blockers);
        Visibility.Clear();
        if (origin != null)
        {
            var count = Blockers.Count;
            for (var i = 0; i < count; ++i)
            {
                var b = Blockers[i];
                var toBlock = b.Center - origin.Value;
                var dist = toBlock.Length();
                Visibility.Add((dist, Angle.FromDirection(toBlock), b.Radius < dist ? Angle.Asin(b.Radius / dist) : 90f.Degrees()));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        if (len == 0)
        {
            return;
        }

        for (var i = 0; i < len; ++i)
        {
            ref readonly var c = ref aoes[i];
            if (c.Risky && !c.Check(actor.Position))
            {
                if (Origin != null && ((WPos)Origin - actor.Position).LengthSq() < MaxRange * MaxRange)
                {
                    hints.Add(WarningText);
                    return;
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            AddSafezone(Module.CastFinishAt(spell), spell.Rotation);
        }
    }

    public void AddSafezone(DateTime activation, Angle rotation = default)
    {
        List<Shape> unionShapes = [with(4)];
        List<Shape> differenceShapes = [with(4)];
        if (Origin != null)
        {
            if (!Rect)
            {
                var count = Visibility.Count;
                for (var i = 0; i < count; ++i)
                {
                    var v = Visibility[i];
                    unionShapes.Add(new DonutSegmentHA(Origin.Value, v.Distance + 0.2f, MaxRange, v.Dir, v.HalfWidth));
                }
            }
            else if (Rect)
            {
                var count = Blockers.Count;
                for (var i = 0; i < count; ++i)
                {
                    var b = Blockers[i];
                    var dir = rotation.ToDirection();
                    unionShapes.Add(new RectangleSE(b.Center + 0.2f * dir, b.Center + MaxRange * dir, b.Radius));
                }
            }
            if (BlockersImpassable || !SafeInsideHitbox)
            {
                var count = Blockers.Count;
                for (var i = 0; i < count; ++i)
                {
                    var b = Blockers[i];
                    differenceShapes.Add(new Circle(b.Center, !SafeInsideHitbox ? b.Radius : b.Radius + 0.5f));
                }
            }
            if (unionShapes.Count != 0)
            {
                var origin = Arena.Center;
                var shape = new AOEShapeCustom(origin, [.. unionShapes], [.. differenceShapes], invertForbiddenZone: true);
                Safezones.Add(new(shape, origin, default, activation, Colors.SafeFromAOE, shapeDistance: shape.Distance(origin, default)));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Safezones.Count != 0 && spell.Action.ID == WatchedAction)
        {
            Safezones.RemoveAt(0);
        }
    }
}

// simple line-of-sight aoe that happens at the end of the cast
[SkipLocalsInit]
public abstract class CastLineOfSightAOE : GenericLineOfSightAOE
{
    public readonly List<Actor> Casters = [];
    public Actor? ActiveCaster
    {
        get
        {
            var count = Casters.Count;
            if (count == 0)
            {
                return null;
            }

            Actor? activeCaster = null;
            var minRemainingTime = double.MaxValue;
            for (var i = 0; i < count; ++i)
            {
                var caster = Casters[i];
                if (caster.CastInfo != null && caster.CastInfo.RemainingTime < minRemainingTime)
                {
                    minRemainingTime = caster.CastInfo.RemainingTime;
                    activeCaster = caster;
                }
            }
            return activeCaster;
        }
    }

    protected CastLineOfSightAOE(BossModule module, uint aid, float maxRange, bool blockersImpassable = false, bool rect = false, bool safeInsideHitbox = true) : base(module, aid, maxRange, blockersImpassable, rect, safeInsideHitbox) => Refresh();

    public abstract ReadOnlySpan<Actor> BlockerActors();

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Add(caster);
            Refresh();
            AddSafezone(Module.CastFinishAt(spell), spell.Rotation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            Casters.Remove(caster);
            Refresh();
            if (Safezones.Count != 0)
            {
                Safezones.RemoveAt(0);
            }
        }
    }

    protected void Refresh()
    {
        var caster = ActiveCaster;
        WPos? position = caster != null ? caster.CastInfo!.LocXZ : null;
        var blockers = BlockerActors();
        var len = blockers.Length;
        var blockerData = new (WPos, float)[len];

        for (var i = 0; i < len; ++i)
        {
            ref readonly var b = ref blockers[i];
            blockerData[i] = (b.Position, b.HitboxRadius);
        }
        Modify(position, blockerData, Module.CastFinishAt(caster?.CastInfo));
    }
}

[SkipLocalsInit]
public abstract class CastLineOfSightAOEComplex(BossModule module, uint aid, RelSimplifiedComplexPolygon blockerShape, int maxCasts = int.MaxValue, double riskyWithSecondsLeft = default, float maxRange = default) : GenericAOEs(module, aid)
{
    public readonly RelSimplifiedComplexPolygon BlockerShape = blockerShape;
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public uint Color; // can be customized if needed
    public bool Risky = true; // can be customized if needed
    public int? MaxDangerColor;
    public int? MaxRisky; // set a maximum amount of AOEs that are considered risky
    public readonly double RiskyWithSecondsLeft = riskyWithSecondsLeft; // can be used to delay risky status of AOEs, so AI waits longer to dodge, if 0 it will just use the bool Risky
    public readonly float MaxRange = maxRange; // useful if the AOE is smaller than the arena size

    public readonly List<AOEInstance> AOEs = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return [];
        }
        var time = WorldState.CurrentTime;
        var max = count > MaxCasts ? MaxCasts : count;
        var hasMaxDangerColor = count > MaxDangerColor;

        var aoes = CollectionsMarshal.AsSpan(AOEs);
        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref aoes[i];
            var color = (hasMaxDangerColor && i < MaxDangerColor) ? Colors.Danger : Color;
            var risky = Risky && (MaxRisky == null || i < MaxRisky);

            if (RiskyWithSecondsLeft != default)
            {
                risky &= aoe.Activation.AddSeconds(-RiskyWithSecondsLeft) <= time;
            }
            aoe.Color = color;
            aoe.Risky = risky;
        }
        return aoes[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var center = Arena.Center;
            var pos = caster.Position; // these LoS casts seem to typically use caster.Position instead of spell.LocXz
            var shape = new AOEShapeCustom(center, [new PolygonCustomRel(BlockerShape.Visibility(pos - center))],
            MaxRange != default ? [new DonutV(pos, MaxRange, 1000f, 64)] : null);
            AOEs.Add(new(shape, center, default, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(center, default)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var count = AOEs.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
