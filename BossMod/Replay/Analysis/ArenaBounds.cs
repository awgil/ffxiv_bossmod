namespace BossMod.ReplayAnalysis;

class ArenaBounds
{
    private List<(Replay, Replay.Participant, DateTime, Vector3, uint)> _points = new();
    private UIPlot _plot = new();

    public ArenaBounds(List<Replay> replays, uint oid)
    {
        _plot.DataMin = new(float.MaxValue, float.MaxValue);
        _plot.DataMax = new(float.MinValue, float.MinValue);
        _plot.MainAreaSize = new(500, 500);
        _plot.TickAdvance = new(10, 10);

        foreach (var replay in replays)
        {
            foreach (var enc in replay.Encounters.Where(enc => enc.OID == oid))
            {
                foreach (var ps in enc.ParticipantsByOID.Values)
                {
                    foreach (var p in ps)
                    {
                        int iStart = p.PosRotHistory.UpperBound(enc.Time.Start);
                        if (iStart > 0)
                            --iStart;
                        int iEnd = p.PosRotHistory.UpperBound(enc.Time.End);
                        int iNextDead = p.DeadHistory.UpperBound(enc.Time.Start);
                        for (int i = iStart; i < iEnd; ++i)
                        {
                            var t = p.PosRotHistory.Keys[i];
                            var pos = p.PosRotHistory.Values[i].XYZ();
                            if (iNextDead < p.DeadHistory.Count && p.DeadHistory.Keys[iNextDead] <= t)
                                ++iNextDead;
                            bool dead = iNextDead > 0 ? p.DeadHistory.Values[iNextDead - 1] : false;
                            uint color = dead ? 0xff404040 : p.Type is ActorType.Enemy ? 0xff00ffff : 0xff808080;
                            _points.Add((replay, p, t, pos, color));
                            _plot.DataMin.X = Math.Min(_plot.DataMin.X, pos.X);
                            _plot.DataMin.Y = Math.Min(_plot.DataMin.Y, pos.Z);
                            _plot.DataMax.X = Math.Max(_plot.DataMax.X, pos.X);
                            _plot.DataMax.Y = Math.Max(_plot.DataMax.Y, pos.Z);
                        }
                    }
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        _plot.Begin();
        foreach (var (replay, participant, time, pos, color) in _points)
            _plot.Point(new(pos.X, pos.Z), color, () => $"{ReplayUtils.ParticipantString(participant, time)} {replay.Path} @ {time:O}");
        _plot.End();
    }
}
