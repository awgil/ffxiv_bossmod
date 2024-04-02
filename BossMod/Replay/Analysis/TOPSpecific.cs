using BossMod.Endwalker.Ultimate.TOP;

namespace BossMod.ReplayAnalysis;

class TOPSpecific
{
    struct FlamethrowerData
    {
        public Replay Replay;
        public DateTime Timestamp;
        public Angle Difference;
    }

    private List<FlamethrowerData> _flamethrowers = new();
    private UIPlot _plotFlamethrowers = new();

    public TOPSpecific(List<Replay> replays, uint oid)
    {
        _plotFlamethrowers.DataMin = new(-180, 0);
        _plotFlamethrowers.DataMax = new(180, 1);
        _plotFlamethrowers.TickAdvance = new(5, 1);
        foreach (var replay in replays)
        {
            var aidFlamethrower = ActionID.MakeSpell(AID.FlameThrowerFirst);
            foreach (var p in replay.Participants)
            {
                foreach (var cast in p.Casts.Where(c => c.ID == aidFlamethrower))
                {
                    var boss = replay.Participants.First(p => (OID)p.OID == OID.Boss && p.ExistsInWorldAt(cast.Time.Start));
                    if (boss != null)
                    {
                        _flamethrowers.Add(new() { Replay = replay, Timestamp = cast.Time.Start, Difference = (cast.Rotation - boss.PosRotAt(cast.Time.Start).W.Radians()).Normalized() });
                    }
                }
            }
        }
    }

    public void Draw(UITree tree)
    {
        foreach (var _ in tree.Node("Flamethrower rotation offsets"))
        {
            _plotFlamethrowers.Begin();
            foreach (var i in _flamethrowers)
                _plotFlamethrowers.Point(new(i.Difference.Deg, 0.5f), 0xff00ffff, () => $"{i.Replay.Path} @ {i.Timestamp:O}");
            _plotFlamethrowers.End();
        }
    }
}
