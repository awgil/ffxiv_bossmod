namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class Duel5MeneniusStates : StateMachineBuilder
{
    public Duel5MeneniusStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ReactiveMunition>()
            .ActivateOnEnter<GunberdShot>()
            .ActivateOnEnter<LargeGigaTempest>()
            .ActivateOnEnter<SmallGigaTempest>()
            .ActivateOnEnter<RuinationExaflare>()
            .ActivateOnEnter<ProactiveMunition>()
            .ActivateOnEnter<MagitekImpetus>()
            .ActivateOnEnter<BlueHiddenMines>()
            .ActivateOnEnter<RedHiddenMines>();
    }

    private void SinglePhase(uint id)
    {
        CallousCrossfire(id, 13);

        MagitekMinefield(id + 0x10000, 12);
        GigaTempest(id + 0x20000, 11.25f);
        ReadyShot(id + 0x30000, 15.5f);
        Gunberd(id + 0x40000, 2);
        MagitekImpetus(id + 0x50000, 2.6f);
        MagitekMinefield(id + 0x60000, 6.2f);
        ReadyShot(id + 0x70000, 10.25f);
        Gunberd(id + 0x80000, 2);
        MagitekMinefield(id + 0x90000, 5.6f);
        Ruination(id + 0xA0000, 6.5f);
        SpiralScourge(id + 0xB0000, 18.25f);

        ProactiveMunition(id + 0x100000, 7.25f);
        MagitekMinefield(id + 0x110000, 7.25f);
        ReactiveMunition(id + 0x120000, 7.25f);
        SenseWeakness(id + 0x130000, 13);
        ReadyShot(id + 0x140000, 12.25f);
        GigaTempest(id + 0x150000, 2);
        MagitekImpetus(id + 0x160000, 3.3f);
        Gunberd(id + 0x170000, 10);
        ReactiveMunition(id + 0x180000, 13.6f);
        Ruination(id + 0x190000, 4f);
        SenseWeakness(id + 0x1A0000, 8.25f);
        IndiscriminateDetonation(id + 0x1B0000, 3.25f);

        ReadyShot(id + 0x200000, 11.25f);
        ReactiveMunition(id + 0x210000, 2);
        MagitekMinefield(id + 0x220000, 2);
        MagitekImpetus(id + 0x230000, 10.3f);
        MagitekMinefield(id + 0x240000, 8.25f);
        Gunberd(id + 0x250000, 9.25f);
        MagitekMinefield(id + 0x260000, 8.6f);
        ReadyShot(id + 0x270000, 12.5f);
        Ruination(id + 0x280000, 2.25f);
        MagitekMinefield(id + 0x290000, 10.3f);
        Gunberd(id + 0x2A0000, 9.3f);
        ReadyShot(id + 0x2B0000, 13.8f);
        GigaTempest(id + 0x2C0000, 2.2f);
        MagitekImpetus(id + 0x2D0000, 3.5f);
        Gunberd(id + 0x2E0000, 10.3f);
        ReactiveMunition(id + 0x2F0000, 13.7f);
        Ruination(id + 0x300000, 4.5f);
        SenseWeakness(id + 0x310000, 8.25f);
        IndiscriminateDetonation(id + 0x320000, 3.2f);

        TeraTempest(id + 0x400000, 12.9f);
    }

    private void CallousCrossfire(uint id, float delay)
    {
        Cast(id, AID.CallousCrossfire, delay, 4, "Turret Crossfire")
            .ActivateOnEnter<CallousCrossfire>()
            .DeactivateOnExit<CallousCrossfire>();
    }

    private void MagitekMinefield(uint id, float delay)
    {
        Cast(id, AID.MagitekMinefield, delay, 3, "Place Mine");
    }

    private void IndiscriminateDetonation(uint id, float delay)
    {
        Cast(id, AID.IndiscriminateDetonation, delay, 4, "Detonate Mines");
    }

    private void GigaTempest(uint id, float delay)
    {
        Cast(id, AID.GigaTempest, delay, 5, "Gigatempest");
    }

    private void MagitekImpetus(uint id, float delay)
    {
        Cast(id, AID.MagitekImpetus, delay, 3, "Place Forced March");
    }

    private void ReadyShot(uint id, float delay)
    {
        CastMulti(id, new[] { AID.DarkShot, AID.WindslicerShot }, delay, 4, "Load Dark/Windslicer Shot");
    }

    private void Gunberd(uint id, float delay)
    {
        CastMulti(id, new[] { AID.GunberdDark, AID.GunberdWindslicer }, delay, 4, "Shoot Dark/Windslicer Shot");
    }

    private void Ruination(uint id, float delay)
    {
        Cast(id, AID.Ruination, delay, 4, "Ruination")
            .ActivateOnEnter<RuinationCross>()
            .DeactivateOnExit<RuinationCross>();
    }

    private void SpiralScourge(uint id, float delay)
    {
        Cast(id, AID.SpiralScourge, delay, 6, "Tankbuster")
            .ActivateOnEnter<SpiralScourge>()
            .DeactivateOnExit<SpiralScourge>();
    }
    private void ProactiveMunition(uint id, float delay)
    {
        Cast(id, AID.ProactiveMunition, delay, 5, "Chasing AOE");
    }

    private void ReactiveMunition(uint id, float delay)
    {
        Cast(id, AID.ReactiveMunition, delay, 3, "Place Acceleration Bomb");
    }

    private void SenseWeakness(uint id, float delay)
    {
        Cast(id, AID.SenseWeakness, delay, 4.5f, "Move")
            .ActivateOnEnter<SenseWeakness>()
            .DeactivateOnExit<SenseWeakness>();
    }

    private void TeraTempest(uint id, float delay)
    {
        Cast(id, AID.TeraTempest, delay, 25, "Enrage");
    }
}
