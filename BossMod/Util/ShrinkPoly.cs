namespace BossMod;

public static class ShrinkPoly
{
    public static List<WDir> Shrink(IEnumerable<WDir> vertices, float shrinkRadius)
    {
        var vertsIn = vertices.ToList();

        // remove collinear pairs of vertices, since we can't intersect their adjusted lines
        for (var i = vertsIn.Count - 1; i > 1; i--)
        {
            var prevDir = vertsIn[i - 1] - vertsIn[i - 2];
            var curDir = vertsIn[i] - vertsIn[i - 1];
            if (prevDir.Normalized() == curDir.Normalized())
            {
                vertsIn[i - 1] = vertsIn[i];
                vertsIn.RemoveAt(i);
            }
        }
        if (vertsIn.Count < 3)
            throw new InvalidOperationException($"Shrink: at least 3 verts required in input");

        var vertsOut = new List<WDir>();

        var firstLine = Nudge(vertsIn[^1], vertsIn[0], shrinkRadius);
        var prevLine = firstLine;

        for (var i = 0; i < vertsIn.Count - 1; i++)
        {
            var curLine = Nudge(vertsIn[i], vertsIn[i + 1], shrinkRadius);
            vertsOut.Add(LineLine(prevLine.Item1, prevLine.Item2, curLine.Item1, curLine.Item2));
            prevLine = curLine;
        }

        vertsOut.Add(LineLine(prevLine.Item1, prevLine.Item2, firstLine.Item1, firstLine.Item2));
        return vertsOut;
    }

    private static (WDir, WDir) Nudge(WDir start, WDir end, float distance)
    {
        var off = (end - start).Normalized().OrthoL() * distance;
        return (start + off, end + off);
    }

    private static WDir LineLine(WDir l11, WDir l12, WDir l21, WDir l22)
    {
        var (x1, y1) = (l11.X, l11.Z);
        var (x2, y2) = (l12.X, l12.Z);
        var (x3, y3) = (l21.X, l21.Z);
        var (x4, y4) = (l22.X, l22.Z);

        // thanks wikipedia!
        var px = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4))
            / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
        var py = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4))
            / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4));
        return new(px, py);
    }
}
