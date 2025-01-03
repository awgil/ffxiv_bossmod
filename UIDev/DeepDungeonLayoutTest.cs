using BossMod;
using BossMod.Global.DeepDungeon;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System.IO;
using System.Text.Json;

namespace UIDev;
internal sealed class Util
{
    public static readonly List<DDTerritory> AllTerritories = [
        new(1, 1, 174),
        new(1, 2, 175),
        new(1, 3, 176),
        new(1, 4, 177),
        new(1, 5, 178),
        new(1, 6, 204),
        new(1, 7, 205),
        new(1, 8, 206),
        new(1, 9, 207),
        new(1, 10, 208),
        new(1, 11, 209),
        new(1, 12, 210),
        new(1, 13, 211),
        new(1, 14, 212),
        new(1, 15, 213),
        new(1, 16, 214),
        new(1, 17, 215),
        new(1, 18, 216),
        new(1, 19, 217),
        new(1, 20, 218),

        new(2, 1, 540),
        new(2, 2, 541),
        new(2, 3, 542),
        new(2, 4, 543),
        new(2, 5, 544),
        new(2, 6, 545),
        new(2, 7, 546),
        new(2, 8, 547),
        new(2, 9, 548),
        new(2, 10, 549),

        new(3, 1, 897),
        new(3, 2, 898),
        new(3, 3, 899),
        new(3, 4, 900),
        new(3, 5, 901),
        new(3, 6, 902),
        new(3, 7, 903),
        new(3, 8, 904),
        new(3, 9, 905),
        new(3, 10, 906),
    ];
}

record class DDTerritory(byte DungeonId, byte Floorset, uint CFCID)
{
    public override string ToString()
    {
        var name = Service.LuminaRow<DeepDungeon>(DungeonId)!.Value.Name.ToString();
        var lastfloor = Floorset * 10;
        var firstfloor = lastfloor - 9;
        return $"{name} {firstfloor}-{lastfloor} (cfc {CFCID})";
    }
}

record struct Collider(uint Id, WPos Position);

record struct Transform(V3 Translation, V3 Rotation)
{
    public static implicit operator Transform(FileLayerGroupTransform t) => new(t.Translation, t.Rotation);
}

record struct V3(float X, float Y, float Z)
{
    public static implicit operator V3(Vector3 v) => new(v.X, v.Y, v.Z);
    public static implicit operator Vector3(V3 v) => new(v.X, v.Y, v.Z);
    public readonly Vector2 XZ() => new(X, Z);
}

record class Group(
    Transform ParentTransform,
    List<Transform> ChildTransform
)
{
    public WPos GetActualPosition()
    {
        var prot = Quaternion.CreateFromYawPitchRoll(ParentTransform.Rotation.Y, ParentTransform.Rotation.X, ParentTransform.Rotation.Z);
        var roombase = new WDir(ParentTransform.Translation.XZ());
        var childt = Vector3.Transform(ChildTransform[0].Translation, prot);
        return (roombase + new WDir(childt.XZ())).ToWPos();
    }
}

record struct TaggedPos(string Key, Group Group, WPos Position);

unsafe class DeepDungeonLayoutTest : TestWindow
{
    private readonly SortedDictionary<uint, Group> Translations = [];

    private readonly Dictionary<string, Floor<WPos>> LoadedFloors = [];

