using System.Numerics;

namespace BossMod.Tests;

[TestClass]
public sealed class VectorizeTest
{
    private static void EnsureEquivalent<T>(
        Func<float, float> input0,
        Func<float, float> input1,
        Func<float, float, float, float, T> runScalar,
        Func<Vector<float>, Vector<float>, Vector<float>, Vector<float>, Vector<T>> runVector,
        Func<T, bool> isUseful,
        int iterations = 2048
    ) where T : struct
    {
        var is0 = new float[iterations];
        var is1 = new float[is0.Length];
        var is2 = new float[is0.Length];
        var is3 = new float[is0.Length];

        var random = new Random();

        for (var i = 0; i < is0.Length; i++)
        {
            is0[i] = input0(random.NextSingle());
            is1[i] = input0(random.NextSingle());
            is2[i] = input1(random.NextSingle());
            is3[i] = input1(random.NextSingle());
        }

        var results0 = new T[is0.Length];
        var results1 = new T[is0.Length];
        for (var i = 0; i < is0.Length; i += 8)
        {
            for (var j = 0; j < 8; j++)
                results0[i + j] = runScalar(is0[i + j], is1[i + j], is2[i + j], is3[i + j]);

            runVector(
                Vector.Create(is0.AsSpan()[i..]),
                Vector.Create(is1.AsSpan()[i..]),
                Vector.Create(is2.AsSpan()[i..]),
                Vector.Create(is3.AsSpan()[i..])
            ).CopyTo(results1, i);
        }

        var anyUseful = false;
        for (var i = 0; i < is0.Length; i++)
        {
            var res0 = results0[i];
            var res1 = results1[i];

            anyUseful |= isUseful(res0);

            Assert.AreEqual(res0, res1, $"Result mismatch for parameters ({is0[i]},{is1[i]},{is2[i]},{is3[i]})");
        }

        Assert.IsTrue(anyUseful, "Got no useful test results");
    }

    [TestMethod]
    public void TestCircle()
    {
        EnsureEquivalent(
            input0: i => i * 10 - 5,
            input1: i => i * 10,
            runScalar: (a, b, c, d) => Intersect.RayCircle(new(a, b), new(c, d), 5),
            runVector: (a, b, c, d) => Intersect.RayCircleV(new(a, b), new(c, d), 5),
            isUseful: x => x < 1000
        );
    }

    [TestMethod]
    public void TestBox()
    {
        EnsureEquivalent(
            input0: i => i * 10 - 5,
            input1: i => i * 10,
            runScalar: (a, b, c, d) => Intersect.RayAABB(new(a, b), new(c, d), 5, 5),
            runVector: (a, b, c, d) => Intersect.RayAABBV(new(a, b), new(c, d), 5, 5),
            isUseful: x => x < 1000
        );
    }

    [TestMethod]
    public void TestZero()
    {
        EnsureEquivalent(
            input0: i => MathF.Abs(i) < 0.05f ? 0 : i * 10 - 5,
            input1: i => 0,
            runScalar: (a, _, _, _) => MathF.Sign(a),
            runVector: (a, _, _, _) => a.Sign(),
            isUseful: _ => true
        );
    }
}
