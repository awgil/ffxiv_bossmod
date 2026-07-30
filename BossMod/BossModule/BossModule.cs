using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

// base for boss modules - provides all the common features, so that look is standardized
// by default, module activates (transitions to phase 0) whenever "primary" actor becomes both targetable and in combat (this is how we detect 'pull') - though this can be overridden if needed
[SkipLocalsInit]
public abstract class BossModule : IDisposable
{
    public readonly WorldState WorldState;
    public Actor PrimaryActor;
    public static readonly BossModuleConfig WindowConfig = Service.Config.Get<BossModuleConfig>();
    public readonly MiniArena Arena;
    public readonly BossModuleRegistry.Info? Info;
    public readonly StateMachine StateMachine;
    public readonly Pathfinding.ObstacleMapManager Obstacles;
    public readonly bool OnlyLoadIfTargetable;

    private readonly EventSubscriptions _subscriptions;

    public Event<BossModule, BossComponent?, string> Error = new();

    public PartyState Raid => WorldState.Party;
    public WPos Center => Arena.Center;
    public ArenaBounds Bounds => Arena.Bounds;

    // per-oid enemy lists; filled on first request
    public readonly Dictionary<uint, List<Actor>> RelevantEnemies = []; // key = actor OID

    public List<Actor> Enemies(uint oid)
    {
        if (!RelevantEnemies.TryGetValue(oid, out var entry))
        {
            entry = [];
            foreach (var actor in WorldState.Actors.Actors.Values)
            {
                if (actor.OID == oid)
                {
                    entry.Add(actor);
                }
            }
            RelevantEnemies[oid] = entry;
        }
        return entry;
    }

    public List<Actor> Enemies(uint[] enemies)
    {
        List<Actor> relevantEnemies = [];
        var len = enemies.Length;
        for (var i = 0; i < len; ++i)
        {
            var enemy = enemies[i];
            if (!RelevantEnemies.TryGetValue(enemy, out var entry))
            {
                entry = [];
                foreach (var actor in WorldState.Actors.Actors.Values)
                {
                    if (actor.OID == enemy)
                    {
                        entry.Add(actor);
                    }
                }
                RelevantEnemies[enemy] = entry;
            }
            relevantEnemies.AddRange(entry);
        }
        return relevantEnemies;
    }

    public virtual Actor? GetDefaultTarget(int slot)
    {
        if (!PrimaryActor.IsDeadOrDestroyed && PrimaryActor.IsTargetable)
        {
            return PrimaryActor;
        }

        return null;
    }

    // component management: at most one component of any given type can be active at any time
    public readonly List<BossComponent> Components = [];

