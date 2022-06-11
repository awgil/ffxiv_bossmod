using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    // common base for lightwaves components
    class LightwaveCommon : CommonComponents.CastCounter
    {
        protected List<Actor> Waves = new();
        protected static AOEShapeRect WaveAOE = new(50, 8); // note that actual length is 15, but we want to show aoe for full path

        private static float _losRadius = 1;

        public LightwaveCommon() : base(ActionID.MakeSpell(AID.LightOfTheCrystal)) { }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            base.OnEventCast(module, info);
            if (info.IsSpell(AID.RayOfLight) && !Waves.Any(w => w.InstanceID == info.CasterID))
            {
                var wave = module.WorldState.Actors.Find(info.CasterID);
                if (wave != null)
                    Waves.Add(wave);
            }
        }

        protected bool InSafeCone(Vector3 origin, Vector3 blocking, Vector3 position)
        {
            var toBlock = blocking - origin;
            var toCheck = position - origin;
            var dist = toBlock.Length();
            if (dist > toCheck.Length())
                return false;

            var center = GeometryUtils.DirectionFromVec3(toBlock);
            var halfAngle = MathF.Asin(_losRadius / dist);
            return GeometryUtils.PointInCone(toCheck, center, halfAngle);
        }

        protected void DrawSafeCone(MiniArena arena, Vector3 origin, Vector3 blocking)
        {
            var toBlock = blocking - origin;
            var dist = toBlock.Length();
            var center = GeometryUtils.DirectionFromVec3(toBlock);
            var halfAngle = MathF.Asin(_losRadius / dist);
            arena.ZoneCone(origin, dist, 40, center, halfAngle, arena.ColorSafeFromAOE);
        }
    }
}
