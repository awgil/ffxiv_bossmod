namespace BossMod.Components;

public class StackTogether(BossModule module, uint iconId, float activationDelay, Enum? onSuccess = null, float radius = 3) : BossComponent(module)
{
    private BitMask Targets;
    private DateTime Activation;
    public readonly uint Icon = iconId;
    public readonly uint SpellId = onSuccess != null ? (uint)(object)onSuccess : 0;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == Icon && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            Targets.Set(slot);
            if (Activation == default)
                Activation = WorldState.FutureTime(activationDelay);
        }
    }

    // two common variants of this mechanic
    // 1. icon appears, nothing happens if you resolve correctly, we just have to clear the mask after the deadline
    // 2. icon appears, player takes minimal damage if resolved correctly
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == SpellId)
            OnResolve();
    }

    public override void Update()
    {
        if (Activation != default && Activation < WorldState.CurrentTime && SpellId == 0)
            OnResolve();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Targets[slot])
            hints.Add("Stack with other targets!", !OtherTargets(slot, actor).Any(t => t.Position.InCircle(actor.Position, radius)));
    }

    private IEnumerable<Actor> OtherTargets(int slot, Actor actor) => Targets[slot] ? Raid.WithSlot().IncludedInMask(Targets).Exclude(actor).Select(t => t.Item2) : [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Targets[pcSlot])
            foreach (var target in OtherTargets(pcSlot, pc))
                Arena.AddCircle(target.Position, radius, ArenaColor.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Targets.Any() && SpellId > 0)
            hints.PredictedDamage.Add(new(Targets, Activation, AIHints.PredictedDamageType.Shared));

        if (!Targets[slot])
            return;

        var otherTargets = ShapeDistance.Intersection([.. OtherTargets(slot, actor).Select(t => ShapeDistance.InvertedCircle(t.Position, radius))]);
        hints.AddForbiddenZone(otherTargets, Activation);
    }

    private void OnResolve()
    {
        Activation = default;
        Targets.Reset();
    }
}
