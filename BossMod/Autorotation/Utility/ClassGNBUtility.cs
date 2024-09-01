namespace BossMod.Autorotation;

public sealed class ClassGNBUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Camouflage = SharedTrack.Count, Nebula, Aurora, Superbolide, HeartOfLight, HeartOfCorundum }
    public enum HoCOption { None, HeartOfStone, HeartOfCorundum }
    public enum AuroraStrategy { None, Force, Delay }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(GNB.AID.GunmetalSoul);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(GNB.AID.RoyalGuard);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(GNB.AID.ReleaseRoyalGuard);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: GNB", "Planner support for utility actions", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.GNB), 100); //How we plan our use of Utility skills
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove); //Stance & LB

        DefineSimpleConfig(res, Track.Camouflage, "Camouflage", "Camo", 450, GNB.AID.Camouflage, 20); //90s CD, 20s duration
        DefineSimpleConfig(res, Track.Nebula, "Nebula", "Nebula", 550, GNB.AID.Nebula, 15); //120s CD, 15s duration

        res.Define(Track.Aurora).As<AuroraStrategy>("Aurora", "", 550) //60s (120s total), 18s duration, 2 charges
            .AddOption(AuroraStrategy.None, "None", "Do not use automatically")
            .AddOption(AuroraStrategy.Force, "Use", "Use Aurora", 60, 18, ActionTargets.Self | ActionTargets.Party, 45)
            .AddOption(AuroraStrategy.Delay, "Don't use", "Delay Aurora")
            .AddAssociatedActions(GNB.AID.Aurora);

        DefineSimpleConfig(res, Track.Superbolide, "Superbolide", "Bolide", 400, GNB.AID.Superbolide, 10); //360s CD, 10s duration
        DefineSimpleConfig(res, Track.HeartOfLight, "HeartOfLight", "HoL", 220, GNB.AID.HeartOfLight, 15); //90s CD, 15s duration

        res.Define(Track.HeartOfCorundum).As<HoCOption>("HeartOfCorundum", "HoC", uiPriority: 350) //25s CD, 4s duration is what we really care about
            .AddOption(HoCOption.None, "None", "Do not use automatically")
            .AddOption(HoCOption.HeartOfStone, "HoS", "Use Heart of Stone", 25, 7, ActionTargets.Self | ActionTargets.Party, 68, 81)
            .AddOption(HoCOption.HeartOfCorundum, "HoC", "Use Heart of Corundum", 25, 4, ActionTargets.Self | ActionTargets.Party, 82)
            .AddAssociatedActions(GNB.AID.HeartOfStone, GNB.AID.HeartOfCorundum);

        // TODO: RoughDivide has been removed as of 7.0 DT. Trajectory is its replacement dash and no longer does damage. Consider how to add this...
        // DefineSimpleConfig(res, Track.Trajectory, "Trajectory", "Dash", 400, GNB.AID.Trajectory, 30s); //30s CD (60s total), 2 charges

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving) //Execution of Utility skills
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)GNB.SID.RoyalGuard, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Camouflage), GNB.AID.Camouflage, Player);
        ExecuteSimple(strategy.Option(Track.Nebula), GNB.AID.Nebula, Player);
        ExecuteSimple(strategy.Option(Track.Superbolide), GNB.AID.Superbolide, Player);
        ExecuteSimple(strategy.Option(Track.HeartOfLight), GNB.AID.HeartOfLight, Player);

        var aur = strategy.Option(Track.Aurora);
        var aurora = aur.As<AuroraStrategy>() switch
        {
            AuroraStrategy.Force => GNB.AID.Aurora,
            AuroraStrategy.Delay => GNB.AID.None,
            _ => default
        };
        if (aurora != default && SelfStatusLeft(GNB.SID.Aurora) <= 3f)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aurora), Player, aur.Priority(), aur.Value.ExpireIn);

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
