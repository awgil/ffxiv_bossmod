using System;
using System.Collections.Generic;

namespace BossMod.Stormblood.Ultimate.UWU
{
    // crimson cyclone has multiple variations:
    // p2 first cast is a single charge along cardinal
    // p2 second cast is two charges along both cardinals
    // p2 third cast is four staggered charges, with different patterns depending on whether awakening happened (TODO: we can predict that very early)
    // p4 predation is a single awakened charge along intercardinal
    class CrimsonCyclone : Components.GenericAOEs
    {
        private float _predictionDelay;
        private List<(AOEShape shape, WPos pos, Angle rot, DateTime activation)> _predicted = new(); // note: there could be 1/2/4 predicted normal charges and 0 or 2 'cross' charges
        private List<Actor> _casters = new();

        private static AOEShapeRect _shapeMain = new(49, 9, 5);
        private static AOEShapeRect _shapeCross = new(44.5f, 5, 0.5f);

        public bool CastsPredicted => _predicted.Count > 0;

        public CrimsonCyclone(float predictionDelay) : base(ActionID.MakeSpell(AID.CrimsonCyclone))
        {
            _predictionDelay = predictionDelay;
        }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_predicted.Count <= 2) // don't draw 4 predicted charges, it is pointless
                foreach (var p in _predicted)
                    yield return new(p.shape, p.pos, p.rot, p.activation);
            foreach (var c in _casters)
                yield return new(_shapeMain, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt);
        }

        public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
        {
            if (NumCasts == 0 && (OID)actor.OID == OID.Ifrit && id == 0x1E43)
                _predicted.Add((_shapeMain, actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(_predictionDelay)));
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
                    _predicted.Add((_shapeCross, module.Bounds.Center - 19.5f * (spell.Rotation + 45.Degrees()).ToDirection(), spell.Rotation + 45.Degrees(), module.WorldState.CurrentTime.AddSeconds(2.2f)));
                    _predicted.Add((_shapeCross, module.Bounds.Center - 19.5f * (spell.Rotation - 45.Degrees()).ToDirection(), spell.Rotation - 45.Degrees(), module.WorldState.CurrentTime.AddSeconds(2.2f)));
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

    class P2CrimsonCyclone : CrimsonCyclone
    {
        public P2CrimsonCyclone() : base(5.2f) { }
    }

    class P4CrimsonCyclone : CrimsonCyclone
    {
        public P4CrimsonCyclone() : base(8.1f) { }
    }
}
