namespace BossMod.Autorotation;

public sealed class ClassSCHUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { WhisperingDawn = SharedTrack.Count, Adloquium, Succor, FeyIllumination, Lustrate, SacredSoil, Indomitability, DeploymentTactics, EmergencyTactics, Dissipation, Excogitation, Aetherpact, Recitation, FeyBlessing, Consolation, Protraction, Expedient, Seraphism, Resurrection, Summons }
    public enum SuccorOption { None, Succor, Concitation }
    public enum DeployOption { None, Use, UseEx }
    public enum AetherpactOption { None, Use, End }
    public enum RecitationOption { None, Use, UseEx }
    public enum PetOption { None, Eos, Seraph }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SCH.AID.AngelFeathers);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SCH", "Planner support for utility actions", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.WhisperingDawn, "WhisperingDawn", "W.Dawn", 140, SCH.AID.WhisperingDawn, 21);
        DefineSimpleConfig(res, Track.Adloquium, "Adloquium", "Adlo", 100, SCH.AID.Adloquium, 30);

        res.Define(Track.Succor).As<SuccorOption>("Succor", "", 200)
            .AddOption(SuccorOption.None, "None", "Do not use automatically")
            .AddOption(SuccorOption.Succor, "Use", "Use Succor", 2, 30, ActionTargets.Self, 35, 95)
            .AddOption(SuccorOption.Concitation, "UseEx", "Use Concitation", 2, 30, ActionTargets.Self, 96)
            .AddAssociatedActions(SCH.AID.Succor, SCH.AID.Concitation);

        DefineSimpleConfig(res, Track.FeyIllumination, "FeyIllumination", "FeyIll", 240, SCH.AID.FeyIllumination, 20);
        DefineSimpleConfig(res, Track.Lustrate, "Lustrate", "Lust", 150, SCH.AID.Lustrate);
        DefineSimpleConfig(res, Track.SacredSoil, "SacredSoil", "Soil", 180, SCH.AID.SacredSoil, 15);
        DefineSimpleConfig(res, Track.Indomitability, "Indomitability", "Indom", 90, SCH.AID.Indomitability);

        res.Define(Track.DeploymentTactics).As<DeployOption>("DeploymentTactics", "Deploy", 150)
            .AddOption(DeployOption.None, "None", "Do not use automatically")
            .AddOption(DeployOption.Use, "Use", "Use Deployment Tactics", 120, 0, ActionTargets.Self, 56, 87)
            .AddOption(DeployOption.UseEx, "UseEx", "Use Enhanced Deployment Tactics", 90, 0, ActionTargets.Self, 88)
            .AddAssociatedActions(SCH.AID.DeploymentTactics);

        DefineSimpleConfig(res, Track.EmergencyTactics, "EmergencyTactics", "Emerg", 100, SCH.AID.EmergencyTactics, 15);
        DefineSimpleConfig(res, Track.Dissipation, "Dissipation", "Dissi", 290, SCH.AID.Dissipation, 15);
        DefineSimpleConfig(res, Track.Excogitation, "Excogitation", "Excog", 100, SCH.AID.Excogitation, 45);

        res.Define(Track.Aetherpact).As<AetherpactOption>("Aetherpact", "", 300)
            .AddOption(AetherpactOption.None, "None", "Do not use automatically")
            .AddOption(AetherpactOption.Use, "Use", "Use Aetherpact", 0, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddOption(AetherpactOption.End, "UseEx", "End Aetherpact", 0, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddAssociatedActions(SCH.AID.Aetherpact, SCH.AID.DissolveUnion);

        res.Define(Track.Recitation).As<RecitationOption>("Recitation", "Recit", 130)
            .AddOption(RecitationOption.None, "None", "Do not use automatically")
            .AddOption(RecitationOption.Use, "Use", "Use Recitation", 90, 0, ActionTargets.Self, 74, 97)
            .AddOption(RecitationOption.UseEx, "UseEx", "Use Enhanced Recitation", 60, 0, ActionTargets.Self, 98)
            .AddAssociatedActions(SCH.AID.Recitation);

        DefineSimpleConfig(res, Track.FeyBlessing, "FeyBlessing", "Bless", 120, SCH.AID.FeyBlessing);
        DefineSimpleConfig(res, Track.Consolation, "Consolation", "Consol", 80, SCH.AID.Consolation, 30);
        DefineSimpleConfig(res, Track.Protraction, "Protraction", "Prot", 110, SCH.AID.Protraction, 10);
        DefineSimpleConfig(res, Track.Expedient, "Expedient", "Exped", 200, SCH.AID.Expedient, 20);
        DefineSimpleConfig(res, Track.Seraphism, "Seraphism", "", 300, SCH.AID.Seraphism, 20);
        DefineSimpleConfig(res, Track.Resurrection, "Resurrection", "Raise", 10, SCH.AID.Resurrection);

        // Pet Summons
        res.Define(Track.Summons).As<PetOption>("Pet", "", 180)
            .AddOption(PetOption.None, "None", "Do not use automatically")
            .AddOption(PetOption.Eos, "Eos", "Summon Eos", 2, 0, ActionTargets.Self, 4)
            .AddOption(PetOption.Seraph, "Seraph", "Summon Seraph", 120, 22, ActionTargets.Self, 80)
            .AddAssociatedActions(SCH.AID.SummonEos, SCH.AID.SummonSeraph);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.WhisperingDawn), SCH.AID.WhisperingDawn, Player);
        ExecuteSimple(strategy.Option(Track.Adloquium), SCH.AID.Adloquium, null);
        ExecuteSimple(strategy.Option(Track.FeyIllumination), SCH.AID.FeyIllumination, Player);
        ExecuteSimple(strategy.Option(Track.Lustrate), SCH.AID.Lustrate, null);
        ExecuteSimple(strategy.Option(Track.SacredSoil), SCH.AID.SacredSoil, Player);
        ExecuteSimple(strategy.Option(Track.Indomitability), SCH.AID.Indomitability, Player);
        ExecuteSimple(strategy.Option(Track.EmergencyTactics), SCH.AID.EmergencyTactics, Player);
        ExecuteSimple(strategy.Option(Track.Dissipation), SCH.AID.Dissipation, Player);
        ExecuteSimple(strategy.Option(Track.Excogitation), SCH.AID.Excogitation, null);
        ExecuteSimple(strategy.Option(Track.FeyBlessing), SCH.AID.FeyBlessing, Player);
        ExecuteSimple(strategy.Option(Track.Consolation), SCH.AID.Consolation, Player);
        ExecuteSimple(strategy.Option(Track.Protraction), SCH.AID.Protraction, null);
        ExecuteSimple(strategy.Option(Track.Expedient), SCH.AID.Expedient, Player);
        ExecuteSimple(strategy.Option(Track.Seraphism), SCH.AID.Seraphism, Player);
        ExecuteSimple(strategy.Option(Track.Resurrection), SCH.AID.Resurrection, null);

        var succ = strategy.Option(Track.Succor);
        var succAction = succ.As<SuccorOption>() switch
        {
            SuccorOption.Succor => SCH.AID.Succor,
            SuccorOption.Concitation => SCH.AID.Concitation,
            _ => default
        };
        if (succAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(succAction), Player, succ.Priority(), succ.Value.ExpireIn);

        var deploy = strategy.Option(Track.DeploymentTactics);
        if (deploy.As<DeployOption>() != DeployOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.DeploymentTactics), Player, deploy.Priority(), deploy.Value.ExpireIn);

        var pact = strategy.Option(Track.Aetherpact);
        var pactAction = pact.As<AetherpactOption>() switch
        {
            AetherpactOption.Use => SCH.AID.Aetherpact,
            AetherpactOption.End => SCH.AID.DissolveUnion,
            _ => default
        };
        if (pactAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(pactAction), Player, pact.Priority(), pact.Value.ExpireIn);

        var recit = strategy.Option(Track.Recitation);
        if (recit.As<RecitationOption>() != RecitationOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Recitation), Player, recit.Priority(), recit.Value.ExpireIn);

        var pet = strategy.Option(Track.Summons);
        var petSummons = pet.As<PetOption>() switch
        {
            PetOption.Eos => SCH.AID.SummonEos,
            PetOption.Seraph => SCH.AID.SummonSeraph,
            _ => default
        };
        if (petSummons != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(petSummons), Player, pet.Priority(), pet.Value.ExpireIn);

    }
}
