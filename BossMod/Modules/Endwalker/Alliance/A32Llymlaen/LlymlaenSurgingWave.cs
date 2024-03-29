using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A32Llymlaen
{
       class SphereShatter : Components.GenericAOEs
    {
        private IReadOnlyList<Actor> _bubbles = ActorEnumeration.EmptyList;

        private static AOEShapeCircle _shape = new(1.5f); // TODO: verify explosion radius

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _bubbles.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));

        public override void Init(BossModule module)
        {
            _bubbles = module.Enemies(OID.SeaFoam);
        }
    }
        class SurgingWaveKnockback : KnockbackFromCastTarget
    {
        public SurgingWaveKnockback() : base(ActionID.MakeSpell(AID.SurgingWaveKnockback), 40f, kind: Kind.AwayFromOrigin) { }
    }
}
