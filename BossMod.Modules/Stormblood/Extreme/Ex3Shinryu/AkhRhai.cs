namespace BossMod.Stormblood.Extreme.Ex3Shinryu;

class AkhRhaiBait(BossModule module) : Components.UniformStackSpread(module, 0, 4, alwaysShowSpreads: true, raidwideOnResolve: false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMorn)
            AddSpreads(Raid.WithoutSlot().Where(p => p.Role != Role.Tank && p.InstanceID != spell.TargetID), Module.CastFinishAt(spell, 2.1f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AkhRhaiFirst)
            Spreads.Clear();
    }
}

class AkhRhai(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(4);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhRhaiFirst or AID.AkhRhai)
        {
            _aoes.Add(new(_shape, spell.TargetXZ, default, WorldState.FutureTime(1)));
            ++NumCasts;
        }
    }

    public override void Update() => _aoes.RemoveAll(a => a.Activation < WorldState.CurrentTime);
}
