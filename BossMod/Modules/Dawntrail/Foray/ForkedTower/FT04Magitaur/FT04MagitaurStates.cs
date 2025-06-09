namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class FT04MagitaurStates : StateMachineBuilder
{
    public FT04MagitaurStates(BossModule module) : base(module)
    {
        DeathPhase(0, Phase1);
    }

    private void Phase1(uint id)
    {
        CastStart(id, AID._Ability_UnsealedAura, 10.1f)
            .ActivateOnEnter<UnsealedAura>();

        ComponentCondition<UnsealedAura>(id + 1, 5, u => u.NumCasts > 0, "Raidwide");

        Cast(id + 0x10, AID._Ability_Unseal, 10.2f, 5, "Select weapon");

        CastStart(id + 0x100, AID._Ability_AssassinsDagger, 16)
            .ActivateOnEnter<AssassinsDagger>();

        ComponentCondition<AssassinsDagger>(id + 0x200, 5, d => d.NumCasts >= 3, "Daggers start")
            .ActivateOnEnter<CriticalAxeblow>()
            .ActivateOnEnter<CriticalAxeblowFloor>()
            .ActivateOnEnter<CriticalLanceblowFloor>()
            .ActivateOnEnter<CriticalLanceblow>();

        CastMulti(id + 0x300, [AID._Ability_CriticalAxeblow, AID._Ability_CriticalLanceblow], 7.3f, 5, "Axe/lance 1");

        CastStartMulti(id + 0x400, [AID._Ability_CriticalAxeblow, AID._Ability_CriticalLanceblow], 5.3f);

        ComponentCondition<AssassinsDagger>(id + 0x40A, 4.1f, d => d.NumCasts >= 36, "Daggers end")
            .DeactivateOnExit<AssassinsDagger>();

        CastEnd(id + 0x410, 1, "Axe/lance 2");

        Timeout(id + 0xFF0000, 9999, "???");
    }
}
