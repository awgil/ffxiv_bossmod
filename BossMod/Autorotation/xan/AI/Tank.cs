using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public class TankAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Stance, Ranged, Interject, Stun, ArmsLength, Mit, Invuln }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Tank AI", "Utilities for tank AI - stance, provoke, interrupt, ranged attack", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PLD, Class.GLA, Class.WAR, Class.MRD, Class.DRK, Class.GNB), 100);

        def.AbilityTrack(Track.Stance, "Stance");
        def.AbilityTrack(Track.Ranged, "Ranged GCD");
        def.AbilityTrack(Track.Interject, "Interject").AddAssociatedActions(ClassShared.AID.Interject);
        def.AbilityTrack(Track.Stun, "Low Blow").AddAssociatedActions(ClassShared.AID.LowBlow);
        def.AbilityTrack(Track.ArmsLength, "Arms' Length").AddAssociatedActions(ClassShared.AID.ArmsLength);
        //def.AbilityTrack(Track.Mit, "Personal mits");
        //def.AbilityTrack(Track.Invuln, "Invuln");

        return def;
    }

    private ActionID RangedAction => Player.Class switch
    {
        Class.GLA or Class.PLD => ActionID.MakeSpell(BossMod.PLD.AID.ShieldLob),
        Class.MRD or Class.WAR => ActionID.MakeSpell(WAR.AID.Tomahawk),
        Class.DRK => ActionID.MakeSpell(BossMod.DRK.AID.Unmend),
        Class.GNB => ActionID.MakeSpell(BossMod.GNB.AID.LightningShot),
        _ => default
    };

    private (ActionID, uint) Stance => Player.Class switch
    {
        Class.GLA or Class.PLD => (ActionID.MakeSpell(BossMod.PLD.AID.IronWill), (uint)BossMod.PLD.SID.IronWill),
        Class.MRD or Class.WAR => (ActionID.MakeSpell(WAR.AID.Defiance), (uint)WAR.SID.Defiance),
        Class.DRK => (ActionID.MakeSpell(BossMod.DRK.AID.Grit), (uint)BossMod.DRK.SID.Grit),
        Class.GNB => (ActionID.MakeSpell(BossMod.GNB.AID.RoyalGuard), (uint)BossMod.GNB.SID.RoyalGuard),
        _ => default
    };

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn)
    {
        // ranged
        if (strategy.Enabled(Track.Ranged) && ActionUnlocked(RangedAction) && Player.DistanceToHitbox(primaryTarget) is > 5 and <= 20 && primaryTarget!.Type is ActorType.Enemy && !primaryTarget.IsAlly)
            Hints.ActionsToExecute.Push(RangedAction, primaryTarget, ActionQueue.Priority.Low);

        // stance
        var (stanceAction, stanceStatus) = Stance;
        if (strategy.Enabled(Track.Stance) && ActionUnlocked(stanceAction) && !Player.Statuses.Any(x => x.ID == stanceStatus))
            Hints.ActionsToExecute.Push(stanceAction, Player, ActionQueue.Priority.Minimal);

        // interrupt
        if (strategy.Enabled(Track.Interject) && Unlocked(ClassShared.AID.Interject) && Cooldown(ClassShared.AID.Interject) == 0)
        {
            var interruptibleEnemy = Hints.PotentialTargets.Find(e => ShouldInterrupt(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (interruptibleEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Interject), interruptibleEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        // low blow
        if (strategy.Enabled(Track.Stun) && Unlocked(ClassShared.AID.LowBlow) && Cooldown(ClassShared.AID.LowBlow) == 0)
        {
            var stunnableEnemy = Hints.PotentialTargets.Find(e => ShouldStun(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (stunnableEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.LowBlow), stunnableEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        switch (Player.Class)
        {
            case Class.PLD:
            case Class.GLA:
                ExecutePLD(strategy, primaryTarget);
                break;
            case Class.GNB:
                ExecuteGNB(strategy, primaryTarget);
                break;
        }
    }

    private void ExecutePLD(StrategyValues strategy, Actor? primaryTarget)
    {
        var gauge = GetGauge<PaladinGauge>();

        var attackers = EnemiesAutoingMe.Count();

        if (/*strategy.Enabled(Track.Mit) && */attackers > 0)
        {
            if (gauge.OathGauge >= 50 && HPRatio < 0.8)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.PLD.AID.Sheltron), Player, ActionQueue.Priority.Minimal);

            if (attackers > 1 && HPRatio < 0.6)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Rampart), Player, ActionQueue.Priority.Minimal);
        }
    }

    private void ExecuteGNB(StrategyValues strategy, Actor? primaryTarget)
    {
        var attackers = EnemiesAutoingMe.Count();

        if (/*strategy.Enabled(Track.Mit) && */attackers > 0)
        {
            if (HPRatio < 0.8 && Cooldown(BossMod.GNB.AID.HeartOfCorundum) == 0)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.GNB.AID.HeartOfCorundum), Player, ActionQueue.Priority.Minimal);

            if (HPRatio < 0.8 && Unlocked(BossMod.GNB.AID.Aurora) && Cooldown(BossMod.GNB.AID.Aurora) < 60 && Player.FindStatus(BossMod.GNB.SID.Aurora) == null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.GNB.AID.Aurora), Player, ActionQueue.Priority.Minimal);
        }
    }
}
