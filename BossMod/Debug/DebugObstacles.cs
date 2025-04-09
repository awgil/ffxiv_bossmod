using BossMod.Pathfinding;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using ImGuiNET;
using System.IO;

namespace BossMod;

sealed class DebugObstacles(ObstacleMapManager obstacles, IDalamudPluginInterface dalamud)
{
    class Editor(DebugObstacles owner, Bitmap bm, ObstacleMapDatabase.Entry e) : UIBitmapEditor(bm)
    {
        private int _deltaCells;

        protected override void DrawSidebar()
        {
            base.DrawSidebar();

            if (ImGui.InputFloat3("Min bounds", ref e.MinBounds))
                owner.MarkModified();
            if (ImGui.InputFloat3("Max bounds", ref e.MaxBounds))
                owner.MarkModified();
            ImGui.TextUnformatted($"Map area: {e.Origin} + {Bitmap.Width}x{Bitmap.Height} * {Bitmap.PixelSize}");
            if (ImGui.InputInt("Half-width in cells", ref e.ViewWidth))
                owner.MarkModified();
            if (ImGui.InputInt("Half-height in cells", ref e.ViewHeight))
                owner.MarkModified();

            ImGui.TextUnformatted($"Change resolution:");
            ImGui.SameLine();
            if (ImGui.Button("increase"))
            {
                var newBitmap = new Bitmap(Bitmap.Width * 2, Bitmap.Height * 2, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution * 2);
                Bitmap.FullRegion.UpsampleTo(newBitmap, 0, 0);
                CheckpointNoClone(newBitmap);
                --ZoomLevel;
            }
            ImGui.SameLine();
            if (ImGui.Button("decrease"))
            {
                var newBitmap = new Bitmap(Bitmap.Width / 2, Bitmap.Height / 2, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution / 2);
                Bitmap.FullRegion.DownsampleTo(newBitmap, 0, 0, true);
                CheckpointNoClone(newBitmap);
                ++ZoomLevel;
            }

            if (ImGui.Button("Save"))
            {
                Bitmap.Save(owner.Obstacles.RootPath + e.Filename);
            }
            ImGui.SameLine();
            if (ImGui.Button("Reload"))
            {
                using var stream = File.OpenRead(owner.Obstacles.RootPath + e.Filename);
                CheckpointNoClone(new(stream));
            }

            ImGui.SetNextItemWidth(100);
            ImGui.InputInt("###delta", ref _deltaCells);
            using (ImRaii.Disabled(_deltaCells == 0))
            {
                ImGui.SameLine();
                if (ImGui.Button("left"))
                {
                    var newBitmap = new Bitmap(Bitmap.Width + _deltaCells, Bitmap.Height, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution);
                    Bitmap.FullRegion.CopyTo(newBitmap, _deltaCells, 0);
                    CheckpointNoClone(newBitmap);
                }
                ImGui.SameLine();
                if (ImGui.Button("top"))
                {
                    var newBitmap = new Bitmap(Bitmap.Width, Bitmap.Height + _deltaCells, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution);
                    Bitmap.FullRegion.CopyTo(newBitmap, 0, _deltaCells);
                    CheckpointNoClone(newBitmap);
                }
                ImGui.SameLine();
                if (ImGui.Button("right"))
                {
                    var newBitmap = new Bitmap(Bitmap.Width + _deltaCells, Bitmap.Height, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution);
                    Bitmap.FullRegion.CopyTo(newBitmap, 0, 0);
                    CheckpointNoClone(newBitmap);
                }
                ImGui.SameLine();
                if (ImGui.Button("bottom"))
                {
                    var newBitmap = new Bitmap(Bitmap.Width, Bitmap.Height + _deltaCells, Bitmap.Color0, Bitmap.Color1, Bitmap.Resolution);
                    Bitmap.FullRegion.CopyTo(newBitmap, 0, 0);
                    CheckpointNoClone(newBitmap);
                }
            }

            var player = owner.Obstacles.World.Party.Player();
            if (player != null)
            {
                var playerOffset = ((player.Position - e.Origin) / Bitmap.PixelSize).Floor();
                var px = (int)playerOffset.X;
                var py = (int)playerOffset.Z;
                var playerDeepInObstacle = px >= 0 && py >= 0 && px < Bitmap.Width && py < Bitmap.Height && Bitmap[px, py]
                    && (px == 0 || Bitmap[px - 1, py]) && (py == 0 || Bitmap[px, py - 1]) && (px == (Bitmap.Width - 1) || Bitmap[px + 1, py]) && (py == (Bitmap.Height - 1) || Bitmap[px, py + 1]);
                using var color = ImRaii.PushColor(ImGuiCol.Text, 0xff0000ff, playerDeepInObstacle);
                ImGui.TextUnformatted($"Player cell: {px}x{py}");
                if (playerDeepInObstacle)
                {
                    ImGui.SameLine();
                    UIMisc.HelpMarker("Warning: player is deep in the obstacle, pathfinding won't find a good way out...");
                }
            }

            if (HoveredPixel.x >= 0 && HoveredPixel.y >= 0)
            {
                var y = player?.PosRot.Y ?? 0;
                var tl = e.Origin + new WDir(HoveredPixel.x, HoveredPixel.y) * Bitmap.PixelSize;
                var br = tl + new WDir(Bitmap.PixelSize, Bitmap.PixelSize);
                Camera.Instance?.DrawWorldLine(new(tl.X, y, tl.Z), new(tl.X, y, br.Z), 0xff00ffff);
                Camera.Instance?.DrawWorldLine(new(tl.X, y, br.Z), new(br.X, y, br.Z), 0xff00ffff);
                Camera.Instance?.DrawWorldLine(new(br.X, y, br.Z), new(br.X, y, tl.Z), 0xff00ffff);
                Camera.Instance?.DrawWorldLine(new(br.X, y, tl.Z), new(tl.X, y, tl.Z), 0xff00ffff);
            }
        }

