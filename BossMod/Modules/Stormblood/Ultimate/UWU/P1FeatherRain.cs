using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UWU
{
    // predict puddles under all players until actual casts start
    class P1FeatherRain : Components.GenericAOEs
    {
        private List<WPos> _predicted = new();
        private List<Actor> _casters = new();
        private DateTime _activation;

        private static AOEShapeCircle _shape = new(3);

        public bool CastsPredicted => _predicted.Count > 0;
        public bool CastsActive => _casters.Count > 0;

        public P1FeatherRain() : base(ActionID.MakeSpell(AID.FeatherRain), "GTFO from puddle!") { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var p in _predicted)
                yield return new(_shape, p, new(), _activation);
            foreach (var c in _casters)
                yield return new(_shape, c.CastInfo!.LocXZ, new(), c.CastInfo!.FinishAt);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (id == 0x1E3A && (OID)actor.OID is OID.Garuda or OID.GarudaSister)
            {
                _predicted.Clear();
                _predicted.AddRange(module.Raid.WithoutSlot().Select(p => p.Position));
                _activation = module.WorldState.CurrentTime.AddSeconds(2.5f);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _predicted.Clear();
                _casters.Add(caster);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Remove(caster);
            }
        }
    }
}
