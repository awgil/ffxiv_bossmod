namespace BossMod.Roleplay;

public enum AID : uint
{
    // magitek reaper in Fly Free, My Pretty
    MagitekCannon = 7619, // range 30 radius 6 ground targeted aoe
    PhotonStream = 7620, // range 10 width 4 rect aoe
    DiffractiveMagitekCannon = 7621, // range 30 radius 10 ground targeted aoe
    HighPoweredMagitekCannon = 7622, // range 42 width 8 rect aoe
}

public enum TraitID : uint { }

public enum SID : uint
{
    RolePlaying = 1534,
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.MagitekCannon);
        d.RegisterSpell(AID.PhotonStream);
        d.RegisterSpell(AID.DiffractiveMagitekCannon);
        d.RegisterSpell(AID.HighPoweredMagitekCannon);
    }

    public void Dispose() { }
}
