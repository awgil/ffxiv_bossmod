using static BossMod.Autorotation.ClassPLDUtility;
using static BossMod.Autorotation.ClassWARUtility;

namespace BossMod.Autorotation;

public sealed class ClassPLDUtility(RotationModuleManager manager, Actor player) : RoleTankUtility(manager, player)
{
    public enum Track { Sheltron = SharedTrack.Count, Sentinel, Cover, Bulwark, DivineVeil, PassageOfArms, HallowedGround }
    public enum ShelOption { None, Sheltron, HolySheltron, Intervention }
    public enum SentOption { None, Sentinel, Guardian }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(PLD.AID.LastBastion);
    public static readonly ActionID IDStanceApply = ActionID.MakeSpell(PLD.AID.IronWill);
    public static readonly ActionID IDStanceRemove = ActionID.MakeSpell(PLD.AID.ReleaseIronWill);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PLD", "Planner support for utility actions", "veyn, Akechi", RotationModuleQuality.WIP, BitMask.Build((int)Class.PLD), 100);

        DefineShared(res, IDLimitBreak3, IDStanceApply, IDStanceRemove);

        res.Define(Track.Sheltron).As<ShelOption>("Sheltron", "Shel", 350)
            .AddOption(ShelOption.None, "None", "Do not use automatically")
            .AddOption(ShelOption.Sheltron, "Sheltron", "Use Sheltron", 0, 4, ActionTargets.Self, 35, 81)
            .AddOption(ShelOption.HolySheltron, "HolySheltron", "Use Holy Sheltron", 0, 4, ActionTargets.Self, 82)
            .AddOption(ShelOption.Intervention, "Intervention", "Use Intervention", 0, 4, ActionTargets.Party, 62)
            .AddAssociatedActions(PLD.AID.Sheltron, PLD.AID.HolySheltron, PLD.AID.Intervention);

        res.Define(Track.Sentinel).As<SentOption>("Sentinel", "Sent", 550)
            .AddOption(SentOption.None, "None", "Do not use automatically")
            .AddOption(SentOption.Sentinel, "Use", "Use Sentinel", 120, 15, ActionTargets.Self, 38, 91)
            .AddOption(SentOption.Guardian, "UseEx", "Use Guardian", 120, 15, ActionTargets.Self, 92)
            .AddAssociatedActions(PLD.AID.Sentinel, PLD.AID.Guardian);

        DefineSimpleConfig(res, Track.Cover, "Cover", "", 320, PLD.AID.Cover, 12);
        DefineSimpleConfig(res, Track.Bulwark, "Bulwark", "Bul", 450, PLD.AID.Bulwark, 10);
        DefineSimpleConfig(res, Track.DivineVeil, "DivineVeil", "Veil", 220, PLD.AID.DivineVeil, 30);
        //DefineSimpleConfig(res, Track.Clemency, "Clemency", "Clem", 420, PLD.AID.Clemency); TODO: we don't really care, do we? maybe later
        DefineSimpleConfig(res, Track.PassageOfArms, "PassageOfArms", "Arms", 470, PLD.AID.PassageOfArms, 3);
        DefineSimpleConfig(res, Track.HallowedGround, "HallowedGround", "Inv", 400, PLD.AID.HallowedGround, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, IDStanceApply, IDStanceRemove, (uint)PLD.SID.IronWill, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Cover), PLD.AID.Cover, Player);
        ExecuteSimple(strategy.Option(Track.Bulwark), PLD.AID.Bulwark, Player);
        ExecuteSimple(strategy.Option(Track.DivineVeil), PLD.AID.DivineVeil, Player);
        //ExecuteSimple(strategy.Option(Track.Clemency), PLD.AID.Clemency, Player); TODO: we don't really care, do we? maybe later
        ExecuteSimple(strategy.Option(Track.PassageOfArms), PLD.AID.PassageOfArms, Player);
        ExecuteSimple(strategy.Option(Track.HallowedGround), PLD.AID.HallowedGround, Player);

        var shel = strategy.Option(Track.Sheltron);
        var shelAction = shel.As<ShelOption>() switch
        {
            ShelOption.HolySheltron => PLD.AID.HolySheltron,
            ShelOption.Sheltron => PLD.AID.Sheltron,
            ShelOption.Intervention => PLD.AID.Intervention,
            _ => default
        };
        if (shelAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(shelAction), shelAction == PLD.AID.Intervention ? ResolveTargetOverride(shel.Value) ?? CoTank() : Player, shel.Priority(), shel.Value.ExpireIn);

        var sent = strategy.Option(Track.Sentinel);
        var sentAction = sent.As<SentOption>() switch
        {
            SentOption.Sentinel => PLD.AID.Sentinel,
            SentOption.Guardian => PLD.AID.Guardian,
            _ => default
        };
        if (sentAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(sentAction), Player, sent.Priority(), sent.Value.ExpireIn);
    }
}
