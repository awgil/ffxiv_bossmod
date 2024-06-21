namespace BossMod.BRD;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    SagittariusArrow = 4244, // LB3, 4.5s cast, range 30, AOE 30+R width 8 rect, targets=hostile, castAnimLock=3.700
    HeavyShot = 97, // L1, instant, GCD, range 25, single-target, targets=hostile
    StraightShot = 98, // L2, instant, GCD, range 25, single-target, targets=hostile
    RagingStrikes = 101, // L4, instant, 120.0s CD (group 13), range 0, single-target, targets=self
    VenomousBite = 100, // L6, instant, GCD, range 25, single-target, targets=hostile
    Bloodletter = 110, // L12, instant, 15.0s CD (group 4/70) (2? charges), range 25, single-target, targets=hostile
    RepellingShot = 112, // L15, instant, 30.0s CD (group 5), range 15, single-target, targets=hostile, animLock=0.800
    QuickNock = 106, // L18, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=hostile
    MagesBallad = 114, // L30, instant, 120.0s CD (group 20), range 25, single-target, targets=hostile
    Windbite = 113, // L30, instant, GCD, range 25, single-target, targets=hostile
    WardensPaean = 3561, // L35, instant, 45.0s CD (group 8), range 30, single-target, targets=self/party
    Barrage = 107, // L38, instant, 120.0s CD (group 19), range 0, single-target, targets=self
    ArmysPaeon = 116, // L40, instant, 120.0s CD (group 16), range 25, single-target, targets=hostile
    RainOfDeath = 117, // L45, instant, 15.0s CD (group 4/70) (2? charges), range 25, AOE 8 circle, targets=hostile
    BattleVoice = 118, // L50, instant, 120.0s CD (group 22), range 0, AOE 30 circle, targets=self
    WanderersMinuet = 3559, // L52, instant, 120.0s CD (group 17), range 25, single-target, targets=hostile
    PitchPerfect = 7404, // L52, instant, 1.0s CD (group 0), range 25, single-target, targets=hostile
    EmpyrealArrow = 3558, // L54, instant, 15.0s CD (group 2), range 25, single-target, targets=hostile
    IronJaws = 3560, // L56, instant, GCD, range 25, single-target, targets=hostile
    Sidewinder = 3562, // L60, instant, 60.0s CD (group 12), range 25, single-target, targets=hostile
    Troubadour = 7405, // L62, instant, 120.0s CD (group 23), range 0, AOE 30 circle, targets=self
    CausticBite = 7406, // L64, instant, GCD, range 25, single-target, targets=hostile
    Stormbite = 7407, // L64, instant, GCD, range 25, single-target, targets=hostile
    NaturesMinne = 7408, // L66, instant, 120.0s CD (group 14), range 0, AOE 30 circle, targets=self
    RefulgentArrow = 7409, // L70, instant, GCD, range 25, single-target, targets=hostile
    Shadowbite = 16494, // L72, instant, GCD, range 25, AOE 5 circle, targets=hostile
    BurstShot = 16495, // L76, instant, GCD, range 25, single-target, targets=hostile
    ApexArrow = 16496, // L80, instant, GCD, range 25, AOE 25+R width 4 rect, targets=hostile
    Ladonsbite = 25783, // L82, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=hostile
    BlastArrow = 25784, // L86, instant, GCD, range 25, AOE 25+R width 4 rect, targets=hostile
    RadiantFinale = 25785, // L90, instant, 110.0s CD (group 15), range 0, AOE 30 circle, targets=self

    // Shared
    BigShot = ClassShared.AID.BigShot, // LB1, 2.0s cast, range 30, AOE 30+R width 4 rect, targets=hostile, castAnimLock=3.100
    Desperado = ClassShared.AID.Desperado, // LB2, 3.0s cast, range 30, AOE 30+R width 5 rect, targets=hostile, castAnimLock=3.100
    LegGraze = ClassShared.AID.LegGraze, // L6, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=self
    FootGraze = ClassShared.AID.FootGraze, // L10, instant, 30.0s CD (group 41), range 25, single-target, targets=hostile
    Peloton = ClassShared.AID.Peloton, // L20, instant, 5.0s CD (group 40), range 0, AOE 30 circle, targets=self
    HeadGraze = ClassShared.AID.HeadGraze, // L24, instant, 30.0s CD (group 43), range 25, single-target, targets=hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
}

