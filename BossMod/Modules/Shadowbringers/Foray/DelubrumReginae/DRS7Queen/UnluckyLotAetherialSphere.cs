﻿using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7Queen
{
    class UnluckyLotAetherialSphere : Components.GenericAOEs
    {
        private AOEInstance? _aoe;

        public UnluckyLotAetherialSphere() : base(ActionID.MakeSpell(AID.UnluckyLotAetherialSphere)) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.OptimalOffensiveMoveSphere)
                _aoe = new(new AOEShapeCircle(20), caster.Position, activation: spell.NPCFinishAt.AddSeconds(2.6));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.UnluckyLotAetherialSphere)
                _aoe = null;
        }
    }
}
