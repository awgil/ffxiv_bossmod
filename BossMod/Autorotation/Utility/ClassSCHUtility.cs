namespace BossMod.Autorotation;

public sealed class ClassSCHUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { WhisperingDawn = SharedTrack.Count, Adloquium, Succor, FeyIllumination, Lustrate, SacredSoil, Indomitability, DeploymentTactics, EmergencyTactics, Dissipation, Excogitation, Aetherpact, Recitation, FeyBlessing, Consolation, Protraction, Expedient, Seraphism, Summons }
    public enum SuccorOption { None, Succor, Concitation }
    public enum SacredSoilOption { None, Use, UseEx }
    public enum DeployOption { None, Use, UseEx }
    public enum AetherpactOption { None, Use, End }
    public enum RecitationOption { None, Use, UseEx }
    public enum PetOption { None, Eos, Seraph }
    public float GetStatusDetail(Actor target, SGE.SID sid) => StatusDetails(target, sid, Player.InstanceID).Left; //Checks if Status effect is on target
    public bool HasEffect(Actor target, SGE.SID sid, float duration) => GetStatusDetail(target, sid) < duration; //Checks if anyone has a status effect
    public float GetStatusDetail(Actor target, SCH.SID sid) => StatusDetails(target, sid, Player.InstanceID).Left; //Checks if Status effect is on target
    public bool HasEffect(Actor target, SCH.SID sid, float duration) => GetStatusDetail(target, sid) < duration; //Checks if anyone has a status effect
    public Actor? TargetChoice(StrategyValues.OptionRef strategy) => ResolveTargetOverride(strategy.Value);

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SCH.AID.AngelFeathers);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SCH", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.WhisperingDawn, "Whispering Dawn", "W.Dawn", 140, SCH.AID.WhisperingDawn, 21);
        DefineSimpleConfig(res, Track.Adloquium, "Adloquium", "Adlo.", 100, SCH.AID.Adloquium, 30);

        res.Define(Track.Succor).As<SuccorOption>("Succor", "Succor", 200)
            .AddOption(SuccorOption.None, "None", "Do not use automatically")
            .AddOption(SuccorOption.Succor, "Use", "Use Succor", 2, 30, ActionTargets.Self, 35, 95)
            .AddOption(SuccorOption.Concitation, "UseEx", "Use Concitation", 2, 30, ActionTargets.Self, 96)
            .AddAssociatedActions(SCH.AID.Succor, SCH.AID.Concitation);

        DefineSimpleConfig(res, Track.FeyIllumination, "Fey Illumination", "F.Illum.", 240, SCH.AID.FeyIllumination, 20);
        DefineSimpleConfig(res, Track.Lustrate, "Lustrate", "Lustrate", 150, SCH.AID.Lustrate);

        res.Define(Track.SacredSoil).As<SacredSoilOption>("Sacred Soil", "S.Soil", 200)
            .AddOption(SacredSoilOption.None, "None", "Do not use automatically")
            .AddOption(SacredSoilOption.Use, "Use", "Use Sacred Soil", 30, 15, ActionTargets.All, 50, 77)
            .AddOption(SacredSoilOption.UseEx, "UseEx", "Use Enhanced Sacred Soil", 30, 15, ActionTargets.All, 78)
            .AddAssociatedActions(SCH.AID.SacredSoil);

        DefineSimpleConfig(res, Track.Indomitability, "Indomitability", "Indom.", 90, SCH.AID.Indomitability);

        res.Define(Track.DeploymentTactics).As<DeployOption>("DeploymentTactics", "Deploy.", 150)
            .AddOption(DeployOption.None, "None", "Do not use automatically")
            .AddOption(DeployOption.Use, "Use", "Use Deployment Tactics", 120, 0, ActionTargets.Self, 56, 87)
            .AddOption(DeployOption.UseEx, "UseEx", "Use Enhanced Deployment Tactics", 90, 0, ActionTargets.Self, 88)
            .AddAssociatedActions(SCH.AID.DeploymentTactics);

        DefineSimpleConfig(res, Track.EmergencyTactics, "EmergencyTactics", "Emerg.", 100, SCH.AID.EmergencyTactics, 15);
        DefineSimpleConfig(res, Track.Dissipation, "Dissipation", "Dissi.", 290, SCH.AID.Dissipation, 15);
        DefineSimpleConfig(res, Track.Excogitation, "Excogitation", "Excog.", 100, SCH.AID.Excogitation, 45);

        res.Define(Track.Aetherpact).As<AetherpactOption>("Aetherpact", "A.pact", 300)
            .AddOption(AetherpactOption.None, "None", "Do not use automatically")
            .AddOption(AetherpactOption.Use, "Use", "Use Aetherpact", 0, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddOption(AetherpactOption.End, "UseEx", "End Aetherpact", 0, 0, ActionTargets.Self | ActionTargets.Party, 70)
            .AddAssociatedActions(SCH.AID.Aetherpact, SCH.AID.DissolveUnion);

        res.Define(Track.Recitation).As<RecitationOption>("Recitation", "Recit.", 130)
            .AddOption(RecitationOption.None, "None", "Do not use automatically")
            .AddOption(RecitationOption.Use, "Use", "Use Recitation", 90, 0, ActionTargets.Self, 74, 97)
            .AddOption(RecitationOption.UseEx, "UseEx", "Use Enhanced Recitation", 60, 0, ActionTargets.Self, 98)
            .AddAssociatedActions(SCH.AID.Recitation);

        DefineSimpleConfig(res, Track.FeyBlessing, "FeyBlessing", "F.Blessing", 120, SCH.AID.FeyBlessing);
        DefineSimpleConfig(res, Track.Consolation, "Consolation", "Consol.", 80, SCH.AID.Consolation, 30);
        DefineSimpleConfig(res, Track.Protraction, "Protraction", "Prot.", 110, SCH.AID.Protraction, 10);
        DefineSimpleConfig(res, Track.Expedient, "Expedient", "Exped.", 200, SCH.AID.Expedient, 20);
        DefineSimpleConfig(res, Track.Seraphism, "Seraphism", "Seraphism", 300, SCH.AID.Seraphism, 20);

        // Pet Summons
        res.Define(Track.Summons).As<PetOption>("Pet", "", 180)
            .AddOption(PetOption.None, "None", "Do not use automatically")
            .AddOption(PetOption.Eos, "Eos", "Summon Eos", 2, 0, ActionTargets.Self, 4)
            .AddOption(PetOption.Seraph, "Seraph", "Summon Seraph", 120, 22, ActionTargets.Self, 80)
            .AddAssociatedActions(SCH.AID.SummonEos, SCH.AID.SummonSeraph);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.WhisperingDawn), SCH.AID.WhisperingDawn, Player);
        ExecuteSimple(strategy.Option(Track.Adloquium), SCH.AID.Adloquium, TargetChoice(strategy.Option(Track.Adloquium)) ?? Player);
        ExecuteSimple(strategy.Option(Track.FeyIllumination), SCH.AID.FeyIllumination, Player);
        ExecuteSimple(strategy.Option(Track.Indomitability), SCH.AID.Indomitability, Player);
        ExecuteSimple(strategy.Option(Track.EmergencyTactics), SCH.AID.EmergencyTactics, Player);
        ExecuteSimple(strategy.Option(Track.Dissipation), SCH.AID.Dissipation, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Excogitation), SCH.AID.Excogitation, TargetChoice(strategy.Option(Track.Excogitation)) ?? Player);
        ExecuteSimple(strategy.Option(Track.FeyBlessing), SCH.AID.FeyBlessing, Player);
        ExecuteSimple(strategy.Option(Track.Consolation), SCH.AID.Consolation, Player);
        ExecuteSimple(strategy.Option(Track.Protraction), SCH.AID.Protraction, TargetChoice(strategy.Option(Track.Protraction)) ?? Player);
        ExecuteSimple(strategy.Option(Track.Expedient), SCH.AID.Expedient, Player);
        ExecuteSimple(strategy.Option(Track.Seraphism), SCH.AID.Seraphism, Player);

        var alreadyUp = HasEffect(Player, SGE.SID.EukrasianPrognosis, 30) || HasEffect(Player, SCH.SID.Galvanize, 30);
        var succ = strategy.Option(Track.Succor);
        var succAction = succ.As<SuccorOption>() switch
        {
            SuccorOption.Succor => SCH.AID.Succor,
            SuccorOption.Concitation => SCH.AID.Concitation,
            _ => default
        };
        if (succAction != default && !alreadyUp)
            QueueOGCD(succAction, Player);

        var soil = strategy.Option(Track.SacredSoil);
        var soilAction = soil.As<SacredSoilOption>() switch
        {
            SacredSoilOption.Use or SacredSoilOption.UseEx => SCH.AID.SacredSoil,
            _ => default
        };
        if (soilAction != default)
            QueueOGCD(soilAction, TargetChoice(soil) ?? primaryTarget ?? Player);

        var deploy = strategy.Option(Track.DeploymentTactics);
        if (deploy.As<DeployOption>() != DeployOption.None)
            QueueOGCD(SCH.AID.DeploymentTactics, Player);

        var pact = strategy.Option(Track.Aetherpact);
        var pactAction = pact.As<AetherpactOption>() switch
        {
            AetherpactOption.Use => SCH.AID.Aetherpact,
            AetherpactOption.End => SCH.AID.DissolveUnion,
            _ => default
        };
        if (pactAction != default)
            QueueOGCD(pactAction, Player);

        var recit = strategy.Option(Track.Recitation);
        if (recit.As<RecitationOption>() != RecitationOption.None)
            QueueOGCD(SCH.AID.Recitation, Player);

        var pet = strategy.Option(Track.Summons);
        var petSummons = pet.As<PetOption>() switch
        {
            PetOption.Eos => SCH.AID.SummonEos,
            PetOption.Seraph => SCH.AID.SummonSeraph,
            _ => default
        };
        if (petSummons != default)
            QueueOGCD(petSummons, Player);
    }

    #region Core Execution Helpers

    public SCH.AID NextGCD; //Next global cooldown action to be used
    public void QueueGCD<P>(SCH.AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueGCD(aid, target, (int)(object)priority, delay);

    public void QueueGCD(SCH.AID aid, Actor? target, int priority = 8, float delay = 0)
    {
        var NextGCDPrio = 0;

        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.High, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
        }
    }

    public void QueueOGCD<P>(SCH.AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueOGCD(aid, target, (int)(object)priority, delay);

    public void QueueOGCD(SCH.AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }

    public bool QueueAction(SCH.AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return false;

        if (def.Range != 0 && target == null)
        {
            return false;
        }

        Vector3 targetPos = default;

        if (def.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (def.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, targetPos: targetPos);
        return true;
    }
    #endregion

}
