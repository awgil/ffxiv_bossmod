using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.NormalTrials.Trial7Zeromus
{

    class VisceralWhirl : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shapeNormal = new(29, 14);
        private static AOEShapeRect _shapeOffset = new(60, 14);

        public bool Active => _aoes.Count > 0;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.VisceralWhirlAOE1:
                    _aoes.Add(new(_shapeNormal, caster.Position, spell.Rotation, spell.NPCFinishAt));
                    break;
                case AID.VisceralWhirlAOE2:
                    _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, spell.NPCFinishAt));
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.VisceralWhirlAOE1 or AID.VisceralWhirlAOE2)
                _aoes.RemoveAll(a => a.Rotation.AlmostEqual(spell.Rotation, 0.05f));
        }
    }

    class DarkDivides : Components.UniformStackSpread
    {
        public DarkDivides() : base(0, 5) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.DivisiveDark)
                AddSpread(actor, status.ExpireAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.DarkDivides)
                Spreads.Clear();
        }
    }
}
