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

        [Track("Unmend", MinLevel = 15, Action = AID.Unmend)]
        public Track<DisabledByDefault> Unmend;

        [Track("Living Shadow", MinLevel = 80, Action = AID.LivingShadow, OGCDPriority = OGCDPriority.LivingShadow)]
        public Track<OffensiveStrategy> Buffs;

        [Track("Disesteem", MinLevel = 100, Action = AID.Disesteem)]
        public Track<DisesteemStrategy> Disesteem;

        [Track("Edge of Shadow", MinLevel = 40, Actions = [AID.EdgeOfDarkness, AID.EdgeOfShadow, AID.FloodOfDarkness, AID.FloodOfShadow], OGCDPriority = OGCDPriority.Edge)]
        public Track<EdgeStrategy> Edge;

        [Track("Bloodspiller", MinLevel = 62, Actions = [AID.Bloodspiller, AID.Quietus])]
        public Track<OffensiveStrategy> Blood;

        [Track("Delirium", MinLevel = 35, Actions = [AID.BloodWeapon, AID.Delirium], OGCDPriority = OGCDPriority.Delirium)]
        public Track<OffensiveStrategy> Delirium;

        [Track("Salted Earth", MinLevel = 52, Actions = [AID.SaltedEarth, AID.SaltAndDarkness], OGCDPriority = OGCDPriority.SaltedEarth)]
        public Track<OffensiveStrategy> Salt;

        [Track("Shadowbringer", MinLevel = 90, Action = AID.Shadowbringer, OGCDPriority = OGCDPriority.SHB)]
        public Track<OffensiveStrategy> ShB;

        [Track("Carve & Spit", MinLevel = 60, Actions = [AID.CarveAndSpit, AID.AbyssalDrain], OGCDPriority = OGCDPriority.Carve)]
        public Track<CarveStrategy> Carve;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum EdgeStrategy
    {
        [Option("Use to refresh Darkside, during raid buffs, or to prevent Dark Arts overcap", Targets = ActionTargets.Hostile)]
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

    public enum CarveStrategy
    {
        [Option("Automatically choose action; use on cooldown")]
        Automatic,
        [Option("Don't use")]
        Delay,
        [Option("Use C&S ASAP")]
        ForceCarve,
        [Option("Use Abyssal Drain ASAP")]
        ForceDrain
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

    private Actor? Salt;

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
        SHB = 600,
        Carve = 610,
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

        Salt = World.Actors.FirstOrDefault(a => a.OID == 0x17C && a.OwnerID == Player.InstanceID);

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, (primary, other) => TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2));

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < 0.98f)
                PushGCD(AID.Unmend, primaryTarget);

            OGCD(strategy, primaryTarget);

            return;
        }

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.Unleash, 3, maximumActionRange: 20);

        if (strategy.Unmend.IsEnabled())
            PushGCD(AID.Unmend, ResolveEnemy(strategy.Unmend) ?? primaryTarget, GCDPriority.Ranged);

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

        if (EnhancedDelirium > GCD && Darkside > GCD)
        {
            if (NumAOETargets > 2)
                PushGCD(AID.Impalement, Player, GCDPriority.DeliAOE);

            // should always use these even on dying targets as they restore mp, plus if the target is dying that probably means impending phase transition so we would lose the buff
            PushGCD(DeliriumStep switch
            {
                0 => AID.ScarletDelirium,
                1 => AID.Comeuppance,
                2 => AID.Torcleaver,
                _ => AID.None
            }, primaryTarget, GCDPriority.Deli);
        }

        UseBlood(strategy, primaryTarget);

        OGCD(strategy, primaryTarget);
    }

    private void OGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        if (SaltedEarth > 0 && Salt is { } s && Hints.NumPriorityTargetsInAOECircle(s.Position, 5) > 0)
            PushOGCD(AID.SaltAndDarkness, Player, OGCDPriority.SaltAndDarkness);

        if (strategy.Buffs.Value switch
        {
            OffensiveStrategy.Automatic => Player.InCombat && DowntimeIn > 22,
            OffensiveStrategy.Force => true,
            _ => false
        })
            UsePlanned(strategy.Buffs, AID.LivingShadow, Player);

        Edge(strategy, primaryTarget);

        if (strategy.Delirium.Value switch
        {
            OffensiveStrategy.Automatic => Player.InCombat && (CanFitGCD(RaidBuffsLeft, 2) || RaidBuffsIn <= GCD || CombatTimer > 30),
            OffensiveStrategy.Force => true,
            _ => false
        })
            UsePlanned(strategy.Delirium, Unlocked(AID.Delirium) ? AID.Delirium : AID.BloodWeapon, Player);

        if (strategy.Salt.Value switch
        {
            OffensiveStrategy.Automatic => Player.InCombat && NumAOETargets > 0 && DowntimeIn > 15 && (HaveRaidBuffsUntil(AnimLock) || CombatTimer > 10),
            OffensiveStrategy.Force => true,
            _ => false
        })
            UsePlanned(strategy.Salt, AID.SaltedEarth, Player);

        if (Darkside > AnimLock && strategy.ShB.Value switch
        {
            OffensiveStrategy.Automatic => Player.InCombat && HaveRaidBuffsUntil(AnimLock),
            OffensiveStrategy.Force => true,
            _ => false
        })
            // +20 prio so overcapped shb (620) is used before c&s (610)
            UsePlanned(strategy.ShB, AID.Shadowbringer, BestLineTarget?.Actor, additionalPriority: MaxChargesIn(AID.Shadowbringer) <= GCD ? 20 : 0);

        switch (strategy.Carve.Value)
        {
            case CarveStrategy.Automatic:
                if (Player.InCombat && (HaveRaidBuffsUntil(AnimLock) || CombatTimer > 10))
                {
                    if (NumRangedAOETargets > 2)
                        UsePlanned(strategy.Carve, AID.AbyssalDrain, BestRangedAOETarget, additionalPriority: 1);

                    UsePlanned(strategy.Carve, AID.CarveAndSpit, primaryTarget);
                }
                break;

            case CarveStrategy.ForceCarve:
                UsePlanned(strategy.Carve, AID.CarveAndSpit, primaryTarget);
                break;
            case CarveStrategy.ForceDrain:
                UsePlanned(strategy.Carve, AID.AbyssalDrain, BestRangedAOETarget ?? primaryTarget);
                break;
        }
    }

    private void UseBlood(in Strategy strategy, Enemy? primaryTarget)
    {
        if (Blood < 50 && Delirium <= GCD) // can't use
            return;

        var nextBlood = Blood;

        if (NextGCD is AID.Souleater or AID.StalwartSoul)
            nextBlood += 20;

        // deli will give us +30 blood starting on next gcd (and we can't spend gauge during it) so prevent overcap
        if (strategy.Delirium != OffensiveStrategy.Delay && CanWeave(AID.Delirium, 1) && (CanFitGCD(RaidBuffsLeft, 3) || RaidBuffsIn <= GCD + GCDLength || CombatTimer > 30))
            nextBlood += 30;

        if (strategy.Blood.Value switch
        {
            OffensiveStrategy.Automatic => HaveRaidBuffs || nextBlood > 100,
            OffensiveStrategy.Force => true,
            _ => false
        })
        {
            if (NumAOETargets > 2)
                PushGCD(AID.Quietus, Player, GCDPriority.BloodAOE);

            PushGCD(AID.Bloodspiller, primaryTarget, GCDPriority.Blood, useOnDyingTarget: false);
        }
    }

    private void Edge(in Strategy strategy, Enemy? primaryTarget)
    {
        var canUse = MP >= 3000 || DarkArts;
        var canUseTBN = MP >= 6000 || DarkArts || !Unlocked(AID.TheBlackestNight);

        var track = strategy.Edge;

        void use(OGCDPriority prio, bool useOnDyingTarget)
        {
            var pExtra = (int)prio - (int)OGCDPriority.Edge;

            if (NumLineTargets > 2 || !Unlocked(AID.EdgeOfDarkness))
                UsePlanned(track, AID.FloodOfDarkness, BestLineTarget, additionalPriority: pExtra + 1, predicate: e => useOnDyingTarget || e?.Priority >= 0);

            UsePlanned(track, AID.EdgeOfDarkness, primaryTarget, additionalPriority: pExtra, predicate: e => useOnDyingTarget || e?.Priority >= 0);
        }

        if (track == EdgeStrategy.Delay || !canUse)
            return;

        if (Darkside <= GCD + 0.6f + AnimationLockDelay && Player.InCombat)
        {
            use(OGCDPriority.EdgeRefresh, true);
            return;
        }

        var canUseAuto = Player.InCombat && (HaveRaidBuffsUntil(AnimLock) || DarkArts && World.Party.WithoutSlot().Any(p => p.FindStatus(SID.TheBlackestNight, Player.InstanceID, DateTime.MaxValue) != null));

        switch (track.Value)
        {
            case EdgeStrategy.Automatic:
                if (canUseAuto)
                    use(OGCDPriority.Edge, false);
                break;
            case EdgeStrategy.AutomaticTBN:
                if (canUseAuto && canUseTBN)
                    use(OGCDPriority.Edge, false);
                break;
            case EdgeStrategy.Force:
                use(OGCDPriority.Edge, true);
                break;
            case EdgeStrategy.ForceTBN:
                if (canUseTBN)
                    use(OGCDPriority.Edge, true);
                break;
        }
    }

    void Disesteem(in Strategy strategy)
    {
        if (Scorn <= GCD || strategy.Disesteem == DisesteemStrategy.Delay)
            return;

        var prio = strategy.Disesteem.Value switch
        {
            DisesteemStrategy.Automatic => HaveRaidBuffs ? GCDPriority.DisesteemRanged : GCDPriority.None,
            DisesteemStrategy.BurstMulti => NumLineTargets > 1 && HaveRaidBuffs ? GCDPriority.Disesteem : GCDPriority.None,
            DisesteemStrategy.AnyMulti => NumLineTargets > 1 ? GCDPriority.Disesteem : GCDPriority.None,
            DisesteemStrategy.Force => GCDPriority.Disesteem,
            _ => GCDPriority.None
        };

        // regardless of strategy, use it or lose it (except Delay, handled earlier)
        // TODO: this is a hack because no party perfectly aligns their buffs per-GCD, we need to track *all* active buffs and check when the first one is going away
        if (!CanFitGCD(Scorn, 1) || CanFitGCD(RaidBuffsLeft) && !CanFitGCD(RaidBuffsLeft, 2))
            prio = GCDPriority.Disesteem;

        PushGCD(AID.Disesteem, BestLineTarget, prio);
    }
}
