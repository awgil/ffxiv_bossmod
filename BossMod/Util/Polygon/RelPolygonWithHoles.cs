using EarcutNet;

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

    // build a triangulation of the polygon
    public bool Triangulate(List<RelTriangle> result)
    {
        var vertexCount = Vertices.Count;
        var pts = vertexCount <= 256 ? stackalloc double[vertexCount * 2] : new double[vertexCount * 2];
        var verticesSpan = CollectionsMarshal.AsSpan(Vertices);
        for (int i = 0, j = 0; i < vertexCount; ++i, j += 2)
        {
            var v = verticesSpan[i];
            pts[j] = v.X;
            pts[j + 1] = v.Z;
        }
        var tess = CollectionsMarshal.AsSpan(Earcut.Tessellate(pts[..(vertexCount * 2)], HoleStarts));
        var count = tess.Length;
        for (var i = 0; i < count; i += 3)
        {
            result.Add(new(verticesSpan[tess[i]], verticesSpan[tess[i + 1]], verticesSpan[tess[i + 2]]));
        }

        return count > 0;
    }

    public List<RelTriangle> Triangulate()
    {
        var result = new List<RelTriangle>(Vertices.Count);
        Triangulate(result);
        return result;
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
