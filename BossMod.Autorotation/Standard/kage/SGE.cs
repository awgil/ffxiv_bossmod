using BossMod.SGE;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.kage;

public sealed class KageSGE(RotationModuleManager manager, Actor player) : TypedRotationModule<KageSGE.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;

        [Track("Eukrasian Dosis", MinLevel = 30, Actions = [AID.Eukrasia, AID.EukrasianDosis, AID.EukrasianDosisII, AID.EukrasianDosisIII, AID.EukrasianDyskrasia])]
        public Track<DotStrategy> Dot;

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
        [Option("Do not use for damage")]
        Delay
    }

    public enum KardiaStrategy
    {
        [Option("Keep on the main tank")]
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

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Kage SGE", "Sage", "Standard rotation (Kage)|Healer", "Kagekazu", RotationModuleQuality.WIP, BitMask.Build(Class.SGE), 100).WithStrategies<Strategy>();
    }

    private static readonly SID[] DotStatus = [SID.EukrasianDosisIII, SID.EukrasianDosisII, SID.EukrasianDosis, SID.EukrasianDyskrasia];

    private int Gall;
    private int Sting;
    private float NextGall;
    private bool Eukrasia;

    private float RaidBuffsLeft;
    private float RaidBuffsIn;
    private float DowntimeIn;
    private bool AOEMode;
    private bool IsMoving;
    private float AnimLockDelay;
    private Targeting TargetMode;

    private float GCDLength => ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level);
    private bool CanCast => !IsMoving && Hints.MaxCastTime >= 1.5f * GCDLength / 2.5f;

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        AnimLockDelay = estimatedAnimLockDelay;
        IsMoving = isMoving;

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

        var gauge = World.Client.GetGauge<SageGauge>();
        Gall = gauge.Addersgall;
        Sting = gauge.Addersting;
        NextGall = MathF.Max(0, 20f - gauge.AddersgallTimer / 1000f);
        Eukrasia = gauge.EukrasiaActive;

        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);
        DowntimeIn = Manager.Planner?.EstimateTimeToNextDowntime() is (var downNow, var stateLeft) ? (downNow ? 0 : stateLeft) : float.MaxValue;

        var numAround = Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);
        AOEMode = Unlocked(AID.Dyskrasia) && strategy.AOE.Value switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.AOE => numAround >= 3,
            _ => false
        };

        Kardia(strategy);

        if (UsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.Medium);

        if (World.Client.CountdownRemaining is > 0 and var countdown)
        {
            if (target != null && countdown < 1.5f * GCDLength / 2.5f + 0.2f)
                PushGCD(AID.Dosis, target.Actor, 10);
            return;
        }

        if (target == null)
            return;

        var goal = Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 25);
        Hints.GoalZones.Add(strategy.AOE.Value == AOEStrategy.ST || !Unlocked(AID.Dyskrasia) ? goal : GoalCombined(goal, Hints.GoalAOECircle(5), 3));

        GCDs(strategy, target, numAround);
        if (Player.InCombat)
            OGCDs(strategy, target);
    }

    #region GCD

    private void GCDs(in Strategy strategy, Enemy target, int numAround)
    {
        // Eukrasia is up: the next GCD has to cash it in
        if (Eukrasia)
        {
            if (AOEMode && Unlocked(AID.EukrasianDyskrasia))
                PushGCD(AID.Dyskrasia, Player, 60);
            PushGCD(AID.Dosis, DotTarget(strategy, target) ?? target.Actor, 59);
            return;
        }

        if (Player.InCombat && strategy.Dot.Value != DotStrategy.Delay)
        {
            var dotAoE = AOEMode && Unlocked(AID.EukrasianDyskrasia) && Hints.PriorityTargets.Count(e => Player.DistanceToHitbox(e.Actor) <= 5 && DotWorthIt(e) && DotLeft(e.Actor) < DotRefresh) >= 3;
            if (dotAoE || !AOEMode && DotTarget(strategy, target) != null)
                PushGCD(AID.Eukrasia, Player, 50);
        }

        if (ShouldPhlegma(strategy, target))
        {
            Hints.GoalZones.Add(Hints.GoalSingleTarget(target.Actor, Player, World.Actors, 6));
            if (Player.DistanceToHitbox(target.Actor) <= 6)
                PushGCD(AID.Phlegma, target.Actor, 40);
        }

        if (strategy.Pneuma.Value == PneumaStrategy.Automatic && CanCast && Player.InCombat && GCDReady(AID.Pneuma) && !RaidwideSoon(20)
            && Hints.NumPriorityTargetsInAOERect(Player.Position, Player.DirectionTo(target.Actor), 25, 2) >= 2)
            PushGCD(AID.Pneuma, target.Actor, 35);

        var toxTargets = Hints.NumPriorityTargetsInAOECircle(target.Actor.Position, 5);
        if (Sting > 0 && strategy.Toxikon.Value == ToxikonStrategy.Automatic && toxTargets >= 2 && numAround < 3)
            PushGCD(AID.Toxikon, target.Actor, 30);

        if (AOEMode)
            PushGCD(AID.Dyskrasia, Player, 20);

        PushGCD(AID.Dosis, target.Actor, 10);

        // movement: instants; the queue skips Dosis when it can't be cast
        if (!CanCast)
        {
            if (Sting > 0 && strategy.Toxikon.Value != ToxikonStrategy.Delay)
                PushGCD(AID.Toxikon, target.Actor, 9);
            if (numAround > 0)
                PushGCD(AID.Dyskrasia, Player, 8);
        }
    }

    // a hard-cast Dosis now + Eukrasia + E.Dosis must still land before it drops
    private float DotRefresh => GCD + GCDLength + 1;

    private float DotLeft(Actor target)
    {
        foreach (var sid in DotStatus)
            if (target.FindStatus((uint)sid, Player.InstanceID) is { } st)
                return StatusDuration(st.ExpireAt);
        return 0;
    }

    // E.Dosis needs ~15s of ticks to beat Dosis
    private bool DotWorthIt(Enemy e)
    {
        if (e.ForbidDOTs || e.Priority < 0 && !IsBossTarget(e.Actor) || DowntimeIn < 15)
            return false;
        return TimeToKill.WillLive(e.Actor, 15);
    }

    private Actor? DotTarget(in Strategy strategy, Enemy target)
    {
        if (DotWorthIt(target) && DotLeft(target.Actor) < DotRefresh)
            return target.Actor;
        if (strategy.Dot.Value != DotStrategy.Automatic)
            return null;
        var dotted = Hints.PriorityTargets.Count(e => DotLeft(e.Actor) >= DotRefresh);
        if (dotted >= 2)
            return null;
        return Hints.PriorityTargets.Where(e => e != target && Player.DistanceToHitbox(e.Actor) <= 25 && DotWorthIt(e) && DotLeft(e.Actor) < DotRefresh).MaxBy(e => e.Actor.HPMP.CurHP)?.Actor;
    }

    private bool IsBossTarget(Actor target)
        => target.IsStrikingDummy || Bossmods.ActiveModule?.PrimaryActor is { } boss && (target == boss || target.HPMP.CurHP >= boss.HPMP.CurHP);

    // 3 charges per 2 minutes: two in raid buffs, the third anywhere as long as two are back for the next burst
    private bool ShouldPhlegma(in Strategy strategy, Enemy target)
    {
        if (!Unlocked(AID.Phlegma) || ReadyIn(AID.Phlegma) > GCD)
            return false;
        switch (strategy.Phlegma.Value)
        {
            case OffensiveStrategy.Force:
                return true;
            case OffensiveStrategy.Delay:
                return false;
        }
        if (MaxChargesIn(AID.Phlegma) <= GCD + GCDLength)
            return true;
        if (!Player.InCombat || target.Priority == Enemy.PriorityPointless && !IsBossTarget(target.Actor))
            return MaxChargesIn(AID.Phlegma) <= GCD + GCDLength;
        if (RaidBuffsLeft > GCD || RaidBuffsIn > 9000 || Hints.NumPriorityTargetsInAOECircle(target.Actor.Position, 5) >= 3)
            return true;
        var cooldown = ActionDefinitions.Instance.Spell(AID.Phlegma)!.Cooldown;
        return MaxChargesIn(AID.Phlegma) + cooldown <= RaidBuffsIn;
    }

    // heal check or raise coming: start Lucid before the MP is needed
    private bool MPCheckSoon => RaidwideSoon(15) || World.Party.WithoutSlot(excludeAlliance: true).Any(p => p.IsDead);

    private bool RaidwideSoon(float within) => Hints.PredictedDamage.Any(d => d.Type is PredictedDamageType.Raidwide or PredictedDamageType.Shared && d.Activation <= World.FutureTime(within));

    #endregion

    #region oGCD

    private void OGCDs(in Strategy strategy, Enemy target)
    {
        if (strategy.Psyche.Value != OffensiveStrategy.Delay && CanWeave(AID.Psyche)
            && (strategy.Psyche.Value == OffensiveStrategy.Force || DowntimeIn > 2 && (target.Priority != Enemy.PriorityPointless || IsBossTarget(target.Actor))))
            PushOGCD(AID.Psyche, target.Actor, 50);

        if (strategy.Rhizomata.Value == OffensiveStrategy.Force || strategy.Rhizomata.Value == OffensiveStrategy.Automatic && Gall <= 1)
            if (CanWeave(AID.Rhizomata))
                PushOGCD(AID.Rhizomata, Player, 40);

        // Kerachole/Ixochole from the healer AI spend it better when a raidwide is coming
        if (strategy.Druochole.Value == DruocholeStrategy.Automatic && Gall >= 3 && NextGall < GCDLength + 1 && CanWeave(AID.Druochole)
            && !(RaidwideSoon(15) && Unlocked(AID.Kerachole) && ReadyIn(AID.Kerachole) < 5))
        {
            var healTarget = World.Party.WithoutSlot(excludeAlliance: true).Where(p => !p.IsDead && Player.DistanceToHitbox(p) <= 30).MinBy(p => p.HPRatio) ?? Player;
            PushOGCD(AID.Druochole, healTarget, 35);
        }

        if (strategy.Soteria.Value == OffensiveStrategy.Force || strategy.Soteria.Value == OffensiveStrategy.Automatic && Player.FindStatus(SID.Kardia) != null)
            if (CanWeave(AID.Soteria))
                PushOGCD(AID.Soteria, Player, 30);

        if (strategy.Lucid.Value == OffensiveStrategy.Force || strategy.Lucid.Value == OffensiveStrategy.Automatic && (Player.HPMP.CurMP <= 6500 || Player.HPMP.CurMP <= 8500 && MPCheckSoon))
            if (CanWeave(ActionID.MakeSpell(ClassShared.AID.LucidDreaming)))
                PushOGCD(ActionID.MakeSpell(ClassShared.AID.LucidDreaming), Player, 20);
    }

    private void Kardia(in Strategy strategy)
    {
        if (!Unlocked(AID.Kardia) || strategy.Kardia.Value == KardiaStrategy.Manual || ReadyIn(AID.Kardia) > 0)
            return;

        var desired = strategy.Kardia.Value == KardiaStrategy.Specific ? ResolveTarget(strategy.Kardia) : KardiaTarget();
        if (desired == null || desired.IsDead || desired.FindStatus(SID.Kardion, Player.InstanceID) != null)
            return;

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.Kardia), desired, Player.InCombat ? ActionQueue.Priority.Low + 60 : ActionQueue.Priority.High);
    }

    // solo: self; one tank: that tank; two tanks: the one the boss is hitting (else the one holding more adds), sticky to the current one
    private Actor? KardiaTarget()
    {
        var party = World.Party.WithoutSlot(excludeAlliance: true).ToList();
        if (party.Count == 1)
            return Player;
        var tanks = party.Where(p => p.Role == Role.Tank && !p.IsDead).ToList();
        if (tanks.Count == 0)
            return null;
        if (tanks.Count == 1)
            return tanks[0];

        var current = tanks.FirstOrDefault(t => t.FindStatus(SID.Kardion, Player.InstanceID) != null);
        int Aggro(Actor t) => Hints.PriorityTargets.Sum(e => e.Actor.TargetID != t.InstanceID ? 0 : e.Actor == Bossmods.ActiveModule?.PrimaryActor ? 10 : 1);
        var best = tanks.MaxBy(Aggro)!;
        if (current != null && Aggro(best) <= Aggro(current))
            return current;
        return Aggro(best) > 0 ? best : current ?? tanks[0];
    }

    #endregion

    #region Helpers

    private bool UsePotion(in Strategy strategy) => strategy.Potion.Value switch
    {
        PotionStrategy.AlignWithRaidBuffs => World.Client.CountdownRemaining is > 0 and < 2 || Player.InCombat && (RaidBuffsLeft > 0 || RaidBuffsIn < 5),
        PotionStrategy.Immediate => true,
        _ => false
    };

    private float CastTime(AID aid)
    {
        if (Eukrasia || SelfStatusLeft(ClassShared.SID.Swiftcast) > GCD)
            return 0;
        var def = ActionDefinitions.Instance.Spell(aid);
        return def == null || def.CastTime == 0 ? 0 : def.CastTime * GCDLength / 2.5f;
    }

    private float SelfStatusLeft(ClassShared.SID sid) => Player.FindStatus((uint)sid, Player.InstanceID) is { } st ? StatusDuration(st.ExpireAt) : 0;

    private float MaxChargesIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ChargeCapIn(World.Client.Cooldowns, World.Client.DutyActions, Player.Level) : float.MaxValue;
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));
    private float ReadyIn(AID aid) => Unlocked(aid) ? ActionDefinitions.Instance.Spell(aid)!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions) : float.MaxValue;
    private bool GCDReady(AID aid) => ReadyIn(aid) <= GCD + 0.05f;

    private bool CanWeave(AID aid) => CanWeave(ActionID.MakeSpell(aid));
    private bool CanWeave(ActionID action)
    {
        if (!ActionUnlocked(action) || ActionDefinitions.Instance[action] is not { } def)
            return false;
        return MathF.Max(def.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions), World.Client.AnimationLock) + def.TotalDuration + AnimLockDelay <= GCD;
    }

    private void PushGCD(AID aid, Actor? target, int priority)
    {
        if (!Unlocked(aid))
            return;
        var action = ActionID.MakeSpell(aid);
        var def = ActionDefinitions.Instance[action];
        if (def == null || def.Range != 0 && target == null)
            return;
        Hints.ActionsToExecute.Push(action, def.Range == 0 ? Player : target, ActionQueue.Priority.High + priority, castTime: CastTime(aid));
    }

    private void PushOGCD(AID aid, Actor? target, int priority) => PushOGCD(ActionID.MakeSpell(aid), target, priority);
    private void PushOGCD(ActionID action, Actor? target, int priority)
    {
        if (!ActionUnlocked(action))
            return;
        Hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.Low + priority);
    }

    #endregion
}
