namespace BossMod.Autorotation.xan;

public class VariantAI(RotationModuleManager manager, Actor player) : AIBase<VariantAI.Strategy>(manager, player)
{
    public struct Strategy
    {
        [Track("Variant Rampart", Actions = [ClassShared.AID.VariantRampart1, ClassShared.AID.VariantRampart2, ClassShared.AID.VariantRampart3])]
        public Track<RampartStrategy> Rampart;
        [Track("Variant Cure", Actions = [ClassShared.AID.VariantCure1, ClassShared.AID.VariantCure2, ClassShared.AID.VariantCure3])]
        public Track<CureStrategy> Cure;
        [Track("Variant Ultimatum", Actions = [ClassShared.AID.VariantUltimatum])]
        public Track<EnabledByDefault> Ultimatum;
        [Track("Variant Spirit Dart", Actions = [ClassShared.AID.VariantSpiritDart1, ClassShared.AID.VariantSpiritDart2, ClassShared.AID.VariantSpiritDart3])]
        public Track<EnabledByDefault> SpiritDart;
        [Track("Variant Eagle Eye Shot", Actions = [ClassShared.AID.VariantEagleEyeShot])]
        public Track<EnabledByDefault> EagleEye;
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
        return new RotationModuleDefinition("Variant AI", "Variant dungeon utilities", "AI (xan)", "xan", RotationModuleQuality.Basic, new(~0ul), MaxLevel: 100).WithStrategies<Strategy>();
    }

    public override void Execute(in Strategy strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        Rampart(strategy);
        Cure(strategy);
        Ultimatum(strategy);
        SpiritDart(strategy, primaryTarget);
        EagleEye(strategy, primaryTarget);
    }

    void Rampart(in Strategy strategy)
    {
        var opt = strategy.Rampart;
        var canUse = false;

        if (opt.Value != RampartStrategy.Disabled && Player.InCombat && TryFindAction([ClassShared.AID.VariantRampart1, ClassShared.AID.VariantRampart2, ClassShared.AID.VariantRampart3], out var act))
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
    }

    void Cure(in Strategy strategy)
    {
        var opt = strategy.Cure;
        var canUse = opt.Value switch
        {
            CureStrategy.Enabled => Player.HPRatio <= 0.5f,
            _ => false
        };

        if (canUse && TryFindAction([ClassShared.AID.VariantCure1, ClassShared.AID.VariantCure2, ClassShared.AID.VariantCure3], out var act))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(act), Player, opt.Priority(ActionQueue.Priority.High + 500));
    }

    void Ultimatum(in Strategy strategy)
    {
        var opt = strategy.Ultimatum;
        var canUse = opt.IsEnabled() && SelfStatusLeft(ClassShared.SID.EnmityUp, 60) < 5;

        if (canUse && TryFindAction([ClassShared.AID.VariantUltimatum], out var act))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(act), Player, opt.Priority(ActionQueue.Priority.Low));
    }

    void SpiritDart(in Strategy strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        var opt = strategy.SpiritDart;
        if (opt.IsEnabled() && TryFindAction([ClassShared.AID.VariantSpiritDart1, ClassShared.AID.VariantSpiritDart2, ClassShared.AID.VariantSpiritDart3], out var act))
        {
            if (StatusDetails(primaryTarget, (uint)ClassShared.SID.SustainedDamage, Player.InstanceID, 30).Left <= GCD)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(act), primaryTarget, opt.Priority(ActionQueue.Priority.Low));
        }
    }

    void EagleEye(in Strategy strategy, Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        var opt = strategy.EagleEye;
        if (opt.IsEnabled() && TryFindAction([ClassShared.AID.VariantEagleEyeShot], out var act))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(act), primaryTarget, opt.Priority(ActionQueue.Priority.Low));
    }

    private bool TryFindAction(ClassShared.AID[] actions, out ClassShared.AID result)
    {
        foreach (var act in actions)
            // check that player is on an allowed class
            if (FindDutyActionSlot(act) >= 0 && ActionUnlocked(act))
            {
                result = act;
                return true;
            }

        result = default;
        return false;
    }
}
