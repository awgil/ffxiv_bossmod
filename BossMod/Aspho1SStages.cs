using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    public class Aspho1SStages
    {
        public struct Stage
        {
            public string Name;
            public float TimeBeforeCastStart;
            public float TimeBeforeCastEnd;
            public float[] PostCastEvents;
            public int SameNameIndex;

            public Stage(string name, float timeBeforeCastStart, float timeBeforeCastEnd, float[]? postCastEvents = null)
            {
                Name = name;
                TimeBeforeCastStart = timeBeforeCastStart;
                TimeBeforeCastEnd = timeBeforeCastEnd;
                PostCastEvents = postCastEvents ?? new float[]{};
                SameNameIndex = 0;
            }
        }
        private static Stage[] _stages;

        static Aspho1SStages()
        {
            _stages = new Stage[]{
                new Stage("Tankbuster",    8,  5),
                new Stage("Shackles",      6,  3, new float[] { 19 }),
                new Stage("Raidwide",      4,  5),
                new Stage("Flails",       14, 12, new float[] { 4 }),
                new Stage("Knockback",     9,  5, new float[] { 5 }),
                new Stage("Flails",        8, 12, new float[] { 4 }),
                new Stage("Raidwide",      9,  5),
                new Stage("Intemperance", 11,  2),
                new Stage("CubeExplode",   6, 10, new float[] { 1, 12, 23 }),
                new Stage("Raidwide",      2,  5),
                new Stage("Raidwide",      5,  5),
                // intemperance1 ends...
                new Stage("Knockback",    11,  5, new float[] { 5 }),
                new Stage("Cells",        13,  7),
                new Stage("Aetherflail",   8, 12, new float[] { 4 }),
                new Stage("Knockback",    11,  5, new float[] { 5 }),
                new Stage("Aetherflail",   7, 12, new float[] { 4 }),
                new Stage("ShackleOfTime", 8,  4, new float[] { 15 }),
                new Stage("Tankbuster",    5,  5),
                new Stage("SlamShut",      6,  6),
                // cells1 ends...
                new Stage("FourShackles", 13,  3, new float[] { 10, 15, 20, 25 }),
                new Stage("Raidwide",     30,  5),
                new Stage("Intemperance", 11,  2),
                new Stage("CubeExplode",   6, 10, new float[] { 1, 12, 23 }),
                new Stage("Flails",        4, 11, new float[] { 4 }),
                // intemperance2 ends...
                new Stage("Raidwide",     11,  5),
                new Stage("Cells",        11,  7),
                // *possible seq A*
                new Stage("Shackles",      9,  3, new float[] { 19 }),
                new Stage("Aetherchain",   6,  5),
                new Stage("Aetherchain",   3,  5),
                new Stage("Raidwide",      7,  5),
                // *possible seq B*
                new Stage("ShackleOfTime", 6,  4, new float[] { 15 }),
                new Stage("Knockback",     2,  5, new float[] { 5 }),
                new Stage("Raidwide",     11,  5),
                // *A->B or B->A*; after this paths merge back
                new Stage("Aetherflail",   9, 12),
                new Stage("Aetherflail",   6, 12),
                new Stage("Aetherflail",   6, 12),
                new Stage("Raidwide",     13,  5),
                new Stage("???",           0,  0)
            };

            Dictionary<string, int> repeats = new();
            for (int i = 0; i < _stages.Length; ++i)
            {
                if (!repeats.ContainsKey(_stages[i].Name))
                    repeats[_stages[i].Name] = 0;
                _stages[i].SameNameIndex = ++repeats[_stages[i].Name];
            }
        }

        class PendingResolve
        {
            public int Stage;
            public int RIndex;
            public DateTime ResolveStart;
            public string Hint = "";
        }

        public string Hint = "";
        private bool _running = false;
        private int _stageIndex = 0;
        private bool _stageCastStarted = false;
        private DateTime _lastEvent = DateTime.Now;
        private PendingResolve?[] _pendingResolves = { null, null };

        public Stage NextStage => _stages[_stageIndex];

        public void Draw()
        {
            var secSinceLastEvent = (DateTime.Now - _lastEvent).TotalSeconds;
            var secUntilEnd = !_running ? NextStage.TimeBeforeCastStart + NextStage.TimeBeforeCastEnd : _stageCastStarted
                ? Math.Max(0, NextStage.TimeBeforeCastEnd - secSinceLastEvent)
                : Math.Max(0, NextStage.TimeBeforeCastStart - secSinceLastEvent) + NextStage.TimeBeforeCastEnd;

            for (int i = 0; i < _pendingResolves.Length; ++i)
            {
                var r = _pendingResolves[i];
                if (r == null)
                {
                    ImGui.Text("");
                    continue;
                }

                var timeSinceEnd = (DateTime.Now - r.ResolveStart).TotalSeconds;
                var s = _stages[r.Stage];
                while (r.RIndex < s.PostCastEvents.Length && timeSinceEnd >= s.PostCastEvents[r.RIndex])
                    ++r.RIndex;

                if (r.RIndex == s.PostCastEvents.Length)
                {
                    _pendingResolves[i] = null;
                    ImGui.Text("");
                }
                else
                {
                    string name = _stages[r.Stage].Name;
                    if (r.Hint.Length != 0)
                        name += $" ({r.Hint})";
                    ImGui.Text($"{name} resolve in {(s.PostCastEvents[r.RIndex] - timeSinceEnd):f1}s");
                }
            }

            string next = $"{NextStage.Name}{NextStage.SameNameIndex}";
            if (Hint.Length != 0)
                next += $" ({Hint})";
            ImGui.Text($"Next: {next} in {secUntilEnd:f1}s");
            ImGui.Text($"Then: {BuildNextEventsHint(_stageIndex + 1, 5)}");
        }

        public void DrawDebugButtons()
        {
            if (ImGui.Button("Reset"))
                Reset();
            ImGui.SameLine();
            if (ImGui.Button("Start"))
                Start();
            ImGui.SameLine();
            if (ImGui.Button("Undo"))
                UndoTransition();
            ImGui.SameLine();
            if (ImGui.Button("Cast-start"))
                NotifyCastStart();
            ImGui.SameLine();
            if (ImGui.Button("Cast-end"))
                NotifyCastEnd();
        }

        public void Reset()
        {
            _running = false;
            _stageIndex = 0;
            _stageCastStarted = false;
            Hint = "";
        }

        public void Start()
        {
            if (_running)
                return;
            _running = true;
            _lastEvent = DateTime.Now;
        }

        public void UndoTransition()
        {
            if (_stageIndex <= 0)
                return;
            --_stageIndex;
            _stageCastStarted = false;
            _lastEvent = DateTime.Now;
            Hint = "";
        }

        public void NotifyCastStart()
        {
            if (!_running || _stageCastStarted)
                return;
            _stageCastStarted = true;
            _lastEvent = DateTime.Now;
        }

        public void NotifyCastEnd()
        {
            if (!_running || _stageIndex + 1 >= _stages.Length)
                return;

            if (_stages[_stageIndex].PostCastEvents.Length > 0)
            {
                for (int i = 0; i < _pendingResolves.Length; ++i)
                {
                    if (_pendingResolves[i] != null)
                        continue;
                    _pendingResolves[i] = new PendingResolve { Stage = _stageIndex, ResolveStart = DateTime.Now, Hint = Hint };
                    break;
                }
            }

            ++_stageIndex;
            _stageCastStarted = false;
            _lastEvent = DateTime.Now;
            Hint = "";
        }

        private static string BuildNextEventsHint(int startEvent, int maxLen)
        {
            var res = new StringBuilder();
            int length = 0;
            while (startEvent < _stages.Length && length < maxLen)
            {
                if (length > 0)
                    res.Append(" ---> ");
                var stage = _stages[startEvent++];
                res.Append($"{stage.Name}{stage.SameNameIndex}");
                length++;
            }
            return res.ToString();
        }
    }
}
