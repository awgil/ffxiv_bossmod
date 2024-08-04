namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class TempestuousTear : Components.GenericWildCharge
{
    public TempestuousTear(BossModule module) : base(module, 3, ActionID.MakeSpell(AID.TempestuousTearAOE), 100)
    {
        Array.Fill(PlayerRoles, PlayerRole.Share, 0, PartyState.MaxPartySize);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TempestuousTearTargetSelect:
                Source = caster;
                var slot = Module.Raid.FindSlot(spell.MainTargetID);
                if (slot >= 0)
                    PlayerRoles[slot] = PlayerRole.Target;
                break;
            case AID.TempestuousTearAOE:
                ++NumCasts;
                break;
        }
    }
}
