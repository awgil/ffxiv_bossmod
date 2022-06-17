using System.Linq;

namespace BossMod.Endwalker.Savage.P1SErichthonios
{
    // state related to normal and fourfold shackles
    class Shackles : BossComponent
    {
        public int NumExpiredDebuffs { get; private set; } = 0;
        private bool _active = false;
        private BitMask _debuffsBlueImminent;
        private BitMask _debuffsBlueFuture;
        private BitMask _debuffsRedImminent;
        private BitMask _debuffsRedFuture;
        private BitMatrix _blueTetherMatrix;
        private BitMatrix _redTetherMatrix; // bit (8*i+j) is set if there is a tether from j to i; bit [i,i] is always set
        private BitMatrix _blueExplosionMatrix;
        private BitMatrix _redExplosionMatrix; // bit (8*i+j) is set if player i is inside explosion of player j; bit [i,i] is never set
        private WPos[] _preferredPositions = new WPos[8];

        private static float _blueExplosionRadius = 4;
        private static float _redExplosionRadius = 8;
        private static uint TetherColor(bool blue, bool red) => blue ? (red ? 0xff00ffff : 0xffff0080) : (red ? 0xff8080ff : 0xff808080);

        public Shackles()
        {
            PartyStatusUpdate(SID.ShacklesOfCompanionship0, (_, slot, _, _, _, _) => _debuffsBlueFuture.Set(slot));
            PartyStatusUpdate(SID.ShacklesOfCompanionship1, (module, slot, _, _, _, _) => { _debuffsBlueFuture.Set(slot); AssignOrder(module, slot, 0, false); });
            PartyStatusUpdate(SID.ShacklesOfCompanionship2, (module, slot, _, _, _, _) => { _debuffsBlueFuture.Set(slot); AssignOrder(module, slot, 1, false); });
            PartyStatusUpdate(SID.ShacklesOfCompanionship3, (module, slot, _, _, _, _) => { _debuffsBlueFuture.Set(slot); AssignOrder(module, slot, 2, false); });
            PartyStatusUpdate(SID.ShacklesOfCompanionship4, (module, slot, _, _, _, _) => { _debuffsBlueFuture.Set(slot); AssignOrder(module, slot, 3, false); });
            PartyStatusUpdate(SID.ShacklesOfLoneliness0, (_, slot, _, _, _, _) => _debuffsRedFuture.Set(slot));
            PartyStatusUpdate(SID.ShacklesOfLoneliness1, (module, slot, _, _, _, _) => { _debuffsRedFuture.Set(slot); AssignOrder(module, slot, 0, true); });
            PartyStatusUpdate(SID.ShacklesOfLoneliness2, (module, slot, _, _, _, _) => { _debuffsRedFuture.Set(slot); AssignOrder(module, slot, 1, true); });
            PartyStatusUpdate(SID.ShacklesOfLoneliness3, (module, slot, _, _, _, _) => { _debuffsRedFuture.Set(slot); AssignOrder(module, slot, 2, true); });
            PartyStatusUpdate(SID.ShacklesOfLoneliness4, (module, slot, _, _, _, _) => { _debuffsRedFuture.Set(slot); AssignOrder(module, slot, 3, true); });
            PartyStatusLose(SID.ShacklesOfCompanionship0, (_, slot, _) => _debuffsBlueFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfCompanionship1, (_, slot, _) => _debuffsBlueFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfCompanionship2, (_, slot, _) => _debuffsBlueFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfCompanionship3, (_, slot, _) => _debuffsBlueFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfCompanionship4, (_, slot, _) => _debuffsBlueFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfLoneliness0, (_, slot, _) => _debuffsRedFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfLoneliness1, (_, slot, _) => _debuffsRedFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfLoneliness2, (_, slot, _) => _debuffsRedFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfLoneliness3, (_, slot, _) => _debuffsRedFuture.Clear(slot));
            PartyStatusLose(SID.ShacklesOfLoneliness4, (_, slot, _) => _debuffsRedFuture.Clear(slot));

            PartyStatusUpdate(SID.InescapableCompanionship, (_, slot, _, _, _, _) => _debuffsBlueImminent.Set(slot));
            PartyStatusUpdate(SID.InescapableLoneliness, (_, slot, _, _, _, _) => _debuffsRedImminent.Set(slot));
            PartyStatusLose(SID.InescapableCompanionship, (_, slot, _) => { _debuffsBlueImminent.Clear(slot); ++NumExpiredDebuffs; });
            PartyStatusLose(SID.InescapableLoneliness, (_, slot, _) => { _debuffsRedImminent.Clear(slot); ++NumExpiredDebuffs; });
        }

