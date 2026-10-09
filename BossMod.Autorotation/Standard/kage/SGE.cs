using BossMod.SGE;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageSGE(RotationModuleManager manager, Actor player) : KageRotation<KageSGE.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Eukrasian Dosis", MinLevel = 30, Actions = [AID.Eukrasia, AID.EukrasianDosis, AID.EukrasianDosisII, AID.EukrasianDosisIII, AID.EukrasianDyskrasia])]
        public Track<DotStrategy> Dot;

        [Track("Burst (Phlegma, Psyche)", InternalName = "Burst", MinLevel = 26)]
        public Track<OffensiveStrategy> Burst;

        [Track("Phlegma", MinLevel = 26, Actions = [AID.Phlegma, AID.PhlegmaII, AID.PhlegmaIII])]
        public Track<OffensiveStrategy> Phlegma;

        [Track("Psyche", MinLevel = 92, Action = AID.Psyche)]
        public Track<OffensiveStrategy> Psyche;

        [Track("Toxikon", MinLevel = 66, Actions = [AID.Toxikon, AID.ToxikonII])]
        public Track<ToxikonStrategy> Toxikon;

        [Track("Pneuma (damage)", MinLevel = 90, Action = AID.Pneuma)]
        public Track<PneumaStrategy> Pneuma;

        [Track("Kardia", MinLevel = 4, Action = AID.Kardia)]
        public Track<KardiaStrategy> Kardia;

        [Track("Soteria", MinLevel = 35, Action = AID.Soteria)]
        public Track<OffensiveStrategy> Soteria;

        [Track("Rhizomata", MinLevel = 74, Action = AID.Rhizomata)]
        public Track<OffensiveStrategy> Rhizomata;

        [Track("Druochole (Addersgall overcap)", MinLevel = 45, Action = AID.Druochole)]
        public Track<DruocholeStrategy> Druochole;

        [Track("Lucid Dreaming", MinLevel = 14, Action = ClassShared.AID.LucidDreaming)]
        public Track<OffensiveStrategy> Lucid;

        [Track("Potion")]
        public Track<PotionStrategy> Potion;

        [Track("Opener", MinLevel = 30, UiPriority = -10)]
        public Track<OpenerStrategy> Opener;
    }

    public enum DotStrategy
    {
        [Option("Keep up on up to two targets; Eukrasian Dyskrasia on 3+")]
        Automatic,
        [Option("Only on the current target")]
        TargetOnly,
        [Option("Do not use")]
        Delay
    }

    public enum ToxikonStrategy
    {
        [Option("Use while moving and on 2+ targets")]
        Automatic,
        [Option("Only while moving")]
        Movement,
        [Option("Do not use")]
        Delay
    }

    public enum PneumaStrategy
    {
        [Option("Use on 2+ targets unless a raidwide is coming")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum KardiaStrategy
    {
        [Option("Keep on the tank taking hits")]
        Automatic,
        [Option("Do not change Kardia")]
        Manual,
        [Option("Keep on the selected party member", Targets = ActionTargets.Party | ActionTargets.Self)]
        Specific
    }

    public enum DruocholeStrategy
    {
        [Option("Use on the lowest HP ally to avoid overcapping")]
        Automatic,
        [Option("Do not use")]
        Delay
    }

    public enum PotionStrategy
    {
        [Option("Do not use automatically")]
        Manual,
        [Option("Use before the pull and with raid buffs")]
        AlignWithRaidBuffs,
        [Option("Use as soon as possible")]
        Immediate
    }

    public enum OpenerStrategy
    {
        [Option("Toxikon opener (Eukrasia at -5s, Toxikon at -1.5s)")]
        Toxikon,
        [Option("Pneuma opener (Eukrasia at -5s, Pneuma at -1.5s)", MinLevel = 90)]
        Pneuma,
        [Option("Dosis at -1.5s, no Eukrasia")]
        None
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage SGE", "Sage", "Standard rotation (Kage)|Healer", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.SGE), 100).WithStrategies<Strategy>();
    }

    private int Gall;
    private int Sting;
    private float NextGall;
    private bool Eukrasia;

    private bool AOEMode;
    private bool ForceST;

    protected override bool UsesSpellSpeed => true;
    private bool CanCast => Hints.MaxCastTime >= SlideCast(ScaledCastTime(AID.Dosis));

    private OffensiveStrategy PhlegmaStrat;
    private OffensiveStrategy PsycheStrat;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var target = SelectTarget(strategy.Targeting.Value, ref primaryTarget, estimatedAnimLockDelay, 25, strategy.AOE.Value);
        PhlegmaStrat = WithBurst(strategy.Burst.Value, strategy.Phlegma.Value);
        PsycheStrat = WithBurst(strategy.Burst.Value, strategy.Psyche.Value);
        ForceST = strategy.AOE.Value == AOEStrategy.ForceST;

        var gauge = World.Client.GetGauge<SageGauge>();
        Gall = gauge.Addersgall;
        Sting = gauge.Addersting;
        NextGall = MathF.Max(0, 20f - gauge.AddersgallTimer / 1000f);
        Eukrasia = gauge.EukrasiaActive;

        // force-ST counts no extra targets, as in Basexan
        var numAround = ForceST ? 0 : Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
        AOEMode = Unlocked(AID.Dyskrasia) && UseAOE(strategy.AOE.Value, numAround, 3);

        Kardia(strategy);

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            var precast = strategy.Opener.Value switch
            {
                OpenerStrategy.Pneuma when GCDReady(AID.Pneuma) => AID.Pneuma,
                OpenerStrategy.Toxikon or OpenerStrategy.Pneuma when Sting > 0 && Unlocked(AID.Toxikon) && strategy.Toxikon.Value != ToxikonStrategy.Delay => AID.Toxikon,
                _ => AID.None
            };
            if (target != null && precast != AID.None && Unlocked(AID.Eukrasia))
            {
                if (!Eukrasia && countdown < 5)
                    PushGCD(AID.Eukrasia, Player, 10);
                else if (countdown < 1.5f)
                    PushGCD(precast, target.Actor, 10);
            }
            else if (target != null && countdown < CastTime(AID.Dosis) + 0.2f)
                PushGCD(AID.Dosis, target.Actor, 10);
            return;
        }

        if (target == null)
            return;

        var goal = Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 25);
        Hints.GoalZones.Add(strategy.AOE.Value is AOEStrategy.ST or AOEStrategy.ForceST || !Unlocked(AID.Dyskrasia) ? goal : GoalCombined(goal, Hints.GoalAOECircle(5), 3));

        GCDs(strategy, target, numAround);
        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target, int numAround)
    {
        // only automatic dots spread with Eukrasian Dyskrasia
        var allowDotAoE = AOEMode && strategy.Dot.Value == DotStrategy.Automatic && Unlocked(AID.EukrasianDyskrasia);
        if (Eukrasia)
        {
            if (allowDotAoE)
                PushGCD(AID.Dyskrasia, Player, 60);
            PushGCD(AID.Dosis, DotTarget(strategy, target) ?? target.Actor, 59);
            return;
        }

        if (Player.InCombat && strategy.Dot.Value != DotStrategy.Delay)
        {
            var dotAoE = allowDotAoE && Hints.PriorityTargets.Count(e => Player.DistanceToHitbox(e.Actor) <= 5 && NeedsDot(e)) >= 3;
            if (dotAoE || !AOEMode && DotTarget(strategy, target) != null)
                PushGCD(AID.Eukrasia, Player, 50);
        }

        if (ShouldPhlegma(strategy, target))
        {
            Hints.GoalZones.Add(Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 6));
            if (Player.DistanceToHitbox(target.Actor) <= 6)
                PushGCD(AID.Phlegma, target.Actor, 40);
        }

        if (strategy.Pneuma.Value == PneumaStrategy.Automatic && CanCast && Player.InCombat && GCDReady(AID.Pneuma) && !RaidwideWithin(20)
            && !ForceST && Hints.NumPriorityTargetsInAOERect(Player.Position, Player.DirectionTo(target.Actor), 25, 2) >= 2)
            PushGCD(AID.Pneuma, target.Actor, 35);

        if (Sting > 0 && strategy.Toxikon.Value == ToxikonStrategy.Automatic && numAround < 3 && !ForceST && Hints.NumPriorityTargetsInAOECircle(target.Actor.Position, 5) >= 2)
            PushGCD(AID.Toxikon, target.Actor, 30);

        if (AOEMode)
            PushGCD(AID.Dyskrasia, Player, 20);

        PushGCD(AID.Dosis, target.Actor, 10);

        if (!CanCast)
        {
            if (Sting > 0 && strategy.Toxikon.Value != ToxikonStrategy.Delay)
                PushGCD(AID.Toxikon, target.Actor, 9);
            if (numAround > 0)
                PushGCD(AID.Dyskrasia, Player, 8);
        }
    }

    private float DotRefresh => GCD + GCDLength + 1;

    private float DotLeft(Actor target) => MaxDotLeft(target, SID.EukrasianDosisIII, SID.EukrasianDosisII, SID.EukrasianDosis, SID.EukrasianDyskrasia);

    private bool NeedsDot(Enemy e) => DotWorthIt(e) && DotLeft(e.Actor) < DotRefresh;

    private Actor? DotTarget(in Strategy strategy, Enemy target)
    {
        if (NeedsDot(target))
            return target.Actor;
        if (strategy.Dot.Value != DotStrategy.Automatic || Hints.PriorityTargets.Count(e => DotLeft(e.Actor) >= DotRefresh) >= 2)
            return null;
        return Hints.PriorityTargets.Where(e => e != target && Player.DistanceToHitbox(e.Actor) <= 25 && NeedsDot(e)).MaxBy(e => e.Actor.HPMP.CurHP)?.Actor;
    }

    private bool ShouldPhlegma(in Strategy strategy, Enemy target)
    {
        if (ReadyIn(AID.Phlegma) > GCD)
            return false;
        switch (PhlegmaStrat)
        {
            case OffensiveStrategy.Force:
                return true;
            case OffensiveStrategy.Delay:
                return false;
        }
        var buffsSoon = Player.InCombat && InBossFight && RaidBuffsLeft <= GCD && RaidBuffsIn < 10;
        if (MaxChargesIn(AID.Phlegma) <= GCD + GCDLength)
            return !buffsSoon;
        if (!Player.InCombat || target.Priority == Enemy.PriorityPointless && !TimeToKill.IsBossTier(Bossmods.ActiveModule, Hints, target.Actor))
            return false;
        if (RaidBuffsLeft > GCD || RaidBuffsIn > 9000 || !InBossFight || !ForceST && Hints.NumPriorityTargetsInAOECircle(target.Actor.Position, 5) >= 3)
            return true;
        var cooldown = ActionDefinitions.Instance.Spell(AID.Phlegma)!.Cooldown;
        return MaxChargesIn(AID.Phlegma) + cooldown <= RaidBuffsIn;
    }

    private bool MPCheckSoon => RaidwideWithin(15) || World.Party.WithoutSlot(excludeAlliance: true).Any(p => p.IsDead);

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        if (PsycheStrat != OffensiveStrategy.Delay && CanWeave(AID.Psyche)
            && (PsycheStrat == OffensiveStrategy.Force || DowntimeIn > 2 && (target.Priority != Enemy.PriorityPointless || TimeToKill.IsBossTier(Bossmods.ActiveModule, Hints, target.Actor))
                && (RaidBuffsLeft > 0 || RaidBuffsIn > 10 || !InBossFight)))
            PushOGCD(AID.Psyche, target.Actor, 50);

        if (strategy.Rhizomata.Value == OffensiveStrategy.Force || strategy.Rhizomata.Value == OffensiveStrategy.Automatic && Gall <= 1)
            if (CanWeave(AID.Rhizomata))
                PushOGCD(AID.Rhizomata, Player, 40);

        if (strategy.Druochole.Value == DruocholeStrategy.Automatic && Gall >= 3 && NextGall < GCDLength + 1 && CanWeave(AID.Druochole)
            && !(RaidwideWithin(15) && ReadyIn(AID.Kerachole) < 5))
        {
            var healTarget = World.Party.WithoutSlot(excludeAlliance: true).Where(p => !p.IsDead && Player.DistanceToHitbox(p) <= 30).MinBy(p => p.PendingHPRatio) ?? Player;
            PushOGCD(AID.Druochole, healTarget, 35);
        }

        if (strategy.Soteria.Value == OffensiveStrategy.Force || strategy.Soteria.Value == OffensiveStrategy.Automatic && Player.FindStatus(SID.Kardia) != null)
            if (CanWeave(AID.Soteria))
                PushOGCD(AID.Soteria, Player, 30);

        if (strategy.Lucid.Value == OffensiveStrategy.Force || strategy.Lucid.Value == OffensiveStrategy.Automatic && (MP <= 6500 || MP <= 8500 && MPCheckSoon))
            if (CanWeave(ClassShared.AID.LucidDreaming))
                PushOGCD(ClassShared.AID.LucidDreaming, Player, 20);
    }

    private void Kardia(in Strategy strategy)
    {
        if (strategy.Kardia.Value == KardiaStrategy.Manual || ReadyIn(AID.Kardia) > 0)
            return;

        var desired = strategy.Kardia.Value == KardiaStrategy.Specific ? ResolveTarget(strategy.Kardia) : KardiaTarget();
        if (desired == null || desired.IsDead || HasKardion(desired))
            return;

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Kardia), desired, Player.InCombat ? ActionQueue.Priority.Low + 60 : ActionQueue.Priority.High);
    }

    private bool HasKardion(Actor a) => a.FindStatus(SID.Kardion, Player.InstanceID, World.FutureTime(1000)) != null;

    private Actor? KardiaTarget()
    {
        var party = World.Party.WithoutSlot(excludeAlliance: true).ToList();
        if (party.Count == 1)
            return Player;
        var tanks = party.Where(p => p.Role == Role.Tank && !p.IsDead).ToList();
        // no living tank (tankless party, NPC allies): keep it on ourselves
        if (tanks.Count <= 1)
            return tanks.FirstOrDefault() ?? Player;

        var current = tanks.FirstOrDefault(HasKardion);
        int Aggro(Actor t) => Hints.PriorityTargets.Sum(e => e.Actor.TargetID != t.InstanceID ? 0 : e.Actor == Bossmods.ActiveModule?.PrimaryActor ? 10 : 1);
        var best = tanks.MaxBy(Aggro)!;
        return current != null && Aggro(best) <= Aggro(current) ? current : best;
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithRaidBuffs => PotionWithRaidBuffs,
        PotionStrategy.Immediate => true,
        _ => false
    };

    private float CastTime(AID aid) => Eukrasia && aid == AID.Dosis || SelfStatusLeft(ClassShared.SID.Swiftcast) > GCD ? 0 : ScaledCastTime(aid);

    private void PushGCD(AID aid, Actor? target, int priority) => base.PushGCD(aid, target, priority, CastTime(aid));

    #endregion
}
