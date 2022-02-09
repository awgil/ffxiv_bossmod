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

        public WorldState.Actor?[] RaidMembers; // this is fixed-size, but some slots could be empty; when player is removed, gap is created - existing players keep their indices
        public int PlayerSlot { get; private set; } = -1;
        public WorldState.Actor? RaidMember(int slot) => (slot >= 0 && slot < RaidMembers.Length) ? RaidMembers[slot] : null; // bounds-checking accessor
        public WorldState.Actor? Player() => RaidMember(PlayerSlot);

        private class EnemyList
        {
            public List<WorldState.Actor> Actors = new();
            public bool Unique; // if true, actors list will always contain 0 or 1 elements
        }
        private Dictionary<uint, EnemyList> _relevantEnemies = new(); // key = actor OID

        // retrieve watched enemy list; it is filled on first request
        // all requests should have matching 'unique' flag
        public List<WorldState.Actor> Enemies<OID>(OID oid, bool unique = false) where OID : Enum
        {
            var castOID = (uint)(object)oid;
            var entry = _relevantEnemies.GetValueOrDefault(castOID);
            if (entry == null)
            {
                entry = new EnemyList() { Unique = unique };
                foreach (var actor in WorldState.Actors.Values.Where(actor => actor.OID == castOID))
                    AddRelevantEnemy(entry, actor);
                _relevantEnemies[castOID] = entry;
            }
            else if (entry.Unique != unique)
            {
                Service.Log($"[BossModule] Mismatched unique flag for list of {oid}");
            }
            return entry.Actors;
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

        // different encounter mechanics can be split into independent components
        public class Component
        {
            public virtual void Reset() { } // called when module is reset
            public virtual void Update() { } // called every frame - it is a good place to update any cached values
            public virtual void AddHints(int slot, WorldState.Actor actor, TextHints hints, MovementHints? movementHints) { } // gather any relevant pieces of advice for specified raid member
            public virtual void DrawArenaBackground(MiniArena arena) { } // called at the beginning of arena draw, good place to draw aoe zones
            public virtual void DrawArenaForeground(MiniArena arena) { } // called after arena background and borders are drawn, good place to draw actors, tethers, etc.

            // world state event handlers
            public virtual void OnStatusGain(WorldState.Actor actor, int index) { }
            public virtual void OnStatusLose(WorldState.Actor actor, int index) { }
            public virtual void OnTethered(WorldState.Actor actor) { }
            public virtual void OnUntethered(WorldState.Actor actor) { }
            public virtual void OnCastStarted(WorldState.Actor actor) { }
            public virtual void OnCastFinished(WorldState.Actor actor) { }
            public virtual void OnEventCast(WorldState.CastResult info) { }
            public virtual void OnEventIcon(uint actorID, uint iconID) { }
            public virtual void OnEventEnvControl(uint featureID, byte index, uint state) { }
        }
        private List<Component> _components = new();

        // register new component
        protected T RegisterComponent<T>(T comp) where T : Component
        {
            var existing = FindComponent<T>();
            if (existing == null)
            {
                existing = comp;
                _components.Add(comp);
            }
            else
            {
                Service.Log($"Trying to register component {comp.GetType()} twice");
            }
            return existing;
        }

        // find existing component by type
        public T? FindComponent<T>() where T : Component
        {
            return (T?)_components.Find(c => c is T);
        }

        public BossModule(WorldState w, int maxRaidMembers)
        {
            WorldState = w;
            RaidMembers = new WorldState.Actor?[maxRaidMembers];

            WorldState.PlayerActorIDChanged += PlayerIDChanged;
            WorldState.PlayerInCombatChanged += EnterExitCombat;
            WorldState.ActorCreated += OnActorCreated;
            WorldState.ActorDestroyed += OnActorDestroyed;
            WorldState.ActorCastStarted += OnActorCastStarted;
            WorldState.ActorCastFinished += OnActorCastFinished;
            WorldState.ActorTethered += OnActorTethered;
            WorldState.ActorUntethered += OnActorUntethered;
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
                WorldState.ActorTethered -= OnActorTethered;
                WorldState.ActorUntethered -= OnActorUntethered;
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

        public virtual void Draw(float cameraAzimuth, MovementHints? pcMovementHints)
        {
            StateMachine.Draw();
            DrawHintForPlayer(pcMovementHints);
            Arena.Begin(cameraAzimuth);
            DrawArena();
            Arena.End();

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

        public TextHints CalculateHintsForRaidMember(int slot, WorldState.Actor actor, MovementHints? movementHints = null)
        {
            TextHints hints = new();
            foreach (var comp in _components)
                comp.AddHints(slot, actor, hints, movementHints);
            return hints;
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

        // iterate over raid members other than specified
        public IEnumerable<(int, WorldState.Actor)> IterateOtherRaidMembers(int skipSlot, bool includeDead = false)
        {
            return IterateRaidMembers(includeDead).Where(indexPlayer => indexPlayer.Item1 != skipSlot);
        }

        // iterate over raid members matching condition (use instead of .Where if you don't care about index in predicate)
        public IEnumerable<(int, WorldState.Actor)> IterateRaidMembersWhere(Func<WorldState.Actor, bool> predicate, bool includeDead = false)
        {
            return IterateRaidMembers(includeDead).Where(indexActor => predicate(indexActor.Item2));
        }

        public static ulong BuildMask(IEnumerable<(int, WorldState.Actor)> players)
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
            return IterateRaidMembersWhere(actor => (actor.Position - position).LengthSquared() <= rsq, includeDead);
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

        // think where to put such utilities...
        public static Vector3 AdjustPositionForKnockback(Vector3 pos, WorldState.Actor? source, float distance)
        {
            if (source != null && source.Position != pos)
                pos += distance * Vector3.Normalize(pos - source.Position);
            return pos;
        }

        protected virtual void ResetModule() { }
        protected virtual void UpdateModule() { }
        protected virtual void DrawArenaBackground() { } // before modules background
        protected virtual void DrawArenaForegroundPre() { } // after border, before modules foreground
        protected virtual void DrawArenaForegroundPost() { } // after modules foreground
        protected virtual void NonPlayerCreated(WorldState.Actor actor) { }
        protected virtual void NonPlayerDestroyed(WorldState.Actor actor) { }
        protected virtual void RaidMemberCreated(int index) { }
        protected virtual void RaidMemberDestroyed(int index) { } // called just before slot is cleared, so still contains old actor

        private void AddRelevantEnemy(EnemyList list, WorldState.Actor actor)
        {
            if (list.Unique && list.Actors.Count > 0)
            {
                Service.Log($"[BossModule] Got multiple instances of {actor.OID}, however it is expected to be unique; replacing {list.Actors[0].InstanceID:X} with {actor.InstanceID:X}");
                list.Actors.Clear();
            }
            list.Actors.Add(actor);
        }

        private void DrawHintForPlayer(MovementHints? movementHints)
        {
            var actor = Player();
            if (actor == null)
                return;

            var hints = CalculateHintsForRaidMember(PlayerSlot, actor, movementHints);
            var riskColor = ImGui.ColorConvertU32ToFloat4(Arena.ColorDanger);
            var safeColor = ImGui.ColorConvertU32ToFloat4(Arena.ColorSafe);
            foreach ((var hint, bool risk) in hints)
            {
                ImGui.TextColored(risk ? riskColor : safeColor, hint);
                ImGui.SameLine();
            }
            ImGui.NewLine();
        }

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
                    AddRelevantEnemy(relevant, actor);

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
                    relevant.Actors.Remove(actor);

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

        private void OnActorTethered(object? sender, WorldState.Actor actor)
        {
            foreach (var comp in _components)
                comp.OnTethered(actor);
        }

        private void OnActorUntethered(object? sender, WorldState.Actor actor)
        {
            foreach (var comp in _components)
                comp.OnUntethered(actor);
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
