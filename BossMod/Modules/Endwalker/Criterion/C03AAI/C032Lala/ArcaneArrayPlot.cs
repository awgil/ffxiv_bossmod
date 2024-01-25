using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala
{
    class ArcaneArrayPlot : Components.GenericAOEs
    {
        public List<AOEInstance> AOEs = new();
        public List<WPos> SafeZoneCenters = new();

        public static AOEShapeRect Shape = new(4, 4, 4);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => AOEs;

        public override void Init(BossModule module)
        {
            for (int z = -16; z <= 16; z += 8)
                for (int x = -16; x <= 16; x += 8)
                    SafeZoneCenters.Add(module.Bounds.Center + new WDir(x, z));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NBrightPulseFirst or AID.NBrightPulseRest or AID.SBrightPulseFirst or AID.SBrightPulseRest)
                ++NumCasts;
        }

        public void AddAOE(WPos pos, DateTime activation)
        {
            AOEs.Add(new(Shape, pos, default, activation));
            SafeZoneCenters.RemoveAll(c => Shape.Check(c, pos, default));
        }

        protected void Advance(ref WPos pos, ref DateTime activation, WDir offset)
        {
            AddAOE(pos, activation);
            activation = activation.AddSeconds(1.2f);
            pos += offset;
        }
    }

    class ArcaneArray : ArcaneArrayPlot
    {
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.ArrowBright)
            {
                var activation = module.WorldState.CurrentTime.AddSeconds(4.6f);
                var pos = actor.Position;
                var offset = 8 * actor.Rotation.ToDirection();
                for (int i = 0; i < 5; ++i)
                {
                    Advance(ref pos, ref activation, offset);
                }
                pos -= offset;
                pos += module.Bounds.Contains(pos + offset.OrthoL()) ? offset.OrthoL() : offset.OrthoR();
                for (int i = 0; i < 5; ++i)
                {
                    Advance(ref pos, ref activation, -offset);
                }

                if (AOEs.Count > 10)
                    AOEs.SortBy(aoe => aoe.Activation);
            }
        }
    }

    class ArcanePlot : ArcaneArrayPlot
    {
        public override void OnActorCreated(BossModule module, Actor actor)
        {
            switch ((OID)actor.OID)
            {
                case OID.ArrowBright:
                    AddLine(module, actor, module.WorldState.CurrentTime.AddSeconds(4.6f), false);
                    break;
                case OID.ArrowDim:
                    AddLine(module, actor, module.WorldState.CurrentTime.AddSeconds(8.2f), true);
                    break;
            }
        }

        private void AddLine(BossModule module, Actor actor, DateTime activation, bool preAdvance)
        {
            var pos = actor.Position;
            var offset = 8 * actor.Rotation.ToDirection();
            if (preAdvance)
                pos += offset;

            do
            {
                Advance(ref pos, ref activation, offset);
            }
            while (module.Bounds.Contains(pos));

            AOEs.SortBy(aoe => aoe.Activation);
        }
    }
}
