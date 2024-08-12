namespace BossMod.Autorotation;

public sealed class ClassGNBUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Camouflage = SharedTrack.Count, Nebula, Aurora, Superbolide, HeartOfLight, HeartOfCorundum }
    public enum HoCOption { None, HeartOfStone, HeartOfCorundum }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(GNB.AID.GunmetalSoul);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(GNB.AID.RoyalGuard);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(GNB.AID.ReleaseRoyalGuard);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: GNB", "Planner support for utility actions", "Akechi-kun", RotationModuleQuality.WIP, BitMask.Build((int)Class.GNB), 100);
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        DefineSimpleConfig(res, Track.Camouflage, "Camouflage", "Camoufl", 450, GNB.AID.Camouflage, 20);
        DefineSimpleConfig(res, Track.Nebula, "Nebula", "Nebula", 550, GNB.AID.Nebula, 15);
        DefineSimpleConfig(res, Track.Aurora, "Aurora", "Aurora", 320, GNB.AID.Aurora, 18);
        DefineSimpleConfig(res, Track.Superbolide, "Superbolide", "Bolide", 400, GNB.AID.Superbolide, 10);
        DefineSimpleConfig(res, Track.HeartOfLight, "HeartOfLight", "HoL", 220, GNB.AID.HeartOfLight, 15);

        res.Define(Track.HeartOfCorundum).As<HoCOption>("HeartOfCorundum", "HOC", uiPriority: 350)
            .AddOption(HoCOption.None, "None", "Do not use automatically")
            .AddOption(HoCOption.HeartOfStone, "HoS", "Use Heart of Stone", 25, 7, ActionTargets.Self | ActionTargets.Party, 68, 81) // note: secondary effect duration 30
            .AddOption(HoCOption.HeartOfCorundum, "HoC", "Use Heart of Corundum", 25, 4, ActionTargets.Self | ActionTargets.Party, 82) // note: secondary effect duration 30
            .AddAssociatedActions(GNB.AID.HeartOfStone, GNB.AID.HeartOfCorundum);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)GNB.SID.RoyalGuard, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Camouflage), GNB.AID.Camouflage, Player);
        ExecuteSimple(strategy.Option(Track.Nebula), GNB.AID.Nebula, Player);
        ExecuteSimple(strategy.Option(Track.Aurora), GNB.AID.Aurora, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Superbolide), GNB.AID.Superbolide, Player);
        ExecuteSimple(strategy.Option(Track.HeartOfLight), GNB.AID.HeartOfLight, Player);

        var hoc = strategy.Option(Track.HeartOfCorundum);
        var aid = hoc.As<HoCOption>() switch
        {
            HoCOption.HeartOfStone => GNB.AID.HeartOfStone,
            HoCOption.HeartOfCorundum => GNB.AID.HeartOfCorundum,
            _ => default
        };
        if (aid != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), ResolveTargetOverride(hoc.Value) ?? CoTank() ?? Player, hoc.Priority(), hoc.Value.ExpireIn);
    }
}
