using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    class P3Burst : Components.GenericAOEs
    {
        private List<Actor> _bombs = new();
        private DateTime _activation;

        private static AOEShape _shape = new AOEShapeCircle(6.3f);

        public P3Burst() : base(ActionID.MakeSpell(AID.Burst)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return _bombs.Select(b => (_shape, b.Position, b.Rotation, b.CastInfo?.FinishAt ?? _activation));
        }

        public override void Init(BossModule module)
        {
            _bombs = module.Enemies(OID.BombBoulder);
            _activation = module.WorldState.CurrentTime.AddSeconds(6.5f);
        }
    }
}
