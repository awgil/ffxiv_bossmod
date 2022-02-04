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
            CurrentZone = Service.ClientState.TerritoryType;
            PlayerInCombat = Service.ClientState.LocalPlayer?.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) ?? false;
            PlayerActorID = Service.ClientState.LocalPlayer?.ObjectId ?? 0;

            Dictionary<uint, GameObject> seenIDs = new();
            foreach (var obj in Service.ObjectTable)
                if (obj.ObjectId != GameObject.InvalidGameObjectId)
                    seenIDs.Add(obj.ObjectId, obj);

            List<uint> delIDs = new();
            foreach (var e in Actors)
                if (!seenIDs.ContainsKey(e.Key))
                    delIDs.Add(e.Key);
            foreach (var id in delIDs)
                RemoveActor(id);

            foreach ((_, var obj) in seenIDs)
            {
                var character = obj as Character;
                uint characterClass = character?.ClassJob.Id ?? 0;

                var act = FindActor(obj.ObjectId);
                if (act == null)
                {
                    act = AddActor(obj.ObjectId, obj.DataId, (ActorType)(((int)obj.ObjectKind << 8) + obj.SubKind), characterClass, (ActorRole?)character?.ClassJob.GameData?.Role ?? ActorRole.None,
                        obj.Position, obj.Rotation, obj.HitboxRadius, Utils.GameObjectIsTargetable(obj));
                }
                else
                {
                    if (act.ClassID != characterClass)
                        ChangeActorClassRole(act, characterClass, (ActorRole?)character?.ClassJob.GameData?.Role ?? ActorRole.None);
                    MoveActor(act, obj.Position, obj.Rotation);
                    ChangeActorIsTargetable(act, Utils.GameObjectIsTargetable(obj));
                }
                ChangeActorTarget(act, obj.TargetObjectId);
                ChangeActorIsDead(act, Utils.GameObjectIsDead(obj));

                var chara = obj as BattleChara;
                if (chara != null)
                {
                    CastInfo? curCast = chara.IsCasting
                        ? new CastInfo
                        {
                            ActionType = (ActionType)chara.CastActionType,
                            ActionID = chara.CastActionId,
                            TargetID = chara.CastTargetObjectId,
                            Location = Utils.BattleCharaCastLocation(chara),
                            CurrentTime = chara.CurrentCastTime,
                            TotalTime = chara.TotalCastTime
                        } : null;
                    UpdateCastInfo(act, curCast);

                    var statuses = new Status[chara.StatusList.Length];
                    for (int i = 0; i < statuses.Length; ++i)
                    {
                        var s = chara.StatusList[i];
                        statuses[i] = s != null ? new Status { ID = s.StatusId, Param = s.Param, StackCount = s.StackCount, RemainingTime = s.RemainingTime, SourceID = s.SourceID } : new Status { };
                    }
                    UpdateStatuses(act, statuses);
                }
            }
        }

        private void OnNetworkActionEffect(object? sender, CastResult info) => DispatchEventCast(info);
        private void OnNetworkActorControlTargetIcon(object? sender, (uint actorID, uint iconID) args) => DispatchEventIcon(args);
        private void OnNetworkActorControlTether(object? sender, (uint actorID, uint targetID, uint tetherID) args)
        {
            var act = FindActor(args.actorID);
            if (act != null)
                UpdateTether(act, new TetherInfo { Target = args.targetID, ID = args.tetherID });
        }
        private void OnNetworkActorControlTetherCancel(object? sender, uint actorID)
        {
            var act = FindActor(actorID);
            if (act != null)
                UpdateTether(act, new());
        }
        private void OnNetworkEnvControl(object? sender, (uint featureID, byte index, uint state) args) => DispatchEventEnvControl(args);
        private void OnNetworkWaymark(object? sender, (WorldState.Waymark waymark, Vector3? pos) args) => SetWaymark(args.waymark, args.pos);
    }
}
