using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.DRG;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiDRG(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { AOE, Hold, Dives, Potion, LifeSurge, Jump, DragonfireDive, Geirskogul, Stardiver, PiercingTalon, TrueNorth, LanceCharge, BattleLitany, MirageDive, Nastrond, WyrmwindThrust, RiseOfTheDragon, Starcross }
    public enum AOEStrategy { AutoFinish, AutoBreak, ForceST, Force123ST, ForceBuffsST, ForceAOE }
    public enum HoldStrategy { Allow, Forbid }
    public enum DivesStrategy { AllowMaxMelee, AllowCloseMelee, Allow, Forbid }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    public enum SurgeStrategy { Automatic, Force, ForceWeave, ForceNextOpti, ForceNextOptiWeave, Delay }
    public enum JumpStrategy { Automatic, Force, ForceEX, ForceEX2, ForceWeave, Delay }
    public enum DragonfireStrategy { Automatic, Force, ForceEX, ForceWeave, Delay }
    public enum GeirskogulStrategy { Automatic, Force, ForceEX, ForceWeave, Delay }
    public enum StardiverStrategy { Automatic, Force, ForceEX, ForceWeave, Delay }
    public enum PiercingTalonStrategy { AllowEX, Allow, Force, ForceEX, Forbid }
    public enum TrueNorthStrategy { Automatic, ASAP, Rear, Flank, Force, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRG", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Excellent, BitMask.Build(Class.LNC, Class.DRG), 100);

        res.Define(Track.AOE).As<AOEStrategy>("Combo Option", "AOE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoFinish, "Auto (Finish combo)", "Automatically execute optimal rotation based on targets; finishes combo if possible", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoBreak, "Auto (Break combo)", "Automatically execute optimal rotation based on targets; breaks combo if necessary", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceST, "Force ST", "Force Single-Target rotation", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.Force123ST, "Only 1-2-3 ST", "Force only ST 1-2-3 rotation (No Buff or DoT)", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceBuffsST, "Only 1-4-5 ST", "Force only ST 1-4-5 rotation (Buff & DoT only)", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "Force AOE", "Force AOE rotation, even if less than 3 targets");
        res.Define(Track.Hold).As<HoldStrategy>("Cooldowns", "CDs", uiPriority: 190)
            .AddOption(HoldStrategy.Allow, "Allow", "Allow the use of all cooldowns & buffs")
            .AddOption(HoldStrategy.Forbid, "Forbid", "Forbid the use of all cooldowns & buffs");
        res.Define(Track.Dives).As<DivesStrategy>("Dives", uiPriority: 185)
            .AddOption(DivesStrategy.AllowMaxMelee, "Allow Max Melee", "Allow Jump, Stardiver, & Dragonfire Dive only at max melee range (within 3y)")
            .AddOption(DivesStrategy.AllowCloseMelee, "Allow Close Melee", "Allow Jump, Stardiver, & Dragonfire Dive only at close melee range (within 1y)")
            .AddOption(DivesStrategy.Allow, "Allow", "Allow the use of Jump, Stardiver, & Dragonfire Dive at any distance")
            .AddOption(DivesStrategy.Forbid, "Forbid", "Forbid the use of Jump, Stardiver, & Dragonfire Dive")
            .AddAssociatedActions(AID.Jump, AID.HighJump, AID.DragonfireDive, AID.Stardiver);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 90)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use potions automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Align With Raid Buffs", "Use potion in sync with 2-minute raid buffs (e.g., 0/6, 2/8)")
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potion as soon as possible, regardless of any buffs")
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.LifeSurge).As<SurgeStrategy>("Life Surge", "L. Surge", uiPriority: 160)
            .AddOption(SurgeStrategy.Automatic, "Automatic", "Use Life Surge normally")
            .AddOption(SurgeStrategy.Force, "Force", "Force Life Surge usage", 40, 5, ActionTargets.Hostile, 6)
            .AddOption(SurgeStrategy.ForceWeave, "Force Weave", "Force Life Surge usage inside the next possible weave window", 40, 5, ActionTargets.Hostile, 6)
            .AddOption(SurgeStrategy.ForceNextOpti, "Force Optimally", "Force Life Surge usage in next possible optimal window", 40, 5, ActionTargets.Hostile, 6)
            .AddOption(SurgeStrategy.ForceNextOptiWeave, "Force Weave Optimally", "Force Life Surge optimally inside the next possible weave window", 40, 5, ActionTargets.Hostile, 6)
            .AddOption(SurgeStrategy.Delay, "Delay", "Delay the use of Life Surge", 0, 0, ActionTargets.None, 6)
            .AddAssociatedActions(AID.LifeSurge);
        res.Define(Track.Jump).As<JumpStrategy>("Jump", uiPriority: 145)
            .AddOption(JumpStrategy.Automatic, "Automatic", "Use Jump normally")
            .AddOption(JumpStrategy.Force, "Force Jump", "Force Jump usage", 30, 0, ActionTargets.Self, 30, 67)
            .AddOption(JumpStrategy.ForceEX, "Force Jump (EX)", "Force Jump usage (Grants Dive Ready buff)", 30, 15, ActionTargets.Self, 68, 74)
            .AddOption(JumpStrategy.ForceEX2, "Force High Jump", "Force High Jump usage", 30, 15, ActionTargets.Self, 75)
            .AddOption(JumpStrategy.ForceWeave, "Force Weave", "Force Jump usage inside the next possible weave window", 30, 15, ActionTargets.Hostile, 68)
            .AddOption(JumpStrategy.Delay, "Delay", "Delay Jump usage", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.Jump, AID.HighJump);
        res.Define(Track.DragonfireDive).As<DragonfireStrategy>("Dragonfire Dive", "D.Dive", uiPriority: 155)
            .AddOption(DragonfireStrategy.Automatic, "Automatic", "Use Dragonfire Dive normally")
            .AddOption(DragonfireStrategy.Force, "Force", "Force Dragonfire Dive usage", 120, 0, ActionTargets.Hostile, 50, 91)
            .AddOption(DragonfireStrategy.ForceEX, "ForceEX", "Force Dragonfire Dive (Grants Dragon's Flight)", 120, 30, ActionTargets.Hostile, 92)
            .AddOption(DragonfireStrategy.ForceWeave, "Force Weave", "Force Dragonfire Dive usage inside the next possible weave window", 120, 0, ActionTargets.Hostile, 68)
            .AddOption(DragonfireStrategy.Delay, "Delay", "Delay Dragonfire Dive usage", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(AID.DragonfireDive);
        res.Define(Track.Geirskogul).As<GeirskogulStrategy>("Geirskogul", "Geirs.", uiPriority: 150)
            .AddOption(GeirskogulStrategy.Automatic, "Automatic", "Use Geirskogul normally")
            .AddOption(GeirskogulStrategy.Force, "Force", "Force Geirskogul usage", 60, 0, ActionTargets.Hostile, 60, 69)
            .AddOption(GeirskogulStrategy.ForceEX, "ForceEX", "Force Geirskogul (Grants Life of the Dragon & Nastrond Ready)", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(GeirskogulStrategy.ForceWeave, "Force Weave", "Force Geirskogul usage inside the next possible weave window", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(GeirskogulStrategy.Delay, "Delay", "Delay Geirskogul usage", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Geirskogul);
        res.Define(Track.Stardiver).As<StardiverStrategy>("Stardiver", "S.diver", uiPriority: 140)
            .AddOption(StardiverStrategy.Automatic, "Automatic", "Use Stardiver normally")
            .AddOption(StardiverStrategy.Force, "Force", "Force Stardiver usage", 30, 0, ActionTargets.Hostile, 80, 99)
            .AddOption(StardiverStrategy.ForceEX, "ForceEX", "Force Stardiver (Grants Starcross Ready)", 30, 0, ActionTargets.Hostile, 100)
            .AddOption(StardiverStrategy.ForceWeave, "Force Weave", "Force Stardiver usage inside the next possible weave window", 30, 0, ActionTargets.Hostile, 80)
            .AddOption(StardiverStrategy.Delay, "Delay", "Delay Stardiver usage", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Stardiver);
        res.Define(Track.PiercingTalon).As<PiercingTalonStrategy>("Piercing Talon", "P.Talon", uiPriority: 100)
            .AddOption(PiercingTalonStrategy.AllowEX, "AllowEX", "Allow use of Piercing Talon if already in combat, outside melee range, & is Enhanced")
            .AddOption(PiercingTalonStrategy.Allow, "Allow", "Allow use of Piercing Talon if already in combat & outside melee range")
            .AddOption(PiercingTalonStrategy.Force, "Force", "Force Piercing Talon usage ASAP (even in melee range)")
            .AddOption(PiercingTalonStrategy.ForceEX, "ForceEX", "Force Piercing Talon usage ASAP when Enhanced")
            .AddOption(PiercingTalonStrategy.Forbid, "Forbid", "Forbid use of Piercing Talon")
            .AddAssociatedActions(AID.PiercingTalon);
        res.Define(Track.TrueNorth).As<TrueNorthStrategy>("True North", "T.North", uiPriority: 95)
            .AddOption(TrueNorthStrategy.Automatic, "Automatic", "Late-weaves True North when out of positional")
            .AddOption(TrueNorthStrategy.ASAP, "ASAP", "Use True North as soon as possible when out of positional", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Rear, "Rear", "Use True North for rear positional only", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Flank, "Flank", "Use True North for flank positional only", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Force, "Force", "Force True North usage", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Delay, "Delay", "Delay True North usage", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(ClassShared.AID.TrueNorth);
        res.DefineOGCD(Track.LanceCharge, AID.LanceCharge, "Lance Charge", "L.Charge", uiPriority: 170, 60, 20, ActionTargets.Self, 30);
        res.DefineOGCD(Track.BattleLitany, AID.BattleLitany, "Battle Litany", "B.Litany", uiPriority: 165, 120, 20, ActionTargets.Self, 52);
        res.DefineOGCD(Track.MirageDive, AID.MirageDive, "Mirage Dive", "M.Dive", uiPriority: 130, 0, 0, ActionTargets.Hostile, 68);
        res.DefineOGCD(Track.Nastrond, AID.Nastrond, "Nastrond", "Nast.", uiPriority: 135, 0, 0, ActionTargets.Hostile, 70);
        res.DefineOGCD(Track.WyrmwindThrust, AID.WyrmwindThrust, "Wyrmwind Thrust", "W.Thrust", uiPriority: 141, 0, 10, ActionTargets.Hostile, 90);
        res.DefineOGCD(Track.RiseOfTheDragon, AID.RiseOfTheDragon, "Rise Of The Dragon", "RotD", uiPriority: 150, 0, 0, ActionTargets.Hostile, 92);
        res.DefineOGCD(Track.Starcross, AID.Starcross, "Starcross", "S.cross", uiPriority: 140, 0, 0, ActionTargets.Hostile, 100);

        return res;
    }
    #endregion

    #region Module Variables
    private bool hasLOTD;
    private bool hasLC;
    private bool hasBL;
    private bool hasMD;
    private bool hasDF;
    private bool hasSC;
    private bool hasNastrond;
    private bool canLC;
    private bool canBL;
    private bool canLS;
    private bool canJump;
    private bool canDD;
    private bool canGeirskogul;
    private bool canMD;
    private bool canNastrond;
    private bool canSD;
    private bool canWT;
    private bool canROTD;
    private bool canSC;
    private float blCD;
    private float lcLeft;
    private float lcCD;
    private float powerLeft;
    private float chaosLeft;
    private int focusCount;
    private int NumAOETargets;
    private int NumSpearTargets;
    private int NumDiveTargets;
    private bool ShouldUseAOE;
    private bool ShouldUseSpears;
    private bool ShouldUseDives;
    private bool ShouldUseDOT;
    private Enemy? BestAOETargets;
    private Enemy? BestSpearTargets;
    private Enemy? BestDiveTargets;
    private Enemy? BestDOTTargets;
    private Enemy? BestAOETarget;
    private Enemy? BestSpearTarget;
    private Enemy? BestDiveTarget;
    private Enemy? BestDOTTarget;
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.CoerthanTorment => AutoBreak,
        AID.SonicThrust => !Unlocked(AID.CoerthanTorment) ? AutoBreak : FullAOE,
        AID.DoomSpike or AID.DraconianFury => !Unlocked(AID.SonicThrust) ? AutoBreak : FullAOE,
        AID.Drakesbane => AutoBreak,
        AID.WheelingThrust or AID.FangAndClaw => !Unlocked(AID.Drakesbane) ? AutoBreak : FullST,
        AID.FullThrust or AID.HeavensThrust => !Unlocked(AID.FangAndClaw) ? AutoBreak : FullST,
        AID.ChaosThrust or AID.ChaoticSpring => !Unlocked(AID.WheelingThrust) ? AutoBreak : FullST,
        AID.VorpalThrust or AID.LanceBarrage => !Unlocked(AID.FullThrust) ? AutoBreak : FullST,
        AID.Disembowel or AID.SpiralBlow => !Unlocked(AID.ChaosThrust) ? AutoBreak : FullST,
        AID.TrueThrust or AID.RaidenThrust => !Unlocked(AID.VorpalThrust) ? AutoBreak : FullST,
        _ => AutoBreak
    };
    private AID AutoBreak => ShouldUseAOE && powerLeft > SkSGCDLength * 2 ? FullAOE : ShouldUseDOT ? STBuffs : FullST;
    private AID FullST => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.Disembowel) && (Unlocked(AID.ChaosThrust) ? (powerLeft <= SkSGCDLength * 6 || chaosLeft <= SkSGCDLength * 4) : (Unlocked(AID.FullThrust) ? powerLeft <= SkSGCDLength * 3 : powerLeft <= SkSGCDLength * 2)) ? Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : AID.Disembowel : Unlocked(AID.LanceBarrage) ? AID.LanceBarrage : Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust,
        AID.Disembowel or AID.SpiralBlow => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust : AID.TrueThrust,
        AID.VorpalThrust or AID.LanceBarrage => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust,
        AID.FullThrust or AID.HeavensThrust => Unlocked(AID.FangAndClaw) ? AID.FangAndClaw : AID.TrueThrust,
        AID.ChaosThrust or AID.ChaoticSpring => Unlocked(AID.WheelingThrust) ? AID.WheelingThrust : AID.TrueThrust,
        AID.WheelingThrust or AID.FangAndClaw => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust,
        _ => PlayerHasEffect(SID.DraconianFire) ? AID.RaidenThrust : AID.TrueThrust,
    };
    private AID STNormal => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.LanceBarrage) ? AID.LanceBarrage : Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust,
        AID.VorpalThrust or AID.LanceBarrage => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust,
        AID.FullThrust or AID.HeavensThrust => Unlocked(AID.FangAndClaw) ? AID.FangAndClaw : AID.TrueThrust,
        AID.WheelingThrust or AID.FangAndClaw => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust,
        _ => PlayerHasEffect(SID.DraconianFire) ? AID.RaidenThrust : AID.TrueThrust,
    };
    private AID STBuffs => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.Disembowel) ? (Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : AID.Disembowel) : AID.TrueThrust,
        AID.Disembowel or AID.SpiralBlow => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust : AID.TrueThrust,
        AID.ChaosThrust or AID.ChaoticSpring => Unlocked(AID.WheelingThrust) ? AID.WheelingThrust : AID.TrueThrust,
        AID.WheelingThrust or AID.FangAndClaw => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust,
        _ => PlayerHasEffect(SID.DraconianFire) ? AID.RaidenThrust : AID.TrueThrust,
    };
    private AID FullAOE => ComboLastMove switch
    {
        AID.DoomSpike => Unlocked(AID.SonicThrust) ? AID.SonicThrust : LowLevelAOE,
        AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : LowLevelAOE,
        _ => PlayerHasEffect(SID.DraconianFire) ? AID.DraconianFury : LowLevelAOE,
    };

    private AID LowLevelAOE => ComboLastMove switch
    {
        AID.Disembowel or AID.SpiralBlow => powerLeft > SkSGCDLength * 2 ? AID.DoomSpike : AID.TrueThrust,
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : AID.Disembowel,
        _ => powerLeft > SkSGCDLength * 2 ? (PlayerHasEffect(SID.DraconianFire) ? AID.DraconianFury : AID.DoomSpike) : (PlayerHasEffect(SID.DraconianFire) ? AID.RaidenThrust : AID.TrueThrust),
    };

    #region DOT
    private static SID[] GetDotStatus() => [SID.ChaosThrust, SID.ChaoticSpring];
    private float ChaosRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 6);
    #endregion

    #endregion

    #region Cooldown Helpers

    #region Buffs
    private bool ShouldUseLanceCharge(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => InsideCombatWith(target) && canLC && powerLeft > 0,
        OGCDStrategy.Force => canLC,
        OGCDStrategy.AnyWeave => canLC && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canLC && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canLC && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBattleLitany(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => InsideCombatWith(target) && canBL && powerLeft > 0,
        OGCDStrategy.Force => canBL,
        OGCDStrategy.AnyWeave => canBL && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canBL && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canBL && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseLifeSurge(SurgeStrategy strategy, Actor? target)
    {
        if (!canLS)
            return false;

        var lv6to17 = ComboLastMove is AID.TrueThrust;
        var lv18to25 = !Unlocked(AID.FullThrust) && (Unlocked(AID.Disembowel) ? (lv6to17 && powerLeft > SkSGCDLength * 2) : lv6to17);
        var lv26to88 = (Unlocked(AID.FullThrust) && ComboLastMove is AID.VorpalThrust or AID.LanceBarrage) || (Unlocked(AID.Drakesbane) && ComboLastMove is AID.WheelingThrust or AID.FangAndClaw);
        var lv88plus = hasLC && (TotalCD(AID.LifeSurge) < 40 || TotalCD(AID.BattleLitany) > 50) && lv26to88;
        var condition = Unlocked(TraitID.EnhancedLifeSurge) ? lv88plus : (lv26to88 || lv18to25);
        var aoe = Unlocked(AID.CoerthanTorment) ? ComboLastMove is AID.SonicThrust : Unlocked(AID.SonicThrust) ? ComboLastMove is AID.DoomSpike : Unlocked(AID.DoomSpike) && powerLeft > SkSGCDLength * 2;
        return strategy switch
        {
            SurgeStrategy.Automatic => InsideCombatWith(target) && (ShouldUseAOE ? aoe : condition),
            SurgeStrategy.Force => true,
            SurgeStrategy.ForceWeave => CanWeaveIn,
            SurgeStrategy.ForceNextOpti => lv26to88,
            SurgeStrategy.ForceNextOptiWeave => lv26to88 && CanWeaveIn,
            _ => false
        };
    }
    #endregion

    #region Dives
    private bool ShouldUseDragonfireDive(DragonfireStrategy strategy, Actor? target)
    {
        if (!canDD)
            return false;

        var lv60plus = hasLC && hasBL && hasLOTD;
        var lv52to59 = hasLC && hasBL;
        var lv50to51 = hasLC;
        var condition = Unlocked(AID.Geirskogul) ? lv60plus : Unlocked(AID.BattleLitany) ? lv52to59 : lv50to51;
        return strategy switch
        {
            DragonfireStrategy.Automatic => InsideCombatWith(target) && In20y(target) && condition,
            DragonfireStrategy.Force => true,
            DragonfireStrategy.ForceEX => true,
            DragonfireStrategy.ForceWeave => CanWeaveIn,
            _ => false
        };
    }
    private bool ShouldUseJump(JumpStrategy strategy, Actor? target) => strategy switch
    {
        JumpStrategy.Automatic => InsideCombatWith(target) && In20y(target) && canJump && (lcLeft > 0 || hasLC || lcCD is < 35 and > 17),
        JumpStrategy.ForceEX => canJump,
        JumpStrategy.ForceEX2 => canJump,
        JumpStrategy.ForceWeave => canJump && CanWeaveIn,
        JumpStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseStardiver(StardiverStrategy strategy, Actor? target) => strategy switch
    {
        StardiverStrategy.Automatic => InsideCombatWith(target) && In20y(target) && canSD && hasLOTD,
        StardiverStrategy.Force => canSD,
        StardiverStrategy.ForceEX => canSD,
        StardiverStrategy.ForceWeave => canSD && CanWeaveIn,
        StardiverStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseMirageDive(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => InsideCombatWith(target) && In20y(target) && canMD,
        OGCDStrategy.Force => canMD,
        OGCDStrategy.AnyWeave => canMD && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canMD && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canMD && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    #endregion

    #region Spears
    private bool ShouldUseGeirskogul(GeirskogulStrategy strategy, Actor? target) => strategy switch
    {
        GeirskogulStrategy.Automatic => InsideCombatWith(target) && In15y(target) && canGeirskogul && ((InOddWindow(AID.BattleLitany) && hasLC) || (!InOddWindow(AID.BattleLitany) && hasLC && hasBL)),
        GeirskogulStrategy.Force or GeirskogulStrategy.ForceEX => canGeirskogul,
        GeirskogulStrategy.ForceWeave => canGeirskogul && CanWeaveIn,
        GeirskogulStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseNastrond(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => InsideCombatWith(target) && In15y(target) && canNastrond,
        OGCDStrategy.Force => canNastrond,
        OGCDStrategy.AnyWeave => canNastrond && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canNastrond && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canNastrond && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseWyrmwindThrust(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => InsideCombatWith(target) && In15y(target) && canWT && lcCD > SkSGCDLength * 2,
        OGCDStrategy.Force => canWT,
        OGCDStrategy.AnyWeave => canWT && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canWT && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canWT && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    #endregion

    private bool ShouldUseRiseOfTheDragon(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => InsideCombatWith(target) && In20y(target) && canROTD,
        OGCDStrategy.Force => canROTD,
        OGCDStrategy.AnyWeave => canROTD && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canROTD && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canROTD && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseStarcross(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => InsideCombatWith(target) && In20y(target) && canSC,
        OGCDStrategy.Force => canSC,
        OGCDStrategy.AnyWeave => canSC && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canSC && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canSC && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUsePiercingTalon(Actor? target, PiercingTalonStrategy strategy) => strategy switch
    {
        PiercingTalonStrategy.AllowEX => InsideCombatWith(target) && !In3y(target) && PlayerHasEffect(SID.EnhancedPiercingTalon),
        PiercingTalonStrategy.Allow => InsideCombatWith(target) && !In3y(target),
        PiercingTalonStrategy.Force => true,
        PiercingTalonStrategy.ForceEX => PlayerHasEffect(SID.EnhancedPiercingTalon),
        PiercingTalonStrategy.Forbid => false,
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && lcCD <= GCD * 2 && blCD <= GCD * 2,
        PotionStrategy.Immediate => true,
        _ => false
    };
    private bool ShouldUseTrueNorth(TrueNorthStrategy strategy, Actor? target)
    {
        var condition = target != null && Player.InCombat && CanTrueNorth;
        var needRear = !IsOnRear(target!) && ((Unlocked(AID.ChaosThrust) && ComboLastMove is AID.Disembowel or AID.SpiralBlow) || (Unlocked(AID.WheelingThrust) && ComboLastMove is AID.ChaosThrust or AID.ChaoticSpring));
        var needFlank = !IsOnFlank(target!) && Unlocked(AID.FangAndClaw) && ComboLastMove is AID.HeavensThrust or AID.FullThrust;
        return strategy switch
        {
            TrueNorthStrategy.Automatic => condition && CanLateWeaveIn && (needRear || needFlank),
            TrueNorthStrategy.ASAP => condition && (needRear || needFlank),
            TrueNorthStrategy.Flank => condition && CanLateWeaveIn && needFlank,
            TrueNorthStrategy.Rear => condition && CanLateWeaveIn && needRear,
            TrueNorthStrategy.Force => !PlayerHasEffect(SID.TrueNorth),
            TrueNorthStrategy.Delay => false,
            _ => false
        };
    }
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<DragoonGauge>();
        focusCount = gauge.FirstmindsFocusCount;
        hasLOTD = gauge.LotdTimer > 0;
        blCD = TotalCD(AID.BattleLitany);
        lcCD = TotalCD(AID.LanceCharge);
        lcLeft = SelfStatusLeft(SID.LanceCharge, 20);
        powerLeft = SelfStatusLeft(SID.PowerSurge, 30);
        chaosLeft = MathF.Max(StatusDetails(primaryTarget?.Actor, SID.ChaosThrust, Player.InstanceID).Left, StatusDetails(primaryTarget?.Actor, SID.ChaoticSpring, Player.InstanceID).Left);
        hasMD = PlayerHasEffect(SID.DiveReady);
        hasNastrond = PlayerHasEffect(SID.NastrondReady);
        hasLC = lcCD is >= 40 and <= 60;
        hasBL = blCD is >= 100 and <= 120;
        hasDF = PlayerHasEffect(SID.DragonsFlight);
        hasSC = PlayerHasEffect(SID.StarcrossReady);
        canLC = ActionReady(AID.LanceCharge);
        canBL = ActionReady(AID.BattleLitany);
        canLS = Unlocked(AID.LifeSurge) && !PlayerHasEffect(SID.LifeSurge) && (Unlocked(TraitID.EnhancedLifeSurge) ? TotalCD(AID.LifeSurge) < 40.6f : ChargeCD(AID.LifeSurge) < 0.6f);
        canJump = ActionReady(AID.Jump);
        canDD = ActionReady(AID.DragonfireDive);
        canGeirskogul = ActionReady(AID.Geirskogul);
        canMD = Unlocked(AID.MirageDive) && hasMD;
        canNastrond = Unlocked(AID.Nastrond) && hasNastrond;
        canSD = ActionReady(AID.Stardiver);
        canWT = ActionReady(AID.WyrmwindThrust) && focusCount == 2;
        canROTD = Unlocked(AID.RiseOfTheDragon) && hasDF;
        canSC = Unlocked(AID.Starcross) && hasSC;
        ShouldUseAOE = Unlocked(AID.DoomSpike) && NumAOETargets > 2 && powerLeft > SkSGCDLength * 2;
        ShouldUseSpears = Unlocked(AID.Geirskogul) && NumSpearTargets > 1;
        ShouldUseDives = Unlocked(AID.Stardiver) && NumDiveTargets > 1;
        ShouldUseDOT = Unlocked(AID.ChaosThrust) && Hints.NumPriorityTargetsInAOECircle(Player.Position, 3.5f) == 2 && In3y(BestDOTTarget?.Actor) && ComboLastMove is AID.Disembowel or AID.SpiralBlow;
        (BestAOETargets, NumAOETargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        (BestSpearTargets, NumSpearTargets) = GetBestTarget(primaryTarget, 15, Is15yRectTarget);
        (BestDiveTargets, NumDiveTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        (BestDOTTargets, chaosLeft) = GetDOTTarget(primaryTarget, ChaosRemaining, 2, 3.5f);
        BestAOETarget = ShouldUseAOE ? BestAOETargets : BestDOTTarget;
        BestSpearTarget = ShouldUseSpears ? BestSpearTargets : primaryTarget;
        BestDiveTarget = ShouldUseDives ? BestDiveTargets : primaryTarget;
        BestDOTTarget = ShouldUseDOT ? BestDOTTargets : primaryTarget;

        #region Strategy Definitions
        var hold = strategy.Option(Track.Hold).As<HoldStrategy>() == HoldStrategy.Forbid;
        var AOE = strategy.Option(Track.AOE);
        var AOEStrategy = AOE.As<AOEStrategy>();
        var dive = strategy.Option(Track.Dives).As<DivesStrategy>();
        var lc = strategy.Option(Track.LanceCharge);
        var lcStrat = lc.As<OGCDStrategy>();
        var bl = strategy.Option(Track.BattleLitany);
        var blStrat = bl.As<OGCDStrategy>();
        var ls = strategy.Option(Track.LifeSurge);
        var lsStrat = ls.As<SurgeStrategy>();
        var jump = strategy.Option(Track.Jump);
        var jumpStrat = jump.As<JumpStrategy>();
        var dd = strategy.Option(Track.DragonfireDive);
        var ddStrat = dd.As<DragonfireStrategy>();
        var geirskogul = strategy.Option(Track.Geirskogul);
        var geirskogulStrat = geirskogul.As<GeirskogulStrategy>();
        var sd = strategy.Option(Track.Stardiver);
        var sdStrat = sd.As<StardiverStrategy>();
        var wt = strategy.Option(Track.WyrmwindThrust);
        var wtStrat = wt.As<OGCDStrategy>();
        var rotd = strategy.Option(Track.RiseOfTheDragon);
        var rotdStrat = rotd.As<OGCDStrategy>();
        var sc = strategy.Option(Track.Starcross);
        var scStrat = sc.As<OGCDStrategy>();
        var md = strategy.Option(Track.MirageDive);
        var mdStrat = md.As<OGCDStrategy>();
        var nastrond = strategy.Option(Track.Nastrond);
        var nastrondStrat = nastrond.As<OGCDStrategy>();
        var pt = strategy.Option(Track.PiercingTalon);
        var ptStrat = pt.As<PiercingTalonStrategy>();
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Dives
        var diveStrategy = dive switch
        {
            DivesStrategy.AllowMaxMelee => In3y(BestDiveTarget?.Actor),
            DivesStrategy.AllowCloseMelee => In0y(BestDiveTarget?.Actor),
            DivesStrategy.Allow => In20y(BestDiveTarget?.Actor),
            DivesStrategy.Forbid => false,
            _ => false,
        };

        var maxMelee = dive == DivesStrategy.AllowMaxMelee;
        var closeMelee = dive == DivesStrategy.AllowCloseMelee;
        var allowed = dive == DivesStrategy.Allow;
        var forbidden = dive == DivesStrategy.Forbid;
        var divesGood = diveStrategy && (maxMelee || closeMelee || allowed) && !forbidden;
        #endregion

        #region Standard Rotations
        if (AOEStrategy is AOEStrategy.AutoFinish)
            QueueGCD(AutoFinish, TargetChoice(AOE) ?? BestAOETarget?.Actor, GCDPriority.Low);
        if (AOEStrategy is AOEStrategy.AutoBreak)
            QueueGCD(AutoBreak, TargetChoice(AOE) ?? BestAOETarget?.Actor, GCDPriority.Low);
        if (AOEStrategy == AOEStrategy.ForceST)
            QueueGCD(FullST, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.High);
        if (AOEStrategy == AOEStrategy.Force123ST)
            QueueGCD(STNormal, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.High);
        if (AOEStrategy == AOEStrategy.ForceBuffsST)
            QueueGCD(STBuffs, TargetChoice(AOE) ?? BestDOTTarget?.Actor, GCDPriority.High);
        if (AOEStrategy == AOEStrategy.ForceAOE)
            QueueGCD(FullAOE, TargetChoice(AOE) ?? (NumAOETargets > 1 ? BestAOETargets?.Actor : primaryTarget?.Actor), GCDPriority.High);
        #endregion

        #region Cooldowns
        if (!hold)
        {
            if (divesGood)
            {
                if (ShouldUseJump(jumpStrat, primaryTarget?.Actor))
                    QueueOGCD(Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump, TargetChoice(jump) ?? primaryTarget?.Actor, jumpStrat is JumpStrategy.Force or JumpStrategy.ForceEX or JumpStrategy.ForceEX2 or JumpStrategy.ForceWeave ? OGCDPriority.Forced : OGCDPriority.SlightlyHigh);
                if (ShouldUseDragonfireDive(ddStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.DragonfireDive, TargetChoice(dd) ?? BestDiveTarget?.Actor, ddStrat is DragonfireStrategy.Force or DragonfireStrategy.ForceWeave ? OGCDPriority.Forced : OGCDPriority.High);
                if (ShouldUseStardiver(sdStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.Stardiver, TargetChoice(sd) ?? BestDiveTarget?.Actor, sdStrat is StardiverStrategy.Force or StardiverStrategy.ForceEX or StardiverStrategy.ForceWeave ? OGCDPriority.Forced : OGCDPriority.Low);
            }
            if (ShouldUseLanceCharge(lcStrat, primaryTarget?.Actor))
                QueueOGCD(AID.LanceCharge, Player, OGCDPrio(lcStrat, OGCDPriority.VerySevere));
            if (ShouldUseBattleLitany(blStrat, primaryTarget?.Actor))
                QueueOGCD(AID.BattleLitany, Player, OGCDPrio(blStrat, OGCDPriority.VerySevere));
            if (ShouldUseLifeSurge(lsStrat, primaryTarget?.Actor))
                QueueOGCD(AID.LifeSurge, Player, lsStrat is SurgeStrategy.Force or SurgeStrategy.ForceWeave or SurgeStrategy.ForceNextOpti or SurgeStrategy.ForceNextOptiWeave ? OGCDPriority.Forced : OGCDPriority.Severe);
            if (ShouldUseGeirskogul(geirskogulStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Geirskogul, TargetChoice(geirskogul) ?? BestSpearTarget?.Actor, geirskogulStrat is GeirskogulStrategy.Force or GeirskogulStrategy.ForceEX or GeirskogulStrategy.ForceWeave ? OGCDPriority.Forced : OGCDPriority.ExtremelyHigh);
            if (ShouldUseMirageDive(mdStrat, primaryTarget?.Actor))
                QueueOGCD(AID.MirageDive, TargetChoice(md) ?? primaryTarget?.Actor, OGCDPrio(mdStrat, OGCDPriority.ExtremelyLow));
            if (ShouldUseNastrond(nastrondStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Nastrond, TargetChoice(nastrond) ?? BestSpearTarget?.Actor, OGCDPrio(nastrondStrat, OGCDPriority.VeryLow));
            if (ShouldUseWyrmwindThrust(wtStrat, primaryTarget?.Actor))
                QueueOGCD(AID.WyrmwindThrust, TargetChoice(wt) ?? BestSpearTarget?.Actor, wtStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave ? OGCDPriority.Forced : PlayerHasEffect(SID.LanceCharge) ? OGCDPriority.ModeratelyHigh : OGCDPriority.Average);
            if (ShouldUseRiseOfTheDragon(rotdStrat, primaryTarget?.Actor))
                QueueOGCD(AID.RiseOfTheDragon, TargetChoice(rotd) ?? BestDiveTarget?.Actor, OGCDPrio(rotdStrat, OGCDPriority.BelowAverage));
            if (ShouldUseStarcross(scStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Starcross, TargetChoice(sc) ?? BestDiveTarget?.Actor, OGCDPrio(scStrat, OGCDPriority.BelowAverage));
            if (ShouldUseTrueNorth(strategy.Option(Track.TrueNorth).As<TrueNorthStrategy>(), primaryTarget?.Actor))
                QueueOGCD(AID.TrueNorth, Player, OGCDPriority.AboveAverage);
        }
        if (Player.Level < 60 && ActionReady(AID.LegSweep))
            QueueOGCD(AID.LegSweep, TargetChoice(AOE) ?? primaryTarget?.Actor, OGCDPrio(mdStrat, OGCDPriority.ExtremelyLow));
        if (ShouldUsePiercingTalon(primaryTarget?.Actor, ptStrat))
            QueueGCD(AID.PiercingTalon, TargetChoice(pt) ?? primaryTarget?.Actor, ptStrat is PiercingTalonStrategy.Force or PiercingTalonStrategy.ForceEX ? GCDPriority.Forced : GCDPriority.SlightlyLow);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, 0, GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        var goalST = primaryTarget?.Actor != null ? Hints.GoalSingleTarget(primaryTarget!.Actor, 3) : null;
        var goalAOE = primaryTarget?.Actor != null ? Hints.GoalAOECone(primaryTarget!.Actor, 10, 45.Degrees()) : null;
        var goal = AOEStrategy switch
        {
            AOEStrategy.ForceST => goalST,
            AOEStrategy.Force123ST => goalST,
            AOEStrategy.ForceBuffsST => goalST,
            AOEStrategy.ForceAOE => goalAOE,
            _ => goalST != null && goalAOE != null ? Hints.GoalCombined(goalST, goalAOE, 3) : goalAOE
        };
        if (goal != null)
            Hints.GoalZones.Add(goal);
        #endregion
    }
}
