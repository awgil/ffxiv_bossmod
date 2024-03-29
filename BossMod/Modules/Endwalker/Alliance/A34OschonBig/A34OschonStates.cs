using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A34OschonBig

{
    class A34OschonStates : StateMachineBuilder
    {           
        public A34OschonStates(BossModule module) : base(module)
        {
            SimplePhase(0, id => { SimpleState(id, 10000, "Enrage"); }, "Single phase")
                            .ActivateOnEnter<PitonPullAOE>()
                            .ActivateOnEnter<WeaponskillAOE>()
                            .ActivateOnEnter<AltitudeAOE>()
                            .ActivateOnEnter<GreatWhirlwindAOE>()
                            .ActivateOnEnter<DownhillSmallAOE>()
                            .ActivateOnEnter<DownhillBigAOE>()
                            //.ActivateOnEnter<ArrowTrail>()
                            //.ActivateOnEnter<ArrowTrailRectAOE>() // I beleive this is redundant
                            .ActivateOnEnter<WanderingVolley>()
                            .ActivateOnEnter<WanderingVolleyAOE>()
                            .ActivateOnEnter<BigFlintedFoehn>()
                            .ActivateOnEnter<TheArrowTankbuster>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HP.Cur <= 1 && !Module.PrimaryActor.IsTargetable;
        }
    }
}
