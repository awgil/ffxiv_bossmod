using BossMod.Components;
using System.Collections.Generic;

namespace BossMod.Modules.RealmReborn.Trial.T09WhorleaterH
{
    class BodySlamKB : Knockback
    {
        private float Distance;
        private Angle Direction;

        private float LeviathanZ;

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (Distance > 0)
                yield return new(module.Bounds.Center, Distance, default, null, Direction, Kind.DirForward);
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
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.BodySlamNorth or AID.BodySlamSouth)
                Distance = 0;
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) { }
    }

    class BodySlamAOE : GenericAOEs
    {
        private bool active;
        private float LeviathanZ;
        private readonly AOEShapeRect rect = new(30, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (active)
                yield return new(rect, module.PrimaryActor.Position, module.PrimaryActor.Rotation);
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
