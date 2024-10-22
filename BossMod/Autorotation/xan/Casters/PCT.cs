using BossMod.PCT;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class PCT(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
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

        def.DefineSimple(Track.Holy, "Holy").AddAssociatedActions(AID.HolyInWhite);
        def.DefineSimple(Track.Hammer, "Hammer").AddAssociatedActions(AID.HammerStamp);

        return def;
    }

    public int Palette; // 0-100
    public int Paint; // 0-5
    public bool Creature;
    public bool Weapon;
    public bool Landscape;
    public bool Moogle;
    public bool Madeen;
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

    private Actor? BestAOETarget;
    private Actor? BestLineTarget;

    public enum GCDPriority : int
    {
        None = 0,
        HolyMove = 100,
        HammerMove = 200,
        Standard = 500,
    }

    private float GetApplicationDelay(AID action) => action switch
    {
        AID.RainbowDrip => 1.24f,
        AID.ClawedMuse => 0.98f,
        AID.FangedMuse => 1.16f,
        _ => 0
    };

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = World.Client.GetGauge<PictomancerGauge>();
        Palette = gauge.PalleteGauge;
        Paint = gauge.Paint;
        Creature = gauge.CreatureMotifDrawn;
        Weapon = gauge.WeaponMotifDrawn;
        Landscape = gauge.LandscapeMotifDrawn;
        Moogle = gauge.MooglePortraitReady;
        Madeen = gauge.MadeenPortraitReady;
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

        var motifOk = IsMotifOk(strategy);

        if (motifOk)
        {
            if (!Creature && Unlocked(AID.CreatureMotif))
                PushGCD(AID.CreatureMotif, Player, GCDPriority.Standard);

            if (!Weapon && Unlocked(AID.WeaponMotif) && HammerTime.Left == 0)
                PushGCD(AID.WeaponMotif, Player, GCDPriority.Standard);

            if (!Landscape && Unlocked(AID.LandscapeMotif) && StarryMuseLeft == 0)
                PushGCD(AID.LandscapeMotif, Player, GCDPriority.Standard);
        }

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining <= GetCastTime(AID.RainbowDrip))
                PushGCD(AID.RainbowDrip, primaryTarget, GCDPriority.Standard);

            if (CountdownRemaining <= GetCastTime(AID.FireInRed))
                PushGCD(AID.FireInRed, primaryTarget, GCDPriority.Standard);

            return;
        }

        if (!Player.InCombat && primaryTarget != null && Paint == 0)
            PushGCD(AID.RainbowDrip, primaryTarget, GCDPriority.Standard);

        if (Player.InCombat && primaryTarget != null)
        {
            if (ShouldWeapon(strategy))
                PushOGCD(AID.StrikingMuse, Player);

            if (CanvasFlags.HasFlag(CanvasFlags.Pom))
                PushOGCD(AID.PomMuse, BestAOETarget);

            if (ShouldLandscape(strategy))
                PushOGCD(AID.StarryMuse, Player, 2);

            if (ShouldSubtract(strategy))
                PushOGCD(AID.SubtractivePalette, Player);

            if (ShouldCreature(strategy))
                PushOGCD(AID.LivingMuse, BestAOETarget);

            if (ShouldMog(strategy))
                PushOGCD(AID.MogOfTheAges, BestLineTarget);

            if (Madeen)
                PushOGCD(AID.RetributionOfTheMadeen, BestLineTarget);

            if (Player.HPMP.CurMP <= 7000)
                PushOGCD(AID.LucidDreaming, Player);
        }

        if (!CanFitGCD(DowntimeIn - GetApplicationDelay(AID.RainbowDrip), 1))
        {
            PushOGCD(AID.Swiftcast, Player, 50);

            if (SwiftcastLeft > GCD)
                PushGCD(AID.RainbowDrip, primaryTarget, GCDPriority.Standard);
        }

        if (Starstruck > GCD)
            PushGCD(AID.StarPrism, BestAOETarget, GCDPriority.Standard);

        if (RainbowBright > GCD)
            PushGCD(AID.RainbowDrip, BestLineTarget, GCDPriority.Standard);

        var shouldWing = WingPlanned(strategy);

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
        if (Player.MountId > 0)
            return false;

        if (!Player.InCombat)
            return true;

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
    private bool WingPlanned(StrategyValues strategy)
    {
        if (strategy.Option(Track.Motif).As<MotifStrategy>() != MotifStrategy.Combat)
            return false;

        return PomOnly && !Creature && CanWeave(AID.LivingMuse, 0, extraFixedDelay: 4);
    }

    protected override float GetCastTime(AID aid) => aid switch
    {
        AID.LandscapeMotif or AID.WeaponMotif or AID.CreatureMotif => SwiftcastLeft > GCD || !Player.InCombat ? 0 : 3,
        AID.RainbowDrip => RainbowBright > GCD ? 0 : base.GetCastTime(aid),
        _ => base.GetCastTime(aid)
    };

    private void Hammer(StrategyValues strategy)
    {
        if (HammerTime.Stacks == 0)
            return;

        var prio = GCDPriority.HammerMove;

        if (RaidBuffsLeft > GCD)
            prio = GCDPriority.Standard;

        // worst case scenario, give at least 8 extra seconds of leeway in case we want to cast both other motifs
        if (HammerTime.Left < GCD + GCDLength + (4 * HammerTime.Stacks - 1))
            prio = GCDPriority.Standard;

        if (NumAOETargets > 1)
            prio = GCDPriority.Standard;

        PushGCD(AID.HammerStamp, BestAOETarget, prio);
    }

    private void Holy(StrategyValues strategy)
    {
        if (Paint == 0)
            return;

        var prio = GCDPriority.HolyMove;

        // use to weave in opener
        if (ShouldSubtract(strategy, 1))
            prio = GCDPriority.Standard;
        if (CombatTimer < 10 && !CreatureFlags.HasFlag(CreatureFlags.Pom))
            prio = GCDPriority.Standard;

        // use comet to prevent overcap or during buffs
        // regular holy can be overcapped without losing dps
        if (Monochrome && (Paint == 5 || RaidBuffsLeft > GCD))
            prio = GCDPriority.Standard;

        // holy always a gain in aoe
        if (NumAOETargets > 1)
            prio = GCDPriority.Standard;

        PushGCD(Monochrome ? AID.CometInBlack : AID.HolyInWhite, BestAOETarget, prio);
    }

    private bool PomOnly => CreatureFlags.HasFlag(CreatureFlags.Pom) && !CreatureFlags.HasFlag(CreatureFlags.Wings);

    private bool ShouldWeapon(StrategyValues strategy)
    {
        // ensure muse alignment
        // ReadyIn will return float.max if not unlocked so no additional check needed
        return Weapon && ReadyIn(AID.StarryMuse) is < 10 or > 60;
    }

    private bool ShouldCreature(StrategyValues strategy)
    {
        // triggers native autotarget if BestAOETarget is null because LivingMuse is self targeted and all the actual muse actions are not
        // TODO figure out buff timing, this code always just sends it
        return Creature && BestAOETarget != null;
    }

    private bool ShouldMog(StrategyValues strategy)
    {
        // ensure muse alignment - moogle takes two 40s charges to rebuild
        // TODO fix this for madeen, i think we swap between mog/madeen every 2min?
        return Moogle && (RaidBuffsLeft > 0 || ReadyIn(AID.StarryMuse) > 80);
    }

    private bool ShouldLandscape(StrategyValues strategy, int gcdsAhead = 0)
    {
        if (!strategy.BuffsOk())
            return false;

        if (CombatTimer < 10 && !CanvasFlags.HasFlag(CanvasFlags.Wing))
            return false;

        return Landscape && CanWeave(AID.StarryMuse, gcdsAhead);
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
}
