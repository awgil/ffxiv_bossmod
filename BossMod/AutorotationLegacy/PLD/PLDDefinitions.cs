namespace BossMod.PLD;

public enum CDGroup : int
{
    Sheltron = 0, // 5.0 max
    Intervention = 1, // 10.0 max
    HolySheltron = 2, // 5.0 max
    IronWill = 3, // variable max, shared by Iron Will, Release Iron Will
    CircleOfScorn = 4, // 30.0 max
    SpiritsWithin = 5, // 30.0 max, shared by Spirits Within, Expiacion
    Intervene = 9, // 2*30.0 max
    FightOrFlight = 10, // 60.0 max
    Requiescat = 11, // 60.0 max
    GoringBlade = 12, // 60.0 max
    DivineVeil = 14, // 90.0 max
    Bulwark = 15, // 90.0 max
    Sentinel = 19, // 120.0 max
    Cover = 20, // 120.0 max
    PassageOfArms = 21, // 120.0 max
    HallowedGround = 24, // 420.0 max
    LowBlow = 41, // 25.0 max
    Provoke = 42, // 30.0 max
    Interject = 43, // 30.0 max
    Reprisal = 44, // 60.0 max
    Rampart = 46, // 90.0 max
    ArmsLength = 48, // 120.0 max
    Shirk = 49, // 120.0 max
}

public enum SID : uint
{
    None = 0,
    FightOrFlight = 76, // applied by Fight or Flight to self, +25% physical damage dealt buff
    CircleOfScorn = 248, // applied by Circle of Scorn to target, dot
    Rampart = 1191, // applied by Rampart to self, -20% damage taken
    Reprisal = 1193, // applied by Reprisal to target
    HallowedGround = 82, // applied by Hallowed Ground to self, immune
    IronWill = 79, // applied by Iron Will to self, tank stance
    Stun = 2, // applied by Low Blow, Shield Bash to target
}
