namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN3Necrophobia;

public enum OID : uint
{
    Necrophobia = 0x4BE5, // R5.001, x1
    Necrophobia2 = 0x4BE9, // R1.000, x1
    NecrophobiaHelper = 0x233C, // R0.500, x25, Helper type

    SeveringHead = 0x4BE6, // R1.410, x8

    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
    Actor1ebfaa = 0x1EBFAA, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 47451, // Necrophobia->player, no cast, single-target

    AncientBlizzardIII = 47456, // Necrophobia->self, 5.0s cast, range 45 width 15 cross
    AncientBlizzardIII1 = 47469, // SeveringHead->self, 5.5s cast, range 45 width 15 cross

    AncientFireIII = 47455, // Necrophobia->self, 5.0s cast, range 18 circle
    AncientFireIII1 = 47468, // SeveringHead->self, 5.5s cast, range 18 circle

    AncientThunderIII = 47457, // Necrophobia->self, 4.2+0.8s cast, single-target
    AncientThunderIII1 = 47458, // NecrophobiaHelper->self, 5.0s cast, range 60 45.000-degree cone
    AncientThunderIII2 = 47470, // SeveringHead->self, 4.7+0.8s cast, single-target
    AncientThunderIII3 = 47471, // NecrophobiaHelper->self, 5.5s cast, range 60 45.000-degree cone

    Capitation = 47460, // Necrophobia->self, no cast, single-target
    CorpseMangler = 47459, // Necrophobia->player, 5.0s cast, single-target

    DarkCurrent = 47476, // Necrophobia->self, 4.2+1.3s cast, single-target
    DarkCurrent1 = 47477, // NecrophobiaHelper->self, 5.5s cast, range 60 width 10 rect
    DarkCurrent2 = 47478, // NecrophobiaHelper->self, 1.0s cast, range 10 width 60 rect

    DeathlyRay = 47475, // SeveringHead->self, 5.0s cast, range 30 width 6 rect
    DeathShroud = 47461, // Necrophobia->self, 7.0s cast, single-target

    HailOfHellflares = 47452, // Necrophobia->self, 5.0s cast, single-target
    HailOfHellflares1 = 47453, // NecrophobiaHelper->self, no cast, ???
    HailOfHellflares2 = 48956, // NecrophobiaHelper->self, no cast, single-target
    HailOfHellflares3 = 48957, // NecrophobiaHelper->self, no cast, ???

    HeadsRoll = 47463, // Necrophobia->self, 3.0s cast, single-target
    HeadsRoll1 = 47474, // Necrophobia->self, no cast, single-target

    SeveredBlizzardIII = 47466, // Necrophobia->self, 5.5s cast, range 45 width 15 cross
    SeveredDarkCurrent = 47479, // Necrophobia->self, 4.2+1.3s cast, single-target
    SeveredFireIII = 47465, // Necrophobia->self, 5.5s cast, range 18 circle

    UnknownAbility1 = 47454, // Necrophobia2->self, no cast, range ?-30 donut
    UnknownAbility2 = 47450, // Necrophobia->location, no cast, single-target
    UnknownAbility3 = 47462, // SeveringHead->location, no cast, single-target
    HeadsRollMove = 47464, // SeveringHead->location, no cast, single-target, heads moving after Heads Roll
    UnknownAbility5 = 47472, // SeveringHead->location, no cast, single-target

    VacuumWave = 47473, // Necrophobia->self, 4.0s cast, range 30 180.000-degree cone
}
public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    UnknownStatus1 = 2552, // none->Necrophobia, extra=0x45A/0x45B/0x45C, 0x45A = fire, 0x45B = ice, 0x45C = lightning, happens 0.1s before severed fire/ice cast starts
    VulnerabilityUp = 2347, // Necrophobia/SeveringHead/NecrophobiaHelper->player, extra=0x1/0x2/0x3
    UnknownStatus2 = 4956, // none->SeveringHead, extra=0x2C4
    UnknownStatus3 = 3558, // none->SeveringHead, extra=0x47C/0x47D/0x47E, 0x47C = fire, 0x47D = ice, 0x47E = lightning

}
public enum IconID : uint
{
    Icon_tank_lockon02k1 = 218, // player->self
}
public enum TetherID : uint
{
    Tether_chn_m0475_mr_c0x = 400, // SeveringHead->Necrophobia, fire
    Tether_chn_m0475_mr_c1x = 401, // SeveringHead->Necrophobia, ice
    Tether_chn_m0475_mr_c2x = 402, // SeveringHead->Necrophobia, lightning
}
