using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public static unsafe void WriteField<T>(void* address, int offset, T value) where T : unmanaged
        {
            *(T*)((IntPtr)address + offset) = value;
        }

        private unsafe delegate byte GameObjectIsFriendlyDelegate(FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* obj);
        private static GameObjectIsFriendlyDelegate GameObjectIsFriendlyFunc = Marshal.GetDelegateForFunctionPointer<GameObjectIsFriendlyDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 33 C9 84 C0 0F 95 C1 8D 41 03"));

        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GameObjectInternal(GameObject? obj) => obj != null ? (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Address : null;
        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.Character* CharacterInternal(Character? chr) => chr != null ? (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address : null;
        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* BattleCharaInternal(BattleChara? chara) => chara != null ? (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara.Address : null;

        public static unsafe bool GameObjectIsDead(GameObject obj) => GameObjectInternal(obj)->IsDead();
        public static unsafe bool GameObjectIsTargetable(GameObject obj) => GameObjectInternal(obj)->GetIsTargetable();
        public static unsafe bool GameObjectIsFriendly(GameObject obj) => GameObjectIsFriendlyFunc(GameObjectInternal(obj)) != 0;
        public static unsafe byte GameObjectEventState(GameObject obj) => ReadField<byte>(GameObjectInternal(obj), 0x70); // see actor control 106
        public static unsafe float GameObjectRadius(GameObject obj) => GameObjectInternal(obj)->GetRadius();
        //public static unsafe Vector3 GameObjectNonInterpolatedPosition(GameObject obj) => ReadField<Vector3>(GameObjectInternal(obj), 0x10);
        //public static unsafe float GameObjectNonInterpolatedRotation(GameObject obj) => ReadField<float>(GameObjectInternal(obj), 0x20);
        public static unsafe byte CharacterShieldValue(Character chr) => CharacterInternal(chr)->ShieldValue; // % of max hp; see effect result
        public static unsafe byte CharacterAnimationState(Character chr, bool second) => ReadField<byte>(CharacterInternal(chr), second ? 0x1ADD : 0x1ADC); // see actor control 62
        public static unsafe byte CharacterModelState(Character chr) => ReadField<byte>(CharacterInternal(chr), 0x1ADE); // see actor control 63
        public static unsafe float CharacterCastRotation(Character chr) => ReadField<float>(CharacterInternal(chr), 0x1A84); // see ActorCast -> Character::StartCast
        public static unsafe ulong CharacterTargetID(Character chr) => ReadField<ulong>(CharacterInternal(chr), 0x1A68); // until FFXIVClientStructs fixes offset and type...
        public static unsafe byte CharacterTetherID(Character chr) => ReadField<byte>(CharacterInternal(chr), 0x1A00); // see ActorControl -> Tether -> Character::SetTether (note that there is also a secondary tether...)
        public static unsafe ulong CharacterTetherTargetID(Character chr) => ReadField<ulong>(CharacterInternal(chr), 0x1A10);
        public static unsafe Vector3 BattleCharaCastLocation(BattleChara chara) => BattleCharaInternal(chara)->SpellCastInfo.CastLocation; // see ActorCast -> Character::StartCast -> Character::StartOmen

        public static unsafe ulong SceneObjectFlags(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
        {
            return ReadField<ulong>(o, 0x38);
        }

        // returns null if countdown is not active, otherwise time left in seconds
        public static unsafe float? CountdownRemaining()
        {
            var agent = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentId.CountDownSettingDialog);
            bool active = agent != null && ReadField<byte>(agent, 56) != 0;
            return active ? ReadField<float>(agent, 40) : null;
        }

        // backport from .net 6, except that it doesn't throw on empty enumerable...
        public static TSource? MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
        {
            using var e = source.GetEnumerator();
            if (!e.MoveNext())
                return default;

            var res = e.Current;
            var score = keySelector(res);
            while (e.MoveNext())
            {
                var cur = e.Current;
                var curScore = keySelector(cur);
                if (curScore.CompareTo(score) < 0)
                {
                    score = curScore;
                    res = cur;
                }
            }
            return res;
        }

        public static TSource? MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : IComparable
        {
            using var e = source.GetEnumerator();
            if (!e.MoveNext())
                return default;

            var res = e.Current;
            var score = keySelector(res);
            while (e.MoveNext())
            {
                var cur = e.Current;
                var curScore = keySelector(cur);
                if (curScore.CompareTo(score) > 0)
                {
                    score = curScore;
                    res = cur;
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

        // sort elements of a list by key
        public static void SortBy<TValue, TKey>(this List<TValue> list, Func<TValue, TKey> proj) where TKey : notnull, IComparable => list.Sort((l, r) => proj(l).CompareTo(proj(r)));
        public static void SortByReverse<TValue, TKey>(this List<TValue> list, Func<TValue, TKey> proj) where TKey : notnull, IComparable => list.Sort((l, r) => proj(r).CompareTo(proj(l)));

        // swap to values
        public static void Swap<T>(ref T l, ref T r)
        {
            var t = l;
            l = r;
            r = t;
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

        // generate valid identifier name from human-readable string
        public static string StringToIdentifier(string v)
        {
            v = v.Replace("'", null);
            v = v.Replace('-', ' ');
            v = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(v);
            v = v.Replace(" ", null);
            return v;
        }
    }
}
