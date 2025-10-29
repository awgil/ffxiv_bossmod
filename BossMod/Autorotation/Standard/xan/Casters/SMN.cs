using BossMod.SMN;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

[Flags]
public enum SmnFlags
{
    None = 0,
    Aetherflow = 1 << 0,
    Aetherflow2 = 1 << 1,
    Phoenix = 1 << 2,
    SolarBahamut = 1 << 3,
    Ruby = 1 << 5,
    Topaz = 1 << 6,
    Emerald = 1 << 7
}

public enum AttunementType
{
    None = 0,
    Ruby = 1,
    Topaz = 2,
    Emerald = 3
}

public enum Favor
{
    None,
    Ifrit,
    Titan,
    Garuda
}

public enum Trance
{
    None,
    Dreadwyrm,
    Phoenix,
    Lightwyrm
}

public sealed class SMN(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player, PotionType.Intelligence)
{
    public enum Track { Cyclone = SharedTrack.Count }
    public enum CycloneUse
    {
        Automatic,
        Delay,
        DelayMove,
        SkipMove,
        Skip
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan SMN", "Summoner", "Standard rotation (xan)|Casters", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.SMN, Class.ACN), 100);

        def.DefineShared().AddAssociatedActions(AID.SearingLight);

        def.Define(Track.Cyclone).As<CycloneUse>("Cyclone")
            .AddOption(CycloneUse.Automatic, "Use when Ifrit is summoned")
            .AddOption(CycloneUse.Delay, "Delay automatic use, but do not overwrite Ifrit with any other summon")
            .AddOption(CycloneUse.DelayMove, "Delay automatic use until player is not holding a movement key - do not overwrite Ifrit with any other summon")
            .AddOption(CycloneUse.SkipMove, "Skip if a movement key is held, otherwise use")
            .AddOption(CycloneUse.Skip, "Do not use at all");

        return def;
    }

    public SmnFlags TranceFlags;
    public Favor Favor;
    public AttunementType AttunementType;
    public int Attunement;
    public float SummonLeft;
    public float FurtherRuin;
    public float SearingLightLeft;
    public float SearingFlash;
    public float RefulgentLux;
    public bool CrimsonStrikeReady;

    public int Aetherflow => TranceFlags.HasFlag(SmnFlags.Aetherflow2) ? 2 : TranceFlags.HasFlag(SmnFlags.Aetherflow) ? 1 : 0;

    public int NumAOETargets;
    public int NumMeleeTargets;

    private Actor? Carbuncle;
    private Enemy? BestAOETarget;
    private Enemy? BestMeleeTarget;

    public Trance Trance
    {
        get
        {
            if (SummonLeft > 0 && AttunementType == AttunementType.None)
            {
                if (TranceFlags.HasFlag(SmnFlags.SolarBahamut))
                    return Trance.Lightwyrm;

                if (TranceFlags.HasFlag(SmnFlags.Phoenix))
                    return Trance.Phoenix;

                if (Unlocked(AID.DreadwyrmTrance))
                    return Trance.Dreadwyrm;
            }

            return Trance.None;
        }
    }

    private static readonly AID[] Gemshines = [
        AID.RubyRuin1, AID.TopazRuin1, AID.EmeraldRuin1,
        AID.RubyRuin2, AID.TopazRuin2, AID.EmeraldRuin2,
        AID.RubyRuin3, AID.TopazRuin3, AID.EmeraldRuin3,
        AID.RubyRite, AID.TopazRite, AID.EmeraldRite
    ];

    private static readonly AID[] Brilliances = [
        AID.RubyOutburst, AID.TopazOutburst, AID.EmeraldOutburst,
        AID.RubyDisaster, AID.TopazDisaster, AID.EmeraldDisaster,
        AID.RubyCatastrophe, AID.TopazCatastrophe, AID.EmeraldCatastrophe
    ];

    public AID BestGemshine
    {
        get
        {
            var offset = AttunementType switch
            {
                AttunementType.Ruby => 0,
                AttunementType.Topaz => 1,
                AttunementType.Emerald => 2,
                _ => -1
            };
            if (offset < 0)
                return AID.None;

            if (Unlocked(TraitID.RuinMastery1))
                offset += 3;
            if (Unlocked(TraitID.RuinMastery2))
                offset += 3;
            if (Unlocked(TraitID.RuinMastery3))
                offset += 3;

            return Gemshines[offset];
        }
    }

