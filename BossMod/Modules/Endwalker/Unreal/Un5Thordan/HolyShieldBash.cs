namespace BossMod.Endwalker.Unreal.Un5Thordan;

class HolyShieldBash(BossModule module) : Components.GenericWildCharge(module, 3)
{
    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => PlayerRoles[playerSlot] == PlayerRole.Target ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HolyShieldBash:
                foreach (var (i, p) in Raid.WithSlot(true))
                {
                    // TODO: we don't really account for possible MT changes...
                    PlayerRoles[i] = p.InstanceID == spell.TargetID ? PlayerRole.Target : p.Role != Role.Tank ? PlayerRole.ShareNotFirst : p.InstanceID != Module.PrimaryActor.TargetID ? PlayerRole.Share : PlayerRole.Avoid;
                }
                break;
            case AID.SpearOfTheFury:
                Source = caster;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HolyShieldBash:
                if (NumCasts == 0)
                    NumCasts = 1;
                break;
            case AID.SpearOfTheFury:
                Source = null;
                NumCasts = 2;
                break;
        }
    }
}
