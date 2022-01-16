using ImGuiNET;
using System;
using System.Numerics;

namespace BossMod
{
    // base for boss modules - provides all the common features, so that look is standardized
    public class BossModule : IDisposable
    {
        public WorldState WorldState { get; init; }
        public WorldState.Actor?[] RaidMembers; // this is fixed-size, but some slots could be empty; when player is removed, gap is created - existing players keep their indices
        public StateMachine StateMachine { get; init; } = new();
        public StateMachine.State? InitialState = null;
        public MiniArena Arena { get; init; } = new();

        public BossModule(WorldState w, int maxRaidMembers)
        {
            WorldState = w;
            RaidMembers = new WorldState.Actor?[maxRaidMembers];
            WorldState.PlayerInCombatChanged += EnterExitCombat;
            WorldState.ActorCreated += OnActorCreated;
            WorldState.ActorDestroyed += OnActorDestroyed;
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
                WorldState.PlayerInCombatChanged -= EnterExitCombat;
                WorldState.ActorCreated -= OnActorCreated;
                WorldState.ActorDestroyed -= OnActorDestroyed;
            }
        }

        public virtual void Update()
        {
            StateMachine.Update();
        }

        public virtual void Draw(float cameraAzimuth)
        {
            StateMachine.Draw();
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
        }

        public int FindRaidMemberSlot(uint instanceID)
        {
            return Array.FindIndex(RaidMembers, x => x != null && x.InstanceID == instanceID);
        }

        public int FindPlayerSlot()
        {
            return FindRaidMemberSlot(WorldState.PlayerActorID);
        }

        // returns mask
        public ulong FindRaidMembersInRange(Vector3 position, float radius, bool includeDead = false)
        {
            var rsq = radius * radius;
            ulong mask = 0;
            for (int i = 0; i < RaidMembers.Length; ++i)
            {
                var target = RaidMembers[i];
                if (target == null)
                    continue;
                if (target.IsDead && !includeDead)
                    continue;
                if ((target.Position - position).LengthSquared() > rsq)
                    continue;
                mask |= 1ul << i;
            }
            return mask;
        }

        public ulong FindRaidMembersInRange(int slot, float radius, bool includeSelf = false, bool includeDead = false)
        {
            var actor = RaidMembers[slot];
            if (actor == null)
                return 0;
            var mask = FindRaidMembersInRange(actor.Position, radius, includeDead);
            if (!includeSelf)
                mask &= ~(1ul << slot);
            return mask;
        }

        protected virtual void DrawHeader() { }
        protected virtual void DrawArena() { }
        protected virtual void DrawFooter() { }
        protected virtual void NonPlayerCreated(WorldState.Actor actor) { }
        protected virtual void NonPlayerDestroyed(WorldState.Actor actor) { }
        protected virtual void RaidMemberCreated(int index) { }
        protected virtual void RaidMemberDestroyed(int index) { } // called just before slot is cleared, so still contains old actor
        protected virtual void Reset() { }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            Reset();
            StateMachine.ActiveState = inCombat ? InitialState : null;
        }

        private void OnActorCreated(object? sender, WorldState.Actor actor)
        {
            if (actor.Type == WorldState.ActorType.Player)
            {
                int slot = Array.FindIndex(RaidMembers, x => x == null);
                if (slot != -1)
                {
                    RaidMembers[slot] = actor;
                    RaidMemberCreated(slot);
                }
                else
                {
                    Service.Log($"[BossModule] Too many raid members: {RaidMembers.Length} already exist; skipping new actor {actor.InstanceID:X}");
                }
            }
            else
            {
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
                }
                else
                {
                    Service.Log($"[BossModule] Destroyed player actor {actor.InstanceID:X} not found among raid members");
                }
            }
            else
            {
                NonPlayerDestroyed(actor);
            }
        }
    }
}
