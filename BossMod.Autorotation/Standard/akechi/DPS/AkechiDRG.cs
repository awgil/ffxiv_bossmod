using BossMod.DRG;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiDRG(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Dives, ElusiveJump, LanceCharge, BattleLitany, LifeSurge, MirageDive, WyrmwindThrust, PiercingTalon, TrueNorth, DragonfireDive, Geirskogul, Stardiver, Jump, Nastrond, RiseOfTheDragon, Starcross }
    public enum AOEStrategy { AutoFinish, ForceSTFinish, ForceSTFinishPS, ForceSTFinishCS, ForceNormalFinish, ForceBuffsFinish, ForceAOEFinish, AutoBreak, ForceSTBreak, ForceSTBreakPS, ForceSTBreakCS, ForceNormalBreak, ForceBuffsBreak, ForceAOEBreak }
    public enum DivesStrategy { InMelee, InMeleeNotMoving, InAny, InAnyNotMoving, Forbid }
    public enum ElusiveDirection { None, CharacterForward, CharacterBackward, CameraForward, CameraBackward }
    public enum BuffsStrategy { Automatic, Together, Force, ForceWeave, Delay }
    public enum SurgeStrategy { Automatic, WhenBuffed, Force, ForceWeave, ForceNextOpti, ForceNextOptiWeave, Delay }
    public enum FlexStrategy { ASAP, Late, LateOrBuffed, Delay }
    public enum PiercingTalonStrategy { AllowEX, Allow, Allow5, Allow10, Force, ForceEX, Forbid }
    public enum TrueNorthStrategy { Automatic, Send, ASAP, Rear, Flank, Force, Delay }
    public enum CommonStrategy { Automatic, HasLC, HasBL, HasAll, Force, ForceWeave, Delay }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRG", "Standard Rotation Module", "Standard rotation (Akechi)|DPS", "Akechi", RotationModuleQuality.Good, BitMask.Build(Class.LNC, Class.DRG), 100);

        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);

        res.Define(Track.AOE).As<AOEStrategy>("ST/AOE", "Single-Target & AoE Rotations", 300)
            .AddOption(AOEStrategy.AutoFinish, "Automatically use best rotation based on targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceSTFinish, "Force full single-target combo loop focusing on player's Power Surge buff or target's Chaotic Spring debuff, regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceSTFinishPS, "Force full single-target combo loop focusing on player's Power Surge buff, regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceSTFinishCS, "Force full single-target combo loop focusing on target's Chaotic Spring debuff, regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceNormalFinish, "Force damage combo loop (TrueThrust->VorpalThrust->FullThrust->FangAndClaw->Drakesbane) regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceBuffsFinish, "Force buffs combo loop (TrueThrust->Disembowel->ChaosThrust->WheelingThrust->Drakesbane) regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.ForceAOEFinish, "Force AoE rotation (DoomSpike->SonicThrust->CoerthanTorment) regardless of targets nearby - finishes current combo loop if possible")
            .AddOption(AOEStrategy.AutoBreak, "Automatically use best rotation depending on targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceSTBreak, "Force full single-target combo loop focusing on player's Power Surge buff or target's Chaotic Spring debuff, regardless of targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceSTBreakPS, "Force full single-target combo loop focusing on player's Power Surge buff, regardless of targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceSTBreakCS, "Force full single-target combo loop focusing on target's Chaotic Spring debuff, regardless of targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceNormalBreak, "Force damage combo loop (TrueThrust->VorpalThrust->FullThrust->FangAndClaw->Drakesbane) regardless of targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceBuffsBreak, "Force buffs combo loop (TrueThrust->Disembowel->ChaosThrust->WheelingThrust->Drakesbane) regardless of targets nearby - will break current combo loop if in one")
            .AddOption(AOEStrategy.ForceAOEBreak, "Force AoE rotation (DoomSpike->SonicThrust->CoerthanTorment) regardless of targets nearby - will break current combo loop if in one")
            .AddAssociatedActions(
                AID.TrueThrust, AID.RaidenThrust, AID.DoomSpike, AID.DraconianFury, //1
                AID.VorpalThrust, AID.LanceBarrage, AID.Disembowel, AID.SpiralBlow, AID.SonicThrust, //2
                AID.FullThrust, AID.HeavensThrust, AID.ChaosThrust, AID.ChaoticSpring, AID.CoerthanTorment, //3
                AID.WheelingThrust, AID.FangAndClaw, //4
                AID.Drakesbane); //5

        res.Define(Track.Dives).As<DivesStrategy>("Dives", "Dive Action Range", 199)
            .AddOption(DivesStrategy.InMelee, "Allow Stardiver & Dragonfire Dive when in melee range of target")
            .AddOption(DivesStrategy.InMeleeNotMoving, "Allow Stardiver & Dragonfire Dive when in melee range of target and not moving")
            .AddOption(DivesStrategy.InAny, "Allow Stardiver & Dragonfire Dive at any range of target")
            .AddOption(DivesStrategy.InAnyNotMoving, "Allow Stardiver & Dragonfire Dive at any range of target but only when not moving")
            .AddOption(DivesStrategy.Forbid, "Forbid Stardiver & Dragonfire Dive")
            .AddAssociatedActions(AID.DragonfireDive, AID.Stardiver);

        res.Define(Track.ElusiveJump).As<ElusiveDirection>("E.Jump", "Elusive Jump", -1)
            .AddOption(ElusiveDirection.None, "Do not use Elusive Jump")
            .AddOption(ElusiveDirection.CharacterForward, "Jump into the direction forward of the character", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CharacterBackward, "Jump into the direction backward of the character (default)", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CameraForward, "Jump into the direction forward of the camera", 30, 15, ActionTargets.Self, 35)
            .AddOption(ElusiveDirection.CameraBackward, "Jump into the direction backward of the camera", 30, 15, ActionTargets.Self, 35)
            .AddAssociatedActions(AID.ElusiveJump);

        res.Define(Track.LanceCharge).As<BuffsStrategy>("L.Charge", "Lance Charge", 198)
            .AddOption(BuffsStrategy.Automatic, "Automatically use Lance Charge")
            .AddOption(BuffsStrategy.Together, "Automatically use Lance Charge with Battle Litany - will delay if necessary (up to 30s)")
            .AddOption(BuffsStrategy.Force, "Force Lance Charge ASAP", 60, 20, ActionTargets.Self, 30)
            .AddOption(BuffsStrategy.ForceWeave, "Force Lance Charge in next possible weave slot", 60, 20, ActionTargets.Self, 30)
            .AddOption(BuffsStrategy.Delay, "Delay Lance Charge", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.LanceCharge);

        res.Define(Track.BattleLitany).As<BuffsStrategy>("B.Litany", "Battle Litany", 198)
            .AddOption(BuffsStrategy.Automatic, "Automatically use Battle Litany")
            .AddOption(BuffsStrategy.Together, "Automatically use Battle Litany with Lance Charge - will delay if necessary")
            .AddOption(BuffsStrategy.Force, "Force Battle Litany ASAP", 120, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.ForceWeave, "Force Battle Litany in next possible weave slot", 120, 20, ActionTargets.Self, 52)
            .AddOption(BuffsStrategy.Delay, "Delay Battle Litany", 0, 0, ActionTargets.None, 52)
            .AddAssociatedActions(AID.BattleLitany);

        res.Define(Track.LifeSurge).As<SurgeStrategy>("L.Surge", "Life Surge", 197)
            .AddOption(SurgeStrategy.Automatic, "Automatically use Life Surge")
            .AddOption(SurgeStrategy.WhenBuffed, "Automatically use Life Surge - will clip GCD if needed")
            .AddOption(SurgeStrategy.Force, "Force Life Surge ASAP", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceWeave, "Force Life Surge in next possible weave slot", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceNextOpti, "Force Life Surge in next possible optimal window", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.ForceNextOptiWeave, "Force Life Surge optimally in next possible weave slot", 40, 5, ActionTargets.Self, 6)
            .AddOption(SurgeStrategy.Delay, "Delay Life Surge", 0, 0, ActionTargets.None, 6)
            .AddAssociatedActions(AID.LifeSurge);

        res.Define(Track.MirageDive).As<FlexStrategy>("M.Dive", "Mirage Dive", 192)
            .AddOption(FlexStrategy.ASAP, "Automatically use Mirage Dive when under Dive Ready buff", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(FlexStrategy.Late, "Automatically use Mirage Dive as late as possible - will clip GCD if needed to avoid waste", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(FlexStrategy.LateOrBuffed, "Automatically use Mirage Dive as late as possible or when under any buffs - will clip GCD if needed to avoid waste", 0, 0, ActionTargets.Hostile, 68)
            .AddOption(FlexStrategy.Delay, "Delay Mirage Dive", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(AID.MirageDive);

        res.Define(Track.WyrmwindThrust).As<FlexStrategy>("W.Thrust", "Wyrmwind Thrust", 192)
            .AddOption(FlexStrategy.ASAP, "Automatically use Wyrmwind Thrust ASAP", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(FlexStrategy.Late, "Automatically use Wyrmwind Thrust as late as possible - will clip GCD if needed to avoid waste", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(FlexStrategy.LateOrBuffed, "Automatically use Wyrmwind Thrust as late as possible or when under any buffs - will clip GCD if needed to avoid waste", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(FlexStrategy.Delay, "Delay Wyrmwind Thrust", 0, 0, ActionTargets.None, 90)
            .AddAssociatedActions(AID.WyrmwindThrust);

        res.Define(Track.PiercingTalon).As<PiercingTalonStrategy>("P.Talon", "Piercing Talon", 100)
            .AddOption(PiercingTalonStrategy.AllowEX, "Automatically use Enhanced Piercing Talon if in combat & outside melee range of target")
            .AddOption(PiercingTalonStrategy.Allow, "Automatically use normal Piercing Talon if in combat & outside melee range of target")
            .AddOption(PiercingTalonStrategy.Allow5, "Automatically use normal Piercing Talon if in combat & 5+ yalms of target")
            .AddOption(PiercingTalonStrategy.Allow10, "Automatically use normal Piercing Talon if in combat & 10+ yalms of target")
            .AddOption(PiercingTalonStrategy.Force, "Force normal Piercing Talon ASAP")
            .AddOption(PiercingTalonStrategy.ForceEX, "Force Enhanced Piercing Talon ASAP")
            .AddOption(PiercingTalonStrategy.Forbid, "Forbid Piercing Talon")
            .AddAssociatedActions(AID.PiercingTalon);

        res.Define(Track.TrueNorth).As<TrueNorthStrategy>("T.North", "True North", 95)
            .AddOption(TrueNorthStrategy.Automatic, "Automatically late-weaves True North when out of positional")
            .AddOption(TrueNorthStrategy.Send, "Automatically late-weaves True North when out of positional - will clip GCD if needed")
            .AddOption(TrueNorthStrategy.ASAP, "Automatically use True North as soon as possible when out of positional", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Rear, "Automatically use True North for saving rear positionals only", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Flank, "Automatically use True North for saving flank positionals only", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Force, "Force True North ASAP", 45, 10, ActionTargets.Self, 50)
            .AddOption(TrueNorthStrategy.Delay, "Delay True North", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(ClassShared.AID.TrueNorth);

        res.Define(Track.DragonfireDive).As<CommonStrategy>("D.Dive", "Dragonfire Dive", 195)
            .AddOption(CommonStrategy.Automatic, "Automatically use Dragonfire Dive")
            .AddOption(CommonStrategy.HasLC, "Automatically use Dragonfire Dive if Lance Charge buff is currently active", 120, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasBL, "Automatically use Dragonfire Dive if Battle Litany buff is currently active", 120, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasAll, "Automatically use Dragonfire Dive if all buffs are currently active (Lance Charge, Battle Litany, & Life of the Dragon)", 120, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.Force, "Force Dragonfire Dive", 120, 0, ActionTargets.Hostile, 50)
            .AddOption(CommonStrategy.ForceWeave, "Force Dragonfire Dive in next possible weave slot", 120, 0, ActionTargets.Hostile, 50)
            .AddOption(CommonStrategy.Delay, "Delay Dragonfire Dive", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(AID.DragonfireDive);

        res.Define(Track.Geirskogul).As<CommonStrategy>("Gsk.", "Geirskogul", 196)
            .AddOption(CommonStrategy.Automatic, "Automatically use Geirskogul")
            .AddOption(CommonStrategy.HasLC, "Automatically use Geirskogul if Lance Charge buff is currently active", 60, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasBL, "Automatically use Geirskogul if Battle Litany buff is currently active", 60, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasAll, "Automatically use Geirskogul if both Lance Charge & Battle Litany buffs are currently active", 60, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.Force, "Force Geirskogul", 60, 0, ActionTargets.Hostile, 60)
            .AddOption(CommonStrategy.ForceWeave, "Force Geirskogul in next possible weave slot", 60, 20, ActionTargets.Hostile, 60)
            .AddOption(CommonStrategy.Delay, "Delay Geirskogul", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.Geirskogul);

        res.Define(Track.Stardiver).As<CommonStrategy>("S.diver", "Stardiver", 194)
            .AddOption(CommonStrategy.Automatic, "Automatically use Stardiver")
            .AddOption(CommonStrategy.HasLC, "Automatically use Stardiver if Lance Charge buff is currently active", 30, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasBL, "Automatically use Stardiver if Battle Litany buff is currently active", 50, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasAll, "Automatically use Stardiver if all buffs are currently active (Lance Charge, Battle Litany, & Life of the Dragon)", 30, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.Force, "Force Stardiver (Grants Starcross Ready)", 20, 0, ActionTargets.Hostile, 80)
            .AddOption(CommonStrategy.ForceWeave, "Force Stardiver in next possible weave slot (Grants Starcross Ready)", 20, 0, ActionTargets.Hostile, 80)
            .AddOption(CommonStrategy.Delay, "Delay Stardiver", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Stardiver);

        res.Define(Track.Jump).As<CommonStrategy>("Jump", uiPriority: 193)
            .AddOption(CommonStrategy.Automatic, "Automatically use Jump")
            .AddOption(CommonStrategy.HasLC, "Automatically use Jump if Lance Charge buff is currently active", 30, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasBL, "Automatically use Jump if Battle Litany buff is currently active", 30, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasAll, "Automatically use Jump if all buffs are currently active (Lance Charge, Battle Litany, & Life of the Dragon)", 30, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.Force, "Force Jump (Grants Dive Ready buff)", 30, 15, ActionTargets.Hostile, 30)
            .AddOption(CommonStrategy.ForceWeave, "Force Jump in next possible weave slot (Grants Dive Ready buff)", 30, 15, ActionTargets.Hostile, 30)
            .AddOption(CommonStrategy.Delay, "Delay Jump", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.Jump, AID.HighJump);

        res.DefineOGCD(Track.Nastrond, AID.Nastrond, "Nast.", "Nastrond", 196, 0, 0, ActionTargets.Hostile, 70);
        res.DefineOGCD(Track.RiseOfTheDragon, AID.RiseOfTheDragon, "RotD", "Rise Of The Dragon", 195, 0, 0, ActionTargets.Hostile, 92);
        res.DefineOGCD(Track.Starcross, AID.Starcross, "S.cross", "Starcross", 194, 0, 0, ActionTargets.Hostile, 100);

        return res;
    }

    private bool WantAOE;
    private bool AutoTarget;
    private float ChaosLeft;
    private int NumAOETargets;
    private Enemy? BestAOETarget;
    private Enemy? BestAOETargets;
    private Enemy? BestDOTTarget;
    private Enemy? BestDOTTargets;

    private DragoonGauge Gauge => World.Client.GetGauge<DragoonGauge>();
    private float PowerLeft => Status(SID.PowerSurge, 30);
    private bool NeedPower => PowerLeft <= SkSGCDLength * 2;
    private bool HasPower => PowerLeft > GCD;
    private bool HasLOTD => Gauge.LotdTimer > GCD;
    private bool HasLC => LCcd is >= 40 and <= 60;
    private bool HasBL => BLcd is >= 100 and <= 120;
    private float BLcd => Cooldown(AID.BattleLitany);
    private float LCcd => Cooldown(AID.LanceCharge);
    private static SID[] GetDotStatus() => [SID.ChaosThrust, SID.ChaoticSpring];
    private float ChaosRemaining(Actor? target)
        => target == null ? float.MaxValue
            : GetDotStatus().Select(stat => StatusDetails(target, (uint)stat, Player.InstanceID).Left).FirstOrDefault(dur => dur > 6);
    private bool WantDOT
        => AutoTarget && Unlocked(AID.ChaosThrust) &&
        ComboLastMove is AID.Disembowel or AID.SpiralBlow &&
        TargetsInAOECircle(3f, 2);

    private AID BestJump => Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump;
    private AID BestTrue => (Unlocked(AID.RaidenThrust) && HasStatus(SID.DraconianFire)) ? AID.RaidenThrust : AID.TrueThrust;
    private AID BestDisembowel => Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : Unlocked(AID.Disembowel) ? AID.Disembowel : AID.TrueThrust;
    private AID BestChaos => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust : AID.TrueThrust;
    private AID BestWheeling => Unlocked(AID.WheelingThrust) ? AID.WheelingThrust : AID.TrueThrust;
    private AID BestVorpal => Unlocked(AID.LanceBarrage) ? AID.LanceBarrage : Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust;
    private AID BestHeavens => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust;
    private AID BestFang => Unlocked(AID.FangAndClaw) ? AID.FangAndClaw : AID.TrueThrust;
    private AID BestDrakesbane => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust;

    //full complete single-target rotation with some tweaks
    //- dotOnly cares only about target's Chaotic Spring debuff
    //- buffOnly cares only about player's Power Surge buff
    //- allowAOE tells whether we should finish or break AOE combo if in one
    private AID FullST(bool dotOnly, bool buffOnly, bool allowAOE)
    {
        var wantChaos = ChaosLeft <= SkSGCDLength * 4; //we want to reapply Chaotic Spring if its remaining duration is less than or equal to 4 GCDs (or 10s)
        var wantPower = PowerLeft <= SkSGCDLength * 6; //we want to reapply Power Surge if its remaining duration is less than or equal to 6 GCDs (or 15s)
        var wantDisembowel =
            Unlocked(AID.ChaosThrust) //we have Chaotic Spring
                ? (dotOnly ? wantChaos //if only caring about Chaotic Spring debuff, focus on that
                    : buffOnly ? wantPower //if only caring about Power Surge buff, focus on that
                    : (wantPower || wantChaos)) //if caring about both, use either
                : (Unlocked(AID.FullThrust) ? PowerLeft <= SkSGCDLength * 3 //without dot, we focus primarily on Power Surge - if we have Full Thrust - 3 GCDs
            : NeedPower); //else - 2 GCDs

        return ComboLastMove switch
        {
            //finish AOE if allowed
            AID.SonicThrust when allowAOE => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : BestTrue,
            AID.DoomSpike or AID.DraconianFury when allowAOE => Unlocked(AID.SonicThrust) ? AID.SonicThrust : BestTrue,

            //else, ST handling
            AID.TrueThrust or AID.RaidenThrust => wantDisembowel ? BestDisembowel : BestVorpal,
            _ => ContinueSTCombo(ComboLastMove),
        };
    }

    //continuation of full single-target combo
    private AID ContinueSTCombo(AID last) => last switch
    {
        AID.WheelingThrust or AID.FangAndClaw => BestDrakesbane,
        AID.FullThrust or AID.HeavensThrust => BestFang,
        AID.ChaosThrust or AID.ChaoticSpring => BestWheeling,
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.Disembowel or AID.SpiralBlow => BestChaos,
        _ => BestTrue,
    };

    //continuation of full AOE combo
    private AID ContinueAOECombo(AID last) => last switch
    {
        AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : AID.DoomSpike,
        AID.DoomSpike or AID.DraconianFury => Unlocked(AID.SonicThrust) ? AID.SonicThrust : NeedPower ? LowLevelAOE : AID.DoomSpike,
        _ => !Unlocked(AID.SonicThrust) ? LowLevelAOE : HasStatus(SID.DraconianFire) ? AID.DraconianFury : AID.DoomSpike,
    };

    private AID FullSTFinishBoth => FullST(false, false, true);
    private AID FullSTFinishCS => FullST(true, false, true);
    private AID FullSTFinishPS => FullST(false, true, true);
    private AID FullSTBreakBoth => FullST(false, false, false);
    private AID FullSTBreakCS => FullST(true, false, false);
    private AID FullSTBreakPS => FullST(false, true, false);
    private AID BuffsSTFinish => ComboLastMove switch
    {
        AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : BestTrue,
        AID.DoomSpike or AID.DraconianFury => Unlocked(AID.SonicThrust) ? AID.SonicThrust : BestTrue,
        AID.TrueThrust or AID.RaidenThrust => BestDisembowel,
        _ => ContinueSTCombo(ComboLastMove),
    };
    private AID BuffsSTBreak => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => BestDisembowel,
        _ => ContinueSTCombo(ComboLastMove),
    };
    private AID NormalSTFinish => ComboLastMove switch
    {
        AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : BestTrue,
        AID.DoomSpike or AID.DraconianFury => Unlocked(AID.SonicThrust) ? AID.SonicThrust : BestTrue,
        AID.TrueThrust or AID.RaidenThrust => BestVorpal,
        _ => ContinueSTCombo(ComboLastMove),
    };
    private AID NormalSTBreak => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => BestVorpal,
        _ => ContinueSTCombo(ComboLastMove),
    };
    private AID FullAOEFinish => ComboLastMove switch
    {
        // finish ST combo if possible
        AID.WheelingThrust or AID.FangAndClaw => BestDrakesbane,
        AID.FullThrust or AID.HeavensThrust => BestFang,
        AID.ChaosThrust or AID.ChaoticSpring => BestWheeling,
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.Disembowel or AID.SpiralBlow => BestChaos,

        AID.TrueThrust or AID.RaidenThrust =>
            Unlocked(AID.Disembowel) &&
            (Unlocked(AID.ChaosThrust)
                ? (PowerLeft <= SkSGCDLength * 6 || ChaosLeft <= SkSGCDLength * 4)
                : (Unlocked(AID.FullThrust)
                    ? PowerLeft <= SkSGCDLength * 3
                    : NeedPower))
                ? BestDisembowel
                : BestVorpal,

        _ => ContinueAOECombo(ComboLastMove),
    };
    private AID FullAOEBreak => ContinueAOECombo(ComboLastMove);
    private AID AutoFinish => WantAOE ? FullAOEFinish : WantDOT ? BuffsSTFinish : FullSTFinishBoth;
    private AID AutoBreak => WantAOE ? FullAOEBreak : WantDOT ? BuffsSTBreak : FullSTBreakBoth;

    //until we unlock Sonic Thrust, we cannot generate Power Surge from AOE
    //so we use True Thrust -> Disembowel for the buff then immediately switch to Doom Spike spam
    private AID LowLevelAOE => ComboLastMove switch
    {
        AID.DoomSpike => NeedPower ? AID.TrueThrust : AID.DoomSpike,
        AID.Disembowel => AID.DoomSpike,
        AID.TrueThrust => AID.Disembowel,
        _ => NeedPower ? AID.TrueThrust : AID.DoomSpike,
    };

    private (Positional, bool) GetBestPositional(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (NumAOETargets > 2 && Unlocked(AID.DoomSpike) || !Unlocked(AID.ChaosThrust) || primaryTarget == null)
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

    private bool ShouldSpear(Actor? target) => InCombat(target) && In15y(target);
    private bool ShouldDive(StrategyValues strategy, Actor? target)
    {
        var dd = strategy.Option(Track.DragonfireDive).As<CommonStrategy>();
        var sd = strategy.Option(Track.Stardiver).As<CommonStrategy>();

        //planned forced usage of either or - don't check here
        if (dd is CommonStrategy.Force or CommonStrategy.ForceWeave ||
            sd is CommonStrategy.Force or CommonStrategy.ForceWeave)
            return true;

        var d = strategy.Option(Track.Dives).As<DivesStrategy>();
        var dontMove = d is DivesStrategy.InAnyNotMoving or DivesStrategy.InMeleeNotMoving;

        return InCombat(target) && (!dontMove || !IsMoving) && d switch
        {
            DivesStrategy.InMelee or DivesStrategy.InMeleeNotMoving => In3y(target),
            DivesStrategy.InAny or DivesStrategy.InAnyNotMoving => In20y(target),
            _ => false,
        };
    }

    private void QueueBuff(BuffsStrategy strategy, Actor? target, AID action, bool together, int extra = 0, bool otherCondition = true)
    {
        if (strategy == BuffsStrategy.Delay || !ActionReady(action))
            return;

        var conditions =
                otherCondition &&
                HasPower && //has Power Surge buff - this is pretty much mandatory
                In3y(target) && //close to target - dont waste it by being far
                InCombat(target); //dont use out of combat

        var (condition, prio) = strategy switch
        {
            BuffsStrategy.Automatic => (conditions, OGCDPriority.Severe),
            BuffsStrategy.Together => (conditions && together, OGCDPriority.Severe),
            BuffsStrategy.Force => (true, OGCDPriority.ToGCDPriority),
            _ => (false, OGCDPriority.None)
        };

        if (condition)
            QueueOGCD(action, Player, prio + extra);
    }
    private void QueueCommonOGCD(CommonStrategy strategy, Actor? target, AID action, bool optimal, bool allBuffs, int extra = 0)
    {
        var (condition, prio) = strategy switch
        {
            CommonStrategy.Automatic => (HasPower && InCombat(target) && ActionReady(action) && CanWeaveIn && optimal, OGCDPriority.High),
            CommonStrategy.Force => (ActionReady(action), OGCDPriority.ToGCDPriority),
            CommonStrategy.ForceWeave => (ActionReady(action) && CanWeaveIn, OGCDPriority.Max + 2),
            CommonStrategy.HasLC => (HasPower && HasLC && InCombat(target) && ActionReady(action), OGCDPriority.High),
            CommonStrategy.HasBL => (HasPower && HasBL && InCombat(target) && ActionReady(action), OGCDPriority.High),
            CommonStrategy.HasAll => (HasPower && allBuffs && InCombat(target) && ActionReady(action), OGCDPriority.High),
            _ => (false, OGCDPriority.None)
        };
        if (condition)
            QueueOGCD(action, target, prio + extra);
    }

    private (bool, OGCDPriority) ShouldUseLifeSurge(StrategyValues strategy, Actor? target)
    {
        var lsRange = In3y(target);
        var enhanced = Unlocked(TraitID.EnhancedLifeSurge);
        var ready = enhanced ? Cooldown(AID.LifeSurge) < 40.6f : ReadyIn(AID.LifeSurge) < 0.6f;

        if (!Unlocked(AID.LifeSurge) || HasStatus(SID.LifeSurge) || !ready)
            return (false, OGCDPriority.None);

        var actionClose =
            ComboLastMove is AID.VorpalThrust or AID.LanceBarrage ||
            (Unlocked(AID.Drakesbane) && ComboLastMove is AID.WheelingThrust or AID.FangAndClaw);

        var close =
            (!Unlocked(AID.FullThrust)
                ? (!Unlocked(AID.Disembowel) || !NeedPower) && ComboLastMove is AID.TrueThrust
            : WantAOE
                ? (Unlocked(AID.CoerthanTorment)
                    ? ComboLastMove is AID.SonicThrust
                    : Unlocked(AID.SonicThrust)
                        ? ComboLastMove is AID.DoomSpike
                        : Unlocked(AID.DoomSpike) && !NeedPower)
            : (((HasLC && HasLOTD || !enhanced) && actionClose) ||
               (HasLC && HasLOTD && HasBL &&
                (actionClose ||
                 ((Status(SID.BattleLitany) is <= 5f and not 0 ||
                   Cooldown(AID.LifeSurge) < 0.6f)
                  && ComboLastMove is not AID.Drakesbane && !NeedPower)))));

        var condition = close && InCombat(target) && HasPower && lsRange;
        return strategy.Option(Track.LifeSurge).As<SurgeStrategy>() switch
        {
            SurgeStrategy.Automatic => (condition, ChangePriority(600, 605, convert: false)),
            SurgeStrategy.WhenBuffed => (condition, ChangePriority(600, 605, lsRange)),
            SurgeStrategy.Force => (ready, OGCDPriority.Severe + 2000),
            SurgeStrategy.ForceWeave => (ready && CanWeaveIn, OGCDPriority.Severe),
            SurgeStrategy.ForceNextOpti => (close, OGCDPriority.Severe + 2000),
            SurgeStrategy.ForceNextOptiWeave => (close && CanWeaveIn, OGCDPriority.Severe),
            _ => (false, OGCDPriority.None),
        };
    }
    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {

        AutoTarget = strategy.AutoTarget();
        var manualTargets = Hints.NumPriorityTargetsInAOERect(Player.Position, Player.Rotation.ToDirection(), 10, 2);
        var mainTarget = primaryTarget?.Actor;
        var aoe = strategy.Option(Track.AOE);
        var aoeStrat = aoe.As<AOEStrategy>();
        WantAOE =
            AutoTarget &&
            Unlocked(AID.DoomSpike) &&
            (NumAOETargets >= 3 || aoeStrat is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak);
        ChaosLeft = MathF.Max(
            StatusDetails(mainTarget, SID.ChaosThrust, Player.InstanceID).Left,
            StatusDetails(mainTarget, SID.ChaoticSpring, Player.InstanceID).Left);
        var buffed1m =
            Unlocked(TraitID.LifeOfTheDragon) ? HasPower && HasLC && HasLOTD :
            Unlocked(AID.LanceCharge) ? HasPower && HasLC :
            (!Unlocked(AID.Disembowel) || HasPower);
        var buffed2m =
            Unlocked(TraitID.LifeOfTheDragon) ? HasPower && HasLC && HasBL && HasLOTD :
            Unlocked(AID.BattleLitany) ? HasPower && HasLC && HasBL :
            Unlocked(AID.LanceCharge) ? HasPower && HasLC :
            (!Unlocked(AID.Disembowel) || HasPower);
        var bursting = InOddWindow(AID.BattleLitany) ? buffed1m : buffed2m;

        (BestAOETargets, NumAOETargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        BestAOETarget = WantAOE ? BestAOETargets : primaryTarget;
        (BestDOTTargets, ChaosLeft) = GetDOTTarget(primaryTarget, ChaosRemaining, 2, 3);
        BestDOTTarget = WantDOT ? BestDOTTargets : primaryTarget;
        var (BestSpearTargets, NumSpearTargets) = GetBestTarget(primaryTarget, 15, LineTargetCheck(15));
        var BestSpearTarget = AutoTarget && Unlocked(AID.Geirskogul) && NumSpearTargets > 1 ? BestSpearTargets : primaryTarget;
        var (BestDiveTargets, NumDiveTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        var BestDiveTarget = AutoTarget && Unlocked(AID.DragonfireDive) && NumDiveTargets > 1 ? BestDiveTargets : primaryTarget;
        var allowedDiveTarget =
            ShouldDive(strategy, BestDiveTarget?.Actor) ? BestDiveTarget?.Actor :
            ShouldDive(strategy, mainTarget) ? mainTarget : null;

        if (strategy.HoldEverything() || CountdownRemaining > 0.7)
            return;

        Actor? SpearTarget(StrategyValues.OptionRef strat) => AOETargetChoice(mainTarget, BestSpearTarget?.Actor, strat, strategy);
        Actor? DiveTarget(StrategyValues.OptionRef strat) => AOETargetChoice(mainTarget, allowedDiveTarget, strat, strategy);

        //standard rotation
        var stTarget = SingleTargetChoice(mainTarget, aoe);
        var ttTarget = AOETargetChoice(mainTarget, BestDOTTarget?.Actor, aoe, strategy);
        var aoesTarget = AOETargetChoice(mainTarget, BestAOETarget?.Actor, aoe, strategy);
        var calculateTarget = AutoTarget ? (WantAOE ? aoesTarget : WantDOT ? ttTarget : stTarget) : stTarget;
        var calculateRange = AutoTarget ? (WantAOE ? In10y(aoesTarget) : WantDOT ? In3y(ttTarget) : In3y(stTarget)) : In3y(stTarget);
        var finishTarget = ComboLastMove switch
        {
            AID.SonicThrust or AID.DoomSpike or AID.DraconianFury => aoesTarget, //AOE
            AID.SpiralBlow or AID.Disembowel => ttTarget, //DOT
            AID.Drakesbane or AID.CoerthanTorment => calculateTarget, //now we can enter any
            _ => stTarget //ST actions
        };
        var (aoeAction, aoeTarget) = aoeStrat switch
        {
            AOEStrategy.AutoFinish => (AutoFinish, finishTarget),
            AOEStrategy.ForceSTFinish => (FullSTFinishBoth, stTarget),
            AOEStrategy.ForceSTFinishPS => (FullSTFinishPS, ttTarget),
            AOEStrategy.ForceSTFinishCS => (FullSTFinishCS, ttTarget),
            AOEStrategy.ForceBuffsFinish => (BuffsSTFinish, finishTarget),
            AOEStrategy.ForceNormalFinish => (NormalSTFinish, finishTarget),
            AOEStrategy.ForceAOEFinish => (FullAOEFinish, finishTarget),
            AOEStrategy.AutoBreak => (AutoBreak, calculateTarget),
            AOEStrategy.ForceSTBreak => (FullSTBreakBoth, ttTarget),
            AOEStrategy.ForceSTBreakPS => (FullSTBreakPS, stTarget),
            AOEStrategy.ForceSTBreakCS => (FullSTBreakCS, ttTarget),
            AOEStrategy.ForceBuffsBreak => (BuffsSTBreak, ttTarget),
            AOEStrategy.ForceNormalBreak => (NormalSTBreak, stTarget),
            AOEStrategy.ForceAOEBreak => (FullAOEBreak, aoesTarget),
            _ => (AID.None, null)
        };
        if (calculateRange)
            QueueGCD(aoeAction, aoeTarget, GCDPriority.Low);

        //abilities
        if (!strategy.HoldAbilities())
        {
            //cooldowns
            if (!strategy.HoldCDs())
            {
                //buffs
                if (!strategy.HoldBuffs())
                {

                    //Lance Charge
                    QueueBuff
                        (
                            strategy.Option(Track.LanceCharge).As<BuffsStrategy>(),
                            mainTarget,
                            AID.LanceCharge,
                            !Unlocked(AID.BattleLitany) || BLcd is > 30 or < 1,
                            2
                        );

                    //Battle Litany
                    QueueBuff
                        (
                            strategy.Option(Track.BattleLitany).As<BuffsStrategy>(),
                            mainTarget,
                            AID.BattleLitany,
                            HasLC,
                            1,
                            CombatTimer >= 30 || LCcd < 58
                        );

                    //Life Surge
                    var (lsCondition, lsPrio) = ShouldUseLifeSurge(strategy, mainTarget);
                    if (lsCondition)
                        QueueOGCD(AID.LifeSurge, Player, lsPrio);
                }

                //Geirskogul
                var gsk = strategy.Option(Track.Geirskogul);
                var gskStrat = gsk.As<CommonStrategy>();
                var gskTarget = SpearTarget(gsk);
                QueueCommonOGCD
                    (
                        gskStrat,
                        gskTarget,
                        AID.Geirskogul,
                        ShouldSpear(gskTarget) && (InOddWindow(AID.BattleLitany) ? HasLC : HasLC && HasBL),
                        HasLC && HasBL,
                        150
                    );

                //Jump
                var jump = strategy.Option(Track.Jump);
                var jumpStrat = jump.As<CommonStrategy>();
                var jumpTarget = SingleTargetChoice(mainTarget, jump);
                QueueCommonOGCD
                    (
                        jumpStrat,
                        jumpTarget,
                        BestJump,
                        buffed1m || buffed2m || LCcd is < 43 and > 13,
                        buffed2m,
                        6
                    );

                //Dragonfire Dive
                var dd = strategy.Option(Track.DragonfireDive);
                var ddStrat = dd.As<CommonStrategy>();
                var ddTarget = DiveTarget(dd);
                QueueCommonOGCD
                    (
                        ddStrat,
                        ddTarget,
                        AID.DragonfireDive,
                        ShouldDive(strategy, ddTarget) && buffed2m,
                        buffed2m,
                        4
                    );

                //Stardiver
                var sd = strategy.Option(Track.Stardiver);
                var sdStrat = sd.As<CommonStrategy>();
                var sdTarget = DiveTarget(sd);
                QueueCommonOGCD
                    (
                        sdStrat,
                        sdTarget,
                        AID.Stardiver,
                        ShouldDive(strategy, sdTarget) && bursting && !ShouldUseLifeSurge(strategy, mainTarget).Item1 &&
                            (SkSGCDLength <= 2.1f || CanWeaveIn),
                        buffed2m,
                        1
                    );

                //True North
                if (CanTrueNorth)
                {
                    var tn = strategy.Option(Track.TrueNorth);
                    var tnStrat = tn.As<TrueNorthStrategy>();
                    var condition = InCombat(mainTarget) && !WantAOE && In3y(mainTarget) && NextPositionalImminent && !NextPositionalCorrect;
                    var needRear = !IsOnRear(mainTarget!) && ((Unlocked(AID.ChaosThrust) && ComboLastMove is AID.Disembowel or AID.SpiralBlow) || (Unlocked(AID.WheelingThrust) && ComboLastMove is AID.ChaosThrust or AID.ChaoticSpring));
                    var needFlank = !IsOnFlank(mainTarget!) && Unlocked(AID.FangAndClaw) && ComboLastMove is AID.HeavensThrust or AID.FullThrust;
                    var (tnCondition, tnPrio) = tnStrat switch
                    {
                        TrueNorthStrategy.Automatic => (condition && CanLateWeaveIn, OGCDPriority.Average + 6),
                        TrueNorthStrategy.Send => (condition && GCD < 1.25f, ChangePriority(highPrio: 406)),
                        TrueNorthStrategy.ASAP => (condition, OGCDPriority.ToGCDPriority),
                        TrueNorthStrategy.Flank => (condition && CanLateWeaveIn && needFlank, OGCDPriority.Average + 5),
                        TrueNorthStrategy.Rear => (condition && CanLateWeaveIn && needRear, OGCDPriority.Average + 5),
                        TrueNorthStrategy.Force => (!HasStatus(SID.TrueNorth), OGCDPriority.ToGCDPriority),
                        _ => (false, OGCDPriority.None)
                    };
                    if (tnCondition)
                        QueueOGCD(AID.TrueNorth, Player, tnPrio);
                }

                //Starcross
                var sc = strategy.Option(Track.Starcross);
                var scStrat = sc.As<OGCDStrategy>();
                var scTarget = AOETargetChoice(mainTarget, BestDiveTarget?.Actor, sc, strategy); //no need for Dive check here since this is a ranged ability
                var scCondition =
                    ShouldUseOGCD
                    (
                        scStrat,
                        scTarget,
                        Unlocked(AID.Starcross) && HasStatus(SID.StarcrossReady),
                        InCombat(scTarget) && In20y(scTarget) && CanWeaveIn
                    );
                if (scCondition)
                    QueueOGCD(AID.Starcross, scTarget, OGCDPrio(scStrat, OGCDPriority.Average + 5));

                //Nastrond
                var nast = strategy.Option(Track.Nastrond);
                var nastStrat = nast.As<OGCDStrategy>();
                var nastTarget = SpearTarget(nast);
                var nastCondition =
                    ShouldUseOGCD
                    (
                        nastStrat,
                        nastTarget,
                        Unlocked(AID.Nastrond) && HasStatus(SID.NastrondReady),
                        ShouldSpear(nastTarget) && CanWeaveIn
                    );
                if (nastCondition)
                    QueueOGCD(AID.Nastrond, nastTarget, OGCDPrio(nastStrat, OGCDPriority.Average + 4));

                //Rise of the Dragon
                var rotd = strategy.Option(Track.RiseOfTheDragon);
                var rotdStrat = rotd.As<OGCDStrategy>();
                var rotdTarget = AOETargetChoice(mainTarget, BestDiveTarget?.Actor, rotd, strategy); //no need for Dive check here since this is a ranged ability
                var rotdCondition =
                    ShouldUseOGCD
                    (
                        rotdStrat,
                        rotdTarget,
                        Unlocked(AID.RiseOfTheDragon) && HasStatus(SID.DragonsFlight),
                        InCombat(rotdTarget) && In20y(rotdTarget) && CanWeaveIn
                    );
                if (rotdCondition)
                    QueueOGCD(AID.RiseOfTheDragon, rotdTarget, OGCDPrio(rotdStrat, OGCDPriority.Average + 3));

                //Mirage Dive
                var md = strategy.Option(Track.MirageDive);
                var mdStrat = md.As<FlexStrategy>();
                var mdTarget = SingleTargetChoice(mainTarget, md);
                var mdStatus = Status(SID.DiveReady);
                var mdMinimum = InCombat(mdTarget) && In20y(mdTarget) && HasStatus(SID.DiveReady);
                var (mdCondition, mdPrio) = mdStrat switch
                {
                    FlexStrategy.ASAP => (mdMinimum && CanWeaveIn, OGCDPriority.Average + 1),
                    FlexStrategy.Late => (mdMinimum && mdStatus <= 3f, ChangePriority(599, 749, In15y(mdTarget))),
                    FlexStrategy.LateOrBuffed => (mdMinimum && (mdStatus is <= 3f and not 0 || bursting), mdStatus is <= 3f and not 0 ? ChangePriority(401, 603) : ChangePriority(401, 603, convert: false)),
                    _ => (false, OGCDPriority.None),
                };
                if (mdCondition)
                    QueueOGCD(AID.MirageDive, mdTarget, mdPrio);

                //Elusive Jump
                if (ActionReady(AID.ElusiveJump))
                {
                    var elusive = strategy.Option(Track.ElusiveJump).As<ElusiveDirection>();
                    var angle = elusive switch
                    {
                        ElusiveDirection.CharacterForward => Player.Rotation,
                        ElusiveDirection.CameraForward => World.Client.CameraAzimuth,
                        ElusiveDirection.CameraBackward => World.Client.CameraAzimuth + 180.Degrees(),
                        _ => Player.Rotation + 180.Degrees()
                    };
                    if (elusive != ElusiveDirection.None)
                        QueueOGCD(AID.ElusiveJump, Player, OGCDPriority.Max + 1, facingAngle: angle);
                }

                //pots
                if (strategy.Potion() switch
                {
                    PotionStrategy.AlignWithBuffs => Player.InCombat && LCcd <= 2 && BLcd <= 3,
                    PotionStrategy.AlignWithRaidBuffs => Player.InCombat && (RaidBuffsIn <= 3 || RaidBuffsLeft > GCD),
                    PotionStrategy.Immediate => true,
                    _ => false
                })
                    Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.High);
            }

            //gauge
            if (!strategy.HoldGauge())
            {
                //Wyrmwind Thrust
                var wt = strategy.Option(Track.WyrmwindThrust);
                var wtStrat = wt.As<FlexStrategy>();
                var wtTarget = SpearTarget(wt);
                var wtMinimum = ShouldSpear(wtTarget) && Gauge.FirstmindsFocusCount == 2;
                var (wtCondition, wtPrio) = wtStrat switch
                {
                    FlexStrategy.ASAP => (wtMinimum && CanWeaveIn, OGCDPriority.Average + 2),
                    FlexStrategy.Late => (wtMinimum && HasStatus(SID.DraconianFire), ChangePriority(402, 603, In15y(wtTarget))),
                    FlexStrategy.LateOrBuffed => (wtMinimum && (HasStatus(SID.DraconianFire) || bursting), HasStatus(SID.DraconianFire) ? ChangePriority(402, 603, In15y(wtTarget)) : ChangePriority(402, 603, In15y(wtTarget), false)),
                    _ => (false, OGCDPriority.None)
                };
                if (wtCondition)
                    QueueOGCD(AID.WyrmwindThrust, wtTarget, wtPrio);
            }
        }

        //Piercing Talon
        if (Unlocked(AID.PiercingTalon))
        {
            var pt = strategy.Option(Track.PiercingTalon);
            var ptStrat = pt.As<PiercingTalonStrategy>();
            var ptTarget = SingleTargetChoice(mainTarget, pt);
            var prio = Status(SID.EnhancedPiercingTalon) is <= 3f and not 0f ? GCDPriority.Low + 1 : GCDPriority.Low;
            var (ptCondition, ptPrio) = ptStrat switch
            {
                PiercingTalonStrategy.AllowEX => (InCombat(ptTarget) && HasStatus(SID.EnhancedPiercingTalon), GCDPriority.Low),
                PiercingTalonStrategy.Allow => (InCombat(ptTarget) && !In3y(ptTarget), GCDPriority.Low),
                PiercingTalonStrategy.Allow5 => (InCombat(ptTarget) && (HasStatus(SID.EnhancedPiercingTalon) ? !In3y(ptTarget) : !In5y(ptTarget)), GCDPriority.Low),
                PiercingTalonStrategy.Allow10 => (InCombat(ptTarget) && (HasStatus(SID.EnhancedPiercingTalon) ? !In3y(ptTarget) : !In10y(ptTarget)), GCDPriority.Low),
                PiercingTalonStrategy.ForceEX => (HasStatus(SID.EnhancedPiercingTalon), GCDPriority.Low + 1),
                PiercingTalonStrategy.Force => (true, GCDPriority.Low + 1),
                _ => (false, GCDPriority.None)
            };
            if (ptCondition)
                QueueGCD(AID.PiercingTalon, ptTarget, ptPrio);
        }

        //targeting
        //TODO: improve
        if (strategy.AutoHardTargeting())
        {
            if (WantDOT)
            {
                if (ttTarget != null &&
                    aoeStrat is AOEStrategy.AutoBreak or
                    AOEStrategy.AutoFinish or
                    AOEStrategy.ForceBuffsBreak or
                    AOEStrategy.ForceBuffsFinish)
                {
                    Hints.ForcedTarget = ttTarget;
                }
            }
        }
        else if (AutoTarget)
        {
            GetNextTarget(strategy, ref primaryTarget, 3);
        }

        //positionals
        var pos = GetBestPositional(strategy, primaryTarget);
        UpdatePositionals(primaryTarget, ref pos);

        if (mainTarget != null)
        {
            GoalZoneCombined(strategy, 3, Hints.GoalAOERect(mainTarget, 10, 2), AID.DoomSpike, minAoe: 3, maximumActionRange: 20);
        }
    }
}

