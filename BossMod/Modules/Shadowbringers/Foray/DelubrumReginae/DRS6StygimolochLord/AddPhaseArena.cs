using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6StygimolochLord
{
    class AddPhaseArena : BossComponent
    {
        private float _innerRingRadius = 14.5f;
        private float _outerRingRadius = 27.5f;
        private float _ringHalfWidth = 2.5f;
        private float _alcoveDepth = 1;
        private float _alcoveWidth = 2;

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.Zone(module.Bounds.ClipAndTriangulate(InDanger(module)), ArenaColor.AOE);
            arena.Zone(module.Bounds.ClipAndTriangulate(MidDanger(module)), ArenaColor.AOE);
            arena.Zone(module.Bounds.ClipAndTriangulate(OutDanger(module)), ArenaColor.AOE);
        }

        private IEnumerable<WPos> RingBorder(BossModule module, Angle centerOffset, float ringRadius, bool innerBorder)
        {
            float offsetMultiplier = innerBorder ? -1 : 1;
            Angle halfWidth = (_alcoveWidth / ringRadius).Radians();
            for (int i = 0; i < 8; ++i)
            {
                var centerAlcove = centerOffset + i * 45.Degrees();
                foreach (var p in CurveApprox.CircleArc(module.Bounds.Center, ringRadius + offsetMultiplier * (_ringHalfWidth + _alcoveDepth), centerAlcove - halfWidth, centerAlcove + halfWidth, module.Bounds.MaxApproxError))
                    yield return p;
                foreach (var p in CurveApprox.CircleArc(module.Bounds.Center, ringRadius + offsetMultiplier * _ringHalfWidth, centerAlcove + halfWidth, centerAlcove + 45.Degrees() - halfWidth, module.Bounds.MaxApproxError))
                    yield return p;
            }
        }

        private IEnumerable<WPos> RepeatFirst(IEnumerable<WPos> pts)
        {
            WPos? first = null;
            foreach (var p in pts)
            {
                first ??= p;
                yield return p;
            }
            if (first != null)
                yield return first.Value;
        }

        private IEnumerable<WPos> InDanger(BossModule module) => RingBorder(module, 22.5f.Degrees(), _innerRingRadius, true);

        private IEnumerable<WPos> MidDanger(BossModule module)
        {
            foreach (var p in RepeatFirst(RingBorder(module, 0.Degrees(), _outerRingRadius, true)))
                yield return p;
            foreach (var p in RepeatFirst(RingBorder(module, 22.5f.Degrees(), _innerRingRadius, false)))
                yield return p;
        }

        private IEnumerable<WPos> OutDanger(BossModule module)
        {
            foreach (var p in RepeatFirst(CurveApprox.Circle(module.Bounds.Center, module.Bounds.HalfSize, module.Bounds.MaxApproxError)))
                yield return p;
            foreach (var p in RepeatFirst(RingBorder(module, 0.Degrees(), _outerRingRadius, false)))
                yield return p;
        }
    }
}
