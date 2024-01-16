using BossMod.Components;
using System.Linq;
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
            var boss = module.Enemies(OID.Boss).FirstOrDefault();
            if (boss != null)
            {
                if (LeviathanZ == default)
                    LeviathanZ = module.Enemies(OID.Boss).First().Position.Z;
                if (boss.Position.Z != LeviathanZ && boss.Position.Z != 0)
                {
                    LeviathanZ = boss.Position.Z;
                    Distance = 25;
                    Direction = boss.Position.Z <= 0 ? 180.Degrees() : 0.Degrees();
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID is AID.BodySlamNorth or AID.BodySlamSouth)
                Distance = 0;
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints) { }
    }

    class BodySlamAOE : GenericAOEs
    {
        private bool active;
        private Angle Direction;
        private float LeviathanZ;
        private static readonly AOEShapeRect rect = new(30, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (active)
                yield return new(rect, module.PrimaryActor.Position, Direction, new());
        }

        public override void Update(BossModule module)
        {
            base.Update(module);
            var boss = module.Enemies(OID.Boss).FirstOrDefault();
            if (boss != null)
            {
                if (LeviathanZ == default)
                    LeviathanZ = module.Enemies(OID.Boss).First().Position.Z;
                if (boss.Position.Z != LeviathanZ && boss.Position.Z != 0)
                {
                    LeviathanZ = boss.Position.Z;
                    if ((boss.Position.Z + boss.Position.X) <= -1f && (boss.Position.Z + boss.Position.X) >= -2f) // Leviathan head slams SW
                    {
                        Direction = boss.Position.Z <= 0 ? 0.Degrees() : 90.Degrees();
                        active = true;
                    }
                    if ((boss.Position.Z + boss.Position.X) <= 28f && (boss.Position.Z + boss.Position.X) >= 27f) // Leviathan head slams SE
                    {
                        Direction = boss.Position.Z <= 0 ? 0.Degrees() : 270.Degrees();
                        active = true;
                    }
                    if ((boss.Position.Z + boss.Position.X) <= -27f && (boss.Position.Z + boss.Position.X) >= -28f) // Leviathan head slams NW
                    {
                        Direction = boss.Position.Z <= 0 ? 90.Degrees() : 0.Degrees();
                        active = true;
                    }
                    if ((boss.Position.Z + boss.Position.X) <= 2f && (boss.Position.Z + boss.Position.X) >= 1f) // Leviathan head slams NE
                    {
                        Direction = boss.Position.Z <= 0 ? 270.Degrees() : 0.Degrees();
                        active = true;
                    }
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.BodySlamRectAOE)
                active = false;
        }
    }
}
