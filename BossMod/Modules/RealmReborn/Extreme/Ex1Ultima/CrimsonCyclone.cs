using System;
using System.Collections.Generic;

namespace BossMod.RealmReborn.Extreme.Ex1Ultima
{
    class CrimsonCyclone : Components.GenericAOEs
    {
        private Actor? _ifrit; // non-null while mechanic is active
        private DateTime _resolve;

        public bool Active => _ifrit != null;

        private static AOEShapeRect _shape = new(43, 6, 5);

        public CrimsonCyclone() : base(ActionID.MakeSpell(AID.CrimsonCyclone)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_ifrit != null)
                yield return (_shape, _ifrit.Position, _ifrit.Rotation, _resolve);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _ifrit = caster;
                _resolve = spell.FinishAt;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _ifrit = null;
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.UltimaIfrit && id == 0x008D)
            {
                _ifrit = actor;
                _resolve = module.WorldState.CurrentTime.AddSeconds(5);
            }
        }
    }
}
