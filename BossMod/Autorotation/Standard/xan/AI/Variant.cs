namespace BossMod.Autorotation.xan;

public class VariantAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Rampart, Cure }

    public enum RampartStrategy
    {
        PermaShield,
        PermaBuff,
        Disabled
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
            .AddOption(RampartStrategy.PermaShield, "Use on cooldown (for shield)")
            .AddOption(RampartStrategy.PermaBuff, "Use if buff is about to expire")
            .AddOption(RampartStrategy.Disabled, "Do not automatically use")
            .AddAssociatedActions(ClassShared.AID.VariantRampart1, ClassShared.AID.VariantRampart2);

        def.Define(Track.Cure).As<CureStrategy>("Cure", "Variant Cure")
            .AddOption(CureStrategy.Enabled, "Use at half HP or lower")
            .AddOption(CureStrategy.Disabled, "Do not use")
            .AddAssociatedActions(ClassShared.AID.VariantCure1, ClassShared.AID.VariantCure2);

        return def;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var opt = strategy.Option(Track.Rampart);
        var canUse = false;

        if (opt.As<RampartStrategy>() != RampartStrategy.Disabled && Player.InCombat && TryFindAction([ClassShared.AID.VariantRampart1, ClassShared.AID.VariantRampart2], out var act))
        {
            switch (opt.As<RampartStrategy>())
            {
                case RampartStrategy.PermaShield:
                    canUse = true;
                    break;
                case RampartStrategy.PermaBuff:
                    canUse = SelfStatusLeft(ClassShared.SID.VulnerabilityDown, 60) < 5;
                    break;
            }

            if (canUse)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(act), Player, opt.Priority(ActionQueue.Priority.Low));
        }

        opt = strategy.Option(Track.Cure);
        canUse = opt.As<CureStrategy>() switch
        {
            CureStrategy.Enabled => Player.HPRatio <= 0.5f,
            _ => false
        };

        if (canUse && TryFindAction([ClassShared.AID.VariantCure1, ClassShared.AID.VariantCure2], out var act2))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(act2), Player, opt.Priority(ActionQueue.Priority.High + 500));
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
