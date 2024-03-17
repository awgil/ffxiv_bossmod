using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3QueensGuard
{
    class GreatBallOfFire : Components.GenericAOEs
    {
        private IReadOnlyList<Actor> _smallFlames = ActorEnumeration.EmptyList;
        private IReadOnlyList<Actor> _bigFlames = ActorEnumeration.EmptyList;
        private DateTime _activation;

        private static AOEShapeCircle _shapeSmall = new(10);
        private static AOEShapeCircle _shapeBig = new(18);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var f in _smallFlames)
                yield return new(_shapeSmall, f.Position, new(), f.CastInfo?.NPCFinishAt ?? _activation);
            foreach (var f in _bigFlames)
                yield return new(_shapeBig, f.Position, new(), f.CastInfo?.NPCFinishAt ?? _activation);
        }

        public override void Init(BossModule module)
        {
            _smallFlames = module.Enemies(OID.RagingFlame);
            _bigFlames = module.Enemies(OID.ImmolatingFlame);
            _activation = module.WorldState.CurrentTime.AddSeconds(6.6f); // TODO: this depends on when do we want to start showing hints...
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.BurnSmall or AID.BurnBig)
                ++NumCasts;
        }
    }
}
