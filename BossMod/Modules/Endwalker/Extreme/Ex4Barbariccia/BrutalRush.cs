namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    class BrutalRush : Components.SelfTargetedAOEs
    {
        private BitMask _pendingRushes;
        public bool HavePendingRushes => _pendingRushes.Any();

        public BrutalRush() : base(ActionID.MakeSpell(AID.BrutalGust), new AOEShapeRect(40, 2)) { }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.BrutalRush)
                _pendingRushes.Set(module.Raid.FindSlot(source.InstanceID));
        }

        public override void OnUntethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if (tether.ID == (uint)TetherID.BrutalRush)
                _pendingRushes.Clear(module.Raid.FindSlot(source.InstanceID));
        }
    }
}
