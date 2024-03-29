using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A32Llymlaen
{   
   class SeafoamSpiralDonut : Components.SelfTargetedAOEs
    {
        public SeafoamSpiralDonut() : base(ActionID.MakeSpell(AID.SeafoamSpiralDonut), new AOEShapeDonut(7, 70)) { }
    }
}
