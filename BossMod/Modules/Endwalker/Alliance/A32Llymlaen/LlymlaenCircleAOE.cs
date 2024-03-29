using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A32Llymlaen
{   
    class WindRoseAOE : Components.SelfTargetedAOEs
    {
        public WindRoseAOE() : base(ActionID.MakeSpell(AID.WindRoseAOE), new AOEShapeCircle(12)) { }
    }
    class SurgingWaveAOE : Components.SelfTargetedAOEs
    {
        public SurgingWaveAOE() : base(ActionID.MakeSpell(AID.SurgingWaveAOE), new AOEShapeCircle(6)) { }
    }
    class LandingAOE : Components.SelfTargetedAOEs
    {
        public LandingAOE() : base(ActionID.MakeSpell(AID.LandingAOE), new AOEShapeCircle(18)) { }
    }
}
