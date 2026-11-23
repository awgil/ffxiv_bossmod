using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace BossMod.ActionTweaks.CustomCombos;

public sealed record class CustomComboContext(
    Func<uint, uint> AdjustOriginal, // unhooked GetAdjustedActionId
    Func<uint, uint> ActionStatus, // action status, ignoring recast & target; 0 means usable
    Func<bool?, ActionQueue.Entry> Recommend); // private autorot suggestion (null = any, true = gcd, false = ogcd)

// note: all comparisons use base actions (see CustomComboData.Canonical), so trait upgrades behave like their base action
public sealed class CustomComboTweak : IDisposable
{
    private static readonly uint[] UsableStatuses = [0, 580, 582]; // from WrathCombo
    private const float WeaveAnimLock = 0.6f;
    private const float QueueWindow = 0.5f;

    private readonly WorldState _ws;
    private readonly AIHints _hints;
    private readonly BossModuleManager _bossmods;
    private readonly AutorotationSuggestions _suggestions;
    private readonly MovementOverride _movement;
    private readonly CustomComboConfig _config;
    private CustomComboContext _ctx = new(a => a, _ => 0, _ => default); // until connected, nothing is replaced anyway
    private readonly Dictionary<uint, (uint Action, Actor? Target)> _cache = []; // trigger -> replacement and its target, valid for a single frame
    private readonly Dictionary<uint, ActionQueue.Entry> _recommendations = []; // valid for a single frame
    private uint _cacheFrame = uint.MaxValue;
    private readonly HashSet<nint> _modifiedSlots = []; // hotbar slots whose apparent action that's overridden
    private readonly Dictionary<uint, DateTime> _lastUsed = []; // base action -> last time player used it
    private readonly EventSubscription _castSubscription;
    private DateTime _combatStart;

    public bool Enabled => _config.Enabled;

    public CustomComboTweak(WorldState ws, AIHints hints, BossModuleManager bossmods, AutorotationSuggestions suggestions, MovementOverride movement, ConfigRoot configRoot)
    {
        _ws = ws;
        _hints = hints;
        _bossmods = bossmods;
        _suggestions = suggestions;
        _movement = movement;
        _config = configRoot.Get<CustomComboConfig>();
        _castSubscription = ws.Actors.CastEvent.Subscribe(OnCastEvent);
    }

    public void Dispose() => _castSubscription.Dispose();

    public void Connect(CustomComboContext ctx) => _ctx = ctx;

    // returns replacement spell id (before game adjustment), or 0 if action should not be replaced
    public uint Resolve(uint trigger) => ResolveWithTarget(trigger).Action;

    private (uint Action, Actor? Target) ResolveWithTarget(uint trigger)
    {
        if (!_config.Enabled || trigger == 0)
            return default;

        UpdateFrameCache();
        if (_cache.TryGetValue(trigger, out var cached))
            return cached;

        _cache[trigger] = default;
        var res = Evaluate(trigger);
        _cache[trigger] = res;
        return res;
    }

    // returns adjusted replacement, or action itself; target is set for autorotation suggestions
    public uint Apply(uint actionId, out Actor? target)
    {
        var (repl, replTarget) = ResolveWithTarget(actionId);
        target = repl != 0 ? replTarget : null;
        return repl != 0 ? _ctx.AdjustOriginal(repl) : actionId;
    }

    public bool WantsSuggestions(Actor player)
        => _config.Enabled && _config.Combos.Any(c => c.Enabled && (c.Job == Class.None || c.Job == player.Class) && c.Steps.Any(s => s.Enabled && CustomComboData.IsAutorotation(s.Action)));

    public List<Autorotation.Preset> SuggestionPresets(Class cls) => _suggestions.PresetsForClass(cls);
    public Autorotation.Preset? SuggestionPreset(Class cls) => _suggestions.ResolvePreset(_config.AutorotationPreset, cls);

    public uint PreviewRecommendation(uint autoAction)
    {
        var rec = Recommendation(autoAction);
        return rec.Action ? _ctx.AdjustOriginal(rec.Action.ID) : 0;
    }

    private void UpdateFrameCache()
    {
        if (_cacheFrame != _ws.Frame.Index)
        {
            _cache.Clear();
            _recommendations.Clear();
            _cacheFrame = _ws.Frame.Index;
        }
    }

