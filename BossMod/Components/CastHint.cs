using System.Collections.Generic;

namespace BossMod.Components
{
    // generic component that is 'active' when any actor casts specific spell
    public class CastHint : CastCounter
    {
        public string Hint;
        private List<Actor> _casters = new();

        public bool Active => _casters.Count > 0;

        public CastHint(ActionID action, string hint) : base(action)
        {
            Hint = hint;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Active)
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
}
