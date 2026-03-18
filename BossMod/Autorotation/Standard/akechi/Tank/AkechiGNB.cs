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

    private float NMstatus;
    private float SBstatus;
    private float Rstatus;
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestSplashTarget;
    private Enemy? BestDOTTarget;
    private bool ForceAOE;
    private bool WantAOE;

    private GunbreakerGauge Gauge => World.Client.GetGauge<GunbreakerGauge>();
    private byte Ammo => Gauge.Ammo; //cartridges
    private byte GunComboStep => Gauge.AmmoComboStep; //Gauge combo - GF & Reign
    private float NMcd => Cooldown(AID.NoMercy);
    private float BFcd => Cooldown(AID.Bloodfest);
    private bool HasNM => NMcd is >= 40f and <= 60;
    private bool HasBF => HasEffect(SID.Bloodfest);
    private bool HasReign => HasEffect(SID.ReadyToReign);
    private bool HasBlast => Unlocked(AID.Hypervelocity) && HasEffect(SID.ReadyToBlast) && !LastActionUsed(AID.Hypervelocity);
    private bool HasRaze => Unlocked(AID.FatedBrand) && HasEffect(SID.ReadyToRaze) && !LastActionUsed(AID.FatedBrand);
    private bool HasRip => HasEffect(SID.ReadyToRip) && !LastActionUsed(AID.JugularRip);
    private bool HasTear => HasEffect(SID.ReadyToTear) && !LastActionUsed(AID.AbdomenTear);
    private bool HasGouge => HasEffect(SID.ReadyToGouge) && !LastActionUsed(AID.EyeGouge);
    private bool Slow => SkSGCDLength >= 2.5f;
    private bool Fast => SkSGCDLength < 2.5f;
    private int MaxCartridges
            => Unlocked(TraitID.CartridgeChargeII) ? HasEffect(SID.Bloodfest) ? 6 : 3 //3 max base, 6 max buffed
            : Unlocked(TraitID.CartridgeCharge) ? HasEffect(SID.Bloodfest) ? 4 : 2 //2 max base, 4 max buffed
            : 0; //none without first trait

    private AID ContinueST(AID last, bool overcap) => last switch
    {
        AID.BrutalShell =>
            overcap && Ammo == MaxCartridges && Unlocked(AID.BurstStrike) ? AID.BurstStrike
            : Unlocked(AID.SolidBarrel) ? AID.SolidBarrel
            : AID.KeenEdge,

        AID.KeenEdge => Unlocked(AID.BrutalShell) ? AID.BrutalShell : AID.KeenEdge,
        _ => AID.KeenEdge,
    };
    private AID ContinueAOE(AID last, bool overcap)
        => overcap && Ammo == MaxCartridges ? (Unlocked(AID.FatedCircle) ? AID.FatedCircle : AID.BurstStrike)
            : last == AID.DemonSlice ? (Unlocked(AID.DemonSlaughter) ? AID.DemonSlaughter : AID.DemonSlice)
            : AID.DemonSlice;

    private AID ContinueSTNoOvercap(AID last) => ContinueST(last, overcap: false);
    private AID ContinueSTWithOvercap(AID last) => ContinueST(last, overcap: true);
    private AID ContinueAOENoOvercap(AID last) => ContinueAOE(last, overcap: false);
    private AID ContinueAOEWithOvercap(AID last) => ContinueAOE(last, overcap: true);

    private AID Finish(bool overcap,
        Func<AID, AID> continueWithOvercap,
        Func<AID, AID> continueNoOvercap)
    {
        var gnash = ComboLastMove switch
        {
            AID.GnashingFang => AID.SavageClaw,
            AID.SavageClaw => AID.WickedTalon,
            AID.ReignOfBeasts => AID.NobleBlood,
            AID.NobleBlood => AID.LionHeart,
            _ => AID.None,
        };
        return gnash != AID.None ? gnash //finish Gauge combos first
            : overcap ? continueWithOvercap(ComboLastMove) //finish combo with overcap protection
            : continueNoOvercap(ComboLastMove); //just finish combo
    }
    private AID STFinish(bool overcap) => Finish(overcap, ContinueSTWithOvercap, ContinueSTNoOvercap);
    private AID AOEFinish(bool overcap) => Finish(overcap, ContinueAOEWithOvercap, ContinueAOENoOvercap);

    //we dont care about finishing combos with these methods, so we just send it
    private AID STBreak(bool overcap) => overcap ? ContinueSTWithOvercap(ComboLastMove) : ContinueSTNoOvercap(ComboLastMove);
    private AID AOEBreak(bool overcap) => overcap ? ContinueAOEWithOvercap(ComboLastMove) : ContinueAOENoOvercap(ComboLastMove);

    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Potion() switch
    {
        PotionStrategy.AlignWithBuffs => Player.InCombat && BFcd <= 4f,
        PotionStrategy.AlignWithRaidBuffs => Player.InCombat && RaidBuffsLeft > 0,
        PotionStrategy.Immediate => true,
        _ => false
    };

    private OGCDPriority ContinuationPrio()
    {
        var prio = ChangePriority(200, 749);
        //standard procs
        if (HasRip || HasTear || HasGouge)
            return prio;

        //these require some tinkering because of the synergy with No Mercy & SkS
        if (HasBlast || HasRaze)
        {
            //if slow, send after NM - else just send it
            return Slow ? prio : prio + 500;
        }
        return OGCDPriority.None;
    }
    private OGCDPriority SkSPrio(OGCDPriority slow, OGCDPriority notslow) => Slow ? slow : Fast ? notslow : OGCDPriority.None;
    private OGCDPriority BowPrio => SkSPrio(OGCDPriority.High, OGCDPriority.ModeratelyHigh);
    private OGCDPriority ZonePrio => SkSPrio(OGCDPriority.ModeratelyHigh, OGCDPriority.High);

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        NMstatus = StatusRemaining(Player, SID.NoMercy, 20f);
        SBstatus = StatusRemaining(Player, SID.ReadyToBreak, 30f);
        Rstatus = StatusRemaining(Player, SID.ReadyToReign, 30f);
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 3.5f, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.ReignOfBeasts) && NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;
        BestDOTTarget = Hints.PriorityTargets.Where(x => Player.DistanceToHitbox(x.Actor) <= 3.5f).OrderByDescending(x => (float)x.Actor.HPMP.CurHP / x.Actor.HPMP.MaxHP).FirstOrDefault();
        var aoe = strategy.Option(Track.AOE);
        var aoeStrat = aoe.As<AOEStrategy>();
        ForceAOE = aoeStrat is AOEStrategy.ForceAOEFinishWithoutOvercap or AOEStrategy.ForceAOEBreakWithoutOvercap or AOEStrategy.ForceAOEFinishWithOvercap or AOEStrategy.ForceAOEBreakWithOvercap;
        WantAOE = TargetsInAOECircle(5f, 2) || ForceAOE;
        var open = (CombatTimer < 30 && ComboLastMove is AID.BrutalShell) || CombatTimer >= 30;
        var mainTarget = primaryTarget?.Actor;

        if (strategy.HoldEverything())
            return;

        if (!strategy.HoldCDs())
        {
            if (!strategy.HoldBuffs())
            {
                //Bloodfest
                if (ActionReady(AID.Bloodfest))
                {
                    var bf = strategy.Option(Track.Bloodfest);
                    var bfStrat = bf.As<BloodfestStrategy>();
                    var bfTarget = SingleTargetChoice(mainTarget, bf);
                    var (bfCondition, bfPrio) = bfStrat switch
                    {
                        BloodfestStrategy.Automatic => (InCombat(bfTarget), OGCDPriority.Severe + 1),
                        BloodfestStrategy.Together => (InCombat(bfTarget) && (HasNM || NMcd <= SkSGCDLength), OGCDPriority.Severe + 1),
                        BloodfestStrategy.Force => (true, OGCDPriority.Severe + 1 + 2000),
                        BloodfestStrategy.ForceW => (CanWeaveIn, OGCDPriority.Severe + 1 + 1000),
                        _ => (false, OGCDPriority.None),
                    };
                    if (bfCondition)
                        QueueOGCD(AID.Bloodfest, bfTarget, bfPrio);
                }

                //No Mercy
                if (ActionReady(AID.NoMercy))
                {
                    var nm = strategy.Option(Track.NoMercy);
                    var nmStrat = nm.As<NoMercyStrategy>();
                    var slow = Slow && CanWeaveIn;
                    var fast = Fast && CanQuarterWeaveIn;
                    var speed = slow || fast;
                    var lv1to89 = speed && Ammo >= 1;
                    var lv90plus = speed && Ammo >= 2;
                    var cartCheck = Unlocked(AID.DoubleDown) ? lv90plus : lv1to89;
                    var burst = cartCheck &&
                            ((!Unlocked(AID.DoubleDown) || Ammo > 2 && Cooldown(AID.DoubleDown) <= 4) ||
                            (!Unlocked(AID.GnashingFang) || Ammo > 1 && Cooldown(AID.GnashingFang) <= 32));
                    var together = cartCheck && (HasBF || BFcd > 45);
                    var common = InCombat(mainTarget) && In5y(mainTarget) && open;

                    var (nmCondition, nmPrio) = nmStrat switch
                    {
                        NoMercyStrategy.Automatic => (common, OGCDPriority.Severe),
                        NoMercyStrategy.BurstReady => (common && burst, OGCDPriority.Severe),
                        NoMercyStrategy.Together => (common && together, OGCDPriority.Severe),
                        NoMercyStrategy.Force => (true, OGCDPriority.Severe + 2000),
                        NoMercyStrategy.ForceW => (CanWeaveIn, OGCDPriority.Severe + 1000),
                        NoMercyStrategy.ForceQW => (CanQuarterWeaveIn, OGCDPriority.Severe + 1000),
                        NoMercyStrategy.Force1 => (Ammo == 1, OGCDPriority.Severe + 2000),
                        NoMercyStrategy.Force1W => (CanWeaveIn && Ammo == 1, OGCDPriority.Severe + 1000),
                        NoMercyStrategy.Force1QW => (CanQuarterWeaveIn && Ammo == 1, OGCDPriority.Severe + 1000),
                        NoMercyStrategy.Force2 => (Ammo == 2, OGCDPriority.Severe + 2000),
                        NoMercyStrategy.Force2W => (CanWeaveIn && Ammo == 2, OGCDPriority.Severe + 1000),
                        NoMercyStrategy.Force2QW => (CanQuarterWeaveIn && Ammo == 2, OGCDPriority.Severe + 1000),
                        NoMercyStrategy.Force3 => (Ammo == 3, OGCDPriority.Severe + 2000),
                        NoMercyStrategy.Force3W => (CanWeaveIn && Ammo == 3, OGCDPriority.Severe + 1000),
                        NoMercyStrategy.Force3QW => (CanQuarterWeaveIn && Ammo == 3, OGCDPriority.Severe + 1000),
                        _ => (false, OGCDPriority.None),
                    };
                    if (nmCondition)
                        QueueOGCD(AID.NoMercy, Player, nmPrio + 2);
                }
            }

            if (!strategy.HoldGauge())
            {
                //Gnashing Fang + combo
                if (Unlocked(AID.GnashingFang))
                {
                    var gf = strategy.Option(Track.GnashingFang);
                    var gfStrat = gf.As<GnashingStrategy>();
                    var gfTarget = SingleTargetChoice(mainTarget, gf);
                    var gfMinimum = gfTarget != null && In3y(gfTarget) && Ammo >= 1 && Cooldown(AID.GnashingFang) < 30.6f && GunComboStep == 0;
                    var canfit = ActualComboTimer >= SkSGCDLength * 4 || ActualComboTimer == 0; //dont use if combo timer will expire (or if combo timer is 0, just send)
                    var st = gfMinimum && !TargetsInAOECircle(5f, 4);
                    var burst = st && HasNM && !HasReign; //if Lv100 & not opener, we send after Reign combo
                    var filler = st && NMcd > 7 && canfit;
                    var overcap = st && Cooldown(AID.GnashingFang) < 0.6f && NMcd > 7;
                    var (gfCondition, gfAction, gfPrio) = gfStrat switch
                    {
                        GnashingStrategy.Automatic => (burst || filler || overcap, AID.GnashingFang, overcap ? GCDPriority.SlightlyHigh + 9 : burst ? GCDPriority.SlightlyHigh + 7 : GCDPriority.SlightlyHigh),
                        GnashingStrategy.ForceGnash => (gfMinimum, AID.GnashingFang, GCDPriority.Forced),
                        GnashingStrategy.ForceGnash1 => (gfMinimum && Ammo == 1, AID.GnashingFang, GCDPriority.Forced),
                        GnashingStrategy.ForceGnash2 => (gfMinimum && Ammo == 2, AID.GnashingFang, GCDPriority.Forced),
                        GnashingStrategy.ForceGnash3 => (gfMinimum && Ammo == 3, AID.GnashingFang, GCDPriority.Forced),
                        GnashingStrategy.ForceClaw => (GunComboStep == 1 && In3y(gfTarget), AID.SavageClaw, GCDPriority.Forced),
                        GnashingStrategy.ForceTalon => (GunComboStep == 2 && In3y(gfTarget), AID.WickedTalon, GCDPriority.Forced),
                        _ => (false, AID.None, GCDPriority.None),
                    };
                    if (gfCondition)
                        QueueGCD(gfAction, gfTarget, gfPrio);
                }

                //Double Down
                if (ActionReady(AID.DoubleDown) && Ammo >= 2)
                {
                    var dd = strategy.Option(Track.DoubleDown);
                    var ddStrat = dd.As<DoubleDownStrategy>();
                    var (ddCondition, ddPrio) = ddStrat switch
                    {
                        DoubleDownStrategy.Automatic => (InCombat(mainTarget) && In5y(mainTarget) && HasNM, GCDPriority.SlightlyHigh + 6),
                        DoubleDownStrategy.Force => (true, GCDPriority.Forced),
                        DoubleDownStrategy.Force3 => (Ammo == 3, GCDPriority.Forced),
                        _ => (false, GCDPriority.None),
                    };
                    if (ddCondition)
                        QueueGCD(AID.DoubleDown, Player, ddPrio);
                }

                //Burst Strike & Fated Fircle
                if (Unlocked(AID.BurstStrike) && Ammo >= 1)
                {
                    var carts = strategy.Option(Track.Cartridges);
                    var cartsStrat = carts.As<CartridgeStrategy>();
                    var onlybs = cartsStrat == CartridgeStrategy.OnlyBS;
                    var onlyfc = cartsStrat == CartridgeStrategy.OnlyFC;
                    var fc = Unlocked(AID.FatedCircle) ? AID.FatedCircle : AID.BurstStrike;
                    var targetsNearby = TargetsInAOECircle(5f, 2);
                    var bsTarget = SingleTargetChoice(mainTarget, carts);
                    var useAOE = ForceAOE || targetsNearby;
                    var lv30to71 = !Unlocked(AID.FatedCircle) && (!TargetsInAOECircle(5f, 3) && In3y(mainTarget)); //Before Lv72 - if more than 2 targets are present, we skip Burst Strike entirely
                    var lv72plus = Unlocked(AID.FatedCircle) && (targetsNearby ? In5y(mainTarget) : In3y(mainTarget)); //After Lv72 - if more than 1 target is present, we choose Fated Circle over Burst Strike
                    var (bsfcCondition, bsfcAction, bsfcTarget, bsfcPrio) = cartsStrat switch
                    {
                        CartridgeStrategy.Automatic or CartridgeStrategy.OnlyBS or CartridgeStrategy.OnlyFC
                            => ((InCombat(mainTarget) && (Unlocked(AID.FatedCircle) ? lv72plus : lv30to71) && GunComboStep == 0 &&
                            (HasNM || //if we have No Mercy, spend as much as possible after all combos (if we can)
                            open && NMcd < 1 || //if No Mercy is imminent, use Burst Strike to buff Hypervelocity (BS/FC>NM>HV/FB)
                            (Ammo > 3 && HasEffect(SID.Bloodfest) && !HasNM))), //if we have extra Bloodfest carts after NM, spend them asap
                            onlyfc ? fc : onlybs ? AID.BurstStrike : (useAOE ? Unlocked(AID.FatedCircle) ? AID.FatedCircle : AID.BurstStrike : AID.BurstStrike),
                            useAOE ? Player : bsTarget,
                            GCDPriority.SlightlyHigh + 2),
                        CartridgeStrategy.ForceBS => (true, AID.BurstStrike, bsTarget, GCDPriority.Forced),
                        CartridgeStrategy.ForceBS1 => (true && Ammo == 1, AID.BurstStrike, bsTarget, GCDPriority.Forced),
                        CartridgeStrategy.ForceBS2 => (true && Ammo == 2, AID.BurstStrike, bsTarget, GCDPriority.Forced),
                        CartridgeStrategy.ForceBS3 => (true && Ammo == 3, AID.BurstStrike, bsTarget, GCDPriority.Forced),
                        CartridgeStrategy.ForceFC => (Unlocked(AID.FatedCircle), fc, Player, GCDPriority.Forced),
                        CartridgeStrategy.ForceFC1 => (Unlocked(AID.FatedCircle) && Ammo == 1, fc, Player, GCDPriority.Forced),
                        CartridgeStrategy.ForceFC2 => (Unlocked(AID.FatedCircle) && Ammo == 2, fc, Player, GCDPriority.Forced),
                        CartridgeStrategy.ForceFC3 => (Unlocked(AID.FatedCircle) && Ammo == 3, fc, Player, GCDPriority.Forced),
                        _ => (false, AID.None, null, GCDPriority.None)
                    };
                    if (bsfcCondition)
                        QueueGCD(bsfcAction, bsfcTarget, bsfcPrio);
                }
            }

            //Sonic Break
            if (Unlocked(AID.SonicBreak) && HasEffect(SID.ReadyToBreak))
            {
                var sb = strategy.Option(Track.SonicBreak);
                var sbStrat = sb.As<SonicBreakStrategy>();
                var sbTarget = AOETargetChoice(mainTarget, BestDOTTarget?.Actor, sb, strategy);
                var (sbCondition, sbPrio) = sbStrat switch
                {
                    SonicBreakStrategy.Automatic => (InCombat(sbTarget) && In3y(sbTarget), GCDPriority.SlightlyHigh + 5),
                    SonicBreakStrategy.Force => (true, GCDPriority.Forced),
                    SonicBreakStrategy.Early => (HasNM, GCDPriority.SlightlyHigh + 10),
                    SonicBreakStrategy.Late => (SBstatus is <= 12.500f and not 0, GCDPriority.SlightlyHigh + 10),
                    _ => (false, GCDPriority.None)
                };
                if (sbCondition)
                    QueueGCD(AID.SonicBreak, sbTarget, sbPrio);
            }

            //Zone
            if (Unlocked(AID.DangerZone))
            {
                var zone = strategy.Option(Track.Zone);
                var zoneStrat = zone.As<OGCDStrategy>();
                var zoneAction = Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone;
                if (ShouldUseOGCD(zoneStrat, mainTarget, ActionReady(zoneAction),
                    InCombat(mainTarget) && In3y(mainTarget) && CanWeaveIn && NMcd is <= 58f and > 15f))
                    QueueOGCD(zoneAction, SingleTargetChoice(mainTarget, zone), OGCDPrio(zoneStrat, ZonePrio));
            }

            //Bow Shock
            if (ActionReady(AID.BowShock))
            {
                var bowStrat = strategy.Option(Track.BowShock).As<OGCDStrategy>();
                if (ShouldUseOGCD(bowStrat, mainTarget, ActionReady(AID.BowShock),
                    InCombat(mainTarget) && In5y(mainTarget) && CanWeaveIn && NMcd is <= 58f and > 39.5f))
                    QueueOGCD(AID.BowShock, Player, OGCDPrio(bowStrat, BowPrio));
            }
        }

        //Reign of Beasts + combo
        if (Unlocked(AID.ReignOfBeasts))
        {
            var r = strategy.Option(Track.Reign);
            var rStrat = r.As<ReignStrategy>();
            var rTarget = AOETargetChoice(mainTarget, BestSplashTarget?.Actor, r, strategy);
            var hold = strategy.HoldCDs();
            var (rCondition, rAction, rPrio) = rStrat switch
            {
                ReignStrategy.Automatic => (
                    InCombat(rTarget) && In3y(rTarget) && !hold && ((HasReign && (Rstatus is <= 2.5f and not 0 || (HasNM && GunComboStep == 0))) || GunComboStep is 3 or 4),
                    GunComboStep == 4 ? AID.LionHeart : GunComboStep == 3 ? AID.NobleBlood : AID.ReignOfBeasts,
                    NMstatus is <= 8 and not 0 || HasReign ? GCDPriority.SlightlyHigh + 8 : GCDPriority.SlightlyHigh + 4
                    ),
                ReignStrategy.ForceReign => (HasReign, AID.ReignOfBeasts, GCDPriority.Forced),
                ReignStrategy.ForceNoble => (GunComboStep == 3, AID.NobleBlood, GCDPriority.Forced),
                ReignStrategy.ForceLion => (GunComboStep == 4, AID.LionHeart, GCDPriority.Forced),
                _ => (false, AID.None, GCDPriority.None),
            };
            if (rCondition)
                QueueGCD(rAction, rTarget, rPrio);
        }

        //GF combo in filler
        QueueGCD(GunComboStep is 2 ? AID.WickedTalon : GunComboStep is 1 ? AID.SavageClaw : AID.None, mainTarget, GCDPriority.SlightlyHigh + 3);

        //Continuation procs
        //Hypervelocity, Fated Brand, Jugular Rip, Abdomen Tear, & Eye Gouge
        if (Unlocked(AID.Continuation))
        {
            var c = strategy.Option(Track.Continuation);
            var cStrat = c.As<ContinuationStrategy>();
            var cTarget = SingleTargetChoice(mainTarget, c);
            var cMinimum = InCombat(cTarget) && (((HasBlast || HasRaze) && ((NMcd > 1 && GCD >= 0.6f) || GCD < 0.6f)) || HasRip || HasTear || HasGouge);
            var cAction = HasGouge ? AID.EyeGouge : HasTear ? AID.AbdomenTear : HasRip ? AID.JugularRip : HasRaze ? AID.FatedBrand : HasBlast ? AID.Hypervelocity : AID.Continuation;
            var (cCondition, cPrio) = cStrat switch
            {
                ContinuationStrategy.Automatic or ContinuationStrategy.Early => (cMinimum, ContinuationPrio()),
                ContinuationStrategy.Late => (cMinimum && GCD <= 1.25f, ContinuationPrio()),
                _ => (false, OGCDPriority.None),
            };
            if (cCondition)
                QueueOGCD(cAction, cTarget, cPrio);
        }

        //Lightning Shot
        if (Unlocked(AID.LightningShot))
        {
            var ls = strategy.Option(Track.LightningShot);
            var lsStrat = ls.As<LightningShotStrategy>();
            var lsTarget = SingleTargetChoice(mainTarget, ls);
            var (lsCondition, lsPrio) = lsStrat switch
            {
                LightningShotStrategy.OpenerFar => ((Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD && !In3y(lsTarget), GCDPriority.Forced),
                LightningShotStrategy.OpenerForce => ((Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD, GCDPriority.Forced),
                LightningShotStrategy.Force => (true, GCDPriority.Forced),
                LightningShotStrategy.Allow => (WantAOE ? !In5y(lsTarget) : !In3y(lsTarget), GCDPriority.Low + 2),
                _ => (false, GCDPriority.None),
            };
        }

        //Strength Pot
        if (ShouldUsePotion(strategy))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.VeryCritical, 0, GCD - 0.9f);

        var justusedST =
            ComboLastMove is
                AID.KeenEdge or AID.BrutalShell or //st
                AID.GnashingFang or AID.SavageClaw or AID.ReignOfBeasts or AID.NobleBlood; //Gauge
        var justusedAOE = ComboLastMove is AID.DemonSlice;
        var stTarget = SingleTargetChoice(mainTarget, aoe);
        var autoTarget = !WantAOE || justusedST ? stTarget : Player;
        var (aoeAction, aoeTarget) = aoeStrat switch
        {
            AOEStrategy.AutoFinishWithOvercap => (WantAOE ? AOEFinish(true) : STFinish(true), autoTarget),
            AOEStrategy.AutoFinishWithoutOvercap => (WantAOE ? AOEFinish(false) : STFinish(false), autoTarget),
            AOEStrategy.AutoBreakWithOvercap => (WantAOE ? AOEBreak(true) : STBreak(true), autoTarget),
            AOEStrategy.AutoBreakWithoutOvercap => (WantAOE ? AOEBreak(false) : STBreak(false), autoTarget),
            AOEStrategy.ForceSTFinishWithOvercap => (STFinish(true), justusedAOE ? Player : stTarget),
            AOEStrategy.ForceSTFinishWithoutOvercap => (STFinish(false), justusedAOE ? Player : stTarget),
            AOEStrategy.ForceAOEFinishWithOvercap => (AOEFinish(true), justusedST ? stTarget : Player),
            AOEStrategy.ForceAOEFinishWithoutOvercap => (AOEFinish(false), justusedST ? stTarget : Player),
            AOEStrategy.ForceSTBreakWithOvercap => (STBreak(true), stTarget),
            AOEStrategy.ForceSTBreakWithoutOvercap => (STBreak(false), stTarget),
            AOEStrategy.ForceAOEBreakWithOvercap => (AOEBreak(true), Player),
            AOEStrategy.ForceAOEBreakWithoutOvercap => (AOEBreak(false), Player),
            _ => (AID.None, null),
        };

        if ((WantAOE ? In5y(aoeTarget) : In3y(aoeTarget)) && aoeTarget != null)
            QueueGCD(aoeAction, aoeTarget, GCDPriority.Low);

        GetNextTarget(strategy, ref primaryTarget, 3);
        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.DemonSlice, 2, maximumActionRange: 20);
    }
}
