namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class SableThread : Components.GenericWildCharge
{
    public SableThread() : base(6, ActionID.MakeSpell(AID.SableThreadAOE))
    {
        FixedLength = 60;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if ((AID)spell.Action.ID == AID.SableThreadTarget)
        {
            Source = caster;
            foreach (var (i, p) in module.Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
        }
    }
}
