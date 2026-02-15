using System.Reflection;

namespace BossMod.Autorotation;

// database containing all registered rotation module definitions and builder functions
public class RotationModuleRegistry(ActionDefinitions defs)
{
    public readonly record struct Entry(RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder);

    public Dictionary<Type, Entry> Modules { get; private set; } = BuildModules(defs);

    private static Dictionary<Type, Entry> BuildModules(ActionDefinitions defs)
    {
        Dictionary<Type, Entry> res = [];

        foreach (var t in Utils.GetDerivedTypes<RotationModule>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            var defMethod = t.GetMethod("Definition", BindingFlags.Static | BindingFlags.Public);
            var def = defMethod?.Invoke(null, [defs]) as RotationModuleDefinition;
            if (def == null)
            {
                Service.Log($"Rotation module {t.FullName} does not register itself properly: it should have a static Definition() method that returns a valid RotationModuleDefinition object");
                continue;
            }

            var factory = New<RotationModule>.ConstructorDerived<RotationModuleManager, Actor>(t);
            res[t] = new(def, factory);
        }

        return res;
    }
}
