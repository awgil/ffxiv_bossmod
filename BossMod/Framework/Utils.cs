using Dalamud.Game.ClientState.Objects.Types;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace BossMod;

public static partial class Utils
{
    public static string ObjectString(GameObject obj) => $"{obj.DataId:X} '{obj.Name}' <{obj.ObjectId:X}>";

    public static string ObjectString(ulong id)
    {
        var obj = (id >> 32) == 0 ? Service.ObjectTable.SearchById((uint)id) : null;
        return obj != null ? ObjectString(obj) : $"(not found) <{id:X}>";
    }

    public static string ObjectKindString(GameObject obj)
        => obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc ? $"{obj.ObjectKind}/{(Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind)obj.SubKind}"
        : obj.SubKind == 0 ? $"{obj.ObjectKind}"
        : $"{obj.ObjectKind}/{obj.SubKind}";

    public static Vector3 XYZ(this Vector4 v) => new(v.X, v.Y, v.Z);
    public static Vector2 XZ(this Vector4 v) => new(v.X, v.Z);
    public static Vector2 XZ(this Vector3 v) => new(v.X, v.Z);
    public static Vector3 ToSystem(this FFXIVClientStructs.FFXIV.Common.Math.Vector3 v) => new(v.X, v.Y, v.Z);

    public static bool AlmostEqual(float a, float b, float eps) => MathF.Abs(a - b) <= eps;
    public static bool AlmostEqual(Vector3 a, Vector3 b, float eps) => (a - b).LengthSquared() <= eps * eps;

    public static string Vec3String(Vector3 pos) => $"[{pos.X:f2}, {pos.Y:f2}, {pos.Z:f2}]";
    public static string QuatString(Quaternion q) => $"[{q.X:f2}, {q.Y:f2}, {q.Z:f2}, {q.W:f2}]";
    public static string PosRotString(Vector4 posRot) => $"[{posRot.X:f2}, {posRot.Y:f2}, {posRot.Z:f2}, {posRot.W.Radians()}]";
    public static bool CharacterIsOmnidirectional(uint oid) => Service.LuminaRow<Lumina.Excel.GeneratedSheets.BNpcBase>(oid)?.Unknown10 ?? false;
    public static string StatusString(uint statusID) => $"{statusID} '{Service.LuminaRow<Lumina.Excel.GeneratedSheets.Status>(statusID)?.Name ?? "<not found>"}'";
    public static bool StatusIsRemovable(uint statusID) => Service.LuminaRow<Lumina.Excel.GeneratedSheets.Status>(statusID)?.CanDispel ?? false;
    public static string StatusTimeString(DateTime expireAt, DateTime now) => $"{Math.Max(0, (expireAt - now).TotalSeconds):f3}";
    public static string CastTimeString(float current, float total) => $"{current:f2}/{total:f2}";
    public static string CastTimeString(ActorCastInfo cast, DateTime now) => CastTimeString((float)(cast.FinishAt - now).TotalSeconds, cast.TotalTime);
    public static string LogMessageString(uint id) => $"{id} '{Service.LuminaRow<Lumina.Excel.GeneratedSheets.LogMessage>(id)?.Text}'";

    public static unsafe T ReadField<T>(void* address, int offset) where T : unmanaged => *(T*)((IntPtr)address + offset);
    public static unsafe void WriteField<T>(void* address, int offset, T value) where T : unmanaged => *(T*)((IntPtr)address + offset) = value;

    public unsafe delegate byte GameObjectIsFriendlyDelegate(FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* obj);
    public static readonly GameObjectIsFriendlyDelegate GameObjectIsFriendly = Marshal.GetDelegateForFunctionPointer<GameObjectIsFriendlyDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 33 C9 84 C0 0F 95 C1 8D 41 03"));

    public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GameObjectInternal(GameObject? obj) => obj != null ? (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj.Address : null;
    public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.Character* CharacterInternal(Character? chr) => chr != null ? (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)chr.Address : null;
    public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* BattleCharaInternal(BattleChara? chara) => chara != null ? (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara.Address : null;

    public static unsafe ulong MouseoverID()
    {
        var pronoun = FFXIVClientStructs.FFXIV.Client.UI.Misc.PronounModule.Instance();
        return pronoun != null && pronoun->UiMouseOverTarget != null ? pronoun->UiMouseOverTarget->EntityId : 0;
    }

    public static unsafe ulong SceneObjectFlags(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o) => ReadField<ulong>(o, 0x38);

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
        if (!map.TryGetValue(key, out var value))
        {
            value = new();
            map[key] = value;
        }
        return value;
    }

    // add value to the list, if it is not null
    public static bool AddIfNonNull<T>(this List<T> list, T? value)
    {
        if (value == null)
            return false;
        list.Add(value);
        return true;
    }

    // get reference to the list element (a bit of a hack, but oh well...)
    public static ref T Ref<T>(this List<T> list, int index) => ref CollectionsMarshal.AsSpan(list)[index];
    public static Span<T> AsSpan<T>(this List<T> list) => CollectionsMarshal.AsSpan(list);

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
    public static void SortBy<TValue, TKey>(this TValue[] arr, Func<TValue, TKey> proj) where TKey : notnull, IComparable => Array.Sort(arr, (l, r) => proj(l).CompareTo(proj(r)));
    public static void SortByReverse<TValue, TKey>(this List<TValue> list, Func<TValue, TKey> proj) where TKey : notnull, IComparable => list.Sort((l, r) => proj(r).CompareTo(proj(l)));
    public static void SortByReverse<TValue, TKey>(this TValue[] arr, Func<TValue, TKey> proj) where TKey : notnull, IComparable => Array.Sort(arr, (l, r) => proj(r).CompareTo(proj(l)));

    // get enumerable of zero or one elements, depending on whether argument is null
    public static IEnumerable<T> ZeroOrOne<T>(T? value) where T : struct
    {
        if (value != null)
            yield return value.Value;
    }

    // enumerate pairs of neighbouring elements
    public static IEnumerable<(T, T)> Pairwise<T>(this IEnumerable<T> source)
    {
        using var e = source.GetEnumerator();
        if (!e.MoveNext())
            yield break;

        var prev = e.Current;
        while (e.MoveNext())
        {
            var curr = e.Current;
            yield return (prev, curr);
            prev = curr;
        }
    }

    // swap two values
    public static void Swap<T>(ref T l, ref T r) => (r, l) = (l, r);

    // sort a list and remove duplicates
    public static void SortAndRemoveDuplicates<T>(this List<T> list) where T : notnull, IComparable
    {
        list.Sort();
        var span = list.AsSpan();
        int last = 0;
        for (int i = 1; i < list.Count; ++i)
        {
            if (!span[i].Equals(span[last]))
            {
                ++last;
                if (i != last)
                    span[last] = span[i];
            }
        }
        ++last;
        if (last < list.Count)
            list.RemoveRange(last, list.Count - last);
    }

    // linear interpolation
    public static float Lerp(float a, float b, float t) => a + (b - a) * t;

    // build an array with N copies of same element
    public static T[] MakeArray<T>(int count, T value)
    {
        var res = new T[count];
        Array.Fill(res, value);
        return res;
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
        v = v.Replace("'", null, StringComparison.Ordinal);
        v = v.Replace('-', ' ');
        v = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(v);
        v = NonAlphaNumRegex().Replace(v, "");
        return v;
    }

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphaNumRegex();
}
