namespace BossMod.Autorotation;

public sealed class ClassWHMUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { PresenceOfMind = SharedTrack.Count, Regen, Cure, Medica, AfflatusSolace, AfflatusRapture, Benediction, Asylum, ThinAir, Tetragrammaton, DivineBenison, PlenaryIndulgence, Temperance, Aquaveil, LiturgyOfTheBell, DivineCaress, AetherialShift }
    public enum CureOption { None, Cure, CureII, CureIII }
    public enum MedicaOption { None, MedicaII, MedicaIII }
    public enum LiturgyOption { None, Use, End }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(WHM.AID.PulseOfLife);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: WHM", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "TrueP", RotationModuleQuality.WIP, BitMask.Build((int)Class.WHM), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.PresenceOfMind, "PresenceOfMind", "PoM", 220, WHM.AID.PresenceOfMind, 15);
        DefineSimpleConfig(res, Track.Regen, "Regen", "", 110, WHM.AID.Regen, 18);

        res.Define(Track.Cure).As<CureOption>("Cure", "", 100)
            .AddOption(CureOption.None, "Do not use automatically")
            .AddOption(CureOption.Cure, "Use Cure", 2.5f, 0, ActionTargets.Self | ActionTargets.Party | ActionTargets.Alliance | ActionTargets.Friendly, 2)
            .AddOption(CureOption.CureII, "Use Cure II", 2.5f, 0, ActionTargets.Self | ActionTargets.Party | ActionTargets.Alliance | ActionTargets.Friendly, 30)
            .AddOption(CureOption.CureIII, "Use Cure III", 2.5f, 0, ActionTargets.Self | ActionTargets.Party, 40)
            .AddAssociatedActions(WHM.AID.Cure, WHM.AID.CureII, WHM.AID.CureIII);

        res.Define(Track.Medica).As<MedicaOption>("Medica", "", 130)
            .AddOption(MedicaOption.None, "Do not use automatically")
            .AddOption(MedicaOption.MedicaII, "Use Medica II", 2.5f, 15, ActionTargets.Self, 50, 95)
            .AddOption(MedicaOption.MedicaIII, "Use Medica III", 2.5f, 15, ActionTargets.Self, 96)
            .AddAssociatedActions(WHM.AID.MedicaII, WHM.AID.MedicaIII);

        DefineSimpleConfig(res, Track.AfflatusSolace, "AfflatusSolace", "Solace", 105, WHM.AID.AfflatusSolace);
        DefineSimpleConfig(res, Track.AfflatusRapture, "AfflatusRapture", "Rapture", 125, WHM.AID.AfflatusRapture);
        DefineSimpleConfig(res, Track.Benediction, "Benediction", "Bene", 300, WHM.AID.Benediction);

        res.Define(Track.Asylum).As<SimpleOption>("Asylum", "", 230)
            .AddOption(SimpleOption.None, "Do not use automatically")
            .AddOption(SimpleOption.Use, "Use Asylum", 90, 24, ActionTargets.Area, 52)
            .AddAssociatedActions(WHM.AID.Asylum);

        DefineSimpleConfig(res, Track.ThinAir, "ThinAir", "", 200, WHM.AID.ThinAir, 10);
        DefineSimpleConfig(res, Track.Tetragrammaton, "Tetragrammaton", "Tetra", 150, WHM.AID.Tetragrammaton);
        DefineSimpleConfig(res, Track.DivineBenison, "DivineBenison", "Benison", 140, WHM.AID.DivineBenison, 15);
        DefineSimpleConfig(res, Track.PlenaryIndulgence, "PlenaryIndulgence", "Plenary", 160, WHM.AID.PlenaryIndulgence, 10);
        DefineSimpleConfig(res, Track.Temperance, "Temperance", "", 290, WHM.AID.Temperance, 20);
        DefineSimpleConfig(res, Track.Aquaveil, "Aquaveil", "", 170, WHM.AID.Aquaveil, 8);

        res.Define(Track.LiturgyOfTheBell).As<LiturgyOption>("LiturgyOfTheBell", "Liturgy", 310)
            .AddOption(LiturgyOption.None, "Do not use automatically")
            .AddOption(LiturgyOption.Use, "Use Liturgy of the Bell", 180, 20, ActionTargets.Area, 90)
            .AddOption(LiturgyOption.End, "Use Liturgy of the Bell End", 0, 1, ActionTargets.Self, 90)
            .AddAssociatedActions(WHM.AID.LiturgyOfTheBell, WHM.AID.LiturgyOfTheBellEnd);

        DefineSimpleConfig(res, Track.DivineCaress, "DivineCaress", "Caress", 280, WHM.AID.DivineCaress, 10);
        DefineSimpleConfig(res, Track.AetherialShift, "AetherialShift", "Dash", 20, WHM.AID.AetherialShift);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        var defaultHealTarget = primaryTarget ?? Player;

        ExecuteSimple(strategy.Option(Track.PresenceOfMind), WHM.AID.PresenceOfMind, Player);
        ExecuteSimple(strategy.Option(Track.AfflatusSolace), WHM.AID.AfflatusSolace, defaultHealTarget);
        ExecuteSimple(strategy.Option(Track.AfflatusRapture), WHM.AID.AfflatusRapture, Player);
        ExecuteSimple(strategy.Option(Track.Benediction), WHM.AID.Benediction, defaultHealTarget);
        ExecuteSimple(strategy.Option(Track.ThinAir), WHM.AID.ThinAir, Player);
        ExecuteSimple(strategy.Option(Track.Tetragrammaton), WHM.AID.Tetragrammaton, defaultHealTarget);
        ExecuteSimple(strategy.Option(Track.DivineBenison), WHM.AID.DivineBenison, defaultHealTarget);
        ExecuteSimple(strategy.Option(Track.PlenaryIndulgence), WHM.AID.PlenaryIndulgence, Player);
        ExecuteSimple(strategy.Option(Track.Temperance), WHM.AID.Temperance, Player);
        ExecuteSimple(strategy.Option(Track.Aquaveil), WHM.AID.Aquaveil, defaultHealTarget);
        ExecuteSimple(strategy.Option(Track.AetherialShift), WHM.AID.AetherialShift, Player);

        var cure = strategy.Option(Track.Cure);
        var cureAction = cure.As<CureOption>() switch
        {
            CureOption.Cure => WHM.AID.Cure,
            CureOption.CureII => WHM.AID.CureII,
            CureOption.CureIII => WHM.AID.CureIII,
            _ => default
        };
        if (cureAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(cureAction), ResolveTargetOverride(cure.Value) ?? defaultHealTarget, cure.Priority(), cure.Value.ExpireIn, castTime: ActionDefinitions.Instance.Spell(cureAction)!.CastTime);

        var regen = strategy.Option(Track.Regen);
        if (regen.As<SimpleOption>() == SimpleOption.Use)
        {
            var regenTarget = ResolveTargetOverride(regen.Value) ?? defaultHealTarget;
            if (regenTarget.FindStatus(WHM.SID.Regen) == null)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.Regen), regenTarget, regen.Priority(), regen.Value.ExpireIn);
        }

        var medica = strategy.Option(Track.Medica);
        var medicaAction = medica.As<MedicaOption>() switch
        {
            MedicaOption.MedicaII => WHM.AID.MedicaII,
            MedicaOption.MedicaIII => WHM.AID.MedicaIII,
            _ => default
        };
        if (medicaAction != default)
        {
            var medicaHotUp = StatusDetails(Player, WHM.SID.MedicaII, Player.InstanceID).Left > 0.1f || StatusDetails(Player, WHM.SID.MedicaIII, Player.InstanceID).Left > 0.1f;
            if (!medicaHotUp)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(medicaAction), Player, medica.Priority(), medica.Value.ExpireIn, castTime: 2);
        }

        var asylum = strategy.Option(Track.Asylum);
        if (asylum.As<SimpleOption>() == SimpleOption.Use)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.Asylum), null, asylum.Priority(), asylum.Value.ExpireIn, targetPos: ResolveTargetLocation(asylum.Value).ToVec3(Player.PosRot.Y));

        var caress = strategy.Option(Track.DivineCaress);
        if (caress.As<SimpleOption>() == SimpleOption.Use && Player.FindStatus(WHM.SID.DivineGrace) != null)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(WHM.AID.DivineCaress), Player, caress.Priority(), caress.Value.ExpireIn);

        var liturgy = strategy.Option(Track.LiturgyOfTheBell);
        var liturgyStrat = liturgy.As<LiturgyOption>();
        var liturgyAction = liturgyStrat switch
        {
            LiturgyOption.Use => WHM.AID.LiturgyOfTheBell,
            LiturgyOption.End => WHM.AID.LiturgyOfTheBellEnd,
            _ => default
        };
        var hasLiturgy = Player.FindStatus(WHM.SID.LiturgyOfTheBell) != null;
        if (liturgyAction != default && ((liturgyStrat == LiturgyOption.Use && !hasLiturgy) || (liturgyStrat == LiturgyOption.End && hasLiturgy)))
        {
            if (liturgyStrat == LiturgyOption.Use)
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(liturgyAction), null, liturgy.Priority(), liturgy.Value.ExpireIn, targetPos: ResolveTargetLocation(liturgy.Value).ToVec3(Player.PosRot.Y));
            else
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(liturgyAction), Player, liturgy.Priority(), liturgy.Value.ExpireIn);
        }
    }
}
