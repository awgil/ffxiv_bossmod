using System.Collections.Generic;

namespace BossMod.BLM
{
    public enum AID : uint
    {
        None = 0,

        // single target GCDs
        Blizzard1 = 142, // L1, 2.5s cast, range 25, single-target 0/0, targets=hostile
        Fire1 = 141, // L2, 2.5s cast, range 25, single-target 0/0, targets=hostile
        Thunder1 = 144, // L6, 2.5s cast, range 25, single-target 0/0, targets=hostile
        Scathe = 156, // L15, instant, range 25, single-target 0/0, targets=hostile
        Blizzard3 = 154, // L35, 3.5s cast, range 25, single-target 0/0, targets=hostile
        Fire3 = 152, // L35, 3.5s cast, range 25, single-target 0/0, targets=hostile
        Thunder3 = 153, // L45, 2.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Blizzard4 = 3576, // L58, 2.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Fire4 = 3577, // L60, 2.8s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Foul = 7422, // L70, 2.5s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
        Despair = 16505, // L72, 3.0s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Xenoglossy = 16507, // L80, instant, range 25, single-target 0/0, targets=hostile, animLock=???
        Paradox = 25797, // L90, 2.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???

        // aoe GCDs
        Blizzard2 = 25793, // L12, 3.0s cast, range 25, AOE circle 5/0, targets=hostile
        Fire2 = 147, // L18, 3.0s cast, range 25, AOE circle 5/0, targets=hostile
        Thunder2 = 7447, // L26, 2.5s cast, range 25, AOE circle 5/0, targets=hostile
        Freeze = 159, // L40, 2.8s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
        Flare = 162, // L50, 4.0s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
        Thunder4 = 7420, // L64, 2.5s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
        HighBlizzard2 = 25795, // L82, 3.0s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???
        HighFire2 = 25794, // L82, 3.0s cast, range 25, AOE circle 5/0, targets=hostile, animLock=???

        // oGCDs
        Transpose = 149, // L4, instant, 5.0s CD (group 0), range 0, single-target 0/0, targets=self
        Sharpcast = 3574, // L54, instant, 60.0s CD (group 19), range 0, single-target 0/0, targets=self, animLock=???

        // offsensive CDs
        Swiftcast = 7561, // L18, instant, 60.0s CD (group 42), range 0, single-target 0/0, targets=self
        Manafont = 158, // L30, instant, 180.0s CD (group 23), range 0, single-target 0/0, targets=self
        LeyLines = 3573, // L52, instant, 120.0s CD (group 14), range 0, Ground circle 3/0, targets=area, animLock=???
        Triplecast = 7421, // L66, instant, 60.0s CD (group 9) (2 charges), range 0, single-target 0/0, targets=self, animLock=???
        Amplifier = 25796, // L86, instant, 120.0s CD (group 20), range 0, single-target 0/0, targets=self, animLock=???
        LucidDreaming = 7562, // L14, instant, 60.0s CD (group 41), range 0, single-target 0/0, targets=self

        // defensive CDs
        Addle = 7560, // L8, instant, 90.0s CD (group 40), range 25, single-target 0/0, targets=hostile
        Manaward = 157, // L30, instant, 120.0s CD (group 21), range 0, single-target 0/0, targets=self
        Surecast = 7559, // L44, instant, 120.0s CD (group 43), range 0, single-target 0/0, targets=self, animLock=???

        // misc
        AetherialManipulation = 155, // L50, instant, 10.0s CD (group 4), range 25, single-target 0/0, targets=party, animLock=???
        BetweenTheLines = 7419, // L62, instant, 3.0s CD (group 1), range 25, Ground circle 0/0, targets=area, animLock=???
        UmbralSoul = 16506, // L76, instant, range 0, single-target 0/0, targets=self, animLock=???
        Sleep = 25880, // L10, 2.5s cast, range 30, AOE circle 5/0, targets=hostile
    }

