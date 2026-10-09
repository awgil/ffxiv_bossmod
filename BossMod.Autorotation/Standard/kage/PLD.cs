using BossMod.PLD;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KagePLD(RotationModuleManager manager, Actor player) : KageRotation<KagePLD.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Fight or Flight", MinLevel = 2, Action = AID.FightOrFlight)]
        public Track<OffensiveStrategy> FightOrFlight;

        [Track("Requiescat / Imperator", MinLevel = 68, Actions = [AID.Requiescat, AID.Imperator])]
        public Track<OffensiveStrategy> Requiescat;

        [Track("Goring Blade", MinLevel = 54, Action = AID.GoringBlade)]
        public Track<OffensiveStrategy> Goring;

        [Track("Circle of Scorn / Expiacion", MinLevel = 30, Actions = [AID.CircleOfScorn, AID.SpiritsWithin, AID.Expiacion])]
        public Track<OffensiveStrategy> Spenders;

        [Track("Intervene", MinLevel = 74, Action = AID.Intervene)]
        public Track<InterveneStrategy> Intervene;

        [Track("Sheltron (Oath overcap)", MinLevel = 35, Actions = [AID.Sheltron, AID.HolySheltron])]
        public Track<SheltronStrategy> Sheltron;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 2, UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum InterveneStrategy
    {
        [Option("Use in Fight or Flight; never overcap")]
        Automatic,
        [Option("Also use as a gap closer", Targets = ActionTargets.Hostile)]
        GapClose,
        [Option("Do not use")]
        Delay
    }

    public enum SheltronStrategy
    {
        [Option("Spend Oath at 95+ while being hit")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use with Fight or Flight")]
        AlignWithBurst,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Standard opener (pre-pull Holy Spirit)")]
        Standard,
        [Option("Early Fight or Flight opener")]
        EarlyBuff,
        [Option("No opener-specific rules")]
        None
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage PLD", "Paladin", "Standard rotation (Kage)|Tank", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.PLD, Class.GLA), 100).WithStrategies<Strategy>();
    }

    private const int HolySpiritMP = 1000;
    private const uint PassageOfArmsSID = 1175;
    private const uint SheltronSID = 1856;
    private const uint HolySheltronSID = 2674;

    private int Oath;
    private float FoFLeft;
    private float RequiescatLeft;
    private float ConfiteorLeft;
    private float GoringLeft;
    private float DivineMightLeft;
    private float AtonementLeft;
    private float SupplicationLeft;
    private float SepulchreLeft;
    private float BladeOfHonorLeft;

    private Actor? BestSplashTarget;
    private Actor? BestMeleeSplashTarget;
    private int NumAOETargets;
    private bool AOEMode;
    private bool InMelee;

    private AID ComboLastMove => (AID)World.Client.ComboState.Action;
    private int AoEMinTargets => Player.Level >= 94 ? 3 : 2;
    private float FoFIn; // fight or flight on Delay never comes up, so nothing waits for it
    private AID BestRequiescat => Unlocked(AID.Imperator) ? AID.Imperator : AID.Requiescat;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Player.FindStatus(PassageOfArmsSID, Player.InstanceID) != null)
            return;

        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 3, strategy.AOE.Value);

        Oath = World.Client.GetGauge<PaladinGauge>().OathGauge;
        FoFLeft = SelfStatusLeft(SID.FightOrFlight);
        FoFIn = strategy.FightOrFlight.Value == OffensiveStrategy.Delay ? float.MaxValue : ReadyIn(AID.FightOrFlight);
        RequiescatLeft = SelfStatusLeft(SID.Requiescat);
        ConfiteorLeft = SelfStatusLeft(SID.ConfiteorReady);
        GoringLeft = SelfStatusLeft(SID.GoringBladeReady);
        DivineMightLeft = SelfStatusLeft(SID.DivineMight);
        AtonementLeft = SelfStatusLeft(SID.AtonementReady);
        SupplicationLeft = SelfStatusLeft(SID.SupplicationReady);
        SepulchreLeft = SelfStatusLeft(SID.SepulchreReady);
        BladeOfHonorLeft = SelfStatusLeft(SID.BladeOfHonorReady);

        var allowAoE = strategy.AOE.Value is AOEStrategy.AOE or AOEStrategy.ForceAOE;
        NumAOETargets = Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
        // force-ST counts only the primary target, keeping the forbidden-target check
        if (strategy.AOE.Value == AOEStrategy.ForceST)
            NumAOETargets = NumAOETargets > 0 && target != null && TargetInAOECircle(target.Actor, Player.Position, 5) ? 1 : 0;
        AOEMode = Unlocked(AID.TotalEclipse) && UseAOE(strategy.AOE.Value, NumAOETargets, AoEMinTargets);
        InMelee = target != null && Player.DistanceToHitbox(target.Actor) <= 3;
        BestSplashTarget = BestAOETarget(target, 25, allowAoE, (c, e) => TargetInAOECircle(e, c.Position, 5)).Best;
        BestMeleeSplashTarget = BestAOETarget(target, 3, allowAoE, (c, e) => TargetInAOECircle(e, c.Position, 5)).Best;

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            // the AI walks into melee after the cast; moving now would stop it from starting
            if (target != null && strategy.Opener.Value == OpenerStrategy.Standard && countdown < 2.2f && Unlocked(AID.HolySpirit) && MP >= HolySpiritMP && Player.DistanceToHitbox(target.Actor) <= 25)
            {
                var pos = Player.Position;
                Hints.GoalZones.Add(p => p.InCircle(pos, 0.5f) ? 100 : 0);
                if (countdown < 1.75f)
                    PushGCD(AID.HolySpirit, target.Actor, 10);
            }
            return;
        }

        if (target == null)
            return;

        var goal = Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 3);
        Hints.GoalZones.Add(allowAoE && Unlocked(AID.TotalEclipse) ? GoalCombined(goal, Hints.GoalAOECircle(5), AoEMinTargets) : goal);

        GCDs(strategy, target);
        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target)
    {
        var holyAction = AOEMode && Unlocked(AID.HolyCircle) ? AID.HolyCircle : AID.HolySpirit;
        var holyTarget = holyAction == AID.HolyCircle ? Player : target.Actor;
        var canHoly = Unlocked(holyAction) && MP >= HolySpiritMP;

        var requiescatSoon = FoFLeft > 0 && strategy.Requiescat.Value != OffensiveStrategy.Delay && ReadyIn(BestRequiescat) <= GCD;
        if (strategy.Goring.Value != OffensiveStrategy.Delay && GoringLeft > GCD && (RequiescatLeft == 0 && !requiescatSoon || strategy.Goring.Value == OffensiveStrategy.Force))
        {
            var goringTarget = AOEMode
                ? Hints.PriorityTargets.Where(e => Player.DistanceToHitbox(e.Actor) <= 3).MaxBy(e => e.Actor.HPMP.CurHP)?.Actor
                : InMelee ? target.Actor : null;
            if (goringTarget != null && (!AOEMode || NumAOETargets <= (Player.Level >= 72 ? 3 : 4)))
                PushGCD(AID.GoringBlade, goringTarget, 60);
        }

        var next = (World.Client.GetGauge<PaladinGauge>().ConfiteorComboStep & 0xFF) switch
        {
            1 => AID.BladeOfFaith,
            2 => AID.BladeOfTruth,
            3 => AID.BladeOfValor,
            _ => ConfiteorLeft > GCD ? AID.Confiteor : AID.None
        };
        // below 90 the confiteor chain doesn't use up requiescat, so the rest goes to holy
        if (next != AID.None && Unlocked(next) && MP >= HolySpiritMP)
            PushGCD(next, BestSplashTarget, 58);
        else if (RequiescatLeft > GCD && canHoly)
            PushGCD(holyAction, holyTarget, 58);

        if (canHoly && DivineMightLeft > GCD && (FoFLeft > GCD || !InMelee || ComboLastMove == (AOEMode ? AID.TotalEclipse : AID.RiotBlade) || DivineMightLeft < 6))
            PushGCD(holyAction, holyTarget, DivineMightLeft < 6 || FoFLeft <= GCD ? 46 : 44);

        if (AOEMode)
        {
            if (Unlocked(AID.Prominence) && ComboLastMove == AID.TotalEclipse && ComboLeft > GCD)
                PushGCD(AID.Prominence, Player, 10);
            PushGCD(AID.TotalEclipse, Player, 1);
        }
        else
        {
            if (!InMelee)
            {
                if (canHoly)
                    PushGCD(AID.HolySpirit, target.Actor, 20);
                PushGCD(AID.ShieldLob, target.Actor, 19);
            }

            var spend = FoFLeft > GCD || ComboLastMove == AID.RiotBlade;
            if (AtonementLeft > GCD)
                PushGCD(AID.Atonement, target.Actor, 48);
            if (SupplicationLeft > GCD && (spend || SupplicationLeft < 6))
                PushGCD(AID.Supplication, target.Actor, 47);
            if (SepulchreLeft > GCD && (spend || SepulchreLeft < 6))
                PushGCD(AID.Sepulchre, target.Actor, 45);

            if (Unlocked(AID.RageOfHalone) && ComboLastMove == AID.RiotBlade && ComboLeft > GCD)
                PushGCD(Unlocked(AID.RoyalAuthority) ? AID.RoyalAuthority : AID.RageOfHalone, target.Actor, 11);
            if (Unlocked(AID.RiotBlade) && ComboLastMove == AID.FastBlade && ComboLeft > GCD)
                PushGCD(AID.RiotBlade, target.Actor, 10);
            PushGCD(AID.FastBlade, target.Actor, 1);
        }
    }

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        var requiescat = BestRequiescat;
        if (ShouldFightOrFlight(strategy, target.Actor))
            PushOGCD(AID.FightOrFlight, Player, 70, ReadyIn(requiescat) <= GCD ? 0 : GCD - 0.8f);

        if (BladeOfHonorLeft > 0)
            PushOGCD(AID.BladeOfHonor, BestSplashTarget, 65);

        // requiescat and spirits within are single target melee; only imperator and expiacion can take the splash target
        var imperator = requiescat == AID.Imperator;
        if (strategy.Requiescat.Value != OffensiveStrategy.Delay && CanWeave(requiescat) && Player.DistanceToHitbox(target.Actor) <= (imperator ? 25 : 3)
            && (strategy.Requiescat.Value == OffensiveStrategy.Force || FoFLeft > 0 || FoFIn > 50))
            PushOGCD(requiescat, imperator ? BestSplashTarget : target.Actor, 64);

        if (strategy.Spenders.Value != OffensiveStrategy.Delay)
        {
            var hold = strategy.Spenders.Value != OffensiveStrategy.Force && FoFLeft == 0 && FoFIn < 2.5f;
            if (!hold && CanWeave(AID.CircleOfScorn) && NumAOETargets > 0)
                PushOGCD(AID.CircleOfScorn, Player, 60);
            var spirits = Unlocked(AID.Expiacion) ? AID.Expiacion : AID.SpiritsWithin;
            if (!hold && CanWeave(spirits))
                PushOGCD(spirits, spirits == AID.Expiacion ? BestMeleeSplashTarget : target.Actor, 59);
        }

        Intervene(strategy, target);

        if (strategy.Sheltron.Value == SheltronStrategy.Automatic && Oath >= 95 && Hints.PotentialTargets.Any(e => e.Actor.TargetID == Player.InstanceID && e.Actor.InCombat))
        {
            var sheltron = Unlocked(AID.HolySheltron) ? AID.HolySheltron : AID.Sheltron;
            if (CanWeave(sheltron) && SelfStatusLeft(sheltron == AID.HolySheltron ? HolySheltronSID : SheltronSID, 8) == 0)
                PushOGCD(sheltron, Player, 40);
        }
    }

    private bool ShouldFightOrFlight(in Strategy strategy, Actor target)
    {
        if (!CanWeave(AID.FightOrFlight) || strategy.FightOrFlight.Value == OffensiveStrategy.Delay)
            return false;
        if (strategy.FightOrFlight.Value == OffensiveStrategy.Force)
            return true;
        if (DowntimeIn < 10)
            return false;
        if (!TimeToKill.BurstWorthIt(Bossmods.ActiveModule, Hints, target, 10))
            return false;
        if (Unlocked(AID.Requiescat) && MP < HolySpiritMP * 3.6f)
            return false;
        if (!InMelee && !CanWeave(AID.Imperator))
            return false;
        if (CombatTime < 15)
        {
            return strategy.Opener.Value switch
            {
                OpenerStrategy.Standard => ComboLastMove == AID.RoyalAuthority || !Unlocked(AID.RoyalAuthority) && CombatTime >= 7,
                OpenerStrategy.EarlyBuff => ComboLastMove is AID.FastBlade or AID.RiotBlade or AID.RoyalAuthority,
                _ => CombatTime >= 8
            };
        }
        return true;
    }

    private void Intervene(in Strategy strategy, Enemy target)
    {
        if (strategy.Intervene.Value == InterveneStrategy.Delay || !CanWeave(AID.Intervene))
            return;
        var dist = Player.DistanceToHitbox(target.Actor);
        if (strategy.Intervene.Value == InterveneStrategy.GapClose && dist > 3 && dist <= 20)
        {
            PushOGCD(AID.Intervene, ResolveTarget(strategy.Intervene) ?? target.Actor, 55);
            return;
        }
        if (dist > 3 || Hints.MaxCastTime <= 0)
            return;
        var capping = MaxChargesIn(AID.Intervene) < GCD + 2;
        if (FoFLeft > 0 || capping && FoFIn > 25)
            PushOGCD(AID.Intervene, target.Actor, 55);
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithBurst => Player.InCombat && ReadyIn(AID.FightOrFlight) < 3 && (CombatTime > 15 || ComboLastMove is AID.RiotBlade or AID.FastBlade),
        PotionStrategy.Immediate => true,
        _ => false
    };

    private void PushGCD(AID aid, Actor? target, int priority) => base.PushGCD(aid, target, priority, CastTime(aid));

    // 1.5s cast unless Divine Might or Requiescat; the queue needs it to not cast into a dodge
    private float CastTime(AID aid)
    {
        if (aid is not (AID.HolySpirit or AID.HolyCircle) || DivineMightLeft > GCD || RequiescatLeft > GCD)
            return 0;
        return ActionDefinitions.Instance.Spell(aid)?.CastTime ?? 0;
    }

    #endregion
}
