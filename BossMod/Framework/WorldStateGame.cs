using Dalamud.Game.ClientState.Objects.Types;
using System.Collections.Generic;

namespace BossMod
{
    // world state that is updated to correspond to game state
    class WorldStateGame : WorldState
    {
        public void Update()
        {
            CurrentZone = Service.ClientState.TerritoryType;
            PlayerInCombat = Service.ClientState.LocalPlayer?.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat) ?? false;
            PlayerActorID = Service.ClientState.LocalPlayer?.ObjectId ?? 0;

            HashSet<uint> seenIDs = new();
            foreach (var obj in Service.ObjectTable)
            {
                if (obj.ObjectId == GameObject.InvalidGameObjectId)
                    continue;

                seenIDs.Add(obj.ObjectId);
                var act = FindActor(obj.ObjectId);
                if (act == null)
                {
                    act = AddActor(obj.ObjectId, obj.DataId, (ActorType)(((int)obj.ObjectKind << 8) + obj.SubKind), obj.Position, obj.Rotation, obj.HitboxRadius);
                }
                else
                {
                    MoveActor(act, obj.Position, obj.Rotation);
                }

                var chara = obj as BattleChara;
                if (chara != null)
                {
                    CastInfo? curCast = chara.IsCasting
                        ? new CastInfo {
                            ActionType = chara.CastActionType,
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

            List<uint> delIDs = new();
            foreach (var e in Actors)
                if (!seenIDs.Contains(e.Key))
                    delIDs.Add(e.Key);
            foreach (var id in delIDs)
                RemoveActor(id);
        }
    }
}
