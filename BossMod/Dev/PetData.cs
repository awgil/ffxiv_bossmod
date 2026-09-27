using BossMod.BST;
using Dalamud.Bindings.ImGui;
using System.Text;

namespace BossMod.Dev;

// TODO: should be a codegen
internal class PetData() : TestWindow("Pet data generator", new(100, 100), ImGuiWindowFlags.None)
{
    public override void Draw()
    {
        if (ImGui.Button("Generate"))
            Generate();
    }

    void Generate()
    {
        StringBuilder sb = new();

        foreach (var xbmPet in Service.LuminaSheet<Lumina.Excel.Sheets.XBMPet>())
        {
            if (Service.LuminaRow<Lumina.Excel.Sheets.Pet>((uint)xbmPet.Unknown4) is not { } pet)
            {
                Service.PluginLog.Warning($"Missing pet row for xbm pet ID {xbmPet.RowId}");
                continue;
            }

            sb.Append($"        // #{xbmPet.RowId} {pet.Name}\n");

            var a0 = pet.Abilities[0].Value;
            var a1 = pet.Abilities[1].Value;

            static string describeShape(Lumina.Excel.Sheets.Action action) => action.CastType switch
            {
                0 => "null",
                1 => "null",
                2 => $"new AOEShapeCircle({action.EffectRange})",
                // special case for squirrel
                12 when action.EffectRange == 8 => "new AOEShapeRect(4, 1.5f, 4)",
                12 => $"new AOEShapeRect({action.EffectRange}, {action.XAxisModifier * 0.5f}f)",
                13 => $"new AOEShapeCone({action.EffectRange}, TODO)",
                var x => throw new NotImplementedException($"unknown ({x})")
            };

            static BeastmasterAffinity affinity(uint icon) => icon switch
            {
                3906 => BeastmasterAffinity.Rampant,
                3907 => BeastmasterAffinity.Durant,
                3908 => BeastmasterAffinity.Eldritch,
                3909 => BeastmasterAffinity.Volant,
                _ => BeastmasterAffinity.None
            };

            // crab (15) tr (haste) counted as a dps action
            var supportRelease = xbmPet.RowId is 2 or 3 or 4 or 6 or 7 or 13 or 18 or 22 or 25 or 35 or 37 or 47;

            var suppString = supportRelease ? "\n            NonDamagingRelease = true," : "";

            sb.Append($@"        new PetInfo {{
            TrickAffinity = BeastmasterAffinity.{affinity(a0.Icon)},
            TrickShape = {describeShape(a0)},
            TrickRange = {a0.Range},
            ReleaseShape = {describeShape(a1)},
            ReleaseRange = {a1.Range},{suppString}
        }},
");
        }

        ImGui.SetClipboardText(sb.ToString());
    }
}
