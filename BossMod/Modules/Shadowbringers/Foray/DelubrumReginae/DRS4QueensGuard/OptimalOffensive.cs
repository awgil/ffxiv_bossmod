namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class OptimalOffensiveSword(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.OptimalOffensiveSword), 2.5f);
class OptimalOffensiveShield(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.OptimalOffensiveShield), 2.5f);

// note: there are two casters (as usual in bozja content for raidwides)
class OptimalOffensiveShieldKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.OptimalOffensiveShieldKnockback), 10, true, 1);

class UnluckyLot(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe = new(new AOEShapeCircle(20), module.Center, default, module.WorldState.FutureTime(7.6f));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.OptimalOffensiveShieldMoveSphere)
            _aoe = new(new AOEShapeCircle(20), caster.Position, default, WorldState.FutureTime(8.6f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.UnluckyLot)
            _aoe = null;
    }
}
