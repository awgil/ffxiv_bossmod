using System;
using System.Collections.Generic;

namespace BossMod.Components
{
    // generic component that is 'active' when any actor casts specific spell
    public class CastHint : CastCounter
    {
        public string Hint;
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public bool Active => _casters.Count > 0;

        public CastHint(ActionID action, string hint) : base(action)
        {
            Hint = hint;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Active && Hint.Length > 0)
                hints.Add(Hint);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }
    }

    public class EnrageCastHint : CastCounter
    {
        private DateTime enragestart;
        public float Casttime;
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public bool Active => _casters.Count > 0;

        public EnrageCastHint(ActionID action, float casttime) : base(action) 
        {
            Casttime = casttime;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Active)
                hints.Add($"Enrage! {Math.Max(Casttime - (module.WorldState.CurrentTime - enragestart).TotalSeconds, 0.0f):f1}s left.");
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Add(caster);
                enragestart = module.WorldState.CurrentTime;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.Remove(caster);
        }
    }
}
