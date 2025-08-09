namespace BossMod.Dawntrail.Extreme.Ex5Necron;

class BlueShockwave(BossModule module) : Components.TankSwap(module, default(AID), AID._Weaponskill_BlueShockwave1, AID._Weaponskill_BlueShockwave1, 4.1f, new AOEShapeCone(100, 45.Degrees()), false)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == 615)
        {
            _source = actor;
            _prevTarget = targetID;
            _activation = WorldState.FutureTime(7.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if (NumCasts >= 2)
        {
            CurrentBaits.Clear();
            _source = null;
            _activation = default;
        }
    }
}

class Ex5NecronStates : StateMachineBuilder
{
    public Ex5NecronStates(BossModule module) : base(module)
    {
        SimplePhase(0, P1, "P1");
    }

    private void P1(uint id)
    {
        BlueShockwave(id, 14.2f);

        Timeout(id + 0xFF0000, 9999, "???");
    }

    private void BlueShockwave(uint id, float delay)
    {
        ComponentCondition<BlueShockwave>(id, delay, b => b.NumCasts > 0, "Tankbuster 1")
            .ActivateOnEnter<BlueShockwave>();
        ComponentCondition<BlueShockwave>(id + 0x10, 4.1f, b => b.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<BlueShockwave>();
    }
}
