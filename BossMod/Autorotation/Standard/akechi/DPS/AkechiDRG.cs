#region Dependencies
using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.DRG;
#endregion

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiDRG(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Combo = SharedTrack.Count, Dives, Potion, ElusiveJump, LanceCharge, BattleLitany, LifeSurge, PiercingTalon, TrueNorth, DragonfireDive, Geirskogul, Stardiver, Jump, MirageDive, Nastrond, WyrmwindThrust, RiseOfTheDragon, Starcross }
    public enum SingleTargetOption { FullST, Force123ST, ForceBuffsST }
    public enum DivesStrategy { AllowMaxMelee, AllowCloseMelee, Allow, Forbid }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    public enum SurgeStrategy { Automatic, WhenBuffed, Force, ForceWeave, ForceNextOpti, ForceNextOptiWeave, Delay }
    public enum PiercingTalonStrategy { AllowEX, Allow, Force, ForceEX, Forbid }
    public enum TrueNorthStrategy { Automatic, ASAP, Rear, Flank, Force, Delay }
    public enum ElusiveDirection { None, CharacterForward, CharacterBackward, CameraForward, CameraBackward }
    public enum BuffsStrategy { Automatic, Together, Force, ForceWeave, Delay }
    public enum CommonStrategy { Automatic, Force, ForceEX, ForceWeave, ForceWeaveEX, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRG", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Excellent, BitMask.Build(Class.LNC, Class.DRG), 100);

        res.DefineAOE().AddAssociatedActions(
            AID.TrueThrust, AID.RaidenThrust, AID.DoomSpike, AID.DraconianFury,
            AID.VorpalThrust, AID.LanceBarrage, AID.Disembowel, AID.SpiralBlow, AID.SonicThrust,
            AID.FullThrust, AID.HeavensThrust, AID.ChaosThrust, AID.ChaoticSpring,
            AID.WheelingThrust, AID.FangAndClaw,
            AID.Drakesbane, AID.CoerthanTorment);
        res.DefineHold();
        res.Define(Track.Combo).As<SingleTargetOption>("Combo", uiPriority: 200)
            .AddOption(SingleTargetOption.FullST, "FullST", "Force full single target combo if 'Force Single-Target' option in is selected")
            .AddOption(SingleTargetOption.Force123ST, "Force Normal ST", "Force normal single target combo if 'Force Single-Target' option is selected")
            .AddOption(SingleTargetOption.ForceBuffsST, "Force Buffs ST", "Force single-target buff combo if 'Force Single-Target' option is selected");
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
        res.Define(Track.ElusiveJump).As<ElusiveDirection>("Elusive Jump", uiPriority: -1)
            .AddOption(ElusiveDirection.None, "None", "Do not use Elusive Jump")
            .AddOption(ElusiveDirection.CharacterForward, "Character Forward", "Jump into the direction forward of the character", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CharacterBackward, "Character Backward", "Jump into the direction backward of the character (default)", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CameraForward, "Camera Forward", "Jump into the direction forward of the camera", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CameraBackward, "Camera Backward", "Jump into the direction backward of the camera", 30, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(AID.ElusiveJump);
        res.Define(Track.LanceCharge).As<BuffsStrategy>("Lance Charge", "L. Charge", uiPriority: 170)
            .AddOption(BuffsStrategy.Automatic, "Automatic", "Use Lance Charge normally")
            .AddOption(BuffsStrategy.Together, "Together", "Use Lance Charge only with Battle Litany; will delay in attempt to align itself with Battle Litany (up to 30s)", 60, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.Force, "Force", "Force Lance Charge usage", 60, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.ForceWeave, "Force Weave", "Force Lance Charge usage inside the next possible weave window", 60, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.Delay, "Delay", "Delay Lance Charge usage", 0, 0, ActionTargets.None, 52)
            .AddAssociatedActions(AID.LanceCharge);
        res.Define(Track.BattleLitany).As<BuffsStrategy>("BattleLitany", "B.Litany", uiPriority: 165)
            .AddOption(BuffsStrategy.Automatic, "Automatic", "Use Battle Litany normally")
            .AddOption(BuffsStrategy.Together, "Together", "Use Battle Litany only with Lance Charge; will delay in attempt to align itself with Lance Charge")
            .AddOption(BuffsStrategy.Force, "Force", "Force Battle Litany usage", 180, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.ForceWeave, "Force Weave", "Force Battle Litany usage inside the next possible weave window", 180, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.Delay, "Delay", "Delay Battle Litany usage", 0, 0, ActionTargets.None, 52)
            .AddAssociatedActions(AID.BattleLitany);
        res.Define(Track.LifeSurge).As<SurgeStrategy>("Life Surge", "L. Surge", uiPriority: 160)
            .AddOption(SurgeStrategy.Automatic, "Automatic", "Use Life Surge normally")
            .AddOption(SurgeStrategy.WhenBuffed, "When Buffed", "Attempts to use Life Surge when under any buffs - this may be wonky to use generally; mainly for rushing use when under raidbuff(s)", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.Force, "Force", "Force Life Surge usage", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceWeave, "Force Weave", "Force Life Surge usage inside the next possible weave window", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceNextOpti, "Force Optimally", "Force Life Surge usage in next possible optimal window", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceNextOptiWeave, "Force Weave Optimally", "Force Life Surge optimally inside the next possible weave window", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.Delay, "Delay", "Delay the use of Life Surge", 0, 0, ActionTargets.None, 6)
            .AddAssociatedActions(AID.LifeSurge);
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
        res.Define(Track.DragonfireDive).As<CommonStrategy>("Dragonfire Dive", "D.Dive", uiPriority: 155)
            .AddOption(CommonStrategy.Automatic, "Automatic", "Use Dragonfire Dive normally")
            .AddOption(CommonStrategy.Force, "Force", "Force Dragonfire Dive usage", 120, 0, ActionTargets.Hostile, 50, 91)
            .AddOption(CommonStrategy.ForceEX, "ForceEX", "Force Dragonfire Dive (Grants Dragon's Flight)", 120, 30, ActionTargets.Hostile, 92)
            .AddOption(CommonStrategy.ForceWeave, "Force Weave", "Force Dragonfire Dive usage inside the next possible weave window", 120, 0, ActionTargets.Hostile, 68)
            .AddOption(CommonStrategy.ForceWeaveEX, "Force Weave (EX)", "Force Dragonfire Dive usage inside the next possible weave window (Grants Dragon's Flight)", 120, 30, ActionTargets.Hostile, 92)
            .AddOption(CommonStrategy.Delay, "Delay", "Delay Dragonfire Dive usage", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(AID.DragonfireDive);
        res.Define(Track.Geirskogul).As<CommonStrategy>("Geirskogul", "Geirs.", uiPriority: 150)
            .AddOption(CommonStrategy.Automatic, "Automatic", "Use Geirskogul normally")
            .AddOption(CommonStrategy.Force, "Force", "Force Geirskogul usage", 60, 0, ActionTargets.Hostile, 60, 69)
            .AddOption(CommonStrategy.ForceEX, "ForceEX", "Force Geirskogul (Grants Life of the Dragon & Nastrond Ready)", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(CommonStrategy.ForceWeave, "Force Weave", "Force Geirskogul usage inside the next possible weave window", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(CommonStrategy.ForceWeaveEX, "Force Weave (EX)", "Force Geirskogul usage inside the next possible weave window (Grants Life of the Dragon & Nastrond Ready)", 60, 20, ActionTargets.Hostile, 70)
            .AddOption(CommonStrategy.Delay, "Delay", "Delay Geirskogul usage", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Geirskogul);
        res.Define(Track.Stardiver).As<CommonStrategy>("Stardiver", "S.diver", uiPriority: 140)
            .AddOption(CommonStrategy.Automatic, "Automatic", "Use Stardiver normally")
            .AddOption(CommonStrategy.Force, "Force", "Force Stardiver usage", 30, 0, ActionTargets.Hostile, 80, 99)
            .AddOption(CommonStrategy.ForceEX, "ForceEX", "Force Stardiver (Grants Starcross Ready)", 30, 0, ActionTargets.Hostile, 100)
            .AddOption(CommonStrategy.ForceWeave, "Force Weave", "Force Stardiver usage inside the next possible weave window", 30, 0, ActionTargets.Hostile, 80)
            .AddOption(CommonStrategy.ForceWeaveEX, "Force Weave (EX)", "Force Stardiver usage inside the next possible weave window (Grants Starcross Ready)", 30, 0, ActionTargets.Hostile, 100)
            .AddOption(CommonStrategy.Delay, "Delay", "Delay Stardiver usage", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Stardiver);
        res.Define(Track.Jump).As<CommonStrategy>("Jump", uiPriority: 145)
            .AddOption(CommonStrategy.Automatic, "Automatic", "Use Jump normally")
            .AddOption(CommonStrategy.Force, "Force Jump", "Force Jump usage", 30, 0, ActionTargets.Hostile, 30, 67)
            .AddOption(CommonStrategy.ForceEX, "Force Jump (EX)", "Force Jump usage (Grants Dive Ready buff)", 30, 15, ActionTargets.Hostile, 68)
            .AddOption(CommonStrategy.ForceWeave, "Force Weave", "Force Jump usage inside the next possible weave window", 30, 0, ActionTargets.Hostile, 30, 67)
            .AddOption(CommonStrategy.ForceWeaveEX, "Force Weave (EX)", "Force Jump usage inside the next possible weave window (Grants Dive Ready buff)", 30, 15, ActionTargets.Hostile, 68)
            .AddOption(CommonStrategy.Delay, "Delay", "Delay Jump usage", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.Jump, AID.HighJump);
        res.DefineOGCD(Track.MirageDive, AID.MirageDive, "Mirage Dive", "M.Dive", uiPriority: 130, 0, 0, ActionTargets.Hostile, 68);
        res.DefineOGCD(Track.Nastrond, AID.Nastrond, "Nastrond", "Nast.", uiPriority: 135, 0, 0, ActionTargets.Hostile, 70);
        res.DefineOGCD(Track.WyrmwindThrust, AID.WyrmwindThrust, "Wyrmwind Thrust", "W.Thrust", uiPriority: 141, 0, 10, ActionTargets.Hostile, 90);
        res.DefineOGCD(Track.RiseOfTheDragon, AID.RiseOfTheDragon, "Rise Of The Dragon", "RotD", uiPriority: 150, 0, 0, ActionTargets.Hostile, 92);
        res.DefineOGCD(Track.Starcross, AID.Starcross, "Starcross", "S.cross", uiPriority: 140, 0, 0, ActionTargets.Hostile, 100);

        return res;
    }
    #endregion

    #region Module Variables
    private int FirstmindsFocus; //DRG gauge (2 = 1 use of Wyrmwind Thrust)
    private bool HasPower; //Power Surge is active
    private bool HasLOTD; //Life of the Dragon is active
    private bool HasLC; //Lance Charge is active
    private bool HasBL; //Battle Litany is active
    private bool HasMD; //Dive Ready is active
    private bool HasROTD; //Dragon's Flight is active
    private bool HasSC; //Starcross Ready is active
    private bool HasNastrond; //Nastrond Ready is active
    private bool CanLC; //can use Lance Charge
    private bool CanBL; //can use Battle Litany
    private bool CanLS; //can use Life Surge
    private bool CanJump; //can use Jump
    private bool CanDD; //can use Dragonfire Dive
    private bool CanGeirskogul; //can use Geirskogul
    private bool CanMD; //can use Mirage Dive
    private bool CanNastrond; //can use Nastrond
    private bool CanSD; //can use Stardiver
    private bool CanWT; //can use Wyrmwind Thrust
    private bool CanROTD; //can use Rise of the Dragon
    private bool CanSC; //can use Starcross
    private float BLcd; //Battle Litany cooldown
    private float LCcd; //Lance Charge cooldown
    private float PowerLeft; //time left on Power Surge
    private float ChaosLeft; //time left on Chaos Thrust or Chaotic Spring
    private bool InsideRange; //inside range of target for ST (3.5y) or AOE (10y)
    private bool OutsideRange; //outside range of target for ST (3.5y) or AOE (10y)
    private bool NeedPower; //need to reapply Power Surge
    private int NumAOETargets; //number of targets in range for AOE
    private int NumSpearTargets; //number of targets in range for Spears
    private int NumDiveTargets; //number of targets in range for Dives
    private bool ShouldUseAOE; //should use AOE
    private bool ShouldUseSpears; //should use Spears
    private bool ShouldUseDives; //should use Dives
    private bool ShouldUseDOT; //should use DoT
    private Enemy? BestAOETarget; //best overall target for AOE
    private Enemy? BestSpearTarget; //best overall target for Spears
    private Enemy? BestDiveTarget; //best overall target for Dives
    private Enemy? BestDOTTarget; //best overall target for DoT
    private Enemy? BestAOETargets; //best targets for AOE
    private Enemy? BestSpearTargets; //best targets for Spears
    private Enemy? BestDiveTargets; //best targets for Dives
    private Enemy? BestDOTTargets; //best targets for DoT
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.SonicThrust => !Unlocked(AID.CoerthanTorment) ? AutoBreak : FullAOE,
        AID.DoomSpike or AID.DraconianFury => !Unlocked(AID.SonicThrust) ? AutoBreak : FullAOE,
        AID.WheelingThrust or AID.FangAndClaw => !Unlocked(AID.Drakesbane) ? AutoBreak : FullST,
        AID.FullThrust or AID.HeavensThrust => !Unlocked(AID.FangAndClaw) ? AutoBreak : FullST,
        AID.ChaosThrust or AID.ChaoticSpring => !Unlocked(AID.WheelingThrust) ? AutoBreak : FullST,
        AID.VorpalThrust or AID.LanceBarrage => !Unlocked(AID.FullThrust) ? AutoBreak : FullST,
        AID.Disembowel or AID.SpiralBlow => !Unlocked(AID.ChaosThrust) ? AutoBreak : FullST,
        AID.TrueThrust or AID.RaidenThrust => !Unlocked(AID.VorpalThrust) ? AutoBreak : FullST,
        AID.CoerthanTorment or AID.Drakesbane or _ => AutoBreak
    };
    private AID AutoBreak => ShouldUseAOE ? FullAOE : ShouldUseDOT ? BuffsST : FullST;
    private AID FullST => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.Disembowel) && (Unlocked(AID.ChaosThrust) ? (PowerLeft <= SkSGCDLength * 6 || ChaosLeft <= SkSGCDLength * 4) : (Unlocked(AID.FullThrust) ? PowerLeft <= SkSGCDLength * 3 : NeedPower)) ? BestDisembowel : Unlocked(AID.LanceBarrage) ? AID.LanceBarrage : Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust,
        AID.Disembowel or AID.SpiralBlow => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust : AID.TrueThrust,
        AID.VorpalThrust or AID.LanceBarrage => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust,
        AID.FullThrust or AID.HeavensThrust => Unlocked(AID.FangAndClaw) ? AID.FangAndClaw : AID.TrueThrust,
        AID.ChaosThrust or AID.ChaoticSpring => Unlocked(AID.WheelingThrust) ? AID.WheelingThrust : AID.TrueThrust,
        AID.WheelingThrust or AID.FangAndClaw => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust,
        _ => BestTrueThrust,
    };
    private AID NormalST => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.LanceBarrage) ? AID.LanceBarrage : Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust,
        AID.VorpalThrust or AID.LanceBarrage => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust,
        AID.FullThrust or AID.HeavensThrust => Unlocked(AID.FangAndClaw) ? AID.FangAndClaw : AID.TrueThrust,
        AID.WheelingThrust or AID.FangAndClaw => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust,
        _ => BestTrueThrust,
    };
    private AID BuffsST => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.Disembowel) ? (BestDisembowel) : AID.TrueThrust,
        AID.Disembowel or AID.SpiralBlow => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust : AID.TrueThrust,
        AID.ChaosThrust or AID.ChaoticSpring => Unlocked(AID.WheelingThrust) ? AID.WheelingThrust : AID.TrueThrust,
        AID.WheelingThrust or AID.FangAndClaw => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust,
        _ => BestTrueThrust,
    };
    private AID FullAOE => ComboLastMove switch
    {
        AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : BestDoomSpike,
        AID.DoomSpike or AID.DraconianFury => Unlocked(AID.SonicThrust) ? AID.SonicThrust : LowLevelAOE,
        _ => Unlocked(AID.SonicThrust) ? BestDoomSpike : LowLevelAOE,
    };
    private AID LowLevelAOE => ComboLastMove switch
    {
        AID.Disembowel or AID.SpiralBlow => Unlocked(AID.SonicThrust) ? BestDoomSpike : (NeedPower ? AID.TrueThrust : AID.DoomSpike),
        AID.TrueThrust or AID.RaidenThrust => Unlocked(AID.SonicThrust) ? BestDoomSpike : BestDisembowel,
        _ => Unlocked(AID.SonicThrust) ? BestDoomSpike : (NeedPower ? AID.TrueThrust : AID.DoomSpike),
    };

    #region Upgrade Paths
    private AID BestTrueThrust => PlayerHasEffect(SID.DraconianFire) ? AID.RaidenThrust : AID.TrueThrust;
    private AID BestDisembowel => Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : AID.Disembowel;
    private AID BestDoomSpike => PlayerHasEffect(SID.DraconianFire) ? AID.DraconianFury : Unlocked(AID.DoomSpike) ? AID.DoomSpike : FullST;
    #endregion

    #region DOT
    private static SID[] GetDotStatus() => [SID.ChaosThrust, SID.ChaoticSpring];
    private float ChaosRemaining(Actor? target) => target == null ? float.MaxValue : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 6);
    #endregion

    #region Positionals
    private (Positional, bool) GetBestPositional(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (NumAOETargets > 2 && Unlocked(AID.DoomSpike) ||
            !Unlocked(AID.ChaosThrust) ||
            primaryTarget == null)
            return (Positional.Any, false);

        if (!Unlocked(AID.FangAndClaw))
            return (Positional.Rear, ComboLastMove == AID.Disembowel);

        (Positional, bool) PredictNextPositional(int StepsBeforeReloop)
        {
            var buffed = CanFitSkSGCD(ChaosLeft, StepsBeforeReloop + 3) && CanFitSkSGCD(PowerLeft, StepsBeforeReloop + 2);
            return (buffed ? Positional.Flank : Positional.Rear, false);
        }

        return ComboLastMove switch
        {
            AID.ChaosThrust => Unlocked(AID.WheelingThrust) ? (Positional.Rear, true) : PredictNextPositional(0),
            AID.Disembowel or AID.SpiralBlow or AID.ChaoticSpring => (Positional.Rear, true),
            AID.TrueThrust or AID.RaidenThrust => PredictNextPositional(-1),
            AID.VorpalThrust or AID.LanceBarrage => (Positional.Flank, false),
            AID.HeavensThrust or AID.FullThrust => (Positional.Flank, true),
            AID.WheelingThrust or AID.FangAndClaw => PredictNextPositional(Unlocked(AID.Drakesbane) ? 1 : 0),
            _ => PredictNextPositional(0)
        };
    }
    #endregion

    #endregion

    #region Cooldown Helpers

    #region Buffs
    private bool ShouldUseLanceCharge(BuffsStrategy strategy, Actor? target)
    {
        if (!CanLC)
            return false;
        var condition = InsideCombatWith(target) && InsideRange && HasPower;
        return strategy switch
        {
            BuffsStrategy.Automatic => condition,
            BuffsStrategy.Together => condition && (BLcd > 30 || BLcd < 1),
            BuffsStrategy.Force => true,
            BuffsStrategy.ForceWeave => CanWeaveIn,
            BuffsStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUseBattleLitany(BuffsStrategy strategy, Actor? target)
    {
        if (!CanBL)
            return false;
        var condition = InsideCombatWith(target) && InsideRange && HasPower;
        return strategy switch
        {
            BuffsStrategy.Automatic => condition,
            BuffsStrategy.Together => condition && HasLC,
            BuffsStrategy.Force => true,
            BuffsStrategy.ForceWeave => CanWeaveIn,
            BuffsStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUseLifeSurge(SurgeStrategy strategy, Actor? target)
    {
        if (!CanLS)
            return false;
        var lv6to17 = ComboLastMove is AID.TrueThrust;
        var lv18to25 = !Unlocked(AID.FullThrust) && (Unlocked(AID.Disembowel) ? (lv6to17 && !NeedPower) : lv6to17);
        var lv26to88 = (Unlocked(AID.FullThrust) && ComboLastMove is AID.VorpalThrust or AID.LanceBarrage) || (Unlocked(AID.Drakesbane) && ComboLastMove is AID.WheelingThrust or AID.FangAndClaw);
        var lv88plus = HasLC && (TotalCD(AID.LifeSurge) < 40 || TotalCD(AID.BattleLitany) > 50) && lv26to88;
        var st = Unlocked(TraitID.EnhancedLifeSurge) ? lv88plus : (lv26to88 || lv18to25);
        var tt = (CanLC ? HasLC : CanLS) && (Unlocked(AID.ChaosThrust) ? (Unlocked(TraitID.EnhancedLifeSurge) ? ComboLastMove is AID.FangAndClaw or AID.WheelingThrust or AID.Drakesbane : ComboLastMove is AID.FangAndClaw or AID.WheelingThrust) : lv26to88);
        var aoe = Unlocked(AID.CoerthanTorment) ? ComboLastMove is AID.SonicThrust : Unlocked(AID.SonicThrust) ? ComboLastMove is AID.DoomSpike : Unlocked(AID.DoomSpike) && !NeedPower;
        var minimal = InsideCombatWith(target) && HasPower && InsideRange;
        var buffed = ((HasLC && HasLOTD) || HasBL) && (ShouldUseAOE ? aoe : lv26to88);
        return strategy switch
        {
            SurgeStrategy.Automatic => minimal && (ShouldUseAOE ? aoe : ShouldUseDOT ? tt : st),
            SurgeStrategy.WhenBuffed => minimal && buffed,
            SurgeStrategy.Force => true,
            SurgeStrategy.ForceWeave => CanWeaveIn,
            SurgeStrategy.ForceNextOpti => lv26to88,
            SurgeStrategy.ForceNextOptiWeave => lv26to88 && CanWeaveIn,
            _ => false
        };
    }
    #endregion

    #region Dives
    private bool ShouldUseDragonfireDive(CommonStrategy strategy, Actor? target)
    {
        if (!CanDD)
            return false;
        var lv60plus = HasLC && HasBL && HasLOTD;
        var lv52to59 = HasLC && HasBL;
        var lv50to51 = HasLC;
        var condition = Unlocked(AID.Geirskogul) ? lv60plus : Unlocked(AID.BattleLitany) ? lv52to59 : lv50to51;
        return strategy switch
        {
            CommonStrategy.Automatic => InsideCombatWith(target) && In20y(target) && condition,
            CommonStrategy.Force or CommonStrategy.ForceEX => true,
            CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX => CanWeaveIn,
            CommonStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUseJump(CommonStrategy strategy, Actor? target)
    {
        if (!CanJump)
            return false;
        return strategy switch
        {
            CommonStrategy.Automatic => InsideCombatWith(target) && In20y(target) && (HasLC || LCcd is < 35 and > 13),
            CommonStrategy.Force or CommonStrategy.ForceEX => true,
            CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX => CanWeaveIn,
            CommonStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUseStardiver(CommonStrategy strategy, Actor? target)
    {
        if (!CanSD)
            return false;
        return strategy switch
        {
            CommonStrategy.Automatic => InsideCombatWith(target) && In20y(target) && HasLOTD,
            CommonStrategy.ForceEX or CommonStrategy.ForceEX => true,
            CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX => CanWeaveIn,
            CommonStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUseMirageDive(OGCDStrategy strategy, Actor? target)
    {
        if (!CanMD)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In20y(target),
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Delay => false,
            _ => false
        };
    }
    #endregion

    #region Spears
    private bool ShouldUseGeirskogul(CommonStrategy strategy, Actor? target)
    {
        if (!CanGeirskogul)
            return false;
        return strategy switch
        {
            CommonStrategy.Automatic => InsideCombatWith(target) && In15y(target) && ((InOddWindow(AID.BattleLitany) && HasLC) || (!InOddWindow(AID.BattleLitany) && HasLC && HasBL)),
            CommonStrategy.ForceEX or CommonStrategy.ForceEX => true,
            CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX => CanWeaveIn,
            CommonStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUseNastrond(OGCDStrategy strategy, Actor? target)
    {
        if (!CanNastrond)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In15y(target),
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUseWyrmwindThrust(OGCDStrategy strategy, Actor? target)
    {
        if (!CanWT)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In15y(target) && LCcd > SkSGCDLength * 2,
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Delay => false,
            _ => false
        };
    }
    #endregion

    private bool ShouldUseRiseOfTheDragon(OGCDStrategy strategy, Actor? target)
    {
        if (!CanROTD)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In20y(target),
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Delay => false,
            _ => false
        };
    }
    private bool ShouldUseStarcross(OGCDStrategy strategy, Actor? target)
    {
        if (!CanSC)
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In20y(target),
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Delay => false,
            _ => false
        };
    }
    private void ShouldUseElusive(ElusiveDirection strategy, Actor? target)
    {
        if (!Unlocked(AID.ElusiveJump))
            return;

        if (ActionReady(AID.ElusiveJump))
        {
            if (strategy != ElusiveDirection.None)
            {
                var angle = strategy switch
                {
                    ElusiveDirection.CharacterForward => Player.Rotation,
                    ElusiveDirection.CameraForward => World.Client.CameraAzimuth,
                    ElusiveDirection.CameraBackward => World.Client.CameraAzimuth + 180.Degrees(),
                    _ => Player.Rotation + 180.Degrees()
                };
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(AID.ElusiveJump), Player, ActionQueue.Priority.Low, facingAngle: angle);
            }
        }
    }
    private bool ShouldUsePiercingTalon(Actor? target, PiercingTalonStrategy strategy)
    {
        if (!Unlocked(AID.PiercingTalon))
            return false;
        var allow = InsideCombatWith(target) && OutsideRange;
        return strategy switch
        {
            PiercingTalonStrategy.AllowEX => allow && PlayerHasEffect(SID.EnhancedPiercingTalon),
            PiercingTalonStrategy.Allow => allow,
            PiercingTalonStrategy.Force => true,
            PiercingTalonStrategy.ForceEX => PlayerHasEffect(SID.EnhancedPiercingTalon),
            PiercingTalonStrategy.Forbid => false,
            _ => false
        };

    }
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && LCcd <= SkSGCDLength * 2 && BLcd <= SkSGCDLength * 2, //attempts to align pots with when both buffs are about to be up
        PotionStrategy.Immediate => true, //force
        _ => false
    };
    private bool ShouldUseTrueNorth(TrueNorthStrategy strategy, Actor? target)
    {
        if (!CanTrueNorth)
            return false;
        var condition = InsideCombatWith(target) && !ShouldUseAOE && In3y(target) && NextPositionalImminent && !NextPositionalCorrect;
        var needRear = !IsOnRear(target!) && ((Unlocked(AID.ChaosThrust) && ComboLastMove is AID.Disembowel or AID.SpiralBlow) || (Unlocked(AID.WheelingThrust) && ComboLastMove is AID.ChaosThrust or AID.ChaoticSpring));
        var needFlank = !IsOnFlank(target!) && Unlocked(AID.FangAndClaw) && ComboLastMove is AID.HeavensThrust or AID.FullThrust;
        return strategy switch
        {
            TrueNorthStrategy.Automatic => condition && CanLateWeaveIn,
            TrueNorthStrategy.ASAP => condition,
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
        FirstmindsFocus = gauge.FirstmindsFocusCount;
        HasPower = PowerLeft > 0;
        HasLOTD = gauge.LotdTimer > 0;
        BLcd = TotalCD(AID.BattleLitany);
        LCcd = TotalCD(AID.LanceCharge);
        PowerLeft = StatusRemaining(Player, SID.PowerSurge, 30);
        ChaosLeft = MathF.Max(StatusDetails(primaryTarget?.Actor, SID.ChaosThrust, Player.InstanceID).Left, StatusDetails(primaryTarget?.Actor, SID.ChaoticSpring, Player.InstanceID).Left);
        HasMD = PlayerHasEffect(SID.DiveReady);
        HasNastrond = PlayerHasEffect(SID.NastrondReady);
        HasLC = LCcd is >= 40 and <= 60;
        HasBL = BLcd is >= 100 and <= 120;
        HasROTD = PlayerHasEffect(SID.DragonsFlight);
        HasSC = PlayerHasEffect(SID.StarcrossReady);
        CanLC = ActionReady(AID.LanceCharge);
        CanBL = ActionReady(AID.BattleLitany);
        CanLS = Unlocked(AID.LifeSurge) && !PlayerHasEffect(SID.LifeSurge) && (Unlocked(TraitID.EnhancedLifeSurge) ? TotalCD(AID.LifeSurge) < 40.6f : ChargeCD(AID.LifeSurge) < 0.6f);
        CanJump = ActionReady(AID.Jump);
        CanDD = ActionReady(AID.DragonfireDive);
        CanGeirskogul = ActionReady(AID.Geirskogul);
        CanMD = Unlocked(AID.MirageDive) && HasMD;
        CanNastrond = Unlocked(AID.Nastrond) && HasNastrond;
        CanSD = ActionReady(AID.Stardiver);
        CanWT = ActionReady(AID.WyrmwindThrust) && FirstmindsFocus == 2;
        CanROTD = Unlocked(AID.RiseOfTheDragon) && HasROTD;
        CanSC = Unlocked(AID.Starcross) && HasSC;
        NeedPower = PowerLeft <= SkSGCDLength * 2;
        ShouldUseAOE = Unlocked(AID.DoomSpike) && NumAOETargets > 2 && !NeedPower;
        ShouldUseSpears = Unlocked(AID.Geirskogul) && NumSpearTargets > 1;
        ShouldUseDives = Unlocked(AID.Stardiver) && NumDiveTargets > 1;
        ShouldUseDOT = Unlocked(AID.ChaosThrust) && Hints.NumPriorityTargetsInAOECircle(Player.Position, 4) == 2 && In3y(BestDOTTarget?.Actor) && ComboLastMove is AID.Disembowel or AID.SpiralBlow;
        (BestAOETargets, NumAOETargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        (BestSpearTargets, NumSpearTargets) = GetBestTarget(primaryTarget, 15, Is15yRectTarget);
        (BestDiveTargets, NumDiveTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        (BestDOTTargets, ChaosLeft) = GetDOTTarget(primaryTarget, ChaosRemaining, 2, 8);
        BestAOETarget = ShouldUseAOE ? BestAOETargets : BestDOTTarget;
        BestSpearTarget = ShouldUseSpears ? BestSpearTargets : primaryTarget;
        BestDiveTarget = ShouldUseDives ? BestDiveTargets : primaryTarget;
        BestDOTTarget = ShouldUseDOT ? BestDOTTargets : primaryTarget;
        InsideRange = ShouldUseAOE ? In10y(BestAOETarget?.Actor) : In3y(BestDOTTarget?.Actor);
        OutsideRange = ShouldUseAOE ? !In10y(BestAOETarget?.Actor) : !In3y(BestDOTTarget?.Actor);

        #region Strategy Definitions
        var AOE = strategy.Option(SharedTrack.AOE);
        var AOEStrategy = AOE.As<AOEStrategy>();
        var combo = strategy.Option(Track.Combo);
        var comboStrat = combo.As<SingleTargetOption>();
        var dive = strategy.Option(Track.Dives).As<DivesStrategy>();
        var lc = strategy.Option(Track.LanceCharge);
        var lcStrat = lc.As<BuffsStrategy>();
        var bl = strategy.Option(Track.BattleLitany);
        var blStrat = bl.As<BuffsStrategy>();
        var ls = strategy.Option(Track.LifeSurge);
        var lsStrat = ls.As<SurgeStrategy>();
        var jump = strategy.Option(Track.Jump);
        var jumpStrat = jump.As<CommonStrategy>();
        var dd = strategy.Option(Track.DragonfireDive);
        var ddStrat = dd.As<CommonStrategy>();
        var geirskogul = strategy.Option(Track.Geirskogul);
        var geirskogulStrat = geirskogul.As<CommonStrategy>();
        var sd = strategy.Option(Track.Stardiver);
        var sdStrat = sd.As<CommonStrategy>();
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
            DivesStrategy.AllowCloseMelee => InRange(BestDiveTarget?.Actor, 1),
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
        if (strategy.AutoFinish())
            QueueGCD(AutoFinish, TargetChoice(AOE) ?? BestAOETarget?.Actor, GCDPriority.Low);
        if (strategy.AutoBreak())
            QueueGCD(AutoBreak, TargetChoice(AOE) ?? BestAOETarget?.Actor, GCDPriority.Low);
        if (strategy.ForceST())
        {
            if (comboStrat == SingleTargetOption.FullST)
                QueueGCD(FullST, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.VeryHigh);
            if (comboStrat == SingleTargetOption.Force123ST)
                QueueGCD(NormalST, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.VeryHigh);
            if (comboStrat == SingleTargetOption.ForceBuffsST)
                QueueGCD(BuffsST, TargetChoice(AOE) ?? BestDOTTarget?.Actor, GCDPriority.VeryHigh);
        }
        if (strategy.ForceAOE())
            QueueGCD(FullAOE, TargetChoice(AOE) ?? (NumAOETargets > 1 ? BestAOETargets?.Actor : primaryTarget?.Actor), GCDPriority.High);
        #endregion

        #region Cooldowns
        if (!strategy.HoldAll())
        {
            if (!strategy.HoldCDs())
            {
                if (!strategy.HoldBuffs())
                {
                    if (ShouldUseLanceCharge(lcStrat, primaryTarget?.Actor))
                        QueueOGCD(AID.LanceCharge, Player, blStrat is BuffsStrategy.Force or BuffsStrategy.ForceWeave ? OGCDPriority.Forced : OGCDPriority.VerySevere);
                    if (ShouldUseBattleLitany(blStrat, primaryTarget?.Actor))
                        QueueOGCD(AID.BattleLitany, Player, blStrat is BuffsStrategy.Force or BuffsStrategy.ForceWeave ? OGCDPriority.Forced : OGCDPriority.Severe);
                    if (ShouldUseLifeSurge(lsStrat, primaryTarget?.Actor))
                        QueueOGCD(AID.LifeSurge, Player, lsStrat is SurgeStrategy.Force or SurgeStrategy.ForceWeave or SurgeStrategy.ForceNextOpti or SurgeStrategy.ForceNextOptiWeave ? OGCDPriority.Forced : OGCDPriority.ExtremelyHigh);
                }
                if (divesGood)
                {
                    if (ShouldUseJump(jumpStrat, primaryTarget?.Actor))
                        QueueOGCD(Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump, TargetChoice(jump) ?? primaryTarget?.Actor, jumpStrat is CommonStrategy.Force or CommonStrategy.ForceEX or CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX ? OGCDPriority.Forced : OGCDPriority.SlightlyHigh);
                    if (ShouldUseDragonfireDive(ddStrat, primaryTarget?.Actor))
                        QueueOGCD(AID.DragonfireDive, TargetChoice(dd) ?? BestDiveTarget?.Actor, ddStrat is CommonStrategy.Force or CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX ? OGCDPriority.Forced : OGCDPriority.High);
                    if (ShouldUseStardiver(sdStrat, primaryTarget?.Actor))
                        QueueOGCD(AID.Stardiver, TargetChoice(sd) ?? BestDiveTarget?.Actor, sdStrat is CommonStrategy.Force or CommonStrategy.ForceEX or CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX ? OGCDPriority.Forced : OGCDPriority.Low);
                }
                if (ShouldUseGeirskogul(geirskogulStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.Geirskogul, TargetChoice(geirskogul) ?? BestSpearTarget?.Actor, geirskogulStrat is CommonStrategy.Force or CommonStrategy.ForceEX or CommonStrategy.ForceWeave or CommonStrategy.ForceWeaveEX ? OGCDPriority.Forced : OGCDPriority.VeryHigh);
                if (ShouldUseMirageDive(mdStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.MirageDive, TargetChoice(md) ?? primaryTarget?.Actor, OGCDPrio(mdStrat, OGCDPriority.ExtremelyLow));
                if (ShouldUseNastrond(nastrondStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.Nastrond, TargetChoice(nastrond) ?? BestSpearTarget?.Actor, OGCDPrio(nastrondStrat, OGCDPriority.VeryLow));
                if (ShouldUseRiseOfTheDragon(rotdStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.RiseOfTheDragon, TargetChoice(rotd) ?? BestDiveTarget?.Actor, OGCDPrio(rotdStrat, OGCDPriority.BelowAverage));
                if (ShouldUseStarcross(scStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.Starcross, TargetChoice(sc) ?? BestDiveTarget?.Actor, OGCDPrio(scStrat, OGCDPriority.BelowAverage));
                if (ShouldUseTrueNorth(strategy.Option(Track.TrueNorth).As<TrueNorthStrategy>(), primaryTarget?.Actor))
                    QueueOGCD(AID.TrueNorth, Player, OGCDPriority.AboveAverage);
            }
            if (!strategy.HoldGauge())
            {
                if (ShouldUseWyrmwindThrust(wtStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.WyrmwindThrust, TargetChoice(wt) ?? BestSpearTarget?.Actor, wtStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave ? OGCDPriority.Forced : PlayerHasEffect(SID.LanceCharge) ? OGCDPriority.ModeratelyHigh : OGCDPriority.Average);
            }
        }
        if (ShouldUsePiercingTalon(primaryTarget?.Actor, ptStrat))
            QueueGCD(AID.PiercingTalon, TargetChoice(pt) ?? primaryTarget?.Actor, ptStrat is PiercingTalonStrategy.Force or PiercingTalonStrategy.ForceEX ? GCDPriority.Forced : GCDPriority.SlightlyLow);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, 0, GCD - 0.9f);
        ShouldUseElusive(strategy.Option(Track.ElusiveJump).As<ElusiveDirection>(), primaryTarget?.Actor);
        #endregion

        #endregion

        #region AI
        if (ComboLastMove is AID.Disembowel or AID.SpiralBlow &&
            BestDOTTargets != null)
        {
            Hints.ForcedTarget = BestDOTTargets.Actor;
        }
        if (BestDOTTargets == null || (ShouldUseAOE ? !In10y(BestAOETarget?.Actor) : !In3y(BestDOTTarget?.Actor)))
        {
            GetNextTarget(strategy, ref primaryTarget, 3);
        }
        var pos = GetBestPositional(strategy, primaryTarget);
        UpdatePositionals(primaryTarget, ref pos);
        if (primaryTarget != null)
        {
            GoalZoneCombined(strategy, 3, Hints.GoalAOERect(primaryTarget.Actor, 10, 2), AID.DoomSpike, minAoe: 3, maximumActionRange: 20);
        }
        #endregion
    }
}
