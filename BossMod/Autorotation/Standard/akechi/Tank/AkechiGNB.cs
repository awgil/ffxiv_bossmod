using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.GNB;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { AOE, Cooldowns, Cartridges, Potion, LightningShot, NoMercy, SonicBreak, GnashingFang, Reign, Bloodfest, DoubleDown, Zone, BowShock }
    public enum AOEStrategy { AutoFinish, AutoBreak, ForceSTwithO, ForceSTwithoutO, ForceAOEwithO, ForceAOEwithoutO }
    public enum CooldownStrategy { Allow, Forbid }
    public enum CartridgeStrategy { Automatic, OnlyBS, OnlyFC, ForceBS, ForceBS1, ForceBS2, ForceBS3, ForceFC, ForceFC1, ForceFC2, ForceFC3, Conserve }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    public enum LightningShotStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }
    public enum NoMercyStrategy { Automatic, Force, ForceW, ForceQW, Force1, Force1W, Force1QW, Force2, Force2W, Force2QW, Force3, Force3W, Force3QW, Delay }
    public enum SonicBreakStrategy { Automatic, Force, Early, Late, Delay }
    public enum GnashingStrategy { Automatic, ForceGnash, ForceGnash1, ForceGnash2, ForceGnash3, ForceClaw, ForceTalon, Delay }
    public enum ReignStrategy { Automatic, ForceReign, ForceNoble, ForceLion, Delay }
    public enum BloodfestStrategy { Automatic, Force, ForceW, Force0, Force0W, Delay }
    public enum DoubleDownStrategy { Automatic, Force, Force1, Force2, Force3, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.GNB), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoFinish, "Auto (Finish combo)", "Automatically execute optimal rotation based on targets; finishes combo if possible", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoBreak, "Auto (Break combo)", "Automatically execute optimal rotation based on targets; breaks combo if necessary", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceSTwithO, "Force ST with Overcap", "Force single-target rotation with overcap protection", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceSTwithoutO, "Force ST without Overcap", "Force ST rotation without overcap protection", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOEwithO, "Force AOE with Overcap", "Force AOE rotation with overcap protection")
            .AddOption(AOEStrategy.ForceAOEwithoutO, "Force AOE without Overcap", "Force AOE rotation without overcap protection")
            .AddAssociatedActions(AID.KeenEdge, AID.BrutalShell, AID.SolidBarrel, AID.DemonSlice, AID.DemonSlaughter);
        res.Define(Track.Cooldowns).As<CooldownStrategy>("Hold", uiPriority: 190)
            .AddOption(CooldownStrategy.Allow, "Allow", "Allows the use of all cooldowns & buffs; will use them optimally")
            .AddOption(CooldownStrategy.Forbid, "Hold", "Forbids the use of all cooldowns & buffs; will not use any actions with a cooldown timer");
        res.Define(Track.Cartridges).As<CartridgeStrategy>("Cartridges", "Carts", uiPriority: 180)
            .AddOption(CartridgeStrategy.Automatic, "Automatic", "Automatically decide when to use cartridges; uses them optimally")
            .AddOption(CartridgeStrategy.OnlyBS, "Only Burst Strike", "Uses Burst Strike optimally as cartridge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.OnlyFC, "Only Fated Circle", "Uses Fated Circle optimally as cartridge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.ForceBS, "Force Burst Strike", "Force use of Burst Strike regardless of cartridge count", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS1, "Force Burst Strike (1 cart)", "Force use of Burst Strike when only 1 cartridge is available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS2, "Force Burst Strike (2 cart)", "Force use of Burst Strike when only 2 cartridges are available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS3, "Force Burst Strike (3 cart)", "Force use of Burst Strike when only 3 cartridges are available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceFC, "Force Fated Circle", "Force use of Fated Circle when any cartridges are available", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.ForceFC1, "Force Fated Circle (1 cart)", "Force use of Fated Circle when only 1 cartridge is available", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.ForceFC2, "Force Fated Circle (2 cart)", "Force use of Fated Circle when only 2 cartridges are available", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.ForceFC3, "Force Fated Circle (3 cart)", "Force use of Fated Circle when only 3 cartridges are available", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.Conserve, "Conserve", "Forbid use of Burst Strike & Fated Circle", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.BurstStrike, AID.FatedCircle);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 90)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with No Mercy & Bloodfest together (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.LightningShot).As<LightningShotStrategy>("Lightning Shot", "L.Shot", uiPriority: 100)
            .AddOption(LightningShotStrategy.OpenerFar, "Far (Opener)", "Use Lightning Shot in pre-pull & out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.OpenerForce, "Force (Opener)", "Force use Lightning Shot in pre-pull in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Force, "Force", "Force use Lightning Shot in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Allow, "Allow", "Allow use of Lightning Shot when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Forbid, "Forbid", "Prohibit use of Lightning Shot")
            .AddAssociatedActions(AID.LightningShot);
        res.Define(Track.NoMercy).As<NoMercyStrategy>("No Mercy", "N.Mercy", uiPriority: 160)
            .AddOption(NoMercyStrategy.Automatic, "Auto", "Normal use of No Mercy")
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
        res.Define(Track.Reign).As<ReignStrategy>("Reign of Beasts", "Reign", uiPriority: 125)
            .AddOption(ReignStrategy.Automatic, "Auto", "Normal use of Reign of Beasts")
            .AddOption(ReignStrategy.ForceReign, "Force", "Force use of Reign of Beasts", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.ForceNoble, "Force", "Force use of Noble Blood", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.ForceLion, "Force", "Force use of Lion Heart", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.Delay, "Delay", "Delay use of Reign of Beasts", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(AID.ReignOfBeasts, AID.NobleBlood, AID.LionHeart);
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
        res.DefineOGCD(Track.Zone, AID.DangerZone, "Zone", "Zone", uiPriority: 135, 30, 0, ActionTargets.Hostile, 18).AddAssociatedActions(AID.BlastingZone, AID.DangerZone);
        res.DefineOGCD(Track.BowShock, AID.BowShock, "BowShock", "B.Shock", uiPriority: 140, 60, 15, ActionTargets.Self, 62);

        return res;
    }
    #endregion

    #region Module Variables
    private byte Ammo;
    private byte GunComboStep;
    private int MaxCartridges;
    private float nmLeft;
    private float nmCD;
    private float bfCD;
    private bool inOdd;
    private bool hasNM;
    private bool hasBreak;
    private bool hasReign;
    private bool hasBlast;
    private bool hasRaze;
    private bool hasRip;
    private bool hasTear;
    private bool hasGouge;
    private bool canNM;
    private bool canBS;
    private bool canGF;
    private bool canFC;
    private bool canDD;
    private bool canBF;
    private bool canZone;
    private bool canBreak;
    private bool canBow;
    private bool canContinue;
    private bool canReign;
    private bool ShouldUseAOE;
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestSplashTarget;
    private Enemy? BestDOTTarget;
    #endregion

    #region Upgrade Paths
    private AID BestZone => Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone;
    private AID BestCartSpender => ShouldUseAOE ? BestFatedCircle : canBS ? AID.BurstStrike : AutoFinish;
    private AID BestFatedCircle => Unlocked(AID.FatedCircle) ? AID.FatedCircle : AID.BurstStrike;
    private AID BestContinuation => hasRaze ? AID.FatedBrand : hasBlast ? AID.Hypervelocity : hasGouge ? AID.EyeGouge : hasTear ? AID.AbdomenTear : hasRip ? AID.JugularRip : AID.Continuation;
    #endregion

    #region Rotation Helpers
    private AID AutoFinish => ComboLastMove switch
    {
        AID.KeenEdge or AID.BrutalShell => STwithoutOvercap,
        AID.DemonSlice => AOEwithoutOvercap,
        AID.SolidBarrel or AID.DemonSlaughter or _ => ShouldUseAOE ? AOEwithoutOvercap : STwithoutOvercap,
    };
    private AID AutoBreak => ShouldUseAOE ? AOEwithoutOvercap : STwithoutOvercap;
    private AID STwithOvercap => Ammo == MaxCartridges ? BestCartSpender : Unlocked(AID.SolidBarrel) & ComboLastMove is AID.BrutalShell ? AID.SolidBarrel : Unlocked(AID.BrutalShell) && ComboLastMove is AID.KeenEdge ? AID.BrutalShell : AID.KeenEdge;
    private AID STwithoutOvercap => Unlocked(AID.SolidBarrel) && ComboLastMove is AID.BrutalShell ? AID.SolidBarrel : Unlocked(AID.BrutalShell) && ComboLastMove is AID.KeenEdge ? AID.BrutalShell : AID.KeenEdge;
    private AID AOEwithOvercap => Ammo == MaxCartridges ? BestCartSpender : Unlocked(AID.DemonSlaughter) && ComboLastMove is AID.DemonSlice ? AID.DemonSlaughter : AID.DemonSlice;
    private AID AOEwithoutOvercap => Unlocked(AID.DemonSlaughter) && ComboLastMove is AID.DemonSlice ? AID.DemonSlaughter : AID.DemonSlice;
    #endregion

    #region Cooldown Helpers
    private bool ShouldUseNoMercy(NoMercyStrategy strategy, Actor? target) => strategy switch
    {
        NoMercyStrategy.Automatic => Player.InCombat && target != null && canNM &&
            ((Unlocked(AID.DoubleDown) && (inOdd && Ammo >= 2 || !inOdd && Ammo < 3)) || //90+
            (!Unlocked(AID.DoubleDown) && CanQuarterWeaveIn && Ammo >= 1)), //2-89
        NoMercyStrategy.Force => canNM,
        NoMercyStrategy.ForceW => canNM && CanWeaveIn,
        NoMercyStrategy.ForceQW => canNM && CanQuarterWeaveIn,
        NoMercyStrategy.Force1 => canNM && Ammo == 1,
        NoMercyStrategy.Force1W => canNM && CanWeaveIn && Ammo == 1,
        NoMercyStrategy.Force1QW => canNM && CanQuarterWeaveIn && Ammo == 1,
        NoMercyStrategy.Force2 => canNM && Ammo == 2,
        NoMercyStrategy.Force2W => canNM && CanWeaveIn && Ammo == 2,
        NoMercyStrategy.Force2QW => canNM && CanQuarterWeaveIn && Ammo == 2,
        NoMercyStrategy.Force3 => canNM && Ammo == 3,
        NoMercyStrategy.Force3W => canNM && CanWeaveIn && Ammo == 3,
        NoMercyStrategy.Force3QW => canNM && CanQuarterWeaveIn && Ammo == 3,
        NoMercyStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBloodfest(BloodfestStrategy strategy, Actor? target) => strategy switch
    {
        BloodfestStrategy.Automatic => Player.InCombat && target != null && canBF && Ammo == 0,
        BloodfestStrategy.Force => canBF,
        BloodfestStrategy.ForceW => canBF && CanWeaveIn,
        BloodfestStrategy.Force0 => canBF && Ammo == 0,
        BloodfestStrategy.Force0W => canBF && Ammo == 0 && CanWeaveIn,
        BloodfestStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseZone(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && In3y(target) && CanWeaveIn && canZone && nmCD is < 57.55f and > 17,
        OGCDStrategy.Force => canZone,
        OGCDStrategy.AnyWeave => canZone && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canZone && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canZone && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBowShock(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && ActionReady(AID.BowShock) && In5y(target) && CanWeaveIn && nmCD is < 57.55f and >= 40,
        OGCDStrategy.Force => canBow,
        OGCDStrategy.AnyWeave => canBow && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canBow && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canBow && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseCartridges(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic => ShouldSpendCarts(CartridgeStrategy.Automatic, target),
        CartridgeStrategy.OnlyBS => ShouldSpendCarts(CartridgeStrategy.Automatic, target),
        CartridgeStrategy.OnlyFC => ShouldSpendCarts(CartridgeStrategy.Automatic, target),
        CartridgeStrategy.ForceBS => canBS,
        CartridgeStrategy.ForceBS1 => canBS && Ammo == 1,
        CartridgeStrategy.ForceBS2 => canBS && Ammo == 2,
        CartridgeStrategy.ForceBS3 => canBS && Ammo == 3,
        CartridgeStrategy.ForceFC => canFC,
        CartridgeStrategy.ForceFC1 => canFC && Ammo == 1,
        CartridgeStrategy.ForceFC2 => canFC && Ammo == 2,
        CartridgeStrategy.ForceFC3 => canFC && Ammo == 3,
        CartridgeStrategy.Conserve => false,
        _ => false
    };
    private bool ShouldUseDoubleDown(DoubleDownStrategy strategy, Actor? target) => strategy switch
    {
        DoubleDownStrategy.Automatic => Player.InCombat && target != null && In5y(target) && canDD && hasNM,
        DoubleDownStrategy.Force => canDD,
        DoubleDownStrategy.Force1 => canDD && Ammo == 1,
        DoubleDownStrategy.Force2 => canDD && Ammo == 2,
        DoubleDownStrategy.Force3 => canDD && Ammo == 3,
        DoubleDownStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseGnashingFang(GnashingStrategy strategy, Actor? target) => strategy switch
    {
        GnashingStrategy.Automatic => Player.InCombat && target != null && In3y(target) && canGF && (nmLeft > 0 || hasNM || nmCD is < 35 and > 17),
        GnashingStrategy.ForceGnash => canGF,
        GnashingStrategy.ForceGnash1 => canGF && Ammo == 1,
        GnashingStrategy.ForceGnash2 => canGF && Ammo == 2,
        GnashingStrategy.ForceGnash3 => canGF && Ammo == 3,
        GnashingStrategy.ForceClaw => Player.InCombat && GunComboStep == 1,
        GnashingStrategy.ForceTalon => Player.InCombat && GunComboStep == 2,
        GnashingStrategy.Delay => false,
        _ => false
    };
    private bool ShouldSpendCarts(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic => Player.InCombat && target != null &&
            ((ShouldUseAOE ? (In5y(target) && canFC) : (In3y(target) && canBS)) &&
            (hasNM || (!(bfCD is <= 90 and >= 30) && nmCD < 1 && Ammo == 3)) ||
            (Ammo == MaxCartridges && ComboLastMove is AID.BrutalShell or AID.DemonSlice)),
        _ => false
    };
    private bool ShouldUseSonicBreak(SonicBreakStrategy strategy, Actor? target) => strategy switch
    {
        SonicBreakStrategy.Automatic => Player.InCombat && In3y(target) && canBreak,
        SonicBreakStrategy.Force => canBreak,
        SonicBreakStrategy.Early => nmCD > 55 || hasBreak,
        SonicBreakStrategy.Late => nmLeft <= SkSGCDLength,
        SonicBreakStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseReign(ReignStrategy strategy, Actor? target) => strategy switch
    {
        ReignStrategy.Automatic => Player.InCombat && target != null && canReign && hasNM && GunComboStep == 0,
        ReignStrategy.ForceReign => canReign,
        ReignStrategy.ForceNoble => Player.InCombat && GunComboStep == 3,
        ReignStrategy.ForceLion => Player.InCombat && GunComboStep == 4,
        ReignStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseLightningShot(Actor? target, LightningShotStrategy strategy) => strategy switch
    {
        LightningShotStrategy.OpenerFar => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD() && !In3y(target),
        LightningShotStrategy.OpenerForce => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD(),
        LightningShotStrategy.Force => true,
        LightningShotStrategy.Allow => !In3y(target),
        LightningShotStrategy.Forbid => false,
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => nmCD < 5 && bfCD < 15,
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
        bfCD = TotalCD(AID.Bloodfest);
        nmCD = TotalCD(AID.NoMercy);
        nmLeft = StatusRemaining(Player, SID.NoMercy, 20);
        hasBreak = PlayerHasEffect(SID.ReadyToBreak, 30);
        hasReign = PlayerHasEffect(SID.ReadyToReign, 30);
        hasNM = nmCD is >= 39.5f and <= 60;
        hasBlast = Unlocked(AID.Hypervelocity) && PlayerHasEffect(SID.ReadyToBlast, 10f) && !LastActionUsed(AID.Hypervelocity);
        hasRaze = Unlocked(AID.FatedBrand) && PlayerHasEffect(SID.ReadyToRaze, 10f) && !LastActionUsed(AID.FatedBrand);
        hasRip = PlayerHasEffect(SID.ReadyToRip, 10f) && !LastActionUsed(AID.JugularRip);
        hasTear = PlayerHasEffect(SID.ReadyToTear, 10f) && !LastActionUsed(AID.AbdomenTear);
        hasGouge = PlayerHasEffect(SID.ReadyToGouge, 10f) && !LastActionUsed(AID.EyeGouge);
        inOdd = bfCD is <= 90 and >= 30;
        ShouldUseAOE = ShouldUseAOECircle(5).OnTwoOrMore;
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 3.5f, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.ReignOfBeasts) && NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        BestDOTTarget = Hints.PriorityTargets.Where(x => Player.DistanceToHitbox(x.Actor) <= 3.5f).OrderByDescending(x => (float)x.Actor.HPMP.CurHP / x.Actor.HPMP.MaxHP).FirstOrDefault();
        canNM = ActionReady(AID.NoMercy);
        canBS = Unlocked(AID.BurstStrike) && Ammo > 0;
        canGF = ActionReady(AID.GnashingFang) && Ammo > 0;
        canFC = Unlocked(AID.FatedCircle) && Ammo > 0;
        canDD = ActionReady(AID.DoubleDown) && Ammo > 0;
        canBF = ActionReady(AID.Bloodfest);
        canZone = ActionReady(AID.DangerZone);
        canBreak = Unlocked(AID.SonicBreak) && hasBreak;
        canBow = ActionReady(AID.BowShock);
        canContinue = Unlocked(AID.Continuation);
        canReign = Unlocked(AID.ReignOfBeasts) && hasReign;

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE);
        var AOEStrategy = AOE.As<AOEStrategy>();
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
        var hold = strategy.Option(Track.Cooldowns).As<CooldownStrategy>() == CooldownStrategy.Forbid;
        var conserve = cartStrat == CartridgeStrategy.Conserve;
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotations
        if (AOEStrategy == AOEStrategy.AutoFinish)
            QueueGCD(AutoFinish, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ExtremelyLow);
        if (AOEStrategy == AOEStrategy.AutoBreak)
            QueueGCD(AutoBreak, ShouldUseAOE ? Player : TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.ExtremelyLow);
        if (AOEStrategy is AOEStrategy.ForceSTwithO)
            QueueGCD(STwithOvercap, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.BelowAverage);
        if (AOEStrategy is AOEStrategy.ForceSTwithoutO)
            QueueGCD(STwithoutOvercap, TargetChoice(AOE) ?? primaryTarget?.Actor, GCDPriority.BelowAverage);
        if (AOEStrategy is AOEStrategy.ForceAOEwithO)
            QueueGCD(AOEwithOvercap, Player, GCDPriority.BelowAverage);
        if (AOEStrategy is AOEStrategy.ForceAOEwithoutO)
            QueueGCD(AOEwithoutOvercap, Player, GCDPriority.BelowAverage);
        #endregion

        #region Cooldowns
        if (!hold)
        {
            if (ShouldUseNoMercy(nmStrat, primaryTarget?.Actor))
                QueueOGCD(AID.NoMercy, Player, nmStrat is NoMercyStrategy.Force or NoMercyStrategy.ForceW or NoMercyStrategy.ForceQW or NoMercyStrategy.Force1 or NoMercyStrategy.Force1W or NoMercyStrategy.Force1QW or NoMercyStrategy.Force2 or NoMercyStrategy.Force2W or NoMercyStrategy.Force2QW or NoMercyStrategy.Force3 or NoMercyStrategy.Force3W or NoMercyStrategy.Force3QW ? OGCDPriority.Forced : OGCDPriority.VeryHigh);
            if (ShouldUseZone(zoneStrat, primaryTarget?.Actor))
                QueueOGCD(BestZone, TargetChoice(zone) ?? primaryTarget?.Actor, OGCDPrio(zoneStrat, OGCDPriority.Average));
            if (ShouldUseBowShock(bowStrat, primaryTarget?.Actor))
                QueueOGCD(AID.BowShock, Player, OGCDPrio(bowStrat, OGCDPriority.AboveAverage));
            if (ShouldUseBloodfest(bfStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Bloodfest, TargetChoice(bf) ?? primaryTarget?.Actor, bfStrat is BloodfestStrategy.Force or BloodfestStrategy.ForceW or BloodfestStrategy.Force0 or BloodfestStrategy.Force0W ? OGCDPriority.Forced : OGCDPriority.High);
            if (ShouldUseSonicBreak(sbStrat, primaryTarget?.Actor))
                QueueGCD(AID.SonicBreak, TargetChoice(sb) ?? BestDOTTarget?.Actor, sbStrat is SonicBreakStrategy.Force ? GCDPriority.Forced : sbStrat is SonicBreakStrategy.Early ? GCDPriority.VeryHigh : GCDPriority.Average);
            if (ShouldUseReign(reignStrat, primaryTarget?.Actor))
                QueueGCD(AID.ReignOfBeasts, TargetChoice(reign) ?? BestSplashTarget?.Actor, reignStrat is ReignStrategy.ForceReign ? GCDPriority.Forced : GCDPriority.BelowAverage);

            if (!conserve)
            {
                if (ShouldUseDoubleDown(ddStrat, primaryTarget?.Actor))
                    QueueGCD(AID.DoubleDown, primaryTarget?.Actor, ddStrat is DoubleDownStrategy.Force or DoubleDownStrategy.Force1 or DoubleDownStrategy.Force2 or DoubleDownStrategy.Force3 ? GCDPriority.Forced : Ammo == 1 ? GCDPriority.VeryHigh : GCDPriority.AboveAverage);
                if (ShouldUseGnashingFang(gfStrat, primaryTarget?.Actor))
                    QueueGCD(AID.GnashingFang, TargetChoice(gf) ?? primaryTarget?.Actor, gfStrat is GnashingStrategy.ForceGnash or GnashingStrategy.ForceGnash1 or GnashingStrategy.ForceGnash2 or GnashingStrategy.ForceGnash3 ? GCDPriority.Forced : GCDPriority.High);
                if (ShouldUseCartridges(cartStrat, primaryTarget?.Actor))
                {
                    if (cartStrat == CartridgeStrategy.Automatic)
                        QueueGCD(BestCartSpender, TargetChoice(carts) ?? primaryTarget?.Actor, nmCD < 1 && Ammo == 3 ? GCDPriority.Forced : GCDPriority.VeryLow);
                    if (cartStrat is CartridgeStrategy.OnlyBS or CartridgeStrategy.ForceBS or CartridgeStrategy.ForceBS1 or CartridgeStrategy.ForceBS2 or CartridgeStrategy.ForceBS3)
                        QueueGCD(AID.BurstStrike, TargetChoice(carts) ?? primaryTarget?.Actor, GCDPriority.VeryLow);
                    if (cartStrat is CartridgeStrategy.ForceFC or CartridgeStrategy.OnlyFC or CartridgeStrategy.ForceFC1 or CartridgeStrategy.ForceFC2 or CartridgeStrategy.ForceFC3)
                        QueueGCD(BestFatedCircle, Unlocked(AID.FatedCircle) ? Player : primaryTarget?.Actor, GCDPriority.VeryLow);
                }
            }
        }
        if (canContinue && (hasBlast || hasRaze || hasRip || hasTear || hasGouge))
            QueueOGCD(BestContinuation, TargetChoice(gf) ?? primaryTarget?.Actor, GCD < 0.4f ? OGCDPriority.Forced + 1500 : OGCDPriority.BelowAverage);
        if (GunComboStep == 1)
            QueueGCD(AID.SavageClaw, TargetChoice(gf) ?? primaryTarget?.Actor, gfStrat is GnashingStrategy.ForceClaw ? GCDPriority.Forced : GCDPriority.BelowAverage);
        if (GunComboStep == 2)
            QueueGCD(AID.WickedTalon, TargetChoice(gf) ?? primaryTarget?.Actor, gfStrat is GnashingStrategy.ForceTalon ? GCDPriority.Forced : GCDPriority.BelowAverage);
        if (GunComboStep == 3)
            QueueGCD(AID.NobleBlood, TargetChoice(reign) ?? BestSplashTarget?.Actor, reignStrat is ReignStrategy.ForceNoble ? GCDPriority.Forced : GCDPriority.BelowAverage);
        if (GunComboStep == 4)
            QueueGCD(AID.LionHeart, TargetChoice(reign) ?? BestSplashTarget?.Actor, reignStrat is ReignStrategy.ForceLion ? GCDPriority.Forced : GCDPriority.BelowAverage);
        if (ShouldUseLightningShot(primaryTarget?.Actor, lsStrat))
            QueueGCD(AID.LightningShot, TargetChoice(ls) ?? primaryTarget?.Actor, lsStrat is >= LightningShotStrategy.Force ? GCDPriority.Forced : GCDPriority.ExtremelyLow);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, 0, GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        var goalST = primaryTarget?.Actor != null ? Hints.GoalSingleTarget(primaryTarget!.Actor, 3) : null;
        var goalAOE = Hints.GoalAOECircle(3);
        var goal = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.ForceSTwithO => goalST,
            AOEStrategy.ForceSTwithoutO => goalST,
            AOEStrategy.ForceAOEwithO => goalAOE,
            AOEStrategy.ForceAOEwithoutO => goalAOE,
            _ => goalST != null ? Hints.GoalCombined(goalST, goalAOE, 2) : goalAOE
        };
        if (goal != null)
            Hints.GoalZones.Add(goal);
        #endregion
    }
}
