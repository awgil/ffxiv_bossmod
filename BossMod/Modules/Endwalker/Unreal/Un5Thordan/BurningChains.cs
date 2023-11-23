namespace BossMod.Endwalker.Unreal.Un5Thordan;

// TODO: consider showing tethers only if distance is too small?..
class BurningChains : Components.CastCounter
{
    private int[] _tetherPartners = Utils.MakeArray(PartyState.MaxPartySize, -1);

    public BurningChains() : base(ActionID.MakeSpell(AID.HolyChain)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_tetherPartners[slot] >= 0)
            hints.Add("Break chains!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var partner = module.Raid[_tetherPartners[pcSlot]];
        if (partner != null)
            arena.AddLine(pc.Position, partner.Position, ArenaColor.Danger);
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BurningChains)
        {
            var src = module.Raid.FindSlot(source.InstanceID);
            var tgt = module.Raid.FindSlot(tether.Target);
            if (src >= 0 && tgt >= 0)
            {
                _tetherPartners[src] = tgt;
                _tetherPartners[tgt] = src;
            }
        }
    }

    public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.BurningChains)
        {
            var src = module.Raid.FindSlot(source.InstanceID);
            var tgt = module.Raid.FindSlot(tether.Target);
            if (src >= 0)
                _tetherPartners[src] = -1;
            if (tgt >= 0)
                _tetherPartners[tgt] = -1;
        }
    }
}
