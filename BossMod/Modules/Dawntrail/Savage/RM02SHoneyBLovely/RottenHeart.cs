namespace BossMod.Dawntrail.Savage.RM02SHoneyBLovely;

class RottenHeart(BossModule module) : Components.CastCounter(module, AID.RottenHeartAOE);

class RottenHeartBigBurst(BossModule module) : Components.CastCounter(module, AID.RottenHeartBigBurst)
{
    private int _numRaidwides;
    private readonly int[] _order = new int[PartyState.MaxPartySize];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_order[slot] > NumCasts)
            hints.Add($"Order: {_order[slot]}", ResolveImminent(slot));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _order[pcSlot] != 0 && _order[pcSlot] == _order[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partner = _order[pcSlot] > NumCasts ? FindPartner(pcSlot) : null;
        if (partner != null)
            Arena.AddLine(pc.Position, partner.Position, ResolveImminent(pcSlot) ? ArenaColor.Danger : ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.BeelovedVenomA or SID.BeelovedVenomB && Raid.TryFindSlot(actor.InstanceID, out var slot))
        {
            _order[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
            {
                < 15 => 1,
                < 30 => 2,
                < 45 => 3,
                _ => 4
            };
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.RottenHeartBigBurst:
                ++NumCasts;
                break;
            case AID.CallMeHoney:
                ++_numRaidwides;
                break;
        }
    }

    private Actor? FindPartner(int slot)
    {
        var order = _order[slot];
        if (order == 0)
            return null;
        for (int i = 0; i < _order.Length; ++i)
            if (i != slot && _order[i] == order)
                return Raid[i];
        return null;
    }

    private bool ResolveImminent(int slot) => _order[slot] == _numRaidwides + 1;
}
