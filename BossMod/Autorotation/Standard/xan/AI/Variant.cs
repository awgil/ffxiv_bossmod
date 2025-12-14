namespace BossMod.Autorotation.xan;

public class VariantAI(RotationModuleManager manager, Actor player) : AIBase<VariantAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Variant Rampart", Actions = [ClassShared.AID.VariantRampart1, ClassShared.AID.VariantRampart2])]
        public Track<RampartStrategy> Rampart;
        [Track("Variant Cure", Actions = [ClassShared.AID.VariantCure1, ClassShared.AID.VariantCure2])]
        public Track<CureStrategy> Cure;
    }

    public enum RampartStrategy
    {
        [Option("Use on cooldown (for shield)")]
        PermaShield,
        [Option("Use if buff is about to expire")]
        PermaBuff,
        [Option("Do not automatically use")]
        Disabled
    }

    public enum CureStrategy
    {
        [Option("Use at half HP or lower")]
        Enabled,
        [Option("Don't use")]
        Disabled,
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("Variant AI", "Variant dungeon utilities", "AI (xan)", "xan", RotationModuleQuality.WIP, new(~0ul), MaxLevel: 90).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var opt = strategy.Rampart;
        var canUse = false;

        if (opt.Value != RampartStrategy.Disabled && Player.InCombat && TryFindAction([ClassShared.AID.VariantRampart1, ClassShared.AID.VariantRampart2], out var act))
        {
            switch (opt.Value)
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

        var opt2 = strategy.Cure;
        canUse = opt2.Value switch
        {
            CureStrategy.Enabled => Player.HPRatio <= 0.5f,
            _ => false
        };

        if (canUse && TryFindAction([ClassShared.AID.VariantCure1, ClassShared.AID.VariantCure2], out var act2))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(act2), Player, opt2.Priority(ActionQueue.Priority.High + 500));
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
