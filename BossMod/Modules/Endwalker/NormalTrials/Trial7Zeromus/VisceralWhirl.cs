using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.NormalTrials.Trial7Zeromus
{
    // note: apparently there's a slight overlap between aoes in the center, which looks ugly, but at least that's the truth...
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
                case AID.VisceralWhirlRAOE1:
                case AID.VisceralWhirlLAOE1:
                    _aoes.Add(new(_shapeNormal, caster.Position, spell.Rotation, spell.NPCFinishAt));
                    break;
                case AID.VisceralWhirlRAOE2:
                    _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, spell.NPCFinishAt));
                    break;
                case AID.VisceralWhirlLAOE2:
                    _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, spell.NPCFinishAt));
                    break;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.VisceralWhirlRAOE1 or AID.VisceralWhirlRAOE2 or AID.VisceralWhirlLAOE1 or AID.VisceralWhirlLAOE2)
                _aoes.RemoveAll(a => a.Rotation.AlmostEqual(spell.Rotation, 0.05f));
        }
    }

    class VoidBio : Components.GenericAOEs
    {
        private IReadOnlyList<Actor> _bubbles = ActorEnumeration.EmptyList;

        private static AOEShapeCircle _shape = new(2); // TODO: verify explosion radius

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _bubbles.Where(actor => !actor.IsDead).Select(b => new AOEInstance(_shape, b.Position));

        public override void Init(BossModule module)
        {
            _bubbles = module.Enemies(OID.ToxicBubble);
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
