using System.IO;
using System.Text.Json;

namespace BossMod.Pathfinding;

public sealed class ObstacleMapDatabase
{
    public sealed record class Entry(Vector3 MinBounds, Vector3 MaxBounds, WPos Origin, int ViewWidth, int ViewHeight, string Filename)
    {
        public Vector3 MinBounds = MinBounds;
        public Vector3 MaxBounds = MaxBounds;
        public WPos Origin = Origin;
        public int ViewWidth = ViewWidth;
        public int ViewHeight = ViewHeight;
        public string Filename = Filename;

        public bool Contains(Vector3 p) => p.X >= MinBounds.X && p.Y >= MinBounds.Y && p.Z >= MinBounds.Z && p.X <= MaxBounds.X && p.Y <= MaxBounds.Y && p.Z <= MaxBounds.Z;
    }

    public readonly Dictionary<uint, List<Entry>> Entries = [];

    public void Load(string listPath)
    {
        Entries.Clear();
        try
        {
            using var json = Serialization.ReadJson(listPath);
            foreach (var jentries in json.RootElement.EnumerateObject())
            {
                var sep = jentries.Name.IndexOf('.', StringComparison.Ordinal);
                var zone = sep >= 0 ? uint.Parse(jentries.Name.AsSpan()[..sep]) : uint.Parse(jentries.Name);
                var cfc = sep >= 0 ? uint.Parse(jentries.Name.AsSpan()[(sep + 1)..]) : 0;
                var entries = Entries[(zone << 16) | cfc] = [];
                foreach (var jentry in jentries.Value.EnumerateArray())
                {
                    entries.Add(new(
                        ReadVec3(jentry, nameof(Entry.MinBounds)),
                        ReadVec3(jentry, nameof(Entry.MaxBounds)),
                        ReadWPos(jentry, nameof(Entry.Origin)),
                        jentry.GetProperty(nameof(Entry.ViewWidth)).GetInt32(),
                        jentry.GetProperty(nameof(Entry.ViewHeight)).GetInt32(),
                        jentry.GetProperty(nameof(Entry.Filename)).GetString() ?? ""
                    ));
                }
            }
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to load obstacle map database '{listPath}': {ex}");
        }
    }

    public void Save(string listPath)
    {
        try
        {
            using var fstream = new FileStream(listPath, FileMode.Create, FileAccess.Write, FileShare.Read);
            using var jwriter = Serialization.WriteJson(fstream);
            jwriter.WriteStartObject();
            foreach (var (key, entries) in Entries)
            {
                if (entries.Count == 0)
                    continue; // no entries, skip

                var zone = key >> 16;
                var cfc = key & 0xFFFF;
                jwriter.WriteStartArray(cfc != 0 ? $"{zone}.{cfc}" : $"{zone}");
                foreach (var e in entries)
                {
                    jwriter.WriteStartObject();
                    WriteVec3(jwriter, nameof(Entry.MinBounds), e.MinBounds);
                    WriteVec3(jwriter, nameof(Entry.MaxBounds), e.MaxBounds);
                    WriteWPos(jwriter, nameof(Entry.Origin), e.Origin);
                    jwriter.WriteNumber(nameof(Entry.ViewWidth), e.ViewWidth);
                    jwriter.WriteNumber(nameof(Entry.ViewHeight), e.ViewHeight);
                    jwriter.WriteString(nameof(Entry.Filename), e.Filename);
                    jwriter.WriteEndObject();
                }
                jwriter.WriteEndArray();
            }
            jwriter.WriteEndObject();
            Service.Log($"Obstacle map database successfully to '{listPath}'");
        }
        catch (Exception ex)
        {
            Service.Log($"Failed to save obstacle map database to '{listPath}': {ex}");
        }
    }

    private Vector3 ReadVec3(JsonElement obj, string tag) => new(obj.GetProperty(tag + "X").GetSingle(), obj.GetProperty(tag + "Y").GetSingle(), obj.GetProperty(tag + "Z").GetSingle());
    private WPos ReadWPos(JsonElement obj, string tag) => new(obj.GetProperty(tag + "X").GetSingle(), obj.GetProperty(tag + "Z").GetSingle());

    private void WriteVec3(Utf8JsonWriter w, string tag, Vector3 v)
    {
        w.WriteNumber(tag + "X", v.X);
        w.WriteNumber(tag + "Y", v.Y);
        w.WriteNumber(tag + "Z", v.Z);
    }

    private void WriteWPos(Utf8JsonWriter w, string tag, WPos v)
    {
        w.WriteNumber(tag + "X", v.X);
        w.WriteNumber(tag + "Z", v.Z);
    }
}
