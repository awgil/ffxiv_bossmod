namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to predatory avarice mechanic
class PredatoryAvarice(BossModule module) : BossComponent(module)
{
    private BitMask _playersWithTides;
    private BitMask _playersWithDepths;
    private BitMask _playersInTides;
    private BitMask _playersInDepths;

    private const float _tidesRadius = 10;
    private const float _depthsRadius = 6;

    public bool Active => (_playersWithTides | _playersWithDepths).Any();

    public override void Update()
    {
        _playersInTides = _playersInDepths = default;
        if (!Active)
            return;

        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            if (_playersWithTides[i])
            {
                _playersInTides |= Raid.WithSlot(false, true, true).InRadiusExcluding(player, _tidesRadius).Mask();
            }
            else if (_playersWithDepths[i])
            {
                _playersInDepths |= Raid.WithSlot(false, true, true).InRadiusExcluding(player, _depthsRadius).Mask();
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        if (_playersWithTides[slot])
        {
            if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _tidesRadius).Any())
            {
                hints.Add("GTFO from raid!");
            }
        }
        else
        {
            if (_playersInTides[slot])
            {
                hints.Add("GTFO from avarice!");
            }

            var warnToStack = _playersWithDepths[slot]
                ? _playersInDepths.NumSetBits() < 6
                : !_playersInDepths[slot];
            if (warnToStack)
            {
                hints.Add("Stack with raid!");
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        var pcHasTides = _playersWithTides[pcSlot];
        var pcHasDepths = _playersWithDepths[pcSlot];
        foreach ((var i, var actor) in Raid.WithSlot(false, true, true))
        {
            if (_playersWithTides[i])
            {
                // tides are always drawn
                Arena.ZoneCircleOutline(actor.Position, _tidesRadius, Colors.Danger);
                Arena.Actor(actor, Colors.Danger);
            }
            else if (_playersWithDepths[i] && !pcHasTides)
            {
                // depths are drawn only if pc has no tides - otherwise it is to be considered a generic player
                Arena.ZoneCircleOutline(actor.Position, _tidesRadius, Colors.Safe);
                Arena.Actor(actor, Colors.Danger);
            }
            else if (pcHasTides || pcHasDepths)
            {
                // other players are only drawn if pc has some debuff
                var playerInteresting = pcHasTides ? _playersInTides[i] : _playersInDepths[i];
                Arena.Actor(actor.Position, actor.Rotation, playerInteresting ? Colors.PlayerInteresting : Colors.PlayerGeneric);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MarkOfTides:
                _playersWithTides.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.MarkOfDepths:
                _playersWithDepths.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MarkOfTides:
                _playersWithTides.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.MarkOfDepths:
                _playersWithDepths.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}
