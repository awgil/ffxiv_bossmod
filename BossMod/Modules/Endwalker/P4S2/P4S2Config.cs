using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.P4S2
{
    public class P4S2Config : ConfigNode
    {
        public bool Act4DarkSoakCCW = false;
        public bool Act4WaterBreakCCW = false;
        public bool CurtainCallDDFirst = false;

        protected override void DrawContents()
        {
            DrawProperty(ref Act4DarkSoakCCW, "Act 4: go 1/8 CCW to soak tower with dark debuff");
            DrawProperty(ref Act4WaterBreakCCW, "Act 4: go 3/8 CCW to break water tether");
            DrawProperty(ref CurtainCallDDFirst, "Curtain call: DD break debuff first");
        }

        protected override string? NameOverride()
        {
            return "P4S2";
        }
    }
}
