namespace BossMod.Autorotation;

public sealed class ClassASTUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track { Helios = SharedTrack.Count, Lightspeed, BeneficII, EssentialDignity, AspectedBenefic, AspectedHelios, Synastry, CollectiveUnconscious, CelestialOpposition, EarthlyStar, CelestialIntersection, Horoscope, NeutralSect, Exaltation, Macrocosmos, SunSign }
    public enum StarOption { None, Use, End }
    public enum HoroscopeOption { None, Use, End }
    public enum MacrocosmosOption { None, Use, End }
    public enum HeliosOption { None, Use, UseEx }
    public float GetStatusDetail(Actor target, AST.SID sid) => StatusDetails(target, sid, Player.InstanceID).Left; //Checks if Status effect is on target
    public bool HasEffect(Actor target, AST.SID sid, float duration) => GetStatusDetail(target, sid) < duration; //Checks if anyone has a status effect
    public Actor? TargetChoice(StrategyValues.OptionRef strategy) => ResolveTargetOverride(strategy.Value);

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

        res.Define(Track.EarthlyStar).As<StarOption>("EarthlyStar", "E.Star", 200) //AoE GCD heal, 60s CD, 10s + 10s effect duration
            .AddOption(StarOption.None, "None", "Do not use automatically")
            .AddOption(StarOption.Use, "Earthly Star", "Use Earthly Star", 60, 10, ActionTargets.Hostile, 62)
            .AddOption(StarOption.End, "Stellar Detonation", "Use Stellar Detonation", 0, 1, ActionTargets.Hostile, 62)
            .AddAssociatedActions(AST.AID.EarthlyStar, AST.AID.StellarDetonation);

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

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Lightspeed), AST.AID.Lightspeed, Player);
        ExecuteSimple(strategy.Option(Track.BeneficII), AST.AID.BeneficII, TargetChoice(strategy.Option(Track.BeneficII)) ?? Player);
        ExecuteSimple(strategy.Option(Track.EssentialDignity), AST.AID.EssentialDignity, TargetChoice(strategy.Option(Track.EssentialDignity)) ?? Player);
        ExecuteSimple(strategy.Option(Track.AspectedBenefic), AST.AID.AspectedBenefic, TargetChoice(strategy.Option(Track.AspectedBenefic)) ?? Player);
        ExecuteSimple(strategy.Option(Track.Synastry), AST.AID.Synastry, TargetChoice(strategy.Option(Track.Synastry)) ?? Player);
        ExecuteSimple(strategy.Option(Track.CollectiveUnconscious), AST.AID.CollectiveUnconscious, Player);
        ExecuteSimple(strategy.Option(Track.CelestialOpposition), AST.AID.CelestialOpposition, Player);
        ExecuteSimple(strategy.Option(Track.CelestialIntersection), AST.AID.CelestialIntersection, Player);
        ExecuteSimple(strategy.Option(Track.NeutralSect), AST.AID.NeutralSect, Player);
        ExecuteSimple(strategy.Option(Track.Exaltation), AST.AID.Exaltation, TargetChoice(strategy.Option(Track.Exaltation)) ?? Player);
        ExecuteSimple(strategy.Option(Track.SunSign), AST.AID.SunSign, Player);

        var star = strategy.Option(Track.EarthlyStar);
        var starAction = star.As<StarOption>() switch
        {
            StarOption.Use => AST.AID.EarthlyStar,
            StarOption.End => AST.AID.StellarDetonation,
            _ => default
        };
        if (starAction != default)
            QueueOGCD(starAction, TargetChoice(star) ?? primaryTarget ?? Player);

        //Aspected Helios full execution
        var heliosUp = HasEffect(Player, AST.SID.AspectedHelios, 15) || HasEffect(Player, AST.SID.HeliosConjunction, 15);
        var helios = strategy.Option(Track.AspectedHelios);
        var heliosAction = helios.As<HeliosOption>() switch
        {
            HeliosOption.Use => AST.AID.AspectedHelios,
            HeliosOption.UseEx => AST.AID.HeliosConjunction,
            _ => default
        };
        if (heliosAction != default && !heliosUp)
            QueueGCD(heliosAction, Player);

        //Horoscope full execution
        var horo = strategy.Option(Track.Horoscope);
        var horoAction = horo.As<HoroscopeOption>() switch
        {
            HoroscopeOption.Use => AST.AID.Horoscope,
            HoroscopeOption.End => AST.AID.HoroscopeEnd,
            _ => default
        };
        if (horoAction != default)
            QueueOGCD(horoAction, Player);

        var cosmos = strategy.Option(Track.Macrocosmos);
        var cosmosAction = cosmos.As<MacrocosmosOption>() switch
        {
            MacrocosmosOption.Use => AST.AID.Macrocosmos,
            MacrocosmosOption.End => AST.AID.MicrocosmosEnd,
            _ => default
        };
        if (cosmosAction != default)
            QueueOGCD(cosmosAction, primaryTarget);
    }

    #region Core Execution Helpers

    public AST.AID NextGCD; //Next global cooldown action to be used
    public void QueueGCD<P>(AST.AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueGCD(aid, target, (int)(object)priority, delay);

    public void QueueGCD(AST.AID aid, Actor? target, int priority = 8, float delay = 0)
    {
        var NextGCDPrio = 0;

        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.High, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
        }
    }

    public void QueueOGCD<P>(AST.AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueOGCD(aid, target, (int)(object)priority, delay);

    public void QueueOGCD(AST.AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }

    public bool QueueAction(AST.AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return false;

        if (def.Range != 0 && target == null)
        {
            return false;
        }

        Vector3 targetPos = default;

        if (def.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (def.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, targetPos: targetPos);
        return true;
    }
    #endregion

}
