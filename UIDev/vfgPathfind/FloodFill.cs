using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UIDev.vfgPathfind;

public class FloodFill
{
    public int NextT { get; private set; }
    private Map _map;
    private BitArray _solutions; // true if voxel is reachable
    private List<(int dx, int dz)> _moves = new();

    public bool Solution(int x, int z, int t) => _map.InBounds(x, z, t) ? _solutions[t * _map.Width * _map.Height + z * _map.Width + x] : false;
    public bool this[int x, int z, int t] => Solution(x, z, t);

    //public ref Node NodeByIndex(int index) => ref _nodes[index];
    //public int CellIndex(int x, int y) => y * _map.Width + x;
    //public WPos CellCenter(int index) => _map.GridToWorld(index % _map.Width, index / _map.Width, 0.5f, 0.5f);

    public FloodFill(Map map, float speed, Vector3 start)
    {
        _map = map;
        _solutions = new(map.Voxels.Length);

        var r = speed * _map.TimeResolution / _map.SpaceResolution;
        var ir = (int)r;
        var rsq = r * r;
        var l = (int)(r / Math.Sqrt(2));
        _moves.Capacity = 5 + (l - 1) * 8;
        _moves.Add((0, 0)); // can stay still
        _moves.Add((0, ir)); // can move cardinals
        _moves.Add((0, -ir));
        _moves.Add((ir, 0));
        _moves.Add((-ir, 0));
        for (int x = 1; x <= l; ++x)
        {
            var z = (int)Math.Sqrt(rsq - x * x);
            _moves.Add((x, z));
            _moves.Add((x, -z));
            _moves.Add((-x, z));
            _moves.Add((-x, -z));
            if (x != z)
            {
                _moves.Add((z, x));
                _moves.Add((z, -x));
                _moves.Add((-z, x));
                _moves.Add((-z, -x));
            }
        }

        var g = _map.WorldToGrid(start);
        _solutions[g.z * _map.Width + g.x] = true;
        NextT = 1;
    }

    public (int t, int minZ) ExecuteStep()
    {
        if (NextT >= _map.Duration)
            return (-1, 0);

        var tPrev = (NextT - 1) * _map.Width * _map.Height;
        var tNext = NextT * _map.Width * _map.Height;
        var minZ = int.MaxValue;
        for (int z = 0; z < _map.Height; ++z)
        {
            for (int x = 0; x < _map.Width; ++x)
            {
                var xz = z * _map.Width + x;
                if (_map.Voxels[tNext + xz])
                    continue;

                foreach (var (dx, dz) in _moves)
                {
                    var x1 = x + dx;
                    var z1 = z + dz;
                    if (x1 >= 0 && x1 < _map.Width && z1 >= 0 && z1 < _map.Height && _solutions[tPrev + z1 * _map.Width + x1])
                    {
                        _solutions[tNext + xz] = true;
                        minZ = Math.Min(minZ, z);
                        break;
                    }
                }
            }
        }

        return (NextT++, minZ);
    }

    public int ExecuteUntilPoint(Vector3 goal)
    {
        var g = _map.WorldToGrid(goal);
        while (ExecuteStep().t is var t && t >= 0)
            if (Solution(g.x, g.z, t))
                return t;
        return -1;
    }

    public (int x, int z)? FindMove(int x, int z, int time)
    {
        var t = time - 1;
        if (!_map.InBounds(x, z, t))
            return null;

        var tPrev = t * _map.Width * _map.Height;
        foreach (var (dx, dz) in _moves)
        {
            var x1 = x + dx;
            var z1 = z + dz;
            if (x1 >= 0 && x1 < _map.Width && z1 >= 0 && z1 < _map.Height && _solutions[tPrev + z1 * _map.Width + x1])
            {
                return (x1, z1);
            }
        }

        return null;
    }
}
