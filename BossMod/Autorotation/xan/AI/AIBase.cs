using Lumina.Excel.GeneratedSheets;

namespace BossMod.Autorotation.xan;

public abstract class AIBase(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    internal bool Unlocked<AID>(AID aid) where AID : Enum => ActionUnlocked(ActionID.MakeSpell(aid));
    internal float Cooldown<AID>(AID aid) where AID : Enum => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    internal bool ShouldInterrupt(Actor act) => IsCastReactable(act) && act.CastInfo!.Interruptible;
    internal bool ShouldStun(Actor act) => IsCastReactable(act) && !act.CastInfo!.Interruptible && !IsBossFromIcon(act.OID);

    private static bool IsBossFromIcon(uint oid) => Service.LuminaRow<BNpcBase>(oid)?.Rank is 1 or 2 or 6;

    internal bool IsCastReactable(Actor act)
    {
        var castInfo = act.CastInfo;
        if (castInfo == null || castInfo.TotalTime <= 1.5 || castInfo.EventHappened)
            return false;

        return castInfo.NPCTotalTime - castInfo.NPCRemainingTime > 1;
    }

    internal IEnumerable<AIHints.Enemy> EnemiesAutoingMe => Hints.PriorityTargets.Where(x => x.Actor.CastInfo == null && x.Actor.TargetID == Player.InstanceID && Player.DistanceToHitbox(x.Actor) <= 6);

    internal float HPRatio => (float)Player.HPMP.CurHP / Player.HPMP.MaxHP;
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
