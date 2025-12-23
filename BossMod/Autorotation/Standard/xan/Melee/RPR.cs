using BossMod.RPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class RPR(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, RPR.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track("Arcane Circle", MinLevel = 72, Action = AID.ArcaneCircle)]
        public Track<OffensiveStrategy> Buffs;

        [Track(Action = AID.Enshroud, MinLevel = 80)]
        public Track<OffensiveStrategy> Enshroud;

        [Track(Action = AID.Communio, MinLevel = 90, Targets = ActionTargets.Hostile)]
        public Track<EnabledByDefault> Communio;

        [Track("Soul Slice", Actions = [AID.SoulSlice, AID.SoulScythe], MinLevel = 60)]
        public Track<SliceStrategy> Slice;

        [Track("Soul Gauge", Actions = [AID.BloodStalk, AID.GrimSwathe, AID.Gluttony], MinLevel = 50)]
        public Track<RedGaugeStrategy> RedGauge;

        [Track("Soul Reaver", Actions = [AID.Gallows, AID.Guillotine, AID.ExecutionersGallows, AID.ExecutionersGuillotine, AID.Gibbet, AID.ExecutionersGibbet], MinLevel = 70)]
        public Track<SoulReaverStrategy> Reaver;

        [Track("Plentiful Harvest", Action = AID.PlentifulHarvest, MinLevel = 88, Targets = ActionTargets.Hostile)]
        public Track<EnabledByDefault> PH;

        [Track("Harvest Moon", Action = AID.HarvestMoon, MinLevel = 82, Targets = ActionTargets.Hostile)]
        public Track<OffensiveStrategy> HM;

        [Track("Perfectio", Action = AID.Perfectio, MinLevel = 100, Targets = ActionTargets.Hostile)]
        public Track<PerfectioStrategy> Perf;

        [Track(Action = AID.Harpe)]
        public Track<HarpeStrategy> Harpe;

        [Track("Soulsow (during combat)", Action = AID.Soulsow, MinLevel = 82)]
        public Track<DisabledByDefault> Soulsow;

        [Track("Auto-Arcane Crest", InternalName = "Crest", Action = AID.ArcaneCrest, MinLevel = 40)]
        public Track<EnabledByDefault> AutoCrest;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum SliceStrategy
    {
        [Option("Use on cooldown, unless it would overcap gauge", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Don't use")]
        Delay,
        [Option("Use ASAP, even if it would overcap gauge", Targets = ActionTargets.Hostile)]
        Force
    }

    public enum RedGaugeStrategy
    {
        [Option("Use ASAP, unless shroud gauge is full or Gluttony is almost off cooldown", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Use Blood Stalk/Grim Swathe at max gauge, otherwise hold", Targets = ActionTargets.Hostile, MinLevel = 76)]
        ReserveGluttony,
        [Option("Use Stalk/Gluttony ASAP, ignoring timers", Targets = ActionTargets.Hostile, MinLevel = 76)]
        Force,
        [Option("Do not use")]
        Delay
    }

    public enum SoulReaverStrategy
    {
        [Option("Hold other GCDs while Soul Reaver is active")]
        Automatic,
        [Option("Use regular GCDs, dropping any remaining Soul Reaver stacks")]
        ForceBreak
    }

    public enum HarpeStrategy
    {
        [Option("Use out of melee range if Enhanced Harpe is active", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Don't use")]
        Forbid,
        [Option("Always use when out of melee range", Targets = ActionTargets.Hostile)]
        Ranged,
    }

    public enum PerfectioStrategy
    {
        [Option("Use in raid buffs", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Save for melee downtime", Targets = ActionTargets.Hostile)]
        Ranged,
        [Option("Don't use")]
        Delay,
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan RPR", "Reaper", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.RPR), 100).WithStrategies<Strategy>();
    }

    public int RedGauge;
    public int BlueGauge;
    public bool Soulsow;
    public float EnshroudLeft;
    public int BlueSouls;
    public int PurpleSouls;
    public bool SoulReaver;
    public bool Executioner;
    public float EnhancedGallows;
    public float EnhancedGibbet;
    public (float Left, int Stacks) ImmortalSacrifice;
    public float BloodsownCircle;
    public float IdealHost;
    public float EnhancedVoidReaping;
    public float EnhancedCrossReaping;
    public float EnhancedHarpe;
    public float Oblatio;
    public float PerfectioParata;

    public float TargetDDLeft;
    public float ShortestNearbyDDLeft;

    public int NumAOETargets; // melee
    public int NumRangedAOETargets; // gluttony, communio
    public int NumConeTargets; // grim swathe, guillotine
    public int NumLineTargets; // plentiful harvest

    private Enemy? BestRangedAOETarget;
    private Enemy? BestConeTarget;
    private Enemy? BestLineTarget;

    public enum GCDPriority
    {
        None = 0,
        Soulsow = 1,
        EnhancedHarpe = 100,
        HarvestMoon = 150,
        PerfectioRanged = 175,
        Filler = 200,
        FillerAOE = 201,
        SoulSlice = 300,
        DDExtend = 600,
        Harvest = 650,
        Reaver = 700,
        Lemure = 700,
        EnshroudMove = 720,
        Communio = 750,
        DDExpiring = 900,
        Max = 990
    }

    public enum OGCDPriority
    {
        Default = 1
    }

    private bool Enshrouded => BlueSouls > 0;

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);

        var gauge = World.Client.GetGauge<ReaperGauge>();

        RedGauge = gauge.Soul;
        BlueGauge = gauge.Shroud;
        EnshroudLeft = gauge.EnshroudedTimeRemaining * 0.001f;
        BlueSouls = gauge.LemureShroud;
        PurpleSouls = gauge.VoidShroud;

        Soulsow = Player.FindStatus(SID.Soulsow) != null;
        Executioner = StatusLeft(SID.Executioner) > GCD;
        SoulReaver = StatusLeft(SID.SoulReaver) > GCD || Executioner;
        EnhancedGallows = StatusLeft(SID.EnhancedGallows);
        EnhancedGibbet = StatusLeft(SID.EnhancedGibbet);
        ImmortalSacrifice = Status(SID.ImmortalSacrifice);
        BloodsownCircle = StatusLeft(SID.BloodsownCircle);
        IdealHost = StatusLeft(SID.IdealHost);
        EnhancedVoidReaping = StatusLeft(SID.EnhancedVoidReaping);
        EnhancedCrossReaping = StatusLeft(SID.EnhancedCrossReaping);
        EnhancedHarpe = StatusLeft(SID.EnhancedHarpe);
        Oblatio = StatusLeft(SID.Oblatio);
        PerfectioParata = StatusLeft(SID.PerfectioParata);

        TargetDDLeft = DDLeft(primaryTarget);
        ShortestNearbyDDLeft = float.MaxValue;

        if (strategy.Reaver == SoulReaverStrategy.ForceBreak)
            SoulReaver = false;

        switch (strategy.AOE.Value)
        {
            case AOEStrategy.AOE:
            case AOEStrategy.ForceAOE:
                var nearbyDD = Hints.PriorityTargets.Where(x => Hints.TargetInAOECircle(x.Actor, Player.Position, 5)).Select(DDLeft);
                var minNeeded = strategy.AOE.Value == AOEStrategy.ForceAOE ? 1 : 3;
                if (MinIfEnoughElements(nearbyDD.Where(x => x < 30), minNeeded) is float m)
                    ShortestNearbyDDLeft = m;
                break;
        }

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 15, (primary, other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2));
        (BestConeTarget, NumConeTargets) = SelectTarget(strategy, primaryTarget, 8, (primary, other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 90.Degrees()));
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);

        var pos = GetNextPositional(primaryTarget?.Actor);
        UpdatePositionals(primaryTarget, ref pos);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining > GetCastTime(AID.Harpe) + 2.55f && !Soulsow)
                PushGCD(AID.Soulsow, Player, GCDPriority.Soulsow);

            if (CountdownRemaining < GetCastTime(AID.Harpe))
                PushGCD(AID.Harpe, primaryTarget);

            return;
        }

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.SpinningScythe, 3, maximumActionRange: 25);

        if (SoulReaver)
        {
            var gib = Executioner ? AID.ExecutionersGibbet : AID.Gibbet;
            var gal = Executioner ? AID.ExecutionersGallows : AID.Gallows;
            var gui = Executioner ? AID.ExecutionersGuillotine : AID.Guillotine;

            if (NumConeTargets > 2)
                PushGCD(gui, BestConeTarget, GCDPriority.Reaver);

            if (primaryTarget != null)
            {
                if (EnhancedGallows > GCD)
                    PushGCD(gal, primaryTarget, GCDPriority.Reaver);
                else if (EnhancedGibbet > GCD)
                    PushGCD(gib, primaryTarget, GCDPriority.Reaver);
                else if (GetCurrentPositional(primaryTarget.Actor) == Positional.Rear)
                    PushGCD(gal, primaryTarget, GCDPriority.Reaver);
                else
                    PushGCD(gib, primaryTarget, GCDPriority.Reaver);
            }
        }

        if (!Player.InCombat)
        {
            if (!Soulsow)
                PushGCD(AID.Soulsow, Player, GCDPriority.Soulsow);

            // if we exit combat while casting, cancel it so we get instant cast instead
            if (Player.CastInfo?.Action.ID == (uint)AID.Soulsow)
                Hints.ForceCancelCast = true;
        }

        if (!SoulReaver)
        {
            switch (strategy.Harpe.Value)
            {
                case HarpeStrategy.Automatic:
                    if (EnhancedHarpe > GCD)
                        PushGCD(AID.Harpe, ResolveTargetOverride(strategy.Harpe) ?? primaryTarget, GCDPriority.EnhancedHarpe);
                    break;
                case HarpeStrategy.Ranged:
                    PushOGCD(AID.Harpe, ResolveTargetOverride(strategy.Harpe) ?? primaryTarget, 50);
                    break;
            }
        }

        HarvestMoon(strategy);
        DDRefresh(primaryTarget);
        Perfectio(strategy);
        EnshroudGCDs(strategy, primaryTarget);
        PlentifulHarvest(strategy);
        Sow(strategy);

        // other GCDs are all disabled during enshroud; normal GCDs break soul reaver
        if (!(Enshrouded || SoulReaver))
        {
            Slice(strategy, primaryTarget);

            if (NumAOETargets > 2)
            {
                if (ComboLastMove == AID.SpinningScythe)
                    PushGCD(AID.NightmareScythe, Player, GCDPriority.FillerAOE);

                PushGCD(AID.SpinningScythe, Player, GCDPriority.FillerAOE);
            }

            if (ComboLastMove == AID.WaxingSlice)
                PushGCD(AID.InfernalSlice, primaryTarget, GCDPriority.Filler);

            if (ComboLastMove == AID.Slice)
                PushGCD(AID.WaxingSlice, primaryTarget, GCDPriority.Filler);

            PushGCD(AID.Slice, primaryTarget, GCDPriority.Filler);
        }

        OGCD(strategy, primaryTarget);
    }

    private void OGCD(Strategy strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        if (strategy.Buffs != OffensiveStrategy.Delay)
        {
            // wait for soul slice in opener
            if (OnCooldown(AID.SoulSlice) || CombatTimer > 10)
                PushOGCD(AID.ArcaneCircle, Player, delay: GCD - 1.6f);
        }

        if (NextPositionalImminent && !NextPositionalCorrect)
            PushOGCD(AID.TrueNorth, Player, delay: GCD - 0.8f);

        if (Oblatio > 0 && (RaidBuffsLeft > 0 || ReadyIn(AID.ArcaneCircle) > EnshroudLeft))
            PushOGCD(AID.Sacrificium, BestRangedAOETarget);

        if (PurpleSouls > 1)
        {
            if (NumConeTargets > 2)
                PushOGCD(AID.LemuresScythe, BestConeTarget);

            PushOGCD(AID.LemuresSlice, primaryTarget);
        }

        if (ShouldEnshroud(strategy))
            PushOGCD(AID.Enshroud, Player);

        UseSoul(strategy, primaryTarget);

        if (strategy.AutoCrest.IsEnabled() && Hints.PredictedDamage.Any(p => p.Players[0] && p.Activation < World.FutureTime(5)))
            PushOGCD(AID.ArcaneCrest, Player);
    }

    private void HarvestMoon(in Strategy strategy)
    {
        if (!Soulsow)
            return;

        var prio = strategy.HM.Value switch
        {
            OffensiveStrategy.Force => GCDPriority.Max,
            OffensiveStrategy.Automatic => SoulReaver ? GCDPriority.None : GCDPriority.HarvestMoon,
            _ => GCDPriority.None
        };

        PushGCD(AID.HarvestMoon, ResolveTargetOverride(strategy.HM) ?? BestRangedAOETarget, prio);
    }

    private void DDRefresh(Enemy? primaryTarget)
    {
        if (SoulReaver)
            return;

        void Extend(float timer, AID action, Enemy? target)
        {
            if (!CanFitGCD(timer, CanWeave(AID.Gluttony) ? 2 : 1))
                PushGCD(action, target, GCDPriority.DDExpiring);

            if (timer < 30 && CanWeave(AID.ArcaneCircle, 1) && OnCooldown(AID.SoulSlice))
                PushGCD(action, target, GCDPriority.DDExtend);
        }

        Extend(ShortestNearbyDDLeft, AID.WhorlofDeath, null);
        Extend(TargetDDLeft, AID.ShadowofDeath, primaryTarget);
    }

    private void Perfectio(in Strategy strategy)
    {
        var opt = strategy.Perf;
        if (PerfectioParata < GCD || opt == PerfectioStrategy.Delay || SoulReaver)
            return;

        var prio = opt.Value switch
        {
            PerfectioStrategy.Automatic => GCDPriority.Communio,
            PerfectioStrategy.Ranged => CanFitGCD(PerfectioParata, 1) ? GCDPriority.PerfectioRanged : GCDPriority.Communio,
            _ => GCDPriority.None
        };

        PushGCD(AID.Perfectio, ResolveTargetOverride(opt) ?? BestRangedAOETarget, prio);
    }

    private void PlentifulHarvest(in Strategy strategy)
    {
        if (ImmortalSacrifice.Left < GCD || BloodsownCircle > GCD || !strategy.PH.IsEnabled() || SoulReaver)
            return;

        PushGCD(AID.PlentifulHarvest, ResolveTargetOverride(strategy.PH) ?? BestLineTarget, GCDPriority.Harvest);
    }

    private void Sow(in Strategy strategy)
    {
        if (!Soulsow && strategy.Soulsow.IsEnabled())
            PushGCD(AID.Soulsow, Player, GCDPriority.Soulsow);
    }

    private bool ShouldEnshroud(in Strategy strategy)
    {
        // hard requirements
        if (Enshrouded || BlueGauge < 50 && IdealHost == 0 || strategy.Enshroud == OffensiveStrategy.Delay)
            return false;

        // forced by plan
        if (strategy.Enshroud == OffensiveStrategy.Force)
            return true;

        // let's assume wasting perfectio or soul reaver is a mistake
        if (PerfectioParata > GCD || SoulReaver)
            return false;

        // Single Enshrouds are somewhat more complicated than Doubles because of the Enshroud that could or could not precede them. General rule of thumb is to not enter Enshroud if Gluttony <13s on its cooldown.
        // TODO: standard rotation intentionally delays gluttony
        //if (ReadyIn(AID.Gluttony) < 13)
        //    return false;

        // 4 reaping GCDs at 1.5s each = 6 seconds
        // maximum communio cast time = 1.3 seconds
        if (RaidBuffsLeft > GCD + 7.3f)
            return true;

        // use early for double enshroud, so we have room for 2 communio + 1 perfectio
        if (CanWeave(AID.ArcaneCircle, 2, extraFixedDelay: 1.5f) && strategy.Buffs != OffensiveStrategy.Delay)
            return true;

        // TODO tweak deadline, i need a simulator or something
        return ReadyIn(AID.ArcaneCircle) > 65;
    }

    private void UseSoul(in Strategy strategy, Enemy? primaryTarget)
    {
        // hard requirements
        if (RedGauge < 50 || Enshrouded || strategy.RedGauge == RedGaugeStrategy.Delay)
            return;

        var nextGCDImportant = NextGCD is AID.PlentifulHarvest or AID.Perfectio;

        // before 70, blood stalk IS our dps output from red gauge, so we don't want to waste it on dying targets
        var haveBlueGauge = Unlocked(AID.Gallows);
        var targetOverride = ResolveTargetOverride(strategy.RedGauge);

        void useBloodStalk()
        {
            if (NumConeTargets > 2 && targetOverride == null)
                PushOGCD(AID.GrimSwathe, BestConeTarget);

            PushOGCD(AID.BloodStalk, targetOverride ?? primaryTarget, OGCDPriority.Default, useOnDyingTarget: haveBlueGauge);
        }

        // use in raidbuffs
        // use to get blue gauge, we need 50 before each 2min window
        var spendEarly = RaidBuffsLeft > 0 || BlueGauge < 50;
        var gluttonySoon = CanWeave(AID.Gluttony, 5);

        var debuffLeft = Math.Min(TargetDDLeft, ShortestNearbyDDLeft);

        switch (strategy.RedGauge.Value)
        {
            case RedGaugeStrategy.Automatic:
                if (BlueGauge == 100 || nextGCDImportant || SoulReaver)
                    return;

                if (CanFitGCD(debuffLeft, 2))
                    PushOGCD(AID.Gluttony, BestRangedAOETarget);

                if (CanFitGCD(debuffLeft, 1) && (RedGauge == 100 || spendEarly && !gluttonySoon))
                    useBloodStalk();
                break;
            case RedGaugeStrategy.ReserveGluttony:
                if (BlueGauge == 100 || nextGCDImportant || SoulReaver)
                    return;

                if (CanFitGCD(debuffLeft, 1) && RedGauge == 100)
                    useBloodStalk();
                break;
            case RedGaugeStrategy.Force:
                PushOGCD(AID.Gluttony, BestRangedAOETarget);
                useBloodStalk();
                break;
        }
    }

    private void Slice(in Strategy strategy, Enemy? primaryTarget)
    {
        if (!GCDReady(AID.SoulSlice))
            return;

        var shouldUse = strategy.Slice.Value switch
        {
            SliceStrategy.Automatic => RedGauge <= 50,
            SliceStrategy.Force => true,
            _ => false
        };

        if (!shouldUse)
            return;

        var sliceTarget = ResolveTargetOverride(strategy.Slice);

        // assuming if a target is specified, we don't want to use AOE rotation - TODO is this necessary at all?
        if (NumAOETargets > 2 && sliceTarget == null)
            PushGCD(AID.SoulScythe, Player, GCDPriority.SoulSlice);

        PushGCD(AID.SoulSlice, sliceTarget ?? primaryTarget, GCDPriority.SoulSlice);
    }

    private void EnshroudGCDs(in Strategy strategy, Enemy? primaryTarget)
    {
        if (BlueSouls == 0)
            return;

        if (BlueSouls == 1)
        {
            if (strategy.Communio.IsEnabled())
                PushGCD(AID.Communio, ResolveTargetOverride(strategy.Communio) ?? BestRangedAOETarget, GCDPriority.Communio);

            if (Soulsow)
                PushGCD(AID.HarvestMoon, BestRangedAOETarget, GCDPriority.EnshroudMove);
        }

        var prio = GCDPriority.Lemure;

        if (CanWeave(AID.ArcaneCircle, 2, extraFixedDelay: 1.5f) && BlueSouls < 5)
            prio = GCDPriority.Filler;

        if (RaidBuffsLeft > 0)
            prio = GCDPriority.Lemure;

        if (NumConeTargets > 2 && prio > 0)
            PushGCD(AID.GrimReaping, BestConeTarget, prio + 1);

        PushGCD(EnhancedCrossReaping > GCD ? AID.CrossReaping : AID.VoidReaping, primaryTarget, prio);
    }

    protected override float GetCastTime(AID aid)
    {
        if (aid == AID.Harpe && EnhancedHarpe > GCD)
            return 0;

        if (aid == AID.Soulsow && !Player.InCombat)
            return 0;

        return base.GetCastTime(aid);
    }

    private (Positional, bool) GetNextPositional(Actor? primaryTarget)
    {
        if (primaryTarget == null || !Unlocked(AID.Gibbet) || NumConeTargets > 2)
            return (Positional.Any, false);

        Positional nextPos;

        if (EnhancedGallows > 0)
            nextPos = Positional.Rear;
        else if (EnhancedGibbet > 0)
            nextPos = Positional.Flank;
        else
        {
            var closest = GetCurrentPositional(primaryTarget);
            nextPos = closest == Positional.Front ? Positional.Flank : closest;
        }

        return (nextPos, SoulReaver);
    }

    private float DDLeft(Enemy? target)
        => (target?.ForbidDOTs ?? false)
            ? float.MaxValue
            : StatusDetails(target?.Actor, SID.DeathsDesign, Player.InstanceID, 30).Left;

    private float? MinIfEnoughElements(IEnumerable<float> collection, int minElements)
    {
        float min = float.MaxValue;
        var elements = 0;
        foreach (var flt in collection)
        {
            elements++;
            min = MathF.Min(flt, min);
        }

        return elements >= minElements ? min : null;
    }
}
