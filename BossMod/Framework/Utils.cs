using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    public static class Utils
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

        public static string StatusString(uint statusID)
        {
            var statusData = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>()?.GetRow(statusID);
            string name = statusData?.Name ?? "<not found>";
            return $"{statusID} '{name}'";
        }

        public static string StatusTimeString(DateTime expireAt, DateTime now)
        {
            return $"{Math.Max(0, (expireAt - now).TotalSeconds):f3}";
        }

        public static string KnockbackString(uint knockbackID)
        {
            var kbData = Service.LuminaGameData?.GetExcelSheet<Lumina.Excel.GeneratedSheets.Knockback>()?.GetRow(knockbackID);
            string details = kbData != null ? $"distance={kbData.Distance}" : "not found";
            return $"{knockbackID} ({details})";
        }

        public static string CastTimeString(float current, float total)
        {
            return $"{current:f2}/{total:f2}";
        }

        public static string CastTimeString(ActorCastInfo cast, DateTime now)
        {
            return CastTimeString((float)(cast.FinishAt - now).TotalSeconds, cast.TotalTime);
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

        // get existing map element or create new
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> map, TKey key) where TValue : new()
        {
            TValue? value;
            if (!map.TryGetValue(key, out value))
            {
                value = new();
                map[key] = value;
            }
            return value;
        }
    }
}
