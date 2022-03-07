using System.Linq;
using System.Numerics;

namespace BossMod.P1S
{
    using static BossModule;

    // state related to normal and fourfold shackles
    // TODO: provide movement hint for fourfold shackles (timer strat)
    class Shackles : Component
    {
        private P1S _module;
        private bool _active = false;
        private byte _debuffsBlueImminent = 0;
        private byte _debuffsBlueFuture = 0;
        private byte _debuffsRedImminent = 0;
        private byte _debuffsRedFuture = 0;
        private ulong _blueTetherMatrix = 0;
        private ulong _redTetherMatrix = 0; // bit (8*i+j) is set if there is a tether from j to i; bit [i,i] is always set
        private ulong _blueExplosionMatrix = 0;
        private ulong _redExplosionMatrix = 0; // bit (8*i+j) is set if player i is inside explosion of player j; bit [i,i] is never set

        private static float _blueExplosionRadius = 4;
        private static float _redExplosionRadius = 8;
        private static uint TetherColor(bool blue, bool red) => blue ? (red ? 0xff00ffff : 0xffff0080) : 0xff8080ff;

        public Shackles(P1S module)
        {
            _module = module;
        }

        public int NumDebuffs() => BitOperations.PopCount((uint)_debuffsBlueFuture | _debuffsBlueImminent | _debuffsRedFuture | _debuffsRedImminent);

