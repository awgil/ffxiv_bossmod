namespace BossMod.Autorotation.Utility;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class RolePvPUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Elixir, Recuperate, Guard, Purify, Sprint }
    public enum ElixirStrategy { Far, Close, Forbid }
    public enum ThresholdStrategy { Seventy, Fifty, Thirty, Forbid }
    public enum GuardStrategy { Auto, Two, Three, Four, Seventy, Fifty, Thirty, Forbid }
    public enum DefensiveStrategy { Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PvP", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build(
                Class.PLD, Class.WAR, Class.DRK, Class.GNB,
                Class.WHM, Class.SCH, Class.AST, Class.SGE,
                Class.MNK, Class.DRG, Class.NIN, Class.SAM, Class.RPR, Class.VPR,
                Class.BRD, Class.MCH, Class.DNC,
                Class.BLM, Class.SMN, Class.RDM, Class.PCT), 100, 30);

        res.Define(Track.Elixir).As<ElixirStrategy>("Elixir", uiPriority: 150)
            .AddOption(ElixirStrategy.Far, "Allows use of Elixir if resources are low and no targets are nearby within 50 yalms")
            .AddOption(ElixirStrategy.Close, "Allows use of Elixir if resources are low and no targets are nearby within 30 yalms")
            .AddOption(ElixirStrategy.Forbid, "Forbid use of Elixir")
            .AddAssociatedActions(ClassShared.AID.ElixirPvP);

        res.Define(Track.Recuperate).As<ThresholdStrategy>("Recuperate", uiPriority: 150)
            .AddOption(ThresholdStrategy.Seventy, "Automatically use Recuperate when HP% is under 70%")
            .AddOption(ThresholdStrategy.Fifty, "Automatically use Recuperate when HP% is under 50%")
            .AddOption(ThresholdStrategy.Thirty, "Automatically use Recuperate when HP% is under 30%")
            .AddOption(ThresholdStrategy.Forbid, "Forbid use of Recuperate")
            .AddAssociatedActions(ClassShared.AID.RecuperatePvP);

        res.Define(Track.Guard).As<GuardStrategy>("Guard", uiPriority: 150)
            .AddOption(GuardStrategy.Auto, "Automatically use Guard when HP% is under 75% and two or more targets are targeting you, or when HP% is below 33%")
            .AddOption(GuardStrategy.Two, "Automatically use Guard when HP is not full and two or more targets are targeting you")
            .AddOption(GuardStrategy.Three, "Automatically use Guard when HP is not full and three or more targets are targeting you")
            .AddOption(GuardStrategy.Four, "Automatically use Guard when HP is not full and four or more targets are targeting you")
            .AddOption(GuardStrategy.Seventy, "Automatically use Guard when HP% is under 70%")
            .AddOption(GuardStrategy.Fifty, "Automatically use Guard when HP% is under 50%")
            .AddOption(GuardStrategy.Thirty, "Automatically use Guard when HP% is under 30%")
            .AddOption(GuardStrategy.Forbid, "Forbid use of Guard")
            .AddAssociatedActions(ClassShared.AID.GuardPvP);

        res.Define(Track.Purify).As<DefensiveStrategy>("Purify", uiPriority: 150)
            .AddOption(DefensiveStrategy.Allow, "Allow use of Purify when under any debuff that can be cleansed")
            .AddOption(DefensiveStrategy.Forbid, "Forbid use of Purify")
            .AddAssociatedActions(ClassShared.AID.PurifyPvP);

        res.Define(Track.Sprint).As<DefensiveStrategy>("Sprint", uiPriority: 150)
            .AddOption(DefensiveStrategy.Allow, "Allow use of Sprint when no target is nearby within 30 yalms")
            .AddOption(DefensiveStrategy.Forbid, "Forbid use of Sprint")
            .AddAssociatedActions(ClassShared.AID.Sprint);

        return res;
    }

    public bool IsReady(ClassShared.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining <= 0.2f;
    public bool EnemiesTargetingSelf(int numEnemies) => Service.ObjectTable.Count(o => o.IsTargetable && !o.IsDead && o.TargetObjectId == Player.InstanceID) >= numEnemies;
    public float PlayerHPP => (float)Player.HPMP.CurHP / Player.HPMP.MaxHP * 100;
    public float DebuffsLeft(Actor? target) => Utils.MaxAll(
        StatusDetails(target, ClassShared.SID.StunPvP, Player.InstanceID, 5).Left,
        StatusDetails(target, ClassShared.SID.HeavyPvP, Player.InstanceID, 5).Left,
        StatusDetails(target, ClassShared.SID.BindPvP, Player.InstanceID, 5).Left,
        StatusDetails(target, ClassShared.SID.SilencePvP, Player.InstanceID, 5).Left,
        StatusDetails(target, ClassShared.SID.DeepFreezePvP, Player.InstanceID, 5).Left,
        StatusDetails(target, WHM.SID.MiracleOfNaturePvP, Player.InstanceID, 5).Left);
    private bool TargetsNearby(float range) => Hints.PriorityTargets.Any(h =>
            !h.Actor.IsDeadOrDestroyed &&
            !h.Actor.IsFriendlyNPC &&
            !h.Actor.IsAlly &&
            h.Actor.DistanceToHitbox(Player) <= range);

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || Player.FindStatus(ClassShared.SID.GuardPvP) != null)
            return;

        if (DebuffsLeft(Player) > 0 && IsReady(ClassShared.AID.PurifyPvP) &&
            strategy.Option(Track.Purify).As<DefensiveStrategy>() == DefensiveStrategy.Allow)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.PurifyPvP), Player, (int)ActionQueue.Priority.VeryHigh);

        if (IsReady(ClassShared.AID.SprintPvP) && Player.MountId == 0 && Player.FindStatus(ClassShared.SID.SprintPvP) == null &&
            !TargetsNearby(32) && strategy.Option(Track.Sprint).As<DefensiveStrategy>() == DefensiveStrategy.Allow)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SprintPvP), Player, (int)ActionQueue.Priority.High);

        if ((Player.HPMP.CurHP != Player.HPMP.MaxHP || Player.HPMP.CurMP != Player.HPMP.MaxMP) && strategy.Option(Track.Elixir).As<ElixirStrategy>() switch
        {
            ElixirStrategy.Close => !TargetsNearby(32),
            ElixirStrategy.Far => !TargetsNearby(52),
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ElixirPvP), Player, (int)ActionQueue.Priority.High);

        if (Player.HPMP.CurMP >= 2500 && strategy.Option(Track.Recuperate).As<ThresholdStrategy>() switch
        {
            ThresholdStrategy.Seventy => PlayerHPP is < 70 and not 0,
            ThresholdStrategy.Fifty => PlayerHPP is < 50 and not 0,
            ThresholdStrategy.Thirty => PlayerHPP is < 30 and not 0,
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.RecuperatePvP), Player, (int)ActionQueue.Priority.VeryHigh);

        if (IsReady(ClassShared.AID.GuardPvP) && strategy.Option(Track.Guard).As<GuardStrategy>() switch
        {
            GuardStrategy.Auto => (PlayerHPP is < 75 and not 0 && EnemiesTargetingSelf(2)) || PlayerHPP is < 33 and not 0,
            GuardStrategy.Two => EnemiesTargetingSelf(2) && PlayerHPP is < 100 and not 0,
            GuardStrategy.Three => EnemiesTargetingSelf(3) && PlayerHPP is < 100 and not 0,
            GuardStrategy.Four => EnemiesTargetingSelf(4) && PlayerHPP is < 100 and not 0,
            GuardStrategy.Seventy => PlayerHPP is < 70 and not 0,
            GuardStrategy.Fifty => PlayerHPP is < 50 and not 0,
            GuardStrategy.Thirty => PlayerHPP is < 30 and not 0,
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.GuardPvP), Player, (int)ActionQueue.Priority.VeryHigh + 1);
    }
}
