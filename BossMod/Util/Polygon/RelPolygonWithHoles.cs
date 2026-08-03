namespace BossMod;

// a complex polygon that is a single simple-polygon exterior minus 0 or more simple-polygon holes; all edges are assumed to be non intersecting
// hole-starts list contains starting index of each hole
[SkipLocalsInit]
public sealed class RelPolygonWithHoles(List<WDir> vertices, List<int> holeStarts)
{
    // constructor for simple polygon
    public readonly List<WDir> Vertices = vertices;
    public readonly List<int> HoleStarts = holeStarts;
    public RelPolygonWithHoles(List<WDir> simpleVertices) : this(simpleVertices, []) { }
    public ReadOnlySpan<WDir> AllVertices => CollectionsMarshal.AsSpan(Vertices);
    public ReadOnlySpan<WDir> Exterior => AllVertices[..ExteriorEnd];
    public ReadOnlySpan<WDir> Interior(int index) => AllVertices[HoleStarts[index]..HoleEnd(index)];

    private int ExteriorEnd => HoleStarts.Count > 0 ? HoleStarts[0] : Vertices.Count;
    private int HoleEnd(int index) => index + 1 < HoleStarts.Count ? HoleStarts[index + 1] : Vertices.Count;

    // add new hole; input is assumed to be a simple polygon
    public void AddHole(List<WDir> simpleHole)
    {
        HoleStarts.Add(Vertices.Count);
        Vertices.AddRange(simpleHole);
    }

    // build a new polygon by transformation
    public RelPolygonWithHoles Transform(WDir offset, WDir rotation)
    {
        var count = Vertices.Count;
        var newVerts = new List<WDir>(count);
        for (var i = 0; i < count; ++i)
        {
            newVerts.Add(Vertices[i].Rotate(rotation) + offset);
        }
        return new RelPolygonWithHoles(newVerts, [.. HoleStarts]);
    }
}
