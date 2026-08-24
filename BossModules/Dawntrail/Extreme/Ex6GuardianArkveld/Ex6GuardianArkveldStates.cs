namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class Ex6GuardianArkveldStates : StateMachineBuilder
{
    private bool _slow = true;

    public Ex6GuardianArkveldStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1)
            .ActivateOnEnter<Roar1>()
            .ActivateOnEnter<Roar2>()
            .ActivateOnEnter<Roar3>()
            // these are interspersed with other mechanics and we don't attach conditions to them
            .ActivateOnEnter<WyvernsRadianceExawave>()
            .ActivateOnEnter<WyvernsRadiancePuddle>()
            .ActivateOnEnter<WyvernsRadianceBigPuddle>();
    }

    private void P1(uint id)
    {
        Roar(id, 0.1f);

        ChainbladeBlow(id + 0x100, 3.8f);

        Siegeflight(id + 0x200, 4.9f);
        Siegeflight(id + 0x300, 7.9f);

        ChainbladeBlow(id + 0x400, 1.8f);

        Roar(id + 0x10000, 1.6f);
        Rush(id + 0x10100, 12.1f);
        SteeltailThrust(id + 0x10500, 3);
        ChainbladeCharge(id + 0x20000, 2.2f);
        ChainbladeBlow(id + 0x20100, 2.7f);
        Roar(id + 0x20200, 1.7f);

        AethericResonance(id + 0x30000, 14.8f);
        ExaCrystals1(id + 0x40000, 10.3f);

        ForgedFury(id + 0x50000, 3.1f);
        Roar(id + 0x50100, 11.1f);

        _slow = false;

        ClamorousChase(id + 0x60000, 3.8f);
        Roar(id + 0x60100, 1.1f);

        AethericResonance(id + 0x70000, 13.6f);
        WyvernsWeal(id + 0x80000, 14.5f);
        Roar(id + 0x80100, 7.5f);

        WyveCannon(id + 0x90000, 6.1f);
        ChainbladeBlow(id + 0x90100, 4.5f);
        Roar(id + 0x90200, 1.7f);

        Rush(id + 0xA0000, 12)
            .DeactivateOnExit<WyveCannonEdge>()
            .DeactivateOnExit<WyveCannonMiddle>();
        SteeltailThrust(id + 0xA0200, 3);
        ChainbladeCharge(id + 0xA0300, 2.2f);
        ChainbladeBlow(id + 0xA0400, 3.7f);
        Roar(id + 0xA0500, 1.7f);

        AethericResonance(id + 0xB0000, 14.8f);
        VengeanceLine(id + 0xB0100, 2.2f);
        ChainbladeBlow(id + 0xB0300, 3.8f);

        Siegeflight(id + 0xC0000, 11.2f);
        Siegeflight(id + 0xC0300, 7.8f);
        ChainbladeBlow(id + 0xC0400, 1.8f);
        Roar(id + 0xC0500, 1.6f);

        Rush(id + 0xD0000, 12.2f);
        SteeltailThrust(id + 0xD0200, 3);
        ChainbladeCharge(id + 0xD0300, 2.2f);
        ChainbladeBlow(id + 0xD0400, 3.9f);
        Roar(id + 0xD0500, 1.7f);
        Roar(id + 0xD0600, 11.9f);

        CastStart(id + 0xE0000, AID.ForgedFuryEnrageCast, 8);
        Timeout(id + 0xE0001, 10.2f, "Enrage");
    }

    private void Roar(uint id, float delay)
    {
        CastMulti(id, [AID.Roar1, AID.Roar2, AID.Roar3], delay, 5, "Raidwide");
    }

    private static readonly AID[] slowCasts = [AID.ChainbladeBossCast2, AID.ChainbladeBossCast1];
    private static readonly AID[] fastCasts = [AID.ChainbladeBossCast4, AID.ChainbladeBossCast3];

    private void ChainbladeBlow(uint id, float delay, bool twice = true)
    {
        var casts = _slow ? slowCasts : fastCasts;
        CastStartMulti(id, casts, delay)
            .ActivateOnEnter<ChainbladeTail>()
            .ActivateOnEnter<ChainbladeSide>()
            .ActivateOnEnter<ChainbladeRepeat>(twice);

        var label = twice ? "Cleave 1" : "Cleave";
        var expected = _slow ? 7.2f : 6.2f;
        ComponentCondition<ChainbladeSide>(id + 0x10, expected, c => c.NumCasts > 0, label)
            .DeactivateOnExit<ChainbladeSide>()
            .DeactivateOnExit<ChainbladeTail>();

        if (!twice)
            return;

        ComponentCondition<ChainbladeRepeat>(id + 0x20, 4.1f, c => c.NumCasts > 0, "Cleave 2")
            .DeactivateOnExit<ChainbladeRepeat>();
    }

    private void Siegeflight(uint id, float delay)
    {
        CastStartMulti(id, [AID.BossFlight1, AID.BossFlight3, AID.BossFlight2, AID.BossFlight4], delay)
            .ActivateOnEnter<BossSiegeflight>()
            .ActivateOnEnter<HelperSiegeflight>()
            .ActivateOnEnter<WhiteFlashDragonspark>()
            .ActivateOnEnter<WhiteFlash>()
            .ActivateOnEnter<Dragonspark>()
            .ActivateOnEnter<GuardianResonance>()
            .ActivateOnEnter<WyvernsRadianceSides>();

        ComponentCondition<BossSiegeflight>(id + 0x10, _slow ? 5 : 4, b => b.NumCasts > 0);
        ComponentCondition<HelperSiegeflight>(id + 0x12, 1.5f, b => b.NumCasts > 0, "Dive")
            .DeactivateOnExit<BossSiegeflight>()
            .DeactivateOnExit<HelperSiegeflight>();

        ComponentCondition<WhiteFlashDragonspark>(id + 0x20, 3.5f, w => w.NumCasts > 0, "In/out + stacks")
            .DeactivateOnExit<WhiteFlashDragonspark>()
            .DeactivateOnExit<WhiteFlash>()
            .DeactivateOnExit<Dragonspark>()
            .DeactivateOnExit<GuardianResonance>()
            .DeactivateOnExit<WyvernsRadianceSides>();
    }

    private State Rush(uint id, float delay)
    {
        CastStart(id, AID.RushCast, delay)
            .ActivateOnEnter<WyvernsRadianceQuake>()
            .ActivateOnEnter<Rush>();

        ComponentCondition<Rush>(id + 0x10, 6, r => r.NumCasts > 0, "Dash 1");
        ComponentCondition<Rush>(id + 0x20, 2.1f, r => r.NumCasts > 1, "Dash 2");
        ComponentCondition<Rush>(id + 0x30, 2, r => r.NumCasts > 2, "Dash 3")
            .DeactivateOnExit<Rush>();

        CastStartMulti(id + 0x100, [AID.OurobladeCast2, AID.OurobladeCast1, AID.OurobladeCast4, AID.OurobladeCast3], _slow ? 7.7f : 8.2f)
            .ActivateOnEnter<WyvernsOuroblade>()
            .ActivateOnEnter<WildEnergy>()
            .DeactivateOnExit<WyvernsRadianceQuake>();

        ComponentCondition<WyvernsOuroblade>(id + 0x110, _slow ? 7 : 6, w => w.NumCasts > 0, "Left/right")
            .DeactivateOnExit<WyvernsOuroblade>();
        return ComponentCondition<WildEnergy>(id + 0x120, 0.2f, w => w.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<WildEnergy>();
    }

    private void SteeltailThrust(uint id, float delay)
    {
        Cast(id, _slow ? AID.SteeltailCast1 : AID.SteeltailCast2, delay, _slow ? 4 : 3)
            .ActivateOnEnter<SteeltailThrust>();

        ComponentCondition<SteeltailThrust>(id + 2, 0.6f, s => s.NumCasts > 0, "Tail")
            .DeactivateOnExit<SteeltailThrust>();
    }

    private void ChainbladeCharge(uint id, float delay)
    {
        CastStart(id, AID.ChainbladeChargeCast, delay)
            .ActivateOnEnter<ChainbladeCharge>();

        ComponentCondition<ChainbladeCharge>(id + 0x10, 8.4f, c => c.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<ChainbladeCharge>();
    }

    private void AethericResonance(uint id, float delay)
    {
        CastStart(id, AID.AethericResonance, delay)
            .ActivateOnEnter<GuardianResonancePuddle>()
            .ActivateOnEnter<GuardianResonanceTowerSmall>()
            .ActivateOnEnter<GuardianResonanceTowerLarge>();

        ComponentCondition<GuardianResonancePuddle>(id + 0x10, 3, p => p.NumStarted > 0, "Puddles start");

        ComponentCondition<GuardianResonancePuddle>(id + 0x20, 2.5f, p => p.NumStarted > 1)
            .ExecOnExit<GuardianResonanceTowerLarge>(l => l.EnableBaitHints(2.5f))
            .ExecOnExit<GuardianResonanceTowerSmall>(l => l.EnableBaitHints(2.5f));

        ComponentCondition<GuardianResonancePuddle>(id + 0x30, 2.5f, p => p.NumStarted > 2)
            .ExecOnExit<GuardianResonanceTowerLarge>(l => l.DisableBaitHints())
            .ExecOnExit<GuardianResonanceTowerSmall>(l => l.DisableBaitHints());

        ComponentCondition<GuardianResonanceTowerLarge>(id + 0x40, 2.9f, l => l.NumCasts > 0, "Towers")
            .DeactivateOnExit<GuardianResonancePuddle>()
            .DeactivateOnExit<GuardianResonanceTowerLarge>()
            .DeactivateOnExit<GuardianResonanceTowerSmall>();
    }

    private void ExaCrystals1(uint id, float delay)
    {
        ComponentCondition<WyvernsVengeance>(id, delay, v => v.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<WyvernsVengeance>()
            .ActivateOnEnter<WyvernsRadianceCrystal>()
            .ActivateOnEnter<WildEnergy>();

        ComponentCondition<WildEnergy>(id + 0x10, 15, w => w.NumFinishedSpreads > 0, "Spreads 1");
        // if too many people are dead, the second set of spreads doesn't even appear, so don't check condition
        Timeout(id + 0x20, 8.1f, "Spreads 2")
            .DeactivateOnExit<WildEnergy>();
    }

    private void ForgedFury(uint id, float delay)
    {
        CastStart(id, AID.ForgedFuryCast, delay)
            .ActivateOnEnter<ForgedFury>()
            .DeactivateOnExit<WyvernsVengeance>()
            .DeactivateOnExit<WyvernsRadianceCrystal>();

        ComponentCondition<ForgedFury>(id + 0x10, 7, f => f.NumCasts > 0, "Raidwide 1");
        ComponentCondition<ForgedFury>(id + 0x20, 0.8f, f => f.NumCasts > 1, "Raidwide 2");
        ComponentCondition<ForgedFury>(id + 0x30, 2.4f, f => f.NumCasts > 2, "Raidwide 3")
            .DeactivateOnExit<ForgedFury>();
    }

    private void ClamorousChase(uint id, float delay)
    {
        // make sure icons are detected even if they go out before cast
        Timeout(id, 0)
            .ActivateOnEnter<ClamorousBait>();

        CastStartMulti(id + 0x10, [AID.ClamorousChaseRightWing, AID.ClamorousChaseLeftWing], delay)
            .ActivateOnEnter<ClamorousCleave>()
            .ActivateOnEnter<ClamorousJump>();

        ComponentCondition<ClamorousJump>(id + 0x20, 8.2f, j => j.NumCasts > 0, "Limit cut start");
        ComponentCondition<ClamorousCleave>(id + 0x30, 23.5f, c => c.NumCasts >= 8, "Limit cut finish")
            .DeactivateOnExit<ClamorousBait>()
            .DeactivateOnExit<ClamorousCleave>()
            .DeactivateOnExit<ClamorousJump>();
    }

    private void WyvernsWeal(uint id, float delay)
    {
        ComponentCondition<WyvernsWealCast>(id, delay, w => w.NumCasts > 0, "Laser 1 start (crystals)")
            .ActivateOnEnter<WyvernsWeal>()
            .ActivateOnEnter<WyvernsWealCast>()
            .ActivateOnEnter<BigCrystal>()
            .ActivateOnEnter<SmallCrystal>();

        ComponentCondition<WyvernsWeal>(id + 0x10, 9.5f, w => w.NumCasts > 9, "Laser 2 start (stacks)")
            .ActivateOnEnter<WhiteFlash>();
        ComponentCondition<WhiteFlash>(id + 0x12, 5.5f, w => w.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<WhiteFlash>()
            .DeactivateOnExit<BigCrystal>()
            .DeactivateOnExit<SmallCrystal>();
        ComponentCondition<WyvernsWeal>(id + 0x20, 4, w => w.NumCasts > 18, "Laser 3 start (spread)")
            .ActivateOnEnter<WildEnergy>()
            .ExecOnEnter<WildEnergy>(w => w.EnableHints = false);
        ComponentCondition<WyvernsWeal>(id + 0x22, 6, w => w.NumCasts == 27)
            .ExecOnExit<WildEnergy>(w => w.EnableHints = true);
        ComponentCondition<WildEnergy>(id + 0x30, 4.5f, w => w.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<WildEnergy>()
            .DeactivateOnExit<WyvernsWeal>()
            .DeactivateOnExit<WyvernsWealCast>();
    }

    private void WyveCannon(uint id, float delay)
    {
        CastStart(id, AID.WrathfulRattle, delay)
            .ActivateOnEnter<WyveCannonMiddle>()
            .ActivateOnEnter<WyveCannonEdge>();
        ComponentCondition<WyveCannonMiddle>(id + 0x10, 3.5f, w => w.NumCasts > 0, "Exaflares start");
    }

    private void VengeanceLine(uint id, float delay)
    {
        Timeout(id, 0)
            .ActivateOnEnter<WyvernsVengeanceLine>()
            .ActivateOnEnter<SmallCrystal>()
            .ActivateOnEnter<BigCrystal>()
            .ExecOnEnter<SmallCrystal>(c => c.Color = ArenaColor.Danger)
            .ExecOnEnter<BigCrystal>(c => c.Color = ArenaColor.Danger);
        Roar(id + 1, delay);

        ComponentCondition<WyvernsVengeanceLine>(id + 0x10, 5, l => l.NumCasts > 0, "Exaflares start");

        ComponentCondition<WyvernsVengeanceLine>(id + 0x20, 11.5f, l => l.NumCasts >= 36, "Exaflares finish")
            .ActivateOnEnter<ChainbladeCharge>()
            .DeactivateOnExit<WyvernsVengeanceLine>();

        ComponentCondition<ChainbladeCharge>(id + 0x30, 6.7f, c => c.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<ChainbladeCharge>();
    }
}
