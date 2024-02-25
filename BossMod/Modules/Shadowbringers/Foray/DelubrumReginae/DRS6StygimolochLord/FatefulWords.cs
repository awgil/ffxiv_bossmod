using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6StygimolochLord
{
    class FatefulWords : Components.Knockback
    {
        private Dictionary<ulong, float> _playerDistances = new();

        public FatefulWords() : base(ActionID.MakeSpell(AID.FatefulWordsAOE), true) { } // TODO: verify it ignores immunities

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            var dist = _playerDistances.GetValueOrDefault(actor.InstanceID);
            if (dist != 0)
                yield return new(module.Bounds.Center, dist, module.PrimaryActor.CastInfo?.NPCFinishAt ?? new());
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            var dist = (SID)status.ID switch
            {
                SID.WanderersFate => 6,
                SID.SacrificesFate => -6,
                _ => 0
            };
            if (dist != 0)
                _playerDistances[actor.InstanceID] = dist;
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID is SID.WanderersFate or SID.SacrificesFate)
                _playerDistances.Remove(actor.InstanceID);
        }
    }
}
