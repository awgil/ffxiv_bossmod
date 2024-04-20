namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS4QueensGuard;

class AboveBoard(BossModule module) : Components.GenericAOEs(module)
{
    public enum State { Initial, ThrowUpDone, ShortExplosionsDone, LongExplosionsDone }

    public State CurState { get; private set; }
    private readonly IReadOnlyList<Actor> _smallBombs = module.Enemies(OID.AetherialBolt);
    private readonly IReadOnlyList<Actor> _bigBombs = module.Enemies(OID.AetherialBurst);
    private bool _invertedBombs; // bombs are always either all normal (big=short) or all inverted
    private BitMask _invertedPlayers; // default for player is 'long', short is considered inverted (has visible status)
    private DateTime _activation = module.WorldState.FutureTime(12);

    private static readonly AOEShapeCircle _shape = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var imminentBombs = AreBigBombsDangerous(slot) ? _bigBombs : _smallBombs;
        return imminentBombs.Select(b => new AOEInstance(_shape, b.Position, new(), _activation));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.ReversalOfForces:
                if ((OID)actor.OID is OID.AetherialBolt or OID.AetherialBurst)
                    _invertedBombs = true;
                else
                    _invertedPlayers.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.AboveBoardPlayerLong:
            case SID.AboveBoardPlayerShort:
            case SID.AboveBoardBombLong:
            case SID.AboveBoardBombShort:
                AdvanceState(State.ThrowUpDone);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LotsCastBigShort:
            case AID.LotsCastSmallShort:
                AdvanceState(State.ShortExplosionsDone);
                break;
            case AID.LotsCastLong:
                AdvanceState(State.LongExplosionsDone);
                _activation = WorldState.FutureTime(4.2f);
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
