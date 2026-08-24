namespace BossMod.Endwalker.Savage.P9SKokytos;

class P9SKokytosStates : StateMachineBuilder
{
    public P9SKokytosStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        GluttonysAugur(id, 6.5f);

        Ravening(id + 0x100000, 8.4f, AID.RaveningMage, false, false);
        DualityOfDeath(id + 0x110000, 10.0f);
        TripleDualspell(id + 0x120000, 5.2f);

        Ravening(id + 0x200000, 16.1f, AID.RaveningMartialist, true, false);
        AscendantFist(id + 0x210000, 7.8f);
        ArchaicRockbreakerCombination(id + 0x220000, 7.4f);
        GluttonysAugur(id + 0x230000, 1.0f);
        AscendantFist(id + 0x240000, 5.8f);

        Ravening(id + 0x300000, 17.3f, AID.RaveningChimeric, true, false);
        LevinstrikeSummoning(id + 0x310000, 2.5f);
        GluttonysAugur(id + 0x320000, 1.0f);

        Ravening(id + 0x400000, 13.6f, AID.RaveningBeast, false, true);
        Charibdys(id + 0x410000, 2.5f);

        Ravening(id + 0x500000, 14.3f, AID.RaveningChimeric, true, false);
        DualityOfDeath(id + 0x510000, 7.9f);
        ArchaicRockbreakerDualspell(id + 0x520000, 1.0f);
        GluttonysAugur(id + 0x530000, 0.3f);
        ChimericSuccession(id + 0x540000, 6.9f);
        Dualspell(id + 0x550000, 3.8f, true);

        Ravening(id + 0x600000, 11.0f, AID.RaveningMage, false, false);
        Dualspell(id + 0x610000, 2.5f);
        GluttonysAugur(id + 0x620000, 0.4f);
        Dualspell(id + 0x630000, 1.6f);
        DualityOfDeath(id + 0x640000, 1.3f);
        Dualspell(id + 0x650000, 1.0f);
        GluttonysAugur(id + 0x660000, 0.3f);

