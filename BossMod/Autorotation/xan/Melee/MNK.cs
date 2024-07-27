using BossMod.MNK;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;
public sealed class MNK(RotationModuleManager manager, Actor player) : Attackxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("MNK", "Monk", "xan", RotationModuleQuality.Good, BitMask.Build(Class.MNK, Class.PGL), 100);

        def.DefineShared().AddAssociatedActions(AID.RiddleOfFire, AID.RiddleOfWind, AID.Brotherhood);

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
    public float FormLeft; // 0 if no form, 30 max

    public float BlitzLeft; // 20 max
    public float PerfectBalanceLeft; // 20 max
    public float FormShiftLeft; // 30 max
    public float BrotherhoodLeft; // 20 max
    public float FireLeft; // 20 max
    public float EarthsReplyLeft; // 30 max, probably doesnt belong in autorotation
    public float FiresReplyLeft; // 20 max
    public float WindsReplyLeft; // 15 max

    public int NumBlitzTargets;
    public int NumAOETargets;
    public int NumLineTargets;

    private Actor? BestBlitzTarget;
    private Actor? BestRangedTarget; // fire's reply
    private Actor? BestLineTarget; // enlightenment, wind's reply

    public bool HasLunar => Nadi.HasFlag(NadiFlags.Lunar);
    public bool HasSolar => Nadi.HasFlag(NadiFlags.Solar);
    public bool HasBothNadi => HasLunar && HasSolar;

    protected override float GetCastTime(AID aid) => 0;

    private (AID action, bool isTargeted) GetCurrentBlitz()
    {
        if (BeastCount != 3)
            return (AID.None, false);

        if (HasBothNadi)
            return (Unlocked(AID.PhantomRush) ? AID.PhantomRush : AID.TornadoKick, true);

        var bc = BeastChakra;
        if (bc[0] == bc[1] && bc[1] == bc[2])
            return (Unlocked(AID.ElixirBurst) ? AID.ElixirBurst : AID.ElixirField, false);
        if (bc[0] != bc[1] && bc[1] != bc[2] && bc[0] != bc[2])
            return (Unlocked(AID.RisingPhoenix) ? AID.RisingPhoenix : AID.FlintStrike, false);
        return (AID.CelestialRevolution, true);
    }

    public int BeastCount => BeastChakra.Count(x => x != BeastChakraType.None);
    public bool ForcedLunar => BeastCount > 1 && BeastChakra[0] == BeastChakra[1] && !HasBothNadi;
    public bool ForcedSolar => BeastCount > 1 && BeastChakra[0] != BeastChakra[1] && !HasBothNadi;

    public bool CanFormShift => Unlocked(AID.FormShift) && PerfectBalanceLeft == 0;

    private (Positional, bool) NextPositional => (CoeurlStacks > 0 ? Positional.Flank : Positional.Rear, EffectiveForm == Form.Coeurl);

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 3);

        var gauge = GetGauge<MonkGauge>();

        Chakra = gauge.Chakra;
        BeastChakra = gauge.BeastChakra;
        BlitzLeft = gauge.BlitzTimeRemaining / 1000f;
        Nadi = gauge.Nadi;

        OpoStacks = gauge.OpoOpoStacks;
        RaptorStacks = gauge.RaptorStacks;
        CoeurlStacks = gauge.CoeurlStacks;

        PerfectBalanceLeft = StatusLeft(SID.PerfectBalance);
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
                BestBlitzTarget = Player;
                NumBlitzTargets = NumAOETargets;
            }
        }
        else
        {
            BestBlitzTarget = Player;
            NumBlitzTargets = 0;
        }

        (CurrentForm, FormLeft) = DetermineForm();

        BestRangedTarget = SelectTarget(strategy, primaryTarget, 20, IsSplashTarget).Best;
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 10, IsEnlightenmentTarget);

        UpdatePositionals(primaryTarget, NextPositional, TrueNorthLeft > GCD);

        OGCD(strategy, primaryTarget);

        if (Chakra < 5 && Unlocked(AID.SteeledMeditation))
            PushGCD(AID.SteeledMeditation, Player, -4500);

        if (Unlocked(AID.FormShift) && PerfectBalanceLeft == 0 && FormShiftLeft < 5)
            PushGCD(AID.FormShift, Player, -4500);

        if (World.Client.CountdownRemaining > 0)
        {
            if (World.Client.CountdownRemaining < 0.2 && Player.DistanceToHitbox(primaryTarget) is > 3 and < 25)
                PushGCD(AID.Thunderclap, primaryTarget);

            return;
        }

        if (NumBlitzTargets > 0)
            PushGCD(currentBlitz, BestBlitzTarget);

        // demo opener might be optimal sometimes
        //if (FormShiftLeft > _state.GCD && CoeurlStacks == 0)
        //    PushGCD(AID.Demolish, primaryTarget);

        if (PerfectBalanceLeft == 0 && BlitzLeft == 0)
        {
            if (FormShiftLeft == 0 && FiresReplyLeft > GCD)
                PushGCD(AID.FiresReply, BestRangedTarget);

            if (WindsReplyLeft > GCD)
                PushGCD(AID.WindsReply, BestLineTarget);
        }

        if (NumAOETargets > 2 && Unlocked(AID.ArmOfTheDestroyer))
        {
            if (EffectiveForm == Form.Coeurl)
                PushGCD(AID.Rockbreaker, Player);

            // TODO this is actually still suboptimal on 3 targets
            if (EffectiveForm == Form.Raptor)
                PushGCD(AID.FourPointFury, Player);

            PushGCD(AID.ArmOfTheDestroyer, Player);
        }
        else
        {
            switch (EffectiveForm)
            {
                case Form.Coeurl:
                    PushGCD(CoeurlStacks == 0 && Unlocked(AID.Demolish) ? AID.Demolish : AID.SnapPunch, primaryTarget); break;
                case Form.Raptor:
                    PushGCD(RaptorStacks == 0 && Unlocked(AID.TwinSnakes) ? AID.TwinSnakes : AID.TrueStrike, primaryTarget); break;
                default:
                    PushGCD(OpoStacks == 0 && Unlocked(AID.DragonKick) ? AID.DragonKick : AID.Bootshine, primaryTarget); break;
            }
        }
    }

    private Form EffectiveForm
    {
        get
        {
            if (PerfectBalanceLeft == 0)
                return CurrentForm;

            // hack: allow double lunar opener
            var forcedSolar = ForcedSolar || HasLunar && !HasSolar && CombatTimer > 30;

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

            return canRaptor ? Form.Raptor : canCoeurl ? Form.Coeurl : Form.OpoOpo;
        }
    }

    private void QueuePB(StrategyValues strategy)
    {
        if (CurrentForm != Form.Raptor || BeastChakra[0] != BeastChakraType.None || FiresReplyLeft > GCD)
            return;

        // prevent odd window double blitz
        if (HasBothNadi && FireLeft > 0)
            return;

        // TODO forced solar in strategy
        // default: solar in odd windows only, opener/2m is always lunar
        var wantSolar = HasLunar && !HasSolar && FireLeft == 0;

        // earliest we can press PB before next RoF
        var gcdsAhead = wantSolar ? 1 : 2;

        if (CanWeave(AID.RiddleOfFire, gcdsAhead))
            PushOGCD(AID.PerfectBalance, Player);

        // can PB if we have 4 GCDs worth of buff remaining
        if (CanFitGCD(FireLeft, 3))
            PushOGCD(AID.PerfectBalance, Player);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Player.InCombat || GCD == 0)
            return;

        if (strategy.BuffsOk())
        {
            QueuePB(strategy);

            if (CombatTimer >= 10 || BeastCount == 3)
                PushOGCD(AID.Brotherhood, Player);

            if (ShouldRoF)
                PushOGCD(AID.RiddleOfFire, Player, delay: GCD - 0.8f);

            if (CD(AID.RiddleOfFire) > 0)
                PushOGCD(AID.RiddleOfWind, Player);

            if (NextPositionalImminent && !NextPositionalCorrect)
                PushOGCD(AID.TrueNorth, Player, delay: ShouldRoF ? 0 : GCD - 0.8f);
        }

        if (Chakra >= 5)
        {
            if (NumLineTargets >= 3)
                PushOGCD(AID.HowlingFist, BestLineTarget);

            PushOGCD(AID.SteelPeak, primaryTarget);
        }
    }

    private bool ShouldRoF => CanWeave(AID.RiddleOfFire) && (CD(AID.Brotherhood) > 0 || !Unlocked(AID.Brotherhood));

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
}
