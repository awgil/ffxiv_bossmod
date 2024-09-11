using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.Legacy;

public sealed class LegacyGNB : LegacyModule
{
    public enum Track { AOE, Gauge, Potion, NoMercy, BloodFest, Gnash, Zone, Bow, RoughDivide }
    public enum AOEStrategy { SingleTarget, ForceAOE, Auto }
    public enum GaugeStrategy { Automatic, Spend, Hold, ForceGF, LightningShotIfNotInMelee }
    public enum PotionStrategy { Manual, Immediate, DelayUntilRaidBuffs, Special, Force }
    public enum OffensiveStrategy { Automatic, Delay, Force }

    public static RotationModuleDefinition Definition()
    {
        // TODO: add in "Hold Double Down" option?
        // TODO: think about target overrides where they make sense (ST stuff, esp things like onslaught?)
        var res = new RotationModuleDefinition("Legacy GNB", "Old pre-refactoring module", "LazyLemo, Akechi-kun", RotationModuleQuality.WIP, BitMask.Build((int)Class.GNB), 100);

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 90)
            .AddOption(AOEStrategy.SingleTarget, "ST", "Use single-target rotation")
            .AddOption(AOEStrategy.ForceAOE, "AOE", "Use aoe rotation")
            .AddOption(AOEStrategy.Auto, "Auto", "Use aoe rotation if 3+ targets would be hit, otherwise use single-target rotation");

