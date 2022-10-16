namespace BossMod.Endwalker.Savage.P8S1Hephaistos
{
    // TODO: add various hints for gaze/explode
    // TODO: show circle around assigned snake
    class Snake1 : PetrifactionCommon
    {
        private BitMask _firstOrder;
        private BitMask _secondOrder;
        private BitMask _debuffPetrify;
        private BitMask _debuffExplode;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);

            int order = _firstOrder[slot] ? 1 : _secondOrder[slot] ? 2 : 0;
            if (order > 0)
            {
                hints.Add($"Order: {(_debuffPetrify[slot] ? "petrify" : "explode")} {order}", false);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);

            if (_debuffPetrify[pcSlot])
                DrawPetrify(pc, _secondOrder[pcSlot] && NumCasts < 2, arena);
            else if (_debuffExplode[pcSlot])
                DrawExplode(pc, _secondOrder[pcSlot] && NumCasts < 2, arena);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.FirstInLine:
                    _firstOrder.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.SecondInLine:
                    _secondOrder.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.EyeOfTheGorgon:
                    _debuffPetrify.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.BloodOfTheGorgon:
                    _debuffExplode.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.FirstInLine:
                    _firstOrder.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.SecondInLine:
                    _secondOrder.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.EyeOfTheGorgon:
                    _debuffPetrify.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.BloodOfTheGorgon:
                    _debuffExplode.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }
    }
}
