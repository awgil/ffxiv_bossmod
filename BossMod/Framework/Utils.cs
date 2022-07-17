using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BossMod
{
    public static class Utils
    {
        public static string ObjectString(GameObject obj)
        {
            return $"{obj.DataId:X} '{obj.Name}' <{obj.ObjectId:X}>";
        }

        public static string ObjectString(ulong id)
        {
            var obj = (id >> 32) == 0 ? Service.ObjectTable.SearchById((uint)id) : null;
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

        public static Vector3 XYZ(this Vector4 v) => new(v.X, v.Y, v.Z);
        public static Vector2 XZ(this Vector4 v) => new(v.X, v.Z);
        public static Vector2 XZ(this Vector3 v) => new(v.X, v.Z);

        public static bool AlmostEqual(float a, float b, float eps) => MathF.Abs(a - b) <= eps;
        public static bool AlmostEqual(Vector3 a, Vector3 b, float eps) => (a - b).LengthSquared() <= eps * eps;

        public static string Vec3String(Vector3 pos)
        {
            return $"[{pos.X:f2}, {pos.Y:f2}, {pos.Z:f2}]";
        }

        public static string QuatString(Quaternion q)
        {
            return $"[{q.X:f2}, {q.Y:f2}, {q.Z:f2}, {q.W:f2}]";
        }

        public static string PosRotString(Vector4 posRot)
        {
            return $"[{posRot.X:f2}, {posRot.Y:f2}, {posRot.Z:f2}, {posRot.W.Radians()}]";
        }

        public static string StatusString(uint statusID)
        {
            var statusData = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Status>(statusID);
            string name = statusData?.Name ?? "<not found>";
            return $"{statusID} '{name}'";
        }

        public static bool StatusIsRemovable(uint statusID)
        {
            var statusData = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Status>(statusID);
            return statusData?.CanDispel ?? false;
        }

        public static string StatusTimeString(DateTime expireAt, DateTime now)
        {
            return $"{Math.Max(0, (expireAt - now).TotalSeconds):f3}";
        }

        public static string KnockbackString(uint knockbackID)
        {
            var kbData = Service.LuminaRow<Lumina.Excel.GeneratedSheets.Knockback>(knockbackID);
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

        private unsafe delegate byte GameObjectIsFriendlyDelegate(FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* obj);
        private static GameObjectIsFriendlyDelegate GameObjectIsFriendlyFunc = Marshal.GetDelegateForFunctionPointer<GameObjectIsFriendlyDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 33 C9 84 C0 0F 95 C1 8D 41 03"));

        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GameObjectInternal(GameObject? obj) => obj != null ? (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Address : null;
        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.Character* CharacterInternal(Character? chr) => chr != null ? (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address : null;
        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* BattleCharaInternal(BattleChara? chara) => chara != null ? (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara.Address : null;

        public static unsafe bool GameObjectIsDead(GameObject obj) => GameObjectInternal(obj)->IsDead();
        public static unsafe bool GameObjectIsTargetable(GameObject obj) => GameObjectInternal(obj)->GetIsTargetable();
        public unsafe static bool GameObjectIsFriendly(GameObject obj) => GameObjectIsFriendlyFunc(GameObjectInternal(obj)) != 0;
        public static unsafe byte CharacterShieldValue(Character chr) => CharacterInternal(chr)->ShieldValue; // % of max hp
        public static unsafe Vector3 BattleCharaCastLocation(BattleChara chara) => BattleCharaInternal(chara)->SpellCastInfo.CastLocation;

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

        // lower bound: given sorted list, find index of first element with key >= than test value
        public static int LowerBound<TKey, TValue>(this SortedList<TKey, TValue> list, TKey test) where TKey : notnull, IComparable
        {
            int first = 0, size = list.Count;
            while (size > 0)
            {
                int step = size / 2;
                int mid = first + step;
                if (list.Keys[mid].CompareTo(test) < 0)
                {
                    first = mid + 1;
                    size -= step + 1;
                }
                else
                {
                    size = step;
                }
            }
            return first;
        }

        // upper bound: given sorted list, find index of first element with key > than test value
        public static int UpperBound<TKey, TValue>(this SortedList<TKey, TValue> list, TKey test) where TKey : notnull, IComparable
        {
            int first = 0, size = list.Count;
            while (size > 0)
            {
                int step = size / 2;
                int mid = first + step;
                if (list.Keys[mid].CompareTo(test) <= 0)
                {
                    first = mid + 1;
                    size -= step + 1;
                }
                else
                {
                    size = step;
                }
            }
            return first;
        }

        // get all types defined in specified assembly
        public static IEnumerable<Type?> GetAllTypes(Assembly asm)
        {
            try
            {
                return asm.DefinedTypes;
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types;
            }
        }

        // get all types derived from specified type in specified assembly
        public static IEnumerable<Type> GetDerivedTypes<Base>(Assembly asm)
        {
            var b = typeof(Base);
            return GetAllTypes(asm).Where(t => t?.IsSubclassOf(b) ?? false).Select(t => t!);
        }
    }
}
