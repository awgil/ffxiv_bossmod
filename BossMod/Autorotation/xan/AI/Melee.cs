namespace BossMod.Autorotation.xan;
public class MeleeAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { SecondWind, Bloodbath, Stun }
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Melee DPS AI", "Utilities for melee - bloodbath, second wind, stun", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PGL, Class.MNK, Class.LNC, Class.DRG, Class.ROG, Class.NIN, Class.SAM, Class.RPR, Class.VPR), 100);

        def.AbilityTrack(Track.SecondWind, "Second Wind");
        def.AbilityTrack(Track.Bloodbath, "Bloodbath");
        def.AbilityTrack(Track.Stun, "Stun");

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn)
    {
        if (Player.Statuses.Any(x => x.ID is (uint)BossMod.NIN.SID.TenChiJin or (uint)BossMod.NIN.SID.Mudra))
            return;

        // second wind
        if (strategy.Enabled(Track.SecondWind) && Unlocked(ClassShared.AID.SecondWind) && Cooldown(ClassShared.AID.SecondWind) == 0 && Player.InCombat && Player.HPMP.CurHP <= Player.HPMP.MaxHP / 2)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SecondWind), Player, ActionQueue.Priority.Medium);

        // bloodbath
        if (strategy.Enabled(Track.Bloodbath) && Unlocked(ClassShared.AID.Bloodbath) && Cooldown(ClassShared.AID.Bloodbath) == 0 && Player.InCombat && Player.HPMP.CurHP <= Player.HPMP.MaxHP * 0.75)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Bloodbath), Player, ActionQueue.Priority.Medium);

        // low blow
        if (strategy.Enabled(Track.Stun) && Unlocked(ClassShared.AID.LegSweep) && Cooldown(ClassShared.AID.LegSweep) == 0)
        {
            var stunnableEnemy = Hints.PotentialTargets.Find(e => ShouldStun(e.Actor) && Player.DistanceToHitbox(e.Actor) <= 3);
            if (stunnableEnemy != null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.LegSweep), stunnableEnemy.Actor, ActionQueue.Priority.Minimal);
        }

        if (Player.Class == Class.SAM)
            AISAM();
    }

    private void AISAM()
    {
        // if nearby enemies are auto-attacking us, use guard skill
        if (Cooldown(BossMod.SAM.AID.ThirdEye) == 0
            && Player.HPMP.CurHP < Player.HPMP.MaxHP * 0.8
            && EnemiesAutoingMe.Any())
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BossMod.SAM.AID.ThirdEye), Player, ActionQueue.Priority.Low);
    }
}
