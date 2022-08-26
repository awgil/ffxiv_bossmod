using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un2Sephirot
{
    class P3FiendishWail : Components.CastCounter
    {
        private BitMask _physResistMask;
        private List<Actor> _towers = new();

        public bool Active => _towers.Count > 0;

        private static float _radius = 5;

        public P3FiendishWail() : base(ActionID.MakeSpell(AID.FiendishWailAOE)) { }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (!Active)
                return;

            bool wantToSoak = _physResistMask.Any() ? _physResistMask[slot] : actor.Role == Role.Tank;
            bool soaking = _towers.InRadius(actor.Position, _radius).Any();
            if (wantToSoak)
                hints.Add("Soak the tower!", !soaking);
            else
                hints.Add("GTFO from tower!", soaking);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var t in _towers)
                arena.AddCircle(t.Position, _radius, ArenaColor.Danger);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.ForceAgainstMight)
                _physResistMask.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _towers.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _towers.Remove(caster);
        }
    }
}
