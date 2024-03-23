using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A32Llymlaen
{   
    //
    [ModuleInfo(CFCID = 962, PrimaryActorOID = 0x4024)]
    public class A32Llymlaen : BossModule
    {
        public A32Llymlaen(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-0.015f, -900.023f),20, 30))
        {
        }
        protected override void DrawArenaForeground(int pcSlot, Actor pc)
        {
            Arena.PathLineTo(new(-20.015f, -870.023f)); // Top Left
            Arena.PathLineTo(new(19.985f, -870.023f)); // Top-Right
            Arena.PathLineTo(new(19.985f, -930.023f)); // Bottom-Right
            Arena.PathLineTo(new(-20.015f, -930.023f)); // Bottom-Left
            Arena.PathLineTo(new(-20.015f, -905.000f)); 
            Arena.PathLineTo(new(-78.000f, -905.000f));
            Arena.PathLineTo(new(-78.000f, -895.000f)); 
            Arena.PathLineTo(new(-20.015f, -895.000f)); 
            Arena.PathStroke(true, ArenaColor.Border);
        }
    }
}
