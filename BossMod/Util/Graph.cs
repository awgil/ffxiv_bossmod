namespace BossMod;

public class Graph<T> where T : IEquatable<T>
{
    public readonly HashSet<T> Nodes = [];
    public readonly HashSet<(T, T)> Edges = [];

    public void Clear()
    {
        Nodes.Clear();
        Edges.Clear();
    }

    public void Add(T from, T to)
    {
        Nodes.Add(from);
        Nodes.Add(to);
        Edges.Add((from, to));
    }

    public bool Topo(out List<T> sorted)
    {
        sorted = [];

        var queue = new Queue<T>(Nodes.Where(n => Edges.All(e => !e.Item2.Equals(n))));
        var edges = new HashSet<(T, T)>(Edges);

        while (queue.TryDequeue(out var n))
        {
            sorted.Add(n);

            foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
            {
                var m = e.Item2;
                edges.Remove(e);

                if (edges.All(me => !me.Item2.Equals(m)))
                    queue.Enqueue(m);
            }
        }

        return edges.Count == 0;
    }
}
