namespace BossMod.Endwalker.Ultimate.TOP;

class P6FlashGale : Components.GenericBaitAway
{
    private readonly Actor? _source;

    private static readonly AOEShapeCircle _shape = new(5);

    public P6FlashGale(BossModule module) : base(module, centerAtTarget: true)
    {
        _source = module.Enemies(OID.BossP6).FirstOrDefault();
        ForbiddenPlayers = Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask();
    }

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null)
        {
            var mainTarget = WorldState.Actors.Find(_source.TargetID);
            var farTarget = Raid.WithoutSlot().Farthest(_source.Position);
            if (mainTarget != null)
                CurrentBaits.Add(new(_source, mainTarget, _shape));
            if (farTarget != null)
                CurrentBaits.Add(new(_source, farTarget, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlashGale)
            ++NumCasts;
    }
}
