using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C03AAI.C031Ketuduke
{
    class StrewnBubbles : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shape = new(20, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(4);

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.BubbleStrewer)
            {
                _aoes.Add(new(_shape, actor.Position, actor.Rotation, module.WorldState.CurrentTime.AddSeconds(10.7f)));
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NSphereShatter or AID.SSphereShatter)
            {
                var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
                if (count != 1)
                    module.ReportError(this, $"{spell.Action} removed {count} aoes");
                ++NumCasts;
            }
        }
    }

    class RecedingEncroachingTwintides : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCircle _shapeOut = new(14);
        private static AOEShapeDonut _shapeIn = new(8, 60);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Skip(NumCasts).Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NRecedingTwintides:
                case AID.SRecedingTwintides:
                    _aoes.Add(new(_shapeOut, caster.Position, default, spell.NPCFinishAt));
                    _aoes.Add(new(_shapeIn, caster.Position, default, spell.NPCFinishAt.AddSeconds(3.1f)));
                    break;
                case AID.NEncroachingTwintides:
                case AID.SEncroachingTwintides:
                    _aoes.Add(new(_shapeIn, caster.Position, default, spell.NPCFinishAt));
                    _aoes.Add(new(_shapeOut, caster.Position, default, spell.NPCFinishAt.AddSeconds(3.1f)));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NRecedingTwintides or AID.NEncroachingTwintides or AID.NFarTide or AID.NNearTide or AID.SRecedingTwintides or AID.SEncroachingTwintides or AID.SFarTide or AID.SNearTide)
                ++NumCasts;
        }
    }
}
