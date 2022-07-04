namespace BossMod.AI
{
    [ConfigDisplay(Name = "AI settings (very experimental!!!)", Order = 5)]
    class AIConfig : ConfigNode
    {
        [PropertyDisplay("Enable AI")]
        public bool Enabled = false;
    }
}
