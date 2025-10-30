namespace BossMod.Components;

public class StackTogether(BossModule module, uint iconId, float activationDelay, float radius = 3) : BossComponent(module)
{
    private BitMask Targets;
    private DateTime Activation;
    public readonly uint Icon = iconId;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == Icon && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            Targets.Set(slot);
            if (Activation == default)
                Activation = WorldState.FutureTime(activationDelay);
        }
    }

    // this type of mechanic is usually only indicated by an icon (which has a fixed animation length) and we only get a cast event from the server if someone fails the mechanic, so we just have to make an educated guess of when the mechanic has resolved
    public override void Update()
    {
        if (Activation != default && Activation < WorldState.CurrentTime)
        {
            Activation = default;
            Targets.Reset();
        }
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
        if (!Targets[slot])
            return;

        var otherTargets = ShapeContains.Intersection([.. OtherTargets(slot, actor).Select(t => ShapeContains.Donut(t.Position, radius, 100))]);
        hints.AddForbiddenZone(otherTargets, Activation);
    }
}
