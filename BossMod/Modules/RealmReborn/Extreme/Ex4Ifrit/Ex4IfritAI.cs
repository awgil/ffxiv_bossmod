using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    // common ai features for whole fight
    class Ex4IfritAICommon : BossComponent
    {
        private Eruption? _eruption;
        private SearingWind? _searingWind;
        private InfernalFetters? _infernalFetters;

        public override void Init(BossModule module)
        {
            _eruption = module.FindComponent<Eruption>();
            _searingWind = module.FindComponent<SearingWind>();
            _infernalFetters = module.FindComponent<InfernalFetters>();
        }

        protected bool IsInvincible(Actor actor) => actor.FindStatus(SID.Invincibility) != null;
        protected int TankVulnStacks(Actor? tank) => tank?.FindStatus(SID.Suppuration)?.Extra ?? 0;
        protected bool EruptionActive => _eruption?.Casters.Count > 0;
        protected bool IsEruptionBaiter(int slot) => _eruption != null && _eruption.Baiters[slot];
        protected bool IsSearingWindTarget(int slot) => _searingWind != null && _searingWind.SpreadMask[slot];
        protected bool IsFettered(int slot) => _infernalFetters != null && _infernalFetters.Fetters[slot];

        protected void UpdateBossTankingProperties(BossModule module, AIHints.Enemy boss, Actor player)
        {
            boss.AttackStrength = 0.25f;
            if (player.Role == Role.Tank)
            {
                if (player.InstanceID == boss.Actor.TargetID)
                {
                    // continue tanking until OT taunts
                    boss.ShouldBeTanked = true;
                }
                else if (TankVulnStacks(player) == 0 && TankVulnStacks(module.WorldState.Actors.Find(boss.Actor.TargetID)) >= 2)
                {
                    // taunt if safe
                    var dirIfTaunted = Angle.FromDirection(player.Position - module.PrimaryActor.Position);
                    boss.PreferProvoking = boss.ShouldBeTanked = !module.Raid.WithoutSlot().Any(a => a.Role != Role.Tank && Incinerate.CleaveShape.Check(a.Position, module.PrimaryActor.Position, dirIfTaunted));
                }
            }
        }

        protected void AddPositionHint(AIHints hints, WPos target, bool asap = true, float radius = 2)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(target, radius), asap ? new() : DateTime.MaxValue);
        }

        protected void AddDefaultEruptionBaiterHints(BossModule module, AIHints hints)
        {
            // avoid non-baiters (TODO: should this be done by eruption component itself?)
            if (_eruption != null)
                foreach (var (i, p) in module.Raid.WithSlot().ExcludedFromMask(_eruption.Baiters))
                    hints.AddForbiddenZone(ShapeDistance.Circle(p.Position, _eruption.Shape.Radius));
        }
    }

    // ai used during 'normal' phases (no nails, plumes or cyclones)
    // during this phase we use specific positioning to simplify dealing with tank swaps, eruptions, searing winds and fetters
    // - MT always points boss to the wall, to ensure most of the arena is not cleaved
    // - searing wind target (typically a healer) prefers standing near edge to the left of the boss
    //   sometimes we get second searing wind before previous expires (typically on phase changes), we have to account for that - but that shouldn't matter, since first target won't be hit by more winds despite remaining debuff?..
    //   one of the spots is very far, out of range of MT healing, but it gives OT space to bait eruptions away from others
    // - OT stays just outside cleave zone to the left, ready to provoke when tank swap is needed
    //   if there are no fetters, he maintains a larger distance that would ensure others are not affected by eruptions on OT
    //   otherwise he stands closer to the center, to reduce distance to tethered partner
    // - non-tank fettered players stand behind boss, so that they don't prevent OT from safely provoking
    // - healers without searing wind stay behind boss around center, far enough away from others so that they won't have to move from eruptions
    // - non-caster dd stand in a clump near right leg (45 degrees, to allow both types of positionals for melees)
    // - casters (typically 1) stand farther away - this way they are out of vulcan burst range and won't have to move if not targeted by eruptions
    // - for dd eruption baiters, we provide a defined bait spot for second eruption, same distance away from both caster and non-caster locations
    class Ex4IfritAINormal : Ex4IfritAICommon
    {
        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var bossAngle = Angle.FromDirection(module.PrimaryActor.Position - module.Bounds.Center);
            var toBoss = bossAngle.ToDirection();

            var boss = hints.PotentialTargets.Find(e => e.Actor == module.PrimaryActor);
            if (boss != null)
            {
                boss.Priority = 1;
                boss.StayAtLongRange = true;
                UpdateBossTankingProperties(module, boss, actor);
                boss.DesiredRotation = bossAngle; // point to the wall
                if (!module.PrimaryActor.Position.InCircle(module.Bounds.Center, 13)) // 13 == radius (20) - tank distance (2) - hitbox (5)
                    boss.DesiredPosition = module.Bounds.Center + 13 * toBoss;
            }

            // position hints
            if (EruptionActive)
            {
                // eruption bait hints
                if (IsEruptionBaiter(slot))
                {
                    if (actor.Role is Role.Melee or Role.Ranged && module.PrimaryActor.CastInfo != null)
                    {
                        // specific spot for first baits
                        AddPositionHint(hints, module.PrimaryActor.Position - 11.5f * toBoss + 11 * toBoss.OrthoR());
                    }
                    else
                    {
                        AddDefaultEruptionBaiterHints(module, hints);
                    }
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
                if (IsSearingWindTarget(slot))
                {
                    // consider possible spots:
                    // - at +135 degrees - gives lots of space to OT eruption baits, but can't heal MT
                    // - at +80 degrees - can heal MT, but tight for OT (eruptions, knockbacks...)
                    // - at +100-110 degrees - can heal MT, decent space for OT, but can't possibly fit two targets - that's actually fine?..
                    var dir = bossAngle + 105.Degrees();
                    AddPositionHint(hints, module.Bounds.Center + 18 * dir.ToDirection());
                }
                else if (IsFettered(slot))
                {
                    var dir = bossAngle + (actor.Role == Role.Tank ? 75 : -135).Degrees();
                    AddPositionHint(hints, module.PrimaryActor.Position + 3 * dir.ToDirection());
                }
                else if (actor.Role == Role.Tank)
                {
                    AddPositionHint(hints, module.PrimaryActor.Position + 7.5f * (bossAngle + 75.Degrees()).ToDirection());
                }
                else if (actor.Role == Role.Healer)
                {
                    //AddPositionHint(hints, module.PrimaryActor.Position - 11.5f * toBoss);
                    AddPositionHint(hints, module.Bounds.Center);
                }
                else
                {
                    var pos = module.PrimaryActor.Position + 6 * (bossAngle - 135.Degrees()).ToDirection();
                    if (actor.Class.GetClassCategory() == ClassCategory.Caster)
                        pos -= 15 * toBoss;
                    AddPositionHint(hints, pos);
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (module.PrimaryActor.TargetID == pc.InstanceID)
            {
                // cone to help mt with proper positioning
                arena.AddCone(module.PrimaryActor.Position, 2, Angle.FromDirection(module.PrimaryActor.Position - module.Bounds.Center), Incinerate.CleaveShape.HalfAngle, ArenaColor.Safe);
            }
        }
    }

    // ai used during 'nails' phases
    // during this phase we destroy nails in CCW order, starting from one closest to boss on phase init; large nail, if any, is killed in the very end
    // - MT drags the boss so that next nail is behind his back; this allows everyone including fettered dd to focus on nails
    // - OT stays just outside cleave zone to the right (near the wall), ready to provoke when tank swap is needed
    // - searing wind target moves far enough in front of the boss, outside cleave range
    // - healers without searing wind stay in center
    // - dd stay anywhere outside cleave range, potential cleave from OT during swap, and center (so that not to bait eruptions on healers)
    class Ex4IfritAINails : Ex4IfritAICommon
    {
        private List<Actor> NailKillOrder = new();

        public override void Init(BossModule module)
        {
            base.Init(module);
            var smallNails = module.Enemies(OID.InfernalNailSmall);
            var startingNail = smallNails.Closest(module.PrimaryActor.Position);
            if (startingNail != null)
            {
                NailKillOrder.Add(startingNail);
                var startingDir = Angle.FromDirection(startingNail.Position - module.Bounds.Center);
                NailKillOrder.AddRange(smallNails.Exclude(startingNail).Select(n => (n, NailDirDist(n.Position - module.Bounds.Center, startingDir))).OrderBy(t => t.Item2.Item1).ThenBy(t => t.Item2.Item2).Select(t => t.Item1));
            }
            NailKillOrder.AddRange(module.Enemies(OID.InfernalNailLarge));
        }

        public override void Update(BossModule module)
        {
            NailKillOrder.RemoveAll(a => a.IsDestroyed || a.IsDead);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var bossAngle = Angle.FromDirection(module.PrimaryActor.Position - module.Bounds.Center);
            var nextNail = NailKillOrder.FirstOrDefault();
            foreach (var e in hints.PotentialTargets)
            {
                e.StayAtLongRange = true;
                switch ((OID)e.Actor.OID)
                {
                    case OID.Boss:
                        e.Priority = 1; // attack only if it's the only thing to do...
                        UpdateBossTankingProperties(module, e, actor);
                        (e.DesiredPosition, e.DesiredRotation) = DesiredBossPosRot(module);
                        break;
                    case OID.InfernalNailSmall:
                    case OID.InfernalNailLarge:
                        e.Priority = e.Actor == nextNail ? 2 : 0;
                        e.AttackStrength = 0;
                        e.ShouldBeTanked = false;
                        e.ForbidDOTs = (OID)e.Actor.OID == OID.InfernalNailSmall;
                        break;
                }
            }

            // position hints
            if (IsEruptionBaiter(slot))
            {
                AddDefaultEruptionBaiterHints(module, hints);
            }
            else if (module.PrimaryActor.TargetID != actor.InstanceID)
            {
                if (IsSearingWindTarget(slot))
                {
                    var dir = bossAngle + 135.Degrees();
                    AddPositionHint(hints, module.Bounds.Center + 18 * dir.ToDirection());
                }
                else if (actor.Role == Role.Tank)
                {
                    // we want to stay at desired rotation - 75 +/- 15 == bossAngle + 15 +/- 15 => inverse is bossAngle + 195 +- 165
                    hints.AddForbiddenZone(ShapeDistance.Cone(module.PrimaryActor.Position, 50, bossAngle + 195.Degrees(), 165.Degrees()));
                }
                else if (actor.Role == Role.Healer)
                {
                    AddPositionHint(hints, module.Bounds.Center);
                }
                else
                {
                    // in addition to usual hints, we want to avoid potential cleave at OT and center (so that we don't bait eruption there)
                    hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, Eruption.Radius));
                    foreach (var ot in module.Raid.WithoutSlot().Where(a => a.InstanceID != module.PrimaryActor.InstanceID && a.Role == Role.Tank))
                        hints.AddForbiddenZone(Incinerate.CleaveShape.Distance(module.PrimaryActor.Position, Angle.FromDirection(ot.Position - module.PrimaryActor.Position)));
                }
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var nextNail = NailKillOrder.FirstOrDefault();
            if (nextNail != null)
                arena.AddCircle(nextNail.Position, 2, ArenaColor.Safe);

            if (module.PrimaryActor.TargetID == pc.InstanceID)
            {
                // cone to help mt with proper positioning
                var (pos, rot) = DesiredBossPosRot(module);
                arena.AddCone(pos, 2, rot, Incinerate.CleaveShape.HalfAngle, ArenaColor.Safe);
            }
        }

        private (WPos, Angle) DesiredBossPosRot(BossModule module)
        {
            var bossOffset = module.PrimaryActor.Position - module.Bounds.Center;
            var bossAngle = Angle.FromDirection(bossOffset);
            var nextNail = NailKillOrder.FirstOrDefault();
            Angle? nextNailDir = nextNail != null && !nextNail.Position.AlmostEqual(module.Bounds.Center, 1) ? Angle.FromDirection(nextNail.Position - module.Bounds.Center) : null;
            if (nextNailDir != null)
            {
                var radius = Math.Max(13, bossOffset.Length());
                var desiredAngle = nextNailDir.Value + 30.Degrees();
                var pos = module.Bounds.Center + radius * desiredAngle.ToDirection();
                var rot = bossAngle + 90.Degrees();
                if (rot.ToDirection().Dot(pos - module.PrimaryActor.Position) < 0)
                {
                    pos = module.PrimaryActor.Position; // do not turn boss around on 'overshoot'
                }
                return (pos, rot);
            }
            else
            {
                // no nails except central, just make boss face the wall...
                return (module.PrimaryActor.Position, bossAngle);
            }
        }

        private (float, float) NailDirDist(WDir offset, Angle startingDir)
        {
            var dir = Angle.FromDirection(offset);
            if (dir.Rad < startingDir.Rad)
                dir += 360.Degrees();
            return (dir.Rad, offset.LengthSq());
        }
    }

    // ai used during invincibility (hellfire) phase
    // extremely simple positioning - mt goes to next plume safespot, searing winds target goes opposite, everyone else stacks in center for easier healing
    class Ex4IfritAIHellfire : Ex4IfritAICommon
    {
        private WDir _safespotOffset;

        public Ex4IfritAIHellfire(Angle safeSpotDir)
        {
            _safespotOffset = 18 * safeSpotDir.ToDirection();
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (IsSearingWindTarget(slot))
            {
                AddPositionHint(hints, module.Bounds.Center - _safespotOffset);
            }
            else if (module.PrimaryActor.TargetID == actor.InstanceID)
            {
                AddPositionHint(hints, module.Bounds.Center + _safespotOffset);
            }
            else
            {
                AddPositionHint(hints, module.Bounds.Center);
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            arena.AddCircle(module.Bounds.Center + _safespotOffset, 2, ArenaColor.Safe);
        }
    }
    class Ex4IfritAIHellfire1 : Ex4IfritAIHellfire { public Ex4IfritAIHellfire1() : base(150.Degrees()) { } }
    class Ex4IfritAIHellfire2 : Ex4IfritAIHellfire { public Ex4IfritAIHellfire2() : base(110.Degrees()) { } }
    class Ex4IfritAIHellfire3 : Ex4IfritAIHellfire { public Ex4IfritAIHellfire3() : base(70.Degrees()) { } }
}
