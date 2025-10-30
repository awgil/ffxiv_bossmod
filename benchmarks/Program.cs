using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using BossMod;
using Microsoft.VSDiagnostics;
using SharpDX;

namespace vbenchmark;

[CPUUsageDiagnoser]
public class RasterizeTest
{
    private readonly WPos[] coords;

    private readonly List<RelTriangle> _tris;
    private WPos arenaCenter = new(-300, -300);

    public RasterizeTest()
    {
        var rand = new Random();

        coords = new WPos[120 * 120];
        for (var i = 0; i < coords.Length; i++)
        {
            var x0 = rand.NextFloat(-30, 30);
            var z0 = rand.NextFloat(-30, 30);
            coords[i] = arenaCenter + new WDir(x0, z0);
        }

        _tris = BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D50Ogbunabali.Rocks.RockShape.Poly.Triangulate();
    }

    [Benchmark]
    public void RasterizeWind()
    {
        var hit = 0;
        foreach (var c in coords)
        {
            var dir = c - arenaCenter;
            if (BossMod.Dawntrail.DeepDungeon.PilgrimsTraverse.D50Ogbunabali.Rocks.RockShape.Poly.Contains(dir))
                hit++;
        }
    }

    [Benchmark]
    public void RasterizeTri()
    {
        var hit = 0;
        foreach (var c in coords)
        {
            foreach (var t in _tris)
                if (c.InTri(arenaCenter + t.A, arenaCenter + t.B, arenaCenter + t.C))
                    hit++;
        }
    }
}

public class Program
{
    public static void Main()
    {
        var config = DefaultConfig.Instance.AddJob(Job.MediumRun.WithLaunchCount(1).WithToolchain(InProcessNoEmitToolchain.Instance));
        BenchmarkRunner.Run<RasterizeTest>(config);
        //BenchmarkRunner.Run<RasterizeTest>();
    }
}
