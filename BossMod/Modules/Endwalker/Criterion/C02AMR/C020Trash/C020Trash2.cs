namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash2;

public enum OID : uint
{
    NKotengu = 0x3F98, // R1.500
    NOnmitsugashira = 0x3F99, // R1.360, x1
    NYamabiko = 0x3FA1, // R0.800, x6

    SKotengu = 0x3F9E, // R1.500
    SOnmitsugashira = 0x3F9F, // R1.360, x1
    SYamabiko = 0x3FA3, // R0.800, x6
};

public enum AID : uint
{
    AutoAttack = 31318, // *Kotengu/*Onmitsugashira->player, no cast, single-target
    // kotengu
    NBackwardBlows = 34396, // NKotengu->self, 4.0s cast, single-target, visual (front-back cleave)
    NLeftwardBlows = 34397, // NKotengu->self, 4.0s cast, single-target, visual (front-left cleave)
    NRightwardBlows = 34398, // NKotengu->self, 4.0s cast, single-target, visual (front-right cleave)
    NBladeOfTheTengu = 34399, // NKotengu->self, no cast, range 50 ?-degree cone
    NWrathOfTheTengu = 34400, // NKotengu->self, 4.0s cast, range 40 circle, raidwide with bleed
    NGazeOfTheTengu = 34401, // NKotengu->self, 4.0s cast, range 60 circle, gaze
    SBackwardBlows = 34414, // SKotengu->self, 4.0s cast, single-target, visual (front-back cleave)
    SLeftwardBlows = 34415, // SKotengu->self, 4.0s cast, single-target, visual (front-left cleave)
    SRightwardBlows = 34416, // SKotengu->self, 4.0s cast, single-target, visual (front-right cleave)
    SBladeOfTheTengu = 34417, // SKotengu->self, no cast, range 50 ?-degree cone
    SWrathOfTheTengu = 34418, // SKotengu->self, 4.0s cast, range 40 circle, raidwide with bleed
    SGazeOfTheTengu = 34419, // SKotengu->self, 4.0s cast, range 60 circle, gaze
    // onmitsugashira
    NIssen = 34402, // NOnmitsugashira->player, 4.0s cast, single-target, tankbuster
    NHuton = 34403, // NOnmitsugashira->self, 4.0s cast, single-target, speed up buff
    NJujiShuriken = 34404, // NOnmitsugashira->self, 3.0s cast, range 40 width 3 rect
    NJujiShurikenFast = 34429, // NOnmitsugashira->self, 1.5s cast, range 40 width 3 rect
    SIssen = 34420, // SOnmitsugashira->player, 4.0s cast, single-target, tankbuster
    SHuton = 34421, // SOnmitsugashira->self, 4.0s cast, single-target, speed up buff
    SJujiShuriken = 34422, // SOnmitsugashira->self, 3.0s cast, range 40 width 3 rect
    SJujiShurikenFast = 34430, // SOnmitsugashira->self, 1.5s cast, range 40 width 3 rect
    // yamabiko
    NMountainBreeze = 34439, // NYamabiko->self, 6.0s cast, range 40 width 8 rect
    SMountainBreeze = 34442, // SYamabiko->self, 6.0s cast, range 40 width 8 rect
};

class MountainBreeze : Components.SelfTargetedAOEs
{
    public MountainBreeze(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(38, 4, 2)) { }
}
class NMountainBreeze : MountainBreeze { public NMountainBreeze() : base(AID.NMountainBreeze) { } }
class SMountainBreeze : MountainBreeze { public SMountainBreeze() : base(AID.SMountainBreeze) { } }

public abstract class C020Trash2 : BossModule
{
    public C020Trash2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(300, 0), 20, 40)) { }
}
