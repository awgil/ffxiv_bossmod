using BossMod.DNC;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageDNC(RotationModuleManager manager, Actor player) : TypedRotationModule<KageDNC.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Technical Step", MinLevel = 70, Actions = [AID.TechnicalStep, AID.QuadrupleTechnicalFinish])]
        public Track<OffensiveStrategy> Technical;

        [Track("Standard Step / Finishing Move", MinLevel = 15, Actions = [AID.StandardStep, AID.DoubleStandardFinish, AID.FinishingMove])]
        public Track<OffensiveStrategy> Standard;

        [Track("Devilment", MinLevel = 62, Action = AID.Devilment)]
        public Track<OffensiveStrategy> Devilment;

        [Track("Flourish", MinLevel = 72, Action = AID.Flourish)]
        public Track<OffensiveStrategy> Flourish;

        [Track("Feathers", MinLevel = 30, Actions = [AID.FanDance, AID.FanDanceII])]
        public Track<FeatherStrategy> Feathers;

        [Track("Dance Partner", MinLevel = 60, Action = AID.ClosedPosition)]
        public Track<PartnerStrategy> Partner;

        [Track("Curing Waltz", MinLevel = 52, Action = AID.CuringWaltz)]
        public Track<WaltzStrategy> Waltz;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 15, UiPriority = -10, Context = StrategyContext.Plan)]
        public Track<OpenerStrategy> Opener;
    }

    public enum FeatherStrategy
    {
        [Option("Spend in Technical Finish; never overcap")]
        Automatic,
        [Option("Only to avoid overcapping")]
        Overcap,
        [Option("Do not use")]
        Delay
    }

    public enum PartnerStrategy
    {
        [Option("Automatically choose the best partner")]
        Automatic,
        [Option("Do not change the partner")]
        Manual,
        [Option("Keep on the selected party member", Targets = ActionTargets.Party, Context = StrategyContext.Plan)]
        SelectTarget
    }

    public enum WaltzStrategy
    {
        [Option("Use when you or 3+ nearby allies are low")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use before the pull and with Technical Step")]
        AlignWithBurst,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("15s Standard Step")]
        Standard15,
        [Option("7s Standard Step")]
        Standard7,
        [Option("30s Technical (Standard at -30, Technical at -7)", MinLevel = 70)]
        Technical30,
        [Option("7s Technical Step", MinLevel = 70)]
        Technical7,
        [Option("No pre-pull dancing")]
        None
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage DNC", "Dancer", "Standard rotation (Kage)|Ranged", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.DNC), 100).WithStrategies<Strategy>();
    }

    private int Feathers;
    private int Esprit;
    private AID NextStep;

    private float StandardStepLeft;
    private float TechStepLeft;
    private float TechFinishLeft;
    private float FlourishingFinishLeft;
    private float DevilmentLeft;
    private float SymmetryLeft;
    private float FlowLeft;
    private float SilkenLeft;
    private float StarfallLeft;
    private float ThreefoldLeft;
    private float FourfoldLeft;
    private float LastDanceLeft;
    private float FinishingMoveLeft;
    private float DawnLeft;

    private float DowntimeIn;
    private Actor? BestSplashTarget;
    private Actor? BestConeTarget;
    private Actor? BestLineTarget;
    private bool AOEMode;
    private bool EnemyIn15;
    private float AnimLockDelay;
    private Targeting TargetMode;

    private float GCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level);
    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private float ComboLeft => World.Client.ComboState.Remaining;
    private bool Dancing => StandardStepLeft > 0 || TechStepLeft > 0;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        AnimLockDelay = estimatedAnimLockDelay;

        TimeToKill.Update(World, Hints);
        var target = Hints.FindEnemy(primaryTarget);
        if (target?.Priority is Enemy.PriorityInvincible or Enemy.PriorityForbidden || target?.Priority == Enemy.PriorityPointless && Hints.PriorityTargets.Any())
            target = null;

        TargetMode = strategy.Targeting.Value == Targeting.AutoTryPri ? (target != null ? Targeting.AutoPrimary : Targeting.Auto) : strategy.Targeting.Value;
        if (TargetMode == Targeting.Auto && target == null)
        {
            target = Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 25).MinBy(e => Player.DistanceToHitbox(e.Actor)) ?? target;
            primaryTarget = target?.Actor;
        }

        var gauge = World.Client.GetGauge<DancerGauge>();
        Feathers = gauge.Feathers;
        Esprit = gauge.Esprit;
        NextStep = (uint)gauge.CurrentStep > 0 ? (AID)((uint)gauge.CurrentStep + 15998) : AID.None;

        StandardStepLeft = SelfStatusLeft(SID.StandardStep);
        TechStepLeft = SelfStatusLeft(SID.TechnicalStep);
        TechFinishLeft = SelfStatusLeft(SID.TechnicalFinish);
        FlourishingFinishLeft = SelfStatusLeft(SID.FlourishingFinish);
        DevilmentLeft = SelfStatusLeft(SID.Devilment);
        SymmetryLeft = MathF.Max(SelfStatusLeft(SID.SilkenSymmetry), SelfStatusLeft(SID.FlourishingSymmetry));
        FlowLeft = MathF.Max(SelfStatusLeft(SID.SilkenFlow), SelfStatusLeft(SID.FlourishingFlow));
        SilkenLeft = MathF.Max(SelfStatusLeft(SID.SilkenSymmetry), SelfStatusLeft(SID.SilkenFlow));
        StarfallLeft = SelfStatusLeft(SID.FlourishingStarfall);
        ThreefoldLeft = SelfStatusLeft(SID.ThreefoldFanDance);
        FourfoldLeft = SelfStatusLeft(SID.FourfoldFanDance);
        LastDanceLeft = SelfStatusLeft(SID.LastDanceReady);
        FinishingMoveLeft = SelfStatusLeft(SID.FinishingMoveReady);
        DawnLeft = SelfStatusLeft(SID.DanceOfTheDawnReady);

        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime() is (var downNow, var stateLeft) ? (downNow ? 0 : stateLeft) : float.MaxValue;
        EnemyIn15 = Hints.PriorityTargets.Any(e => Player.DistanceToHitbox(e.Actor) <= 15);

        var allowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        AOEMode = Unlocked(AID.Windmill) && strategy.AOE.Value switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.AOE => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5) >= 2,
            _ => false
        };
        BestSplashTarget = BestAOETarget(target, 25, allowAoE, (c, e) => TargetInAOECircle(e, c.Position, 5));
        BestConeTarget = BestAOETarget(target, 15, allowAoE, (c, e) => TargetInAOECone(e, Player.Position, 15, Player.DirectionTo(c), 60.Degrees()));
        BestLineTarget = BestAOETarget(target, 25, allowAoE, (c, e) => TargetInAOERect(e, Player.Position, Player.DirectionTo(c), 25, 2));

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionDex, Player, ActionQueue.Priority.Medium);

        Partner(strategy);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            Prepull(strategy, countdown);
            return;
        }

        if (Dancing)
        {
            Dance();
            if (target != null)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 14.5f));
            return;
        }

        if (target == null)
        {
            if (strategy.Technical.Value == OffensiveStrategy.Force && Unlocked(AID.TechnicalStep) && ReadyIn(AID.TechnicalStep) <= GCD + 0.5f && FlourishingFinishLeft == 0)
                PushGCD(AID.TechnicalStep, Player, 50);
            return;
        }

        var danceSoon = GCDReady(AID.StandardStep) || GCDReady(AID.TechnicalStep) || FinishingMoveLeft > 0 || FlourishingFinishLeft > 0;
        var goal = Hints.GoalSingleTarget(target.Actor, Player, World.Actors, danceSoon ? 14.5f : 25);
        Hints.GoalZones.Add(allowAoE && Unlocked(AID.Windmill) ? AIHints.GoalCombined(goal, Hints.GoalAOECircle(5), 2) : goal);

        GCDs(strategy, target);
        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    #region Dancing

    private void Prepull(in Strategy strategy, float countdown)
    {
        var mode = strategy.Opener.Value;
        if (mode == OpenerStrategy.None || !Unlocked(AID.StandardStep))
            return;
        if (mode is OpenerStrategy.Technical30 or OpenerStrategy.Technical7 && !Unlocked(AID.TechnicalStep))
            mode = OpenerStrategy.Standard15;

        if (countdown < 5 && SelfStatusLeft(ClassShared.SID.Peloton) == 0 && !Player.InCombat)
            PushOGCD(ClassShared.AID.Peloton, Player, 5);

        if (Dancing)
        {
            if (NextStep != AID.None)
            {
                PushGCD(NextStep, Player, 50);
                return;
            }
            if (StandardStepLeft > 0 && (mode == OpenerStrategy.Technical30 ? countdown < 15.2f : countdown < 0.3f || StandardStepLeft < 0.6f))
                PushGCD(AID.DoubleStandardFinish, Player, 50);
            if (TechStepLeft > 0 && (countdown < 0.3f || TechStepLeft < 0.6f))
                PushGCD(AID.QuadrupleTechnicalFinish, Player, 50);
            return;
        }

        var startStandard = mode switch
        {
            OpenerStrategy.Standard15 => 15f,
            OpenerStrategy.Standard7 => 7f,
            OpenerStrategy.Technical30 => 30f,
            _ => -1f
        };
        if (startStandard > 0 && countdown <= startStandard && SelfStatusLeft(SID.StandardFinish) == 0 && GCDReady(AID.StandardStep))
            PushGCD(AID.StandardStep, Player, 40);
        if (mode is OpenerStrategy.Technical30 or OpenerStrategy.Technical7 && countdown <= 7 && GCDReady(AID.TechnicalStep) && (mode == OpenerStrategy.Technical7 || SelfStatusLeft(SID.StandardFinish) > 0))
            PushGCD(AID.TechnicalStep, Player, 40);
    }

    private void Dance()
    {
        if (NextStep != AID.None)
        {
            PushGCD(NextStep, Player, 50);
            return;
        }
        if (StandardStepLeft > 0 && (EnemyIn15 || StandardStepLeft < GCDLength * 1.5f))
            PushGCD(AID.DoubleStandardFinish, Player, 50);
        if (TechStepLeft > 0 && (EnemyIn15 || TechStepLeft < GCDLength * 1.5f))
            PushGCD(AID.QuadrupleTechnicalFinish, Player, 50);
    }

    #endregion

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        var targetAlive = TimeToKill.WillLive(target.Actor, AOEMode ? 10 : 3);
        var splash = BestSplashTarget ?? target.Actor;
        var expiring = GCD + GCDLength * 1.5f;

        if (LastDanceLeft > GCD && LastDanceLeft < expiring)
            PushGCD(AID.LastDance, splash, 60);
        if (FlowLeft > GCD && FlowLeft < expiring)
            PushGCD(Flow, FlowTarget(target), 59);
        if (SymmetryLeft > GCD && SymmetryLeft < expiring)
            PushGCD(Symmetry, FlowTarget(target), 58);
        if (StarfallLeft > GCD && StarfallLeft < expiring)
            PushGCD(AID.StarfallDance, BestLineTarget ?? target.Actor, 57);
        if (FlourishingFinishLeft > GCD && FlourishingFinishLeft < expiring && Esprit < 100 && Unlocked(AID.Tillana) && EnemyIn15)
            PushGCD(AID.Tillana, Player, 56);

        if (ShouldTechnical(strategy, TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target.Actor, 15)) && FlourishingFinishLeft == 0)
            PushGCD(AID.TechnicalStep, Player, 50);

        if (TechFinishLeft > GCD)
            BurstGCDs(strategy, target, targetAlive);
        else
            NormalGCDs(strategy, target, targetAlive);

        if (AOEMode)
            ComboAoE();
        else
            ComboST(target);
    }

    private void BurstGCDs(in Strategy strategy, Enemy target, bool targetAlive)
    {
        var splash = BestSplashTarget ?? target.Actor;
        if (FlourishingFinishLeft > GCD && Esprit <= 50 && Unlocked(AID.Tillana) && EnemyIn15)
            PushGCD(AID.Tillana, Player, 48);
        if (DawnLeft > GCD && Esprit >= 50)
            PushGCD(AID.DanceOfTheDawn, splash, 47);
        if (LastDanceLeft > GCD && FinishingMoveLeft > GCD)
            PushGCD(AID.LastDance, splash, 46);
        if (StandardAllowed(strategy, targetAlive) && FinishingMoveLeft > GCD && LastDanceLeft == 0 && GCDReady(AID.FinishingMove) && EnemyIn15)
            PushGCD(AID.FinishingMove, Player, 45);
        if (StandardAllowed(strategy, targetAlive) && NeedToStandard(strategy))
            PushGCD(AID.StandardStep, Player, 44);
        if (Esprit > 80 && Unlocked(AID.SaberDance))
            PushGCD(AID.SaberDance, splash, 43);
        if (StarfallLeft > GCD)
            PushGCD(AID.StarfallDance, BestLineTarget ?? target.Actor, 42);
        if (Esprit >= 50 && Unlocked(AID.SaberDance))
            PushGCD(AID.SaberDance, splash, 41);
        if (LastDanceLeft > GCD)
            PushGCD(AID.LastDance, splash, 40);
        if (FlourishingFinishLeft > GCD && Unlocked(AID.Tillana) && EnemyIn15)
            PushGCD(AID.Tillana, Player, 39);
        if (FlowLeft > GCD)
            PushGCD(Flow, FlowTarget(target), 30);
        if (SymmetryLeft > GCD)
            PushGCD(Symmetry, FlowTarget(target), 29);
    }

    private void NormalGCDs(in Strategy strategy, Enemy target, bool targetAlive)
    {
        var splash = BestSplashTarget ?? target.Actor;
        if (StandardAllowed(strategy, targetAlive))
        {
            if (FinishingMoveLeft > GCD && NeedToFinish(strategy) && EnemyIn15)
                PushGCD(AID.FinishingMove, Player, 45);
            if (NeedToStandard(strategy))
                PushGCD(AID.StandardStep, Player, 44);
        }
        if (Esprit >= 85 && Unlocked(AID.SaberDance))
            PushGCD(AID.SaberDance, splash, 43);

        var combo2 = AOEMode ? AID.Bladeshower : AID.Fountain;
        var combo2Ready = Unlocked(combo2) && ComboLastMove == (AOEMode ? AID.Windmill : AID.Cascade) && ComboLeft > GCD;
        if (combo2Ready && ComboLeft < GCD + GCDLength * 1.5f)
        {
            if (FlowLeft > GCD)
                PushGCD(Flow, FlowTarget(target), 42);
            PushGCD(combo2, AOEMode ? Player : target.Actor, 41);
        }

        if (Esprit >= 50 && Unlocked(AID.SaberDance))
            PushGCD(DawnLeft > GCD ? AID.DanceOfTheDawn : AID.SaberDance, splash, 40);
        if (LastDanceLeft > GCD && ShouldLastDance())
            PushGCD(AID.LastDance, splash, 39);
        if (StarfallLeft > GCD)
            PushGCD(AID.StarfallDance, BestLineTarget ?? target.Actor, 38);
        if (FlourishingFinishLeft > GCD && Unlocked(AID.Tillana) && EnemyIn15)
            PushGCD(AID.Tillana, Player, 37);
        if (FlowLeft > GCD)
            PushGCD(Flow, FlowTarget(target), 30);
        if (SymmetryLeft > GCD)
            PushGCD(Symmetry, FlowTarget(target), 29);
    }

    private AID Flow => AOEMode && Unlocked(AID.Bloodshower) ? AID.Bloodshower : AID.Fountainfall;
    private AID Symmetry => AOEMode && Unlocked(AID.RisingWindmill) ? AID.RisingWindmill : AID.ReverseCascade;
    private Actor? FlowTarget(Enemy target) => AOEMode ? Player : target.Actor;

    private bool StandardAllowed(in Strategy strategy, bool targetAlive)
        => strategy.Standard.Value != OffensiveStrategy.Delay && (targetAlive || strategy.Standard.Value == OffensiveStrategy.Force);

    private void ComboST(Enemy target)
    {
        if (Unlocked(AID.Fountain) && ComboLastMove == AID.Cascade && ComboLeft > GCD)
            PushGCD(AID.Fountain, target.Actor, 10);
        PushGCD(AID.Cascade, target.Actor, 1);
    }

    private void ComboAoE()
    {
        if (Unlocked(AID.Bladeshower) && ComboLastMove == AID.Windmill && ComboLeft > GCD)
            PushGCD(AID.Bladeshower, Player, 10);
        PushGCD(AID.Windmill, Player, 1);
    }

    private bool ShouldTechnical(in Strategy strategy, bool targetAlive)
    {
        if (!Unlocked(AID.TechnicalStep) || ReadyIn(AID.TechnicalStep) > GCD + 0.5f)
            return false;
        return strategy.Technical.Value switch
        {
            OffensiveStrategy.Force => true,
            OffensiveStrategy.Delay => false,
            _ => targetAlive && !GCDReady(AID.StandardStep) && DowntimeIn > GCD + 7
        };
    }

    private bool ShouldLastDance()
    {
        var techIn = ReadyIn(AID.TechnicalStep);
        var canHoldForTech = TimeToKill.InBossFight(Bossmods.ActiveModule, World.Actors.Find(Player.TargetID)) && Unlocked(AID.TechnicalStep) && techIn > 0 && techIn < 20 && LastDanceLeft > techIn + 4;
        return TechFinishLeft > GCD && Esprit < 95 || !canHoldForTech || LastDanceLeft < 4;
    }

    private bool NeedToFinish(in Strategy strategy)
    {
        if (strategy.Standard.Value == OffensiveStrategy.Force)
            return true;
        if (LastDanceLeft > GCD)
            return false;
        var stdIn = ReadyIn(AID.StandardStep);
        return TechFinishLeft > GCD ? stdIn < GCD + 0.3f : stdIn < GCD + 0.1f;
    }

    private bool NeedToStandard(in Strategy strategy)
    {
        if (!Unlocked(AID.StandardStep) || ReadyIn(AID.StandardStep) > GCD + 0.5f || FinishingMoveLeft > 0)
            return false;
        if (strategy.Standard.Value == OffensiveStrategy.Force)
            return true;
        if (Unlocked(AID.FinishingMove) && TechFinishLeft > GCD)
            return false;
        if (DowntimeIn < GCD + 4)
            return false;
        var flourishIn = ReadyIn(AID.Flourish);
        return !Unlocked(AID.Flourish) || flourishIn == 0 || flourishIn > 5;
    }

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        if (ShouldDevilment(strategy))
        {
            PushOGCD(AID.Devilment, Player, 70);
            return;
        }

        if (ShouldFlourish(strategy))
            PushOGCD(AID.Flourish, Player, 60);

        if ((ThreefoldLeft > 0 || FourfoldLeft > 0) && TechFinishLeft > 0 && ReadyIn(AID.Flourish) > 58)
        {
            if (ThreefoldLeft > 0)
                PushOGCD(AID.FanDanceIII, BestSplashTarget ?? target.Actor, 58);
            else if (FourfoldLeft > 0)
                PushOGCD(AID.FanDanceIV, BestConeTarget ?? target.Actor, 58);
        }

        if (ThreefoldLeft > 0)
            PushOGCD(AID.FanDanceIII, BestSplashTarget ?? target.Actor, 55);
        if (FourfoldLeft > 0)
            PushOGCD(AID.FanDanceIV, BestConeTarget ?? target.Actor, 54);

        if (ShouldSpendFeather(strategy, target))
            PushOGCD(AOEMode && Unlocked(AID.FanDanceII) ? AID.FanDanceII : AID.FanDance, AOEMode ? Player : target.Actor, 50);

        if (strategy.Waltz.Value == WaltzStrategy.Automatic && (Player.PendingHPRatio < 0.4f || World.Party.WithoutSlot(excludeAlliance: true).Count(p => !p.IsDead && p.Position.InCircle(Player.Position, 5) && p.PendingHPRatio < 0.5f) >= 3) && CanWeave(AID.CuringWaltz))
            PushOGCD(AID.CuringWaltz, Player, 45);
    }

    private bool ShouldDevilment(in Strategy strategy)
    {
        if (!Unlocked(AID.Devilment) || !CanWeave(AID.Devilment))
            return false;
        return strategy.Devilment.Value switch
        {
            OffensiveStrategy.Force => true,
            OffensiveStrategy.Delay => false,
            _ => TechFinishLeft > 0 || !Unlocked(AID.TechnicalStep)
        };
    }

    private bool ShouldFlourish(in Strategy strategy)
    {
        if (!Unlocked(AID.Flourish) || !CanWeave(AID.Flourish))
            return false;
        if (strategy.Flourish.Value == OffensiveStrategy.Force)
            return true;
        if (strategy.Flourish.Value == OffensiveStrategy.Delay)
            return false;
        var devilmentIn = ReadyIn(AID.Devilment);
        if (Unlocked(AID.Devilment) && (devilmentIn == 0 || devilmentIn <= 50 && (DevilmentLeft == 0 || DevilmentLeft >= 19)))
            return false;
        if (ThreefoldLeft > 0 || FourfoldLeft > 0 || SelfStatusLeft(SID.FlourishingSymmetry) > 0 || SelfStatusLeft(SID.FlourishingFlow) > 0 || FinishingMoveLeft > 0)
            return false;
        return TechFinishLeft > 0 || !Unlocked(AID.TechnicalStep) || ReadyIn(AID.TechnicalStep) > 15;
    }

    private bool ShouldSpendFeather(in Strategy strategy, Enemy target)
    {
        if (Feathers == 0 || !Unlocked(AID.FanDance) || strategy.Feathers.Value == FeatherStrategy.Delay)
            return false;
        var overcap = Feathers > 3 && SilkenLeft > 0;
        if (strategy.Feathers.Value == FeatherStrategy.Overcap)
            return overcap;
        if (!TimeToKill.WillLive(target.Actor, 5) || !Unlocked(AID.TechnicalStep))
            return true;
        return TechFinishLeft > 0 || overcap;
    }

    #endregion

    #region Dance partner

    private void Partner(in Strategy strategy)
    {
        if (!Unlocked(AID.ClosedPosition) || Dancing || strategy.Partner.Value == PartnerStrategy.Manual || ReadyIn(AID.ClosedPosition) > 0)
            return;

        var desired = strategy.Partner.Value == PartnerStrategy.SelectTarget ? ResolveTarget(strategy.Partner) : BestPartner();
        if (strategy.Partner.Value == PartnerStrategy.SelectTarget && desired != null && (desired.IsDead || desired.FindStatus(Weakness) != null || desired.FindStatus(BrinkOfDeath) != null))
            desired = BestPartner();
        if (desired == null || World.Party.Members.BoundSafeAt(World.Party.FindSlot(desired.InstanceID)).InCutscene)
            return;

        var current = World.Party.WithoutSlot(excludeAlliance: true).FirstOrDefault(p => p.FindStatus(SID.DancePartner, Player.InstanceID) != null);
        if (current == desired)
            return;
        if (Player.InCombat && current != null && (DevilmentLeft > 0 || strategy.Partner.Value == PartnerStrategy.Automatic
            && !current.IsDead && current.FindStatus(Weakness) == null && current.FindStatus(BrinkOfDeath) == null && !current.Statuses.Any(st => _damageDown.Contains(st.ID))))
            return;

        if (SelfStatusLeft(SID.ClosedPosition) > 0)
            PushOGCD(AID.Ending, Player, 40);
        else
            PushOGCD(AID.ClosedPosition, desired, 40);
    }

    private static readonly uint[] _damageDown = [2911, 62, 3304];
    private const uint Weakness = 43;
    private const uint BrinkOfDeath = 44;

    private Actor? BestPartner()
    {
        var candidates = World.Party.WithoutSlot(excludeAlliance: true).Exclude(Player)
            .Where(p => !p.IsDead && p.IsTargetable && Player.DistanceToHitbox(p) <= 30 && (p.FindStatus(SID.DancePartner) == null || p.FindStatus(SID.DancePartner, Player.InstanceID) != null))
            .ToList();
        if (candidates.Count == 0)
            return World.Actors.FirstOrDefault(x => x.Type == ActorType.Chocobo && x.OwnerID == Player.InstanceID);

        bool NotDD(Actor a) => !a.Statuses.Any(s => _damageDown.Contains(s.ID));
        bool NotSick(Actor a) => a.FindStatus(Weakness) == null && a.FindStatus(BrinkOfDeath) == null;
        bool NotBrink(Actor a) => a.FindStatus(BrinkOfDeath) == null;
        bool Melee(Actor a) => a.Role == Role.Melee;
        bool Dps(Actor a) => a.Role is Role.Melee or Role.Ranged;

        Func<Actor, bool>[] steps =
        [
            a => Melee(a) && NotDD(a) && NotSick(a),
            a => Dps(a) && NotDD(a) && NotSick(a),
            a => Melee(a) && NotSick(a),
            a => Dps(a) && NotSick(a),
            a => Melee(a) && NotBrink(a),
            a => Dps(a) && NotBrink(a),
            a => NotDD(a) && NotSick(a),
            NotSick,
            NotBrink,
            _ => true
        ];
        foreach (var step in steps)
        {
            var filtered = candidates.Where(step).ToList();
            if (filtered.Count > 0)
                return filtered.OrderBy(RolePrio).ThenBy(JobPrio).ThenByDescending(a => a.HPMP.MaxHP).First();
        }
        return candidates[0];
    }

    private static int RolePrio(Actor a) => a.Role switch
    {
        Role.Melee or Role.Ranged => 1,
        Role.Tank => 2,
        Role.Healer => 3,
        _ => 4
    };

    private int JobPrio(Actor a) => Player.Level switch
    {
        < 80 => a.Class switch
        {
            Class.SMN => 1,
            Class.MNK => 2,
            Class.BLM => 3,
            Class.DRG or Class.VPR or Class.PCT or Class.SAM or Class.RPR or Class.NIN or Class.RDM or Class.MCH => 4,
            Class.BRD => 5,
            Class.DNC => 6,
            _ => 9
        },
        < 90 => a.Class switch
        {
            Class.SAM => 1,
            Class.BLM => 2,
            Class.DRG or Class.MNK => 3,
            Class.PCT => 4,
            Class.MCH => 5,
            Class.NIN or Class.RDM or Class.RPR or Class.VPR or Class.SMN => 6,
            Class.BRD => 7,
            Class.DNC => 8,
            _ => 9
        },
        < 100 => a.Class switch
        {
            Class.SAM or Class.PCT => 1,
            Class.BLM => 2,
            Class.MNK or Class.VPR or Class.DRG => 3,
            Class.MCH or Class.RPR or Class.NIN => 4,
            Class.SMN or Class.RDM => 5,
            Class.BRD => 6,
            Class.DNC => 7,
            _ => 9
        },
        _ => a.Class switch
        {
            Class.SAM => 1,
            Class.PCT or Class.RPR or Class.VPR or Class.MNK or Class.NIN => 2,
            Class.DRG or Class.BLM => 3,
            Class.RDM => 4,
            Class.SMN => 5,
            Class.MCH => 6,
            Class.BRD => 7,
            Class.DNC => 8,
            _ => 9
        }
    };

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => World.Client.CountdownRemaining is > 0 and < 2 || Player.InCombat && Unlocked(AID.TechnicalStep) && (TechStepLeft > 0 || ReadyIn(AID.TechnicalStep) < 3),
        PotionStrategy.Immediate => true,
        _ => false
    };

    private Actor? BestAOETarget(Enemy? primary, float range, bool allowAoE, Func<Actor, Actor, bool> hits)
    {
        if (primary == null)
            return null;

        int Count(Actor center) => Hints.ForbiddenTargets.Any(e => hits(center, e.Actor)) ? 0 : Hints.PriorityTargets.Count(e => hits(center, e.Actor));

        var best = primary.Actor;
        if (!allowAoE || TargetMode == Targeting.Manual)
            return best;
        var bestCount = Count(best);
        foreach (var e in Hints.PriorityTargets)
        {
            if (e.Actor == primary.Actor || Player.DistanceToHitbox(e.Actor) > range || TargetMode == Targeting.AutoPrimary && !hits(e.Actor, primary.Actor))
                continue;
            var c = Count(e.Actor);
            if (c > bestCount)
                (best, bestCount) = (e.Actor, c);
        }
        return best;
    }

    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
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
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + priority);

    private void PushOGCD(AID aid, Actor? target, int priority)
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.Low + priority);

    private void PushOGCD(ClassShared.AID aid, Actor? target, int priority)
        => PushAction(ActionID.MakeSpell(aid), target, ActionQueue.Priority.Low + priority);

    private void PushAction(ActionID action, Actor? target, float priority)
    {
        if (action.ID == 0 || !ActionUnlocked(action))
            return;
        var def = ActionDefinitions.Instance[action];
        if (def == null || def.Range != 0 && target == null)
            return;
        Hints.ActionsToExecute.Push(action, target, priority);
    }

    #endregion
}
