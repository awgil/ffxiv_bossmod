namespace BossMod.Roleplay;

public enum AID : uint
{
    // magitek reaper in Fly Free, My Pretty
    MagitekCannon = 7619, // range 30 radius 6 ground targeted aoe
    PhotonStream = 7620, // range 10 width 4 rect aoe
    DiffractiveMagitekCannon = 7621, // range 30 radius 10 ground targeted aoe
    HighPoweredMagitekCannon = 7622, // range 42 width 8 rect aoe

    // Alphinaud - I forgot the name of this quest and it needs a module
    RuinIII = 11191,
    Physick = 11192,
    TriShackle = 11482,

    // Y'shtola - Will of the Moon (StB)
    StoneIVSeventhDawn = 13423,
    AeroIISeventhDawn = 13424,
    CureIISeventhDawn = 13425,
    Aetherwell = 13426,

    // Hien - Requiem for Heroes (StB)
    Kyokufu = 14840,
    Gofu = 19046,
    Yagetsu = 19047,
    Ajisai = 14841,
    HissatsuGyoten = 14842,
    SecondWind = 15375,
}

public enum TraitID : uint { }

public enum SID : uint
{
    RolePlaying = 1534,

    // Hien
    Ajisai = 1779,
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.MagitekCannon);
        d.RegisterSpell(AID.PhotonStream);
        d.RegisterSpell(AID.DiffractiveMagitekCannon);
        d.RegisterSpell(AID.HighPoweredMagitekCannon);

        d.RegisterSpell(AID.RuinIII);
        d.RegisterSpell(AID.Physick);
        d.RegisterSpell(AID.TriShackle);

        d.RegisterSpell(AID.StoneIVSeventhDawn);
        d.RegisterSpell(AID.AeroIISeventhDawn);
        d.RegisterSpell(AID.CureIISeventhDawn);
        d.RegisterSpell(AID.Aetherwell);

        d.RegisterSpell(AID.Kyokufu);
        d.RegisterSpell(AID.Gofu);
        d.RegisterSpell(AID.Yagetsu);
        d.RegisterSpell(AID.Ajisai);
        d.RegisterSpell(AID.HissatsuGyoten);
        d.RegisterSpell(AID.SecondWind);
    }

    public void Dispose() { }
}
