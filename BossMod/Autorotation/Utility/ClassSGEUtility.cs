namespace BossMod.Autorotation;

public sealed class ClassSGEUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Kardia = SharedTrack.Count, Physis, Eukrasia, Diagnosis, Prognosis, Druochole, Kerachole, Ixochole, Zoe, Pepsis, Taurochole, Haima, Rhizomata, Holos, Panhaima, Krasis, Philosophia, Icarus }
    public enum KardiaOption { None, Kardia, Soteria }
    public enum DiagOption { None, Use, UseED }
    public enum ProgOption { None, Use, UseEP, UseEPEx }
    public enum PhysisOption { None, Use, UseEx }
    public enum ZoeOption { None, Use, UseEx }
    public enum DashStrategy { None, Force, GapClose } //GapCloser strategy
    public bool InMeleeRange(Actor? target) => Player.DistanceToHitbox(target) <= 3; //Checks if we're inside melee range
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null; //Checks if Status effect is on self

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SGE.AID.TechneMakre);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SGE", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SGE), 100); //How we're planning our skills listed below
        DefineShared(res, IDLimitBreak3); //Shared Healer actions

        res.Define(Track.Kardia).As<KardiaOption>("Kardia", "", 200) //Kardia & Soteria
            .AddOption(KardiaOption.None, "None", "Do not use automatically")
            .AddOption(KardiaOption.Kardia, "Kardia", "Use Kardia", 5, 0, ActionTargets.Self | ActionTargets.Party, 4)
            .AddOption(KardiaOption.Soteria, "Soteria", "Use Soteria", 60, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(SGE.AID.Kardia, SGE.AID.Soteria);

        res.Define(Track.Physis).As<PhysisOption>("Physis", "", 200) //Physis
            .AddOption(PhysisOption.None, "None", "Do not use automatically")
            .AddOption(PhysisOption.Use, "Use", "Use Physis", 60, 15, ActionTargets.Self, 20, 59)
            .AddOption(PhysisOption.UseEx, "UseEx", "Use Physis II", 60, 15, ActionTargets.Self, 60)
            .AddAssociatedActions(SGE.AID.Physis, SGE.AID.PhysisII);

        DefineSimpleConfig(res, Track.Eukrasia, "Eukrasia", "", 110, SGE.AID.Eukrasia); //Eukrasia (spell only)

        res.Define(Track.Diagnosis).As<DiagOption>("Diagnosis", "Diag", 200) //Diagnosis & EukrasianDiagnosis
            .AddOption(DiagOption.None, "None", "Do not use automatically")
            .AddOption(DiagOption.Use, "Use", "Use Diagnosis", 0, 0, ActionTargets.Self | ActionTargets.Party, 2)
            .AddOption(DiagOption.UseED, "UseEx", "Use Eukrasian Diagnosis", 0, 30, ActionTargets.Self | ActionTargets.Party, 30)
            .AddAssociatedActions(SGE.AID.Diagnosis, SGE.AID.EukrasianDiagnosis);

        res.Define(Track.Prognosis).As<ProgOption>("Prognosis", "Prog", 200) //Prognosis & EukrasianPrognosis
            .AddOption(ProgOption.None, "None", "Do not use automatically")
            .AddOption(ProgOption.Use, "Use", "Use Prognosis", 0, 0, ActionTargets.Self, 2)
            .AddOption(ProgOption.UseEP, "UseEx", "Use Eukrasian Prognosis", 0, 30, ActionTargets.Self, 30, 95)
            .AddOption(ProgOption.UseEPEx, "UseEx", "Use Eukrasian Prognosis II", 0, 30, ActionTargets.Self, 96)
            .AddAssociatedActions(SGE.AID.Prognosis, SGE.AID.EukrasianPrognosis, SGE.AID.EukrasianPrognosisII);

        DefineSimpleConfig(res, Track.Druochole, "Druochole", "Druo", 150, SGE.AID.Druochole); //Druochole
        DefineSimpleConfig(res, Track.Kerachole, "Kerachole", "Kera", 180, SGE.AID.Kerachole, 15); //Kerachole
        DefineSimpleConfig(res, Track.Ixochole, "Ixochole", "Ixo", 190, SGE.AID.Ixochole); //Ixochole

        res.Define(Track.Zoe).As<ZoeOption>("Zoe", "", 200) //Zoe
            .AddOption(ZoeOption.None, "None", "Do not use automatically")
            .AddOption(ZoeOption.Use, "Use", "Use Zoe", 120, 30, ActionTargets.Self, 56, 87)
            .AddOption(ZoeOption.UseEx, "UseEx", "Use Enhanced Zoe", 90, 30, ActionTargets.Self, 88)
            .AddAssociatedActions(SGE.AID.Zoe);

        DefineSimpleConfig(res, Track.Pepsis, "Pepsis", "", 170, SGE.AID.Pepsis); //Pepsis
        DefineSimpleConfig(res, Track.Taurochole, "Taurochole", "Tauro", 200, SGE.AID.Taurochole, 15); //Taurchole
        DefineSimpleConfig(res, Track.Haima, "Haima", "", 100, SGE.AID.Haima, 15); //Haima
        DefineSimpleConfig(res, Track.Rhizomata, "Rhizomata", "Rhizo", 230, SGE.AID.Rhizomata); //Rhizomata
        DefineSimpleConfig(res, Track.Holos, "Holos", "", 240, SGE.AID.Holos, 20); //Holos
        DefineSimpleConfig(res, Track.Panhaima, "Panhaima", "", 250, SGE.AID.Panhaima, 15); //Panhaima
        DefineSimpleConfig(res, Track.Krasis, "Krasis", "", 210, SGE.AID.Krasis, 10); //Krasis
        DefineSimpleConfig(res, Track.Philosophia, "Philosophia", "Philo", 260, SGE.AID.Philosophia, 20); //Philosophia

        res.Define(Track.Icarus).As<DashStrategy>("Icarus", "", 20)
            .AddOption(DashStrategy.None, "None", "No use")
            .AddOption(DashStrategy.Force, "Force", "Use ASAP", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 45)
            .AddOption(DashStrategy.GapClose, "GapClose", "Use as gapcloser if outside melee range", 30, 0, ActionTargets.Party | ActionTargets.Hostile, 45)
            .AddAssociatedActions(SGE.AID.Icarus);

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //How we're executing our skills listed below
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Eukrasia), SGE.AID.Eukrasia, Player);
        ExecuteSimple(strategy.Option(Track.Druochole), SGE.AID.Druochole, primaryTarget ?? Player);
        ExecuteSimple(strategy.Option(Track.Kerachole), SGE.AID.Kerachole, Player);
        ExecuteSimple(strategy.Option(Track.Ixochole), SGE.AID.Ixochole, Player);
        ExecuteSimple(strategy.Option(Track.Pepsis), SGE.AID.Pepsis, Player);
        ExecuteSimple(strategy.Option(Track.Taurochole), SGE.AID.Taurochole, primaryTarget ?? Player);
        ExecuteSimple(strategy.Option(Track.Haima), SGE.AID.Haima, primaryTarget ?? Player);
        ExecuteSimple(strategy.Option(Track.Rhizomata), SGE.AID.Rhizomata, Player);
        ExecuteSimple(strategy.Option(Track.Holos), SGE.AID.Holos, Player);
        ExecuteSimple(strategy.Option(Track.Panhaima), SGE.AID.Panhaima, Player);
        ExecuteSimple(strategy.Option(Track.Krasis), SGE.AID.Krasis, Player);
        ExecuteSimple(strategy.Option(Track.Philosophia), SGE.AID.Philosophia, Player);

        //Kardia full execution
        var kardia = strategy.Option(Track.Kardia);
        var kardiaAction = kardia.As<KardiaOption>() switch
        {
            KardiaOption.Kardia => SGE.AID.Kardia,
            KardiaOption.Soteria => SGE.AID.Soteria,
            _ => default
        };
        if (kardiaAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(kardiaAction), primaryTarget, kardia.Priority(), kardia.Value.ExpireIn);

        //Diagnosis full execution
        var hasEukrasia = HasEffect(SGE.SID.Eukrasia); //Eukrasia
        var diag = strategy.Option(Track.Diagnosis);
        var diagAction = diag.As<DiagOption>() switch
        {
            DiagOption.Use => SGE.AID.Diagnosis,
            DiagOption.UseED => SGE.AID.EukrasianDiagnosis,
            _ => default
        };

        if (diagAction != default)
        {
            if (!hasEukrasia)
            {
                // Push the primary action based on the selected option
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(diagAction), primaryTarget, diag.Priority(), diag.Value.ExpireIn);
            }

            // Check for EukrasianDiagnosis if the effect is active
            if (hasEukrasia)
            {
                if (diag.As<DiagOption>() == DiagOption.UseED)
                {
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.EukrasianDiagnosis), primaryTarget, diag.Priority(), diag.Value.ExpireIn);
                }
            }
        }

        //Prognosis full execution
        var alreadyUp = HasEffect(SGE.SID.EukrasianPrognosis) || HasEffect(SCH.SID.Galvanize);
        var prog = strategy.Option(Track.Prognosis);
        var progAction = prog.As<ProgOption>() switch
        {
            ProgOption.Use => SGE.AID.Prognosis,
            ProgOption.UseEP => SGE.AID.Eukrasia,
            ProgOption.UseEPEx => SGE.AID.Eukrasia,
            _ => default
        };

        if (progAction != default)
        {
            if (!hasEukrasia)// Push the primary action based on the selected option
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(progAction), Player, prog.Priority(), prog.Value.ExpireIn);

            if (hasEukrasia && !alreadyUp)
            {
                // Check if UseEP is selected and push EukrasianPrognosis
                if (prog.As<ProgOption>() == ProgOption.UseEP)
                {
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.EukrasianPrognosis), Player, prog.Priority(), prog.Value.ExpireIn);
                }
                // Check if UseEPEx is selected and push EukrasianPrognosisII
                else if (prog.As<ProgOption>() == ProgOption.UseEPEx)
                {
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.EukrasianPrognosisII), Player, prog.Priority(), prog.Value.ExpireIn);
                }
            }
        }

        //Physis execution
        var physis = strategy.Option(Track.Physis);
        if (physis.As<PhysisOption>() != PhysisOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Physis), Player, physis.Priority(), physis.Value.ExpireIn);

        //Zoe execution
        var zoe = strategy.Option(Track.Zoe);
        if (zoe.As<ZoeOption>() != ZoeOption.None)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Zoe), Player, zoe.Priority(), zoe.Value.ExpireIn);

        //Icarus execution
        var dash = strategy.Option(Track.Icarus);
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var dashStrategy = strategy.Option(Track.Icarus).As<DashStrategy>();
        if (ShouldUseDash(dashStrategy, dashTarget))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Icarus), dashTarget, dash.Priority());
    }
    private bool ShouldUseDash(DashStrategy strategy, Actor? primaryTarget) => strategy switch
    {
        DashStrategy.None => false,
        DashStrategy.Force => true,
        DashStrategy.GapClose => !InMeleeRange(primaryTarget),
        _ => false,
    };
}
