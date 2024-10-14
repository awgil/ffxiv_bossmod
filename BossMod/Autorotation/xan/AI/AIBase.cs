using Lumina.Excel.GeneratedSheets;

namespace BossMod.Autorotation.xan;

public abstract class AIBase(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    internal bool Unlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));
    internal float NextChargeIn<AID>(AID aid) where AID : Enum => NextChargeIn(ActionID.MakeSpell(aid));
    internal float NextChargeIn(ActionID action) => ActionDefinitions.Instance[action]!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);

    internal static ActionID Spell<AID>(AID aid) where AID : Enum => ActionID.MakeSpell(aid);

    internal bool ShouldInterrupt(Actor act) => IsCastReactable(act) && act.CastInfo!.Interruptible;
    internal bool ShouldStun(Actor act) => IsCastReactable(act) && !act.CastInfo!.Interruptible && !IsBossFromIcon(act.OID);

    private static bool IsBossFromIcon(uint oid) => Service.LuminaRow<BNpcBase>(oid)?.Rank is 1 or 2 or 6;

    internal bool IsCastReactable(Actor act)
    {
        var castInfo = act.CastInfo;
        return !(castInfo == null || castInfo.TotalTime <= 1.5 || castInfo.EventHappened);
    }

    internal IEnumerable<AIHints.Enemy> EnemiesAutoingMe => Hints.PriorityTargets.Where(x => x.Actor.CastInfo == null && x.Actor.TargetID == Player.InstanceID && Player.DistanceToHitbox(x.Actor) <= 6);

    internal float HPRatio(Actor actor) => (float)actor.HPMP.CurHP / Player.HPMP.MaxHP;
    internal float HPRatio() => HPRatio(Player);

    internal uint PredictedHP(Actor actor) => (uint)Math.Clamp(actor.HPMP.CurHP + World.PendingEffects.PendingHPDifference(actor.InstanceID), 0, actor.HPMP.MaxHP);
    internal float PredictedHPRatio(Actor actor) => (float)PredictedHP(actor) / actor.HPMP.MaxHP;

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
    public static RotationModuleDefinition.ConfigRef<AbilityUse> AbilityTrack<Track>(this RotationModuleDefinition def, Track track, string name) where Track : Enum
    {
        return def.Define(track).As<AbilityUse>(name).AddOption(AbilityUse.Enabled, "Enabled").AddOption(AbilityUse.Disabled, "Disabled");
    }

    public static bool Enabled<Track>(this StrategyValues strategy, Track track) where Track : Enum
        => strategy.Option(track).As<AbilityUse>() == AbilityUse.Enabled;
}