public enum TraitID : uint
{
    None = 0,
    HeavierShot = 17, // L2, straight shot proc on heavy shot
    IncreasedActionDamage1 = 18, // L20, damage increase
    IncreasedActionDamage2 = 20, // L40, damage increase
    BiteMastery1 = 168, // L64, dot upgrade
    EnhancedEmpyrealArrow = 169, // L68, empyreal arrow triggers repertoire
    StraightShotMastery = 282, // L70, straight shot -> refulgent arrow upgrade
    EnhancedQuickNock = 283, // L72, shadowbite proc on quick nock
    BiteMastery2 = 284, // L76, straight shot proc on dot
    HeavyShotMastery = 285, // L76, heavy shot -> burst shot upgrade
    EnhancedArmysPaeon = 286, // L78, army muse effect
    SoulVoice = 287, // L80, gauge unlock
    QuickNockMastery = 444, // L82, quck nock -> ladonsbite upgrade
    EnhancedBloodletter = 445, // L84, third charge
    EnhancedApexArrow = 446, // L86, blast arrow proc
    EnhancedTroubadour = 447, // L88, reduce cd
    MinstrelsCoda = 448, // L90, radiant finale mechanics
}

public enum SID : uint
{
    None = 0,
    StraightShotReady = 122, // applied by Barrage, Iron Jaws, Caustic Bite, Stormbite, Burst Shot to self
    VenomousBite = 124, // applied by Venomous Bite, dot
    Windbite = 129, // applied by Windbite, dot
    CausticBite = 1200, // applied by Caustic Bite, Iron Jaws to target, dot
    Stormbite = 1201, // applied by Stormbite, Iron Jaws to target, dot
    RagingStrikes = 125, // applied by Raging Strikes to self, damage buff
    Barrage = 128, // applied by Barrage to self
    Peloton = 1199, // applied by Peloton to self/target
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

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.SagittariusArrow, true, castAnimLock: 3.70f);
        d.RegisterSpell(AID.HeavyShot, true);
        d.RegisterSpell(AID.StraightShot, true);
        d.RegisterSpell(AID.RagingStrikes, true);
        d.RegisterSpell(AID.VenomousBite, true);
        d.RegisterSpell(AID.Bloodletter, true, maxCharges: 3);
        d.RegisterSpell(AID.RepellingShot, true, instantAnimLock: 0.80f);
        d.RegisterSpell(AID.QuickNock, true);
        d.RegisterSpell(AID.MagesBallad, true);
        d.RegisterSpell(AID.Windbite, true);
        d.RegisterSpell(AID.WardensPaean, true);
        d.RegisterSpell(AID.Barrage, true);
        d.RegisterSpell(AID.ArmysPaeon, true);
        d.RegisterSpell(AID.RainOfDeath, true, maxCharges: 3);
        d.RegisterSpell(AID.BattleVoice, true);
        d.RegisterSpell(AID.WanderersMinuet, true);
        d.RegisterSpell(AID.PitchPerfect, true);
        d.RegisterSpell(AID.EmpyrealArrow, true);
        d.RegisterSpell(AID.IronJaws, true);
        d.RegisterSpell(AID.Sidewinder, true);
        d.RegisterSpell(AID.Troubadour, true);
        d.RegisterSpell(AID.CausticBite, true);
        d.RegisterSpell(AID.Stormbite, true);
        d.RegisterSpell(AID.NaturesMinne, true);
        d.RegisterSpell(AID.RefulgentArrow, true);
        d.RegisterSpell(AID.Shadowbite, true);
        d.RegisterSpell(AID.BurstShot, true);
        d.RegisterSpell(AID.ApexArrow, true);
        d.RegisterSpell(AID.Ladonsbite, true);
        d.RegisterSpell(AID.BlastArrow, true);
        d.RegisterSpell(AID.RadiantFinale, true);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // smart targets
        d.Spell(AID.WardensPaean)!.SmartTarget = ActionDefinitions.SmartTargetCoTank;

        // upgrades/button replacement (TODO: don't think we actually care...)
        //d.Spell(AID.HeavyShot)!.TransformAction = d.Spell(AID.BurstShot)!.TransformAction = () => ActionID.MakeSpell(_state.BestBurstShot);
        //d.Spell(AID.StraightShot)!.TransformAction = d.Spell(AID.RefulgentArrow)!.TransformAction = () => ActionID.MakeSpell(_state.BestRefulgentArrow);
        //d.Spell(AID.VenomousBite)!.TransformAction = d.Spell(AID.CausticBite)!.TransformAction = () => ActionID.MakeSpell(_state.BestCausticBite);
        //d.Spell(AID.Windbite)!.TransformAction = d.Spell(AID.Stormbite)!.TransformAction = () => ActionID.MakeSpell(_state.BestStormbite);
        //d.Spell(AID.QuickNock)!.TransformAction = d.Spell(AID.Ladonsbite)!.TransformAction = () => ActionID.MakeSpell(_state.BestLadonsbite);
        //d.Spell(AID.WanderersMinuet)!.TransformAction = d.Spell(AID.PitchPerfect)!.TransformAction = () => ActionID.MakeSpell(_state.ActiveSong == Rotation.Song.WanderersMinuet ? AID.PitchPerfect : AID.WanderersMinuet);
    }
}
