using BossMod.DRK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class DRK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public enum Track { Edge = SharedTrack.Count }
    public enum EdgeStrategy
    {
        Automatic,
        AutomaticTBN,
        Delay,
        Force,
        ForceTBN
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan DRK", "Dark Knight", "xan", RotationModuleQuality.WIP, BitMask.Build(Class.DRK), 100);

        def.DefineShared().AddAssociatedActions(AID.LivingShadow);

        def.Define(Track.Edge).As<EdgeStrategy>("Edge")
            .AddOption(EdgeStrategy.Automatic, "Use to refresh Darkside, or during raid buffs")
            .AddOption(EdgeStrategy.AutomaticTBN, "Use to refresh Darkside, or during raid buffs - save MP for TBN")
            .AddOption(EdgeStrategy.Delay, "Do not use")
            .AddOption(EdgeStrategy.Force, "Use ASAP")
            .AddOption(EdgeStrategy.ForceTBN, "Use ASAP - save MP for TBN");

        return def;
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

    private Actor? BestRangedAOETarget;
    private Actor? BestLineTarget;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
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
        None = -1,
        Edge = 200,
        SaltAndDarkness = 500,
        Carve = 550,
        SHB = 600,
        SaltedEarth = 650,
        Delirium = 700,
        LivingShadow = 800,
        EdgeRefresh = 900,
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        Edge(strategy, primaryTarget);

        if (Darkside > 0 && strategy.BuffsOk())
            PushOGCD(AID.LivingShadow, Player, OGCDPriority.LivingShadow);

        if (Blood > 0 || !Unlocked(TraitID.Blackblood))
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

    private bool ShouldBlood(StrategyValues strategy)
    {
        if (Darkside < GCD)
            return false;

        if (Delirium > 0)
            return true;

        if (Blood < 50)
            return false;

        if (RaidBuffsLeft > GCD)
            return true;

        var impendingBlood = ComboLastMove == (NumAOETargets > 2 ? AID.Unleash : AID.Souleater);

        return Blood + (impendingBlood ? 20 : 0) > 100;
    }

    private void Edge(StrategyValues strategy, Actor? primaryTarget)
    {
        var canUse = MP >= 3000 || DarkArts;
        var canUseTBN = MP >= 6000 || DarkArts || !Unlocked(AID.TheBlackestNight);

        void use(OGCDPriority prio)
        {
            if (NumLineTargets > 2)
                PushOGCD(AID.FloodOfDarkness, BestLineTarget, prio);

            PushOGCD(AID.EdgeOfDarkness, primaryTarget, prio);
        }

        var track = strategy.Option(Track.Edge).As<EdgeStrategy>();
        if (track == EdgeStrategy.Delay || !canUse)
            return;

        if (Darkside < GCD)
        {
            use(OGCDPriority.EdgeRefresh);
            return;
        }

        switch (track)
        {
            case EdgeStrategy.Automatic:
                if (RaidBuffsLeft > GCD)
                    use(OGCDPriority.Edge);
                break;
            case EdgeStrategy.AutomaticTBN:
                if (RaidBuffsLeft > GCD && canUseTBN)
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
