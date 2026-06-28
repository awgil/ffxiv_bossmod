using BossMod.DRK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class DRK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, DRK.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track("Living Shadow", MinLevel = 80, Action = AID.LivingShadow)]
        public Track<OffensiveStrategy> Buffs;

        [Track("Disesteem", MinLevel = 100, Action = AID.Disesteem)]
        public Track<DisesteemStrategy> Disesteem;

        [Track("Edge of Shadow", MinLevel = 40)]
        public Track<EdgeStrategy> Edge;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum EdgeStrategy
    {
        [Option("Use to refresh Darkside, or during raid buffs", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Automatic usage, but save 3000 MP for TBN", MinLevel = 70, Targets = ActionTargets.Hostile)]
        AutomaticTBN,
        [Option("Do not use")]
        Delay,
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Use ASAP, but save 3000 MP for TBN", MinLevel = 70, Targets = ActionTargets.Hostile)]
        ForceTBN
    }

    public enum DisesteemStrategy
    {
        [Option("Use as a ranged GCD, or if raid buffs are about to expire")]
        Automatic,
        [Option("Use during burst, only if multiple targets will be hit")]
        BurstMulti,
        [Option("Do not use")]
        Delay,
        [Option("Use ASAP")]
        Force,
        [Option("Use ASAP, if multiple targets will be hit")]
        AnyMulti,
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan DRK", "Dark Knight", "Standard rotation (xan)|Tanks", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.DRK), 100).WithStrategies<Strategy>();
    }

    public int BloodWeapon;
    public int Delirium;
    public int EnhancedDelirium;
    public float SaltedEarth;
    public float Scorn;

    public float Darkside;
    public int Blood;
    public bool DarkArts;
    public int DeliriumStep;

    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumLineTargets;

    private Enemy? BestRangedAOETarget;
    private Enemy? BestLineTarget;

    public enum GCDPriority
    {
        None = 0,
        Ranged = 50,
        DisesteemRanged = 100,
        Standard = 200,
        AOE = 300,
        Blood = 400,
        BloodAOE = 500,
        Deli = 600,
        DeliAOE = 700,
        Disesteem = 900,
    }

    public enum OGCDPriority
    {
        None = 0,
        Edge = 200,
        SaltAndDarkness = 500,
        Carve = 550,
        SHB = 600,
        SaltedEarth = 650,
        Delirium = 700,
        LivingShadow = 800,
        EdgeRefresh = 900,
    }

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<DarkKnightGauge>();
        Darkside = gauge.DarksideTimer * 0.001f;
        Blood = gauge.Blood;
        DarkArts = gauge.DarkArtsState == 1;
        DeliriumStep = gauge.DeliriumStep;

        BloodWeapon = StatusStacks(SID.BloodWeapon);
        Delirium = StatusStacks(SID.Delirium);
        EnhancedDelirium = StatusStacks(SID.EnhancedDelirium);
        SaltedEarth = StatusLeft(SID.SaltedEarth);
        Scorn = StatusLeft(SID.Scorn);

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2));

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (Player.DistanceToHitbox(primaryTarget) is > 3 and <= 20 && CountdownRemaining < 0.7f)
                PushOGCD(AID.Shadowstride, primaryTarget);

            return;
        }

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.Unleash, 3, maximumActionRange: 20);

        if (ComboLastMove == AID.SyphonStrike)
            PushGCD(AID.Souleater, primaryTarget, GCDPriority.Standard);

        if (ComboLastMove == AID.HardSlash)
            PushGCD(AID.SyphonStrike, primaryTarget, GCDPriority.Standard);

        PushGCD(AID.HardSlash, primaryTarget, GCDPriority.Standard);

        if (NumAOETargets > 2)
        {
            if (ComboLastMove == AID.Unleash)
                PushGCD(AID.StalwartSoul, Player, GCDPriority.AOE);

            PushGCD(AID.Unleash, Player, GCDPriority.AOE);
        }

        Disesteem(strategy);

        if (EnhancedDelirium > GCD)
        {
            if (NumAOETargets > 1)
                PushGCD(AID.Impalement, Player, GCDPriority.DeliAOE);

            PushGCD(DeliriumStep switch
            {
                0 => AID.ScarletDelirium,
                1 => AID.Comeuppance,
                2 => AID.Torcleaver,
                _ => AID.None
            }, primaryTarget, GCDPriority.Deli);
        }

        UseBlood(strategy, primaryTarget);
    }

    private void OGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        // prepull shadow if requested
        if (strategy.Buffs == OffensiveStrategy.Force)
            PushOGCD(AID.LivingShadow, Player, OGCDPriority.LivingShadow);

        if (primaryTarget == null || !Player.InCombat)
            return;

        Edge(strategy, primaryTarget);

        if (strategy.Buffs != OffensiveStrategy.Delay)
            PushOGCD(AID.LivingShadow, Player, OGCDPriority.LivingShadow);

        if (!Unlocked(AID.Delirium))
            PushOGCD(AID.BloodWeapon, Player, OGCDPriority.Delirium);

        if (CanFitGCD(RaidBuffsLeft, 2) || RaidBuffsIn <= GCD || CombatTimer > 30)
            PushOGCD(AID.Delirium, Player, OGCDPriority.Delirium);

        if (OnCooldown(AID.Delirium))
        {
            if (NumAOETargets > 0 && DowntimeIn > 15)
                PushOGCD(AID.SaltedEarth, Player, OGCDPriority.SaltedEarth);

            if (NumLineTargets > 0 && (RaidBuffsLeft > AnimLock || RaidBuffsIn > 9000))
                PushOGCD(AID.Shadowbringer, BestLineTarget, OGCDPriority.SHB);

            if (NumRangedAOETargets > 2)
                PushOGCD(AID.AbyssalDrain, BestRangedAOETarget, OGCDPriority.Carve);

            PushOGCD(AID.CarveAndSpit, primaryTarget, OGCDPriority.Carve);

            if (SaltedEarth > 0)
                PushOGCD(AID.SaltAndDarkness, Player, OGCDPriority.SaltAndDarkness);
        }
    }

    private void UseBlood(in Strategy strategy, Enemy? primaryTarget)
    {
        if (Blood < 50 && Delirium <= GCD) // can't use
            return;

        var nextBlood = NextGCD is AID.Souleater or AID.StalwartSoul ? Blood + 20 : Blood;

        if (RaidBuffsLeft > GCD || nextBlood > 100)
        {
            if (NumAOETargets > 2)
                PushGCD(AID.Quietus, Player, GCDPriority.BloodAOE);

            PushGCD(AID.Bloodspiller, primaryTarget, GCDPriority.Blood);
        }
    }

    private void Edge(in Strategy strategy, Enemy? primaryTarget)
    {
        var canUse = MP >= 3000 || DarkArts;
        var canUseTBN = MP >= 6000 || DarkArts || !Unlocked(AID.TheBlackestNight);

        var track = strategy.Edge;
        var edgeTarget = ResolveTargetOverride(track);

        void use(OGCDPriority prio)
        {
            if (NumLineTargets > 2 || !Unlocked(AID.EdgeOfDarkness))
                PushOGCD(AID.FloodOfDarkness, edgeTarget ?? BestLineTarget, prio);

            PushOGCD(AID.EdgeOfDarkness, edgeTarget ?? primaryTarget, prio);
        }

        if (track == EdgeStrategy.Delay || !canUse)
            return;

        if (Darkside <= GCD)
        {
            use(OGCDPriority.EdgeRefresh);
            return;
        }

        var buffs = RaidBuffsLeft > GCD || RaidBuffsIn > 9000;

        switch (track.Value)
        {
            case EdgeStrategy.Automatic:
                if (buffs)
                    use(OGCDPriority.Edge);
                break;
            case EdgeStrategy.AutomaticTBN:
                if (buffs && canUseTBN)
                    use(OGCDPriority.Edge);
                break;
            case EdgeStrategy.Force:
                use(OGCDPriority.Edge);
                break;
            case EdgeStrategy.ForceTBN:
                if (canUseTBN)
                    use(OGCDPriority.Edge);
                break;
        }
    }

    void Disesteem(in Strategy strategy)
    {
        if (Scorn <= GCD || strategy.Disesteem == DisesteemStrategy.Delay)
            return;

        var isBurst = RaidBuffsLeft > GCD || RaidBuffsIn > 9000;

        var prio = strategy.Disesteem.Value switch
        {
            DisesteemStrategy.Automatic => isBurst ? GCDPriority.DisesteemRanged : GCDPriority.None,
            DisesteemStrategy.BurstMulti => NumLineTargets > 1 && isBurst ? GCDPriority.Disesteem : GCDPriority.None,
            DisesteemStrategy.AnyMulti => NumLineTargets > 1 ? GCDPriority.Disesteem : GCDPriority.None,
            DisesteemStrategy.Force => GCDPriority.Disesteem,
            _ => GCDPriority.None
        };

        // regardless of strategy, use it or lose it (except Delay, handled earlier)
        if (!CanFitGCD(Scorn, 1) || CanFitGCD(RaidBuffsLeft) && !CanFitGCD(RaidBuffsLeft, 1))
            prio = GCDPriority.Disesteem;

        PushGCD(AID.Disesteem, BestLineTarget, prio);
    }
}
