namespace BossMod.Autorotation;

public sealed class ClassSGEUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Kardia, Egeiro, Physis, Eukrasia, Druochole, Kerachole, Ixochole, Zoe, Pepsis, Taurochole, Haima, Rhizomata, Holos, Panhaima, Krasis, Philosophia, Diagnosis, Prognosis, Icarus, Count }
    //public enum xOption { None, x, x }
    public enum PhysisOption { None, Use, UseEx }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SGE.AID.TechneMakre);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SGE", "Planner support for utility actions", "Akechi", RotationModuleQuality.WIP, BitMask.Build((int)Class.SGE), 100);
        DefineShared(res, IDLimitBreak3);
        DefineSimpleConfig(res, Track.Kardia, "Kardia", "", 100, SGE.AID.Kardia);
        DefineSimpleConfig(res, Track.Egeiro, "Egeiro", "Raise", 100, SGE.AID.Egeiro);
        //DefineSimpleConfig(res, Track.Physis, "Physis", "", 100, SGE.AID.Physis, 60);

        res.Define(Track.Physis).As<PhysisOption>("Physis", "", 250)
            .AddOption(PhysisOption.None, "None", "Do not use automatically")
            .AddOption(PhysisOption.Use, "Use", "Use Physis", 60, 15, ActionTargets.Self, 22, 59)
            .AddOption(PhysisOption.UseEx, "UseEx", "Use Physis II", 60, 15, ActionTargets.Self, 60)
            .AddAssociatedActions(SGE.AID.Physis);

        DefineSimpleConfig(res, Track.Eukrasia, "Eukrasia", "", 100, SGE.AID.Eukrasia);
        DefineSimpleConfig(res, Track.Diagnosis, "Diagnosis", "Diag", 100, SGE.AID.Diagnosis);
        DefineSimpleConfig(res, Track.Prognosis, "Prognosis", "Prog", 100, SGE.AID.Prognosis);
        DefineSimpleConfig(res, Track.Druochole, "Druochole", "Druo", 100, SGE.AID.Druochole, 1);
        DefineSimpleConfig(res, Track.Kerachole, "Kerachole", "Kera", 100, SGE.AID.Kerachole, 30);
        DefineSimpleConfig(res, Track.Ixochole, "Ixochole", "Ixo", 100, SGE.AID.Ixochole, 30);
        DefineSimpleConfig(res, Track.Zoe, "Zoe", "", 100, SGE.AID.Zoe, 90);
        DefineSimpleConfig(res, Track.Pepsis, "Pepsis", "", 100, SGE.AID.Pepsis, 30);
        DefineSimpleConfig(res, Track.Taurochole, "Taurochole", "Tauro", 100, SGE.AID.Taurochole, 45);
        DefineSimpleConfig(res, Track.Haima, "Haima", "", 100, SGE.AID.Haima, 120);
        DefineSimpleConfig(res, Track.Rhizomata, "Rhizomata", "", 100, SGE.AID.Rhizomata, 90);
        DefineSimpleConfig(res, Track.Holos, "Holos", "", 100, SGE.AID.Holos, 120);
        DefineSimpleConfig(res, Track.Panhaima, "Panhaima", "", 100, SGE.AID.Panhaima, 120);
        DefineSimpleConfig(res, Track.Krasis, "Krasis", "", 100, SGE.AID.Krasis, 60);
        DefineSimpleConfig(res, Track.Philosophia, "Philosophia", "", 100, SGE.AID.Philosophia, 180);
        DefineSimpleConfig(res, Track.Icarus, "Icarus", "", 100, SGE.AID.Icarus, 45);
        // DefineSimpleConfig(res, Track.x, "x", "", 100, SGE.AID.x, x);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? target, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Kardia), SGE.AID.Kardia, target);
        ExecuteSimple(strategy.Option(Track.Egeiro), SGE.AID.Egeiro, target);
        ExecuteSimple(strategy.Option(Track.Physis), SGE.AID.Physis, Player);
        ExecuteSimple(strategy.Option(Track.Eukrasia), SGE.AID.Eukrasia, Player);
        ExecuteSimple(strategy.Option(Track.Diagnosis), SGE.AID.Diagnosis, target);
        ExecuteSimple(strategy.Option(Track.Prognosis), SGE.AID.Prognosis, Player);
        ExecuteSimple(strategy.Option(Track.Druochole), SGE.AID.Druochole, target);
        ExecuteSimple(strategy.Option(Track.Kerachole), SGE.AID.Kerachole, Player);
        ExecuteSimple(strategy.Option(Track.Ixochole), SGE.AID.Ixochole, target);
        ExecuteSimple(strategy.Option(Track.Zoe), SGE.AID.Zoe, Player);
        ExecuteSimple(strategy.Option(Track.Pepsis), SGE.AID.Pepsis, Player);
        ExecuteSimple(strategy.Option(Track.Taurochole), SGE.AID.Taurochole, target);
        ExecuteSimple(strategy.Option(Track.Haima), SGE.AID.Haima, target);
        ExecuteSimple(strategy.Option(Track.Rhizomata), SGE.AID.Rhizomata, Player);
        ExecuteSimple(strategy.Option(Track.Holos), SGE.AID.Holos, Player);
        ExecuteSimple(strategy.Option(Track.Panhaima), SGE.AID.Panhaima, Player);
        ExecuteSimple(strategy.Option(Track.Krasis), SGE.AID.Krasis, Player);
        ExecuteSimple(strategy.Option(Track.Philosophia), SGE.AID.Philosophia, Player);
        ExecuteSimple(strategy.Option(Track.Icarus), SGE.AID.Icarus, target);
        //ExecuteSimple(strategy.Option(Track.x), SGE.AID.x, Player);

        var physis = strategy.Option(Track.Physis);
        if (physis.As<PhysisOption>() != PhysisOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Physis), target, physis.Priority(), physis.Value.ExpireIn);
    }
}
