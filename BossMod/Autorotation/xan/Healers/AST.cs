using BossMod.AST;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
namespace BossMod.Autorotation.xan;
public sealed class AST(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
{
    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan AST", "Astrologian", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.AST), 100);

        def.DefineShared().AddAssociatedActions(AID.Divination);

        return def;
    }

    public AstrologianCard[] Cards = [];
    public AstrologianCard Arcana;
    public int NumCards => Cards.Count(x => x != AstrologianCard.None);

    public float LightspeedLeft;
    public float DivinationLeft;
    public float Divining;
    public float TargetDotLeft;

    public int NumCrownTargets;
    public int NumAOETargets;

    private Actor? BestAOETarget;
    private Actor? BestDotTarget;

    protected override float GetCastTime(AID aid)
    {
        var b = base.GetCastTime(aid);

        if (LightspeedLeft > GCD)
            b = MathF.Max(0, b - 2.5f);

        return b;
    }

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<AstrologianGauge>();

        Cards = gauge.CurrentCards;
        Arcana = gauge.CurrentArcana;

        LightspeedLeft = StatusLeft(SID.Lightspeed);
        DivinationLeft = StatusDetails(Player, SID.Divination, Player.InstanceID, 20).Left;
        Divining = StatusLeft(SID.Divining);

        (BestAOETarget, NumAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        NumCrownTargets = NumNearbyTargets(strategy, 20);
        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, CombustLeft, 2);

        OGCD(strategy, primaryTarget);

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining < GetCastTime(AID.Malefic))
                PushGCD(AID.Malefic, primaryTarget);

            return;
        }

        if (NumAOETargets > 2)
            PushGCD(AID.Gravity, BestAOETarget);

        if (!CanFitGCD(TargetDotLeft, 1))
            PushGCD(AID.Combust, BestDotTarget);

        PushGCD(AID.Malefic, primaryTarget);
    }

    private void OGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Player.InCombat || primaryTarget == null)
            return;

        if (ShouldLightspeed(strategy))
            PushOGCD(AID.Lightspeed, Player);

        if (!HaveBuffCard && !HaveLord)
            PushOGCD(AID.AstralDraw, Player);

        if (CombatTimer > 5 && strategy.BuffsOk())
            PushOGCD(AID.Divination, Player);

        if (UseCards)
        {
            if (HaveBuffCard)
                PushOGCD(AID.PlayI, FindBestCardTarget(strategy, isRanged: Cards[0] == AstrologianCard.Spear));

            if (HaveLord && NumCrownTargets > 0)
                PushOGCD(AID.LordOfCrowns, Player);
        }

        if (Divining > 0 && NumAOETargets > 0)
            PushOGCD(AID.Oracle, BestAOETarget);

        if (MP <= 7000)
            PushOGCD(AID.LucidDreaming, Player);
    }

    private bool ShouldLightspeed(StrategyValues strategy)
    {
        if (CanWeave(MaxChargesIn(AID.Lightspeed), 0.6f))
            return true;

        return LightspeedLeft == 0 && (DivinationLeft > 10 || CanWeave(AID.Divination, 2));
    }

    private float CombustLeft(Actor? actor) => actor == null ? float.MaxValue : Utils.MaxAll(
        StatusDetails(actor, SID.Combust, Player.InstanceID).Left,
        StatusDetails(actor, SID.CombustII, Player.InstanceID).Left,
        StatusDetails(actor, SID.CombustIII, Player.InstanceID).Left
    );

    private bool HaveLord => Unlocked(AID.MinorArcana) && Arcana == AstrologianCard.Lord;
    private bool HaveBuffCard => Cards[0] != AstrologianCard.None;
    private bool UseCards => DivinationLeft > 10 || !Unlocked(AID.Divination);

    private bool HasCard(Actor actor) => StatusDetails(actor, SID.TheBalance, Player.InstanceID).Left > 0 || StatusDetails(actor, SID.TheSpear, Player.InstanceID).Left > 0;

    private Actor FindBestCardTarget(StrategyValues strategy, bool isRanged)
    {
        int MeleePrioBase(Class klass) => klass switch
        {
            Class.SAM => 50,
            Class.NIN => 49,
            Class.VPR => 48,
            Class.RPR => 47,
            Class.MNK => 46,
            Class.DRG => 45,
            Class.ROG or Class.PGL or Class.LNC => 20,
            _ => klass.IsDD() ? 0 : -50
        };
        int RangedPrioBase(Class klass) => klass switch
        {
            Class.PCT => 50,
            Class.SMN => 40,
            Class.MCH => 39,
            Class.RDM => 38,
            Class.BRD => 37,
            Class.BLM => 36,
            Class.DNC => 35,
            Class.ACN or Class.ARC or Class.THM => 20,
            _ => klass.IsDD() ? 0 : -50
        };

        int Prio(Actor actor)
        {
            var def = isRanged ? RangedPrioBase(actor.Class) : MeleePrioBase(actor.Class);

            // weakness
            if (actor.FindStatus(43) != null)
                def -= 25;

            if (actor.FindStatus(44) != null)
                def -= 50;

            return def;
        }

        return World.Party.WithoutSlot().Where(actor => Player.DistanceToHitbox(actor) <= 30 && !HasCard(actor)).MaxBy(Prio) ?? Player;
    }
}
