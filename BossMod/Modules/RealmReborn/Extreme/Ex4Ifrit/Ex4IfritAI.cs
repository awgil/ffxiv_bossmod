using System;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    class Ex4IfritAI : BossComponent
    {
        private SearingWind? _searingWind;
        private Eruption? _eruption;
        private RadiantPlume? _radiantPlume;
        private CrimsonCyclone? _crimsonCyclone;
        private int _numPlumes;

        public override void Init(BossModule module)
        {
            _searingWind = module.FindComponent<SearingWind>();
            _eruption = module.FindComponent<Eruption>();
            _radiantPlume = module.FindComponent<RadiantPlume>();
            _crimsonCyclone = module.FindComponent<CrimsonCyclone>();
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            Actor? nextNailToKill = null;
            //if (actor.FindStatus(SID.VulnerabilityUp) == null)
            {
                var castModule = (Ex4Ifrit)module;
                nextNailToKill = castModule.SmallNails.Where(a => !a.IsDead && a.IsTargetable).Closest(module.PrimaryActor.Position);
                nextNailToKill ??= castModule.LargeNails.FirstOrDefault(a => !a.IsDead && a.IsTargetable);
            }
            if (nextNailToKill != null &&
                actor.Role is Role.Tank or Role.Melee &&
                _searingWind != null &&
                _searingWind.SpreadMask.Any() &&
                module.Raid.WithSlot().IncludedInMask(_searingWind.SpreadMask).InRadius(nextNailToKill.Position, _searingWind.SpreadRadius - nextNailToKill.HitboxRadius - 3.5f).Any())
            {
                nextNailToKill = null; // next nail is not reachable without entering searing wind
            }

            foreach (var e in hints.PotentialTargets)
            {
                e.StayAtLongRange = true;
                switch ((OID)e.Actor.OID)
                {
                    case OID.Boss:
                        e.Priority = 1;
                        e.AttackStrength = 0.25f;
                        e.DesiredRotation = NextSafeSpot();
                        e.DesiredPosition = module.Bounds.Center + 12 * e.DesiredRotation.ToDirection(); // 12 == radius (20) - tank distance (2) - hitbox (5) - leeway (1)
                        if (actor.Role == Role.Tank)
                        {
                            bool isCurrentTank = actor.InstanceID == module.PrimaryActor.TargetID;
                            bool needTankSwap = TankVulnStacks(module) >= 3;
                            e.PreferProvoking = e.ShouldBeTanked = isCurrentTank != needTankSwap;
                        }
                        break;
                    case OID.InfernalNailSmall:
                    case OID.InfernalNailLarge:
                        e.Priority = e.Actor == nextNailToKill ? 2 : -1;
                        e.AttackStrength = 0;
                        e.ShouldBeTanked = false;
                        e.ForbidDOTs = true;
                        break;
                }
            }

            bool haveActiveEruptions = _eruption?.Casters.Count > 0;
            bool haveActivePlumes = _radiantPlume?.Casters.Count > 0;
            bool haveActiveCyclones = _crimsonCyclone?.Casters.Count > 0;
            if (haveActiveEruptions || haveActivePlumes || haveActiveCyclones /*|| nextNailToKill != null*/)
            {
                // if doing mechanics, avoid searing wind
                if (_searingWind != null && _searingWind.Active)
                {
                    var toAvoid = _searingWind.SpreadMask[slot]
                        ? module.Raid.WithSlot().ExcludedFromMask(_searingWind.SpreadMask)
                        : module.Raid.WithSlot().IncludedInMask(_searingWind.SpreadMask);
                    foreach (var (i, p) in toAvoid)
                        hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, _searingWind.SpreadRadius), _searingWind.ActivateAt);
                }
            }
            else if (module.PrimaryActor.TargetID != actor.InstanceID)
            {
                // default positions:
                // - searing wind target opposite boss
                // - otherwise dd & OT in two 'camps', so that eruption doesn't interfere with them
                // - healers are further back closer to center, far enough away from both camps
                var bossToCenter = (module.Bounds.Center - module.PrimaryActor.Position).Normalized();
                var pos = _searingWind != null && _searingWind.SpreadMask[slot]
                    ? module.Bounds.Center + 18 * bossToCenter
                    : module.PrimaryActor.Position + actor.Role switch
                {
                    Role.Healer => 11 * bossToCenter,
                    Role.Tank => 4 * (bossToCenter + bossToCenter.OrthoR()),
                    _ => 4 * (bossToCenter + bossToCenter.OrthoL()),
                };
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos, 2)/*, DateTime.MaxValue*/ );
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.RadiantPlume)
                ++_numPlumes;
        }

        private Angle NextSafeSpot() => _numPlumes switch
        {
            0 => 150.Degrees(),
            1 => 110.Degrees(),
            _ => 70.Degrees()
        };

        private int TankVulnStacks(BossModule module) => module.WorldState.Actors.Find(module.PrimaryActor.TargetID)?.FindStatus(SID.Suppuration)?.Extra ?? 0;
    }
}
