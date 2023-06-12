using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P10SPandaemonium
{
    class PandaemoniacRay : Components.SelfTargetedAOEs
    {
        public PandaemoniacRay() : base(ActionID.MakeSpell(AID.PandaemoniacRayAOE), new AOEShapeRect(30, 25)) { }
    }

    class JadePassage : Components.GenericAOEs
    {
        private List<Actor> _spheres = new();
        private DateTime _activation;

        private static AOEShapeRect _shape = new(80, 1);

        public JadePassage() : base(ActionID.MakeSpell(AID.JadePassage)) { }

        public override void Init(BossModule module)
        {
            _spheres = module.Enemies(OID.ArcaneSphere);
            _activation = module.WorldState.CurrentTime.AddSeconds(3.6f);
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _spheres.Where(s => !s.IsDead).Select(s => new AOEInstance(_shape, s.Position, s.Rotation, s.CastInfo?.FinishAt ?? _activation));
        }
    }
}
