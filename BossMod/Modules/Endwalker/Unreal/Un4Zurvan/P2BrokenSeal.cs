using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un4Zurvan
{
    class P2BrokenSeal : BossComponent
    {
        public enum Color { None, Fire, Ice }

        public struct PlayerState
        {
            public Color Color;
            public int Partner;
            public bool TooFar;
        }

        public int NumAssigned { get; private set; }
        public int NumCasts { get; private set; }
        private PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
        private List<Actor> _fireTowers = new();
        private List<Actor> _iceTowers = new();

        public P2BrokenSeal()
        {
            Array.Fill(_playerStates, new() { Partner = -1 });
        }

        public override void Init(BossModule module)
        {
            _fireTowers = module.Enemies(OID.FireTower);
            _iceTowers = module.Enemies(OID.IceTower);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (NumCasts > 0)
                return;

            if (_playerStates[slot].TooFar)
                hints.Add("Move closer to partner!");

            var towers = _playerStates[slot].Color switch
            {
                Color.Fire => _fireTowers,
                Color.Ice => _iceTowers,
                _ => null
            };
            if (towers?.Count > 0 && !towers.Any(t => actor.Position.InCircle(t.Position, 2)))
                hints.Add("Soak the tower!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _playerStates[pcSlot].Color != Color.None && _playerStates[pcSlot].Partner == playerSlot ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (NumCasts > 0)
                return;

            var state = _playerStates[pcSlot];
            var partner = state.Color != Color.None && state.Partner >= 0 ? module.Raid[state.Partner] : null;
            if (partner != null)
            {
                arena.AddLine(pc.Position, partner.Position, state.Color == Color.Fire ? 0xff0080ff : 0xffff8000, state.TooFar ? 2 : 1);
            }

            foreach (var t in _fireTowers)
                arena.AddCircle(t.Position, 2, state.Color == Color.Fire ? ArenaColor.Safe : ArenaColor.Danger);
            foreach (var t in _iceTowers)
                arena.AddCircle(t.Position, 2, state.Color == Color.Ice ? ArenaColor.Safe : ArenaColor.Danger);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.InfiniteFire:
                    AssignColor(module, spell.MainTargetID, Color.Fire);
                    break;
                case AID.InfiniteIce:
                    AssignColor(module, spell.MainTargetID, Color.Ice);
                    break;
                case AID.SouthStar:
                case AID.NorthStar:
                case AID.SouthStarUnsoaked:
                case AID.NorthStarUnsoaked:
                case AID.SouthStarWrong:
                case AID.NorthStarWrong:
                    ++NumCasts;
                    break;
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if ((TetherID)tether.ID is TetherID.InfiniteAnguish or TetherID.InfiniteFire or TetherID.InfiniteIce)
            {
                var from = module.Raid.FindSlot(source.InstanceID);
                var to = module.Raid.FindSlot(tether.Target);
                if (from >= 0 && to >= 0)
                {
                    _playerStates[from].Partner = to;
                    _playerStates[to].Partner = from;
                    _playerStates[from].TooFar = _playerStates[to].TooFar = (TetherID)tether.ID == TetherID.InfiniteAnguish;
                }
            }
        }

        private void AssignColor(BossModule module, ulong playerID, Color color)
        {
            ++NumAssigned;
            var slot = module.Raid.FindSlot(playerID);
            if (slot >= 0)
                _playerStates[slot].Color = color;
        }
    }
}
