using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public class TankAI(RotationModuleManager manager, Actor player) : AIBase<TankAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        public Track<StanceStrategy> Stance;
        [Track(InternalName = "Ranged GCD")]
        public Track<EnabledByDefault> Ranged;
        [Track(InternalName = "Interject2", Action = ClassShared.AID.Interject)]
        public Track<HintedStrategy> Interject;
        [Track(InternalName = "Low Blow", Action = ClassShared.AID.LowBlow)]
        public Track<EnabledByDefault> Stun;
        [Track("Arms' Length", InternalName = "Arms' Length", Action = ClassShared.AID.ArmsLength)]
        public Track<EnabledByDefault> ArmsLength;
        [Track("Personal mits", InternalName = "Personal mits")]
        public Track<EnabledByDefault> Mit;
        [Track(InternalName = "Invuln")]
        public Track<EnabledByDefault> Invuln;
        [Track("Protect party members", InternalName = "Protect party members")]
        public Track<EnabledByDefault> Protect;
    }

    public enum StanceStrategy
    {
        Enabled,
        Disabled,
        [Option("Leech mode: Only enable stance in FATEs")]
        LeechMode
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Tank AI", "Utilities for tank AI - stance, provoke, interrupt, ranged attack", "AI (xan)", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PLD, Class.GLA, Class.WAR, Class.MRD, Class.DRK, Class.GNB), 100).WithStrategies<Strategy>();
    }

    public record struct Buff(ActionID ID, float Duration, float ApplicationDelay = 0, Func<RotationModule, bool>? CanUse = null)
    {
        public Buff(object ID, float Duration, float ApplicationDelay = 0, Func<RotationModule, bool>? CanUse = null) : this(ActionID.MakeSpell((ClassShared.AID)ID), Duration, ApplicationDelay, CanUse) { }
    }

    public record struct TankActions(
        ActionID Ranged,
        ActionID Stance,
        uint StanceBuff,
        Buff Invuln,
        Buff PartyMit,
        Buff LongMit,
        Buff ShortMit,
        Buff AllyMit,
        Buff SmallMit = default
    );

    // 120s mit application delays are guessed here, the DT sheet doesn't show them (but Rampart is 0.62s)

    public static readonly TankActions WARActions = new(
        Ranged: Spell(WAR.AID.Tomahawk),
        Stance: Spell(WAR.AID.Defiance),
        StanceBuff: (uint)WAR.SID.Defiance,
        Invuln: new(WAR.AID.Holmgang, 10, 0),
        PartyMit: new(WAR.AID.ShakeItOff, 30),
        LongMit: new(WAR.AID.Vengeance, 15, 0.62f),

        // 8s lifesteal, 4s damage reduction, 20s shield
        // before upgrade: 6s lifesteal, 6s damage reduction
        ShortMit: new(WAR.AID.RawIntuition, 4, 0.62f),
        // 8s lifesteal, 4s damage reduction, 20s shield
        AllyMit: new(WAR.AID.NascentFlash, 4, 0.62f)
    );

    public static readonly TankActions PLDActions = new(
        Ranged: Spell(BossMod.PLD.AID.ShieldLob),
        Stance: Spell(BossMod.PLD.AID.IronWill),
        StanceBuff: (uint)BossMod.PLD.SID.IronWill,
        Invuln: new(BossMod.PLD.AID.HallowedGround, 10, 0),
        PartyMit: new(BossMod.PLD.AID.DivineVeil, 30),
        LongMit: new(BossMod.PLD.AID.Sentinel, 15, 0.62f),
        SmallMit: new(BossMod.PLD.AID.Bulwark, 10, 0.62f),

        // 8s 15% mit, 4s of an additional 15% mit, 12s regen
        // before upgrade: 6s 15%
        ShortMit: new(BossMod.PLD.AID.Sheltron, 8, 0, mod => mod.World.Client.GetGauge<PaladinGauge>().OathGauge >= 50),
        // same as above, no pre-upgrade version
        AllyMit: new(BossMod.PLD.AID.Intervention, 8, 0.80f, mod => mod.World.Client.GetGauge<PaladinGauge>().OathGauge >= 50)
    );

    public static readonly TankActions DRKActions = new(
        Ranged: Spell(BossMod.DRK.AID.Unmend),
        Stance: Spell(BossMod.DRK.AID.Grit),
        StanceBuff: (uint)BossMod.DRK.SID.Grit,
        Invuln: new(BossMod.DRK.AID.LivingDead, 10, 0),
        PartyMit: new(BossMod.DRK.AID.DarkMissionary, 15, 0.62f),
        LongMit: new(BossMod.DRK.AID.ShadowWall, 15, 0.62f),
        SmallMit: new(BossMod.DRK.AID.DarkMind, 10, 0.62f),

        ShortMit: new(BossMod.DRK.AID.TheBlackestNight, 7, 0.62f, mod => mod.Player.HPMP.CurMP >= 3000),
        AllyMit: new(BossMod.DRK.AID.TheBlackestNight, 7, 0.62f, mod => mod.Player.HPMP.CurMP >= 3000)
    );

    public static readonly TankActions GNBActions = new(
        Ranged: Spell(BossMod.GNB.AID.LightningShot),
        Stance: Spell(BossMod.GNB.AID.RoyalGuard),
        StanceBuff: (uint)BossMod.GNB.SID.RoyalGuard,
        Invuln: new(BossMod.GNB.AID.Superbolide, 10, 0),
        PartyMit: new(BossMod.GNB.AID.HeartOfLight, 15, 0.62f),
        LongMit: new(BossMod.GNB.AID.Nebula, 15, 0.54f),
        SmallMit: new(BossMod.GNB.AID.Camouflage, 20, 0.62f),

        // 8s 15% mit, 4s of an additional 15% mit, 20s excog
        ShortMit: new(BossMod.GNB.AID.HeartOfStone, 8, 0.62f),
        AllyMit: new(BossMod.GNB.AID.HeartOfStone, 8, 0.62f)
    );

    public static TankActions ActionsForJob(Class c) => c switch
    {
        Class.GLA or Class.PLD => PLDActions,
        Class.MRD or Class.WAR => WARActions,
        Class.DRK => DRKActions,
        Class.GNB => GNBActions,
        _ => throw new InvalidOperationException($"{c} is not a tank class")
    };

    private TankActions JobActions => ActionsForJob(Player.Class);

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        // ranged
        if (ShouldRanged(strategy, primaryTarget))
            Hints.ActionsToExecute.Push(JobActions.Ranged, primaryTarget, ActionQueue.Priority.Low);

        // stance
        AutoStance(strategy);

        var interjectStrategy = strategy.Interject.Value;

        // interrupt
        if (interjectStrategy.IsEnabled() && NextChargeIn(ClassShared.AID.Interject) == 0)
        {
            bool shouldInterrupt(AIHints.Enemy e) => e.Actor.InCombat && interjectStrategy.Check(e.ShouldBeInterrupted) && e.Actor.CastInfo?.Interruptible == true;

            var interruptibleEnemy = Hints.PotentialTargets.FirstOrDefault(e => shouldInterrupt(e) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Interject), interruptibleEnemy.Actor, ActionQueue.Priority.VeryLow);
        }

        // low blow
        if (strategy.Stun.IsEnabled() && NextChargeIn(ClassShared.AID.LowBlow) == 0)
        {
            var stunnableEnemy = Hints.PotentialTargets.Find(e => ShouldInterrupt(e) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (stunnableEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.LowBlow), stunnableEnemy.Actor, ActionQueue.Priority.VeryLow);
        }

        if (strategy.ArmsLength.IsEnabled() && EnemiesAutoingMe.Count() > 1)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), Player, ActionQueue.Priority.VeryLow);

        if (strategy.Protect.IsEnabled())
            AutoProtect();

        if (strategy.Mit.IsEnabled())
            AutoMit(strategy);

        switch (Player.Class)
        {
            case Class.GNB:
                ExecuteGNB(strategy);
                break;
            case Class.MRD:
            case Class.WAR:
                ExecuteWAR(strategy);
                break;
        }
    }

    private void AutoStance(in Strategy strategy)
    {
        var stanceStrategy = strategy.Stance.Value;
        switch (stanceStrategy)
        {
            case StanceStrategy.Enabled:
                if (Player.FindStatus(JobActions.StanceBuff) == null)
                    Hints.ActionsToExecute.Push(JobActions.Stance, Player, ActionQueue.Priority.VeryLow);
                return;
            case StanceStrategy.LeechMode:
                var wantOn = World.Client.ActiveFate.ID != 0;
                var haveOn = Player.FindStatus(JobActions.StanceBuff) != null;
                if (wantOn != haveOn)
                    Hints.ActionsToExecute.Push(JobActions.Stance, Player, ActionQueue.Priority.VeryLow);
                return;
        }
    }

    private bool ShouldRanged(in Strategy strategy, Actor? primaryTarget)
    {
        return strategy.Ranged.IsEnabled()
            && Player.DistanceToHitbox(primaryTarget) is > 5 and <= 20
            && !primaryTarget!.IsAlly
            && !Player.Statuses.Any(x => x.ID is (uint)WAR.SID.Berserk or (uint)WAR.SID.InnerRelease);
    }

    private void AutoProtect()
    {
        var threat = Hints.PotentialTargets.TakeWhile(t => t.Priority >= AIHints.Enemy.PriorityInvincible).FirstOrDefault(x =>
            // fate mobs are immune to provoke and we probably don't care about this anyway
            x.Actor.FateID == 0
            && World.Party.TryFindSlot(x.Actor.TargetID, out var slot)
            && World.Party[slot]!.Class.GetRole() != Role.Tank
        );
        if (threat != null)
        {
            // provoke works on invincible mobs
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Provoke), threat.Actor, ActionQueue.Priority.Medium);

            if (threat.Priority > AIHints.Enemy.PriorityInvincible)
            {
                if (Player.DistanceToHitbox(threat.Actor) > 3)
                    Hints.ActionsToExecute.Push(JobActions.Ranged, threat.Actor, ActionQueue.Priority.ManualGCD - 100);
                else
                    // in case all mobs are in melee range, but there aren't enough mobs to switch to aoe
                    Hints.ForcedTarget = threat.Actor;
            }
        }

        foreach (var rw in Raidwides)
            if (World.FutureTime(5) > rw)
            {
                Hints.ActionsToExecute.Push(JobActions.PartyMit.ID, Player, ActionQueue.Priority.Medium);
                if (Player.DistanceToHitbox(Bossmods.ActiveModule?.PrimaryActor) <= 5)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Reprisal), Player, ActionQueue.Priority.Low);
            }

        foreach (var (ally, t) in Tankbusters)
            if (ally != Player && (t - World.CurrentTime).TotalSeconds < 4)
                Hints.ActionsToExecute.Push(JobActions.AllyMit.ID, ally, ActionQueue.Priority.Low);
    }

    private void AutoMit(in Strategy strategy)
    {
        if (Player.PendingHPRaw == Player.HPMP.CurHP && !Player.InCombat)
            return;

        if (Player.PendingHPRatio < 0.8)
        {
            var delay = 0f;
            if (JobActions.ShortMit.ID == ActionID.MakeSpell(WAR.AID.RawIntuition))
                delay = GCD - 0.8f;
            Hints.ActionsToExecute.Push(JobActions.ShortMit.ID, Player, ActionQueue.Priority.VeryLow, delay: delay);
        }

        if (Player.PendingHPRatio < 0.6)
            // set arbitrary deadline to 1 second in the future
            UseOneMit(1);

        // PLD/GNB don't have application delay protection so trying to use it in this case would just waste the CD
        if (Player.PendingHPRaw <= 0 && strategy.Invuln.IsEnabled() && Player.Class is Class.MRD or Class.WAR or Class.DRK)
            Hints.ActionsToExecute.Push(JobActions.Invuln.ID, Player, ActionQueue.Priority.VeryHigh);

        foreach (var t in Tankbusters)
            if (t.Item1 == Player)
                UseOneMit((float)(t.Item2 - World.CurrentTime).TotalSeconds);
    }

    private void ExecuteGNB(in Strategy strategy)
    {
        if (strategy.Mit.IsEnabled() && EnemiesAutoingMe.Any())
        {
            if (Player.PendingHPRatio < 0.8 && Player.FindStatus(BossMod.GNB.SID.Aurora) == null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.GNB.AID.Aurora), Player, ActionQueue.Priority.VeryLow);
        }
    }

    private void ExecuteWAR(in Strategy strategy)
    {
        if (strategy.Mit.IsEnabled() && EnemiesAutoingMe.Any())
        {
            if (Player.PendingHPRatio < 0.75)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WAR.AID.Bloodwhetting), Player, ActionQueue.Priority.Low, delay: GCD - 1f);

            if (Player.PendingHPRatio < 0.5)
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WAR.AID.ThrillOfBattle), Player, ActionQueue.Priority.Low);
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WAR.AID.Equilibrium), Player, ActionQueue.Priority.Low);
            }
        }
    }

    private void UseOneMit(float deadline)
    {
        var longmit = GetMitStatus(JobActions.LongMit.ID, 15, deadline);
        var rampart = GetMitStatus(ActionID.MakeSpell(ClassShared.AID.Rampart), 20, deadline);
        var shortmit = GetMitStatus(JobActions.ShortMit.ID, JobActions.ShortMit.Duration, deadline, JobActions.ShortMit.CanUse);

        if (longmit.Active || rampart.Active && shortmit.Active)
            return;

        if (longmit.Usable)
        {
            Hints.ActionsToExecute.Push(JobActions.LongMit.ID, Player, ActionQueue.Priority.Low);
            return;
        }

        if (rampart.Usable)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Rampart), Player, ActionQueue.Priority.Low);

        if (shortmit.Usable)
            Hints.ActionsToExecute.Push(JobActions.ShortMit.ID, Player, ActionQueue.Priority.Low);
    }

    private (bool Ready, bool Active, bool Usable) GetMitStatus(ActionID action, float actionDuration, float deadline, Func<RotationModule, bool>? resourceCheck = null)
    {
        var currentCD = NextChargeIn(action);
        var maxCD = ActionDefinitions.Instance[action]!.Cooldown;
        var effectRemaining = MathF.Max(0, actionDuration + currentCD - maxCD);
        var ready = currentCD < deadline;
        if (resourceCheck != null)
            ready &= resourceCheck(this);
        return (ready, effectRemaining > deadline, ready || effectRemaining > deadline);
    }
}
