namespace BossMod
{
    [ConfigDisplay(Parent = typeof(AutorotationConfig))]
    class WHMConfig : ConfigNode
    {
        public bool FullRotation = true;
        public bool SwiftFreeRaise = true;
        public bool MouseoverFriendly = true;
        public bool SmartCure3Target = true;
        public bool NeverOvercapBloodLilies = false;

        public override void DrawContents(Tree tree)
        {
            DrawProperty(ref FullRotation, "Execute optimal rotations on Glare (ST damage), Holy (AOE damage), Cure1 (ST heal) and Medica1 (AOE heal)");
            DrawProperty(ref SwiftFreeRaise, "When trying to cast raise, apply swiftcast and thin air automatically, if possible");
            DrawProperty(ref MouseoverFriendly, "Use mouseover targeting for friendly spells");
            DrawProperty(ref SmartCure3Target, "Smart targeting for Cure 3 (target/mouseover if friendly, otherwise party member that has most nearby damaged players)");
            DrawProperty(ref NeverOvercapBloodLilies, "Never overcap blood lilies: cast misery instead of solace/rapture if needed");
        }
    }
}
