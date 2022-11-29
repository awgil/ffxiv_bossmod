using System;
using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    // TODO: revise for awakened charge...
    class P2CrimsonCyclone : Components.GenericAOEs
    {
        private List<(Actor caster, DateTime activation)> _predicted = new();
        private List<Actor> _casters = new();

        private static AOEShapeRect _shape = new(49, 9, 5);

        public bool CastsPredicted => _predicted.Count > 0;

        public P2CrimsonCyclone() : base(ActionID.MakeSpell(AID.CrimsonCyclone)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var p in _predicted)
                yield return (_shape, p.caster.Position, p.caster.Rotation, p.activation);
            foreach (var c in _casters)
                yield return (_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if ((OID)actor.OID == OID.Ifrit && id == 0x1E43)
                _predicted.Add((actor, module.WorldState.CurrentTime.AddSeconds(5.2f)));
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
