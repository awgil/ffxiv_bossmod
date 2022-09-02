namespace BossMod.Endwalker.Savage.P2SHippokampos
{
    class DoubledImpact : Components.SharedTankbuster
    {
        public DoubledImpact() : base(ActionID.MakeSpell(AID.DoubledImpact), 6) { }
    }

    class SewageEruption : Components.LocationTargetedAOEs
    {
        public SewageEruption() : base(ActionID.MakeSpell(AID.SewageEruptionAOE), 6) { }
    }

    [ConfigDisplay(Order = 0x120, Parent = typeof(EndwalkerConfig))]
    public class P2SConfig : CooldownPlanningConfigNode { }

    public class P2S : BossModule
    {
        public P2S(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
    }
}
