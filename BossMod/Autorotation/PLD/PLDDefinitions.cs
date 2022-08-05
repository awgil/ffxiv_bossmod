using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.PLD
{
    public enum AID : uint
    {
        None = 0,

        // single target GCDs
        FastBlade = 9, // L1, instant, range 3, single-target 0/0, targets=hostile
        RiotBlade = 15, // L4, instant, range 3, single-target 0/0, targets=hostile
        RageOfHalone = 21, // L26, instant, range 3, single-target 0/0, targets=hostile
        GoringBlade = 3538, // L54, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        RoyalAuthority = 3539, // L60, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        HolySpirit = 7384, // L64, 1.5s cast, range 25, single-target 0/0, targets=hostile, animLock=???
        Atonement = 16460, // L76, instant, range 3, single-target 0/0, targets=hostile, animLock=???
        Confiteor = 16459, // L80, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
        BladeOfFaith = 25748, // L90, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
        BladeOfTruth = 25749, // L90, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???
        BladeOfValor = 25750, // L90, instant, range 25, AOE circle 5/0, targets=hostile, animLock=???

        // aoe GCDs
        TotalEclipse = 7381, // L6, instant, range 0, AOE circle 5/0, targets=self
        Prominence = 16457, // L40, instant, range 0, AOE circle 5/0, targets=self, animLock=???
        HolyCircle = 16458, // L72, 1.5s cast, range 0, AOE circle 5/0, targets=self, animLock=???

        // oGCDs
        SpiritsWithin = 29, // L30, instant, 30.0s CD (group 5), range 3, single-target 0/0, targets=hostile, animLock=???
        CircleOfScorn = 23, // L50, instant, 30.0s CD (group 4), range 0, AOE circle 5/0, targets=self, animLock=???
        Intervene = 16461, // L74, instant, 30.0s CD (group 9) (2 charges), range 20, single-target 0/0, targets=hostile, animLock=???
        Expiacion = 25747, // L86, instant, 30.0s CD (group 5), range 3, AOE circle 5/0, targets=hostile, animLock=???

        // offensive CDs
        FightOrFlight = 20, // L2, instant, 60.0s CD (group 10), range 0, single-target 0/0, targets=self
        Requiescat = 7383, // L68, instant, 60.0s CD (group 11), range 3, single-target 0/0, targets=hostile, animLock=???

        // defensive CDs
        Rampart = 7531, // L8, instant, 90.0s CD (group 40), range 0, single-target 0/0, targets=self
        Sheltron = 3542, // L35, instant, 5.0s CD (group 0), range 0, single-target 0/0, targets=self, animLock=???
        Sentinel = 17, // L38, instant, 120.0s CD (group 19), range 0, single-target 0/0, targets=self, animLock=???
        Cover = 27, // L45, instant, 120.0s CD (group 20), range 10, single-target 0/0, targets=party, animLock=???
        HolySheltron = 25746, // L82, instant, 5.0s CD (group 2), range 0, single-target 0/0, targets=self, animLock=???
        HallowedGround = 30, // L50, instant, 420.0s CD (group 24), range 0, single-target 0/0, targets=self, animLock=???
        Reprisal = 7535, // L22, instant, 60.0s CD (group 43), range 0, AOE circle 5/0, targets=self
        PassageOfArms = 7385, // L70, instant, 120.0s CD (group 21), range 0, Ground circle 8/0, targets=self, animLock=???
        DivineVeil = 3540, // L56, instant, 90.0s CD (group 14), range 0, single-target 0/0, targets=self, animLock=???
        Intervention = 7382, // L62, instant, 10.0s CD (group 1), range 30, single-target 0/0, targets=party, animLock=???
        ArmsLength = 7548, // L32, instant, 120.0s CD (group 46), range 0, single-target 0/0, targets=self, animLock=???

        // misc
        Clemency = 3541, // L58, 1.5s cast, range 30, single-target 0/0, targets=self/party/friendly, animLock=???
        ShieldBash = 16, // L10, instant, range 3, single-target 0/0, targets=hostile
        ShieldLob = 24, // L15, instant, range 20, single-target 0/0, targets=hostile
        IronWill = 28, // L10, instant, 3.0s CD (group 3), range 0, single-target 0/0, targets=self
        Provoke = 7533, // L15, instant, 30.0s CD (group 42), range 25, single-target 0/0, targets=hostile
        Shirk = 7537, // L48, instant, 120.0s CD (group 45), range 25, single-target 0/0, targets=party, animLock=???
        LowBlow = 7540, // L12, instant, 25.0s CD (group 41), range 3, single-target 0/0, targets=hostile
        Interject = 7538, // L18, instant, 30.0s CD (group 44), range 3, single-target 0/0, targets=hostile
    }
}
