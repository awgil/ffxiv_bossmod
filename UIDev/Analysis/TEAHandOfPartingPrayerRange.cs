using BossMod;
using BossMod.Shadowbringers.Ultimate.TEA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UIDev.Analysis
{
    class TEAHandOfPartingPrayerRange
    {
        struct BaitData
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

        private List<BaitData> _baits = new();
        private List<ResolveData> _resolvesParting = new();
        private List<ResolveData> _resolvesPrayer = new();
        private UIPlot _plotBaits = new();
        private UIPlot _plotResolvesParting = new();
        private UIPlot _plotResolvesPrayer = new();

        public TEAHandOfPartingPrayerRange(List<Replay> replays, uint oid)
        {
            _plotBaits.DataMin = new(0, 0);
            _plotBaits.DataMax = new(100, 3);
            _plotBaits.TickAdvance = new(5, 1);
            _plotResolvesParting.DataMin = new(0, 0);
            _plotResolvesParting.DataMax = new(100, 200000);
            _plotResolvesParting.TickAdvance = new(5, 10000);
            _plotResolvesPrayer.DataMin = new(0, 0);
            _plotResolvesPrayer.DataMax = new(100, 200000);
            _plotResolvesPrayer.TickAdvance = new(5, 10000);
            foreach (var replay in replays)
            {
                foreach (var op in replay.Ops.OfType<ActorState.OpModelState>().Where(op => op.Value.ModelState is 19 or 20))
                {
                    var hand = replay.Participants.Find(p => p.InstanceID == op.InstanceID);
                    var boss = replay.Participants.Find(p => p.OID == oid && p.Existence.Contains(op.Timestamp));
                    if (hand != null && boss != null)
                    {
                        var handPos = new WPos(hand.PosRotAt(op.Timestamp).XZ());
                        var bossPos = new WPos(boss.PosRotAt(op.Timestamp).XZ());
                        _baits.Add(new() { Replay = replay, Timestamp = op.Timestamp, IsPrayer = op.Value.ModelState == 19, Distance = (handPos - bossPos).Length() });
                    }
                }

                var aidParting = ActionID.MakeSpell(AID.HandOfParting);
                var aidPrayer = ActionID.MakeSpell(AID.HandOfPrayer);
                foreach (var action in replay.Actions.Where(a => a.ID == aidParting || a.ID == aidPrayer))
                {
                    var hand = replay.Participants.Find(p => p.InstanceID == action.Source?.InstanceID);
                    var boss = replay.Participants.Find(p => p.OID == oid && p.Existence.Contains(action.Timestamp));
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
                            var resolves = action.ID == aidPrayer ? _resolvesPrayer : _resolvesParting;
                            resolves.Add(new() { Replay = replay, Timestamp = action.Timestamp, Distance = (handPos - bossPos).Length(), Damage = damageSum / damageCount });
                        }
                    }
                }
            }
        }

        public void Draw(UITree tree)
        {
            foreach (var _ in tree.Node("Baits"))
            {
                _plotBaits.Begin();
                foreach (var i in _baits)
                    _plotBaits.Point(new Vector2(i.Distance, i.IsPrayer ? 2 : 1), i.IsPrayer ? 0xff00ffff : 0xff808080, () => $"{(i.IsPrayer ? "prayer" : "parting")} {i.Replay.Path} @ {i.Timestamp:O}");
                _plotBaits.End();
            }
            foreach (var _ in tree.Node("Parting resolves"))
            {
                _plotResolvesParting.Begin();
                foreach (var i in _resolvesParting)
                    _plotResolvesParting.Point(new Vector2(i.Distance, i.Damage), 0xff00ffff, () => $"{i.Replay.Path} @ {i.Timestamp:O}");
                _plotResolvesParting.End();
            }
            foreach (var _ in tree.Node("Prayer resolves"))
            {
                _plotResolvesPrayer.Begin();
                foreach (var i in _resolvesPrayer)
                    _plotResolvesPrayer.Point(new Vector2(i.Distance, i.Damage), 0xff00ffff, () => $"{i.Replay.Path} @ {i.Timestamp:O}");
                _plotResolvesPrayer.End();
            }
        }
    }
}