        public override void Update(BossModule module)
        {
            _blueTetherMatrix = _redTetherMatrix = _blueExplosionMatrix = _redExplosionMatrix = new();
            var blueDebuffs = _debuffsBlueImminent | _debuffsBlueFuture;
            var redDebuffs = _debuffsRedImminent | _debuffsRedFuture;
            _active = (blueDebuffs | redDebuffs).Any();
            if (!_active)
                return; // nothing to do...

            // update tether matrices
            foreach ((int iSrc, var src) in module.Raid.WithSlot())
            {
                // blue => 3 closest
                if (blueDebuffs[iSrc])
                {
                    _blueTetherMatrix[iSrc, iSrc] = true;
                    foreach ((int iTgt, _) in module.Raid.WithSlot().Exclude(iSrc).SortedByRange(src.Position).Take(3))
                        _blueTetherMatrix[iTgt, iSrc] = true;
                }

                // red => 3 furthest
                if (redDebuffs[iSrc])
                {
                    _redTetherMatrix[iSrc, iSrc] = true;
                    foreach ((int iTgt, _) in module.Raid.WithSlot().Exclude(iSrc).SortedByRange(src.Position).TakeLast(3))
                        _redTetherMatrix[iTgt, iSrc] = true;
                }
            }

            // update explosion matrices and detect problems (has to be done in a separate pass)
            foreach ((int i, var actor) in module.Raid.WithSlot())
            {
                if (_blueTetherMatrix[i].Any())
                    foreach ((int j, _) in module.Raid.WithSlot().InRadiusExcluding(actor, _blueExplosionRadius))
                        _blueExplosionMatrix[j, i] = true;

                if (_redTetherMatrix[i].Any())
                    foreach ((int j, _) in module.Raid.WithSlot().InRadiusExcluding(actor, _redExplosionRadius))
                        _redExplosionMatrix[j, i] = true;
            }
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_blueTetherMatrix[slot].Any() && _redTetherMatrix[slot].Any())
            {
                hints.Add("Target of two tethers!");
            }
            if (_blueExplosionMatrix[slot].Any() || _redExplosionMatrix[slot].Any())
            {
                hints.Add("GTFO from explosion!");
            }

            if (movementHints != null && _preferredPositions[slot] != new WPos())
            {
                movementHints.Add(actor.Position, _preferredPositions[slot], ArenaColor.Safe);
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
                var blueTetheredTo = _blueTetherMatrix[i];
                var redTetheredTo = _redTetherMatrix[i];
                arena.Actor(actor, TetherColor(blueTetheredTo.Any(), redTetheredTo.Any()));

                // draw tethers
                foreach ((int j, var target) in module.Raid.WithSlot(true).Exclude(i).IncludedInMask(blueTetheredTo | redTetheredTo))
                    arena.AddLine(actor.Position, target.Position, TetherColor(blueTetheredTo[j], redTetheredTo[j]));

                // draw explosion circles that hit me
                if (_blueExplosionMatrix[pcSlot, i])
                    arena.AddCircle(actor.Position, _blueExplosionRadius, ArenaColor.Danger);
                if (_redExplosionMatrix[pcSlot, i])
                    arena.AddCircle(actor.Position, _redExplosionRadius, ArenaColor.Danger);

                drawBlueAroundMe |= _blueExplosionMatrix[i, pcSlot];
                drawRedAroundMe |= _redExplosionMatrix[i, pcSlot];
            }

            // draw explosion circles if I hit anyone
            if (drawBlueAroundMe)
                arena.AddCircle(pc.Position, _blueExplosionRadius, ArenaColor.Danger);
            if (drawRedAroundMe)
                arena.AddCircle(pc.Position, _redExplosionRadius, ArenaColor.Danger);

            // draw assigned spot, if any
            if (_preferredPositions[pcSlot] != new WPos())
                arena.AddCircle(_preferredPositions[pcSlot], 2, ArenaColor.Safe);

        }

        private void AssignOrder(BossModule module, int slot, int order, bool far)
        {
            var way1 = module.WorldState.Waymarks[(Waymark)((int)Waymark.A + order)];
            var way2 = module.WorldState.Waymarks[(Waymark)((int)Waymark.N1 + order)];
            if (way1 == null || way2 == null)
                return;

            var w1 = new WPos(way1.Value.XZ());
            var w2 = new WPos(way2.Value.XZ());
            var d1 = (w1 - module.Bounds.Center).LengthSq();
            var d2 = (w2 - module.Bounds.Center).LengthSq();
            bool use1 = far ? d1 > d2 : d1 < d2;
            _preferredPositions[slot] = use1 ? w1 : w2;
        }
    }
}
