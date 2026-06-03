namespace BossMod.Dawntrail.Ultimate.UMAD;

class UMADStates : StateMachineBuilder
{
    public UMADStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1);
    }

    private void P1(uint id)
    {
        P1RevoltingRuin(id, 10.2f);
        P1FireBlizzard1(id + 0x10000, 7.8f);
        P1WaveCannon(id + 0x20000, 3.4f);
        P1JudgmentBuster(id + 0x30000, 4.1f);
        P1Gravitas1(id + 0x40000, 7.2f);
        P1Gravitas2(id + 0x50000, 4.6f);

        Timeout(id + 0xFF0000, 10000, "???");
    }

    void P1RevoltingRuin(uint id, float delay)
    {
        CastStart(id, AID._Ability_RevoltingRuinIII, delay)
            .ActivateOnEnter<P1RevoltingRuinIIIFirst>()
            .ActivateOnEnter<P1RevoltingRuinIIISecond>();

        ComponentCondition<P1RevoltingRuinIIIFirst>(id + 0x10, 5, r => r.NumCasts > 0, "Tankbuster (1st aggro)");
        ComponentCondition<P1RevoltingRuinIIISecond>(id + 0x20, 3.4f, r => r.NumCasts > 0, "Tankbuster (2nd aggro)")
            .DeactivateOnExit<P1RevoltingRuinIIIFirst>()
            .DeactivateOnExit<P1RevoltingRuinIIISecond>();
    }

    void P1FireBlizzard1(uint id, float delay)
    {
        Cast(id, AID._Ability_GravenImage, delay, 3)
            .ActivateOnEnter<P1PulseWave>()
            .ActivateOnEnter<P1BlizzardIIIBlowout>()
            .ActivateOnEnter<P1FlagrantFireIII>()
            .ExecOnEnter<P1FlagrantFireIII>(f => f.EnableHints = false)
            .ExecOnEnter<P1BlizzardIIIBlowout>(b => b.Risky = false);

        CastStart(id + 0x10, AID._Ability_MysteryMagic, 3.2f);

        ComponentCondition<P1PulseWave>(id + 0x100, 2.5f, p => p.NumCasts > 0, "Knockback")
            .DeactivateOnExit<P1PulseWave>()
            .ExecOnExit<P1FlagrantFireIII>(f => f.EnableHints = true)
            .ExecOnExit<P1BlizzardIIIBlowout>(b => b.Risky = true);

        ComponentCondition<P1BlizzardIIIBlowout>(id + 0x110, 2.3f, p => p.NumCasts > 0, "Quadrants")
            .DeactivateOnExit<P1BlizzardIIIBlowout>();

        ComponentCondition<P1FlagrantFireIII>(id + 0x120, 0.9f, p => !p.Active, "Stack/spread")
            .DeactivateOnExit<P1FlagrantFireIII>();
    }

    void P1WaveCannon(uint id, float delay)
    {
        CastStart(id, AID._Ability_DoubleTroubleTrap, delay)
            .ActivateOnEnter<P1WaveCannon>();

        ComponentCondition<P1WaveCannon>(id + 0x10, 0.8f, p => p.NumCasts > 0, "Lasers")
            .ActivateOnEnter<P1Explosion>()
            .DeactivateOnExit<P1WaveCannon>();

        CastEnd(id + 0x20, 2.1f);

        ComponentCondition<P1Explosion>(id + 0x100, 1.5f, e => e.NumCasts > 0, "Towers")
            .ActivateOnEnter<P1DoubleTroubleTrap>()
            .DeactivateOnExit<P1Explosion>()
            .ExecOnExit<P1DoubleTroubleTrap>(t => t.EnableHints = true);

        CastStart(id + 0x110, AID._Ability_MysteryMagic, 2.7f)
            .ActivateOnEnter<P1DoubleTroubleTrapKB>()
            .ActivateOnEnter<P1BlizzardIIIBlowout>()
            .ActivateOnEnter<P1ThrummingThunderIII>()
            .ExecOnEnter<P1BlizzardIIIBlowout>(b => b.Risky = false)
            .ExecOnEnter<P1ThrummingThunderIII>(t => t.Risky = false);

        ComponentCondition<P1DoubleTroubleTrap>(id + 0x200, 1, e => e.NumCasts > 0, "Confetti 1")
            .DeactivateOnExit<P1DoubleTroubleTrap>()
            .DeactivateOnExit<P1DoubleTroubleTrapKB>()
            .ExecOnExit<P1BlizzardIIIBlowout>(b => b.Risky = true)
            .ExecOnExit<P1ThrummingThunderIII>(t => t.Risky = true);

        ComponentCondition<P1BlizzardIIIBlowout>(id + 0x210, 3.9f, b => b.NumCasts > 0, "AOEs")
            .DeactivateOnExit<P1BlizzardIIIBlowout>()
            .DeactivateOnExit<P1ThrummingThunderIII>();
    }

    void P1JudgmentBuster(uint id, float delay)
    {
        Cast(id, AID._Ability_LightOfJudgment, delay, 5, "Raidwide")
            .ActivateOnEnter<P1LightOfJudgment>()
            .ActivateOnEnter<P1Hyperdrive>()
            .ExecOnEnter<P1Hyperdrive>(h => h.EnableHints = false)
            .DeactivateOnExit<P1LightOfJudgment>();

        ComponentCondition<P1Hyperdrive>(id + 0x10, 3.1f, h => h.NumCasts > 0, "Tankbuster 1")
            .ExecOnEnter<P1Hyperdrive>(h => h.EnableHints = true);
        ComponentCondition<P1Hyperdrive>(id + 0x20, 4.3f, h => h.NumCasts > 2, "Tankbuster 3")
            .DeactivateOnExit<P1Hyperdrive>();
    }

    void P1Gravitas1(uint id, float delay)
    {
        Cast(id, AID._Ability_GravenImage, delay, 3);

        CastStart(id + 0x10, AID._Ability_BlizzardIIIBlowout3, 2.1f)
            .ActivateOnEnter<P1BlizzardIIIBlowout>()
            .ActivateOnEnter<P1GravitasVitrophyre>()
            .ActivateOnEnter<P1GravitasPuddle>();

        ComponentCondition<P1BlizzardIIIBlowout>(id + 0x20, 5.1f, b => b.NumCasts > 0, "Quadrants")
            .DeactivateOnExit<P1BlizzardIIIBlowout>();
        ComponentCondition<P1GravitasVitrophyre>(id + 0x30, 0.1f, v => v.Stacks.Count == 0, "Stacks + puddles appear");
        ComponentCondition<P1GravitasVitrophyre>(id + 0x40, 4, g => g.Spreads.Count == 0, "Spreads")
            .ActivateOnEnter<P1IntemperateWill>()
            .DeactivateOnExit<P1GravitasVitrophyre>();

        P1RevoltingRuin(id + 0x100, 0.8f);

        ComponentCondition<P1IntemperateWill>(id + 0x200, 0.8f, p => p.NumCasts > 0, "Left/right")
            .DeactivateOnExit<P1IntemperateWill>();
    }

    void P1Gravitas2(uint id, float delay)
    {
        ComponentCondition<P1GravitasVitrophyre>(id, 0, g => g.Stacks.Count > 0)
            .ActivateOnEnter<P1GravitasVitrophyre>()
            .ExecOnEnter<P1GravitasVitrophyre>(p => p.SetNegativeOffset(1.9f));

        ComponentCondition<P1GravitasVitrophyre>(id + 0x10, delay, g => g.Stacks.Count == 0, "Stacks");

        ComponentCondition<P1GravitasVitrophyre>(id + 0x20, 4, g => g.Spreads.Count == 0, "Spreads")
            .ActivateOnEnter<P1IntemperateWill>()
            .DeactivateOnExit<P1GravitasVitrophyre>();
    }
}
