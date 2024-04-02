namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class FlareTowers : Components.CastTowers
{
    public FlareTowers() : base(ActionID.MakeSpell(AID.FlareAOE), 5, 4, 4) { }
}

class FlareScald : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new();

    private static readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlareAOE:
                _aoes.Add(new(_shape, caster.Position, default, module.WorldState.CurrentTime.AddSeconds(2.1f)));
                break;
            case AID.FlareScald:
            case AID.FlareKill:
                ++NumCasts;
                break;
        }
    }
}

class ProminenceSpine : Components.SelfTargetedAOEs
{
    public ProminenceSpine() : base(ActionID.MakeSpell(AID.ProminenceSpine), new AOEShapeRect(60, 5)) { }
}

class SparklingBrandingFlare : Components.CastStackSpread
{
    public SparklingBrandingFlare() : base(ActionID.MakeSpell(AID.BrandingFlareAOE), ActionID.MakeSpell(AID.SparkingFlareAOE), 4, 4) { }
}

class Nox : Components.StandardChasingAOEs
{
    public Nox() : base(new AOEShapeCircle(10), ActionID.MakeSpell(AID.NoxAOEFirst), ActionID.MakeSpell(AID.NoxAOERest), 5.5f, 1.6f, 5) { }

    public override void Init(BossModule module) => ExcludedTargets = module.Raid.WithSlot(true).Mask();

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Nox)
            ExcludedTargets.Clear(module.Raid.FindSlot(actor.InstanceID));
    }
}
