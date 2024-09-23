﻿namespace BossMod.Autorotation;

public sealed class ClassSGEUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Kardia = SharedTrack.Count, Egeiro, Physis, Eukrasia, Diagnosis, Prognosis, Druochole, Kerachole, Ixochole, Zoe, Pepsis, Taurochole, Haima, Rhizomata, Holos, Panhaima, Krasis, Philosophia, Icarus }
    public enum KardiaOption { None, Kardia, Soteria }
    public enum DiagOption { None, Use, UseED }
    public enum ProgOption { None, Use, UseEP, UseEPEx }
    public enum PhysisOption { None, Use, UseEx }
    public enum ZoeOption { None, Use, UseEx }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SGE.AID.TechneMakre);

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Utility: SGE", "Planner support for utility actions", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SGE), 100); //How we're planning our skills listed below
        DefineShared(def, IDLimitBreak3); //Shared Healer actions

        def.Define(Track.Kardia).As<KardiaOption>("Kardia", "", 200) //Kardia & Soteria
            .AddOption(KardiaOption.None, "None", "Do not use automatically")
            .AddOption(KardiaOption.Kardia, "Kardia", "Use Kardia", 5, 0, ActionTargets.Self | ActionTargets.Party, 4)
            .AddOption(KardiaOption.Soteria, "Soteria", "Use Soteria", 60, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(SGE.AID.Kardia, SGE.AID.Soteria);

        DefineSimpleConfig(def, Track.Egeiro, "Egeiro", "Raise", 100, SGE.AID.Egeiro); //Raise

        def.Define(Track.Physis).As<PhysisOption>("Physis", "", 200) //Physis
            .AddOption(PhysisOption.None, "None", "Do not use automatically")
            .AddOption(PhysisOption.Use, "Use", "Use Physis", 60, 15, ActionTargets.Self, 20, 59)
            .AddOption(PhysisOption.UseEx, "UseEx", "Use Physis II", 60, 15, ActionTargets.Self, 60)
            .AddAssociatedActions(SGE.AID.Physis, SGE.AID.PhysisII);

        DefineSimpleConfig(def, Track.Eukrasia, "Eukrasia", "", 110, SGE.AID.Eukrasia); //Eukrasia (spell only)

        def.Define(Track.Diagnosis).As<DiagOption>("Diagnosis", "Diag", 200) //Diagnosis & EukrasianDiagnosis
            .AddOption(DiagOption.None, "None", "Do not use automatically")
            .AddOption(DiagOption.Use, "Use", "Use Diagnosis", 0, 0, ActionTargets.Self | ActionTargets.Party, 2)
            .AddOption(DiagOption.UseED, "UseEx", "Use Eukrasian Diagnosis", 0, 30, ActionTargets.Self | ActionTargets.Party, 30)
            .AddAssociatedActions(SGE.AID.Diagnosis, SGE.AID.EukrasianDiagnosis);

        def.Define(Track.Prognosis).As<ProgOption>("Prognosis", "Prog", 200) //Prognosis & EukrasianPrognosis
            .AddOption(ProgOption.None, "None", "Do not use automatically")
            .AddOption(ProgOption.Use, "Use", "Use Prognosis", 0, 0, ActionTargets.Self, 2)
            .AddOption(ProgOption.UseEP, "UseEx", "Use Eukrasian Prognosis", 0, 30, ActionTargets.Self, 30, 95)
            .AddOption(ProgOption.UseEPEx, "UseEx", "Use Eukrasian Prognosis II", 0, 30, ActionTargets.Self, 96)
            .AddAssociatedActions(SGE.AID.Prognosis, SGE.AID.EukrasianPrognosis, SGE.AID.EukrasianPrognosisII);

        DefineSimpleConfig(def, Track.Druochole, "Druochole", "Druo", 150, SGE.AID.Druochole); //Druochole
        DefineSimpleConfig(def, Track.Kerachole, "Kerachole", "Kera", 180, SGE.AID.Kerachole, 15); //Kerachole
        DefineSimpleConfig(def, Track.Ixochole, "Ixochole", "Ixo", 190, SGE.AID.Ixochole); //Ixochole

        def.Define(Track.Zoe).As<ZoeOption>("Zoe", "", 200) //Zoe
            .AddOption(ZoeOption.None, "None", "Do not use automatically")
            .AddOption(ZoeOption.Use, "Use", "Use Zoe", 120, 30, ActionTargets.Self, 56, 87)
            .AddOption(ZoeOption.UseEx, "UseEx", "Use Enhanced Zoe", 90, 30, ActionTargets.Self, 88)
            .AddAssociatedActions(SGE.AID.Zoe);

        DefineSimpleConfig(def, Track.Pepsis, "Pepsis", "", 170, SGE.AID.Pepsis); //Pepsis
        DefineSimpleConfig(def, Track.Taurochole, "Taurochole", "Tauro", 200, SGE.AID.Taurochole, 15); //Taurchole
        DefineSimpleConfig(def, Track.Haima, "Haima", "", 100, SGE.AID.Haima, 15); //Haima
        DefineSimpleConfig(def, Track.Rhizomata, "Rhizomata", "Rhizo", 230, SGE.AID.Rhizomata); //Rhizomata
        DefineSimpleConfig(def, Track.Holos, "Holos", "", 240, SGE.AID.Holos, 20); //Holos
        DefineSimpleConfig(def, Track.Panhaima, "Panhaima", "", 250, SGE.AID.Panhaima, 15); //Panhaima
        DefineSimpleConfig(def, Track.Krasis, "Krasis", "", 210, SGE.AID.Krasis, 10); //Krasis
        DefineSimpleConfig(def, Track.Philosophia, "Philosophia", "Philo", 260, SGE.AID.Philosophia, 20); //Philosophia
        DefineSimpleConfig(def, Track.Icarus, "Icarus", "", 10, SGE.AID.Icarus, 45); //Dash

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving) //How we're executing our skills listed below
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Egeiro), SGE.AID.Egeiro, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Eukrasia), SGE.AID.Eukrasia, Player);
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

        var kardia = strategy.Option(Track.Kardia);
        var kardiaAction = kardia.As<KardiaOption>() switch
        {
            KardiaOption.Kardia => SGE.AID.Kardia,
            KardiaOption.Soteria => SGE.AID.Soteria,
            _ => default
        };
        if (kardiaAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(kardiaAction), null, kardia.Priority(), kardia.Value.ExpireIn);

        var diag = strategy.Option(Track.Diagnosis);
        var diagAction = diag.As<DiagOption>() switch
        {
            DiagOption.Use => SGE.AID.Diagnosis,
            DiagOption.UseED => SGE.AID.EukrasianDiagnosis,
            _ => default
        };
        if (diagAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(diagAction), null, diag.Priority(), diag.Value.ExpireIn);

        var prog = strategy.Option(Track.Prognosis);
        var progAction = prog.As<ProgOption>() switch
        {
            ProgOption.Use => SGE.AID.Prognosis,
            ProgOption.UseEP => SGE.AID.EukrasianPrognosis,
            ProgOption.UseEPEx => SGE.AID.EukrasianPrognosisII,
            _ => default
        };
        if (progAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(progAction), Player, prog.Priority(), prog.Value.ExpireIn);

        var physis = strategy.Option(Track.Physis);
        if (physis.As<PhysisOption>() != PhysisOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Physis), Player, physis.Priority(), physis.Value.ExpireIn);

        var zoe = strategy.Option(Track.Zoe);
        if (zoe.As<ZoeOption>() != ZoeOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Zoe), Player, zoe.Priority(), zoe.Value.ExpireIn);
    }
}
