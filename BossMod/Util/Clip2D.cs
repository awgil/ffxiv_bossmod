using ClipperLib;
using EarcutNet;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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

        public void SetClipPoly(IEnumerable<Vector2> pts)
        {
            _clipPoly = BuildPointList(pts);
        }

        public List<Vector2> ClipAndTriangulate(IEnumerable<Vector2> pts)
        {
            _clipper.Clear();
            _clipper.AddPath(_clipPoly, PolyType.ptClip, true);
            _clipper.AddPath(BuildPointList(pts), PolyType.ptSubject, true);

            PolyTree solution = new();
            _clipper.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd);

            List<Vector2> triangulation = new();
            Triangulate(triangulation, solution);
            return triangulation;
        }

        private static void Triangulate(List<Vector2> result, PolyNode node)
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
                foreach (int v in tess)
                    result.Add(new((float)pts[2 * v], (float)pts[2 * v + 1]));
            }
        }

        private static void AddClipperPoly(List<double> output, List<IntPoint> pts)
        {
            foreach (var p in pts)
            {
                output.Add(p.X * _invScale);
                output.Add(p.Y * _invScale);
            }
        }

        private static List<IntPoint> BuildPointList(IEnumerable<Vector2> pts)
        {
            List<IntPoint> ipts = new();
            foreach (var p in pts)
                ipts.Add(new(p.X * _scale, p.Y * _scale));
            return ipts;
        }
    }
}
