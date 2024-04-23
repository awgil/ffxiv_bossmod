namespace BossMod.Endwalker.Savage.P6SHegemone;

// TODO: improve...
class PathogenicCells(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.PathogenicCellsAOE))
{
    private readonly int[] _order = new int[PartyState.MaxPartySize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_order[slot] != 0)
            hints.Add($"Order: {_order[slot]}", false);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID is >= (uint)IconID.Pathogenic1 and <= (uint)IconID.Pathogenic8)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _order[slot] = (int)iconID - (int)IconID.Pathogenic1 + 1;
        }
    }
}
