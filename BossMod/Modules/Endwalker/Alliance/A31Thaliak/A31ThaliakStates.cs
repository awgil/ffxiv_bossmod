using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A31Thaliak
{
    class A31ThaliakStates : StateMachineBuilder
    {           
        public A31ThaliakStates(BossModule module) : base(module)
        {
            SimplePhase(0, id => { SimpleState(id, 10000, "Enrage"); }, "Single phase")
                .ActivateOnEnter<Hieroglyphika>()
                .ActivateOnEnter<Tetraktys>()
                .ActivateOnEnter<Thlipsis>()
                .ActivateOnEnter<Hydroptosis>()
                .ActivateOnEnter<Rhyton>()
                .ActivateOnEnter<LeftBank>()
                .ActivateOnEnter<LeftBank2>()
                .ActivateOnEnter<RightBank>()
                .ActivateOnEnter<RightBank2>()
                .ActivateOnEnter<RheognosisKnockback>()
                //.ActivateOnEnter<RheognosisCrashExaflare>()
                .ActivateOnEnter<TetraBlueTriangles>()
                .ActivateOnEnter<TetraGreenTriangles>()
                .ActivateOnEnter<TetraktuosKosmosHelper>()
                .ActivateOnEnter<TetraktuosKosmosHelper2>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed;
        }
    }
}
