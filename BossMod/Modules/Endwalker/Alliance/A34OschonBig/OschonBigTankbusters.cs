using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A34OschonBig

{
    class TheArrowTankbuster : Components.BaitAwayCast
    {
        public TheArrowTankbuster() : base(ActionID.MakeSpell(AID.TheArrowTankbuster), new AOEShapeCircle(10), true) { }
    }
}
