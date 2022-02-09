using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    static class Utils
    {
        public static string ObjectString(GameObject obj)
        {
            return $"{obj.DataId:X} '{obj.Name}' <{obj.ObjectId:X}>";
        }

        public static string ObjectString(uint id)
        {
            var obj = Service.ObjectTable.SearchById(id);
            return obj != null ? ObjectString(obj) : $"(not found) <{id:X}>";
        }

        public static string ObjectKindString(GameObject obj)
        {
            if (obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc)
                return $"{obj.ObjectKind}/{(Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind)obj.SubKind}";
            else if (obj.SubKind == 0)
                return $"{obj.ObjectKind}";
            else
                return $"{obj.ObjectKind}/{obj.SubKind}";
        }

        public static string CharacterClassString(uint classID)
        {
            var classData = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.ClassJob>()?.GetRow(classID);
            return classData?.Abbreviation ?? "<not found>";
        }

        public static string RadianString(float rad)
        {
            return $"{(rad / Math.PI * 180):f0}";
        }

        public static string Vec3String(Vector3 pos)
        {
            return $"[{pos.X:f2}, {pos.Y:f2}, {pos.Z:f2}]";
        }

        public static string QuatString(Quaternion q)
        {
            return $"[{q.X:f2}, {q.Y:f2}, {q.Z:f2}, {q.W:f2}]";
        }

        public static string ActionString(uint actionID, WorldState.ActionType actionType = WorldState.ActionType.Spell)
        {
            switch (actionType)
            {
                case WorldState.ActionType.Spell:
                    {
                        var actionData = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(actionID);
                        string name = actionData?.Name ?? "<not found>";
                        return $"{actionType} {actionID} '{name}'";
                    }
                case WorldState.ActionType.Item:
                    {
                        // see Dalamud.Game.Text.SeStringHandling.Payloads.GetAdjustedId
                        // TODO: id > 500000 is "collectible", >2000000 is "event" ??
                        bool isHQ = actionID > 1000000;
                        var itemData = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Item>()?.GetRow(actionID % 1000000);
                        string name = itemData?.Name ?? "<not found>";
                        return $"{actionType} {actionID} '{name}'{(isHQ ? " (HQ)" : "")}";
                    }
                default:
                    return $"{actionType} {actionID}";
            }
        }

        public static string StatusString(uint statusID)
        {
            var statusData = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>()?.GetRow(statusID);
            string name = statusData?.Name ?? "<not found>";
            return $"{statusID} '{name}'";
        }

        public static string CastTimeString(float current, float total)
        {
            return $"{current:f2}/{total:f2}";
        }

        public static unsafe T ReadField<T>(void* address, int offset) where T : unmanaged
        {
            return *(T*)((IntPtr)address + offset);
        }

        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GameObjectInternal(GameObject? obj)
        {
            return (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj?.Address;
        }

        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* BattleCharaInternal(BattleChara? chara)
        {
            return (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara?.Address;
        }

        public static unsafe bool GameObjectIsDead(GameObject obj)
        {
            return GameObjectInternal(obj)->IsDead();
        }

        public static unsafe bool GameObjectIsTargetable(GameObject obj)
        {
            return GameObjectInternal(obj)->GetIsTargetable();
        }

        public static unsafe Vector3 BattleCharaCastLocation(BattleChara chara)
        {
            return BattleCharaInternal(chara)->SpellCastInfo.CastLocation;
        }

        public static unsafe ulong SceneObjectFlags(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
        {
            return ReadField<ulong>(o, 0x38);
        }

        // actor iteration utilities
        public static IEnumerable<WorldState.Actor> SortedByRange(this IEnumerable<WorldState.Actor> range, Vector3 origin)
        {
            return range
                .Select(actor => (actor, (actor.Position - origin).LengthSquared()))
                .OrderBy(actorDist => actorDist.Item2)
                .Select(actorDist => actorDist.Item1);
        }

        public static IEnumerable<(int, WorldState.Actor)> SortedByRange(this IEnumerable<(int, WorldState.Actor)> range, Vector3 origin)
        {
            return range
                .Select(indexPlayer => (indexPlayer.Item1, indexPlayer.Item2, (indexPlayer.Item2.Position - origin).LengthSquared()))
                .OrderBy(indexPlayerDist => indexPlayerDist.Item3)
                .Select(indexPlayerDist => (indexPlayerDist.Item1, indexPlayerDist.Item2));
        }

        public static IEnumerable<WorldState.Actor> InRadius(this IEnumerable<WorldState.Actor> range, Vector3 origin, float radius)
        {
            return range.Where(actor => GeometryUtils.PointInCircle(actor.Position - origin, radius));
        }

        // backport from .net 6
        public static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
        {
            var res = source.FirstOrDefault();
            if (res != null)
            {
                var score = keySelector(res);
                foreach (var s in source.Skip(1))
                {
                    var cur = keySelector(s);
                    if (cur.CompareTo(score) < 0)
                    {
                        score = cur;
                        res = s;
                    }
                }
            }
            return res;
        }

        public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
        {
            var res = source.FirstOrDefault();
            if (res != null)
            {
                var score = keySelector(res);
                foreach (var s in source.Skip(1))
                {
                    var cur = keySelector(s);
                    if (cur.CompareTo(score) > 0)
                    {
                        score = cur;
                        res = s;
                    }
                }
            }
            return res;
        }
    }
}
