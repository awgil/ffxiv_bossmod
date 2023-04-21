using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Ultimate.TEA
{
    class P2IntermissionLimitCut : LimitCut
    {
        public P2IntermissionLimitCut() : base(3.2f) { }
    }

    class P2IntermissionHawkBlaster : Components.GenericAOEs
    {
        private Angle _blasterStartingDirection;

        private static float _blasterOffset = 14;
        private static AOEShapeCircle _blasterShape = new(10);

        public P2IntermissionHawkBlaster() : base(ActionID.MakeSpell(AID.HawkBlasterIntermission)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in FutureBlasterCenters(module))
                yield return new(_blasterShape, c, risky: false);
            foreach (var c in ImminentBlasterCenters(module))
                yield return new(_blasterShape, c, color: ArenaColor.Danger);
        }

        // TODO: reconsider
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            base.AddHints(module, slot, actor, hints, movementHints);
            if (movementHints != null && SafeSpotHint(module, slot) is var safespot && safespot != null)
                movementHints.Add(actor.Position, safespot.Value, ArenaColor.Safe);
        }

        // TODO: reconsider
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (SafeSpotHint(module, pcSlot) is var safespot && safespot != null)
                arena.AddCircle(safespot.Value, 1, ArenaColor.Safe);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (spell.Action == WatchedAction)
            {
                if (NumCasts == 0)
                {
                    var offset = spell.TargetXZ - module.Bounds.Center;
                    // a bit of a hack: most strats (lpdu etc) select a half between W and NE inclusive to the 'first' group; ensure 'starting' direction is one of these
                    bool invert = Math.Abs(offset.Z) < 2 ? offset.X > 0 : offset.Z > 0;
                    if (invert)
                        offset = -offset;
                    _blasterStartingDirection = Angle.FromDirection(offset);
                }
                ++NumCasts;
            }
        }

        // 0,1,2,3 - offset aoes, 4 - center aoe
        private int NextBlasterIndex => NumCasts switch
        {
            0 or 1 => 0,
            2 or 3 => 1,
            4 or 5 => 2,
            6 or 7 => 3,
            8 => 4,
            9 or 10 => 5,
            11 or 12 => 6,
            13 or 14 => 7,
            15 or 16 => 8,
            17 => 9,
            _ => 10
        };

        private IEnumerable<WPos> BlasterCenters(BossModule module, int index)
        {
            switch (index)
            {
                case 0: case 1: case 2: case 3:
                    {
                        var dir = (_blasterStartingDirection - index * 45.Degrees()).ToDirection();
                        yield return module.Bounds.Center + _blasterOffset * dir;
                        yield return module.Bounds.Center - _blasterOffset * dir;
                    }
                    break;
                case 5: case 6: case 7: case 8:
                    {
                        var dir = (_blasterStartingDirection - (index - 5) * 45.Degrees()).ToDirection();
                        yield return module.Bounds.Center + _blasterOffset * dir;
                        yield return module.Bounds.Center - _blasterOffset * dir;
                    }
                    break;
                case 4: case 9:
                    yield return module.Bounds.Center;
                    break;
            }
        }

        private IEnumerable<WPos> ImminentBlasterCenters(BossModule module) => NumCasts > 0 ? BlasterCenters(module, NextBlasterIndex) : Enumerable.Empty<WPos>();
        private IEnumerable<WPos> FutureBlasterCenters(BossModule module) => NumCasts > 0 ? BlasterCenters(module, NextBlasterIndex + 1) : Enumerable.Empty<WPos>();

        // TODO: reconsider
        private WPos? SafeSpotHint(BossModule module, int slot)
        {
            //var safespots = NextBlasterIndex switch
            //{
            //    1 or 2 or 3 or 4 => BlasterCenters(module, NextBlasterIndex - 1),
            //    5 => BlasterCenters(module, 3),
            //    6 or 7 or 8 => BlasterCenters(module, NextBlasterIndex - 1),
            //    _ => Enumerable.Empty<WPos>()
            //};
            if (NextBlasterIndex != 1)
                return null;

            var strategy = Service.Config.Get<TEAConfig>().P2IntermissionHints;
            if (strategy == TEAConfig.P2Intermission.None)
                return null;

            bool invert = strategy == TEAConfig.P2Intermission.FirstForOddPairs && (module.FindComponent<LimitCut>()?.PlayerOrder[slot] is 3 or 4 or 7 or 8);
            var offset = _blasterOffset * _blasterStartingDirection.ToDirection();
            return module.Bounds.Center + (invert ? -offset : offset);
        }
    }
}
