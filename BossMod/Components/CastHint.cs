using System.Collections.Generic;
using System.Linq;
using BossMod.MaskedCarnivale.Stage01;

namespace BossMod.Components
{
    // generic component that is 'active' when any actor casts specific spell
    public class CastHint : CastCounter
    {
        public string Hint;
        public bool ShowCastTimeLeft; // if true, show cast time left until next instance
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public bool Active => _casters.Count > 0;

        public CastHint(ActionID action, string hint, bool showCastTimeLeft = false) : base(action)
        {
            Hint = hint;
            ShowCastTimeLeft = showCastTimeLeft;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Active && Hint.Length > 0)
                hints.Add(ShowCastTimeLeft ? $"{Hint} {((Casters.First().CastInfo?.NPCFinishAt ?? module.WorldState.CurrentTime) - module.WorldState.CurrentTime).TotalSeconds:f1}s left" : Hint);
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

    public class CastInterruptHint : CastHint
    {
        public bool Canbeinterrupted;
        public bool Canbestunned;
        private List<Actor> _casters = new();
        public new IReadOnlyList<Actor> Casters => _casters;
        public new bool Active => _casters.Count > 0;

        public CastInterruptHint(ActionID aid, bool canbeinterrupted = true, bool canbestunned = false, string hint = "") : base(aid, hint)
        {
            Canbeinterrupted = canbeinterrupted;
            Canbestunned = canbestunned;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            foreach (var caster in _casters)
            {
                if (Active && Canbeinterrupted && !Canbestunned)
                    hints.Add($"Interrupt {caster.Name}! " + Hint);
                if (Active && !Canbeinterrupted && Canbestunned)
                    hints.Add($"Stun {caster.Name}! " + Hint);
                if (Active && Canbeinterrupted && Canbestunned)
                    hints.Add($"Interrupt or stun {caster.Name}! " + Hint);
            }
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

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.AddAIHints(module, slot, actor, assignment, hints);
            if (Active)
                foreach (var caster in _casters)
                    foreach (var e in hints.PotentialTargets)
                    {
                        if (Canbeinterrupted)
                        {
                            e.Priority = 1;
                            e.ShouldBeInterrupted = true;
                        }
                        if (Canbestunned)
                        {
                            e.Priority = 1;
                            e.ShouldBeInterrupted = true;
                        }
                    }
        }
    }
}
