using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // world state that is updated to correspond to game state
    class WorldStateGame : WorldState, IDisposable
    {
        private Network _network;
        private Dictionary<uint, float[]> _prevStatusDurations = new();

        public WorldStateGame(Network network)
        {
            _network = network;
            _network.EventActionEffect += OnNetworkActionEffect;
            _network.EventActorControlTargetIcon += OnNetworkActorControlTargetIcon;
            _network.EventActorControlTether += OnNetworkActorControlTether;
            _network.EventActorControlTetherCancel += OnNetworkActorControlTetherCancel;
            _network.EventEnvControl += OnNetworkEnvControl;
            _network.EventWaymark += OnNetworkWaymark;
        }

        public void Dispose()
        {
            _network.EventActionEffect -= OnNetworkActionEffect;
            _network.EventActorControlTargetIcon -= OnNetworkActorControlTargetIcon;
            _network.EventActorControlTether -= OnNetworkActorControlTether;
            _network.EventActorControlTetherCancel -= OnNetworkActorControlTetherCancel;
            _network.EventEnvControl -= OnNetworkEnvControl;
            _network.EventWaymark -= OnNetworkWaymark;
        }

        public void Update()
        {
            CurrentTime = DateTime.Now;
            CurrentZone = Service.ClientState.TerritoryType;
            UpdateActors();
            UpdateParty();
        }

        private void UpdateActors()
        {
            Dictionary<uint, GameObject> seenIDs = new();
            foreach (var obj in Service.ObjectTable)
                if (obj.ObjectId != GameObject.InvalidGameObjectId)
                    seenIDs.Add(obj.ObjectId, obj);

            List<uint> delIDs = new();
            foreach (var e in Actors)
                if (!seenIDs.ContainsKey(e.InstanceID))
                    delIDs.Add(e.InstanceID);

            foreach (var id in delIDs)
            {
                Actors.Remove(id);
                _prevStatusDurations.Remove(id);
            }

            foreach ((_, var obj) in seenIDs)
            {
                var character = obj as Character;
                var classID = (Class)(character?.ClassJob.Id ?? 0);

                var act = Actors.Find(obj.ObjectId);
                if (act == null)
                {
                    act = Actors.Add(obj.ObjectId, obj.DataId, obj.Name.TextValue, (ActorType)(((int)obj.ObjectKind << 8) + obj.SubKind), classID, new(obj.Position, obj.Rotation), obj.HitboxRadius, Utils.GameObjectIsTargetable(obj), SanitizedObjectID(obj.OwnerId));
                    _prevStatusDurations[obj.ObjectId] = new float[30];
                }
                else
                {
                    Actors.ChangeClass(act, classID);
                    Actors.Rename(act, obj.Name.TextValue);
                    Actors.Move(act, new(obj.Position, obj.Rotation));
                    Actors.ChangeIsTargetable(act, Utils.GameObjectIsTargetable(obj));
                }
                Actors.ChangeTarget(act, SanitizedObjectID(obj.TargetObjectId));
                Actors.ChangeIsDead(act, Utils.GameObjectIsDead(obj));
                Actors.ChangeInCombat(act, character?.StatusFlags.HasFlag(StatusFlags.InCombat) ?? false);

                var chara = obj as BattleChara;
                if (chara != null)
                {
                    ActorCastInfo? curCast = chara.IsCasting
                        ? new ActorCastInfo
                        {
                            Action = new((ActionType)chara.CastActionType, chara.CastActionId),
                            TargetID = SanitizedObjectID(chara.CastTargetObjectId),
                            Location = Utils.BattleCharaCastLocation(chara),
                            TotalTime = chara.TotalCastTime,
                            FinishAt = CurrentTime.AddSeconds(Math.Clamp(chara.CurrentCastTime, 0, 100000))
                        } : null;
                    Actors.UpdateCastInfo(act, curCast);

                    var prevDurations = _prevStatusDurations[obj.ObjectId];
                    for (int i = 0; i < chara.StatusList.Length; ++i)
                    {
                        // note: some statuses have non-zero remaining time but never tick down (e.g. FC buffs); currently we ignore that fact, to avoid log spam...
                        // note: RemainingTime is not monotonously decreasing (I assume because it is really calculated by game and frametime fluctuates...), we ignore 'slight' duration increases (<1 sec)
                        // note: sometimes (Ocean Fishing) remaining-time is weird (I assume too large?) and causes exception in AddSeconds - so we just clamp it to some reasonable range
                        var s = chara.StatusList[i];
                        var dur = Math.Clamp(s?.RemainingTime ?? 0, 0, 100000);
                        var srcID = SanitizedObjectID(s?.SourceID ?? 0);
                        if (s == null)
                        {
                            Actors.UpdateStatus(act, i, new());
                        }
                        else if (s.StatusId != act.Statuses[i].ID || srcID != act.Statuses[i].SourceID || StatusExtra(s) != act.Statuses[i].Extra || dur > prevDurations[i] + 1)
                        {
                            ActorStatus status = new();
                            status.ID = s.StatusId;
                            status.SourceID = srcID;
                            status.Extra = StatusExtra(s);
                            status.ExpireAt = CurrentTime.AddSeconds(dur);
                            Actors.UpdateStatus(act, i, status);
                        }
                        prevDurations[i] = dur;
                    }
                }
            }
        }

        private void UpdateParty()
        {
            if (Service.ClientState.LocalContentId != Party.ContentIDs[PartyState.PlayerSlot])
            {
                // player content ID has changed - so clear any old party state
                // remove old player last
                for (int i = Party.ContentIDs.Length - 1; i >= 0; --i)
                    if (Party.ContentIDs[i] != 0)
                        Party.Remove(i);

                // if player is now available, add as first element
                if (Service.ClientState.LocalContentId != 0)
                {
                    var playerSlot = Party.Add(Service.ClientState.LocalContentId, Service.ClientState.LocalPlayer?.ObjectId ?? 0, true);
                    if (playerSlot != PartyState.PlayerSlot)
                    {
                        Service.Log($"[WorldState] Player was added to wrong slot {playerSlot}");
                    }
                }
            }

            if (Service.ClientState.LocalContentId == 0)
                return; // player not in world, party is empty

            if (Service.PartyList.Length == 0)
            {
                // solo - just update player actor
                Party.AssignActor(PartyState.PlayerSlot, Service.ClientState.LocalContentId, Service.ClientState.LocalPlayer?.ObjectId ?? 0);
                return;
            }

            Dictionary<ulong, uint> seenIDs = new();
            foreach (var obj in Service.PartyList)
                seenIDs[(ulong)obj.ContentId] = SanitizedObjectID(obj.ObjectId);

            for (int i = 1; i < Party.ContentIDs.Length; ++i)
                if (Party.ContentIDs[i] != 0 && !seenIDs.ContainsKey(Party.ContentIDs[i]))
                    Party.Remove(i);

            foreach ((ulong contentID, uint actorID) in seenIDs)
            {
                int slot = Party.ContentIDs.IndexOf(contentID);
                if (slot != -1)
                {
                    Party.AssignActor(slot, contentID, actorID);
                }
                else
                {
                    Party.Add(contentID, actorID, false);
                }
            }
        }

        private ushort StatusExtra(Dalamud.Game.ClientState.Statuses.Status s) => (ushort)((s.Param << 8) | s.StackCount);
        private uint SanitizedObjectID(uint raw) => raw != GameObject.InvalidGameObjectId ? raw : 0;

        private void OnNetworkActionEffect(object? sender, CastEvent info) => Events.DispatchCast(info);
        private void OnNetworkActorControlTargetIcon(object? sender, (uint actorID, uint iconID) args) => Events.DispatchIcon(args);
        private void OnNetworkActorControlTether(object? sender, (uint actorID, uint targetID, uint tetherID) args)
        {
            var act = Actors.Find(args.actorID);
            if (act != null)
                Actors.UpdateTether(act, new ActorTetherInfo { Target = args.targetID, ID = args.tetherID });
        }
        private void OnNetworkActorControlTetherCancel(object? sender, uint actorID)
        {
            var act = Actors.Find(actorID);
            if (act != null)
                Actors.UpdateTether(act, new());
        }
        private void OnNetworkEnvControl(object? sender, (uint featureID, byte index, uint state) args) => Events.DispatchEnvControl(args);
        private void OnNetworkWaymark(object? sender, (Waymark waymark, Vector3? pos) args) => Waymarks[args.waymark] = args.pos;
    }
}
