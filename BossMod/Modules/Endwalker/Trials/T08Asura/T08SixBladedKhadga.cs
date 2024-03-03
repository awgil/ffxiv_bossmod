using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Trials.T08Asura
{
   class SixBladedKhadga : Components.GenericAOEs
    {
        private List<ActorCastInfo> _spell = new();
        private DateTime _start;
        private static readonly AOEShapeCone Cone = new(20, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_spell.Count > 0 && NumCasts == 0)
                yield return new(Cone, module.PrimaryActor.Position, _spell[0].Rotation, _start.AddSeconds(12.9f), ArenaColor.Danger);
            if (_spell.Count > 1 && NumCasts == 0)
                yield return new(Cone, module.PrimaryActor.Position, _spell[1].Rotation, _start.AddSeconds(14.9f));
            if (_spell.Count > 1 && NumCasts == 1)
                yield return new(Cone, module.PrimaryActor.Position, _spell[1].Rotation, _start.AddSeconds(14.9f), ArenaColor.Danger);
            if (_spell.Count > 2 && NumCasts == 1)
                yield return new(Cone, module.PrimaryActor.Position, _spell[2].Rotation, _start.AddSeconds(17));
            if (_spell.Count > 3 && NumCasts == 2)
                yield return new(Cone, module.PrimaryActor.Position, _spell[2].Rotation, _start.AddSeconds(17), ArenaColor.Danger);
            if (_spell.Count > 3 && NumCasts == 2)
                yield return new(Cone, module.PrimaryActor.Position, _spell[3].Rotation, _start.AddSeconds(19));
            if (_spell.Count > 4 && NumCasts == 3)
                yield return new(Cone, module.PrimaryActor.Position, _spell[3].Rotation, _start.AddSeconds(19), ArenaColor.Danger);
            if (_spell.Count > 4 && NumCasts == 3)
                yield return new(Cone, module.PrimaryActor.Position, _spell[4].Rotation, _start.AddSeconds(21.1f));
            if (_spell.Count > 5 && NumCasts == 4)
                yield return new(Cone, module.PrimaryActor.Position, _spell[4].Rotation, _start.AddSeconds(21.1f), ArenaColor.Danger);
            if (_spell.Count > 5 && NumCasts == 4)
                yield return new(Cone, module.PrimaryActor.Position, _spell[5].Rotation, _start.AddSeconds(23.2f));
            if (_spell.Count > 5 && NumCasts == 5)
                yield return new(Cone, module.PrimaryActor.Position, _spell[5].Rotation, _start.AddSeconds(23.2f), ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.KhadgaTelegraph1 or AID.KhadgaTelegraph2 or AID.KhadgaTelegraph3)
            {
                _spell.Add(spell);
                if (_start == default)
                    _start = module.WorldState.CurrentTime;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.Khadga1 or AID.Khadga2 or AID.Khadga3 or AID.Khadga4 or AID.Khadga5 or AID.Khadga6)
            {
                ++NumCasts;
                if (NumCasts == 6)
                {
                    NumCasts = 0;
                    _start = default;
                    _spell.Clear();
                }
            }
        }
    }
}