namespace BossMod.Components;

// generic component that shows arbitrary shapes representing avoidable aoes
public abstract class GenericAOEs(BossModule module, Enum? aid = default, string warningText = "GTFO from aoe!", string invertedText = "Go to safe zone!") : CastCounter(module, aid)
{
    public record struct AOEInstance(AOEShape Shape, WPos Origin, Angle Rotation = default, DateTime Activation = default, uint Color = 0, bool Risky = true, bool Inverted = false)
    {
        public readonly bool Check(WPos pos)
        {
            var res = Shape.Check(pos, Origin, Rotation);
            return Inverted ? !res : res;
        }
    }

    public string WarningText = warningText;
    public string InvertedText = invertedText;

    public abstract IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).FirstOrNull(c => c.Risky && c.Check(actor.Position)) is { } aoe)
            hints.Add(aoe.Inverted ? InvertedText : WarningText);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in ActiveAOEs(slot, actor))
            if (c.Risky)
                hints.AddForbiddenZone(c.Check, c.Activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in ActiveAOEs(pcSlot, pc))
        {
            var col = c.Color;
            if (col == 0 && c.Inverted)
                col = ArenaColor.SafeFromAOE;

            c.Shape.Draw(Arena, c.Origin, c.Rotation, col);
        }
    }
}

// standard AOE, targeted at some location (usually caster), resolves on cast event
public class StandardAOEs(BossModule module, Enum aid, AOEShape shape, int maxCasts = int.MaxValue, string warningText = "GTFO from aoe!", bool highlightImminent = false) : GenericAOEs(module, aid, warningText)
{
    public AOEShape Shape { get; init; } = shape;
    public int MaxCasts = maxCasts; // used for staggered aoes, when showing all active would be pointless
    public uint Color; // can be customized if needed
    public uint ColorImminent = ArenaColor.Danger;
    public bool Risky = true; // can be customized if needed
    public readonly List<Actor> Casters = [];

    public StandardAOEs(BossModule module, Enum aid, float radius, int maxCasts = int.MaxValue, string warningText = "GTFO from aoe!", bool highlightImminent = false) : this(module, aid, new AOEShapeCircle(radius), maxCasts, warningText, highlightImminent) { }

    public IEnumerable<Actor> ActiveCasters => Casters.Take(MaxCasts);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime nextActivation = default;
        foreach (var c in ActiveCasters)
        {
            var color = Color;
            var thisActivation = Module.CastFinishAt(c.CastInfo);
            if (highlightImminent)
            {
                if (nextActivation == default)
                    nextActivation = thisActivation.AddSeconds(0.5f);
                if (thisActivation < nextActivation)
                    color = ColorImminent;
            }
            yield return new AOEInstance(Shape, c.CastInfo!.LocXZ, c.CastInfo!.Rotation, thisActivation, color, Risky);
        }
    }

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

// standard AOE where multiple action IDs should be grouped together as logically the same action
public class GroupedAOEs(BossModule module, Enum[] aids, AOEShape shape, int maxCasts = int.MaxValue, bool highlightImminent = false) : StandardAOEs(module, aids[0], shape, maxCasts, highlightImminent: highlightImminent)
{
    public readonly List<ActionID> IDs = [.. aids.Select(ActionID.MakeSpell)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (IDs.Contains(spell.Action))
        {
            Casters.Add(caster);
            Casters.SortBy(c => Module.CastFinishAt(c.CastInfo));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (IDs.Contains(spell.Action))
            Casters.Remove(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (IDs.Contains(spell.Action))
            NumCasts++;
    }
}

// 'charge at location' aoes that happen at the end of the cast
public class ChargeAOEs(BossModule module, Enum aid, float halfWidth) : GenericAOEs(module, aid)
{
    public float HalfWidth { get; init; } = halfWidth;
    public readonly List<(Actor caster, AOEShape shape, Angle direction)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(csr => new AOEInstance(csr.shape, csr.caster.Position, csr.direction, Module.CastFinishAt(csr.caster.CastInfo)));

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
