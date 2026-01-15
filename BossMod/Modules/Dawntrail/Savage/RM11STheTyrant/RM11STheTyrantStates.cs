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
        TrophyWeapons(id, 5.2f);
        DanceOfDomination(id + 0x10000, 10.7f);
        UltimateTrophyWeapons(id + 0x20000, 10.3f);
        Meteorain(id + 0x30000, 7.2f);
        Flatliner(id + 0x40000, 9.2f);

        SimpleState(id + 0xFF0000, 10000, "???");
    }

    void TrophyWeapons(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_CrownOfArcadia, delay, 5, "Raidwide")
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();

        RawSteelTrophy(id + 0x10, 5.2f);

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

        Cast(id + 0x200, AID._Weaponskill_TrophyWeapons, 9.2f, 3)
            .ActivateOnEnter<TrophyWeaponsAOE>()
            .ActivateOnEnter<TrophyWeaponsBait>()
            .ActivateOnEnter<TrophyWeaponsHints>(_config.WeaponHints)
            .ExecOnEnter<TrophyWeaponsAOE>(t => t.Risky = false)
            .ExecOnEnter<TrophyWeaponsBait>(t => t.EnableHints = false);

        Cast(id + 0x210, AID._Spell_VoidStardust, 3.1f, 4, "Puddles start")
            .ActivateOnEnter<VoidStardustBait>()
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<Comets>();

        ComponentCondition<Comets>(id + 0x212, 10.1f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Stack/spread")
            .DeactivateOnExit<VoidStardustBait>()
            .DeactivateOnExit<Cometite>()
            .DeactivateOnExit<Comets>()
            .ExecOnExit<TrophyWeaponsAOE>(t => t.Risky = true)
            .ExecOnExit<TrophyWeaponsBait>(t => t.EnableHints = true);

        ComponentCondition<TrophyWeaponsAOE>(id + 0x220, 5.1f, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<TrophyWeaponsAOE>(id + 0x221, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<TrophyWeaponsAOE>(id + 0x222, 5.2f, t => t.NumCasts > 2, "Weapon 3")
            .DeactivateOnExit<TrophyWeaponsAOE>()
            .DeactivateOnExit<TrophyWeaponsBait>()
            .DeactivateOnExit<TrophyWeaponsHints>(_config.WeaponHints);

        ComponentCondition<Comets>(id + 0x230, 11.2f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Stack/spread")
            .ActivateOnEnter<VoidStardustBait>()
            .ActivateOnEnter<Cometite>()
            .ActivateOnEnter<Comets>()
            .DeactivateOnExit<VoidStardustBait>()
            .DeactivateOnExit<Cometite>()
            .DeactivateOnExit<Comets>();

        Cast(id + 0x240, AID._Weaponskill_CrownOfArcadia, 1, 5, "Raidwide")
            .ActivateOnEnter<CrownOfArcadia>()
            .DeactivateOnExit<CrownOfArcadia>();
    }

    void RawSteelTrophy(uint id, float delay)
    {
        CastMulti(id, [AID.RawSteelTrophyAxe, AID.RawSteelTrophyScythe], delay, 2)
            .ActivateOnEnter<RawSteelTrophyAxe>()
            .ActivateOnEnter<RawSteelTrophyScythe>()
            .ActivateOnEnter<RawSteelTrophyCounter>();

        ComponentCondition<RawSteelTrophyCounter>(id + 2, 7.5f, c => c.NumCasts > 0, "Tankbuster + stack/spread").DeactivateOnExit<RawSteelTrophyAxe>()
            .DeactivateOnExit<RawSteelTrophyScythe>()
            .DeactivateOnExit<RawSteelTrophyCounter>();
    }

    void DanceOfDomination(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_DanceOfDominationTrophy, delay, 2)
            .ActivateOnEnter<DanceOfDomination1>()
            .ActivateOnEnter<DanceOfDomination2>()
            .ActivateOnEnter<HurricaneExplosion>()
            .ActivateOnEnter<EyeOfTheHurricane>();

        ComponentCondition<DanceOfDomination1>(id + 0x10, 6.6f, d => d.NumCasts > 0, "Raidwide 1")
            .DeactivateOnExit<DanceOfDomination1>();
        ComponentCondition<DanceOfDomination2>(id + 0x11, 4.2f, d => d.NumCasts > 0, "Raidwide 7")
            .DeactivateOnExit<DanceOfDomination2>();

        ComponentCondition<EyeOfTheHurricane>(id + 0x20, 4.9f, e => e.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<EyeOfTheHurricane>()
            .DeactivateOnExit<HurricaneExplosion>();

        RawSteelTrophy(id + 0x100, 4.2f);
    }

    void UltimateTrophyWeapons(uint id, float delay)
    {
        Cast(id, AID._Spell_Charybdistopia, delay, 5, "1 HP");
        Cast(id + 0x10, AID._Weaponskill_UltimateTrophyWeapons, 2.6f, 3)
            .ActivateOnEnter<Maelstrom>()
            .ActivateOnEnter<UltimateTrophyWeaponsAOE>()
            .ActivateOnEnter<UltimateTrophyWeaponsBait>()
            .ActivateOnEnter<UltimateTrophyWeaponsHints>(_config.WeaponHints);

        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x20, 10, t => t.NumCasts > 0, "Weapon 1");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x21, 5.2f, t => t.NumCasts > 1, "Weapon 2");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x22, 5.2f, t => t.NumCasts > 2, "Weapon 3");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x23, 5.2f, t => t.NumCasts > 3, "Weapon 4");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x24, 5.2f, t => t.NumCasts > 4, "Weapon 5");
        ComponentCondition<UltimateTrophyWeaponsAOE>(id + 0x25, 5.2f, t => t.NumCasts > 5, "Weapon 6")
            .DeactivateOnExit<UltimateTrophyWeaponsAOE>()
            .DeactivateOnExit<UltimateTrophyWeaponsBait>()
            .DeactivateOnExit<UltimateTrophyWeaponsHints>(_config.WeaponHints);

        ComponentCondition<PowerfulGust>(id + 0x30, 6.3f, p => p.NumCasts > 0, "Tornado baits")
            .ActivateOnEnter<PowerfulGust>()
            .DeactivateOnExit<PowerfulGust>();

        CastEnd(id + 0x40, 1).ActivateOnEnter<OneAndOnly>();
        Cast(id + 0x100, AID._Weaponskill_OneAndOnly1, 4.1f, 6);
        ComponentCondition<OneAndOnly>(id + 0x102, 3, o => o.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Maelstrom>()
            .DeactivateOnExit<OneAndOnly>();
    }

    void Meteorain(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_GreatWallOfFire, delay, 5)
            .ActivateOnEnter<Firewall1>()
            .ActivateOnEnter<Firewall2>()
            .ActivateOnEnter<FirewallExplosion>();

        ComponentCondition<Firewall1>(id + 0x10, 0.3f, f => f.NumCasts > 0, "Line buster 1")
            .DeactivateOnExit<Firewall1>();
        ComponentCondition<Firewall2>(id + 0x11, 3.2f, f => f.NumCasts > 1, "Line buster 2")
            .DeactivateOnExit<Firewall2>();

        Cast(id + 0x20, AID._Spell_OrbitalOmen, 5.1f, 5)
            .ActivateOnEnter<OrbitalOmen>()
            .ActivateOnEnter<FireAndFury>()
            .DeactivateOnExit<FirewallExplosion>();

        ComponentCondition<OrbitalOmen>(id + 0x22, 9.1f, o => o.NumCasts == 2, "Orbital start");
        ComponentCondition<FireAndFury>(id + 0x23, 0.6f, f => f.NumCasts > 0, "Sides safe")
            .DeactivateOnExit<FireAndFury>();
        ComponentCondition<OrbitalOmen>(id + 0x24, 3.9f, o => o.NumCasts == 8, "Orbital end")
            .DeactivateOnExit<OrbitalOmen>();

        Cast(id + 0x30, AID._Spell_Meteorain, 7.5f, 5)
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<FearsomeFireball1>();

        Cast(id + 0x40, AID._Weaponskill_FearsomeFireball, 2.7f, 5);
        ComponentCondition<FearsomeFireball1>(id + 0x42, 0.4f, f => f.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<FearsomeFireball1>();
        ComponentCondition<CosmicKiss>(id + 0x43, 1.2f, c => c.NumCasts > 0, "Meteors 1")
            .ActivateOnEnter<ForegoneFatality>()
            .ActivateOnEnter<FearsomeFireball2>()
            .ActivateOnEnter<CometExplosion>();
        ComponentCondition<ForegoneFatality>(id + 0x44, 7.9f, f => f.NumCasts > 0, "Tethers");
        ComponentCondition<FearsomeFireball2>(id + 0x45, 0.8f, f => f.NumCasts > 0, "Wild charge");

        ComponentCondition<CosmicKiss>(id + 0x46, 1.3f, c => c.NumCasts > 2, "Meteors 2");
        ComponentCondition<ForegoneFatality>(id + 0x47, 7.9f, f => f.NumCasts > 2, "Tethers");
        ComponentCondition<FearsomeFireball2>(id + 0x48, 0.8f, f => f.NumCasts > 1, "Wild charge");

        ComponentCondition<CosmicKiss>(id + 0x49, 1.3f, c => c.NumCasts > 4, "Meteors 3");
        ComponentCondition<ForegoneFatality>(id + 0x4A, 7.9f, f => f.NumCasts > 4, "Tethers");
        ComponentCondition<FearsomeFireball2>(id + 0x4B, 0.8f, f => f.NumCasts > 2, "Wild charge")
            .DeactivateOnExit<CosmicKiss>()
            .DeactivateOnExit<ForegoneFatality>()
            .DeactivateOnExit<FearsomeFireball2>();

        Cast(id + 0x100, AID._Weaponskill_TripleTyrannhilation1, 5.6f, 7)
            .ActivateOnEnter<TripleTyrannhilation>()
            .ActivateOnEnter<ShockwaveCounter>();

        ComponentCondition<ShockwaveCounter>(id + 0x102, 1.1f, c => c.NumCasts > 0, "LoS 1");
        ComponentCondition<ShockwaveCounter>(id + 0x103, 3.2f, c => c.NumCasts > 2, "LoS 3")
            .DeactivateOnExit<TripleTyrannhilation>()
            .DeactivateOnExit<ShockwaveCounter>()
            .DeactivateOnExit<Comet>()
            .DeactivateOnExit<CometExplosion>();
    }

    void Flatliner(uint id, float delay)
    {
        // flatliner knockback is 15 units
        // tower knockback is 23 units
        Cast(id, AID._Weaponskill_Flatliner, delay, 4)
            .ActivateOnEnter<Flatliner>()
            .ActivateOnEnter<FlatlinerArena>();

        ComponentCondition<Flatliner>(id + 0x10, 2, f => f.NumCasts > 0, "Arena split")
            .DeactivateOnExit<Flatliner>()
            .DeactivateOnExit<FlatlinerArena>();

        Cast(id + 0x100, AID._Spell_MajesticMeteor, 9.2f, 5)
            .ActivateOnEnter<ExplosionKnockback>()
            .ActivateOnEnter<ExplosionTower>()
            .ActivateOnEnter<MeteowrathTether>();
    }
}
