namespace BossMod.Stormblood.Ultimate.UCOB
{
    // TODO: generalize to tankswap
    class P2Ravensbeak : BossComponent
    {
        private Actor? _caster;
        private ulong _targetId;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_caster == null || _caster.TargetID != _targetId)
                return;

            if (actor.InstanceID == _targetId)
                hints.Add("Pass aggro!");
            else if (actor.Role == Role.Tank)
                hints.Add("Taunt!");
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Ravensbeak)
            {
                _caster = caster;
                _targetId = spell.TargetID;
            }
        }
    }
}
