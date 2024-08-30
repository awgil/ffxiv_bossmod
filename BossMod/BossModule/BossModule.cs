using ImGuiNET;

namespace BossMod;

// base for boss modules - provides all the common features, so that look is standardized
// by default, module activates (transitions to phase 0) whenever "primary" actor becomes both targetable and in combat (this is how we detect 'pull') - though this can be overridden if needed
public abstract class BossModule : IDisposable
{
    public readonly WorldState WorldState;
    public readonly Actor PrimaryActor;
    public readonly BossModuleConfig WindowConfig;
    public readonly MiniArena Arena;
    public readonly ModuleRegistry.Info? Info;
    public readonly StateMachine StateMachine;

    private readonly EventSubscriptions _subscriptions;

    public Event<BossModule, BossComponent?, string> Error = new();

    public PartyState Raid => WorldState.Party;
    public WPos Center => Arena.Center;
    public ArenaBounds Bounds => Arena.Bounds;
    public bool InBounds(WPos position) => Arena.InBounds(position);

    // per-oid enemy lists; filled on first request
    private readonly Dictionary<uint, List<Actor>> _relevantEnemies = []; // key = actor OID
    public IReadOnlyDictionary<uint, List<Actor>> RelevantEnemies => _relevantEnemies;
    public IReadOnlyList<Actor> Enemies(uint oid)
    {
        IReadOnlyList<Actor>? entry = _relevantEnemies.GetValueOrDefault(oid);
        entry ??= _relevantEnemies[oid] = WorldState.Actors.Where(actor => actor.OID == oid).ToList();
        return entry;
    }
    public IReadOnlyList<Actor> Enemies<OID>(OID oid) where OID : Enum => Enemies((uint)(object)oid);

    // component management: at most one component of any given type can be active at any time
    private readonly List<BossComponent> _components = [];
    public IReadOnlyList<BossComponent> Components => _components;
    public T? FindComponent<T>() where T : BossComponent => _components.OfType<T>().FirstOrDefault();

    public void ActivateComponent<T>() where T : BossComponent
    {
        if (FindComponent<T>() != null)
        {
            ReportError(null, $"State {StateMachine.ActiveState?.ID:X}: Activating a component of type {typeof(T)} when another of the same type is already active; old one is deactivated automatically");
            DeactivateComponent<T>();
        }
        var comp = New<T>.Create(this);
        _components.Add(comp);

        // execute callbacks for existing state
        foreach (var actor in WorldState.Actors)
        {
            bool nonPlayer = actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo;
            if (nonPlayer)
            {
                comp.OnActorCreated(actor);
                if (actor.CastInfo?.IsSpell() ?? false)
                    comp.OnCastStarted(actor, actor.CastInfo);
            }
            if (actor.Tether.ID != 0)
                comp.OnTethered(actor, actor.Tether);
            for (int i = 0; i < actor.Statuses.Length; ++i)
                if (actor.Statuses[i].ID != 0)
                    comp.OnStatusGain(actor, actor.Statuses[i]);
        }
    }

    public void DeactivateComponent<T>() where T : BossComponent
    {
        int count = _components.RemoveAll(x => x is T);
        if (count == 0)
            ReportError(null, $"State {StateMachine.ActiveState?.ID:X}: Could not find a component of type {typeof(T)} to deactivate");
    }

    public void ClearComponents(Predicate<BossComponent> condition) => _components.RemoveAll(condition);

