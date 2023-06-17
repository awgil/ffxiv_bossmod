namespace BossMod.Endwalker.Savage.P11SThemis
{
    class DarkAndLight : BossComponent
    {
        public enum TetherType { None, Near, Far }

        public struct PlayerState
        {
            public TetherType Tether;
            public int PartnerSlot;
            public bool TetherBad;
        }

        private PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var state = _states[slot];
            if (state.Tether != TetherType.None)
                hints.Add($"{state.Tether} tether", state.TetherBad);
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            var pcState = _states[pcSlot];
            return pcState.Tether != TetherType.None && pcState.PartnerSlot == playerSlot ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var pcState = _states[pcSlot];
            if (pcState.Tether != TetherType.None && module.Raid[pcState.PartnerSlot] is var partner && partner != null)
                arena.AddLine(pc.Position, partner.Position, pcState.TetherBad ? ArenaColor.Danger : ArenaColor.Safe);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            switch ((TetherID)tether.ID)
            {
                case TetherID.LightLightGood:
                case TetherID.DarkDarkGood:
                    UpdateTether(module.Raid.FindSlot(source.InstanceID), module.Raid.FindSlot(tether.Target), TetherType.Far, false);
                    break;
                case TetherID.LightLightBad:
                case TetherID.DarkDarkBad:
                    UpdateTether(module.Raid.FindSlot(source.InstanceID), module.Raid.FindSlot(tether.Target), TetherType.Far, true);
                    break;
                case TetherID.DarkLightGood:
                    UpdateTether(module.Raid.FindSlot(source.InstanceID), module.Raid.FindSlot(tether.Target), TetherType.Near, false);
                    break;
                case TetherID.DarkLightBad:
                    UpdateTether(module.Raid.FindSlot(source.InstanceID), module.Raid.FindSlot(tether.Target), TetherType.Near, true);
                    break;
            }
        }

        private void UpdateTether(int from, int to, TetherType type, bool bad)
        {
            if (from < 0 || to < 0)
                return;
            _states[from] = new() { Tether = type, PartnerSlot = to, TetherBad = bad };
            _states[to] = new() { Tether = type, PartnerSlot = from, TetherBad = bad };
        }
    }
}