    private ActionQueue.Entry Recommendation(uint autoAction)
    {
        UpdateFrameCache();
        if (!_recommendations.TryGetValue(autoAction, out var rec))
            _recommendations[autoAction] = rec = _ctx.Recommend(CustomComboData.AutorotationGCDFilter(autoAction));
        return rec;
    }

    public void Update()
    {
        var player = _ws.Party.Player();
        if (player?.InCombat ?? false)
        {
            if (_combatStart == default)
                _combatStart = _ws.CurrentTime;
        }
        else
        {
            _combatStart = default;
        }

        SyncHotbars(_config.Enabled);
    }

    public void RestoreHotbars() => SyncHotbars(false);

    public bool? EvaluateLive(CustomComboCondition c) => _ws.Party.Player() is { } player ? Check(player, c) : null;
    public bool? EvaluateLive(CustomComboStep s) => _ws.Party.Player() is { } player ? CheckStep(player, s) : null;

    // ignores enabled flags & class restrictions; 0 if there's no player
    public uint PreviewLive(CustomCombo combo)
    {
        if (_ws.Party.Player() is not { } player)
            return 0;
        var res = EvaluateCombo(player, combo).Action;
        return _ctx.AdjustOriginal(res != 0 ? res : CustomComboData.Canonical(combo.Trigger));
    }

    private void OnCastEvent(Actor actor, ActorCastEvent ev)
    {
        if (ev.Action.Type == ActionType.Spell && actor.InstanceID == _ws.Party.Player()?.InstanceID)
            _lastUsed[CustomComboData.Canonical(ev.Action.ID)] = _ws.CurrentTime;
    }

    // note: slots in 'adjusted action' mode query the GetAdjustedActionId hook every frame, so only other slots need their apparent action set
    private unsafe void SyncHotbars(bool wantIcons)
    {
        if (!wantIcons && _modifiedSlots.Count == 0)
            return;

        var module = RaptureHotbarModule.Instance();
        if (module == null)
            return;

        for (var i = 0; i < module->Hotbars.Length; ++i)
        {
            ref var hotbar = ref module->Hotbars[i];
            for (var j = 0u; j < 16; ++j)
            {
                var slot = hotbar.GetHotbarSlot(j);
                if (slot == null)
                    continue;

                var key = (nint)slot;
                var isAction = slot->CommandType == RaptureHotbarModule.HotbarSlotType.Action && slot->CommandId != 0;
                var gameRefreshes = slot->ApparentActionMode == HotbarSlotApparentActionMode.AdjustedAction;
                var repl = wantIcons && isAction && !gameRefreshes ? Resolve(slot->CommandId) : 0;
                if (repl != 0)
                {
                    var desired = _ctx.AdjustOriginal(repl);
                    if (slot->ApparentActionId != desired || slot->ApparentSlotType != RaptureHotbarModule.HotbarSlotType.Action)
                    {
                        slot->ApparentSlotType = RaptureHotbarModule.HotbarSlotType.Action;
                        slot->ApparentActionId = desired;
                        slot->LoadIconId();
                    }
                    _modifiedSlots.Add(key);
                }
                else if (_modifiedSlots.Remove(key) && isAction)
                {
                    // restore whatever the game would normally show
                    slot->ApparentSlotType = RaptureHotbarModule.HotbarSlotType.Action;
                    slot->ApparentActionId = _ctx.AdjustOriginal(slot->CommandId);
                    slot->LoadIconId();
                }
            }
        }
    }

    private (uint Action, Actor? Target) Evaluate(uint trigger)
    {
        var player = _ws.Party.Player();
        if (player == null)
            return default;

        var baseTrigger = CustomComboData.Canonical(trigger);
        foreach (var combo in _config.Combos)
        {
            if (!combo.Enabled || CustomComboData.Canonical(combo.Trigger) != baseTrigger || combo.Job != Class.None && combo.Job != player.Class)
                continue;
            var res = EvaluateCombo(player, combo);
            return res.Action == baseTrigger ? default : res; // first matching combo wins, even if nothing in it applies
        }
        return default;
    }

