namespace BossMod.Autorotation;

public sealed class ClassASTUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Helios = SharedTrack.Count, Lightspeed, BeneficII, EssentialDignity, AspectedBenefic, AspectedHelios, Synastry, CollectiveUnconscious, CelestialOpposition, CelestialIntersection, Horoscope, NeutralSect, Exaltation, Macrocosmos, SunSign }
    public enum HoroscopeOption { None, Use, End }
    public enum MacrocosmosOption { None, Use, End }
    public enum HeliosOption { None, Use, UseEx }
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null; //Checks if Status effect is on self

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(AST.AID.AstralStasis);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: AST", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.AST), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.Helios, "Helios", "", 140, AST.AID.Helios);
        DefineSimpleConfig(res, Track.Lightspeed, "Lightspeed", "L.Speed", 140, AST.AID.Lightspeed, 15); //Self oGCD, 60s CD (120s total), 2 charges, 15s effect duration
        DefineSimpleConfig(res, Track.BeneficII, "BeneficII", "Bene2", 100, AST.AID.BeneficII); //ST GCD heal
        DefineSimpleConfig(res, Track.EssentialDignity, "EssentialDignity", "E.Dig", 140, AST.AID.EssentialDignity); //ST oGCD heal, 40s CD (120s Total), 3 charges
        DefineSimpleConfig(res, Track.AspectedBenefic, "AspectedBenefic", "A.Benefic", 100, AST.AID.AspectedBenefic, 15); //ST GCD regen, 15s effect duration

        res.Define(Track.AspectedHelios).As<HeliosOption>("AspectedHelios", "A.Helios", 130) //AoE 15s GCD heal/regen, 15s effect duration
            .AddOption(HeliosOption.None, "None", "Do not use automatically")
            .AddOption(HeliosOption.Use, "Use", "Use Aspected Helios", 1, 15, ActionTargets.Self, 40, 95)
            .AddOption(HeliosOption.UseEx, "UseEx", "Use Helios Conjunction", 1, 15, ActionTargets.Self, 96)
            .AddAssociatedActions(AST.AID.AspectedHelios, AST.AID.HeliosConjunction);

        DefineSimpleConfig(res, Track.Synastry, "Synastry", "", 200, AST.AID.Synastry, 20); //ST oGCD "kardia"-like heal, 120s CD, 20s effect duration
        DefineSimpleConfig(res, Track.CollectiveUnconscious, "CollectiveUnconscious", "C.Uncon", 100, AST.AID.CollectiveUnconscious, 5); //AoE oGCD mit/regen, 60s CD, 5s mitigation / 15s regen effect durations 
        DefineSimpleConfig(res, Track.CelestialOpposition, "CelestialOpposition", "C.Oppo", 100, AST.AID.CelestialOpposition, 15); //AoE oGCD heal/regen, 60s CD, 15s effect duration
        DefineSimpleConfig(res, Track.CelestialIntersection, "CelestialIntersection", "C.Inter", 100, AST.AID.CelestialIntersection, 30); //ST oGCD heal/shield, 30s CD (60s Total), 2 charges

        res.Define(Track.Horoscope).As<HoroscopeOption>("Horoscope", "Horo", 130) //Conditional AoE heal, 60s CD, 30s effect duration
            .AddOption(HoroscopeOption.None, "None", "Do not use automatically")
            .AddOption(HoroscopeOption.Use, "Use", "Use Horoscope", 60, 10, ActionTargets.Self, 76)
            .AddOption(HoroscopeOption.End, "UseEx", "Use Enhanced Horoscope", 0, 1, ActionTargets.Self, 76)
            .AddAssociatedActions(AST.AID.Horoscope, AST.AID.HoroscopeEnd);

        DefineSimpleConfig(res, Track.NeutralSect, "NeutralSect", "Sect", 250, AST.AID.NeutralSect, 30); //Self oGCD buffs, 120s CD, 20s heal+ / 30s buffed Aspected casts effect duration  
        DefineSimpleConfig(res, Track.Exaltation, "Exaltation", "Exalt", 100, AST.AID.Exaltation, 8); //ST oGCD mit, 60s CD, 8s effect duration

        res.Define(Track.Macrocosmos).As<MacrocosmosOption>("Macrocosmos", "Macro", 300) //AoE GCD heal (after damage taken), 180s CD, 15s effect duration
            .AddOption(MacrocosmosOption.None, "None", "Do not use automatically")
            .AddOption(MacrocosmosOption.Use, "Use", "Use Macrocosmos", 120, 2, ActionTargets.Hostile, 90)
            .AddOption(MacrocosmosOption.End, "UseEx", "Use Microcosmos", 0, 1, ActionTargets.Hostile, 90)
            .AddAssociatedActions(AST.AID.Macrocosmos, AST.AID.MicrocosmosEnd);

        DefineSimpleConfig(res, Track.SunSign, "SunSign", "", 290, AST.AID.SunSign, 15); //AoE oGCD mit (only can use when under NeutralSect), 15s effect duration

        return res;
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3);
        ExecuteSimple(strategy.Option(Track.Lightspeed), AST.AID.Lightspeed, Player);
        ExecuteSimple(strategy.Option(Track.BeneficII), AST.AID.BeneficII, primaryTarget ?? Player);
        ExecuteSimple(strategy.Option(Track.EssentialDignity), AST.AID.EssentialDignity, primaryTarget ?? Player);
        ExecuteSimple(strategy.Option(Track.AspectedBenefic), AST.AID.AspectedBenefic, primaryTarget ?? Player);
        ExecuteSimple(strategy.Option(Track.Synastry), AST.AID.Synastry, primaryTarget ?? Player);
        ExecuteSimple(strategy.Option(Track.CollectiveUnconscious), AST.AID.CollectiveUnconscious, Player);
        ExecuteSimple(strategy.Option(Track.CelestialOpposition), AST.AID.CelestialOpposition, Player);
        ExecuteSimple(strategy.Option(Track.CelestialIntersection), AST.AID.CelestialIntersection, Player);
        ExecuteSimple(strategy.Option(Track.NeutralSect), AST.AID.NeutralSect, Player);
        ExecuteSimple(strategy.Option(Track.Exaltation), AST.AID.Exaltation, primaryTarget ?? Player);
        ExecuteSimple(strategy.Option(Track.SunSign), AST.AID.SunSign, Player);

        //Aspected Helios full execution
        var heliosUp = HasEffect(AST.SID.AspectedHelios) || HasEffect(AST.SID.HeliosConjunction);
        var helios = strategy.Option(Track.AspectedHelios);
        var heliosAction = helios.As<HeliosOption>() switch
        {
            HeliosOption.Use => AST.AID.AspectedHelios,
            HeliosOption.UseEx => AST.AID.HeliosConjunction,
            _ => default
        };
        if (heliosAction != default && !heliosUp)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(heliosAction), Player, helios.Priority(), helios.Value.ExpireIn);

        //Horoscope full execution
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

    }
}