    public T? FindComponent<T>() where T : BossComponent
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            if (Components[i] is T matchingComponent)
            {
                return matchingComponent;
            }
        }
        return null;
    }

    public void ActivateComponent<T>() where T : BossComponent
    {
        if (FindComponent<T>() != null)
        {
            ReportError(null, $"State {StateMachine.ActiveState?.ID:X}: Activating a component of type {typeof(T)} when another of the same type is already active; old one is deactivated automatically");
            DeactivateComponent<T>();
        }
        var comp = New<T>.Create(this);
        Components.Add(comp);

        // execute callbacks for existing state
        foreach (var actor in WorldState.Actors)
        {
            if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo and not ActorType.Buddy)
            {
                comp.OnActorCreated(actor);
                var castinfo = actor.CastInfo;
                if (castinfo?.IsSpell() ?? false)
                {
                    comp.OnCastStarted(actor, castinfo);
                }
                if (actor.IsTargetable)
                {
                    comp.OnActorTargetable(actor);
                }
                else
                {
                    comp.OnActorUntargetable(actor);
                }
                if (actor.IsDead)
                {
                    comp.OnActorDeath(actor);
                }
                comp.OnActorRenderflagsChange(actor, actor.Renderflags);
                comp.OnActorEventStateChange(actor, actor.EventState);
            }
            ref readonly var tether = ref actor.Tether;
            if (tether.ID != default)
            {
                comp.OnTethered(actor, tether);
            }
            var len = actor.Statuses.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var status = ref actor.Statuses[i];
                if (status.ID != default)
                {
                    comp.OnStatusGain(actor, ref status);
                }
            }
        }
    }

    public void DeactivateComponent<T>() where T : BossComponent
    {
        var count = Components.Count;
        var removed = false;
        for (var i = 0; i < count; ++i)
        {
            var comp = Components[i];
            if (comp is T)
            {
                Components.Remove(comp);
                removed = true;
                break;
            }
        }
        if (!removed)
        {
            ReportError(null, $"State {StateMachine.ActiveState?.ID:X}: Could not find a component of type {typeof(T)} to deactivate");
        }
    }

    public void ClearComponents(Predicate<BossComponent> condition) => Components.RemoveAll(condition);

    protected BossModule(WorldState ws, Actor primary, WPos center, ArenaBounds bounds, bool onlyLoadIfTargetable = false)
    {
        Obstacles = new(ws);
        WorldState = ws;
        PrimaryActor = primary;
        Arena = new(center, bounds);
        OnlyLoadIfTargetable = onlyLoadIfTargetable;
        Info = BossModuleRegistry.FindByOID(primary.OID);
        StateMachine = Info != null ? ((StateMachineBuilder)Activator.CreateInstance(Info.StatesType, this)!).Build() : new([]);

        _subscriptions = new
        (
            WorldState.Actors.Added.Subscribe(OnActorCreated),
            WorldState.Actors.Removed.Subscribe(OnActorDestroyed),
            WorldState.Actors.CastStarted.Subscribe(OnActorCastStarted),
            WorldState.Actors.CastFinished.Subscribe(OnActorCastFinished),
            WorldState.Actors.IsTargetableChanged.Subscribe(OnIsTargetableChange),
            WorldState.Actors.IsDeadChanged.Subscribe(OnActorIsDead),
            WorldState.Actors.RenderflagsChanged.Subscribe(OnActorRenderflagsChange),
            WorldState.Actors.EventStateChanged.Subscribe(OnActorEventStateChange),
            WorldState.Actors.Tethered.Subscribe(OnActorTethered),
            WorldState.Actors.Untethered.Subscribe(OnActorUntethered),
            WorldState.Actors.StatusGain.Subscribe(OnActorStatusGain),
            WorldState.Actors.StatusLose.Subscribe(OnActorStatusLose),
            WorldState.Actors.IconAppeared.Subscribe(OnActorIcon),
            WorldState.Actors.VFXAppeared.Subscribe(OnActorVFX),
            WorldState.Actors.CastEvent.Subscribe(OnActorCastEvent),
            WorldState.Actors.EventObjectStateChange.Subscribe(OnActorEState),
            WorldState.Actors.EventObjectAnimation.Subscribe(OnActorEAnim),
            WorldState.Actors.PlayActionTimelineEvent.Subscribe(OnActorPlayActionTimelineEvent),
            WorldState.Actors.PlayActionTimelineSync.Subscribe(OnActorPlayActionTimelineSync),
            WorldState.Actors.EventNpcYell.Subscribe(OnActorNpcYell),
            WorldState.Actors.ModelStateChanged.Subscribe(OnActorModelStateChange),
            WorldState.MapEffect.Subscribe(OnMapEffect),
            WorldState.LegacyMapEffect.Subscribe(OnLegacyMapEffect),
            WorldState.DirectorUpdate.Subscribe(OnDirectorUpdate)
        );

        foreach (var v in WorldState.Actors)
        {
            OnActorCreated(v);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        StateMachine.Reset();
        ClearComponents(_ => true);
        _subscriptions.Dispose();
        Obstacles.Dispose();
    }

    public void Update()
    {
        if (StateMachine.ActivePhaseIndex < 0 && CheckPull())
        {
            StateMachine.Start(WorldState.CurrentTime);
        }

        if (StateMachine.ActiveState != null)
        {
            StateMachine.Update(WorldState.CurrentTime);
        }

        if (StateMachine.ActiveState != null)
        {
            UpdateModule();
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].Update();
            }
        }
    }

    public void Draw(Angle cameraAzimuth, int pcSlot, bool includeText, bool includeArena)
    {
        var pc = Raid[pcSlot];
        if (pc == null)
        {
            return;
        }

        var pcHints = CalculateHintsForRaidMember(pcSlot, pc);
        if (includeText)
        {
            if (WindowConfig.ShowMechanicTimers)
            {
                StateMachine.Draw();
            }

            if (WindowConfig.ShowGlobalHints)
            {
                DrawGlobalHints(CalculateGlobalHints());
            }

            if (WindowConfig.ShowPlayerHints)
            {
                DrawPlayerHints(pcHints);
            }
        }
        if (includeArena)
        {
            Arena.Begin(cameraAzimuth);
            var haveRisks = false;
            var count = pcHints.Count;
            for (var i = 0; i < count; ++i)
            {
                if (pcHints[i].Item2)
                {
                    haveRisks = true;
                    break;
                }
            }
            DrawArena(pcSlot, pc, haveRisks);
            MiniArena.End();
        }
    }

    public virtual void DrawArena(int pcSlot, Actor pc, bool haveRisks)
    {
        // draw background
        DrawArenaBackground(pcSlot, pc);

        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].DrawArenaBackground(pcSlot, pc);
        }

        // draw borders
        if (WindowConfig.ShowBorder)
        {
            Arena.AddComplexPolygon(Bounds.ShapeSimplified, haveRisks && WindowConfig.ShowBorderRisk ? Colors.Enemy : Colors.Border, 2f, false);
        }

        if (WindowConfig.ShowCardinals)
        {
            Arena.CardinalNames();
        }

        if (WindowConfig.ShowWaymarks && WorldState.Waymarks.AnyWaymarks)
        {
            DrawWaymarks();
        }

        if (WindowConfig.ShowSigns)
        {
            DrawSigns();
        }

        // draw non-player alive party members
        DrawPartyMembers(pcSlot, pc);

        // draw foreground
        DrawArenaForeground(pcSlot, pc);
        for (var i = 0; i < count; ++i)
        {
            Components[i].DrawArenaForeground(pcSlot, pc);
        }

        if (WindowConfig.ShowMeleeRangeIndicator)
        {
            var t = WorldState.Actors.Find(pc.TargetID);
            var actor = t != null && !t.IsAlly && !t.IsDead && t.InCombat ? t : !PrimaryActor.IsDead && PrimaryActor.IsTargetable ? PrimaryActor : null;
            if (actor != null)
            {
                Arena.ZoneDonut(actor.Position, actor.HitboxRadius + 2.6f, actor.HitboxRadius + 2.9f, Colors.MeleeRangeIndicator);
            }
        }
        // draw enemies & player
        DrawEnemies(pcSlot, pc);
        Arena.Actor(pc, Colors.PC, true);
    }

    public BossComponent.TextHints CalculateHintsForRaidMember(int slot, Actor actor)
    {
        BossComponent.TextHints hints = [];
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].AddHints(slot, actor, hints);
        }

        return hints;
    }

    public BossComponent.MovementHints CalculateMovementHintsForRaidMember(int slot, Actor actor)
    {
        BossComponent.MovementHints hints = [];
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].AddMovementHints(slot, actor, hints);
        }

        return hints;
    }

    public BossComponent.GlobalHints CalculateGlobalHints()
    {
        BossComponent.GlobalHints hints = [];
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].AddGlobalHints(hints);
        }

        return hints;
    }

    public void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PathfindMapCenter = Center;
        hints.PathfindMapBounds = Bounds;

        if (Arena.Bounds.AllowObstacleMap)
        {
            var (entry, bitmap) = Obstacles.Find(new Vector3(Center.X, actor.PosRot.Y, Center.Z));
            if (entry != null && bitmap != null)
            {
                var originCell = (Center - entry.Origin) / bitmap.PixelSize;
                var originX = (int)originCell.X;
                var originZ = (int)originCell.Z;
                var halfSize = (int)(Bounds.Radius / bitmap.PixelSize);
                hints.PathfindMapObstacles = new(bitmap, new(originX - halfSize, originZ - halfSize, originX + halfSize, originZ + halfSize));
            }
        }
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].AddAIHints(slot, actor, assignment, hints);
        }

        CalculateModuleAIHints(slot, actor, assignment, hints);
        if (!WindowConfig.AllowAutomaticActions && AI.AIManager.Instance?.Beh == null && Autorotation.MiscAI.NormalMovement.Instance == null)
        {
            hints.ActionsToExecute.Clear();
        }
    }

    public void ReportError(BossComponent? comp, string message)
    {
        Service.Log($"[ModuleError] [{GetType().Name}] [{comp?.GetType().Name}] {message}");
        Error.Fire(this, comp, message);
    }

    // utility to calculate expected time when cast finishes (plus an optional delay); returns fallback value if argument is null
    // for whatever reason, npc spells have reported remaining cast time consistently 0.3s smaller than reality - this delta is added automatically, in addition to optional delay
    public DateTime CastFinishAt(ActorCastInfo? cast, double extraDelay = default, DateTime fallback = default) => cast != null ? WorldState.FutureTime(cast.NPCRemainingTime + extraDelay) : fallback;

    // called during update if module is not yet active, should return true if it is to be activated
    // default implementation activates if primary target is both targetable and in combat
    protected virtual bool CheckPull() => PrimaryActor.IsTargetable && PrimaryActor.InCombat;

    // called during update if module is active; should return true if module is to be reset (i.e. deleted and new instance recreated for same actor)
    // default implementation never resets, but it's useful for outdoor bosses that can be leashed
    public virtual bool CheckReset() => false;

    // return true if out-of-combat enemies should be set to priority 0 - useful for multi-phase encounters when player wants to use automatic targeting via cdplan
    public virtual bool ShouldPrioritizeAllEnemies => false;

    protected virtual void UpdateModule() { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Actor? GetActor(uint enemy)
    {
        var b = Enemies(enemy);
        return b.Count != 0 ? b[0] : null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsActorInCombat(uint enemy)
    {
        var b = Enemies(enemy);
        return b.Count != 0 && b[0].InCombat;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsAnyActorInCombat(uint enemy)
    {
        var enemies = Enemies(enemy);
        var count = enemies.Count;
        for (var j = 0; j < count; ++j)
        {
            var e = enemies[j];
            if (e.InCombat)
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsAnyActorTargetable(uint enemy)
    {
        var enemies = Enemies(enemy);
        var count = enemies.Count;
        for (var j = 0; j < count; ++j)
        {
            var e = enemies[j];
            if (e.IsTargetable)
            {
                return true;
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsAnyActorInCombat(uint[] enemies)
    {
        var allenemies = enemies;
        var len = allenemies.Length;
        for (var i = 0; i < len; ++i)
        {
            var enemies_ = Enemies(allenemies[i]);
            var count = enemies_.Count;
            for (var j = 0; j < count; ++j)
            {
                var enemy = enemies_[j];
                if (enemy.InCombat)
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool IsAnyActorInBoundsInCombat(uint[] enemies)
    {
        var allenemies = enemies;
        var len = allenemies.Length;
        var center = Arena.Center;
        var radius = Bounds.Radius;
        for (var i = 0; i < len; ++i)
        {
            var enemies_ = Enemies(allenemies[i]);
            var count = enemies_.Count;
            for (var j = 0; j < count; ++j)
            {
                var enemy = enemies_[j];
                if (enemy.InCombat && enemy.Position.AlmostEqual(center, radius))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Actor? GetActiveActor(List<Actor> enemy)
    {
        var count = enemy.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = enemy[i];
            if (e.IsTargetable && !e.IsDead)
            {
                return e;
            }
        }
        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<Actor> GetActiveActors(List<Actor> enemy)
    {
        var count = enemy.Count;
        List<Actor> actors = [with(count)];
        for (var i = 0; i < count; ++i)
        {
            var e = enemy[i];
            if (e.IsTargetable && !e.IsDead)
            {
                actors.Add(e);
            }
        }
        return actors;
    }

    protected virtual void DrawArenaBackground(int pcSlot, Actor pc) { } // before modules background
    protected virtual void DrawArenaForeground(int pcSlot, Actor pc) { } // after border, before modules foreground
    protected virtual void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }

    // called at the very end to draw important enemies, default implementation draws primary actor
    protected virtual void DrawEnemies(int pcSlot, Actor pc) => Arena.Actor(PrimaryActor);

    private void DrawGlobalHints(BossComponent.GlobalHints hints)
    {
        using var color = ImRaii.PushColor(ImGuiCol.Text, Colors.TextColor11);
        var count = hints.Count;
        for (var i = 0; i < count; ++i)
        {
            ImGui.TextUnformatted(hints[i]);
            ImGui.SameLine();
        }
        ImGui.NewLine();
    }

    private void DrawPlayerHints(BossComponent.TextHints hints)
    {
        var count = hints.Count;
        for (var i = 0; i < count; ++i)
        {
            var hint = hints[i];
            using var color = ImRaii.PushColor(ImGuiCol.Text, hint.Item2 ? Colors.Danger : Colors.Safe);
            ImGui.TextUnformatted(hint.Item1);
            ImGui.SameLine();
        }
        ImGui.NewLine();
    }

    private void DrawWaymarks()
    {
        DrawWaymark(WorldState.Waymarks[Waymark.A], "A", Colors.WaymarkA);
        DrawWaymark(WorldState.Waymarks[Waymark.B], "B", Colors.WaymarkB);
        DrawWaymark(WorldState.Waymarks[Waymark.C], "C", Colors.WaymarkC);
        DrawWaymark(WorldState.Waymarks[Waymark.D], "D", Colors.WaymarkD);
        DrawWaymark(WorldState.Waymarks[Waymark.N1], "1", Colors.Waymark1);
        DrawWaymark(WorldState.Waymarks[Waymark.N2], "2", Colors.Waymark2);
        DrawWaymark(WorldState.Waymarks[Waymark.N3], "3", Colors.Waymark3);
        DrawWaymark(WorldState.Waymarks[Waymark.N4], "4", Colors.Waymark4);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DrawWaymark(Vector3? position, string text, uint color)
        {
            if (position?.XZ() is Vector2 vec2)
            {
                WPos pos = new(vec2);
                if (WindowConfig.ShowOutlinesAndShadows)
                {
                    Arena.TextWorld(pos, text, Colors.Shadows, WindowConfig.WaymarkFontSize + 3f);
                }
                Arena.TextWorld(pos, text, color, WindowConfig.WaymarkFontSize);
            }
        }
    }

    private void DrawSigns()
    {
        for (var i = Sign.Attack1; i < Sign.Count; ++i)
        {
            var actor = WorldState.Actors.Find(WorldState.Waymarks[i]);
            if (actor == null)
            {
                continue;
            }

            var iconId = i.IconId();
            if (Service.Texture.TryGetFromGameIcon(iconId, out var tex))
            {
                var wrap = tex.GetWrapOrEmpty();
                var pos = Arena.WorldPositionToScreenPosition(actor.Position);
                var scale = WindowConfig.ArenaScale * 24f;

                ImGui.GetWindowDrawList().AddImage(wrap.Handle, pos - new Vector2(scale), pos);
            }
        }
    }

    private void DrawPartyMembers(int pcSlot, Actor pc)
    {
        var raid = Raid.WithSlot();
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var (slot, player) = raid[i];
            if (slot == pcSlot)
            {
                continue;
            }

            var (prio, color) = CalculateHighestPriority(pcSlot, pc, slot, player);

            var isFocus = WorldState.Client.FocusTargetId == player.InstanceID;
            if (prio == BossComponent.PlayerPriority.Irrelevant && !WindowConfig.ShowIrrelevantPlayers && !(isFocus && WindowConfig.ShowFocusTargetPlayer))
            {
                continue;
            }

            if (color == 0)
            {
                color = prio switch
                {
                    BossComponent.PlayerPriority.Interesting => Colors.PlayerInteresting,
                    BossComponent.PlayerPriority.Danger => Colors.Danger,
                    BossComponent.PlayerPriority.Critical => Colors.Vulnerable,
                    _ => Colors.PlayerGeneric
                };

                if (color == Colors.PlayerGeneric)
                {
                    // optional focus/role-based overrides
                    if (isFocus)
                    {
                        color = Colors.Focus;
                    }
                    else if (WindowConfig.ColorPlayersBasedOnRole)
                    {
                        color = player.ClassCategory switch
                        {
                            ClassCategory.Tank => Colors.Tank,
                            ClassCategory.Healer => Colors.Healer,
                            ClassCategory.Melee => Colors.Melee,
                            ClassCategory.Caster => Colors.Caster,
                            ClassCategory.PhysRanged => Colors.PhysRanged,
                            _ => color
                        };
                    }
                }
            }

            Arena.Actor(player, color);
        }
    }

    private (BossComponent.PlayerPriority, uint) CalculateHighestPriority(int pcSlot, Actor pc, int playerSlot, Actor player)
    {
        uint color = 0;
        var highestPrio = BossComponent.PlayerPriority.Irrelevant;
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            uint subColor = 0;
            var subPrio = Components[i].CalcPriority(pcSlot, pc, playerSlot, player, ref subColor);
            if (subPrio > highestPrio)
            {
                highestPrio = subPrio;
                color = subColor;
            }
        }
        return (highestPrio, color);
    }

    private void OnActorCreated(Actor actor)
    {
        RelevantEnemies.GetValueOrDefault(actor.OID)?.Add(actor);
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo and not ActorType.Buddy)
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnActorCreated(actor);
            }
        }
    }

    private void OnActorDestroyed(Actor actor)
    {
        RelevantEnemies.GetValueOrDefault(actor.OID)?.Remove(actor);
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo and not ActorType.Buddy)
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnActorDestroyed(actor);
            }
        }
    }

    private void OnActorCastStarted(Actor actor)
    {
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo and not ActorType.Buddy && (actor.CastInfo?.IsSpell() ?? false))
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnCastStarted(actor, actor.CastInfo);
            }
        }
    }

    private void OnActorCastFinished(Actor actor)
    {
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo and not ActorType.Buddy && (actor.CastInfo?.IsSpell() ?? false))
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnCastFinished(actor, actor.CastInfo);
            }
        }
    }

    private void OnIsTargetableChange(Actor actor)
    {
        if (actor.IsTargetable)
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnActorTargetable(actor);
            }
        }
        else
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnActorUntargetable(actor);
            }
        }
    }

    private void OnActorIsDead(Actor actor)
    {
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo and not ActorType.Buddy)
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnActorDeath(actor);
            }
        }
    }

    private void OnActorRenderflagsChange(Actor actor)
    {
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo and not ActorType.Buddy)
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnActorRenderflagsChange(actor, actor.Renderflags);
            }
        }
    }

    private void OnActorTethered(Actor actor)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnTethered(actor, actor.Tether);
        }
    }

    private void OnActorUntethered(Actor actor)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnUntethered(actor, actor.Tether);
        }
    }

    private void OnActorStatusGain(Actor actor, int index)
    {
        if (actor.Type != ActorType.Pet)
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnStatusGain(actor, ref actor.Statuses[index]);
            }
        }
    }

    private void OnActorStatusLose(Actor actor, int index)
    {
        if (actor.Type != ActorType.Pet)
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnStatusLose(actor, ref actor.Statuses[index]);
            }
        }
    }

    private void OnActorIcon(Actor actor, uint iconID, ulong targetID)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnEventIcon(actor, iconID, targetID);
        }
    }

    private void OnActorVFX(Actor actor, uint vfxID, ulong targetID)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnEventVFX(actor, vfxID, targetID);
        }
    }

    private void OnActorCastEvent(Actor actor, ActorCastEvent cast)
    {
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo and not ActorType.Buddy && cast.IsSpell())
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                Components[i].OnEventCast(actor, cast);
            }
        }
    }

    private void OnActorEState(Actor actor, ushort state)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnActorEState(actor, state);
        }
    }

    private void OnActorEAnim(Actor actor, ushort p1, ushort p2)
    {
        var state = ((uint)p1 << 16) | p2;
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnActorEAnim(actor, state);
        }
    }

    private void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnActorPlayActionTimelineEvent(actor, id);
        }
    }

    private void OnActorPlayActionTimelineSync(Actor actor, List<(ulong, ushort)> events)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnActorPlayActionTimelineSync(actor, events);
        }
    }

    private void OnActorNpcYell(Actor actor, ushort id)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnActorNpcYell(actor, id);
        }
    }

    private void OnActorModelStateChange(Actor actor)
    {
        if (actor.Type != ActorType.Pet)
        {
            var count = Components.Count;
            for (var i = 0; i < count; ++i)
            {
                ref readonly var state = ref actor.ModelState;
                Components[i].OnActorModelStateChange(actor, state.ModelState, state.AnimState1, state.AnimState2);
            }
        }
    }

    private void OnActorEventStateChange(Actor actor)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnActorEventStateChange(actor, actor.EventState);
        }
    }

    private void OnMapEffect(WorldState.OpMapEffect op)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnMapEffect(op.Index, op.State);
        }
    }

    private void OnLegacyMapEffect(WorldState.OpLegacyMapEffect op)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnLegacyMapEffect(op.Sequence, op.Param, op.Data);
        }
    }

    private void OnDirectorUpdate(WorldState.OpDirectorUpdate op)
    {
        var count = Components.Count;
        for (var i = 0; i < count; ++i)
        {
            Components[i].OnEventDirectorUpdate(op.UpdateID, op.Param1, op.Param2, op.Param3, op.Param4);
        }
    }
}
