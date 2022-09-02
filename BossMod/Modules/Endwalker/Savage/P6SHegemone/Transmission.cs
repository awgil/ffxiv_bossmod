using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P6SHegemone
{
    // note: we use statuses rather than tethers, even though tethers allow predicting status significantly earlier
    // reason is that we want to activate component late for second mechanic, and tethers disappear quickly
    class Transmission : Components.CastCounter
    {
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
            if (_clippedByOthers[slot])
                hints.Add("Avoid transmission aoe!");
        }

        public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        {
            return _clips[playerSlot, pcSlot] ? PlayerPriority.Danger : _clippedByOthers[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var e in ActiveAOEs(module))
                _shape.Draw(arena, e.player.Position, e.direction);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.Glossomorph:
                    _snakeInfection.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.Chelomorph:
                    _wingInfection.Set(module.Raid.FindSlot(actor.InstanceID));
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
    }
}