        Ravening(id + 0x700000, 16.8f, AID.RaveningChimeric, false, false);
        Cast(id + 0x710000, AID.Disintegration, 2.5f, 10, "Enrage");
    }

    private void GluttonysAugur(uint id, float delay)
    {
        Cast(id, AID.GluttonysAugur, delay, 5);
        ComponentCondition<GluttonysAugur>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<GluttonysAugur>()
            .DeactivateOnExit<GluttonysAugur>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Ravening(uint id, float delay, AID aid, bool withUplift, bool shortCast)
    {
        Cast(id, aid, delay, shortCast ? 3 : 4)
            .ActivateOnEnter<Uplift>(withUplift);
        ComponentCondition<SoulSurge>(id + 2, shortCast ? 7.8f : 6.6f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<SoulSurge>()
            .DeactivateOnExit<SoulSurge>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void DualityOfDeath(uint id, float delay)
    {
        CastStart(id, AID.DualityOfDeath, delay)
            .ActivateOnEnter<DualityOfDeath>(); // icons appear 0.1s before cast start
        CastEnd(id + 1, 5);
        ComponentCondition<DualityOfDeath>(id + 2, 0.8f, comp => comp.NumCasts >= 1, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<DualityOfDeath>(id + 3, 2.3f, comp => comp.NumCasts >= 2, "Tankbuster 2")
            .DeactivateOnExit<DualityOfDeath>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private State Dualspell(uint id, float delay, bool isTwoMinds = false)
    {
        CastMulti(id, [isTwoMinds ? AID.TwoMindsIceFire : AID.DualspellIceFire, isTwoMinds ? AID.TwoMindsIceLightning : AID.DualspellIceLightning], delay, isTwoMinds ? 7 : 5)
            .ActivateOnEnter<DualspellFire>()
            .ActivateOnEnter<DualspellLightning>()
            .ActivateOnEnter<DualspellIce>();
        return ComponentCondition<DualspellIce>(id + 0x10, isTwoMinds ? 1.2f : 7.8f, comp => comp.NumCasts > 0, "Dualspell")
            .DeactivateOnExit<DualspellFire>()
            .DeactivateOnExit<DualspellLightning>()
            .DeactivateOnExit<DualspellIce>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void TripleDualspell(uint id, float delay)
    {
        Dualspell(id, delay);
        Dualspell(id + 0x1000, 0.3f);
        Dualspell(id + 0x2000, 0.3f);
    }

    // TODO: tankswap component
    private void AscendantFist(uint id, float delay)
    {
        Cast(id, AID.AscendantFist, delay, 5, "Tankbuster swap")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    // keeps lines component active
    private void ArchaicRockbreaker(uint id, float delay)
    {
        Cast(id, AID.ArchaicRockbreakerCenter, delay, 5)
            .ActivateOnEnter<ArchaicRockbreakerCenter>()
            .ActivateOnEnter<ArchaicRockbreakerShockwave>()
            .ActivateOnEnter<ArchaicRockbreakerPairs>()
            .DeactivateOnExit<ArchaicRockbreakerCenter>();
        ComponentCondition<ArchaicRockbreakerShockwave>(id + 0x10, 1.5f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<ArchaicRockbreakerShockwave>();
        ComponentCondition<ArchaicRockbreakerPairs>(id + 0x20, 1.3f, comp => !comp.Active, "Pairs")
            .ActivateOnEnter<ArchaicRockbreakerLine>() // first 8 casts start together with knockback
            .DeactivateOnExit<ArchaicRockbreakerPairs>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ArchaicRockbreakerCombination(uint id, float delay)
    {
        ArchaicRockbreaker(id, delay);

        CastMulti(id + 0x100, [AID.FrontCombinationOut, AID.FrontCombinationIn, AID.RearCombinationOut, AID.RearCombinationIn], 0.3f, 6) // note: second set of lines start casting ~4.4s into cast, overlapping with first
            .ActivateOnEnter<ArchaicRockbreakerCombination>();
        ComponentCondition<ArchaicRockbreakerLine>(id + 0x110, 0.4f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<ArchaicRockbreakerLine>(); // hide second set for a time
        ComponentCondition<ArchaicRockbreakerCombination>(id + 0x111, 0.6f, comp => comp.NumCasts >= 1, "In/out");
        ComponentCondition<ArchaicRockbreakerCombination>(id + 0x120, 3.1f, comp => comp.NumCasts >= 2, "Front/back")
            .ActivateOnEnter<ArchaicRockbreakerLine>(); // start showing lines again
        ComponentCondition<ArchaicRockbreakerCombination>(id + 0x130, 2.9f, comp => comp.NumCasts >= 3, "Out/in")
            .DeactivateOnExit<ArchaicRockbreakerLine>() // lines end ~0.6s before
            .DeactivateOnExit<ArchaicRockbreakerCombination>();

        Cast(id + 0x200, AID.ArchaicDemolish, 2.4f, 4)
            .ActivateOnEnter<ArchaicDemolish>();
        ComponentCondition<ArchaicDemolish>(id + 0x210, 1.2f, comp => !comp.Active, "Party stacks")
            .DeactivateOnExit<ArchaicDemolish>()
            .DeactivateOnExit<Uplift>() // walls disappear after next raidwide, but that doesn't really matter
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ArchaicRockbreakerDualspell(uint id, float delay)
    {
        ArchaicRockbreaker(id, delay);
        Dualspell(id + 0x100, 2.3f)
            .DeactivateOnExit<ArchaicRockbreakerLine>()
            .DeactivateOnExit<Uplift>();
    }

    private void LevinstrikeSummoning(uint id, float delay)
    {
        Cast(id, AID.LevinstrikeSummoning, delay, 4)
            .ActivateOnEnter<LevinstrikeSummoningIcemeld>()
            .ActivateOnEnter<LevinstrikeSummoningFiremeld>()
            .ActivateOnEnter<LevinstrikeSummoningShock>();
        Cast(id + 0x10, AID.ScrambledSuccession, 2.1f, 10);
        Targetable(id + 0x20, false, 0.1f, "Disappear");
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x30, 2.5f, comp => comp.NumCasts >= 1);
        ComponentCondition<LevinstrikeSummoningFiremeld>(id + 0x31, 2.3f, comp => comp.NumCasts >= 1);
        ComponentCondition<LevinstrikeSummoningIcemeld>(id + 0x32, 0.2f, comp => comp.NumCasts >= 1);
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x33, 0.8f, comp => comp.NumTowers >= 1, "Tower 1");
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x40, 2.4f, comp => comp.NumCasts >= 2);
        ComponentCondition<LevinstrikeSummoningFiremeld>(id + 0x41, 2.3f, comp => comp.NumCasts >= 2);
        ComponentCondition<LevinstrikeSummoningIcemeld>(id + 0x42, 0.3f, comp => comp.NumCasts >= 2);
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x43, 0.8f, comp => comp.NumTowers >= 2, "Tower 2");
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x50, 2.4f, comp => comp.NumCasts >= 3);
        ComponentCondition<LevinstrikeSummoningFiremeld>(id + 0x51, 2.3f, comp => comp.NumCasts >= 3);
        ComponentCondition<LevinstrikeSummoningIcemeld>(id + 0x52, 0.3f, comp => comp.NumCasts >= 3);
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x53, 0.8f, comp => comp.NumTowers >= 3, "Tower 3");
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x60, 2.5f, comp => comp.NumCasts >= 4);
        ComponentCondition<LevinstrikeSummoningFiremeld>(id + 0x61, 2.3f, comp => comp.NumCasts >= 4)
            .DeactivateOnExit<LevinstrikeSummoningFiremeld>();
        ComponentCondition<LevinstrikeSummoningIcemeld>(id + 0x62, 0.3f, comp => comp.NumCasts >= 4)
            .DeactivateOnExit<LevinstrikeSummoningIcemeld>();
        ComponentCondition<LevinstrikeSummoningShock>(id + 0x63, 0.8f, comp => comp.NumTowers >= 4, "Tower 4")
            .DeactivateOnExit<LevinstrikeSummoningShock>();
        Targetable(id + 0x70, true, 1.4f, "Reappear");

        Dualspell(id + 0x1000, 0.1f, true)
            .DeactivateOnExit<Uplift>();
    }

    private void ChimericSuccession(uint id, float delay)
    {
        Cast(id, AID.ChimericSuccession, delay, 5);
        CastStartMulti(id + 0x10, [AID.FrontFirestrikes, AID.RearFirestrikes], 7.5f)
            .ActivateOnEnter<ChimericSuccession>();
        ComponentCondition<ChimericSuccession>(id + 0x20, 3.3f, comp => comp.NumCasts >= 1, "Defamation 1");
        ComponentCondition<ChimericSuccession>(id + 0x21, 3.0f, comp => comp.NumCasts >= 2, "Defamation 2");
        CastEnd(id + 0x30, 1.7f);
        ComponentCondition<ChimericSuccession>(id + 0x31, 0.4f, comp => !comp.JumpActive, "Baited jump");
        ComponentCondition<ChimericSuccession>(id + 0x40, 0.9f, comp => comp.NumCasts >= 3, "Defamation 3");
        CastStartMulti(id + 0x50, [AID.SwingingKickFront, AID.SwingingKickRear], 1.2f);
        ComponentCondition<ChimericSuccession>(id + 0x60, 1.8f, comp => comp.NumCasts >= 4, "Defamation 4")
            .ActivateOnEnter<SwingingKickFront>()
            .ActivateOnEnter<SwingingKickRear>()
            .DeactivateOnExit<ChimericSuccession>();
        CastEnd(id + 0x70, 1.2f, "Front/back cleave")
            .DeactivateOnExit<SwingingKickFront>()
            .DeactivateOnExit<SwingingKickRear>();
    }

    private void Charibdys(uint id, float delay)
    {
        Cast(id, AID.Charybdis, delay, 3)
            .ActivateOnEnter<Charibdys>();
        Cast(id + 0x10, AID.Comet, 2.1f, 5)
            .ActivateOnEnter<CometImpact>();
        ComponentCondition<CometImpact>(id + 0x20, 1.1f, comp => comp.NumCasts > 0, "Proximity")
            .DeactivateOnExit<CometImpact>();

        CastStart(id + 0x100, AID.BeastlyBile, 3.1f)
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<CometBurst>()
            .ActivateOnEnter<BeastlyBile>()
            .ActivateOnEnter<Thunderbolt>();
        CastEnd(id + 0x101, 5);
        Cast(id + 0x110, AID.Thunderbolt, 2.1f, 3);
        ComponentCondition<Thunderbolt>(id + 0x120, 1.0f, comp => comp.NumCasts > 0, "Proteans 1");
        ComponentCondition<BeastlyBile>(id + 0x121, 0.9f, comp => comp.NumCasts >= 1, "Stack 1");
        // +0.8s: burst 1 should start
        Cast(id + 0x130, AID.Thunderbolt, 1.3f, 3);
        ComponentCondition<Thunderbolt>(id + 0x140, 1.0f, comp => comp.NumCasts > 4, "Proteans 2")
            .DeactivateOnExit<Thunderbolt>();
        ComponentCondition<BeastlyBile>(id + 0x141, 0.9f, comp => comp.NumCasts >= 2, "Stack 2")
            .DeactivateOnExit<BeastlyBile>();
        // +0.8s: burst 1 should finish, burst 2 should start

        CastStart(id + 0x200, AID.EclipticMeteor, 2.4f)
            .ActivateOnEnter<EclipticMeteor>();
        // +4.4s: burst 2 should finish
        CastEnd(id + 0x201, 7);
        ComponentCondition<EclipticMeteor>(id + 0x210, 1.9f, comp => comp.NumCasts > 0, "LOS meteor")
            .DeactivateOnExit<EclipticMeteor>();

        Cast(id + 0x300, AID.BeastlyFury, 6.7f, 5);
        ComponentCondition<BeastlyFury>(id + 0x310, 1.0f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<BeastlyFury>()
            .DeactivateOnExit<BeastlyFury>()
            .DeactivateOnExit<Comet>()
            .DeactivateOnExit<CometBurst>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
