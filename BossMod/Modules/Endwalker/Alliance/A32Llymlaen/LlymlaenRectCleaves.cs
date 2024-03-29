using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A32Llymlaen
{
   class FrothingSeaRectAOE : Components.SelfTargetedAOEs
    {
        public FrothingSeaRectAOE() : base(ActionID.MakeSpell(AID.FrothingSeaRectAOE), new AOEShapeRect(25, 50)) { }
    }
    
    class SerpentsTideRectAOE1 : Components.SelfTargetedAOEs
    {
        public SerpentsTideRectAOE1() : base(ActionID.MakeSpell(AID.SerpentsTideRectAOE1), new AOEShapeRect(80, 10)) { }
    }
    class SerpentsTideRectAOE2 : Components.SelfTargetedAOEs
    {
        public SerpentsTideRectAOE2() : base(ActionID.MakeSpell(AID.SerpentsTideRectAOE2), new AOEShapeRect(80, 10)) { }
    }
    class SerpentsTideRectAOE3 : Components.SelfTargetedAOEs
    {
        public SerpentsTideRectAOE3() : base(ActionID.MakeSpell(AID.SerpentsTideRectAOE3), new AOEShapeRect(80, 10)) { }
    }
    class SerpentsTideRectAOE4 : Components.SelfTargetedAOEs
    {
        public SerpentsTideRectAOE4() : base(ActionID.MakeSpell(AID.SerpentsTideRectAOE4), new AOEShapeRect(80, 10)) { }
    }
   class SerpentsTideRectAOE5 : Components.SelfTargetedAOEs
    {
        public SerpentsTideRectAOE5() : base(ActionID.MakeSpell(AID.SerpentsTideRectAOE5), new AOEShapeRect(80, 10)) { }
    }
    class SerpentsTideRectAOE6 : Components.SelfTargetedAOEs
    {
        public SerpentsTideRectAOE6() : base(ActionID.MakeSpell(AID.SerpentsTideRectAOE6), new AOEShapeRect(80, 5)) { }
    }
    class ToTheLastRectAOE : Components.SelfTargetedAOEs
    {
        public ToTheLastRectAOE() : base(ActionID.MakeSpell(AID.ToTheLastRectAOE), new AOEShapeRect(80, 5)) { }
    }
}