    public AID BestBrilliance
    {
        get
        {
            var offset = AttunementType switch
            {
                AttunementType.Ruby => 0,
                AttunementType.Topaz => 1,
                AttunementType.Emerald => 2,
                _ => -1
            };
            if (offset < 0)
                return AID.None;

            if (Unlocked(TraitID.OutburstMastery1))
                offset += 3;
            if (Unlocked(TraitID.OutburstMastery2))
                offset += 3;

            return Brilliances[offset];
        }
    }

    public AID BestRuin => Trance switch
    {
        Trance.Dreadwyrm => AID.AstralImpulse,
        Trance.Phoenix => AID.FountainOfFire,
        Trance.Lightwyrm => AID.UmbralImpulse,
        _ => Unlocked(AID.Ruin3)
            ? AID.Ruin3
            : Unlocked(AID.Ruin2)
                ? AID.Ruin2
                : AID.Ruin1,
    };

    public AID BestOutburst => Trance switch
    {
        Trance.Dreadwyrm => AID.AstralFlare,
        Trance.Phoenix => AID.BrandOfPurgatory,
        Trance.Lightwyrm => AID.UmbralFlare,
        _ => Unlocked(AID.TriDisaster) ? AID.TriDisaster : AID.Outburst,
    };

    public AID BestAethercharge
    {
        get
        {
            if (TranceFlags.HasFlag(SmnFlags.SolarBahamut))
                return AID.SummonSolarBahamut;

            if (TranceFlags.HasFlag(SmnFlags.Phoenix))
                return AID.SummonPhoenix;

            if (Unlocked(AID.SummonBahamut))
                return AID.SummonBahamut;

            if (Unlocked(AID.DreadwyrmTrance))
                return AID.DreadwyrmTrance;

            return AID.Aethercharge;
        }
    }

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<SummonerGauge>();
        TranceFlags = (SmnFlags)gauge.AetherFlags;
        SummonLeft = gauge.SummonTimer * 0.001f;
        AttunementType = (AttunementType)(gauge.Attunement & 3);
        Attunement = gauge.Attunement >> 2;

        // intentionally not using activepet as it is cleared when current summon's duration expires, even though the actor still exists, causing autorot to constantly do redundant summons
        Carbuncle = World.Actors.FirstOrDefault(x => x.Type == ActorType.Pet && x.OwnerID == Player.InstanceID);

        var favor = Player.Statuses.FirstOrDefault(x => (SID)x.ID is SID.GarudasFavor or SID.IfritsFavor or SID.TitansFavor);

        Favor = (SID)favor.ID switch
        {
            SID.GarudasFavor => Favor.Garuda,
            SID.IfritsFavor => Favor.Ifrit,
            SID.TitansFavor => Favor.Titan,
            _ => Favor.None
        };
        FurtherRuin = StatusLeft(SID.FurtherRuin);
        SearingFlash = StatusLeft(SID.RubysGlimmer);
        SearingLightLeft = Player.FindStatus(SID.SearingLight) is ActorStatus s ? StatusDuration(s.ExpireAt) : 0;
        RefulgentLux = StatusLeft(SID.RefulgentLux);
        CrimsonStrikeReady = Player.FindStatus(SID.CrimsonStrikeReady) != null;

        (BestAOETarget, NumAOETargets) = SelectTargetByHP(strategy, primaryTarget, 25, IsSplashTarget);
        (BestMeleeTarget, NumMeleeTargets) = SelectTarget(strategy, primaryTarget, 3, IsSplashTarget);

        if (Carbuncle == null)
            PushGCD(AID.SummonCarbuncle, Player);

