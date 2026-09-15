namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class TempestuousTear : Components.GenericWildCharge
{
    public TempestuousTear(BossModule module) : base(module, 3, AID.TempestuousTearAOE, 100)
    {
        Array.Fill(PlayerRoles, PlayerRole.Share, 0, PartyState.MaxPartySize);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TempestuousTearTargetSelect:
                Source = caster;
                if (Raid.TryFindSlot(spell.MainTargetID, out var slot))
                    PlayerRoles[slot] = PlayerRole.Target;
                break;
            case AID.TempestuousTearAOE:
                ++NumCasts;
                break;
        }
    }
}
