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

        var S = new Queue<T>(Nodes.Where(n => Edges.All(e => !e.Item2.Equals(n))));
        var E = new HashSet<(T, T)>(Edges);

        while (S.TryDequeue(out var n))
        {
            sorted.Add(n);

            foreach (var e in E.Where(e => e.Item1.Equals(n)).ToList())
            {
                var m = e.Item2;
                E.Remove(e);

                if (E.All(me => !me.Item2.Equals(m)))
                    S.Enqueue(m);
            }
        }

        return E.Count == 0;
    }
}
