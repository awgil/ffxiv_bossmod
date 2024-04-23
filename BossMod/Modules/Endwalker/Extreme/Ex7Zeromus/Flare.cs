namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class FlareTowers(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.FlareAOE), 5, 4, 4);

class FlareScald(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlareAOE:
                _aoes.Add(new(_shape, caster.Position, default, WorldState.FutureTime(2.1f)));
                break;
            case AID.FlareScald:
            case AID.FlareKill:
                ++NumCasts;
                break;
        }
    }
}

class ProminenceSpine(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ProminenceSpine), new AOEShapeRect(60, 5));
class SparklingBrandingFlare(BossModule module) : Components.CastStackSpread(module, ActionID.MakeSpell(AID.BrandingFlareAOE), ActionID.MakeSpell(AID.SparkingFlareAOE), 4, 4);

class Nox : Components.StandardChasingAOEs
{
    public Nox(BossModule module) : base(module, new AOEShapeCircle(10), ActionID.MakeSpell(AID.NoxAOEFirst), ActionID.MakeSpell(AID.NoxAOERest), 5.5f, 1.6f, 5)
    {
        ExcludedTargets = Raid.WithSlot(true).Mask();
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Nox)
            ExcludedTargets.Clear(Raid.FindSlot(actor.InstanceID));
    }
}