    public DeepDungeonLayoutTest() : base("Floorset generator", new(400, 400), ImGuiWindowFlags.None)
    {
        using (var fstream = new FileStream(AutoClear.WallsFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
        {
            LoadedFloors = JsonSerializer.Deserialize<Dictionary<string, Floor<WPos>>>(fstream)!;
        }
    }

    public override void Draw()
    {
        if (ImGui.Button("Search"))
        {
            foreach (var row in Service.LuminaSheet<DeepDungeonRoom>()!)
            {
                foreach (var l in row.Level)
                {
                    if (l.RowId == 0x5FF640)
                        Service.Log($"found it in {row}");
                }
            }
        }

        if (ImGui.Button("Generate"))
        {
            foreach (var t in Util.AllTerritories)
            {
                Translations.Clear();
                LoadedFloors[$"{t.DungeonId}.{t.Floorset}"] = Generate(t).Map(m => m > 0 ? Translations[m].GetActualPosition() : default);
            }
            Save2();
            Service.Log($"saved {LoadedFloors.Count} wall positions");
        }
    }

    private void Save2()
    {
        using var fstream = new FileStream(AutoClear.WallsFile, FileMode.Create, FileAccess.Write, FileShare.Read);
        JsonSerializer.Serialize(fstream, LoadedFloors, new JsonSerializerOptions() { WriteIndented = true });
    }

    private Floor<uint> Generate(DDTerritory terr)
    {
        var t = Service.LuminaRow<ContentFinderCondition>(terr.CFCID)!.Value;
        var layer = Service.LuminaSheet<DeepDungeonLayer>()!.First(l => l.DeepDungeon.RowId == terr.DungeonId && l.FloorSet == terr.Floorset - 1);

        var data = Service.LuminaGameData?.GetFile($"bg/{t.TerritoryType.Value.Bg}.lvb");
        if (data != null)
        {
            fixed (byte* lvbData = &data.Data[0])
            {
                WalkFileScene(FindSection<FileSceneHeader>((FileHeader*)lvbData, 0x314E4353));
            }
        }

        return MakeFloor(layer);
    }

    private Floor<uint> MakeFloor(DeepDungeonLayer layer) => new(layer.DeepDungeon.RowId, layer.FloorSet, MakeTileset(layer.RoomA), MakeTileset(layer.RoomB));

    // DDMap5X is listed left to right, top to bottom, so room 0 is at 0,0, room 1 is at 1,0, etc
    private Tileset<uint> MakeTileset(SubrowRef<DeepDungeonMap5X> Rooms) => new(Rooms.Value.SelectMany(v => v.DeepDungeonRoom).Select(r => MakeRoom(r.Value)).ToList());

    private RoomData<uint> MakeRoom(DeepDungeonRoom room) => new(room.Level[0].RowId, SensibleId(room.Level[1].RowId), SensibleId(room.Level[2].RowId), SensibleId(room.Level[3].RowId), SensibleId(room.Level[4].RowId));

    private uint SensibleId(uint input) => input > 1 ? input : 0;

    private T* FindSection<T>(FileHeader* header, uint magic) where T : unmanaged
    {
        foreach (var s in header->Sections)
            if (s->Magic == magic)
                return s->Data<T>();
        return null;
    }

    private void WalkFileScene(FileSceneHeader* scene)
    {
        if (scene == null)
            return;

        for (var i = 0; i < scene->NumEmbeddedLayerGroups; i++)
            Service.Log($"fill from layer group: {i}");

        foreach (var off in scene->LayerGroupResourceOffsets)
        {
            var lcb = Service.LuminaGameData?.GetFile(MemoryHelper.ReadStringNullTerminated((nint)scene->LayerGroupResource(off)));
            if (lcb != null)
                fixed (byte* lcbData = &lcb.Data[0])
                    WalkFileLayerGroup(FindSection<FileLayerGroupHeader>((FileHeader*)lcbData, 0x3150474C));
        }
    }

    private void WalkFileLayerGroup(FileLayerGroupHeader* lg)
    {
        if (lg == null)
            return;

        foreach (var layerOff in lg->LayerOffsets)
        {
            var layer = lg->Layer(layerOff);
            WalkFileLayer(layer, lg->Id, layer->Key, 0, 32);
        }
    }

    private void WalkFilePrefab(FileSceneHeader* scene, int layerGroupId, ushort layerId, ulong prefabKey, int subShift)
    {
        if (scene == null || subShift < 0)
            return;
        if (scene->NumEmbeddedLayerGroups != 1)
        {
            Service.Log("prefab is poopoo");
            return;
        }
        ref var lg = ref scene->EmbeddedLayerGroups[0];
        if (lg.NumLayers != 1)
        {
            Service.Log($"prefab is very poopoo");
            return;
        }
        WalkFileLayer(lg.Layer(lg.LayerOffsets[0]), layerGroupId, layerId, prefabKey, subShift);
    }

    private void WalkFileLayer(FileLayerGroupLayer* layer, int layerGroupId, ushort layerId, ulong prefabKey, int subShift)
    {
        foreach (var instOffset in layer->InstanceOffsets)
        {
            var inst = layer->Instance(instOffset);
            var key = prefabKey == 0 ? ((ulong)inst->Key) << 32 : prefabKey | (inst->Key << subShift);
            if (inst->Type is InstanceType.SharedGroup)
                Translations[inst->Key] = new(inst->Transform, []);

            if (inst->Type is InstanceType.CollisionBox && prefabKey > 0 && Translations.ContainsKey((uint)(prefabKey >> 32)))
                Translations[(uint)(prefabKey >> 32)].ChildTransform.Add(inst->Transform);

            if (inst->Type is InstanceType.SharedGroup or InstanceType.HelperObject)
            {
                var prefab = (FileLayerGroupInstanceSharedGroup*)inst;
                var sgb = Service.LuminaGameData?.GetFile(MemoryHelper.ReadStringNullTerminated((nint)prefab->Path));
                if (sgb != null)
                    fixed (byte* sgbData = &sgb.Data[0])
                        WalkFilePrefab(FindSection<FileSceneHeader>((FileHeader*)sgbData, 0x314E4353), layerGroupId, layerId, key, subShift - 8);
            }
        }
    }
}
