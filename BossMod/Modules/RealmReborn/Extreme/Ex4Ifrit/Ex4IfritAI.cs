namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

// common ai features for whole fight
class Ex4IfritAICommon(BossModule module) : BossComponent(module)
{
    private readonly Incinerate? _incinerate = module.FindComponent<Incinerate>();
    private readonly Eruption? _eruption = module.FindComponent<Eruption>();
    private readonly SearingWind? _searingWind = module.FindComponent<SearingWind>();
    private readonly InfernalFetters? _infernalFetters = module.FindComponent<InfernalFetters>();
    protected DateTime CreatedAt = module.WorldState.CurrentTime;
    public PartyRolesConfig.Assignment BossTankRole = PartyRolesConfig.Assignment.Unassigned;

    protected bool IsInvincible(Actor actor) => actor.FindStatus(SID.Invincibility) != null;
    protected int TankVulnStacks(Actor? tank) => tank?.FindStatus(SID.Suppuration)?.Extra ?? 0;
    protected bool EruptionActive => _eruption?.Casters.Count > 0;
    protected bool IsEruptionBaiter(int slot) => _eruption != null && _eruption.Baiters[slot];
    protected bool IsSearingWindTarget(Actor actor) => _searingWind?.IsSpreadTarget(actor) ?? false;
    protected bool IsFettered(int slot) => _infernalFetters != null && _infernalFetters.Fetters[slot];
    protected int IncinerateCount => _incinerate?.NumCasts ?? 0;

    protected void UpdateBossTankingProperties(AIHints.Enemy boss, Actor player, PartyRolesConfig.Assignment assignment)
    {
        boss.AttackStrength = 0.35f;
        boss.DesiredRotation = Angle.FromDirection(Module.PrimaryActor.Position - Module.Center); // point to the wall
        if (!Module.PrimaryActor.Position.InCircle(Module.Center, 13)) // 13 == radius (20) - tank distance (2) - hitbox (5)
            boss.DesiredPosition = Module.Center + 13 * boss.DesiredRotation.ToDirection();
        if (player.Role == Role.Tank)
        {
            if (player.InstanceID == boss.Actor.TargetID)
            {
                // continue tanking until OT taunts
                boss.ShouldBeTanked = true;
            }
            else if (assignment == BossTankRole)
            //else if (TankVulnStacks(player) == 0 && TankVulnStacks(WorldState.Actors.Find(boss.Actor.TargetID)) >= 2)
            {
                // taunt if safe
                var dirIfTaunted = Angle.FromDirection(player.Position - Module.PrimaryActor.Position);
                boss.PreferProvoking = boss.ShouldBeTanked = !Raid.WithoutSlot().Any(a => a.Role != Role.Tank && Incinerate.CleaveShape.Check(a.Position, Module.PrimaryActor.Position, dirIfTaunted));
            }
        }
    }

