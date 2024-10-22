namespace BossMod.Autorotation;

public sealed class ClassGNBUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Camouflage = SharedTrack.Count, Nebula, Aurora, Superbolide, HeartOfLight, HeartOfCorundum, Trajectory } //Our defensives and utilities
    public enum HoCOption { None, HeartOfStone, HeartOfCorundum } //Checks for proper HoC
    public enum AuroraStrategy { None, Force } //Aurora
    public enum DashStrategy { None, GapClose } //Gapcloser purposes
    public bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range
    public bool TargetHasEffect<SID>(Actor target, SID sid) where SID : Enum => target?.FindStatus((uint)(object)sid, Player.InstanceID) != null; //Checks if Status effect is on target

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(GNB.AID.GunmetalSoul);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(GNB.AID.RoyalGuard);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(GNB.AID.ReleaseRoyalGuard);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: GNB", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.GNB), 100); //How we plan our use of Utility skills
        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove); //Stance & LB

        DefineSimpleConfig(res, Track.Camouflage, "Camouflage", "Camo", 500, GNB.AID.Camouflage, 20); //90s CD, 20s duration
        DefineSimpleConfig(res, Track.Nebula, "Nebula", "Nebula", 550, GNB.AID.Nebula, 15); //120s CD, 15s duration

        res.Define(Track.Aurora).As<AuroraStrategy>("Aurora", "", 150) //60s (120s total), 18s duration, 2 charges
            .AddOption(AuroraStrategy.None, "None", "Do not use automatically")
            .AddOption(AuroraStrategy.Force, "Use", "Use Aurora", 60, 18, ActionTargets.Self | ActionTargets.Party, 45)
            .AddAssociatedActions(GNB.AID.Aurora);

        DefineSimpleConfig(res, Track.Superbolide, "Superbolide", "Bolide", 600, GNB.AID.Superbolide, 10); //360s CD, 10s duration
        DefineSimpleConfig(res, Track.HeartOfLight, "HeartOfLight", "HoL", 245, GNB.AID.HeartOfLight, 15); //90s CD, 15s duration

        res.Define(Track.HeartOfCorundum).As<HoCOption>("HeartOfCorundum", "HoC", 350) //25s CD, 4s duration is what we really care about
            .AddOption(HoCOption.None, "None", "Do not use automatically")
            .AddOption(HoCOption.HeartOfStone, "HoS", "Use Heart of Stone", 25, 7, ActionTargets.Self | ActionTargets.Party, 68, 81)
            .AddOption(HoCOption.HeartOfCorundum, "HoC", "Use Heart of Corundum", 25, 4, ActionTargets.Self | ActionTargets.Party, 82)
            .AddAssociatedActions(GNB.AID.HeartOfStone, GNB.AID.HeartOfCorundum);

        res.Define(Track.Trajectory).As<DashStrategy>("Trajectory", "Dash", 20)
            .AddOption(DashStrategy.None, "None", "No use")
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 30, 0, ActionTargets.Hostile, 56)
            .AddAssociatedActions(GNB.AID.Trajectory);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Execution of Utility skills
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)GNB.SID.RoyalGuard, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Camouflage), GNB.AID.Camouflage, Player);
        ExecuteSimple(strategy.Option(Track.Nebula), GNB.AID.Nebula, Player);
        ExecuteSimple(strategy.Option(Track.Superbolide), GNB.AID.Superbolide, Player);
        ExecuteSimple(strategy.Option(Track.HeartOfLight), GNB.AID.HeartOfLight, Player);

        //Aurora execution
        var aur = strategy.Option(Track.Aurora);
        var aurTarget = ResolveTargetOverride(aur.Value) ?? primaryTarget ?? Player; //Smart-Targeting
        var aurStatus = TargetHasEffect(aurTarget, GNB.SID.Aurora); //Checks if status is present
        var aurora = aur.As<AuroraStrategy>() switch
        {
            AuroraStrategy.Force => GNB.AID.Aurora,
            _ => default
        };
        if (aurora != default && !aurStatus)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(GNB.AID.Aurora), aurTarget, aur.Priority(), aur.Value.ExpireIn);

        //Heart of Corundum execution
        var hoc = strategy.Option(Track.HeartOfCorundum);
        var hocTarget = ResolveTargetOverride(hoc.Value) ?? CoTank() ?? primaryTarget ?? Player; //Smart-Targets Co-Tank if set to Automatic, if no Co-Tank then targets self
        var aid = hoc.As<HoCOption>() switch
        {
            HoCOption.HeartOfStone => GNB.AID.HeartOfStone,
            HoCOption.HeartOfCorundum => GNB.AID.HeartOfCorundum,
            _ => default
        };
        if (aid != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), hocTarget, hoc.Priority(), hoc.Value.ExpireIn);

        var dashStrategy = strategy.Option(Track.Trajectory).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, primaryTarget))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(GNB.AID.Trajectory), primaryTarget, hoc.Priority());
    }
    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.None => false,
        DashStrategy.GapClose => !InMeleeRange(primaryTarget),
        _ => false,
    };

}
