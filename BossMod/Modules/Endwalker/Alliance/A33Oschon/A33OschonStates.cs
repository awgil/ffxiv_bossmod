using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A33Oschon

{
    class A33OschonStates : StateMachineBuilder
    {           
        public A33OschonStates(BossModule module) : base(module)
        {
            SimplePhase(0, id => { SimpleState(id, 10000, "Enrage"); }, "Single phase")
                .ActivateOnEnter<Downhill2>()
                .ActivateOnEnter<ClimbingShot>()
                .ActivateOnEnter<ClimbingShot2>()
                .ActivateOnEnter<SoaringMinuet1>()
                .ActivateOnEnter<SoaringMinuet2>()
                .ActivateOnEnter<FlintedFoehn>()
                .ActivateOnEnter<TheArrow2>()
                .ActivateOnEnter<TrekDraws>()
                //.ActivateOnEnter<Ability1>() // this is here to test what these do
                //.ActivateOnEnter<Ability2>()
                //.ActivateOnEnter<Ability3>()
                //.ActivateOnEnter<Ability4>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HP.Cur <= 1 && !Module.PrimaryActor.IsTargetable;
        }
    }
}