        res.Define(Track.Gauge).As<GaugeStrategy>("Gauge", "Gauge", uiPriority: 80)
            .AddOption(GaugeStrategy.Automatic, "Automatic") // optimal spend (for the most part)
            .AddOption(GaugeStrategy.Spend, "Spend", "Spend all gauge ASAP") // burn all carts; Double Down > GF combo > Burst Strike > 123 combo
            .AddOption(GaugeStrategy.Hold, "Hold", "Hold Carts") // Hold cartridges optimally; works for both ST/AOE
            .AddOption(GaugeStrategy.ForceGF, "ForceGF", "Force Gnashing combo") // forces GF combo
            .AddOption(GaugeStrategy.LightningShotIfNotInMelee, "LightningShotIfNotInMelee", "Use Lightning Shot if outside melee");

        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 70)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, but delay slightly during opener", 270, 30)
            .AddOption(PotionStrategy.DelayUntilRaidBuffs, "DelayUntilRaidBuffs", "Delay until raidbuffs", 270, 30)
            .AddOption(PotionStrategy.Special, "Special", "2min / 8min Potion use", 270, 30)
            .AddOption(PotionStrategy.Force, "Force", "Use ASAP", 270, 30)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        res.Define(Track.NoMercy).As<OffensiveStrategy>("NoM", uiPriority: 60)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.BloodFest).As<OffensiveStrategy>("Fest", uiPriority: 50)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Gnash).As<OffensiveStrategy>("Gnash", uiPriority: 40)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Zone).As<OffensiveStrategy>("Zone", uiPriority: 30)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        res.Define(Track.Bow).As<OffensiveStrategy>("Bow", uiPriority: 20)
            .AddOption(OffensiveStrategy.Automatic, "Automatic")
            .AddOption(OffensiveStrategy.Delay, "Delay")
            .AddOption(OffensiveStrategy.Force, "Force");

        return res;
    }

    // full state needed for determining next action
    public class State(RotationModule module) : CommonState(module)
    {
        public int Ammo; // 0 to 3
        public int GunComboStep; // 0 to 4
        public float NoMercyLeft; // 0 if buff not up, max 20
        public bool ReadyToRip; // 0 if buff not up, max 10
        public bool ReadyToTear; // 0 if buff not up, max 10
        public bool ReadyToGouge; // 0 if buff not up, max 10
        public bool ReadyToBlast; // 0 if buff not up, max 10
        public float AuroraLeft; // 0 if buff not up, max 18
        public float ReadyToBreak; // 0 if buff not up, max 30
        public int NumTargetsHitByAOE;
        public int MaxCartridges;

        // upgrade paths
        public GNB.AID BestZone => Unlocked(GNB.AID.BlastingZone) ? GNB.AID.BlastingZone : GNB.AID.DangerZone;
        public GNB.AID BestHeart => Unlocked(GNB.AID.HeartOfCorundum) ? GNB.AID.HeartOfCorundum : GNB.AID.HeartOfStone;
        public GNB.AID BestContinuation => ReadyToRip ? GNB.AID.JugularRip : ReadyToTear ? GNB.AID.AbdomenTear : ReadyToGouge ? GNB.AID.EyeGouge : ReadyToBlast ? GNB.AID.Hypervelocity : GNB.AID.Continuation;
        public GNB.AID BestGnash => GunComboStep == 1 ? GNB.AID.SavageClaw : GunComboStep == 2 ? GNB.AID.WickedTalon : GNB.AID.GnashingFang;
        public GNB.AID ComboLastMove => (GNB.AID)ComboLastAction;

        public bool Unlocked(GNB.AID aid) => Module.ActionUnlocked(ActionID.MakeSpell(aid));
        public bool Unlocked(GNB.TraitID tid) => Module.TraitUnlocked((uint)tid);

        public override string ToString()
        {
            return $"ammo={Ammo}, ReadytoBlast={ReadyToBlast}, ReadytoGouge={ReadyToGouge}, ReadytoRip={ReadyToRip}, ReadytoTear={ReadyToTear}, CBT={ComboTimeLeft:f1}, RB={RaidBuffsLeft:f1}, PotCD={PotionCD:f1}, GCD={GCD:f3}, ALock={AnimationLock:f3}+{AnimationLockDelay:f3}, lvl={Level}";
        }
    }

    private readonly State _state;

    public LegacyGNB(RotationModuleManager manager, Actor player) : base(manager, player)
    {
        _state = new(this);
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        _state.UpdateCommon(primaryTarget, estimatedAnimLockDelay);
        _state.HaveTankStance = Player.FindStatus(GNB.SID.RoyalGuard) != null;

        var gauge = World.Client.GetGauge<GunbreakerGauge>();
        _state.Ammo = gauge.Ammo;
        _state.GunComboStep = gauge.AmmoComboStep;
        _state.MaxCartridges = _state.Unlocked(GNB.TraitID.CartridgeChargeII) ? 3 : 2;

        _state.NoMercyLeft = _state.StatusDetails(Player, GNB.SID.NoMercy, Player.InstanceID).Left;
        _state.ReadyToBreak = _state.StatusDetails(Player, GNB.SID.ReadyToBreak, Player.InstanceID).Left;
        _state.ReadyToRip = Player.FindStatus(GNB.SID.ReadyToRip) != null;
        _state.ReadyToTear = Player.FindStatus(GNB.SID.ReadyToTear) != null;
        _state.ReadyToGouge = Player.FindStatus(GNB.SID.ReadyToGouge) != null;
        _state.ReadyToBlast = Player.FindStatus(GNB.SID.ReadyToBlast) != null;
        _state.AuroraLeft = _state.StatusDetails(Player, GNB.SID.Aurora, Player.InstanceID).Left;
        _state.NumTargetsHitByAOE = NumTargetsHitByAOE();

        var aoe = strategy.Option(Track.AOE).As<AOEStrategy>() switch
        {
            AOEStrategy.ForceAOE => true,
            AOEStrategy.Auto => NumTargetsHitByAOE() >= 3,
            _ => false,
        };

        // TODO: refactor all that, it's kinda senseless now
        GNB.AID gcd = GetNextBestGCD(strategy, aoe);
        PushResult(gcd, primaryTarget);

        ActionID ogcd = default;
        var deadline = _state.GCD > 0 && gcd != default ? _state.GCD : float.MaxValue;
        if (_state.CanWeave(deadline - _state.OGCDSlotLength)) // first ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline - _state.OGCDSlotLength, aoe);
        if (!ogcd && _state.CanWeave(deadline)) // second/only ogcd slot
            ogcd = GetNextBestOGCD(strategy, deadline, aoe);
        PushResult(ogcd, primaryTarget);
    }

    public override string DescribeState() => _state.ToString();

    private int NumTargetsHitByAOE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    private GNB.AID GetNextUnlockedComboAction(StrategyValues strategy, bool aoe)
    {
        if (aoe)
        {
            return _state.ComboLastMove switch
            {
                GNB.AID.DemonSlice => _state.Unlocked(GNB.AID.DemonSlaughter) ? GNB.AID.DemonSlaughter : GNB.AID.DemonSlice,
                GNB.AID.BrutalShell => _state.Unlocked(GNB.AID.SolidBarrel) ? GNB.AID.SolidBarrel : GNB.AID.KeenEdge,
                GNB.AID.KeenEdge => _state.Unlocked(GNB.AID.BrutalShell) ? GNB.AID.BrutalShell : GNB.AID.KeenEdge,
                _ => GNB.AID.DemonSlice
            };
        }
        else
        {
            return _state.ComboLastMove switch
            {
                GNB.AID.DemonSlice => _state.Unlocked(GNB.AID.DemonSlaughter) ? GNB.AID.DemonSlaughter : GNB.AID.DemonSlice,
                GNB.AID.BrutalShell => _state.Unlocked(GNB.AID.SolidBarrel) ? GNB.AID.SolidBarrel : GNB.AID.KeenEdge,
                GNB.AID.KeenEdge => _state.Unlocked(GNB.AID.BrutalShell) ? GNB.AID.BrutalShell : GNB.AID.KeenEdge,
                _ => GNB.AID.KeenEdge
            };
        }
    }

    private GNB.AID GetNextAmmoAction(StrategyValues strategy, bool aoe)
    {
        var gaugeStrategy = strategy.Option(Track.Gauge).As<GaugeStrategy>();
        if (gaugeStrategy == GaugeStrategy.Spend)
        {
            if (_state.Ammo >= 1)
            {
                if (_state.CD(GNB.AID.DoubleDown) < 0.6f && _state.Ammo >= 2)
                {
                    return GNB.AID.DoubleDown;
                }
                else if (_state.CD(GNB.AID.GnashingFang) < 0.6f && _state.Ammo <= 3)
                {
                    if (_state.GunComboStep == 0)
                        return GNB.AID.GnashingFang;
                    if (_state.GunComboStep == 1)
                        return GNB.AID.SavageClaw;
                    if (_state.GunComboStep == 2)
                        return GNB.AID.WickedTalon;
                }
                return GNB.AID.BurstStrike;
            }

            if (_state.Ammo == 0 && _state.GunComboStep <= 0)
            {
                return GNB.AID.KeenEdge;
            }
        }

        if (gaugeStrategy == GaugeStrategy.Hold && _state.Ammo >= 0 && _state.NoMercyLeft >= 0)
        {
            if (aoe)
            {
                if (_state.ComboLastMove == GNB.AID.DemonSlice && _state.Ammo < 3)
                {
                    return GNB.AID.DemonSlaughter;
                }
                else if (_state.ComboLastMove == GNB.AID.DemonSlice && _state.Ammo == 3)
                {
                    return GNB.AID.FatedCircle;
                }
                else if (_state.ComboLastMove == GNB.AID.KeenEdge)
                {
                    return GNB.AID.BrutalShell;
                }

                return GNB.AID.DemonSlice;
            }
            else if (_state.ComboLastMove == GNB.AID.BrutalShell)
            {
                if (_state.Ammo < 3)
                {
                    return GNB.AID.SolidBarrel;
                }
                else if (_state.Ammo == 3)
                {
                    return GNB.AID.BurstStrike;
                }
            }
            else if (_state.ComboLastMove == GNB.AID.KeenEdge)
            {
                return GNB.AID.BrutalShell;
            }
            return GNB.AID.KeenEdge;
        }

        if (gaugeStrategy == GaugeStrategy.ForceGF)
        {
            if (_state.Ammo >= 1)
            {
                if (_state.CD(GNB.AID.GnashingFang) < 0.6f && _state.CD(GNB.AID.DoubleDown) < 0.6f)
                {
                    if (_state.GunComboStep == 0)
                    {
                        return GNB.AID.GnashingFang;
                    }
                    else if (_state.GunComboStep == 1)
                    {
                        return GNB.AID.SavageClaw;
                    }
                    else if (_state.GunComboStep == 2)
                    {
                        return GNB.AID.WickedTalon;
                    }
                }
                else if (_state.GunComboStep == 1)
                {
                    return GNB.AID.SavageClaw;
                }
                else if (_state.GunComboStep == 2)
                {
                    return GNB.AID.WickedTalon;
                }
            }
        }

        if (Service.Config.Get<GNBConfig>().Skscheck && _state.Ammo == _state.MaxCartridges - 1 && _state.ComboLastMove == GNB.AID.BrutalShell && _state.GunComboStep == 0 && _state.CD(GNB.AID.GnashingFang) < 2.5)
            return GNB.AID.SolidBarrel;
        if (!Service.Config.Get<GNBConfig>().Skscheck && _state.Ammo == _state.MaxCartridges - 1 && _state.ComboLastMove == GNB.AID.BrutalShell && _state.GunComboStep == 0 && _state.CD(GNB.AID.GnashingFang) < 2.5 && (_state.CD(GNB.AID.Bloodfest) > 20 && _state.Unlocked(GNB.AID.Bloodfest)))
            return GNB.AID.SolidBarrel;

        if (Service.Config.Get<GNBConfig>().EarlySonicBreak && _state.CD(GNB.AID.NoMercy) > 40 && _state.ReadyToBreak > 27.5)
            return GNB.AID.SonicBreak;

        // Lv30-53 NM proc ST
        if (_state.Unlocked(GNB.AID.NoMercy))
        {
            bool canUseBurstStrike = !_state.Unlocked(GNB.AID.FatedCircle) &&
                                     !_state.Unlocked(GNB.AID.DoubleDown) &&
                                     !_state.Unlocked(GNB.AID.Bloodfest) &&
                                     !_state.Unlocked(GNB.AID.Continuation) &&
                                     !_state.Unlocked(GNB.AID.GnashingFang) &&
                                     !_state.Unlocked(GNB.AID.SonicBreak);

            // ST
            if (!aoe)
            {
                if (!_state.Unlocked(GNB.AID.FatedCircle) &&
                    !_state.Unlocked(GNB.AID.DoubleDown) &&
                    !_state.Unlocked(GNB.AID.Bloodfest) &&
                    !_state.Unlocked(GNB.AID.Continuation) &&
                    !_state.Unlocked(GNB.AID.GnashingFang) &&
                    !_state.Unlocked(GNB.AID.SonicBreak) &&
                    _state.Ammo >= 2)
                {
                    return GNB.AID.NoMercy;
                }
                else if (canUseBurstStrike && _state.CD(GNB.AID.NoMercy) < 40 && _state.Ammo >= 2 && _state.ComboLastMove == GNB.AID.BrutalShell)
                {
                    return GNB.AID.BurstStrike;
                }
            }

            // AOE
            if (aoe)
            {
                if (!_state.Unlocked(GNB.AID.FatedCircle) &&
                    !_state.Unlocked(GNB.AID.DoubleDown) &&
                    !_state.Unlocked(GNB.AID.Bloodfest) &&
                    !_state.Unlocked(GNB.AID.Continuation) &&
                    !_state.Unlocked(GNB.AID.GnashingFang) &&
                    !_state.Unlocked(GNB.AID.SonicBreak) &&
                    _state.Ammo >= 2)
                {
                    return GNB.AID.NoMercy;
                }
                else if (canUseBurstStrike && _state.CD(GNB.AID.NoMercy) < 40 && _state.Ammo >= 2 && _state.ComboLastMove == GNB.AID.DemonSlice)
                {
                    return GNB.AID.BurstStrike;
                }
            }
        }

        if (_state.CD(GNB.AID.NoMercy) > 17)
        {
            if (_state.GunComboStep == 0 && _state.Unlocked(GNB.AID.GnashingFang) && _state.CD(GNB.AID.GnashingFang) < 0.6f && _state.Ammo >= 1 && ShouldUseGnash(strategy) && _state.NumTargetsHitByAOE <= 3)
                return GNB.AID.GnashingFang;
        }

        if (_state.NoMercyLeft > _state.AnimationLock)
        {
            if (_state.Unlocked(GNB.AID.SonicBreak) && _state.ReadyToBreak > 27.5)
                return GNB.AID.SonicBreak;
            if (_state.CD(GNB.AID.DoubleDown) < 0.6f && _state.Unlocked(GNB.AID.DoubleDown) && _state.Ammo >= 2 && _state.RangeToTarget <= 5)
                return GNB.AID.DoubleDown;
            if (!aoe && _state.CD(GNB.AID.DoubleDown) < _state.GCD && _state.CD(GNB.AID.GnashingFang) > _state.GCD && _state.Unlocked(GNB.AID.DoubleDown) && _state.Ammo == 1 && _state.CD(GNB.AID.Bloodfest) < 1.9)
                return GNB.AID.BurstStrike;
            if (aoe && _state.CD(GNB.AID.DoubleDown) < _state.GCD && _state.CD(GNB.AID.GnashingFang) > _state.GCD && _state.Unlocked(GNB.AID.DoubleDown) && _state.Ammo == 1 && _state.CD(GNB.AID.Bloodfest) < 1.9)
                return GNB.AID.FatedCircle;
            if (!aoe && _state.Ammo >= 1 && _state.CD(GNB.AID.GnashingFang) > _state.GCD && _state.CD(GNB.AID.DoubleDown) > _state.GCD && _state.Unlocked(GNB.AID.DoubleDown) && _state.GunComboStep == 0)
                return GNB.AID.BurstStrike;
            if (!aoe && _state.Ammo >= 1 && _state.CD(GNB.AID.GnashingFang) > _state.GCD && !_state.Unlocked(GNB.AID.DoubleDown) && _state.GunComboStep == 0)
                return GNB.AID.BurstStrike;
            if (!aoe && _state.Ammo >= 1 && _state.CD(GNB.AID.GnashingFang) > _state.GCD && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.SonicBreak) && _state.GunComboStep == 0)
                return GNB.AID.BurstStrike;
            if (!_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.Bloodfest) && !_state.Unlocked(GNB.AID.Continuation) && !_state.Unlocked(GNB.AID.GnashingFang) && !_state.Unlocked(GNB.AID.SonicBreak) && _state.Ammo >= 2)
                return GNB.AID.BurstStrike;

            if (!aoe)
            {
                if (_state.Ammo >= 1 && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.Bloodfest) && !_state.Unlocked(GNB.AID.Continuation) && !_state.Unlocked(GNB.AID.GnashingFang) && !_state.Unlocked(GNB.AID.SonicBreak))
                    return GNB.AID.BurstStrike;
            }
            // AOE
            else if (aoe)
            {
                if (_state.NoMercyLeft > 0)
                {
                    if (_state.Ammo >= 1)
                    {
                        if (_state.Unlocked(GNB.AID.GnashingFang) && _state.CD(GNB.AID.GnashingFang) == 0)
                        {
                            return GNB.AID.GnashingFang; // Lv60+ AOE GF
                        }
                        if (!_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown))
                        {
                            return GNB.AID.BurstStrike; // Lv 30-72 AOE BS
                        }
                    }
                    if (_state.Ammo >= 2 && !_state.Unlocked(GNB.AID.DoubleDown) &&
                        !_state.Unlocked(GNB.AID.Bloodfest) && !_state.Unlocked(GNB.AID.Continuation) && !_state.Unlocked(GNB.AID.GnashingFang) && !_state.Unlocked(GNB.AID.SonicBreak))
                    {
                        return GNB.AID.BurstStrike; // Lv30-53 AOE BS
                    }
                    if (_state.Ammo >= 2 && _state.Unlocked(GNB.AID.SonicBreak) && _state.Unlocked(GNB.AID.GnashingFang) &&
                        !_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown))
                    {
                        return GNB.AID.GnashingFang; // Lv60 AOE GF fix
                    }
                }
                if (_state.Ammo >= 1 && _state.GunComboStep == 0)
                {
                    if (_state.NoMercyLeft > 0 && !_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.Bloodfest) &&
                        _state.Unlocked(GNB.AID.Continuation))
                    {
                        return GNB.AID.BurstStrike; // Lv70 AOE BS
                    }
                    if (_state.Ammo >= 1 && _state.NoMercyLeft > 0 && _state.Unlocked(GNB.AID.SonicBreak) && _state.Unlocked(GNB.AID.GnashingFang) &&
                        !_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown))
                    {
                        return GNB.AID.BurstStrike; // Lv60 AOE BS
                    }
                    if (_state.CD(GNB.AID.GnashingFang) > _state.GCD && _state.CD(GNB.AID.DoubleDown) > _state.GCD
                         && _state.Unlocked(GNB.AID.DoubleDown))
                    {
                        return GNB.AID.FatedCircle; // Lv80 AOE
                    }
                    if (_state.CD(GNB.AID.GnashingFang) > _state.GCD && _state.Unlocked(GNB.AID.FatedCircle) &&
                        !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.SonicBreak))
                    {
                        return GNB.AID.FatedCircle; // Lv80 AOE 
                    }
                    if (_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown) &&
                        !_state.Unlocked(GNB.AID.SonicBreak) && !_state.Unlocked(GNB.AID.GnashingFang))
                    {
                        return GNB.AID.FatedCircle; // Lv80 AOE
                    }
                }
            }
        }

        if (_state.GunComboStep > 0)
        {
            if (_state.GunComboStep == 2)
                return GNB.AID.WickedTalon;
            if (_state.GunComboStep == 1)
                return GNB.AID.SavageClaw;
        }

        if (_state.ComboLastMove == GNB.AID.BrutalShell && _state.Ammo == _state.MaxCartridges && _state.Unlocked(GNB.AID.BurstStrike))
        {
            return GNB.AID.BurstStrike;
        }

        if (aoe && _state.ComboLastMove == GNB.AID.DemonSlice && _state.Ammo == _state.MaxCartridges && _state.Unlocked(GNB.AID.FatedCircle))
        {
            return GNB.AID.FatedCircle;
        }

        if (gaugeStrategy == GaugeStrategy.Spend && _state.Ammo >= 0)
        {
            if (_state.Ammo >= 2)
            {
                if (_state.CD(GNB.AID.DoubleDown) < 0.6f && _state.Ammo > 2)
                    return GNB.AID.DoubleDown;

                if (_state.CD(GNB.AID.GnashingFang) < 0.6f && _state.Ammo <= 2)
                {
                    if (_state.GunComboStep == 0)
                        return GNB.AID.GnashingFang;
                    if (_state.GunComboStep == 1)
                        return GNB.AID.SavageClaw;
                    if (_state.GunComboStep == 2)
                        return GNB.AID.WickedTalon;
                }

                return GNB.AID.BurstStrike;
            }

            if (_state.Ammo == 0 && _state.GunComboStep <= 0)
            {
                return GNB.AID.KeenEdge;
            }
        }

        if (gaugeStrategy == GaugeStrategy.Hold && _state.Ammo >= 0 && _state.NoMercyLeft >= 0)
        {
            if (aoe)
            {
                if (_state.ComboLastMove == GNB.AID.DemonSlice)
                {
                    return GNB.AID.DemonSlaughter;
                }
                else if (_state.ComboLastMove == GNB.AID.BrutalShell)
                {
                    return GNB.AID.SolidBarrel;
                }
                else if (_state.ComboLastMove == GNB.AID.KeenEdge)
                {
                    return GNB.AID.BrutalShell;
                }

                return GNB.AID.DemonSlice;
            }
            else if (_state.ComboLastMove == GNB.AID.BrutalShell)
            {
                if (_state.Ammo < 3)
                {
                    return GNB.AID.SolidBarrel;
                }
                else if (_state.Ammo == 3)
                {
                    return GNB.AID.BurstStrike;
                }
            }
            else if (_state.ComboLastMove == GNB.AID.KeenEdge)
            {
                return GNB.AID.BrutalShell;
            }
            return GNB.AID.KeenEdge;
        }

        if (gaugeStrategy == GaugeStrategy.ForceGF)
        {
            if (_state.Ammo >= 1)
            {
                if (_state.CD(GNB.AID.GnashingFang) < 0.6f && _state.CD(GNB.AID.DoubleDown) < 0.6f)
                {
                    if (_state.GunComboStep == 0)
                    {
                        return GNB.AID.GnashingFang;
                    }
                    else if (_state.GunComboStep == 1)
                    {
                        return GNB.AID.SavageClaw;
                    }
                    else if (_state.GunComboStep == 2)
                    {
                        return GNB.AID.WickedTalon;
                    }
                }
                else if (_state.GunComboStep == 1)
                {
                    return GNB.AID.SavageClaw;
                }
                else if (_state.GunComboStep == 2)
                {
                    return GNB.AID.WickedTalon;
                }
            }
        }

        return GetNextUnlockedComboAction(strategy, aoe);
    }

    private bool ShouldSpendGauge(StrategyValues strategy) => strategy.Option(Track.Gauge).As<GaugeStrategy>() switch
    {
        GaugeStrategy.Automatic or GaugeStrategy.LightningShotIfNotInMelee => (_state.RaidBuffsLeft > _state.GCD || _state.FightEndIn <= _state.RaidBuffsIn + 10),
        GaugeStrategy.Spend => true,
        GaugeStrategy.ForceGF => true,
        GaugeStrategy.Hold => true,
        _ => true
    };

    private bool ShouldUsePotion(StrategyValues strategy) => strategy.Option(Track.Potion).As<PotionStrategy>() switch
    {
        PotionStrategy.Manual => false,
        PotionStrategy.Immediate => (!Service.Config.Get<GNBConfig>().EarlyNoMercy && _state.ComboLastMove == GNB.AID.KeenEdge && _state.Ammo == 0) || (_state.CD(GNB.AID.NoMercy) < 5.5 && _state.CD(GNB.AID.Bloodfest) < 15 /*&& strategy.CombatTimer > 30*/) || (Service.Config.Get<GNBConfig>().EarlyNoMercy && _state.CD(GNB.AID.NoMercy) < 5.5 && _state.CD(GNB.AID.Bloodfest) < 15),
        PotionStrategy.Special => _state.ComboLastMove == GNB.AID.BrutalShell && _state.Ammo == 3 && _state.CD(GNB.AID.NoMercy) < 3 && _state.CD(GNB.AID.Bloodfest) < 15,
        PotionStrategy.Force => true,
        _ => false
    };

    private bool ShouldUseNoMercy(StrategyValues strategy)
    {
        var noMergyStrategy = strategy.Option(Track.NoMercy).As<OffensiveStrategy>();
        if (noMergyStrategy == OffensiveStrategy.Delay)
        {
            return false;
        }
        else if (noMergyStrategy == OffensiveStrategy.Force)
        {
            return true;
        }
        else
        {
            var gnbConfig = Service.Config.Get<GNBConfig>();
            bool isEarlyNoMercy = gnbConfig.EarlyNoMercy;

            bool isGnashingFangReady = _state.CD(GNB.AID.GnashingFang) < 2.5 && _state.Unlocked(GNB.AID.GnashingFang);
            bool isSonicBreakReady = _state.CD(GNB.AID.NoMercy) >= 40 && _state.Unlocked(GNB.AID.SonicBreak) && _state.ReadyToBreak > 0;
            bool isDoubleDownReady = _state.CD(GNB.AID.DoubleDown) < 2.5 && _state.Unlocked(GNB.AID.DoubleDown);
            bool justusewhenever = !_state.Unlocked(GNB.AID.BurstStrike) && _state.TargetingEnemy && _state.RangeToTarget < 5;

            bool shouldUseEarlyNoMercy = _state.TargetingEnemy && ((!isEarlyNoMercy && _state.ComboLastMove == GNB.AID.BrutalShell) || (isEarlyNoMercy && _state.ComboLastMove == GNB.AID.KeenEdge)) && _state.Unlocked(GNB.AID.Bloodfest) && (_state.Ammo == 0 || _state.Ammo == _state.MaxCartridges) && ((_state.GCD < 0.8 && gnbConfig.Skscheck) || (!gnbConfig.Skscheck));

            bool shouldUseRegularNoMercy = (!gnbConfig.Skscheck
                && (isGnashingFangReady || isSonicBreakReady || isDoubleDownReady)
                && _state.TargetingEnemy
                && ((_state.Ammo == _state.MaxCartridges)
                || (_state.Ammo == _state.MaxCartridges - 1 && _state.ComboLastMove == GNB.AID.BrutalShell && _state.CD(GNB.AID.Bloodfest) > 20)
                || (_state.CD(GNB.AID.Bloodfest) < 15 && _state.Ammo == 1 && _state.Unlocked(GNB.AID.Bloodfest)))) || shouldUseEarlyNoMercy;

            bool shouldUseSksCheck = (gnbConfig.Skscheck && _state.GCD < 0.8
                && (isGnashingFangReady || isSonicBreakReady || isDoubleDownReady)
                && _state.TargetingEnemy
                && (_state.Ammo == _state.MaxCartridges
                || (_state.CD(GNB.AID.Bloodfest) < 15 && _state.Ammo == 1 && _state.Unlocked(GNB.AID.Bloodfest))
                || (_state.Ammo == _state.MaxCartridges - 1 && _state.ComboLastMove == GNB.AID.BrutalShell && _state.CD(GNB.AID.Bloodfest) > 20 && _state.Unlocked(GNB.AID.Bloodfest)))) || shouldUseEarlyNoMercy;

            return shouldUseRegularNoMercy || shouldUseSksCheck || justusewhenever;
        }
    }

    private bool ShouldUseGnash(StrategyValues strategy) => strategy.Option(Track.Gnash).As<OffensiveStrategy>() switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.TargetingEnemy && _state.Unlocked(GNB.AID.GnashingFang) && _state.CD(GNB.AID.GnashingFang) < 0.6f && _state.Ammo >= 1
    };

    private bool ShouldUseZone(StrategyValues strategy) => strategy.Option(Track.Zone).As<OffensiveStrategy>() switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.TargetingEnemy && _state.Unlocked(GNB.AID.SonicBreak) && _state.CD(GNB.AID.NoMercy) <= 57.5 && _state.CD(GNB.AID.NoMercy) > 17
    };

    private bool ShouldUseFest(StrategyValues strategy)
    {
        var festStrategy = strategy.Option(Track.Zone).As<OffensiveStrategy>();
        if (festStrategy == OffensiveStrategy.Delay)
        {
            return false;
        }
        else if (festStrategy == OffensiveStrategy.Force)
        {
            return true;
        }
        else
        {
            bool inNoMercy = _state.NoMercyLeft > _state.AnimationLock && _state.Unlocked(GNB.AID.Bloodfest);
            return inNoMercy && _state.Ammo == 0;
        }
    }

    private bool ShouldUseBow(StrategyValues strategy) => strategy.Option(Track.Bow).As<OffensiveStrategy>() switch
    {
        OffensiveStrategy.Delay => false,
        OffensiveStrategy.Force => true,
        _ => Player.InCombat && _state.TargetingEnemy && _state.Unlocked(GNB.AID.BowShock) && _state.CD(GNB.AID.NoMercy) > 40
    };

    private GNB.AID GetNextBestGCD(StrategyValues strategy, bool aoe)
    {
        // prepull
        if (!_state.TargetingEnemy || _state.CountdownRemaining > 0.7f)
            return GNB.AID.None;

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.LightningShotIfNotInMelee && _state.RangeToTarget > 3)
            return GNB.AID.LightningShot;

        if (ShouldSpendGauge(strategy))
        {
            if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.Spend && _state.Ammo > 0)
            {
                if (_state.CD(GNB.AID.DoubleDown) < 0.6f && _state.Ammo > 2)
                    return GNB.AID.DoubleDown;

                if (_state.CD(GNB.AID.GnashingFang) < 0.6f && _state.Ammo <= 2)
                {
                    if (_state.GunComboStep == 0)
                        return GNB.AID.GnashingFang;
                    if (_state.GunComboStep == 1)
                        return GNB.AID.SavageClaw;
                    if (_state.GunComboStep == 2)
                        return GNB.AID.WickedTalon;
                }

                return GNB.AID.BurstStrike;
            }

            if (_state.Ammo == 0 && _state.GunComboStep <= 0)
            {
                return GNB.AID.KeenEdge;
            }

            if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.Hold && _state.Ammo >= 0 && _state.NoMercyLeft >= 0 && (aoe || !aoe))
            {
                if (_state.ComboLastMove == GNB.AID.DemonSlice)
                {
                    return GNB.AID.DemonSlaughter;
                }
                else if (_state.ComboLastMove == GNB.AID.BrutalShell)
                {
                    return GNB.AID.SolidBarrel;
                }
                else if (_state.ComboLastMove == GNB.AID.KeenEdge)
                {
                    return GNB.AID.BrutalShell;
                }

                return GNB.AID.DemonSlice;
            }
            else if (_state.ComboLastMove == GNB.AID.BrutalShell)
            {
                if (_state.Ammo < 3)
                {
                    return GNB.AID.SolidBarrel;
                }
                else if (_state.Ammo == 3)
                {
                    return GNB.AID.BurstStrike;
                }
            }
            else if (_state.ComboLastMove == GNB.AID.KeenEdge)
            {
                return GNB.AID.BrutalShell;
            }

            if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.ForceGF && _state.Ammo >= 1)
            {
                if (_state.CD(GNB.AID.GnashingFang) < 0.6f && _state.CD(GNB.AID.DoubleDown) < 0.6f)
                {
                    if (_state.GunComboStep == 0)
                    {
                        return GNB.AID.GnashingFang;
                    }
                    else if (_state.GunComboStep == 1)
                    {
                        return GNB.AID.SavageClaw;
                    }
                    else if (_state.GunComboStep == 2)
                    {
                        return GNB.AID.WickedTalon;
                    }
                }
                else if (_state.GunComboStep == 1)
                {
                    return GNB.AID.SavageClaw;
                }
                else if (_state.GunComboStep == 2)
                {
                    return GNB.AID.WickedTalon;
                }
            }
        }

        if (!aoe)
        {
            if (_state.Ammo >= 2 && !_state.Unlocked(GNB.AID.DoubleDown) &&
                !_state.Unlocked(GNB.AID.Bloodfest) && !_state.Unlocked(GNB.AID.Continuation) && !_state.Unlocked(GNB.AID.GnashingFang) &&
                !_state.Unlocked(GNB.AID.SonicBreak))
            {
                return GNB.AID.BurstStrike;
            }
        }
        // AOE Logic
        else if (aoe)
        {
            if (_state.Ammo >= 2)
            {
                if (_state.Ammo >= 2)
                {
                    if (_state.Unlocked(GNB.AID.GnashingFang) && _state.CD(GNB.AID.GnashingFang) == 0)
                    {
                        return GNB.AID.GnashingFang; // Lv60+ AOE GF
                    }
                    if (!_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown))
                    {
                        return GNB.AID.BurstStrike; // Lv30-72 AOE BS
                    }
                }
                if (_state.Ammo >= 2 && !_state.Unlocked(GNB.AID.DoubleDown) &&
                    !_state.Unlocked(GNB.AID.Bloodfest) && !_state.Unlocked(GNB.AID.Continuation) && !_state.Unlocked(GNB.AID.GnashingFang) && !_state.Unlocked(GNB.AID.SonicBreak))
                {
                    return GNB.AID.BurstStrike; // Lv30-53 AOE BS
                }
                if (_state.Ammo >= 2 && _state.Unlocked(GNB.AID.SonicBreak) && _state.Unlocked(GNB.AID.GnashingFang) &&
                    !_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown))
                {
                    return GNB.AID.GnashingFang; // Lv60 AOE GF fix
                }
                else if (_state.Ammo >= 2 && _state.Unlocked(GNB.AID.SonicBreak) && _state.Unlocked(GNB.AID.GnashingFang) && (_state.CD(GNB.AID.GnashingFang) > _state.AnimationLock && !_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown)))
                {
                    return GNB.AID.BurstStrike; // Lv60 AOE BS 
                }
            }
            if (_state.Ammo >= 2 && _state.GunComboStep == 0)
            {
                if (!_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.Bloodfest) &&
                    _state.Unlocked(GNB.AID.Continuation))
                {
                    return GNB.AID.BurstStrike; // Lv70 AOE BS
                }
                if (_state.CD(GNB.AID.GnashingFang) > _state.GCD && _state.CD(GNB.AID.DoubleDown) > _state.GCD &&
                    _state.Unlocked(GNB.AID.DoubleDown))
                {
                    return GNB.AID.FatedCircle; // Lv80 AOE
                }
                if (_state.CD(GNB.AID.GnashingFang) > _state.GCD && _state.Unlocked(GNB.AID.FatedCircle) &&
                    !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.SonicBreak))
                {
                    return GNB.AID.FatedCircle; // Lv80 AOE
                }
                if (_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown) &&
                    !_state.Unlocked(GNB.AID.SonicBreak) && !_state.Unlocked(GNB.AID.GnashingFang))
                {
                    return GNB.AID.FatedCircle; // Lv80 AOE
                }
            }
        }

        if (_state.ReadyToBlast)
            return _state.BestContinuation;
        if (_state.ReadyToGouge)
            return _state.BestContinuation;
        if (_state.ReadyToTear)
            return _state.BestContinuation;
        if (_state.ReadyToRip)
            return _state.BestContinuation;

        if (_state.Ammo >= _state.MaxCartridges && _state.ComboLastMove == GNB.AID.BrutalShell)
            return GetNextAmmoAction(strategy, aoe);

        if (_state.Ammo >= _state.MaxCartridges && _state.ComboLastMove == GNB.AID.DemonSlice)
            return GetNextAmmoAction(strategy, aoe);

        if (_state.NoMercyLeft > _state.AnimationLock)
            return GetNextAmmoAction(strategy, aoe);

        if (_state.CD(GNB.AID.GnashingFang) < 0.6f)
            return GetNextAmmoAction(strategy, aoe);

        if (_state.GunComboStep > 0)
        {
            if (_state.GunComboStep == 2)
                return GNB.AID.WickedTalon;
            if (_state.GunComboStep == 1)
                return GNB.AID.SavageClaw;
        }

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.Spend)
            return GetNextAmmoAction(strategy, aoe);

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.Hold)
            return GetNextUnlockedComboAction(strategy, aoe);

        if (strategy.Option(Track.Gauge).As<GaugeStrategy>() == GaugeStrategy.ForceGF)
            return GetNextAmmoAction(strategy, aoe);

        return GetNextUnlockedComboAction(strategy, aoe);
    }

    private ActionID GetNextBestOGCD(StrategyValues strategy, float deadline, bool aoe)
    {
        if (ShouldUsePotion(strategy) && _state.CanWeave(_state.PotionCD, 1.1f, deadline))
            return ActionDefinitions.IDPotionStr;

        if (_state.Unlocked(GNB.AID.NoMercy))
        {
            if (ShouldUseNoMercy(strategy) && _state.CanWeave(GNB.AID.NoMercy, 0.6f, deadline))
                return ActionID.MakeSpell(GNB.AID.NoMercy);
        }

        if (_state.Unlocked(GNB.AID.DangerZone) && ShouldUseZone(strategy) && _state.CanWeave(GNB.AID.DangerZone, 0.6f, deadline))
            return ActionID.MakeSpell(_state.BestZone);

        if (_state.Unlocked(GNB.AID.BowShock) && ShouldUseBow(strategy) && _state.CanWeave(GNB.AID.BowShock, 0.6f, deadline))
            return ActionID.MakeSpell(GNB.AID.BowShock);

        if (_state.ReadyToBlast && _state.Unlocked(GNB.AID.Hypervelocity))
            return ActionID.MakeSpell(_state.BestContinuation);
        if (_state.ReadyToGouge && _state.Unlocked(GNB.AID.Continuation))
            return ActionID.MakeSpell(_state.BestContinuation);
        if (_state.ReadyToTear && _state.Unlocked(GNB.AID.Continuation))
            return ActionID.MakeSpell(_state.BestContinuation);
        if (_state.ReadyToRip && _state.Unlocked(GNB.AID.Continuation))
            return ActionID.MakeSpell(_state.BestContinuation);

        if (_state.Unlocked(GNB.AID.Bloodfest) && _state.CanWeave(GNB.AID.Bloodfest, 0.6f, deadline) && ShouldUseFest(strategy))
            return ActionID.MakeSpell(GNB.AID.Bloodfest);

        // Lv30-53 NM proc ST
        if (_state.Unlocked(GNB.AID.NoMercy))
        {
            if (!_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.Bloodfest) && !_state.Unlocked(GNB.AID.Continuation) && !_state.Unlocked(GNB.AID.GnashingFang) && !_state.Unlocked(GNB.AID.SonicBreak) && _state.Ammo == 2 && _state.CanWeave(GNB.AID.NoMercy, 0.6f, deadline))
                return ActionID.MakeSpell(GNB.AID.NoMercy);
        }

        // Lv30-53 NM proc AOE
        if (_state.Unlocked(GNB.AID.NoMercy))
        {
            if (aoe && !_state.Unlocked(GNB.AID.FatedCircle) && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.Bloodfest) && !_state.Unlocked(GNB.AID.Continuation) && !_state.Unlocked(GNB.AID.GnashingFang) && _state.Ammo == 2 && !_state.Unlocked(GNB.AID.SonicBreak) && _state.Ammo == 2 && _state.CanWeave(GNB.AID.NoMercy, 0.6f, deadline))
                return ActionID.MakeSpell(GNB.AID.NoMercy);
        }

        if (_state.CanWeave(_state.CD(GNB.AID.Aurora) - 60, 0.6f, deadline) && _state.Unlocked(GNB.AID.Aurora) && _state.AuroraLeft < _state.GCD && _state.CD(GNB.AID.NoMercy) > 1 && _state.CD(GNB.AID.GnashingFang) > 1 && _state.CD(GNB.AID.DoubleDown) > 1)
            return ActionID.MakeSpell(GNB.AID.Aurora);

        if (_state.CanWeave(_state.CD(GNB.AID.Aurora) - 60, 0.6f, deadline) && _state.Unlocked(GNB.AID.Aurora) && !_state.Unlocked(GNB.AID.DoubleDown) && _state.AuroraLeft < _state.GCD && _state.CD(GNB.AID.NoMercy) > 1 && _state.CD(GNB.AID.GnashingFang) > 1)
            return ActionID.MakeSpell(GNB.AID.Aurora);

        if (_state.CanWeave(_state.CD(GNB.AID.Aurora) - 60, 0.6f, deadline) && _state.Unlocked(GNB.AID.Aurora) && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.SonicBreak) && _state.AuroraLeft < _state.GCD && _state.CD(GNB.AID.NoMercy) > 1 && _state.CD(GNB.AID.GnashingFang) > 1)
            return ActionID.MakeSpell(GNB.AID.Aurora);

        if (_state.CanWeave(_state.CD(GNB.AID.Aurora) - 60, 0.6f, deadline) && _state.Unlocked(GNB.AID.Aurora) && !_state.Unlocked(GNB.AID.DoubleDown) && !_state.Unlocked(GNB.AID.SonicBreak) && !_state.Unlocked(GNB.AID.GnashingFang) && _state.AuroraLeft < _state.GCD && _state.CD(GNB.AID.NoMercy) > 1)
            return ActionID.MakeSpell(GNB.AID.Aurora);

        return new();
    }
}
