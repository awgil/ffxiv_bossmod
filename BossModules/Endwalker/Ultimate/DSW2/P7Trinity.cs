namespace BossMod.Endwalker.Ultimate.DSW2;

// TODO: improve...
class P7Trinity(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly Actor? _source = module.Enemies(OID.DragonKingThordan).FirstOrDefault();

    private static readonly AOEShapeCircle _shape = new(3);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null)
        {
            foreach (var target in Raid.WithoutSlot().Where(p => p.Role == Role.Tank))
                CurrentBaits.Add(new(_source, target, _shape));
            var closest = Raid.WithoutSlot().Where(p => p.Role != Role.Tank).Closest(_source.Position);
            if (closest != null)
                CurrentBaits.Add(new(_source, closest, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TrinityAOE1 or AID.TrinityAOE2 or AID.TrinityAOE3)
        {
            ++NumCasts;
        }
    }
}
