﻿namespace BossMod.Autorotation;

public sealed class ClassPLDUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Sheltron = SharedTrack.Count, Sentinel, Cover, Bulwark, DivineVeil, PassageOfArms, HallowedGround } //What we're tracking
    public enum ShelOption { None, Sheltron, HolySheltron, Intervention } //Sheltron Options
    public enum SentOption { None, Sentinel, Guardian } //Sentinel enhancement

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(PLD.AID.LastBastion); //LB
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(PLD.AID.IronWill); //StanceOn
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(PLD.AID.ReleaseIronWill); //StanceOff

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PLD", "Planner support for utility actions", "veyn, Akechi", RotationModuleQuality.Good, BitMask.Build((int)Class.PLD), 100);

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
        DefineSimpleConfig(res, Track.PassageOfArms, "PassageOfArms", "Arms", 470, PLD.AID.PassageOfArms, 3); //120s CD, 18s max duration
        DefineSimpleConfig(res, Track.HallowedGround, "HallowedGround", "Inv", 400, PLD.AID.HallowedGround, 10); //420s CD, 10s duration
        //DefineSimpleConfig(res, Track.Clemency, "Clemency", "Clem", 420, PLD.AID.Clemency); (TODO: we don't really care about this, do we? maybe later)

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)PLD.SID.IronWill, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Cover), PLD.AID.Cover, Player); //Cover execution
        ExecuteSimple(strategy.Option(Track.Bulwark), PLD.AID.Bulwark, Player); //Bulwark execution
        ExecuteSimple(strategy.Option(Track.DivineVeil), PLD.AID.DivineVeil, Player); //DivineVeil execution
        ExecuteSimple(strategy.Option(Track.PassageOfArms), PLD.AID.PassageOfArms, Player); //PassageOfArms execution
        ExecuteSimple(strategy.Option(Track.HallowedGround), PLD.AID.HallowedGround, Player); //HallowedGround execution
        //DefineSimpleConfig(res, Track.Clemency, "Clemency", "Clem", 420, PLD.AID.Clemency); (TODO: we don't really care about this, do we? maybe later)

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
    }
}
