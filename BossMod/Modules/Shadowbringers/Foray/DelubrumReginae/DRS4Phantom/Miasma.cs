using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4Phantom
{
    // TODO: improve hints, currently they are not good... we probably don't need a fully generic implementation, since there are few possible patterns
    class Miasma : Components.GenericAOEs
    {
        public enum Order { Unknown, LowHigh, HighLow }

        public struct LaneState
        {
            public AOEShape? Shape;
            public int NumCasts;
            public DateTime Activation;
            public WPos NextOrigin;
        }

        public int NumLanesFinished { get; private set; }
        private LaneState[,] _laneStates = new LaneState[4, 2];
        private Order _order;

        private static AOEShapeRect _shapeRect = new(50, 6);
        private static AOEShapeCircle _shapeCircle = new(8);
        private static AOEShapeDonut _shapeDonut = new(5, 19);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_order == Order.Unknown)
                yield break;

            var (order1, order2) = _order == Order.HighLow ? (1, 0) : (0, 1);
            for (int i = 0; i < 4; ++i)
            {
                var l1 = _laneStates[i, order1];
                var l2 = _laneStates[i, order2];
                if (l1.Shape != null)
                    yield return new(l1.Shape, l1.NextOrigin, new(), l1.Activation);
                if (l2.Shape != null && (l1.Shape == null || l1.Shape == _shapeDonut && l2.Shape == _shapeRect))
                    yield return new(l2.Shape, l2.NextOrigin, new(), l2.Activation);
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_order != Order.Unknown)
                hints.Add($"Order: {(_order == Order.HighLow ? "high > low" : "low > high")}");
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.ManipulateMiasma => Order.LowHigh,
                AID.InvertMiasma => Order.HighLow,
                _ => Order.Unknown
            };
            if (order != Order.Unknown)
                _order = order;
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.CreepingMiasmaFirst or AID.CreepingMiasmaRest => _shapeRect,
                AID.LingeringMiasmaFirst or AID.LingeringMiasmaRest => _shapeCircle,
                AID.SwirlingMiasmaFirst or AID.SwirlingMiasmaRest => _shapeDonut,
                _ => null
            };
            if (shape == null)
                return;

            int laneIndex = LaneIndex(module, shape == _shapeRect ? caster.Position : spell.TargetXZ);
            if ((AID)spell.Action.ID is AID.CreepingMiasmaFirst or AID.LingeringMiasmaFirst or AID.SwirlingMiasmaFirst)
            {
                int heightIndex = (_laneStates[laneIndex, 0].NumCasts, _laneStates[laneIndex, 1].NumCasts) switch
                {
                    (_, > 0) => 0,
                    ( > 0, _) => 1,
                    _ => _order == Order.HighLow ? 1 : 0,
                };

                ref var l = ref _laneStates[laneIndex, heightIndex];
                if (l.Shape != shape || l.NumCasts != 0)
                {
                    module.ReportError(this, $"Unexpected state at first-cast end");
                }
                else
                {
                    AdvanceLane(module, ref l);
                }
            }
            else
            {
                // note: for non-rects, we get single 'rest' cast belonging to first set right after 'first' cast of second set
                int heightIndex =
                    _laneStates[laneIndex, 0].Shape != shape ? 1 :
                    _laneStates[laneIndex, 1].Shape != shape ? 0 :
                    _laneStates[laneIndex, 0].NumCasts > _laneStates[laneIndex, 1].NumCasts ? 0 : 1;

                ref var l = ref _laneStates[laneIndex, heightIndex];
                if (l.Shape != shape)
                {
                    module.ReportError(this, $"Unexpected state at rest-cast end");
                }
                else
                {
                    AdvanceLane(module, ref l);
                }
            }
        }

        public override void OnActorEAnim(BossModule module, Actor actor, uint state)
        {
            if (state != 0x00010002)
                return; // other states: 00080010 - start glowing, 00040020 - disappear
            AOEShape? shape = (OID)actor.OID switch
            {
                OID.MiasmaLowRect or OID.MiasmaHighRect => _shapeRect,
                OID.MiasmaLowCircle or OID.MiasmaHighCircle => _shapeCircle,
                OID.MiasmaLowDonut or OID.MiasmaHighDonut => _shapeDonut,
                _ => null
            };
            if (shape == null)
                return;
            int heightIndex = (OID)actor.OID is OID.MiasmaLowRect or OID.MiasmaLowCircle or OID.MiasmaLowDonut ? 0 : 1;
            int laneIndex = LaneIndex(module, actor.Position);
            _laneStates[laneIndex, heightIndex] = new() { Shape = shape, Activation = module.WorldState.CurrentTime.AddSeconds(16.1f), NextOrigin = new(actor.Position.X, module.Bounds.Center.Z - module.Bounds.HalfSize + (shape == _shapeRect ? 0 : 5)) };
        }

        private int LaneIndex(BossModule module, WPos pos) => (pos.X - module.Bounds.Center.X) switch
        {
            < -10 => 0,
            < 0 => 1,
            < 10 => 2,
            _ => 3,
        };

        private void AdvanceLane(BossModule module, ref LaneState lane)
        {
            lane.Activation = module.WorldState.CurrentTime.AddSeconds(1.6f);
            ++lane.NumCasts;
            if (lane.Shape == _shapeRect)
            {
                if (lane.NumCasts >= 3)
                {
                    lane.Shape = null;
                    ++NumLanesFinished;
                }
            }
            else
            {
                lane.NextOrigin.Z += 6;
                if (lane.NumCasts >= 8)
                {
                    lane.Shape = null;
                    ++NumLanesFinished;
                }
            }
        }
    }
}
