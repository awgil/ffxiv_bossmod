using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3QueensGuard
{
    class AboveBoard : Components.GenericAOEs
    {
        public enum State { Initial, ThrowUpDone, ShortExplosionsDone, LongExplosionsDone }

        public State CurState { get; private set; }
        private List<Actor> _smallBombs = new();
        private List<Actor> _bigBombs = new();
        private bool _invertedBombs; // bombs are always either all normal (big=short) or all inverted
        private BitMask _invertedPlayers; // default for player is 'long', short is considered inverted (has visible status)
        private DateTime _activation;

        private static AOEShapeCircle _shape = new(10);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            var imminentBombs = AreBigBombsDangerous(slot) ? _bigBombs : _smallBombs;
            return imminentBombs.Select(b => new AOEInstance(_shape, b.Position, new(), _activation));
        }

        public override void Init(BossModule module)
        {
            _smallBombs = module.Enemies(OID.AetherialBolt);
            _bigBombs = module.Enemies(OID.AetherialBurst);
            _activation = module.WorldState.CurrentTime.AddSeconds(12);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.ReversalOfForces:
                    if ((OID)actor.OID is OID.AetherialBolt or OID.AetherialBurst)
                        _invertedBombs = true;
                    else
                        _invertedPlayers.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.AboveBoardPlayerLong:
                case SID.AboveBoardPlayerShort:
                case SID.AboveBoardBombLong:
                case SID.AboveBoardBombShort:
                    AdvanceState(State.ThrowUpDone);
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.LotsCastBigShort:
                case AID.LotsCastSmallShort:
                    AdvanceState(State.ShortExplosionsDone);
                    break;
                case AID.LotsCastLong:
                    AdvanceState(State.LongExplosionsDone);
                    _activation = module.WorldState.CurrentTime.AddSeconds(4.2f);
                    break;
            }
        }

        private bool AreBigBombsDangerous(int slot)
        {
            if (_invertedPlayers[slot])
            {
                // inverted players fall right before first bomb explosion, so they have to avoid first bombs, then move to avoid second bombs
                var firstSetImminent = CurState < State.ShortExplosionsDone;
                return firstSetImminent != _invertedBombs; // first set is big if inverted
            }
            else
            {
                // normally players fall right before second bomb explosion, so they only avoid second bombs
                // second bombs are normally small, big if inverted
                return _invertedBombs;
            }
        }

        private void AdvanceState(State dest)
        {
            if (CurState < dest)
                CurState = dest;
        }
    }
}