    // base action, exact action for autorotation suggestions (with target), or 0 if nothing applies
    private (uint Action, Actor? Target) EvaluateCombo(Actor player, CustomCombo combo)
    {
        foreach (var step in combo.Steps)
        {
            if (!step.Enabled)
                continue;
            if (CustomComboData.IsAutorotation(step.Action))
            {
                // conditions first, to avoid querying autorotation needlessly
                if (CheckStep(player, step) && Recommendation(step.Action) is var rec && rec.Action)
                    return (rec.Action.ID, rec.Target);
                continue;
            }
            var action = CustomComboData.Canonical(step.Action);
            if (action != 0 && IsUnlocked(player, action) && CheckStep(player, step))
                return (action, null);
        }
        return (NextInSequence(player, combo.Sequence), null);
    }

    // action after the last used combo action, or first action if combo isn't in progress
    private uint NextInSequence(Actor player, List<uint> sequence)
    {
        if (sequence.Count == 0)
            return 0;
        if (_ws.Client.ComboState.Remaining > 0)
        {
            var last = CustomComboData.Canonical(_ws.Client.ComboState.Action);
            // search from the end, so that repeated actions in a sequence advance as far as possible
            for (var i = sequence.Count - 1; i > 0; --i)
            {
                var action = CustomComboData.Canonical(sequence[i]);
                if (CustomComboData.Canonical(sequence[i - 1]) == last && action != 0 && IsUnlocked(player, action))
                    return action;
            }
        }
        var first = CustomComboData.Canonical(sequence[0]);
        return first != 0 && IsUnlocked(player, first) ? first : 0;
    }

    private bool IsUnlocked(Actor player, uint action) => ActionDefinitions.Instance[new(ActionType.Spell, action)]?.IsUnlocked(_ws, player) ?? true;

    private bool CheckStep(Actor player, CustomComboStep step) => CheckList(player, step.Conditions, step.Match);

    // empty lists pass, so that an override or group without conditions is 'always'
    private bool CheckList(Actor player, List<CustomComboCondition> list, CustomComboGroupMode mode)
        => list.Count == 0 || (mode == CustomComboGroupMode.Any ? list.Any(c => Check(player, c)) : list.All(c => Check(player, c)));

    private bool Check(Actor player, CustomComboCondition c)
    {
        var res = c.Group != CustomComboGroupMode.None ? CheckList(player, c.Children, c.Group) : CheckLeaf(player, c);
        return res != c.Negate;
    }

