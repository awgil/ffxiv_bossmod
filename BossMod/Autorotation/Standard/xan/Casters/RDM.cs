using BossMod.RDM;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class RDM(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player, PotionType.Intelligence)
{
    public enum Track { Combo = SharedTrack.Count, Dash }
    public enum ComboStrategy
    {
        Complete,
        Break
    }
    public enum DashStrategy
    {
        CloseMove,
        Close,
        Move,
        Any,
        Forbid
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan RDM", "Red Mage", "Standard rotation (xan)|Casters", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.RDM), 100);

        def.DefineShared().AddAssociatedActions(AID.Embolden, AID.Manafication);

        def.Define(Track.Combo).As<ComboStrategy>("Melee Combo")
            .AddOption(ComboStrategy.Complete, "Complete", "Require melee combo to be completed - do nothing while target is out of range")
            .AddOption(ComboStrategy.Break, "Break", "Allow breaking melee combo if target moves out of range");

        def.Define(Track.Dash).As<DashStrategy>("Corps-a-Corps")
            .AddOption(DashStrategy.CloseMove, "CloseMove", "Only use while close to target and not moving")
            .AddOption(DashStrategy.Close, "Close", "Only use while close to target")
            .AddOption(DashStrategy.Move, "Move", "Only use while not moving")
            .AddOption(DashStrategy.Any, "Any", "Use ASAP")
            .AddOption(DashStrategy.Forbid, "Forbid", "Don't use at all");

        return def;
    }

    public uint BlackMana;
    public uint WhiteMana;
    public uint Stacks;

    public float Dualcast;
    public float Acceleration;
    public float VerfireReady;
    public float VerstoneReady;
    public int Swordplay;
    public float ThornedFlourish;
    public float GrandImpact;
    public float Prefulgence;

    public uint LowestMana => Math.Min(BlackMana, WhiteMana);

    public int NumAOETargets;
    public int NumConeTargets;
    public int NumLineTargets;

    private Enemy? BestAOETarget;
    private Enemy? BestConeTarget;
    private Enemy? BestLineTarget;

    private bool InCombo
        => ComboLastMove == AID.Riposte && Unlocked(AID.Zwerchhau)
        || ComboLastMove == AID.Zwerchhau && Unlocked(AID.Redoublement)
        || ComboLastMove is AID.EnchantedMoulinetDeux or AID.EnchantedMoulinet;

    protected override float GetCastTime(AID aid)
    {
        if (Dualcast > GCD)
            return 0;

        if (Acceleration > GCD)
        {
            switch (aid)
            {
                case AID.Verthunder:
                case AID.VerthunderIII:
                case AID.Veraero:
                case AID.VeraeroIII:
                case AID.Scatter:
                case AID.Impact:
                    return 0;
            }
        }

        return base.GetCastTime(aid);
    }

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<RedMageGauge>();
        BlackMana = gauge.BlackMana;
        WhiteMana = gauge.WhiteMana;
        Stacks = gauge.ManaStacks;

        Dualcast = StatusLeft(SID.Dualcast);
        Acceleration = StatusLeft(SID.Acceleration);
        VerfireReady = StatusLeft(SID.VerfireReady);
        VerstoneReady = StatusLeft(SID.VerstoneReady);
        Swordplay = StatusStacks(SID.MagickedSwordplay);
        ThornedFlourish = StatusLeft(SID.ThornedFlourish);
        GrandImpact = StatusLeft(SID.GrandImpactReady);
        Prefulgence = StatusLeft(SID.PrefulgenceReady);

        (BestAOETarget, NumAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestLineTarget, NumLineTargets) = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget);
        (BestConeTarget, NumConeTargets) = SelectTarget(strategy, primaryTarget, 8, (primary, other) => Hints.TargetInAOECone(other, Player.Position, 8, Player.DirectionTo(primary), 60.Degrees()));

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.Veraero))
                PushGCD(AID.Veraero, primaryTarget);

            return;
        }

        var comboMana = Unlocked(AID.Redoublement) ? 50
            : Unlocked(AID.Zwerchhau) ? 35
            : 20;

        if (primaryTarget is { } tar && (Swordplay > 0 || LowestMana >= comboMana || InCombo))
            Hints.GoalZones.Add(Hints.GoalSingleTarget(tar.Actor, 3));

        GoalZoneSingle(25);

        OGCD(strategy, primaryTarget);

        if (ComboLastMove is AID.Scorch)
            PushGCD(AID.Resolution, BestLineTarget);

        if (ComboLastMove is AID.Verflare or AID.Verholy)
            PushGCD(AID.Scorch, BestAOETarget);

        if (Stacks == 3)
            PushGCD(BlackMana > WhiteMana ? AID.Verholy : AID.Verflare, BestAOETarget);

        if (ComboLastMove == AID.Zwerchhau && Unlocked(AID.Redoublement))
            PushGCD(AID.Redoublement, primaryTarget);

        if (ComboLastMove == AID.Riposte && Unlocked(AID.Zwerchhau))
            PushGCD(AID.Zwerchhau, primaryTarget);

        if (ComboLastMove == AID.EnchantedMoulinetDeux)
            PushGCD(AID.EnchantedMoulinetTrois, BestConeTarget);

        if (ComboLastMove == AID.EnchantedMoulinet)
            PushGCD(AID.EnchantedMoulinetDeux, BestConeTarget);

        if (InCombo && strategy.Option(Track.Combo).As<ComboStrategy>() == ComboStrategy.Complete)
            return;

        if (GrandImpact > GCD)
            PushGCD(AID.GrandImpact, BestAOETarget);

        if (Acceleration > GCD)
        {
            if (NumAOETargets > 2)
                PushGCD(AID.Scatter, BestAOETarget);

            if (BlackMana > WhiteMana)
                PushGCD(AID.Veraero, primaryTarget);

            PushGCD(AID.Verthunder, primaryTarget);
        }

        if (Dualcast == 0 && (Swordplay > 0 || LowestMana >= comboMana))
        {
            if (NumConeTargets > 2)
                PushGCD(AID.EnchantedMoulinet, BestConeTarget);

            PushGCD(AID.Riposte, primaryTarget);
        }

        if (Dualcast > GCD || SwiftcastLeft > GCD)
        {
            if (NumAOETargets > 2)
                PushGCD(AID.Scatter, BestAOETarget);

            if (BlackMana > WhiteMana)
                PushGCD(AID.Veraero, primaryTarget);

            PushGCD(AID.Verthunder, primaryTarget);
        }

        if (NumAOETargets > 2)
        {
            if (BlackMana > WhiteMana)
                PushGCD(AID.VeraeroII, BestAOETarget);

            PushGCD(AID.VerthunderII, BestAOETarget);
        }

        if (VerfireReady > GCD)
            PushGCD(AID.Verfire, primaryTarget);

        if (VerstoneReady > GCD)
            PushGCD(AID.Verstone, primaryTarget);

        PushGCD(AID.Jolt, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        PushOGCD(AID.Swiftcast, Player);

        PushOGCD(AID.Fleche, primaryTarget);

        // intentionally checking for max charges - balance says to save a charge for movement
        if (CanWeave(MaxChargesIn(AID.Acceleration), 0.6f))
            PushOGCD(AID.Acceleration, Player);

        if (strategy.BuffsOk())
        {
            PushOGCD(AID.Embolden, Player);
            if (!InCombo)
                PushOGCD(AID.Manafication, Player);
        }

        PushOGCD(AID.ContreSixte, BestAOETarget);
        PushOGCD(AID.Engagement, primaryTarget);

        if (DashOk(strategy, primaryTarget))
            PushOGCD(AID.CorpsACorps, primaryTarget);

        if (ThornedFlourish > 0)
            PushOGCD(AID.ViceOfThorns, BestAOETarget);

        if (Prefulgence > 0)
            PushOGCD(AID.Prefulgence, BestAOETarget);

        if (RaidBuffsLeft > GCD && Acceleration == 0)
            PushOGCD(AID.Acceleration, Player);

        if (MP <= Player.HPMP.MaxMP * 0.7f)
            PushOGCD(AID.LucidDreaming, Player);
    }

    private bool DashOk(StrategyValues strategy, Enemy? primaryTarget) => strategy.Option(Track.Dash).As<DashStrategy>() switch
    {
        DashStrategy.Any => true,
        DashStrategy.Move => !IsMoving,
        DashStrategy.Close => Player.DistanceToHitbox(primaryTarget) < 3,
        DashStrategy.CloseMove => Player.DistanceToHitbox(primaryTarget) < 3 && !IsMoving,
        _ => false
    };
}
