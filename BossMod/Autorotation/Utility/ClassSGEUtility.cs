namespace BossMod.Autorotation;

public sealed class ClassSGEUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Kardia = SharedTrack.Count, Physis, Eukrasia, Diagnosis, Prognosis, Druochole, Kerachole, Ixochole, Zoe, Pepsis, Taurochole, Haima, Rhizomata, Holos, Panhaima, Krasis, Pneuma, Philosophia, Icarus }
    public enum KardiaOption { None, Kardia, Soteria }
    public enum DiagnosisOption { None, Use, UseED }
    public enum PrognosisOption { None, Use, UseEP, UseEPEx }
    public enum PhysisOption { None, Use, UseEx }
    public enum ZoeOption { None, Use, UseEx }
    public enum DashStrategy { None, GapClose }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SGE.AID.TechneMakre);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SGE", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SGE), 100); //How we're planning our skills listed below
        DefineShared(res, IDLimitBreak3); //Shared Healer actions

        res.Define(Track.Kardia).As<KardiaOption>("Kardia", "", 200) //Kardia & Soteria
            .AddOption(KardiaOption.None, "Do not use automatically")
            .AddOption(KardiaOption.Kardia, "Use Kardia", 5, 0, ActionTargets.Self | ActionTargets.Party, 4)
            .AddOption(KardiaOption.Soteria, "Use Soteria", 60, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(SGE.AID.Kardia, SGE.AID.Soteria);

        res.Define(Track.Physis).As<PhysisOption>("Physis", "", 200) //Physis
            .AddOption(PhysisOption.None, "Do not use automatically")
            .AddOption(PhysisOption.Use, "Use Physis", 60, 15, ActionTargets.Self, 20, 59)
            .AddOption(PhysisOption.UseEx, "Use Physis II", 60, 15, ActionTargets.Self, 60)
            .AddAssociatedActions(SGE.AID.Physis, SGE.AID.PhysisII);

        DefineSimpleConfig(res, Track.Eukrasia, "Eukrasia", "", 110, SGE.AID.Eukrasia); //Eukrasia (spell only)

        res.Define(Track.Diagnosis).As<DiagnosisOption>("Diagnosis", "Diag", 200) //Diagnosis & EukrasianDiagnosis
            .AddOption(DiagnosisOption.None, "Do not use automatically")
            .AddOption(DiagnosisOption.Use, "Use normal Diagnosis", 0, 0, ActionTargets.Self | ActionTargets.Party, 2)
            .AddOption(DiagnosisOption.UseED, "Use Eukrasian Diagnosis", 0, 30, ActionTargets.Self | ActionTargets.Party, 30)
            .AddAssociatedActions(SGE.AID.Diagnosis, SGE.AID.EukrasianDiagnosis);

        res.Define(Track.Prognosis).As<PrognosisOption>("Prognosis", "Prog", 200) //Prognosis & EukrasianPrognosis
            .AddOption(PrognosisOption.None, "Do not use automatically")
            .AddOption(PrognosisOption.Use, "Use normal Prognosis", 0, 0, ActionTargets.Self, 2)
            .AddOption(PrognosisOption.UseEP, "Use Eukrasian Prognosis", 0, 30, ActionTargets.Self, 30, 95)
            .AddOption(PrognosisOption.UseEPEx, "Use Eukrasian Prognosis II", 0, 30, ActionTargets.Self, 96)
            .AddAssociatedActions(SGE.AID.Prognosis, SGE.AID.EukrasianPrognosis, SGE.AID.EukrasianPrognosisII);

        DefineSimpleConfig(res, Track.Druochole, "Druochole", "Druo", 150, SGE.AID.Druochole); //Druochole
        DefineSimpleConfig(res, Track.Kerachole, "Kerachole", "Kera", 180, SGE.AID.Kerachole, 15); //Kerachole
        DefineSimpleConfig(res, Track.Ixochole, "Ixochole", "Ixo", 190, SGE.AID.Ixochole); //Ixochole

        res.Define(Track.Zoe).As<ZoeOption>("Zoe", "", 200) //Zoe
            .AddOption(ZoeOption.None, "Do not use automatically")
            .AddOption(ZoeOption.Use, "Use Zoe", 120, 30, ActionTargets.Self, 56, 87)
            .AddOption(ZoeOption.UseEx, "Use Enhanced Zoe", 90, 30, ActionTargets.Self, 88)
            .AddAssociatedActions(SGE.AID.Zoe);

        DefineSimpleConfig(res, Track.Pepsis, "Pepsis", "", 170, SGE.AID.Pepsis); //Pepsis
        DefineSimpleConfig(res, Track.Taurochole, "Taurochole", "Tauro", 200, SGE.AID.Taurochole, 15); //Taurchole
        DefineSimpleConfig(res, Track.Haima, "Haima", "", 100, SGE.AID.Haima, 15); //Haima
        DefineSimpleConfig(res, Track.Rhizomata, "Rhizomata", "Rhizo", 230, SGE.AID.Rhizomata); //Rhizomata
        DefineSimpleConfig(res, Track.Holos, "Holos", "", 240, SGE.AID.Holos, 20); //Holos
        DefineSimpleConfig(res, Track.Panhaima, "Panhaima", "", 250, SGE.AID.Panhaima, 15); //Panhaima
        DefineSimpleConfig(res, Track.Krasis, "Krasis", "", 210, SGE.AID.Krasis, 10); //Krasis
        DefineSimpleConfig(res, Track.Pneuma, "Pneuma", "", 220, SGE.AID.Pneuma); //Pneuma
        DefineSimpleConfig(res, Track.Philosophia, "Philosophia", "Philo", 260, SGE.AID.Philosophia, 20); //Philosophia

        res.Define(Track.Icarus).As<DashStrategy>("Icarus", "", 20)
            .AddOption(DashStrategy.None, "No use")
            .AddOption(DashStrategy.GapClose, "Use as gapcloser if outside melee range", 45, 0, ActionTargets.Party | ActionTargets.Hostile, 45)
            .AddAssociatedActions(SGE.AID.Icarus);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //How we're executing our skills listed below
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
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

        if (World.Client.Cooldowns[ActionDefinitions.Instance.Spell(SGE.AID.Pneuma)!.MainCooldownGroup].Remaining < 0.6f)
        {
            ExecuteSimple(strategy.Option(Track.Pneuma), SGE.AID.Pneuma, primaryTarget);
        }

        //Kardia full execution
        var kardia = strategy.Option(Track.Kardia);
        var kardiaStrat = kardia.As<KardiaOption>();
        if (kardiaStrat != KardiaOption.None)
        {
            var kardiaTarget = ResolveTargetOverride(kardia.Value) ?? primaryTarget ?? Player;
            var hasKardia = kardiaTarget.FindStatus(SGE.SID.Kardia) != null;
            if (kardiaStrat == KardiaOption.Kardia && !hasKardia)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Kardia), kardiaTarget, kardia.Priority(), kardia.Value.ExpireIn);
            if (kardiaStrat == KardiaOption.Soteria)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Soteria), Player, kardia.Priority(), kardia.Value.ExpireIn);
        }

        //Diagnosis full execution
        var hasEukrasia = Player.FindStatus(SGE.SID.Eukrasia) != null;
        var ed = strategy.Option(Track.Diagnosis);
        var edStrat = ed.As<DiagnosisOption>();
        if (edStrat != DiagnosisOption.None)
        {
            if (edStrat == DiagnosisOption.Use)
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Diagnosis), primaryTarget, ed.Priority(), ed.Value.ExpireIn, castTime: ActionDefinitions.Instance.Spell(SGE.AID.Diagnosis)!.CastTime);
            }
            if (edStrat == DiagnosisOption.UseED)
            {
                if (!hasEukrasia)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Eukrasia), Player, ed.Priority(), ed.Value.ExpireIn);
                if (hasEukrasia)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.EukrasianDiagnosis), ResolveTargetOverride(ed.Value) ?? primaryTarget ?? Player, ed.Priority(), ed.Value.ExpireIn);
            }
        }

        //Prognosis full execution
        var shieldUp = StatusDetails(Player, SCH.SID.Galvanize, Player.InstanceID).Left > 0.1f || StatusDetails(Player, SGE.SID.EukrasianPrognosis, Player.InstanceID).Left > 0.1f;
        var ep = strategy.Option(Track.Prognosis);
        var epStrat = ep.As<PrognosisOption>();
        if (epStrat != PrognosisOption.None)
        {
            if (epStrat == PrognosisOption.Use)
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Prognosis), Player, ep.Priority(), ep.Value.ExpireIn, castTime: ActionDefinitions.Instance.Spell(SGE.AID.Prognosis)!.CastTime);
            }
            if (epStrat is PrognosisOption.UseEP or PrognosisOption.UseEPEx)
            {
                if (!hasEukrasia)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Eukrasia), Player, ep.Priority(), ep.Value.ExpireIn);
                if (hasEukrasia && !shieldUp)
                    Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.EukrasianPrognosis), Player, ep.Priority(), ep.Value.ExpireIn);
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
        var dashStrategy = strategy.Option(Track.Icarus).As<DashStrategy>();
        var dashTarget = ResolveTargetOverride(dash.Value) ?? primaryTarget; //Smart-Targeting
        var distance = Player.DistanceToHitbox(dashTarget);
        var shouldDash = dashStrategy switch
        {
            DashStrategy.None => false,
            DashStrategy.GapClose => distance is > 3f and <= 20f,
            _ => false,
        };
        if (shouldDash)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SGE.AID.Icarus), dashTarget, dash.Priority(), dash.Value.ExpireIn);
    }
}