    private bool CheckLeaf(Actor player, CustomComboCondition c)
    {
        var target = _ws.Actors.Find(player.TargetID);
        return c.Type switch
        {
            CustomComboConditionType.ActionUsable => IsUsable(player, c.ID),
            CustomComboConditionType.ActionReady => ReadyIn(c.ID) <= c.Value + 0.001f,
            CustomComboConditionType.ActionCharges => Charges(player, c.ID) >= Math.Max(1, c.Count),
            CustomComboConditionType.UsedRecently => _lastUsed.TryGetValue(CustomComboData.Canonical(c.ID), out var t) && (_ws.CurrentTime - t).TotalSeconds <= c.Value,

            CustomComboConditionType.LastComboAction => _ws.Client.ComboState.Remaining > 0 && CustomComboData.Canonical(_ws.Client.ComboState.Action) == CustomComboData.Canonical(c.ID),
            CustomComboConditionType.ComboTimeRemaining => Compare(_ws.Client.ComboState.Remaining, c.Compare, c.Value),
            CustomComboConditionType.CanWeave => CanWeave(player, Math.Max(c.Value, 0.1f)),
            CustomComboConditionType.CanLateWeave => CanLateWeave(player),
            CustomComboConditionType.CombatTime => Compare(_combatStart == default ? 0 : (float)(_ws.CurrentTime - _combatStart).TotalSeconds, c.Compare, c.Value),

            CustomComboConditionType.StatusOnSelf => HasStatus(player, player, c),
            CustomComboConditionType.PlayerHP => player.HPMP.MaxHP > 0 && Compare(player.HPRatio * 100, c.Compare, c.Value),
            CustomComboConditionType.PlayerMP => Compare(player.HPMP.CurMP, c.Compare, c.Value),
            CustomComboConditionType.PlayerLevel => player.Level >= c.Value,
            CustomComboConditionType.InCombat => player.InCombat,
            CustomComboConditionType.Moving => _movement.IsMoving(),
            CustomComboConditionType.PetPresent => _ws.Actors.Any(a => a.OwnerID == player.InstanceID && a.Type == ActorType.Pet && !a.IsDead),

            CustomComboConditionType.Gauge => CustomComboGauge.Read(_ws, player.Class, c.Field) is { } v && Compare(v, c.Compare, c.Value),

            CustomComboConditionType.HasTarget => target != null,
            CustomComboConditionType.StatusOnTarget => target != null && HasStatus(player, target, c),
            CustomComboConditionType.TargetHP => target != null && target.HPMP.MaxHP > 0 && Compare(target.HPRatio * 100, c.Compare, c.Value),
            CustomComboConditionType.TargetInActionRange => target != null && InRange(player, target, c.ID),
            CustomComboConditionType.TargetDistance => target != null && Compare(player.DistanceToHitbox(target), c.Compare, c.Value),
            CustomComboConditionType.Positional => target != null && GetPositional(player, target) == c.Positional,

            CustomComboConditionType.EnemiesNearPlayer => CountEnemies(player.Position, c.Value) >= Math.Max(1, c.Count),
            CustomComboConditionType.EnemiesNearTarget => target != null && CountEnemies(target.Position, c.Value) >= Math.Max(1, c.Count),
            CustomComboConditionType.InBossFight => _bossmods.ActiveModule != null,
            CustomComboConditionType.RaidwideIncoming => DamageIncoming(AIHints.PredictedDamageType.Raidwide, c.Value),
            CustomComboConditionType.TankbusterIncoming => DamageIncoming(AIHints.PredictedDamageType.Tankbuster, c.Value),

            CustomComboConditionType.PartyMembersBelowHP => _ws.Party.WithoutSlot(excludeAlliance: true).Count(p => p.HPMP.MaxHP > 0 && p.HPRatio * 100 < c.Value) >= Math.Max(1, c.Count),
            CustomComboConditionType.PartyAverageHP => PartyAverageHP() is { } avg && Compare(avg, c.Compare, c.Value),

            _ => false
        };
    }

    public static bool Compare(float actual, CustomComboCompare op, float value) => op switch
    {
        CustomComboCompare.Less => actual < value,
        CustomComboCompare.LessEqual => actual <= value,
        CustomComboCompare.Equal => Math.Abs(actual - value) < 0.001f,
        CustomComboCompare.GreaterEqual => actual >= value,
        CustomComboCompare.Greater => actual > value,
        _ => false
    };

    private ActionDefinition? CurrentDefinition(uint action)
    {
        var baseAction = CustomComboData.Canonical(action);
        return ActionDefinitions.Instance[new(ActionType.Spell, _ctx.AdjustOriginal(baseAction))] ?? ActionDefinitions.Instance[new(ActionType.Spell, baseAction)];
    }

    // see WrathCombo's ActionReady
    private bool IsUsable(Actor player, uint action)
    {
        var baseAction = CustomComboData.Canonical(action);
        if (baseAction == 0 || !IsUnlocked(player, baseAction))
            return false;
        var def = CurrentDefinition(baseAction);
        var offCooldown = Charges(player, baseAction) >= 1 || (def?.IsGCD ?? false) && ReadyIn(baseAction) <= GCDRemaining + QueueWindow;
        return offCooldown && UsableStatuses.Contains(_ctx.ActionStatus(_ctx.AdjustOriginal(baseAction)));
    }

    private float ReadyIn(uint action)
    {
        var def = CurrentDefinition(action);
        if (def == null)
            return 0;
        var main = def.MainCooldownGroup is >= 0 and not ActionDefinitions.GCDGroup ? def.MainReadyIn(_ws.Client.Cooldowns, _ws.Client.DutyActions) : 0;
        var extra = def.ExtraCooldownGroup is >= 0 and not ActionDefinitions.GCDGroup ? def.ExtraReadyIn(_ws.Client.Cooldowns) : 0;
        return Math.Max(main, extra);
    }

