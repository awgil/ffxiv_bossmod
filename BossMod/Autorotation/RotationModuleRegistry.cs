using System.Reflection;

namespace BossMod.Autorotation;

// database containing all registered rotation module definitions and builder functions
public static class RotationModuleRegistry
{
    public readonly record struct Entry(RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder, Type ModuleType);

    public static IReadOnlyDictionary<string, Entry> Modules => _modules;

    private static readonly Dictionary<string, Entry> _modules = [];

    public static void ScanAssembly(Assembly assembly)
    {
        foreach (var t in Utils.GetDerivedTypes<RotationModule>(assembly).Where(t => !t.IsAbstract))
        {
            var defMethod = t.GetMethod("Definition", BindingFlags.Static | BindingFlags.Public);
            var def = defMethod?.Invoke(null, null) as RotationModuleDefinition;
            if (def == null)
            {
                Service.Log($"Rotation module {t.FullName} does not register itself properly: it should have a static Definition() method that returns a valid RotationModuleDefinition object");
                continue;
            }

            _modules[t.FullName!] = new(def, New<RotationModule>.ConstructorDerived<RotationModuleManager, Actor>(t), t);
        }
    }

    public static void UnloadFrom(Assembly assembly)
    {
        foreach (var (k, _) in _modules.Where(k => k.Value.ModuleType.Assembly == assembly).ToList())
            _modules.Remove(k);
    }
}
