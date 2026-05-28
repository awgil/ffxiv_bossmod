using BossMod.PCT;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using static BossMod.AIHints;

namespace BossMod.Autorotation.xan;

public sealed class PCT(RotationModuleManager manager, Actor player) : Castxan<AID, TraitID, PCT.Strategy>(manager, player, PotionType.Intelligence)
{
    public struct Strategy : IStrategyCommon
    {
        public Track<Targeting> Targeting;
        public Track<AOEStrategy> AOE;
        [Track("Starry Muse", Actions = [AID.StarryMuse, AID.StarrySkyMotif])]
        public Track<OffensiveStrategy> Buffs;

        [Track(Actions = [AID.MawMotif, AID.PomMotif, AID.ClawMotif, AID.WingMotif, AID.HammerMotif, AID.StarrySkyMotif])]
        public Track<MotifStrategy> Motifs;

        [Track(Actions = [AID.HolyInWhite, AID.CometInBlack])]
        public Track<HolyStrategy> Holy;

        [Track(Actions = [AID.HammerStamp, AID.HammerBrush, AID.PolishingHammer])]
        public Track<HammerStrategy> Hammer;

        [Track(Actions = [AID.PomMuse, AID.WingedMuse, AID.ClawedMuse, AID.FangedMuse])]
        public Track<MuseStrategy> Muse;

        [Track(Actions = [AID.MogOfTheAges, AID.RetributionOfTheMadeen])]
        public Track<PortraitStrategy> Portrait;

        readonly Targeting IStrategyCommon.Targeting => Targeting.Value;
        readonly AOEStrategy IStrategyCommon.AOE => AOE.Value;
    }

    public enum MotifStrategy
    {
        [Option("Cast motifs in combat, outside of burst window")]
        Combat,
        [Option("Cast motifs in combat if there are no nearby targets")]
        Downtime,
        [Option("Only cast motifs out of combat")]
        Instant
    }

