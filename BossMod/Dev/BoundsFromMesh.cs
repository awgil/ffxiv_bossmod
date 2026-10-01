using Dalamud.Bindings.ImGui;
using Newtonsoft.Json;
using System.IO;

namespace BossMod.Dev;

class BoundsFromMesh() : TestWindow("Generate arena bounds from navmesh", new(400, 400), ImGuiWindowFlags.None)
{
    private Vector2 _center = new(100, 100);

    public override void Draw()
    {
        ImGui.DragFloat2("Arena center", ref _center);

        if (ImGui.Button("Convert"))
        {
            List<List<WDir>> polys = [];
            var center = new WPos(_center);

            using var data = Utils.OpenShareable("E:\\navmesh.txt");

            using (var contents = new StreamReader(data))
            {
                string? line;

                while ((line = contents.ReadLine()) != null)
                {
                    List<WDir> poly = [];
                    foreach (var pos in line.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var coords = pos.Split(',');
                        if (coords.Length != 2)
                            throw new InvalidOperationException("corrupt data");

                        if (!float.TryParse(coords[0], out var x))
                            throw new InvalidOperationException("corrupt data");

                        if (!float.TryParse(coords[1], out var z))
                            throw new InvalidOperationException("corrupt data");

                        poly.Add(new WPos(x, z) - center);
                    }
                    polys.Add(poly);
                }
            }

            var p = polys[0];
            polys.RemoveAt(0);

            var clipper = new PolygonClipper();
            var arena = clipper.UnionAll(new(p), [.. polys.Select(p => new PolygonClipper.Operand(p))]);

            ImGui.SetClipboardText(JsonConvert.SerializeObject(arena));
        }
    }
}
