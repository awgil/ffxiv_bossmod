namespace BossMod.Stormblood.Ultimate.UCOB;

class P3EarthShaker(BossModule module) : Components.GenericBaitAway(module, AID.EarthShakerAOE)
{
    private List<Bait> _futureBaits = [];

    private static readonly AOEShapeCone _shape = new(60, 45.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker && Module.Enemies(OID.BahamutPrime).FirstOrDefault() is var source && source != null)
        {
            var list = CurrentBaits.Count < 4 ? CurrentBaits : _futureBaits;
            list.Add(new(source, actor, _shape, WorldState.FutureTime(5.1f)));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.FindComponent<P3QuickmarchTrio>() is { } qmt)
        {
            var dirNorth = (qmt.RelativeNorth - Arena.Center).ToAngle();
            if (CurrentBaits.FirstOrNull(b => b.Target == actor) is { } bait)
            {
                var (safeDir, far) = assignment switch
                {
                    PartyRolesConfig.Assignment.H1 => (dirNorth + 45.Degrees(), true),
                    PartyRolesConfig.Assignment.H2 => (dirNorth - 45.Degrees(), true),
                    _ => (dirNorth - 135.Degrees(), false)
                };

                hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, safeDir, 60, 0, 1), bait.Activation);
                if (far)
                    hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 12), bait.Activation);
            }

            var damage = new BitMask();
            foreach (var b in CurrentBaits)
                damage.Set(Raid.FindSlot(b.Target.InstanceID));

            if (damage.Any())
                hints.AddPredictedDamage(damage, CurrentBaits[0].Activation);

            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);
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

class P3EarthShakerVoidzone(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private readonly IReadOnlyList<Actor> _voidzones = module.Enemies(OID.VoidzoneEarthShaker);
    private readonly List<AOEInstance> _predicted = [];
    private BitMask _targets;

    private static readonly AOEShapeCircle _shape = new(5); // TODO: verify radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var z in _voidzones.Where(z => z.EventState != 7))
            yield return new(_shape, z.Position);
        foreach (var p in _predicted)
            yield return p;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.VoidzoneEarthShaker)
            _predicted.Clear();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Earthshaker)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EarthShaker)
            foreach (var (_, p) in Raid.WithSlot().IncludedInMask(_targets))
                _predicted.Add(new(_shape, p.Position, default, WorldState.FutureTime(1.4f)));
    }
}
