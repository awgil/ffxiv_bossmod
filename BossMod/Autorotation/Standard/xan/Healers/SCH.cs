using BossMod.SCH;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class SCH(RotationModuleManager manager, Actor player) : CastxanOld<AID, TraitID>(manager, player, PotionType.Mind)
{
    public enum Track { Place = SharedTrack.Count }
    public enum FairyPlacement
    {
        Manual,
        AutoHeel,
        FullAuto,
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SCH", "Scholar", "Standard rotation (xan)|Healers", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SCH), 100);

        def.DefineShared("Chain Stratagem").AddAssociatedActions(AID.ChainStratagem, AID.Dissipation);

        def.Define(Track.Place).As<FairyPlacement>("FairyPlace", "Fairy placement")
            .AddOption(FairyPlacement.Manual, "Do not automatically move fairy")
            .AddOption(FairyPlacement.AutoHeel, "Automatically order fairy to follow player when combat ends")
            .AddOption(FairyPlacement.FullAuto, "Automatically place fairy at arena center (if one exists) - order fairy to follow when out of combat");

        return def;
    }

    public int Aetherflow;
    public int FairyGauge;
    public float SeraphTimer;
    public bool FairyGone;

    public float ImpactImminent;
    public float TargetDotLeft;
    public int NumAOETargets;
    public int NumRangedAOETargets;

    public enum PetOrder
    {
        None = 0,
        Follow = 2,
        Place = 3
    }

    public PetOrder FairyOrder;

    private Actor? Eos;
    private Enemy? BestDotTarget;
    private Enemy? BestRangedAOETarget;

    private DateTime _summonWait;

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<ScholarGauge>();
        Aetherflow = gauge.Aetherflow;
        FairyGauge = gauge.FairyGauge;
        SeraphTimer = gauge.SeraphTimer * 0.001f;
        FairyGone = gauge.DismissedFairy > 0;

        var pet = World.Client.ActivePet;

        Eos = pet.InstanceID == 0xE0000000 ? null : World.Actors.Find(pet.InstanceID);

        FairyOrder = (PetOrder)pet.Order;

        ImpactImminent = StatusLeft(SID.ImpactImminent);

        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, DotDuration, 2);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        NumAOETargets = NumMeleeAOETargets(strategy);

        // annoying hack to work around delay between no-pet status ending and pet actor reappearing
        if (Eos != null || FairyGone)
            _summonWait = World.CurrentTime.AddSeconds(1);

        if (Eos == null && !FairyGone && World.CurrentTime > _summonWait)
            PushGCD(AID.SummonEos, Player);

        OGCD(strategy, primaryTarget);

        OrderFairy(strategy);

        if (primaryTarget == null)
            return;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining <= GetCastTime(AID.Broil1))
                PushGCD(AID.Broil1, primaryTarget);

            return;
        }

        if (!CanFitGCD(TargetDotLeft, 1))
            PushGCD(AID.Bio1, BestDotTarget);

        if (RaidBuffsLeft > 0 && !CanFitGCD(RaidBuffsLeft, 1))
            PushGCD(AID.Bio1, BestDotTarget);

        // for about 8 levels starting at 46, art of war (aoe) is our best single target action
        var rangeToTarget = Unlocked(AID.ArtOfWar1) && !Unlocked(AID.Broil1) ? 5 : 25;

        var needAOETargets = Unlocked(AID.Broil1) ? 2 : 1;

        GoalZoneCombined(strategy, rangeToTarget, Hints.GoalAOECircle(5), AID.ArtOfWar1, needAOETargets);

        if (NumAOETargets >= needAOETargets)
            PushGCD(AID.ArtOfWar1, Player);

        PushGCD(AID.Ruin1, primaryTarget);

        // instant cast - fallback for movement
        PushGCD(AID.Ruin2, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (primaryTarget == null || !Player.InCombat)
            return;

        if (strategy.BuffsOk())
        {
            //if (Eos != null)
            //    PushOGCD(AID.Dissipation, Player);

            if (RaidBuffsLeft > 15 || RaidBuffsIn > 1000)
                PushOGCD(AID.ChainStratagem, primaryTarget);
        }

        if (Aetherflow == 0)
            PushOGCD(AID.Aetherflow, Player);

        if (Aetherflow > 0 && CanWeave(AID.Aetherflow, Aetherflow))
            PushOGCD(AID.EnergyDrain, primaryTarget);

        if (ImpactImminent > 0)
            PushOGCD(AID.BanefulImpaction, BestRangedAOETarget);

        if (MP <= Player.HPMP.MaxMP * 0.7f)
            PushOGCD(AID.LucidDreaming, Player);
    }

    private void OrderFairy(StrategyValues strategy)
    {
        if (Eos == null)
            return;

        void autoheel()
        {
            if (FairyOrder != PetOrder.Follow && !Player.InCombat && CountdownRemaining == null)
                Hints.ActionsToExecute.Push(new ActionID(ActionType.PetAction, 2), null, ActionQueue.Priority.VeryHigh);
        }

        void autoplace()
        {
            if (FairyOrder != PetOrder.Place && (Player.InCombat || CountdownRemaining > 0))
            {
                if (Bossmods.ActiveModule?.Arena.Center is WPos p)
                    Hints.ActionsToExecute.Push(new ActionID(ActionType.PetAction, 3), null, ActionQueue.Priority.VeryHigh, targetPos: p.ToVec3(Player.PosRot.Y));
            }
        }

        var strat = strategy.Option(Track.Place).As<FairyPlacement>();

        switch (strat)
        {
            case FairyPlacement.Manual:
                return;
            case FairyPlacement.AutoHeel:
                autoheel();
                return;
            case FairyPlacement.FullAuto:
                autoheel();
                autoplace();
                return;
        }
    }

    static readonly SID[] DotStatus = [SID.Bio1, SID.Bio2, SID.Biolysis];

    private float DotDuration(Actor? x)
    {
        if (x == null)
            return float.MaxValue;

        foreach (var stat in DotStatus)
        {
            var dur = StatusDetails(x, (uint)stat, Player.InstanceID).Left;
            if (dur > 0)
                return dur;
        }

        return 0;
    }
}
