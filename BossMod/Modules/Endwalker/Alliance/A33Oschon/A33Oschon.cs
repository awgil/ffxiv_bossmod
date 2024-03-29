using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A33Oschon

{   
    class TheArrow2 : Components.BaitAwayCast
    {
        public TheArrow2() : base(ActionID.MakeSpell(AID.TheArrow2), new AOEShapeCircle(6), true) { }
    }

    class FlintedFoehnStack : Components.StackWithCastTargets
    {
        public FlintedFoehnStack() : base(ActionID.MakeSpell(AID.FlintedFoehnStack), 6) { }
    }
    

    [ModuleInfo(CFCID = 962, PrimaryActorOID = 0x406D)]
    public class A33Oschon : BossModule
    {
        public A33Oschon(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-0.015f, 749.996f), 25, 25)) { }
    }
}
