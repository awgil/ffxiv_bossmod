using System.Reflection;
using System.Text.Json;

namespace BossMod.Autorotation;

// database containing all registered rotation module definitions and builder functions
public static class RotationModuleRegistry
{
    public readonly record struct Entry(RotationModuleDefinition Definition, Func<RotationModuleManager, Actor, RotationModule> Builder);

    public static readonly Dictionary<Type, Entry> Modules = BuildModules();

    private static Dictionary<Type, Entry> BuildModules()
    {
        Dictionary<Type, Entry> res = [];

        List<Dictionary<string, string>> _objs = [];

        foreach (var t in Utils.GetDerivedTypes<RotationModule>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            var defMethod = t.GetMethod("Definition", BindingFlags.Static | BindingFlags.Public);
            var def = defMethod?.Invoke(null, null) as RotationModuleDefinition;
            if (def == null)
            {
                Service.Log($"Rotation module {t.FullName} does not register itself properly: it should have a static Definition() method that returns a valid RotationModuleDefinition object");
                continue;
            }

            foreach (var tr in def.Configs.OfType<StrategyConfigTrack>())
            {
                for (var i = 0; i < tr.Options.Count; i++)
                {
                    var opt = tr.Options[i];
                    var ename = tr.OptionEnum.GetEnumValues().Cast<Enum>().ToArray();
                    var enameNice = i < ename.Length ? ename[i].ToString() : "???";

                    if (opt.InternalName != enameNice)
                    {
                        _objs.Add(new()
                        {
                            ["Module"] = t.FullName!,
                            ["Option"] = tr.InternalName,
                            ["Internal"] = opt.InternalName,
                            ["Enum"] = enameNice
                        });
                    }
                }
            }

            var factory = New<RotationModule>.ConstructorDerived<RotationModuleManager, Actor>(t);
            res[t] = new(def, factory);
        }

        if (_objs.Count > 0)
            Service.Log(JsonSerializer.Serialize(_objs));

        return res;
    }
}
