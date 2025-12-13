using BossMod.MNK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class MNK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID, MNK.Strategy>(manager, player, PotionType.Strength)
{
    public struct Strategy : IStrategyCommon
    {
        [Track(UiPriority = 500)]
        public Track<Targeting> Targeting;
        [Track(UiPriority = 499)]
        public Track<AOEStrategy> AOE;

        [Track(InternalName = "BH", MinLevel = 70, UiPriority = 99, Action = AID.Brotherhood)]
        public Track<OffensiveStrategy> Brotherhood;

        // RoF
        [Track("Riddle of Fire", MinLevel = 68, UiPriority = 96, Action = AID.RiddleOfFire)]
        public Track<RoFStrategy> RoF;
        [Track("Fire's Reply", MinLevel = 100, UiPriority = 95, Action = AID.FiresReply)]
        public Track<FRStrategy> FiresReply;

        // RoW
        [Track("Riddle of Wind", UiPriority = 94, MinLevel = 72, Action = AID.RiddleOfWind)]
        public Track<OffensiveStrategy> RoW;
        [Track("Wind's Reply", UiPriority = 93, MinLevel = 96, Action = AID.WindsReply)]
        public Track<WRStrategy> WindsReply;

        // blitz/nadi stuff
        [Track("Perfect Balance", UiPriority = 89, Action = AID.PerfectBalance)]
        public Track<PBStrategy> PB;
        [Track(UiPriority = 88, MinLevel = 60)]
        public Track<NadiStrategy> Nadi;
        [Track(UiPriority = 87, MinLevel = 60, Actions = [AID.ElixirField, AID.FlintStrike, AID.TornadoKick, AID.ElixirBurst, AID.RisingPhoenix, AID.PhantomRush])]
        public Track<BlitzStrategy> Blitz;

        // downtime
        [Track("Six-Sided Star", InternalName = "SixSidedStar", MinLevel = 80, UiPriority = 79, Action = AID.SixSidedStar)]
        public Track<OffensiveStrategy> SSS;
        [Track("Form Shift", MinLevel = 52, UiPriority = 78, Action = AID.FormShift)]
        public Track<OffensiveStrategy> FormShift;
        [Track("Meditate", UiPriority = 77, Actions = [AID.SteeledMeditation, AID.ForbiddenMeditation, AID.InspiritedMeditation, AID.EnlightenedMeditation])]
        public Track<MeditationStrategy> Meditate;

        // other
        [Track("Thunderclap", MinLevel = 35, UiPriority = 69, Action = AID.Thunderclap)]
        public Track<TCStrategy> TC;
        [Track("Potion", UiPriority = 59, Item = 1045995)]
        public Track<PotionStrategy> Pot;
        [Track(UiPriority = 49)]
        public Track<EngageStrategy> Engage;
        [Track("True North", MinLevel = 50, UiPriority = 48, Action = AID.TrueNorth)]
        public Track<OffensiveStrategy> TrueNorth;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum PotionStrategy
    {
        [Option("Do not automatically use")]
        Manual,
        [Option("Use ~4 GCDs before next buff window")]
        PreBuffs,
        [Option("Use ASAP")]
        Now
    }
    public enum MeditationStrategy
    {
        [Option("Use out of combat or if no enemies are nearby")]
        Safe,
        [Option("Use out of combat, or if no enemies are in melee range")]
        Greedy,
        [Option("Use ASAP")]
        Force,
        [Option("Do not use")]
        Delay
    }
    public enum FRStrategy
    {
        [Option("Use after Opo form GCD", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Hold until a ranged attack is needed", Targets = ActionTargets.Hostile)]
        Ranged,
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Do not use")]
        Delay
    }
    public enum WRStrategy
    {
        [Option("Hold until a ranged attack is needed", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Use ASAP", Targets = ActionTargets.Hostile)]
        Force,
        [Option("Ensure usage at least 2 GCDs before next downtime", Targets = ActionTargets.Hostile)]
        PreDowntime
    }
    public enum NadiStrategy
    {
        [Option("Automatically choose best nadi (cycle L/L/S); use double lunar opener")]
        Automatic,
        [Option(Color = 0xFFDB8BCA)]
        Lunar,
        [Option(Color = 0xFF8EE6FA)]
        Solar
    }
    public enum RoFStrategy
    {
        [Option("Automatically use during burst window")]
        Automatic,
        [Option("Use ASAP (early weave)")]
        Force,
        [Option("Use ASAP (late weave)")]
        ForceMidWeave,
        [Option("Do not use")]
        Delay,
    }
    public enum PBStrategy
    {
        [Option("Standard usage: after Opo form GCD, before or during Riddle of Fire window", MinLevel = 50)]
        Automatic,
        [Option("Use after next Opo GCD", MinLevel = 50)]
        ForceOpo,
        [Option("Use ASAP", MinLevel = 50)]
        Force,
        [Option("Do not use", MinLevel = 50)]
        Delay,
        [Option("Downtime prep: Solar", MinLevel = 60, Effect = 39)]
        DowntimeSolar,
        [Option("Downtime prep: Lunar", MinLevel = 60, Effect = 39)]
        DowntimeLunar
    }
    public enum TCStrategy
    {
        [Option("Do not use")]
        None,
        [Option("Use when outside melee range", Targets = ActionTargets.Party | ActionTargets.Hostile)]
        GapClose
    }
    public enum BlitzStrategy
    {
        [Option("Use ASAP")]
        Automatic,
        [Option("Hold until Riddle of Fire is active")]
        RoF,
        [Option("Hold until multiple targets will be hit")]
        Multi,
        [Option("Hold until RoF and multiple targets")]
        MultiRoF,
        [Option("Do not use")]
        Delay
    }
    public enum EngageStrategy
    {
        [Option("Thunderclap to target")]
        TC,
        [Option("Sprint into melee range")]
        Sprint,
        [Option("Precast Dragon Kick from melee range")]
        FacepullDK,
        [Option("Precast Demolish from melee range")]
        FacepullDemo
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan MNK", "Monk", "Standard rotation (xan)|Melee", "xan", RotationModuleQuality.Good, BitMask.Build(Class.MNK, Class.PGL), 100).WithStrategies<Strategy>();
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

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
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

        switch (strategy.SSS.Value)
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

    private void Prep(in Strategy strategy)
    {
        bool lunar;
        switch (strategy.PB.Value)
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

    private Form GetEffectiveForm(in Strategy strategy)
    {
        if (PerfectBalanceLeft == 0)
        {
            if (Unlocked(AID.SnapPunch))
                return CurrentForm;

            if (Unlocked(AID.TrueStrike))
                return CurrentForm == Form.Raptor ? Form.Raptor : Form.OpoOpo;

            return Form.OpoOpo;
        }

        var nadi = strategy.Nadi.Value;

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

    private void QueuePB(in Strategy strategy, Enemy? primaryTarget)
    {
        var pbstrat = strategy.PB.Value;

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

    private void OGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        var potionPrio = strategy.Pot.Priority(ActionQueue.Priority.Low + 100 + (float)OGCDPriority.Potion);
        switch (strategy.Pot.Value)
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

        if (strategy.RoF.Value == RoFStrategy.Force && !HaveTarget)
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

        var tc = strategy.TC;
        if (tc.Value == TCStrategy.GapClose)
        {
            var tcTarget = ResolveTargetOverride(tc) ?? primaryTarget;
            if (Player.DistanceToHitbox(tcTarget) is > 3 and < 25)
                PushOGCD(AID.Thunderclap, tcTarget, OGCDPriority.TrueNorth);
        }
    }

    private void Brotherhood(in Strategy strategy, Enemy? primaryTarget)
    {
        switch (strategy.Brotherhood.Value)
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

    private void Meditate(in Strategy strategy, Enemy? primaryTarget)
    {
        if (Chakra >= 5 || !Unlocked(AID.SteeledMeditation))
            return;

        var prio = GCDPriority.None;

        switch (strategy.Meditate.Value)
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

    private void FormShift(in Strategy strategy, Enemy? primaryTarget)
    {
        if (!Unlocked(AID.FormShift) || PerfectBalanceLeft > 0)
            return;

        var prio = GCDPriority.None;

        switch (strategy.FormShift.Value)
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

    private void UseBlitz(in Strategy strategy, AID currentBlitz)
    {
        var should = NumBlitzTargets > 0;
        should &= strategy.Blitz.Value switch
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

    private void FiresReply(in Strategy strategy)
    {
        if (FiresReplyLeft <= GCD)
            return;

        var prio = strategy.FiresReply.Value switch
        {
            FRStrategy.Automatic => CurrentForm == Form.Raptor ? GCDPriority.FiresReply : GCDPriority.None,
            FRStrategy.Ranged => CanFitGCD(FiresReplyLeft, 1) ? GCDPriority.FireRanged : GCDPriority.FiresReply,
            FRStrategy.Force => GCDPriority.FiresReply,
            _ => GCDPriority.None
        };

        if (!CanFitGCD(FiresReplyLeft, 1))
            prio = GCDPriority.FiresReply;

        PushGCD(AID.FiresReply, ResolveTargetOverride(strategy.FiresReply) ?? BestRangedTarget, prio);
    }

    private void WindsReply(in Strategy strategy)
    {
        if (WindsReplyLeft <= GCD)
            return;

        // always queue with low prio, this lets us fallback to winds reply when out of range for melee GCDs
        var prio = GCDPriority.WindRanged;

        // if riddle of fire is about to expire, or if the WR buff itself is about to expire, use ASAP
        if (FireLeft > GCD && !CanFitGCD(FireLeft, 1) || !CanFitGCD(WindsReplyLeft, 1))
            prio = GCDPriority.WindsReply;

        switch (strategy.WindsReply.Value)
        {
            case WRStrategy.Force:
                prio = GCDPriority.WindsReply;
                break;
            case WRStrategy.PreDowntime:
                if (EffectiveDowntimeIn < WindsReplyLeft && !CanFitGCD(EffectiveDowntimeIn, 2))
                    prio = GCDPriority.WindsReply;
                break;
        }

        PushGCD(AID.WindsReply, ResolveTargetOverride(strategy.WindsReply) ?? BestLineTarget, prio);
    }

    private float DesiredFireWindow => GCDLength * 10;
    private float EarliestRoF(float estimatedDelay) => MathF.Max(estimatedDelay + 0.8f, 20.6f - DesiredFireWindow);

    private void Potion(float priority) => Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, priority);

    private (bool Use, bool LateWeave) ShouldRoF(Strategy strategy, int extraGCDs = 0)
    {
        if (!CanWeave(AID.RiddleOfFire, extraGCDs))
            return (false, false);

        return strategy.RoF.Value switch
        {
            RoFStrategy.Automatic => (HaveTarget && (extraGCDs > 0 || !CanWeave(AID.Brotherhood)) && DowntimeIn > AnimLock + 20, true),
            RoFStrategy.Force => (true, false),
            RoFStrategy.ForceMidWeave => (true, true),
            _ => (false, false)
        };
    }

    private bool ShouldRoW(in Strategy strategy) => strategy.RoW.Value switch
    {
        OffensiveStrategy.Automatic => HaveTarget && !CanWeave(AID.RiddleOfFire) && DowntimeIn > AnimLock + 15,
        OffensiveStrategy.Force => true,
        _ => false
    };

    private void UseTN(in Strategy strategy, Enemy? primaryTarget, bool rofPlanned)
    {
        switch (strategy.TrueNorth.Value)
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

    private void SmartEngage(in Strategy strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null)
            return;
        var facepullAction = AID.None;

        // invariant: countdown is > 0
        switch (strategy.Engage.Value)
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
