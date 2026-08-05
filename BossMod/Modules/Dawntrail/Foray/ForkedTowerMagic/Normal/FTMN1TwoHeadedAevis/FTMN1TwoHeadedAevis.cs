namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

sealed class ThunderfrostTempest(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThunderfrostTempest);
sealed class PoisonBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoisonBreath, new AOEShapeCircle(18.0f));
sealed class StormsBreath(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.StormsBreath2, 14.0f);
sealed class TwoTerrors(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwoTerrors, new AOEShapeRect(40.0f, 5.0f));

sealed class HissingReprise(BossModule module) : Components.GenericKnockback(module) {
    private List<Knockback> knockbacks = [];
    private enum knockbackType { None, West, East}
    private (knockbackType type, DateTime expireAt)[] knockbackDebuffs = new (knockbackType, DateTime)[PartyState.MaxPartySize];
    private const float knockbackDistance = 20.0f;

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        var debuff = status.ID switch {
            (uint)SID.EasterlyReprise => knockbackType.East,
            (uint)SID.WesterlyReprise => knockbackType.West,
            _ => knockbackType.None
        };

        if (debuff != knockbackType.None) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0 && slot < PartyState.MaxPartySize) {
                knockbackDebuffs[slot] = (debuff, status.ExpireAt);
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID is (uint)SID.EasterlyReprise or (uint)SID.WesterlyReprise) {
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0 && slot < PartyState.MaxPartySize) {
                knockbackDebuffs[slot] = (knockbackType.None, default);
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        knockbacks.Clear();

        if (slot > PartyState.MaxPartySize) {
            return [];
        }

        foreach (var (i, player) in Raid.WithSlot(false, true, true)) {
            if (knockbackDebuffs[i].type == knockbackType.East) {
                knockbacks.Add(new(player.Position, knockbackDistance, knockbackDebuffs[i].expireAt, kind: Kind.DirRight));
            }

            if (knockbackDebuffs[i].type == knockbackType.West) {
                knockbacks.Add(new(player.Position, knockbackDistance, knockbackDebuffs[i].expireAt, kind: Kind.DirLeft));
            }
        }

        return CollectionsMarshal.AsSpan(knockbacks);
    }
}

// TODO figure out cast timers for ones that explode after being hit
// TODO consider changing the raidwide part to the single cast one + figure out the cast timers for the raidwide difference one as well
// TODO clean up
sealed class TwoHeadedAevisCluster(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    public List<(Actor actor, bool clusterHit)> orbs = []; // clusterHit is for when the orb is hit by its cluster element attack
    public readonly AOEShapeCircle shape = new(15.0f);

    public override void OnActorCreated(Actor actor) {
        if (actor.OID is (uint)OID.SwirlingOrb or (uint)OID.BallLightning) {
            orbs.Add((actor, false));
        }
    }

    public override void OnActorDeath(Actor actor) {
        if (actor.OID is (uint)OID.SwirlingOrb or (uint)OID.BallLightning) {
            orbs.Remove((actor, false));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.Summon)
        {
            orbs.Clear();
            aoes.Clear();
        }

        if (spell.Action.ID == (uint)AID.IceClusterTeleport) {
            var aoe = new AOEInstance(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
            aoes.Add(aoe);
        }

        if (spell.Action.ID == (uint)AID.IceClusterTeleport) {
            var aoe = new AOEInstance(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
            for (int i = 0; i < orbs.Count; i++) {
                var orb = orbs[i];
                if (orb.actor.OID == (uint)OID.SwirlingOrb && aoe.Shape.Check(aoe.Origin, orb.actor.Position, orb.actor.Rotation)) {
                    aoes.Add(new(shape, orb.actor.Position, orb.actor.Rotation, Module.CastFinishAt(spell, 3.0f)));
                    orb.clusterHit = true;
                    orbs[i] = orb;
                }
            }
        }

        if (spell.Action.ID == (uint)AID.LightningClusterTeleport) {
            var aoe = new AOEInstance(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
            aoes.Add(aoe);
        }


        if (spell.Action.ID == (uint)AID.LightningClusterTeleport) {
            var aoe = new AOEInstance(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
            for (int i = 0; i < orbs.Count; i++) {
                var orb = orbs[i];
                if (orb.actor.OID == (uint)OID.BallLightning && aoe.Shape.Check(aoe.Origin, orb.actor.Position, orb.actor.Rotation)) {
                    aoes.Add(new(shape, orb.actor.Position, orb.actor.Rotation, Module.CastFinishAt(spell, 3.0f)));
                    orb.clusterHit = true;
                    orbs[i] = orb;
                }
            }
        }

        if (spell.Action.ID == (uint)AID.TwoHeadedAevisThunderfrostTempest) { // Uses sinlge cast, so the aoes are not drawn multiple times
            foreach (var orb in orbs) {
                if (orb.clusterHit == false) {
                    aoes.Add(new(shape, orb.actor.Position, orb.actor.Rotation, Module.CastFinishAt(spell, 3.0f)));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.IceCluster or (uint)AID.HypothermalCombustion or (uint)AID.LightningCluster or (uint)AID.Shock) {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

// TODO confirm spell timers
sealed class Blaze(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle inner = new(5.0f);
    private readonly AOEShapeDonut outer = new(5.0f, 60.0f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.BlazeInner or (uint)AID.BlazeInner1 or (uint)AID.BlazeInner2) {
            aoes.Add(new(inner, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(outer, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 2.5f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.BlazeInner or (uint)AID.BlazeInner1 or (uint)AID.BlazeInner2 or (uint)AID.BlazeloopOuter) {
            if (aoes.Count > 0) {
                aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs)) {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class ArcaneBeacon : Components.SimpleAOEs {
    public ArcaneBeacon(BossModule module) : base(module, (uint)AID.ArcaneBeacon, new AOEShapeRect(60.0f, 2.5f)) {
        MaxCasts = 8;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(TwoHeadedAevisStates),
    ConfigType = null, // replace null with typeof(TwoHeadedAevisConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.GreenHead1,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14489u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class TwoHeadedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-900f, 700f), new ArenaBoundsSquare(20f));

