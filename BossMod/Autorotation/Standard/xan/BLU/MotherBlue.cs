using BossMod.BLU;
using System.Reflection.Metadata.Ecma335;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class MotherBlue(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player, PotionType.Intelligence)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Mother BLU17", "Mother's Blue Mage17", "Standard rotation (Mother17)", "Mother Blue17", RotationModuleQuality.WIP, BitMask.Build(Class.BLU), 80);

        def.DefineShared().AddAssociatedActions(AID.Nightbloom, AID.BeingMortal, AID.BothEnds, AID.Apokalypsis, AID.MatraMagic);

        return def;
    }
    #region Enums: Abilities / strategies
    //Categories Mimicry type depending on the current Aetheric Mimicry status.
    public enum Mimicry
    {
        None,
        Tank,
        DPS,
        Healer
    }
    //Priority of GCDs in the rotation.
    public enum GCDPriority : int
    {
        None = 0,
        FillerST = 100,
        FillerAOE = 150,
        GCDWithCooldown = 200,
        Ultravibe = 250,
        Poop = 300,
        Scoop = 301,
        BuffRefresh = 600,
        SurpanakhaRepeat = 900,
    }
    #endregion

    #region Module Variables
    //Universal variables
    private Mimicry Mimic;
    private bool WasBurstTriggered = false;
    #endregion

    #region Cooldown Helpers
    //Additional Functions
    //Universal
    protected override bool CanUse(AID action) => action switch
    {
        // TODO add other transformed actions here
        AID.DivineCataract => true,
        _ => World.Client.BlueMageSpells.Contains((uint)action)
    };
    //Detect Mimicry 
    public Mimicry CurrentMimic()
    {
        foreach (var st in Player.Statuses)
        {
            switch ((SID)st.ID)
            {
                case SID.AethericMimicryTank:
                    return Mimicry.Tank;
                case SID.AethericMimicryDPS:
                    return Mimicry.DPS;
                case SID.AethericMimicryHealer:
                    return Mimicry.Healer;
            }
        }

        return Mimicry.None;
    }
    //Tank Specific
    private void TankSpecific(Enemy? primaryTarget)
    {
        if (CanUse(AID.Devour) && !CanFitGCD(StatusLeft(SID.HPBoost), 1))
        {
            if (primaryTarget is { } t)
                Hints.GoalZones.Add(Hints.GoalSingleTarget(t.Actor, 3));
            PushGCD(AID.Devour, primaryTarget, GCDPriority.BuffRefresh);
        }

        var d = Hints.PredictedDamage.Count(p => p.Players[0] && p.Activation >= World.FutureTime(GCD + GetCastTime(AID.ChelonianGate)));
        if (d > 0)
            PushGCD(AID.ChelonianGate, Player, GCDPriority.BuffRefresh);

        if (Player.FindStatus(2497u) != null)
            PushGCD(AID.DivineCataract, Player, GCDPriority.SurpanakhaRepeat);
    }
    //Dps specific
    //Check the status of several key abilities to determine whether we can burst.
    private bool AllBurstCooldownsReady()
    {
        return
            ReadyIn(AID.PhantomFlurry) < 10 &&
            ReadyIn(AID.MatraMagic) < 10 &&
            ReadyIn(AID.BeingMortal) < 10 &&
            ReadyIn(AID.SeaShanty) < 10 &&
            ReadyIn(AID.Nightbloom) < 10;
    }
    //Determines the conditions for being in a burst window.

    #endregion

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        Mimic = CurrentMimic();

        if (World.CurrentCFCID > 0 && World.Party.WithoutSlot().Count(p => p.Type == ActorType.Player) == 1)
        {
            if (CanUse(AID.BasicInstinct) && Player.FindStatus(SID.MightyGuard) == null)
                PushGCD(AID.MightyGuard, Player);

            if (Player.FindStatus(SID.BasicInstinct) == null)
                PushGCD(AID.BasicInstinct, Player);
        }

        if (Mimic == Mimicry.DPS)
        {
            if (Player.HPMP.CurMP < Player.HPMP.MaxMP * 0.2f && CanUse(AID.LucidDreaming))
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.LucidDreaming), Player, (int)ActionQueue.Priority.VeryHigh);
                return;
            }

            if (!OnCooldown(AID.WingedReprobation) && StatusStacks(SID.WingedReprobation) == 2 && OnCooldown(AID.PhantomFlurry) && StatusStacks(SID.PhantomFlurry, pendingDuration: null) == 0)
            {
                PushGCD(AID.WingedReprobation, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (!OnCooldown(AID.WingedReprobation) && StatusStacks(SID.WingedReprobation) == 3 && OnCooldown(AID.PhantomFlurry) && StatusStacks(SID.PhantomFlurry, pendingDuration: null) == 0)
            {
                PushGCD(AID.WingedReprobation, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (Player.FindStatus(SID.Harmonize) == null && !OnCooldown(AID.TripleTrident))
            {
                PushGCD(AID.Whistle, Player, GCDPriority.BuffRefresh);
                return;
            }

            if (Player.FindStatus(SID.Tingling) == null && !OnCooldown(AID.TripleTrident))
            {
                PushGCD(AID.Tingle, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (!OnCooldown(AID.MoonFlute) && !OnCooldown(AID.Nightbloom))
            {
                PushGCD(AID.MoonFlute, Player, GCDPriority.BuffRefresh);
                return;
            }

            if (!OnCooldown(AID.JKick))
            {
                PushGCD(AID.JKick, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (!OnCooldown(AID.TripleTrident))
            {
                PushGCD(AID.TripleTrident, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (primaryTarget is { } p &&
                StatusDetails(p.Actor, SID.Bleeding, Player.InstanceID).Left < 5 &&
                Hints.PriorityTargets.Count() >= 1 &&
                OnCooldown(AID.Nightbloom))
            {
                if (Player.FindStatus(SID.Boost) == null!)
                {
                    if (Player.FindStatus(SID.Boost) == null)
                        PushGCD(AID.Bristle, primaryTarget, GCDPriority.BuffRefresh);
                }
                PushGCD(AID.SongOfTorment, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            // === Step 2: Begin Burst GCD Sequence ===
            if (!OnCooldown(AID.Nightbloom))
            {
                PushGCD(AID.Nightbloom, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            // Winged Reprobation opener (apply DoT)
            if (!OnCooldown(AID.WingedReprobation) && StatusStacks(SID.WingedReprobation) == 0)
            {
                PushGCD(AID.WingedReprobation, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            // Feather Rain (early if off CD)
            if (!OnCooldown(AID.FeatherRain))
            {
                PushGCD(AID.FeatherRain, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            // === Step 3: OGCD Usage ===
            if (!OnCooldown(AID.SeaShanty))
            {
                PushOGCD(AID.SeaShanty, Player, GCDPriority.BuffRefresh);
                return;
            }

            // Winged Reprobation follow-ups
            if (!OnCooldown(AID.WingedReprobation) && StatusStacks(SID.WingedReprobation) == 1)
            {
                PushGCD(AID.WingedReprobation, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            // More high-damage GCDs
            if (!OnCooldown(AID.ShockStrike))
            {
                PushGCD(AID.ShockStrike, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (!OnCooldown(AID.BeingMortal))
            {
                PushGCD(AID.BeingMortal, Player, GCDPriority.BuffRefresh);
                return;
            }

            // === Step 4: Bristle + Matra Combo ===
            if (!OnCooldown(AID.MatraMagic) && Player.FindStatus(SID.Boost) == null)
            {
                PushGCD(AID.Bristle, Player, GCDPriority.BuffRefresh);
                return;
            }

            if (Player.FindStatus(ClassShared.SID.Swiftcast) == null && !OnCooldown(AID.MatraMagic))
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Swiftcast), Player, (int)ActionQueue.Priority.VeryHigh);
                return;
            }

            // Fix for CS1525, CS1002, and CS1513 errors
            if (MaxChargesIn(AID.Surpanakha) < 15 && OnCooldown(AID.Nightbloom))
            {
                PushGCD(AID.Surpanakha, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (!OnCooldown(AID.MatraMagic))
            {
                PushGCD(AID.MatraMagic, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (MaxChargesIn(AID.Surpanakha) < 90 && OnCooldown(AID.Nightbloom))
            {
                PushGCD(AID.Surpanakha, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (OnCooldown(AID.PhantomFlurry))
            {
                PushGCD(AID.SonicBoom, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }

            if (OnCooldown(AID.MatraMagic))
            {
                PushGCD(AID.PhantomFlurry, primaryTarget, GCDPriority.BuffRefresh);
                return;
            }
        }

        if (CanUse(AID.TheRamsVoice) && CanUse(AID.Ultravibration) && CanUse(AID.HydroPull))
        {
            Hints.GoalZones.Add(Hints.GoalAOECircle(15));
            var priorityTotal = 0;
            var nearbyTotal = 0;
            var nearbyFrozen = 0;

            foreach (var target in Hints.PriorityTargets)
            {
                priorityTotal++;
                if (target.Actor.Position.InCircle(Player.Position, 6 + Player.HitboxRadius + target.Actor.HitboxRadius))
                {
                    nearbyTotal++;
                    if (StatusDetails(target.Actor, SID.DeepFreeze, Player.InstanceID).Left > 3)
                        nearbyFrozen++;
                }
            }
            if (nearbyTotal == priorityTotal && nearbyTotal > 2)
            {
                if (nearbyFrozen == nearbyTotal)
                    PushGCD(AID.Ultravibration, Player, GCDPriority.Ultravibe + 1);

                PushGCD(AID.TheRamsVoice, Player, GCDPriority.Ultravibe);
            }
        }
    }
}
