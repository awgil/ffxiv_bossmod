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
        UnsealAutos(id + 0x10100, 8, count: 3);
        Daggers1(id + 0x20000, 1.7f);
        ForkedFury(id + 0x30000, 4.9f);
        UnsealAutos(id + 0x30100, 6, count: 2);
        Conduit(id + 0x40000, 1.7f);
        SagesStaff(id + 0x50000, 14.9f);
        RuneAxe(id + 0x60000, 8.3f);
        Conduit(id + 0x70000, 1.7f);
        Daggers1(id + 0x80000, 4.2f);
        HolyLance(id + 0x90000, 4);
        Enrage(id + 0xA0000, 4);

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
        switch (weapon)
        {
            case 0:
                CastMulti(id, [AID.UnsealAxe, AID.UnsealLance], delay, 5, "Select weapon");
                break;
            case 1:
                Cast(id, AID.UnsealAxe, delay, 5, "Select axe");
                break;
            case 2:
                Cast(id, AID.UnsealLance, delay, 5, "Select lance");
                break;
        }
    }

    private void UnsealAutos(uint id, float delay, int count)
    {
        var lastState = ComponentCondition<UnsealAutos>(id + 0x10, delay, a => a.NumCasts > 0, "Autos 1", maxOverdue: 1000)
            .ActivateOnEnter<UnsealAutos>()
            .ExecOnEnter<UnsealAutos>(u => u.Predict(delay));
        for (var i = 1u; i < count; i++)
        {
            var cnt = 6 * i;
            lastState = ComponentCondition<UnsealAutos>(id + 0x10 + i, 3.2f, a => a.NumCasts > cnt, $"Autos {i + 1}", maxOverdue: 1000);
        }
        lastState.DeactivateOnExit<UnsealAutos>();
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

        CastStartMulti(id + 0x20, [AID.CriticalAxeblowCast, AID.CriticalLanceblowCast], 4);

        ComponentCondition<AssassinsDagger>(id + 0x30, 4.1f, d => d.NumCasts >= 36, "Daggers end")
            .DeactivateOnExit<AssassinsDagger>();

        ComponentCondition<CriticalCounter>(id + 0x40, 2.1f, c => c.NumCasts > 1, "Axe/lance 2")
            .DeactivateOnExit<CriticalCounter>()
            .DeactivateOnExit<CriticalAxeblow>()
            .DeactivateOnExit<CriticalLanceblow>();
    }

    private void ForkedFury(uint id, float delay)
    {
        Cast(id, AID._Ability_ForkedFury, delay, 4.5f).ActivateOnEnter<ForkedFury>();
        ComponentCondition<ForkedFury>(id + 0x10, 0.6f, t => t.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<ForkedFury>();
    }

    private void Conduit(uint id, float delay)
    {
        CastStartMulti(id, [AID._Ability_AuraBurst, AID._Ability_Holy], delay, "Conduits appear")
            .ActivateOnEnter<Conduit>()
            .ActivateOnEnter<ArcaneReaction>()
            .ActivateOnEnter<ArcaneRecoil>();

        ComponentCondition<Conduit>(id + 0x100, 19.9f, c => c.NumCasts > 0, "Raidwide + conduit enrage")
            .DeactivateOnExit<Conduit>()
            .DeactivateOnExit<ArcaneReaction>()
            .DeactivateOnExit<ArcaneRecoil>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SagesStaff(uint id, float delay)
    {
        Cast(id, AID._Ability_SagesStaff, delay, 5)
            .ActivateOnEnter<SagesStaff>();

        CastStartMulti(id + 0x10, [AID.CriticalAxeblowCast, AID.CriticalLanceblowCast], 3.4f)
            .ActivateOnEnter<CriticalCounter>()
            .ActivateOnEnter<CriticalAxeblow>()
            .ActivateOnEnter<CriticalLanceblow>();

        ComponentCondition<CriticalCounter>(id + 0x12, 6.1f, c => c.NumCasts > 0, "Axe/lance 1")
            .ExecOnExit<SagesStaff>(s => s.Enabled = true);

        ComponentCondition<SagesStaff>(id + 0x14, 4.9f, s => s.NumCasts > 0, "Line stacks 1")
            .ExecOnExit<SagesStaff>(s => s.Enabled = false);

        CastEnd(id + 0x20, 3.1f); // second Sage's Staff cast starts before line stacks resolve

        CastStartMulti(id + 0x30, [AID.CriticalAxeblowCast, AID.CriticalLanceblowCast], 3.2f);

        ComponentCondition<CriticalCounter>(id + 0x31, 6.3f, c => c.NumCasts > 1, "Axe/lance 2")
            .DeactivateOnExit<CriticalCounter>()
            .DeactivateOnExit<CriticalAxeblow>()
            .DeactivateOnExit<CriticalLanceblow>()
            .ExecOnExit<SagesStaff>(s => s.Enabled = true);

        ComponentCondition<SagesStaff>(id + 0x40, 5, s => s.NumCasts > 3, "Line stacks 2")
            .DeactivateOnExit<SagesStaff>();

        UnsealedAura(id + 0x100, 0);
    }

    private void RuneAxe(uint id, float delay)
    {
        Unseal(id, delay, 1);

        UnsealAutos(id + 0x10, 6, 3);

        Cast(id + 0x100, AID._Ability_RuneAxe, 1.8f, 5)
            .ActivateOnEnter<RuneAxe>();

        ComponentCondition<RuneAxe>(id + 0x110, 20, r => r.ActiveSpreads.Any(), "Spreads appear");

        ComponentCondition<RuneAxe>(id + 0x111, 9.2f, r => r.NumCasts > 0, "Spread 1");

        CastStartMulti(id + 0x120, [AID.CriticalAxeblowCast, AID.CriticalLanceblowCast], 0.1f)
            .ActivateOnEnter<CriticalCounter>()
            .ActivateOnEnter<CriticalAxeblow>()
            .ActivateOnEnter<CriticalLanceblow>()
            .ExecOnEnter<CriticalAxeblow>(c => c.Risky = false)
            .ExecOnEnter<CriticalLanceblow>(c => c.Risky = false);

        ComponentCondition<RuneAxe>(id + 0x121, 3.8f, r => r.NumCasts > 1, "Spreads 2")
            .ExecOnExit<RuneAxe>(r => r.Enabled = false)
            .ExecOnExit<CriticalLanceblow>(c => c.Risky = true)
            .ExecOnExit<CriticalAxeblow>(c => c.Risky = true);

        ComponentCondition<CriticalCounter>(id + 0x130, 2.5f, c => c.NumCasts > 0, "Axe/lance + platforms")
            .ExecOnExit<RuneAxe>(r => r.Enabled = true)
            .DeactivateOnExit<CriticalLanceblow>()
            .DeactivateOnExit<CriticalAxeblow>()
            .DeactivateOnExit<CriticalCounter>();

        ComponentCondition<RuneAxe>(id + 0x140, 5.5f, r => r.NumCasts > 4, "Spreads 3")
            .DeactivateOnExit<RuneAxe>();

        ForkedFury(id + 0x200, 7.3f);

        ComponentCondition<UnsealAutos>(id + 0x300, 6, u => u.NumCasts > 0, "Autos 1")
            .ActivateOnEnter<UnsealAutos>();
        ComponentCondition<UnsealAutos>(id + 0x301, 3.2f, u => u.NumCasts > 6, "Autos 2")
            .DeactivateOnExit<UnsealAutos>();
    }

    private void HolyLance(uint id, float delay)
    {
        Unseal(id, delay, 2);
        UnsealAutos(id + 0x10, 6.3f, 3);

        Cast(id + 0x100, AID._Ability_HolyLance, 1.6f, 5)
            .ActivateOnEnter<HolyLance>()
            .ActivateOnEnter<HolyIV>();

        CastStartMulti(id + 0x120, [AID.CriticalAxeblowCast, AID.CriticalLanceblowCast], 4.3f)
            .ActivateOnEnter<CriticalCounter>()
            .ActivateOnEnter<CriticalAxeblow>()
            .ActivateOnEnter<CriticalLanceblow>()
            .ActivateOnEnter<PreyLancepoint>();

        ComponentCondition<CriticalCounter>(id + 0x121, 6.3f, c => c.NumCasts > 0, "Axe/lance 1")
            .ExecOnExit<HolyLance>(l => l.Enabled = true);

        ComponentCondition<HolyLance>(id + 0x130, 3.6f, h => h.NumCasts > 0, "Outside 1");
        ComponentCondition<HolyLance>(id + 0x131, 2, h => h.NumCasts > 1, "Platform 1 start");
        ComponentCondition<HolyIV>(id + 0x132, 2, h => h.NumFinishedStacks > 0, "Stacks 1");
        ComponentCondition<HolyLance>(id + 0x133, 2, h => h.NumCasts > 3, "Platform 1 end");
        ComponentCondition<HolyLance>(id + 0x134, 2, h => h.NumCasts > 4, "Outside 2");
        ComponentCondition<HolyLance>(id + 0x135, 2, h => h.NumCasts > 5, "Platform 2 start");
        ComponentCondition<HolyIV>(id + 0x136, 2, h => h.NumFinishedStacks > 3, "Stacks 2");
        ComponentCondition<HolyLance>(id + 0x137, 2, h => h.NumCasts > 7, "Platform 2 end");
        ComponentCondition<HolyLance>(id + 0x138, 2, h => h.NumCasts > 8, "Outside 3");
        ComponentCondition<HolyLance>(id + 0x139, 2, h => h.NumCasts > 9, "Platform 3 start");
        ComponentCondition<HolyIV>(id + 0x13A, 2, h => h.NumFinishedStacks > 6, "Stacks 3")
            .DeactivateOnExit<HolyIV>()
            .DeactivateOnExit<PreyLancepoint>();
        ComponentCondition<HolyLance>(id + 0x13B, 2, h => h.NumCasts > 11, "Lances end")
            .DeactivateOnExit<HolyLance>();

        ComponentCondition<CriticalCounter>(id + 0x150, 3, c => c.NumCasts > 1, "Axe/lance 2")
            .DeactivateOnExit<CriticalCounter>()
            .DeactivateOnExit<CriticalAxeblow>()
            .DeactivateOnExit<CriticalLanceblow>();
    }

    private void Enrage(uint id, float delay)
    {
        UnsealedAura(id, delay);
        ForkedFury(id + 0x100, 4.2f);
        UnsealAutos(id + 0x200, 6.3f, 2);
        Conduit(id + 0x300, 20);
    }
}
