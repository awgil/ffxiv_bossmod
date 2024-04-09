namespace BossMod.Endwalker.Unreal.Un5Thordan;

// TODO: consider showing tethers only if distance is too small?..
class BurningChains : Components.CastCounter
{
    private int[] _tetherPartners = Utils.MakeArray(PartyState.MaxPartySize, -1);

    public BurningChains() : base(ActionID.MakeSpell(AID.HolyChain)) { }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tetherPartners[slot] >= 0)
            hints.Add("Break chains!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partner = Raid[_tetherPartners[pcSlot]];
        if (partner != null)
            arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BurningChains)
        {
            var src = Raid.FindSlot(source.InstanceID);
            var tgt = Raid.FindSlot(tether.Target);
            if (src >= 0 && tgt >= 0)
            {
                _tetherPartners[src] = tgt;
                _tetherPartners[tgt] = src;
            }
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BurningChains)
        {
            var src = Raid.FindSlot(source.InstanceID);
            var tgt = Raid.FindSlot(tether.Target);
            if (src >= 0)
                _tetherPartners[src] = -1;
            if (tgt >= 0)
                _tetherPartners[tgt] = -1;
        }
    }
}
