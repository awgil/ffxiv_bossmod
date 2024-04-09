namespace BossMod.RealmReborn.Extreme.Ex3Titan;

class MountainBuster : Components.Cleave
{
    public MountainBuster(BossModule module) : base(module, ActionID.MakeSpell(AID.MountainBuster), new AOEShapeCone(21.25f, 60.Degrees())) // TODO: verify angle
    {
        NextExpected = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Tankbuster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var boss = hints.PotentialTargets.Find(e => (OID)e.Actor.OID == OID.Boss);
        if (boss != null)
            boss.AttackStrength += 0.25f;
    }
}

class RockBuster : Components.Cleave
{
    public RockBuster(BossModule module) : base(module, ActionID.MakeSpell(AID.RockBuster), new AOEShapeCone(11.25f, 60.Degrees())) // TODO: verify angle
    {
        NextExpected = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Tankbuster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var boss = hints.PotentialTargets.Find(e => (OID)e.Actor.OID == OID.TitansHeart);
        if (boss != null)
            boss.AttackStrength += 0.25f;
    }
}
