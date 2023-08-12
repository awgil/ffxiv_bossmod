using Lumina.Data.Parsing.Layer;
using static BossMod.BossComponent;

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

        public bool ShowSafespots = true;
        private PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];

        private static float _farOffset = 13;
        private static float _nearOffset = 7;

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var state = _states[slot];
            if (state.Tether != TetherType.None)
                hints.Add($"{state.Tether} tether", state.TetherBad);
            if (movementHints != null && Safespot(module, slot, actor) is var safespot && safespot != null)
                movementHints.Add(actor.Position, safespot.Value, ArenaColor.Safe);
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
            if (Safespot(module, pcSlot, pc) is var safespot && safespot != null)
                arena.AddCircle(safespot.Value, 1, ArenaColor.Safe);
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

        // note: this uses default strats (kindred etc)
        private WPos? Safespot(BossModule module, int slot, Actor actor)
        {
            var tether = _states[slot].Tether;
            if (!ShowSafespots || tether == TetherType.None)
                return null;

            bool isFar = tether == TetherType.Far;
            Angle dir = actor.Role switch
            {
                Role.Tank => isFar ? 180.Degrees() : -90.Degrees(),
                Role.Healer => isFar ? 0.Degrees() : 90.Degrees(),
                _ => module.Raid[_states[slot].PartnerSlot]?.Role == Role.Tank ? (isFar ? -45.Degrees() : -135.Degrees()) : (isFar ? 135.Degrees() : 45.Degrees())
            };
            return module.Bounds.Center + (isFar ? _farOffset : _nearOffset) * dir.ToDirection();
        }
    }
}
