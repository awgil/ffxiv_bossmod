namespace BossMod;

public class MiniProfiler
{
    public List<(string name, DateTime time)> Points = [("start", DateTime.Now)];

    public TimeSpan Measure(string name)
    {
        var now = DateTime.Now;
        Points.Add((name, now));
        return now - Points[^2].time;
    }

    public bool Finish(string tag, float minMS)
    {
        Measure("finish");
        var total = (Points[^1].time - Points[0].time).TotalMilliseconds;
        if (total < minMS)
            return false;

        Service.Log($"profile {tag}: total={total:f3}ms, measures=[{string.Join(", ", Points.Pairwise().Select(pair => $"{pair.Item2.name}={(pair.Item2.time - pair.Item1.time).TotalMilliseconds:f3}ms"))}]");
        return true;
    }
}
