namespace BossMod.Stormblood.Ultimate.UCOB;

class P2Heavensfall : Components.Knockback
{
    public P2Heavensfall() : base(ActionID.MakeSpell(AID.Heavensfall), true) { }

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        yield return new(module.Bounds.Center, 11); // TODO: activation
    }
}

class P2HeavensfallPillar : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorEAnim(BossModule module, Actor actor, uint state)
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

class P2ThermionicBurst : Components.SelfTargetedAOEs
{
    public P2ThermionicBurst() : base(ActionID.MakeSpell(AID.ThermionicBurst), new AOEShapeCone(24.5f, 11.25f.Degrees())) { }
}

class P2MeteorStream : Components.UniformStackSpread
{
    public int NumCasts;

    public P2MeteorStream() : base(0, 4, alwaysShowSpreads: true) { }

    public override void Init(BossModule module)
    {
        AddSpreads(module.Raid.WithoutSlot(true), module.WorldState.CurrentTime.AddSeconds(5.6f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorStream)
        {
            ++NumCasts;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }
}

class P2HeavensfallDalamudDive : Components.GenericBaitAway
{
    private Actor? _target;

    private static readonly AOEShapeCircle _shape = new(5);

    public P2HeavensfallDalamudDive() : base(ActionID.MakeSpell(AID.DalamudDive), true, true) { }

    public void Show()
    {
        if (_target != null)
        {
            CurrentBaits.Add(new(_target, _target, _shape));
        }
    }

    public override void Init(BossModule module)
    {
        _target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);
    }
}
