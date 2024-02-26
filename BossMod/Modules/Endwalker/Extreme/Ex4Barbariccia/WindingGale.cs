using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    // TODO: not sure how 'spiral arms' are really implemented
    class WindingGale : Components.GenericAOEs
    {
        private List<Actor> _casters = new();

        private static AOEShapeDonutSector _shape = new(9, 11, 90.Degrees());

        public WindingGale() : base(ActionID.MakeSpell(AID.WindingGale)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var c in _casters)
                yield return new(_shape, c.Position + _shape.OuterRadius * c.Rotation.ToDirection(), c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt);
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