        protected override IEnumerable<(int x, int y, Color c)> HighlighedCells()
        {
            var player = owner.Obstacles.World.Party.Player();
            if (player != null)
            {
                var playerOffset = ((player.Position - e.Origin) / Bitmap.PixelSize).Floor();
                yield return ((int)playerOffset.X, (int)playerOffset.Z, new(0xff00ff00));
            }
        }
    }

    internal readonly ObstacleMapManager Obstacles = obstacles;
    private readonly UITree _tree = new();
    private readonly Func<Vector3, string, float, Vector3, Vector3, (Vector3, Vector3)> _createMap = (startingPos, filename, pixelSize, minBounds, maxBounds) => dalamud.GetIpcSubscriber<Vector3, string, float, Vector3, Vector3, (Vector3, Vector3)>("vnavmesh.Nav.BuildBitmapBounded").InvokeFunc(startingPos, filename, pixelSize, minBounds, maxBounds);
    private bool _dbModified;

    private static readonly Vector3 DefaultMinBounds = new(-1024);
    private static readonly Vector3 DefaultMaxBounds = new(1024);

    private Vector3 _minBounds = DefaultMinBounds;
    private Vector3 _maxBounds = DefaultMaxBounds;

    public void Draw()
    {
        ImGui.TextUnformatted($"Database root: {Obstacles.RootPath}");

        var curZoneEntries = Obstacles.Database.Entries.GetValueOrDefault(Obstacles.CurrentKey());
        using (ImRaii.Disabled(!Obstacles.CanEditDatabase()))
        {
            if (ImGui.Button("Create new region"))
                CreateRegion();
            ImGui.SameLine();
            using (ImRaii.Disabled(!_dbModified))
                if (ImGui.Button("Save"))
                    SaveDatabase();
            ImGui.SameLine();
            if (ImGui.Button(_dbModified ? "Revert" : "Reload"))
                ReloadDatabase();
        }

        ImGui.DragFloat3("Map min bounds", ref _minBounds, 1, -1024, 1024);
        ImGui.DragFloat3("Map max bounds", ref _maxBounds, 1, -1024, 1024);

        foreach (var n in _tree.Node($"Current zone: {Obstacles.World.CurrentZone}.{Obstacles.World.CurrentCFCID}###curr", curZoneEntries == null || curZoneEntries.Count == 0))
            DrawEntries(curZoneEntries ?? []);
        foreach (var n in _tree.Nodes(Obstacles.Database.Entries, kv => new($"Zone {kv.Key >> 16}.{kv.Key & 0xFFFF}", kv.Value.Count == 0)))
            DrawEntries(n.Value);
    }

