using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex4Ifrit
{
    // common ai features for whole fight
    class Ex4IfritAICommon : BossComponent
    {
        private Incinerate? _incinerate;
        private Eruption? _eruption;
        private SearingWind? _searingWind;
        private InfernalFetters? _infernalFetters;
        protected DateTime CreatedAt;
        public PartyRolesConfig.Assignment BossTankRole = PartyRolesConfig.Assignment.Unassigned;

        public override void Init(BossModule module)
        {
            _incinerate = module.FindComponent<Incinerate>();
            _eruption = module.FindComponent<Eruption>();
            _searingWind = module.FindComponent<SearingWind>();
            _infernalFetters = module.FindComponent<InfernalFetters>();
            CreatedAt = module.WorldState.CurrentTime;
        }

        protected bool IsInvincible(Actor actor) => actor.FindStatus(SID.Invincibility) != null;
        protected int TankVulnStacks(Actor? tank) => tank?.FindStatus(SID.Suppuration)?.Extra ?? 0;
        protected bool EruptionActive => _eruption?.Casters.Count > 0;
        protected bool IsEruptionBaiter(int slot) => _eruption != null && _eruption.Baiters[slot];
        protected bool IsSearingWindTarget(Actor actor) => _searingWind?.IsSpreadTarget(actor) ?? false;
        protected bool IsFettered(int slot) => _infernalFetters != null && _infernalFetters.Fetters[slot];
        protected int IncinerateCount => _incinerate?.NumCasts ?? 0;

        protected void UpdateBossTankingProperties(BossModule module, AIHints.Enemy boss, Actor player, PartyRolesConfig.Assignment assignment)
        {
            boss.AttackStrength = 0.35f;
            boss.DesiredRotation = Angle.FromDirection(module.PrimaryActor.Position - module.Bounds.Center); // point to the wall
            if (!module.PrimaryActor.Position.InCircle(module.Bounds.Center, 13)) // 13 == radius (20) - tank distance (2) - hitbox (5)
                boss.DesiredPosition = module.Bounds.Center + 13 * boss.DesiredRotation.ToDirection();
            if (player.Role == Role.Tank)
            {
                if (player.InstanceID == boss.Actor.TargetID)
                {
                    // continue tanking until OT taunts
                    boss.ShouldBeTanked = true;
                }
                else if (assignment == BossTankRole)
                //else if (TankVulnStacks(player) == 0 && TankVulnStacks(module.WorldState.Actors.Find(boss.Actor.TargetID)) >= 2)
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

        // TODO: this shouldn't be here...
        protected void PlanAction(AIHints hints, Actor player, ActionID action, float phaseTime, float minPhaseTime, float maxPhaseTime)
        {
            if (action && phaseTime >= minPhaseTime && phaseTime < maxPhaseTime)
            {
                hints.PlannedActions.Add((action, player, maxPhaseTime - phaseTime, false));
            }
        }

        protected void PlanStrongCooldown(AIHints hints, Actor player, float phaseTime, float minPhaseTime, float maxPhaseTime)
        {
            var aid = player.Class switch
            {
                Class.WAR => ActionID.MakeSpell(WAR.AID.Vengeance),
                Class.PLD => ActionID.MakeSpell(PLD.AID.Sentinel),
                _ => new()
            };
            PlanAction(hints, player, aid, phaseTime, minPhaseTime, maxPhaseTime);
        }
        protected void PlanRampart(AIHints hints, Actor player, float phaseTime, float minPhaseTime, float maxPhaseTime) => PlanAction(hints, player, ActionID.MakeSpell(WAR.AID.Rampart), phaseTime, minPhaseTime, maxPhaseTime);
        protected void PlanReprisal(AIHints hints, Actor player, float phaseTime, float minPhaseTime, float maxPhaseTime) => PlanAction(hints, player, ActionID.MakeSpell(WAR.AID.Reprisal), phaseTime, minPhaseTime, maxPhaseTime);
    }

    // ai used during 'normal' phases (no plumes or cyclones)
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
                UpdateBossTankingProperties(module, boss, actor, assignment);
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
                if (IsSearingWindTarget(actor))
                {
                    // consider possible spots:
                    // - at +135 degrees - gives lots of space to OT eruption baits, but can't heal MT
                    // - at +80 degrees - can heal MT, but tight for OT (eruptions, knockbacks...)
                    // - at +100-110 degrees - can heal MT, decent space for OT, but can't possibly fit two targets - that's actually fine?..
                    var dir = !actor.Position.InCircle(module.Bounds.Center, 10) ? Angle.FromDirection(actor.Position - module.Bounds.Center)
                        : bossAngle + 105.Degrees();
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

    // common ai used during 'nails' phases
    // while there are still nails to kill, uses custom positioning hints and cooldowns
    // nail kill order is always CW starting from E; large nail is last
    // - MT keeps boss where he is, rotating to face wall (unless next nail to kill is in cleave)
    // - searing wind target moves either CW from boss or CCW, depending on phase-specific remaining nail threshold
    // - healers without searing wind stay in center
    // - dd stay anywhere outside cleave range and center (so that not to bait eruptions on healers)
    class Ex4IfritAINails : Ex4IfritAINormal
    {
        private List<Actor> NailKillOrder = new();
        private int MinNailsForCWSearingWinds;
        private BitMask OTTankAtIncinerateCounts;

        public Ex4IfritAINails(int minNailsForCWSearingWinds, ulong otTankAtIncinerateCounts)
        {
            MinNailsForCWSearingWinds = minNailsForCWSearingWinds;
            OTTankAtIncinerateCounts = new(otTankAtIncinerateCounts);
        }

        public override void Init(BossModule module)
        {
            base.Init(module);
            var smallNails = module.Enemies(OID.InfernalNailSmall);
            var startingNail = smallNails.Closest(module.Bounds.Center + new WDir(15, 0));
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
            BossTankRole = OTTankAtIncinerateCounts[IncinerateCount] ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT;
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var nextNail = NailKillOrder.FirstOrDefault();
            if (nextNail == null)
            {
                base.AddAIHints(module, slot, actor, assignment, hints);
            }
            else
            {
                var bossAngle = Angle.FromDirection(module.PrimaryActor.Position - module.Bounds.Center);
                foreach (var e in hints.PotentialTargets)
                {
                    e.StayAtLongRange = true;
                    switch ((OID)e.Actor.OID)
                    {
                        case OID.Boss:
                            e.Priority = 1; // attack only if it's the only thing to do...
                            UpdateBossTankingProperties(module, e, actor, assignment);
                            if (nextNail.Position.InCone(e.Actor.Position, e.DesiredRotation, Incinerate.CleaveShape.HalfAngle))
                            {
                                var bossToNail = Angle.FromDirection(nextNail.Position - e.Actor.Position);
                                e.DesiredRotation = bossToNail + (bossToNail.Rad > e.DesiredRotation.Rad ? -75 : 75).Degrees();
                            }
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
                    bool invertedSW = NailKillOrder.Count <= MinNailsForCWSearingWinds;
                    if (IsSearingWindTarget(actor))
                    {
                        var dir = !actor.Position.InCircle(module.Bounds.Center, 10) ? Angle.FromDirection(actor.Position - module.Bounds.Center)
                            : bossAngle + (invertedSW ? -105 : 105).Degrees();
                        AddPositionHint(hints, module.Bounds.Center + 18 * dir.ToDirection());
                    }
                    else if (assignment == BossTankRole)
                    {
                        var dir = bossAngle + (invertedSW ? 75 : -75).Degrees();
                        AddPositionHint(hints, module.PrimaryActor.Position + 7.5f * dir.ToDirection());
                    }
                    else if (actor.Role == Role.Healer)
                    {
                        AddPositionHint(hints, module.Bounds.Center);
                    }
                    else
                    {
                        // in addition to usual hints, we want to avoid center (so that we don't bait eruption there)
                        if (!EruptionActive)
                            hints.AddForbiddenZone(ShapeDistance.Circle(module.Bounds.Center, Eruption.Radius));
                    }
                }

                // heavy raidwide on large nail death
                if ((OID)nextNail.OID == OID.InfernalNailLarge && nextNail.HP.Cur < 0.5f * nextNail.HP.Max)
                    hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), new()));
            }
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            base.DrawArenaForeground(module, pcSlot, pc, arena);
            var nextNail = NailKillOrder.FirstOrDefault();
            if (nextNail != null)
                arena.AddCircle(nextNail.Position, 2, ArenaColor.Safe);
        }

        private (float, float) NailDirDist(WDir offset, Angle startingDir)
        {
            var dir = startingDir - Angle.FromDirection(offset);
            if (dir.Rad < 0)
                dir += 360.Degrees();
            return (dir.Rad, offset.LengthSq());
        }
    }

    class Ex4IfritAINails1 : Ex4IfritAINails
    {
        public Ex4IfritAINails1() : base(1, 0x8) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);

            if (module.PrimaryActor.TargetID == actor.InstanceID)
            {
                var phaseTime = (float)(module.WorldState.CurrentTime - CreatedAt).TotalSeconds;
                PlanRampart(hints, actor, phaseTime, 10, 20);
            }
        }
    }

    class Ex4IfritAINails2 : Ex4IfritAINails
    {
        public Ex4IfritAINails2() : base(4, 0x7) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);

            if (module.PrimaryActor.TargetID == actor.InstanceID)
            {
                var phaseTime = (float)(module.WorldState.CurrentTime - CreatedAt).TotalSeconds;
                PlanRampart(hints, actor, phaseTime, 8, 13);
            }
        }
    }

    class Ex4IfritAINails3 : Ex4IfritAINails
    {
        public Ex4IfritAINails3() : base(7, 0x3C70) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);

            if (module.PrimaryActor.TargetID == actor.InstanceID)
            {
                var phaseTime = (float)(module.WorldState.CurrentTime - CreatedAt).TotalSeconds;
                PlanRampart(hints, actor, phaseTime, 9, 13);
                PlanReprisal(hints, actor, phaseTime, 20, 25);
                PlanStrongCooldown(hints, actor, phaseTime, 35, 40);
                PlanStrongCooldown(hints, actor, phaseTime, 57, 63);
                PlanRampart(hints, actor, phaseTime, 91, 97);
                PlanReprisal(hints, actor, phaseTime, 102, 109);
            }
        }
    }

    // ai used during invincibility (hellfire) phase
    // extremely simple positioning - mt goes to next plume safespot, searing winds target goes opposite, everyone else stacks in center for easier healing
    class Ex4IfritAIHellfire : Ex4IfritAICommon
    {
        private WDir _safespotOffset;

        public Ex4IfritAIHellfire(Angle safeSpotDir, PartyRolesConfig.Assignment tankRole)
        {
            _safespotOffset = 18 * safeSpotDir.ToDirection();
            BossTankRole = tankRole;
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            var boss = hints.PotentialTargets.Find(e => e.Actor == module.PrimaryActor);
            if (boss != null)
            {
                boss.Priority = 1;
                boss.StayAtLongRange = true;
                boss.DesiredRotation = Angle.FromDirection(_safespotOffset);
                boss.DesiredPosition = module.Bounds.Center + 13 * boss.DesiredRotation.ToDirection();
                boss.PreferProvoking = boss.ShouldBeTanked = assignment == BossTankRole;
            }

            if (IsSearingWindTarget(actor))
            {
                AddPositionHint(hints, module.Bounds.Center - _safespotOffset);
            }
            else if (BossTankRole == assignment)
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
    class Ex4IfritAIHellfire1 : Ex4IfritAIHellfire { public Ex4IfritAIHellfire1() : base(150.Degrees(), PartyRolesConfig.Assignment.MT) { } }
    class Ex4IfritAIHellfire2 : Ex4IfritAIHellfire { public Ex4IfritAIHellfire2() : base(110.Degrees(), PartyRolesConfig.Assignment.OT) { } }
    class Ex4IfritAIHellfire3 : Ex4IfritAIHellfire { public Ex4IfritAIHellfire3() : base(70.Degrees(), PartyRolesConfig.Assignment.MT) { } }
}
