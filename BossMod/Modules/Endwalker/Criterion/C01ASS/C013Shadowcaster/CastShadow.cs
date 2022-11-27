using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    // TODO: this is incorrect...
    class CastShadow : Components.GenericAOEs
    {
        private Angle _startingAngle;
        //protected DateTime Activation;

        private static AOEShapeCone _shape = new(65, 15.Degrees());

        public CastShadow(AID aid, Angle startingAngle) : base(ActionID.MakeSpell(aid))
        {
            _startingAngle = startingAngle;
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            //for (int i = 0; i < 6; ++i)
            //    yield return (_shape, module.PrimaryActor.Position, _startingAngle + i * 60.Degrees(), Activation);
            yield break;
        }
    }

    class CastShadow1 : CastShadow
    {
        public CastShadow1() : base(AID.CastShadowAOE1, -15.Degrees()) { }
        //public override void Init(BossModule module) { Activation = module.PrimaryActor.CastInfo!.FinishAt.AddSeconds(0.5f); }
    }

    class CastShadow2 : CastShadow
    {
        public CastShadow2() : base(AID.CastShadowAOE2, 15.Degrees()) { }
        //public override void Init(BossModule module) { Activation = module.WorldState.CurrentTime.AddSeconds(2); }
    }
}
