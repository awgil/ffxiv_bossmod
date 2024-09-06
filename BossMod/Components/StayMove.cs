namespace BossMod.Components;

// component for mechanics that either require players to move or stay still
public class StayMove(BossModule module, float maxTimeToShowHint = float.PositiveInfinity) : BossComponent(module)
{
    public enum Requirement { None, Stay, Move }
    public record struct PlayerState(Requirement Requirement, DateTime Activation);

    public readonly PlayerState[] PlayerStates = new PlayerState[PartyState.MaxAllianceSize];
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
}
