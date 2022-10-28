using System;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    class Ex4IfritAI : BossComponent
    {
        private SearingWind? _searingWind;
        private Eruption? _eruption;
        private Hellfire? _hellfire;
        private RadiantPlume? _radiantPlume;
        private CrimsonCyclone? _crimsonCyclone;
        private InfernalFetters? _infernalFetters;

        private Actor? _nextNailToKill;

        public override void Init(BossModule module)
        {
            _searingWind = module.FindComponent<SearingWind>();
            _eruption = module.FindComponent<Eruption>();
            _hellfire = module.FindComponent<Hellfire>();
            _radiantPlume = module.FindComponent<RadiantPlume>();
            _crimsonCyclone = module.FindComponent<CrimsonCyclone>();
            _infernalFetters = module.FindComponent<InfernalFetters>();
        }

        public override void Update(BossModule module)
        {
            if (_nextNailToKill == null || _nextNailToKill.IsDestroyed || _nextNailToKill.IsDead || !_nextNailToKill.IsTargetable)
            {
                // find next nail to destroy - closest to boss or to previous nail
                var castModule = (Ex4Ifrit)module;
                _nextNailToKill = castModule.SmallNails.Where(a => !a.IsDead && a.IsTargetable).Closest((_nextNailToKill ?? module.PrimaryActor).Position) ?? castModule.LargeNails.FirstOrDefault(a => !a.IsDead && a.IsTargetable);
            }
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var vulnStacks = TankVulnStacks(actor);
            var bossAngle = Angle.FromDirection(module.PrimaryActor.Position - module.Bounds.Center);
            var toBoss = bossAngle.ToDirection();
            foreach (var e in hints.PotentialTargets)
            {
                e.StayAtLongRange = true;
                switch ((OID)e.Actor.OID)
                {
                    case OID.Boss:
                        e.Priority = _nextNailToKill == null ? 1 : -1; // reconsider...
                        e.AttackStrength = 0.25f;
                        if (_hellfire?.PlumesImminent ?? false)
                        {
                            e.DesiredRotation = _hellfire.NextSafeSpot;
                            e.DesiredPosition = module.Bounds.Center + 13 * e.DesiredRotation.ToDirection();
                        }
                        else
                        {
                            e.DesiredRotation = bossAngle; // try really hard to face boss along radius
                            e.DesiredPosition = module.PrimaryActor.Position.InCircle(module.Bounds.Center, 12) ? module.Bounds.Center + 13 * toBoss : module.PrimaryActor.Position; // 13 == radius (20) - tank distance (2) - hitbox (5)
                        }
                        if (actor.Role == Role.Tank)
                        {
                            if (actor.InstanceID == module.PrimaryActor.TargetID)
                            {
                                // continue tanking until OT taunts
                                e.ShouldBeTanked = true;
                            }
                            else if (vulnStacks == 0 && TankVulnStacks(module.WorldState.Actors.Find(module.PrimaryActor.TargetID)) >= 2)
                            {
                                // taunt if safe
                                var dirIfTaunted = Angle.FromDirection(actor.Position - module.PrimaryActor.Position);
                                e.PreferProvoking = e.ShouldBeTanked = !module.Raid.WithoutSlot().Any(a => a.Role != Role.Tank && a.Position.InCircleCone(module.PrimaryActor.Position, 21, dirIfTaunted, 60.Degrees()));
                            }
                        }
                        break;
                    case OID.InfernalNailSmall:
                        e.Priority = (e.Actor == _nextNailToKill /*|| HPLargerThanThreshold(module, e.Actor, 0.5f)*/) && NailReachable(module, e.Actor, actor) ? 2 : -1;
                        e.AttackStrength = 0;
                        e.ShouldBeTanked = false;
                        break;
                    case OID.InfernalNailLarge:
                        e.Priority = e.Actor == _nextNailToKill && NailReachable(module, e.Actor, actor) ? 2 : -1;
                        e.AttackStrength = 0;
                        e.ShouldBeTanked = false;
                        e.ForbidDOTs = true;
                        break;
                }
            }

            // position hints
            bool isFettered = _infernalFetters != null && _infernalFetters.Fetters[slot];
            if (_radiantPlume?.Casters.Count > 0 || _crimsonCyclone?.Casters.Count > 0 || _eruption?.Casters.Count > 0)
            {
                // during plumes/cyclone/nails, just make sure searing winds doesn't intersect with others
                if (_searingWind != null && _searingWind.Active)
                {
                    var toAvoid = _searingWind.SpreadMask[slot]
                        ? module.Raid.WithSlot().ExcludedFromMask(_searingWind.SpreadMask)
                        : module.Raid.WithSlot().IncludedInMask(_searingWind.SpreadMask);
                    foreach (var (i, p) in toAvoid)
                        hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, _searingWind.SpreadRadius), _searingWind.ActivateAt);
                }

                // and also stay near fetter partner
                var fetterPartner = isFettered ? module.Raid.WithSlot().Exclude(actor).IncludedInMask(_infernalFetters!.Fetters).FirstOrDefault().Item2 : null;
                if (fetterPartner != null)
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(fetterPartner.Position, 5));
                }

                // eruption bait hints
                if (_eruption != null && _eruption.Baiters[slot])
                {
                    if (actor.Role is Role.Melee or Role.Ranged && module.PrimaryActor.CastInfo != null)
                    {
                        // specific spot for first baits
                        var baitSpot = module.PrimaryActor.Position - 11.5f * toBoss + 11 * toBoss.OrthoR();
                        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(baitSpot, 2));
                    }
                    else
                    {
                        // avoid non-baiters
                        foreach (var (i, p) in module.Raid.WithSlot().ExcludedFromMask(_eruption.Baiters))
                            hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, _eruption.Shape.Radius));
                    }
                }

                if (actor.Role == Role.Melee && _nextNailToKill != null && NailCanBeAttackedByMelee(module, _nextNailToKill, actor))
                {
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_nextNailToKill.Position, MaxRange(_nextNailToKill, actor)));
                }
            }
            else if (module.PrimaryActor.TargetID != actor.InstanceID)
            {
                // default positions:
                // - MT assumed to point boss along radius (both to avoid own knockbacks and to simplify positioning); others position relative to direction to boss (this will fail if MT positions boss incorrectly, but oh well)
                // - OT + fetters stay right out of cleave - this ensures that incinerate right after taunt still won't hit anyone
                // - melee + phys ranged stay on the other side at 45 degrees, to allow positionals
                // - healer stays opposite MT far enough away to not be affected by eruptions
                // - caster stays behind dd camp, so that eruptions at melee won't force him to move and out of range of knockbacks
                // - healer with searing winds moves opposite at 45 degrees, so that other healer won't be knocked into searing winds
                WPos pos;
                float radius = 2;
                if (_searingWind != null && _searingWind.SpreadMask[slot])
                {
                    pos = module.Bounds.Center + 18 * (bossAngle + 135.Degrees()).ToDirection();
                }
                else if (_hellfire?.Invincible ?? false)
                {
                    pos = module.Bounds.Center; // stack in center during hellfire for easier healing
                }
                else if (actor.Role == Role.Tank || isFettered)
                {
                    pos = module.PrimaryActor.Position + 7.5f * (bossAngle + 75.Degrees()).ToDirection();
                }
                else if (actor.Role == Role.Healer)
                {
                    pos = module.PrimaryActor.Position - 11.5f * toBoss;
                }
                else if (actor.Role == Role.Melee && _nextNailToKill != null && NailCanBeAttackedByMelee(module, _nextNailToKill, actor))
                {
                    pos = _nextNailToKill.Position;
                    radius = MaxRange(_nextNailToKill, actor);
                }
                else
                {
                    pos = module.PrimaryActor.Position + 6 * (bossAngle - 135.Degrees()).ToDirection();
                    if (assignment == PartyRolesConfig.Assignment.R2) // assumed caster, TODO
                        pos -= 15 * toBoss;
                }

                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos, radius)/*, DateTime.MaxValue*/ );
            }
            else if (_hellfire?.Invincible ?? false)
            {
                // MT should go to safe spot when boss is casting hellfire...
                var pos = module.Bounds.Center + 13 * _hellfire.NextSafeSpot.ToDirection();
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos, 2));
            }

            // cooldowns
            //if (module.PrimaryActor.TargetID == actor.InstanceID && vulnStacks > 1)
            //{

            //}
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (module.PrimaryActor.TargetID == pc.InstanceID)
            {
                // cone to help mt with proper positioning
                arena.AddCone(module.PrimaryActor.Position, 2, Angle.FromDirection(module.PrimaryActor.Position - module.Bounds.Center), 60.Degrees(), ArenaColor.Safe);
            }

            // debug
            //Action<WPos, float, uint> drawSpot = (p, r, c) =>
            //{
            //    arena.AddCircle(p, r, c, 2);
            //    arena.AddCircle(p, 1, c, 1);
            //};
            //var bossAngle = Angle.FromDirection(module.PrimaryActor.Position - module.Bounds.Center);
            //var ddSpot = module.PrimaryActor.Position + 6 * (bossAngle - 135.Degrees()).ToDirection();
            //var blmSpot = ddSpot - 15 * bossAngle.ToDirection();
            //var healSpot = module.PrimaryActor.Position - 11.5f * bossAngle.ToDirection();
            //drawSpot(module.PrimaryActor.Position + 7.5f * (bossAngle + 75.Degrees()).ToDirection(), 8, 0xffff0000); // offtank/fetter
            //drawSpot(ddSpot, 8, 0xff0000ff); // dd
            //drawSpot(blmSpot, 8, 0xffff00ff); // caster
            //drawSpot(healSpot, 8, 0xff00ff00);
            //drawSpot(healSpot + 9 * (bossAngle - 90.Degrees()).ToDirection(), 8, 0xff00ffff); // dd drop spot 1
            //drawSpot(ddSpot + 10 * (bossAngle - 90.Degrees()).ToDirection(), 8, 0x8000ffff); // dd drop spot 2
            //drawSpot(blmSpot + 10 * (bossAngle - 90.Degrees()).ToDirection(), 8, 0x4000ffff); // dd drop spot 3
            //drawSpot(module.Bounds.Center + 18 * (bossAngle + 135.Degrees()).ToDirection(), 14, 0xff00ff00); // searing winds
        }

        private int TankVulnStacks(Actor? tank) => tank?.FindStatus(SID.Suppuration)?.Extra ?? 0;
        private float MaxRange(Actor nail, Actor player) => (player.Role is Role.Healer or Role.Ranged ? 25 : 3) + nail.HitboxRadius + player.HitboxRadius;

        private bool NailCanBeAttackedByMelee(BossModule module, Actor nail, Actor player)
        {
            if (nail.Position.InCone(module.PrimaryActor.Position, module.PrimaryActor.Rotation, 60.Degrees()))
                return false; // in cleave
            var maxRange = MaxRange(nail, player);
            if (_searingWind != null && _searingWind.SpreadMask.Any() && maxRange < _searingWind.SpreadRadius && module.Raid.WithSlot().IncludedInMask(_searingWind.SpreadMask).InRadius(nail.Position, _searingWind.SpreadRadius - maxRange).Any())
                return false; // in searing wind
            return true;
        }

        private bool NailReachable(BossModule module, Actor nail, Actor player)
        {
            return nail.Position.InCircle(player.Position, MaxRange(nail, player));
        }

        private bool HPLargerThanThreshold(BossModule module, Actor target, float threshold) => target.HP.Cur + module.WorldState.PendingEffects.PendingHPDifference(target.InstanceID) > threshold * target.HP.Max;
    }
}