    protected BossModule(WorldState ws, Actor primary, WPos center, ArenaBounds bounds)
    {
        WorldState = ws;
        PrimaryActor = primary;
        WindowConfig = Service.Config.Get<BossModuleConfig>();
        Arena = new(WindowConfig, center, bounds);
        Info = ModuleRegistry.FindByOID(primary.OID);
        StateMachine = Info != null ? ((StateMachineBuilder)Activator.CreateInstance(Info.StatesType, this)!).Build() : new([]);

        _subscriptions = new
        (
            WorldState.Actors.Added.Subscribe(OnActorCreated),
            WorldState.Actors.Removed.Subscribe(OnActorDestroyed),
            WorldState.Actors.CastStarted.Subscribe(OnActorCastStarted),
            WorldState.Actors.CastFinished.Subscribe(OnActorCastFinished),
            WorldState.Actors.Tethered.Subscribe(OnActorTethered),
            WorldState.Actors.Untethered.Subscribe(OnActorUntethered),
            WorldState.Actors.StatusGain.Subscribe(OnActorStatusGain),
            WorldState.Actors.StatusLose.Subscribe(OnActorStatusLose),
            WorldState.Actors.IconAppeared.Subscribe(OnActorIcon),
            WorldState.Actors.CastEvent.Subscribe(OnActorCastEvent),
            WorldState.Actors.EventObjectStateChange.Subscribe(OnActorEState),
            WorldState.Actors.EventObjectAnimation.Subscribe(OnActorEAnim),
            WorldState.Actors.PlayActionTimelineEvent.Subscribe(OnActorPlayActionTimelineEvent),
            WorldState.Actors.EventNpcYell.Subscribe(OnActorNpcYell),
            WorldState.Actors.ModelStateChanged.Subscribe(OnActorModelStateChange),
            WorldState.EnvControl.Subscribe(OnEnvControl),
            WorldState.DirectorUpdate.Subscribe(OnDirectorUpdate)
        );

        foreach (var v in WorldState.Actors)
            OnActorCreated(v);
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
    }

    public void Update()
    {
        if (StateMachine.ActivePhaseIndex < 0 && CheckPull())
            StateMachine.Start(WorldState.CurrentTime);

        if (StateMachine.ActiveState != null)
            StateMachine.Update(WorldState.CurrentTime);

        if (StateMachine.ActiveState != null)
        {
            UpdateModule();
            foreach (var comp in _components)
                comp.Update();
        }
    }

    public void Draw(Angle cameraAzimuth, int pcSlot, bool includeText, bool includeArena)
    {
        var pc = Raid[pcSlot];
        if (pc == null)
            return;

        var pcHints = CalculateHintsForRaidMember(pcSlot, pc);
        if (includeText)
        {
            if (WindowConfig.ShowMechanicTimers)
                StateMachine.Draw();

            if (WindowConfig.ShowGlobalHints)
                DrawGlobalHints(CalculateGlobalHints());

            if (WindowConfig.ShowPlayerHints)
                DrawPlayerHints(pcHints);
        }
        if (includeArena)
        {
            Arena.Begin(cameraAzimuth);
            DrawArena(pcSlot, pc, pcHints.Any(h => h.Item2));
            Arena.End();
        }
    }

    public virtual void DrawArena(int pcSlot, Actor pc, bool haveRisks)
    {
        // draw background
        DrawArenaBackground(pcSlot, pc);
        foreach (var comp in _components)
            comp.DrawArenaBackground(pcSlot, pc);

        // draw borders
        if (WindowConfig.ShowBorder)
            Arena.Border(haveRisks && WindowConfig.ShowBorderRisk ? ArenaColor.Enemy : ArenaColor.Border);
        if (WindowConfig.ShowCardinals)
            Arena.CardinalNames();
        if (WindowConfig.ShowWaymarks)
            DrawWaymarks();

        // draw non-player alive party members
        DrawPartyMembers(pcSlot, pc);

        // draw foreground
        DrawArenaForeground(pcSlot, pc);
        foreach (var comp in _components)
            comp.DrawArenaForeground(pcSlot, pc);

        // draw enemies & player
        DrawEnemies(pcSlot, pc);
        Arena.Actor(pc, ArenaColor.PC, true);
    }

    public BossComponent.TextHints CalculateHintsForRaidMember(int slot, Actor actor)
    {
        BossComponent.TextHints hints = [];
        foreach (var comp in _components)
            comp.AddHints(slot, actor, hints);
        return hints;
    }

    public BossComponent.MovementHints CalculateMovementHintsForRaidMember(int slot, Actor actor)
    {
        BossComponent.MovementHints hints = [];
        foreach (var comp in _components)
            comp.AddMovementHints(slot, actor, hints);
        return hints;
    }

    public BossComponent.GlobalHints CalculateGlobalHints()
    {
        BossComponent.GlobalHints hints = [];
        foreach (var comp in _components)
            comp.AddGlobalHints(hints);
        return hints;
    }

