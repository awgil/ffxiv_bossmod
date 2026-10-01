using BossMod.VPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageVPR(RotationModuleManager manager, Actor player) : TypedRotationModule<KageVPR.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

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

        [Track("Opener", MinLevel = 100, UiPriority = -10, Context = StrategyContext.Plan)]
        public Track<OpenerStrategy> Opener;
    }

    public enum ReawakenStrategy
    {
        [Option("Use with Serpent's Ire, during raid buffs, at full Offering, and at the odd-minute window")]
        Automatic,
        [Option("Use as soon as Offering allows", MinLevel = 90)]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum UncoiledStrategy
    {
        [Option("Spend between combos, never overcap, use when out of melee range", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Like Automatic, but keep one stack", Targets = ActionTargets.Hostile)]
        HoldOne,
        [Option("Only use when out of melee range or about to overcap", Targets = ActionTargets.Hostile)]
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
        [Option("Slither to the target, or use the opening GCD right before the pull if already in melee range")]
        Slither,
        [Option("Sprint into melee range")]
        Sprint,
        [Option("Walk into melee range and use the opening GCD right before the pull")]
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
        [Option("Standard; FRU or DMU opener in those ultimates")]
        Automatic,
        [Option("Reaving Fangs, Swiftskin's Sting, Vicewinder, Hunter's Coil first")]
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
        return new RotationModuleDefinition("Kage VPR", "Viper", "Standard rotation (Kage)|Melee", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.VPR), 100).WithStrategies<Strategy>();
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
    private float TrueNorthLeft;

    private float RaidBuffsLeft;
    private float RaidBuffsIn;
    private float DowntimeIn;
    private bool InBossFight;

    private Actor? BestSplashTarget;
    private bool AllowAoE;
    private bool AOEMode;
    private bool InMelee;
    private float AnimLockDelay;

    private Targeting TargetMode;
    private OpenerStrategy OpenerMode;
    private AID NextGCD;
    private float NextGCDPrio;

    private float GCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private float ComboLeft => World.Client.ComboState.Remaining;
    private int CoilMax => Unlocked(TraitID.EnhancedVipersRattle) ? 3 : 2;
    private bool HasBothBuffs => Swiftscaled > GCD && Instinct > GCD;
    private bool TwinWeavesPending => HuntersVenom > 0 || SwiftskinsVenom > 0 || FellhuntersVenom > 0 || FellskinsVenom > 0 || PoisedForTwinfang > 0 || PoisedForTwinblood > 0;
    private float IreIn => ReadyIn(AID.SerpentsIre);
    private float CombatTime => Player.InCombat ? (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds : 0;
    private bool VicewinderOpener => OpenerMode is OpenerStrategy.FRU or OpenerStrategy.DMU;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        AnimLockDelay = estimatedAnimLockDelay;
        NextGCD = AID.None;
        NextGCDPrio = 0;

        var target = Hints.FindEnemy(primaryTarget);
        if (target?.Priority is Enemy.PriorityInvincible or Enemy.PriorityForbidden)
            target = null;

        TargetMode = strategy.Targeting.Value == Targeting.AutoTryPri ? (target != null ? Targeting.AutoPrimary : Targeting.Auto) : strategy.Targeting.Value;
        if (TargetMode == Targeting.Auto && target == null)
        {
            target = Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 3).MinBy(e => Player.DistanceToHitbox(e.Actor)) ?? target;
            primaryTarget = target?.Actor;
        }

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
        Dread = gauge.DreadCombo;
        Coil = gauge.RattlingCoilStacks;
        Offering = gauge.SerpentOffering;
        Anguine = gauge.AnguineTribute;
        CurSerpentsTail = gauge.SerpentCombo switch
        {
            SerpentCombo.DeathRattle => AID.DeathRattle,
            SerpentCombo.LastLash => AID.LastLash,
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
        FellhuntersVenom = SelfStatusLeft(SID.FellhuntersVenom);
        FellskinsVenom = SelfStatusLeft(SID.FellskinsVenom);
        PoisedForTwinfang = SelfStatusLeft(SID.PoisedForTwinfang);
        PoisedForTwinblood = SelfStatusLeft(SID.PoisedForTwinblood);
        ReawakenReady = SelfStatusLeft(SID.ReawakenReady);
        Reawakened = SelfStatusLeft(SID.Reawakened);
        TrueNorthLeft = SelfStatusLeft(SID.TrueNorth);

        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);
        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime() is (var downNow, var stateLeft) ? (downNow ? 0 : stateLeft) : float.MaxValue;
        InBossFight = Bossmods.ActiveModule != null || primaryTarget?.IsStrikingDummy == true;

        AllowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        AOEMode = Unlocked(AID.SteelMaw) && strategy.AOE.Value switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.AOE => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5) >= 3,
            _ => false
        };
        InMelee = target != null && Player.DistanceToHitbox(target.Actor) <= 3;
        BestSplashTarget = target == null ? null : BestSplash(target, 20);

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

        GCDs(strategy, target);
        UpdatePositionals(strategy, target);
        AddGoalZone(strategy, target);

        if (Player.InCombat)
            OGCD(strategy, target);
    }

    private void Engage(in Strategy strategy, Enemy target, float countdown)
    {
        var first = VicewinderOpener ? AID.Vicewinder : OpenerMode == OpenerStrategy.Standard ? AID.ReavingFangs : AID.SteelFangs;
        switch (strategy.Engage.Value)
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

        // twinblade continuations always come first
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

        if (ShouldReawaken(strategy, dying))
            PushGCD(AID.Reawaken, target.Actor, 30);

        if (strategy.Uncoiled.Value != UncoiledStrategy.Delay && Coil >= CoilMax && Dread == 0 && Reawakened == 0 && !TwinWeavesPending && (IreIn <= GCDLength * 3 || GCDReady(AID.Vicewinder)))
            PushGCD(AID.UncoiledFury, target.Actor, 25);

        if (ShouldTwinblade(strategy))
            PushGCD(AOEMode ? AID.Vicepit : AID.Vicewinder, AOEMode ? Player : target.Actor, 20);

        if (ShouldUncoil(strategy, dying))
            PushGCD(AID.UncoiledFury, BestSplashTarget ?? target.Actor, 15);

        if (!InMelee)
        {
            if (Coil > 0 && strategy.Uncoiled.Value != UncoiledStrategy.Delay)
                PushGCD(AID.UncoiledFury, target.Actor, 5);
            else if (strategy.Snap.Value == SnapStrategy.Ranged)
                PushGCD(AID.WrithingSnap, target.Actor, 4);
        }

        if (AOEMode)
            DualWieldAoE(target);
        else
            DualWieldST(target);
    }

    // positional of the dual-wield finisher that follows the twinblade combo, from the venom buff we're holding
    private Positional NextFinisherPositional
        => FlankstungVenom > 0 || FlanksbaneVenom > 0 ? Positional.Flank
         : HindstungVenom > 0 || HindsbaneVenom > 0 ? Positional.Rear
         : Positional.Any;

    // order the coils so the second one is on the same side as the next finisher (one reposition instead of two);
    // an expiring buff always goes first, and without a venom we start on the side we're already standing
    private AID FirstCoil(Actor target)
    {
        if (CombatTime < 10 && OpenerMode is OpenerStrategy.Standard or OpenerStrategy.DMU)
            return AID.HuntersCoil;
        if (CombatTime < 10 && OpenerMode == OpenerStrategy.FRU)
            return AID.SwiftskinsCoil;

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

    private Positional CurrentPositional(Actor target)
    {
        var dir = target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized());
        return dir < -0.7071068f ? Positional.Rear : dir < 0.7071068f ? Positional.Flank : Positional.Front;
    }

    private AID NextGeneration()
    {
        var max = Unlocked(TraitID.EnhancedSerpentsLineage) ? 5 : 4;
        return (max - Anguine) switch
        {
            0 => AID.FirstGeneration,
            1 => AID.SecondGeneration,
            2 => AID.ThirdGeneration,
            3 => AID.FourthGeneration,
            4 => AID.Ouroboros,
            _ => AID.None
        };
    }

    private void DualWieldST(Enemy target)
    {
        if (ComboLeft > 0)
        {
            if (ComboLastMove == AID.HuntersSting)
            {
                PushGCD(FlanksbaneVenom > 0 || HindsbaneVenom > 0 ? AID.FlanksbaneFang : AID.FlankstingStrike, target.Actor, 3);
                return;
            }
            if (ComboLastMove == AID.SwiftskinsSting)
            {
                PushGCD(HindsbaneVenom > 0 || FlanksbaneVenom > 0 ? AID.HindsbaneFang : AID.HindstingStrike, target.Actor, 3);
                return;
            }
            if (ComboLastMove is AID.SteelFangs or AID.ReavingFangs)
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

    // hold a fresh twinblade combo when Serpent's Ire is about to come up, so its Rattling Coil isn't wasted
    private bool HoldForIre => InBossFight && IreIn is > 0 and <= 10 && Swiftscaled > GCDLength * 4 && Instinct > GCDLength * 4;

    private bool ShouldTwinblade(in Strategy strategy)
    {
        var aid = AOEMode ? AID.Vicepit : AID.Vicewinder;
        if (!Unlocked(aid) || !GCDReady(aid) || Dread != 0 || Reawakened > 0 || Anguine > 0 || TwinWeavesPending)
            return false;

        switch (strategy.Twinblade.Value)
        {
            case OffensiveStrategy.Force:
                return true;
            case OffensiveStrategy.Delay:
                return false;
        }

        if (HoldForIre || !AOEMode && !InMelee)
            return false;
        // standard opener: Swiftskin's Sting before the first Vicewinder
        if (OpenerMode == OpenerStrategy.Standard && CombatTime < 10 && ComboLastMove != AID.SwiftskinsSting)
            return false;
        // the combo is 3 GCDs plus two twin weaves; don't start it into downtime
        if (DowntimeIn < GCD + GCDLength * 3)
            return false;
        if (ComboLeft > 0 && ComboLeft < GCDLength * 6)
            return false;
        return !HasBothBuffs || Swiftscaled < GCDLength * 4 || Instinct < GCDLength * 4 || IreIn >= GCDLength * 3 || !InBossFight;
    }

    private bool ShouldUncoil(in Strategy strategy, bool dying)
    {
        if (Coil == 0 || strategy.Uncoiled.Value is UncoiledStrategy.Delay or UncoiledStrategy.Overcap)
            return false;
        if (strategy.Uncoiled.Value == UncoiledStrategy.HoldOne && Coil <= 1)
            return false;
        if (dying)
            return true;
        if (Dread != 0 || Reawakened > 0 || ReawakenReady > 0 || TwinWeavesPending || HoldForIre)
            return false;
        if (!HasBothBuffs || Swiftscaled < GCDLength * 3 || Instinct < GCDLength * 3)
            return false;
        return ComboLeft == 0 || ComboLeft > GCDLength * 2;
    }

    private bool ShouldReawaken(in Strategy strategy, bool dying)
    {
        if (!Unlocked(AID.Reawaken) || Reawakened > 0 || ReawakenReady == 0 && Offering < 50 || Dread != 0 || TwinWeavesPending)
            return false;

        switch (strategy.Reawaken.Value)
        {
            case ReawakenStrategy.Delay:
                return false;
            case ReawakenStrategy.ASAP:
                return true;
        }

        if (!InMelee && !AOEMode)
            return false;

        // generations need both buffs to last the whole window
        var windowLength = (Unlocked(TraitID.EnhancedSerpentsLineage) ? 5 : 4) * GCDLength + GCDLength;
        if (Swiftscaled < windowLength || Instinct < windowLength || ComboLeft > 0 && ComboLeft < GCDLength * 6)
            return false;

        // no raid-buff jobs in the party: nothing to align with
        if (dying || !InBossFight || RaidBuffsIn > 9000 && RaidBuffsLeft == 0)
            return true;

        if (DowntimeIn < windowLength)
            return false;

        if (ReawakenReady > 0 || Offering >= 100)
            return true;

        if (RaidBuffsLeft > windowLength)
            return true;

        // odd-minute Reawaken: ~1 minute into Serpent's Ire cooldown, unless the burst is close
        return IreIn is >= 50 and <= 62;
    }

    #endregion

    #region oGCD

    private void OGCD(in Strategy strategy, Enemy target)
    {
        if (CurSerpentsTail != AID.None)
            PushOGCD(CurSerpentsTail, target.Actor, 60);

        if (PoisedForTwinfang > 0)
            PushOGCD(AID.UncoiledTwinfang, target.Actor, 55);
        if (PoisedForTwinblood > 0)
            PushOGCD(AID.UncoiledTwinblood, target.Actor, 55);

        if (FellhuntersVenom > 0)
            PushOGCD(AID.TwinfangThresh, Player, 55);
        if (FellskinsVenom > 0)
            PushOGCD(AID.TwinbloodThresh, Player, 55);

        if (HuntersVenom > 0)
            PushOGCD(AID.TwinfangBite, target.Actor, 55);
        if (SwiftskinsVenom > 0)
            PushOGCD(AID.TwinbloodBite, target.Actor, 55);

        if (ShouldIre(strategy, target))
            PushOGCD(AID.SerpentsIre, Player, 40);

        if (strategy.Slither.Value == SlitherStrategy.GapClose && !InMelee && Player.DistanceToHitbox(target.Actor) <= 20)
            PushOGCD(AID.Slither, ResolveTarget(strategy.Slither) ?? target.Actor, 30);
    }

    private bool ShouldIre(in Strategy strategy, Enemy target)
    {
        if (!Unlocked(AID.SerpentsIre) || !CanWeave(AID.SerpentsIre))
            return false;
        return strategy.Ire.Value switch
        {
            OffensiveStrategy.Force => true,
            OffensiveStrategy.Automatic => Coil < CoilMax && target.Priority != Enemy.PriorityPointless,
            _ => false
        };
    }

    private void UpdatePositionals(in Strategy strategy, Enemy target)
    {
        var (pos, imminent) = NextPositional(target.Actor);

        var actor = target.Actor;
        if (actor.Omnidirectional || actor.TargetID == Player.InstanceID && actor.CastInfo == null && !actor.IsStrikingDummy || target.Priority < 0)
            (pos, imminent) = (Positional.Any, false);

        var tn = TrueNorthLeft > GCD;
        var dir = actor.Rotation.ToDirection().Dot((Player.Position - actor.Position).Normalized());
        var correct = tn || pos switch
        {
            Positional.Flank => MathF.Abs(dir) < 0.7071067f,
            Positional.Rear => dir < -0.7071068f,
            _ => true
        };
        Hints.RecommendedPositional = (actor, pos, imminent && !tn, correct);

        if (strategy.TrueNorth.Value == TrueNorthStrategy.Automatic && imminent && !correct && InMelee)
            PushOGCD(ClassShared.AID.TrueNorth, Player, 20, GCD - 0.8f);
    }

    // melee goal; includes the positional when the next hit needs one (or Vicewinder is queued), so the AI walks there
    private void AddGoalZone(in Strategy strategy, Enemy target)
    {
        var (_, pos, imminent, _) = Hints.RecommendedPositional;
        var wantPositional = imminent || NextGCD == AID.Vicewinder && pos is Positional.Flank or Positional.Rear;
        var single = Hints.GoalSingleTarget(target.Actor, wantPositional ? pos : Positional.Any, Player, World.Actors, 3);

        // coils need the target for positionals; dens need enemies around us; otherwise stack up 3+ for the AoE combo
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

        // start walking towards the first coil as soon as Vicewinder is queued
        if (NextGCD == AID.Vicewinder)
            return (FirstCoil(target) == AID.HuntersCoil ? Positional.Flank : Positional.Rear, false);

        var imminent = NextGCD is AID.FlankstingStrike or AID.FlanksbaneFang or AID.HindstingStrike or AID.HindsbaneFang or AID.HuntersCoil or AID.SwiftskinsCoil;
        var pos = NextGCD switch
        {
            AID.FlankstingStrike or AID.FlanksbaneFang or AID.HuntersCoil => Positional.Flank,
            AID.HindstingStrike or AID.HindsbaneFang or AID.SwiftskinsCoil => Positional.Rear,
            _ => ComboLastMove switch
            {
                AID.HuntersSting => Positional.Flank,
                AID.SwiftskinsSting => Positional.Rear,
                _ => Swiftscaled <= Instinct ? Positional.Rear : Positional.Flank
            }
        };
        return (pos, imminent);
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => World.Client.CountdownRemaining is > 0 and < 2 || Player.InCombat && Unlocked(AID.SerpentsIre) && ReadyIn(AID.SerpentsIre) < 6,
        PotionStrategy.AlignWithRaidBuffs => World.Client.CountdownRemaining is > 0 and < 2 || Player.InCombat && (RaidBuffsLeft > 0 || RaidBuffsIn < 5),
        PotionStrategy.Immediate => true,
        _ => false
    };

    // 5y splash target that hits the most priority targets without touching a forbidden one
    private Actor BestSplash(Enemy primary, float range)
    {
        int Count(Actor c) => Hints.ForbiddenTargets.Any(e => TargetInAOECircle(e.Actor, c.Position, 5)) ? 0 : Hints.PriorityTargets.Count(e => TargetInAOECircle(e.Actor, c.Position, 5));
        var best = primary.Actor;
        if (!AllowAoE || TargetMode == Targeting.Manual)
            return best;
        var bestCount = Count(best);
        foreach (var e in Hints.PriorityTargets)
        {
            if (e.Actor == best || Player.DistanceToHitbox(e.Actor) > range || TargetMode == Targeting.AutoPrimary && !TargetInAOECircle(primary.Actor, e.Actor.Position, 5))
                continue;
            var c = Count(e.Actor);
            if (c > bestCount)
                (best, bestCount) = (e.Actor, c);
        }
        return best;
    }

    private bool Unlocked(AID aid) => ActionUnlocked(aid);
    private bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    private float ReadyIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
    private bool GCDReady(AID aid) => ReadyIn(aid) < GCD + 0.05f;

    private bool CanWeave(AID aid)
    {
        if (!Unlocked(aid))
            return false;
        var def = ActionDefinitions.Instance.Spell(aid)!;
        return MathF.Max(ReadyIn(aid), World.Client.AnimationLock) + def.TotalDuration + AnimLockDelay <= GCD;
    }

    private void PushGCD(AID aid, Actor? target, int priority)
    {
        if (PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + priority, 0) && priority > NextGCDPrio)
        {
            NextGCD = aid;
            NextGCDPrio = priority;
        }
    }

    private void PushOGCD(AID aid, Actor? target, int priority, float delay = 0)
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.Low + priority, delay);

    private void PushOGCD(ClassShared.AID aid, Actor? target, int priority, float delay = 0)
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.Low + priority, delay);

    private bool PushAction(ActionID action, Actor? target, float priority, float delay)
    {
        if (action.ID == 0 || !ActionUnlocked(action))
            return false;
        var def = ActionDefinitions.Instance[action];
        if (def == null || def.Range != 0 && target == null)
            return false;
        Hints.ActionsToExecute.Push(action, target, priority, delay: delay);
        return true;
    }

    #endregion
}
