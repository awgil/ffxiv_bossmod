using BossMod.BLM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;
using static BossMod.StrikingDummy.StrikingDummyRotation;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiBLM(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Movement = SharedTrack.Count, Thunder, Polyglot, Manafont, Triplecast, Swiftcast, LeyLines, TPUS, Casting, Amplifier, Retrace, BTL }
    public enum MovementStrategy { Allow, AllowNoScathe, OnlyGCDs, OnlyOGCDs, OnlyScathe, Forbid }
    public enum ThunderStrategy { Allow3, Allow6, Allow9, AllowNoDOT, ForceAny, ForceST, ForceAOE, Forbid }
    public enum PolyglotStrategy { AutoSpendAll, AutoHold1, AutoHold2, AutoHold3, XenoSpendAll, XenoHold1, XenoHold2, XenoHold3, FoulSpendAll, FoulHold1, FoulHold2, FoulHold3, ForceXeno, ForceFoul, Delay }
    public enum ManafontStrategy { Automatic, Force, ForceWeave, ForceEX, ForceWeaveEX, Delay }
    public enum CommonStrategy { Automatic, Force, Force1, ForceWeave, ForceWeave1, Delay }
    public enum TPUSStrategy { Allow, OOConly, Forbid }
    public enum CastingOption { Allow, Forbid }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi BLM", "Standard Rotation Module", "Akechi|DPS", "Akechi", RotationModuleQuality.Basic, BitMask.Build(Class.THM, Class.BLM), 100);
        res.DefineAOE().AddAssociatedActions(
            AID.Fire1, AID.Fire2, AID.Fire3, AID.Fire4, AID.HighFire2, //Fire
            AID.Blizzard1, AID.Blizzard2, AID.Blizzard3, AID.Blizzard4, AID.HighBlizzard2, //Blizzard
            AID.Flare, AID.Freeze, AID.Despair, AID.FlareStar); //Other
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionInt);
        res.Define(Track.Movement).As<MovementStrategy>("Movement", uiPriority: 195)
            .AddOption(MovementStrategy.Allow, "Allow", "Allow the use of all appropriate abilities for use while moving")
            .AddOption(MovementStrategy.AllowNoScathe, "AllowNoScathe", "Allow the use of all appropriate abilities for use while moving except for Scathe")
            .AddOption(MovementStrategy.OnlyGCDs, "OnlyGCDs", "Only use instant cast GCDs for movement; Polyglots->Firestarter->Thunder->Scathe if nothing left")
            .AddOption(MovementStrategy.OnlyOGCDs, "OnlyOGCDs", "Only use OGCDs for movement; Swiftcast->Triplecast")
            .AddOption(MovementStrategy.OnlyScathe, "OnlyScathe", "Only use Scathe for use while moving")
            .AddOption(MovementStrategy.Forbid, "Forbid", "Forbid the use of any abilities for use while moving");
        res.Define(Track.Thunder).As<ThunderStrategy>("Thunder", "DOT", uiPriority: 190)
            .AddOption(ThunderStrategy.Allow3, "Allow3", "Allow the use Thunder if target Has 3s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow6, "Allow6", "Allow the use Thunder if target Has 6s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Allow9, "Allow9", "Allow the use Thunder if target Has 9s or less remaining on DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.AllowNoDOT, "AllowNoDOT", "Allow the use Thunder only if target does not have DoT effect", 0, 0, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAny, "Force", "Force use of best Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceST, "Force", "Force use of single-target Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.ForceAOE, "Force", "Force use of AOE Thunder regardless of DoT effect", 0, 24, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Forbid, "Forbid", "Forbid the use of Thunder for manual or strategic usage", 0, 0, ActionTargets.Hostile, 6)
            .AddAssociatedActions(AID.Thunder1, AID.Thunder2, AID.Thunder3, AID.Thunder4, AID.HighThunder, AID.HighThunder2);
        res.Define(Track.Polyglot).As<PolyglotStrategy>("Polyglot", "Polyglot", uiPriority: 180)
            .AddOption(PolyglotStrategy.AutoSpendAll, "AutoSpendAll", "Automatically select best polyglot based on targets; Spend all Polyglots as soon as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold1, "AutoHold1", "Automatically select best polyglot based on targets; holds 1 for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold2, "AutoHold2", "Automatically select best polyglot based on targets; holds 2 for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.AutoHold3, "AutoHold3", "Automatically select best polyglot based on targets; holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.XenoSpendAll, "XenoSpendAll", "Use Xenoglossy as Polyglot spender, regardless of targets nearby; spends all Polyglots", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold1, "XenoHold1", "Use Xenoglossy as Polyglot spender, regardless of targets nearby; holds 1 Polyglot for manual usage", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold2, "XenoHold2", "Use Xenoglossy as Polyglot spender, regardless of targets nearby; holds 2 Polyglots for manual usage", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.XenoHold3, "XenoHold3", "Use Xenoglossy as Polyglot spender; Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.FoulSpendAll, "FoulSpendAll", "Use Foul as Polyglot spender, regardless of targets nearby", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold1, "FoulHold1", "Use Foul as Polyglot spender, regardless of targets nearby; holds 1 Polyglot for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold2, "FoulHold2", "Use Foul as Polyglot spender, regardless of targets nearby; holds 2 Polyglots for manual usage", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.FoulHold3, "FoulHold3", "Use Foul as Polyglot spender; Holds all Polyglots for as long as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.ForceXeno, "Force Xenoglossy", "Force use of Xenoglossy", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.ForceFoul, "Force Foul", "Force use of Foul", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.Delay, "Delay", "Delay the use of Polyglot abilities for manual or strategic usage", 0, 0, ActionTargets.Hostile, 70)
            .AddAssociatedActions(AID.Xenoglossy, AID.Foul);

        res.Define(Track.Manafont).As<ManafontStrategy>("Manafont", "M.font", uiPriority: 165)
            .AddOption(ManafontStrategy.Automatic, "Auto", "Automatically use Manafont optimally", 0, 0, ActionTargets.Self, 30)
            .AddOption(ManafontStrategy.Force, "Force", "Force the use of Manafont (180s CDRemaining), regardless of weaving conditions", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(ManafontStrategy.ForceWeave, "ForceWeave", "Force the use of Manafont (180s CDRemaining) in any next possible weave slot", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(ManafontStrategy.ForceEX, "ForceEX", "Force the use of Manafont (100s CDRemaining), regardless of weaving conditions", 100, 0, ActionTargets.Self, 84)
            .AddOption(ManafontStrategy.ForceWeaveEX, "ForceWeaveEX", "Force the use of Manafont (100s CDRemaining) in any next possible weave slot", 100, 0, ActionTargets.Self, 84)
            .AddOption(ManafontStrategy.Delay, "Delay", "Delay the use of Manafont for strategic reasons", 0, 0, ActionTargets.Self, 30)
            .AddAssociatedActions(AID.Manafont);

        res.Define(Track.Triplecast).As<CommonStrategy>("T.cast", uiPriority: 170)
            .AddOption(CommonStrategy.Automatic, "Auto", "Use any charges available during Ley Lines window or every 2 minutes (NOTE: does not take into account charge overcap, will wait for 2 minute windows to spend both)", 0, 0, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.Force, "Force", "Force the use of Triplecast; uses all charges", 60, 15, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.Force1, "Force1", "Force the use of Triplecast; holds one charge for manual usage", 60, 15, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.ForceWeave, "ForceWeave", "Force the use of Triplecast in any next possible weave slot", 60, 15, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.ForceWeave1, "ForceWeave1", "Force the use of Triplecast in any next possible weave slot; holds one charge for manual usage", 60, 15, ActionTargets.Self, 66)
            .AddOption(CommonStrategy.Delay, "Delay", "Delay the use of Triplecast", 0, 0, ActionTargets.Self, 66)
            .AddAssociatedActions(AID.Triplecast);
        res.DefineOGCD(Track.Swiftcast, AID.Swiftcast, "Swiftcast", "Swiftcast", uiPriority: 171, 5, 0, ActionTargets.Self, 4);
        res.Define(Track.LeyLines).As<CommonStrategy>("L.Lines", uiPriority: 170)
            .AddOption(CommonStrategy.Automatic, "Auto", "Automatically decide when to use Ley Lines", 0, 0, ActionTargets.Self, 2)
            .AddOption(CommonStrategy.Force, "Force", "Force the use of Ley Lines, regardless of weaving conditions", 120, 30, ActionTargets.Self, 2)
            .AddOption(CommonStrategy.Force1, "Force1", "Force the use of Ley Lines; holds one charge for manual usage", 120, 30, ActionTargets.Self, 2)
            .AddOption(CommonStrategy.ForceWeave, "ForceWeave", "Force the use of Ley Lines in any next possible weave slot", 120, 30, ActionTargets.Self, 2)
            .AddOption(CommonStrategy.ForceWeave1, "ForceWeave1", "Force the use of Ley Lines in any next possible weave slot; holds one charge for manual usage", 120, 30, ActionTargets.Self, 2)
            .AddOption(CommonStrategy.Delay, "Delay", "Delay the use of Ley Lines", 0, 0, ActionTargets.Self, 2)
            .AddAssociatedActions(AID.LeyLines);
        res.Define(Track.TPUS).As<TPUSStrategy>("Transpose & Umbral Soul", "TP/US", uiPriority: 160)
            .AddOption(TPUSStrategy.Allow, "Allow", "Allow Transpose & Umbral Soul combo when out of combat or no targetable enemy is nearby", 0, 0, ActionTargets.Self, 35)
            .AddOption(TPUSStrategy.OOConly, "OOConly", "Only use Transpose & Umbral Soul combo when fully out of combat", 0, 0, ActionTargets.Self, 35)
            .AddOption(TPUSStrategy.Forbid, "Forbid", "Forbid Transpose & Umbral Soul combo", 0, 0, ActionTargets.Self, 35)
            .AddAssociatedActions(AID.Transpose, AID.UmbralSoul);
        res.Define(Track.Casting).As<CastingOption>("Casting", uiPriority: 155)
            .AddOption(CastingOption.Allow, "Allow", "Allow casting while moving")
            .AddOption(CastingOption.Forbid, "Forbid", "Forbid casting while moving");
        res.DefineOGCD(Track.Amplifier, AID.Amplifier, "Amplifier", "Amplifier", uiPriority: 170, 120, 0, ActionTargets.Self, 86);
        res.DefineOGCD(Track.Retrace, AID.Retrace, "Retrace", "Retrace", uiPriority: 170, 40, 0, ActionTargets.Self, 96);
        res.DefineOGCD(Track.BTL, AID.BetweenTheLines, "BTL", "Between the Lines", uiPriority: 170, 40, 0, ActionTargets.Self, 62);

        return res;
    }
    #endregion

    #region Priorities
    public enum NewGCDPriority //priorities for GCDs (higher number = higher priority)
    {
        None = 0,

        //Rotation
        SixthStep = 100,
        FifthStep = 125,
        FourthStep = 150,
        ThirdStep = 175,
        SecondStep = 200,
        FirstStep = 250,
        ForcedStep = 299,

        //GCDs
        Standard = 300,       //standard abilities
        DOT = 350,            //damage-over-time abilities
        FlareStar = 375,      //Flare Star
        Despair = 400,        //Despair
        F3P = 450,            //Fire III proc
        NeedB3 = 460,         //Need to use Blizzard III
        Polyglot = 475,       //Polyglots
        Paradox = 500,        //Paradox

        //Necessities
        NeedDOT = 600,        //Need to apply DOTs
        NeedF3P = 625,        //Need to use Fire III proc
        NeedPolyglot = 650,   //Need to use Polyglots

        //Moving
        Moving3 = 700,        //Moving (3rd priority)
        Moving2 = 710,        //Moving (2nd priority)
        Moving1 = 720,        //Moving (1st priority)

        //Forced
        ForcedGCD = 900,      //Forced GCDs

        //Opener
        Opener = 1000,        //Opener
    }
    public enum NewOGCDPriority //priorities for oGCDs (higher number = higher priority)
    {
        None = 0,
        Transpose = 400,      //Transpose
        Manafont = 450,       //Manafont
        LeyLines = 500,       //Ley Lines
        Amplifier = 550,      //Amplifier
        Triplecast = 600,     //Triplecast
        Potion = 800,         //Potion
        ForcedOGCD = 900,     //Forced oGCDs
    }
    #endregion

    #region Upgrade Paths
    private AID BestThunderST
        => Unlocked(AID.HighThunder) ? AID.HighThunder
        : Unlocked(AID.Thunder3) ? AID.Thunder3
        : AID.Thunder1;
    private AID BestThunderAOE
        => Unlocked(AID.HighThunder2) ? AID.HighThunder2
        : Unlocked(AID.Thunder4) ? AID.Thunder4
        : Unlocked(AID.Thunder2) ? AID.Thunder2
        : AID.Thunder1;
    private AID BestThunder
        => ShouldUseAOE ? BestThunderAOE : BestThunderST;
    private AID BestPolyglot
        => ShouldUseAOE ? AID.Foul : BestXenoglossy;
    private AID BestXenoglossy
        => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;
    #endregion

    #region Module Variables
    private bool NoUIorAF; //No active elemnt stance
    private bool HasAF; //In Astral Fire
    private bool HasUI; //In Umbral Ice
    private sbyte ElementStance; //Elemental Stance
    private byte UmbralHearts; //Umbral Hearts
    private int MaxUmbralHearts; //Max Umbral Hearts
    private bool HasMaxHearts;
    private int UmbralIce; //Umbral Ice Stacks
    private int AstralFire; //Astral Fire Stacks
    private int AstralSouls; //Stacks for Flare Star (Lv100)
    private bool HasParadox; //Paradox is active
    private bool HasThunderhead; //Has Thunderhead buff
    private bool HasFirestarter; //Has Firestarter buff

    private byte Polyglots; //Polyglot Stacks
    private int MaxPolyglots; //Max Polyglot Stacks
    private bool HasMaxPolyglots;
    private int PolyglotTimer; //Enochian timer
    private bool CanFoul; //Can use Foul
    private bool CanXG; //Can use Xenoglossy
    private bool CanParadox; //Can use Paradox
    private bool CanLL; //Can use Ley Lines
    private bool CanAmplify; //Can use Amplifier
    private bool CanTC; //Can use Triplecast
    private bool CanMF; //Can use Manafont
    private bool CanRetrace; //Can use Retrace
    private bool CanBTL; //Can use Between the Lines
    private float thunderLeft; //Time left on DOT effect (30s base)
    private bool ShouldUseAOE;
    private bool ShouldUseSTDOT;
    private bool ShouldUseAOEDOT;
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestDOTTargets;
    private Enemy? BestSplashTarget;
    private Enemy? BestDOTTarget;

    #endregion

    #region Module Helpers
    public float GetCastTime(AID aid)
    {
        var aspect = ActionDefinitions.Instance.Spell(aid)!.Aspect;
        var castTime = EffectiveCastTime(aid);
        if (PlayerHasEffect(SID.Triplecast, 15f) || PlayerHasEffect(SID.Swiftcast, 10f))
            return 0f;
        if (aid == AID.Fire3 && PlayerHasEffect(SID.Firestarter, 30f)
            || aid == AID.Foul && Unlocked(TraitID.EnhancedFoul)
            || aspect == ActionAspect.Thunder && PlayerHasEffect(SID.Thunderhead, 30f)
            || aid == AID.Despair && Unlocked(TraitID.EnhancedAstralFire))
            return 0;
        if (castTime == 0)
            return 0;
        if (ElementStance == -3 && aspect == ActionAspect.Fire ||
            ElementStance == 3 && aspect == ActionAspect.Ice)
            castTime *= 0.5f;
        return castTime;
    }
    #endregion

    #region Rotation Helpers
    public void BestST(Actor? target)
    {
        if (HasAF) //Fire
        {
            if ((UmbralHearts > 0 ? MP >= 800 : MP >= 1600) && AstralSouls != 6)
                QueueGCD(Unlocked(AID.Fire4) ? AID.Fire4 : AID.Fire1, target, GCDPriority.Low);
            if (Unlocked(AID.Blizzard3) && (((!Unlocked(AID.Despair) || LastActionUsed(AID.Despair)) || MP == 0) && ((!HasEffect(SID.Swiftcast) && !Unlocked(AID.Triplecast)) || (!HasEffect(SID.Swiftcast) || !HasEffect(SID.Triplecast)))))
                QueueGCD(AID.Blizzard3, target, GCDPriority.Low);
        }
        if (HasUI) //Ice
        {
            if (Unlocked(AID.Blizzard3) && UmbralIce != 3 && ((HasEffect(SID.Swiftcast) || !Unlocked(AID.Triplecast)) || (HasEffect(SID.Swiftcast) || HasEffect(SID.Triplecast))))
                QueueGCD(AID.Blizzard3, target, GCDPriority.Low);
            if (LastActionUsed(AID.Blizzard3) || (Unlocked(TraitID.UmbralHeart) ? (UmbralIce == 3 && !HasMaxHearts) : MP != MaxMP))
                QueueGCD(Unlocked(AID.Blizzard4) ? AID.Blizzard4 : AID.Blizzard1, target, GCDPriority.Low);
        }
    }
    private void BestRotation(Actor? target)
    {
        if (ShouldUseAOE)
            BestAOE(target);
        else
            BestST(target);
    }
    private void BestAOE(Actor? target) //AOE rotation based on level 
    {
        if (In25y(target))
        {
            if (NoUIorAF)
            {
                if (Unlocked(AID.Blizzard2) && !Unlocked(AID.HighBlizzard2))
                {
                    if (MP >= 10000)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.NeedB3);
                    if (MP < 10000 && Player.InCombat)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.NeedB3);
                }
                if (Unlocked(AID.HighBlizzard2))
                {
                    if (MP >= 10000)
                        QueueGCD(AID.HighBlizzard2, target, NewGCDPriority.NeedB3);
                    if (MP < 10000 && Player.InCombat)
                        QueueGCD(AID.HighBlizzard2, target, NewGCDPriority.NeedB3);
                }
            }
            if (Player.Level is >= 12 and <= 35)
            {
                //Fire
                if (Unlocked(AID.Fire2) && //if Fire is unlocked
                    HasAF && MP >= 3000) //or if Astral Fire is active and MP is 1600 or more
                    QueueGCD(AID.Fire2, target, NewGCDPriority.Standard); //Queue Fire II
                //Ice
                //TODO: MP tick is not fast enough before next cast, this will cause an extra unnecessary cast
                if (HasUI &&
                    MP <= 9600)
                    QueueGCD(AID.Blizzard2, target, NewGCDPriority.Standard); //Queue Blizzard II
                //Transpose 
                if (OGCDReady(AID.Transpose) && //if Transpose is unlocked & off cooldown
                    (HasAF && MP < 3000 || //if Astral Fire is active and MP is less than 1600
                    HasUI && MP > 9600)) //or if Umbral Ice is active and MP is max
                    QueueOGCD(AID.Transpose, Player, NewOGCDPriority.Transpose); //Queue Transpose
                //if in AF but no F2 yet, TP back to UI for B2 spam
                if (HasAF && !Unlocked(AID.Fire2))
                    QueueOGCD(AID.Transpose, Player, NewOGCDPriority.Transpose);
            }
            if (Player.Level is >= 35 and <= 39)
            {
                if (HasUI)
                {
                    //Step 1 - max stacks in UI
                    //TODO: MP tick is not fast enough before next cast, this will cause an extra unnecessary cast
                    if (Unlocked(AID.Blizzard2) &&
                        MP < 9600)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.FirstStep);
                    //Step 2 - swap from UI to AF
                    if (Unlocked(AID.Fire2) &&
                        MP >= 9600 &&
                        UmbralIce == 3)
                        QueueGCD(AID.Fire2, target, NewGCDPriority.SecondStep);
                }
                if (HasAF)
                {
                    if (MP >= 3000)
                        QueueGCD(AID.Fire2, target, NewGCDPriority.FirstStep);
                    if (Unlocked(AID.Blizzard2) &&
                        MP < 3000)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.SecondStep);
                }
            }
            if (Player.Level is >= 40 and <= 49)
            {
                if (HasUI)
                {
                    //Step 1 - max stacks in UI
                    if (Unlocked(AID.Blizzard2) &&
                        UmbralIce < 3)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.FirstStep);
                    //Step 2 - Freeze
                    if (Unlocked(AID.Freeze) && !LastActionUsed(AID.Freeze) &&
                        (LastActionUsed(AID.Blizzard2) || MP < 10000))
                        QueueGCD(AID.Freeze, target, NewGCDPriority.SecondStep);
                    //Step 3 - swap from UI to AF
                    if (Unlocked(AID.Fire2) &&
                        MP >= 10000 &&
                        UmbralIce == 3)
                        QueueGCD(AID.Fire2, target, NewGCDPriority.ThirdStep);
                }
                if (HasAF)
                {
                    if (MP >= 3000)
                        QueueGCD(AID.Fire2, target, NewGCDPriority.FirstStep);
                    if (Unlocked(AID.Blizzard2) &&
                        MP < 3000)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.SecondStep);
                }
            }
            if (Player.Level is >= 50 and <= 57)
            {
                if (HasUI)
                {
                    //Step 1 - max stacks in UI
                    if (Unlocked(AID.Blizzard2) &&
                        UmbralIce < 3)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.FirstStep);
                    //Step 2 - Freeze
                    if (Unlocked(AID.Freeze) && !LastActionUsed(AID.Freeze) &&
                        (LastActionUsed(AID.Blizzard2) || MP < 10000))
                        QueueGCD(AID.Freeze, target, NewGCDPriority.SecondStep);
                    //Step 3 - swap from UI to AF
                    if (Unlocked(AID.Fire2) &&
                        MP >= 10000 &&
                        UmbralIce == 3)
                        QueueGCD(AID.Fire2, target, NewGCDPriority.ThirdStep);
                }
                if (HasAF)
                {
                    //Step 1 - spam Fire 2
                    if (MP >= 3000)
                        QueueGCD(AID.Fire2, target, NewGCDPriority.FirstStep);
                    //Step 2 - Flare
                    if (Unlocked(AID.Flare) &&
                        MP < 3000)
                        QueueGCD(AID.Flare, target, NewGCDPriority.SecondStep);
                    //Step 3 - swap from AF to UI
                    if (Unlocked(AID.Blizzard2) &&
                        !Unlocked(AID.Flare) && MP < 3000 || //do your job quests, fool
                        Unlocked(AID.Flare) && MP < 400)
                        QueueGCD(AID.Blizzard2, target, MP < 400 ? NewGCDPriority.ForcedStep : NewGCDPriority.ThirdStep);
                }
            }
            if (Player.Level is >= 58 and <= 81)
            {
                if (HasUI)
                {
                    //Step 1 - max stacks in UI
                    if (Unlocked(AID.Blizzard2) &&
                        UmbralIce < 3)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.FirstStep);
                    //Step 2 - Freeze
                    if (Unlocked(AID.Freeze) && !LastActionUsed(AID.Freeze) &&
                        (LastActionUsed(AID.Blizzard2) || MP < 10000))
                        QueueGCD(AID.Freeze, target, NewGCDPriority.SecondStep);
                    //Step 3 - swap from UI to AF
                    if (Unlocked(AID.Fire2) &&
                        MP >= 10000 &&
                        UmbralIce == 3)
                        QueueGCD(AID.Fire2, target, NewGCDPriority.ThirdStep);
                }
                if (HasAF)
                {
                    //Step 1 - spam Fire 2
                    if (UmbralHearts > 1)
                        QueueGCD(AID.Fire2, target, NewGCDPriority.FirstStep);
                    //Step 2 - Flare
                    if (Unlocked(AID.Flare))
                    {
                        //first cast
                        if (UmbralHearts == 1)
                            QueueGCD(AID.Flare, target, NewGCDPriority.SecondStep);
                        //second cast
                        if (UmbralHearts == 0 &&
                            MP >= 800)
                            QueueGCD(AID.Flare, target, NewGCDPriority.ThirdStep);
                    }
                    //Step 3 - swap from AF to UI
                    if (Unlocked(AID.Blizzard2) &&
                        MP < 400)
                        QueueGCD(AID.Blizzard2, target, NewGCDPriority.FourthStep);
                }
            }
            if (Player.Level is >= 82 and <= 99)
            {
                if (HasUI)
                {
                    //Step 1 - max stacks in UI
                    if (Unlocked(AID.HighBlizzard2) &&
                        UmbralIce < 3)
                        QueueGCD(AID.HighBlizzard2, target, NewGCDPriority.FirstStep);
                    //Step 2 - Freeze
                    if (Unlocked(AID.Freeze) && !LastActionUsed(AID.Freeze) &&
                        (LastActionUsed(AID.HighBlizzard2) || MP < 10000))
                        QueueGCD(AID.Freeze, target, NewGCDPriority.SecondStep);
                    //Step 3 - swap from UI to AF
                    if (Unlocked(AID.HighFire2) &&
                        MP >= 10000 &&
                        UmbralIce == 3)
                        QueueGCD(AID.HighFire2, target, NewGCDPriority.ThirdStep);
                }
                if (HasAF)
                {
                    //Step 1 - spam Fire 2
                    if (MP > 5500)
                        QueueGCD(AID.HighFire2, target, NewGCDPriority.FirstStep);
                    //Step 2 - Flare
                    if (Unlocked(AID.Flare))
                    {
                        //first cast
                        if (UmbralHearts == 1)
                            QueueGCD(AID.Flare, target, NewGCDPriority.SecondStep);
                        //second cast
                        if (UmbralHearts == 0 &&
                            MP >= 800)
                            QueueGCD(AID.Flare, target, NewGCDPriority.ThirdStep);
                    }
                    //Step 3 - swap from AF to UI
                    if (Unlocked(AID.HighBlizzard2) &&
                        MP < 400)
                        QueueGCD(AID.HighBlizzard2, target, NewGCDPriority.ThirdStep);
                }
            }
            if (Player.Level is 100)
            {
                if (HasUI)
                {
                    //Step 1 - max stacks in UI
                    if (Unlocked(AID.HighBlizzard2) &&
                        UmbralIce < 3)
                        QueueGCD(AID.HighBlizzard2, target, NewGCDPriority.FirstStep);
                    //Step 2 - Freeze
                    if (Unlocked(AID.Freeze) && !LastActionUsed(AID.Freeze) &&
                        (LastActionUsed(AID.HighBlizzard2) || MP < 10000))
                        QueueGCD(AID.Freeze, target, NewGCDPriority.SecondStep);
                    //Step 3 - swap from UI to AF
                    if (Unlocked(AID.HighFire2) &&
                        MP >= 10000 &&
                        UmbralIce == 3)
                        QueueGCD(AID.HighFire2, target, NewGCDPriority.ThirdStep);
                }
                if (HasAF)
                {
                    //Step 0 - if forced over from ST
                    if (Unlocked(AID.HighFire2) &&
                        MP >= 8001)
                        QueueGCD(AID.HighFire2, target, NewGCDPriority.FirstStep);
                    //Step 1 - Flare
                    if (Unlocked(AID.Flare))
                    {
                        //first cast
                        if (UmbralHearts == 1)
                            QueueGCD(AID.Flare, target, NewGCDPriority.FirstStep);
                        //second cast
                        if (UmbralHearts == 0 &&
                            MP >= 800)
                            QueueGCD(AID.Flare, target, NewGCDPriority.SecondStep);
                    }
                    //Step 2 - Flare Star
                    if (AstralSouls == 6) //if Astral Soul stacks are max
                        QueueGCD(AID.FlareStar, target, NewGCDPriority.ThirdStep); //Queue Flare Star
                    //Step 3 - swap from AF to UI
                    if (Unlocked(AID.HighBlizzard2) &&
                        MP < 400)
                        QueueGCD(AID.HighBlizzard2, target, NewGCDPriority.FourthStep);
                }
            }
        }
    }
    #endregion

    #region Cooldown Helpers

    #region GCD
    public void ShouldOpen(Actor? target)
    {
        if (NoUIorAF)
        {
            //Opener - F3
            if (IsFirstGCD() && target != null)
            {
                if (CountdownRemaining <= GetCastTime(AID.Fire3) + 0.5f) //cast time + app delay
                    QueueGCD(AID.Fire3, target, GCDPriority.SlightlyHigh);
            }
            //Recovery - B3
            if (target != null && (Player.InCombat || CombatTimer >= 0.1f))
            {
                if (Unlocked(AID.Blizzard3) && UmbralIce < 3 && Player.InCombat)
                    QueueGCD(OGCDReady(AID.Swiftcast) && !HasEffect(SID.Triplecast) ? AID.Swiftcast : CanTC && !HasEffect(SID.Swiftcast) ? AID.Triplecast : AID.Blizzard3, target, GCDPriority.SlightlyHigh);
            }
        }
    }
    private (bool, GCDPriority) ShouldUsePolyglot(Actor? target, PolyglotStrategy strategy)
    {
        var max = HasMaxPolyglots;
        var veryclose = PolyglotTimer <= 3000f && max;
        var overcap = PolyglotTimer <= 7500f && max;
        var condition = Polyglots > 0 && InsideCombatWith(target) && (overcap ||
                (CDRemaining(AID.LeyLines) < 5 || CDRemaining(AID.LeyLines) == 0) || //LL overcap prep
                (CDRemaining(AID.Manafont) <= 1.5f && HasAF && MP <= 400) || //MF prep
                (CDRemaining(AID.Amplifier) <= GCD && HasMaxPolyglots)); //Amp prep
        var prio = veryclose ? GCDPriority.ExtremelyHigh + 1 : GCDPriority.SlightlyHigh;
        return strategy switch
        {
            PolyglotStrategy.AutoSpendAll or PolyglotStrategy.XenoSpendAll or PolyglotStrategy.FoulSpendAll => (Polyglots > 0 && condition, prio),
            PolyglotStrategy.AutoHold1 or PolyglotStrategy.XenoHold1 or PolyglotStrategy.FoulHold1 => (Polyglots > 1 && condition, prio),
            PolyglotStrategy.AutoHold2 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.FoulHold2 => (Polyglots > 2 && condition, prio),
            PolyglotStrategy.AutoHold3 or PolyglotStrategy.XenoHold3 or PolyglotStrategy.FoulHold3 => (HasMaxPolyglots && overcap, prio),
            PolyglotStrategy.ForceXeno => (CanXG, GCDPriority.Forced),
            PolyglotStrategy.ForceFoul => (CanFoul, GCDPriority.Forced),
            _ => (false, GCDPriority.None),
        };
    }

    #region DOT
    private static SID[] GetDotStatus() => [SID.Thunder, SID.ThunderII, SID.ThunderIII, SID.ThunderIV, SID.HighThunder, SID.HighThunderII];
    private float ThunderRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 0);
    private (bool, GCDPriority) ShouldUseThunder(Actor? target, ThunderStrategy strategy)
    {
        if (!HasThunderhead)
            return (false, GCDPriority.None);

        var condition = InsideCombatWith(target) && In25y(target);
        return strategy switch
        {
            ThunderStrategy.Allow3 => (condition && thunderLeft <= 3, GCDPriority.High),
            ThunderStrategy.Allow6 => (condition && thunderLeft < 6, GCDPriority.High),
            ThunderStrategy.Allow9 => (condition && thunderLeft < 9, GCDPriority.High),
            ThunderStrategy.AllowNoDOT => (condition && thunderLeft == 0, GCDPriority.ExtremelyHigh),
            ThunderStrategy.ForceAny or ThunderStrategy.ForceST or ThunderStrategy.ForceAOE => (true, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
    }
    #endregion

    private bool ShouldUseF3P => Unlocked(AID.Fire3) && HasFirestarter && HasAF && AstralFire < 3;
    private bool ShouldUseParadox => CanParadox && CombatTimer > 35 && ((HasUI && UmbralHearts == MaxUmbralHearts) || (HasAF && !HasFirestarter));
    private bool ShouldUseDespair => Unlocked(AID.Despair) && HasAF && MP is < 1600 and >= 800;
    private bool ShouldUseFlareStar => Unlocked(AID.FlareStar) && AstralSouls == 6;
    #endregion

    #region OGCD
    private (bool, OGCDPriority) ShouldUseTranspose => ((HasUI && UmbralIce == 3 && UmbralHearts == MaxUmbralHearts && !HasParadox) || (HasAF && AstralFire == 3 && (LastActionUsed(AID.Despair) || MP == 0)), OGCDPriority.ExtremelyHigh);
    private (bool, OGCDPriority) ShouldUseManafont(Actor? target, ManafontStrategy strategy)
    {
        if (!CanMF)
            return (false, OGCDPriority.None);

        var condition = InsideCombatWith(target) && HasAF && !HasParadox && MP < 800;
        return strategy switch
        {
            ManafontStrategy.Automatic => (condition, OGCDPriority.Severe),
            ManafontStrategy.Force or ManafontStrategy.ForceEX => (CanMF, OGCDPriority.Severe),
            ManafontStrategy.ForceWeave or ManafontStrategy.ForceWeaveEX => (CanMF && CanWeaveIn, OGCDPriority.Severe),
            _ => (false, OGCDPriority.None)
        };
    }
    private (bool, OGCDPriority) ShouldUseLeyLines(Actor? target, CommonStrategy strategy)
    {
        if (!CanLL)
            return (false, OGCDPriority.None);

        return strategy switch
        {
            CommonStrategy.Automatic => (InsideCombatWith(target) && (RaidBuffsLeft > 0f || RaidBuffsIn < 3000f), OGCDPriority.Average + 1),
            CommonStrategy.Force => (true, OGCDPriority.Average + 1),
            CommonStrategy.Force1 => (CDRemaining(AID.LeyLines) <= 5, OGCDPriority.Average + 1),
            CommonStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Average + 1),
            CommonStrategy.ForceWeave1 => (CanWeaveIn && CDRemaining(AID.LeyLines) <= 5, OGCDPriority.Average + 1),
            _ => (false, OGCDPriority.None)
        };
    }
    private bool ShouldUseTriplecast(Actor? target, CommonStrategy strategy)
    {
        if (!CanTC)
            return false;

        return strategy switch
        {
            CommonStrategy.Automatic => InsideCombatWith(target) && CanTC && HasUI && UmbralIce < 3 && !HasEffect(SID.Swiftcast) && CDRemaining(AID.Swiftcast) > 0.6f,
            CommonStrategy.Force => true,
            CommonStrategy.Force1 => CDRemaining(AID.Triplecast) <= 5f,
            CommonStrategy.ForceWeave => CanWeaveIn,
            CommonStrategy.ForceWeave1 => CanWeaveIn && CDRemaining(AID.Triplecast) <= 5f,
            _ => false
        };
    }
    private bool ShouldUseSwiftcast(Actor? target, OGCDStrategy strategy)
    {
        if (!CanSwiftcast)
            return false;

        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && HasUI && UmbralIce < 3 && !HasEffect(SID.Triplecast),
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            _ => false
        };
    }
    private (bool, OGCDPriority) TriggerInstantCasts(Actor? target, StrategyValues strategy)
    {
        //we want Swiftcast first primarily.. and if not up, send Triplecast
        if (HasUI && (UmbralIce < 3 || LastActionUsed(AID.Transpose)))
        {
            if (ShouldUseSwiftcast(target, strategy.Option(Track.Swiftcast).As<OGCDStrategy>()))
                return (true, OGCDPriority.Average + 1);
            if (ShouldUseTriplecast(target, strategy.Option(Track.Triplecast).As<CommonStrategy>()))
                return (true, OGCDPriority.Average);
        }

        return (false, OGCDPriority.None);
    }
    private bool ShouldUseOGCD(bool canUse, OGCDStrategy strategy)
    {
        if (!canUse)
            return false;

        return strategy switch
        {
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            _ => false
        };
    }
    private bool ShouldUseAmplifier(Actor? target, OGCDStrategy strategy) => strategy == OGCDStrategy.Automatic ? (InsideCombatWith(target) && !HasMaxPolyglots) : ShouldUseOGCD(CanAmplify, strategy);
    private bool ShouldUseRetrace(OGCDStrategy strategy) => ShouldUseOGCD(CanRetrace, strategy);
    private bool ShouldUseBTL(OGCDStrategy strategy) => ShouldUseOGCD(CanBTL, strategy);
    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Potion() switch
    {
        PotionStrategy.AlignWithBuffs or PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 5000 || RaidBuffsLeft > 0),
        PotionStrategy.Immediate => true,
        _ => false
    };

    #endregion

    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<BlackMageGauge>(); //Retrieve BLM gauge
        ElementStance = gauge.ElementStance; //Elemental Stance
        NoUIorAF = ElementStance is 0 and not (1 or 2 or 3 or -1 or -2 or -3); //No stance
        HasAF = ElementStance is 1 or 2 or 3 and not (0 or -1 or -2 or -3); //In Astral Fire
        AstralFire = gauge.AstralStacks; //Astral Fire Stacks
        HasUI = ElementStance is -1 or -2 or -3 and not (0 or 1 or 2 or 3); //In Umbral Ice
        UmbralIce = gauge.UmbralStacks; //Umbral Ice Stacks
        UmbralHearts = gauge.UmbralHearts; //Umbral Hearts
        MaxUmbralHearts = Unlocked(TraitID.UmbralHeart) ? 3 : 0;
        HasMaxHearts = UmbralHearts == MaxUmbralHearts; //Has max Umbral Hearts
        AstralSouls = gauge.AstralSoulStacks; //Stacks for Flare Star (Lv100)
        HasParadox = gauge.ParadoxActive; //Paradox is active
        Polyglots = gauge.PolyglotStacks; //Polyglot Stacks
        MaxPolyglots = Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : 1;
        HasMaxPolyglots = Polyglots == MaxPolyglots; //Has max Polyglot stacks
        PolyglotTimer = gauge.EnochianTimer; //Enochian timer
        CanFoul = Unlocked(AID.Foul) && Polyglots > 0; //Can use Foul
        CanXG = Unlocked(AID.Xenoglossy) && Polyglots > 0; //Can use Xenoglossy
        CanParadox = Unlocked(AID.Paradox) && HasParadox && MP >= 1600; //Can use Paradox
        CanLL = Unlocked(AID.LeyLines) && CDRemaining(AID.LeyLines) <= 120 && Player.FindStatus(SID.LeyLines) == null; //Can use Ley Lines
        CanAmplify = OGCDReady(AID.Amplifier); //Can use Amplifier
        CanTC = Unlocked(AID.Triplecast) && CDRemaining(AID.Triplecast) <= 60 && Player.FindStatus(SID.Triplecast) == null; //Can use Triplecast
        CanMF = OGCDReady(AID.Manafont); //Can use Manafont
        CanRetrace = OGCDReady(AID.Retrace) && Player.FindStatus(SID.LeyLines) != null; //Can use Retrace
        CanBTL = OGCDReady(AID.BetweenTheLines) && Player.FindStatus(SID.LeyLines) != null; //Can use Between the Lines
        HasThunderhead = Player.FindStatus(SID.Thunderhead) != null; //Has Thunderhead buff
        HasFirestarter = Player.FindStatus(SID.Firestarter) != null; //Has Firestarter buff
        thunderLeft = Utils.MaxAll( //Time left on DOT effect
            StatusDetails(BestSplashTarget?.Actor, SID.Thunder, Player.InstanceID, 24).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderII, Player.InstanceID, 18).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderIII, Player.InstanceID, 27).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.ThunderIV, Player.InstanceID, 21).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.HighThunder, Player.InstanceID, 30).Left,
            StatusDetails(BestSplashTarget?.Actor, SID.HighThunderII, Player.InstanceID, 24).Left);
        ShouldUseAOE = Unlocked(AID.Blizzard2) && NumSplashTargets > 2;
        ShouldUseSTDOT = Unlocked(AID.Thunder1) && NumSplashTargets <= 2;
        ShouldUseAOEDOT = Unlocked(AID.Thunder2) && NumSplashTargets > 2;
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 25, IsSplashTarget);
        (BestDOTTargets, thunderLeft) = GetDOTTarget(primaryTarget, ThunderRemaining, 2);
        BestSplashTarget = ShouldUseAOE ? BestSplashTargets : primaryTarget;
        BestDOTTarget = ShouldUseAOEDOT ? BestSplashTargets : ShouldUseSTDOT ? BestDOTTargets : primaryTarget;

        #region Strategy Definitions
        var AOE = strategy.Option(SharedTrack.AOE); //AOE track
        var movementStrat = strategy.Option(Track.Movement).As<MovementStrategy>();
        var thunder = strategy.Option(Track.Thunder); //Thunder track
        var thunderStrat = thunder.As<ThunderStrategy>(); //Thunder strategy
        var polyglot = strategy.Option(Track.Polyglot); //Polyglot track
        var polyglotStrat = polyglot.As<PolyglotStrategy>(); //Polyglot strategy
        var mf = strategy.Option(Track.Manafont); //Manafont track
        var mfStrat = mf.As<ManafontStrategy>(); //Manafont strategy
        var tc = strategy.Option(Track.Triplecast); //Triplecast track
        var tcStrat = tc.As<CommonStrategy>(); //Triplecast strategy
        var sc = strategy.Option(Track.Swiftcast); //Swiftcast track
        var scStrat = sc.As<OGCDStrategy>(); //Swiftcast strategy
        var ll = strategy.Option(Track.LeyLines); //Ley Lines track
        var llStrat = ll.As<CommonStrategy>(); //Ley Lines strategy
        var amp = strategy.Option(Track.Amplifier); //Amplifier track
        var ampStrat = amp.As<OGCDStrategy>(); //Amplifier strategy
        var retrace = strategy.Option(Track.Retrace); //Retrace track
        var retraceStrat = retrace.As<OGCDStrategy>(); //Retrace strategy
        var btl = strategy.Option(Track.BTL); //Between the Lines track
        var btlStrat = btl.As<OGCDStrategy>(); //Between the Lines strategy
        var tpusStrat = strategy.Option(Track.TPUS).As<TPUSStrategy>(); //Transpose/Umbral Soul strategy
        var movingOption = strategy.Option(Track.Casting).As<CastingOption>(); //Casting while moving strategy

        #endregion

        #endregion

        #region Full Rotation Execution

        #region Movement
        if (Player.InCombat &&
            BestSplashTarget?.Actor != null &&
            IsMoving)
        {
            if (movementStrat is MovementStrategy.Allow
                or MovementStrategy.AllowNoScathe
                or MovementStrategy.OnlyGCDs)
            {
                // GCDs
                if (!HasEffect(AID.Swiftcast) || !HasEffect(SID.Triplecast))
                {
                    if (Unlocked(TraitID.EnhancedPolyglot) && Polyglots > 0)
                        QueueGCD(strategy.ForceST() ? BestXenoglossy : strategy.ForceAOE() ? AID.Foul : BestPolyglot,
                                 TargetChoice(polyglot) ?? (strategy.ForceST() ? primaryTarget?.Actor : BestSplashTarget?.Actor),
                                 NewGCDPriority.Moving1);

                    if (PlayerHasEffect(SID.Firestarter, 30))
                        QueueGCD(AID.Fire3,
                                 TargetChoice(AOE) ?? primaryTarget?.Actor,
                                 NewGCDPriority.Moving1);

                    if (HasThunderhead)
                        QueueGCD(strategy.ForceST() ? BestThunderST : strategy.ForceAOE() ? BestThunderAOE : BestThunder,
                                 TargetChoice(thunder) ?? (strategy.ForceST() ? primaryTarget?.Actor : BestSplashTarget?.Actor),
                                 NewGCDPriority.Moving1);
                }
            }
            if (movementStrat is MovementStrategy.Allow
                or MovementStrategy.AllowNoScathe
                or MovementStrategy.OnlyOGCDs)
            {
                //OGCDs
                if (OGCDReady(AID.Swiftcast) &&
                    !PlayerHasEffect(SID.Triplecast, 15))
                    QueueOGCD(AID.Swiftcast, Player, NewGCDPriority.Moving2);
                if (CanTC &&
                    !HasEffect(AID.Swiftcast))
                    QueueOGCD(AID.Triplecast, Player, NewGCDPriority.Moving3);
            }
            if (movementStrat is MovementStrategy.Allow
                or MovementStrategy.OnlyScathe)
            {
                if (Unlocked(AID.Scathe) && MP >= 800)
                    QueueGCD(AID.Scathe, TargetChoice(AOE) ?? primaryTarget?.Actor, NewGCDPriority.Moving1);
            }
        }
        #endregion

        #region Out of Combat
        if (BestSplashTarget?.Actor == null &&
            (tpusStrat == TPUSStrategy.Allow && (!Player.InCombat || Player.InCombat && Hints.NumPriorityTargetsInAOECircle(Player.Position, 30) == 0) ||
            tpusStrat == TPUSStrategy.OOConly && !Player.InCombat))
        {
            if (Unlocked(AID.Transpose))
            {
                if (!Unlocked(AID.UmbralSoul))
                {
                    if (CDRemaining(AID.Transpose) < 0.6f &&
                        (HasAF || HasUI))
                        QueueOGCD(AID.Transpose, Player, NewOGCDPriority.Transpose);
                }
                if (Unlocked(AID.UmbralSoul))
                {
                    if (HasAF || (HasUI && !HasThunderhead))
                        QueueOGCD(AID.Transpose, Player, NewOGCDPriority.Transpose);
                    if (HasUI && HasThunderhead && (UmbralIce < 3 || !HasMaxHearts))
                        QueueGCD(AID.UmbralSoul, Player, NewGCDPriority.Standard);
                }
            }
        }

        if (CountdownRemaining > 0 || (CountdownRemaining == null || CombatTimer == 0))
        {
            ShouldOpen(primaryTarget?.Actor);
        }
        #endregion

        #region Standard Rotations
        if (movingOption is CastingOption.Allow ||
            movingOption is CastingOption.Forbid &&
            (!IsMoving || HasEffect(SID.Swiftcast) || HasEffect(SID.Triplecast) || ShouldUseParadox || ShouldUseF3P || (Unlocked(TraitID.EnhancedAstralFire) && HasAF && MP is <= 800 and not 0)))
        {
            if (strategy.AutoFinish() || strategy.AutoBreak())
                BestRotation(TargetChoice(AOE) ?? BestSplashTarget?.Actor);
            if (strategy.ForceST())
                BestST(TargetChoice(AOE) ?? primaryTarget?.Actor);
            if (strategy.ForceAOE())
                BestAOE(TargetChoice(AOE) ?? BestSplashTarget?.Actor);
        }
        #endregion

        #region Cooldowns
        #region GCD
        //Thunder
        var (tCondition, tPrio) = ShouldUseThunder(primaryTarget?.Actor, thunderStrat);
        if (tCondition)
        {
            var action = strategy.Rotation() switch
            {
                AOEStrategy.AutoFinish or AOEStrategy.AutoBreak => BestThunder,
                AOEStrategy.ForceST => BestThunderST,
                AOEStrategy.ForceAOE => BestThunderAOE,
                _ => AID.None
            };
            QueueGCD(action, TargetChoice(thunder) ?? BestDOTTarget?.Actor, tPrio);
        }
        //F3P
        if (InsideCombatWith(primaryTarget?.Actor) && ShouldUseF3P)
            QueueGCD(AID.Fire3, primaryTarget?.Actor, GCDPriority.Average);
        //Paradox
        if (InsideCombatWith(primaryTarget?.Actor) && ShouldUseParadox)
            QueueGCD(AID.Paradox, primaryTarget?.Actor, GCDPriority.Average);
        //FlareStar
        if (InsideCombatWith(BestSplashTarget?.Actor) && ShouldUseFlareStar)
            QueueGCD(AID.FlareStar, BestSplashTarget?.Actor, GCDPriority.Average + 6);
        //Despair
        if (InsideCombatWith(primaryTarget?.Actor) && ShouldUseDespair)
            QueueGCD(AID.Despair, primaryTarget?.Actor, GCDPriority.Average + 1);
        //Polyglots
        var (pgCondition, pgPrio) = ShouldUsePolyglot(primaryTarget?.Actor, polyglotStrat);
        if (pgCondition)
        {
            var action = polyglotStrat switch
            {
                PolyglotStrategy.AutoSpendAll or PolyglotStrategy.AutoHold1 or PolyglotStrategy.AutoHold2 or PolyglotStrategy.AutoHold3 => BestPolyglot,
                PolyglotStrategy.XenoSpendAll or PolyglotStrategy.XenoHold1 or PolyglotStrategy.XenoHold2 or PolyglotStrategy.XenoHold3 or PolyglotStrategy.ForceXeno => BestXenoglossy,
                PolyglotStrategy.FoulSpendAll or PolyglotStrategy.FoulHold1 or PolyglotStrategy.FoulHold2 or PolyglotStrategy.FoulHold3 or PolyglotStrategy.ForceFoul => AID.Foul,
                _ => AID.None
            };
            QueueGCD(action, TargetChoice(polyglot) ?? BestSplashTarget?.Actor, pgPrio);
        }

        #endregion

        #region OGCD
        //Swiftcast & Triplecast
        var (icCondition, icPrio) = TriggerInstantCasts(primaryTarget?.Actor, strategy);
        if (icCondition)
        {
            QueueOGCD(AID.Swiftcast, Player, icPrio + 1);
            QueueOGCD(AID.Triplecast, Player, icPrio);
        }
        //Transpose
        var (tpCondition, tpPrio) = ShouldUseTranspose;
        if (tpCondition)
            QueueOGCD(AID.Transpose, Player, tpPrio);
        //Manafont
        var (mfCondition, mfPrio) = ShouldUseManafont(primaryTarget?.Actor, mfStrat);
        if (mfCondition)
            QueueOGCD(AID.Manafont, Player, mfPrio);
        //LeyLines
        var (llCondition, llPrio) = ShouldUseLeyLines(primaryTarget?.Actor, llStrat);
        if (llCondition)
            QueueOGCD(AID.LeyLines, Player, llPrio);

        //Amplifier
        if (ShouldUseAmplifier(BestSplashTarget?.Actor, ampStrat))
            QueueOGCD(AID.Amplifier,
                Player,
                ampStrat is OGCDStrategy.Force
                or OGCDStrategy.AnyWeave
                or OGCDStrategy.EarlyWeave
                or OGCDStrategy.LateWeave
                ? NewOGCDPriority.ForcedOGCD
                : NewOGCDPriority.Amplifier);
        //Retrace
        //TODO: more options?
        if (ShouldUseRetrace(retraceStrat))
            QueueOGCD(AID.Retrace,
                Player,
                NewOGCDPriority.ForcedOGCD);
        //Between the Lines
        //TODO: Utility maybe?
        if (ShouldUseBTL(btlStrat))
            QueueOGCD(AID.BetweenTheLines,
                Player,
                NewOGCDPriority.ForcedOGCD);
        #endregion

        //Potion
        if (ShouldUsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionInt, Player, ActionQueue.Priority.Medium);

        #endregion

        #region AI
        GoalZoneSingle(25);
        #endregion

        #endregion
    }
}
