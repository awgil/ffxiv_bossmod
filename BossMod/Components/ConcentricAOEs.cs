namespace BossMod.Components;

// generic 'concentric aoes' component - a sequence of aoes (typically cone then donuts) with same origin and increasing size
public class ConcentricAOEs(BossModule module, AOEShape[] shapes) : GenericAOEs(module)
{
    public struct Sequence
    {
        public WPos Origin;
        public Angle Rotation;
        public DateTime NextActivation;
        public int NumCastsDone;
    }

    public AOEShape[] Shapes = shapes;
    public List<Sequence> Sequences = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Sequences.Where(s => s.NumCastsDone < Shapes.Length).Select(s => new AOEInstance(Shapes[s.NumCastsDone], s.Origin, s.Rotation, s.NextActivation));

    public void AddSequence(WPos origin, DateTime activation = default, Angle rotation = default) => Sequences.Add(new() { Origin = origin, Rotation = rotation, NextActivation = activation });

    // return false if sequence was not found
    public bool AdvanceSequence(int order, WPos origin, DateTime activation = default, Angle rotation = default)
    {
        if (order < 0)
            return true; // allow passing negative as a no-op

        ++NumCasts;

        var index = Sequences.FindIndex(s => s.NumCastsDone == order && s.Origin.AlmostEqual(origin, 1) && s.Rotation.AlmostEqual(rotation, 0.05f));
        if (index < 0)
            return false;

        ref var s = ref Sequences.AsSpan()[index];
        ++s.NumCastsDone;
        s.NextActivation = activation;
        return true;
    }
}
