using static BossMod.Shadowbringers.Foray.Duel.Duel5Menenius.GigaTempest;

namespace BossMod.Shadowbringers.Foray.Duel.Duel5Menenius;

class Duel5MeneniusStates : StateMachineBuilder
{
    uint i = 1;
    private uint nextId => i++ * 0x10000;

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

        MagitekMinefield(id + nextId, 12);
        GigaTempest(id + nextId, 11.25f);
        ReadyShot(id + nextId, 15.5f);
        Gunberd(id + nextId, 2);
        MagitekImpetus(id + nextId, 2.6f);
        MagitekMinefield(id + nextId, 6.2f);
        ReadyShot(id + nextId, 10.25f);
        Gunberd(id + nextId, 2);
        MagitekMinefield(id + nextId, 5.6f);
        Ruination(id + nextId, 6.5f);
        SpiralScourge(id + nextId, 18.25f);

        ProactiveMunition(id + nextId, 7.25f);
        MagitekMinefield(id + nextId, 7.25f);
        ReactiveMunition(id + nextId, 7.25f);
        SenseWeakness(id + nextId, 13);
        ReadyShot(id + nextId, 12.25f);
        GigaTempest(id + nextId, 2);
        MagitekImpetus(id + nextId, 3.3f);
        Gunberd(id + nextId, 10);
        ReactiveMunition(id + nextId, 13.6f);
        Ruination(id + nextId, 4f);
        SenseWeakness(id + nextId, 8.25f);
        IndiscriminateDetonation(id + nextId, 3.25f);

        ReadyShot(id + nextId, 11.25f);
        ReactiveMunition(id + nextId, 2);
        MagitekMinefield(id + nextId, 2);
        MagitekImpetus(id + nextId, 10.3f);
        MagitekMinefield(id + nextId, 8.25f);
        Gunberd(id + nextId, 9.25f);
        MagitekMinefield(id + nextId, 8.6f);
        ReadyShot(id + nextId, 12.5f);
        Ruination(id + nextId, 2.25f);
        MagitekMinefield(id + nextId, 10.3f);
        Gunberd(id + nextId, 9.3f);
        ReadyShot(id + nextId, 13.8f);
        GigaTempest(id + nextId, 2.2f);
        MagitekImpetus(id + nextId, 3.5f);
        Gunberd(id + nextId, 10.3f);
        ReactiveMunition(id + nextId, 13.7f);
        Ruination(id + nextId, 4.5f);
        SenseWeakness(id + nextId, 8.25f);
        IndiscriminateDetonation(id + nextId, 3.2f);

        TeraTempest(id + nextId, 12.9f);
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
        Cast(id + nextId, AID.TeraTempest, delay, 25, "Enrage");
    }
}
