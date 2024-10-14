namespace BossMod.Components;

// component for mechanics that either require players to move or stay still
// priorities can be used to simplify implementation when e.g. status changes at different stages of the mechanic (eg if prep status is replaced with pyretic, we want to allow them to happen in any sequence)
public class StayMove(BossModule module, float maxTimeToShowHint = float.PositiveInfinity) : BossComponent(module)
{
    public enum Requirement { None, Stay, Move }
    public record struct PlayerState(Requirement Requirement, DateTime Activation, int Priority = 0);

    public readonly PlayerState[] PlayerStates = new PlayerState[PartyState.MaxAllies];
    public float MaxTimeToShowHint = maxTimeToShowHint;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        switch (PlayerStates[slot].Requirement)
        {
            case Requirement.Stay:
                if (float.IsInfinity(MaxTimeToShowHint) || PlayerStates[slot].Activation <= WorldState.FutureTime(MaxTimeToShowHint))
                    hints.Add("Stay!", actor.PrevPosition != actor.PrevPosition || actor.CastInfo != null || actor.TargetID != 0); // note: assume if target is selected, we might autoattack...
                break;
            case Requirement.Move:
                if (float.IsInfinity(MaxTimeToShowHint) || PlayerStates[slot].Activation <= WorldState.FutureTime(MaxTimeToShowHint))
                    hints.Add("Move!", actor.PrevPosition == actor.PrevPosition);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        switch (PlayerStates[slot].Requirement)
        {
            case Requirement.Stay:
                hints.AddSpecialMode(AIHints.SpecialMode.Pyretic, PlayerStates[slot].Activation);
                break;
            case Requirement.Move:
                hints.AddSpecialMode(AIHints.SpecialMode.Freezing, PlayerStates[slot].Activation);
                break;
        }
    }

    // update player state, if current priority is same or lower
    protected void SetState(int slot, PlayerState state)
    {
        if (slot >= 0 && PlayerStates[slot].Priority <= state.Priority)
            PlayerStates[slot] = state;
    }

    protected void ClearState(int slot, int priority = int.MaxValue)
    {
        if (slot >= 0 && PlayerStates[slot].Priority <= priority)
            PlayerStates[slot] = default;
    }
}
