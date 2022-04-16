using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UIDev.Analysis
{
    class ArenaBounds
    {
        class EncounterData
        {
            public List<(Replay, Replay.Participant, DateTime, Vector3, uint)> Points = new();
            public Plot Plot = new();

            public EncounterData()
            {
                Plot.DataMin = new(float.MaxValue, float.MaxValue);
                Plot.DataMax = new(float.MinValue, float.MinValue);
                Plot.MainAreaSize = new(500, 500);
                Plot.TickAdvance = new(10, 10);
            }
        }

        private Tree _tree;
        private Dictionary<uint, EncounterData> _data = new(); // [encounter-oid]

        public ArenaBounds(List<Replay> replays, Tree tree)
        {
            _tree = tree;
            foreach (var replay in replays)
            {
                foreach (var enc in replay.Encounters)
                {
                    var data = _data.GetOrAdd(enc.OID);
                    foreach (var ps in enc.Participants.Values)
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
                                data.Points.Add((replay, p, t, pos, color));
                                data.Plot.DataMin.X = Math.Min(data.Plot.DataMin.X, pos.X);
                                data.Plot.DataMin.Y = Math.Min(data.Plot.DataMin.Y, pos.Z);
                                data.Plot.DataMax.X = Math.Max(data.Plot.DataMax.X, pos.X);
                                data.Plot.DataMax.Y = Math.Max(data.Plot.DataMax.Y, pos.Z);
                            }
                        }
                    }
                }
            }
        }

        public void Draw()
        {
            foreach (var (encOID, perEnc) in _tree.Nodes(_data, kv => ($"{kv.Key:X} ({ModuleRegistry.TypeForOID(kv.Key)?.Name})", false)))
            {
                var moduleType = ModuleRegistry.TypeForOID(encOID);
                var oidType = moduleType?.Module.GetType($"{moduleType.Namespace}.OID");
                perEnc.Plot.Begin();
                foreach (var (replay, participant, time, pos, color) in perEnc.Points)
                    perEnc.Plot.Point(new(pos.X, pos.Z), color, () => $"{ReplayUtils.ParticipantString(participant)} {replay.Path} @ {time:O}");
                perEnc.Plot.End();
            }
        }
    }
}
