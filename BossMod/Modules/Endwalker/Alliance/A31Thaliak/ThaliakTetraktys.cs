using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A31Thaliak

{   
    class Tetraktys : BossComponent
        {
            private bool active;

            public override void OnEventEnvControl(BossModule module, byte index, uint state)
            {
                if (state == 0x00080004 && index <= 4)
                    active = false;
            }
            public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
            {
                if ((AID)spell.Action.ID == AID.Tetraktys)
                    active = true;
            }
            public override void Update(BossModule module)
            {
                if (!active)
                    module.Arena.Bounds = A31Thaliak.BoundsSquare;
                else if (active)
                    module.Arena.Bounds = A31Thaliak.BoundsTri;
            }
        }
    class TetraBlueTriangles : Components.SelfTargetedAOEs
    {
        public TetraBlueTriangles() : base(ActionID.MakeSpell(AID.TetraBlueTriangles), new AOEShapeTriangle(16)) { }
    }
    class TetraGreenTriangles : Components.SelfTargetedAOEs
    {
        public TetraGreenTriangles() : base(ActionID.MakeSpell(AID.TetraGreenTriangles), new AOEShapeTriangle(32)) { }
    }
    class TetraktuosKosmosHelper : Components.SelfTargetedAOEs
    {
        public TetraktuosKosmosHelper() : base(ActionID.MakeSpell(AID.TetraktuosKosmosHelper), new AOEShapeRect(30, 8)) { }
    }
    public class TetraktuosKosmosHelper2 : Components.SelfTargetedAOEs
    {
        public TetraktuosKosmosHelper2() : base(ActionID.MakeSpell(AID.TetraktuosKosmosHelper2), new AOEShapeRect(30, 8)) { }
    }
}
