namespace BossMod.Autorotation;

public sealed class ClassPCTUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { TemperaCoat = SharedTrack.Count }
    public enum TemperaCoatOption { None, CoatOnly, CoatGrassaASAP, CoatGrassaWhenever }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(PCT.AID.ChromaticFantasy);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PCT", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.PCT), 100);
        DefineShared(res, IDLimitBreak3);

        res.Define(Track.TemperaCoat).As<TemperaCoatOption>("Tempera Coat", "T.Coat", 600)
            .AddOption(TemperaCoatOption.None, "Do not use automatically")
            .AddOption(TemperaCoatOption.CoatOnly, "Use Tempera Coat only; ignores Tempera Grassa (if available)", 60, 10, ActionTargets.Self, 10)
            .AddOption(TemperaCoatOption.CoatGrassaASAP, "Use Tempera Coat + Tempera Grassa ASAP, regardless of casting & weaving", 90, 10, ActionTargets.Self, 88)
            .AddOption(TemperaCoatOption.CoatGrassaWhenever, "Use Tempera Coat + Tempera Grassa when weaving or not casting", 90, 10, ActionTargets.Self, 88)
            .AddAssociatedActions(PCT.AID.TemperaCoat, PCT.AID.TemperaGrassa);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var tempera = strategy.Option(Track.TemperaCoat);
        var temperaStrat = tempera.As<TemperaCoatOption>();
        if (temperaStrat != TemperaCoatOption.None)
        {
            var canCoat = ActionUnlocked(ActionID.MakeSpell(PCT.AID.TemperaCoat)) && World.Client.Cooldowns[ActionDefinitions.Instance.Spell(PCT.AID.TemperaCoat)!.MainCooldownGroup].Remaining < 0.6f;
            var hasCoat = StatusDetails(Player, PCT.SID.TemperaCoat, Player.InstanceID).Left > 0.1f;
            var canGrassa = ActionUnlocked(ActionID.MakeSpell(PCT.AID.TemperaGrassa)) && hasCoat;
            var hasGrassa = StatusDetails(Player, PCT.SID.TemperaGrassa, Player.InstanceID).Left > 0.1f;
            if (temperaStrat == TemperaCoatOption.CoatOnly)
            {
                if (canCoat && (!hasCoat || !hasGrassa))
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(PCT.AID.TemperaCoat), Player, tempera.Priority(), tempera.Value.ExpireIn);
            }
            if (temperaStrat == TemperaCoatOption.CoatGrassaASAP)
            {
                if (canCoat && !hasCoat)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(PCT.AID.TemperaCoat), Player, tempera.Priority() + 2000, tempera.Value.ExpireIn); // TODO: revise, this is bad, utility modules should not arbitrarily adjust priorities
                if (canGrassa && !hasGrassa)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(PCT.AID.TemperaGrassa), Player, tempera.Priority() + 2000, tempera.Value.ExpireIn); // TODO: revise, this is bad, utility modules should not arbitrarily adjust priorities
            }
            if (temperaStrat == TemperaCoatOption.CoatGrassaWhenever)
            {
                if (canCoat && !hasCoat)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(PCT.AID.TemperaCoat), Player, tempera.Priority(), tempera.Value.ExpireIn);
                if (canGrassa && !hasGrassa)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(PCT.AID.TemperaGrassa), Player, tempera.Priority(), tempera.Value.ExpireIn);
            }
        }
    }
}
