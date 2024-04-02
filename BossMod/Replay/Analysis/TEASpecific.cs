using BossMod.Shadowbringers.Ultimate.TEA;

namespace BossMod.ReplayAnalysis;

class TEASpecific
{
    struct HandBaitData
    {
        public Replay Replay;
        public DateTime Timestamp;
        public bool IsPrayer;
        public float Distance;
    }

    struct ResolveData
    {
        public Replay Replay;
        public DateTime Timestamp;
        public float Distance;
        public int Damage;
    }

    private List<HandBaitData> _handBaits = new();
    private List<ResolveData> _handResolvesParting = new();
    private List<ResolveData> _handResolvesPrayer = new();
    private List<ResolveData> _suretyResolvesClose = new();
    private List<ResolveData> _suretyResolvesFar = new();
    private UIPlot _plotHandBaits = new();
    private UIPlot _plotHandResolvesParting = new();
    private UIPlot _plotHandResolvesPrayer = new();
    private UIPlot _plotSuretyResolvesClose = new();
    private UIPlot _plotSuretyResolvesFar = new();

    public TEASpecific(List<Replay> replays, uint oid)
    {
        _plotHandBaits.DataMin = new(0, 0);
        _plotHandBaits.DataMax = new(100, 3);
        _plotHandBaits.TickAdvance = new(5, 1);
        _plotHandResolvesParting.DataMin = new(0, 0);
        _plotHandResolvesParting.DataMax = new(100, 200000);
        _plotHandResolvesParting.TickAdvance = new(5, 10000);
        _plotHandResolvesPrayer.DataMin = new(0, 0);
        _plotHandResolvesPrayer.DataMax = new(100, 200000);
        _plotHandResolvesPrayer.TickAdvance = new(5, 10000);
        _plotSuretyResolvesClose.DataMin = new(0, 0);
        _plotSuretyResolvesClose.DataMax = new(100, 200000);
        _plotSuretyResolvesClose.TickAdvance = new(5, 10000);
        _plotSuretyResolvesFar.DataMin = new(0, 0);
        _plotSuretyResolvesFar.DataMax = new(100, 200000);
        _plotSuretyResolvesFar.TickAdvance = new(5, 10000);
        foreach (var replay in replays)
        {
            foreach (var op in replay.Ops.OfType<ActorState.OpModelState>().Where(op => op.Value.ModelState is 19 or 20))
            {
                var hand = replay.FindParticipant(op.InstanceID, op.Timestamp);
                var boss = replay.Participants.First(p => p.OID == oid && p.ExistsInWorldAt(op.Timestamp));
                if (hand != null && boss != null)
                {
                    var handPos = new WPos(hand.PosRotAt(op.Timestamp).XZ());
                    var bossPos = new WPos(boss.PosRotAt(op.Timestamp).XZ());
                    _handBaits.Add(new() { Replay = replay, Timestamp = op.Timestamp, IsPrayer = op.Value.ModelState == 19, Distance = (handPos - bossPos).Length() });
                }
            }

            var aidParting = ActionID.MakeSpell(AID.HandOfParting);
            var aidPrayer = ActionID.MakeSpell(AID.HandOfPrayer);
            foreach (var action in replay.Actions.Where(a => a.ID == aidParting || a.ID == aidPrayer))
            {
                var hand = replay.FindParticipant(action.Source.InstanceID, action.Timestamp);
                var boss = replay.Participants.First(p => p.OID == oid && p.ExistsInWorldAt(action.Timestamp));
                if (hand != null && boss != null)
                {
                    var handPos = new WPos(hand.PosRotAt(action.Timestamp).XZ());
                    var bossPos = new WPos(boss.PosRotAt(action.Timestamp).XZ());
                    int damageSum = 0;
                    int damageCount = 0;
                    foreach (var t in action.Targets)
                    {
                        var damage = ReplayUtils.ActionDamage(t);
                        if (damage > 0)
                        {
                            damageSum += damage;
                            ++damageCount;
                        }
                    }
                    if (damageCount > 0)
                    {
                        var resolves = action.ID == aidPrayer ? _handResolvesPrayer : _handResolvesParting;
                        resolves.Add(new() { Replay = replay, Timestamp = action.Timestamp, Distance = (handPos - bossPos).Length(), Damage = damageSum / damageCount });
                    }
                }
            }

            var aidSurety = ActionID.MakeSpell(AID.PlaintOfSurety);
            foreach (var action in replay.Actions.Where(a => a.ID == aidSurety && a.Targets.Count == 2))
            {
                var t1 = action.Targets[0].Target;
                var t2 = action.Targets[1].Target;
                var adjTS = action.Timestamp.AddSeconds(-2);
                var s = replay.Statuses.Find(s => s.Target == t1 && s.Time.Contains(adjTS) && (SID)s.ID is SID.HouseArrest or SID.RestrainingOrder);
                if (s == null)
                    continue;

                var list = (SID)s.ID == SID.HouseArrest ? _suretyResolvesClose : _suretyResolvesFar;
                var p1 = new WPos(t1.PosRotAt(action.Timestamp).XZ());
                var p2 = new WPos(t2.PosRotAt(action.Timestamp).XZ());
                list.Add(new() { Replay = replay, Timestamp = action.Timestamp, Distance = (p1 - p2).Length(), Damage = ReplayUtils.ActionDamage(action.Targets[0]) });
            }
        }
    }

    public void Draw(UITree tree)
    {
        foreach (var _ in tree.Node("Hand of Prayer/Parting: bait range"))
        {
            _plotHandBaits.Begin();
            foreach (var i in _handBaits)
                _plotHandBaits.Point(new Vector2(i.Distance, i.IsPrayer ? 2 : 1), i.IsPrayer ? 0xff00ffff : 0xff808080, () => $"{(i.IsPrayer ? "prayer" : "parting")} {i.Replay.Path} @ {i.Timestamp:O}");
            _plotHandBaits.End();
        }
        foreach (var _ in tree.Node("Hand of Parting: resolves damage"))
        {
            _plotHandResolvesParting.Begin();
            foreach (var i in _handResolvesParting)
                _plotHandResolvesParting.Point(new Vector2(i.Distance, i.Damage), 0xff00ffff, () => $"{i.Replay.Path} @ {i.Timestamp:O}");
            _plotHandResolvesParting.End();
        }
        foreach (var _ in tree.Node("Hand of Prayer: resolves damage"))
        {
            _plotHandResolvesPrayer.Begin();
            foreach (var i in _handResolvesPrayer)
                _plotHandResolvesPrayer.Point(new Vector2(i.Distance, i.Damage), 0xff00ffff, () => $"{i.Replay.Path} @ {i.Timestamp:O}");
            _plotHandResolvesPrayer.End();
        }
        foreach (var _ in tree.Node("Plaint of Surety: damage on close"))
        {
            _plotSuretyResolvesClose.Begin();
            foreach (var i in _suretyResolvesClose)
                _plotSuretyResolvesClose.Point(new Vector2(i.Distance, i.Damage), 0xff00ffff, () => $"{i.Replay.Path} @ {i.Timestamp:O}");
            _plotSuretyResolvesClose.End();
        }
        foreach (var _ in tree.Node("Plaint of Surety: damage on far"))
        {
            _plotSuretyResolvesFar.Begin();
            foreach (var i in _suretyResolvesFar)
                _plotSuretyResolvesFar.Point(new Vector2(i.Distance, i.Damage), 0xff00ffff, () => $"{i.Replay.Path} @ {i.Timestamp:O}");
            _plotSuretyResolvesFar.End();
        }
    }
}
