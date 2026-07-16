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

        [Track(InternalName = "BH", MinLevel = 70, UiPriority = 99, Action = AID.Brotherhood, OGCDPriority = OGCDPriority.Brotherhood)]
        public Track<BHStrategy> Brotherhood;

        // RoF
        [Track("Riddle of Fire", MinLevel = 68, UiPriority = 96, Action = AID.RiddleOfFire, OGCDPriority = OGCDPriority.RiddleOfFire)]
        public Track<RoFStrategy> RoF;
        [Track("Fire's Reply", MinLevel = 100, UiPriority = 95, Action = AID.FiresReply)]
        public Track<FRStrategy> FiresReply;

        // RoW
        [Track("Riddle of Wind", UiPriority = 94, MinLevel = 72, Action = AID.RiddleOfWind, OGCDPriority = OGCDPriority.RiddleOfWind)]
        public Track<OffensiveStrategy> RoW;
        [Track("Wind's Reply", UiPriority = 93, MinLevel = 96, Action = AID.WindsReply)]
        public Track<WRStrategy> WindsReply;

        // blitz/nadi stuff
        [Track("Perfect Balance", UiPriority = 89, Action = AID.PerfectBalance, OGCDPriority = OGCDPriority.PerfectBalance)]
        public Track<PBStrategy> PB;
        [Track(UiPriority = 88, MinLevel = 60)]
        public Track<NadiStrategy> Nadi;
        [Track(UiPriority = 87, MinLevel = 60, Actions = [AID.ElixirField, AID.FlintStrike, AID.TornadoKick, AID.ElixirBurst, AID.RisingPhoenix, AID.PhantomRush], Targets = ActionTargets.Hostile)]
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
        [Track("Potion", UiPriority = 59, Item = 1045995, OGCDPriority = OGCDPriority.Potion)]
        public Track<PotionStrategy> Pot;
        [Track(UiPriority = 49)]
        public Track<EngageStrategy> Engage;
        [Track("True North", MinLevel = 50, UiPriority = 48, Action = AID.TrueNorth, OGCDPriority = OGCDPriority.TrueNorth)]
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

    public enum BHStrategy
    {
        [Option("Use in opener, or on cooldown (unless downtime would interrupt it)")]
        Automatic,
        [Option("Don't use")]
        Delay,
        [Option("Use ASAP")]
        Force,
        [Option("Use ASAP, as long as a GCD has been used first (for early opener)")]
        ForceGCD
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
        [Option("Use as soon as multiple targets can be hit", Targets = ActionTargets.Hostile)]
        Multi,
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
        [Option("Use 3 GCDs before next RoF window, regardless of current form", MinLevel = 50)]
        ForceMinus3,
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
        [Option("Use when outside melee range", Targets = ActionTargets.Hostile, OGCDPriority = OGCDPriority.TrueNorth)]
        GapClose,
        [Option("Use to cancel knockback", Targets = ActionTargets.Party | ActionTargets.Hostile, OGCDPriority = OGCDPriority.PerfectBalance)]
        Knockback
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
    public int NumWindTargets;
    public int NumEnlightenmentTargets;

    private DateTime LastDash;

    private Enemy? BlitzTarget;
    private Enemy? FiresReplyTarget; // fire's reply
    private Enemy? WindTarget; // wind's reply
    private Enemy? EnlightenmentTarget;

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

    public override string DescribeState() => $"F={BuffedGCDsLeft}, PB={PBGCDsLeft}";

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
    {
        if (Manager.LastCast is (var ts, { } data) && data.IsSpell(AID.Thunderclap))
            LastDash = ts;

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
        var (currentBlitz, currentBlitzIsTargeted) = GetCurrentBlitz();

        NumAOETargets = NumMeleeAOETargets(strategy);

        if (BlitzLeft > GCD)
        {
            if (currentBlitzIsTargeted)
            {
                (BlitzTarget, NumBlitzTargets) = OrSelectTarget(strategy, primaryTarget, strategy.Blitz, 3, IsSplashTarget);
            }
            else
            {
                BlitzTarget = null;
                NumBlitzTargets = NumAOETargets;
            }
        }
        else
        {
            BlitzTarget = null;
            NumBlitzTargets = 0;
        }

        (CurrentForm, FormLeft) = DetermineForm();

        FiresReplyTarget = OrSelectTarget(strategy, primaryTarget, strategy.FiresReply, 20, IsSplashTarget).Best;
        (WindTarget, NumWindTargets) = OrSelectTarget(strategy, primaryTarget, strategy.WindsReply, 10, IsEnlightenmentTarget);
        (EnlightenmentTarget, NumEnlightenmentTargets) = SelectTarget(strategy, primaryTarget, 10, IsEnlightenmentTarget);

        EffectiveForm = GetEffectiveForm(strategy);

        Meditate(strategy, primaryTarget);
        FormShift(strategy, primaryTarget);

        var sprint = StatusLeft(ClassShared.SID.Sprint);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining is > 3 and < 15 && FormShiftLeft == 0)
                PushGCD(AID.FormShift, Player);

            SmartEngage(strategy, primaryTarget);

            // prepull row/bh/etc
            OGCD(strategy, primaryTarget);

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
                _ => Unlocked(AID.ShadowOfTheDestroyer) ? AID.ShadowOfTheDestroyer : AID.ArmOfTheDestroyer
            };
            PushGCD(aoeAction, Player, GCDPriority.AOE);
        }

        GCDPriority prioBuffed(int balls) => balls > 0 && primaryTarget?.Priority >= 0 ? GCDPriority.BasicSpender : GCDPriority.Basic;

        switch (EffectiveForm)
        {
            case Form.Coeurl:
                PushGCD(AID.Demolish, primaryTarget, GCDPriority.BasicSaver);
                PushGCD(Unlocked(AID.PouncingCoeurl) ? AID.PouncingCoeurl : AID.SnapPunch, primaryTarget, prioBuffed(CoeurlStacks), useOnDyingTarget: false);
                break;
            case Form.Raptor:
                PushGCD(AID.TwinSnakes, primaryTarget, GCDPriority.BasicSaver);
                PushGCD(Unlocked(AID.RisingRaptor) ? AID.RisingRaptor : AID.TrueStrike, primaryTarget, prioBuffed(RaptorStacks), useOnDyingTarget: false);
                break;
            case Form.OpoOpo:
                PushGCD(AID.DragonKick, primaryTarget, GCDPriority.BasicSaver);
                PushGCD(Unlocked(AID.LeapingOpo) ? AID.LeapingOpo : AID.Bootshine, primaryTarget, prioBuffed(OpoStacks), useOnDyingTarget: false);
                break;
            default:
                PushGCD(AID.DragonKick, primaryTarget, GCDPriority.BasicSaver);
                PushGCD(Unlocked(AID.LeapingOpo) ? AID.LeapingOpo : AID.Bootshine, primaryTarget, FormShiftLeft > GCD ? prioBuffed(OpoStacks) : GCDPriority.Basic, useOnDyingTarget: false);
                break;
        }

        switch (strategy.SSS.Value)
        {
            case OffensiveStrategy.Force:
                PushGCD(AID.SixSidedStar, primaryTarget, GCDPriority.SSS);
                break;
            case OffensiveStrategy.Automatic:
                var shouldUse = EffectiveDowntimeIn > 0 && !CanFitGCD(EffectiveDowntimeIn, 1);

                if (Hints.ImminentSpecialMode.mode == SpecialMode.Pyretic)
                {
                    var pyreticDeadline = (float)Math.Max(0, (Hints.ImminentSpecialMode.activation - World.CurrentTime).TotalSeconds);
                    shouldUse |= pyreticDeadline > 0 && !CanFitGCD(pyreticDeadline, 1);
                }

                if (shouldUse)
                    PushGCD(AID.SixSidedStar, primaryTarget, GCDPriority.SSS, useOnDyingTarget: false);
                break;
        }

        Prep(strategy);

        var pos = NextPositional;

        UpdatePositionals(primaryTarget, ref pos);

        GoalZoneCombined(strategy, 3, Hints.GoalAOECircle(5), AID.ArmOfTheDestroyer, AOEBreakpoint, maximumActionRange: 20);

        OGCD(strategy, primaryTarget);
    }

    // TODO: just a convenience method, this should be in basexan
    (Enemy? Best, int Targets) OrSelectTarget<T>(in Strategy strategy, Enemy? primaryTarget, in Track<T> strategyTrack, float range, PositionCheck isInAOE) where T : struct
    {
        return ResolveEnemy(strategyTrack) is { } targetOverride
            ? (targetOverride, Hints.PriorityTargets.Count(p => isInAOE(targetOverride.Actor, p.Actor)))
            : SelectTarget(strategy, primaryTarget, range, isInAOE);
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

        PushGCD(AID.FiresReply, FiresReplyTarget, prio);
    }

    private void WindsReply(in Strategy strategy)
    {
        if (WindsReplyLeft <= GCD)
            return;

        var expiring = !CanFitGCD(WindsReplyLeft, 1) || EffectiveDowntimeIn < WindsReplyLeft && !CanFitGCD(EffectiveDowntimeIn, 2);
        var buffsExpiring = FireLeft > GCD && !CanFitGCD(FireLeft, 1);

        switch (strategy.WindsReply.Value)
        {
            case WRStrategy.Automatic:
                PushGCD(AID.WindsReply, WindTarget, expiring || buffsExpiring ? GCDPriority.WindsReply : GCDPriority.WindRanged);
                break;
            case WRStrategy.Force:
                PushGCD(AID.WindsReply, WindTarget, GCDPriority.WindsReply);
                break;
            case WRStrategy.Multi:
                if (NumWindTargets > 1 || expiring)
                    PushGCD(AID.WindsReply, WindTarget, GCDPriority.WindsReply);
                break;
        }
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

                // real downtime according to state machine, we have time to meditate
                if (UptimeIn > GCD + 1)
                    prio = GCDPriority.Meditate;

                // regular combat and no target, meditate is ok
                if (UptimeIn == null && primaryTarget == null)
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
            PushGCD(currentBlitz, BlitzTarget, currentBlitz is AID.TornadoKick or AID.PhantomRush ? GCDPriority.PR : GCDPriority.Blitz);
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

    private void OGCD(in Strategy strategy, Enemy? primaryTarget)
    {
        var potionPrio = strategy.Pot.Priority();
        if (strategy.Pot.Value switch
        {
            PotionStrategy.Now => true,
            PotionStrategy.PreBuffs => HaveTarget && CanWeave(AID.Brotherhood, 4),
            _ => false
        })
            UsePlanned(strategy.Pot, (AID)ActionDefinitions.IDPotionStr.ID, Player);

        UseBrotherhood(strategy);
        UsePB(strategy, primaryTarget);
        UseRoF(strategy);
        UseRoW(strategy);
        UseTN(strategy, primaryTarget);

        if (HaveTarget && Chakra >= 5 && Player.InCombat)
        {
            if (NumEnlightenmentTargets >= 3)
                PushOGCD(AID.HowlingFist, EnlightenmentTarget, OGCDPriority.TFC);

            PushOGCD(AID.SteelPeak, primaryTarget, OGCDPriority.TFC, useOnDyingTarget: false);
        }

        // TODO: all of this needs to be moved to ClassMNKUtility
        var tc = strategy.TC;
        switch (tc.Value)
        {
            case TCStrategy.GapClose:
                UsePlanned(tc, AID.Thunderclap, primaryTarget?.Actor, forced: true, predicate: (t) => Player.DistanceToHitbox(t) > 3);
                break;

            // we can't consistently use effectresult to time the dash since action requests are affected by RTT. maybe it would work for someone with better ping but not me
            case TCStrategy.Knockback:
                // FIXME: should instead check whether the last dash was before the start of this plan entry but that will be more work
                if (LastDash.AddSeconds(2) < World.CurrentTime && Player.PendingKnockbacks.Count > 0)
                    UsePlanned(tc, AID.Thunderclap, primaryTarget?.Actor ?? World.Party.WithoutSlot(includeDead: false).Exclude(Player).Closest(Player.Position), additionalPriority: 1900, forced: true);
                break;
        }
    }

    private void UseBrotherhood(in Strategy strategy)
    {
        if (strategy.Brotherhood.Value switch
        {
            BHStrategy.Automatic => HaveTarget && (CombatTimer > 10 || BeastCount >= 2) && DowntimeIn > AnimLock + 20 && GCD > 0,
            BHStrategy.Force => true,
            BHStrategy.ForceGCD => GCD > 0,
            _ => false
        })
            UsePlanned(strategy.Brotherhood, AID.Brotherhood, Player);
    }

    private void UsePB(in Strategy strategy, Enemy? primaryTarget)
    {
        var track = strategy.PB;
        var pbstrat = track.Value;

        void use() => UsePlanned(track, AID.PerfectBalance, Player);

        // hard requirements missing, or pb delayed by plan
        if (BeastChakra[0] != BeastChakraType.None || PerfectBalanceLeft > 0 || !Player.InCombat || pbstrat == PBStrategy.Delay)
            return;

        // forced usage
        if (pbstrat == PBStrategy.Force || pbstrat is PBStrategy.DowntimeSolar or PBStrategy.DowntimeLunar && primaryTarget == null)
        {
            use();
            return;
        }
        if (pbstrat == PBStrategy.ForceMinus3 && CanWeave(AID.RiddleOfFire, 3))
        {
            use();
            return;
        }

        // basic autorot logic to optimize number of opo gcds
        if (NextGCD == AID.FiresReply || CurrentForm != Form.Raptor)
            return;

        if (pbstrat == PBStrategy.ForceOpo || !Unlocked(AID.RiddleOfFire))
        {
            use();
            return;
        }

        // prevent odd window double blitz
        // TODO figure out the actual mathematical equation that differentiates odd windows, this is stupid
        if (BrotherhoodLeft == 0 && MaxChargesIn(AID.PerfectBalance) > 30)
            return;

        var shouldUse = CanFitGCD(FireLeft, 3) || strategy.RoF.Value switch
        {
            RoFStrategy.Automatic => HaveTarget && CanWeave(AID.RiddleOfFire, 3) && DowntimeIn > ReadyIn(AID.RiddleOfFire) + 20,
            // automatic strategy should never result in downtime PB regardless of RoF state. player should use the Prep strategies if needed
            RoFStrategy.Force or RoFStrategy.ForceMidWeave => DowntimeIn > 0 && CanWeave(AID.RiddleOfFire, 3),
            _ => false
        };

        if (shouldUse)
        {
            // in case of drift or whatever, if we end up wanting to triple weave after opo, delay PB in favor of using FR to get formless
            // check if BH cooldown is >118s. if we only checked CanWeave for both then autorotation would do BH -> PB because RoF is slightly delayed to get the optimal late weave
            var bhImminentOrUsed = CanWeave(AID.Brotherhood) || ReadyIn(AID.Brotherhood) + GCDLength > 120;

            if (CombatTimer > 10 && bhImminentOrUsed && CanWeave(AID.RiddleOfFire) && Unlocked(AID.FiresReply))
                return;

            use();
        }
    }

    private void UseRoF(in Strategy strategy)
    {
        var earliestRof = MathF.Max(AnimationLockDelay + 0.8f, 20.6f - GCDLength * 10);

        switch (strategy.RoF.Value)
        {
            case RoFStrategy.Automatic:
                // standard opener, use once bh is pressed, hold if downtime is imminent
                if (HaveTarget && !CanWeave(AID.Brotherhood) && DowntimeIn > AnimLock + 20)
                    UsePlanned(strategy.RoF, AID.RiddleOfFire, Player, GCD - earliestRof);
                break;
            case RoFStrategy.Force:
                // downtime rof etc, gcd is irrelevant
                UsePlanned(strategy.RoF, AID.RiddleOfFire, Player);
                break;
            case RoFStrategy.ForceMidWeave:
                // mid weave assumes our GCD is rolling which implies a target
                if (HaveTarget)
                    UsePlanned(strategy.RoF, AID.RiddleOfFire, Player, GCD - earliestRof);
                break;
        }
    }

    private void UseRoW(in Strategy strategy)
    {
        if (strategy.RoW.Value switch
        {
            OffensiveStrategy.Automatic => HaveTarget && !CanWeave(AID.RiddleOfFire) && DowntimeIn > AnimLock + 15,
            OffensiveStrategy.Force => true,
            _ => false
        })
            UsePlanned(strategy.RoW, AID.RiddleOfWind, Player);
    }

    private void UseTN(in Strategy strategy, Enemy? primaryTarget)
    {
        switch (strategy.TrueNorth.Value)
        {
            case OffensiveStrategy.Automatic:
                if (NextPositionalImminent && !NextPositionalCorrect && Player.DistanceToHitbox(primaryTarget) < 6 && primaryTarget?.Priority >= 0)
                    UsePlanned(strategy.TrueNorth, AID.TrueNorth, Player, GCD - 0.8f);
                break;
            case OffensiveStrategy.Force:
                if (TrueNorthLeft == 0)
                    UsePlanned(strategy.TrueNorth, AID.TrueNorth, Player);
                break;
        }
    }

    private bool IsEnlightenmentTarget(Actor primary, Actor other) => TargetInAOERect(other, Player.Position, Player.DirectionTo(primary), 10, 2);

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