    public void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.Center = Center;
        hints.Bounds = Bounds;
        foreach (var comp in _components)
            comp.AddAIHints(slot, actor, assignment, hints);
        CalculateModuleAIHints(slot, actor, assignment, hints);
        if (!WindowConfig.AllowAutomaticActions)
            hints.ActionsToExecute.Clear();
    }

    public virtual bool NeedToJump(WPos from, WDir dir) => false; // if arena has complicated shape that requires jumps to navigate, module can provide this info to AI

    public void ReportError(BossComponent? comp, string message)
    {
        Service.Log($"[ModuleError] [{GetType().Name}] [{comp?.GetType().Name}] {message}");
        Error.Fire(this, comp, message);
    }

    // utility to calculate expected time when cast finishes (plus an optional delay); returns fallback value if argument is null
    // for whatever reason, npc spells have reported remaining cast time consistently 0.3s smaller than reality - this delta is added automatically, in addition to optional delay
    public DateTime CastFinishAt(ActorCastInfo? cast, float extraDelay = 0, DateTime fallback = default) => cast != null ? WorldState.FutureTime(cast.NPCRemainingTime + extraDelay) : fallback;

    // called during update if module is not yet active, should return true if it is to be activated
    // default implementation activates if primary target is both targetable and in combat
    protected virtual bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat; }

    protected virtual void UpdateModule() { }
    protected virtual void DrawArenaBackground(int pcSlot, Actor pc) { } // before modules background
    protected virtual void DrawArenaForeground(int pcSlot, Actor pc) { } // after border, before modules foreground
    protected virtual void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }

    // called at the very end to draw important enemies, default implementation draws primary actor
    protected virtual void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
    }

    private void DrawGlobalHints(BossComponent.GlobalHints hints)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, 0xffffff00);
        foreach (var hint in hints)
        {
            ImGui.TextUnformatted(hint);
            ImGui.SameLine();
        }
        ImGui.PopStyleColor();
        ImGui.NewLine();
    }

    private void DrawPlayerHints(BossComponent.TextHints hints)
    {
        foreach ((var hint, bool risk) in hints)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, risk ? ArenaColor.Danger : ArenaColor.Safe);
            ImGui.TextUnformatted(hint);
            ImGui.PopStyleColor();
            ImGui.SameLine();
        }
        ImGui.NewLine();
    }

    private void DrawWaymarks()
    {
        DrawWaymark(WorldState.Waymarks[Waymark.A], "A", 0xff964ee5);
        DrawWaymark(WorldState.Waymarks[Waymark.B], "B", 0xff11a2c6);
        DrawWaymark(WorldState.Waymarks[Waymark.C], "C", 0xffe29f30);
        DrawWaymark(WorldState.Waymarks[Waymark.D], "D", 0xffbc567a);
        DrawWaymark(WorldState.Waymarks[Waymark.N1], "1", 0xff964ee5);
        DrawWaymark(WorldState.Waymarks[Waymark.N2], "2", 0xff11a2c6);
        DrawWaymark(WorldState.Waymarks[Waymark.N3], "3", 0xffe29f30);
        DrawWaymark(WorldState.Waymarks[Waymark.N4], "4", 0xffbc567a);
    }

    private void DrawWaymark(Vector3? pos, string text, uint color)
    {
        if (pos != null)
        {
            if (WindowConfig.ShowOutlinesAndShadows)
                Arena.TextWorld(new(pos.Value.XZ()), text, 0xFF000000, 25);
            Arena.TextWorld(new(pos.Value.XZ()), text, color, 22);
        }
    }

    private void DrawPartyMembers(int pcSlot, Actor pc)
    {
        foreach (var (slot, player) in Raid.WithSlot().Exclude(pcSlot))
        {
            var (prio, color) = CalculateHighestPriority(pcSlot, pc, slot, player);
            if (prio == BossComponent.PlayerPriority.Irrelevant && !WindowConfig.ShowIrrelevantPlayers)
                continue;

            if (color == 0)
            {
                color = prio switch
                {
                    BossComponent.PlayerPriority.Interesting => ArenaColor.PlayerInteresting,
                    BossComponent.PlayerPriority.Danger => ArenaColor.Danger,
                    BossComponent.PlayerPriority.Critical => ArenaColor.Vulnerable, // TODO: select some better color...
                    _ => ArenaColor.PlayerGeneric
                };
            }
            Arena.Actor(player, color);
        }
    }

    private (BossComponent.PlayerPriority, uint) CalculateHighestPriority(int pcSlot, Actor pc, int playerSlot, Actor player)
    {
        uint color = 0;
        var highestPrio = BossComponent.PlayerPriority.Irrelevant;
        foreach (var s in _components)
        {
            uint subColor = 0;
            var subPrio = s.CalcPriority(pcSlot, pc, playerSlot, player, ref subColor);
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
        _relevantEnemies.GetValueOrDefault(actor.OID)?.Add(actor);
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo)
            foreach (var comp in _components)
                comp.OnActorCreated(actor);
    }

    private void OnActorDestroyed(Actor actor)
    {
        _relevantEnemies.GetValueOrDefault(actor.OID)?.Remove(actor);
        if (actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo)
            foreach (var comp in _components)
                comp.OnActorDestroyed(actor);
    }

    private void OnActorCastStarted(Actor actor)
    {
        if ((actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo) && (actor.CastInfo?.IsSpell() ?? false))
            foreach (var comp in _components)
                comp.OnCastStarted(actor, actor.CastInfo);
    }

    private void OnActorCastFinished(Actor actor)
    {
        if ((actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo) && (actor.CastInfo?.IsSpell() ?? false))
            foreach (var comp in _components)
                comp.OnCastFinished(actor, actor.CastInfo);
    }

    private void OnActorTethered(Actor actor)
    {
        foreach (var comp in _components)
            comp.OnTethered(actor, actor.Tether);
    }

    private void OnActorUntethered(Actor actor)
    {
        foreach (var comp in _components)
            comp.OnUntethered(actor, actor.Tether);
    }

    private void OnActorStatusGain(Actor actor, int index)
    {
        foreach (var comp in _components)
            comp.OnStatusGain(actor, actor.Statuses[index]);
    }

    private void OnActorStatusLose(Actor actor, int index)
    {
        foreach (var comp in _components)
            comp.OnStatusLose(actor, actor.Statuses[index]);
    }

    private void OnActorIcon(Actor actor, uint iconID)
    {
        foreach (var comp in _components)
            comp.OnEventIcon(actor, iconID);
    }

    private void OnActorCastEvent(Actor actor, ActorCastEvent cast)
    {
        if ((actor.Type is not ActorType.Player and not ActorType.Pet and not ActorType.Chocobo) && cast.IsSpell())
            foreach (var comp in _components)
                comp.OnEventCast(actor, cast);
    }

    private void OnActorEState(Actor actor, ushort state)
    {
        foreach (var comp in _components)
            comp.OnActorEState(actor, state);
    }

    private void OnActorEAnim(Actor actor, ushort p1, ushort p2)
    {
        uint state = ((uint)p1 << 16) | p2;
        foreach (var comp in _components)
            comp.OnActorEAnim(actor, state);
    }

    private void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        foreach (var comp in _components)
            comp.OnActorPlayActionTimelineEvent(actor, id);
    }

    private void OnActorNpcYell(Actor actor, ushort id)
    {
        foreach (var comp in _components)
            comp.OnActorNpcYell(actor, id);
    }

    private void OnActorModelStateChange(Actor actor)
    {
        foreach (var comp in _components)
            comp.OnActorModelStateChange(actor, actor.ModelState.ModelState, actor.ModelState.AnimState1, actor.ModelState.AnimState2);
    }

    private void OnEnvControl(WorldState.OpEnvControl op)
    {
        foreach (var comp in _components)
            comp.OnEventEnvControl(op.Index, op.State);
    }

    private void OnDirectorUpdate(WorldState.OpDirectorUpdate op)
    {
        foreach (var comp in _components)
            comp.OnEventDirectorUpdate(op.UpdateID, op.Param1, op.Param2, op.Param3, op.Param4);
    }
}
