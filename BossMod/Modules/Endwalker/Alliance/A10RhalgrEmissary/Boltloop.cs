﻿using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary
{
    class Boltloop : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShape[] _shapes = { new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30) };

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Skip(NumCasts).Take(2);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.BoltloopAOE1 => _shapes[0],
                AID.BoltloopAOE2 => _shapes[1],
                AID.BoltloopAOE3 => _shapes[2],
                _ => null
            };
            if (shape != null)
            {
                _aoes.Add(new(shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
                _aoes.SortBy(aoe => aoe.Activation);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.BoltloopAOE1 or AID.BoltloopAOE2 or AID.BoltloopAOE3)
                ++NumCasts;
        }
    }
}
