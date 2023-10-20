using ClipperLib;
using EarcutNet;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    // reusable polygon clipper & tesselator
    // currently uses Clipper library (Vatti algorithm) for clipping and Earcut.net library (earcutting) for triangulating
    public class Clip2D
    {
        private const float _scale = 1000;
        private const float _invScale = 1 / _scale;

        private Clipper _clipper;
        private List<IntPoint> _clipPoly = new();

        public IEnumerable<WPos> ClipPoly
        {
            get => _clipPoly.Select(ConvertPoint);
            set => _clipPoly = value.Select(ConvertPoint).ToList();
        }

        public Clip2D(bool strictlySimple = true)
        {
            _clipper = new(strictlySimple ? Clipper.ioStrictlySimple : 0);
        }

        public PolyTree Simplify(IEnumerable<IEnumerable<WPos>> poly)
        {
            _clipper.Clear();
            foreach (var p in poly)
                _clipper.AddPath(p.Select(ConvertPoint).ToList(), PolyType.ptSubject, true);
            PolyTree solution = new();
            _clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftEvenOdd);
            return solution;
        }

        public PolyTree Union(IEnumerable<IEnumerable<WPos>> polys)
        {
            PolyTree solution = new();
            foreach (var pts in polys)
            {
                _clipper.Clear();
                _clipper.AddPaths(Clipper.PolyTreeToPaths(solution), PolyType.ptClip, true);
                _clipper.AddPath(pts.Select(ConvertPoint).ToList(), PolyType.ptSubject, true);
                _clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftEvenOdd);
            }
            return solution;
        }

        public PolyTree Union(PolyTree starting, IEnumerable<IEnumerable<WPos>> add)
        {
            _clipper.Clear();
            _clipper.AddPaths(Clipper.PolyTreeToPaths(starting), PolyType.ptSubject, true);
            foreach (var p in add)
                _clipper.AddPath(p.Select(ConvertPoint).ToList(), PolyType.ptClip, true);
            PolyTree solution = new();
            _clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftEvenOdd);
            return solution;
        }

        public PolyTree Difference(PolyTree starting, IEnumerable<IEnumerable<WPos>> remove)
        {
            _clipper.Clear();
            _clipper.AddPaths(Clipper.PolyTreeToPaths(starting), PolyType.ptSubject, true);
            foreach (var p in remove)
                _clipper.AddPath(p.Select(ConvertPoint).ToList(), PolyType.ptClip, true);
            PolyTree solution = new();
            _clipper.Execute(ClipType.ctDifference, solution, PolyFillType.pftEvenOdd);
            return solution;
        }

        public PolyTree Difference(IEnumerable<WPos> starting, PolyTree remove)
        {
            _clipper.Clear();
            _clipper.AddPath(starting.Select(ConvertPoint).ToList(), PolyType.ptSubject, true);
            _clipper.AddPaths(Clipper.PolyTreeToPaths(remove), PolyType.ptClip, true);
            PolyTree solution = new();
            _clipper.Execute(ClipType.ctDifference, solution, PolyFillType.pftEvenOdd);
            return solution;
        }

        public PolyTree Intersect(PolyTree subj, IEnumerable<IEnumerable<WPos>> clip)
        {
            _clipper.Clear();
            _clipper.AddPaths(Clipper.PolyTreeToPaths(subj), PolyType.ptSubject, true);
            foreach (var p in clip)
                _clipper.AddPath(p.Select(ConvertPoint).ToList(), PolyType.ptClip, true);
            PolyTree solution = new();
            _clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd);
            return solution;
        }

        public PolyTree Intersect(PolyTree subj, PolyTree clip)
        {
            _clipper.Clear();
            _clipper.AddPaths(Clipper.PolyTreeToPaths(subj), PolyType.ptSubject, true);
            _clipper.AddPaths(Clipper.PolyTreeToPaths(clip), PolyType.ptClip, true);
            PolyTree solution = new();
            _clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd);
            return solution;
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulate(PolyTree poly)
        {
            _clipper.Clear();
            _clipper.AddPath(_clipPoly, PolyType.ptClip, true);
            _clipper.AddPaths(Clipper.PolyTreeToPaths(poly), PolyType.ptSubject, true);

            PolyTree solution = new();
            _clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd);
            return Triangulate(solution);
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulate(IEnumerable<WPos> pts)
        {
            _clipper.Clear();
            _clipper.AddPath(_clipPoly, PolyType.ptClip, true);
            _clipper.AddPath(pts.Select(ConvertPoint).ToList(), PolyType.ptSubject, true);

            PolyTree solution = new();
            _clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd);
            return Triangulate(solution);
        }

        public static List<(WPos, WPos, WPos)> Triangulate(PolyTree solution)
        {
            List<(WPos, WPos, WPos)> triangulation = new();
            Triangulate(triangulation, solution);
            return triangulation;
        }

        public static IEnumerable<IEnumerable<WPos>> FullContour(PolyTree tree)
        {
            foreach (var p in Clipper.PolyTreeToPaths(tree))
                yield return p.Select(ConvertPoint);
        }

        // returns root node (which is 'hole') if point is outside any 'root' polys
        public static PolyNode FindNodeContainingPoint(PolyTree solution, WPos point)
        {
            var pt = ConvertPoint(point);
            PolyNode node = solution;
            while (true)
            {
                var child = node.Childs.Find(o => !o.IsOpen && o.Contour.Count > 0 && Clipper.PointInPolygon(pt, o.Contour) != 0);
                if (child == null)
                    return node;
                node = child;
            }
        }

        public static IEnumerable<(WPos, WPos)> Contour(PolyNode n)
        {
            if (n.Contour.Count == 0)
                yield break;
            var prev = ConvertPoint(n.Contour.Last());
            foreach (var next in n.Contour.Select(ConvertPoint))
            {
                yield return (prev, next);
                prev = next;
            }
        }

        private static void Triangulate(List<(WPos, WPos, WPos)> result, PolyNode node)
        {
            foreach (var outer in node.Childs.Where(o => !o.IsOpen && o.Contour.Count > 0))
            {
                List<double> pts = new();
                List<int> holes = new();

                AddClipperPoly(pts, outer.Contour);
                foreach (var hole in outer.Childs.Where(h => h.Contour.Count > 0))
                {
                    holes.Add(pts.Count / 2);
                    AddClipperPoly(pts, hole.Contour);
                }

                var tess = Earcut.Tessellate(pts, holes);
                for (int i = 0; i < tess.Count; i += 3)
                    result.Add((GetVertex(pts, tess[i]), GetVertex(pts, tess[i + 1]), GetVertex(pts, tess[i + 2])));
            }
        }

        private static void AddClipperPoly(List<double> output, List<IntPoint> pts)
        {
            foreach (var p in pts.Select(ConvertPoint))
            {
                output.Add(p.X);
                output.Add(p.Z);
            }
        }

        private static WPos GetVertex(List<double> coords, int index)
        {
            var x = (float)coords[2 * index];
            var z = (float)coords[2 * index + 1];
            return new(x, z);
        }

        private static IntPoint ConvertPoint(WPos pt) => new(pt.X * _scale, pt.Z * _scale);
        private static WPos ConvertPoint(IntPoint pt) => new(pt.X * _invScale, pt.Y * _invScale);
    }
}
