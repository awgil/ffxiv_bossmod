namespace BossMod.WAR;

public enum CDGroup : int
{
    Defiance = 2, // variable max, shared by Defiance, Release Defiance
    Bloodwhetting = 3, // 25.0 max, shared by Raw Intuition, Nascent Flash, Bloodwhetting
    Upheaval = 5, // 30.0 max, shared by Upheaval, Orogeny
    Onslaught = 9, // 3*30.0 max
    Berserk = 10, // 60.0 max
    InnerRelease = 11, // 60.0 max
    Equilibrium = 14, // 60.0 max
    ThrillOfBattle = 16, // 90.0 max
    ShakeItOff = 17, // 90.0 max
    Infuriate = 19, // 2*60.0 max
    Vengeance = 21, // 120.0 max
    Holmgang = 23, // 240.0 max
    LowBlow = 41, // 25.0 max
    Provoke = 42, // 30.0 max
    Interject = 43, // 60.0 max
    Reprisal = 44, // 30.0 max
    Rampart = 46, // 90.0 max
    ArmsLength = 48, // 120.0 max
    Shirk = 49, // 120.0 max
    LimitBreak = 71, // special/fake (TODO: remove need for it?)
}

public enum SID : uint
{
    None = 0,
    SurgingTempest = 2677, // applied by Storm's Eye, Mythril Tempest to self, damage buff
    NascentChaos = 1897, // applied by Infuriate to self, converts next FC to IC
    Berserk = 86, // applied by Berserk to self, next 3 GCDs are crit dhit
    InnerRelease = 1177, // applied by Inner Release to self, next 3 GCDs should be free FCs
    PrimalRend = 2624, // applied by Inner Release to self, allows casting PR
    InnerStrength = 2663, // applied by Inner Release to self, immunes
    VengeanceRetaliation = 89, // applied by Vengeance to self, retaliation for physical attacks
    VengeanceDefense = 912, // applied by Vengeance to self, -30% damage taken
    Rampart = 1191, // applied by Rampart to self, -20% damage taken
    ThrillOfBattle = 87, // applied by Thrill of Battle to self
    Holmgang = 409, // applied by Holmgang to self
    EquilibriumRegen = 2681, // applied by Equilibrium to self, hp regen
    Reprisal = 1193, // applied by Reprisal to target
    ShakeItOff = 1457, // applied by ShakeItOff to self/target, damage shield
    RawIntuition = 735, // applied by Raw Intuition to self
    NascentFlashSelf = 1857, // applied by Nascent Flash to self, heal on hit
    NascentFlashTarget = 1858, // applied by Nascent Flash to target, -10% damage taken + heal on hit
    BloodwhettingDefenseLong = 2678, // applied by Bloodwhetting to self, -10% damage taken + heal on hit for 8 sec
    BloodwhettingDefenseShort = 2679, // applied by Bloodwhetting, Nascent Flash to self/target, -10% damage taken for 4 sec
    BloodwhettingShield = 2680, // applied by Bloodwhetting, Nascent Flash to self/target, damage shield
    ArmsLength = 1209, // applied by Arm's Length to self
    Defiance = 91, // applied by Defiance to self, tank stance
    Stun = 2, // applied by Low Blow to target
}
