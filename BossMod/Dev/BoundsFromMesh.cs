namespace BossMod.Dev;

/*
List<List<WDir>> polys = [];

        using (var contents = Utils.LoadResource("BossMod.Global.Crucible.Board2.LoosefroxInkyjots.dat"))
        {
            string? line;

            while ((line = contents.ReadLine()) != null)
            {
                List<WDir> poly = [];
                foreach (var pos in line.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    var coords = pos.Split(',');
                    if (coords.Length != 2)
                        throw new InvalidOperationException("corrupt data");

                    if (!float.TryParse(coords[0], out var x))
                        throw new InvalidOperationException("corrupt data");

                    if (!float.TryParse(coords[1], out var z))
                        throw new InvalidOperationException("corrupt data");

poly.Add(new WPos(x, z) - new WPos(520, -420));
                }
                polys.Add(poly);
            }
        }

        var p = polys[0];
polys.RemoveAt(0);

var clipper = new PolygonClipper();
var arena = clipper.UnionAll(new(p), [.. polys.Select(p => new PolygonClipper.Operand(p))]);

Service.Log(JsonConvert.SerializeObject(arena));
*/
