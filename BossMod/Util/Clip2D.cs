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

        private Clipper _clipper = new(Clipper.ioStrictlySimple);
        private List<IntPoint> _clipPoly = new();

        public IEnumerable<WPos> ClipPoly
        {
            get => ConvertPoints(_clipPoly);
            set => _clipPoly = ConvertPoints(value).ToList();
        }

        public List<(WPos, WPos, WPos)> ClipAndTriangulate(IEnumerable<WPos> pts)
        {
            _clipper.Clear();
            _clipper.AddPath(_clipPoly, PolyType.ptClip, true);
            _clipper.AddPath(ConvertPoints(pts).ToList(), PolyType.ptSubject, true);

            PolyTree solution = new();
            _clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd);

            List<(WPos, WPos, WPos)> triangulation = new();
            Triangulate(triangulation, solution);
            return triangulation;
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
            foreach (var p in ConvertPoints(pts))
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

        private static IEnumerable<IntPoint> ConvertPoints(IEnumerable<WPos> pts) => pts.Select(v => new IntPoint(v.X * _scale, v.Z * _scale));
        private static IEnumerable<WPos> ConvertPoints(IEnumerable<IntPoint> pts) => pts.Select(v => new WPos(v.X * _invScale, v.Y * _invScale));
    }
}
