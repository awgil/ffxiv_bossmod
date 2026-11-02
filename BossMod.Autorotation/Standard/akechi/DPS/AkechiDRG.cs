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
                AID.TrueThrust, AID.RaidenThrust, AID.DoomSpike, AID.DraconianFury,
                AID.VorpalThrust, AID.LanceBarrage, AID.Disembowel, AID.SpiralBlow, AID.SonicThrust,
                AID.FullThrust, AID.HeavensThrust, AID.ChaosThrust, AID.ChaoticSpring, AID.CoerthanTorment,
                AID.WheelingThrust, AID.FangAndClaw,
                AID.Drakesbane);

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

        res.Define(Track.Stardiver).As<CommonStrategy>("S.diver", "Stardiver", 193)
            .AddOption(CommonStrategy.Automatic, "Automatically use Stardiver")
            .AddOption(CommonStrategy.HasLC, "Automatically use Stardiver if Lance Charge buff is currently active", 30, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasBL, "Automatically use Stardiver if Battle Litany buff is currently active", 50, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.HasAll, "Automatically use Stardiver if all buffs are currently active (Lance Charge, Battle Litany, & Life of the Dragon)", 30, 0, ActionTargets.Hostile)
            .AddOption(CommonStrategy.Force, "Force Stardiver (Grants Starcross Ready)", 20, 0, ActionTargets.Hostile, 80)
            .AddOption(CommonStrategy.ForceWeave, "Force Stardiver in next possible weave slot (Grants Starcross Ready)", 20, 0, ActionTargets.Hostile, 80)
            .AddOption(CommonStrategy.Delay, "Delay Stardiver", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Stardiver);

        res.Define(Track.Jump).As<CommonStrategy>("Jump", uiPriority: 194)
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

    private enum Route { Both, Power, Chaos, Damage, Buffs, AOE }

    private DragoonGauge Gauge => World.Client.GetGauge<DragoonGauge>();
    private bool HasLOTD => Gauge.LotdTimer > GCD;
    private (float Left, bool Active, bool Needed) PS => Status(SID.PowerSurge, 30) is var left ? (left, left > GCD, left <= SkSGCDLength * 2) : default;
    private (float CD, bool Active) LC => Cooldown(AID.LanceCharge) is var cd ? (cd, cd is >= 40 and <= 60) : default;
    private (float CD, bool Active) BL => Cooldown(AID.BattleLitany) is var cd ? (cd, cd is >= 100 and <= 120) : default;
    private float LSCD => Cooldown(AID.LifeSurge);
    private float MDLeft => Status(SID.DiveReady, 15);
    private bool BuffsImminent => Unlocked(AID.LanceCharge) && LC.CD is > 0 and <= 2.5f || Unlocked(AID.BattleLitany) && BL.CD is > 0 and <= 2.5f;
    private bool Buffed => PS.Active && LC.Active && HasLOTD;

    private AID BestJump => Unlocked(AID.HighJump) ? AID.HighJump : AID.Jump;
    private AID BestTrue => Unlocked(AID.RaidenThrust) && HasStatus(SID.DraconianFire) ? AID.RaidenThrust : AID.TrueThrust;
    private AID BestDisembowel => Unlocked(AID.SpiralBlow) ? AID.SpiralBlow : Unlocked(AID.Disembowel) ? AID.Disembowel : AID.TrueThrust;
    private AID BestChaos => Unlocked(AID.ChaoticSpring) ? AID.ChaoticSpring : Unlocked(AID.ChaosThrust) ? AID.ChaosThrust : AID.TrueThrust;
    private AID BestVorpal => Unlocked(AID.LanceBarrage) ? AID.LanceBarrage : Unlocked(AID.VorpalThrust) ? AID.VorpalThrust : AID.TrueThrust;
    private AID BestHeavens => Unlocked(AID.HeavensThrust) ? AID.HeavensThrust : Unlocked(AID.FullThrust) ? AID.FullThrust : AID.TrueThrust;
    private AID BestDrakesbane => Unlocked(AID.Drakesbane) ? AID.Drakesbane : AID.TrueThrust;

    private bool AutoTarget;
    private bool WantAOE;
    private bool CleaveDOT;
    private int AOETargets;
    private float ChaosLeft;
    private Actor? AOETarget;
    private Actor? DOTTarget;
    private Actor? SpearTarget;
    private Actor? SplashTarget;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        var aoe = strategy.Option(Track.AOE);
        AutoTarget = strategy.AutoTarget();
        (var bestLine, AOETargets) = GetBestTarget(primaryTarget, 10, Is10yRectTarget);
        WantAOE = Unlocked(AID.DoomSpike) && (aoe.As<AOEStrategy>() is AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak || AutoTarget && AOETargets >= 3);
        CleaveDOT = AutoTarget && !WantAOE && Unlocked(AID.ChaosThrust) && TargetsInAOECircle(3f, 2);
        ChaosLeft = ChaosOn(PrimaryTarget);
        var (bestSpear, spearTargets) = GetBestTarget(primaryTarget, 15, Is15yRectTarget);
        var (bestSplash, splashTargets) = GetBestTarget(primaryTarget, 20, IsSplashTarget);
        SpearTarget = AutoTarget && spearTargets > 1 ? bestSpear?.Actor : PrimaryTarget;
        SplashTarget = AutoTarget && splashTargets > 1 ? bestSplash?.Actor : PrimaryTarget;
        AOETarget = AOETargetChoice(PrimaryTarget, (WantAOE ? bestLine : primaryTarget)?.Actor, aoe, strategy);
        DOTTarget = AOETargetChoice(PrimaryTarget, (CleaveDOT ? GetDOTTarget(primaryTarget, ChaosRemaining, 2, 3).Best : primaryTarget)?.Actor, aoe, strategy);

        if (strategy.HoldEverything())
            return;

        if (strategy.AllowBuffs())
        {
            QueueLanceCharge(strategy);
            QueueBattleLitany(strategy);
            QueueLifeSurge(strategy);
        }
        if (strategy.AllowCDs())
        {
            QueueGeirskogul(strategy);
            QueueHighJump(strategy);
            QueueDragonfireDive(strategy);
            QueueStardiver(strategy);
            QueueFollowUp(strategy, Track.Starcross, AID.Starcross, SID.StarcrossReady, SplashTarget, 20, OGCDPriority.Average + 5);
            QueueFollowUp(strategy, Track.Nastrond, AID.Nastrond, SID.NastrondReady, SpearTarget, 15, OGCDPriority.Average + 4);
            QueueFollowUp(strategy, Track.RiseOfTheDragon, AID.RiseOfTheDragon, SID.DragonsFlight, SplashTarget, 20, OGCDPriority.Average + 3);
            QueueMirageDive(strategy);
            QueueTrueNorth(strategy);
            QueueElusiveJump(strategy);
        }
        if (strategy.AllowGauge())
            QueueWyrmwindThrust(strategy);
        if (strategy.AllowAbilities())
            QueuePotion(strategy, ActionDefinitions.IDPotionStr, LC.CD <= 2 && BL.CD <= 3);
        QueuePiercingTalon(strategy);
        QueueCombo(strategy);

        var spreadDOT = strategy.AutoHardTargeting() && CleaveDOT && ComboLastMove is AID.Disembowel or AID.SpiralBlow && DOTTarget != null;
        if (spreadDOT)
            Hints.ForcedTarget = DOTTarget;
        else if (AutoTarget && !strategy.AutoHardTargeting())
            GetNextTarget(strategy, ref primaryTarget, 3);
        var positional = GetBestPositional(primaryTarget);
        UpdatePositionals(primaryTarget, ref positional);
        if (PrimaryTarget != null)
            GoalZoneCombined(strategy, 3, Hints.GoalAOERect(PrimaryTarget, 10, 2), AID.DoomSpike, minAoe: 3, maximumActionRange: 20);
    }

    private (bool Condition, OGCDPriority Priority) ShouldBuff(BuffsStrategy buff, bool together, bool optimal)
    {
        var ready = optimal && PS.Active && InCombat(PrimaryTarget) && In3y(PrimaryTarget);
        return buff switch
        {
            BuffsStrategy.Automatic => (ready, OGCDPriority.Severe),
            BuffsStrategy.Together => (ready && together, OGCDPriority.Severe),
            BuffsStrategy.Force => (true, OGCDPriority.ToGCDPriority),
            BuffsStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.ToGCDPriority),
            _ => (false, OGCDPriority.None)
        };
    }
    private void QueueLanceCharge(StrategyValues strategy)
    {
        var lc = strategy.Option(Track.LanceCharge).As<BuffsStrategy>();
        var (condition, priority) = ShouldBuff(lc, !Unlocked(AID.BattleLitany) || BL.CD is > 30 or < 1, true);
        if (condition && ActionReady(AID.LanceCharge))
            QueueOGCD(AID.LanceCharge, Player, priority + 2);
    }
    private void QueueBattleLitany(StrategyValues strategy)
    {
        var bl = strategy.Option(Track.BattleLitany).As<BuffsStrategy>();
        var (condition, priority) = ShouldBuff(bl, LC.Active, CombatTimer >= 30 || LC.CD < 58);
        if (condition && ActionReady(AID.BattleLitany))
            QueueOGCD(AID.BattleLitany, Player, priority + 1);
    }
    private void QueueLifeSurge(StrategyValues strategy)
    {
        var fullCharges = LSCD < 0.6f;
        var ready = Unlocked(AID.LifeSurge) && !HasStatus(SID.LifeSurge) && ReadyIn(AID.LifeSurge) < 0.6f;
        var aoeBigHit = Unlocked(AID.CoerthanTorment) ? ComboLastMove is AID.SonicThrust : ComboLastMove is AID.DoomSpike or AID.DraconianFury;
        var stBigHit = ComboLastMove is AID.VorpalThrust or AID.LanceBarrage
            || Unlocked(AID.Drakesbane) && ComboLastMove is AID.WheelingThrust or AID.FangAndClaw
            || !Unlocked(AID.FullThrust) && ComboLastMove is AID.TrueThrust && !PS.Needed;
        var bigHitNext = WantAOE ? aoeBigHit : CleaveDOT ? Unlocked(AID.Drakesbane) && ComboLastMove is AID.WheelingThrust : stBigHit;
        var keepForEven = !Unlocked(AID.BattleLitany) || BL.Active || LSCD + 40 <= BL.CD;
        var window = !Unlocked(AID.LanceCharge) || LC.Active && keepForEven || fullCharges && LC.CD > 10;
        var inRange = WantAOE ? In10y(PrimaryTarget) : In3y(PrimaryTarget);
        var optimal = bigHitNext && window && PS.Active && InCombat(PrimaryTarget) && inRange;
        var (condition, priority) = strategy.Option(Track.LifeSurge).As<SurgeStrategy>() switch
        {
            SurgeStrategy.Automatic => (optimal, ChangePriority(600, 605, convert: false)),
            SurgeStrategy.WhenBuffed => (optimal, ChangePriority(600, 605, inRange)),
            SurgeStrategy.Force => (true, OGCDPrio(OGCDStrategy.Force, OGCDPriority.Severe)),
            SurgeStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Severe),
            SurgeStrategy.ForceNextOpti => (bigHitNext, OGCDPrio(OGCDStrategy.Force, OGCDPriority.Severe)),
            SurgeStrategy.ForceNextOptiWeave => (bigHitNext && CanWeaveIn, OGCDPriority.Severe),
            _ => (false, OGCDPriority.None)
        };
        if (condition && ready)
            QueueOGCD(AID.LifeSurge, Player, priority);
    }
    private void QueueCommonOGCD(CommonStrategy common, Actor? target, AID action, bool optimal, int extra)
    {
        var engaged = PS.Active && InCombat(target);
        var (condition, priority) = common switch
        {
            CommonStrategy.Automatic => (engaged && optimal && CanWeaveIn, OGCDPriority.High),
            CommonStrategy.HasLC => (engaged && LC.Active, OGCDPriority.High),
            CommonStrategy.HasBL => (engaged && BL.Active, OGCDPriority.High),
            CommonStrategy.HasAll => (engaged && LC.Active && BL.Active && (action == AID.Geirskogul || HasLOTD), OGCDPriority.High),
            CommonStrategy.Force => (true, OGCDPriority.ToGCDPriority),
            CommonStrategy.ForceWeave => (CanWeaveIn, OGCDPriority.Max + 2),
            _ => (false, OGCDPriority.None)
        };
        if (condition && ActionReady(action))
            QueueOGCD(action, target, priority + extra);
    }
    private void QueueGeirskogul(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Geirskogul);
        var target = AOETargetChoice(PrimaryTarget, SpearTarget, track, strategy);
        QueueCommonOGCD(track.As<CommonStrategy>(), target, AID.Geirskogul, In15y(target) && !BuffsImminent, 150);
    }
    private void QueueHighJump(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Jump);
        var target = SingleTargetChoice(PrimaryTarget, track);
        QueueCommonOGCD(track.As<CommonStrategy>(), target, BestJump, In20y(target) && !BuffsImminent, 6);
    }
    private void QueueDragonfireDive(StrategyValues strategy)
    {
        var track = strategy.Option(Track.DragonfireDive);
        var target = AOETargetChoice(PrimaryTarget, ShouldDive(strategy, SplashTarget) ? SplashTarget : PrimaryTarget, track, strategy);
        QueueCommonOGCD(track.As<CommonStrategy>(), target, AID.DragonfireDive, ShouldDive(strategy, target) && !BuffsImminent, 4);
    }
    private void QueueStardiver(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Stardiver);
        var target = AOETargetChoice(PrimaryTarget, ShouldDive(strategy, SplashTarget) ? SplashTarget : PrimaryTarget, track, strategy);
        QueueCommonOGCD(track.As<CommonStrategy>(), target, AID.Stardiver, ShouldDive(strategy, target) && HasLOTD && GCD >= 1.5f, 1);
    }
    private void QueueFollowUp(StrategyValues strategy, Track track, AID action, SID ready, Actor? autoTarget, float range, OGCDPriority priority)
    {
        var option = strategy.Option(track);
        var followUp = option.As<OGCDStrategy>();
        var target = AOETargetChoice(PrimaryTarget, autoTarget, option, strategy);
        if (ShouldUseOGCD(followUp, target, Unlocked(action) && HasStatus(ready), InCombat(target) && DistanceFrom(target, range) && CanWeaveIn))
            QueueOGCD(action, target, OGCDPrio(followUp, priority));
    }
    private void QueueMirageDive(StrategyValues strategy)
    {
        var track = strategy.Option(Track.MirageDive);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var expiring = MDLeft is > 0 and <= 3f;
        var ready = MDLeft > 0 && InCombat(target) && In20y(target);
        var (condition, priority) = track.As<FlexStrategy>() switch
        {
            FlexStrategy.ASAP => (ready && CanWeaveIn, OGCDPriority.Average + 1),
            FlexStrategy.Late => (ready && expiring, ChangePriority(599, 749, In15y(target))),
            FlexStrategy.LateOrBuffed => (ready && (expiring || Buffed), ChangePriority(401, 603, convert: expiring)),
            _ => (false, OGCDPriority.None)
        };
        if (condition)
            QueueOGCD(AID.MirageDive, target, priority);
    }
    private void QueueTrueNorth(StrategyValues strategy)
    {
        var optimal = InCombat(PrimaryTarget) && !WantAOE && In3y(PrimaryTarget) && NextPositionalImminent && !NextPositionalCorrect;
        var needRear = !IsOnRear(PrimaryTarget) && ComboLastMove is AID.Disembowel or AID.SpiralBlow or AID.ChaosThrust or AID.ChaoticSpring;
        var needFlank = !IsOnFlank(PrimaryTarget) && ComboLastMove is AID.HeavensThrust or AID.FullThrust;
        var (condition, priority) = strategy.Option(Track.TrueNorth).As<TrueNorthStrategy>() switch
        {
            TrueNorthStrategy.Automatic => (optimal && CanLateWeaveIn, OGCDPriority.Average + 6),
            TrueNorthStrategy.Send => (optimal && GCD < 1.25f, ChangePriority(highPrio: 406)),
            TrueNorthStrategy.ASAP => (optimal, OGCDPriority.ToGCDPriority),
            TrueNorthStrategy.Rear => (optimal && CanLateWeaveIn && needRear, OGCDPriority.Average + 5),
            TrueNorthStrategy.Flank => (optimal && CanLateWeaveIn && needFlank, OGCDPriority.Average + 5),
            TrueNorthStrategy.Force => (!HasStatus(SID.TrueNorth), OGCDPriority.ToGCDPriority),
            _ => (false, OGCDPriority.None)
        };
        if (condition && CanTrueNorth)
            QueueOGCD(AID.TrueNorth, Player, priority);
    }
    private void QueueElusiveJump(StrategyValues strategy)
    {
        var elusive = strategy.Option(Track.ElusiveJump).As<ElusiveDirection>();
        var angle = elusive switch
        {
            ElusiveDirection.CharacterForward => Player.Rotation,
            ElusiveDirection.CameraForward => World.Client.CameraAzimuth,
            ElusiveDirection.CameraBackward => World.Client.CameraAzimuth + 180.Degrees(),
            _ => Player.Rotation + 180.Degrees()
        };
        if (elusive != ElusiveDirection.None && ActionReady(AID.ElusiveJump))
            QueueOGCD(AID.ElusiveJump, Player, OGCDPriority.Max + 1, facingAngle: angle);
    }
    private void QueueWyrmwindThrust(StrategyValues strategy)
    {
        var track = strategy.Option(Track.WyrmwindThrust);
        var target = AOETargetChoice(PrimaryTarget, SpearTarget, track, strategy);
        var raidenNext = HasStatus(SID.DraconianFire);
        var ready = Gauge.FirstmindsFocusCount == 2 && InCombat(target) && In15y(target);
        var (condition, priority) = track.As<FlexStrategy>() switch
        {
            FlexStrategy.ASAP => (ready && CanWeaveIn, OGCDPriority.Average + 2),
            FlexStrategy.Late => (ready && raidenNext, ChangePriority(402, 603, In15y(target))),
            FlexStrategy.LateOrBuffed => (ready && (raidenNext || Buffed), ChangePriority(402, 603, In15y(target), raidenNext)),
            _ => (false, OGCDPriority.None)
        };
        if (condition)
            QueueOGCD(AID.WyrmwindThrust, target, priority);
    }

    private void QueuePiercingTalon(StrategyValues strategy)
    {
        var track = strategy.Option(Track.PiercingTalon);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var enhanced = HasStatus(SID.EnhancedPiercingTalon);
        var engaged = InCombat(target);
        var prepull = enhanced && NoPrepullWithin(0.85f) && !In3y(target) && In20y(target);
        var (condition, priority) = track.As<PiercingTalonStrategy>() switch
        {
            PiercingTalonStrategy.AllowEX => (engaged && enhanced || prepull, GCDPriority.Low),
            PiercingTalonStrategy.Allow => (engaged && !In3y(target) || prepull, GCDPriority.Low),
            PiercingTalonStrategy.Allow5 => (engaged && (enhanced ? !In3y(target) : !In5y(target)) || prepull, GCDPriority.Low),
            PiercingTalonStrategy.Allow10 => (engaged && (enhanced ? !In3y(target) : !In10y(target)) || prepull, GCDPriority.Low),
            PiercingTalonStrategy.ForceEX => (enhanced, GCDPriority.Low + 1),
            PiercingTalonStrategy.Force => (true, GCDPriority.Low + 1),
            _ => (false, GCDPriority.None)
        };
        if (condition && Unlocked(AID.PiercingTalon))
            QueueGCD(AID.PiercingTalon, target, priority);
    }
    private void QueueCombo(StrategyValues strategy)
    {
        var track = strategy.Option(Track.AOE);
        var aoe = track.As<AOEStrategy>();
        var action = NextCombo(ComboRoute(aoe), aoe <= AOEStrategy.ForceAOEFinish);
        var aoeAction = action is AID.DoomSpike or AID.DraconianFury or AID.SonicThrust or AID.CoerthanTorment;
        var target = aoeAction ? AOETarget : ComboLastMove is AID.Disembowel or AID.SpiralBlow ? DOTTarget : SingleTargetChoice(PrimaryTarget, track);
        if ((aoeAction ? In10y(target) : In3y(target)) && CountdownRemaining is not > 0.7f)
            QueueGCD(action, target, GCDPriority.Low);
    }
    private Route ComboRoute(AOEStrategy aoe) => aoe switch
    {
        AOEStrategy.AutoFinish or AOEStrategy.AutoBreak => WantAOE ? Route.AOE : CleaveDOT ? Route.Buffs : Route.Both,
        AOEStrategy.ForceSTFinishPS or AOEStrategy.ForceSTBreakPS => Route.Power,
        AOEStrategy.ForceSTFinishCS or AOEStrategy.ForceSTBreakCS => Route.Chaos,
        AOEStrategy.ForceNormalFinish or AOEStrategy.ForceNormalBreak => Route.Damage,
        AOEStrategy.ForceBuffsFinish or AOEStrategy.ForceBuffsBreak => Route.Buffs,
        AOEStrategy.ForceAOEFinish or AOEStrategy.ForceAOEBreak => Route.AOE,
        _ => Route.Both
    };
    private AID NextCombo(Route route, bool finish)
    {
        var inAOECombo = ComboLastMove is AID.DoomSpike or AID.DraconianFury or AID.SonicThrust;
        var inSTCombo = ComboLastMove is AID.TrueThrust or AID.RaidenThrust or AID.VorpalThrust or AID.LanceBarrage or AID.FullThrust or AID.HeavensThrust
            or AID.FangAndClaw or AID.WheelingThrust or AID.Disembowel or AID.SpiralBlow or AID.ChaosThrust or AID.ChaoticSpring;
        return route == Route.AOE
            ? finish && inSTCombo ? NextST(Route.Both) : NextAOE()
            : finish && inAOECombo ? FinishAOE() : NextST(route);
    }
    private AID NextST(Route route) => ComboLastMove switch
    {
        AID.TrueThrust or AID.RaidenThrust => ChaosRoute(route) ? BestDisembowel : BestVorpal,
        AID.Disembowel or AID.SpiralBlow => BestChaos,
        AID.ChaosThrust or AID.ChaoticSpring => Unlocked(AID.WheelingThrust) ? AID.WheelingThrust : AID.TrueThrust,
        AID.VorpalThrust or AID.LanceBarrage => BestHeavens,
        AID.FullThrust or AID.HeavensThrust => Unlocked(AID.FangAndClaw) ? AID.FangAndClaw : AID.TrueThrust,
        AID.WheelingThrust or AID.FangAndClaw => BestDrakesbane,
        _ => BestTrue
    };
    private bool ChaosRoute(Route route)
    {
        var refreshChaos = ChaosLeft <= SkSGCDLength * 4;
        var refreshPower = PS.Left <= SkSGCDLength * 6;
        var refresh = route switch
        {
            Route.Power => refreshPower,
            Route.Chaos => refreshChaos,
            Route.Buffs => true,
            Route.Damage => false,
            _ => refreshPower || refreshChaos
        };
        var lowLevel = Unlocked(AID.FullThrust) ? PS.Left <= SkSGCDLength * 3 : PS.Needed;
        return Unlocked(AID.Disembowel) && (route is Route.Buffs or Route.Damage || Unlocked(AID.ChaosThrust) ? refresh : lowLevel);
    }
    private AID FinishAOE() => ComboLastMove == AID.SonicThrust
        ? Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : BestTrue
        : Unlocked(AID.SonicThrust) ? AID.SonicThrust : BestTrue;
    private AID NextAOE()
    {
        var lowLevel = ComboLastMove switch
        {
            AID.Disembowel => AID.DoomSpike,
            AID.TrueThrust => AID.Disembowel,
            _ => PS.Needed ? AID.TrueThrust : AID.DoomSpike
        };
        return ComboLastMove switch
        {
            AID.SonicThrust => Unlocked(AID.CoerthanTorment) ? AID.CoerthanTorment : AID.DoomSpike,
            AID.DoomSpike or AID.DraconianFury => Unlocked(AID.SonicThrust) ? AID.SonicThrust : PS.Needed ? lowLevel : AID.DoomSpike,
            _ => !Unlocked(AID.SonicThrust) ? lowLevel : HasStatus(SID.DraconianFire) ? AID.DraconianFury : AID.DoomSpike
        };
    }

    private float ChaosOn(Actor? target) => MathF.Max(StatusDetails(target, SID.ChaosThrust, Player.InstanceID).Left, StatusDetails(target, SID.ChaoticSpring, Player.InstanceID).Left);
    private float ChaosRemaining(Actor? target) => target == null ? float.MaxValue : ChaosOn(target) is var left && left > 6 ? left : 0;
    private bool ShouldDive(StrategyValues strategy, Actor? target)
    {
        var dives = strategy.Option(Track.Dives).As<DivesStrategy>();
        var standing = dives is not (DivesStrategy.InAnyNotMoving or DivesStrategy.InMeleeNotMoving) || !IsMoving;
        var inRange = dives switch
        {
            DivesStrategy.InMelee or DivesStrategy.InMeleeNotMoving => In3y(target),
            DivesStrategy.InAny or DivesStrategy.InAnyNotMoving => In20y(target) && DashSafe(target),
            _ => false
        };
        return InCombat(target) && standing && inRange;
    }
    private (Positional, bool) GetBestPositional(Enemy? target)
    {
        (Positional, bool) Predict(int stepsBeforeReloop) => (CanFitSkSGCD(ChaosLeft, stepsBeforeReloop + 3) && CanFitSkSGCD(PS.Left, stepsBeforeReloop + 2) ? Positional.Flank : Positional.Rear, false);
        var none = AOETargets > 2 && Unlocked(AID.DoomSpike) || !Unlocked(AID.ChaosThrust) || target == null;
        return none ? (Positional.Any, false)
            : !Unlocked(AID.FangAndClaw) ? (Positional.Rear, ComboLastMove == AID.Disembowel)
            : ComboLastMove switch
            {
                AID.ChaosThrust => Unlocked(AID.WheelingThrust) ? (Positional.Rear, true) : Predict(0),
                AID.Disembowel or AID.SpiralBlow or AID.ChaoticSpring => (Positional.Rear, true),
                AID.TrueThrust or AID.RaidenThrust => Predict(-1),
                AID.VorpalThrust or AID.LanceBarrage => (Positional.Flank, false),
                AID.HeavensThrust or AID.FullThrust => (Positional.Flank, true),
                AID.WheelingThrust or AID.FangAndClaw => Predict(Unlocked(AID.Drakesbane) ? 1 : 0),
                _ => Predict(0)
            };
    }
}
