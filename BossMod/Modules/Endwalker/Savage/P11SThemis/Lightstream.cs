using System.Collections.Generic;

namespace BossMod.Endwalker.Savage.P11SThemis
{
    class Lightstream : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shape = new(50, 5);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.LightstreamAOEFirst or AID.LightstreamAOERest)
            {
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                ++NumCasts;
            }
        }

        public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
        {
            var rotation = (IconID)iconID switch
            {
                IconID.RotateCW => -10.Degrees(),
                IconID.RotateCCW => 10.Degrees(),
                _ => default
            };
            if (rotation != default)
            {
                for (int i = 0; i < 7; ++i)
                    _aoes.Add(new(_shape, actor.Position, actor.Rotation + i * rotation, module.WorldState.CurrentTime.AddSeconds(8 + i * 1.1f)));
                _aoes.SortBy(aoe => aoe.Activation);
            }
        }
    }
}
