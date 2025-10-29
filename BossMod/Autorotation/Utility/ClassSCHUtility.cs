namespace BossMod.Autorotation;

public sealed class ClassSCHUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { WhisperingDawn = SharedTrack.Count, Adloquium, Succor, FeyIllumination, Lustrate, SacredSoil, Indomitability, DeploymentTactics, EmergencyTactics, Dissipation, Excogitation, Aetherpact, Recitation, FeyBlessing, Consolation, Protraction, Expedient, Seraphism, Seraph }
    public enum SuccorOption { None, Succor, Concitation }
    public enum SacredSoilOption { None, Use, UseEx }
    public enum DeployOption { None, Use, UseEx }
    public enum AetherpactOption { None, Use, End }
    public enum RecitationOption { None, Use, UseEx }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SCH.AID.AngelFeathers);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SCH", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.WhisperingDawn, "Whispering Dawn", "W.Dawn", 140, SCH.AID.WhisperingDawn, 21);
        DefineSimpleConfig(res, Track.Adloquium, "Adloquium", "Adlo.", 100, SCH.AID.Adloquium, 30);

        res.Define(Track.Succor).As<SuccorOption>("Succor", "Succor", 200)
            .AddOption(SuccorOption.None, "Do not use automatically")
            .AddOption(SuccorOption.Succor, "Use Succor", 2, 30, ActionTargets.Self, 35, 95)
            .AddOption(SuccorOption.Concitation, "Use Concitation", 2, 30, ActionTargets.Self, 96)
            .AddAssociatedActions(SCH.AID.Succor, SCH.AID.Concitation);

        DefineSimpleConfig(res, Track.FeyIllumination, "Fey Illumination", "F.Illum.", 240, SCH.AID.FeyIllumination, 20);
        DefineSimpleConfig(res, Track.Lustrate, "Lustrate", "Lustrate", 150, SCH.AID.Lustrate);

        res.Define(Track.SacredSoil).As<SacredSoilOption>("Sacred Soil", "S.Soil", 200)
            .AddOption(SacredSoilOption.None, "Do not use automatically")
            .AddOption(SacredSoilOption.Use, "Use Sacred Soil", 30, 15, ActionTargets.Area, 50, 77)
            .AddOption(SacredSoilOption.UseEx, "Use Enhanced Sacred Soil", 30, 15, ActionTargets.Area, 78)
            .AddAssociatedActions(SCH.AID.SacredSoil);

        DefineSimpleConfig(res, Track.Indomitability, "Indomitability", "Indom.", 90, SCH.AID.Indomitability);

        res.Define(Track.DeploymentTactics).As<DeployOption>("DeploymentTactics", "Deploy.", 150)
            .AddOption(DeployOption.None, "Do not use automatically")
            .AddOption(DeployOption.Use, "Use Deployment Tactics", 120, 0, ActionTargets.Self, 56, 87)
            .AddOption(DeployOption.UseEx, "Use Enhanced Deployment Tactics", 90, 0, ActionTargets.Self, 88)
            .AddAssociatedActions(SCH.AID.DeploymentTactics);

        DefineSimpleConfig(res, Track.EmergencyTactics, "EmergencyTactics", "Emerg.", 100, SCH.AID.EmergencyTactics, 15);
        DefineSimpleConfig(res, Track.Dissipation, "Dissipation", "Dissi.", 290, SCH.AID.Dissipation, 30);
        DefineSimpleConfig(res, Track.Excogitation, "Excogitation", "Excog.", 100, SCH.AID.Excogitation, 45);

        res.Define(Track.Aetherpact).As<AetherpactOption>("Aetherpact", "A.pact", 300)
            .AddOption(AetherpactOption.None, "Do not use automatically")
            .AddOption(AetherpactOption.Use, "Use Aetherpact", 0, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddOption(AetherpactOption.End, "End Aetherpact", 0, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddAssociatedActions(SCH.AID.Aetherpact, SCH.AID.DissolveUnion);

        res.Define(Track.Recitation).As<RecitationOption>("Recitation", "Recit.", 130)
            .AddOption(RecitationOption.None, "Do not use automatically")
            .AddOption(RecitationOption.Use, "Use Recitation", 90, 0, ActionTargets.Self, 74, 97)
            .AddOption(RecitationOption.UseEx, "Use Enhanced Recitation", 60, 0, ActionTargets.Self, 98)
            .AddAssociatedActions(SCH.AID.Recitation);

        DefineSimpleConfig(res, Track.FeyBlessing, "FeyBlessing", "F.Blessing", 120, SCH.AID.FeyBlessing);
        DefineSimpleConfig(res, Track.Consolation, "Consolation", "Consol.", 80, SCH.AID.Consolation, 30);
        DefineSimpleConfig(res, Track.Protraction, "Protraction", "Prot.", 110, SCH.AID.Protraction, 10);
        DefineSimpleConfig(res, Track.Expedient, "Expedient", "Exped.", 200, SCH.AID.Expedient, 20);
        DefineSimpleConfig(res, Track.Seraphism, "Seraphism", "Seraphism", 300, SCH.AID.Seraphism, 20);
        DefineSimpleConfig(res, Track.Seraph, "Seraph", "Seraph", 300, SCH.AID.SummonSeraph, 20);

        return res;
    }

    // TODO: revise, this should be much simpler
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.WhisperingDawn), SCH.AID.WhisperingDawn, Player);
        ExecuteSimple(strategy.Option(Track.Adloquium), SCH.AID.Adloquium, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.FeyIllumination), SCH.AID.FeyIllumination, Player);
        ExecuteSimple(strategy.Option(Track.Indomitability), SCH.AID.Indomitability, Player);
        ExecuteSimple(strategy.Option(Track.EmergencyTactics), SCH.AID.EmergencyTactics, Player);
        ExecuteSimple(strategy.Option(Track.Dissipation), SCH.AID.Dissipation, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Excogitation), SCH.AID.Excogitation, Player);
        ExecuteSimple(strategy.Option(Track.FeyBlessing), SCH.AID.FeyBlessing, Player);
        ExecuteSimple(strategy.Option(Track.Consolation), SCH.AID.Consolation, Player);
        ExecuteSimple(strategy.Option(Track.Protraction), SCH.AID.Protraction, Player);
        ExecuteSimple(strategy.Option(Track.Expedient), SCH.AID.Expedient, Player);
        ExecuteSimple(strategy.Option(Track.Seraphism), SCH.AID.Seraphism, Player);
        ExecuteSimple(strategy.Option(Track.Seraph), SCH.AID.SummonSeraph, Player);

        var shieldUp = StatusDetails(Player, SCH.SID.Galvanize, Player.InstanceID).Left > 0.1f || StatusDetails(Player, SGE.SID.EukrasianPrognosis, Player.InstanceID).Left > 0.1f;
        var succ = strategy.Option(Track.Succor);
        var succAction = succ.As<SuccorOption>() switch
        {
            SuccorOption.Succor => SCH.AID.Succor,
            SuccorOption.Concitation => SCH.AID.Concitation,
            _ => default
        };
        if (succAction != default && !shieldUp)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(succAction), Player, succ.Priority(), succ.Value.ExpireIn, castTime: 2); // TODO[cast-time]: adjustment (swiftcast etc)

        var soil = strategy.Option(Track.SacredSoil);
        var soilAction = soil.As<SacredSoilOption>() switch
        {
            SacredSoilOption.Use or SacredSoilOption.UseEx => SCH.AID.SacredSoil,
            _ => default
        };
        if (soilAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(soilAction), null, soil.Priority(), soil.Value.ExpireIn, targetPos: ResolveTargetLocation(soil.Value).ToVec3(Player.PosRot.Y));

        var deploy = strategy.Option(Track.DeploymentTactics);
        if (deploy.As<DeployOption>() != DeployOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.DeploymentTactics), Player, deploy.Priority(), deploy.Value.ExpireIn);

        var pact = strategy.Option(Track.Aetherpact);
        var pactStrat = pact.As<AetherpactOption>();
        var pactTarget = ResolveTargetOverride(pact.Value) ?? primaryTarget ?? Player;
        var juicing = pactTarget.FindStatus(SCH.SID.FeyUnion) != null;
        if (pactStrat != AetherpactOption.None)
        {
            if (pactStrat == AetherpactOption.Use && !juicing)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Aetherpact), pactTarget, pact.Priority(), pact.Value.ExpireIn);
            if (pactStrat == AetherpactOption.End && juicing)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.DissolveUnion), pactTarget, pact.Priority(), pact.Value.ExpireIn);
        }

        var recit = strategy.Option(Track.Recitation);
        if (recit.As<RecitationOption>() != RecitationOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Recitation), Player, recit.Priority(), recit.Value.ExpireIn);
    }
}
