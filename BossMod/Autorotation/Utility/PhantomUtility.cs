using BossMod.Data;

namespace BossMod.Autorotation;

public class PhantomUtility(RotationModuleManager manager, Actor player) : GenericUtility(manager, player)
{
    public enum Track
    {
        Guard,
        Pledge,
        Shirahadori,
        Unicorn,
        Bell,
        RomeosBallad,
        MightyMarch,
        Predict,
        Rejuvenation,
        Invuln
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Utility: Phantom Jobs", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "xan", RotationModuleQuality.Ok, new BitMask(~0ul), 100);

        DefineSimpleConfig(def, Track.Guard, "PGuard", "Phantom Guard", -10, PhantomID.PhantomGuard, 10);
        DefineSimpleConfig(def, Track.Pledge, "Pledge", "Pledge", -10, PhantomID.Pledge, 10);
        DefineSimpleConfig(def, Track.Shirahadori, "Shirahadori", "Shira", -10, PhantomID.Shirahadori, 4);
        DefineSimpleConfig(def, Track.Unicorn, "Unicorn", "Unicorn", -10, PhantomID.OccultUnicorn, 30);
        DefineSimpleConfig(def, Track.Bell, "BattleBell", "Bell", -10, PhantomID.BattleBell, 60);
        DefineSimpleConfig(def, Track.RomeosBallad, "Romeo", "Romeo", -10, PhantomID.RomeosBallad, 3);
        DefineSimpleConfig(def, Track.MightyMarch, "MightyMarch", "March", -10, PhantomID.MightyMarch, 30);
        DefineSimpleConfig(def, Track.Predict, "Predict", "Predict", -10, PhantomID.Predict, 16, ActionQueue.Priority.VeryHigh - 10);
        DefineSimpleConfig(def, Track.Rejuvenation, "Rejuvenation", "Rejuv", -10, PhantomID.PhantomRejuvenation, 20);
        DefineSimpleConfig(def, Track.Invuln, "Invulnerability", "Invuln", -10, PhantomID.Invulnerability, 8);

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var cotank = ActionDefinitions.FindCoTank(World, Player);

        ExecutePhantom(strategy.Option(Track.Guard), PhantomID.PhantomGuard, Player);
        ExecutePhantom(strategy.Option(Track.Pledge), PhantomID.Pledge, cotank);
        ExecutePhantom(strategy.Option(Track.Shirahadori), PhantomID.Shirahadori, Player);
        ExecutePhantom(strategy.Option(Track.Unicorn), PhantomID.OccultUnicorn, Player);
        ExecutePhantom(strategy.Option(Track.Bell), PhantomID.BattleBell, Player);
        ExecutePhantom(strategy.Option(Track.RomeosBallad), PhantomID.RomeosBallad, Player);
        ExecutePhantom(strategy.Option(Track.MightyMarch), PhantomID.MightyMarch, Player);
        ExecutePhantom(strategy.Option(Track.Predict), PhantomID.Predict, Player);
        ExecutePhantom(strategy.Option(Track.Rejuvenation), PhantomID.PhantomRejuvenation, Player);
        ExecutePhantom(strategy.Option(Track.Invuln), PhantomID.Invulnerability, cotank);
    }

    private void ExecutePhantom(in StrategyValues.OptionRef opt, PhantomID pid, Actor? defaultTarget, float castTime = 0)
    {
        if (World.Client.DutyActions.All(d => d.Action.ID != (uint)(object)pid))
            return;

        ExecuteSimple(opt, pid, defaultTarget, castTime);
    }
}
