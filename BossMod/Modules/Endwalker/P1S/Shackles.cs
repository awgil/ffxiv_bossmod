using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P1S
{
    using static BossModule;

    // state related to normal and fourfold shackles
    class Shackles : Component
    {
        public int NumExpiredDebuffs { get; private set; } = 0;
        private bool _active = false;
        private byte _debuffsBlueImminent = 0;
        private byte _debuffsBlueFuture = 0;
        private byte _debuffsRedImminent = 0;
        private byte _debuffsRedFuture = 0;
        private ulong _blueTetherMatrix = 0;
        private ulong _redTetherMatrix = 0; // bit (8*i+j) is set if there is a tether from j to i; bit [i,i] is always set
        private ulong _blueExplosionMatrix = 0;
        private ulong _redExplosionMatrix = 0; // bit (8*i+j) is set if player i is inside explosion of player j; bit [i,i] is never set
        private Vector3[] _preferredPositions = new Vector3[8];

        private static float _blueExplosionRadius = 4;
        private static float _redExplosionRadius = 8;
        private static uint TetherColor(bool blue, bool red) => blue ? (red ? 0xff00ffff : 0xffff0080) : (red ? 0xff8080ff : 0xff808080);

        public override void Update(BossModule module)
        {
            _blueTetherMatrix = _redTetherMatrix = _blueExplosionMatrix = _redExplosionMatrix = 0;
            byte blueDebuffs = (byte)(_debuffsBlueImminent | _debuffsBlueFuture);
            byte redDebuffs = (byte)(_debuffsRedImminent | _debuffsRedFuture);
            _active = (blueDebuffs | redDebuffs) != 0;
            if (!_active)
                return; // nothing to do...

            // update tether matrices
            foreach ((int iSrc, var src) in module.Raid.WithSlot())
            {
                // blue => 3 closest
                if (BitVector.IsVector8BitSet(blueDebuffs, iSrc))
                {
                    BitVector.SetMatrix8x8Bit(ref _blueTetherMatrix, iSrc, iSrc, true);
                    foreach ((int iTgt, _) in module.Raid.WithSlot().Exclude(iSrc).SortedByRange(src.Position).Take(3))
                        BitVector.SetMatrix8x8Bit(ref _blueTetherMatrix, iTgt, iSrc, true);
                }

                // red => 3 furthest
                if (BitVector.IsVector8BitSet(redDebuffs, iSrc))
                {
                    BitVector.SetMatrix8x8Bit(ref _redTetherMatrix, iSrc, iSrc, true);
                    foreach ((int iTgt, _) in module.Raid.WithSlot().Exclude(iSrc).SortedByRange(src.Position).TakeLast(3))
                        BitVector.SetMatrix8x8Bit(ref _redTetherMatrix, iTgt, iSrc, true);
                }
            }

            // update explosion matrices and detect problems (has to be done in a separate pass)
            foreach ((int i, var actor) in module.Raid.WithSlot())
            {
                if (BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, i) != 0)
                    foreach ((int j, _) in module.Raid.WithSlot().InRadiusExcluding(actor, _blueExplosionRadius))
                        BitVector.SetMatrix8x8Bit(ref _blueExplosionMatrix, j, i, true);

                if (BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, i) != 0)
                    foreach ((int j, _) in module.Raid.WithSlot().InRadiusExcluding(actor, _redExplosionRadius))
                        BitVector.SetMatrix8x8Bit(ref _redExplosionMatrix, j, i, true);
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, slot) != 0 && BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, slot) != 0)
            {
                hints.Add("Target of two tethers!");
            }
            if (BitVector.ExtractVectorFromMatrix8x8(_blueExplosionMatrix, slot) != 0 || BitVector.ExtractVectorFromMatrix8x8(_redExplosionMatrix, slot) != 0)
            {
                hints.Add("GTFO from explosion!");
            }

            if (movementHints != null && _preferredPositions[slot] != Vector3.Zero)
            {
                movementHints.Add(actor.Position, _preferredPositions[slot], module.Arena.ColorSafe);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!_active)
                return;

            bool drawBlueAroundMe = false;
            bool drawRedAroundMe = false;
            foreach ((int i, var actor) in module.Raid.WithSlot())
            {
                var blueTetheredTo = BitVector.ExtractVectorFromMatrix8x8(_blueTetherMatrix, i);
                var redTetheredTo = BitVector.ExtractVectorFromMatrix8x8(_redTetherMatrix, i);
                arena.Actor(actor, TetherColor(blueTetheredTo != 0, redTetheredTo != 0));

                // draw tethers
                foreach ((int j, var target) in module.Raid.WithSlot(true).Exclude(i).IncludedInMask((ulong)(blueTetheredTo | redTetheredTo)))
                    arena.AddLine(actor.Position, target.Position, TetherColor(BitVector.IsVector8BitSet(blueTetheredTo, j), BitVector.IsVector8BitSet(redTetheredTo, j)));

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

            // draw assigned spot, if any
            if (_preferredPositions[pcSlot] != Vector3.Zero)
                arena.AddCircle(_preferredPositions[pcSlot], 2, arena.ColorSafe);

        }

        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.ShacklesOfCompanionship0:
                    ModifyDebuff(module, actor, ref _debuffsBlueFuture, true);
                    break;
                case SID.ShacklesOfCompanionship1:
                    ModifyDebuff(module, actor, ref _debuffsBlueFuture, true);
                    AssignOrder(module, actor, 0, false);
                    break;
                case SID.ShacklesOfCompanionship2:
                    ModifyDebuff(module, actor, ref _debuffsBlueFuture, true);
                    AssignOrder(module, actor, 1, false);
                    break;
                case SID.ShacklesOfCompanionship3:
                    ModifyDebuff(module, actor, ref _debuffsBlueFuture, true);
                    AssignOrder(module, actor, 2, false);
                    break;
                case SID.ShacklesOfCompanionship4:
                    ModifyDebuff(module, actor, ref _debuffsBlueFuture, true);
                    AssignOrder(module, actor, 3, false);
                    break;
                case SID.ShacklesOfLoneliness0:
                    ModifyDebuff(module, actor, ref _debuffsRedFuture, true);
                    break;
                case SID.ShacklesOfLoneliness1:
                    ModifyDebuff(module, actor, ref _debuffsRedFuture, true);
                    AssignOrder(module, actor, 0, true);
                    break;
                case SID.ShacklesOfLoneliness2:
                    ModifyDebuff(module, actor, ref _debuffsRedFuture, true);
                    AssignOrder(module, actor, 1, true);
                    break;
                case SID.ShacklesOfLoneliness3:
                    ModifyDebuff(module, actor, ref _debuffsRedFuture, true);
                    AssignOrder(module, actor, 2, true);
                    break;
                case SID.ShacklesOfLoneliness4:
                    ModifyDebuff(module, actor, ref _debuffsRedFuture, true);
                    AssignOrder(module, actor, 3, true);
                    break;
                case SID.InescapableCompanionship:
                    ModifyDebuff(module, actor, ref _debuffsBlueImminent, true);
                    break;
                case SID.InescapableLoneliness:
                    ModifyDebuff(module, actor, ref _debuffsRedImminent, true);
                    break;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, int index)
        {
            switch ((SID)actor.Statuses[index].ID)
            {
                case SID.ShacklesOfCompanionship0:
                case SID.ShacklesOfCompanionship1:
                case SID.ShacklesOfCompanionship2:
                case SID.ShacklesOfCompanionship3:
                case SID.ShacklesOfCompanionship4:
                    ModifyDebuff(module, actor, ref _debuffsBlueFuture, false);
                    break;
                case SID.ShacklesOfLoneliness0:
                case SID.ShacklesOfLoneliness1:
                case SID.ShacklesOfLoneliness2:
                case SID.ShacklesOfLoneliness3:
                case SID.ShacklesOfLoneliness4:
                    ModifyDebuff(module, actor, ref _debuffsRedFuture, false);
                    break;
                case SID.InescapableCompanionship:
                    ModifyDebuff(module, actor, ref _debuffsBlueImminent, false);
                    ++NumExpiredDebuffs;
                    break;
                case SID.InescapableLoneliness:
                    ModifyDebuff(module, actor, ref _debuffsRedImminent, false);
                    ++NumExpiredDebuffs;
                    break;
            }
        }

        private void ModifyDebuff(BossModule module, Actor actor, ref byte vector, bool active)
        {
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                BitVector.ModifyVector8Bit(ref vector, slot, active);
        }

        private void AssignOrder(BossModule module, Actor actor, int order, bool far)
        {
            var way1 = module.WorldState.Waymarks[(Waymark)((int)Waymark.A + order)];
            var way2 = module.WorldState.Waymarks[(Waymark)((int)Waymark.N1 + order)];
            if (way1 == null || way2 == null)
                return;

            var d1 = (way1.Value - module.Arena.WorldCenter).LengthSquared();
            var d2 = (way2.Value - module.Arena.WorldCenter).LengthSquared();
            bool use1 = far ? d1 > d2 : d1 < d2;
            int slot = module.Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                _preferredPositions[slot] = use1 ? way1.Value : way2.Value;
        }
    }
}