    protected void AddPositionHint(AIHints hints, WPos target, bool asap = true, float radius = 2)
    {
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(target, radius), asap ? new() : DateTime.MaxValue);
    }

    protected void AddDefaultEruptionBaiterHints(AIHints hints)
    {
        // avoid non-baiters (TODO: should this be done by eruption component itself?)
        if (_eruption != null)
            foreach (var (i, p) in Raid.WithSlot().ExcludedFromMask(_eruption.Baiters))
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
class Ex4IfritAINormal(BossModule module) : Ex4IfritAICommon(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var bossAngle = Angle.FromDirection(Module.PrimaryActor.Position - Module.Center);
        var toBoss = bossAngle.ToDirection();

        var boss = hints.PotentialTargets.Find(e => e.Actor == Module.PrimaryActor);
        if (boss != null)
        {
            boss.Priority = 1;
            boss.StayAtLongRange = true;
            UpdateBossTankingProperties(boss, actor, assignment);
        }

        // position hints
        if (EruptionActive)
        {
            // eruption bait hints
            if (IsEruptionBaiter(slot))
            {
                if (actor.Role is Role.Melee or Role.Ranged && Module.PrimaryActor.CastInfo != null)
                {
                    // specific spot for first baits
                    AddPositionHint(hints, Module.PrimaryActor.Position - 11.5f * toBoss + 11 * toBoss.OrthoR());
                }
                else
                {
                    AddDefaultEruptionBaiterHints(hints);
                }
            }
        }
        else if (Module.PrimaryActor.TargetID != actor.InstanceID)
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
                var dir = !actor.Position.InCircle(Module.Center, 10) ? Angle.FromDirection(actor.Position - Module.Center)
                    : bossAngle + 105.Degrees();
                AddPositionHint(hints, Module.Center + 18 * dir.ToDirection());
            }
            else if (IsFettered(slot))
            {
                var dir = bossAngle + (actor.Role == Role.Tank ? 75 : -135).Degrees();
                AddPositionHint(hints, Module.PrimaryActor.Position + 3 * dir.ToDirection());
            }
            else if (actor.Role == Role.Tank)
            {
                AddPositionHint(hints, Module.PrimaryActor.Position + 7.5f * (bossAngle + 75.Degrees()).ToDirection());
            }
            else if (actor.Role == Role.Healer)
            {
                //AddPositionHint(hints, Module.PrimaryActor.Position - 11.5f * toBoss);
                AddPositionHint(hints, Module.Center);
            }
            else
            {
                var pos = Module.PrimaryActor.Position + 6 * (bossAngle - 135.Degrees()).ToDirection();
                if (actor.Class.GetClassCategory() == ClassCategory.Caster)
                    pos -= 15 * toBoss;
                AddPositionHint(hints, pos);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Module.PrimaryActor.TargetID == pc.InstanceID)
        {
            // cone to help mt with proper positioning
            Arena.AddCone(Module.PrimaryActor.Position, 2, Angle.FromDirection(Module.PrimaryActor.Position - Module.Center), Incinerate.CleaveShape.HalfAngle, ArenaColor.Safe);
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
    private readonly List<Actor> NailKillOrder = [];
    private readonly int MinNailsForCWSearingWinds;
    private BitMask OTTankAtIncinerateCounts;

    public Ex4IfritAINails(BossModule module, int minNailsForCWSearingWinds, ulong otTankAtIncinerateCounts) : base(module)
    {
        MinNailsForCWSearingWinds = minNailsForCWSearingWinds;
        OTTankAtIncinerateCounts = new(otTankAtIncinerateCounts);

        var smallNails = module.Enemies(OID.InfernalNailSmall);
        var startingNail = smallNails.Closest(Module.Center + new WDir(15, 0));
        if (startingNail != null)
        {
            NailKillOrder.Add(startingNail);
            var startingDir = Angle.FromDirection(startingNail.Position - Module.Center);
            NailKillOrder.AddRange(smallNails.Exclude(startingNail).Select(n => (n, NailDirDist(n.Position - Module.Center, startingDir))).OrderBy(t => t.Item2.Item1).ThenBy(t => t.Item2.Item2).Select(t => t.n));
        }
        NailKillOrder.AddRange(module.Enemies(OID.InfernalNailLarge));
    }

    public override void Update()
    {
        NailKillOrder.RemoveAll(a => a.IsDestroyed || a.IsDead);
        BossTankRole = OTTankAtIncinerateCounts[IncinerateCount] ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var nextNail = NailKillOrder.FirstOrDefault();
        if (nextNail == null)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
        else
        {
            var bossAngle = Angle.FromDirection(Module.PrimaryActor.Position - Module.Center);
            foreach (var e in hints.PotentialTargets)
            {
                e.StayAtLongRange = true;
                switch ((OID)e.Actor.OID)
                {
                    case OID.Boss:
                        e.Priority = 1; // attack only if it's the only thing to do...
                        UpdateBossTankingProperties(e, actor, assignment);
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
                AddDefaultEruptionBaiterHints(hints);
            }
            else if (Module.PrimaryActor.TargetID != actor.InstanceID)
            {
                bool invertedSW = NailKillOrder.Count <= MinNailsForCWSearingWinds;
                if (IsSearingWindTarget(actor))
                {
                    var dir = !actor.Position.InCircle(Module.Center, 10) ? Angle.FromDirection(actor.Position - Module.Center)
                        : bossAngle + (invertedSW ? -105 : 105).Degrees();
                    AddPositionHint(hints, Module.Center + 18 * dir.ToDirection());
                }
                else if (assignment == BossTankRole)
                {
                    var dir = bossAngle + (invertedSW ? 75 : -75).Degrees();
                    AddPositionHint(hints, Module.PrimaryActor.Position + 7.5f * dir.ToDirection());
                }
                else if (actor.Role == Role.Healer)
                {
                    AddPositionHint(hints, Module.Center);
                }
                else
                {
                    // in addition to usual hints, we want to avoid center (so that we don't bait eruption there)
                    if (!EruptionActive)
                        hints.AddForbiddenZone(ShapeDistance.Circle(Module.Center, Eruption.Radius));
                }
            }

            // heavy raidwide on large nail death
            if ((OID)nextNail.OID == OID.InfernalNailLarge && nextNail.HPMP.CurHP < 0.5f * nextNail.HPMP.MaxHP)
                hints.PredictedDamage.Add((Raid.WithSlot().Mask(), new()));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var nextNail = NailKillOrder.FirstOrDefault();
        if (nextNail != null)
            Arena.AddCircle(nextNail.Position, 2, ArenaColor.Safe);
    }

    private (float, float) NailDirDist(WDir offset, Angle startingDir)
    {
        var dir = startingDir - Angle.FromDirection(offset);
        if (dir.Rad < 0)
            dir += 360.Degrees();
        return (dir.Rad, offset.LengthSq());
    }
}

class Ex4IfritAINails1(BossModule module) : Ex4IfritAINails(module, 1, 0x8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Module.PrimaryActor.TargetID == actor.InstanceID)
        {
            var phaseTime = (float)(WorldState.CurrentTime - CreatedAt).TotalSeconds;
            PlanRampart(hints, actor, phaseTime, 10, 20);
        }
    }
}

class Ex4IfritAINails2(BossModule module) : Ex4IfritAINails(module, 4, 0x7)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Module.PrimaryActor.TargetID == actor.InstanceID)
        {
            var phaseTime = (float)(WorldState.CurrentTime - CreatedAt).TotalSeconds;
            PlanRampart(hints, actor, phaseTime, 8, 13);
        }
    }
}

