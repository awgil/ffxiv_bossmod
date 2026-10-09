using BossMod.SAM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageSAM(RotationModuleManager manager, Actor player) : KageRotation<KageSAM.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Rotation loop", MinLevel = 76, UiPriority = -5)]
        public Track<LoopStrategy> Loop;

        [Track("Meikyo Shisui", MinLevel = 50, Action = AID.MeikyoShisui)]
        public Track<OffensiveStrategy> Meikyo;

        [Track("Higanbana", MinLevel = 30, Action = AID.Higanbana)]
        public Track<HiganbanaStrategy> Higanbana;

        [Track("Tsubame-gaeshi", MinLevel = 76, Actions = [AID.KaeshiSetsugekka, AID.TendoKaeshiSetsugekka, AID.KaeshiGoken, AID.TendoKaeshiGoken])]
        public Track<TsubameStrategy> Tsubame;

        [Track("Ikishoten / Ogi Namikiri / Zanshin", MinLevel = 68, Actions = [AID.Ikishoten, AID.OgiNamikiri, AID.Zanshin])]
        public Track<OffensiveStrategy> Ikishoten;

        [Track("Senei / Guren", MinLevel = 70, Actions = [AID.HissatsuSenei, AID.HissatsuGuren])]
        public Track<OffensiveStrategy> Senei;

        [Track("Shoha", MinLevel = 80, Action = AID.Shoha)]
        public Track<OffensiveStrategy> Shoha;

        [Track("Shinten / Kyuten", MinLevel = 52, Actions = [AID.HissatsuShinten, AID.HissatsuKyuten])]
        public Track<KenkiStrategy> Kenki;

        [Track("Gyoten (dash)", MinLevel = 54, Action = AID.HissatsuGyoten)]
        public Track<DashStrategy> Dash;

        [Track("Yaten (backstep)", MinLevel = 56, Action = AID.HissatsuYaten)]
        public Track<YatenStrategy> Yaten;

        [Track("True North", MinLevel = 50, Action = ClassShared.AID.TrueNorth)]
        public Track<TrueNorthStrategy> TrueNorth;

        [Track("Enpi", MinLevel = 15, Action = AID.Enpi)]
        public Track<EnpiStrategy> Enpi;

        [Track("Meditate", MinLevel = 60, Action = AID.Meditate)]
        public Track<MeditateStrategy> Meditate;

        [Track("Engage")]
        public Track<EngageStrategy> Engage;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 50, UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum LoopStrategy
    {
        [Option("Pick by GCD with Fuka: Meikyo Acceleration Loop at 2.11s or slower, Standard Loop if faster")]
        Automatic,
        [Option("Meikyo Acceleration Loop (2.14s GCD)")]
        MAL,
        [Option("Standard Loop (2.07-2.08s GCD)")]
        Standard
    }

    public enum HiganbanaStrategy
    {
        [Option("Refresh every minute with the burst; only on targets that live 48s+")]
        Automatic,
        [Option("Use as soon as possible", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Do not use")]
        Delay
    }

    public enum TsubameStrategy
    {
        [Option("Hold Kaeshi: Setsugekka for the next burst; never let it expire")]
        Automatic,
        [Option("Use as soon as possible")]
        ASAP,
        [Option("Do not use")]
        Delay
    }

    public enum KenkiStrategy
    {
        [Option("Pool for raid buffs and spend inside them; never overcap")]
        Automatic,
        [Option("Only to avoid overcapping")]
        Overcap,
        [Option("Do not use")]
        Delay
    }

    public enum DashStrategy
    {
        [Option("Close gaps, and spend spare Kenki inside raid buffs", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Only to close gaps", Targets = ActionTargets.Hostile)]
        GapClose,
        [Option("Do not use")]
        Delay
    }

    public enum YatenStrategy
    {
        [Option("Backstep out of an incoming AoE when the landing spot is safe, then Enhanced Enpi")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum TrueNorthStrategy
    {
        [Option("Use when the next positional is wrong")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum EnpiStrategy
    {
        [Option("Use when out of melee range", Targets = ActionTargets.Hostile)]
        Ranged,
        [Option("Do not use")]
        Delay
    }

    public enum MeditateStrategy
    {
        [Option("Use during downtime while standing still")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum EngageStrategy
    {
        [Option("Walk into melee range during the countdown, arriving at the pull")]
        WalkIn,
        [Option("Do not move before the pull")]
        Manual
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use with raid buffs")]
        AlignWithRaidBuffs,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Standard opener (Meikyo at -14s, Gekko first)")]
        Standard,
        [Option("Kasha first, for fights with early phasing")]
        KashaFirst,
        [Option("No pre-pull Meikyo")]
        None
    }

    private enum Repeat { None, Setsugekka, TendoSetsugekka, Goken, TendoGoken }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage SAM", "Samurai", "Standard rotation (Kage)|Melee", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.SAM), 100).WithStrategies<Strategy>();
    }

    private int Kenki;
    private int Meditation;
    private SenFlags Sen;
    private int SenCount;
    private float FugetsuLeft;
    private float FukaLeft;
    private float MeikyoLeft;
    private int MeikyoStacks;
    private float TendoLeft;
    private (float Left, Repeat Kind) Tsubame;
    private bool OgiRepeat;
    private float OgiLeft;
    private float ZanshinLeft;
    private float EnhancedEnpiLeft;
    private float TrueNorthLeft;

    private bool AOEMode;
    private bool InMelee;
    private int NumCircleTargets;
    private int NumTenkaTargets;
    private Actor? BestLineTarget;
    private int NumLineTargets;
    private Actor? BestConeTarget;
    private Actor? DotTarget;

    private AID NextGCD;
    private int NextGCDPrio;
    private bool WorthBurst;
    private bool BossDying;
    private DateTime DowntimeStart;
    private DateTime ReopenUntil;

    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private float ComboLeft => World.Client.ComboState.Remaining;
    private bool HasGetsu => Sen.HasFlag(SenFlags.Getsu);
    private bool HasKa => Sen.HasFlag(SenFlags.Ka);
    private bool HasSetsu => Sen.HasFlag(SenFlags.Setsu);
    private bool HaveBuffs => FugetsuLeft > GCD + IaiCastTime && FukaLeft > GCD;
    private float IaiCastTime => Unlocked(TraitID.EnhancedIaijutsu) ? 1.3f : 1.8f;
    private float FukaGCD => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, Math.Min(World.Client.PlayerStats.Haste, 87), Player.Level);
    private float SeneiIn => Unlocked(AID.HissatsuSenei) ? ReadyIn(AID.HissatsuSenei) : float.MaxValue;
    private float SeneiCooldown => Unlocked(TraitID.EnhancedHissatsu) ? 60 : 120;
    private bool SeneiJustUsed(float within) => Unlocked(AID.HissatsuSenei) && SeneiIn > SeneiCooldown - within;
    private bool MidCombo => ComboLeft > GCD && ComboLastMove is AID.Hakaze or AID.Gyofu or AID.Jinpu or AID.Shifu or AID.Fuga or AID.Fuko;
    private bool Recovering => Player.InCombat && CombatTime > 10 && (FugetsuLeft == 0 || FukaLeft == 0);
    private bool Reopening => World.CurrentTime < ReopenUntil;
    private bool DowntimeWithin(int gcds) => DowntimeIn < GCD + GCDLength * gcds;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        NextGCD = AID.None;
        NextGCDPrio = 0;

        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 3);

        var gauge = World.Client.GetGauge<SamuraiGauge>();
        Kenki = gauge.Kenki;
        Meditation = gauge.MeditationStacks;
        Sen = gauge.SenFlags;
        SenCount = (HasGetsu ? 1 : 0) + (HasKa ? 1 : 0) + (HasSetsu ? 1 : 0);
        OgiRepeat = gauge.Kaeshi == KaeshiAction.Namikiri;

        FugetsuLeft = SelfStatusLeft(SID.Fugetsu);
        FukaLeft = SelfStatusLeft(SID.Fuka);
        (MeikyoLeft, MeikyoStacks) = SelfStatusDetails(SID.MeikyoShisui);
        TendoLeft = SelfStatusLeft(SID.Tendo);
        OgiLeft = SelfStatusLeft(SID.OgiNamikiriReady);
        ZanshinLeft = SelfStatusLeft(SID.ZanshinReady);
        EnhancedEnpiLeft = SelfStatusLeft(SID.EnhancedEnpi);
        TrueNorthLeft = SelfStatusLeft(ClassShared.SID.TrueNorth);
        Tsubame = ReadTsubame();

        var allowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        NumCircleTargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
        NumTenkaTargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 8);
        AOEMode = Unlocked(AID.Fuga) && strategy.AOE.Value switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.AOE => NumCircleTargets >= 3,
            _ => false
        };
        InMelee = target != null && Player.DistanceToHitbox(target.Actor) <= 3;
        (BestLineTarget, NumLineTargets) = BestAOETarget(target, 10, allowAoE, (c, e) => TargetInAOERect(e, Player.Position, Player.DirectionTo(c), 10, 2));
        BestConeTarget = BestAOETarget(target, 8, allowAoE, (c, e) => TargetInAOECone(e, Player.Position, 8, Player.DirectionTo(c), 60.Degrees())).Best;

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            Prepull(strategy, target, countdown);
            return;
        }

        if (target == null)
        {
            if (Player.InCombat && DowntimeStart == default)
                DowntimeStart = World.CurrentTime;
            if (strategy.Meditate.Value == MeditateStrategy.Automatic && Player.InCombat && Hints.MaxCastTime > 3 && Unlocked(AID.Meditate) && ReadyIn(AID.Meditate) <= GCD)
                PushGCD(AID.Meditate, Player, 1);
            return;
        }

        if (DowntimeStart != default)
        {
            if ((World.CurrentTime - DowntimeStart).TotalSeconds > 5)
                ReopenUntil = World.FutureTime(GCDLength * 2);
            DowntimeStart = default;
        }

        WorthBurst = TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 10);
        BossDying = Bossmods.ActiveModule?.PrimaryActor == target.Actor && !TimeToKill.WillLive(target.Actor, 5);
        DotTarget = SelectDotTarget(strategy, target);

        GCDs(strategy, target);
        UpdatePositionals(strategy, target);
        AddGoalZone(target, allowAoE);
        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    private void Prepull(in Strategy strategy, Enemy? target, float countdown)
    {
        if (target == null)
            return;
        if (strategy.Engage.Value == EngageStrategy.WalkIn && !InMelee && countdown < (Player.DistanceToHitbox(target.Actor) - 3) / 6)
            Hints.ForcedMovement = Player.DirectionTo(target.Actor).ToVec3();
        if (strategy.Opener.Value == OpenerStrategy.None || !Unlocked(AID.MeikyoShisui))
            return;
        if (MeikyoLeft == 0 && countdown < 14)
            PushGCD(AID.MeikyoShisui, Player, 10);
        if (countdown < 5 && TrueNorthLeft == 0 && HasPositionals(target.Actor) && strategy.TrueNorth.Value == TrueNorthStrategy.Automatic)
            PushOGCD(ClassShared.AID.TrueNorth, Player, 10);
        if (MeikyoLeft > countdown && countdown < 0.76f)
            PushGCD(strategy.Opener.Value == OpenerStrategy.KashaFirst ? AID.Kasha : AID.Gekko, target.Actor, 10);
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        if (OgiRepeat)
            PushGCD(AID.KaeshiNamikiri, BestConeTarget ?? target.Actor, 85);

        UseTsubame(strategy, target);
        UseIaijutsu(strategy, target);
        UseOgi(strategy, target);

        if (MeikyoLeft > GCD && MeikyoStacks > 0)
            PushGCD(MeikyoFinisher(strategy), AOEMode && MeikyoAoE ? Player : target.Actor, 40);

        if (AOEMode)
            ComboAoE(target);
        else
            ComboST(target);

        if (!InMelee && strategy.Enpi.Value == EnpiStrategy.Ranged && Player.DistanceToHitbox(target.Actor) <= 20)
            PushGCD(AID.Enpi, target.Actor, EnhancedEnpiLeft > GCD ? 6 : 5);
    }

    private (float, Repeat) ReadTsubame()
    {
        float Left(SID sid) => SelfStatusLeft(sid);
        if (Left(SID.TendoKaeshiSetsugekka) is > 0 and var ts)
            return (ts, Repeat.TendoSetsugekka);
        if (Left(SID.TendoKaeshiGoken) is > 0 and var tg)
            return (tg, Repeat.TendoGoken);
        if (Left(SID.KaeshiGoken) is > 0 and var g)
            return (g, Repeat.Goken);
        if (Left(SID.KaeshiSetsugekka) is > 0 and var s)
            return (s, Repeat.Setsugekka);
        return (0, Repeat.None);
    }

    private void UseTsubame(in Strategy strategy, Enemy target)
    {
        if (Tsubame.Kind == Repeat.None || strategy.Tsubame.Value == TsubameStrategy.Delay)
            return;

        switch (Tsubame.Kind)
        {
            case Repeat.TendoSetsugekka:
                PushGCD(AID.TendoKaeshiSetsugekka, target.Actor, 75);
                return;
            case Repeat.TendoGoken:
                PushGCD(AID.TendoKaeshiGoken, Player, 75);
                return;
            case Repeat.Goken:
                PushGCD(AID.KaeshiGoken, Player, 75);
                return;
        }

        var use = strategy.Tsubame.Value == TsubameStrategy.ASAP
            || SenCount == 3
            || Tsubame.Left < GCD + GCDLength
            || !InBossFight || NoRaidBuffs
            || Recovering
            || RaidBuffsLeft > GCD
            || SeneiIn < 7
            || MeikyoLeft > 0
            || DowntimeIn < GCD + GCDLength * 2
            || !TimeToKill.WillLive(target.Actor, GCD + GCDLength);
        if (use)
            PushGCD(AID.KaeshiSetsugekka, target.Actor, 75);
    }

    private void UseIaijutsu(in Strategy strategy, Enemy target)
    {
        if (!Unlocked(AID.Higanbana))
            return;

        if (SenCount == 1 && DotTarget != null && (HaveBuffs && !(HasSetsu && DowntimeWithin(3)) || strategy.Higanbana.Value == HiganbanaStrategy.Force))
            PushGCD(AID.Higanbana, DotTarget, 80);

        if (!HaveBuffs)
        {
            if (!AOEMode && SenCount == 3 && Tsubame.Kind != Repeat.Setsugekka && (TendoLeft > GCD || !Unlocked(AID.MeikyoShisui) || ReadyIn(AID.MeikyoShisui) > GCD && MeikyoLeft == 0))
                PushGCD(TendoLeft > GCD ? AID.TendoSetsugekka : AID.MidareSetsugekka, target.Actor, 70);
            return;
        }

        if (SenCount == 2 && Unlocked(AID.TenkaGoken) && (NumTenkaTargets >= 3 && AOEMode || !Unlocked(AID.MidareSetsugekka)))
            PushGCD(TendoLeft > GCD ? AID.TendoGoken : AID.TenkaGoken, Player, 70);

        if (SenCount == 3 && Tsubame.Kind != Repeat.Setsugekka && !(TendoLeft == 0 && MeikyoBurstNow(strategy)))
            PushGCD(TendoLeft > GCD ? AID.TendoSetsugekka : AID.MidareSetsugekka, target.Actor, 70);
    }

    private void UseOgi(in Strategy strategy, Enemy target)
    {
        if (OgiLeft <= GCD || strategy.Ikishoten.Value == OffensiveStrategy.Delay || !HaveBuffs)
            return;
        if (MeikyoLeft > 0 && MeikyoLeft < GCD + GCDLength * (2 + MeikyoStacks))
            return;

        var dotLeft = DotLeft(target.Actor);
        if (strategy.Ikishoten.Value != OffensiveStrategy.Force && !WorthBurst && OgiLeft > 8)
            return;
        var use = strategy.Ikishoten.Value == OffensiveStrategy.Force
            || OgiLeft <= 8
            || dotLeft > 50
            || dotLeft > 15 && (RaidBuffsLeft > GCD || !HasRaidBuffJobs)
            || AOEMode;
        if (use)
            PushGCD(AID.OgiNamikiri, BestConeTarget ?? target.Actor, 78);
    }

    private bool MeikyoBurstNow(in Strategy strategy)
        => Unlocked(AID.TendoSetsugekka) && MeikyoLeft == 0 && strategy.Meikyo.Value != OffensiveStrategy.Delay && WorthBurst
        && ReadyIn(AID.MeikyoShisui) <= GCD && (SeneiIn < 7 || HasRaidBuffJobs && RaidBuffsLeft > GCD);

    private bool MeikyoAoE => Unlocked(AID.Oka) && NumCircleTargets >= 4;

    private AID MeikyoFinisher(in Strategy strategy)
    {
        if (AOEMode && MeikyoAoE)
        {
            if (!HasGetsu || FugetsuLeft <= FukaLeft && HasKa)
                return Unlocked(AID.Mangetsu) ? AID.Mangetsu : AID.Oka;
            return AID.Oka;
        }

        if (strategy.Opener.Value == OpenerStrategy.KashaFirst && CombatTime < 10 && FukaLeft == 0 && Unlocked(AID.Kasha))
            return AID.Kasha;
        if (Unlocked(AID.Gekko) && (!HasGetsu || FugetsuLeft < GCD + GCDLength))
            return AID.Gekko;
        if (Unlocked(AID.Kasha) && (!HasKa || FukaLeft < GCD + GCDLength))
            return AID.Kasha;
        if (Unlocked(AID.Yukikaze) && !HasSetsu)
            return AID.Yukikaze;
        return FugetsuLeft <= FukaLeft || !Unlocked(AID.Kasha) ? AID.Gekko : AID.Kasha;
    }

    private void ComboST(Enemy target)
    {
        var starter = Unlocked(AID.Gyofu) ? AID.Gyofu : AID.Hakaze;
        if (ComboLeft > GCD)
        {
            if (ComboLastMove is AID.Hakaze or AID.Gyofu)
            {
                var next = AfterStarter(target.Actor);
                if (next != AID.None)
                {
                    PushGCD(next, target.Actor, next == AID.Yukikaze ? 35 : 30);
                    return;
                }
            }
            if (ComboLastMove == AID.Jinpu && Unlocked(AID.Gekko))
            {
                PushGCD(AID.Gekko, target.Actor, 35);
                return;
            }
            if (ComboLastMove == AID.Shifu && Unlocked(AID.Kasha))
            {
                PushGCD(AID.Kasha, target.Actor, 35);
                return;
            }
        }
        PushGCD(starter, target.Actor, 10);
    }

    private AID AfterStarter(Actor target)
    {
        var refreshFuka = FukaLeft <= FugetsuLeft;
        if (!Unlocked(AID.Gekko))
        {
            if (Unlocked(AID.Shifu) && (FukaLeft == 0 || FugetsuLeft > 0 && refreshFuka))
                return AID.Shifu;
            return Unlocked(AID.Jinpu) ? AID.Jinpu : AID.None;
        }

        if (Unlocked(AID.Yukikaze) && !HasSetsu && (DowntimeWithin(2) || FugetsuLeft > 7 && (FukaLeft > 7 || !Unlocked(AID.Kasha))))
            return AID.Yukikaze;

        var pos = CurrentPositional(target);
        if (Unlocked(AID.Shifu) && (FukaLeft == 0
            || Unlocked(AID.Kasha) && !HasKa && (pos is Positional.Flank or Positional.Front || HasGetsu)
            || SenCount == 3 && refreshFuka))
            return AID.Shifu;
        if (Unlocked(AID.Jinpu))
            return AID.Jinpu;
        return Unlocked(AID.Shifu) ? AID.Shifu : AID.None;
    }

    private void ComboAoE(Enemy target)
    {
        var starter = Unlocked(AID.Fuko) ? AID.Fuko : AID.Fuga;
        if (ComboLeft > GCD && ComboLastMove is AID.Fuga or AID.Fuko)
        {
            var refreshFuka = FukaLeft <= FugetsuLeft;
            if (Unlocked(AID.Oka) && (!HasKa || FukaLeft == 0 || SenCount >= 2 && refreshFuka))
            {
                PushGCD(AID.Oka, Player, 30);
                return;
            }
            if (Unlocked(AID.Mangetsu))
            {
                PushGCD(AID.Mangetsu, Player, 30);
                return;
            }
        }
        PushGCD(starter, starter == AID.Fuko ? Player : BestConeTarget ?? target.Actor, 10);
    }

    private Actor? SelectDotTarget(in Strategy strategy, Enemy target)
    {
        if (strategy.Higanbana.Value == HiganbanaStrategy.Delay)
            return null;
        if (strategy.Higanbana.Value == HiganbanaStrategy.Force)
            return ResolveTarget(strategy.Higanbana) ?? target.Actor;
        if (BanaWanted(target, 48))
            return target.Actor;
        return Hints.PriorityTargets.Where(e => e != target && Player.DistanceToHitbox(e.Actor) <= 6 && BanaWanted(e, 30)).MaxBy(e => e.Actor.HPMP.CurHP)?.Actor;
    }

    private bool BanaWanted(Enemy e, float minLife)
    {
        if (e.ForbidDOTs || !TimeToKill.WillLive(e.Actor, minLife) || DowntimeIn < 15)
            return false;
        var left = DotLeft(e.Actor);
        if (left == 0)
            return true;
        if (left > 15)
            return false;
        if (left <= GCD + GCDLength)
            return true;
        if (SeneiIn < 7)
            return false;
        if (Unlocked(TraitID.EnhancedHissatsu))
            return SeneiJustUsed(35) || Unlocked(AID.Ikishoten) && ReadyIn(AID.Ikishoten) > 85;
        return true;
    }

    private float DotLeft(Actor target) => target.FindStatus((uint)SID.Higanbana, Player.InstanceID) is { } st ? StatusDuration(st.ExpireAt) : 0;

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        UseMeikyo(strategy, target);

        if (strategy.Senei.Value != OffensiveStrategy.Delay && Kenki >= 25 && (strategy.Senei.Value == OffensiveStrategy.Force || (WorthBurst || BossDying) && SeneiWanted()))
        {
            if (Unlocked(AID.HissatsuGuren) && NumLineTargets >= 2 && CanWeave(AID.HissatsuGuren))
                PushOGCD(AID.HissatsuGuren, BestLineTarget ?? target.Actor, 60);
            else if (Unlocked(AID.HissatsuSenei) ? CanWeave(AID.HissatsuSenei) : CanWeave(AID.HissatsuGuren))
                PushOGCD(Unlocked(AID.HissatsuSenei) ? AID.HissatsuSenei : AID.HissatsuGuren, Unlocked(AID.HissatsuSenei) ? target.Actor : BestLineTarget ?? target.Actor, 60);
        }

        if (strategy.Ikishoten.Value != OffensiveStrategy.Delay && CanWeave(AID.Ikishoten) && ZanshinLeft == 0 && Kenki <= 50
            && (strategy.Ikishoten.Value == OffensiveStrategy.Force || WorthBurst && DowntimeIn > 10 && (!Unlocked(AID.HissatsuSenei) || SeneiJustUsed(20) || SeneiIn <= GCD + GCDLength * 2)))
            PushOGCD(AID.Ikishoten, Player, 58);

        if (strategy.Ikishoten.Value != OffensiveStrategy.Delay && ZanshinLeft > 0 && Kenki >= 50 && CanWeave(AID.Zanshin)
            && (ZanshinLeft <= 8 || BossDying || WorthBurst && (RaidBuffsLeft > GCD || !HasRaidBuffJobs || RaidBuffsIn > ZanshinLeft)))
            PushOGCD(AID.Zanshin, BestConeTarget ?? target.Actor, 56);

        if (strategy.Shoha.Value != OffensiveStrategy.Delay && Meditation >= 3 && CanWeave(AID.Shoha)
            && (strategy.Shoha.Value == OffensiveStrategy.Force || BossDying || GrantsMeditation(NextGCD) || RaidBuffsLeft > GCD || !HasRaidBuffJobs && SeneiIn >= 7))
            PushOGCD(AID.Shoha, BestLineTarget ?? target.Actor, 54);

        if (strategy.Kenki.Value != KenkiStrategy.Delay && ShouldSpendKenki(strategy))
        {
            if (Unlocked(AID.HissatsuKyuten) && NumCircleTargets >= 3 && CanWeave(AID.HissatsuKyuten))
                PushOGCD(AID.HissatsuKyuten, Player, 50);
            else if (CanWeave(AID.HissatsuShinten))
                PushOGCD(AID.HissatsuShinten, target.Actor, 50);
        }

        if (BossDying && SenCount is > 0 and < 3 && Kenki <= 100 - 10 * SenCount && CanWeave(AID.Hagakure))
            PushOGCD(AID.Hagakure, Player, 52);

        UseYaten(strategy, target);
        UseGyoten(strategy, target);
    }

    private void UseGyoten(in Strategy strategy, Enemy target)
    {
        if (strategy.Dash.Value == DashStrategy.Delay || Kenki < 10 || !CanWeave(AID.HissatsuGyoten))
            return;
        var dashTarget = ResolveTarget(strategy.Dash) ?? target.Actor;
        var dist = Player.DistanceToHitbox(dashTarget);
        if (dist is > 3 and <= 20)
        {
            if (Hints.MaxCastTime > 0 && !Hints.ForbiddenZones.Any(z => z.activation <= World.FutureTime(3) && z.shape.Check(dashTarget.Position)))
                PushOGCD(AID.HissatsuGyoten, dashTarget, 30);
            return;
        }
        if (strategy.Dash.Value == DashStrategy.Automatic && InMelee && Kenki < 25 && RaidBuffsLeft > GCD && RaidBuffsLeft < GCD + GCDLength * 2 && ZanshinLeft == 0)
            PushOGCD(AID.HissatsuGyoten, target.Actor, 30);
    }

    private void UseYaten(in Strategy strategy, Enemy target)
    {
        if (strategy.Yaten.Value == YatenStrategy.Delay || !InMelee || Kenki < 10 || !CanWeave(AID.HissatsuYaten))
            return;
        var soon = World.FutureTime(2.5f);
        if (!Hints.ForbiddenZones.Any(z => z.activation <= soon && z.shape.Check(Player.Position)))
            return;
        var away = (Player.Position - target.Actor.Position).Normalized();
        var landing = Player.Position + away * 10;
        if (!Hints.PathfindMapBounds.Contains(landing - Hints.PathfindMapCenter) || Hints.ForbiddenZones.Any(z => z.activation <= World.FutureTime(5) && z.shape.Check(landing)))
            return;
        PushOGCD(AID.HissatsuYaten, target.Actor, 75);
    }

    private void UseMeikyo(in Strategy strategy, Enemy target)
    {
        if (strategy.Meikyo.Value == OffensiveStrategy.Delay || !Unlocked(AID.MeikyoShisui) || MeikyoLeft > 0 || !CanWeave(AID.MeikyoShisui))
            return;
        if (strategy.Meikyo.Value == OffensiveStrategy.Force)
        {
            PushOGCD(AID.MeikyoShisui, Player, 70);
            return;
        }
        if (TendoLeft > 0 || MidCombo && !Reopening || DowntimeIn < GCDLength * 3 || !WorthBurst && CombatTime > 25)
            return;

        if (AOEMode)
        {
            PushOGCD(AID.MeikyoShisui, Player, 70);
            return;
        }

        var dotLeft = DotLeft(target.Actor);
        var use = strategy.Opener.Value != OpenerStrategy.None && CombatTime < 25 && SenCount == 0 && Tsubame.Kind == Repeat.None && Unlocked(AID.TendoSetsugekka)
            || SenCount == 0 && DotTarget != null && dotLeft <= 15
            || Recovering
            || Reopening && SenCount < 3
            || MaxChargesIn(AID.MeikyoShisui) <= GCD + GCDLength && SeneiIn >= 7
            || !Unlocked(AID.HissatsuSenei)
            || UseMAL(strategy) && NeedAcceleration()
            || SeneiIn < 7;
        if (use)
            PushOGCD(AID.MeikyoShisui, Player, 70);
    }

    private bool UseMAL(in Strategy strategy) => strategy.Loop.Value switch
    {
        LoopStrategy.MAL => true,
        LoopStrategy.Standard => false,
        _ => FukaGCD >= 2.11f
    };

    private bool NeedAcceleration()
    {
        if (SenCount == 3)
            return false;
        var gcds = SenCount switch { 0 => 8, 1 => 5, _ => 3 };
        return gcds * GCDLength > SeneiIn + GCD;
    }

    private bool SeneiWanted()
    {
        if (!HasRaidBuffJobs || RaidBuffsLeft > GCD || RaidBuffsIn > 45)
            return true;
        return Recovering && (!Unlocked(AID.MeikyoShisui) || ReadyIn(AID.MeikyoShisui) > GCD) && MeikyoLeft == 0;
    }

    private bool ShouldSpendKenki(in Strategy strategy)
    {
        if (Kenki < 25)
            return false;
        if (Kenki >= 95 || BossDying)
            return true;
        if (strategy.Kenki.Value == KenkiStrategy.Overcap)
            return Kenki >= 90;

        var reserve = (ZanshinLeft > 0 ? 50 : 0) + (Unlocked(AID.HissatsuSenei) && SeneiIn < 7 ? 25 : 0);
        if (Unlocked(AID.Ikishoten) && ZanshinLeft == 0 && Kenki > 50 && ReadyIn(AID.Ikishoten) <= GCD + GCDLength * 4)
            return Kenki - 25 >= reserve;
        if (RaidBuffsLeft > GCD || SeneiJustUsed(20))
            return Kenki - 25 >= reserve;
        return Kenki >= (HasRaidBuffJobs ? 90 : 65) && Kenki - 25 >= reserve;
    }

    private static bool GrantsMeditation(AID aid) => aid is AID.Higanbana or AID.MidareSetsugekka or AID.TenkaGoken or AID.TendoSetsugekka or AID.TendoGoken or AID.OgiNamikiri;

    #endregion

    #region Positionals

    private void UpdatePositionals(in Strategy strategy, Enemy target)
    {
        var actor = target.Actor;
        var (pos, imminent) = NextGCD switch
        {
            AID.Gekko => (Positional.Rear, true),
            AID.Kasha => (Positional.Flank, true),
            AID.Jinpu => (Positional.Rear, false),
            AID.Shifu => (Positional.Flank, false),
            _ => (Positional.Any, false)
        };
        if (AOEMode || !Unlocked(AID.Gekko) || !HasPositionals(actor) || actor.TargetID == Player.InstanceID && actor.CastInfo == null && !actor.IsStrikingDummy || target.Priority < 0)
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

    private void AddGoalZone(Enemy target, bool allowAoE)
    {
        var (_, pos, imminent, _) = Hints.RecommendedPositional;
        var single = Hints.GoalSingleTarget(target.Actor, imminent ? pos : Positional.Any, Player, World.Actors, 3);
        Hints.GoalZones.Add(allowAoE && Unlocked(AID.Fuga) ? GoalCombined(single, Hints.GoalAOECircle(5), 3) : single);
    }

    private Positional CurrentPositional(Actor target)
    {
        var dir = target.Rotation.ToDirection().Dot((Player.Position - target.Position).Normalized());
        return dir < -0.7071068f ? Positional.Rear : dir < 0.7071068f ? Positional.Flank : Positional.Front;
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsLeft > 0 || RaidBuffsIn < 5),
        PotionStrategy.Immediate => true,
        _ => false
    };

    private bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid);

    private void PushGCD(AID aid, Actor? target, int priority, float castTime = 0)
    {
        if (castTime == 0 && aid is AID.Higanbana or AID.MidareSetsugekka or AID.TenkaGoken or AID.TendoSetsugekka or AID.TendoGoken or AID.OgiNamikiri)
            castTime = MathF.Max(0, IaiCastTime - 0.5f);
        if (PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + priority, castTime: castTime) && priority > NextGCDPrio)
        {
            NextGCD = aid;
            NextGCDPrio = priority;
        }
    }

    #endregion
}