    internal void MarkModified() => _dbModified = true;

    private void DrawEntries(List<ObstacleMapDatabase.Entry> entries)
    {
        Action? modifications = null;
        using var disableScope = ImRaii.Disabled(!Obstacles.CanEditDatabase());
        for (int i = 0; i < entries.Count; ++i)
        {
            using var id = ImRaii.PushId(i);
            var index = i;
            using (ImRaii.Disabled(i == 0))
                if (ImGui.Button("Move up"))
                    modifications += () => (entries[index], entries[index - 1]) = (entries[index - 1], entries[index]);
            ImGui.SameLine();
            using (ImRaii.Disabled(i == entries.Count - 1))
                if (ImGui.Button("Move down"))
                    modifications += () => (entries[index], entries[index + 1]) = (entries[index + 1], entries[index]);
            ImGui.SameLine();
            if (ImGui.Button("Delete"))
                modifications += () => DeleteMap(entries, index);
            ImGui.SameLine();
            if (ImGui.Button("Edit"))
                OpenEditor(entries[index]);
            ImGui.SameLine();
            ImGui.TextUnformatted($"[{i}] {entries[i]}");
        }

        modifications?.Invoke();
        _dbModified |= modifications != null;
    }

    private void CreateRegion()
    {
        var pos = Obstacles.World.Party.Player()?.PosRot.XYZ() ?? default;
        var filename = GenerateMapName();
        var (min, max) = _createMap(pos, Obstacles.RootPath + filename, 0.5f, _minBounds, _maxBounds);
        var (tweakMin, tweakMax) = _minBounds == DefaultMinBounds && _maxBounds == DefaultMaxBounds ? (min - new Vector3(0, 1, 0), max + new Vector3(0, 10, 0)) : (min, max); // account for jumping etc...
        var entry = new ObstacleMapDatabase.Entry(tweakMin, tweakMax, new(min.XZ()), 60, 60, filename);
        OpenEditor(entry);
        Obstacles.Database.Entries.GetOrAdd(Obstacles.CurrentKey()).Add(entry);
        _dbModified = true;
    }

    private void ReloadDatabase()
    {
        Obstacles.ReloadDatabase();
        _dbModified = false;
    }

    private void SaveDatabase()
    {
        Obstacles.SaveDatabase();
        _dbModified = false;
    }

    private void DeleteMap(List<ObstacleMapDatabase.Entry> entries, int index)
    {
        new FileInfo(Obstacles.RootPath + entries[index].Filename).Delete();
        entries.RemoveAt(index);
        _dbModified = true;
    }

    private string GenerateMapName()
    {
        for (int i = 1; ; ++i)
        {
            var name = $"{Obstacles.World.CurrentZone}.{Obstacles.World.CurrentCFCID}.{i}.bmp";
            if (!new FileInfo(Obstacles.RootPath + name).Exists)
                return name;
        }
    }

    private void OpenEditor(ObstacleMapDatabase.Entry entry)
    {
        using var stream = File.OpenRead(Obstacles.RootPath + entry.Filename);
        var editor = new Editor(this, new(stream), entry);
        _ = new UISimpleWindow($"Obstacle map {entry.Filename}", editor.Draw, true, new(1000, 1000));
    }
}
