namespace BossMod.Endwalker.Savage.P6SHegemone
{
    // TODO: improve...
    class Aetheronecrosis : Components.CastCounter
    {
        private int[] _orders = new int[PartyState.MaxPartySize];

        public Aetheronecrosis() : base(ActionID.MakeSpell(AID.Aetheronecrosis)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_orders[slot] > 0)
                hints.Add($"Order: {_orders[slot]}", false);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Aetheronecrosis)
            {
                var slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    _orders[slot] = (status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds switch
                    {
                        < 10 => 2,
                        < 14 => 3,
                        < 18 => 4,
                        _ => 1,
                    };
                }
            }
        }
    }
}
