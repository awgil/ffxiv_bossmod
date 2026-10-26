using BossMod.PCT;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KagePCT(RotationModuleManager manager, Actor player) : KageShared<KagePCT.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Burst (Starry Muse)", InternalName = "Burst", MinLevel = 70, Action = AID.StarryMuse)]
        public Track<OffensiveStrategy> Burst;

        [Track("Living Muse", MinLevel = 30, Actions = [AID.PomMuse, AID.WingedMuse, AID.ClawedMuse, AID.FangedMuse])]
        public Track<MuseStrategy> Living;

        [Track("Striking Muse", MinLevel = 50, Action = AID.StrikingMuse)]
        public Track<MuseStrategy> Striking;

        [Track("Portraits", MinLevel = 30, Actions = [AID.MogOfTheAges, AID.RetributionOfTheMadeen])]
        public Track<OffensiveStrategy> Portrait;

        [Track("Hammer combo", MinLevel = 50, Actions = [AID.HammerStamp, AID.HammerBrush, AID.PolishingHammer])]
        public Track<SpendStrategy> Hammer;

        [Track("Holy in White / Comet in Black", MinLevel = 80, Actions = [AID.HolyInWhite, AID.CometInBlack])]
        public Track<SpendStrategy> Holy;

        [Track("Subtractive Palette", MinLevel = 60, Action = AID.SubtractivePalette)]
        public Track<OffensiveStrategy> Palette;

        [Track("Motifs")]
        public Track<MotifStrategy> Motifs;

        [Track("Lucid Dreaming", MinLevel = 14, Action = ClassShared.AID.LucidDreaming)]
        public Track<OffensiveStrategy> Lucid;

        [Track("Tempera Coat / Grassa", MinLevel = 10, Actions = [AID.TemperaCoat, AID.TemperaGrassa])]
        public Track<TemperaStrategy> Tempera;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 90, UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum MuseStrategy
    {
        [Option("Use with Starry Muse; never overcap")]
        Automatic,
        [Option("Use as soon as possible")]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum SpendStrategy
    {
        [Option("Automatic: Starry Muse burst, movement, AoE; Comet after the Subtractive combo")]
        Automatic,
        [Option("Spend as soon as possible")]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum MotifStrategy
    {
        [Option("Paint out of combat, in downtime and between bursts")]
        Automatic,
        [Option("Only out of combat and in downtime")]
        Downtime,
        [Option("Do not paint")]
        Delay
    }

    public enum TemperaStrategy
    {
        [Option("Coat + Grassa (party shield) before raidwides")]
        Automatic,
        [Option("Tempera Coat only (self) before raidwides")]
        Self,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use before the pull and with Starry Muse")]
        AlignWithBurst,
        [Option("Use before the pull and with raid buffs")]
        AlignWithRaidBuffs,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Starry Muse after the 2nd GCD")]
        SecondGCD,
        [Option("Starry Muse after the 3rd GCD (Holy / Comet before the Wing motif)")]
        ThirdGCD,
        [Option("No opener-specific rules")]
        None
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage PCT", "Pictomancer", "Standard rotation (Kage)|Casters", "Kagekazu", RotationModuleQuality.Ok, BitMask.Build(Class.PCT), 100).WithStrategies<Strategy>();
    }

    protected override bool UsesSpellSpeed => true;

    private int Palette;
    private int Paint;
    private CanvasFlags Canvas;
    private CreatureFlags Creature;

    private bool Subtractive;
    private bool Monochrome;
    private float StarryLeft;
    private float Spectrum;
    private float Hyperphantasia;
    private int HyperStacks;
    private float Starstruck;
    private float RainbowBright;
    private float HammerTime;
    private float Aetherhues2;

    private bool AllowAoE;
    private bool AOEMode;
    private int SplashCount;
    private Actor? BestSplash;
    private Actor? BestLine;

    private bool CreatureDrawn => (Canvas & (CanvasFlags.Pom | CanvasFlags.Wing | CanvasFlags.Claw | CanvasFlags.Maw)) != 0;
    private bool WeaponDrawn => Canvas.HasFlag(CanvasFlags.Weapon);
    private bool LandscapeDrawn => Canvas.HasFlag(CanvasFlags.Landscape);
    private bool PortraitReady => (Creature & (CreatureFlags.MooglePortait | CreatureFlags.MadeenPortrait)) != 0;
    private float StarryIn => ReadyIn(AID.StarryMuse);
    private bool CanCast => SwiftcastUp || CastFits(ScaledCastTime(AID.FireInRed)) && DowntimeIn > GCD + ScaledCastTime(AID.FireInRed);
    private OpenerStrategy OpenerMode;
    private bool InOpener => OpenerMode != OpenerStrategy.None && CombatTime < 30;
    private bool Dying;

    private OffensiveStrategy StarryStrat;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 25, strategy.AOE.Value);
        StarryStrat = strategy.Burst.Value;
        OpenerMode = Unlocked(AID.StarrySkyMotif) && Unlocked(AID.RainbowDrip) ? strategy.Opener.Value : OpenerStrategy.None;

        var gauge = World.Client.GetGauge<PictomancerGauge>();
        Palette = gauge.PalleteGauge;
        Paint = gauge.Paint;
        Canvas = gauge.CanvasFlags;
        Creature = gauge.CreatureFlags;

        Subtractive = HasSelfStatus(SID.SubtractivePalette);
        Monochrome = HasSelfStatus(SID.MonochromeTones);
        StarryLeft = SelfStatusLeft(SID.StarryMuse);
        Spectrum = SelfStatusLeft(SID.SubtractiveSpectrum);
        (Hyperphantasia, HyperStacks) = SelfStatusDetails(SID.Hyperphantasia);
        Starstruck = SelfStatusLeft(SID.Starstruck);
        RainbowBright = SelfStatusLeft(SID.RainbowBright);
        HammerTime = SelfStatusLeft(SID.HammerTime);
        Aetherhues2 = SelfStatusLeft(SID.AetherhuesII);

        AllowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        (BestSplash, SplashCount) = BestAOETarget(target, 25, AllowAoE, (c, e) => TargetInAOECircle(e, c.Position, 5));
        BestLine = BestAOETarget(target, 25, AllowAoE, (c, e) => TargetInAOERect(e, Player.Position, Player.DirectionTo(c), 25, 2)).Best;
        var breakpoint = Unlocked(TraitID.EnhancedPictomancyIV) || !Subtractive && Aetherhues2 == 0 ? 4 : 3;
        AOEMode = Unlocked(AID.FireIIInRed) && UseAOE(strategy.AOE.Value, SplashCount, breakpoint);

        if (UsePotion(strategy))
            PushPotion(ActionDefinitions.IDPotionInt);

        if (!Player.InCombat && strategy.Motifs.Value != MotifStrategy.Delay)
            Motifs(true, null);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (target != null)
                Engage(target, countdown);
            return;
        }

        if (Player.InCombat && strategy.Tempera.Value != TemperaStrategy.Delay && RaidwideWithin(4))
        {
            if (SelfStatusLeft(SID.TemperaCoat) > 0)
            {
                if (strategy.Tempera.Value == TemperaStrategy.Automatic && Unlocked(AID.TemperaGrassa))
                    PushOGCD(AID.TemperaGrassa, Player, 46);
            }
            else if (CanWeave(AID.TemperaCoat))
                PushOGCD(AID.TemperaCoat, Player, 45);
        }

        if (target == null)
        {
            if (Player.InCombat && strategy.Motifs.Value != MotifStrategy.Delay)
                Motifs(true, null);
            return;
        }

        Dying = target.Priority == Enemy.PriorityPointless;
        Hints.GoalZones.Add(Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 25));

        GCDs(strategy, target);

        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    private void Engage(Enemy target, float countdown)
    {
        var rainbow = Unlocked(AID.RainbowDrip) ? AID.RainbowDrip : AID.FireInRed;
        if (countdown < ScaledCastTime(rainbow) + 0.5f)
            PushGCD(rainbow, target.Actor, 10, ScaledCastTime(rainbow));
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        var splash = BestSplash ?? target.Actor;
        var inStarry = StarryLeft > GCD;
        var prismReserve = Starstruck > GCD ? 1 : 0;
        var cometReady = Monochrome && Paint > 0;

        if (Starstruck > GCD)
            PushGCD(AID.StarPrism, splash, Starstruck < GCD + GCDLength * 2 ? 63 : 60);

        if (inStarry && Hyperphantasia > 0 && Subtractive && HyperStacks > prismReserve + (cometReady ? 1 : 0))
            PushGCD(AOEMode ? AID.BlizzardIIInCyan : AID.BlizzardInCyan, AOEMode ? splash : target.Actor, 62, ScaledCastTime(AID.BlizzardInCyan));

        if (RainbowBright > GCD)
            PushGCD(AID.RainbowDrip, BestLine ?? target.Actor, inStarry && HammerTime > GCD ? 47 : 58);

        if (cometReady && UseComet(strategy.Holy.Value, prismReserve))
            PushGCD(AID.CometInBlack, splash, !inStarry ? 55 : Hyperphantasia > 0 ? 61 : HammerTime > GCD ? 45 : 55);

        if (SwiftRainbowBeforeDowntime)
        {
            if (SwiftcastUp)
                PushGCD(AID.RainbowDrip, BestLine ?? target.Actor, 57);
            else
                PushOGCD(ClassShared.AID.Swiftcast, Player, 81);
        }

        if (HammerTime > GCD && UseHammer(strategy.Hammer.Value))
            PushGCD(AID.HammerStamp, splash, 50);

        if (InOpener && OpenerMode == OpenerStrategy.ThirdGCD && CombatTime < 5 && Paint > 0 && !Canvas.HasFlag(CanvasFlags.Wing))
            PushGCD(Monochrome ? AID.CometInBlack : AID.HolyInWhite, splash, 35);

        if (strategy.Motifs.Value == MotifStrategy.Automatic && !Dying && StarryLeft == 0 && Hyperphantasia == 0 && Spectrum <= GCD)
            Motifs(false, target);

        if (!CanCast)
            MovementGCDs(strategy, target, splash);

        if (Subtractive)
            PushGCD(AOEMode ? AID.BlizzardIIInCyan : AID.BlizzardInCyan, AOEMode ? splash : target.Actor, 20, ScaledCastTime(AID.BlizzardInCyan));

        if (strategy.Holy.Value != SpendStrategy.Delay && !Monochrome && Paint > 0
            && (strategy.Holy.Value == SpendStrategy.ASAP || Dying || AOEMode || SplashCount > 1 && Paint > 1))
            PushGCD(AID.HolyInWhite, splash, AOEMode ? 25 : 15);

        PushGCD(AOEMode ? AID.FireIIInRed : AID.FireInRed, AOEMode ? splash : target.Actor, 10, ScaledCastTime(AID.FireInRed));
    }

    private void MovementGCDs(in Strategy strategy, Enemy target, Actor splash)
    {
        if (strategy.Hammer.Value != SpendStrategy.Delay && HammerTime > GCD && Hyperphantasia == 0)
            PushGCD(AID.HammerStamp, splash, 45);
        if (strategy.Holy.Value != SpendStrategy.Delay && Monochrome && Paint > 0)
            PushGCD(AID.CometInBlack, splash, 44);
        if (StarryLeft == 0 && NeedsMotif && ReadyIn(ClassShared.AID.Swiftcast) <= GCD)
            PushOGCD(ClassShared.AID.Swiftcast, Player, 80);
        if (strategy.Holy.Value != SpendStrategy.Delay && !Monochrome && Paint > 0)
            PushGCD(AID.HolyInWhite, splash, 43);
    }

    private bool SwiftRainbowBeforeDowntime => Unlocked(AID.RainbowDrip) && RainbowBright == 0 && StarryLeft == 0
        && DowntimeIn > GCD + 1.3f && DowntimeIn < GCD + GCDLength && (SwiftcastUp || ReadyIn(ClassShared.AID.Swiftcast) <= GCD);

    private bool NextCreatureGivesPortrait => Creature.HasFlag(CreatureFlags.Pom) && (!Creature.HasFlag(CreatureFlags.Wings) || Creature.HasFlag(CreatureFlags.Claw));

    private bool NeedsMotif => Unlocked(AID.CreatureMotif) && !CreatureDrawn || Unlocked(AID.WeaponMotif) && !WeaponDrawn && HammerTime == 0 || Unlocked(AID.LandscapeMotif) && !LandscapeDrawn;

    private void Motifs(bool free, Enemy? target)
    {
        var alive = target == null || TimeToKill.WillLive(target.Actor, 10);
        var creatureAlive = target == null || TimeToKill.WillLive(target.Actor, NextCreatureGivesPortrait || !Unlocked(AID.WingMotif) ? 8 : 16);
        var burstPrep = Unlocked(AID.StarryMuse) && StarryIn <= 20;

        if (Unlocked(AID.CreatureMotif) && !CreatureDrawn && (free || creatureAlive && (burstPrep || ReadyIn(AID.LivingMuse) <= 8 || SwiftcastUp)))
            PushGCD(AID.CreatureMotif, Player, 30, MotifCast);

        if (Unlocked(AID.WeaponMotif) && !WeaponDrawn && HammerTime == 0 && (free || alive && (burstPrep || !Unlocked(AID.StarryMuse) && ReadyIn(AID.StrikingMuse) <= 8 || SwiftcastUp)))
            PushGCD(AID.WeaponMotif, Player, 29, MotifCast);

        if (Unlocked(AID.LandscapeMotif) && !LandscapeDrawn && StarryLeft == 0 && (free || alive && (burstPrep || SwiftcastUp)))
            PushGCD(AID.LandscapeMotif, Player, 28, MotifCast);
    }

    private float MotifCast => HardcastTime(AID.CreatureMotif);

    private bool UseComet(SpendStrategy strategy, int prismReserve) => strategy switch
    {
        SpendStrategy.Delay => false,
        SpendStrategy.ASAP => true,
        _ => Dying || (StarryLeft > GCD ? Hyperphantasia > 0 && HyperStacks > prismReserve || Hyperphantasia == 0 : !Subtractive)
    };

    private bool UseHammer(SpendStrategy strategy) => strategy switch
    {
        SpendStrategy.Delay => false,
        SpendStrategy.ASAP => true,
        _ => Dying || Hyperphantasia == 0 && (StarryIn >= 10 || !Unlocked(AID.StarryMuse))
            && (StarryLeft > GCD || HammerTime <= 30 || StarryIn <= 30 || AOEMode)
    };

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        var splash = BestSplash ?? target.Actor;

        if (UsePalette(strategy.Palette.Value) && CanWeave(AID.SubtractivePalette))
            PushOGCD(AID.SubtractivePalette, Player, 70);

        if (LandscapeDrawn && UseStarry(target) && CanWeave(AID.StarryMuse))
            PushOGCD(AID.StarryMuse, Player, 68);

        if (CreatureDrawn && UseLiving(strategy.Living.Value) && CanWeave(AID.LivingMuse))
            PushOGCD(LivingMuseAID, splash, 60);

        if (WeaponDrawn && HammerTime == 0 && UseStriking(strategy.Striking.Value) && CanWeave(AID.StrikingMuse))
            PushOGCD(AID.StrikingMuse, Player, 58);

        if (PortraitReady && UsePortrait(strategy.Portrait.Value) && CanWeave(AID.MogOfTheAges))
            PushOGCD(Creature.HasFlag(CreatureFlags.MadeenPortrait) ? AID.RetributionOfTheMadeen : AID.MogOfTheAges, BestLine ?? target.Actor, 55);

        if (strategy.Lucid.Value == OffensiveStrategy.Force || strategy.Lucid.Value == OffensiveStrategy.Automatic && MP <= 6500)
            PushOGCD(ClassShared.AID.LucidDreaming, Player, 20);
    }

    private bool UsePalette(OffensiveStrategy strategy)
    {
        if (strategy == OffensiveStrategy.Delay || !Unlocked(AID.SubtractivePalette) || Subtractive || Monochrome)
            return false;
        if (strategy == OffensiveStrategy.Force)
            return Palette >= 50 || Spectrum > 0;
        return Spectrum > 0 || Dying && Palette >= 50
            || Palette >= 50 && StarryIn > 35
            || Palette == 100 && Aetherhues2 > 0
            || Palette >= 50 && StarryIn < GCDLength * 1.5f && StarryStrat != OffensiveStrategy.Delay;
    }

    private bool UseStarry(Enemy target) => StarryStrat switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 20) && DowntimeIn > 15 && GCD < 1.2f
            && (!InOpener || CombatTime > 10 || Canvas.HasFlag(CanvasFlags.Wing) || Canvas.HasFlag(CanvasFlags.Maw) || !Unlocked(AID.WingMotif))
    };

    private AID LivingMuseAID
        => Canvas.HasFlag(CanvasFlags.Pom) ? AID.PomMuse
        : Canvas.HasFlag(CanvasFlags.Wing) ? AID.WingedMuse
        : Canvas.HasFlag(CanvasFlags.Claw) ? AID.ClawedMuse
        : Canvas.HasFlag(CanvasFlags.Maw) ? AID.FangedMuse
        : AID.None;

    private bool UseLiving(MuseStrategy strategy)
    {
        switch (strategy)
        {
            case MuseStrategy.Delay:
                return false;
            case MuseStrategy.ASAP:
                return true;
        }
        if (MaxChargesIn(AID.LivingMuse) <= GCD + 0.6f || Dying)
            return true;
        if (LivingMuseAID is AID.WingedMuse or AID.FangedMuse && PortraitReady)
            return false;
        if (!Unlocked(AID.StarryMuse) || StarryStrat == OffensiveStrategy.Delay || TrashAoE)
            return true;
        return StarryLeft > 0 || RaidBuffsLeft > GCD || StarryIn > 40;
    }

    private bool UseStriking(MuseStrategy strategy)
    {
        switch (strategy)
        {
            case MuseStrategy.Delay:
                return false;
            case MuseStrategy.ASAP:
                return true;
        }
        if (!Unlocked(AID.StarryMuse) || StarryStrat == OffensiveStrategy.Delay || MaxChargesIn(AID.StrikingMuse) <= GCD + 3 || Dying)
            return true;
        return StarryLeft > 0 || StarryIn is < 10 or > 60;
    }

    private bool UsePortrait(OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Dying || StarryLeft > 0 || StarryIn >= 60 || !Unlocked(AID.StarryMuse) || StarryStrat == OffensiveStrategy.Delay || TrashAoE
    };

    private bool TrashAoE => AOEMode && Bossmods.ActiveModule == null;

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => PotionPrepull || Player.InCombat && (Unlocked(AID.StarryMuse) ? StarryIn < 6 : RaidBuffsLeft > 0 || RaidBuffsIn < 5),
        PotionStrategy.AlignWithRaidBuffs => PotionWithRaidBuffs,
        PotionStrategy.Immediate => true,
        _ => false
    };

    private bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    #endregion
}
