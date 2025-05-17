namespace BossMod.Autorotation;

public sealed class ClassPLDUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Sheltron = SharedTrack.Count, Sentinel, Cover, Bulwark, DivineVeil, PassageOfArms, HallowedGround, ShieldBash } //What we're tracking
    public enum ShelOption { None, Sheltron, HolySheltron, Intervention } //Sheltron Options
    public enum SentOption { None, Sentinel, Guardian } //Sentinel enhancement
    public enum ArmsDirection { None, CharacterForward, CharacterBackward, CameraForward, CameraBackward }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(PLD.AID.LastBastion); //LB
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(PLD.AID.IronWill); //StanceOn
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(PLD.AID.ReleaseIronWill); //StanceOff

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PLD", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "veyn, Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.PLD), 100);

        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        res.Define(Track.Sheltron).As<ShelOption>("Sheltron", "Shel", 350) //Sheltron definitions
            .AddOption(ShelOption.None, "None", "Do not use automatically")
            .AddOption(ShelOption.Sheltron, "Sheltron", "Use Sheltron", 0, 4, ActionTargets.Self, 35, 81) //5s CD, 6s duration, -50 OathGauge cost
            .AddOption(ShelOption.HolySheltron, "HolySheltron", "Use Holy Sheltron", 0, 4, ActionTargets.Self, 82) //5s CD, 8s duration (similar to GNB/WAR we only really care about the first 4s of the mit), -50 OathGauge cost
            .AddOption(ShelOption.Intervention, "Intervention", "Use Intervention", 0, 4, ActionTargets.Party, 62) //10s CD, 8s duration (similar to GNB/WAR we only really care about the first 4s of the buddy mit), -50 OathGauge cost
            .AddAssociatedActions(PLD.AID.Sheltron, PLD.AID.HolySheltron, PLD.AID.Intervention);

        res.Define(Track.Sentinel).As<SentOption>("Sentinel", "Sent", 550) //Sentinel definition for CD plans
            .AddOption(SentOption.None, "None", "Do not use automatically")
            .AddOption(SentOption.Sentinel, "Use", "Use Sentinel", 120, 15, ActionTargets.Self, 38, 91) //120s CD, 15s duration
            .AddOption(SentOption.Guardian, "UseEx", "Use Guardian", 120, 15, ActionTargets.Self, 92) //120s CD, 15s duration
            .AddAssociatedActions(PLD.AID.Sentinel, PLD.AID.Guardian);

        DefineSimpleConfig(res, Track.Cover, "Cover", "", 320, PLD.AID.Cover, 12); //120s CD, 12s duration, -50 OathGauge cost
        DefineSimpleConfig(res, Track.Bulwark, "Bulwark", "Bul", 450, PLD.AID.Bulwark, 10); //90s CD, 15s duration
        DefineSimpleConfig(res, Track.DivineVeil, "DivineVeil", "Veil", 220, PLD.AID.DivineVeil, 30); //90s CD, 30s duration

        res.Define(Track.PassageOfArms).As<ArmsDirection>("PassageOfArms", "PoA", 400) //PassageOfArms definition for CD plans
            .AddOption(ArmsDirection.None, "None", "Do not use automatically")
            .AddOption(ArmsDirection.CharacterForward, "CharacterForward", "Faces the Forward direction relative to the Character", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CharacterBackward, "CharacterBackward", "Faces the Backward direction relative to the Character", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CameraForward, "CameraForward", "Faces the Forward direction relative to the Camera", 120, 18, ActionTargets.Self, 70)
            .AddOption(ArmsDirection.CameraBackward, "CameraBackward", "Faces the Backward direction relative to the Camera", 120, 18, ActionTargets.Self, 70)
            .AddAssociatedActions(PLD.AID.PassageOfArms);

        DefineSimpleConfig(res, Track.HallowedGround, "HallowedGround", "Inv", 400, PLD.AID.HallowedGround, 10); //420s CD, 10s duration

        DefineSimpleConfig(res, Track.ShieldBash, "ShieldBash", "ShieldBash", 340, PLD.AID.ShieldBash, 6);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)PLD.SID.IronWill, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Cover), PLD.AID.Cover, primaryTarget ?? Player); //Cover execution
        ExecuteSimple(strategy.Option(Track.Bulwark), PLD.AID.Bulwark, Player); //Bulwark execution
        ExecuteSimple(strategy.Option(Track.DivineVeil), PLD.AID.DivineVeil, Player); //DivineVeil execution
        ExecuteSimple(strategy.Option(Track.HallowedGround), PLD.AID.HallowedGround, Player); //HallowedGround execution

        var shel = strategy.Option(Track.Sheltron);
        var shelAction = shel.As<ShelOption>() switch
        {
            ShelOption.HolySheltron => PLD.AID.HolySheltron,
            ShelOption.Sheltron => PLD.AID.Sheltron,
            ShelOption.Intervention => PLD.AID.Intervention,
            _ => default
        };
        if (shelAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(shelAction), shelAction == PLD.AID.Intervention ? ResolveTargetOverride(shel.Value) ?? CoTank() : Player, shel.Priority(), shel.Value.ExpireIn); //Sheltron & Intervention execution

        var sent = strategy.Option(Track.Sentinel);
        var sentAction = sent.As<SentOption>() switch
        {
            SentOption.Sentinel => PLD.AID.Sentinel,
            SentOption.Guardian => PLD.AID.Guardian,
            _ => default
        };
        if (sentAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(sentAction), Player, sent.Priority(), sent.Value.ExpireIn); //Sentinel execution

        var poa = strategy.Option(Track.PassageOfArms);
        if (poa.As<ArmsDirection>() != ArmsDirection.None)
        {
            var angle = poa.As<ArmsDirection>() switch
            {
                ArmsDirection.CharacterBackward => Player.Rotation + 180.Degrees(),
                ArmsDirection.CameraForward => World.Client.CameraAzimuth + 180.Degrees(),
                ArmsDirection.CameraBackward => World.Client.CameraAzimuth,
                _ => Player.Rotation
            };
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(PLD.AID.PassageOfArms), Player, poa.Priority(), poa.Value.ExpireIn, facingAngle: angle);
        }

        var bash = strategy.Option(Track.ShieldBash);
        if (bash.As<SimpleOption>() == SimpleOption.Use)
        {
            var target = ResolveTargetOverride(bash.Value) ?? primaryTarget;
            if (target?.FindStatus(WAR.SID.Stun) == null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(PLD.AID.ShieldBash), target, ActionQueue.Priority.VeryHigh, bash.Value.ExpireIn);
        }
    }
}
