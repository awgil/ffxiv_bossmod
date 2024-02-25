using BossMod.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    class BodySlamKB : Knockback
    {
        private DateTime _activation;
        private float Distance;
        private Angle Direction;

        private float LeviathanZ;

        public BodySlamKB()
        {
            StopAtWall = true;
        }

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (Distance > 0)
                yield return new(module.Bounds.Center, Distance, _activation, null, Direction, Kind.DirForward);
        }

        public override void Update(BossModule module)
        {
            base.Update(module);
            {
                if (LeviathanZ == default)
                    LeviathanZ = module.PrimaryActor.Position.Z;
                if (module.PrimaryActor.Position.Z != LeviathanZ && module.PrimaryActor.Position.Z != 0)
                {
                    LeviathanZ = module.PrimaryActor.Position.Z;
                    Distance = 25;
                    Direction = module.PrimaryActor.Position.Z <= 0 ? 180.Degrees() : 0.Degrees();
                    _activation = module.WorldState.CurrentTime.AddSeconds(4.8f);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.BodySlamNorth or AID.BodySlamSouth)
                Distance = 0;
        }
        public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos) => (module.FindComponent<Hydroshot>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || (module.FindComponent<Dreadstorm>()?.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false);

    }

    class BodySlamAOE : GenericAOEs
    {
        private bool active;
        private float LeviathanZ;
        private DateTime _activation;
        private static readonly AOEShapeRect rect = new(30, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (active)
                yield return new(rect, module.PrimaryActor.Position, module.PrimaryActor.Rotation, _activation);
        }

        public override void Update(BossModule module)
        {
            base.Update(module);
            {
                if (LeviathanZ == default)
                    LeviathanZ = module.PrimaryActor.Position.Z;
                if (module.PrimaryActor.Position.Z != LeviathanZ && module.PrimaryActor.Position.Z != 0)
                {
                    LeviathanZ = module.PrimaryActor.Position.Z;
                    active = true;
                    _activation = module.WorldState.CurrentTime.AddSeconds(2.6f);
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.BodySlamRectAOE)
                active = false;
        }
    }
}