    private int Charges(Actor player, uint action)
    {
        var def = CurrentDefinition(action);
        if (def == null || def.MainCooldownGroup < 0)
            return 1;
        var maxAtLevel = def.MaxChargesAtLevel(player.Level);
        var cd = _ws.Client.Cooldowns[def.ActualMainCooldownGroup(_ws.Client.DutyActions)];
        if (cd.Total <= 0)
            return maxAtLevel;
        if (def.MainCooldownGroup == ActionDefinitions.GCDGroup)
            return 1;
        // when first charge is used, total is set to cd*max-at-cap and elapsed to cd*(max-at-level - 1), so elapsed/single-charge-cd is the current charge count
        var singleCharge = cd.Total / Math.Max(1, def.MaxChargesAtCap());
        return Math.Min(maxAtLevel, (int)((cd.Elapsed + 0.001f) / singleCharge));
    }

    private float GCDRemaining => _ws.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining;
    private static float CastRemaining(Actor player) => player.CastInfo is { } cast ? Math.Max(0, cast.TotalTime - cast.ElapsedTime) : 0;

    // see WrathCombo's CanWeave
    private bool CanWeave(Actor player, float weaveTime)
    {
        var cast = CastRemaining(player);
        var animLock = _ws.Client.AnimationLock;
        return cast <= QueueWindow && animLock <= WeaveAnimLock && GCDRemaining > cast + animLock + weaveTime;
    }

    // see WrathCombo's CanDelayedWeave
    private bool CanLateWeave(Actor player)
    {
        var gcd = GCDRemaining;
        var halfGCD = _ws.Client.Cooldowns[ActionDefinitions.GCDGroup].Total * 0.5f;
        var animLock = _ws.Client.AnimationLock;
        return CastRemaining(player) <= 0 && animLock <= QueueWindow && gcd > WeaveAnimLock + animLock && gcd <= Math.Min(1.25f, halfGCD);
    }

    private bool HasStatus(Actor player, Actor target, CustomComboCondition c)
    {
        // statuses that were just applied but not yet confirmed by server are treated as having full duration, to avoid double-casting
        var pendingExpiration = DateTime.MaxValue;
        var status = c.OnlySelf ? target.FindStatus(c.ID, player.InstanceID, pendingExpiration) : target.FindStatus(c.ID, pendingExpiration);
        if (status == null)
            return false;
        var s = status.Value;
        var remaining = s.ExpireAt == default || s.ExpireAt == DateTime.MaxValue ? float.MaxValue : (float)(s.ExpireAt - _ws.CurrentTime).TotalSeconds;
        return remaining > c.Value && (c.Count <= 0 || s.Extra >= c.Count);
    }

    private bool InRange(Actor player, Actor target, uint action)
    {
        var def = CurrentDefinition(action);
        var range = def?.Range ?? Service.LuminaRow<Lumina.Excel.Sheets.Action>(CustomComboData.Canonical(action))?.Range ?? 0;
        return range <= 0 || player.DistanceToHitbox(target) <= range;
    }

    private static Positional GetPositional(Actor player, Actor target)
    {
        var offset = player.Position - target.Position;
        if (offset.LengthSq() < 0.0001f)
            return Positional.Any;
        var dir = offset.Normalized();
        var facing = target.Rotation.ToDirection();
        var front = facing.Dot(dir);
        var side = Math.Abs(facing.OrthoL().Dot(dir));
        return front < -side ? Positional.Rear : front > side ? Positional.Front : Positional.Flank;
    }

    private int CountEnemies(WPos center, float radius)
    {
        var count = 0;
        foreach (var e in _hints.PotentialTargets)
            if (e.Priority > AIHints.Enemy.PriorityUndesirable && !e.Actor.IsDead && e.Actor.Position.InCircle(center, radius + e.Actor.HitboxRadius))
                ++count;
        return count;
    }

    private bool DamageIncoming(AIHints.PredictedDamageType type, float within)
    {
        var deadline = _ws.FutureTime(within);
        return _hints.PredictedDamage.Any(d => d.Type == type && d.Players[PartyState.PlayerSlot] && d.Activation <= deadline);
    }

    private float? PartyAverageHP()
    {
        float sum = 0;
        var count = 0;
        foreach (var p in _ws.Party.WithoutSlot(excludeAlliance: true))
        {
            if (p.HPMP.MaxHP == 0)
                continue;
            sum += p.HPRatio * 100;
            ++count;
        }
        return count > 0 ? sum / count : null;
    }
}
