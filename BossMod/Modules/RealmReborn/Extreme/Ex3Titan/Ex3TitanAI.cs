using System;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex3Titan
{
    class Ex3TitanAI : BossComponent
    {
        public bool KillNextBomb;
        private GraniteGaol? _rockThrow;

        public override void Init(BossModule module)
        {
            _rockThrow = module.FindComponent<GraniteGaol>();
        }

        public override void Update(BossModule module)
        {
            if (KillNextBomb && !module.Enemies(OID.BombBoulder).Any())
                KillNextBomb = false;
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            bool haveGaolers = module.Enemies(OID.GraniteGaoler).Any(a => a.IsTargetable && !a.IsDead);
            foreach (var e in hints.PotentialTargets)
            {
                e.StayAtLongRange = true;
                switch ((OID)e.Actor.OID)
                {
                    case OID.Boss:
                    case OID.TitansHeart:
                        e.Priority = 1;
                        e.AttackStrength = 0.25f;
                        e.DesiredPosition = module.Bounds.Center - new WDir(0, module.Arena.Bounds.HalfSize - 6);
                        e.DesiredRotation = 180.Degrees();
                        e.TankDistance = 0;
                        if (actor.Role == Role.Tank)
                        {
                            // note on tank swaps
                            // during phase 1, each 'repeat' lasts for 52 seconds and has 3 busters
                            // theoretically we can swap to OT right after 1st buster, then MT's vuln will expire right after 3rd buster and he can taunt back
                            // OT's vuln will expire right before 5th buster, so MT will eat 1/4/7/... and OT will eat 2+3/5+6/...
                            // however, in reality phase is going to be extremely short - 1 or 2 tb's?..
                            bool isCurrentTank = actor.InstanceID == module.PrimaryActor.TargetID;
                            bool needTankSwap = !haveGaolers && module.FindComponent<MountainBuster>() == null && TankVulnStacks(module) >= 2;
                            e.PreferProvoking = e.ShouldBeTanked = isCurrentTank != needTankSwap;
                        }
                        break;
                    case OID.GraniteGaoler:
                        e.Priority = 2;
                        e.DesiredPosition = module.Bounds.Center + (module.Arena.Bounds.HalfSize - 4) * 30.Degrees().ToDirection(); // move them away from boss, healer gaol spots and upheaval knockback spots
                        e.ShouldBeTanked = module.PrimaryActor.TargetID != actor.InstanceID && actor.Role == Role.Tank;
                        break;
                    case OID.BombBoulder:
                        e.Priority = KillNextBomb && e.Actor.Position.AlmostEqual(module.Bounds.Center, 1) ? 3 : 0; // kill center bomb when needed
                        e.ShouldBeTanked = false;
                        break;
                    case OID.GraniteGaol:
                        e.Priority = e.Actor.Position.InCircle(module.PrimaryActor.Position, 5) ? 5 : 4; // prefer killing gaol under boss first
                        e.AttackStrength = 0;
                        e.ShouldBeTanked = false;
                        break;
                }
            }

            // if there are no active mechanics, all except current tank prefer stacking on max melee behind boss, at an angle that allows all positionals
            if (!haveGaolers && !KillNextBomb && actor.InstanceID != module.PrimaryActor.TargetID && hints.ForbiddenZones.Count == 0)
            {
                if (_rockThrow != null && _rockThrow.PendingFetters[slot])
                {
                    var pos = actor.Role == Role.Healer
                        ? module.Bounds.Center + module.Arena.Bounds.HalfSize * (-30).Degrees().ToDirection() // healers should go to the back; 30 degrees will be safe if landslide is baited straight to south (which it should, since it will follow upheaval)
                        : module.PrimaryActor.Position + new WDir(0, 1);
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos, 2), _rockThrow.ResolveAt);
                }
                else
                {
                    var pos = StackPosition(module);
                    if (pos != null)
                        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos.Value, 2), /*module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.BossCastStart)*/DateTime.MaxValue);
                }
            }
        }

        private WPos? StackPosition(BossModule module)
        {
            var boss = module.PrimaryActor;
            var res = boss.Position + 3 * (boss.Rotation + 135.Degrees()).ToDirection();
            if (module.Arena.Bounds.Contains(res))
                return res;
            res = boss.Position + 3 * (boss.Rotation - 135.Degrees()).ToDirection();
            if (module.Arena.Bounds.Contains(res))
                return res;
            return null;
        }

        private int TankVulnStacks(BossModule module) => module.WorldState.Actors.Find(module.PrimaryActor.TargetID)?.FindStatus(SID.PhysicalVulnerabilityUp)?.Extra ?? 0;
    }
}
