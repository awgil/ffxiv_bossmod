using System.Text.Json;
using static BossMod.Shadowcast;

namespace BossMod.Tests;

[TestClass]
public sealed class ShadowcastTests
{
    [DataRow("shadow.jsonc")]
    [DataRow("min1.jsonc")]
    [DataRow("min2.jsonc")]
    [TestMethod]
    public void TestMethod1(string json)
    {
        using var s = File.OpenRead(json);
        var contents = JsonSerializer.Deserialize<List<LineSegment>>(s, new JsonSerializerOptions() { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })!;

        List<LineSegment> output = [];

        for (var i = 0; i < contents.Count; i++)
        {
            output.Clear();
            try
            {
                Uncross(contents.Take(i), output);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine($"Failed at {i} iterations");
                Console.WriteLine($"Input: {JsonSerializer.Serialize(contents.Take(i))}");
                Console.WriteLine($"Contents: {JsonSerializer.Serialize(output)}");
                throw;
            }
        }
    }
}