    public enum HolyStrategy
    {
        [Option("Use for movement or for multiple targets; always use Comet in burst", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Do not use")]
        Delay,
        [Option("Use on primary target ASAP", Targets = ActionTargets.Hostile)]
        Force
    }

    public enum HammerStrategy
    {
        [Option("Use for movement, to weave in burst, or to hit multiple targets", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Do not use")]
        Delay,
        [Option("Use on primary target ASAP", Targets = ActionTargets.Hostile)]
        Force
    }

    public enum MuseStrategy
    {
        [Option("Use when available; save a charge for burst window", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Do not use")]
        Delay,
        [Option("Use on primary target ASAP (do not hold any charges)", Targets = ActionTargets.Hostile)]
        Force
    }

    public enum PortraitStrategy
    {
        [Option("Use portrait on given target when available; hold if burst window is imminent", Targets = ActionTargets.Hostile)]
        Automatic,
        [Option("Do not use portrait")]
        Delay,
        [Option("Use portrait on given target ASAP", Targets = ActionTargets.Hostile)]
        Force
    }

    public static RotationModuleDefinition Definition()
    {
        return new RotationModuleDefinition("xan PCT", "Pictomancer", "Standard rotation (xan)|Casters", "xan", RotationModuleQuality.Basic, BitMask.Build(Class.PCT), 100).WithStrategies<Strategy>();
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

    private Enemy? MuseTarget;
    private Enemy? PortraitTarget;
    private Enemy? HammerTarget;
    private Enemy? HolyTarget;

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

    public override void Exec(in Strategy strategy, Enemy? primaryTarget)
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

        MuseTarget = ResolveTargetOverride(strategy.Muse) ?? BestAOETarget;
        PortraitTarget = ResolveTargetOverride(strategy.Portrait) ?? BestLineTarget;
        HammerTarget = ResolveTargetOverride(strategy.Hammer) ?? BestAOETarget;
        HolyTarget = ResolveTargetOverride(strategy.Holy) ?? BestAOETarget;

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

        if (Player.InCombat && primaryTarget != null)
        {
            if (ShouldWeapon(strategy))
                PushOGCD(AID.StrikingMuse, Player);

            if (ShouldCreatureMuse(strategy))
                PushOGCD(BestLivingMuse, MuseTarget);

            if (ShouldLandscape(strategy))
                PushOGCD(AID.StarryMuse, Player, 2);

            if (ShouldSubtract(strategy))
                PushOGCD(AID.SubtractivePalette, Player);

            if (ShouldCreaturePortrait(strategy))
                PushOGCD(BestPortrait, PortraitTarget);

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

    private bool IsMotifOk(in Strategy strategy)
    {
        if (!Player.InCombat)
            return true;

        if (Utils.IsNonBossFate(World.Client.ActiveFate.ID) || World.DeepDungeon.DungeonId > 0 && !World.DeepDungeon.IsBossFloor)
            return !Player.InCombat;

        // spend buffs instead of casting motifs
        if (Hyperphantasia > 0 || SpectrumLeft > GCD || RainbowBright > GCD || Starstruck > GCD)
            return false;

        return strategy.Motifs.Value switch
        {
            MotifStrategy.Downtime => !Hints.PriorityTargets.Any(),
            MotifStrategy.Combat => RaidBuffsLeft == 0,
            _ => false
        };
    }

    // only relevant during opener
    private bool ShouldPaintInOpener(in Strategy strategy)
    {
        if (strategy.Motifs != MotifStrategy.Combat)
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

    private void Hammer(in Strategy strategy)
    {
        if (HammerTime.Stacks == 0 || strategy.Hammer == HammerStrategy.Delay)
            return;

        var prio = GCDPriority.HammerMove;

        // forced usage
        if (strategy.Hammer == HammerStrategy.Force)
            prio = GCDPriority.High;

        // worst case scenario, give at least 8 extra seconds of leeway in case we want to cast both other motifs
        if (HammerTime.Left < GCD + GCDLength + (4 * HammerTime.Stacks - 1))
            prio = GCDPriority.High;

        // use to weave in opener, now that hammer sucks
        if (ShouldSubtract(strategy, 1))
            prio = GCDPriority.Standard;

        if (NumAOETargets > 1)
            prio = GCDPriority.Standard;

        PushGCD(AID.HammerStamp, HammerTarget, prio);
    }

    private bool PaintOvercap => Paint == 5 && Hues == AetherHues.Two;

    private void Holy(in Strategy strategy)
    {
        if (Paint == 0 || strategy.Holy == HolyStrategy.Delay)
            return;

        var prio = GCDPriority.HolyMove;

        // forced
        if (strategy.Holy == HolyStrategy.Force)
            prio = GCDPriority.High;

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

        PushGCD(Monochrome ? AID.CometInBlack : AID.HolyInWhite, HolyTarget, prio);
    }

    private bool ShouldWeapon(in Strategy strategy)
    {
        // ensure muse alignment
        // ReadyIn will return float.max if not unlocked so no additional check needed
        return WeaponPainted && ReadyIn(AID.StarryMuse) is < 10 or > 60;
    }

    private bool ShouldLandscape(in Strategy strategy, int gcdsAhead = 0)
    {
        if (!LandscapePainted)
            return false;

        switch (strategy.Buffs.Value)
        {
            case OffensiveStrategy.Delay:
                return false;
            case OffensiveStrategy.Force:
                return true;
        }

        if (CombatTimer < 10 && !WingFangMuse)
            return false;

        return CanWeave(AID.StarryMuse, gcdsAhead);
    }

    private bool ShouldSubtract(in Strategy strategy, int gcdsAhead = 0)
    {
        if (!CanWeave(AID.SubtractivePalette, gcdsAhead)
            || Subtractive > 0
            || ShouldLandscape(strategy, gcdsAhead)
            || Palette < 50 && SpectrumLeft == 0)
            return false;

        return Palette > 75 || RaidBuffsLeft > 0 || SpectrumLeft > 0;
    }

    private bool ShouldCreatureMuse(in Strategy strategy)
    {
        switch (strategy.Muse.Value)
        {
            case MuseStrategy.Force:
                return true;
            case MuseStrategy.Delay:
                return false;
        }

        // no overcap pls
        if (CanWeave(MaxChargesIn(AID.LivingMuse), 0.6f, 1))
            return true;

        // dont overwrite
        if (BestLivingMuse is AID.WingedMuse or AID.FangedMuse && BestPortrait != AID.None)
            return false;

        // no buff window to play around
        if (!Unlocked(AID.StarryMuse))
            return true;

        // use during buffs
        if (RaidBuffsLeft > AnimLock)
            return true;

        // canvas is empty, we should prep a painting for next burst window
        if (BestLivingMuse is AID.PomMuse or AID.ClawedMuse)
            return true;

        return false;
    }

    private bool ShouldCreaturePortrait(in Strategy strategy) => strategy.Portrait.Value switch
    {
        PortraitStrategy.Force => true,
        // figure out math
        PortraitStrategy.Automatic => StarryMuseLeft > AnimLock || !Unlocked(AID.StarryMuse) /* || ReadyIn(AID.StarryMuse) > 20 */,
        _ => false
    };
}
