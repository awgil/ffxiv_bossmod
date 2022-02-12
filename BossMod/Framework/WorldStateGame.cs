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
                var classID = (Class)(character?.ClassJob.Id ?? 0);

                var act = FindActor(obj.ObjectId);
                if (act == null)
                {
                    act = AddActor(obj.ObjectId, obj.DataId, obj.Name.TextValue, (ActorType)(((int)obj.ObjectKind << 8) + obj.SubKind), classID, new(obj.Position, obj.Rotation), obj.HitboxRadius, Utils.GameObjectIsTargetable(obj));
                }
                else
                {
                    ChangeActorClass(act, classID);
                    RenameActor(act, obj.Name.TextValue);
                    MoveActor(act, new(obj.Position, obj.Rotation));
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
                            Action = new((ActionType)chara.CastActionType, chara.CastActionId),
                            TargetID = chara.CastTargetObjectId,
                            Location = Utils.BattleCharaCastLocation(chara),
                            TotalTime = chara.TotalCastTime,
                            FinishAt = DateTime.Now.AddSeconds(chara.CurrentCastTime)
                        } : null;
                    UpdateCastInfo(act, curCast);

                    for (int i = 0; i < chara.StatusList.Length; ++i)
                    {
                        Status status = new();
                        var s = chara.StatusList[i];
                        if (s != null)
                        {
                            status.ID = s.StatusId;
                            status.SourceID = s.SourceID;
                            status.Extra = (ushort)((s.Param << 8) | s.StackCount);
                            status.ExpireAt = s.RemainingTime == 0 ? DateTime.MaxValue : DateTime.Now.AddSeconds(s.RemainingTime);
                        }
                        UpdateStatus(act, i, status);
                    }
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
