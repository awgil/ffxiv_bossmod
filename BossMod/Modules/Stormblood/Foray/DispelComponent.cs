namespace BossMod.Stormblood.Foray;

public class DispelComponent(BossModule module, uint statusID, ActionID action = default) : Components.CastHint(module, action, "Prepare to dispel!")
{
    private readonly List<Actor> Targets = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in Targets)
            if (hints.FindEnemy(t) is { } target)
                target.ShouldBeDispelled = true;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Targets.Count > 0)
            hints.Add("Dispel!", false);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == statusID)
            Targets.Add(actor);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == statusID)
            Targets.Remove(actor);
    }
}
