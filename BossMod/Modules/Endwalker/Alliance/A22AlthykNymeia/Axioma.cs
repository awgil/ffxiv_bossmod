namespace BossMod.Endwalker.Alliance.A22AlthykNymeia
{
    // TODO: show zone shapes
    class Axioma : BossComponent
    {
        public bool ShouldBeInZone { get; private set; }
        private BitMask _inZone;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_inZone[slot] != ShouldBeInZone)
                hints.Add(ShouldBeInZone ? "Go to dark zone!" : "GTFO from dark zone!");
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Heavy)
                _inZone.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Heavy)
                _inZone.Clear(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.InexorablePullAOE)
                ShouldBeInZone = true;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.InexorablePullAOE)
                ShouldBeInZone = false;
        }
    }
}
