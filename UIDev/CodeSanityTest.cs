using BossMod;
using ImGuiNET;
using System.Reflection;

namespace UIDev;

class CodeSanityTest : TestWindow
{
    private List<string> _warnings = new();

    public CodeSanityTest() : base("Code sanity test", new(1100, 800), ImGuiWindowFlags.None)
    {
        foreach (var t in Utils.GetDerivedTypes<BossComponent>(typeof(BossComponent).Assembly))
        {
            foreach (var f in t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(f => !f.IsInitOnly && !f.IsLiteral))
            {
                _warnings.Add($"{t.FullName} :: {f.Name} - RW static field in component");
            }
            foreach (var p in t.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(p => p.CanWrite))
            {
                _warnings.Add($"{t.FullName} :: {p.Name} - RW static property in component");
            }
        }
    }

    public override void Draw()
    {
        foreach (var w in _warnings)
            ImGui.TextUnformatted(w);
    }
}
