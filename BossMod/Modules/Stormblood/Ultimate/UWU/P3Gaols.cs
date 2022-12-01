namespace BossMod.Stormblood.Ultimate.UWU
{
    class P3Gaols : BossComponent
    {
        private BitMask _targets;

        public bool Active => _targets.Any();

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.RockThrowBoss:
                case AID.RockThrowHelper:
                    _targets.Set(module.Raid.FindSlot(spell.MainTargetID));
                    break;
            }
        }
    }
}
