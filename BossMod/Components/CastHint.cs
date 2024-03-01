using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    public enum AID : uint
    {
        SpittingSardine = 11423, //blue mage skill
        BombToss = 11396, //blue mage skill
        Faze = 11403, //blue mage skill
        TheRamsVoice = 11419, //blue mage skill
        StickyTongue = 11412, //blue mage skill
        PerpetualRay = 18314, //blue mage skill
        Tatamigaeshi = 23266, //blue mage skill
        WhiteDeath = 23268, //blue mage skill
        TranquilizerL = 12884, //logos action in Eureka
    };

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

    public class CastInterruptHint : CastCounter
    {
        public uint EnemyOID;
        public bool Canbeinterrupted;
        public bool Canbestunned;
        private List<Actor> _casters = new();
        public IReadOnlyList<Actor> Casters => _casters;
        public bool Active => _casters.Count > 0;

        public CastInterruptHint(ActionID action, uint enemyOID, bool canbeinterrupted = true, bool canbestunned = false) : base(action)
        {
            EnemyOID = enemyOID;
            Canbeinterrupted = canbeinterrupted;
            Canbestunned = canbestunned;
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (Active && Canbeinterrupted && !Canbestunned)
                hints.Add($"Interrupt {module.Enemies(EnemyOID).FirstOrDefault()!.Name}!");
            if (Active && !Canbeinterrupted && Canbestunned)
                hints.Add($"Stun {module.Enemies(EnemyOID).FirstOrDefault()!.Name}!");
            if (Active && Canbeinterrupted && Canbestunned)
                hints.Add($"Interrupt or stun {module.Enemies(EnemyOID).FirstOrDefault()!.Name}!");
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
            foreach (var caster in _casters)
            {
                if (Active && Canbeinterrupted && actor.Role == Role.Tank && Service.ClientState.LocalPlayer?.Level >= 18)
                    hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Interject), caster, 1, false));
                if (Active && Canbeinterrupted && actor.Class.GetClassCategory() == ClassCategory.PhysRanged && Service.ClientState.LocalPlayer?.Level >= 24)
                    hints.PlannedActions.Add((ActionID.MakeSpell(BRD.AID.HeadGraze), caster, 1, false));
                if (Active && Canbestunned && actor.Class is Class.GLA or Class.PLD && Service.ClientState.LocalPlayer?.Level >= 10)
                    hints.PlannedActions.Add((ActionID.MakeSpell(PLD.AID.ShieldBash), caster, 1, false));
                if (Active && Canbestunned && actor.Role == Role.Tank && Service.ClientState.LocalPlayer?.Level >= 12)
                    hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.LowBlow), caster, 1, false));
                if (Active && Canbestunned && actor.Class.GetClassCategory() == ClassCategory.Melee && Service.ClientState.LocalPlayer?.Level >= 10)
                    hints.PlannedActions.Add((ActionID.MakeSpell(SAM.AID.LegSweep), caster, 1, false));
                if (Active && Canbestunned && actor.Class == Class.WHM && Service.ClientState.LocalPlayer?.Level >= 45 && Service.ClientState.LocalPlayer?.Level <= 82)
                    hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Holy1), caster, 1, false));
                if (Active && Canbestunned && actor.Class == Class.WHM && Service.ClientState.LocalPlayer?.Level >= 82)
                    hints.PlannedActions.Add((ActionID.MakeSpell(WHM.AID.Holy3), caster, 1, false));
                if (Active && Canbeinterrupted && actor.Class == Class.BLU)
                    hints.PlannedActions.Add((ActionID.MakeSpell(AID.SpittingSardine), caster, 1, false));
                if (Active && Canbestunned && actor.Class == Class.BLU)
                {
                    hints.PlannedActions.Add((ActionID.MakeSpell(AID.BombToss), caster, 1, false));
                    hints.PlannedActions.Add((ActionID.MakeSpell(AID.Faze), caster, 1, false));
                    hints.PlannedActions.Add((ActionID.MakeSpell(AID.StickyTongue), caster, 1, false));
                    hints.PlannedActions.Add((ActionID.MakeSpell(AID.PerpetualRay), caster, 1, false));
                    hints.PlannedActions.Add((ActionID.MakeSpell(AID.Tatamigaeshi), caster, 1, false));
                    hints.PlannedActions.Add((ActionID.MakeSpell(AID.TheRamsVoice), caster, 1, false)); //actually a deep freeze, but similar effect
                    hints.PlannedActions.Add((ActionID.MakeSpell(AID.WhiteDeath), caster, 1, false)); //actually a deep freeze, but similar effect
                }
            }
        }
    }
}
