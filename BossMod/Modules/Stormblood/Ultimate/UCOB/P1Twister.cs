using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P1Twister : Components.GenericAOEs
    {
        private List<WPos> _predicted = new();
        private List<Actor> _twisters = new();
        private DateTime _predictStart = DateTime.MaxValue;

        private static AOEShapeCircle _shape = new(2); // TODO: verify radius
        private static float PredictBeforeCastEnd = 0.7f;

        public IEnumerable<Actor> ActiveTwisters => _twisters.Where(v => v.EventState != 7);
        public bool Active => ActiveTwisters.Count() > 0;

        public P1Twister() : base(default, "GTFO from twister!") { KeepOnPhaseChange = true; }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var p in _predicted)
                yield return new(_shape, p, default, _predictStart.AddSeconds(PredictBeforeCastEnd + 0.3f));
            foreach (var p in ActiveTwisters)
                yield return new(_shape, p.Position);
        }

        public override void Init(BossModule module)
        {
            _twisters = module.Enemies(OID.VoidzoneTwister);
        }

        public override void Update(BossModule module)
        {
            if (_predicted.Count == 0 && _twisters.Count == 0 && module.WorldState.CurrentTime >= _predictStart)
                _predicted.AddRange(module.Raid.WithoutSlot().Select(a => a.Position));
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.VoidzoneTwister)
            {
                _predicted.Clear();
                _predictStart = DateTime.MaxValue;
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.Twister)
                _predictStart = spell.FinishAt.AddSeconds(-PredictBeforeCastEnd);
        }
    }
}
