using BossMod.PCT;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation.xan;

public sealed class PCT(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID>(manager, player)
{
    public enum Track { Motif = SharedTrack.Count, Holy, Hammer }
    public enum MotifStrategy { Combat, Downtime, Instant }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("xan PCT", "Pictomancer", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PCT), 100);

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

    private bool WingPlanned => PomOnly && !Creature && CD(AID.LivingMuse) - 80 < GCD + 4;

    public override void Exec(StrategyValues strategy, Actor? primaryTarget)
    {
        SelectPrimaryTarget(strategy, ref primaryTarget, 25);

        var gauge = GetGauge<PictomancerGauge>();
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
                PushGCD(AID.CreatureMotif, Player);

            if (!Weapon && Unlocked(AID.WeaponMotif) && HammerTime.Left == 0)
                PushGCD(AID.WeaponMotif, Player);

            if (!Landscape && Unlocked(AID.LandscapeMotif) && StarryMuseLeft == 0)
                PushGCD(AID.LandscapeMotif, Player);
        }

        if (CountdownRemaining > 0)
        {
            if (CountdownRemaining <= GetCastTime(AID.RainbowDrip))
                PushGCD(AID.RainbowDrip, primaryTarget);

            return;
        }

        if (Player.InCombat && primaryTarget != null)
        {
            if (ShouldWeapon(strategy))
                PushOGCD(AID.SteelMuse, Player);

            if (CanvasFlags.HasFlag(CanvasFlags.Pom))
                PushOGCD(AID.PomMuse, BestAOETarget);

            if (ShouldLandscape(strategy))
                PushOGCD(AID.StarryMuse, Player);

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

        if (Starstruck > GCD)
            PushGCD(AID.StarPrism, BestAOETarget);

        if (RainbowBright > GCD)
            PushGCD(AID.RainbowDrip, BestLineTarget);

        // hardcasting wing motif is #1 prio in opener
        if (WingPlanned)
            PushGCD(AID.CreatureMotif, Player);

        if (ShouldHammer(strategy))
            PushGCD(AID.HammerStamp, BestAOETarget);

        if (ShouldHoly(strategy))
            PushGCD(Monochrome ? AID.CometInBlack : AID.HolyInWhite, BestAOETarget);

        if (NumAOETargets > 3 && Unlocked(AID.FireIIInRed))
        {
            if (Subtractive > 0)
                PushGCD(AID.BlizzardIIInCyan, BestAOETarget);

            PushGCD(AID.FireIIInRed, BestAOETarget);
        }
        else
        {
            if (Subtractive > 0)
                PushGCD(AID.BlizzardInCyan, primaryTarget);

            PushGCD(AID.FireInRed, primaryTarget);
        }
    }

    private bool IsMotifOk(StrategyValues strategy)
    {
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

    protected override float GetCastTime(AID aid) => aid switch
    {
        AID.LandscapeMotif or AID.WeaponMotif or AID.CreatureMotif => SwiftcastLeft > GCD || !Player.InCombat ? 0 : 3,
        AID.RainbowDrip => RainbowBright > GCD ? 0 : base.GetCastTime(aid),
        _ => base.GetCastTime(aid)
    };

    private bool ShouldHoly(StrategyValues strategy)
    {
        if (Paint == 0)
            return false;

        // use for movement, or to weave raid buff at fight start
        if (ForceMovementIn == 0 || ShouldSubtract(strategy, 1))
            return true;

        if (CombatTimer < 10 && !CreatureFlags.HasFlag(CreatureFlags.Pom))
            return true;

        // use comet to prevent overcap or during buffs
        // (we don't use regular holy to prevent overcap, it's a single target dps loss)
        if (Monochrome && (Paint == 5 || RaidBuffsLeft > 0))
            return true;

        return false;
    }

    private bool ShouldHammer(StrategyValues strategy) => HammerTime.Stacks > 0 &&
         (RaidBuffsLeft > GCD
             || ForceMovementIn == 0
             // set to 4s instead of GCD timer in case we end up wanting to hardcast all 3 motifs
             || HammerTime.Left < GCD + 4 * HammerTime.Stacks);

    private bool PomOnly => CreatureFlags.HasFlag(CreatureFlags.Pom) && !CreatureFlags.HasFlag(CreatureFlags.Wings);

    private bool ShouldWeapon(StrategyValues strategy)
    {
        // ensure muse alignment
        return Weapon && (!Unlocked(AID.StarryMuse) || CD(AID.StarryMuse) is < 10 or > 60);
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
        return Moogle && (!Unlocked(AID.StarryMuse) || RaidBuffsLeft > 0 || CD(AID.StarryMuse) > 80);
    }

    private bool ShouldLandscape(StrategyValues strategy, int gcdsAhead = 0)
    {
        if (!strategy.BuffsOk())
            return false;

        if (CombatTimer < 10 && !CanvasFlags.HasFlag(CanvasFlags.Wing))
            return false;

        return Landscape && CanWeave(AID.ScenicMuse, gcdsAhead);
    }

    private bool ShouldSubtract(StrategyValues strategy, int gcdsAhead = 0)
    {
        if (!Unlocked(AID.SubtractivePalette)
            || !CanWeave(AID.SubtractivePalette, gcdsAhead)
            || Subtractive > 0
            || ShouldLandscape(strategy, gcdsAhead)
            || Palette < 50 && SpectrumLeft == 0)
            return false;

        return Palette > 75 || RaidBuffsLeft > 0 || SpectrumLeft > 0;
    }
}
