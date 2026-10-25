using BossMod.DRG;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageDRG(RotationModuleManager manager, Actor player) : KageShared<KageDRG.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Burst (Lance Charge, Battle Litany, Dragonfire Dive)", InternalName = "Burst", MinLevel = 30)]
        public Track<OffensiveStrategy> Burst;

        [Track("Lance Charge", MinLevel = 30, Action = AID.LanceCharge)]
        public Track<OffensiveStrategy> LanceCharge;

        [Track("Battle Litany", MinLevel = 52, Action = AID.BattleLitany)]
        public Track<OffensiveStrategy> BattleLitany;

        [Track("Life Surge", MinLevel = 6, Action = AID.LifeSurge)]
        public Track<LifeSurgeStrategy> LifeSurge;

        [Track("Geirskogul / Nastrond", MinLevel = 60, Actions = [AID.Geirskogul, AID.Nastrond])]
        public Track<OffensiveStrategy> Geirskogul;

        [Track("Jump / High Jump", MinLevel = 30, Actions = [AID.Jump, AID.HighJump, AID.MirageDive])]
        public Track<OffensiveStrategy> Jump;

        [Track("Dragonfire Dive", MinLevel = 50, Actions = [AID.DragonfireDive, AID.RiseOfTheDragon])]
        public Track<OffensiveStrategy> DragonfireDive;

        [Track("Stardiver", MinLevel = 80, Actions = [AID.Stardiver, AID.Starcross])]
        public Track<OffensiveStrategy> Stardiver;

        [Track("Wyrmwind Thrust", MinLevel = 90, Action = AID.WyrmwindThrust)]
        public Track<OffensiveStrategy> Wyrmwind;

        [Track("Piercing Talon", MinLevel = 15, Action = AID.PiercingTalon)]
        public Track<TalonStrategy> Talon;

        [Track("True North", MinLevel = 50, Action = ClassShared.AID.TrueNorth)]
        public Track<TrueNorthStrategy> TrueNorth;

        [Track("Winged Glide", MinLevel = 45, Action = AID.WingedGlide)]
        public Track<GlideStrategy> Glide;

        [Track("Engage")]
        public Track<EngageStrategy> Engage;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 100, UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum LifeSurgeStrategy
    {
        [Option("Use on Heavens' Thrust / Drakesbane in burst; never overcap")]
        Automatic,
        [Option("Use on the next strong GCD")]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum TalonStrategy
    {
        [Option("Use when out of melee range", Targets = ActionTargets.Hostile)]
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

    public enum GlideStrategy
    {
        [Option("Do not use")]
        None,
        [Option("Use when outside melee range", Targets = ActionTargets.Hostile)]
        GapClose
    }

    public enum EngageStrategy
    {
        [Option("Walk in and open at the pull")]
        Facepull,
        [Option("Sprint into melee range")]
        Sprint,
        [Option("Piercing Talon at the pull, then walk in")]
        Talon
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use before the pull and with Lance Charge")]
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
        [Option("High Jump first, Lance Charge after Drakesbane (FRU)")]
        FRU,
        [Option("Lance Charge + Battle Litany before the first GCD (DMU)")]
        DMU,
        [Option("No opener-specific rules")]
        None
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage DRG", "Dragoon", "Standard rotation (Kage)|Melee", "Kagekazu", RotationModuleQuality.Ok, BitMask.Build(Class.DRG, Class.LNC), 100).WithStrategies<Strategy>();
    }

    private float LotD;
    private int Focus;
    private float PowerSurge;
    private float LanceChargeLeft;
    private float BattleLitanyLeft;
    private float LifeSurgeLeft;
    private float DiveReady;
    private float NastrondReady;
    private float DragonsFlight;
    private float StarcrossReady;
    private float DraconianFire;
    private float DotLeft;

    private bool AllowAoE;
    private bool AOEMode;
    private Actor? BestLine;

    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private AID NextGCD => NextGCDAction.As<AID>();
    private bool InLotD => LotD > 0;
    private bool InBuffs => LanceChargeLeft > GCD || BattleLitanyLeft > GCD || RaidBuffsLeft > GCD;

    private OpenerStrategy OpenerMode;
    private bool InOpener => OpenerMode != OpenerStrategy.None && CombatTime < 30;
    private bool Dying;
    private bool _lastChaos;
    private bool EndOfUptime;
    private bool _talonPull;
    private Enemy? DotEnemy;
    private bool TwoTargets;

    private OffensiveStrategy LanceStrat;
    private OffensiveStrategy LitanyStrat;
    private OffensiveStrategy DiveStrat;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 3, strategy.AOE.Value);
        LanceStrat = WithBurst(strategy.Burst.Value, strategy.LanceCharge.Value);
        LitanyStrat = WithBurst(strategy.Burst.Value, strategy.BattleLitany.Value);
        DiveStrat = WithBurst(strategy.Burst.Value, strategy.DragonfireDive.Value);
        OpenerMode = !Unlocked(AID.Drakesbane) ? OpenerStrategy.None : strategy.Opener.Value switch
        {
            OpenerStrategy.Automatic => World.CurrentZone switch
            {
                1238 => OpenerStrategy.FRU,
                1363 => OpenerStrategy.DMU,
                _ => OpenerStrategy.Standard
            },
            var o => o
        };

        var gauge = World.Client.GetGauge<DragoonGauge>();
        LotD = gauge.LotdTimer * 0.001f;
        Focus = gauge.FirstmindsFocusCount;

        PowerSurge = SelfStatusLeft(SID.PowerSurge, 30);
        LanceChargeLeft = SelfStatusLeft(SID.LanceCharge);
        BattleLitanyLeft = SelfStatusLeft(SID.BattleLitany);
        LifeSurgeLeft = SelfStatusLeft(SID.LifeSurge);
        DiveReady = SelfStatusLeft(SID.DiveReady);
        NastrondReady = SelfStatusLeft(SID.NastrondReady);
        DragonsFlight = SelfStatusLeft(SID.DragonsFlight);
        StarcrossReady = SelfStatusLeft(SID.StarcrossReady);
        DraconianFire = SelfStatusLeft(SID.DraconianFire);
        DotLeft = target != null ? MaxDotLeft(target.Actor, SID.ChaoticSpring, SID.ChaosThrust) : 0;

        AllowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        var (bestLine, lineCount) = BestAOETarget(target, 10, AllowAoE, (c, e) => TargetInAOERect(e, Player.Position, Player.DirectionTo(c), 10, 2));
        BestLine = bestLine;
        AOEMode = Unlocked(AID.DoomSpike) && UseAOE(strategy.AOE.Value, lineCount, 3);

        if (UsePotion(strategy))
            PushPotion(ActionDefinitions.IDPotionStr);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (target != null)
                Engage(strategy, target, countdown);
            return;
        }

        if (target == null)
            return;

        Dying = target.Priority == Enemy.PriorityPointless;
        if (CombatTime > 30)
            _talonPull = false;
        if (ComboLastMove is AID.SpiralBlow or AID.Disembowel)
            _lastChaos = true;
        else if (ComboLastMove is AID.LanceBarrage or AID.VorpalThrust)
            _lastChaos = false;
        var dotTargets = Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 3 && DotWorthIt(e)).ToList();
        TwoTargets = !AOEMode && Unlocked(AID.ChaosThrust) && dotTargets.Count >= 2;
        DotEnemy = TwoTargets ? dotTargets.MinBy(e => MaxDotLeft(e.Actor, SID.ChaoticSpring, SID.ChaosThrust)) : null;
        var dotPayoff = GCD + GCDLength * 2 + 12;
        EndOfUptime = DowntimeIn < dotPayoff || !TimeToKill.WillLive(target.Actor, dotPayoff);

        GCDs(strategy, target);
        UpdatePositionals(strategy, target);
        AddMeleeGoal(target, AllowAoE && Unlocked(AID.DoomSpike) ? Hints.GoalAOERect(BestLine ?? target.Actor, 10, 2) : null);

        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    private void Engage(in Strategy strategy, Enemy target, float countdown)
    {
        _talonPull = strategy.Engage.Value == EngageStrategy.Talon;
        switch (strategy.Engage.Value)
        {
            case EngageStrategy.Talon:
                if (countdown < 0.85f && Unlocked(AID.PiercingTalon))
                    PushGCD(AID.PiercingTalon, target.Actor, 10);
                break;
            case EngageStrategy.Sprint:
                SprintPull(target, AID.TrueThrust, countdown);
                break;
            case EngageStrategy.Facepull:
                Facepull(target, AID.TrueThrust, countdown);
                break;
        }
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        if (!InMelee && !AOEMode && strategy.Talon.Value == TalonStrategy.Ranged)
            PushGCD(AID.PiercingTalon, target.Actor, 5);

        if (AOEMode && (Unlocked(AID.SonicThrust) || PowerSurge > GCD + GCDLength))
            ComboAoE(target);
        else
            ComboST(target);
    }

    private bool ChaosBranch => Unlocked(AID.Disembowel) && (Unlocked(AID.ChaosThrust)
        ? TwoTargets || (!_lastChaos || DotLeft == 0) && !EndOfUptime || PowerSurge < GCD + GCDLength * 2
        : PowerSurge < 15);

    private void ComboST(Enemy target)
    {
        if (ComboLeft > GCD)
        {
            switch (ComboLastMove)
            {
                case AID.TrueThrust or AID.RaidenThrust when Unlocked(AID.VorpalThrust):
                    PushGCD(ChaosBranch ? BestActionUnlocked(AID.SpiralBlow, AID.Disembowel) : BestActionUnlocked(AID.LanceBarrage, AID.VorpalThrust), ChaosBranch ? ChaosTarget(target) : target.Actor, 3);
                    return;
                case AID.Disembowel or AID.SpiralBlow when Unlocked(AID.ChaosThrust):
                    PushGCD(BestActionUnlocked(AID.ChaoticSpring, AID.ChaosThrust), ChaosTarget(target), 3);
                    return;
                case AID.ChaosThrust or AID.ChaoticSpring when Unlocked(AID.WheelingThrust):
                    PushGCD(AID.WheelingThrust, ChaosTarget(target), 3);
                    return;
                case AID.VorpalThrust or AID.LanceBarrage when Unlocked(AID.FullThrust):
                    PushGCD(BestActionUnlocked(AID.HeavensThrust, AID.FullThrust), target.Actor, 3);
                    return;
                case AID.FullThrust or AID.HeavensThrust when Unlocked(AID.FangAndClaw):
                    PushGCD(AID.FangAndClaw, target.Actor, 3);
                    return;
                case AID.WheelingThrust or AID.FangAndClaw when Unlocked(AID.Drakesbane):
                    PushGCD(AID.Drakesbane, target.Actor, 3);
                    return;
            }
        }
        PushGCD(DraconianFire > GCD && Unlocked(AID.RaidenThrust) ? AID.RaidenThrust : AID.TrueThrust, target.Actor, 1);
    }

    private void ComboAoE(Enemy target)
    {
        var line = BestLine ?? target.Actor;
        if (ComboLeft > GCD)
        {
            if (ComboLastMove is AID.DoomSpike or AID.DraconianFury && Unlocked(AID.SonicThrust))
            {
                PushGCD(AID.SonicThrust, line, 3);
                return;
            }
            if (ComboLastMove == AID.SonicThrust && Unlocked(AID.CoerthanTorment))
            {
                PushGCD(AID.CoerthanTorment, line, 3);
                return;
            }
        }
        PushGCD(DraconianFire > GCD && Unlocked(AID.DraconianFury) ? AID.DraconianFury : AID.DoomSpike, line, 1);
    }

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        var worth = TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 15) && DowntimeIn > 10;

        if (InOpener && OpenerMode == OpenerStrategy.DMU && CombatTime < 3)
        {
            if (LitanyStrat != OffensiveStrategy.Delay && CanWeave(AID.BattleLitany))
                PushOGCD(AID.BattleLitany, Player, 70);
            if (LanceStrat != OffensiveStrategy.Delay && CanWeave(AID.LanceCharge))
                PushOGCD(AID.LanceCharge, Player, 71);
        }
        if (InOpener && OpenerMode == OpenerStrategy.FRU && PowerSurge == 0 && strategy.Jump.Value != OffensiveStrategy.Delay && CanWeave(AID.HighJump))
            PushOGCD(AID.HighJump, target.Actor, 72);

        if (strategy.Glide.Value == GlideStrategy.GapClose && !InMelee && Player.DistanceToHitbox(target.Actor) <= 20)
            PushOGCD(AID.WingedGlide, ResolveTarget(strategy.Glide) ?? target.Actor, 30);

        if (PowerSurge == 0 && Unlocked(AID.Disembowel))
            return;

        if (InOpener && OpenerMode == OpenerStrategy.Standard && CombatTime < 10 && ComboLastMove is AID.TrueThrust or AID.RaidenThrust or AID.SpiralBlow)
        {
            if (_talonPull && ComboLastMove == AID.SpiralBlow && LanceStrat != OffensiveStrategy.Delay && CanWeave(AID.LanceCharge))
                PushOGCD(AID.LanceCharge, Player, 79);
            return;
        }

        if (LitanyStrat == OffensiveStrategy.Force || LitanyStrat == OffensiveStrategy.Automatic && worth)
            if (CanWeave(AID.BattleLitany))
                PushOGCD(AID.BattleLitany, Player, 80);

        var fruLanceHold = InOpener && OpenerMode == OpenerStrategy.FRU && CombatTime < GCDLength * 5;
        if (LanceStrat == OffensiveStrategy.Force || LanceStrat == OffensiveStrategy.Automatic && (worth || Dying) && !fruLanceHold)
            if (CanWeave(AID.LanceCharge))
                PushOGCD(AID.LanceCharge, Player, 79);

        var lineTarget = BestLongLine(target);
        if (strategy.Geirskogul.Value != OffensiveStrategy.Delay && !InLotD && CanWeave(AID.Geirskogul)
            && (strategy.Geirskogul.Value == OffensiveStrategy.Force || Dying || TimeToKill.WillLive(target.Actor, 10)))
            PushOGCD(AID.Geirskogul, lineTarget, 78);

        if ((strategy.Jump.Value == OffensiveStrategy.Force || strategy.Jump.Value == OffensiveStrategy.Automatic && TimeToKill.WillLive(target.Actor, 5)) && CanWeave(BestActionUnlocked(AID.HighJump, AID.Jump)))
            PushOGCD(BestActionUnlocked(AID.HighJump, AID.Jump), target.Actor, 76);

        if ((DiveStrat == OffensiveStrategy.Force || DiveStrat == OffensiveStrategy.Automatic && (worth || Dying) && DragonsFlight == 0) && CanWeave(AID.DragonfireDive))
            PushOGCD(AID.DragonfireDive, target.Actor, 75);

        if (UseLifeSurge(strategy.LifeSurge.Value))
            PushOGCD(AID.LifeSurge, Player, 77);

        if (Focus == 2 && strategy.Wyrmwind.Value != OffensiveStrategy.Delay)
        {
            if (NextGCD is AID.RaidenThrust or AID.DraconianFury)
                PushOGCD(AID.WyrmwindThrust, lineTarget, 72);
            else if (strategy.Wyrmwind.Value == OffensiveStrategy.Force || Dying || InBuffs || AOEMode || !BuffsSoon(GCD + GCDLength * 3))
                PushOGCD(AID.WyrmwindThrust, lineTarget, 69);
        }

        if (strategy.Geirskogul.Value != OffensiveStrategy.Delay && InLotD && NastrondReady > 0)
            Filler(AID.Nastrond, lineTarget, 54, LotD);

        if ((strategy.Stardiver.Value == OffensiveStrategy.Force || strategy.Stardiver.Value == OffensiveStrategy.Automatic && InLotD && StarcrossReady == 0) && CanWeave(AID.Stardiver))
            Filler(AID.Stardiver, target.Actor, 53, LotD);

        if (strategy.Stardiver.Value != OffensiveStrategy.Delay && StarcrossReady > 0)
            Filler(AID.Starcross, target.Actor, 52, StarcrossReady);

        if (DiveStrat != OffensiveStrategy.Delay && DragonsFlight > 0)
            Filler(AID.RiseOfTheDragon, target.Actor, 51, DragonsFlight);

        if (strategy.Jump.Value != OffensiveStrategy.Delay && DiveReady > 0)
            Filler(AID.MirageDive, target.Actor, 50, DiveReady);

    }

    private void Filler(AID aid, Actor target, int priority, float left)
    {
        if (left < GCD + GCDLength)
            PushOGCD(aid, target, 72);
        else if (Dying || InBuffs || !BuffsSoon(left - GCD - 1))
            PushOGCD(aid, target, priority);
    }

    private bool BuffsSoon(float within) => LanceStrat != OffensiveStrategy.Delay && ReadyIn(AID.LanceCharge) < within || HasRaidBuffJobs && RaidBuffsIn < within;

    private Actor ChaosTarget(Enemy target) => DotEnemy?.Actor ?? target.Actor;

    private bool MustMoveSoon => ForbiddenSoon(Player.Position, GCD + 0.5f);

    private bool UseLifeSurge(LifeSurgeStrategy strategy)
    {
        if (strategy == LifeSurgeStrategy.Delay || LifeSurgeLeft > 0 || !CanWeave(AID.LifeSurge) || MustMoveSoon)
            return false;

        bool strongNext;
        if (AOEMode)
            strongNext = Unlocked(AID.CoerthanTorment) ? NextGCD == AID.CoerthanTorment : NextGCD is AID.SonicThrust or AID.DoomSpike or AID.DraconianFury;
        else if (Unlocked(AID.Drakesbane))
            strongNext = NextGCD is AID.HeavensThrust or AID.FullThrust or AID.Drakesbane;
        else if (Unlocked(AID.FullThrust))
            strongNext = NextGCD == AID.FullThrust;
        else
            strongNext = NextGCD == AID.VorpalThrust;

        if (!strongNext)
            return false;
        if (strategy == LifeSurgeStrategy.ASAP || Dying || !Unlocked(AID.Drakesbane) && !AOEMode)
            return true;
        return InBuffs || MaxChargesIn(AID.LifeSurge) < GCDLength * 3 || !BuffsSoon(40);
    }

    #endregion

    #region Positionals

    private void UpdatePositionals(in Strategy strategy, Enemy target)
    {
        var (pos, imminent) = NextPositional();
        if (DotEnemy != null && NextGCD is AID.SpiralBlow or AID.Disembowel or AID.ChaoticSpring or AID.ChaosThrust or AID.WheelingThrust)
            target = DotEnemy;
        RecommendPositional(target, pos, imminent, strategy.TrueNorth.Value == TrueNorthStrategy.Automatic && InMelee);
    }

    private (Positional, bool) NextPositional()
    {
        if (AOEMode || !Unlocked(AID.ChaosThrust))
            return (Positional.Any, false);
        return NextGCD switch
        {
            AID.ChaosThrust or AID.ChaoticSpring or AID.WheelingThrust => (Positional.Rear, true),
            AID.FangAndClaw => (Positional.Flank, true),
            AID.Disembowel or AID.SpiralBlow => (Positional.Rear, false),
            AID.VorpalThrust or AID.LanceBarrage or AID.FullThrust or AID.HeavensThrust => (Positional.Flank, false),
            AID.TrueThrust or AID.RaidenThrust => (ChaosBranch ? Positional.Rear : Positional.Flank, false),
            _ => (Positional.Any, false)
        };
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => PotionPrepull || Player.InCombat && (Unlocked(AID.LanceCharge) ? ReadyIn(AID.LanceCharge) < 6 : RaidBuffsLeft > 0 || RaidBuffsIn < 5),
        PotionStrategy.AlignWithRaidBuffs => PotionWithRaidBuffs,
        PotionStrategy.Immediate => true,
        _ => false
    };

    private Actor BestLongLine(Enemy primary)
        => BestAOETarget(primary, 15, AllowAoE, (c, e) => TargetInAOERect(e, Player.Position, Player.DirectionTo(c), 15, 2)).Best ?? primary.Actor;

    #endregion
}
