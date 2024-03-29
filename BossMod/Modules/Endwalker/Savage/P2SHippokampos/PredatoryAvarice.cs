namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to predatory avarice mechanic
class PredatoryAvarice : BossComponent
{
    private BitMask _playersWithTides;
    private BitMask _playersWithDepths;
    private BitMask _playersInTides;
    private BitMask _playersInDepths;

    private static readonly float _tidesRadius = 10;
    private static readonly float _depthsRadius = 6;

    public bool Active => (_playersWithTides | _playersWithDepths).Any();

    public override void Update(BossModule module)
    {
        _playersInTides = _playersInDepths = new();
        if (!Active)
            return;

        foreach ((int i, var player) in module.Raid.WithSlot())
        {
            if (_playersWithTides[i])
            {
                _playersInTides |= module.Raid.WithSlot().InRadiusExcluding(player, _tidesRadius).Mask();
            }
            else if (_playersWithDepths[i])
            {
                _playersInDepths |= module.Raid.WithSlot().InRadiusExcluding(player, _depthsRadius).Mask();
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (!Active)
            return;

        if (_playersWithTides[slot])
        {
            if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _tidesRadius).Any())
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

            bool warnToStack = _playersWithDepths[slot]
                ? _playersInDepths.NumSetBits() < 6
                : !_playersInDepths[slot];
            if (warnToStack)
            {
                hints.Add("Stack with raid!");
            }
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (!Active)
            return;

        bool pcHasTides = _playersWithTides[pcSlot];
        bool pcHasDepths = _playersWithDepths[pcSlot];
        foreach ((int i, var actor) in module.Raid.WithSlot())
        {
            if (_playersWithTides[i])
            {
                // tides are always drawn
                arena.AddCircle(actor.Position, _tidesRadius, ArenaColor.Danger);
                arena.Actor(actor, ArenaColor.Danger);
            }
            else if (_playersWithDepths[i] && !pcHasTides)
            {
                // depths are drawn only if pc has no tides - otherwise it is to be considered a generic player
                arena.AddCircle(actor.Position, _tidesRadius, ArenaColor.Safe);
                arena.Actor(actor, ArenaColor.Danger);
            }
            else if (pcHasTides || pcHasDepths)
            {
                // other players are only drawn if pc has some debuff
                bool playerInteresting = pcHasTides ? _playersInTides[i] : _playersInDepths[i];
                arena.Actor(actor.Position, actor.Rotation, playerInteresting ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
            }
        }
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MarkOfTides:
                _playersWithTides.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.MarkOfDepths:
                _playersWithDepths.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MarkOfTides:
                _playersWithTides.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.MarkOfDepths:
                _playersWithDepths.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}
