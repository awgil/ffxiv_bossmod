using BossMod.Autorotation.xan;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace BossMod.Autorotation.MiscAI;

public sealed class AutoTarget(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { General, Retarget, QuestBattle, DeepDungeon, EpicEcho, Hunt, FATE, Everything, CollectFATE, Treasure, MaxTargets, Zodiac }
    public enum GeneralStrategy { Aggressive, Passive }
    public enum RetargetStrategy { NoTarget, Hostiles, Always, Never }
    public enum Flag { Disabled, Enabled }

    public static RotationModuleDefinition Definition()
    {
        RotationModuleDefinition res = new("Automatic targeting", "Collection of utilities to automatically target and pull mobs based on different criteria.", "AI", "veyn", RotationModuleQuality.Basic, new(~0ul), 1000, 1, RotationModuleOrder.HighLevel, CanUseWhileRoleplaying: true, PvP: PvPCompatibility.Any);

        res.Define(Track.General).As<GeneralStrategy>("General")
            .AddOption(GeneralStrategy.Aggressive, "Automatically prioritize targets", supportedTargets: ActionTargets.Hostile)
            .AddOption(GeneralStrategy.Passive, "Do nothing");

        res.Define(Track.Retarget).As<RetargetStrategy>("Retarget")
            .AddOption(RetargetStrategy.NoTarget, "Only switch target if player has no target")
            .AddOption(RetargetStrategy.Hostiles, "Only switch target if player is not targeting an ally")
            .AddOption(RetargetStrategy.Always, "Always switch target to the highest priority enemy")
            .AddOption(RetargetStrategy.Never, "Never switch target; only apply priority changes to enemies");

        res.Define(Track.QuestBattle).As<Flag>("QuestBattle", "Prioritize bosses in quest battles", renderer: typeof(DefaultOffRenderer), uiPriority: -50)
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.DeepDungeon).As<Flag>("DD", "Prioritize deep dungeon bosses (solo only)", renderer: typeof(DefaultOffRenderer), uiPriority: -60)
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.EpicEcho).As<Flag>("EE", "Prioritize all targets in unsynced duties", renderer: typeof(DefaultOffRenderer), uiPriority: -70)
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.Hunt).As<Flag>("Hunt", "Prioritize hunt marks once they have been pulled", renderer: typeof(DefaultOffRenderer), uiPriority: -80)
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.FATE).As<Flag>("FATE", "Prioritize mobs in the current FATE", renderer: typeof(DefaultOffRenderer), uiPriority: -90)
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.Everything).As<Flag>("Everything", "Prioritize EVERYTHING", renderer: typeof(DefaultOffRenderer), uiPriority: -100)
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.CollectFATE).As<Flag>("CollectFATE", "Ignore passive mobs in hand-in FATEs", renderer: typeof(DefaultOffRenderer), uiPriority: -110)
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.Define(Track.Treasure).As<Flag>("Treasure", "Open treasure chests", renderer: typeof(DefaultOffRenderer))
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        res.DefineInt(Track.MaxTargets, "Maximum targets to pull (0 = no max)", minValue: 0, maxValue: 30, uiPriority: -120);

        res.Define(Track.Zodiac).As<Flag>("Zodiac", "Prioritize mobs in the current Zodiac Book", renderer: typeof(DefaultOffRenderer), uiPriority: -95)
            .AddOption(Flag.Disabled)
            .AddOption(Flag.Enabled);

        return res;
    }

    // all targets closer than this many units to the player are considered to have the same priority
    // we use "is this the player's current target?" as a tiebreaker
    // due to the way goalzones work for jobs with weirdly shaped AOEs (cone, rect, etc), AI tends to move closer to a mob that isn't its primary target, and without a threshold, that results in switching target rapidly (sometimes every frame)
    public const float MinPriorityDistance = 3;

    record struct TargetKey(bool ShouldTarget, int Priority, float InvDistance, bool IsCurrentTarget) : IComparable<TargetKey>
    {
        public readonly int CompareTo(TargetKey other)
        {
            if (ShouldTarget.CompareTo(other.ShouldTarget) is var i && i != 0)
                return i;
            if (Priority.CompareTo(other.Priority) is var j && j != 0)
                return j;
            if (InvDistance.CompareTo(other.InvDistance) is var k && k != 0)
                return k;
            return IsCurrentTarget.CompareTo(other.IsCurrentTarget);
        }

        public static TargetKey Create(AIHints.Enemy enemy, Actor player)
        {
            return new(enemy.ShouldBeTargeted, enemy.Priority, -Math.Max(MinPriorityDistance, player.DistanceToHitbox(enemy.Actor)), player.TargetID == enemy.Actor.InstanceID);
        }

        public static bool operator <(TargetKey left, TargetKey right) => left.CompareTo(right) < 0;
        public static bool operator <=(TargetKey left, TargetKey right) => left.CompareTo(right) <= 0;
        public static bool operator >(TargetKey left, TargetKey right) => left.CompareTo(right) > 0;
        public static bool operator >=(TargetKey left, TargetKey right) => left.CompareTo(right) >= 0;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (strategy.Option(Track.Treasure).As<Flag>() == Flag.Enabled)
            Hints.InteractWithTarget ??= World.Actors.Where(a => a.Type == ActorType.Treasure && a.IsTargetable && !a.IsOpenTreasure).OrderBy(a => (a.Position - Player.Position).LengthSq()).FirstOrDefault();

        var generalOpt = strategy.Option(Track.General);
        var generalStrategy = generalOpt.As<GeneralStrategy>();
        if (generalStrategy == GeneralStrategy.Passive)
            return;

        var maxTargets = strategy.GetInt(Track.MaxTargets);
        var canPullMore = maxTargets == 0 || World.Actors.Count(a => a.AggroPlayer && !a.IsDead) < maxTargets;

        var currentTargetId = primaryTarget?.InstanceID ?? 0;

        Actor? bestTarget = null; // non-null if we bump any priorities
        var bestTargetKey = new TargetKey(false, 0, float.MinValue, false);
        void prioritize(AIHints.Enemy e, int prio)
        {
            e.Priority = prio;

            var key = TargetKey.Create(e, Player);
            if (key.CompareTo(bestTargetKey) > 0)
            {
                bestTarget = e.Actor;
                bestTargetKey = key;
            }
        }

        var allowAll = strategy.Option(Track.Everything).As<Flag>() == Flag.Enabled;

        if (strategy.Option(Track.QuestBattle).As<Flag>() == Flag.Enabled)
            allowAll |= Bossmods.LoadedModules is [{ Info.Category: BossModuleInfo.Category.Quest }];

        if (strategy.Option(Track.DeepDungeon).As<Flag>() == Flag.Enabled && World.Party.WithoutSlot(includeDead: true, excludeNPCs: true).Count() == 1)
            allowAll |= Bossmods.LoadedModules is [{ Info.Category: BossModuleInfo.Category.DeepDungeon }];

        if (strategy.Option(Track.EpicEcho).As<Flag>() == Flag.Enabled)
            allowAll |= Utils.IsUnsynced(World, Player);

        ulong huntTarget = 0;

        if (strategy.Option(Track.Hunt).As<Flag>() == Flag.Enabled && Bossmods.ActiveModule is
            {
                Info.Category: BossModuleInfo.Category.Hunt,
                PrimaryActor:
                {
                    InCombat: true,
                    HPRatio: <= 0.95f,
                    InstanceID: var i
                }
            }
        )
            huntTarget = i;

        var targetFates = strategy.Option(Track.FATE).As<Flag>() == Flag.Enabled && Utils.IsPlayerSyncedToFate(World);
        var targetFateMobs = World.Client.ActiveFate.Progress < 100;

        var turnin = Utils.GetFateItem(World.Client.ActiveFate.ID);
        if (turnin > 0)
        {
            if (strategy.Option(Track.CollectFATE).As<Flag>() == Flag.Enabled)
                targetFateMobs = false;
            else
                // keep targeting mobs until we have enough turnin items (unless we are holding 10, in which case FateUtils is probably trying to perform turnin, let's not interrupt it)
                targetFateMobs |= World.Client.ActiveFate.HandInCount < FateUtils.TurnInGoldReq && World.Client.GetInventoryItemQuantity(turnin) < FateUtils.TurnInGoldReq;
        }

        var targetZodiac = strategy.Option(Track.Zodiac).As<Flag>() == Flag.Enabled;

        // first deal with pulling new enemies
        foreach (var target in Hints.PotentialTargets)
        {
            if (target.Actor.InstanceID == huntTarget)
            {
                prioritize(target, 0);
                continue;
            }

            if (canPullMore && allowAll && !target.Actor.IsStrikingDummy && target.Priority == AIHints.Enemy.PriorityUndesirable)
            {
                prioritize(target, 0);
                continue;
            }

            if (targetFates && target.Actor.FateID == World.Client.ActiveFate.ID)
            {
                if (target.Actor.NameID is 6737 or 6738)
                {
                    prioritize(target, 1);
                    continue;
                }
                if (targetFateMobs && canPullMore)
                {
                    prioritize(target, 0);
                    continue;
                }
            }

            if (targetZodiac && IsRelicTarget(target.Actor))
            {
                prioritize(target, 0);
                continue;
            }

            // add all other targets to potential targets list (e.g. if modules modify out-of-combat mob priority)
            if (target.Priority >= 0)
                prioritize(target, target.Priority);
        }

        // prioritizer yielded no results meaning there are no targets to pick, do nothing
        if (bestTarget == null)
            return;

        Hints.PotentialTargets.SortByReverse(x => x.Priority);
        Hints.HighestPotentialTargetPriority = Math.Max(0, Hints.PotentialTargets[0].Priority);

        var retargetStrategy = strategy.Option(Track.Retarget).As<RetargetStrategy>();
        if (retargetStrategy == RetargetStrategy.Never)
            return;

        var currentTarget = World.Actors.Find(Player.TargetID);

        var changeTarget = retargetStrategy switch
        {
            RetargetStrategy.Hostiles => currentTarget == null || !currentTarget.IsAlly,
            RetargetStrategy.NoTarget => currentTarget == null,
            _ => true
        };

        // if we have target to switch to, do that
        if (changeTarget)
            primaryTarget = Hints.ForcedTarget = bestTarget;
    }

    // TODO: this shouldn't be here
    private unsafe bool IsRelicTarget(Actor a)
    {
        if (Service.IsMock)
            return false;

        // leve targets xDDDD
        var obj = GameObjectManager.Instance()->Objects.IndexSorted[a.SpawnIndex];
        if (obj != null && obj.Value->NamePlateIconId == 71244)
            return true;

        var mgr = FFXIVClientStructs.FFXIV.Client.Game.UI.RelicNote.Instance();
        if (Service.LuminaRow<Lumina.Excel.Sheets.RelicNote>(mgr->RelicNoteId) is not { } book)
            return false;

        if (book.Fate[0].RowId == 0)
            return false;

        var i = 0;
        foreach (var mon in book.MonsterNoteTargetCommon)
        {
            var monster = mon.Value;
            if (mgr->GetMonsterProgress(i) < 3 && a.NameID == monster.BNpcName.RowId)
                return true;
            i++;
        }

        return false;
    }
}
