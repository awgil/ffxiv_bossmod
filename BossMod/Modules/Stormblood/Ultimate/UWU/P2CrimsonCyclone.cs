using System;
using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    // TODO: consider predicting charge order for awakened cyclone
    class P2CrimsonCyclone : Components.GenericAOEs
    {
        private List<(WPos pos, Angle rot, DateTime activation)> _predicted = new(); // note: there could be 1/2/4 predicted normal charges and 0 or 2 'cross' charges
        private List<Actor> _casters = new();

        private static AOEShapeRect _shape = new(49, 9, 5);

        public bool CastsPredicted => _predicted.Count > 0;

        public P2CrimsonCyclone() : base(ActionID.MakeSpell(AID.CrimsonCyclone)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_predicted.Count <= 2) // don't draw 4 predicted charges, it is pointless
                foreach (var p in _predicted)
                    yield return (_shape, p.pos, p.rot, p.activation);
            foreach (var c in _casters)
                yield return (_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (NumCasts == 0 && (OID)actor.OID == OID.Ifrit && id == 0x1E43)
                _predicted.Add((actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(5.2f)));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                if (NumCasts == 0)
                    _predicted.Clear();
                _casters.Add(caster);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                _casters.Remove(caster);
                if (caster == ((UWU)module).Ifrit() && caster.FindStatus(SID.Woken) != null)
                {
                    _predicted.Add((module.Bounds.Center - 19.5f * (spell.Rotation + 45.Degrees()).ToDirection(), spell.Rotation + 45.Degrees(), module.WorldState.CurrentTime.AddSeconds(2.2f)));
                    _predicted.Add((module.Bounds.Center - 19.5f * (spell.Rotation - 45.Degrees()).ToDirection(), spell.Rotation - 45.Degrees(), module.WorldState.CurrentTime.AddSeconds(2.2f)));
                }
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if ((AID)spell.Action.ID == AID.CrimsonCycloneCross)
            {
                _predicted.Clear();
            }
        }
    }
}
