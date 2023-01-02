using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7Queen
{
    // TODO: this is essentialy copy-paste of DRS3 component, generalize?.. the only different thing is AIDs
    class WindsOfWeight : Components.GenericAOEs
    {
        private List<Actor> _green = new();
        private List<Actor> _purple = new();
        private BitMask _invertedPlayers;

        private static AOEShapeCircle _shape = new(20);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return (_invertedPlayers[slot] ? _purple : _green).Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt));
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.ReversalOfForces)
                _invertedPlayers.Set(module.Raid.FindSlot(actor.InstanceID));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            CasterList(spell)?.Add(caster);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            CasterList(spell)?.Remove(caster);
        }

        private List<Actor>? CasterList(ActorCastInfo spell) => (AID)spell.Action.ID switch
        {
            AID.WindsOfFate => _green,
            AID.WeightOfFortune => _purple,
            _ => null
        };
    }
}
