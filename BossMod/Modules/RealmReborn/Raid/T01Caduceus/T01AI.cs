namespace BossMod.RealmReborn.Raid.T01Caduceus;

class T01AI : BossComponent
{
    private Platforms? _platforms;
    private HoodSwing? _hoodSwing;
    private CloneMerge? _clone;
    private IReadOnlyList<Actor> _slimes = ActorEnumeration.EmptyList;

    public override void Init(BossModule module)
    {
        _platforms = module.FindComponent<Platforms>();
        _hoodSwing = module.FindComponent<HoodSwing>();
        _clone = module.FindComponent<CloneMerge>();
        _slimes = module.Enemies(OID.DarkMatterSlime);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // note on positioning:
        // 1. after clone spawns, we just burn bosses and not spawn any slimes (on MINE there is enough DPS to kill boss at ~3 stacks)
        // 2. main boss is tanked on platform #2, clone is tanked on platform #6
        // 3. before clone is spawned, R1 (assumed to be physical) stays on platform #8 and spawns slimes except on two highest platforms
        // 4. before clone is spawned, R2 (assumed to be caster) stays on platform #0 and spawns slime there
        // 5. healers stand on platform #5 to be in range of everyone
        var cloneSpawned = _clone?.Clone != null;
        bool cloneSpawningSoon = !cloneSpawned && module.PrimaryActor.HP.Cur < 0.73f * module.PrimaryActor.HP.Max;
        var clone = _clone?.CloneIfValid;
        var hpDiff = clone != null ? (int)(clone.HP.Cur - module.PrimaryActor.HP.Cur) * 100.0f / module.PrimaryActor.HP.Max : 0;

        var activePlatforms = _platforms?.ActivePlatforms ?? new BitMask();
        if (module.StateMachine.TimeSincePhaseEnter < 10)
        {
            // do nothing for first few seconds to let MT position the boss
        }
        else if (activePlatforms.Any())
        {
            bool actorIsSpawner = !cloneSpawned && assignment == (activePlatforms[0] ? PartyRolesConfig.Assignment.R2 : PartyRolesConfig.Assignment.R1);
            Func<WPos, float> nonAllowedPlatforms = actorIsSpawner
                ? p => -activePlatforms.SetBits().Min(platform => Platforms.PlatformShapes[platform](p)) - 1 // inverse union of active, slightly reduced to avoid standing on borders
                : p => activePlatforms.SetBits().Min(platform => Platforms.PlatformShapes[platform](p)); // union of active
            hints.AddForbiddenZone(nonAllowedPlatforms, _platforms!.ExplosionAt);
        }
        else
        {
            var kitedSlime = _slimes.FirstOrDefault(slime => slime.TargetID == actor.InstanceID);
            if (kitedSlime != null)
            {
                // kiting a slime: bring it near boss until low hp, then into boss
                var dest = module.PrimaryActor.Position;
                if (kitedSlime.HP.Cur > 0.2f * kitedSlime.HP.Max)
                    dest += (kitedSlime.Position - module.PrimaryActor.Position).Normalized() * (module.PrimaryActor.HitboxRadius + kitedSlime.HitboxRadius + 6); // 6 to avoid triggering whip back
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(dest, 2), DateTime.MaxValue);
            }
            else
            {
                switch (assignment)
                {
                    //case PartyRolesConfig.Assignment.MT:
                    //    SetPreferredPlatform(hints, 2);
                    //    break;
                    case PartyRolesConfig.Assignment.OT:
                        // when clone is about to spawn, have OT move closer to tank position
                        if (cloneSpawningSoon)
                            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Platforms.HexaPlatformCenters[6], 20), DateTime.MaxValue);
                        //else if (clone != null)
                        //    SetPreferredPlatform(hints, 6);
                        break;
                    case PartyRolesConfig.Assignment.H1:
                    case PartyRolesConfig.Assignment.H2:
                        SetPreferredPlatform(hints, 5);
                        break;
                    case PartyRolesConfig.Assignment.R1:
                        if (!cloneSpawned)
                            SetPreferredPlatform(hints, 8);
                        break;
                    case PartyRolesConfig.Assignment.R2:
                        // TODO: there's a LOS problem if standing on starting platform - maybe just ignore these platforms?..
                        if (!cloneSpawned && actor.PosRot.Y > Platforms.PlatformHeights[0] - 0.1f)
                            SetPreferredPlatform(hints, 0);
                        break;
                }
            }
        }

        // attack priorities:
        // - first few seconds after split - only OT on boss to simplify pickup
        // - after that and until 30% - MT/H1/M1/R1 on boss, rest on clone
        // - after that - equal priorities unless hp diff is larger than 5%
        foreach (var e in hints.PotentialTargets)
        {
            if ((OID)e.Actor.OID == OID.Boss)
            {
                if (e.Actor == module.PrimaryActor)
                {
                    e.Priority = 1; // this is a baseline; depending on whether we want to prioritize clone vs boss, clone's priority changes
                    if (cloneSpawningSoon && e.Actor.FindStatus(SID.SteelScales) != null)
                        e.Priority = AIHints.Enemy.PriorityForbidAI; // stop dps until stack can be dropped
                }
                else
                {
                    e.Priority = assignment switch
                    {
                        PartyRolesConfig.Assignment.MT => 0,
                        PartyRolesConfig.Assignment.OT => 2,
                        _ => (module.WorldState.CurrentTime - _clone!.CloneSpawnTime).TotalSeconds < 3 || hpDiff < -5 ? 0
                            : hpDiff > 5 ? 2
                            : Math.Min(e.Actor.HP.Cur, module.PrimaryActor.HP.Cur) <= 0.3f * e.Actor.HP.Max ? 1
                            : assignment is PartyRolesConfig.Assignment.H2 or PartyRolesConfig.Assignment.M2 or PartyRolesConfig.Assignment.R2 ? 2 : 0
                    };
                    e.ShouldBeTanked = assignment == PartyRolesConfig.Assignment.OT;
                    e.PreferProvoking = true;
                    if ((e.Actor.Position - module.PrimaryActor.Position).LengthSq() < 625)
                        e.DesiredPosition = Platforms.HexaPlatformCenters[6];
                    e.DesiredRotation = -90.Degrees();
                }
                e.AttackStrength = _hoodSwing!.SecondsUntilNextCast(module) < 3 ? 0.5f : 0.2f;
                e.StayAtLongRange = true;
            }
            else if ((OID)e.Actor.OID == OID.DarkMatterSlime)
            {
                // for now, let kiter damage it until 20%
                var predictedHP = (int)e.Actor.HP.Cur + module.WorldState.PendingEffects.PendingHPDifference(e.Actor.InstanceID);
                e.Priority =
                    //predictedHP > 0.7f * e.Actor.HP.Max ? (actor.Role is Role.Ranged or Role.Melee ? 3 : AIHints.Enemy.PriorityForbidAI) :
                    predictedHP > 0.2f * e.Actor.HP.Max ? (e.Actor.TargetID == actor.InstanceID ? 3 : AIHints.Enemy.PriorityForbidAI) :
                    AIHints.Enemy.PriorityForbidAI;
                e.ShouldBeTanked = false;
                e.ForbidDOTs = true;
            }
        }
    }

    private void SetPreferredPlatform(AIHints hints, int platform)
    {
        //Func<WPos, float> nonAllowedPlatforms = p => -allowedPlatforms.SetBits().Min(platform => Platforms.PlatformShapes[platform](p)) - 1; // inverse union of allowed, slightly reduced to avoid standing on borders
        Func<WPos, float> invAllowed = p => -Platforms.PlatformShapes[platform](p) - 1; // inverted and slightly reduced to avoid standing on borders
        hints.AddForbiddenZone(invAllowed, DateTime.MaxValue);
    }
}
