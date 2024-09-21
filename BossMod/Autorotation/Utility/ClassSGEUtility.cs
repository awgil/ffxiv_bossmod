namespace BossMod.Autorotation;

public sealed class ClassSGEUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Kardia = SharedTrack.Count, Egeiro, Physis, Eukrasia, Diagnosis, Prognosis, Druochole, Kerachole, Ixochole, Zoe, Pepsis, Taurochole, Haima, Rhizomata, Holos, Panhaima, Krasis, Philosophia, Icarus }
    public enum PhysisOption { None, Use, UseEx }
    public enum ZoeOption { None, Use, UseEx }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SGE.AID.TechneMakre);

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Utility: SGE", "Planner support for utility actions", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.SGE), 100);
        DefineShared(def, IDLimitBreak3);

        DefineSimpleConfig(def, Track.Kardia, "Kardia", "", 100, SGE.AID.Kardia, 1);
        DefineSimpleConfig(def, Track.Egeiro, "Egeiro", "Raise", 100, SGE.AID.Egeiro);

        def.Define(Track.Physis).As<PhysisOption>("Physis", "", 200)
            .AddOption(PhysisOption.None, "None", "Do not use automatically")
            .AddOption(PhysisOption.Use, "Use", "Use Physis", 60, 15, ActionTargets.Self, 20, 59)
            .AddOption(PhysisOption.UseEx, "UseEx", "Use Physis II", 60, 15, ActionTargets.Self, 60)
            .AddAssociatedActions(SGE.AID.Physis, SGE.AID.PhysisII);

        DefineSimpleConfig(def, Track.Eukrasia, "Eukrasia", "", 110, SGE.AID.Eukrasia, 1);
        DefineSimpleConfig(def, Track.Diagnosis, "Diagnosis", "Diag", 100, SGE.AID.Diagnosis);
        DefineSimpleConfig(def, Track.Prognosis, "Prognosis", "Prog", 100, SGE.AID.Prognosis);
        DefineSimpleConfig(def, Track.Druochole, "Druochole", "Druo", 150, SGE.AID.Druochole, 1);
        DefineSimpleConfig(def, Track.Kerachole, "Kerachole", "Kera", 180, SGE.AID.Kerachole, 15);
        DefineSimpleConfig(def, Track.Ixochole, "Ixochole", "Ixo", 190, SGE.AID.Ixochole);

        def.Define(Track.Zoe).As<ZoeOption>("Zoe", "", 200)
            .AddOption(ZoeOption.None, "None", "Do not use automatically")
            .AddOption(ZoeOption.Use, "Use", "Use Zoe", 120, 30, ActionTargets.Self, 56, 87)
            .AddOption(ZoeOption.UseEx, "UseEx", "Use Enhanced Zoe", 90, 30, ActionTargets.Self, 88)
            .AddAssociatedActions(SGE.AID.Zoe);

        DefineSimpleConfig(def, Track.Pepsis, "Pepsis", "", 170, SGE.AID.Pepsis);
        DefineSimpleConfig(def, Track.Taurochole, "Taurochole", "Tauro", 200, SGE.AID.Taurochole);
        DefineSimpleConfig(def, Track.Haima, "Haima", "", 100, SGE.AID.Haima);
        DefineSimpleConfig(def, Track.Rhizomata, "Rhizomata", "Rhizo", 230, SGE.AID.Rhizomata);
        DefineSimpleConfig(def, Track.Holos, "Holos", "", 240, SGE.AID.Holos);
        DefineSimpleConfig(def, Track.Panhaima, "Panhaima", "", 250, SGE.AID.Panhaima);
        DefineSimpleConfig(def, Track.Krasis, "Krasis", "", 210, SGE.AID.Krasis);
        DefineSimpleConfig(def, Track.Philosophia, "Philosophia", "Philo", 260, SGE.AID.Philosophia);
        DefineSimpleConfig(def, Track.Icarus, "Icarus", "", 10, SGE.AID.Icarus, 45);

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Kardia), SGE.AID.Kardia, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Egeiro), SGE.AID.Egeiro, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Eukrasia), SGE.AID.Eukrasia, Player);
        ExecuteSimple(strategy.Option(Track.Diagnosis), SGE.AID.Diagnosis, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Prognosis), SGE.AID.Prognosis, Player);
        ExecuteSimple(strategy.Option(Track.Druochole), SGE.AID.Druochole, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Kerachole), SGE.AID.Kerachole, Player);
        ExecuteSimple(strategy.Option(Track.Ixochole), SGE.AID.Ixochole, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Pepsis), SGE.AID.Pepsis, Player);
        ExecuteSimple(strategy.Option(Track.Taurochole), SGE.AID.Taurochole, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Haima), SGE.AID.Haima, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Rhizomata), SGE.AID.Rhizomata, Player);
        ExecuteSimple(strategy.Option(Track.Holos), SGE.AID.Holos, Player);
        ExecuteSimple(strategy.Option(Track.Panhaima), SGE.AID.Panhaima, Player);
        ExecuteSimple(strategy.Option(Track.Krasis), SGE.AID.Krasis, Player);
        ExecuteSimple(strategy.Option(Track.Philosophia), SGE.AID.Philosophia, Player);
        ExecuteSimple(strategy.Option(Track.Icarus), SGE.AID.Icarus, primaryTarget);

        var physis = strategy.Option(Track.Physis);
        if (physis.As<PhysisOption>() != PhysisOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Physis), Player, physis.Priority(), physis.Value.ExpireIn);

        var zoe = strategy.Option(Track.Zoe);
        if (zoe.As<ZoeOption>() != ZoeOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Zoe), Player, zoe.Priority(), zoe.Value.ExpireIn);
    }
}
