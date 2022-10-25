using System;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex1Ultima
{
    class Ex1UltimaAI : BossComponent
    {
        private ViscousAetheroplasm? _viscousAetheroplasm;

        private static float _meleeRange = 7;
        private static float _rangedRange = 15; // outside ceruleum vent range, which is 14

        public override void Init(BossModule module)
        {
            _viscousAetheroplasm = module.FindComponent<ViscousAetheroplasm>();
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (module.PrimaryActor.TargetID != actor.InstanceID && hints.ForbiddenZones.Count == 1 && !module.Enemies(OID.MagitekBit).Any(a => !a.IsDead)) // for non-mt, there is always a cleave
            {
                // default positions: tank boss at the edge facing N, OT south of boss, M1/M2 to the left/right (so that they can slightly adjust for positionals), H1/H2/R1/R2 to S outside ceruleum vent range, all spread somewhat to avoid homing lasers
                // when tanks need to swap, OT moves between boss and MT and taunts; OT needs to ignore diffractive lasers at this point
                WDir hintOffset = assignment switch
                {
                    PartyRolesConfig.Assignment.M1 => _meleeRange * 45.Degrees().ToDirection(),
                    PartyRolesConfig.Assignment.M2 => _meleeRange * (-45).Degrees().ToDirection(),
                    PartyRolesConfig.Assignment.R1 => _rangedRange * 30.Degrees().ToDirection(),
                    PartyRolesConfig.Assignment.R2 => _rangedRange * (-30).Degrees().ToDirection(),
                    PartyRolesConfig.Assignment.H1 => _rangedRange * 10.Degrees().ToDirection(),
                    PartyRolesConfig.Assignment.H2 => _rangedRange * (-10).Degrees().ToDirection(),
                    _ => new(0, _viscousAetheroplasm!.NeedTankSwap ? -2 : _meleeRange)
                };
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(module.PrimaryActor.Position + hintOffset, 1.5f), DateTime.MaxValue);
            }

            foreach (var e in hints.PotentialTargets)
            {
                e.StayAtLongRange = true;
                switch ((OID)e.Actor.OID)
                {
                    case OID.Boss:
                        e.Priority = 1;
                        e.AttackStrength = 0.25f;
                        e.DesiredPosition = new(0, -10);
                        e.DesiredRotation = 180.Degrees();
                        e.PreferProvoking = e.ShouldBeTanked = module.PrimaryActor.TargetID == actor.InstanceID ? !_viscousAetheroplasm!.NeedTankSwap : _viscousAetheroplasm!.NeedTankSwap && actor.Role == Role.Tank && actor.PosRot.Z < module.PrimaryActor.PosRot.Z;
                        break;
                    case OID.MagitekBit:
                        e.Priority = 2;
                        e.AttackStrength = 0;
                        e.ShouldBeTanked = false;
                        break;
                }
            }
        }
    }
}
