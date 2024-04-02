namespace BossMod.Endwalker.Ultimate.DSW2;

// TODO: improve...
class P7Trinity : Components.GenericBaitAway
{
    private Actor? _source;

    private static readonly AOEShapeCircle _shape = new(3);

    public P7Trinity() : base(centerAtTarget: true) { }

    public override void Init(BossModule module)
    {
        _source = module.Enemies(OID.DragonKingThordan).FirstOrDefault();
    }

    public override void Update(BossModule module)
    {
        CurrentBaits.Clear();
        if (_source != null)
        {
            foreach (var target in module.Raid.WithoutSlot().Where(p => p.Role == Role.Tank))
                CurrentBaits.Add(new(_source, target, _shape));
            var closest = module.Raid.WithoutSlot().Where(p => p.Role != Role.Tank).Closest(_source.Position);
            if (closest != null)
                CurrentBaits.Add(new(_source, closest, _shape));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TrinityAOE1 or AID.TrinityAOE2 or AID.TrinityAOE3)
        {
            ++NumCasts;
        }
    }
}
