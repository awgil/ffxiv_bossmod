using System;
using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.Endwalker.Alliance.A35Eulogia

{   
    class Hieroglyphika : Components.GenericAOEs
    {
        private AOEShapeRect _shape = new(6, 6, 6);
        private List<AOEInstance> _aoes = new();
        
        public Hieroglyphika() : base(ActionID.MakeSpell(AID.HieroglyphikaAOE)) { }
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(15);
        
        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.HieroglyphikaAOE)
                _aoes.Add(new(_shape, caster.Position, caster.Rotation, spell.NPCFinishAt));
        }
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HieroglyphikaAOE)
            {
                ++NumCasts;
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
            }
        }
    }
}
