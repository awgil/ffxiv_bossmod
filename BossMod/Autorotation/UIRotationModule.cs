using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Dalamud.Bindings.ImGui;

namespace BossMod.Autorotation;

public sealed class UIRotationModule
{
    public static void DescribeModule(Type type, RotationModuleDefinition definition)
    {
        ImGui.TextUnformatted(definition.DisplayName);
        ImGui.TextUnformatted(definition.Description);
        ImGui.TextUnformatted($"L{definition.MinLevel}-{definition.MaxLevel} {string.Join(" ", definition.Classes.SetBits().Select(b => (Class)b))}");
        ImGui.TextUnformatted($"Author/contributors: {definition.Author}");
        ImGui.TextUnformatted($"Quality: {(int)definition.Quality}/{(int)RotationModuleQuality.Count - 1} {definition.Quality.GetAttribute<PropertyDisplayAttribute>()?.Label ?? ""}");
        using (ImRaii.Disabled())
        {
            ImGui.TextUnformatted($"Class: {type.FullName}");
            ImGui.TextUnformatted($"Order group: {definition.Order}");
        }
    }
}
