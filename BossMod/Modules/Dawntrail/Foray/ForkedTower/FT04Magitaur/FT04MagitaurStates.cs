namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class FT04MagitaurStates : StateMachineBuilder
{
    public FT04MagitaurStates(BossModule module) : base(module)
    {
        DeathPhase(0, Phase1);
    }

    private void Phase1(uint id)
    {
        UnsealedAura(id, 10.1f);
        Unseal(id + 0x10000, 10.2f);
        Daggers1(id + 0x20000, 1.7f);
        ForkedFury(id + 0x30000, 6.1f);
        Conduit(id + 0x40000, 11);

        Timeout(id + 0xFF0000, 9999, "???")
            .ActivateOnEnter<AssassinsDagger>();
    }

    private void UnsealedAura(uint id, float delay)
    {
        CastStart(id, AID.UnsealedAuraCast, delay)
            .ActivateOnEnter<UnsealedAura>();

        ComponentCondition<UnsealedAura>(id + 1, 5, u => u.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<UnsealedAura>();
    }

    private void Unseal(uint id, float delay, int weapon = 0)
    {
        var autosState = weapon switch
        {
            0 => CastMulti(id, [AID.UnsealAxe, AID.UnsealLance], delay, 5, "Select weapon"),
            1 => Cast(id, AID.UnsealAxe, delay, 5, "Select axe"),
            2 => Cast(id, AID.UnsealLance, delay, 5, "Select lance"),
            _ => throw new ArgumentException($"invalid value for {nameof(weapon)}")
        };

        autosState.ActivateOnEnter<UnsealAutos>();

        ComponentCondition<UnsealAutos>(id + 0x10, 8, a => a.NumCasts > 0, "Autos 1");
        ComponentCondition<UnsealAutos>(id + 0x11, 3.2f, a => a.NumCasts > 6, "Autos 2");
        ComponentCondition<UnsealAutos>(id + 0x12, 3.2f, a => a.NumCasts > 12, "Autos 3")
            .DeactivateOnExit<UnsealAutos>();
    }

    private void Daggers1(uint id, float delay)
    {
        CastStart(id, AID._Ability_AssassinsDagger, delay)
            .ActivateOnEnter<AssassinsDagger>();

        ComponentCondition<AssassinsDagger>(id + 1, 5, d => d.NumCasts >= 3, "Daggers start")
            .ActivateOnEnter<CriticalCounter>()
            .ActivateOnEnter<CriticalAxeblow>()
            .ActivateOnEnter<CriticalLanceblow>();

        CastMulti(id + 0x10, [AID.CriticalAxeblowCast, AID.CriticalLanceblowCast], 7.3f, 5);
        ComponentCondition<CriticalCounter>(id + 0x12, 1.3f, c => c.NumCasts > 0, "Axe/lance 1");

        CastStartMulti(id + 0x20, [AID.CriticalAxeblowCast, AID.CriticalLanceblowCast], 5.3f);

        ComponentCondition<AssassinsDagger>(id + 0x30, 4.1f, d => d.NumCasts >= 36, "Daggers end")
            .DeactivateOnExit<AssassinsDagger>();

        ComponentCondition<CriticalCounter>(id + 0x40, 2.1f, c => c.NumCasts > 1, "Axe/lance 2");
    }

    private void ForkedFury(uint id, float delay)
    {
        Cast(id, AID._Ability_ForkedFury, delay, 4.5f)
            .ActivateOnEnter<ForkedFury>();
        ComponentCondition<ForkedFury>(id + 0x10, 0.6f, t => t.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<ForkedFury>();
    }

    private void Conduit(uint id, float delay)
    {
        CastStartMulti(id, [AID._Ability_AuraBurst, AID._Ability_Holy], delay, "Conduits appear")
            .ActivateOnEnter<Conduit>()
            .ActivateOnEnter<ArcaneReaction>()
            .ActivateOnEnter<ArcaneRecoil>();

        ComponentCondition<ArcaneReaction>(id + 0x10, 8, r => r.NumCasts > 0, "Hysteria bait")
            .DeactivateOnExit<ArcaneReaction>();

        ComponentCondition<Conduit>(id + 0x100, 11.9f, c => c.NumCasts > 0, "Raidwide + conduit enrage")
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
