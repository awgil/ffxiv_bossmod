namespace BossMod.Endwalker.Savage.P6SHegemone;

// TODO: improve...
class PathogenicCells : Components.CastCounter
{
    private int[] _order = new int[PartyState.MaxPartySize];

    public PathogenicCells() : base(ActionID.MakeSpell(AID.PathogenicCellsAOE)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_order[slot] != 0)
            hints.Add($"Order: {_order[slot]}", false);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID is >= (uint)IconID.Pathogenic1 and <= (uint)IconID.Pathogenic8)
        {
            var slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _order[slot] = (int)iconID - (int)IconID.Pathogenic1 + 1;
        }
    }
}
