using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex6Golbez
{
    class VoidStardust : Components.GenericAOEs
    {
        private List<(WPos pos, DateTime activation)> _aoes = new();

        private static AOEShapeCircle _shape = new(6);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            foreach (var aoe in _aoes.Skip(NumCasts + 2).Take(10))
                yield return new(_shape, aoe.pos, default, aoe.activation);
            foreach (var aoe in _aoes.Skip(NumCasts).Take(2))
                yield return new(_shape, aoe.pos, default, aoe.activation, ArenaColor.Danger);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.VoidStardustFirst:
                    _aoes.Add((caster.Position, spell.NPCFinishAt));
                    break;
                case AID.VoidStardustRestVisual:
                    _aoes.Add((caster.Position, spell.NPCFinishAt.AddSeconds(2.9f)));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.VoidStardustFirst or AID.VoidStardustRestAOE)
                ++NumCasts;
        }
    }

    class AbyssalQuasar : Components.StackWithCastTargets
    {
        public AbyssalQuasar() : base(ActionID.MakeSpell(AID.AbyssalQuasar), 3, 2) { }
    }
}
