namespace BossMod.BRD;

public enum CDGroup : int
{
    PitchPerfect = 0, // 1.0 max
    EmpyrealArrow = 2, // 15.0 max
    Bloodletter = 4, // 3*15.0 max, shared by Bloodletter, Rain of Death
    RepellingShot = 5, // 30.0 max
    WardensPaean = 8, // 45.0 max
    Sidewinder = 12, // 60.0 max
    RagingStrikes = 13, // 120.0 max
    NaturesMinne = 14, // 120.0 max
    RadiantFinale = 15, // 110.0 max
    ArmysPaeon = 16, // 120.0 max
    WanderersMinuet = 17, // 120.0 max
    Barrage = 19, // 120.0 max
    MagesBallad = 20, // 120.0 max
    BattleVoice = 22, // 120.0 max
    Troubadour = 23, // 120.0 max
    Peloton = 40, // 5.0 max
    FootGraze = 41, // 30.0 max
    LegGraze = 42, // 30.0 max
    HeadGraze = 43, // 30.0 max
    ArmsLength = 48, // 120.0 max
    SecondWind = 49, // 120.0 max
}

public enum SID : uint
{
    None = 0,
    StraightShotReady = 122, // procced or applied by Barrage to self
    VenomousBite = 124, // applied by Venomous Bite, dot
    Windbite = 129, // applied by Windbite, dot
    CausticBite = 1200, // applied by Caustic Bite, Iron Jaws to target, dot
    Stormbite = 1201, // applied by Stormbite, Iron Jaws to target, dot
    RagingStrikes = 125, // applied by Raging Strikes to self, damage buff
    Barrage = 128, // applied by Barrage to self
    Peloton = 1199,
    ShadowbiteReady = 3002, // applied by Ladonsbite to self
    NaturesMinne = 1202, // applied by Nature's Minne to self
    WardensPaean = 866, // applied by the Warden's Paean to self
    BattleVoice = 141, // applied by Battle Voice to self
    Troubadour = 1934, // applied by Troubadour to self
    ArmsLength = 1209, // applied by Arm's Length to self
    Bind = 13, // applied by Foot Graze to target
    BlastArrowReady = 2692, // applied by Apex Arrow to self
    RadiantFinale = 2964, // applied by Radiant Finale to self. damage up
    RadiantFinaleVisual = 2722, // applied by Radiant Finale to self, visual effect
    ArmysMuse = 1932, // applied when leaving army's paeon
}
