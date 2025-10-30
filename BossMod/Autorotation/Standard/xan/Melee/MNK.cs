using BossMod.MNK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class MNK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player, PotionType.Strength)
{
    public enum Track { BH = SharedTrack.Buffs, RoF, FiresReply, RoW, WindsReply, PB, Nadi, Blitz, SSS, FormShift, Meditation, TC, Potion, Engage, TN }
    public enum PotionStrategy
    {
        Manual,
        PreBuffs,
        Now
    }
    public enum MeditationStrategy
    {
        Safe,
        Greedy,
        Force,
        Delay
    }
    public enum FRStrategy
    {
        Automatic,
        Ranged,
        Force,
        Delay
    }
    public enum WRStrategy
    {
        Automatic,
        Force,
        PreDowntime
    }
    public enum NadiStrategy
    {
        Automatic,
        [PropertyDisplay("Lunar", 0xFFDB8BCA)]
        Lunar,
        [PropertyDisplay("Solar", 0xFF8EE6FA)]
        Solar
    }
    public enum RoFStrategy
    {
        Automatic,
        Force,
        ForceMidWeave,
        Delay,
    }
    public enum RoWStrategy
    {
        Automatic,
        Force,
        Delay
    }
    public enum PBStrategy
    {
        Automatic,
        ForceOpo,
        Force,
        Delay,
        DowntimeSolar,
        DowntimeLunar
    }
    public enum TCStrategy
    {
        None,
        GapClose
    }
    public enum PositionalStrategy
    {
        Automatic,
        Ignore
    }
    public enum BlitzStrategy
    {
        Automatic,
        RoF,
        Multi,
        MultiRoF,
        Delay
    }
    public enum EngageStrategy
    {
        TC,
        Sprint,
        FacepullDK,
        FacepullDemo
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan MNK", "Monk", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Good, BitMask.Build(Class.MNK, Class.PGL), 100);

        def.DefineSharedTA();

        // buffs
        def.DefineSimple(Track.BH, "BH", minLevel: 70, uiPriority: 99).AddAssociatedActions(AID.Brotherhood);

        def.Define(Track.RoF).As<RoFStrategy>("RoF", uiPriority: 96)
            .AddOption(RoFStrategy.Automatic, "Automatically use RoF during burst window", minLevel: 68)
            .AddOption(RoFStrategy.Force, "Use ASAP", minLevel: 68)
            .AddOption(RoFStrategy.ForceMidWeave, "Use ASAP, but retain late-weave to ensure maximum GCDs covered", minLevel: 68)
            .AddOption(RoFStrategy.Delay, "Do not use", minLevel: 68)
            .AddAssociatedActions(AID.RiddleOfFire);
        def.Define(Track.FiresReply).As<FRStrategy>("FiresReply", uiPriority: 95)
            .AddOption(FRStrategy.Automatic, "Use after Opo GCD", minLevel: 100)
            .AddOption(FRStrategy.Ranged, "Use when out of melee range, or if about to expire", minLevel: 100)
            .AddOption(FRStrategy.Force, "Use ASAP", minLevel: 100)
            .AddOption(FRStrategy.Delay, "Do not use", minLevel: 100)
            .AddAssociatedActions(AID.FiresReply);

        def.DefineSimple(Track.RoW, "RoW", minLevel: 72, uiPriority: 94).AddAssociatedActions(AID.RiddleOfWind);
        def.Define(Track.WindsReply).As<WRStrategy>("WindsReply", uiPriority: 93)
            .AddOption(WRStrategy.Automatic, "Use out of melee range, or if about to expire", minLevel: 96)
            .AddOption(WRStrategy.Force, "Use ASAP", minLevel: 96)
            .AddOption(WRStrategy.PreDowntime, "Ensure usage at least 2 GCDs before next downtime", minLevel: 96)
            .AddAssociatedActions(AID.WindsReply);

        // PB-related settings
        def.Define(Track.PB).As<PBStrategy>("PB", uiPriority: 89)
            .AddOption(PBStrategy.Automatic, "Automatically use after Opo before or during Riddle of Fire", minLevel: 50)
            .AddOption(PBStrategy.ForceOpo, "Use ASAP after next Opo", minLevel: 50)
            .AddOption(PBStrategy.Force, "Use ASAP", minLevel: 50)
            .AddOption(PBStrategy.Delay, "Do not use", minLevel: 50)
            .AddOption(PBStrategy.DowntimeSolar, "Downtime prep: Solar", minLevel: 60, effect: 39)
            .AddOption(PBStrategy.DowntimeLunar, "Downtime prep: Lunar", minLevel: 60, effect: 39)
            .AddAssociatedActions(AID.PerfectBalance);
        def.Define(Track.Nadi).As<NadiStrategy>("Nadi", uiPriority: 88)
            .AddOption(NadiStrategy.Automatic, "Automatically choose best nadi (double lunar opener, otherwise alternate)", minLevel: 60)
            .AddOption(NadiStrategy.Lunar, minLevel: 60)
            .AddOption(NadiStrategy.Solar, minLevel: 60);
        def.Define(Track.Blitz).As<BlitzStrategy>("Blitz", uiPriority: 87)
            .AddOption(BlitzStrategy.Automatic, "Use ASAP", minLevel: 60)
            .AddOption(BlitzStrategy.RoF, "Hold blitz until Riddle of Fire is active", minLevel: 60)
            .AddOption(BlitzStrategy.Multi, "Hold blitz until at least two targets will be hit", minLevel: 60)
            .AddOption(BlitzStrategy.MultiRoF, "Hold blitz until Riddle of Fire and 2+ targets", minLevel: 60)
            .AddOption(BlitzStrategy.Delay, "Do not use", minLevel: 60)
            .AddAssociatedActions(AID.ElixirField, AID.FlintStrike, AID.TornadoKick, AID.ElixirBurst, AID.RisingPhoenix, AID.PhantomRush);

        // downtime stuff
        def.DefineSimple(Track.SSS, "SixSidedStar", minLevel: 80, uiPriority: 79).AddAssociatedActions(AID.SixSidedStar);
        def.DefineSimple(Track.FormShift, "FormShift", minLevel: 52, uiPriority: 78).AddAssociatedActions(AID.FormShift);
        def.Define(Track.Meditation).As<MeditationStrategy>("Meditate", uiPriority: 77)
            .AddOption(MeditationStrategy.Safe, "Use out of combat, during countdown, or if no enemies are targetable")
            .AddOption(MeditationStrategy.Greedy, "Allow using when primary enemy is targetable, but out of range")
            .AddOption(MeditationStrategy.Force, "Use even if enemy is in melee range")
            .AddOption(MeditationStrategy.Delay, "Do not use")
            .AddAssociatedActions(AID.SteeledMeditation);

        // other utils
        def.Define(Track.TC).As<TCStrategy>("TC", uiPriority: 69)
            .AddOption(TCStrategy.None, "Do not use", minLevel: 35)
            .AddOption(TCStrategy.GapClose, "Use if outside melee range", minLevel: 35, supportedTargets: ActionTargets.Party | ActionTargets.Hostile)
            .AddAssociatedActions(AID.Thunderclap);

        def.Define(Track.Potion).As<PotionStrategy>("Pot", uiPriority: 59)
            .AddOption(PotionStrategy.Manual, "Do not automatically use")
            .AddOption(PotionStrategy.PreBuffs, "Use ~4 GCDs before raid buff window")
            .AddOption(PotionStrategy.Now, "Use ASAP")
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        def.Define(Track.Engage).As<EngageStrategy>("Engage", uiPriority: 49)
            .AddOption(EngageStrategy.TC, "Thunderclap to target")
            .AddOption(EngageStrategy.Sprint, "Sprint to melee range")
            .AddOption(EngageStrategy.FacepullDK, "Precast Dragon Kick from melee range")
            .AddOption(EngageStrategy.FacepullDemo, "Precast Demolish from melee range");

        def.DefineSimple(Track.TN, "TrueNorth", minLevel: 50, uiPriority: 48).AddAssociatedActions(AID.TrueNorth);

        return def;
    }

    public enum Form { None, OpoOpo, Raptor, Coeurl }

    public int Chakra; // 0-5 (0-10 during Brotherhood)
    public BeastChakraType[] BeastChakra = [];
    public int OpoStacks; // 0-1
    public int RaptorStacks; // 0-1
    public int CoeurlStacks; // 0-2
    public NadiFlags Nadi;

    public Form CurrentForm;
    public Form EffectiveForm;
    public float FormLeft; // 0 if no form, 30 max

    public float BlitzLeft; // 20 max
    public float PerfectBalanceLeft => PerfectBalance.Left;
    public (float Left, int Stacks) PerfectBalance;
    public float FormShiftLeft; // 30 max
    public float BrotherhoodLeft; // 20 max
    public float FireLeft; // 20 max
    public float EarthsReplyLeft; // 30 max, probably doesnt belong in autorotation
    public float FiresReplyLeft; // 20 max
    public float WindsReplyLeft; // 15 max

    public int NumBlitzTargets;
    public int NumAOETargets;
    public int NumLineTargets;

    private Enemy? BestBlitzTarget;
    private Enemy? BestRangedTarget; // fire's reply
    private Enemy? BestLineTarget; // enlightenment, wind's reply

    public bool HaveLunar => Nadi.HasFlag(NadiFlags.Lunar);
    public bool HaveSolar => Nadi.HasFlag(NadiFlags.Solar);
    public bool HaveBothNadi => HaveLunar && HaveSolar;

    protected override float GetCastTime(AID aid) => 0;

    public float EffectiveDowntimeIn => MathF.Max(0, DowntimeIn - GetApplicationDelay(AID.SixSidedStar));

    private (AID action, bool isTargeted) GetCurrentBlitz()
    {
        if (BeastCount != 3)
            return (AID.None, false);

        if (HaveBothNadi)
            return (Unlocked(AID.PhantomRush) ? AID.PhantomRush : AID.TornadoKick, true);

        var bc = BeastChakra;
        if (bc[0] == bc[1] && bc[1] == bc[2])
            return (Unlocked(AID.ElixirBurst) ? AID.ElixirBurst : AID.ElixirField, false);
        if (bc[0] != bc[1] && bc[1] != bc[2] && bc[0] != bc[2])
            return (Unlocked(AID.RisingPhoenix) ? AID.RisingPhoenix : AID.FlintStrike, false);
        return (AID.CelestialRevolution, true);
    }

    public int BeastCount => BeastChakra.Count(x => x != BeastChakraType.None);
    public bool ForcedLunar => BeastCount > 1 && BeastChakra[0] == BeastChakra[1] && !HaveBothNadi;
    public bool ForcedSolar => BeastCount > 1 && BeastChakra[0] != BeastChakra[1] && !HaveBothNadi;

    public bool CanFormShift => Unlocked(AID.FormShift) && PerfectBalanceLeft == 0;

    // rockbreaker is a gain on 3 at 22.1% crit but i aint calculating that
    public int AOEBreakpoint => Unlocked(AID.ShadowOfTheDestroyer) && EffectiveForm == Form.OpoOpo && OpoStacks == 0 ? 3 : 4;
    public bool UseAOE => NumAOETargets >= AOEBreakpoint;

    public int BuffedGCDsLeft => FireLeft > GCD ? (int)MathF.Floor((FireLeft - GCD) / AttackGCDLength) + 1 : 0;
    public int PBGCDsLeft => PerfectBalance.Stacks + (ReadyIn(AID.PerfectBalance) <= GCD ? 3 : 0);

    private (Positional, bool) NextPositional
    {
        get
        {
            if (UseAOE || !Unlocked(AID.SnapPunch))
                return (Positional.Any, false);

            var pos = Unlocked(AID.Demolish) && CoeurlStacks == 0 ? Positional.Rear : Positional.Flank;
            var imm = NextGCD is AID.Demolish or AID.SnapPunch or AID.PouncingCoeurl;

            return (pos, imm);
        }
    }

    private bool HaveTarget;

    public enum GCDPriority
    {
        None = 0,
        Meditate = 1,
        WindRanged = 100,
        FireRanged = 200,
        Basic = 300,
        BasicSaver = 310,
        BasicSpender = 320,
        AOE = 400,
        SSS = 500,
        Blitz = 600,
        FiresReply = 700,
        WindsReply = 800,
        PR = 900,
        MeditateForce = 950,
    }

    // some monk OGCDs will be queued with higher prio than what user presses manually - the rotation is very drift-sensitive and monk has much less time to weave than other classes do
    public enum OGCDPriority
    {
        None = 0,
        TrueNorth = 100,
        TFC = 150,
        Potion = 200,
        RiddleOfWind = 300,
        ManualOGCD = 2001, // included for reference, not used here - actual value is 2001 + Low (2000) = 4001
        RiddleOfFire = 2002,
        Brotherhood = 2003,
        PerfectBalance = 2004
    }

    private float GetApplicationDelay(AID action) => action switch
    {
        AID.SixSidedStar => 0.62f,
        AID.DragonKick => 1.29f,
        AID.ForbiddenChakra => 1.48f,
        AID.Demolish => 1.60f,
        // add more if needed
        _ => 0
    };

    public override string DescribeState() => $"F={BuffedGCDsLeft}, PB={PBGCDsLeft}";

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);
        HaveTarget = primaryTarget != null && Player.InCombat;

        var gauge = World.Client.GetGauge<MonkGauge>();

        Chakra = gauge.Chakra;
        BeastChakra = gauge.BeastChakra;
        BlitzLeft = gauge.BlitzTimeRemaining / 1000f;
        Nadi = gauge.Nadi;

        OpoStacks = gauge.OpoOpoStacks;
        RaptorStacks = gauge.RaptorStacks;
        CoeurlStacks = gauge.CoeurlStacks;

        PerfectBalance = Status(SID.PerfectBalance);
        FormShiftLeft = StatusLeft(SID.FormlessFist);
        FireLeft = StatusLeft(SID.RiddleOfFire);
        WindsReplyLeft = StatusLeft(SID.WindsRumination);
        FiresReplyLeft = StatusLeft(SID.FiresRumination);
        EarthsReplyLeft = StatusLeft(SID.EarthsRumination);
        BrotherhoodLeft = StatusLeft(SID.Brotherhood);
        (var currentBlitz, var currentBlitzIsTargeted) = GetCurrentBlitz();

        NumAOETargets = NumMeleeAOETargets(strategy);

        if (BlitzLeft > GCD)
        {
            if (currentBlitzIsTargeted)
                (BestBlitzTarget, NumBlitzTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);
            else
            {
                BestBlitzTarget = null;
                NumBlitzTargets = NumAOETargets;
            }
        }
        else
        {
            BestBlitzTarget = null;
            NumBlitzTargets = 0;
        }

        (CurrentForm, FormLeft) = DetermineForm();

        BestRangedTarget = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget).Best;
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, IsEnlightenmentTarget);

        EffectiveForm = GetEffectiveForm(strategy);

        Meditate(strategy, primaryTarget);
        FormShift(strategy, primaryTarget);

        var sprint = StatusLeft(BossMod.SGE.SID.Sprint);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining is > 3 and < 15 && FormShiftLeft == 0)
                PushGCD(AID.FormShift, Player);

            SmartEngage(strategy, primaryTarget);
            return;
        }

        UseBlitz(strategy, currentBlitz);
        FiresReply(strategy);
        WindsReply(strategy);

        if (UseAOE)
        {
            var aoeAction = EffectiveForm switch
            {
                Form.Coeurl => AID.Rockbreaker,
                Form.Raptor => AID.FourPointFury,
                _ => AID.ArmOfTheDestroyer
            };
            PushGCD(aoeAction, Player, GCDPriority.AOE);
        }

        GCDPriority prioBuffed(int balls) => balls > 0 && primaryTarget?.Priority >= 0 ? GCDPriority.BasicSpender : GCDPriority.Basic;

        switch (EffectiveForm)
        {
            case Form.Coeurl:
                PushGCD(AID.Demolish, primaryTarget, GCDPriority.BasicSaver);
                PushGCD(AID.SnapPunch, primaryTarget, prioBuffed(CoeurlStacks), useOnDyingTarget: false);
                break;
            case Form.Raptor:
                PushGCD(AID.TwinSnakes, primaryTarget, GCDPriority.BasicSaver);
                PushGCD(AID.TrueStrike, primaryTarget, prioBuffed(RaptorStacks), useOnDyingTarget: false);
                break;
            case Form.OpoOpo:
                PushGCD(AID.DragonKick, primaryTarget, GCDPriority.BasicSaver);
                PushGCD(AID.Bootshine, primaryTarget, prioBuffed(OpoStacks), useOnDyingTarget: false);
                break;
            default:
                PushGCD(AID.DragonKick, primaryTarget, GCDPriority.BasicSaver);
                PushGCD(AID.Bootshine, primaryTarget, FormShiftLeft > GCD ? prioBuffed(OpoStacks) : GCDPriority.Basic, useOnDyingTarget: false);
                break;
        }

        switch (strategy.Simple(Track.SSS))
        {
            case OffensiveStrategy.Force:
                PushGCD(AID.SixSidedStar, primaryTarget, GCDPriority.SSS);
                break;
            case OffensiveStrategy.Automatic:
                if (EffectiveDowntimeIn > 0 && !CanFitGCD(EffectiveDowntimeIn, 1))
                    PushGCD(AID.SixSidedStar, primaryTarget, GCDPriority.SSS, useOnDyingTarget: false);
                break;
        }

        Prep(strategy);

        var pos = NextPositional;

        UpdatePositionals(primaryTarget, ref pos);

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.ArmOfTheDestroyer, AOEBreakpoint, maximumActionRange: 20);

        if (Player.InCombat)
            OGCD(strategy, primaryTarget);
    }

    private void Prep(StrategyValues strategy)
    {
        bool lunar;
        switch (strategy.Option(Track.PB).As<PBStrategy>())
        {
            case PBStrategy.DowntimeSolar:
                lunar = false;
                break;
            case PBStrategy.DowntimeLunar:
                lunar = true;
                break;
            default:
                return;
        }

        if (PerfectBalanceLeft == 0)
            return;

        var deadlineAll = MathF.Min(UptimeIn ?? float.MaxValue, PerfectBalanceLeft) - 0.5f;
        var gcdsLeft = 3 - BeastCount;
        var deadlineNext = deadlineAll - gcdsLeft * AttackGCDLength;
        if (lunar)
            PushGCD(AID.ArmOfTheDestroyer, Player, GCDPriority.Basic, deadlineNext);
        else
            PushGCD(BeastCount switch
            {
                0 => AID.Rockbreaker,
                1 => AID.FourPointFury,
                _ => AID.ArmOfTheDestroyer
            }, Player, GCDPriority.Basic, deadlineNext);
    }

    private Form GetEffectiveForm(StrategyValues strategy)
    {
        if (PerfectBalanceLeft == 0)
        {
            if (Unlocked(AID.SnapPunch))
                return CurrentForm;

            if (Unlocked(AID.TrueStrike))
                return CurrentForm == Form.Raptor ? Form.Raptor : Form.OpoOpo;

            return Form.OpoOpo;
        }

        var nadi = strategy.Option(Track.Nadi).As<NadiStrategy>();

        if (ForcedLunar || nadi == NadiStrategy.Lunar)
            return Form.OpoOpo;

        // TODO throw away all this crap and fix odd lunar PB (it should not be used before rof)

        // force lunar PB iff we are in opener, have lunar nadi already, and this is our last PB charge, aka double lunar opener
        // if we have lunar but this is NOT our last charge, it means we came out of downtime with lunar nadi (i.e. dungeon), so solar -> pr is optimal
        // this condition is unfortunately a little contrived. there are no other general cases in the monk rotation where we want to overwrite a lunar, as it's overall a dps loss
        // NextChargeIn(PerfectBalance) > GCD is also not quite correct. ideally this would test whether a PB charge will come up during the riddle of fire window
        // but in fights with extended downtime, nadis will already be explicitly planned out, so this isn't super important
        var forcedDoubleLunar = CombatTimer < 30 && HaveLunar && ReadyIn(AID.PerfectBalance) > GCD && CanFitGCD(FireLeft, 3);
        var forcedSolar = nadi == NadiStrategy.Solar
            || ForcedSolar
            || HaveLunar && !HaveSolar && !forcedDoubleLunar;

        var canCoeurl = forcedSolar;
        var canRaptor = forcedSolar;
        var canOpo = true;

        foreach (var chak in BeastChakra)
        {
            canCoeurl &= chak != BeastChakraType.Coeurl;
            canRaptor &= chak != BeastChakraType.Raptor;
            if (ForcedSolar)
                canOpo &= chak != BeastChakraType.OpoOpo;
        }

        // nice conditional
        return canOpo && OpoStacks == 0
            ? Form.OpoOpo
            : canRaptor && RaptorStacks == 0
                ? Form.Raptor
                : canCoeurl
                    ? Form.Coeurl
                    : canRaptor
                        ? Form.Raptor
                        : Form.OpoOpo;
    }

    private void QueuePB(StrategyValues strategy, Enemy? primaryTarget)
    {
        var pbstrat = strategy.Option(Track.PB).As<PBStrategy>();

        if (BeastChakra[0] != BeastChakraType.None || NextGCD == AID.FiresReply || pbstrat == PBStrategy.Delay || PerfectBalanceLeft > 0)
            return;

        if (pbstrat == PBStrategy.Force || pbstrat is PBStrategy.DowntimeSolar or PBStrategy.DowntimeLunar && primaryTarget is null)
        {
            PushOGCD(AID.PerfectBalance, Player, OGCDPriority.PerfectBalance);
            return;
        }

        if (CurrentForm != Form.Raptor)
            return;

        if (pbstrat == PBStrategy.ForceOpo || !Unlocked(AID.RiddleOfFire))
        {
            PushOGCD(AID.PerfectBalance, Player, OGCDPriority.PerfectBalance);
            return;
        }

        // prevent odd window double blitz
        // TODO figure out the actual mathematical equation that differentiates odd windows, this is stupid
        if (BrotherhoodLeft == 0 && MaxChargesIn(AID.PerfectBalance) > 30)
            return;

        if (ShouldRoF(strategy, 3).Use || CanFitGCD(FireLeft, 3))
        {
            // in case of drift or whatever, if we end up wanting to triple weave after opo, delay PB in favor of using FR to get formless
            // check if BH cooldown is >118s. if we only checked CanWeave for both then autorotation would do BH -> PB because RoF is slightly delayed to get the optimal late weave
            var bhImminentOrUsed = CanWeave(AID.Brotherhood) || ReadyIn(AID.Brotherhood) + AttackGCDLength > 120;

            if (CombatTimer > 10 && bhImminentOrUsed && CanWeave(AID.RiddleOfFire) && Unlocked(AID.FiresReply))
                return;

            PushOGCD(AID.PerfectBalance, Player, OGCDPriority.PerfectBalance);
        }
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        var potionTrack = strategy.Option(Track.Potion);
        var potionPrio = potionTrack.Priority(ActionQueue.Priority.Low + 100 + (float)OGCDPriority.Potion);
        switch (potionTrack.As<PotionStrategy>())
        {
            case PotionStrategy.Now:
                Potion(potionPrio);
                break;
            case PotionStrategy.PreBuffs:
                if (HaveTarget && CanWeave(AID.Brotherhood, 4))
                    Potion(potionPrio);
                break;
        }

        Brotherhood(strategy, primaryTarget);
        QueuePB(strategy, primaryTarget);

        var (useRof, rofLate) = ShouldRoF(strategy);

        if (useRof)
            PushOGCD(AID.RiddleOfFire, Player, OGCDPriority.RiddleOfFire, rofLate ? GCD - EarliestRoF(AnimationLockDelay) : 0);

        if (strategy.Option(Track.RoF).As<RoFStrategy>() == RoFStrategy.Force && !HaveTarget)
            PushOGCD(AID.RiddleOfFire, Player, OGCDPriority.RiddleOfFire);

        if (ShouldRoW(strategy))
            PushOGCD(AID.RiddleOfWind, Player, OGCDPriority.RiddleOfWind);

        UseTN(strategy, primaryTarget, useRof);

        if (HaveTarget && Chakra >= 5 && !useRof)
        {
            if (NumLineTargets >= 3)
                PushOGCD(AID.HowlingFist, BestLineTarget, OGCDPriority.TFC);

            PushOGCD(AID.SteelPeak, primaryTarget, OGCDPriority.TFC, useOnDyingTarget: false);
        }

        var tc = strategy.Option(Track.TC);
        if (tc.As<TCStrategy>() == TCStrategy.GapClose)
        {
            var tcTarget = ResolveTargetOverride(tc.Value) ?? primaryTarget?.Actor;
            if (Player.DistanceToHitbox(tcTarget) is > 3 and < 25)
                PushOGCD(AID.Thunderclap, tcTarget, OGCDPriority.TrueNorth);
        }
    }

    private void Brotherhood(StrategyValues strategy, Enemy? primaryTarget)
    {
        switch (strategy.Simple(Track.BH))
        {
            case OffensiveStrategy.Automatic:
                if (HaveTarget && (CombatTimer > 10 || BeastCount >= 2) && DowntimeIn > AnimLock + 20 && GCD > 0)
                    PushOGCD(AID.Brotherhood, Player, OGCDPriority.Brotherhood);
                break;
            case OffensiveStrategy.Force:
                PushOGCD(AID.Brotherhood, Player, OGCDPriority.Brotherhood);
                break;
            default:
                return;
        }
    }

    private void Meditate(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (Chakra >= 5 || !Unlocked(AID.SteeledMeditation))
            return;

        var prio = GCDPriority.None;

        switch (strategy.Option(Track.Meditation).As<MeditationStrategy>())
        {
            case MeditationStrategy.Force:
                prio = GCDPriority.MeditateForce;
                break;
            case MeditationStrategy.Safe:
                if (!Player.InCombat)
                    prio = GCDPriority.Meditate;

                if (UptimeIn > GCD + 1 || (UptimeIn ?? 0) == 0 && primaryTarget == null)
                    prio = GCDPriority.Meditate;
                break;
            case MeditationStrategy.Greedy:
                if (Player.DistanceToHitbox(primaryTarget) > 3)
                    prio = GCDPriority.Meditate;
                break;
        }

        PushGCD(AID.SteeledMeditation, Player, prio);
    }

    private void FormShift(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (!Unlocked(AID.FormShift) || PerfectBalanceLeft > 0)
            return;

        var prio = GCDPriority.None;

        switch (strategy.Simple(Track.FormShift))
        {
            case OffensiveStrategy.Force:
                prio = GCDPriority.MeditateForce;
                break;
            case OffensiveStrategy.Automatic:
                if (UptimeIn > MathF.Max(GCD + AttackGCDLength, FormShiftLeft) && UptimeIn < 25)
                    prio = GCDPriority.Meditate;
                break;
        }

        PushGCD(AID.FormShift, Player, prio);
    }

    private void UseBlitz(StrategyValues strategy, AID currentBlitz)
    {
        var should = NumBlitzTargets > 0;
        should &= strategy.Option(Track.Blitz).As<BlitzStrategy>() switch
        {
            BlitzStrategy.Automatic => true,
            BlitzStrategy.RoF => FireLeft > GCD,
            BlitzStrategy.Multi => NumBlitzTargets > 1,
            BlitzStrategy.MultiRoF => FireLeft > GCD && NumBlitzTargets > 1,
            _ => false
        };

        if (should)
            PushGCD(currentBlitz, BestBlitzTarget, currentBlitz is AID.TornadoKick or AID.PhantomRush ? GCDPriority.PR : GCDPriority.Blitz);
    }

    private void FiresReply(StrategyValues strategy)
    {
        if (FiresReplyLeft <= GCD)
            return;

        var prio = strategy.Option(Track.FiresReply).As<FRStrategy>() switch
        {
            FRStrategy.Automatic => CurrentForm == Form.Raptor ? GCDPriority.FiresReply : GCDPriority.None,
            FRStrategy.Ranged => CanFitGCD(FiresReplyLeft, 1) ? GCDPriority.FireRanged : GCDPriority.FiresReply,
            FRStrategy.Force => GCDPriority.FiresReply,
            _ => GCDPriority.None
        };

        if (!CanFitGCD(FiresReplyLeft, 1))
            prio = GCDPriority.FiresReply;

        PushGCD(AID.FiresReply, BestRangedTarget, prio);
    }

    private void WindsReply(StrategyValues strategy)
    {
        if (WindsReplyLeft <= GCD)
            return;

        // always queue with low prio, this lets us fallback to winds reply when out of range for melee GCDs
        var prio = GCDPriority.WindRanged;

        // if riddle of fire is about to expire, or if the WR buff itself is about to expire, use ASAP
        if (FireLeft > GCD && !CanFitGCD(FireLeft, 1) || !CanFitGCD(WindsReplyLeft, 1))
            prio = GCDPriority.WindsReply;

        switch (strategy.Option(Track.WindsReply).As<WRStrategy>())
        {
            case WRStrategy.Force:
                prio = GCDPriority.WindsReply;
                break;
            case WRStrategy.PreDowntime:
                if (EffectiveDowntimeIn < WindsReplyLeft && !CanFitGCD(EffectiveDowntimeIn, 2))
                    prio = GCDPriority.WindsReply;
                break;
        }

        PushGCD(AID.WindsReply, BestLineTarget, prio);
    }

    private float DesiredFireWindow => GCDLength * 10;
    private float EarliestRoF(float estimatedDelay) => MathF.Max(estimatedDelay + 0.8f, 20.6f - DesiredFireWindow);

    private void Potion(float priority) => Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, priority);

    private (bool Use, bool LateWeave) ShouldRoF(StrategyValues strategy, int extraGCDs = 0)
    {
        if (!CanWeave(AID.RiddleOfFire, extraGCDs))
            return (false, false);

        return strategy.Option(Track.RoF).As<RoFStrategy>() switch
        {
            RoFStrategy.Automatic => (HaveTarget && (extraGCDs > 0 || !CanWeave(AID.Brotherhood)) && DowntimeIn > AnimLock + 20, true),
            RoFStrategy.Force => (true, false),
            RoFStrategy.ForceMidWeave => (true, true),
            _ => (false, false)
        };
    }

    private bool ShouldRoW(StrategyValues strategy) => strategy.Simple(Track.RoW) switch
    {
        OffensiveStrategy.Automatic => HaveTarget && !CanWeave(AID.RiddleOfFire) && DowntimeIn > AnimLock + 15,
        OffensiveStrategy.Force => true,
        _ => false
    };

    private void UseTN(StrategyValues strategy, Enemy? primaryTarget, bool rofPlanned)
    {
        switch (strategy.Simple(Track.TN))
        {
            case OffensiveStrategy.Automatic:
                if (NextPositionalImminent && !NextPositionalCorrect && Player.DistanceToHitbox(primaryTarget) < 6)
                    PushOGCD(AID.TrueNorth, Player, OGCDPriority.TrueNorth, rofPlanned ? 0 : GCD - 0.8f);
                break;
            case OffensiveStrategy.Force:
                if (TrueNorthLeft == 0)
                    PushOGCD(AID.TrueNorth, Player, OGCDPriority.TrueNorth);
                break;
        }
    }

    private bool IsEnlightenmentTarget(Actor primary, Actor other) => Hints.TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2);

    private (Form, float) DetermineForm()
    {
        if (PerfectBalanceLeft > 0)
            return (Form.None, 0);

        var s = StatusLeft(SID.OpoOpoForm);
        if (s > 0)
            return (Form.OpoOpo, s);
        s = StatusLeft(SID.RaptorForm);
        if (s > 0)
            return (Form.Raptor, s);
        s = StatusLeft(SID.CoeurlForm);

        return s > 0 ? (Form.Coeurl, s) : (Form.None, 0);
    }

    private void SmartEngage(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null)
            return;
        var facepullAction = AID.None;

        // invariant: countdown is > 0
        switch (strategy.Option(Track.Engage).As<EngageStrategy>())
        {
            case EngageStrategy.TC:
                if (CountdownRemaining < 0.7f && Player.DistanceToHitbox(primaryTarget) > 3)
                    PushGCD(AID.Thunderclap, primaryTarget);

                if (CountdownRemaining < GetApplicationDelay(AID.DragonKick))
                    PushGCD(AID.DragonKick, primaryTarget);
                return;

            case EngageStrategy.Sprint:
                if (CountdownRemaining < 10)
                    PushGCD(AID.Sprint, Player);

                var distToMelee = Player.DistanceToHitbox(primaryTarget) - 3;
                var secToMelee = distToMelee / 7.8f;
                // TODO account for acceleration
                if (CountdownRemaining < secToMelee + 0.5f)
                {
                    Hints.ForcedMovement = Player.DirectionTo(primaryTarget.Actor).ToVec3();
                    PushGCD(AID.DragonKick, primaryTarget);
                }

                return;

            case EngageStrategy.FacepullDK:
                facepullAction = AID.DragonKick;
                break;
            case EngageStrategy.FacepullDemo:
                facepullAction = AID.Demolish;
                break;
        }

        if (facepullAction == default)
            return;

        if (Player.DistanceToHitbox(primaryTarget) > 3)
            Hints.ForcedMovement = Player.DirectionTo(primaryTarget.Actor).ToVec3();

        if (CountdownRemaining < GetApplicationDelay(facepullAction))
            PushGCD(facepullAction, primaryTarget);
    }
}
