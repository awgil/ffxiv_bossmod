namespace BossMod.Autorotation.xan;

public abstract class AIBase(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    internal bool Unlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));
    internal float NextChargeIn<AID>(AID aid) where AID : Enum => NextChargeIn(ActionID.MakeSpell(aid));
    internal float NextChargeIn(ActionID action) => ActionDefinitions.Instance[action]!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);

    internal static ActionID Spell<AID>(AID aid) where AID : Enum => ActionID.MakeSpell(aid);

    // note "in combat" check here, as deep dungeon enemies can randomly cast interruptible spells out of combat - interjecting causes aggro
    internal bool ShouldInterrupt(AIHints.Enemy e) => e.Actor.InCombat /* && e.ShouldBeInterrupted */ && (e.Actor.CastInfo?.Interruptible ?? false);
    internal bool ShouldStun(AIHints.Enemy e) => e.Actor.InCombat && e.ShouldBeStunned;

    internal IEnumerable<AIHints.Enemy> EnemiesAutoingMe => Hints.PriorityTargets.Where(x => x.Actor.CastInfo == null && x.Actor.TargetID == Player.InstanceID && Player.DistanceToHitbox(x.Actor) <= 6);

    internal IEnumerable<DateTime> Raidwides => Hints.PredictedDamage.Where(d => World.Party.WithSlot(excludeAlliance: true).IncludedInMask(d.players).Count() >= 2).Select(t => t.activation);
    internal IEnumerable<(Actor, DateTime)> Tankbusters
    {
        get
        {
            foreach (var d in Hints.PredictedDamage)
            {
                var targets = World.Party.WithSlot(excludeAlliance: true).IncludedInMask(d.players).GetEnumerator();
                targets.MoveNext();
                var target1 = targets.Current;
                if (targets.MoveNext())
                    continue;

                yield return (target1.Item2, d.activation);
            }
        }
    }
}

public enum AbilityUse
{
    Enabled,
    Disabled
}

internal static class AIExt
{
    public static RotationModuleDefinition.ConfigRef<AbilityUse> AbilityTrack<Track>(this RotationModuleDefinition def, Track track, string name, string display = "") where Track : Enum
    {
        return def.Define(track).As<AbilityUse>(name, display).AddOption(AbilityUse.Enabled, "Enabled").AddOption(AbilityUse.Disabled, "Disabled");
    }

    public static bool Enabled<Track>(this StrategyValues strategy, Track track) where Track : Enum
        => strategy.Option(track).As<AbilityUse>() == AbilityUse.Enabled;
}
