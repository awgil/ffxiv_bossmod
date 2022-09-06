namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    class BoldBoulderTrample : Components.StackSpread
    {
        public BoldBoulderTrample() : base(6, 20, 6) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.BoldBoulder)
                SpreadMask.Set(module.Raid.FindSlot(spell.TargetID));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.BoldBoulder)
                SpreadMask.Clear(module.Raid.FindSlot(spell.TargetID));
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            if ((IconID)iconID == IconID.Trample)
                StackMask.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.Trample)
                StackMask.Clear(module.Raid.FindSlot(spell.MainTargetID));
        }
    }
}
