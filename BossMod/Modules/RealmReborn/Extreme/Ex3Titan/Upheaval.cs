using System;

namespace BossMod.RealmReborn.Extreme.Ex3Titan
{
    // TODO: most of what's here should be handled by KnockbackFromCaster component...
    class Upheaval : Components.KnockbackFromPoints
    {
        private DateTime _remainInPosition;

        public Upheaval() : base(13, ActionID.MakeSpell(AID.Upheaval)) { }

        public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            if (_remainInPosition > module.WorldState.CurrentTime)
            {
                // stack just behind boss, this is a good place to bait imminent landslide correctly
                var dirToCenter = (module.Bounds.Center - module.PrimaryActor.Position).Normalized();
                var pos = module.PrimaryActor.Position + 2 * dirToCenter;
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(pos, 1.5f), _remainInPosition);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _remainInPosition = spell.FinishAt.AddSeconds(1); // TODO: just wait for effectresult instead...
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _remainInPosition = module.WorldState.CurrentTime.AddSeconds(1); // TODO: just wait for effectresult instead...
        }
    }
}