class Ex4IfritAINails3(BossModule module) : Ex4IfritAINails(module, 7, 0x3C70)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Module.PrimaryActor.TargetID == actor.InstanceID)
        {
            var phaseTime = (float)(WorldState.CurrentTime - CreatedAt).TotalSeconds;
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

    public Ex4IfritAIHellfire(BossModule module, Angle safeSpotDir, PartyRolesConfig.Assignment tankRole) : base(module)
    {
        _safespotOffset = 18 * safeSpotDir.ToDirection();
        BossTankRole = tankRole;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var boss = hints.PotentialTargets.Find(e => e.Actor == Module.PrimaryActor);
        if (boss != null)
        {
            boss.Priority = 1;
            boss.StayAtLongRange = true;
            boss.DesiredRotation = Angle.FromDirection(_safespotOffset);
            boss.DesiredPosition = Module.Center + 13 * boss.DesiredRotation.ToDirection();
            boss.PreferProvoking = boss.ShouldBeTanked = assignment == BossTankRole;
        }

        if (IsSearingWindTarget(actor))
        {
            AddPositionHint(hints, Module.Center - _safespotOffset);
        }
        else if (BossTankRole == assignment)
        {
            AddPositionHint(hints, Module.Center + _safespotOffset);
        }
        else
        {
            AddPositionHint(hints, Module.Center);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddCircle(Module.Center + _safespotOffset, 2, ArenaColor.Safe);
    }
}
class Ex4IfritAIHellfire1(BossModule module) : Ex4IfritAIHellfire(module, 150.Degrees(), PartyRolesConfig.Assignment.MT);
class Ex4IfritAIHellfire2(BossModule module) : Ex4IfritAIHellfire(module, 110.Degrees(), PartyRolesConfig.Assignment.OT);
class Ex4IfritAIHellfire3(BossModule module) : Ex4IfritAIHellfire(module, 70.Degrees(), PartyRolesConfig.Assignment.MT);
