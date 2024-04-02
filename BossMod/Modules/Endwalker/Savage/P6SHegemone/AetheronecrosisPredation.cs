namespace BossMod.Endwalker.Savage.P6SHegemone;

// TODO: improve...
class AetheronecrosisPredation : BossComponent
{
    public int NumCastsAetheronecrosis { get; private set; }
    public int NumCastsDualPredation { get; private set; }
    private int[] _orders = new int[PartyState.MaxPartySize];
    private BitMask _vulnSnake;
    private BitMask _vulnWing;

    public bool Active => (_vulnSnake | _vulnWing).Any();

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_orders[slot] > 0)
            hints.Add($"Order: {_orders[slot]}, side: {(_vulnSnake[slot] ? "wing" : _vulnWing[slot] ? "snake" : "???")}", false);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.GlossalResistanceDown:
                _vulnSnake.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.ChelicResistanceDown:
                _vulnWing.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.Aetheronecrosis:
                var slot = module.Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    _orders[slot] = (status.ExpireAt - module.WorldState.CurrentTime).TotalSeconds switch
                    {
                        < 10 => 2,
                        < 14 => 3,
                        < 18 => 4,
                        _ => 1,
                    };
                }
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.GlossalResistanceDown:
                _vulnSnake.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.ChelicResistanceDown:
                _vulnWing.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Aetheronecrosis:
                ++NumCastsAetheronecrosis;
                break;
            case AID.GlossalPredation:
                ++NumCastsDualPredation;
                break;
        }
    }
}
