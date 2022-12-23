using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P6SHegemone
{
    class Transmission : Components.CastCounter
    {
        private DateTime[] _infectionExpire = new DateTime[PartyState.MaxPartySize]; // when status expires, it will be replaced with stun - we show aoes for last few seconds only
        private BitMask _snakeInfection; // hits front
        private BitMask _wingInfection; // hits back
        private BitMask _stuns;
        private BitMatrix _clips; // row = player, col = others that he clips
        private BitMask _clippedByOthers;

        private static AOEShapeCone _shape = new(60, 15.Degrees());

        public bool StunsActive => _stuns.Any();

        public Transmission() : base(ActionID.MakeSpell(AID.ReekHavoc)) { }

        public override void Update(BossModule module)
        {
            _clips.Reset();
            _clippedByOthers.Reset();
            foreach (var e in ActiveAOEs(module))
            {
                _clippedByOthers |= _clips[e.slot] = module.Raid.WithSlot().Exclude(e.player).InShape(_shape, e.player.Position, e.direction).Mask();
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_snakeInfection[slot])
                hints.Add("Face away from raid", _clips[slot].Any());
            if (_wingInfection[slot])
                hints.Add("Face raid", _clips[slot].Any());
            if (_clippedByOthers[slot] && ExpireImminent(module, slot))
                hints.Add("Avoid transmission aoe!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _clips[playerSlot, pcSlot] ? PlayerPriority.Danger : _clippedByOthers[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var e in ActiveAOEs(module))
                if (e.slot == pcSlot || ExpireImminent(module, e.slot))
                    _shape.Draw(arena, e.player.Position, e.direction);
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            switch ((TetherID)tether.ID)
            {
                case TetherID.TransmissionSnake:
                    _snakeInfection.Set(module.Raid.FindSlot(source.InstanceID));
                    break;
                case TetherID.TransmissionWing:
                    _wingInfection.Set(module.Raid.FindSlot(source.InstanceID));
                    break;
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.Glossomorph:
                case SID.Chelomorph:
                    var slot = module.Raid.FindSlot(actor.InstanceID);
                    if (slot >= 0)
                        _infectionExpire[slot] = status.ExpireAt;
                    break;
                case SID.OutOfControlSnake:
                case SID.OutOfControlWing:
                    _stuns.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.OutOfControlSnake:
                case SID.OutOfControlWing:
                    _stuns.Clear(module.Raid.FindSlot(actor.InstanceID));
                    break;
            }
        }

        private IEnumerable<(int slot, Actor player, Angle direction)> ActiveAOEs(BossModule module)
        {
            foreach (var (slot, player) in module.Raid.WithSlot(true))
            {
                if (_snakeInfection[slot])
                    yield return (slot, player, player.Rotation);
                if (_wingInfection[slot])
                    yield return (slot, player, player.Rotation + 180.Degrees());
            }
        }

        private bool ExpireImminent(BossModule module, int slot)
        {
            var expire = _infectionExpire[slot];
            return expire != default && (expire - module.WorldState.CurrentTime).TotalSeconds < 2;
        }
    }
}
