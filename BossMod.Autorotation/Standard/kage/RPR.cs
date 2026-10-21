using BossMod.Autorotation.xan;
using BossMod.RPR;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

// based on xan RPR, with the extra options requested in awgil/ffxiv_bossmod#759
public sealed class KageRPR(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, KageRPR.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Arcane Circle", MinLevel = 72, Action = AID.ArcaneCircle, OGCDPriority = OGCDPriority.Default)]
        public Track<OffensiveStrategy> Buffs;

        [Track(Action = AID.Enshroud, MinLevel = 80, OGCDPriority = OGCDPriority.Default)]
        public Track<EnshroudStrategy> Enshroud;

        [Track(Action = AID.Communio, MinLevel = 90)]
        public Track<CommunioStrategy> Communio;

        [Track("Soul Slice", Actions = [AID.SoulSlice, AID.SoulScythe], MinLevel = 60)]
        public Track<SliceStrategy> Slice;

        [Track("Soul Gauge", Actions = [AID.BloodStalk, AID.GrimSwathe, AID.Gluttony], MinLevel = 50)]
        public Track<RedGaugeStrategy> RedGauge;

        [Track("Soul Reaver", Actions = [AID.Gallows, AID.Guillotine, AID.ExecutionersGallows, AID.ExecutionersGuillotine, AID.Gibbet, AID.ExecutionersGibbet], MinLevel = 70)]
        public Track<SoulReaverStrategy> Reaver;

        [Track("Shadow of Death", Actions = [AID.ShadowofDeath, AID.WhorlofDeath], MinLevel = 10)]
        public Track<DeathsDesignStrategy> DD;

        [Track("Plentiful Harvest", Action = AID.PlentifulHarvest, MinLevel = 88, Targets = ActionTargets.Hostile)]
        public Track<EnabledByDefault> PH;

        [Track("Harvest Moon", Action = AID.HarvestMoon, MinLevel = 82)]
        public Track<HarvestMoonStrategy> HM;

        [Track("Perfectio", Action = AID.Perfectio, MinLevel = 100, Targets = ActionTargets.Hostile)]
        public Track<PerfectioStrategy> Perf;

        [Track(Action = AID.Harpe)]
        public Track<HarpeStrategy> Harpe;

        [Track("True North", Action = AID.TrueNorth, MinLevel = 50)]
        public Track<TrueNorthStrategy> TrueNorth;

        [Track("Hell's Ingress", Action = AID.HellsIngress, MinLevel = 20)]
        public Track<IngressStrategy> Ingress;

        [Track("Soulsow (during combat)", Action = AID.Soulsow, MinLevel = 82)]
        public Track<DisabledByDefault> Soulsow;

        [Track("Auto-Arcane Crest", InternalName = "Crest", Action = AID.ArcaneCrest, MinLevel = 40)]
        public Track<EnabledByDefault> AutoCrest;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum EnshroudStrategy
    {
        [Option("Use when optimal")]
        Automatic,
        [Option("Use ASAP, but don't overwrite Soul Reaver, Executioner or Perfectio")]
        ForceSafe,
        [Option("Only use with Ideal Host; never spend Shroud gauge", MinLevel = 92)]
        IdealHostOnly,
        [Option("Use ASAP")]
        Force,
        [Option("Don't use")]
        Delay
    }

    public enum CommunioStrategy
    {
        [Option("Use on the last Lemure Shroud", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Use at 2 Lemure Shroud", Targets = ActionTargets.Hostile)]
        At2,
        [Option("Use at 3 Lemure Shroud", Targets = ActionTargets.Hostile)]
        At3,
        [Option("Use at 4 Lemure Shroud", Targets = ActionTargets.Hostile)]
        At4,
        [Option("Use ASAP (5 Lemure Shroud)", Targets = ActionTargets.Hostile)]
        ASAP,
        [Option("Don't use")]
        Delay
    }

    public enum SliceStrategy
    {
        [Option("Use on cooldown, unless it would overcap gauge", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Pool charges for raid buffs; outside of them only use to avoid capping charges", Targets = ActionTargets.Hostile)]
        Pool,
        [Option("Don't use")]
        Delay,
        [Option("Use ASAP, even if it would overcap gauge", Targets = ActionTargets.Hostile)]
        Force
    }

    public enum RedGaugeStrategy
    {
        [Option("Use ASAP, unless shroud gauge is full or Gluttony is almost off cooldown", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Reserve 50 gauge for Gluttony; use other spenders at maximum gauge", Targets = ActionTargets.Hostile, MinLevel = 76)]
        ReserveGluttony,
        [Option("Automatic, but use Gluttony before Plentiful Harvest / Perfectio", Targets = ActionTargets.Hostile, MinLevel = 76)]
        GluttonyFirst,
        [Option("Use Gluttony ASAP, ignoring timers and stacks; don't use other spenders", Targets = ActionTargets.Hostile, MinLevel = 76)]
        ForceGluttony,
        [Option("Use Blood Stalk / Unveiled Gibbet / Unveiled Gallows ASAP, ignoring timers and stacks; don't use Gluttony", Targets = ActionTargets.Hostile)]
        ForceStalk,
        [Option("Use Stalk/Gluttony ASAP, ignoring timers and stacks", Targets = ActionTargets.Hostile, MinLevel = 76)]
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

    public enum DeathsDesignStrategy
    {
        [Option("Refresh when optimal")]
        Automatic,
        [Option("Automatic; at full Soul gauge, use Shadow of Death instead of Unveiled + Gibbet/Gallows to keep Gluttony aligned", MinLevel = 76)]
        KeepGluttonyAligned,
        [Option("Use Shadow of Death ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Use Whorl of Death ASAP", MinLevel = 35)]
        ForceWhorl,
        [Option("Don't use")]
        Delay
    }

    public enum HarvestMoonStrategy
    {
        [Option("Use out of melee range or when moving at the end of Enshroud", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Automatic, and also use during raid buffs", Targets = ActionTargets.Hostile)]
        Burst,
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Don't use")]
        Delay
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

    public enum TrueNorthStrategy
    {
        [Option("Use when the next positional is wrong")]
        Automatic,
        [Option("Use when the next positional is wrong, but keep a charge for Gluttony")]
        PoolGluttony,
        [Option("Keep a charge for Gluttony, and spend Soul gauge / Soul Slice while True North is up to cover more positionals")]
        Bundle,
        [Option("Don't use")]
        Delay
    }

    public enum IngressStrategy
    {
        [Option("Don't use")]
        Manual,
        [Option("Dash out of an incoming AOE, to the safe spot closest to the target")]
        Dodge,
        [Option("Dash towards the specified position", Targets = ActionTargets.Area, Context = StrategyContext.Plan)]
        Position
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

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage RPR", "Reaper", "Standard rotation (Kage)|Melee", "Kagekazu", RotationModuleQuality.Basic, BitMask.Build(Class.RPR), 100).WithStrategies<Strategy>();
    }

    public int RedGauge;
    public int BlueGauge;
    public bool Soulsow;
    public float EnshroudLeft;
    public int BlueSouls;
    public int PurpleSouls;
    public bool SoulReaver;
    public int SoulReaverStacks;
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

    // at full soul gauge with gluttony coming up, refresh death's design instead of starting soul reaver
    private bool AvoidGluttonyDrift;
    // true north is up and we want to cover as many positionals with it as possible
    private bool SpendUnderTrueNorth;

    public enum GCDPriority
    {
        None = 0,
        Soulsow = 1,
        EnhancedHarpe = 100,
        HarvestMoon = 150,
        PerfectioRanged = 175,
        Filler = 200,
        FillerAOE = 201,
        HarvestMoonBurst = 250,
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
        Default = 1,
        Ingress = 50
    }

    private bool Enshrouded => BlueSouls > 0;

    private bool HaveTarget;

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 3);
        HaveTarget = primaryTarget != null && Player.InCombat;

        var gauge = World.Client.GetGauge<ReaperGauge>();

        RedGauge = gauge.Soul;
        BlueGauge = gauge.Shroud;
        EnshroudLeft = gauge.EnshroudedTimeRemaining * 0.001f;
        BlueSouls = gauge.LemureShroud;
        PurpleSouls = gauge.VoidShroud;

        Soulsow = Player.FindStatus(SID.Soulsow) != null;
        Executioner = StatusLeft(SID.Executioner) > GCD;
        SoulReaverStacks = StatusStacks(SID.SoulReaver);
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
                var nearbyDD = Hints.PriorityTargets.Where(x => TargetInAOECircle(x.Actor, Player.Position, 5)).Select(DDLeft);
                var minNeeded = strategy.AOE.Value == AOEStrategy.ForceAOE ? 1 : 3;
                if (MinIfEnoughElements(nearbyDD.Where(x => x < 30), minNeeded) is float m)
                    ShortestNearbyDDLeft = m;
                break;
        }

        AvoidGluttonyDrift = strategy.DD == DeathsDesignStrategy.KeepGluttonyAligned && primaryTarget != null && RedGauge == 100 && ReadyIn(AID.Gluttony) < 2 && TargetDDLeft < 25 && !SoulReaver && !Enshrouded;
        SpendUnderTrueNorth = strategy.TrueNorth == TrueNorthStrategy.Bundle && TrueNorthLeft > GCD + GCDLength;

        NumAOETargets = NumMeleeAOETargets(strategy);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 15, (primary, other) => TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 15, 2));
        (BestConeTarget, NumConeTargets) = SelectTarget(strategy, primaryTarget, 8, (primary, other) => TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 90.Degrees()));
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);

        var pos = GetNextPositional(primaryTarget?.Actor);
        UpdatePositionals(primaryTarget, ref pos);

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.Medium);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining > GetCastTime(AID.Harpe) + 2.55f && !Soulsow)
                PushGCD(AID.Soulsow, Player, GCDPriority.Soulsow);

            if (CountdownRemaining < GetCastTime(AID.Harpe))
                PushGCD(AID.Harpe, primaryTarget);

            OGCD(strategy, primaryTarget);

            return;
        }

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.SpinningScythe, 3, maximumActionRange: 25);

        if (SoulReaver)
        {
            var gib = Executioner ? AID.ExecutionersGibbet : AID.Gibbet;
            var gal = Executioner ? AID.ExecutionersGallows : AID.Gallows;
            var gui = Executioner ? AID.ExecutionersGuillotine : AID.Guillotine;

            if (NumConeTargets > 2)
                PushGCD(gui, BestConeTarget, GCDPriority.Reaver, setRotation: true);

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
            Sow(strategy);

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
                        PushGCD(AID.Harpe, ResolveEnemy(strategy.Harpe) ?? primaryTarget, GCDPriority.EnhancedHarpe);
                    break;
                case HarpeStrategy.Ranged:
                    PushOGCD(AID.Harpe, ResolveEnemy(strategy.Harpe) ?? primaryTarget, 50);
                    break;
            }
        }

        HarvestMoon(strategy);
        DDRefresh(strategy, primaryTarget);
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

    private void OGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        switch (strategy.Buffs.Value)
        {
            case OffensiveStrategy.Automatic:
                if (HaveTarget && (OnCooldown(AID.SoulSlice) || CombatTimer > 10))
                    UsePlanned(strategy.Buffs, AID.ArcaneCircle, Player, delay: GCD - 1.6f);
                break;
            case OffensiveStrategy.Force:
                UsePlanned(strategy.Buffs, AID.ArcaneCircle, Player);
                break;
        }

        if (ShouldEnshroud(strategy))
            UsePlanned(strategy.Enshroud, AID.Enshroud, Player);

        HellsIngress(strategy, primaryTarget);

        // all the other OGCDs are gauge spenders or whatever
        if (!HaveTarget)
            return;

        if (NextPositionalImminent && !NextPositionalCorrect && UseTrueNorth(strategy))
            PushOGCD(AID.TrueNorth, Player, delay: GCD - 0.8f);

        if (Oblatio > 0 && (RaidBuffsLeft > 0 || ReadyIn(AID.ArcaneCircle) > EnshroudLeft))
            PushOGCD(AID.Sacrificium, BestRangedAOETarget);

        if (PurpleSouls > 1)
        {
            if (NumConeTargets > 2)
                PushOGCD(AID.LemuresScythe, BestConeTarget, setRotation: true);

            PushOGCD(AID.LemuresSlice, primaryTarget);
        }

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
            HarvestMoonStrategy.Force => GCDPriority.Max,
            HarvestMoonStrategy.Automatic => SoulReaver ? GCDPriority.None : GCDPriority.HarvestMoon,
            HarvestMoonStrategy.Burst => SoulReaver ? GCDPriority.None : RaidBuffsLeft > GCD ? GCDPriority.HarvestMoonBurst : GCDPriority.HarvestMoon,
            _ => GCDPriority.None
        };

        PushGCD(AID.HarvestMoon, ResolveEnemy(strategy.HM) ?? BestRangedAOETarget, prio);
    }

    private void DDRefresh(in Strategy strategy, Enemy? primaryTarget)
    {
        switch (strategy.DD.Value)
        {
            case DeathsDesignStrategy.Delay:
                return;
            case DeathsDesignStrategy.Force:
                PushGCD(AID.ShadowofDeath, ResolveEnemy(strategy.DD) ?? primaryTarget, GCDPriority.Max);
                return;
            case DeathsDesignStrategy.ForceWhorl:
                PushGCD(AID.WhorlofDeath, Player, GCDPriority.Max);
                return;
        }

        if (SoulReaver)
            return;

        if (AvoidGluttonyDrift)
            PushGCD(AID.ShadowofDeath, primaryTarget, GCDPriority.DDExtend);

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
        if (PerfectioParata <= GCD || opt == PerfectioStrategy.Delay || SoulReaver)
            return;

        var prio = opt.Value switch
        {
            PerfectioStrategy.Automatic => GCDPriority.Communio,
            PerfectioStrategy.Ranged => CanFitGCD(PerfectioParata, 1) ? GCDPriority.PerfectioRanged : GCDPriority.Communio,
            _ => GCDPriority.None
        };

        PushGCD(AID.Perfectio, ResolveEnemy(opt) ?? BestRangedAOETarget, prio);
    }

    private void PlentifulHarvest(in Strategy strategy)
    {
        if (ImmortalSacrifice.Left <= GCD || BloodsownCircle > GCD || !strategy.PH.IsEnabled() || SoulReaver)
            return;

        PushGCD(AID.PlentifulHarvest, ResolveEnemy(strategy.PH) ?? BestLineTarget, GCDPriority.Harvest, setRotation: NumLineTargets > 1);
    }

    private void Sow(in Strategy strategy)
    {
        if (!Soulsow && !SoulReaver && (strategy.Soulsow.IsEnabled() || !Player.InCombat))
            PushGCD(AID.Soulsow, Player, GCDPriority.Soulsow);
    }

    private bool ShouldEnshroud(in Strategy strategy)
    {
        // hard requirements
        if (Enshrouded || BlueGauge < 50 && IdealHost == 0 || strategy.Enshroud == EnshroudStrategy.Delay)
            return false;

        // let's assume wasting perfectio or soul reaver is a mistake
        var wouldOverwrite = PerfectioParata > GCD || SoulReaver;

        switch (strategy.Enshroud.Value)
        {
            case EnshroudStrategy.Force:
                return true;
            case EnshroudStrategy.ForceSafe:
                return !wouldOverwrite;
            case EnshroudStrategy.IdealHostOnly:
                return IdealHost > 0 && HaveTarget && !wouldOverwrite;
        }

        // need to be in combat for automatic usage
        if (!HaveTarget || wouldOverwrite)
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
        if (CanWeave(AID.ArcaneCircle, 1, extraFixedDelay: 1.5f) && strategy.Buffs != OffensiveStrategy.Delay)
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
        var targetOverride = ResolveEnemy(strategy.RedGauge);

        void useBloodStalk()
        {
            if (NumConeTargets > 2 && targetOverride == null)
                PushOGCD(AID.GrimSwathe, BestConeTarget, setRotation: true);

            PushOGCD(AID.BloodStalk, targetOverride ?? primaryTarget, OGCDPriority.Default, useOnDyingTarget: haveBlueGauge);
        }

        void useGluttony() => PushOGCD(AID.Gluttony, targetOverride ?? BestRangedAOETarget);

        // use in raidbuffs
        // use to get blue gauge, we need 50 before each 2min window
        var spendEarly = RaidBuffsLeft > 0 || BlueGauge < 50 || SpendUnderTrueNorth;
        var gluttonySoon = CanWeave(AID.Gluttony, 5);

        var debuffLeft = Math.Min(TargetDDLeft, ShortestNearbyDDLeft);

        switch (strategy.RedGauge.Value)
        {
            case RedGaugeStrategy.Automatic:
            case RedGaugeStrategy.GluttonyFirst:
                if (BlueGauge == 100 || SoulReaver)
                    return;

                if (CanFitGCD(debuffLeft, 2) && (!nextGCDImportant || strategy.RedGauge == RedGaugeStrategy.GluttonyFirst))
                    useGluttony();

                if (!nextGCDImportant && !AvoidGluttonyDrift && CanFitGCD(debuffLeft, 1) && (RedGauge == 100 || spendEarly && !gluttonySoon))
                    useBloodStalk();
                break;
            case RedGaugeStrategy.ReserveGluttony:
                if (BlueGauge == 100 || nextGCDImportant || SoulReaver)
                    return;

                if (!AvoidGluttonyDrift && CanFitGCD(debuffLeft, 1) && RedGauge == 100)
                    useBloodStalk();
                break;
            case RedGaugeStrategy.ForceGluttony:
                useGluttony();
                break;
            case RedGaugeStrategy.ForceStalk:
                useBloodStalk();
                break;
            case RedGaugeStrategy.Force:
                useGluttony();
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
            SliceStrategy.Pool => RedGauge <= 50 && (RaidBuffsLeft > GCD || MaxChargesIn(AID.SoulSlice) <= GCD + GCDLength || SpendUnderTrueNorth),
            SliceStrategy.Force => true,
            _ => false
        };

        if (!shouldUse)
            return;

        var sliceTarget = ResolveEnemy(strategy.Slice);

        // assuming if a target is specified, we don't want to use AOE rotation - TODO is this necessary at all?
        if (NumAOETargets > 2 && sliceTarget == null)
            PushGCD(AID.SoulScythe, Player, GCDPriority.SoulSlice);

        PushGCD(AID.SoulSlice, sliceTarget ?? primaryTarget, GCDPriority.SoulSlice);
    }

    private void EnshroudGCDs(in Strategy strategy, Enemy? primaryTarget)
    {
        if (BlueSouls == 0)
            return;

        var communioAt = strategy.Communio.Value switch
        {
            CommunioStrategy.Automatic => 1,
            CommunioStrategy.At2 => 2,
            CommunioStrategy.At3 => 3,
            CommunioStrategy.At4 => 4,
            CommunioStrategy.ASAP => 5,
            _ => 0
        };

        if (BlueSouls <= communioAt)
            PushGCD(AID.Communio, ResolveEnemy(strategy.Communio) ?? BestRangedAOETarget, GCDPriority.Communio);

        if (BlueSouls == 1 && Soulsow)
            PushGCD(AID.HarvestMoon, BestRangedAOETarget, GCDPriority.EnshroudMove);

        var prio = GCDPriority.Lemure;

        if (CanWeave(AID.ArcaneCircle, 2, extraFixedDelay: 1.5f) && BlueSouls < 5)
            prio = GCDPriority.Filler;

        if (RaidBuffsLeft > 0)
            prio = GCDPriority.Lemure;

        if (NumConeTargets > 2 && prio > 0)
            PushGCD(AID.GrimReaping, BestConeTarget, prio + 1, setRotation: true);

        PushGCD(EnhancedCrossReaping > GCD ? AID.CrossReaping : AID.VoidReaping, primaryTarget, prio);
    }

    private bool UseTrueNorth(in Strategy strategy) => strategy.TrueNorth.Value switch
    {
        TrueNorthStrategy.Automatic => true,
        // only spend the last charge if it comes back before gluttony, or if this is the gluttony window
        TrueNorthStrategy.PoolGluttony or TrueNorthStrategy.Bundle => Executioner || SoulReaverStacks >= 2 || MaxChargesIn(AID.TrueNorth) <= GCD || MaxChargesIn(AID.TrueNorth) < ReadyIn(AID.Gluttony),
        _ => false
    };

    private void HellsIngress(in Strategy strategy, Enemy? primaryTarget)
    {
        var prio = strategy.Ingress.Priority(ActionQueue.Priority.Low + (int)OGCDPriority.Ingress);
        switch (strategy.Ingress.Value)
        {
            case IngressStrategy.Dodge:
                if (Player.InCombat && DodgeDirection(primaryTarget) is Angle dir)
                    PushAction(AID.HellsIngress, Player, prio, 0, facingAngle: dir);
                break;
            case IngressStrategy.Position:
                var dest = ResolveTargetLocation(strategy.Ingress.TrackRaw);
                if (dest != default && !Player.Position.InCircle(dest, 3))
                    PushAction(AID.HellsIngress, Player, prio, 0, facingAngle: Angle.FromDirection(dest - Player.Position));
                break;
        }
    }

    // ingress dashes 15y forward; if we're standing in an imminent aoe, pick a safe landing spot, preferring the one closest to the target
    private Angle? DodgeDirection(Enemy? primaryTarget)
    {
        var soon = World.FutureTime(2.5f);
        if (!Hints.ForbiddenZones.Any(z => z.activation <= soon && z.shape.Check(Player.Position)))
            return null;

        var later = World.FutureTime(5);
        Angle? best = null;
        var bestDist = float.MaxValue;
        for (var i = 0; i < 16; ++i)
        {
            var dir = (i * 22.5f).Degrees();
            var landing = Player.Position + dir.ToDirection() * 15;
            if (!Hints.PathfindMapBounds.Contains(landing - Hints.PathfindMapCenter) || Hints.ForbiddenZones.Any(z => z.activation <= later && z.shape.Check(landing)))
                continue;

            var dist = primaryTarget != null ? (landing - primaryTarget.Actor.Position).Length() : 0;
            if (dist < bestDist)
                (best, bestDist) = (dir, dist);
        }
        return best;
    }

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithRaidBuffs => HaveTarget && (RaidBuffsLeft > 0 || RaidBuffsIn < 5 || RaidBuffsIn > 9000),
        PotionStrategy.Immediate => HaveTarget,
        _ => false
    };

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

    private static float? MinIfEnoughElements(IEnumerable<float> collection, int minElements)
    {
        var min = float.MaxValue;
        var elements = 0;
        foreach (var flt in collection)
        {
            elements++;
            min = MathF.Min(flt, min);
        }

        return elements >= minElements ? min : null;
    }
}
