using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A32Llymlaen

{   
    class StormwhorlLocAOE : Components.LocationTargetedAOEs
    {
        public StormwhorlLocAOE() : base(ActionID.MakeSpell(AID.StormwhorlLocAOE), 6) { }
    }
    class MaelstromLocAOE : Components.LocationTargetedAOEs
    {
        public MaelstromLocAOE() : base(ActionID.MakeSpell(AID.MaelstromLocAOE), 6) { }
    }
}
