namespace BossMod.RealmReborn.Alliance.A22Glasya;
public enum OID : uint
{
    Boss = 0xBD7, // R4.000, x?
    GlasyaLabolasHelper = 0x1B2, // R0.500, x?, 523 type
    ClockworkWright = 0xBD9, // R0.600, x?
}

public enum AID : uint
{
    AutoAttackk = 872, // Boss->player, no cast, single-target
    AstralPunch = 2346, // Boss->self, no cast, range 8+R 90-degree cone
    Aura = 2348, // GlasyaLabolasHelper->location, 3.5s cast, range 8 circle
    VileUtterance = 2353, // Boss->self, 3.0s cast, range 60+R ?-degree cone
    BloodMoon = 2349, // Boss->self, no cast, range 60 circle
}

public enum SID : uint
{
    Invincibility = 1570, // none->player, extra=0x0
    Fetters = 510, // none->player, extra=0x0
    Electrocution = 288, // none->player, extra=0x0
}

public enum TetherID : uint
{
    Tether_22 = 22, // ClockworkWright->Boss
}
