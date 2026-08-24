using System.Reflection;

namespace BossMod.Autorotation;

// database containing all registered rotation module definitions and builder functions
public static class RotationModuleRegistry
{
    public readonly record struct Entry(RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder);

    public static IReadOnlyDictionary<Type, Entry> Modules => _modules;

    private static readonly Dictionary<Type, Entry> _modules = [];

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

            _modules[t] = new(def, New<RotationModule>.ConstructorDerived<RotationModuleManager, Actor>(t));
        }
    }

    public static void UnloadFrom(Assembly assembly)
    {
        foreach (var k in _modules.Keys.Where(k => k.Assembly == assembly).ToList())
            _modules.Remove(k);
    }
}
