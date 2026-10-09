using BossMod.VPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageVPR(RotationModuleManager manager, Actor player) : KageRotation<KageVPR.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Burst (Serpent's Ire, Reawaken)", InternalName = "Burst", MinLevel = 86)]
        public Track<OffensiveStrategy> Burst;

        [Track("Reawaken", MinLevel = 90, Actions = [AID.Reawaken, AID.FirstGeneration, AID.SecondGeneration, AID.ThirdGeneration, AID.FourthGeneration, AID.Ouroboros])]
        public Track<ReawakenStrategy> Reawaken;

        [Track("Serpent's Ire", MinLevel = 86, Action = AID.SerpentsIre)]
        public Track<OffensiveStrategy> Ire;

        [Track("Vicewinder / Vicepit", MinLevel = 65, Actions = [AID.Vicewinder, AID.Vicepit])]
        public Track<OffensiveStrategy> Twinblade;

        [Track("Uncoiled Fury", MinLevel = 82, Action = AID.UncoiledFury)]
        public Track<UncoiledStrategy> Uncoiled;

        [Track("Writhing Snap", MinLevel = 15, Action = AID.WrithingSnap)]
        public Track<SnapStrategy> Snap;

        [Track("True North", MinLevel = 50, Action = ClassShared.AID.TrueNorth)]
        public Track<TrueNorthStrategy> TrueNorth;

        [Track("Slither", MinLevel = 40, Action = AID.Slither)]
        public Track<SlitherStrategy> Slither;

        [Track("Engage")]
        public Track<EngageStrategy> Engage;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 96, UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum ReawakenStrategy
    {
        [Option("Use with Serpent's Ire and raid buffs; saved for the boss")]
        Automatic,
        [Option("Use as soon as possible", MinLevel = 90)]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum UncoiledStrategy
    {
        [Option("Spend between combos, keep one for movement; never overcap", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Spend between combos, but always keep one for movement", Targets = ActionTargets.Hostile)]
        HoldOne,
        [Option("Only out of melee range or to avoid overcapping", Targets = ActionTargets.Hostile)]
        Overcap,
        [Option("Do not use")]
        Delay
    }

    public enum SnapStrategy
    {
        [Option("Use when out of melee range with no Rattling Coils", Targets = ActionTargets.Hostile)]
        Ranged,
        [Option("Do not use")]
        Never
    }

    public enum TrueNorthStrategy
    {
        [Option("Use when the next positional is wrong")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum SlitherStrategy
    {
        [Option("Do not use")]
        None,
        [Option("Use when outside melee range", Targets = ActionTargets.Hostile)]
        GapClose
    }

    public enum EngageStrategy
    {
        [Option("Slither in, or open at the pull if already in melee")]
        Slither,
        [Option("Sprint into melee range")]
        Sprint,
        [Option("Walk in and open at the pull")]
        Facepull
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use before the pull and with Serpent's Ire")]
        AlignWithBurst,
        [Option("Use before the pull and with raid buffs")]
        AlignWithRaidBuffs,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Automatic (FRU / DMU openers in those fights)")]
        Automatic,
        [Option("Standard opener")]
        Standard,
        [Option("Vicewinder first, Swiftskin's Coil first (FRU)")]
        FRU,
        [Option("Vicewinder first, Hunter's Coil first (DMU)")]
        DMU,
        [Option("No opener-specific rules")]
        None
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage VPR", "Viper", "Standard rotation (Kage)|Melee", "Kagekazu", RotationModuleQuality.Ok, BitMask.Build(Class.VPR), 100).WithStrategies<Strategy>();
    }

    private DreadCombo Dread;
    private int Coil;
    private int Offering;
    private int Anguine;
    private AID CurSerpentsTail;

    private float Swiftscaled;
    private float Instinct;
    private float HonedReavers;
    private float HonedSteel;
    private float FlankstungVenom;
    private float FlanksbaneVenom;
    private float HindstungVenom;
    private float HindsbaneVenom;
    private float GrimskinsVenom;
    private float HuntersVenom;
    private float SwiftskinsVenom;
    private float FellhuntersVenom;
    private float FellskinsVenom;
    private float PoisedForTwinfang;
    private float PoisedForTwinblood;
    private float ReawakenReady;
    private float Reawakened;

    private bool AllowAoE;
    private bool AOEMode;
    private bool InMelee;

    private OpenerStrategy OpenerMode;
    private bool IreWanted;

    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private AID NextGCD => NextGCDAction.As<AID>();
    private int CoilMax => Unlocked(TraitID.EnhancedVipersRattle) ? 3 : 2;
    private int MaxAnguine => Unlocked(TraitID.EnhancedSerpentsLineage) ? 5 : 4;
    private bool BuffsExpiring(float within)
        => IsExpiring(FlankstungVenom, within) || IsExpiring(FlanksbaneVenom, within) || IsExpiring(HindstungVenom, within) || IsExpiring(HindsbaneVenom, within)
        || IsExpiring(HonedSteel, within) || IsExpiring(HonedReavers, within);
    private static bool IsExpiring(float left, float within) => left > 0 && left < within;

    private bool TwinWeavesPending => Unlocked(AID.TwinfangBite) && (HuntersVenom > 0 || SwiftskinsVenom > 0 || FellhuntersVenom > 0 || FellskinsVenom > 0 || PoisedForTwinfang > 0 || PoisedForTwinblood > 0);
    private float IreIn => ReadyIn(AID.SerpentsIre);
    private bool VicewinderOpener => OpenerMode is OpenerStrategy.FRU or OpenerStrategy.DMU;

    private ReawakenStrategy ReawakenStrat;
    private OffensiveStrategy IreStrat;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 3, strategy.AOE.Value);
        ReawakenStrat = strategy.Burst.Value switch { OffensiveStrategy.Delay => ReawakenStrategy.Delay, OffensiveStrategy.Force => ReawakenStrategy.ASAP, _ => strategy.Reawaken.Value };
        IreStrat = WithBurst(strategy.Burst.Value, strategy.Ire.Value);
        var forceST = strategy.AOE.Value == AOEStrategy.ForceST;

        OpenerMode = !Unlocked(TraitID.EnhancedSerpentsLineage) ? OpenerStrategy.None : strategy.Opener.Value switch
        {
            OpenerStrategy.Automatic => World.CurrentZone switch
            {
                1238 => OpenerStrategy.FRU,
                1363 => OpenerStrategy.DMU,
                _ => OpenerStrategy.Standard
            },
            var o => o
        };

        var gauge = World.Client.GetGauge<ViperGauge>();
        // force-ST drops AoE-only follow-ups left over from AoE (Den chain, Thresh weaves, Last Lash)
        Dread = forceST && gauge.DreadCombo is DreadCombo.PitOfDread or DreadCombo.HuntersDen or DreadCombo.SwiftskinsDen ? default : gauge.DreadCombo;
        Coil = gauge.RattlingCoilStacks;
        Offering = gauge.SerpentOffering;
        Anguine = gauge.AnguineTribute;
        CurSerpentsTail = gauge.SerpentCombo switch
        {
            SerpentCombo.DeathRattle => AID.DeathRattle,
            SerpentCombo.LastLash when !forceST => AID.LastLash,
            SerpentCombo.FirstLegacy => AID.FirstLegacy,
            SerpentCombo.SecondLegacy => AID.SecondLegacy,
            SerpentCombo.ThirdLegacy => AID.ThirdLegacy,
            SerpentCombo.FourthLegacy => AID.FourthLegacy,
            _ => AID.None
        };

        Swiftscaled = SelfStatusLeft(SID.Swiftscaled);
        Instinct = SelfStatusLeft(SID.HuntersInstinct);
        HonedReavers = SelfStatusLeft(SID.HonedReavers);
        HonedSteel = SelfStatusLeft(SID.HonedSteel);
        FlankstungVenom = SelfStatusLeft(SID.FlankstungVenom);
        FlanksbaneVenom = SelfStatusLeft(SID.FlanksbaneVenom);
        HindstungVenom = SelfStatusLeft(SID.HindstungVenom);
        HindsbaneVenom = SelfStatusLeft(SID.HindsbaneVenom);
        GrimskinsVenom = SelfStatusLeft(SID.GrimskinsVenom);
        HuntersVenom = SelfStatusLeft(SID.HuntersVenom);
        SwiftskinsVenom = SelfStatusLeft(SID.SwiftskinsVenom);
        FellhuntersVenom = forceST ? 0 : SelfStatusLeft(SID.FellhuntersVenom);
        FellskinsVenom = forceST ? 0 : SelfStatusLeft(SID.FellskinsVenom);
        PoisedForTwinfang = SelfStatusLeft(SID.PoisedForTwinfang);
        PoisedForTwinblood = SelfStatusLeft(SID.PoisedForTwinblood);
        ReawakenReady = SelfStatusLeft(SID.ReawakenReady);
        Reawakened = SelfStatusLeft(SID.Reawakened);

        AllowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        AOEMode = Unlocked(AID.SteelMaw) && UseAOE(strategy.AOE.Value, Hints.NumPriorityTargetsInAOECircle(Player.Position, 5), 3);
        InMelee = target != null && Player.DistanceToHitbox(target.Actor) <= 3;

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (target != null)
                Engage(strategy, target, countdown);
            return;
        }

        if (target == null)
            return;

        // only hold for an Ire the rotation will press; capped coils block it until Uncoiled Fury spends one
        IreWanted = IreStrat switch
        {
            OffensiveStrategy.Force => true,
            OffensiveStrategy.Automatic => target.Priority != Enemy.PriorityPointless && (Coil < CoilMax || strategy.Uncoiled.Value != UncoiledStrategy.Delay) && TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 10),
            _ => false
        };

        GCDs(strategy, target);
        UpdatePositionals(strategy, target);
        AddGoalZone(target);

        if (Player.InCombat)
            OGCD(strategy, target);
    }

    private void Engage(in Strategy strategy, Enemy target, float countdown)
    {
        var first = VicewinderOpener ? AID.Vicewinder : OpenerMode == OpenerStrategy.Standard ? AID.ReavingFangs : AID.SteelFangs;
        var engage = strategy.Engage.Value == EngageStrategy.Slither && !Unlocked(AID.Slither) ? EngageStrategy.Facepull : strategy.Engage.Value;
        switch (engage)
        {
            case EngageStrategy.Slither:
                if (InMelee ? countdown < 1.16f : countdown < 0.7f)
                    PushGCD(InMelee ? first : AID.Slither, target.Actor, 10);
                break;
            case EngageStrategy.Sprint:
                if (countdown < 10)
                    PushOGCD(ClassShared.AID.Sprint, Player, 10);
                if (countdown < (Player.DistanceToHitbox(target.Actor) - 3) / 7.8f + 0.5f)
                {
                    if (!InMelee)
                        Hints.ForcedMovement = Player.DirectionTo(target.Actor).ToVec3();
                    PushGCD(first, target.Actor, 10);
                }
                break;
            case EngageStrategy.Facepull:
                if (!InMelee)
                    Hints.ForcedMovement = Player.DirectionTo(target.Actor).ToVec3();
                else if (countdown < 1.16f)
                    PushGCD(first, target.Actor, 10);
                break;
        }
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        var dying = target.Priority == Enemy.PriorityPointless;

        switch (Dread)
        {
            case DreadCombo.Dreadwinder:
                PushGCD(FirstCoil(target.Actor), target.Actor, 40);
                break;
            case DreadCombo.HuntersCoil:
                PushGCD(AID.SwiftskinsCoil, target.Actor, 40);
                break;
            case DreadCombo.SwiftskinsCoil:
                PushGCD(AID.HuntersCoil, target.Actor, 40);
                break;
            case DreadCombo.PitOfDread:
                PushGCD(Swiftscaled <= Instinct ? AID.SwiftskinsDen : AID.HuntersDen, Player, 40);
                break;
            case DreadCombo.HuntersDen:
                PushGCD(AID.SwiftskinsDen, Player, 40);
                break;
            case DreadCombo.SwiftskinsDen:
                PushGCD(AID.HuntersDen, Player, 40);
                break;
        }

        if (Anguine > 0)
            PushGCD(NextGeneration(), BestSplash(target, 3), 35);

        if (ShouldReawaken(strategy, dying, target.Actor))
            PushGCD(AID.Reawaken, target.Actor, 30);

        if (strategy.Uncoiled.Value != UncoiledStrategy.Delay && Coil >= CoilMax && Dread == 0 && Reawakened == 0 && !TwinWeavesPending && (IreIn <= GCDLength * 3 || GCDReady(AID.Vicewinder)))
            PushGCD(AID.UncoiledFury, target.Actor, 25);

        var twinblade = AOEMode && Unlocked(AID.Vicepit) ? AID.Vicepit : AID.Vicewinder;
        if (ShouldTwinblade(strategy, twinblade))
            PushGCD(twinblade, target.Actor, 20);

        if (ShouldUncoil(strategy, dying, target.Actor))
            PushGCD(AID.UncoiledFury, BestSplash(target, 20), 15);

        if (!InMelee)
        {
            if (Coil > 0 && Unlocked(AID.UncoiledFury) && strategy.Uncoiled.Value != UncoiledStrategy.Delay)
                PushGCD(AID.UncoiledFury, target.Actor, 5);
            else if (strategy.Snap.Value == SnapStrategy.Ranged)
                PushGCD(AID.WrithingSnap, target.Actor, 4);
        }

        if (AOEMode)
            DualWieldAoE(target);
        else
            DualWieldST(target);
    }

    private Positional NextFinisherPositional
        => FlankstungVenom > 0 || FlanksbaneVenom > 0 ? Positional.Flank
         : HindstungVenom > 0 || HindsbaneVenom > 0 ? Positional.Rear
         : Positional.Any;

    private AID FirstCoil(Actor target)
    {
        if (CombatTime < 10 && OpenerMode != OpenerStrategy.None)
            return OpenerMode == OpenerStrategy.FRU ? AID.SwiftskinsCoil : AID.HuntersCoil;

        var refresh = GCDLength * 6;
        if (Swiftscaled < refresh && Swiftscaled <= Instinct)
            return AID.SwiftskinsCoil;
        if (Instinct < refresh)
            return AID.HuntersCoil;

        return NextFinisherPositional switch
        {
            Positional.Rear => AID.HuntersCoil,
            Positional.Flank => AID.SwiftskinsCoil,
            _ => CurrentPositional(target) == Positional.Rear ? AID.SwiftskinsCoil : AID.HuntersCoil
        };
    }

    private AID NextGeneration() => (MaxAnguine - Anguine) switch
    {
        0 => AID.FirstGeneration,
        1 => AID.SecondGeneration,
        2 => AID.ThirdGeneration,
        3 => AID.FourthGeneration,
        4 => AID.Ouroboros,
        _ => AID.None
    };

    private void DualWieldST(Enemy target)
    {
        if (ComboLeft > 0)
        {
            if (ComboLastMove == AID.HuntersSting && Unlocked(AID.FlankstingStrike))
            {
                PushGCD(FlanksbaneVenom > 0 || HindsbaneVenom > 0 ? AID.FlanksbaneFang : AID.FlankstingStrike, target.Actor, 3);
                return;
            }
            if (ComboLastMove == AID.SwiftskinsSting && Unlocked(AID.HindstingStrike))
            {
                PushGCD(HindsbaneVenom > 0 || FlanksbaneVenom > 0 ? AID.HindsbaneFang : AID.HindstingStrike, target.Actor, 3);
                return;
            }
            if (ComboLastMove is AID.SteelFangs or AID.ReavingFangs && Unlocked(AID.HuntersSting))
            {
                var hindNext = HindstungVenom > 0 || HindsbaneVenom > 0;
                var flankNext = FlankstungVenom > 0 || FlanksbaneVenom > 0;
                var swift = Unlocked(AID.SwiftskinsSting) && (hindNext || Swiftscaled == 0 || !flankNext && Swiftscaled <= Instinct);
                PushGCD(swift ? AID.SwiftskinsSting : AID.HuntersSting, target.Actor, 2);
                return;
            }
        }
        PushGCD(Unlocked(AID.ReavingFangs) && (HonedReavers > 0 || HonedSteel == 0) ? AID.ReavingFangs : AID.SteelFangs, target.Actor, 1);
    }

    private void DualWieldAoE(Enemy target)
    {
        if (ComboLeft > 0)
        {
            if (ComboLastMove is AID.HuntersBite or AID.SwiftskinsBite && Unlocked(AID.JaggedMaw))
            {
                PushGCD(GrimskinsVenom > 0 ? AID.BloodiedMaw : AID.JaggedMaw, Player, 3);
                return;
            }
            if (ComboLastMove is AID.SteelMaw or AID.ReavingMaw && Unlocked(AID.HuntersBite))
            {
                var swift = Unlocked(AID.SwiftskinsBite) && (GrimskinsVenom > 0 || Swiftscaled <= Instinct);
                PushGCD(swift ? AID.SwiftskinsBite : AID.HuntersBite, Player, 2);
                return;
            }
        }
        PushGCD(Unlocked(AID.ReavingMaw) && (HonedReavers > 0 || HonedSteel == 0) ? AID.ReavingMaw : AID.SteelMaw, Player, 1);
    }

    private bool HoldForIre => IreWanted && InBossFight && IreIn <= 10 && Swiftscaled > GCDLength * 4 && Instinct > GCDLength * 4;

    private bool ShouldTwinblade(in Strategy strategy, AID aid)
    {
        if (!GCDReady(aid) || Dread != 0 || Reawakened > 0 || Anguine > 0 || TwinWeavesPending)
            return false;

        switch (strategy.Twinblade.Value)
        {
            case OffensiveStrategy.Force:
                return true;
            case OffensiveStrategy.Delay:
                return false;
        }

        if (HoldForIre || aid == AID.Vicewinder && !InMelee)
            return false;
        if (OpenerMode == OpenerStrategy.Standard && CombatTime < 10 && ComboLastMove != AID.SwiftskinsSting)
            return false;
        if (DowntimeIn < GCD + GCDLength * 3)
            return false;
        return !IsExpiring(ComboLeft, GCDLength * 6) && !BuffsExpiring(GCDLength * 4);
    }

    private bool ShouldUncoil(in Strategy strategy, bool dying, Actor target)
    {
        if (Coil == 0 || strategy.Uncoiled.Value is UncoiledStrategy.Delay or UncoiledStrategy.Overcap)
            return false;
        if (strategy.Uncoiled.Value == UncoiledStrategy.HoldOne && Coil <= 1)
            return false;
        if (dying)
            return true;
        var ireSoon = IreWanted && IreIn <= GCDLength * 2;
        var inBurst = RaidBuffsLeft > GCD || OpenerMode != OpenerStrategy.None && CombatTime < 30;
        if (Coil <= 1 && !ireSoon && !inBurst && TimeToKill.WillLive(target, 6) && DowntimeIn > GCDLength * 3)
            return false;
        if (Dread != 0 || Reawakened > 0 || ReawakenReady > 0 && Unlocked(AID.Reawaken) || TwinWeavesPending || HoldForIre && !ireSoon)
            return false;
        if (Swiftscaled < GCDLength * 3 || Instinct < GCDLength * 3)
            return false;
        return !IsExpiring(ComboLeft, GCDLength * 2) && !BuffsExpiring(GCDLength * 2);
    }

    private bool ShouldReawaken(in Strategy strategy, bool dying, Actor target)
    {
        if (!Unlocked(AID.Reawaken) || Reawakened > 0 || ReawakenReady == 0 && Offering < 50 || Dread != 0 || TwinWeavesPending)
            return false;

        switch (ReawakenStrat)
        {
            case ReawakenStrategy.Delay:
                return false;
            case ReawakenStrategy.ASAP:
                return true;
        }

        if (!InMelee && !AOEMode)
            return false;

        if (!TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target, GCDLength * 6))
            return false;

        var windowLength = (MaxAnguine + 1) * GCDLength;
        if (Swiftscaled < windowLength || Instinct < windowLength || IsExpiring(ComboLeft, GCDLength * 6))
            return false;

        if (dying || !InBossFight || NoRaidBuffs || !TimeToKill.WillLive(target, 20))
            return true;

        if (ReawakenReady > 30 - GCDLength && DowntimeIn > windowLength * 2 + GCDLength)
            return false;

        if (DowntimeIn < windowLength)
            return false;

        if (ReawakenReady > 0 || Offering >= 100)
            return true;

        if (RaidBuffsLeft > windowLength)
            return true;

        return IreIn is >= 50 and <= 62;
    }

    #endregion

    #region oGCD

    private void OGCD(in Strategy strategy, Enemy target)
    {
        // these expire on the next GCD, so they go ahead of plan utilities (Feint, Bloodbath...)
        if (CurSerpentsTail != AID.None)
            PushExpiringOGCD(CurSerpentsTail, target.Actor, 60);

        if (PoisedForTwinfang > 0)
            PushExpiringOGCD(AID.UncoiledTwinfang, target.Actor, 55);
        if (PoisedForTwinblood > 0)
            PushExpiringOGCD(AID.UncoiledTwinblood, target.Actor, 55);

        if (FellhuntersVenom > 0)
            PushExpiringOGCD(AID.TwinfangThresh, Player, 55);
        if (FellskinsVenom > 0)
            PushExpiringOGCD(AID.TwinbloodThresh, Player, 55);

        if (HuntersVenom > 0)
            PushExpiringOGCD(AID.TwinfangBite, target.Actor, 55);
        if (SwiftskinsVenom > 0)
            PushExpiringOGCD(AID.TwinbloodBite, target.Actor, 55);

        if (IreWanted && (Coil < CoilMax || IreStrat == OffensiveStrategy.Force) && CanWeave(AID.SerpentsIre))
            PushOGCD(AID.SerpentsIre, Player, 40);

        if (strategy.Slither.Value == SlitherStrategy.GapClose && !InMelee && Player.DistanceToHitbox(target.Actor) <= 20)
            PushOGCD(AID.Slither, ResolveTarget(strategy.Slither) ?? target.Actor, 30);
    }

    private void UpdatePositionals(in Strategy strategy, Enemy target)
    {
        var (pos, imminent) = NextPositional(target.Actor);
        RecommendPositional(target, pos, imminent, strategy.TrueNorth.Value == TrueNorthStrategy.Automatic && InMelee);
    }

    private void AddGoalZone(Enemy target)
    {
        var (_, pos, imminent, _) = Hints.RecommendedPositional;
        var wantPositional = imminent || NextGCD == AID.Vicewinder && pos is Positional.Flank or Positional.Rear;
        var single = Hints.GoalSingleTarget(target.Actor, wantPositional ? pos : Positional.Any, Player, World.Actors, 3);

        var aoeBreakpoint = Dread switch
        {
            DreadCombo.Dreadwinder or DreadCombo.HuntersCoil or DreadCombo.SwiftskinsCoil => 50,
            DreadCombo.PitOfDread or DreadCombo.HuntersDen or DreadCombo.SwiftskinsDen => 1,
            _ => Anguine > 0 ? 50 : 3
        };
        Hints.GoalZones.Add(AllowAoE && Unlocked(AID.SteelMaw) && aoeBreakpoint < 50
            ? GoalCombined(single, Hints.GoalAOECircle(5), aoeBreakpoint)
            : single);
    }

    private (Positional, bool) NextPositional(Actor target)
    {
        if (!Unlocked(AID.FlankstingStrike) || AOEMode || Reawakened > 0)
            return (Positional.Any, false);

        return NextGCD switch
        {
            AID.FlankstingStrike or AID.FlanksbaneFang or AID.HuntersCoil => (Positional.Flank, true),
            AID.HindstingStrike or AID.HindsbaneFang or AID.SwiftskinsCoil => (Positional.Rear, true),
            AID.Vicewinder => (FirstCoil(target) == AID.HuntersCoil ? Positional.Flank : Positional.Rear, false),
            _ => (ComboLastMove switch
            {
                AID.HuntersSting => Positional.Flank,
                AID.SwiftskinsSting => Positional.Rear,
                _ => Swiftscaled <= Instinct ? Positional.Rear : Positional.Flank
            }, false)
        };
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => PotionPrepull || Player.InCombat && (Unlocked(AID.SerpentsIre) ? IreIn < 6 : RaidBuffsLeft > 0 || RaidBuffsIn < 5),
        PotionStrategy.AlignWithRaidBuffs => PotionWithRaidBuffs,
        PotionStrategy.Immediate => true,
        _ => false
    };

    private Actor BestSplash(Enemy primary, float range)
        => BestAOETarget(primary, range, AllowAoE, (c, e) => TargetInAOECircle(e, c.Position, 5)).Best ?? primary.Actor;

    private bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    private void PushExpiringOGCD(AID aid, Actor? target, int priority)
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.Medium + priority);

    #endregion
}
