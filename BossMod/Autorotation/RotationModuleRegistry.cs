using System.Reflection;

namespace BossMod.Autorotation;

// database containing all registered rotation module definitions and builder functions
public static class RotationModuleRegistry
{
    public static readonly Event Modified = new();

    public readonly record struct Entry(Type ModuleType, RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder);

    public static IReadOnlyDictionary<string, Entry> Modules => _modules;

    private static readonly Dictionary<string, Entry> _modules = [];

    private static bool ScanAssembly(Assembly assembly)
    {
        var modified = false;

        foreach (var t in Utils.GetDerivedTypes<RotationModule>(assembly).Where(t => !t.IsAbstract))
        {
            var defMethod = t.GetMethod("Definition", BindingFlags.Static | BindingFlags.Public);
            var def = defMethod?.Invoke(null, null) as RotationModuleDefinition;
            if (def == null)
            {
                Service.Log($"Rotation module {t.FullName} does not register itself properly: it should have a static Definition() method that returns a valid RotationModuleDefinition object");
                continue;
            }

            modified = true;
            _modules[t.FullName!] = new(t, def, New<RotationModule>.ConstructorDerived<RotationModuleManager, Actor>(t));
        }

        return modified;
    }

    private static bool UnloadFrom(Assembly assembly)
    {
        var modified = false;

        foreach (var (k, _) in _modules.Where(k => k.Value.ModuleType.Assembly == assembly).ToList())
            modified |= _modules.Remove(k);

        return modified;
    }

    public static void Reload(IEnumerable<Assembly> old, IEnumerable<Assembly> @new)
    {
        var modified = false;

        foreach (var a in old)
            modified |= UnloadFrom(a);

        foreach (var a in @new)
            modified |= ScanAssembly(a);

        if (modified)
            Modified.Fire();
    }
}
