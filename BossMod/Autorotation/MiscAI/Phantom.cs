using BossMod.Data;
using Lumina.Extensions;

namespace BossMod.Autorotation.MiscAI;

public sealed class PhantomActions(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Tracks
    {
        PhantomOracleJudgment,
        PhantomOracleClensing,
        PhantomOracleBlessing,
        PhantomOracleStarfall,
        PhantomOracleRejuvination,
        PhantomBerserker,
        PhantomGeomancerBuffs,
    }

    public enum PhantomEnabled
    {
        Off = 0,
        On = 1,
        Fallback = 2,
    }

    public enum PhantomClass
    {
        Freelancer = 4242,
        Knight = 4358,
        Berserker = 4359,
        Monk = 4360,
        Ranger = 4361,
        Samurai = 4362,
        Bard = 4363,
        Geomancer = 4364,
        TimeMage = 4365,
        Cannoneer = 4366,
        Chemist = 4367,
        Oracle = 4368,
        Thief = 4369,
    }

    public enum PhantomStatus : uint
    {
        BattleBell = 4251,
        RingingRespite = 4257,
        PredictionJudegment = 4265,
        PredictionCleansing = 4266,
        PredictionBlessing = 4267,
        PredictionStarfall = 4268,
    }

    private uint PhantomJobLevel(Actor actor, PhantomClass job)
    {
        return (uint)(actor.Statuses.FirstOrNull(s => s.ID == (uint)job)?.Extra & 0xff ?? 0);
    }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition def = new("Misc AI: Phantom Jobs", "Phantom job utilities", "AI", "Erisen", RotationModuleQuality.WIP, new(~0ul), 1000);

        def.Define(Tracks.PhantomOracleJudgment).As<PhantomEnabled>("Judgement", "Oracle: Use Phantom Judgment")
        .AddOption(PhantomEnabled.Off, "Disabled")
        .AddOption(PhantomEnabled.On, "Enabled")
        .AddOption(PhantomEnabled.Fallback, "FallbackOnly", "Use only if HP is too low for Starfall")
        .AddAssociatedActions(
            PhantomID.Predict,
            PhantomID.PhantomJudgement
        );
        def.Define(Tracks.PhantomOracleClensing).As<PhantomEnabled>("Cleansing", "Oracle: Use Cleansing")
        .AddOption(PhantomEnabled.Off, "Disabled")
        .AddOption(PhantomEnabled.On, "Enabled")
        .AddOption(PhantomEnabled.Fallback, "FallbackOnly", "Use only if HP is too low for Starfall")
        .AddAssociatedActions(
            PhantomID.Predict,
            PhantomID.Cleansing
        );
        def.Define(Tracks.PhantomOracleBlessing).As<PhantomEnabled>("Blessing", "Oracle: Use Blessing")
        .AddOption(PhantomEnabled.Off, "Disabled")
        .AddOption(PhantomEnabled.On, "Enabled")
        .AddOption(PhantomEnabled.Fallback, "FallbackOnly", "Use only if HP is too low for Starfall")
        .AddAssociatedActions(
            PhantomID.Predict,
            PhantomID.Blessing
        );
        def.Define(Tracks.PhantomOracleStarfall).As<PhantomEnabled>("Starfall", "Oracle: Use Starfall")
        .AddOption(PhantomEnabled.Off, "Disabled")
        .AddOption(PhantomEnabled.On, "Enabled", "Use always, may cause death")
        .AddOption(PhantomEnabled.Fallback, "WhenSafe", "Use only if HP is full")
        .AddAssociatedActions(
            PhantomID.Predict,
            PhantomID.Starfall
        );
        def.Define(Tracks.PhantomOracleRejuvination).As<PhantomEnabled>("Rejuvination", "Oracle: Use Rejuvination")
        .AddOption(PhantomEnabled.Off, "Disabled")
        .AddOption(PhantomEnabled.On, "Enabled", "Use when damaged")
        .AddAssociatedActions(
            PhantomID.PhantomRejuvenation
        );
        def.Define(Tracks.PhantomBerserker).As<PhantomEnabled>("Berserker", "Berserker: Use Berserker actions")
        .AddOption(PhantomEnabled.Off, "Disabled")
        .AddOption(PhantomEnabled.On, "Enabled")
        .AddAssociatedActions(
            PhantomID.Rage,
            PhantomID.DeadlyBlow
        );
        def.Define(Tracks.PhantomGeomancerBuffs).As<PhantomEnabled>("BattleBell", "Geomancer: Use Battle Bell and Ringing Respite")
        .AddOption(PhantomEnabled.Off, "Disabled")
        .AddOption(PhantomEnabled.On, "Enabled", "Use on self")
        .AddOption(PhantomEnabled.Fallback, "FallbackOnly", "Use on main tank if available, otherwise use on self")
        .AddAssociatedActions(
            PhantomID.BattleBell,
            PhantomID.RingingRespite
        );

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        //resolve prediction before incombat check because you just die if you dont use one of them.
        var predict = false;
        if (PhantomJobLevel(Player, PhantomClass.Oracle) >= 1)
        {
            if (strategy.Option(Tracks.PhantomOracleJudgment).As<PhantomEnabled>() == PhantomEnabled.On ||
                strategy.Option(Tracks.PhantomOracleJudgment).As<PhantomEnabled>() == PhantomEnabled.Fallback && Player.PendingHPRatio < .95)
            {
                if (Player.Statuses.Any(s => s.ID == (uint)PhantomStatus.PredictionJudegment))
                    UseSkill(PhantomID.PhantomJudgement, Player, strategy.Option(Tracks.PhantomOracleJudgment).Priority(ActionQueue.Priority.Medium + 900));
                predict = true;
            }
            if (strategy.Option(Tracks.PhantomOracleClensing).As<PhantomEnabled>() == PhantomEnabled.On ||
                strategy.Option(Tracks.PhantomOracleClensing).As<PhantomEnabled>() == PhantomEnabled.Fallback && Player.PendingHPRatio < .95)
            {
                if (Player.Statuses.Any(s => s.ID == (uint)PhantomStatus.PredictionCleansing))
                    UseSkill(PhantomID.Cleansing, Player, strategy.Option(Tracks.PhantomOracleClensing).Priority(ActionQueue.Priority.Medium + 900));
                predict = true;
            }
            if (strategy.Option(Tracks.PhantomOracleBlessing).As<PhantomEnabled>() == PhantomEnabled.On ||
                strategy.Option(Tracks.PhantomOracleBlessing).As<PhantomEnabled>() == PhantomEnabled.Fallback && Player.PendingHPRatio < .95)
            {
                if (Player.Statuses.Any(s => s.ID == (uint)PhantomStatus.PredictionBlessing))
                    UseSkill(PhantomID.Blessing, Player, strategy.Option(Tracks.PhantomOracleBlessing).Priority(ActionQueue.Priority.Medium + 900));
                predict = true;
            }
            if (strategy.Option(Tracks.PhantomOracleStarfall).As<PhantomEnabled>() == PhantomEnabled.On ||
                strategy.Option(Tracks.PhantomOracleStarfall).As<PhantomEnabled>() == PhantomEnabled.Fallback && Player.PendingHPRatio >= .95)
            {
                if (Player.Statuses.Any(s => s.ID == (uint)PhantomStatus.PredictionStarfall))
                    UseSkill(PhantomID.Starfall, Player, strategy.Option(Tracks.PhantomOracleStarfall).Priority(ActionQueue.Priority.Medium + 900));
                predict = true;
            }
        }

        if (!Player.InCombat)
            return;

        if (predict)
        {
            UseSkill(PhantomID.Predict, Player, strategy.Option(Tracks.PhantomOracleJudgment).Priority(ActionQueue.Priority.High + 100));
        }
        if (strategy.Option(Tracks.PhantomOracleRejuvination).As<PhantomEnabled>() == PhantomEnabled.On &&
            Player.PendingHPRatio < 1 && PhantomJobLevel(Player, PhantomClass.Oracle) >= 4)
        {
            UseSkill(PhantomID.PhantomRejuvenation, Player, strategy.Option(Tracks.PhantomOracleRejuvination).Priority(ActionQueue.Priority.Low + 500));
        }
        if (strategy.Option(Tracks.PhantomBerserker).As<PhantomEnabled>() == PhantomEnabled.On)
        {
            var level = PhantomJobLevel(Player, PhantomClass.Berserker);
            if (level >= 1
                && Player.DistanceToHitbox(primaryTarget) is < 3
                && !primaryTarget!.IsAlly)
                UseSkill(PhantomID.Rage, Player, strategy.Option(Tracks.PhantomBerserker).Priority(ActionQueue.Priority.Low + 500));
            if (level >= 2)
                UseSkill(PhantomID.DeadlyBlow, primaryTarget, strategy.Option(Tracks.PhantomBerserker).Priority(ActionQueue.Priority.High + 600));
        }
        if (strategy.Option(Tracks.PhantomGeomancerBuffs).As<PhantomEnabled>() != PhantomEnabled.Off)
        {
            var level = PhantomJobLevel(Player, PhantomClass.Geomancer);

            var target = Player;
            if (strategy.Option(Tracks.PhantomGeomancerBuffs).As<PhantomEnabled>() == PhantomEnabled.Fallback)
                target = World.Actors.Find(primaryTarget?.TargetID ?? 0) ?? Player; // if no target, use on self

            if (level >= 1)
            {
                var status = target.FindStatus(PhantomStatus.BattleBell);
                if (status == null || StatusDuration(status.Value.ExpireAt) < 30)
                {
                    UseSkill(PhantomID.BattleBell, target, strategy.Option(Tracks.PhantomGeomancerBuffs).Priority(ActionQueue.Priority.Low + 500));
                }
                else
                {
                    UseSkill(PhantomID.BattleBell, Player, strategy.Option(Tracks.PhantomGeomancerBuffs).Priority(ActionQueue.Priority.Low + 500));
                }
            }

            if (level >= 3)
            {
                var status = target.FindStatus(PhantomStatus.RingingRespite);
                if (status == null || StatusDuration(status.Value.ExpireAt) < 30)
                {
                    UseSkill(PhantomID.RingingRespite, target, strategy.Option(Tracks.PhantomGeomancerBuffs).Priority(ActionQueue.Priority.Low + 500));
                }
                else
                {
                    UseSkill(PhantomID.RingingRespite, Player, strategy.Option(Tracks.PhantomGeomancerBuffs).Priority(ActionQueue.Priority.Low + 500));
                }
            }
        }
    }

    private void UseSkill<AID>(AID action, Actor? target, float priority = 0) where AID : Enum
        => UseSkill(ActionID.MakeSpell(action), target, priority);

    private void UseSkill(ActionID action, Actor? target, float priority = 0)
    {
        Hints.ActionsToExecute.Push(action, target, priority);
    }
}
