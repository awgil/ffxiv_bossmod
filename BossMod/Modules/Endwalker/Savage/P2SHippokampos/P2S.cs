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

    public class P2S : BossModule
    {
        public P2S(BossModuleManager manager, Actor primary)
            : base(manager, primary, new ArenaBoundsSquare(new(100, 100), 20))
        {
            InitStates(new P2SStates(this).Build());
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            Arena.Actor(pc, ArenaColor.PC);
        }
    }
}
