namespace BossMod.Stormblood.Ultimate.UCOB;

class P3EarthShaker : Components.GenericBaitAway
{
    private List<Bait> _futureBaits = new();

    private static readonly AOEShapeCone _shape = new(60, 45.Degrees());

    public P3EarthShaker() : base(ActionID.MakeSpell(AID.EarthShakerAOE)) { }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Earthshaker && module.Enemies(OID.BahamutPrime).FirstOrDefault() is var source && source != null)
        {
            var list = CurrentBaits.Count < 4 ? CurrentBaits : _futureBaits;
            list.Add(new(source, actor, _shape));
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

class P3EarthShakerVoidzone : Components.GenericAOEs
{
    private IReadOnlyList<Actor> _voidzones = ActorEnumeration.EmptyList;
    private List<AOEInstance> _predicted = new();
    private BitMask _targets;

    private static readonly AOEShapeCircle _shape = new(5); // TODO: verify radius

    public P3EarthShakerVoidzone() : base(default, "GTFO from voidzone!") { }

    public override void Init(BossModule module)
    {
        _voidzones = module.Enemies(OID.VoidzoneEarthShaker);
    }

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

    public override void OnEventIcon(Actor actor, uint iconID)
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