    public enum CDGroup : int
    {
        Transpose = 0, // 5.0 max
        BetweenTheLines = 1, // 3.0 max
        AetherialManipulation = 4, // 10.0 max
        Triplecast = 9, // 2*60.0 max
        LeyLines = 14, // 120.0 max
        Sharpcast = 19, // 60.0 max
        Amplifier = 20, // 120.0 max
        Manaward = 21, // 120.0 max
        Manafont = 23, // 180.0 max
        Addle = 40, // 90.0 max
        LucidDreaming = 41, // 60.0 max
        Swiftcast = 42, // 60.0 max
        Surecast = 43, // 120.0 max
    }

    public enum MinLevel : int
    {
        // actions
        Fire1 = 2,
        Transpose = 4,
        Thunder1 = 6,
        Addle = 8,
        Sleep = 10,
        Blizzard2 = 12,
        LucidDreaming = 14,
        Scathe = 15, // unlocked by quest 65886 'The Threat of Superiority'
        Fire2 = 18,
        Swiftcast = 18,
        Thunder2 = 26,
        Manaward = 30, // unlocked by quest 65889 'Facing Your Demons'
        Manafont = 30, // unlocked by quest 66609 'Taking the Black'
        Fire3 = 35,
        Blizzard3 = 35, // unlocked by quest 66610 'You'll Never Go Back'
        Freeze = 40, // unlocked by quest 66611 'International Relations'
        Surecast = 44,
        Thunder3 = 45, // unlocked by quest 66612 'The Voidgate Breathes Gloomy'
        AetherialManipulation = 50,
        Flare = 50, // unlocked by quest 66614 'Always Bet on Black'
        LeyLines = 52, // unlocked by quest 67215 'An Unexpected Journey'
        Sharpcast = 54, // unlocked by quest 67216 'A Cunning Plan'
        Blizzard4 = 58, // unlocked by quest 67218 'Destruction in the Name of Justice'
        Fire4 = 60, // unlocked by quest 67219 'The Defiant Ones'
        BetweenTheLines = 62,
        Thunder4 = 64,
        Triplecast = 66,
        Foul = 70, // unlocked by quest 68128 'One Golem to Rule Them All'
        Despair = 72,
        UmbralSoul = 76,
        Xenoglossy = 80,
        HighFire2 = 82,
        HighBlizzard2 = 82,
        Amplifier = 86,
        Paradox = 90,

        // traits
        MaimAndMend1 = 20, // passive, damage increase
        AspectMastery2 = 20, // passive, second stack of astral fire / umbral ice
        Thundercloud = 28, // passive, thunder proc
        AspectMastery3 = 35, // passive, third stack of astral fire / umbral ice
        MaimAndMend2 = 40, // passive, damage increase
        Firestarter = 42, // passive, fire proc
        Enochian = 56, // unlocked by quest 67217 'Black Squawk Down'; passive, increased damage dealt under astral fire / umbral ice
        EnhancedFreeze = 58, // passive, freeze grants 3 umbral hearts
        EnhancedEnochian1 = 70, // unlocked by quest 68128 'One Golem to Rule Them All'; passive
        EnhancedSharpcast1 = 74, // passive, reduce cd
        EnhancedEnochian2 = 78, // passive
        EnhancedPolyglot = 80, // passive, second stack
        EnhancedFoul = 80, // passive
        EnhancedManafont = 84, // passive, reduce cd
        EnhancedEnochian3 = 86, // passive
        EnhancedSharpcast2 = 88, // passive, second charge
    }

    public static class Definitions
    {
        public static QuestLockEntry[] QuestsPerLevel = {
            new(15, 65886),
            new(30, 65889),
            new(30, 66609),
            new(35, 66610),
            new(40, 66611),
            new(45, 66612),
            new(50, 66614),
            new(52, 67215),
            new(54, 67216),
            new(56, 67217),
            new(58, 67218),
            new(60, 67219),
            new(70, 68128),
        };

