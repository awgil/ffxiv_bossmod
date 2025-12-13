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
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2));

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
            return;

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.Unleash, 3, maximumActionRange: 20);

        if (Darkside > GCD)
        {
            if (Scorn > GCD && (RaidBuffsLeft > GCD || RaidBuffsIn > 9000))
                PushGCD(AID.Disesteem, BestLineTarget);

            if (EnhancedDelirium > 0)
            {
                if (NumAOETargets > 1)
                    PushGCD(AID.Impalement, Player);

                switch (DeliriumStep)
                {
                    case 0:
                        PushGCD(AID.ScarletDelirium, primaryTarget);
                        break;
                    case 1:
                        PushGCD(AID.Comeuppance, primaryTarget);
                        break;
                    case 2:
                        PushGCD(AID.Torcleaver, primaryTarget);
                        break;
                }
            }

            if (ShouldBlood(strategy))
            {
                if (NumAOETargets > 2)
                    PushGCD(AID.Quietus, Player);

                PushGCD(AID.Bloodspiller, primaryTarget);
            }
        }

        if (NumAOETargets > 2)
        {
            if (ComboLastMove == AID.Unleash)
                PushGCD(AID.StalwartSoul, Player);

            PushGCD(AID.Unleash, Player);
        }

        if (ComboLastMove == AID.SyphonStrike)
            PushGCD(AID.Souleater, primaryTarget);

        if (ComboLastMove == AID.HardSlash)
            PushGCD(AID.SyphonStrike, primaryTarget);

        PushGCD(AID.HardSlash, primaryTarget);
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

    private void OGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        Edge(strategy, primaryTarget);

        if (Darkside > 0 && strategy.Buffs != OffensiveStrategy.Delay)
            PushOGCD(AID.LivingShadow, Player, OGCDPriority.LivingShadow);

        if (!Unlocked(AID.Delirium))
            PushOGCD(AID.BloodWeapon, Player, OGCDPriority.Delirium);

        if (Blood > 0 || CombatTimer > 30)
            PushOGCD(AID.Delirium, Player, OGCDPriority.Delirium);

        if (!CanWeave(AID.Delirium))
        {
            if (NumAOETargets > 0)
                PushOGCD(AID.SaltedEarth, Player, OGCDPriority.SaltedEarth);

            if (NumLineTargets > 0 && (RaidBuffsLeft > 0 || RaidBuffsIn > 9000))
                PushOGCD(AID.Shadowbringer, BestLineTarget, OGCDPriority.SHB);

            if (NumRangedAOETargets > 2)
                PushOGCD(AID.AbyssalDrain, BestRangedAOETarget, OGCDPriority.Carve);

            PushOGCD(AID.CarveAndSpit, primaryTarget, OGCDPriority.Carve);

            if (SaltedEarth > 0)
                PushOGCD(AID.SaltAndDarkness, Player, OGCDPriority.SaltAndDarkness);
        }
    }

    private bool ShouldBlood(in Strategy strategy)
    {
        if (Darkside < GCD)
            return false;

        if (Delirium > 0)
            return true;

        if (Blood < 50)
            return false;

        if (RaidBuffsLeft > GCD)
            return true;

        var impendingBlood = ComboLastMove == (NumAOETargets > 2 ? AID.Unleash : AID.SyphonStrike);

        return Blood + (impendingBlood ? 20 : 0) > 100;
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

        if (Darkside < GCD)
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
}
