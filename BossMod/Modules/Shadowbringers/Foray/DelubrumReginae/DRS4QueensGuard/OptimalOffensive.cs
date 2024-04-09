namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class OptimalOffensiveSword(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.OptimalOffensiveSword), 2.5f);

class OptimalOffensiveShield(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.OptimalOffensiveShield), 2.5f);

// note: there are two casters (as usual in bozja content for raidwides)
// TODO: not sure whether it ignores immunes, I assume so...
class OptimalOffensiveShieldKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.OptimalOffensiveShieldKnockback), 10, true, 1);

class UnluckyLot : Components.GenericAOEs
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void Init(BossModule module)
    {
        _aoe = new(new AOEShapeCircle(20), Module.Bounds.Center, activation: WorldState.FutureTime(7.6));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OptimalOffensiveShieldMoveSphere)
            _aoe = new(new AOEShapeCircle(20), caster.Position, activation: WorldState.FutureTime(8.6));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UnluckyLot)
            _aoe = null;
    }
}
