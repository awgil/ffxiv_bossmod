namespace BossMod.Autorotation;

public sealed class ClassGNBUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Camouflage = SharedTrack.Count, Nebula, Aurora, Superbolide, HeartOfLight, HeartOfCorundum, Trajectory } //Our defensives and utilities
    public enum HoCOption { None, HeartOfStone, HeartOfCorundum } //Checks for proper HoC
    public enum AuroraStrategy { None, Force, ForceHold1 } //Aurora
    public enum DashStrategy { None, GapClose, GapCloseHold1 } //Gapcloser purposes

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
            .AddOption(AuroraStrategy.None, "Do not use automatically")
            .AddOption(AuroraStrategy.Force, "Use Aurora", 60, 18, ActionTargets.Self | ActionTargets.Party, 45)
            .AddOption(AuroraStrategy.ForceHold1, "Use Aurora; Holds 1 charge for manual usage", 60, 18, ActionTargets.Self | ActionTargets.Party, 82)
            .AddAssociatedActions(GNB.AID.Aurora);

        DefineSimpleConfig(res, Track.Superbolide, "Superbolide", "Bolide", 600, GNB.AID.Superbolide, 10); //360s CD, 10s duration
        DefineSimpleConfig(res, Track.HeartOfLight, "HeartOfLight", "HoL", 245, GNB.AID.HeartOfLight, 15); //90s CD, 15s duration

        res.Define(Track.HeartOfCorundum).As<HoCOption>("HeartOfCorundum", "HoC", 350) //25s CD, 4s duration is what we really care about
            .AddOption(HoCOption.None, "Do not use automatically")
            .AddOption(HoCOption.HeartOfStone, "Use Heart of Stone", 25, 7, ActionTargets.Self | ActionTargets.Party, 68, 81)
            .AddOption(HoCOption.HeartOfCorundum, "Use Heart of Corundum", 25, 4, ActionTargets.Self | ActionTargets.Party, 82)
            .AddAssociatedActions(GNB.AID.HeartOfStone, GNB.AID.HeartOfCorundum);

        res.Define(Track.Trajectory).As<DashStrategy>("Trajectory", "Dash", 20)
            .AddOption(DashStrategy.None, "No use")
            .AddOption(DashStrategy.GapClose, "Use as gapcloser if outside melee range", 30, 0, ActionTargets.Hostile, 56)
            .AddOption(DashStrategy.GapCloseHold1, "Use as gapcloser if outside melee range; conserves 1 charge for manual usage", 60, 0, ActionTargets.Hostile, 84)
            .AddAssociatedActions(GNB.AID.Trajectory);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Execution of Utility skills
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)GNB.SID.RoyalGuard, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Camouflage), GNB.AID.Camouflage, Player);
        ExecuteSimple(strategy.Option(Track.Nebula), GNB.AID.Nebula, Player);
        ExecuteSimple(strategy.Option(Track.Superbolide), GNB.AID.Superbolide, Player);
        ExecuteSimple(strategy.Option(Track.HeartOfLight), GNB.AID.HeartOfLight, Player);

        //Aurora execution
        var aurora = strategy.Option(Track.Aurora);
        var auroraTarget = ResolveTargetOverride(aurora.Value) ?? primaryTarget ?? Player; //Smart-Targeting
        var auroraStrat = aurora.As<AuroraStrategy>();
        var auroraCD = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(DRK.AID.Oblation)!.MainCooldownGroup].Remaining;
        var hasAurora = StatusDetails(auroraTarget, GNB.SID.Aurora, Player.InstanceID).Left > 0.1f; //Checks if status is present
        if (auroraStrat != AuroraStrategy.None && !hasAurora)
        {
            if ((auroraStrat == AuroraStrategy.Force) ||
                (auroraStrat == AuroraStrategy.ForceHold1) && auroraCD <= 0.6f)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(GNB.AID.Aurora), auroraTarget, aurora.Priority(), aurora.Value.ExpireIn);
        }

        //Heart of Stone / Corundum execution
        var hoc = strategy.Option(Track.HeartOfCorundum);
        var hocTarget = ResolveTargetOverride(hoc.Value) ?? CoTank() ?? primaryTarget ?? Player; //Smart-Targets Co-Tank if set to Automatic, if no Co-Tank then targets self
        var BestHOC = ActionUnlocked(ActionID.MakeSpell(GNB.AID.HeartOfCorundum)) ? GNB.AID.HeartOfCorundum : GNB.AID.HeartOfStone;
        var hocStrat = hoc.As<HoCOption>();
        if (hocStrat != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(BestHOC), hocTarget, hoc.Priority(), hoc.Value.ExpireIn);

        //Trajectory execution
        var dash = strategy.Option(Track.Trajectory);
        var dashStrategy = strategy.Option(Track.Trajectory).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var distance = Player.DistanceToHitbox(dashTarget);
        var cd = World.Client.Cooldowns[ActionDefinitions.Instance.Spell(GNB.AID.Trajectory)!.MainCooldownGroup].Remaining;
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3f and <= 20f && cd <= 30.5f,
            DashStrategy.GapCloseHold1 => distance is > 3f and <= 20f && cd < 0.6f,
            _ => false,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(GNB.AID.Trajectory), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
