namespace BossMod.Endwalker.Savage.P12S1Athena
{
    [ConfigDisplay(Order = 0x1C1, Parent = typeof(EndwalkerConfig))]
    public class P12S1AthenaConfig : CooldownPlanningConfigNode
    {
        public enum EngravementOfSouls1Strategy
        {
            None,

            [PropertyDisplay("Supports CW from N, look for first matching person in final spot")]
            Default,
        }

        [PropertyDisplay("Engravement of Souls 1: resolution hints")]
        public EngravementOfSouls1Strategy Engravement1Hints = EngravementOfSouls1Strategy.Default;

        public P12S1AthenaConfig() : base(90) { }
    }
}
