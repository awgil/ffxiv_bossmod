namespace BossMod.Endwalker.Savage.P6SHegemone;

class Transmission(BossModule module) : Components.CastCounter(module, AID.ReekHavoc)
{
    private readonly DateTime[] _infectionExpire = new DateTime[PartyState.MaxPartySize]; // when status expires, it will be replaced with stun - we show aoes for last few seconds only
    private BitMask _snakeInfection; // hits front
    private BitMask _wingInfection; // hits back
    private BitMask _stuns;
    private BitMatrix _clips; // row = player, col = others that he clips
    private BitMask _clippedByOthers;

    private static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    public bool StunsActive => _stuns.Any();

    public override void Update()
    {
        _clips.Reset();
        _clippedByOthers.Reset();
        foreach (var e in ActiveAOEs())
        {
            _clippedByOthers |= _clips[e.slot] = Raid.WithSlot().Exclude(e.player).InShape(_shape, e.player.Position, e.direction).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_snakeInfection[slot])
            hints.Add("Face away from raid", _clips[slot].Any());
        if (_wingInfection[slot])
            hints.Add("Face raid", _clips[slot].Any());
        if (_clippedByOthers[slot] && ExpireImminent(slot))
            hints.Add("Avoid transmission aoe!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _clips[playerSlot, pcSlot] ? PlayerPriority.Danger : _clippedByOthers[playerSlot] ? PlayerPriority.Interesting : PlayerPriority.Normal;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var e in ActiveAOEs())
            if (e.slot == pcSlot || ExpireImminent(e.slot))
                _shape.Draw(Arena, e.player.Position, e.direction);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch ((TetherID)tether.ID)
        {
            case TetherID.TransmissionSnake:
                _snakeInfection.Set(Raid.FindSlot(source.InstanceID));
                break;
            case TetherID.TransmissionWing:
                _wingInfection.Set(Raid.FindSlot(source.InstanceID));
                break;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Glossomorph:
            case SID.Chelomorph:
                if (Raid.TryFindSlot(actor, out var slot))
                    _infectionExpire[slot] = status.ExpireAt;
                break;
            case SID.OutOfControlSnake:
            case SID.OutOfControlWing:
                _stuns.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.OutOfControlSnake:
            case SID.OutOfControlWing:
                _stuns.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    private IEnumerable<(int slot, Actor player, Angle direction)> ActiveAOEs()
    {
        foreach (var (slot, player) in Raid.WithSlot(true))
        {
            if (_snakeInfection[slot])
                yield return (slot, player, player.Rotation);
            if (_wingInfection[slot])
                yield return (slot, player, player.Rotation + 180.Degrees());
        }
    }

    private bool ExpireImminent(int slot)
    {
        var expire = _infectionExpire[slot];
        return expire != default && (expire - WorldState.CurrentTime).TotalSeconds < 2;
    }
}
