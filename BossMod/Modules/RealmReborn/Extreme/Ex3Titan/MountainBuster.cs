namespace BossMod.RealmReborn.Extreme.Ex3Titan
{
    class MountainBuster : Components.Cleave
    {
        public MountainBuster() : base(ActionID.MakeSpell(AID.MountainBuster), new AOEShapeCone(21.25f, 60.Degrees())) { } // TODO: verify angle

        public override void Init(BossModule module)
        {
            base.Init(module);
            NextExpected = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Tankbuster);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            var boss = hints.PotentialTargets.Find(e => (OID)e.Actor.OID == OID.Boss);
            if (boss != null)
                boss.AttackStrength += 0.25f;
        }
    }

    class RockBuster : Components.Cleave
    {
        public RockBuster() : base(ActionID.MakeSpell(AID.RockBuster), new AOEShapeCone(11.25f, 60.Degrees())) { } // TODO: verify angle

        public override void Init(BossModule module)
        {
            base.Init(module);
            NextExpected = module.StateMachine.NextTransitionWithFlag(StateMachine.StateHint.Tankbuster);
        }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            var boss = hints.PotentialTargets.Find(e => (OID)e.Actor.OID == OID.TitansHeart);
            if (boss != null)
                boss.AttackStrength += 0.25f;
        }
    }
}
