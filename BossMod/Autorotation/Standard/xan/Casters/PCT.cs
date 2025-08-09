using BossMod.PCT;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class PCT(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player, PotionType.Intelligence)
{
    public enum Track { Motif = SharedTrack.Count, Holy, Hammer }
    public enum MotifStrategy { Combat, Downtime, Instant }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan PCT", "Pictomancer", "Standard rotation (xan)|Casters", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PCT), 100);

        def.DefineShared().AddAssociatedActions(AID.StarryMuse);

        def.Define(Track.Motif).As<MotifStrategy>("Motifs")
            .AddOption(MotifStrategy.Combat, "Combat", "Cast motifs in combat, outside of burst window")
            .AddOption(MotifStrategy.Downtime, "Downtime", "Cast motifs in combat if there are no targets nearby")
            .AddOption(MotifStrategy.Instant, "Instant", "Only cast motifs when they are instant (out of combat)");

        def.DefineSimple(Track.Holy, "Holy").AddAssociatedActions(AID.HolyInWhite, AID.CometInBlack);
        def.DefineSimple(Track.Hammer, "Hammer").AddAssociatedActions(AID.HammerStamp);

        return def;
    }

    public int Palette; // 0-100
    public int Paint; // 0-5

    public bool PomClawMuse => CanvasFlags.HasFlag(CanvasFlags.Pom) || CanvasFlags.HasFlag(CanvasFlags.Claw);
    public bool WingFangMuse => CanvasFlags.HasFlag(CanvasFlags.Wing) || CanvasFlags.HasFlag(CanvasFlags.Maw);
    public bool Portrait => CreatureFlags.HasFlag(CreatureFlags.MooglePortait) || CreatureFlags.HasFlag(CreatureFlags.MadeenPortrait);

    public bool CreaturePainted => PomClawMuse || WingFangMuse;
    public bool WeaponPainted => CanvasFlags.HasFlag(CanvasFlags.Weapon);
    public bool LandscapePainted => CanvasFlags.HasFlag(CanvasFlags.Landscape);
    public bool Monochrome;
    public CreatureFlags CreatureFlags;
    public CanvasFlags CanvasFlags;

    public enum AetherHues : uint
    {
        None = 0,
        One = 1,
        Two = 2
    }

    public AetherHues Hues;
    public int Subtractive;
    public float StarryMuseLeft; // 20s max
    public (float Left, int Stacks) HammerTime;
    public float SpectrumLeft; // 30s max
    public int Hyperphantasia;
    public float RainbowBright;
    public float Starstruck;

    public int NumAOETargets;
    public int NumLineTargets;

    private Enemy? BestAOETarget;
    private Enemy? BestLineTarget;

    public enum GCDPriority : int
    {
        None = 0,
        HolyMove = 100,
        HammerMove = 200,
        Standard = 500,
        High = 600
    }

    private float GetApplicationDelay(AID action) => action switch
    {
        AID.RainbowDrip => 1.24f,
        AID.FireInRed => 0.84f,
        AID.ClawedMuse => 0.98f,
        AID.FangedMuse => 1.16f,
        AID.MogOfTheAges => 1.15f,
        AID.RetributionOfTheMadeen => 1.30f,
        _ => 0
    };

    public const uint LeylinesOID = 0x6DF;

    private AID BestLivingMuse
    {
        get
        {
            if (CanvasFlags.HasFlag(CanvasFlags.Pom))
                return AID.PomMuse;
            if (CanvasFlags.HasFlag(CanvasFlags.Wing))
                return AID.WingedMuse;
            if (CanvasFlags.HasFlag(CanvasFlags.Claw))
                return AID.ClawedMuse;
            if (CanvasFlags.HasFlag(CanvasFlags.Maw))
                return AID.FangedMuse;
            return AID.None;
        }
    }

    private AID BestPortrait
    {
        get
        {
            if (CreatureFlags.HasFlag(CreatureFlags.MooglePortait))
                return AID.MogOfTheAges;
            if (CreatureFlags.HasFlag(CreatureFlags.MadeenPortrait))
                return AID.RetributionOfTheMadeen;
            return AID.None;
        }
    }

    public override void Exec(StrategyValues strategy, Enemy? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<PictomancerGauge>();
        Palette = gauge.PalleteGauge;
        Paint = gauge.Paint;
        CreatureFlags = gauge.CreatureFlags;
        CanvasFlags = gauge.CanvasFlags;

        Subtractive = StatusStacks(SID.SubtractivePalette);
        StarryMuseLeft = StatusLeft(SID.StarryMuse);
        HammerTime = Status(SID.HammerTime);
        SpectrumLeft = StatusLeft(SID.SubtractiveSpectrum);
        Monochrome = Player.FindStatus(SID.MonochromeTones) != null;
        Hyperphantasia = StatusStacks(SID.Hyperphantasia);
        RainbowBright = StatusLeft(SID.RainbowBright);
        Starstruck = StatusLeft(SID.Starstruck);

        var ah1 = StatusLeft(SID.Aetherhues);
        var ah2 = StatusLeft(SID.AetherhuesII);

        Hues = ah1 > 0 ? AetherHues.One : ah2 > 0 ? AetherHues.Two : AetherHues.None;

        (BestAOETarget, NumAOETargets) = SelectTargetByHP(strategy, primaryTarget, 25, IsSplashTarget);

        BestLineTarget = SelectTarget(strategy, primaryTarget, 25, Is25yRectTarget).Best;

        if (!Player.InCombat && Player.CastInfo is { Action: var act } && (AID)act.ID is AID.PomMotif or AID.WingMotif or AID.ClawMotif or AID.MawMotif or AID.HammerMotif or AID.StarrySkyMotif)
            Hints.ForceCancelCast = true;

        var motifOk = IsMotifOk(strategy);

        if (motifOk)
        {
            if (!CreaturePainted && Unlocked(AID.CreatureMotif))
                PushGCD(AID.CreatureMotif, Player, GCDPriority.Standard);

            if (!WeaponPainted && Unlocked(AID.WeaponMotif) && HammerTime.Left == 0)
                PushGCD(AID.WeaponMotif, Player, GCDPriority.Standard);

            if (!LandscapePainted && Unlocked(AID.LandscapeMotif) && StarryMuseLeft == 0)
                PushGCD(AID.LandscapeMotif, Player, GCDPriority.Standard);
        }

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining <= GetCastTime(AID.RainbowDrip) + GetApplicationDelay(AID.RainbowDrip))
                PushGCD(AID.RainbowDrip, primaryTarget, GCDPriority.Standard);

            if (CountdownRemaining <= GetCastTime(AID.FireInRed) + GetApplicationDelay(AID.FireInRed))
                PushGCD(AID.FireInRed, primaryTarget, GCDPriority.Standard);

            return;
        }

        GoalZoneSingle(25);

        if (Player.InCombat && World.Actors.FirstOrDefault(x => x.OID is LeylinesOID && x.OwnerID == Player.InstanceID) is Actor ll)
            Hints.GoalZones.Add(p => p.InCircle(ll.Position, 8) ? 0.5f : 0);

        //if (!Player.InCombat && primaryTarget != null && Paint == 0)
        //    PushGCD(AID.RainbowDrip, primaryTarget, GCDPriority.Standard);

        if (Player.InCombat && primaryTarget != null)
        {
            if (ShouldWeapon(strategy))
                PushOGCD(AID.StrikingMuse, Player);

            if (ShouldCreatureMuse(strategy))
                PushOGCD(BestLivingMuse, BestAOETarget);

            if (ShouldLandscape(strategy))
                PushOGCD(AID.StarryMuse, Player, 2);

            if (ShouldSubtract(strategy))
                PushOGCD(AID.SubtractivePalette, Player);

            if (ShouldCreaturePortrait(strategy))
                PushOGCD(BestPortrait, BestLineTarget);

            if (MP <= Player.HPMP.MaxMP * 0.7f)
                PushOGCD(AID.LucidDreaming, Player);
        }

        if (DowntimeIn > 0 && !CanFitGCD(DowntimeIn - GetApplicationDelay(AID.RainbowDrip), 1))
        {
            PushOGCD(AID.Swiftcast, Player, 50);

            if (SwiftcastLeft > GCD)
                PushGCD(AID.RainbowDrip, primaryTarget, GCDPriority.High);
        }

        if (Starstruck > GCD)
            PushGCD(AID.StarPrism, BestAOETarget, GCDPriority.Standard);

        if (RainbowBright > GCD)
            PushGCD(AID.RainbowDrip, BestLineTarget, GCDPriority.Standard);

        var shouldWing = ShouldPaintInOpener(strategy);

        // hardcasting wing motif is #1 prio in opener
        if (shouldWing)
            PushGCD(AID.CreatureMotif, Player, GCDPriority.Standard);

        Hammer(strategy);
        Holy(strategy);

        int aoeBreakpoint;

        if (Unlocked(TraitID.EnhancedPictomancyIV))
            aoeBreakpoint = 4;
        else if (Subtractive > 0 || ah2 > GCD)
            aoeBreakpoint = 3;
        else
            aoeBreakpoint = 4;

        if (NumAOETargets >= aoeBreakpoint && Unlocked(AID.FireIIInRed))
        {
            if (Subtractive > 0)
                PushGCD(AID.BlizzardIIInCyan, BestAOETarget, GCDPriority.Standard);
            else
                PushGCD(AID.FireIIInRed, BestAOETarget, GCDPriority.Standard);
        }
        else
        {
            if (Subtractive > 0)
                PushGCD(AID.BlizzardInCyan, primaryTarget, GCDPriority.Standard);
            else
                PushGCD(AID.FireInRed, primaryTarget, GCDPriority.Standard);
        }
    }

    private bool IsMotifOk(StrategyValues strategy)
    {
        if (!Player.InCombat)
            return true;

        if (Utils.IsNonBossFate(World.Client.ActiveFate.ID))
            return !Player.InCombat;

        // spend buffs instead of casting motifs
        if (Hyperphantasia > 0 || SpectrumLeft > GCD || RainbowBright > GCD || Starstruck > GCD)
            return false;

        return strategy.Option(Track.Motif).As<MotifStrategy>() switch
        {
            MotifStrategy.Downtime => !Hints.PriorityTargets.Any(),
            MotifStrategy.Combat => RaidBuffsLeft == 0,
            _ => false
        };
    }

    // only relevant during opener
    private bool ShouldPaintInOpener(StrategyValues strategy)
    {
        if (strategy.Option(Track.Motif).As<MotifStrategy>() != MotifStrategy.Combat)
            return false;

        return !CreaturePainted
            && BestPortrait == AID.None
            && (CreatureFlags.HasFlag(CreatureFlags.Pom) || CreatureFlags.HasFlag(CreatureFlags.Claw))
            && CanWeave(AID.LivingMuse, 0, extraFixedDelay: 4)
            && CanWeave(AID.MogOfTheAges, 5);
    }

    protected override float GetCastTime(AID aid) => aid switch
    {
        AID.LandscapeMotif or AID.WeaponMotif or AID.CreatureMotif => SwiftcastLeft > GCD || !Player.InCombat ? 0 : 3,
        AID.RainbowDrip => RainbowBright > GCD ? 0 : base.GetCastTime(aid),
        _ => base.GetCastTime(aid)
    };

    private void Hammer(StrategyValues strategy)
    {
        if (HammerTime.Stacks == 0 || strategy.Option(Track.Hammer).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return;

        var prio = GCDPriority.HammerMove;

        // hammer isnt that good in the opener anymore xddd
        //if (RaidBuffsLeft > GCD)
        //    prio = GCDPriority.Standard;

        // worst case scenario, give at least 8 extra seconds of leeway in case we want to cast both other motifs
        if (HammerTime.Left < GCD + GCDLength + (4 * HammerTime.Stacks - 1))
            prio = GCDPriority.High;

        // use to weave in opener, now that hammer sucks
        if (ShouldSubtract(strategy, 1))
            prio = GCDPriority.Standard;

        if (NumAOETargets > 1)
            prio = GCDPriority.Standard;

        PushGCD(AID.HammerStamp, BestAOETarget, prio);
    }

    private bool PaintOvercap => Paint == 5 && Hues == AetherHues.Two;

    private void Holy(StrategyValues strategy)
    {
        if (Paint == 0 || strategy.Option(Track.Holy).As<OffensiveStrategy>() == OffensiveStrategy.Delay)
            return;

        var prio = GCDPriority.HolyMove;

        // use to weave in opener
        if (ShouldSubtract(strategy, 1))
            prio = GCDPriority.Standard;
        if (CombatTimer < 10 && !CreatureFlags.HasFlag(CreatureFlags.Pom) && CanvasFlags.HasFlag(CanvasFlags.Pom) && CanWeave(AID.LivingMuse, 1))
            prio = GCDPriority.Standard;

        // use comet to prevent overcap or during buffs
        // regular holy can be overcapped without losing dps
        if (Monochrome && (PaintOvercap || RaidBuffsLeft > GCD))
            prio = GCDPriority.Standard;

        // holy always a gain in aoe
        if (NumAOETargets > 1)
            prio = GCDPriority.Standard;

        PushGCD(Monochrome ? AID.CometInBlack : AID.HolyInWhite, BestAOETarget, prio);
    }

    private bool ShouldWeapon(StrategyValues strategy)
    {
        // ensure muse alignment
        // ReadyIn will return float.max if not unlocked so no additional check needed
        return WeaponPainted && ReadyIn(AID.StarryMuse) is < 10 or > 60;
    }

    private bool ShouldLandscape(StrategyValues strategy, int gcdsAhead = 0)
    {
        if (!strategy.BuffsOk())
            return false;

        if (CombatTimer < 10 && !WingFangMuse)
            return false;

        return LandscapePainted && CanWeave(AID.StarryMuse, gcdsAhead);
    }

    private bool ShouldSubtract(StrategyValues strategy, int gcdsAhead = 0)
    {
        if (!CanWeave(AID.SubtractivePalette, gcdsAhead)
            || Subtractive > 0
            || ShouldLandscape(strategy, gcdsAhead)
            || Palette < 50 && SpectrumLeft == 0)
            return false;

        return Palette > 75 || RaidBuffsLeft > 0 || SpectrumLeft > 0;
    }

    private bool ShouldCreatureMuse(StrategyValues strategy)
    {
        if (BestLivingMuse is AID.WingedMuse or AID.FangedMuse)
            // prevent overcap
            return BestPortrait == AID.None;

        // otherwise should always be fine to use
        return true;
    }

    private bool ShouldCreaturePortrait(StrategyValues strategy)
    {
        return StarryMuseLeft > AnimLock;
    }
}