        if (primaryTarget == null)
            return;

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining <= GetCastTime(AID.Ruin1))
                PushGCD(AID.Ruin1, primaryTarget);

            return;
        }

        GoalZoneSingle(25);

        if (CrimsonStrikeReady)
        {
            Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget.Actor, 3));
            PushGCD(AID.CrimsonStrike, BestMeleeTarget);
        }

        if (Favor == Favor.Garuda)
            PushGCD(AID.Slipstream, BestAOETarget);

        if (AttunementType != AttunementType.None)
        {
            if (NumAOETargets > 2)
                PushGCD(BestBrilliance, BestAOETarget);

            PushGCD(BestGemshine, primaryTarget);
        }

        if (Favor == Favor.Ifrit)
        {
            switch (strategy.Option(Track.Cyclone).As<CycloneUse>())
            {
                case CycloneUse.Automatic:
                    PushGCD(AID.CrimsonCyclone, BestAOETarget);
                    break;
                case CycloneUse.Delay: // do nothing, pause rotation
                    return;
                case CycloneUse.DelayMove:
                    if (IsMoving)
                        return;
                    else
                        PushGCD(AID.CrimsonCyclone, BestAOETarget);
                    break;
                case CycloneUse.SkipMove:
                    if (!IsMoving)
                        PushGCD(AID.CrimsonCyclone, BestAOETarget);
                    break;
                case CycloneUse.Skip:
                    break;
            }
        }

        if (SummonLeft <= GCD)
        {
            // TODO make this configurable - this will summon baha/phoenix and ignore current gems
            // balance says to default to summons if you don't know whether you will lose a usage or not
            if (ReadyIn(AID.Aethercharge) <= GCD && Player.InCombat)
            {
                if (!Unlocked(AID.DreadwyrmTrance) || DowntimeIn > GCD + 15)
                    // scarlet flame and wyrmwave are both single target, this is ok
                    PushGCD(BestAethercharge, primaryTarget);
            }

            if (TranceFlags.HasFlag(SmnFlags.Topaz))
                PushGCD(AID.SummonTopaz, Unlocked(TraitID.TopazSummoningMastery) ? BestAOETarget : primaryTarget);

            if (TranceFlags.HasFlag(SmnFlags.Emerald))
                PushGCD(AID.SummonEmerald, Unlocked(TraitID.EmeraldSummoningMastery) ? BestAOETarget : primaryTarget);

            if (TranceFlags.HasFlag(SmnFlags.Ruby))
                PushGCD(AID.SummonRuby, Unlocked(TraitID.RubySummoningMastery) ? BestAOETarget : primaryTarget);
        }

        if (FurtherRuin > GCD && SummonLeft == 0)
            PushGCD(AID.Ruin4, BestAOETarget);

        if (NumAOETargets > 2)
            PushGCD(BestOutburst, BestAOETarget);

        PushGCD(BestRuin, primaryTarget);

        OGCDs(strategy, primaryTarget);
    }

    private void OGCDs(StrategyValues strategy, Enemy? primaryTarget)
    {
        if (NextGCD == AID.Slipstream)
            PushOGCD(AID.Swiftcast, Player);

        if (!Player.InCombat || primaryTarget == null)
            return;

        if (Favor == Favor.Titan)
            PushOGCD(AID.MountainBuster, BestAOETarget);

        // don't overwrite other player's searing light in roulettes lol i guess
        if (ShouldBuff(strategy))
            PushOGCD(AID.SearingLight, Player);

        switch (Trance)
        {
            case Trance.Dreadwyrm:
                PushOGCD(AID.EnkindleBahamut, BestAOETarget);
                PushOGCD(AID.Deathflare, BestAOETarget);
                break;

            case Trance.Lightwyrm:
                PushOGCD(AID.EnkindleSolarBahamut, BestAOETarget);
                PushOGCD(AID.Sunflare, BestAOETarget);
                break;

            case Trance.Phoenix:
                PushOGCD(AID.EnkindlePhoenix, BestAOETarget);

                if (CanWeave(AID.Rekindle))
                {
                    var rekindleTarget = World.Party.WithoutSlot(excludeAlliance: true).MinBy(x => x.HPRatio);
                    if (rekindleTarget?.HPRatio < 1)
                        PushOGCD(AID.Rekindle, rekindleTarget);

                    if (SummonLeft < 2)
                        PushOGCD(AID.Rekindle, Player);
                }
                break;
        }

        if (Aetherflow > 0)
        {
            // have to separate these because they don't share a cdgroup, meaning you can accidentally do painflare and fester in one window when 2 festers in 2 windows is optimal
            if (Unlocked(AID.Painflare) && NumAOETargets > 2)
                PushOGCD(AID.Painflare, BestAOETarget);
            else
                PushOGCD(AID.Fester, primaryTarget);
        }

        if (NumAOETargets > 2)
            PushOGCD(AID.EnergySiphon, BestAOETarget);

        PushOGCD(AID.EnergyDrain, primaryTarget);

        if (SearingFlash > 0)
            PushOGCD(AID.SearingFlash, BestAOETarget);

        if (MP <= Player.HPMP.MaxMP * 0.7f)
            PushOGCD(AID.LucidDreaming, Player);

        if (RefulgentLux is > 0 and < 2.5f)
            PushOGCD(AID.LuxSolaris, Player);
    }

    private bool ShouldBuff(StrategyValues strategy)
    {
        if (!strategy.BuffsOk())
            return false;

        if (CombatTimer < 10)
            return SummonLeft < 13;

        return SearingLightLeft == 0;
    }
}
