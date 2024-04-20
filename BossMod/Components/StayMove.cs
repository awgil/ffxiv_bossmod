namespace BossMod.Components;

// component for mechanics that either require players to move or stay still
public class StayMove(BossModule module) : BossComponent(module)
{
    public enum Requirement { None, Stay, Move }

    public Requirement[] Requirements = new Requirement[PartyState.MaxPartySize];
    private readonly (Vector3 prev, Vector3 curr)[] _lastPositions = new (Vector3, Vector3)[PartyState.MaxPartySize];

    public override void Update()
    {
        for (int i = 0; i < _lastPositions.Length; ++i)
            _lastPositions[i] = (_lastPositions[i].curr, Raid[i]?.PosRot.XYZ() ?? default);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        switch (Requirements[slot])
        {
            case Requirement.Stay:
                hints.Add("Stay!", _lastPositions[slot].prev != _lastPositions[slot].curr || actor.CastInfo != null || actor.TargetID != 0); // note: assume if target is selected, we might autoattack...
                break;
            case Requirement.Move:
                hints.Add("Move!", _lastPositions[slot].prev == _lastPositions[slot].curr);
                break;
        }
    }
}
