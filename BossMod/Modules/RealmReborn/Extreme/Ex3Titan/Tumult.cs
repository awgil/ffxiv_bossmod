namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class Tumult : Components.CastCounter
{
    private DateTime _nextExpected;

    public Tumult() : base(ActionID.MakeSpell(AID.TumultBoss)) { }

    public override void Init(BossModule module)
    {
        _nextExpected = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Raidwide);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PredictedDamage.Add((module.Raid.WithSlot().Mask(), _nextExpected));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if (spell.Action == WatchedAction)
            _nextExpected = module.WorldState.CurrentTime.AddSeconds(1.2f);
    }
}
