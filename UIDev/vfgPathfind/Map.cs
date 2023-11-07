using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace UIDev.vfgPathfind;

public class Map
{
    public float SpaceResolution { get; private init; } // voxel size, in world units
    public float TimeResolution { get; private init; } // voxel size, in seconds
    public int Width { get; private init; } // in voxels, x coord
    public int Height { get; private init; } // in voxels, z coord
    public int Duration { get; private init; } // in voxels, t coord
    public BitArray Voxels { get; private set; } // true if voxel is blocked

    public Vector3 Center { get; private init; }
    public Vector3 InvResolution { get; private init; }

    public bool this[int x, int z, int t] => InBounds(x, z, t) ? Voxels[t * Width * Height + z * Width + x] : false;

    public Map(float spaceResolution, float timeResolution, Vector3 center, float worldHalfWidth, float worldHalfHeight, float maxDuration)
    {
        SpaceResolution = spaceResolution;
        TimeResolution = timeResolution;
        Width = 2 * (int)MathF.Ceiling(worldHalfWidth / spaceResolution);
        Height = 2 * (int)MathF.Ceiling(worldHalfHeight / spaceResolution);
        Duration = (int)MathF.Ceiling(maxDuration / timeResolution);
        Voxels = new(Width * Height * Duration);

        Center = center;
        var invSpaceRes = 1.0f / SpaceResolution;
        InvResolution = new(invSpaceRes, invSpaceRes, 1.0f / TimeResolution);
    }

    public Map Clone()
    {
        var res = (Map)MemberwiseClone();
        res.Voxels = new(Voxels);
        return res;
    }

    public Vector2 WorldToGridFrac(Vector3 world)
    {
        var offset = world - Center;
        return new Vector2(Width / 2 + offset.X * InvResolution.X, Height / 2 + offset.Z * InvResolution.Y);
    }

    public int ClampX(int x) => Math.Clamp(x, 0, Width - 1);
    public int ClampZ(int z) => Math.Clamp(z, 0, Height - 1);
    public int ClampT(int t) => Math.Clamp(t, 0, Duration - 1);

    public (int x, int z) FracToGrid(Vector2 frac) => ((int)MathF.Floor(frac.X), (int)MathF.Floor(frac.Y));
    public (int x, int z) WorldToGrid(Vector3 world) => FracToGrid(WorldToGridFrac(world));
    //public (int x, int z, int t) ClampToGrid((int x, int z, int t) pos) => (ClampX(pos.x), ClampZ(pos.z), ClampT(pos.t));
    public bool InBounds(int x, int z, int t) => x >= 0 && x < Width && z >= 0 && z < Height && t >= 0 && t < Duration;

    public Vector3 GridToWorld(int gx, int gz, float fx, float fz)
    {
        // TODO: reconstruct Y from height profile
        return Center + new Vector3(gx - Width / 2 + fx, 0, gz - Height / 2 + fz) * SpaceResolution;
    }

    // block all the voxels for which the shape function returns true for specified time intervals
    public void BlockPixelsInside(Vector3 boundsMin, Vector3 boundsMax, Func<Vector3, bool> shape, float tStart, float tDuration, float tRepeat)
    {
        List<(int begin, int end)> intTime = new();
        var tMax = Duration * TimeResolution;
        for (var t = tStart; t < tMax; t += tRepeat)
        {
            var ib = ClampT((int)MathF.Floor(t * InvResolution.Z) - 1);
            var ie = ClampT((int)MathF.Floor((t + tDuration) * InvResolution.Z) + 1);
            intTime.Add((ib, ie));
        }
        //foreach (var (b, e) in time)
        //{
        //    var ib = ClampT((int)MathF.Floor(b * InvResolution.Z));
        //    var ie = ClampT((int)MathF.Floor(e * InvResolution.Z));
        //    if (ib <= ie)
        //        intTime.Add((ib, ie));
        //}
        if (intTime.Count == 0)
            return;

        //intTime.SortBy(e => e.Item1);
        //int mergeTo = 0;
        //for (int i = 1; i < intTime.Count; ++i)
        //{
        //    if (intTime[i].begin <= intTime[0].end + 1)
        //    {
        //        intTime[mergeTo] = (intTime[mergeTo].begin, intTime[i].end);
        //    }
        //    else
        //    {
        //        intTime[++mergeTo] = intTime[i];
        //    }
        //}
        //++mergeTo;
        //intTime.RemoveRange(mergeTo, intTime.Count - mergeTo);

        var relMin = boundsMin - Center;
        var relMax = boundsMax - Center;
        var xmin = ClampX(Width / 2 + (int)MathF.Floor(relMin.X * InvResolution.X));
        var xmax = ClampX(Width / 2 + (int)MathF.Ceiling(relMax.X * InvResolution.X));
        var zmin = ClampZ(Height / 2 + (int)MathF.Floor(relMin.Z * InvResolution.Y));
        var zmax = ClampZ(Height / 2 + (int)MathF.Ceiling(relMax.Z * InvResolution.Y));

        var tOff = Width * Height;
        var cz = Center + new Vector3(xmin - Width / 2 + 0.5f, 0, zmin - Height / 2 + 0.5f) * SpaceResolution;
        for (int z = zmin; z <= zmax; ++z)
        {
            var cx = cz;
            for (int x = xmin; x <= xmax; ++x)
            {
                if (shape(cx))
                {
                    var xz = z * Width + x;
                    foreach (var (t0, t1) in intTime)
                    {
                        for (int t = t0; t <= t1; ++t)
                        {
                            Voxels[t * tOff + xz] = true;
                        }
                    }
                }
                cx.X += SpaceResolution;
            }
            cz.Z += SpaceResolution;
        }
    }
}
