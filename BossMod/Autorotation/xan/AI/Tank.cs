using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public class TankAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Stance, Ranged, Interject, Stun, ArmsLength, Mit, Invuln, Protect }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Tank AI", "Utilities for tank AI - stance, provoke, interrupt, ranged attack", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PLD, Class.GLA, Class.WAR, Class.MRD, Class.DRK, Class.GNB), 100);

        def.AbilityTrack(Track.Stance, "Stance");
        def.AbilityTrack(Track.Ranged, "Ranged GCD");
        def.AbilityTrack(Track.Interject, "Interject").AddAssociatedActions(ClassShared.AID.Interject);
        def.AbilityTrack(Track.Stun, "Low Blow").AddAssociatedActions(ClassShared.AID.LowBlow);
        def.AbilityTrack(Track.ArmsLength, "Arms' Length").AddAssociatedActions(ClassShared.AID.ArmsLength);
        def.AbilityTrack(Track.Mit, "Personal mits");
        def.AbilityTrack(Track.Invuln, "Invuln");
        def.AbilityTrack(Track.Protect, "Protect party members");

        return def;
    }

    public record struct TankActions(ActionID Ranged, ActionID Stance, uint StanceBuff, ActionID PartyMit, ActionID LongMit, ActionID ShortMit, float ShortMitDuration, ActionID AllyMit, ActionID SmallMit = default, Func<RotationModule, bool>? ShortMitCheck = null);

    private static TankActions WARActions = new(
        Ranged: Spell(WAR.AID.Tomahawk),
        Stance: Spell(WAR.AID.Defiance),
        StanceBuff: (uint)WAR.SID.Defiance,
        PartyMit: Spell(WAR.AID.ShakeItOff),
        LongMit: Spell(WAR.AID.Vengeance),
        ShortMit: Spell(WAR.AID.RawIntuition),
        ShortMitDuration: 8,
        AllyMit: Spell(WAR.AID.NascentFlash)
    );
    private static TankActions PLDActions = new(
        Ranged: Spell(BossMod.PLD.AID.ShieldLob),
        Stance: Spell(BossMod.PLD.AID.IronWill),
        StanceBuff: (uint)BossMod.PLD.SID.IronWill,
        PartyMit: Spell(BossMod.PLD.AID.DivineVeil),
        LongMit: Spell(BossMod.PLD.AID.Sentinel),
        ShortMit: Spell(BossMod.PLD.AID.Sheltron),
        ShortMitDuration: 8,
        AllyMit: Spell(BossMod.PLD.AID.Intervention),
        SmallMit: Spell(BossMod.PLD.AID.Bulwark),
        ShortMitCheck: (mod) => mod.World.Client.GetGauge<PaladinGauge>().OathGauge >= 50
    );
    private static TankActions DRKActions = new(
        Ranged: Spell(BossMod.DRK.AID.Unmend),
        Stance: Spell(BossMod.DRK.AID.Grit),
        StanceBuff: (uint)BossMod.DRK.SID.Grit,
        PartyMit: Spell(BossMod.DRK.AID.DarkMissionary),
        LongMit: Spell(BossMod.DRK.AID.ShadowWall),
        ShortMit: Spell(BossMod.DRK.AID.TheBlackestNight),
        ShortMitDuration: 7,
        AllyMit: Spell(BossMod.DRK.AID.TheBlackestNight),
        SmallMit: Spell(BossMod.DRK.AID.DarkMind),
        ShortMitCheck: (mod) => mod.Player.HPMP.CurMP >= 3000
    );
    private static TankActions GNBActions = new(
        Ranged: Spell(BossMod.GNB.AID.LightningShot),
        Stance: Spell(BossMod.GNB.AID.RoyalGuard),
        StanceBuff: (uint)BossMod.GNB.SID.RoyalGuard,
        PartyMit: Spell(BossMod.GNB.AID.HeartOfLight),
        LongMit: Spell(BossMod.GNB.AID.Nebula),
        ShortMit: Spell(BossMod.GNB.AID.HeartOfCorundum),
        ShortMitDuration: 8,
        AllyMit: Spell(BossMod.GNB.AID.HeartOfCorundum),
        SmallMit: Spell(BossMod.GNB.AID.Camouflage)
    );

    private TankActions JobActions => Player.Class switch
    {
        Class.GLA or Class.PLD => PLDActions,
        Class.MRD or Class.WAR => WARActions,
        Class.DRK => DRKActions,
        Class.GNB => GNBActions,
        _ => default
    };

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        // ranged
        if (strategy.Enabled(Track.Ranged) && Player.DistanceToHitbox(primaryTarget) is > 5 and <= 20 && primaryTarget!.Type is ActorType.Enemy && !primaryTarget.IsAlly)
            Hints.ActionsToExecute.Push(JobActions.Ranged, primaryTarget, ActionQueue.Priority.Low);

        // stance
        if (strategy.Enabled(Track.Stance) && !Player.Statuses.Any(x => x.ID == JobActions.StanceBuff))
            Hints.ActionsToExecute.Push(JobActions.Stance, Player, ActionQueue.Priority.Minimal);

        // interrupt
        if (strategy.Enabled(Track.Interject) && NextChargeIn(ClassShared.AID.Interject) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.FirstOrDefault(e => ShouldInterrupt(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Interject), interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        // low blow
        if (strategy.Enabled(Track.Stun) && NextChargeIn(ClassShared.AID.LowBlow) == 0)
        {
            var stunnableEnemy = Hints.PotentialTargets.Find(e => ShouldStun(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (stunnableEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.LowBlow), stunnableEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        if (strategy.Enabled(Track.ArmsLength) && EnemiesAutoingMe.Count() > 1)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), Player, ActionQueue.Priority.Minimal);

        if (strategy.Enabled(Track.Protect))
            AutoProtect();

        if (strategy.Enabled(Track.Mit))
            AutoMit();

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

    private void AutoProtect()
    {
        var threat = Hints.PriorityTargets.FirstOrDefault(x =>
            // skip all of this for fates mobs, we can't provoke them and probably don't care about this anyway
            x.Actor.FateID == 0
            && World.Actors.Find(x.Actor.TargetID) is Actor victim
            && victim.IsAlly
            && victim.Class.GetRole() != Role.Tank
        );
        if (threat != null)
        {
            if (Player.DistanceToHitbox(threat.Actor) > 3)
                Hints.ActionsToExecute.Push(JobActions.Ranged, threat.Actor, ActionQueue.Priority.VeryHigh);
            else
                // in case all mobs are in melee range, but there aren't enough mobs to switch to aoe
                Hints.ForcedTarget = threat.Actor;

            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Provoke), threat.Actor, ActionQueue.Priority.Medium);
        }

        foreach (var rw in Raidwides)
            if ((rw - World.CurrentTime).TotalSeconds < 5)
            {
                Hints.ActionsToExecute.Push(JobActions.PartyMit, Player, ActionQueue.Priority.Medium);
                if (Player.DistanceToHitbox(Bossmods.ActiveModule?.PrimaryActor) <= 5)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Reprisal), Player, ActionQueue.Priority.Low);
            }

        foreach (var (ally, t) in Tankbusters)
            if (ally != Player && (t - World.CurrentTime).TotalSeconds < 4)
                Hints.ActionsToExecute.Push(JobActions.AllyMit, ally, ActionQueue.Priority.Low);
    }

    private void AutoMit()
    {
        if (EnemiesAutoingMe.Count() > 1)
        {
            if (HPRatio() < 0.8)
                Hints.ActionsToExecute.Push(JobActions.ShortMit, Player, ActionQueue.Priority.Minimal);

            if (HPRatio() < 0.6)
                // set arbitrary deadline to 1 second in the future
                UseOneMit(1);
        }

        foreach (var t in Tankbusters)
            if (t.Item1 == Player)
                UseOneMit((float)(t.Item2 - World.CurrentTime).TotalSeconds);
    }

    private void ExecuteGNB(StrategyValues strategy)
    {
        if (strategy.Enabled(Track.Mit) && EnemiesAutoingMe.Any())
        {
            if (HPRatio() < 0.8 && Player.FindStatus(BossMod.GNB.SID.Aurora) == null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.GNB.AID.Aurora), Player, ActionQueue.Priority.Minimal);
        }
    }

    private void ExecuteWAR(StrategyValues strategy)
    {
        if (strategy.Enabled(Track.Mit) && EnemiesAutoingMe.Any())
        {
            if (HPRatio() < 0.75)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WAR.AID.Bloodwhetting), Player, ActionQueue.Priority.Low, delay: GCD - 1f);

            if (HPRatio() < 0.5)
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WAR.AID.ThrillOfBattle), Player, ActionQueue.Priority.Low);
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WAR.AID.Equilibrium), Player, ActionQueue.Priority.Low);
            }
        }
    }

    private void UseOneMit(float deadline)
    {
        var longmit = GetMitStatus(JobActions.LongMit, 15, deadline);
        var rampart = GetMitStatus(ActionID.MakeSpell(ClassShared.AID.Rampart), 20, deadline);
        var shortmit = GetMitStatus(JobActions.ShortMit, JobActions.ShortMitDuration, deadline, JobActions.ShortMitCheck);

        if (longmit.Active || rampart.Active && shortmit.Active)
            return;

        if (longmit.Usable)
        {
            Hints.ActionsToExecute.Push(JobActions.LongMit, Player, ActionQueue.Priority.Low);
            return;
        }

        if (rampart.Usable)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Rampart), Player, ActionQueue.Priority.Low);

        if (shortmit.Usable)
            Hints.ActionsToExecute.Push(JobActions.ShortMit, Player, ActionQueue.Priority.Low);
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
