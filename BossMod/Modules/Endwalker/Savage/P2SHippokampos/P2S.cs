namespace BossMod.Endwalker.Savage.P2SHippokampos
{
    class DoubledImpact : CommonComponents.SharedTankbuster
    {
        public DoubledImpact() : base(ActionID.MakeSpell(AID.DoubledImpact), 6) { }
    }

    class SewageEruption : CommonComponents.Puddles
    {
        public SewageEruption() : base(ActionID.MakeSpell(AID.SewageEruptionAOE), 6) { }
    }

    [ConfigDisplay(Order = 0x120, Parent = typeof(EndwalkerConfig))]
    public class P2SConfig : CooldownPlanningConfigNode { }

    [CooldownPlanning(typeof(P2SConfig))]
    public class P2S : BossModule
    {
        public P2S(WorldState ws, Actor primary)
            : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20))
        {
            StateMachine = new P2SStates(this).Build();
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        }
    }
}
