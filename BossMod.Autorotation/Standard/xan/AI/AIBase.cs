namespace BossMod.Autorotation.xan;

public abstract class AIBase<TValues>(RotationModuleManager manager, Actor player) : TypedRotationModule<TValues>(manager, player) where TValues : struct
{
    internal bool Unlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));
    internal float NextChargeIn<AID>(AID aid) where AID : Enum => NextChargeIn(ActionID.MakeSpell(aid));
    internal float NextChargeIn(ActionID action) => ActionDefinitions.Instance[action]!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);

    internal static ActionID Spell<AID>(AID aid) where AID : Enum => ActionID.MakeSpell(aid);

    // note "in combat" check here, as deep dungeon enemies can randomly cast interruptible spells out of combat - interjecting causes aggro
    internal bool ShouldInterrupt(AIHints.Enemy e) => e.Actor.InCombat && e.ShouldBeInterrupted && (e.Actor.CastInfo?.Interruptible ?? false);
    internal bool ShouldStun(AIHints.Enemy e) => e.Actor.InCombat && e.ShouldBeStunned;

    internal IEnumerable<AIHints.Enemy> EnemiesAutoingMe => Hints.PotentialTargets.Where(x => x.Actor.CastInfo == null && x.Actor.TargetID == Player.InstanceID && Player.DistanceToHitbox(x.Actor) <= 6);

    // predicted damage, plus raidwides / tankbusters only marked in the module's timeline
    internal IEnumerable<DateTime> Raidwides => StateTimeline.Raidwides(Bossmods.ActiveModule, World, Hints);
    internal IEnumerable<(Actor, DateTime)> Tankbusters => StateTimeline.Tankbusters(Bossmods.ActiveModule, World, Hints);
    internal bool RaidwideWithin(float seconds) => Raidwides.Any(t => t >= World.CurrentTime && t <= World.FutureTime(seconds));
}

public enum HintedStrategy
{
    [Option("Don't use")]
    Disabled,
    [Option("Use if the current module suggests it")]
    HintOnly,
    [Option("Always use on applicable targets")]
    Enabled
}

internal static class AIExt
{
    public static RotationModuleDefinition.ConfigRef<EnabledByDefault> AbilityTrack<Track>(this RotationModuleDefinition def, Track track, string name, string display = "", float uiPriority = 0) where Track : Enum
    {
        return def.Define(track).As<EnabledByDefault>(name, display, uiPriority, renderer: typeof(DefaultOnRenderer)).AddOption(EnabledByDefault.Enabled).AddOption(EnabledByDefault.Disabled);
    }

    public static bool Enabled<Track>(this StrategyValues strategy, Track track) where Track : Enum
        => strategy.Option(track).As<EnabledByDefault>().IsEnabled();

    public static bool HintEnabled<Track>(this StrategyValues strategy, Track track, bool hintValue) where Track : Enum
        => strategy.Option(track).As<HintedStrategy>() switch
        {
            HintedStrategy.HintOnly => hintValue,
            HintedStrategy.Enabled => true,
            _ => false
        };

    public static bool IsEnabled(this HintedStrategy s) => s != HintedStrategy.Disabled;
    public static bool Check(this HintedStrategy s, bool hintValue) => s switch
    {
        HintedStrategy.HintOnly => hintValue,
        HintedStrategy.Enabled => true,
        _ => false
    };
}
