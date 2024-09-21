namespace BossMod.Autorotation;

public sealed class ClassASTUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Helios = SharedTrack.Count, Lightspeed, BeneficII, EssentialDignity, AspectedBenefic, AspectedHelios, Synastry, CollectiveUnconscious, CelestialOpposition, CelestialIntersection, Horoscope, NeutralSect, Exaltation, Macrocosmos, SunSign, Ascend, Play }
    public enum HoroscopeOption { None, Use, End }
    public enum MacrocosmosOption { None, Use, End }
    public enum HeliosOption { None, Use, UseEx }
    public enum CardsOption { None, PlayII, PlayIII }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(AST.AID.AstralStasis);

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Utility: AST", "Planner support for utility actions", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.AST), 100);
        DefineShared(def, IDLimitBreak3);

        DefineSimpleConfig(def, Track.Helios, "Helios", "", 140, AST.AID.Helios);
        DefineSimpleConfig(def, Track.Lightspeed, "Lightspeed", "L.Speed", 140, AST.AID.Lightspeed);
        DefineSimpleConfig(def, Track.BeneficII, "BeneficII", "Bene2", 100, AST.AID.BeneficII);
        DefineSimpleConfig(def, Track.EssentialDignity, "EssentialDignity", "E.Dig", 140, AST.AID.EssentialDignity);
        DefineSimpleConfig(def, Track.AspectedBenefic, "AspectedBenefic", "A.Benefic", 100, AST.AID.AspectedBenefic, 15);

        def.Define(Track.AspectedHelios).As<HeliosOption>("AspectedHelios", "A.Helios", 130)
            .AddOption(HeliosOption.None, "None", "Do not use automatically")
            .AddOption(HeliosOption.Use, "Use", "Use Aspected Helios", 1, 15, ActionTargets.Self, 40, 95)
            .AddOption(HeliosOption.UseEx, "UseEx", "Use Helios Conjunction", 1, 15, ActionTargets.Self, 96)
            .AddAssociatedActions(AST.AID.AspectedHelios, AST.AID.HeliosConjunction);

        DefineSimpleConfig(def, Track.Synastry, "Synastry", "", 200, AST.AID.Synastry, 20);
        DefineSimpleConfig(def, Track.CollectiveUnconscious, "CollectiveUnconscious", "C.Uncon", 100, AST.AID.CollectiveUnconscious, 5); //15s regen also
        DefineSimpleConfig(def, Track.CelestialOpposition, "CelestialOpposition", "C.Oppo", 100, AST.AID.CelestialOpposition, 15);
        DefineSimpleConfig(def, Track.CelestialIntersection, "CelestialIntersection", "C.Inter", 100, AST.AID.CelestialIntersection, 30);

        def.Define(Track.Horoscope).As<HoroscopeOption>("Horoscope", "Horo", 130)
            .AddOption(HoroscopeOption.None, "None", "Do not use automatically")
            .AddOption(HoroscopeOption.Use, "Use", "Use Horoscope", 60, 10, ActionTargets.Self, 76)
            .AddOption(HoroscopeOption.End, "UseEx", "End Horoscope", 0, 1, ActionTargets.Self, 76)
            .AddAssociatedActions(AST.AID.Horoscope, AST.AID.HoroscopeEnd);

        DefineSimpleConfig(def, Track.NeutralSect, "NeutralSect", "Sect", 250, AST.AID.NeutralSect, 30);
        DefineSimpleConfig(def, Track.Exaltation, "Exaltation", "Exalt", 100, AST.AID.Exaltation, 8);

        def.Define(Track.Macrocosmos).As<MacrocosmosOption>("Macrocosmos", "Macro", 300)
            .AddOption(MacrocosmosOption.None, "None", "Do not use automatically")
            .AddOption(MacrocosmosOption.Use, "Use", "Use Macrocosmos", 120, 2, ActionTargets.Hostile, 90)
            .AddOption(MacrocosmosOption.End, "UseEx", "Use Microcosmos", 0, 1, ActionTargets.Hostile, 90)
            .AddAssociatedActions(AST.AID.Macrocosmos, AST.AID.MicrocosmosEnd);

        DefineSimpleConfig(def, Track.SunSign, "SunSign", "", 290, AST.AID.SunSign);
        DefineSimpleConfig(def, Track.Ascend, "Ascend", "Raise", 10, AST.AID.Ascend, 7);

        def.Define(Track.Play).As<CardsOption>("Play", "Play", 130)
            .AddOption(CardsOption.None, "None", "Do not use automatically")
            .AddOption(CardsOption.PlayII, "PlayII", "Use Play II's Card", 1, 15, ActionTargets.Self | ActionTargets.Party, 30)
            .AddOption(CardsOption.PlayIII, "PlayIII", "Use Play III's Card", 1, 15, ActionTargets.Self | ActionTargets.Party, 30)
            .AddAssociatedActions(AST.AID.PlayII, AST.AID.PlayIII);

        return def;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Lightspeed), AST.AID.Lightspeed, Player);
        ExecuteSimple(strategy.Option(Track.BeneficII), AST.AID.BeneficII, primaryTarget);
        ExecuteSimple(strategy.Option(Track.EssentialDignity), AST.AID.EssentialDignity, primaryTarget);
        ExecuteSimple(strategy.Option(Track.AspectedBenefic), AST.AID.AspectedBenefic, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Synastry), AST.AID.Synastry, primaryTarget);
        ExecuteSimple(strategy.Option(Track.CollectiveUnconscious), AST.AID.CollectiveUnconscious, Player);
        ExecuteSimple(strategy.Option(Track.CelestialOpposition), AST.AID.CelestialOpposition, Player);
        ExecuteSimple(strategy.Option(Track.CelestialIntersection), AST.AID.CelestialIntersection, Player);
        ExecuteSimple(strategy.Option(Track.NeutralSect), AST.AID.NeutralSect, Player);
        ExecuteSimple(strategy.Option(Track.Exaltation), AST.AID.Exaltation, primaryTarget);
        ExecuteSimple(strategy.Option(Track.SunSign), AST.AID.SunSign, Player);
        ExecuteSimple(strategy.Option(Track.Ascend), AST.AID.Ascend, primaryTarget);

        var helios = strategy.Option(Track.Macrocosmos);
        var heliosAction = helios.As<HeliosOption>() switch
        {
            HeliosOption.Use => AST.AID.AspectedHelios,
            HeliosOption.UseEx => AST.AID.HeliosConjunction,
            _ => default
        };
        if (heliosAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(heliosAction), Player, helios.Priority(), helios.Value.ExpireIn);

        var horo = strategy.Option(Track.Horoscope);
        var horoAction = horo.As<HoroscopeOption>() switch
        {
            HoroscopeOption.Use => AST.AID.Horoscope,
            HoroscopeOption.End => AST.AID.HoroscopeEnd,
            _ => default
        };
        if (horoAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(horoAction), Player, horo.Priority(), horo.Value.ExpireIn);

        var cosmos = strategy.Option(Track.Macrocosmos);
        var cosmosAction = cosmos.As<MacrocosmosOption>() switch
        {
            MacrocosmosOption.Use => AST.AID.Macrocosmos,
            MacrocosmosOption.End => AST.AID.MicrocosmosEnd,
            _ => default
        };
        if (cosmosAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(cosmosAction), Player, cosmos.Priority(), cosmos.Value.ExpireIn);

        var cards = strategy.Option(Track.Play);
        var cardsAction = cards.As<CardsOption>() switch
        {
            CardsOption.PlayII => AST.AID.PlayII,
            CardsOption.PlayIII => AST.AID.PlayIII,
            _ => default
        };
        if (cardsAction != default)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(cardsAction), Player, cards.Priority(), cards.Value.ExpireIn);
    }
}
