namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator
{
    class SculptorsPassion : Components.GenericWildCharge
    {
        public SculptorsPassion(AID aid) : base(4, ActionID.MakeSpell(aid)) { }

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
    class NSculptorsPassion : SculptorsPassion { public NSculptorsPassion() : base(AID.NSculptorsPassion) { } }
    class SSculptorsPassion : SculptorsPassion { public SSculptorsPassion() : base(AID.SSculptorsPassion) { } }
}
