namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

class RM11STheTyrantStates : StateMachineBuilder
{
    private readonly RM11STheTyrantConfig _config = Service.Config.Get<RM11STheTyrantConfig>();

    public RM11STheTyrantStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Weapons1(id, 5.2f);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    void Weapons1(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_CrownOfArcadia, delay, 5, "Raidwide")
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();

        CastMulti(id + 0x10, [AID.RawSteelTrophyAxe, AID.RawSteelTrophyScythe], 5.2f, 2)
            .ActivateOnEnter<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>()
            .ActivateOnEnter<RawSteelTrophyCounter>();

        ComponentCondition<RawSteelTrophyCounter>(id + 0x12, 7.5f, c => c.NumCasts > 0, "Tankbuster + stack/spread").DeactivateOnExit<RawSteelTrophyAxe>()
            .DeactivateOnExit<RawSteelTrophyScythe>()
            .DeactivateOnExit<RawSteelTrophyCounter>();

        Cast(id + 0x100, AID._Weaponskill_TrophyWeapons, 10.5f, 3)
            .ActivateOnEnter<TrophyWeaponsAOE>()
            .ActivateOnEnter<TrophyWeaponsBait>()
            .ActivateOnEnter<TrophyWeaponsHints>(_config.WeaponHints);

        Cast(id + 0x110, AID._Weaponskill_AssaultEvolved, 3.4f, 6);
        ComponentCondition<TrophyWeaponsAOE>(id + 0x112, 2.2f, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<TrophyWeaponsAOE>(id + 0x113, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<TrophyWeaponsAOE>(id + 0x114, 5.2f, t => t.NumCasts > 2, "Weapon 3")
            .DeactivateOnExit<TrophyWeaponsAOE>()
            .DeactivateOnExit<TrophyWeaponsBait>()
            .DeactivateOnExit<TrophyWeaponsHints>(_config.WeaponHints);

        Cast(id + 0x200, AID._Weaponskill_TrophyWeapons, 9.3f, 3);
    }
}
