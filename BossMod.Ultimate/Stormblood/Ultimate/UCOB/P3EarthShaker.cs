namespace BossMod.Stormblood.Ultimate.UCOB;

class P3EarthShaker(BossModule module) : Components.GenericBaitAway(module, AID.EarthShakerAOE)
{
    protected List<Bait> _futureBaits = [];

    private static readonly AOEShapeCone _shape = new(60, 45.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker && Module.Enemies(OID.BahamutPrime).FirstOrDefault() is var source && source != null)
        {
            var list = CurrentBaits.Count < 4 ? CurrentBaits : _futureBaits;
            list.Add(new(source, actor, _shape, WorldState.FutureTime(5.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.EarthShaker)
        {
            CurrentBaits.Clear();
            Utils.Swap(ref CurrentBaits, ref _futureBaits);
        }
    }
}

class P3EarthShakerVoidzone(BossModule module) : Components.VoidzoneAtCastTarget(module, 4, AID.EarthShakerAOE, OID.VoidzoneEarthShaker, 1.4f, activationDelay: 1.9f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            var target = spell.Targets.Count > 0 ? WorldState.Actors.Find(spell.Targets[^1].ID) : Raid.Player();
            if (target != null)
                _predictedByEvent.Add((target.Position, WorldState.FutureTime(CastEventToSpawn + ActivationDelay)));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // spawn is quite delayed and doesn't correspond with a targeted AOE, like liquid hell, so we only want to avoid actually spawned zones
        foreach (var (z, spawn) in _sources)
            hints.AddForbiddenZone(Shape, z.Position, activation: spawn.AddSeconds(ActivationDelay));
    }
}

class P3QuickmarchEarthShaker : P3EarthShaker
{
    public P3QuickmarchEarthShaker(BossModule module) : base(module)
    {
        EnableHints = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;

        var qmt = Module.FindComponent<P3QuickmarchTrio>()!;

        var dirNorth = (qmt.RelativeNorth - Arena.Center).ToAngle();
        if (CurrentBaits.FirstOrNull(b => b.Target == actor) is { } bait)
        {
            var safeDir = assignment switch
            {
                PartyRolesConfig.Assignment.H1 => dirNorth + 45.Degrees(),
                PartyRolesConfig.Assignment.H2 => dirNorth - 45.Degrees(),
                _ => dirNorth - 135.Degrees()
            };

            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, safeDir, 60, 0, 1), bait.Activation);
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 6), bait.Activation);
            // healers should move closer to arena center to be in range of the whole party, in case i.e. R2 gets hit by megaflare
            hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 10, 0.5f));
        }
        else if (actor.Role != Role.Tank)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, dirNorth + 135.Degrees(), 60, 0, 1), CurrentBaits[0].Activation);
        }

        var damage = new BitMask();
        foreach (var b in CurrentBaits)
            damage.Set(Raid.FindSlot(b.Target.InstanceID));

        if (damage.Any())
            hints.AddPredictedDamage(damage, CurrentBaits[0].Activation);
    }
}
