namespace BossMod.Endwalker.Criterion.C01ASS.C012GladiatorOfSildih
{
    class SculptorsPassion : Components.GenericWildCharge
    {
        public SculptorsPassion() : base(4, ActionID.MakeSpell(AID.SculptorsPassionAOE)) { }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.SculptorsPassionTargetSelection)
            {
                Source = module.PrimaryActor;
                foreach (var (slot, player) in module.Raid.WithSlot(true))
                    PlayerRoles[slot] = player.InstanceID == spell.MainTargetID ? PlayerRole.Target : player.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
            }
        }
    }
}
