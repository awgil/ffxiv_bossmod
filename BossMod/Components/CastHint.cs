using System.Collections.Generic;
using System.Linq;

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
        public bool CanBeInterrupted;
        public bool CanBeStunned;
        public bool ShowNameInHint;
        public string HintExtra;
        private List<Actor> _casters = new();
        public new IReadOnlyList<Actor> Casters => _casters;
        public new bool Active => _casters.Count > 0;

        public CastInterruptHint(ActionID aid, bool canBeInterrupted = true, bool canBeStunned = false, string hintExtra = "", bool showNameInHint = false) : base(aid, hintExtra)
        {
            CanBeInterrupted = canBeInterrupted;
            CanBeStunned = canBeStunned;
            ShowNameInHint = showNameInHint;
            HintExtra = hintExtra;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
                if (Active && CanBeInterrupted && !CanBeStunned)
                {
                    if (ShowNameInHint && Casters.Count > 0 && HintExtra.Length == 0)
                    hints.Add($"Interrupt {Casters[0].Name}!");
                    if (!ShowNameInHint && Casters.Count > 0 && HintExtra.Length == 0)
                    hints.Add($"Interrupt!");
                    if (ShowNameInHint && Casters.Count > 0 && HintExtra.Length > 0)
                    hints.Add($"Interrupt {Casters[0].Name}!" + $" {HintExtra}");
                    if (!ShowNameInHint && Casters.Count > 0 && HintExtra.Length > 0)
                    hints.Add($"Interrupt!" + $" {HintExtra}");
                }
                if (Active && !CanBeInterrupted && CanBeStunned)
                {
                    if (ShowNameInHint && Casters.Count > 0 && HintExtra.Length == 0)
                    hints.Add($"Stun {Casters[0].Name}!");
                    if (!ShowNameInHint && Casters.Count > 0 && HintExtra.Length == 0)
                    hints.Add($"Stun!");
                    if (ShowNameInHint && Casters.Count > 0 && HintExtra.Length > 0)
                    hints.Add($"Stun {Casters[0].Name}!" + $" {HintExtra}");
                    if (!ShowNameInHint && Casters.Count > 0 && HintExtra.Length > 0)
                    hints.Add($"Stun!" + $" {HintExtra}");
                }
                if (Active && CanBeInterrupted && CanBeStunned)
                {
                    if (ShowNameInHint && Casters.Count > 0 && HintExtra.Length == 0)
                    hints.Add($"Interrupt/stun {Casters[0].Name}!");
                    if (!ShowNameInHint && Casters.Count > 0 && HintExtra.Length == 0)
                    hints.Add($"Interrupt/stun!");
                    if (ShowNameInHint && Casters.Count > 0 && HintExtra.Length > 0)
                    hints.Add($"Interrupt/stun {Casters[0].Name}!" + $" {HintExtra}");
                    if (!ShowNameInHint && Casters.Count > 0 && HintExtra.Length > 0)
                    hints.Add($"Interrupt/stun!" + $" {HintExtra}");
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
            foreach (var c in Casters)
            {
                var e = hints.PotentialTargets.Find(e => e.Actor == c);
                if (e != null)
                {
                    e.ShouldBeInterrupted |= CanBeInterrupted;
                    e.ShouldBeStunned |= CanBeStunned;
                }
            }
        }
    }
}
