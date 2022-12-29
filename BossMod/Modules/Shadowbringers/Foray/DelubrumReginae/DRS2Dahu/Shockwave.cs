using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2Dahu
{
    class Shockwave : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();
        private static AOEShapeCone _shape = new(20, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.LeftSidedShockwaveFirst or AID.RightSidedShockwaveFirst)
            {
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.FinishAt));
                _aoes.Add(new(_shape, caster.Position, spell.Rotation + 180.Degrees(), spell.FinishAt.AddSeconds(2.6f)));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.LeftSidedShockwaveFirst or AID.RightSidedShockwaveFirst or AID.LeftSidedShockwaveSecond or AID.RightSidedShockwaveSecond && _aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}
