using BossMod.Global.DeepDungeon;
using Dalamud.Bindings.ImGui;
using Lumina.Data.Files;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace BossMod.Dev;

class ExportWalls() : TestWindow("Deep Dungeon wall export tool", new(400, 400), ImGuiWindowFlags.None)
{
    static readonly uint[] LayerToCFC = [
        0, // empty row

        // potd
        174, 175, 176, 177, 178,
        204, 205, 206, 207, 208, 209, 210, 211, 212, 213, 214, 215, 216, 217, 218,

        // hoh
        540, 541, 542, 543, 544, 545, 546, 547, 548, 549,

        // eo
        897, 898, 899, 900, 901, 902, 903, 904, 905, 906,

        // pt
        1032, 1033, 1034, 1035, 1036, 1037, 1038, 1039, 1040, 1041
    ];

    public override void Draw()
    {
        if (ImGui.Button("Regenerate walls.json"))
            Task.Run(GenerateWalls);
    }

    void GenerateWalls()
    {
        Dictionary<string, Floor<Wall>> allFloors = [];

        foreach (var (layer, cfc) in Service.LuminaSheet<Lumina.Excel.Sheets.DeepDungeonLayer>()!.Zip(LayerToCFC))
        {
            if (layer.RowId == 0)
                continue;

            if (layer.Unknown0) // PT has two extra layer rows with this flag set, presumably related to final verse
                continue;

            var tag = $"{layer.DeepDungeon.RowId}.{layer.FloorSet + 1}";

            Service.Log($"detecting walls for {layer.DeepDungeon.Value.Name} floorset {layer.FloorSet} ({tag}) cfc={cfc}");

            var cc = Service.LuminaRow<Lumina.Excel.Sheets.ContentFinderCondition>(cfc)!;
            var bg = cc.Value.TerritoryType.Value.Bg.ToString();

            var lgb = $"bg/{bg[..bg.LastIndexOf('/')]}/bg.lgb";
            var contents = Service.LuminaGameData!.GetFile<LgbFile>(lgb)!;

            var newFloor = new Floor<Wall>(layer.DeepDungeon.RowId, layer.FloorSet, new([]), new([]));

            foreach (var row in layer.RoomA.Value)
                foreach (var room in row.DeepDungeonRoom)
                {
                    var levels = room.Value.Level.Select(l => l.RowId).ToList();
                    newFloor.RoomsA.Rooms.Add(new(DetectWall(contents, levels[1]), DetectWall(contents, levels[2]), DetectWall(contents, levels[3]), DetectWall(contents, levels[4])));
                }

            foreach (var row in layer.RoomB.Value)
                foreach (var room in row.DeepDungeonRoom)
                {
                    var levels = room.Value.Level.Select(l => l.RowId).ToList();
                    newFloor.RoomsB.Rooms.Add(new(DetectWall(contents, levels[1]), DetectWall(contents, levels[2]), DetectWall(contents, levels[3]), DetectWall(contents, levels[4])));
                }

            allFloors.Add(tag, newFloor);
        }

        using var tf = File.OpenWrite("scratch.json");
        using var ts = new StreamWriter(tf);
        ts.Write(JsonSerializer.Serialize(allFloors));

        Service.Log($"wrote floors to scratch.json");
    }

    Wall DetectWall(LgbFile levelData, uint layoutId)
    {
        if (layoutId < 2)
            return new(default, 0);

        foreach (var layer in levelData.Layers)
        {
            foreach (var obj in layer.InstanceObjects)
            {
                if (obj.InstanceId == layoutId)
                {
                    if (obj.Object is Lumina.Data.Parsing.Layer.LayerCommon.SharedGroupInstanceObject sg)
                    {
                        var contents = Service.LuminaGameData!.GetFile<SgbFile>(sg.AssetPath)!;
                        var wall = contents.LayerGroups.SelectMany(g => g.Layers).SelectMany(l => l.InstanceObjects).FirstOrDefault(obj => obj.AssetType is Lumina.Data.Parsing.Layer.LayerEntryType.CollisionBox);
                        if (wall.Object is Lumina.Data.Parsing.Layer.LayerCommon.CollisionBoxInstanceObject cb)
                        {
                            var depth = wall.Transform.Scale.Z;

                            var z0 = Quaternion.CreateFromYawPitchRoll(obj.Transform.Rotation.Y, obj.Transform.Rotation.X, obj.Transform.Rotation.Z);
                            var localT = wall.Transform.Translation.ToSystem();
                            var adj = Vector3.Transform(localT, z0);
                            var abs = obj.Transform.Translation.ToSystem() + Vector3.Transform(localT, z0);

                            return new Wall(new(abs.X, abs.Z), depth);
                        }
                    }
                }
            }
        }

        Service.Log($"WARNING: object {layoutId:X} not found, it will be skipped");
        return default;
    }
}