        public static Dictionary<ActionID, ActionDefinition> SupportedActions;
        static Definitions()
        {
            SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionInt);
            SupportedActions.GCDCast(AID.Blizzard1, 25, 2.5f);
            SupportedActions.GCDCast(AID.Fire1, 25, 2.5f);
            SupportedActions.GCDCast(AID.Thunder1, 25, 2.5f); // note: animation lock is 0.1 if cast or 0.6 if instant (under thundercloud)
            SupportedActions.GCD(AID.Scathe, 25);
            SupportedActions.GCDCast(AID.Blizzard3, 25, 3.5f);
            SupportedActions.GCDCast(AID.Fire3, 25, 3.5f);
            SupportedActions.GCDCast(AID.Thunder3, 25, 2.5f);
            SupportedActions.GCDCast(AID.Blizzard4, 25, 2.5f);
            SupportedActions.GCDCast(AID.Fire4, 25, 2.8f);
            SupportedActions.GCDCast(AID.Foul, 25, 2.5f);
            SupportedActions.GCDCast(AID.Despair, 25, 3.0f);
            SupportedActions.GCD(AID.Xenoglossy, 25);
            SupportedActions.GCDCast(AID.Paradox, 25, 2.5f);
            SupportedActions.GCDCast(AID.Blizzard2, 25, 3.0f);
            SupportedActions.GCDCast(AID.Fire2, 25, 3.0f);
            SupportedActions.GCDCast(AID.Thunder2, 25, 2.5f);
            SupportedActions.GCDCast(AID.Freeze, 25, 2.8f);
            SupportedActions.GCDCast(AID.Flare, 25, 4.0f);
            SupportedActions.GCDCast(AID.Thunder4, 25, 2.5f);
            SupportedActions.GCDCast(AID.HighBlizzard2, 25, 3.0f);
            SupportedActions.GCDCast(AID.HighFire2, 25, 3.0f);
            SupportedActions.OGCD(AID.Transpose, 0, CDGroup.Transpose, 5.0f);
            SupportedActions.OGCD(AID.Sharpcast, 0, CDGroup.Sharpcast, 60.0f);
            SupportedActions.OGCD(AID.Swiftcast, 0, CDGroup.Swiftcast, 60.0f);
            SupportedActions.OGCD(AID.Manafont, 0, CDGroup.Manafont, 180.0f);
            SupportedActions.OGCD(AID.LeyLines, 0, CDGroup.LeyLines, 120.0f);
            SupportedActions.OGCDWithCharges(AID.Triplecast, 0, CDGroup.Triplecast, 60.0f, 2);
            SupportedActions.OGCD(AID.Amplifier, 0, CDGroup.Amplifier, 120.0f);
            SupportedActions.OGCD(AID.LucidDreaming, 0, CDGroup.LucidDreaming, 60.0f);
            SupportedActions.OGCD(AID.Addle, 25, CDGroup.Addle, 90.0f);
            SupportedActions.OGCD(AID.Manaward, 0, CDGroup.Manaward, 120.0f);
            SupportedActions.OGCD(AID.Surecast, 0, CDGroup.Surecast, 120.0f);
            SupportedActions.OGCD(AID.AetherialManipulation, 25, CDGroup.AetherialManipulation, 10.0f);
            SupportedActions.OGCD(AID.BetweenTheLines, 25, CDGroup.BetweenTheLines, 3.0f);
            SupportedActions.GCD(AID.UmbralSoul, 0);
            SupportedActions.GCDCast(AID.Sleep, 30, 2.5f);
        }
    }

    public enum SID : uint
    {
        None = 0,
        Thunder1 = 161, // applied by Thunder1 to target, dot
        Thunder2 = 162, // applied by Thunder2 to target, dot
        Thundercloud = 164, // proc
        Firestarter = 165, // applied by Fire to self, next fire3 is free and instant
        Addle = 1203, // applied by Addle to target, -5% phys and -10% magic damage dealt
        LucidDreaming = 1204, // applied by Lucid Dreaming to self, MP restore
        Swiftcast = 167, // applied by Swiftcast to self, next cast is instant
        Sleep = 3, // applied by Sleep to target
    }
}
