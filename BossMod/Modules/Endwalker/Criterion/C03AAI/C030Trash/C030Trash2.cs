namespace BossMod.Endwalker.Criterion.C03AAI.C030Trash2
{
    public enum OID : uint
    {
        NWoodGolem = 0x40D0, // R2.660
        NIslekeeper = 0x40D1, // R2.550

        SWoodGolem = 0x40DA, // R2.660
        SIslekeeper = 0x40DB, // R2.550
    };

    public enum AID : uint
    {
        AutoAttack = 31320, // *WoodGolem/*Islekeeper->player, no cast, single-target

        NAncientAero = 35916, // NWoodGolem->self, 5.0s cast, range 100 circle, interruptible heavy raidwide
        NTornado = 35917, // NWoodGolem->player, 5.0s cast, range 4 circle spread
        NOvation = 35777, // NWoodGolem->self, 4.0s cast, range 12 width 4 rect
        SAncientAero = 35794, // SWoodGolem->self, 5.0s cast, range 100 circle, interruptible heavy raidwide
        STornado = 35795, // SWoodGolem->player, 5.0s cast, range 4 circle spread
        SOvation = 35796, // SWoodGolem->self, 4.0s cast, range 12 width 4 rect

        NAncientQuaga = 35918, // NIslekeeper->self, 5.0s cast, range 100 circle, raidwide
        NGravityForce = 35781, // NIslekeeper->players, 5.0s cast, range 6 circle stack
        NIsleDrop = 35951, // NIslekeeper->location, 5.0s cast, range 6 circle puddle
        NAncientQuagaEnrage = 35887, // NIslekeeper->self, 10.0s cast, range 100 circle enrage
        SAncientQuaga = 35897, // SIslekeeper->self, 5.0s cast, range 100 circle, raidwide
        SGravityForce = 35898, // SIslekeeper->players, 5.0s cast, range 6 circle stack
        SIsleDrop = 35900, // SIslekeeper->location, 5.0s cast, range 6 circle puddle
        SAncientQuagaEnrage = 35914, // SIslekeeper->self, 10.0s cast, range 100 circle enrage
    };

    public abstract class C030Trash2 : BossModule
    {
        public C030Trash2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(200, 128), 30)) { }
    }
}
