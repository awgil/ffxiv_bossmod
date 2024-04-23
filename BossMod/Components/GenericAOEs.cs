namespace BossMod.Components;

// generic component that shows arbitrary shapes representing avoidable aoes
public abstract class GenericAOEs(BossModule module, ActionID aid = default, string warningText = "GTFO from aoe!") : CastCounter(module, aid)
{
    public record struct AOEInstance(AOEShape Shape, WPos Origin, Angle Rotation = default, DateTime Activation = default, uint Color = ArenaColor.AOE, bool Risky = true)
    {
        public readonly bool Check(WPos pos) => Shape.Check(pos, Origin, Rotation);
    }

    public string WarningText = warningText;

    public abstract IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).Any(c => c.Risky && c.Check(actor.Position)))
            hints.Add(WarningText);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in ActiveAOEs(slot, actor))
            if (c.Risky)
                hints.AddForbiddenZone(c.Shape, c.Origin, c.Rotation, c.Activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in ActiveAOEs(pcSlot, pc))
            c.Shape.Draw(Arena, c.Origin, c.Rotation, c.Color);
    }
}

// self-targeted aoe that happens at the end of the cast
public class SelfTargetedAOEs(BossModule module, ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : GenericAOEs(module, aid)
{
    public AOEShape Shape { get; init; } = shape;
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public uint Color = ArenaColor.AOE; // can be customized if needed
    public bool Risky = true; // can be customized if needed
    public readonly List<Actor> Casters = [];

    public IEnumerable<Actor> ActiveCasters => Casters.Take(MaxCasts);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt, Color, Risky));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}

// self-targeted aoe that uses current caster's rotation instead of rotation from cast-info - used by legacy modules written before i've reversed real cast rotation
public class SelfTargetedLegacyRotationAOEs(BossModule module, ActionID aid, AOEShape shape, int maxCasts = int.MaxValue) : GenericAOEs(module, aid)
{
    public AOEShape Shape { get; init; } = shape;
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public readonly List<Actor> Casters = [];

    public IEnumerable<Actor> ActiveCasters => Casters.Take(MaxCasts);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, c.Position, c.Rotation, c.CastInfo!.NPCFinishAt));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}

// location-targeted aoe that happens at the end of the cast
public class LocationTargetedAOEs(BossModule module, ActionID aid, float radius, string warningText = "GTFO from puddle!", int maxCasts = int.MaxValue) : GenericAOEs(module, aid, warningText)
{
    public AOEShapeCircle Shape { get; init; } = new(radius);
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public uint Color = ArenaColor.AOE; // can be customized if needed
    public bool Risky = true; // can be customized if needed
    public readonly List<Actor> Casters = [];

    public IEnumerable<Actor> ActiveCasters => Casters.Take(MaxCasts);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, c.CastInfo!.LocXZ, c.CastInfo.Rotation, c.CastInfo.NPCFinishAt, Color, Risky));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }
}

// 'charge at location' aoes that happen at the end of the cast
public class ChargeAOEs(BossModule module, ActionID aid, float halfWidth) : GenericAOEs(module, aid)
{
    public float HalfWidth { get; init; } = halfWidth;
    public readonly List<(Actor caster, AOEShape shape, Angle direction)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(csr => new AOEInstance(csr.shape, csr.caster.Position, csr.direction, csr.caster.CastInfo!.NPCFinishAt));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var dir = spell.LocXZ - caster.Position;
            Casters.Add((caster, new AOEShapeRect(dir.Length(), HalfWidth), Angle.FromDirection(dir)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.RemoveAll(e => e.caster == caster);
    }
}
