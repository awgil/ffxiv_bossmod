namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class Ex6GuardianArkveldStates : StateMachineBuilder
{
    public Ex6GuardianArkveldStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1)
            .ActivateOnEnter<Roar1>()
            .ActivateOnEnter<Roar2>()
            .ActivateOnEnter<Roar3>();
    }

    private void P1(uint id)
    {
        Roar(id, 0.1f);

        ChainbladeBlow(id + 0x100, 3.8f, true);

        Siegeflight(id + 0x200, 5, true);
        Siegeflight(id + 0x300, 7.9f, true);

        ChainbladeBlow(id + 0x400, 1.8f, true);

        Roar(id + 0x10000, 1.6f);

        Timeout(id + 0xFF0000, 10000, "???");
    }

    private static readonly AID[] slowCasts = [AID._Weaponskill_ChainbladeBlow, AID._Weaponskill_ChainbladeBlow6];
    private static readonly AID[] fastCasts = [AID._Weaponskill_ChainbladeBlow12, AID._Weaponskill_ChainbladeBlow18];

    private void Roar(uint id, float delay)
    {
        CastMulti(id, [AID._Weaponskill_Roar, AID._Weaponskill_Roar1, AID._Weaponskill_Roar2], delay, 5, "Raidwide");
    }

    private void ChainbladeBlow(uint id, float delay, bool slow, bool twice = true)
    {
        var casts = slow ? slowCasts : fastCasts;
        CastStartMulti(id, casts, delay)
            .ActivateOnEnter<ChainbladeBlow>()
            .ActivateOnEnter<ChainbladeRadiance>()
            .ActivateOnEnter<ChainbladeRepeat>(twice);

        var label = twice ? "Cleave 1" : "Cleave";
        var expected = slow ? 7.2f : 6.2f;
        ComponentCondition<ChainbladeRadiance>(id + 0x10, expected, c => c.NumCasts > 0, label)
            .DeactivateOnExit<ChainbladeRadiance>()
            .DeactivateOnExit<ChainbladeBlow>();

        if (!twice)
            return;

        ComponentCondition<ChainbladeRepeat>(id + 0x20, 4.1f, c => c.NumCasts > 0, "Cleave 2")
            .DeactivateOnExit<ChainbladeRepeat>();
    }

    private void Siegeflight(uint id, float delay, bool slow)
    {
        CastStartMulti(id, [AID._Weaponskill_GuardianSiegeflight, AID._Weaponskill_GuardianSiegeflight2, AID._Weaponskill_WyvernsSiegeflight, AID._Weaponskill_WyvernsSiegeflight2], delay)
            .ActivateOnEnter<BossSiegeflight>()
            .ActivateOnEnter<HelperSiegeflight>()
            .ActivateOnEnter<WhiteFlashDragonspark>()
            .ActivateOnEnter<WhiteFlash>()
            .ActivateOnEnter<Dragonspark>()
            .ActivateOnEnter<GuardianResonance>()
            .ActivateOnEnter<WyvernsRadianceSides>();

        ComponentCondition<BossSiegeflight>(id + 0x10, slow ? 5 : 4, b => b.NumCasts > 0);
        ComponentCondition<HelperSiegeflight>(id + 0x12, 1.6f, b => b.NumCasts > 0, "Dive")
            .DeactivateOnExit<BossSiegeflight>()
            .DeactivateOnExit<HelperSiegeflight>();

        ComponentCondition<WhiteFlashDragonspark>(id + 0x20, 3.4f, w => w.NumCasts > 1, "In/out + stacks")
            .DeactivateOnExit<WhiteFlashDragonspark>()
            .DeactivateOnExit<WhiteFlash>()
            .DeactivateOnExit<Dragonspark>()
            .DeactivateOnExit<GuardianResonance>()
            .DeactivateOnExit<WyvernsRadianceSides>();
    }
}
