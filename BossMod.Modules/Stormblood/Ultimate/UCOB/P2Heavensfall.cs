namespace BossMod.Stormblood.Ultimate.UCOB;

class P2Heavensfall(BossModule module) : Components.Knockback(module, AID.Heavensfall, true)
{
    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.Center, 11); // TODO: activation
    }
}

class P2HeavensfallPillar(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID != OID.EventHelper)
            return;
        switch (state)
        {
            case 0x00040008: // appear
                _aoe = new(_shape, actor.Position, actor.Rotation);
                break;
            // 0x00100020: ? 0.5s after appear
            // 0x00400080: ? 4.0s after appear
            // 0x01000200: ? 5.8s after appear
            // 0x04000800: ? 7.5s after appear
            // 0x10002000: ? 9.4s after appear
            case 0x40008000: // disappear (11.1s after appear)
                _aoe = null;
                break;
        }
    }
}

class P2ThermionicBurst(BossModule module) : Components.StandardAOEs(module, AID.ThermionicBurst, new AOEShapeCone(24.5f, 11.25f.Degrees()));

class P2MeteorStream : Components.UniformStackSpread
{
    public int NumCasts;

    public P2MeteorStream(BossModule module) : base(module, 0, 4, alwaysShowSpreads: true)
    {
        AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(5.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorStream)
        {
            ++NumCasts;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }
}

class P2HeavensfallDalamudDive(BossModule module) : Components.GenericBaitAway(module, AID.DalamudDive, true, true)
{
    private readonly Actor? _target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);

    private static readonly AOEShapeCircle _shape = new(5);

    public void Show()
    {
        if (_target != null)
        {
            CurrentBaits.Add(new(_target, _target, _shape));
        }
    }
}
