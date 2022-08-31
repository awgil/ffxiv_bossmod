using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    class TopazCluster : Components.GenericAOEs
    {
        private BitMask[] _aoeQuadrants = new BitMask[4]; // [i] = danger quardants at explosion #i, bits: 0=NW, 1=NE, 2=SW, 3=SE

        private static float _quarterWidth = 7.5f;
        private static AOEShapeRect _shape = new(_quarterWidth, _quarterWidth, _quarterWidth);

        public TopazCluster() : base(ActionID.MakeSpell(AID.RubyReflection1)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module)
        {
            var aoeQuadrants = NumCasts switch
            {
                < 2 => _aoeQuadrants[0],
                < 4 => _aoeQuadrants[1],
                < 7 => _aoeQuadrants[2],
                < 10 => _aoeQuadrants[3],
                _ => new BitMask()
            };
            foreach (var q in aoeQuadrants.SetBits())
            {
                // TODO: correct explosion time
                yield return (_shape, module.Bounds.Center + QuadrantCenterOffset(q), new(), module.WorldState.CurrentTime);
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (_aoeQuadrants[2].Any())
            {
                var safe = (~_aoeQuadrants[2]).LowestSetBit();
                var safeWaymark = safe < 4 ? WaymarkForQuadrant(module, safe) : Waymark.Count;
                if (safeWaymark != Waymark.Count)
                {
                    hints.Add($"Safespot for third: {safeWaymark}");
                }
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            int quadrant = (AID)spell.Action.ID switch
            {
                AID.TopazClusterHit1 => 0,
                AID.TopazClusterHit2 => 1,
                AID.TopazClusterHit3 => 2,
                AID.TopazClusterHit4 => 3,
                _ => -1
            };

            if (quadrant >= 0)
            {
                var offset = caster.Position - module.Bounds.Center;
                int bit = 0;
                if (offset.X > 0)
                    bit |= 1;
                if (offset.Z > 0)
                    bit |= 2;
                _aoeQuadrants[quadrant].Set(bit);
            }
        }

        private WDir QuadrantCenterOffset(int q) => new((q & 1) != 0 ? +_quarterWidth : -_quarterWidth, (q & 2) != 0 ? +_quarterWidth : -_quarterWidth);

        private Waymark WaymarkForQuadrant(BossModule module, int q)
        {
            var c = module.Bounds.Center + QuadrantCenterOffset(q);
            Waymark w = Waymark.Count;
            float wd = float.MaxValue;
            for (int i = 0; i < (int)Waymark.Count; i++)
            {
                var pos = module.WorldState.Waymarks[(Waymark)i];
                var dist = pos != null ? (new WPos(pos.Value.XZ()) - c).LengthSq() : float.MaxValue;
                if (dist < wd)
                {
                    w = (Waymark)i;
                    wd = dist;
                }
            }
            return w;
        }
    }
}
