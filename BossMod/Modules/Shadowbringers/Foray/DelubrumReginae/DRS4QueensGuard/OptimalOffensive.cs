namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class OptimalOffensiveSword : Components.ChargeAOEs
{
    public OptimalOffensiveSword() : base(ActionID.MakeSpell(AID.OptimalOffensiveSword), 2.5f) { }
}

class OptimalOffensiveShield : Components.ChargeAOEs
{
    public OptimalOffensiveShield() : base(ActionID.MakeSpell(AID.OptimalOffensiveShield), 2.5f) { }
}

// note: there are two casters (as usual in bozja content for raidwides)
// TODO: not sure whether it ignores immunes, I assume so...
class OptimalOffensiveShieldKnockback : Components.KnockbackFromCastTarget
{
    public OptimalOffensiveShieldKnockback() : base(ActionID.MakeSpell(AID.OptimalOffensiveShieldKnockback), 10, true, 1) { }
}

class UnluckyLot : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void Init(BossModule module)
    {
        _aoe = new(new AOEShapeCircle(20), module.Bounds.Center, activation: module.WorldState.CurrentTime.AddSeconds(7.6));
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OptimalOffensiveShieldMoveSphere)
            _aoe = new(new AOEShapeCircle(20), caster.Position, activation: module.WorldState.CurrentTime.AddSeconds(8.6));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UnluckyLot)
            _aoe = null;
    }
}
