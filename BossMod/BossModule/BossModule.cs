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

        public WorldState.Actor?[] RaidMembers; // this is fixed-size, but some slots could be empty; when player is removed, gap is created - existing players keep their indices
        public int PlayerSlot { get; private set; } = -1;
        public WorldState.Actor? RaidMember(int slot) => (slot >= 0 && slot < RaidMembers.Length) ? RaidMembers[slot] : null; // bounds-checking accessor
        public WorldState.Actor? Player() => RaidMember(PlayerSlot);

        public StateMachine StateMachine { get; init; } = new();
        public StateMachine.State? InitialState = null;
        public MiniArena Arena { get; init; } = new();

        public class RelevantEnemy
        {
            public List<WorldState.Actor> Actors;
            public bool Unique; // if true, actors list will always contain 0 or 1 elements

            public RelevantEnemy(List<WorldState.Actor> actors, bool unique = false)
            {
                Actors = actors;
                Unique = unique;
            }
        }
        private IReadOnlyDictionary<uint, RelevantEnemy> _relevantEnemies; // key = actor OID

        // different encounter mechanics can be split into independent components
        public class Component
        {
            public Component(BossModule module)
            {
                module._components.Add(this);
            }

            public virtual void Reset() { } // called when module is reset
            public virtual void Update() { } // called every frame - it is a good place to update any cached values
            public virtual void AddHints(int slot, WorldState.Actor actor, List<string> hints) { } // gather any relevant pieces of advice for specified raid member
            public virtual void DrawArenaBackground(MiniArena arena) { } // called at the beginning of arena draw, good place to draw aoe zones
            public virtual void DrawArenaForeground(MiniArena arena) { } // called after arena background and borders are drawn, good place to draw actors, tethers, etc.

            // world state event handlers
            public virtual void OnStatusGain(WorldState.Actor actor, int index) { }
            public virtual void OnStatusLose(WorldState.Actor actor, int index) { }
            public virtual void OnCastStarted(WorldState.Actor actor) { }
            public virtual void OnCastFinished(WorldState.Actor actor) { }
            public virtual void OnEventCast(WorldState.CastResult info) { }
            public virtual void OnEventIcon(uint actorID, uint iconID) { }
            public virtual void OnEventEnvControl(uint featureID, byte index, uint state) { }
        }
        private List<Component> _components = new();

        public BossModule(WorldState w, int maxRaidMembers)
        {
            WorldState = w;
            RaidMembers = new WorldState.Actor?[maxRaidMembers];
            _relevantEnemies = DefineRelevantEnemies();

            WorldState.PlayerActorIDChanged += PlayerIDChanged;
            WorldState.PlayerInCombatChanged += EnterExitCombat;
            WorldState.ActorCreated += OnActorCreated;
            WorldState.ActorDestroyed += OnActorDestroyed;
            WorldState.ActorCastStarted += OnActorCastStarted;
            WorldState.ActorCastFinished += OnActorCastFinished;
            WorldState.ActorStatusGain += OnActorStatusGain;
            WorldState.ActorStatusLose += OnActorStatusLose;
            WorldState.EventIcon += OnEventIcon;
            WorldState.EventCast += OnEventCast;
            WorldState.EventEnvControl += OnEventEnvControl;
            foreach (var v in WorldState.Actors)
                OnActorCreated(null, v.Value);
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
                WorldState.PlayerActorIDChanged -= PlayerIDChanged;
                WorldState.PlayerInCombatChanged -= EnterExitCombat;
                WorldState.ActorCreated -= OnActorCreated;
                WorldState.ActorDestroyed -= OnActorDestroyed;
                WorldState.ActorCastStarted -= OnActorCastStarted;
                WorldState.ActorCastFinished -= OnActorCastFinished;
                WorldState.ActorStatusGain -= OnActorStatusGain;
                WorldState.ActorStatusLose -= OnActorStatusLose;
                WorldState.EventIcon -= OnEventIcon;
                WorldState.EventCast -= OnEventCast;
                WorldState.EventEnvControl -= OnEventEnvControl;
            }
        }

        public virtual void Reset()
        {
            ResetModule();
            foreach (var comp in _components)
                comp.Reset();
        }

        public virtual void Update()
        {
            StateMachine.Update();
            UpdateModule();
            foreach (var comp in _components)
                comp.Update();
        }

        public virtual void Draw(float cameraAzimuth)
        {
            StateMachine.Draw();
            DrawHintForRaidMember(PlayerSlot);
            DrawHeader();
            Arena.Begin(cameraAzimuth);
            DrawArena();
            Arena.End();
            DrawFooter();

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

        public virtual void DrawArena()
        {
            DrawArenaBackground();
            foreach (var comp in _components)
                comp.DrawArenaBackground(Arena);
            Arena.Border();
            DrawArenaForegroundPre();
            foreach (var comp in _components)
                comp.DrawArenaForeground(Arena);
            DrawArenaForegroundPost();
        }

        public void DrawHintForRaidMember(int slot)
        {
            var actor = RaidMember(slot);
            if (actor == null)
                return;

            List<string> hints = new();
            foreach (var comp in _components)
                comp.AddHints(slot, actor, hints);
            AddHints(slot, actor, hints);

            var hintColor = ImGui.ColorConvertU32ToFloat4(0xff00ffff);
            foreach (var hint in hints)
            {
                ImGui.TextColored(hintColor, hint);
                ImGui.SameLine();
            }
            ImGui.NewLine();
        }

        public int FindRaidMemberSlot(uint instanceID)
        {
            return instanceID != 0 ? Array.FindIndex(RaidMembers, x => x != null && x.InstanceID == instanceID) : -1;
        }

        // iterate over raid members, skipping empty slots and optionally dead players
        public IEnumerable<(int, WorldState.Actor)> IterateRaidMembers(bool includeDead = false)
        {
            for (int i = 0; i < RaidMembers.Length; ++i)
            {
                var player = RaidMembers[i];
                if (player == null)
                    continue;
                if (player.IsDead && !includeDead)
                    continue;
                yield return (i, player);
            }
        }

        public ulong BuildMask(IEnumerable<(int, WorldState.Actor)> players)
        {
            ulong mask = 0;
            foreach ((var i, _) in players)
                BitVector.SetVector64Bit(ref mask, i);
            return mask;
        }

        // note that overloads accepting actor ignore self; use overload accepting position if self is needed
        public IEnumerable<(int, WorldState.Actor)> IterateRaidMembersInRange(Vector3 position, float radius, bool includeDead = false)
        {
            var rsq = radius * radius;
            return IterateRaidMembers(includeDead).Where(indexActor => (indexActor.Item2.Position - position).LengthSquared() <= rsq);
        }

        public IEnumerable<(int, WorldState.Actor)> IterateRaidMembersInRange(int slot, float radius, bool includeDead = false)
        {
            var actor = RaidMember(slot);
            if (actor == null)
                return Enumerable.Empty<(int, WorldState.Actor)>();
            var rsq = radius * radius;
            return IterateRaidMembers(includeDead).Where(indexActor => indexActor.Item1 != slot && (indexActor.Item2.Position - actor.Position).LengthSquared() <= rsq);
        }

        public ulong FindRaidMembersInRange(Vector3 position, float radius, bool includeDead = false)
        {
            return BuildMask(IterateRaidMembersInRange(position, radius, includeDead));
        }

        public ulong FindRaidMembersInRange(int slot, float radius, bool includeDead = false)
        {
            return BuildMask(IterateRaidMembersInRange(slot, radius, includeDead));
        }

        protected virtual void ResetModule() { }
        protected virtual void UpdateModule() { }
        protected virtual void AddHints(int slot, WorldState.Actor actor, List<string> hints) { } // temporary
        protected virtual void DrawHeader() { } // deprecated, will be removed after refactoring
        protected virtual void DrawArenaBackground() { } // before modules background
        protected virtual void DrawArenaForegroundPre() { } // after border, before modules foreground
        protected virtual void DrawArenaForegroundPost() { } // after modules foreground
        protected virtual void DrawFooter() { }
        protected virtual Dictionary<uint, RelevantEnemy> DefineRelevantEnemies() { return new(); } // called once by constructor, boss mod can provide lists for interesting enemies that will be automatically managed
        protected virtual void NonPlayerCreated(WorldState.Actor actor) { }
        protected virtual void NonPlayerDestroyed(WorldState.Actor actor) { }
        protected virtual void RaidMemberCreated(int index) { }
        protected virtual void RaidMemberDestroyed(int index) { } // called just before slot is cleared, so still contains old actor

        private void PlayerIDChanged(object? sender, uint id)
        {
            PlayerSlot = FindRaidMemberSlot(id);
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
            }
        }

        private void OnActorCreated(object? sender, WorldState.Actor actor)
        {
            if (actor.Type == WorldState.ActorType.Player)
            {
                int slot = Array.FindIndex(RaidMembers, x => x == null);
                if (slot != -1)
                {
                    RaidMembers[slot] = actor;
                    if (actor.InstanceID == WorldState.PlayerActorID)
                        PlayerSlot = slot;
                    RaidMemberCreated(slot);
                }
                else
                {
                    Service.Log($"[BossModule] Too many raid members: {RaidMembers.Length} already exist; skipping new actor {actor.InstanceID:X}");
                }
            }
            else
            {
                var relevant = _relevantEnemies.GetValueOrDefault(actor.OID);
                if (relevant != null)
                {
                    if (relevant.Unique && relevant.Actors.Count > 0)
                    {
                        Service.Log($"[BossModule] Got multiple instances of {actor.OID}, however it is expected to be unique; replacing {relevant.Actors[0].InstanceID:X} with {actor.InstanceID:X}");
                        relevant.Actors.Clear();
                    }
                    relevant.Actors.Add(actor);
                }

                NonPlayerCreated(actor);
            }
        }

        private void OnActorDestroyed(object? sender, WorldState.Actor actor)
        {
            if (actor.Type == WorldState.ActorType.Player)
            {
                int slot = FindRaidMemberSlot(actor.InstanceID);
                if (slot != -1)
                {
                    RaidMemberDestroyed(slot);
                    RaidMembers[slot] = null;
                    if (PlayerSlot == slot)
                        PlayerSlot = -1;
                }
                else
                {
                    Service.Log($"[BossModule] Destroyed player actor {actor.InstanceID:X} not found among raid members");
                }
            }
            else
            {
                var relevant = _relevantEnemies.GetValueOrDefault(actor.OID);
                if (relevant != null)
                {
                    relevant.Actors.Remove(actor);
                }

                NonPlayerDestroyed(actor);
            }
        }

        private void OnActorCastStarted(object? sender, WorldState.Actor actor)
        {
            foreach (var comp in _components)
                comp.OnCastStarted(actor);
        }

        private void OnActorCastFinished(object? sender, WorldState.Actor actor)
        {
            foreach (var comp in _components)
                comp.OnCastFinished(actor);
        }

        private void OnActorStatusGain(object? sender, (WorldState.Actor actor, int index) arg)
        {
            foreach (var comp in _components)
                comp.OnStatusGain(arg.actor, arg.index);
        }

        private void OnActorStatusLose(object? sender, (WorldState.Actor actor, int index) arg)
        {
            foreach (var comp in _components)
                comp.OnStatusLose(arg.actor, arg.index);
        }

        private void OnEventIcon(object? sender, (uint actorID, uint iconID) arg)
        {
            foreach (var comp in _components)
                comp.OnEventIcon(arg.actorID, arg.iconID);
        }

        private void OnEventCast(object? sender, WorldState.CastResult info)
        {
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
