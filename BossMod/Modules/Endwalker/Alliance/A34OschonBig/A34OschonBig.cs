using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A34OschonBig

{   

    [ModuleInfo(CFCID = 962, PrimaryActorOID = 0x406F)]
    public class A34Oschon : BossModule
    {
        public A34Oschon(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-0.015f, 749.996f), 20, 20)) { }
        
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
            foreach (var s in Enemies(OID.OschonHelper))
                Arena.Actor(s, ArenaColor.Object, false);
        }
    }
}
