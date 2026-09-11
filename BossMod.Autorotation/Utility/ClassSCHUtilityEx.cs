namespace BossMod.Autorotation.Utility;

public sealed class ClassSCHUtilityEx(RotationModuleManager manager, Actor player) : TypedRotationModule<ClassSCHUtilityEx.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track(Actions = [SCH.AID.Adloquium, SCH.AID.DeploymentTactics, SCH.AID.Protraction, SCH.AID.Recitation], DefaultPriority = ActionQueue.Priority.High + 300, Context = StrategyContext.Plan)]
        public Track<SpreadloStrategy> Spreadlo;

        [Track(Actions = [SCH.AID.Adloquium, SCH.AID.Succor, SCH.AID.Concitation], Context = StrategyContext.Plan)]
        public Track<ShieldStrategy> Shield;
    }

    public enum SpreadloStrategy
    {
        [Option("Don't use")]
        None,
        [Option("Use Adloquium + Deployment Tactics", Targets = ActionTargets.Self | ActionTargets.Party, Effect = 30, MinLevel = 56)]
        Basic,
        [Option("Recitation -> Spreadlo", Targets = ActionTargets.Self | ActionTargets.Party, Effect = 30, MinLevel = 74)]
        Recite,
        [Option("Protraction -> Recitation -> Spreadlo", Targets = ActionTargets.Self | ActionTargets.Party, Effect = 30, MinLevel = 86)]
        Protract
    }

    public enum ShieldStrategy
    {
        [Option("Disabled")]
        Disabled,
        [Option("Apply shield to matching party members, if missing; ensure shield lasts until plan entry end", Targets = ActionTargets.Self | ActionTargets.Party, DefaultPriority = ActionQueue.Priority.High + 500)]
        Enabled
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Utility: SCH (extra)", "Extra stuff for SCH", "Utility for planner", "xan", RotationModuleQuality.Ok, BitMask.Build(Class.SCH), 100).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Shield(strategy, ref primaryTarget);
        Spreadlo(strategy, ref primaryTarget);
    }

    void Shield(in Strategy strategy, ref Actor? primaryTarget)
    {
        if (strategy.Shield.Value == ShieldStrategy.Disabled || strategy.Shield.TrackRaw.Target == StrategyTarget.Automatic)
            return;

        // gcd shield lasts 30 seconds, if the entry is longer than that, just wait it out
        if (strategy.Shield.TrackRaw.ExpireIn > 30)
            return;

        var entryEnd = World.FutureTime(strategy.Shield.TrackRaw.ExpireIn);

        var shieldPlayers = Manager.ResolvePartyMembers(strategy.Shield.TrackRaw.Target, strategy.Shield.TrackRaw.TargetParam).Where(p => !(p.FindStatus(SCH.SID.Galvanize, World.FutureTime(30))?.ExpireAt > entryEnd)).ToList();

        // use succor to hit multiple allies
        // TODO: option to force adlo? not sure if it would ever be practical though
        if (shieldPlayers.InRadius(Player.Position, 20).Count() > 1)
        {
            var shield = ActionUnlocked(SCH.AID.Concitation) ? SCH.AID.Concitation : SCH.AID.Succor;
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(shield), Player, strategy.Shield.Priority(), castTime: 2);
            return;
        }

        foreach (var p in shieldPlayers)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Adloquium), p, strategy.Shield.Priority(), castTime: 2);
    }

    void Spreadlo(in Strategy strategy, ref Actor? primaryTarget)
    {
        var recite = false;
        var protract = false;

        if (ResolveTarget(strategy.Spreadlo) is not { } target)
        {
            if (strategy.Spreadlo.TrackRaw.Target == StrategyTarget.Automatic)
                target = Player;
            else
                return;
        }

        switch (strategy.Spreadlo.Value)
        {
            case SpreadloStrategy.None:
                return;
            case SpreadloStrategy.Recite:
                recite = true;
                break;
            case SpreadloStrategy.Protract:
                recite = protract = true;
                break;
        }

        if (recite)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Recitation), Player, ActionQueue.Priority.High);

        if (protract)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Protraction), target, ActionQueue.Priority.High);

        var canShield = true;

        if (recite)
            canShield &= Player.FindStatus(SCH.SID.Recitation, DateTime.MaxValue) != null || target.FindStatus(SCH.SID.Catalyze, DateTime.MaxValue) != null;

        if (protract)
            canShield &= target.FindStatus(SCH.SID.Protraction, DateTime.MaxValue) != null;

        if (canShield)
        {
            if (target.FindStatus(SCH.SID.Galvanize, DateTime.MaxValue) == null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Adloquium), target, strategy.Spreadlo.Priority());
            else
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.DeploymentTactics), target, ActionQueue.Priority.High);
        }
    }
}
