using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace BossMod
{
    // this is the heart of the boss mod framework
    // every frame, it checks a bunch of values and fires events when interesting things happen
    // bossmod modules subscribe to these events
    class EventGenerator : IDisposable
    {
        public event EventHandler<ushort>? CurrentZoneChanged;
        public ushort CurrentZone { get; private set; }
        private void UpdateCurrentZone(ushort zone)
        {
            if (CurrentZone != zone)
            {
                CurrentZone = zone;
                CurrentZoneChanged?.Invoke(this, zone);
            }
        }

        public bool PlayerInCombat { get; private set; }
        public event EventHandler<PlayerCharacter?>? PlayerCombatEnter; // argument is player
        public event EventHandler<PlayerCharacter?>? PlayerCombatExit; // argument is player
        private void UpdatePlayerInCombat(bool inCombat)
        {
            if (PlayerInCombat != inCombat)
            {
                PlayerInCombat = inCombat;
                if (inCombat)
                    PlayerCombatEnter?.Invoke(this, Service.ClientState.LocalPlayer);
                else
                    PlayerCombatExit?.Invoke(this, Service.ClientState.LocalPlayer);
            }
        }

        public class Actor
        {
            public BattleChara Chara;
            public uint ObjectID;
            public Vector3 Position;
            public bool IsPlayerOrPet;
            public bool Visited;
            public bool IsCasting;
            public uint CastActionID;
            public uint CastTargetID;
            public Vector3 CastLocation = new();
            public float CastCurrentTime;
            public float CastTotalTime;

            public Actor(BattleChara chara)
            {
                Chara = chara;
                ObjectID = chara.ObjectId;
                Position = chara.Position;
                IsPlayerOrPet = chara.SubKind == 2 || chara.SubKind == 3 || chara.SubKind == 4; // pet, player or chocobo
            }
        }
        public Dictionary<uint, Actor> Actors { get; } = new();
        public event EventHandler<Actor>? ActorCreated;
        public event EventHandler<Actor>? ActorDestroyed;
        public event EventHandler<Actor>? ActorCastStarted;
        public event EventHandler<Actor>? ActorCastFinished; // note that actor structure still contains cast details when this is invoked; not invoked if actor disappears without finishing cast?..
        public event EventHandler<Actor>? ActorTeleported;

        public EventGenerator()
        {
        }

        public void Dispose()
        {
        }

        public void Update()
        {
            UpdateCurrentZone(Service.ClientState.TerritoryType);
            UpdatePlayerInCombat(Service.ClientState.LocalPlayer?.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) ?? false);

            foreach (var obj in Service.ObjectTable)
            {
                var chara = obj as BattleChara;
                if (chara)
                    UpdateActor(chara!);
            }
            CleanupActors();
        }

        public Actor? FindActor(uint oid)
        {
            Actor? res;
            Actors.TryGetValue(oid, out res);
            return res;
        }

        public string ActorString(uint oid)
        {
            if (oid == 0)
                return "<none>";
            var actor = FindActor(oid);
            return actor != null ? Utils.ObjectString(actor.Chara) : $"<not found> <{oid:X}>";
        }

        private void UpdateActor(BattleChara chara)
        {
            var oid = chara.ObjectId;
            if (!Actors.ContainsKey(oid))
            {
                Actors[oid] = new Actor(chara);
                ActorCreated?.Invoke(this, Actors[oid]);
            }

            var actor = Actors[oid];
            actor.Visited = true;

            var movement = chara.Position - actor.Position;
            actor.Position = chara.Position;
            if (movement.LengthSquared() >= 4)
            {
                ActorTeleported?.Invoke(this, actor);
            }

            if (chara.IsCasting)
            {
                if (actor.IsCasting && actor.CastActionID == chara.CastActionId && actor.CastTargetID == chara.CastTargetObjectId)
                {
                    // continuing casting same spell
                    actor.CastCurrentTime = chara.CurrentCastTime;
                    actor.CastTotalTime = chara.TotalCastTime;
                }
                else
                {
                    if (actor.IsCasting)
                    {
                        // finish previous cast before starting new one
                        ActorCastFinished?.Invoke(this, actor);
                    }
                    // start new cast
                    actor.IsCasting = true;
                    actor.CastActionID = chara.CastActionId;
                    actor.CastTargetID = chara.CastTargetObjectId;
                    actor.CastLocation = Utils.BattleCharaCastLocation(chara);
                    actor.CastCurrentTime = chara.CurrentCastTime;
                    actor.CastTotalTime = chara.TotalCastTime;
                    ActorCastStarted?.Invoke(this, actor);
                }
            }
            else
            {
                if (actor.IsCasting)
                {
                    // cast finished
                    ActorCastFinished?.Invoke(this, actor);
                    actor.IsCasting = false;
                    actor.CastActionID = 0;
                    actor.CastTargetID = 0;
                    actor.CastLocation.X = actor.CastLocation.Y = actor.CastLocation.Z = 0;
                    actor.CastCurrentTime = 0;
                    actor.CastTotalTime = 0;
                }
                // else: was not and is not casting
            }
        }

        private void CleanupActors()
        {
            var del = new List<KeyValuePair<uint, Actor>>();
            foreach (var elem in Actors)
            {
                if (elem.Value.Visited)
                    elem.Value.Visited = false;
                else
                    del.Add(elem);
            }
            foreach (var elem in del)
            {
                ActorDestroyed?.Invoke(this, elem.Value);
                Actors.Remove(elem.Key);
            }
        }
    }
}
