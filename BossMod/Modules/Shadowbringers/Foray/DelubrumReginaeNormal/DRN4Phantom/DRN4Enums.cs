namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN4Phantom;

public enum OID : uint
{
    Boss = 0x30AC, // R2.400, x?
    BozjanPhantom = 0x233C, // R0.500, x?, 523 type
    StuffyWraith = 0x30AD, // R2.200, x?
    MiasmaLowDonut = 0x1EB0DF, // R0.500, x0 (spawn during fight), EventObj type
    MiasmaLowRect = 0x1EB0DD, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    WeaveMiasma = 22435, // Boss->self, 3.0s cast, single-target, visual (create miasma markers)
    ManipulateMiasma = 22436, // Boss->self, 7.0s cast, single-target, single-target, visual (low -> high)
    SwirlingMiasmaFirst = 22441, // BozjanPhantom->location, 8.0s cast, range 5-19 donut
    SwirlingMiasmaRest = 22442, // BozjanPhantom->location, 1.0s cast, range 5-19 donut
    CreepingMiasmaFirst = 22437, // BozjanPhantom->self, 12.0s cast, range 50 width 12 rect
    CreepingMiasmaRest = 22438, // BozjanPhantom->self, 1.0s cast, range 50 width 12 rect
    MaledictionOfAgony = 22447, // Boss->self, 4.0s cast, range 70 circle
    Summon = 22443, // Boss->self, 3.0s cast, single-target, visual (go untargetable and spawn adds)
    UndyingHatred = 22444, // StuffyWraith->self, 6.0s cast, range 60 width 48 rect knockback
    Transference = 22445, // Boss->location, no cast, single-target, teleport
    VileWave = 22449, // Boss->self, 6.0s cast, range 45 120-degree cone
}

public enum SID : uint
{
    TwiceComeRuin = 2485, // BozjanPhantom->player, extra=0x1
}