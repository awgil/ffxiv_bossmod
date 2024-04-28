namespace BossMod.RealmReborn.Extreme.Ex2Garuda;

// common AI for all phases
class Ex2GarudaAI(BossModule module) : BossComponent(module)
{
    private readonly AerialBlast? _aerialBlast = module.FindComponent<AerialBlast>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.StayAtLongRange = true;
            switch ((OID)e.Actor.OID)
            {
                case OID.Boss:
                    e.Priority = 1;
                    e.AttackStrength = 0.2f;
                    if (_aerialBlast?.NumCasts > 0)
                    {
                        e.DesiredRotation = 135.Degrees();
                        e.DesiredPosition = Module.Center + 18 * e.DesiredRotation.ToDirection();
                    }
                    else
                    {
                        e.DesiredRotation = 180.Degrees();
                        e.DesiredPosition = Module.Center + 8 * e.DesiredRotation.ToDirection();
                    }
                    break;
                case OID.Chirada:
                    e.Priority = 2;
                    e.AttackStrength = 0.15f;
                    break;
                case OID.Suparna:
                    e.Priority = assignment != PartyRolesConfig.Assignment.MT ? 3 : 0;
                    e.AttackStrength = 0.15f;
                    e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.OT;
                    e.DesiredRotation = (_aerialBlast?.NumCasts > 0 ? -45 : 0).Degrees();
                    e.DesiredPosition = Module.Center + 18 * e.DesiredRotation.ToDirection();
                    break;
                case OID.RazorPlume:
                    e.Priority = assignment != PartyRolesConfig.Assignment.MT ? 4 : 0;
                    e.AttackStrength = 0;
                    e.ShouldBeTanked = false;
                    break;
                case OID.SatinPlume:
                    e.Priority = assignment != PartyRolesConfig.Assignment.MT ? 5 : 0;
                    e.AttackStrength = 0;
                    e.ShouldBeTanked = false;
                    break;
                case OID.SpinyPlume:
                    e.Priority = Module.PrimaryActor.IsTargetable ? AIHints.Enemy.PriorityForbidAI : 6;
                    e.AttackStrength = 0;
                    e.ShouldBeTanked = false;
                    if (actor.Role == Role.Tank && e.Actor.TargetID != actor.InstanceID && (WorldState.Actors.Find(e.Actor.TargetID)?.FindStatus(SID.ThermalLow)?.Extra ?? 0) >= 2)
                    {
                        e.Priority = 6;
                        e.ShouldBeTanked = e.PreferProvoking = true;
                    }
                    break;
            }
        }

        // don't stand near monoliths to avoid clipping them with friction
        bool haveMonoliths = false;
        foreach (var monolith in Module.Enemies(OID.Monolith).Where(a => !a.IsDead))
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(monolith.Position, 5));
            haveMonoliths = true;
        }

        if (haveMonoliths && actor.Role is Role.Healer or Role.Ranged)
        {
            // have ranged stay in center to avoid los issues
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center, 7), DateTime.MaxValue);
        }
    }
}
