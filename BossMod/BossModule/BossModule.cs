using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    // base for boss modules - provides all the common features, so that look is standardized
    public class BossModule : IDisposable
    {
        public WorldState WorldState { get; init; }
        public StateMachine StateMachine { get; init; } = new();
        public StateMachine.State? InitialState = null;
        public MiniArena Arena { get; init; } = new();
        public RaidCooldowns RaidCooldowns { get; init; } = new();

        public PartyState Raid => WorldState.Party;

        public bool ShowStateMachine = true;
        public bool ShowGlobalHints = true;
        public bool ShowPlayerHints = true;
        public bool ShowControlButtons = true;
        public bool ShowWaymarks = true;

        // per-oid enemy lists; filled on first request
        private Dictionary<uint, List<Actor>> _relevantEnemies = new(); // key = actor OID
        public IReadOnlyDictionary<uint, List<Actor>> RelevantEnemies => _relevantEnemies;
        public List<Actor> Enemies<OID>(OID oid) where OID : Enum
        {
            var castOID = (uint)(object)oid;
            var entry = _relevantEnemies.GetValueOrDefault(castOID);
            if (entry == null)
            {
                entry = new();
                foreach (var actor in WorldState.Actors.Where(actor => actor.OID == castOID))
                    entry.Add(actor);
                _relevantEnemies[castOID] = entry;
            }
            return entry;
        }

        // list of actor-specific hints (string + whether this is a "risk" type of hint)
        public class TextHints : List<(string, bool)>
        {
            public void Add(string text, bool isRisk = true) => base.Add((text, isRisk));
        }

        // list of actor-specific "movement hints" (arrow start/end pos + color)
        public class MovementHints : List<(Vector3, Vector3, uint)>
        {
            public void Add(Vector3 from, Vector3 to, uint color) => base.Add((from, to, color));
        }

        // list of global hints
        public class GlobalHints : List<string> { }

        // different encounter mechanics can be split into independent components
        // individual components should be activated and deactivated when needed (typically by state machine transitions)
        public class Component
        {
            public virtual void Update() { } // called every frame - it is a good place to update any cached values
            public virtual void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints) { } // gather any relevant pieces of advice for specified raid member
            public virtual void AddGlobalHints(GlobalHints hints) { } // gather any relevant pieces of advice for whole raid
            public virtual void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena) { } // called at the beginning of arena draw, good place to draw aoe zones
            public virtual void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena) { } // called after arena background and borders are drawn, good place to draw actors, tethers, etc.

            // world state event handlers
            public virtual void OnStatusGain(Actor actor, int index) { }
            public virtual void OnStatusLose(Actor actor, int index) { }
            public virtual void OnStatusChange(Actor actor, int index) { }
            public virtual void OnTethered(Actor actor) { }
            public virtual void OnUntethered(Actor actor) { }
            public virtual void OnCastStarted(Actor actor) { }
            public virtual void OnCastFinished(Actor actor) { }
            public virtual void OnEventCast(CastEvent info) { }
            public virtual void OnEventIcon(uint actorID, uint iconID) { }
            public virtual void OnEventEnvControl(uint featureID, byte index, uint state) { }
        }
        private List<Component> _components = new();
        public IReadOnlyList<Component> Components => _components;

        protected T ActivateComponent<T>(T comp) where T : Component
        {
            if (FindComponent<T>() != null)
            {
                Service.Log($"[BossModule] Activating a component of type {comp.GetType()} when another of the same type is already active; old one is deactivated automatically");
                DeactivateComponent<T>();
            }
            _components.Add(comp);
            foreach (var actor in WorldState.Actors)
            {
                if (actor.CastInfo != null)
                    comp.OnCastStarted(actor);
                if (actor.Tether.ID != 0)
                    comp.OnTethered(actor);
                for (int i = 0; i < actor.Statuses.Length; ++i)
                    if (actor.Statuses[i].ID != 0)
                        comp.OnStatusGain(actor, i);
            }
            return comp;
        }

        protected void DeactivateComponent<T>() where T : Component
        {
            int count = _components.RemoveAll(x => x is T);
            if (count == 0)
                Service.Log($"[BossModule] Could not find a component of type {typeof(T)} to deactivate");
        }

        public T? FindComponent<T>() where T : Component
        {
            return _components.OfType<T>().FirstOrDefault();
        }

        public BossModule(WorldState w)
        {
            WorldState = w;
            WorldState.PlayerInCombatChanged += EnterExitCombat;
            WorldState.Actors.Added += OnActorCreated;
            WorldState.Actors.Removed += OnActorDestroyed;
            WorldState.Actors.CastStarted += OnActorCastStarted;
            WorldState.Actors.CastFinished += OnActorCastFinished;
            WorldState.Actors.Tethered += OnActorTethered;
            WorldState.Actors.Untethered += OnActorUntethered;
            WorldState.Actors.StatusGain += OnActorStatusGain;
            WorldState.Actors.StatusLose += OnActorStatusLose;
            WorldState.Actors.StatusChange += OnActorStatusChange;
            WorldState.Events.Icon += OnEventIcon;
            WorldState.Events.Cast += OnEventCast;
            WorldState.Events.EnvControl += OnEventEnvControl;
            foreach (var v in WorldState.Actors)
                OnActorCreated(null, v);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                WorldState.PlayerInCombatChanged -= EnterExitCombat;
                WorldState.Actors.Added -= OnActorCreated;
                WorldState.Actors.Removed -= OnActorDestroyed;
                WorldState.Actors.CastStarted -= OnActorCastStarted;
                WorldState.Actors.CastFinished -= OnActorCastFinished;
                WorldState.Actors.Tethered -= OnActorTethered;
                WorldState.Actors.Untethered -= OnActorUntethered;
                WorldState.Actors.StatusGain -= OnActorStatusGain;
                WorldState.Actors.StatusLose -= OnActorStatusLose;
                WorldState.Actors.StatusChange -= OnActorStatusChange;
                WorldState.Events.Icon -= OnEventIcon;
                WorldState.Events.Cast -= OnEventCast;
                WorldState.Events.EnvControl -= OnEventEnvControl;
            }
        }

        public virtual void Reset()
        {
            _components.Clear();
            ResetModule();
        }

        public virtual void Update()
        {
            StateMachine.Update(WorldState.CurrentTime);
            UpdateModule();
            foreach (var comp in _components)
                comp.Update();
        }

        public virtual void Draw(float cameraAzimuth, int pcSlot, MovementHints? pcMovementHints)
        {
            if (ShowStateMachine)
                StateMachine.Draw();

            if (ShowGlobalHints)
                DrawGlobalHints();

            if (ShowPlayerHints)
                DrawHintForPlayer(pcSlot, pcMovementHints);

            Arena.Begin(cameraAzimuth);
            DrawArena(pcSlot);
            Arena.End();

            if (ShowControlButtons)
            {
                if (ImGui.Button("Show timeline"))
                {
                    var timeline = new StateMachineVisualizer(InitialState);
                    var w = WindowManager.CreateWindow($"{GetType()} Timeline", () => timeline.Draw(StateMachine), () => { });
                    w.SizeHint = new(600, 600);
                    w.MinSize = new(100, 100);
                }
                ImGui.SameLine();
                if (ImGui.Button("Force transition") && StateMachine.ActiveState?.Next != null)
                {
                    StateMachine.ActiveState = StateMachine.ActiveState.Next;
                }
            }
        }

        public virtual void DrawArena(int pcSlot)
        {
            var pc = Raid[pcSlot];
            if (pc == null)
                return;

            DrawArenaBackground(pcSlot, pc);
            foreach (var comp in _components)
                comp.DrawArenaBackground(pcSlot, pc, Arena);
            Arena.Border();
            if (ShowWaymarks)
                DrawWaymarks();
            DrawArenaForegroundPre(pcSlot, pc);
            foreach (var comp in _components)
                comp.DrawArenaForeground(pcSlot, pc, Arena);
            DrawArenaForegroundPost(pcSlot, pc);
        }

        public TextHints CalculateHintsForRaidMember(int slot, Actor actor, MovementHints? movementHints = null)
        {
            TextHints hints = new();
            foreach (var comp in _components)
                comp.AddHints(slot, actor, hints, movementHints);
            return hints;
        }

        public GlobalHints CalculateGlobalHints()
        {
            GlobalHints hints = new();
            foreach (var comp in _components)
                comp.AddGlobalHints(hints);
            return hints;
        }

        // TODO: move to some better place...
        public static Vector3 AdjustPositionForKnockback(Vector3 pos, Vector3 origin, float distance)
        {
            return pos != origin ? pos + distance * Vector3.Normalize(pos - origin) : pos;
        }

        public static Vector3 AdjustPositionForKnockback(Vector3 pos, Actor? source, float distance)
        {
            return source != null ? AdjustPositionForKnockback(pos, source.Position, distance) : pos;
        }

        protected virtual void ResetModule() { }
        protected virtual void UpdateModule() { }
        protected virtual void DrawArenaBackground(int pcSlot, Actor pc) { } // before modules background
        protected virtual void DrawArenaForegroundPre(int pcSlot, Actor pc) { } // after border, before modules foreground
        protected virtual void DrawArenaForegroundPost(int pcSlot, Actor pc) { } // after modules foreground

        private void DrawGlobalHints()
        {
            var hints = CalculateGlobalHints();
            var hintColor = ImGui.ColorConvertU32ToFloat4(0xffff8000);
            foreach (var hint in hints)
            {
                ImGui.TextColored(hintColor, hint);
                ImGui.SameLine();
            }
            ImGui.NewLine();
        }

        private void DrawHintForPlayer(int pcSlot, MovementHints? movementHints)
        {
            var pc = Raid[pcSlot];
            if (pc == null)
                return;

            var hints = CalculateHintsForRaidMember(pcSlot, pc, movementHints);
            var riskColor = ImGui.ColorConvertU32ToFloat4(Arena.ColorDanger);
            var safeColor = ImGui.ColorConvertU32ToFloat4(Arena.ColorSafe);
            foreach ((var hint, bool risk) in hints)
            {
                ImGui.TextColored(risk ? riskColor : safeColor, hint);
                ImGui.SameLine();
            }
            ImGui.NewLine();
        }

        private void DrawWaymarks()
        {
            DrawWaymark(WorldState.Waymarks[Waymark.A], "A", 0xffba4e53);
            DrawWaymark(WorldState.Waymarks[Waymark.B], "B", 0xffd5aa39);
            DrawWaymark(WorldState.Waymarks[Waymark.C], "C", 0xff6cb4e0);
            DrawWaymark(WorldState.Waymarks[Waymark.D], "D", 0xff7128c0);
            DrawWaymark(WorldState.Waymarks[Waymark.N1], "1", 0xffba4e53);
            DrawWaymark(WorldState.Waymarks[Waymark.N2], "2", 0xffd5aa39);
            DrawWaymark(WorldState.Waymarks[Waymark.N3], "3", 0xff6cb4e0);
            DrawWaymark(WorldState.Waymarks[Waymark.N4], "4", 0xff7128c0);
        }

        private void DrawWaymark(Vector3? pos, string text, uint color)
        {
            if (pos != null)
            {
                Arena.TextWorld(pos.Value, text, color, 22);
            }
        }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            if (inCombat)
            {
                Reset();
                StateMachine.ActiveState = InitialState;
            }
            else
            {
                StateMachine.ActiveState = null;
                Reset();
                RaidCooldowns.Clear();
            }
        }

        private void OnActorCreated(object? sender, Actor actor)
        {
            switch (actor.Type)
            {
                case ActorType.Unknown:
                case ActorType.Enemy:
                    var relevant = _relevantEnemies.GetValueOrDefault(actor.OID);
                    if (relevant != null)
                        relevant.Add(actor);
                    break;
            }
        }

        private void OnActorDestroyed(object? sender, Actor actor)
        {
            switch (actor.Type)
            {
                case ActorType.Unknown:
                case ActorType.Enemy:
                    var relevant = _relevantEnemies.GetValueOrDefault(actor.OID);
                    if (relevant != null)
                        relevant.Remove(actor);
                    break;
            }
        }

        private void OnActorCastStarted(object? sender, Actor actor)
        {
            foreach (var comp in _components)
                comp.OnCastStarted(actor);
        }

        private void OnActorCastFinished(object? sender, Actor actor)
        {
            foreach (var comp in _components)
                comp.OnCastFinished(actor);
        }

        private void OnActorTethered(object? sender, Actor actor)
        {
            foreach (var comp in _components)
                comp.OnTethered(actor);
        }

        private void OnActorUntethered(object? sender, Actor actor)
        {
            foreach (var comp in _components)
                comp.OnUntethered(actor);
        }

        private void OnActorStatusGain(object? sender, (Actor actor, int index) arg)
        {
            foreach (var comp in _components)
                comp.OnStatusGain(arg.actor, arg.index);
        }

        private void OnActorStatusLose(object? sender, (Actor actor, int index) arg)
        {
            foreach (var comp in _components)
                comp.OnStatusLose(arg.actor, arg.index);
        }

        private void OnActorStatusChange(object? sender, (Actor actor, int index, ushort prevExtra, DateTime prevExpire) arg)
        {
            foreach (var comp in _components)
                comp.OnStatusChange(arg.actor, arg.index);
        }

        private void OnEventIcon(object? sender, (uint actorID, uint iconID) arg)
        {
            foreach (var comp in _components)
                comp.OnEventIcon(arg.actorID, arg.iconID);
        }

        private void OnEventCast(object? sender, CastEvent info)
        {
            RaidCooldowns.HandleCast(WorldState.CurrentTime, WorldState.Party, info);
            foreach (var comp in _components)
                comp.OnEventCast(info);
        }

        private void OnEventEnvControl(object? sender, (uint featureID, byte index, uint state) arg)
        {
            foreach (var comp in _components)
                comp.OnEventEnvControl(arg.featureID, arg.index, arg.state);
        }
    }
}
