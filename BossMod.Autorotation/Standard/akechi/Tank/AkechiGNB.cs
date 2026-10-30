using BossMod.GNB;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.akechi;

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    public enum Track { AOE = SharedTrack.Count, Cartridges, LightningShot, Zone, NoMercy, SonicBreak, GnashingFang, BowShock, Continuation, Bloodfest, DoubleDown, Reign }
    public enum AOEStrategy
    {
        AutoFinishWithOvercap, AutoFinishWithoutOvercap,
        ForceSTFinishWithOvercap, ForceSTFinishWithoutOvercap,
        ForceAOEFinishWithOvercap, ForceAOEFinishWithoutOvercap,
        AutoBreakWithOvercap, AutoBreakWithoutOvercap,
        ForceSTBreakWithOvercap, ForceSTBreakWithoutOvercap,
        ForceAOEBreakWithOvercap, ForceAOEBreakWithoutOvercap
    }
    public enum CartridgeStrategy
    {
        Automatic, OnlyBS, OnlyFC,
        ForceBS, ForceBS1, ForceBS2, ForceBS3,
        ForceFC, ForceFC1, ForceFC2, ForceFC3,
        Delay
    }
    public enum LightningShotStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }
    public enum NoMercyStrategy
    {
        Automatic, BurstReady, Together,
        Force, ForceW, ForceQW,
        Force1, Force1W, Force1QW,
        Force2, Force2W, Force2QW,
        Force3, Force3W, Force3QW,
        Delay
    }
    public enum SonicBreakStrategy { Automatic, Early, Late, Force, Delay }
    public enum GnashingStrategy { Automatic, ForceGnash, ForceGnash1, ForceGnash2, ForceGnash3, ForceClaw, ForceTalon, Delay }
    public enum ContinuationStrategy { Automatic, Early, Late }
    public enum BloodfestStrategy { Automatic, Together, Force, ForceW, Delay }
    public enum DoubleDownStrategy { Automatic, Force, Force3, Delay }
    public enum ReignStrategy { Automatic, ForceReign, ForceNoble, ForceLion, Delay }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB", "Standard Rotation Module", "Standard rotation (Akechi)|Tank", "Akechi", RotationModuleQuality.Excellent, BitMask.Build((int)Class.GNB), 100);

        res.DefineTargeting();
        res.DefineHold();
        res.DefinePotion(ActionDefinitions.IDPotionStr);

        res.Define(Track.AOE).As<AOEStrategy>("ST/AOE", "Single-Target & AoE Rotations", 300)
            .AddOption(AOEStrategy.AutoFinishWithOvercap, "Automatically select best rotation based on targets nearby - finishes current combo (1-2-3, GF, or Reign) if possible & protects cartridges from overcapping")
            .AddOption(AOEStrategy.AutoFinishWithoutOvercap, "Automatically select best rotation based on targets nearby - finishes current combo (1-2-3, GF, or Reign) if possible, but does not protect cartridges from overcapping")
            .AddOption(AOEStrategy.ForceSTFinishWithOvercap, "Force Single-Target rotation, regardless of targets nearby - finishes current combo (1-2-3, GF, or Reign) if possible & protects cartridges from overcapping")
            .AddOption(AOEStrategy.ForceSTFinishWithoutOvercap, "Force Single-Target rotation, regardless of targets nearby - finishes current combo (1-2-3, GF, or Reign) if possible, but does not protect cartridges from overcapping")
            .AddOption(AOEStrategy.ForceAOEFinishWithOvercap, "Force AoE rotation, regardless of targets nearby - finishes current combo (1-2-3, GF, or Reign) if possible & protects cartridges from overcapping")
            .AddOption(AOEStrategy.ForceAOEFinishWithoutOvercap, "Force AoE rotation, regardless of targets nearby - finishes current combo (1-2-3, GF, or Reign) if possible, but does not protect cartridges from overcapping")
            .AddOption(AOEStrategy.AutoBreakWithOvercap, "Automatically select best rotation based on targets nearby - will break current combo if in one & protects cartridges from overcapping")
            .AddOption(AOEStrategy.AutoBreakWithoutOvercap, "Automatically select best rotation based on targets nearby - will break any combo if in one & does not protect cartridges from overcapping")
            .AddOption(AOEStrategy.ForceSTBreakWithOvercap, "Force Single-Target rotation, regardless of targets nearby - will break any combo if in one, but protects cartridges from overcapping")
            .AddOption(AOEStrategy.ForceSTBreakWithoutOvercap, "Force Single-Target rotation, regardless of targets nearby - will break any combo if in one & does not protect cartridges from overcapping")
            .AddOption(AOEStrategy.ForceAOEBreakWithOvercap, "Force AoE rotation, regardless of targets nearby - will break any combo if in one, but protects cartridges from overcapping")
            .AddOption(AOEStrategy.ForceAOEBreakWithoutOvercap, "Force AoE rotation, regardless of targets nearby - will break any combo if in one & does not protect cartridges from overcapping")
            .AddAssociatedActions(AID.KeenEdge, AID.BrutalShell, AID.SolidBarrel, AID.DemonSlice, AID.DemonSlaughter);

        res.Define(Track.Cartridges).As<CartridgeStrategy>("Carts", "Cartridge Usage", 199)
            .AddOption(CartridgeStrategy.Automatic, "Automatically use Burst Strike or Fated Circle based on targets nearby", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.OnlyBS, "Automatically use Burst Strike as cartridge spender regardless of targets", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.OnlyFC, "Automatically use Fated Circle as cartridge spender regardless of targets", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.ForceBS, "Force Burst Strike regardless of cartridge count", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS1, "Force Burst Strike when 1 cartridge is available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS2, "Force Burst Strike when 2 cartridges are available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceBS3, "Force Burst Strike when 3 cartridges are available", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceFC, "Force Fated Circle when any cartridges are available", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.ForceFC1, "Force Fated Circle when 1 cartridge is available", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.ForceFC2, "Force Fated Circle when 2 cartridges are available", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.ForceFC3, "Force Fated Circle when 3 cartridges are available", 0, 0, ActionTargets.Self, 72)
            .AddOption(CartridgeStrategy.Delay, "Forbid Burst Strike & Fated Circle", minLevel: 30)
            .AddAssociatedActions(AID.BurstStrike, AID.FatedCircle);

        res.Define(Track.LightningShot).As<LightningShotStrategy>("Ranged", "Lightning Shot", 180)
            .AddOption(LightningShotStrategy.OpenerFar, "Automatically use Lightning Shot in pre-pull if out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.OpenerForce, "Automatically use Lightning Shot in pre-pull from any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Force, "Force Lightning Shot from any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Allow, "Allow Lightning Shot when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Forbid, "Forbid Lightning Shot")
            .AddAssociatedActions(AID.LightningShot);

        res.DefineOGCD(Track.Zone, AID.DangerZone, "Zone", "Danger / Blasting Zone", 193, 30, 0, ActionTargets.Hostile, 18).AddAssociatedActions(AID.BlastingZone, AID.DangerZone);

        res.Define(Track.NoMercy).As<NoMercyStrategy>("NM", "No Mercy", 197)
            .AddOption(NoMercyStrategy.Automatic, "Automatically use No Mercy", supportedTargets: ActionTargets.Self)
            .AddOption(NoMercyStrategy.BurstReady, "Automatically use No Mercy if full burst is ready (NM, DD, & BF) - will delay if necessary", supportedTargets: ActionTargets.Self)
            .AddOption(NoMercyStrategy.Together, "Automatically use No Mercy alongside Bloodfest, regardless of burst - will delay if necessary", supportedTargets: ActionTargets.Self)
            .AddOption(NoMercyStrategy.Force, "Force No Mercy ASAP", 0, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceW, "Force No Mercy in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceQW, "Force No Mercy in next possible last second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1, "Force No Mercy ASAP if 1 cartridge is available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1W, "Force No Mercy in next possible weave slot if 1 cartridge is available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1QW, "Force No Mercy in next possible last second weave slot if 1 cartridge is available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2, "Force No Mercy ASAP if 2 cartridges are available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2W, "Force No Mercy in next possible weave slot if 2 cartridges are available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2QW, "Force No Mercy in next possible last second weave slot if 2 cartridges are available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3, "Force No Mercy ASAP if 3 cartridges are available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3W, "Force No Mercy in next possible weave slot if 3 cartridges are available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3QW, "Force No Mercy in next possible last second weave slot if 3 cartridges are available", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Delay, "Delay No Mercy")
            .AddAssociatedActions(AID.NoMercy);

        res.Define(Track.SonicBreak).As<SonicBreakStrategy>("SB", "Sonic Break", 195)
            .AddOption(SonicBreakStrategy.Automatic, "Automatically use Sonic Break", supportedTargets: ActionTargets.Hostile, minLevel: 54)
            .AddOption(SonicBreakStrategy.Early, "Automatically use Sonic Break as the very first GCD when in No Mercy", supportedTargets: ActionTargets.Hostile, minLevel: 54)
            .AddOption(SonicBreakStrategy.Late, "Automatically use Sonic Break as the very last GCD when in No Mercy", supportedTargets: ActionTargets.Hostile, minLevel: 54)
            .AddOption(SonicBreakStrategy.Force, "Force Sonic Break ASAP", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Delay, "Delay Sonic Break", minLevel: 54)
            .AddAssociatedActions(AID.SonicBreak);

        res.Define(Track.GnashingFang).As<GnashingStrategy>("GF", "Gnashing Fang Combo", 196)
            .AddOption(GnashingStrategy.Automatic, "Automatically use Gnashing Fang & its combo chain", supportedTargets: ActionTargets.Hostile, minLevel: 60)
            .AddOption(GnashingStrategy.ForceGnash, "Force Gnashing Fang ASAP", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceGnash1, "Force Gnashing Fang ASAP if 1 cartridge is available", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceGnash2, "Force Gnashing Fang ASAP if 2 cartridges are available", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceGnash3, "Force Gnashing Fang ASAP if 3 cartridges are available", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceClaw, "Force Savage Claw ASAP", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceTalon, "Force Wicked Talon ASAP", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.Delay, "Delay Gnashing Fang", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.GnashingFang, AID.SavageClaw, AID.WickedTalon);

        res.DefineOGCD(Track.BowShock, AID.BowShock, "BS", "Bow Shock", 194, 60, 15, ActionTargets.Self, 62).AddAssociatedActions(AID.BowShock);

        res.Define(Track.Continuation).As<ContinuationStrategy>("Cont.", "Continuation", 190)
            .AddOption(ContinuationStrategy.Automatic, "Automatically use Continuation procs", supportedTargets: ActionTargets.Hostile, minLevel: 70)
            .AddOption(ContinuationStrategy.Early, "Use Continuation procs as early as possible", supportedTargets: ActionTargets.Hostile, minLevel: 70)
            .AddOption(ContinuationStrategy.Late, "Use Continuation procs as late as possible", supportedTargets: ActionTargets.Hostile, minLevel: 70)
            .AddAssociatedActions(AID.EyeGouge, AID.AbdomenTear, AID.JugularRip, AID.Hypervelocity, AID.FatedBrand);

        res.Define(Track.Bloodfest).As<BloodfestStrategy>("BF", "Bloodfest", 198)
            .AddOption(BloodfestStrategy.Automatic, "Automatically use Bloodfest", supportedTargets: ActionTargets.Hostile, minLevel: 80)
            .AddOption(BloodfestStrategy.Together, "Automatically use Bloodfest alongside No Mercy - will delay if necessary", supportedTargets: ActionTargets.Hostile, minLevel: 80)
            .AddOption(BloodfestStrategy.Force, "Force Bloodfest ASAP", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.ForceW, "Force Bloodfest in next possible weave slot", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Delay, "Delay Bloodfest", minLevel: 80)
            .AddAssociatedActions(AID.Bloodfest);

        res.Define(Track.DoubleDown).As<DoubleDownStrategy>("DD", "Double Down", 196)
            .AddOption(DoubleDownStrategy.Automatic, "Automatically use Double Down", supportedTargets: ActionTargets.Self, minLevel: 90)
            .AddOption(DoubleDownStrategy.Force, "Force Double Down ASAP", 60, 0, ActionTargets.Self, 90)
            .AddOption(DoubleDownStrategy.Force3, "Force Double Down ASAP if 3 cartridges are available", 60, 0, ActionTargets.Self, 90)
            .AddOption(DoubleDownStrategy.Delay, "Delay Double Down", minLevel: 90)
            .AddAssociatedActions(AID.DoubleDown);

        res.Define(Track.Reign).As<ReignStrategy>("Reign", "Reign of Beasts Combo", 194)
            .AddOption(ReignStrategy.Automatic, "Automatically use Reign of Beasts & its combo chain", supportedTargets: ActionTargets.Hostile, minLevel: 100)
            .AddOption(ReignStrategy.ForceReign, "Force Reign of Beasts ASAP", supportedTargets: ActionTargets.Hostile, minLevel: 100)
            .AddOption(ReignStrategy.ForceNoble, "Force Noble Blood ASAP", supportedTargets: ActionTargets.Hostile, minLevel: 100)
            .AddOption(ReignStrategy.ForceLion, "Force Lion Heart ASAP", supportedTargets: ActionTargets.Hostile, minLevel: 100)
            .AddOption(ReignStrategy.Delay, "Delay Reign of Beasts", minLevel: 100)
            .AddAssociatedActions(AID.ReignOfBeasts, AID.NobleBlood, AID.LionHeart);

        return res;
    }

    private GunbreakerGauge Gauge => World.Client.GetGauge<GunbreakerGauge>();
    private int Ammo => Gauge.Ammo;
    private int GunComboStep => Gauge.AmmoComboStep;
    private (float CD, float Left, bool Active) NM => Cooldown(AID.NoMercy) is var cd ? (cd, Status(SID.NoMercy, 20f), cd is >= 40f and <= 60) : default;
    private (float CD, float Left, bool Active) BF => Status(SID.Bloodfest, 30f) is var left ? (Cooldown(AID.Bloodfest), left, left > 0) : default;
    private (float Left, bool Active) Reign => Status(SID.ReadyToReign, 30f) is var left ? (left, left > 0) : default;
    private float SBLeft => Status(SID.ReadyToBreak, 30f);

    private bool HasBlast => Unlocked(AID.Hypervelocity) && HasStatus(SID.ReadyToBlast) && !LastActionUsed(AID.Hypervelocity);
    private bool HasRaze => Unlocked(AID.FatedBrand) && HasStatus(SID.ReadyToRaze) && !LastActionUsed(AID.FatedBrand);
    private bool HasRip => HasStatus(SID.ReadyToRip) && !LastActionUsed(AID.JugularRip);
    private bool HasTear => HasStatus(SID.ReadyToTear) && !LastActionUsed(AID.AbdomenTear);
    private bool HasGouge => HasStatus(SID.ReadyToGouge) && !LastActionUsed(AID.EyeGouge);

    private bool Slow => SkSGCDLength >= 2.5f;
    private int MaxCartridges => Unlocked(TraitID.CartridgeChargeII) ? (BF.Active ? 6 : 3) : Unlocked(TraitID.CartridgeCharge) ? (BF.Active ? 4 : 2) : 0;
    private bool FullCartridges => Ammo == MaxCartridges;
    private AID BestFatedCircle => Unlocked(AID.FatedCircle) ? AID.FatedCircle : AID.BurstStrike;
    private bool BurstReady => CombatTimer >= 30 || (Unlocked(AID.Bloodfest) ? ComboLastMove is AID.BrutalShell : ComboLastMove is AID.SolidBarrel);

    private bool ForceAOE;
    private bool WantAOE;

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        ForceAOE = strategy.Option(Track.AOE).As<AOEStrategy>() is AOEStrategy.ForceAOEFinishWithOvercap or AOEStrategy.ForceAOEFinishWithoutOvercap or AOEStrategy.ForceAOEBreakWithOvercap or AOEStrategy.ForceAOEBreakWithoutOvercap;
        WantAOE = ForceAOE || TargetsInAOECircle(5f, 3);

        if (strategy.HoldEverything())
            return;

        if (strategy.AllowBuffs())
        {
            QueueBloodfest(strategy);
            QueueNoMercy(strategy);
        }
        if (strategy.AllowCDs() && strategy.AllowGauge())
        {
            QueueGnashingFang(strategy);
            QueueDoubleDown(strategy);
            QueueCartridges(strategy);
        }
        if (strategy.AllowCDs())
        {
            QueueSonicBreak(strategy);
            QueueZone(strategy);
            QueueBowShock(strategy);
        }

        QueueReign(strategy, !strategy.AllowCDs());
        QueueGnashingCombo();
        QueueContinuation(strategy);
        QueueLightningShot(strategy);
        QueuePotion(strategy, ActionDefinitions.IDPotionStr, BF.CD <= 4f, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, GCD - 0.9f);
        QueueCombo(strategy);

        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.DemonSlice, Ammo > 0 && Unlocked(AID.FatedCircle) ? 2 : 3, maximumActionRange: 20);
    }

    private void QueueBloodfest(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Bloodfest);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var (condition, priority) = track.As<BloodfestStrategy>() switch
        {
            BloodfestStrategy.Automatic => (InCombat(target), OGCDPriority.Severe + 1),
            BloodfestStrategy.Together => (InCombat(target) && (NM.Active || NM.CD <= SkSGCDLength), OGCDPriority.Severe + 1),
            BloodfestStrategy.Force => (true, OGCDPrio(OGCDStrategy.Force, OGCDPriority.Severe + 1)),
            BloodfestStrategy.ForceW => (CanWeaveIn, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe + 1)),
            _ => (false, OGCDPriority.None)
        };
        if (condition && ActionReady(AID.Bloodfest))
            QueueOGCD(AID.Bloodfest, target, priority);
    }
    private void QueueNoMercy(StrategyValues strategy)
    {
        var weave = Slow ? CanWeaveIn : CanQuarterWeaveIn;
        var cartridges = weave && Ammo >= (Unlocked(AID.DoubleDown) ? 2 : 1);
        var burst = cartridges && (!Unlocked(AID.DoubleDown) || Ammo > 2 && Cooldown(AID.DoubleDown) <= 4 || !Unlocked(AID.GnashingFang) || Ammo > 1 && Cooldown(AID.GnashingFang) <= 32);
        var together = cartridges && (BF.Active || BF.CD > 45);
        var common = InCombat(PrimaryTarget) && In5y(PrimaryTarget) && BurstReady;

        var (condition, priority) = strategy.Option(Track.NoMercy).As<NoMercyStrategy>() switch
        {
            NoMercyStrategy.Automatic => (common, OGCDPriority.Severe),
            NoMercyStrategy.BurstReady => (common && burst, OGCDPriority.Severe),
            NoMercyStrategy.Together => (common && together, OGCDPriority.Severe),
            NoMercyStrategy.Force => (true, OGCDPrio(OGCDStrategy.Force, OGCDPriority.Severe)),
            NoMercyStrategy.ForceW => (CanWeaveIn, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe)),
            NoMercyStrategy.ForceQW => (CanQuarterWeaveIn, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe)),
            NoMercyStrategy.Force1 => (Ammo == 1, OGCDPrio(OGCDStrategy.Force, OGCDPriority.Severe)),
            NoMercyStrategy.Force1W => (CanWeaveIn && Ammo == 1, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe)),
            NoMercyStrategy.Force1QW => (CanQuarterWeaveIn && Ammo == 1, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe)),
            NoMercyStrategy.Force2 => (Ammo == 2, OGCDPrio(OGCDStrategy.Force, OGCDPriority.Severe)),
            NoMercyStrategy.Force2W => (CanWeaveIn && Ammo == 2, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe)),
            NoMercyStrategy.Force2QW => (CanQuarterWeaveIn && Ammo == 2, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe)),
            NoMercyStrategy.Force3 => (Ammo == 3, OGCDPrio(OGCDStrategy.Force, OGCDPriority.Severe)),
            NoMercyStrategy.Force3W => (CanWeaveIn && Ammo == 3, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe)),
            NoMercyStrategy.Force3QW => (CanQuarterWeaveIn && Ammo == 3, OGCDPrio(OGCDStrategy.AnyWeave, OGCDPriority.Severe)),
            _ => (false, OGCDPriority.None)
        };
        if (condition && ActionReady(AID.NoMercy))
            QueueOGCD(AID.NoMercy, Player, priority + 2);
    }
    private void QueueZone(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Zone);
        var zone = track.As<OGCDStrategy>();
        var action = Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone;
        if (Unlocked(AID.DangerZone) && ShouldUseOGCD(zone, PrimaryTarget, ActionReady(action), InCombat(PrimaryTarget) && In3y(PrimaryTarget) && CanWeaveIn && NM.CD is <= 58f and > 15f))
            QueueOGCD(action, SingleTargetChoice(PrimaryTarget, track), OGCDPrio(zone, Slow ? OGCDPriority.ModeratelyHigh : OGCDPriority.High));
    }
    private void QueueBowShock(StrategyValues strategy)
    {
        var bow = strategy.Option(Track.BowShock).As<OGCDStrategy>();
        if (ShouldUseOGCD(bow, PrimaryTarget, ActionReady(AID.BowShock), InCombat(PrimaryTarget) && In5y(PrimaryTarget) && CanWeaveIn && NM.CD is <= 58f and > 39.5f))
            QueueOGCD(AID.BowShock, Player, OGCDPrio(bow, Slow ? OGCDPriority.High : OGCDPriority.ModeratelyHigh));
    }
    private void QueueContinuation(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Continuation);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var blastOrRaze = (HasBlast || HasRaze) && (NM.CD > 1 || GCD < 0.6f);
        var procReady = InCombat(target) && (blastOrRaze || HasRip || HasTear || HasGouge);
        var timing = track.As<ContinuationStrategy>() switch
        {
            ContinuationStrategy.Late => GCD <= 1.25f,
            _ => true
        };
        var action = HasGouge ? AID.EyeGouge
            : HasTear ? AID.AbdomenTear
            : HasRip ? AID.JugularRip
            : HasRaze ? AID.FatedBrand
            : HasBlast ? AID.Hypervelocity
            : AID.Continuation;
        if (procReady && timing && Unlocked(AID.Continuation))
            QueueOGCD(action, target, ContinuationPriority);
    }
    private OGCDPriority ContinuationPriority
    {
        get
        {
            var priority = ChangePriority(200, 749);
            return HasRip || HasTear || HasGouge ? priority
                : HasBlast || HasRaze ? (Slow ? priority : priority + 500)
                : OGCDPriority.None;
        }
    }
    private void QueueGnashingFang(StrategyValues strategy)
    {
        var track = strategy.Option(Track.GnashingFang);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var cooldown = Cooldown(AID.GnashingFang);
        var ready = In3y(target) && Ammo >= 1 && cooldown < 30.6f && GunComboStep == 0;
        var canFit = ActualComboTimer >= SkSGCDLength * 4 || ActualComboTimer == 0;
        var singleTarget = ready && !TargetsInAOECircle(5f, 3);
        var burst = singleTarget && NM.Active && !Reign.Active;
        var filler = singleTarget && NM.CD > 7 && canFit;
        var overcap = singleTarget && cooldown < 0.6f && NM.CD > 7;
        var autoPriority = overcap ? GCDPriority.SlightlyHigh + 9 : burst ? GCDPriority.SlightlyHigh + 7 : GCDPriority.SlightlyHigh;

        var (condition, action, priority) = track.As<GnashingStrategy>() switch
        {
            GnashingStrategy.Automatic => (burst || filler || overcap, AID.GnashingFang, autoPriority),
            GnashingStrategy.ForceGnash => (ready, AID.GnashingFang, GCDPriority.Forced),
            GnashingStrategy.ForceGnash1 => (ready && Ammo == 1, AID.GnashingFang, GCDPriority.Forced),
            GnashingStrategy.ForceGnash2 => (ready && Ammo == 2, AID.GnashingFang, GCDPriority.Forced),
            GnashingStrategy.ForceGnash3 => (ready && Ammo == 3, AID.GnashingFang, GCDPriority.Forced),
            GnashingStrategy.ForceClaw => (GunComboStep == 1 && In3y(target), AID.SavageClaw, GCDPriority.Forced),
            GnashingStrategy.ForceTalon => (GunComboStep == 2 && In3y(target), AID.WickedTalon, GCDPriority.Forced),
            _ => (false, AID.None, GCDPriority.None)
        };
        if (condition && Unlocked(AID.GnashingFang))
            QueueGCD(action, target, priority);
    }
    private void QueueDoubleDown(StrategyValues strategy)
    {
        var (condition, priority) = strategy.Option(Track.DoubleDown).As<DoubleDownStrategy>() switch
        {
            DoubleDownStrategy.Automatic => (InCombat(PrimaryTarget) && In5y(PrimaryTarget) && NM.Active, GCDPriority.SlightlyHigh + 6),
            DoubleDownStrategy.Force => (true, GCDPriority.Forced),
            DoubleDownStrategy.Force3 => (Ammo == 3, GCDPriority.Forced),
            _ => (false, GCDPriority.None)
        };
        if (condition && Ammo >= 2 && ActionReady(AID.DoubleDown))
            QueueGCD(AID.DoubleDown, Player, priority);
    }
    private void QueueSonicBreak(StrategyValues strategy)
    {
        var track = strategy.Option(Track.SonicBreak);
        var sb = track.As<SonicBreakStrategy>();
        var ready = sb != SonicBreakStrategy.Delay && Unlocked(AID.SonicBreak) && HasStatus(SID.ReadyToBreak);
        var target = ready ? AOETargetChoice(PrimaryTarget, HighestHPTarget(3.5f)?.Actor, track, strategy) : null;
        var (condition, priority) = sb switch
        {
            SonicBreakStrategy.Automatic => (InCombat(target) && In3y(target) && (!NM.Active || !TargetsInAOECircle(5f, 3) || SBLeft <= 10.5f), GCDPriority.SlightlyHigh + 5),
            SonicBreakStrategy.Early => (NM.Active, GCDPriority.SlightlyHigh + 10),
            SonicBreakStrategy.Late => (SBLeft is > 0 and <= 12.5f, GCDPriority.SlightlyHigh + 10),
            _ => (true, GCDPriority.Forced)
        };
        if (ready && condition)
            QueueGCD(AID.SonicBreak, target, priority);
    }
    private Enemy? HighestHPTarget(float range)
    {
        Enemy? best = null;
        var bestRatio = float.MinValue;
        var enemies = Hints.PotentialTargets;
        for (var i = 0; i < enemies.Count && enemies[i].Priority == Hints.HighestPotentialTargetPriority; ++i)
        {
            var actor = enemies[i].Actor;
            var ratio = (float)actor.HPMP.CurHP / actor.HPMP.MaxHP;
            if (DistanceFrom(actor, range) && ratio > bestRatio)
                (best, bestRatio) = (enemies[i], ratio);
        }
        return best;
    }
    private void QueueReign(StrategyValues strategy, bool holdCooldowns)
    {
        var track = strategy.Option(Track.Reign);
        var reign = track.As<ReignStrategy>();
        if (reign == ReignStrategy.Delay || !Unlocked(AID.ReignOfBeasts) || !Reign.Active && GunComboStep is not (3 or 4))
            return;

        var splash = GetBestTarget(PrimaryEnemy, 3, IsSplashTarget);
        var target = AOETargetChoice(PrimaryTarget, (splash.Targets > 1 ? splash.Best : PrimaryEnemy)?.Actor, track, strategy);
        var reignNow = Reign.Active && (Reign.Left is > 0 and <= 2.5f || NM.Active && GunComboStep == 0);
        var autoAction = GunComboStep switch
        {
            4 => AID.LionHeart,
            3 => AID.NobleBlood,
            _ => AID.ReignOfBeasts
        };
        var autoPriority = NM.Left is > 0 and <= 8 || Reign.Active ? GCDPriority.SlightlyHigh + 8 : GCDPriority.SlightlyHigh + 4;

        var (condition, action, priority) = reign switch
        {
            ReignStrategy.Automatic => (InCombat(target) && In3y(target) && !holdCooldowns && (reignNow || GunComboStep is 3 or 4), autoAction, autoPriority),
            ReignStrategy.ForceReign => (Reign.Active, AID.ReignOfBeasts, GCDPriority.Forced),
            ReignStrategy.ForceNoble => (GunComboStep == 3, AID.NobleBlood, GCDPriority.Forced),
            _ => (GunComboStep == 4, AID.LionHeart, GCDPriority.Forced)
        };
        if (condition)
            QueueGCD(action, target, priority);
    }
    private void QueueCartridges(StrategyValues strategy)
    {
        var track = strategy.Option(Track.Cartridges);
        var carts = track.As<CartridgeStrategy>();
        var fatedCircle = Unlocked(AID.FatedCircle);
        var nearby = TargetsInAOECircle(5f, 2);
        var stTarget = SingleTargetChoice(PrimaryTarget, track);
        var inRange = fatedCircle ? (nearby ? In5y(PrimaryTarget) : In3y(PrimaryTarget)) : In3y(PrimaryTarget) && !TargetsInAOECircle(5f, 3);
        var spend = NM.Active
            || BurstReady && Unlocked(AID.ReignOfBeasts) && NM.CD < 1
            || Ammo > 3 && BF.Left is > 0 and <= 20 && !NM.Active;
        var automatic = InCombat(PrimaryTarget) && inRange && GunComboStep == 0 && spend;
        var autoAction = carts switch
        {
            CartridgeStrategy.OnlyFC => BestFatedCircle,
            CartridgeStrategy.OnlyBS => AID.BurstStrike,
            _ => ForceAOE || nearby ? BestFatedCircle : AID.BurstStrike
        };
        var autoTarget = autoAction == AID.FatedCircle ? Player : stTarget;

        var (condition, action, target, priority) = carts switch
        {
            CartridgeStrategy.Automatic or CartridgeStrategy.OnlyBS or CartridgeStrategy.OnlyFC => (automatic, autoAction, autoTarget, GCDPriority.SlightlyHigh + 2),
            CartridgeStrategy.ForceBS => (true, AID.BurstStrike, stTarget, GCDPriority.Forced),
            CartridgeStrategy.ForceBS1 => (Ammo == 1, AID.BurstStrike, stTarget, GCDPriority.Forced),
            CartridgeStrategy.ForceBS2 => (Ammo == 2, AID.BurstStrike, stTarget, GCDPriority.Forced),
            CartridgeStrategy.ForceBS3 => (Ammo == 3, AID.BurstStrike, stTarget, GCDPriority.Forced),
            CartridgeStrategy.ForceFC => (fatedCircle, AID.FatedCircle, Player, GCDPriority.Forced),
            CartridgeStrategy.ForceFC1 => (fatedCircle && Ammo == 1, AID.FatedCircle, Player, GCDPriority.Forced),
            CartridgeStrategy.ForceFC2 => (fatedCircle && Ammo == 2, AID.FatedCircle, Player, GCDPriority.Forced),
            CartridgeStrategy.ForceFC3 => (fatedCircle && Ammo == 3, AID.FatedCircle, Player, GCDPriority.Forced),
            _ => (false, AID.None, null, GCDPriority.None)
        };
        if (condition && Ammo >= 1 && Unlocked(AID.BurstStrike))
            QueueGCD(action, target, priority);
    }

    private void QueueGnashingCombo()
    {
        var action = GunComboStep switch
        {
            1 => AID.SavageClaw,
            2 => AID.WickedTalon,
            _ => AID.None
        };
        if (action != AID.None)
            QueueGCD(action, PrimaryTarget, GCDPriority.SlightlyHigh + 3);
    }
    private void QueueLightningShot(StrategyValues strategy)
    {
        var track = strategy.Option(Track.LightningShot);
        var target = SingleTargetChoice(PrimaryTarget, track);
        var prepull = NoPrepullWithin(0.7f);
        var (condition, priority) = track.As<LightningShotStrategy>() switch
        {
            LightningShotStrategy.OpenerFar => (prepull && !In3y(target), GCDPriority.Forced),
            LightningShotStrategy.OpenerForce => (prepull, GCDPriority.Forced),
            LightningShotStrategy.Force => (true, GCDPriority.Forced),
            LightningShotStrategy.Allow => ((Player.InCombat || prepull) && (WantAOE ? !In5y(target) : !In3y(target)), GCDPriority.Low + 2),
            _ => (false, GCDPriority.None)
        };
        if (condition && Unlocked(AID.LightningShot))
            QueueGCD(AID.LightningShot, target, priority);
    }
    private void QueueCombo(StrategyValues strategy)
    {
        var track = strategy.Option(Track.AOE);
        var justUsedST = ComboLastMove is AID.KeenEdge or AID.BrutalShell or AID.GnashingFang or AID.SavageClaw or AID.ReignOfBeasts or AID.NobleBlood;
        var justUsedAOE = ComboLastMove is AID.DemonSlice;
        var stTarget = SingleTargetChoice(PrimaryTarget, track);
        var autoTarget = !WantAOE || justUsedST ? stTarget : Player;

        var (action, target) = track.As<AOEStrategy>() switch
        {
            AOEStrategy.AutoFinishWithOvercap => (Finish(WantAOE ? NextAOE(true) : NextST(true)), autoTarget),
            AOEStrategy.AutoFinishWithoutOvercap => (Finish(WantAOE ? NextAOE(false) : NextST(false)), autoTarget),
            AOEStrategy.AutoBreakWithOvercap => (WantAOE ? NextAOE(true) : NextST(true), autoTarget),
            AOEStrategy.AutoBreakWithoutOvercap => (WantAOE ? NextAOE(false) : NextST(false), autoTarget),
            AOEStrategy.ForceSTFinishWithOvercap => (Finish(NextST(true)), justUsedAOE ? Player : stTarget),
            AOEStrategy.ForceSTFinishWithoutOvercap => (Finish(NextST(false)), justUsedAOE ? Player : stTarget),
            AOEStrategy.ForceAOEFinishWithOvercap => (Finish(NextAOE(true)), justUsedST ? stTarget : Player),
            AOEStrategy.ForceAOEFinishWithoutOvercap => (Finish(NextAOE(false)), justUsedST ? stTarget : Player),
            AOEStrategy.ForceSTBreakWithOvercap => (NextST(true), stTarget),
            AOEStrategy.ForceSTBreakWithoutOvercap => (NextST(false), stTarget),
            AOEStrategy.ForceAOEBreakWithOvercap => (NextAOE(true), Player),
            AOEStrategy.ForceAOEBreakWithoutOvercap => (NextAOE(false), Player),
            _ => (AID.None, null)
        };
        if (target != null && (WantAOE ? In5y(target) : In3y(target)) && CountdownRemaining is not > 0.7f)
            QueueGCD(action, target, GCDPriority.Low);
    }
    private AID NextST(bool overcap) => ComboLastMove switch
    {
        AID.BrutalShell => overcap && FullCartridges && Unlocked(AID.BurstStrike) ? AID.BurstStrike : Unlocked(AID.SolidBarrel) ? AID.SolidBarrel : AID.KeenEdge,
        AID.KeenEdge => Unlocked(AID.BrutalShell) ? AID.BrutalShell : AID.KeenEdge,
        _ => AID.KeenEdge
    };
    private AID NextAOE(bool overcap)
        => overcap && FullCartridges && ComboLastMove == AID.DemonSlice ? BestFatedCircle
        : ComboLastMove == AID.DemonSlice && Unlocked(AID.DemonSlaughter) ? AID.DemonSlaughter
        : AID.DemonSlice;
    private AID Finish(AID next) => ComboLastMove switch
    {
        AID.GnashingFang => AID.SavageClaw,
        AID.SavageClaw => AID.WickedTalon,
        AID.ReignOfBeasts => AID.NobleBlood,
        AID.NobleBlood => AID.LionHeart,
        _ => next
    };
}
