namespace BossMod.Stormblood.Ultimate.UCOB;

class P3EarthShaker(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.EarthShakerAOE))
{
    private List<Bait> _futureBaits = [];

    private static readonly AOEShapeCone _shape = new(60, 45.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Earthshaker && Module.Enemies(OID.BahamutPrime).FirstOrDefault() is var source && source != null)
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
