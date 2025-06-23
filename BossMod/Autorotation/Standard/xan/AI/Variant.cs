namespace BossMod.Autorotation.xan;

public class VariantAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Rampart, Cure }

    public enum RampartStrategy
    {
        Combat,
        Always,
        Never
    }

    public enum CureStrategy
    {
        Enabled,
        Disabled,
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Variant AI", "Variant dungeon utilities", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 90);

        def.Define(Track.Rampart).As<RampartStrategy>("Rampart", "Variant Rampart")
            .AddOption(RampartStrategy.Combat, "Combat", "Use in combat if buff is about to expire")
            .AddOption(RampartStrategy.Always, "Always", "Use if buff is about to expire")
            .AddOption(RampartStrategy.Never, "Never", "Do not automatically use")
            .AddAssociatedActions(ClassShared.AID.VariantRampart1, ClassShared.AID.VariantRampart2);

        def.Define(Track.Cure).As<CureStrategy>("Cure", "Variant Cure")
            .AddOption(CureStrategy.Enabled, "Enabled", "Use at half HP or lower")
            .AddOption(CureStrategy.Disabled, "Disabled", "Do not use")
            .AddAssociatedActions(ClassShared.AID.VariantCure1, ClassShared.AID.VariantCure2);

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var opt = strategy.Option(Track.Rampart);
        var canUse = opt.As<RampartStrategy>() switch
        {
            RampartStrategy.Combat => Player.InCombat,
            RampartStrategy.Always => true,
            _ => false
        };

        if (canUse && TryFindAction([ClassShared.AID.VariantRampart1, ClassShared.AID.VariantRampart2], out var act))
        {
            var rampartLeft = SelfStatusLeft(ClassShared.SID.VulnerabilityDown, 60);
            if (rampartLeft < 5)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(act), Player, opt.Priority(ActionQueue.Priority.Medium));
        }

        opt = strategy.Option(Track.Cure);
        canUse = opt.As<CureStrategy>() switch
        {
            CureStrategy.Enabled => Player.HPRatio <= 0.5f,
            _ => false
        };

        if (canUse && TryFindAction([ClassShared.AID.VariantCure1, ClassShared.AID.VariantCure2], out var act2))
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(act2), Player, opt.Priority(ActionQueue.Priority.High + 500));
        }
    }

    private bool TryFindAction(ClassShared.AID[] actions, out ClassShared.AID result)
    {
        foreach (var act in actions)
            if (FindDutyActionSlot(act) >= 0)
            {
                result = act;
                return true;
            }

        result = default;
        return false;
    }
}