        public override void Update()
        {
            _blueTetherMatrix = _redTetherMatrix = _blueExplosionMatrix = _redExplosionMatrix = 0;
            byte blueDebuffs = (byte)(_debuffsBlueImminent | _debuffsBlueFuture);
            byte redDebuffs = (byte)(_debuffsRedImminent | _debuffsRedFuture);
            _active = (blueDebuffs | redDebuffs) != 0;
            if (!_active)
                return; // nothing to do...

            // update tether matrices
            foreach ((int iSrc, var src) in _module.Raid.WithSlot())
            {
                // blue => 3 closest
                if (BitVector.IsVector8BitSet(blueDebuffs, iSrc))
                {
                    BitVector.SetMatrix8x8Bit(ref _blueTetherMatrix, iSrc, iSrc, true);
                    foreach ((int iTgt, _) in _module.Raid.WithSlot().Exclude(iSrc).SortedByRange(src.Position).Take(3))
                        BitVector.SetMatrix8x8Bit(ref _blueTetherMatrix, iTgt, iSrc, true);
                }

                // red => 3 furthest
                if (BitVector.IsVector8BitSet(redDebuffs, iSrc))
                {
                    BitVector.SetMatrix8x8Bit(ref _redTetherMatrix, iSrc, iSrc, true);
                    foreach ((int iTgt, _) in _module.Raid.WithSlot().Exclude(iSrc).SortedByRange(src.Position).TakeLast(3))
                        BitVector.SetMatrix8x8Bit(ref _redTetherMatrix, iTgt, iSrc, true);
                }
            }

            // update explosion matrices and detect problems (has to be done in a separate pass)
            foreach ((int i, var actor) in _module.Raid.WithSlot())
            {
                if (BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, i) != 0)
                    foreach ((int j, _) in _module.Raid.WithSlot().InRadiusExcluding(actor, _blueExplosionRadius))
                        BitVector.SetMatrix8x8Bit(ref _blueExplosionMatrix, j, i, true);

                if (BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, i) != 0)
                    foreach ((int j, _) in _module.Raid.WithSlot().InRadiusExcluding(actor, _redExplosionRadius))
                        BitVector.SetMatrix8x8Bit(ref _redExplosionMatrix, j, i, true);
            }
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, slot) != 0 && BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, slot) != 0)
            {
                hints.Add("Target of two tethers!");
            }
            if (BitVector.ExtractVectorFromMatrix8x8(_blueExplosionMatrix, slot) != 0 || BitVector.ExtractVectorFromMatrix8x8(_redExplosionMatrix, slot) != 0)
            {
                hints.Add("GTFO from explosion!");
            }
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (!_active)
                return;

            bool drawBlueAroundMe = false;
            bool drawRedAroundMe = false;
            foreach ((int i, var actor) in _module.Raid.WithSlot())
            {
                // draw tethers
                var blueTetheredTo = BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, i);
                var redTetheredTo = BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, i);
                var tetherMask = (byte)(blueTetheredTo | redTetheredTo);
                if (tetherMask != 0)
                {
                    arena.Actor(actor, TetherColor(blueTetheredTo != 0, redTetheredTo != 0));
                    foreach ((int j, var target) in _module.Raid.WithSlot(true))
                    {
                        if (i != j && BitVector.IsVector8BitSet(tetherMask, j))
                        {
                            arena.AddLine(actor.Position, target.Position, TetherColor(BitVector.IsVector8BitSet(blueTetheredTo, j), BitVector.IsVector8BitSet(redTetheredTo, j)));
                        }
                    }
                }

                // draw explosion circles that hit me
                if (BitVector.IsMatrix8x8BitSet(_blueExplosionMatrix, pcSlot, i))
                    arena.AddCircle(actor.Position, _blueExplosionRadius, arena.ColorDanger);
                if (BitVector.IsMatrix8x8BitSet(_redExplosionMatrix, pcSlot, i))
                    arena.AddCircle(actor.Position, _redExplosionRadius, arena.ColorDanger);

                drawBlueAroundMe |= BitVector.IsMatrix8x8BitSet(_blueExplosionMatrix, i, pcSlot);
                drawRedAroundMe |= BitVector.IsMatrix8x8BitSet(_redExplosionMatrix, i, pcSlot);
            }

            // draw explosion circles if I hit anyone
            if (drawBlueAroundMe)
                arena.AddCircle(pc.Position, _blueExplosionRadius, arena.ColorDanger);
            if (drawRedAroundMe)
                arena.AddCircle(pc.Position, _redExplosionRadius, arena.ColorDanger);
        }

        public override void OnStatusGain(Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.ShacklesOfCompanionship0:
                case SID.ShacklesOfCompanionship1:
                case SID.ShacklesOfCompanionship2:
                case SID.ShacklesOfCompanionship3:
                case SID.ShacklesOfCompanionship4:
                    ModifyDebuff(actor, ref _debuffsBlueFuture, true);
                    break;
                case SID.ShacklesOfLoneliness0:
                case SID.ShacklesOfLoneliness1:
                case SID.ShacklesOfLoneliness2:
                case SID.ShacklesOfLoneliness3:
                case SID.ShacklesOfLoneliness4:
                    ModifyDebuff(actor, ref _debuffsRedFuture, true);
                    break;
                case SID.InescapableCompanionship:
                    ModifyDebuff(actor, ref _debuffsBlueImminent, true);
                    break;
                case SID.InescapableLoneliness:
                    ModifyDebuff(actor, ref _debuffsRedImminent, true);
                    break;
            }
        }

        public override void OnStatusLose(Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.ShacklesOfCompanionship0:
                case SID.ShacklesOfCompanionship1:
                case SID.ShacklesOfCompanionship2:
                case SID.ShacklesOfCompanionship3:
                case SID.ShacklesOfCompanionship4:
                    ModifyDebuff(actor, ref _debuffsBlueFuture, false);
                    break;
                case SID.ShacklesOfLoneliness0:
                case SID.ShacklesOfLoneliness1:
                case SID.ShacklesOfLoneliness2:
                case SID.ShacklesOfLoneliness3:
                case SID.ShacklesOfLoneliness4:
                    ModifyDebuff(actor, ref _debuffsRedFuture, false);
                    break;
                case SID.InescapableCompanionship:
                    ModifyDebuff(actor, ref _debuffsBlueImminent, false);
                    break;
                case SID.InescapableLoneliness:
                    ModifyDebuff(actor, ref _debuffsRedImminent, false);
                    break;
            }
        }

        private void ModifyDebuff(Actor actor, ref byte vector, bool active)
        {
            int slot = _module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                BitVector.ModifyVector8Bit(ref vector, slot, active);
        }
    }
}
