using BossMod.SGE;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class SGE(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
{
    public enum Track { Kardia = SharedTrack.Count, Druo }
    public enum KardiaStrategy { Auto, Manual }
    public enum DruoStrategy { Auto, Manual }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SGE", "Sage", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SGE), 100);

        def.DefineShared();

        def.Define(Track.Kardia).As<KardiaStrategy>("Kardia")
            .AddOption(KardiaStrategy.Auto, "Auto", "Automatically choose Kardia target")
            .AddOption(KardiaStrategy.Manual, "Manual", "Don't automatically choose Kardia target");
        def.Define(Track.Druo).As<DruoStrategy>("Druochole")
            .AddOption(DruoStrategy.Auto, "Auto", "Prevent Addersgall overcap by using Druochole on lowest-HP ally")
            .AddOption(DruoStrategy.Manual, "Manual", "Do not automatically use Druochole");

        return def;
    }

    public int Gall;
    public float NextGall;
    public int Sting;
    public bool Eukrasia;
    public float ZoeLeft;

    public int NumAOETargets;
    public int NumRangedAOETargets;
    public int NumPhlegmaTargets;
    public int NumPneumaTargets;

    public int NumNearbyDotTargets;
    public float TargetDotLeft;

    private Actor? BestPhlegmaTarget; // 6y/5y
    private Actor? BestRangedAOETarget; // 25y/5y toxikon, psyche
    private Actor? BestPneumaTarget; // 25y/4y rect

    private Actor? BestDotTarget;

    protected override float GetCastTime(AID aid) => Eukrasia ? 0 : base.GetCastTime(aid);

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, range: 25);

        var gauge = World.Client.GetGauge<SageGauge>();

        Gall = gauge.Addersgall;
        Sting = gauge.Addersting;
        NextGall = MathF.Max(0, 20f - gauge.AddersgallTimer / 1000f);
        Eukrasia = gauge.EukrasiaActive;

        (BestPhlegmaTarget, NumPhlegmaTargets) = SelectTarget(strategy, primaryTarget, 6, IsSplashTarget);
        (BestRangedAOETarget, NumRangedAOETargets) = SelectTarget(strategy, primaryTarget, 25, IsSplashTarget);
        (BestPneumaTarget, NumPneumaTargets) = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget);

        NumAOETargets = 0;
        NumNearbyDotTargets = 0;
        foreach (var t in Hints.PotentialTargets.Where(x => x.Actor.DistanceToHitbox(Player) <= 5))
        {
            if (t.Priority < 0)
            {
                NumAOETargets = 0;
                NumNearbyDotTargets = 0;
                break;
            }
            NumAOETargets++;
            if (DotDuration(t.Actor) < 3)
                NumNearbyDotTargets++;
        }

        NumAOETargets = AdjustNumTargets(strategy, NumAOETargets);
        NumNearbyDotTargets = AdjustNumTargets(strategy, NumNearbyDotTargets);

        (BestDotTarget, TargetDotLeft) = SelectDotTarget(strategy, primaryTarget, DotDuration, 2);

        DoGCD(strategy, primaryTarget);
        DoOGCD(strategy, primaryTarget);
    }

    private void DoGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (strategy.Option(Track.Kardia).As<KardiaStrategy>() == KardiaStrategy.Auto
            && Unlocked(AID.Kardia)
            && Player.FindStatus((uint)SID.Kardia) == null
            && FindKardiaTarget() is Actor kardiaTarget
            && !World.Party.Members[World.Party.FindSlot(kardiaTarget.InstanceID)].InCutscene)
            PushGCD(AID.Kardia, kardiaTarget);

        if (!Player.InCombat && Unlocked(AID.Eukrasia) && !Eukrasia)
            PushGCD(AID.Eukrasia, Player);

        if (Unlocked(AID.Eukrasia))
        {
            if (NumNearbyDotTargets > 1 && Unlocked(AID.EukrasianDyskrasia))
            {
                if (!Eukrasia)
                    PushGCD(AID.Eukrasia, Player);

                PushGCD(AID.Dyskrasia, Player);
            }
            else if (!CanFitGCD(TargetDotLeft, 1))
            {
                if (!Eukrasia)
                    PushGCD(AID.Eukrasia, Player);

                PushGCD(AID.Dosis, BestDotTarget);
            }
        }

        if (CD(AID.Pneuma) <= GCD && NumPneumaTargets > 1)
            PushGCD(AID.Pneuma, BestPneumaTarget);

        if (NumPhlegmaTargets > 2 && CD(AID.Phlegma) - 40 <= GCD || CD(AID.Phlegma) <= GCD)
            PushGCD(AID.Phlegma, BestPhlegmaTarget);

        if (NumAOETargets > 1)
        {
            if (Sting > 0 && NumPhlegmaTargets > 1)
                PushGCD(AID.Toxikon, BestPhlegmaTarget);

            PushGCD(AID.Dyskrasia, Player);
        }

        PushGCD(AID.Dosis, primaryTarget);

        // fallbacks for forced movement
        if (CD(AID.Phlegma) - 40 <= GCD && NumPhlegmaTargets > 0)
            PushGCD(AID.Phlegma, BestPhlegmaTarget);
        if (NumRangedAOETargets > 0 && Sting > 0)
            PushGCD(AID.Toxikon, BestRangedAOETarget);
        if (NumAOETargets > 0)
            PushGCD(AID.Dyskrasia, Player);
    }

    private void DoOGCD(StrategyValues strategy, Actor? primaryTarget)
    {
        if (!Player.InCombat)
            return;

        if (Gall < 2 && NextGall > 10)
            PushOGCD(AID.Rhizomata, Player);

        if ((Gall == 3 || Gall == 2 && NextGall < 2.5f) && Player.HPMP.CurMP <= 9000 && strategy.Option(Track.Druo).As<DruoStrategy>() == DruoStrategy.Auto)
        {
            var healTarget = World.Party.WithoutSlot(partyOnly: true).MinBy(x => x.HPMP.CurHP / x.HPMP.MaxHP);
            PushOGCD(AID.Druochole, healTarget);
        }

        if (MP <= 7000)
            PushOGCD(AID.LucidDreaming, Player);

        if (NumRangedAOETargets > 0)
            PushOGCD(AID.Psyche, BestRangedAOETarget);
    }

    static readonly SID[] DotStatus = [SID.EukrasianDosis, SID.EukrasianDosisII, SID.EukrasianDosisIII, SID.EukrasianDyskrasia];

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

    private Actor? FindKardiaTarget()
    {
        var party = World.Party.WithoutSlot(partyOnly: true);
        var total = 0;
        var tanks = 0;
        Actor? tank = null;
        foreach (var actor in party)
        {
            total++;
            if (actor.Class.GetRole() == Role.Tank)
            {
                tanks++;
                tank ??= actor;
            }
        }
        if (total == 1)
            return Player;

        if (tanks == 1)
            return tank;

        return null;
    }
}
