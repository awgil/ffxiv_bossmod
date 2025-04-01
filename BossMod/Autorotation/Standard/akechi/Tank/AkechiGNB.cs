﻿using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.GNB;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { Combo = SharedTrack.Count, Cartridges, Potion, LightningShot, Zone, NoMercy, SonicBreak, GnashingFang, BowShock, Continuation, Bloodfest, DoubleDown, Reign, }
    public enum ComboStrategy { ForceSTwithO, ForceSTwithoutO, ForceAOEwithO, ForceAOEwithoutO }
    public enum CartridgeStrategy { Automatic, OnlyBS, OnlyFC, ForceBS, ForceBS1, ForceBS2, ForceBS3, ForceFC, ForceFC1, ForceFC2, ForceFC3, Delay }
    public enum PotionStrategy { Manual, AlignWithBloodfest, Immediate }
    public enum LightningShotStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }
    public enum NoMercyStrategy { Automatic, BurstReady, Force, ForceW, ForceQW, Force1, Force1W, Force1QW, Force2, Force2W, Force2QW, Force3, Force3W, Force3QW, Delay }
    public enum SonicBreakStrategy { Automatic, Force, Early, Late, Delay }
    public enum GnashingStrategy { Automatic, ForceGnash, ForceGnash1, ForceGnash2, ForceGnash3, ForceClaw, ForceTalon, Delay }
    public enum ContinuationStrategy { Automatic, Early, Late }
    public enum BloodfestStrategy { Automatic, Force, ForceW, Force0, Force0W, Delay }
    public enum DoubleDownStrategy { Automatic, Force, Force1, Force2, Force3, Delay }
    public enum ReignStrategy { Automatic, ForceReign, ForceNoble, ForceLion, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.GNB), 100);

        res.DefineAOE().AddAssociatedActions(AID.KeenEdge, AID.BrutalShell, AID.SolidBarrel, AID.DemonSlice, AID.DemonSlaughter);
        res.DefineHold();
        res.Define(Track.Combo).As<ComboStrategy>("Combo", uiPriority: 200)
            .AddOption(ComboStrategy.ForceSTwithO, "Force ST with Overcap", "if 'Force Single-Target Rotation' is selected, it will prevent overcapping cartridges with Burst Strike (if available)")
            .AddOption(ComboStrategy.ForceSTwithoutO, "Force ST without Overcap", "if 'Force Single-Target Rotation' is selected, it will not prevent overcapping cartridges")
            .AddOption(ComboStrategy.ForceAOEwithO, "Force AOE with Overcap", "if 'Force AOE Rotation' is selected, it will prevent overcapping cartridges with Fated Circle (if available)")
            .AddOption(ComboStrategy.ForceAOEwithoutO, "Force AOE without Overcap", "if 'Force AOE Rotation' is selected, it will not prevent overcapping cartridges");
        res.Define(Track.Cartridges).As<CartridgeStrategy>("Cartridges", "Carts", uiPriority: 180)
            .AddOption(CartridgeStrategy.Automatic, "Automatic", "Automatically decide when to use cartridges; uses them optimally")
            .AddOption(CartridgeStrategy.OnlyBS, "Only Burst Strike", "Uses Burst Strike optimally as cartridge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.OnlyFC, "Only Fated Circle", "Uses Fated Circle optimally as cartridge spender only, regardless of targets", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.ForceBS, "Force Burst Strike", "Force use of Burst Strike regardless of cartridge count", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS1, "Force Burst Strike (1 cart)", "Force use of Burst Strike when only 1 cartridge is available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS2, "Force Burst Strike (2 cart)", "Force use of Burst Strike when only 2 cartridges are available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS3, "Force Burst Strike (3 cart)", "Force use of Burst Strike when only 3 cartridges are available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceFC, "Force Fated Circle", "Force use of Fated Circle when any cartridges are available", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.ForceFC1, "Force Fated Circle (1 cart)", "Force use of Fated Circle when only 1 cartridge is available", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.ForceFC2, "Force Fated Circle (2 cart)", "Force use of Fated Circle when only 2 cartridges are available", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.ForceFC3, "Force Fated Circle (3 cart)", "Force use of Fated Circle when only 3 cartridges are available", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.Delay, "Delay", "Forbid use of Burst Strike & Fated Circle", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.BurstStrike, AID.FatedCircle);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 90)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithBloodfest, "AlignWithBloodfest", "Align with No Mercy & Bloodfest together (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.LightningShot).As<LightningShotStrategy>("Lightning Shot", "L.Shot", uiPriority: 100)
            .AddOption(LightningShotStrategy.OpenerFar, "Far (Opener)", "Use Lightning Shot in pre-pull & out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.OpenerForce, "Force (Opener)", "Force use Lightning Shot in pre-pull in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Force, "Force", "Force use Lightning Shot in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Allow, "Allow", "Allow use of Lightning Shot when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Forbid, "Forbid", "Prohibit use of Lightning Shot")
            .AddAssociatedActions(AID.LightningShot);
        res.DefineOGCD(Track.Zone, AID.DangerZone, "Zone", "Zone", uiPriority: 135, 30, 0, ActionTargets.Hostile, 18).AddAssociatedActions(AID.BlastingZone, AID.DangerZone);
        res.Define(Track.NoMercy).As<NoMercyStrategy>("No Mercy", "N.Mercy", uiPriority: 160)
            .AddOption(NoMercyStrategy.Automatic, "Auto", "Normal use of No Mercy")
            .AddOption(NoMercyStrategy.BurstReady, "Burst Ready", "Use No Mercy only when burst is ready; will delay if necessary", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force, "Force", "Force use of No Mercy, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceW, "Force (Weave)", "Force use of No Mercy in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceQW, "Force (Q.Weave)", "Force use of No Mercy in next possible last second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1, "Force (1 cart)", "Force use of No Mercy when only 1 cartridge is available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1W, "Force (1 cart, Weave)", "Force use of No Mercy when only 1 cartridge is available & in next weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1QW, "Force (1 cart, Q.Weave)", "Force use of No Mercy when only 1 cartridge is available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2, "Force (2 carts)", "Force use of No Mercy when only 2 cartridges are available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2W, "Force (2 carts, Weave)", "Force use of No Mercy when only 2 cartridges are available & in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2QW, "Force (2 carts, Q.Weave)", "Force use of No Mercy when only 2 cartridges are available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3, "Force (3 carts)", "Force use of No Mercy when only 3 cartridges are available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3W, "Force (3 carts, Weave)", "Force use of No Mercy when only 3 cartridges are available & in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3QW, "Force (3 carts, Q.Weave)", "Force use of No Mercy when only 3 cartridges are available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Delay, "Delay", "Delay use of No Mercy", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.NoMercy);
        res.Define(Track.SonicBreak).As<SonicBreakStrategy>("Sonic Break", "S.Break", uiPriority: 145)
            .AddOption(SonicBreakStrategy.Automatic, "Auto", "Normal use of Sonic Break")
            .AddOption(SonicBreakStrategy.Force, "Force", "Force use of Sonic Break", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Early, "Early Sonic Break", "Uses Sonic Break as the very first GCD when in No Mercy", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Late, "Late Sonic Break", "Uses Sonic Break as the very last GCD when in No Mercy", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Delay, "Delay", "Delay use of Sonic Break", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(AID.SonicBreak);
        res.Define(Track.GnashingFang).As<GnashingStrategy>("Gnashing Fang", "G.Fang", uiPriority: 150)
            .AddOption(GnashingStrategy.Automatic, "Auto", "Normal use of Gnashing Fang")
            .AddOption(GnashingStrategy.ForceGnash, "Force", "Force use of Gnashing Fang", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceGnash1, "Force (1 cart)", "Force use of Gnashing Fang when only 1 cartridge is available", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceGnash2, "Force (2 carts)", "Force use of Gnashing Fang when only 2 cartridges are available", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceGnash3, "Force (3 carts)", "Force use of Gnashing Fang when only 3 cartridges are available", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceClaw, "Force Savage Claw", "Force use of Savage Claw", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceTalon, "Force Talon", "Force use of Wicked Talon", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.Delay, "Delay", "Delay use of Gnashing Fang", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.GnashingFang, AID.SavageClaw, AID.WickedTalon);
        res.DefineOGCD(Track.BowShock, AID.BowShock, "BowShock", "B.Shock", uiPriority: 140, 60, 15, ActionTargets.Self, 62).AddAssociatedActions(AID.BowShock);
        res.Define(Track.Continuation).As<ContinuationStrategy>("Continuation", "Cont.", uiPriority: 135)
            .AddOption(ContinuationStrategy.Automatic, "Auto", "Normal use of Continuation")
            .AddOption(ContinuationStrategy.Early, "Early", "Use Continuation procs as early as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(ContinuationStrategy.Late, "Late", "Use Continuation procs as late as possible", 0, 0, ActionTargets.Hostile, 70)
            .AddAssociatedActions(AID.EyeGouge, AID.AbdomenTear, AID.JugularRip, AID.Hypervelocity, AID.FatedBrand);
        res.Define(Track.Bloodfest).As<BloodfestStrategy>("Bloodfest", "Fest", uiPriority: 150)
            .AddOption(BloodfestStrategy.Automatic, "Auto", "Normal use of Bloodfest")
            .AddOption(BloodfestStrategy.Force, "Force", "Force use of Bloodfest, regardless of ammo count & weaving", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.ForceW, "Force (Weave)", "Force use of Bloodfest in next possible weave slot, regardless of ammo count", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Force0, "Force (0 cart)", "Force use of Bloodfest only if empty on cartridges", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Force0W, "Force (0 cart, Weave)", "Force use of Bloodfest only if empty on cartridges & in next possible weave slot", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Delay, "Delay", "Delay use of Bloodfest", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Bloodfest);
        res.Define(Track.DoubleDown).As<DoubleDownStrategy>("DoubleDown", "D.Down", uiPriority: 130)
            .AddOption(DoubleDownStrategy.Automatic, "Automatic", "Normal use of Double Down")
            .AddOption(DoubleDownStrategy.Force, "Force Double Down", "Force use of Double Down regardless of cartridge count", 60, 0, ActionTargets.Hostile, 90)
            .AddOption(DoubleDownStrategy.Force1, "Force Double Down (1 cart)", "Force use of Double Down when only 1 cartridge is available", 60, 0, ActionTargets.Hostile, 90)
            .AddOption(DoubleDownStrategy.Force2, "Force Double Down (2 cart)", "Force use of Double Down when only 2 cartridges are available", 60, 0, ActionTargets.Hostile, 90)
            .AddOption(DoubleDownStrategy.Force3, "Force Double Down (3 cart)", "Force use of Double Down when only 3 cartridges are available", 60, 0, ActionTargets.Hostile, 90)
            .AddOption(DoubleDownStrategy.Delay, "Delay", "Delay use of Double Down", 0, 0, ActionTargets.None, 90)
            .AddAssociatedActions(AID.DoubleDown);
        res.Define(Track.Reign).As<ReignStrategy>("Reign of Beasts", "Reign", uiPriority: 125)
            .AddOption(ReignStrategy.Automatic, "Auto", "Normal use of Reign of Beasts")
            .AddOption(ReignStrategy.ForceReign, "Force", "Force use of Reign of Beasts", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.ForceNoble, "Force", "Force use of Noble Blood", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.ForceLion, "Force", "Force use of Lion Heart", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.Delay, "Delay", "Delay use of Reign of Beasts", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(AID.ReignOfBeasts, AID.NobleBlood, AID.LionHeart);

        return res;
    }
    #endregion

    #region Module Variables
    private byte Ammo;
    private byte GunComboStep;
    private int MaxCartridges;
    private float NMcd;
    private float BFcd;
    private bool HasNM;
    private bool HasBlast;
    private bool HasRaze;
    private bool HasRip;
    private bool HasTear;
    private bool HasGouge;
    private bool CanBS;
    private bool CanGF;
    private bool CanFC;
    private bool CanDD;
    private bool CanBreak;
    private bool CanReign;
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestSplashTarget;
    private Enemy? BestDOTTarget;
    #endregion

    #region Upgrade Paths
    private AID BestCartSpender => CanFC ? (ShouldUseAOECircle(5).OnTwoOrMore ? AID.FatedCircle : AID.BurstStrike) : CanBS ? AID.BurstStrike : AutoFinish;
    private AID BestContinuation => HasRaze ? AID.FatedBrand : HasBlast ? AID.Hypervelocity : HasGouge ? AID.EyeGouge : HasTear ? AID.AbdomenTear : HasRip ? AID.JugularRip : AID.Continuation;
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.SavageClaw => AID.WickedTalon,
        AID.GnashingFang => AID.SavageClaw,
        AID.KeenEdge or AID.BrutalShell => STwithoutOvercap,
        AID.DemonSlice => AOEwithoutOvercap,
        AID.SolidBarrel or AID.DemonSlaughter or AID.WickedTalon or _ => ShouldUseAOECircle(5).OnTwoOrMore ? AOEwithoutOvercap : STwithoutOvercap,
    };
    private AID AutoBreak => (ShouldUseAOECircle(5).OnTwoOrMore && GunComboStep == 0) ? AOEwithoutOvercap : STwithoutOvercap;
    private AID STwithOvercap => Ammo == MaxCartridges ? BestCartSpender : Unlocked(AID.SolidBarrel) & ComboLastMove is AID.BrutalShell ? AID.SolidBarrel : Unlocked(AID.BrutalShell) && ComboLastMove is AID.KeenEdge ? AID.BrutalShell : AID.KeenEdge;
    private AID STwithoutOvercap => Unlocked(AID.SolidBarrel) && ComboLastMove is AID.BrutalShell ? AID.SolidBarrel : Unlocked(AID.BrutalShell) && ComboLastMove is AID.KeenEdge ? AID.BrutalShell : AID.KeenEdge;
    private AID AOEwithOvercap => Ammo == MaxCartridges ? BestCartSpender : Unlocked(AID.DemonSlaughter) && ComboLastMove is AID.DemonSlice ? AID.DemonSlaughter : AID.DemonSlice;
    private AID AOEwithoutOvercap => Unlocked(AID.DemonSlaughter) && ComboLastMove is AID.DemonSlice ? AID.DemonSlaughter : AID.DemonSlice;
    #endregion

    #region Cooldown Helpers
    private bool ShouldUseNoMercy(NoMercyStrategy strategy, Actor? target)
    {
        if (!ActionReady(AID.NoMercy))
            return false;

        var slow = SkSGCDLength >= 2.4800f && CanWeaveIn;
        var fast = SkSGCDLength <= 2.4799f && CanQuarterWeaveIn;
        var speed = slow || fast;
        var lv1to89 = speed && Ammo >= 1;
        var lv90plus = speed && ((InOddWindow(AID.Bloodfest) && Ammo >= 2) || (!InOddWindow(AID.Bloodfest) && Ammo < 3));
        var open = ((fast && CombatTimer < 30 && ComboLastMove is AID.BrutalShell) ||
                   (slow && CombatTimer < 30 && ComboLastMove is AID.KeenEdge) ||
                   CombatTimer >= 30);
        var burst = speed && Ammo >= 2 && (((Unlocked(AID.DoubleDown) && TotalCD(AID.DoubleDown) <= 3) || !Unlocked(AID.DoubleDown)) && ((Unlocked(AID.GnashingFang) && TotalCD(AID.GnashingFang) <= 1) || !Unlocked(AID.GnashingFang)));
        return strategy switch
        {
            NoMercyStrategy.Automatic => InsideCombatWith(target) && In5y(target) && open && (Unlocked(AID.DoubleDown) ? lv90plus : lv1to89),
            NoMercyStrategy.BurstReady => InsideCombatWith(target) && In5y(target) && burst,
            NoMercyStrategy.Force => true,
            NoMercyStrategy.ForceW => CanWeaveIn,
            NoMercyStrategy.ForceQW => CanQuarterWeaveIn,
            NoMercyStrategy.Force1 => Ammo == 1,
            NoMercyStrategy.Force1W => CanWeaveIn && Ammo == 1,
            NoMercyStrategy.Force1QW => CanQuarterWeaveIn && Ammo == 1,
            NoMercyStrategy.Force2 => Ammo == 2,
            NoMercyStrategy.Force2W => CanWeaveIn && Ammo == 2,
            NoMercyStrategy.Force2QW => CanQuarterWeaveIn && Ammo == 2,
            NoMercyStrategy.Force3 => Ammo == 3,
            NoMercyStrategy.Force3W => CanWeaveIn && Ammo == 3,
            NoMercyStrategy.Force3QW => CanQuarterWeaveIn && Ammo == 3,
            NoMercyStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseGnashingFang(GnashingStrategy strategy, Actor? target)
    {
        //we usually use this in two ways: inside & outside No Mercy, whilst trying to keep it aligned with Burst
        //we also skip entirely if more than 3 targets are present
        var condition = (CanGF && !ShouldUseAOECircle(5).OnFourOrMore && NMcd is <= 60 and > 17);
        return strategy switch
        {
            GnashingStrategy.Automatic => InsideCombatWith(target) && In3y(target) && condition,
            GnashingStrategy.ForceGnash => CanGF,
            GnashingStrategy.ForceGnash1 => CanGF && Ammo == 1,
            GnashingStrategy.ForceGnash2 => CanGF && Ammo == 2,
            GnashingStrategy.ForceGnash3 => CanGF && Ammo == 3,
            GnashingStrategy.ForceClaw => GunComboStep == 1,
            GnashingStrategy.ForceTalon => GunComboStep == 2,
            GnashingStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseContinuation(ContinuationStrategy strategy, Actor? target)
    {
        var condition = InsideCombatWith(target) && In5y(target) && (Unlocked(AID.Continuation) && (HasBlast || HasRaze || HasRip || HasTear || HasGouge));
        return strategy switch
        {
            ContinuationStrategy.Automatic or ContinuationStrategy.Early => condition,
            ContinuationStrategy.Late => condition && GCD < 1.25,
            _ => false,
        };
    }
    private bool ShouldUseBowShock(OGCDStrategy strategy, Actor? target)
    {
        if (!ActionReady(AID.BowShock))
            return false;
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In5y(target) && CanWeaveIn && NMcd is < 57.5f and >= 40,
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseDoubleDown(DoubleDownStrategy strategy, Actor? target)
    {
        if (!CanDD)
            return false;
        return strategy switch
        {
            DoubleDownStrategy.Automatic => InsideCombatWith(target) && In5y(target) && HasNM,
            DoubleDownStrategy.Force => true,
            DoubleDownStrategy.Force1 => Ammo == 1,
            DoubleDownStrategy.Force2 => Ammo == 2,
            DoubleDownStrategy.Force3 => Ammo == 3,
            DoubleDownStrategy.Delay or _ => false,
        };

    }
    private bool ShouldUseBloodfest(BloodfestStrategy strategy, Actor? target)
    {
        if (!ActionReady(AID.Bloodfest))
            return false;
        return strategy switch
        {
            BloodfestStrategy.Automatic => InsideCombatWith(target) && Ammo == 0,
            BloodfestStrategy.Force => true,
            BloodfestStrategy.ForceW => CanWeaveIn,
            BloodfestStrategy.Force0 => Ammo == 0,
            BloodfestStrategy.Force0W => Ammo == 0 && CanWeaveIn,
            BloodfestStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseZone(OGCDStrategy strategy, Actor? target)
    {
        if (!ActionReady(AID.DangerZone))
            return false;
        //we usually use this in two ways: inside & outside No Mercy, whilst trying to keep it aligned with Burst
        return strategy switch
        {
            OGCDStrategy.Automatic => InsideCombatWith(target) && In3y(target) && CanWeaveIn && NMcd is < 57.5f and > 17,
            OGCDStrategy.Force => true,
            OGCDStrategy.AnyWeave => CanWeaveIn,
            OGCDStrategy.EarlyWeave => CanEarlyWeaveIn,
            OGCDStrategy.LateWeave => CanLateWeaveIn,
            OGCDStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseSonicBreak(SonicBreakStrategy strategy, Actor? target)
    {
        if (!CanBreak)
            return false;
        return strategy switch
        {
            SonicBreakStrategy.Automatic => InsideCombatWith(target) && In3y(target),
            SonicBreakStrategy.Force => true,
            SonicBreakStrategy.Early => HasNM,
            SonicBreakStrategy.Late => StatusRemaining(Player, SID.NoMercy, 20) <= SkSGCDLength,
            SonicBreakStrategy.Delay or _ => false,
        };
    }
    private bool ShouldUseReign(ReignStrategy strategy, Actor? target)
    {
        var condition = (HasNM && CanReign && GunComboStep == 0) || GunComboStep is 3 or 4;
        return strategy switch
        {
            ReignStrategy.Automatic => InsideCombatWith(target) && condition,
            ReignStrategy.ForceReign => CanReign,
            ReignStrategy.ForceNoble => GunComboStep == 3,
            ReignStrategy.ForceLion => GunComboStep == 4,
            ReignStrategy.Delay or _ => false,
        };

    }
    private bool ShouldUseCartridges(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic or CartridgeStrategy.OnlyBS or CartridgeStrategy.OnlyFC => ShouldSpendCarts(CartridgeStrategy.Automatic, target),
        CartridgeStrategy.ForceBS => CanBS,
        CartridgeStrategy.ForceBS1 => CanBS && Ammo == 1,
        CartridgeStrategy.ForceBS2 => CanBS && Ammo == 2,
        CartridgeStrategy.ForceBS3 => CanBS && Ammo == 3,
        CartridgeStrategy.ForceFC => CanFC,
        CartridgeStrategy.ForceFC1 => CanFC && Ammo == 1,
        CartridgeStrategy.ForceFC2 => CanFC && Ammo == 2,
        CartridgeStrategy.ForceFC3 => CanFC && Ammo == 3,
        CartridgeStrategy.Delay or _ => false,
    };
    private bool ShouldSpendCarts(CartridgeStrategy strategy, Actor? target)
    {
        //until Lv71 - if more than 2 targets are present, we skip Burst Strike entirely
        var lv30to71 = !ShouldUseAOECircle(5).OnThreeOrMore && In3y(target) && CanBS;
        //if more than 1 target is present, we choose Fated Circle over Burst Strike
        var lv72plus = ShouldUseAOECircle(5).OnTwoOrMore ? (In5y(target) && CanFC) : (In3y(target) && CanBS);
        var condition = Unlocked(AID.FatedCircle) ? lv72plus : lv30to71;
        var slow = SkSGCDLength >= 2.4800f;
        var fast = SkSGCDLength <= 2.4799f;

        var open = ((fast && CombatTimer < 30 && ComboLastMove is AID.BrutalShell) ||
                   (slow && CombatTimer < 30 && ComboLastMove is AID.KeenEdge) ||
                   CombatTimer >= 30);

        return strategy == CartridgeStrategy.Automatic &&
            //minimal
            (InsideCombatWith(target) && condition && open &&
            //if we have No Mercy, spend as much as possible
            ((HasNM ||
            //when Lv90+, if we enter No Mercy with 3 Ammo it is a loss, as we Cannot get 9 GCDs inside a 20 second window
            //technically we Can with Skill Speed, but we're not working around that here
            //so, if Bloodfest & No Mercy are imminent and Ammo is 3, burn a cartridge to enter Burst with 2
            (!InOddWindow(AID.Bloodfest) && NMcd < 1 && Ammo == 3)) ||
            //overcap protection
            (Ammo == MaxCartridges && ComboLastMove is AID.BrutalShell or AID.DemonSlice)));
    }
    private bool ShouldUseLightningShot(LightningShotStrategy strategy, Actor? target) => strategy switch
    {
        LightningShotStrategy.OpenerFar => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD() && !In3y(target),
        LightningShotStrategy.OpenerForce => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD(),
        LightningShotStrategy.Force => true,
        LightningShotStrategy.Allow => !In3y(target),
        LightningShotStrategy.Forbid or _ => false,
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithBloodfest => NMcd < 5 && BFcd < 15,
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        Ammo = gauge.Ammo;
        GunComboStep = gauge.AmmoComboStep;
        MaxCartridges = Unlocked(TraitID.CartridgeChargeII) ? 3 : 2;
        BFcd = TotalCD(AID.Bloodfest);
        NMcd = TotalCD(AID.NoMercy);
        HasNM = NMcd is >= 39.5f and <= 60;
        HasBlast = Unlocked(AID.Hypervelocity) && PlayerHasEffect(SID.ReadyToBlast, 10f) && !LastActionUsed(AID.Hypervelocity);
        HasRaze = Unlocked(AID.FatedBrand) && PlayerHasEffect(SID.ReadyToRaze, 10f) && !LastActionUsed(AID.FatedBrand);
        HasRip = PlayerHasEffect(SID.ReadyToRip, 10f) && !LastActionUsed(AID.JugularRip);
        HasTear = PlayerHasEffect(SID.ReadyToTear, 10f) && !LastActionUsed(AID.AbdomenTear);
        HasGouge = PlayerHasEffect(SID.ReadyToGouge, 10f) && !LastActionUsed(AID.EyeGouge);
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 3.5f, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.ReignOfBeasts) && NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        BestDOTTarget = Hints.PriorityTargets.Where(x => Player.DistanceToHitbox(x.Actor) <= 3.5f).OrderByDescending(x => (float)x.Actor.HPMP.CurHP / x.Actor.HPMP.MaxHP).FirstOrDefault();
        CanBS = Unlocked(AID.BurstStrike) && Ammo > 0;
        CanGF = ActionReady(AID.GnashingFang) && Ammo > 0;
        CanFC = Unlocked(AID.FatedCircle) && Ammo > 0;
        CanDD = ActionReady(AID.DoubleDown) && Ammo > 0;
        CanBreak = Unlocked(AID.SonicBreak) && PlayerHasEffect(SID.ReadyToBreak, 30);
        CanReign = Unlocked(AID.ReignOfBeasts) && PlayerHasEffect(SID.ReadyToReign, 30);

        #region Strategy Definitions
        var AOE = strategy.Option(SharedTrack.AOE);
        var AOEStrategy = AOE.As<AOEStrategy>();
        var combo = strategy.Option(Track.Combo);
        var comboStrat = combo.As<ComboStrategy>();
        var carts = strategy.Option(Track.Cartridges);
        var cartStrat = carts.As<CartridgeStrategy>();
        var nm = strategy.Option(Track.NoMercy);
        var nmStrat = nm.As<NoMercyStrategy>();
        var zone = strategy.Option(Track.Zone);
        var zoneStrat = zone.As<OGCDStrategy>();
        var bow = strategy.Option(Track.BowShock);
        var bowStrat = bow.As<OGCDStrategy>();
        var bf = strategy.Option(Track.Bloodfest);
        var bfStrat = bf.As<BloodfestStrategy>();
        var dd = strategy.Option(Track.DoubleDown);
        var ddStrat = dd.As<DoubleDownStrategy>();
        var gf = strategy.Option(Track.GnashingFang);
        var gfStrat = gf.As<GnashingStrategy>();
        var reign = strategy.Option(Track.Reign);
        var reignStrat = reign.As<ReignStrategy>();
        var sb = strategy.Option(Track.SonicBreak);
        var sbStrat = sb.As<SonicBreakStrategy>();
        var ls = strategy.Option(Track.LightningShot);
        var lsStrat = ls.As<LightningShotStrategy>();
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotations
        if (strategy.AutoFinish() && InsideCombatWith(primaryTarget?.Actor))
            QueueGCD(AutoFinish, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ExtremelyLow);
        if (strategy.AutoBreak() && InsideCombatWith(primaryTarget?.Actor))
            QueueGCD(AutoBreak, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ExtremelyLow);
        if (strategy.ForceST() && InsideCombatWith(primaryTarget?.Actor))
        {
            if (comboStrat != ComboStrategy.ForceSTwithoutO)
                QueueGCD(STwithOvercap, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.BelowAverage);
            if (comboStrat != ComboStrategy.ForceSTwithO)
                QueueGCD(STwithoutOvercap, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.Forced);
        }
        if (strategy.ForceAOE() && InsideCombatWith(primaryTarget?.Actor))
        {
            if (comboStrat != ComboStrategy.ForceAOEwithoutO)
                QueueGCD(AOEwithOvercap, Player, GCDPriority.BelowAverage);
            if (comboStrat != ComboStrategy.ForceAOEwithO)
                QueueGCD(AOEwithoutOvercap, Player, GCDPriority.Forced);
        }
        #endregion

        #region Cooldowns
        if (!strategy.HoldCDs())
        {
            if (!strategy.HoldBuffs())
            {
                if (ShouldUseNoMercy(nmStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.NoMercy, Player, nmStrat is NoMercyStrategy.Force or NoMercyStrategy.ForceW or NoMercyStrategy.ForceQW or NoMercyStrategy.Force1 or NoMercyStrategy.Force1W or NoMercyStrategy.Force1QW or NoMercyStrategy.Force2 or NoMercyStrategy.Force2W or NoMercyStrategy.Force2QW or NoMercyStrategy.Force3 or NoMercyStrategy.Force3W or NoMercyStrategy.Force3QW ? OGCDPriority.Forced : OGCDPriority.VeryHigh);
                if (ShouldUseBloodfest(bfStrat, primaryTarget?.Actor))
                    QueueOGCD(AID.Bloodfest, TargetChoice(bf) ?? primaryTarget?.Actor, bfStrat is BloodfestStrategy.Force or BloodfestStrategy.ForceW or BloodfestStrategy.Force0 or BloodfestStrategy.Force0W ? OGCDPriority.Forced : OGCDPriority.High);
            }
            if (!strategy.HoldGauge())
            {
                if (ShouldUseDoubleDown(ddStrat, primaryTarget?.Actor))
                    QueueGCD(AID.DoubleDown, primaryTarget?.Actor, ddStrat is DoubleDownStrategy.Force or DoubleDownStrategy.Force1 or DoubleDownStrategy.Force2 or DoubleDownStrategy.Force3 ? GCDPriority.Forced : Ammo == 1 ? GCDPriority.VeryHigh : GCDPriority.AboveAverage);
                if (ShouldUseGnashingFang(gfStrat, primaryTarget?.Actor))
                {
                    if (gfStrat == GnashingStrategy.Automatic)
                        QueueGCD(AID.GnashingFang, TargetChoice(gf) ?? primaryTarget?.Actor, GunComboStep is 1 or 2 ? GCDPriority.BelowAverage : GCDPriority.High);
                    if (gfStrat is GnashingStrategy.Automatic or GnashingStrategy.ForceGnash or GnashingStrategy.ForceGnash1 or GnashingStrategy.ForceGnash2 or GnashingStrategy.ForceGnash3)
                        QueueGCD(AID.GnashingFang, primaryTarget?.Actor, GCDPriority.Forced);
                }
                if (ShouldUseCartridges(cartStrat, primaryTarget?.Actor))
                {
                    if (cartStrat != CartridgeStrategy.Delay)
                    {
                        if (cartStrat == CartridgeStrategy.Automatic)
                            QueueGCD(BestCartSpender, TargetChoice(carts) ?? primaryTarget?.Actor, NMcd < 1 && Ammo == 3 ? GCDPriority.Forced : GCDPriority.VeryLow);
                        if (cartStrat is CartridgeStrategy.OnlyBS or CartridgeStrategy.ForceBS or CartridgeStrategy.ForceBS1 or CartridgeStrategy.ForceBS2 or CartridgeStrategy.ForceBS3)
                            QueueGCD(AID.BurstStrike, TargetChoice(carts) ?? primaryTarget?.Actor, GCDPriority.VeryLow);
                        if (cartStrat is CartridgeStrategy.ForceFC or CartridgeStrategy.OnlyFC or CartridgeStrategy.ForceFC1 or CartridgeStrategy.ForceFC2 or CartridgeStrategy.ForceFC3)
                            QueueGCD(Unlocked(AID.FatedCircle) ? AID.FatedCircle : AID.BurstStrike, Unlocked(AID.FatedCircle) ? Player : primaryTarget?.Actor, GCDPriority.VeryLow);
                    }
                }
            }
            if (ShouldUseZone(zoneStrat, primaryTarget?.Actor))
                QueueOGCD(Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone, TargetChoice(zone) ?? primaryTarget?.Actor, OGCDPrio(zoneStrat, OGCDPriority.Average));
            if (ShouldUseBowShock(bowStrat, primaryTarget?.Actor))
                QueueOGCD(AID.BowShock, Player, OGCDPrio(bowStrat, OGCDPriority.AboveAverage));
            if (ShouldUseSonicBreak(sbStrat, primaryTarget?.Actor))
                QueueGCD(AID.SonicBreak, TargetChoice(sb) ?? BestDOTTarget?.Actor, sbStrat is SonicBreakStrategy.Force ? GCDPriority.Forced : sbStrat is SonicBreakStrategy.Early ? GCDPriority.VeryHigh : GCDPriority.Average);
            if (ShouldUseReign(reignStrat, primaryTarget?.Actor))
            {
                if (reignStrat == ReignStrategy.Automatic)
                    QueueGCD(GunComboStep == 4 ? AID.LionHeart : GunComboStep == 3 ? AID.NobleBlood : AID.ReignOfBeasts, TargetChoice(reign) ?? BestSplashTarget?.Actor, GCDPriority.BelowAverage);
                if (reignStrat == ReignStrategy.ForceReign)
                    QueueGCD(AID.ReignOfBeasts, TargetChoice(reign) ?? BestSplashTarget?.Actor, GCDPriority.Forced);
            }
        }
        if (gfStrat == GnashingStrategy.ForceClaw || GunComboStep is 1)
            QueueGCD(AID.SavageClaw, primaryTarget?.Actor, gfStrat != GnashingStrategy.ForceClaw ? GCDPriority.BelowAverage : GCDPriority.Forced);
        if (gfStrat == GnashingStrategy.ForceTalon || GunComboStep is 2)
            QueueGCD(AID.WickedTalon, primaryTarget?.Actor, gfStrat != GnashingStrategy.ForceTalon ? GCDPriority.BelowAverage : GCDPriority.Forced);
        if (ShouldUseReign(reignStrat, primaryTarget?.Actor))
        {
            if (reignStrat == ReignStrategy.ForceNoble)
                QueueGCD(AID.NobleBlood, TargetChoice(reign) ?? BestSplashTarget?.Actor, GCDPriority.Forced);
            if (reignStrat == ReignStrategy.ForceLion)
                QueueGCD(AID.LionHeart, TargetChoice(reign) ?? BestSplashTarget?.Actor, GCDPriority.Forced);
        }

        if (ShouldUseContinuation(strategy.Option(Track.Continuation).As<ContinuationStrategy>(), primaryTarget?.Actor))
            QueueOGCD(BestContinuation,
                TargetChoice(strategy.Option(Track.Continuation)) ?? primaryTarget?.Actor,
                GCD is < 0.5f and 0 ? OGCDPriority.Forced + 1500 : //add a LOT of prio
                strategy.Option(Track.Continuation).As<ContinuationStrategy>() == ContinuationStrategy.Early ? OGCDPriority.Forced : //add some prio
                GCD is < 1.25f and >= 0.6f ? OGCDPriority.VeryHigh - 10 : //add a little bit of prio
                OGCDPriority.BelowAverage); //default prio

        if (ShouldUseLightningShot(lsStrat, primaryTarget?.Actor))
            QueueGCD(AID.LightningShot, TargetChoice(ls) ?? primaryTarget?.Actor, lsStrat is >= LightningShotStrategy.Force ? GCDPriority.Forced : GCDPriority.ExtremelyLow);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, 0, GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.DemonSlice, 2, maximumActionRange: 20);
        #endregion
    }
}
