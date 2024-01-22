using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice
{
    class RingARingOExplosions : Components.GenericAOEs
    {
        public List<Actor> ActiveBombs = new();
        private List<Actor> _bombs = new();
        private DateTime _activation;

        private static AOEShapeCircle _shape = new(12);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => ActiveBombs.Select(b => new AOEInstance(_shape, b.Position, default, _activation));

        public override void Update(BossModule module)
        {
            if (_bombs.Count == 6 && ActiveBombs.Count == 0)
            {
                var glowingBomb = _bombs.FirstOrDefault(b => b.ModelState.AnimState1 == 1);
                if (glowingBomb != null)
                {
                    var cur = glowingBomb;
                    do
                    {
                        ActiveBombs.Add(cur);
                        cur = module.WorldState.Actors.Find(cur.Tether.Target);
                    } while (cur != null && cur != glowingBomb);
                    _activation = module.WorldState.CurrentTime.AddSeconds(17.4f);
                }
            }
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID is OID.NBomb or OID.SBomb)
                _bombs.Add(actor);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NBombBurst or AID.SBombBurst)
            {
                ++NumCasts;
                ActiveBombs.Remove(caster);
            }
        }
    }
}
