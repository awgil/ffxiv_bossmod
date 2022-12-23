using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // this class represents parts of a world state that are interesting to boss modules
    // it does not know anything about dalamud, so it can be used for UI test - there is a separate utility that updates it based on game state every frame
    // world state is supposed to be modified using "operations" - this provides opportunity to listen and react to state changes
    public class WorldState
    {
        // state access
        public DateTime CurrentTime { get; private set; }
        public ushort CurrentZone { get; private set; }
        public WaymarkState Waymarks { get; init; } = new();
        public ActorState Actors { get; init; } = new();
        public PartyState Party { get; init; }
        public PendingEffects PendingEffects { get; init; } = new();

        public WorldState()
        {
            Party = new(Actors);
        }

        // state modification
        public event EventHandler<Operation>? Modified;
        public abstract class Operation
        {
            public DateTime Timestamp { get; private set; } // TODO: this should be removed...

            internal void Execute(WorldState ws)
            {
                Exec(ws);
                Timestamp = ws.CurrentTime;
            }

            protected abstract void Exec(WorldState ws);
            public abstract string Str(WorldState? ws);
            public override string ToString() => Str(null);

            protected static string StrVec3(Vector3 v) => $"{v.X:f3}/{v.Y:f3}/{v.Z:f3}";
        }

        public void Execute(Operation op)
        {
            op.Execute(this);
            Modified?.Invoke(this, op);
        }

        // generate a set of operations that would turn default-constructed state into current state
        public IEnumerable<Operation> CompareToInitial()
        {
            if (CurrentTime != default)
                yield return new OpFrameStart() { NewTimestamp = CurrentTime };
            if (CurrentZone != 0)
                yield return new OpZoneChange() { Zone = CurrentZone };
            foreach (var o in Waymarks.CompareToInitial())
                yield return o;
            foreach (var o in Actors.CompareToInitial())
                yield return o;
            foreach (var o in Party.CompareToInitial())
                yield return o;
        }

        // implementation of operations
        public event EventHandler<OpFrameStart>? FrameStarted;
        public class OpFrameStart : Operation
        {
            public DateTime NewTimestamp;
            public TimeSpan PrevUpdateTime;
            public long FrameTimeMS;
            public ulong GaugePayload;

            protected override void Exec(WorldState ws)
            {
                ws.CurrentTime = NewTimestamp;
                ws.FrameStarted?.Invoke(ws, this);
            }

            public override string Str(WorldState? ws) => $"FRAM|{PrevUpdateTime.TotalMilliseconds:f3}|{FrameTimeMS}|{GaugePayload:X16}";
        }

        public event EventHandler<OpRSVData>? RSVDataReceived;
        public class OpRSVData : Operation
        {
            public string Key = "";
            public string Value = "";

            protected override void Exec(WorldState ws)
            {
                Service.LuminaGameData?.Excel.RsvProvider.Add(Key, Value);
                ws.RSVDataReceived?.Invoke(ws, this);
            }

            public override string Str(WorldState? ws) => $"RSV |{Key}|{Value}";
        }

        public event EventHandler<OpZoneChange>? CurrentZoneChanged;
        public class OpZoneChange : Operation
        {
            public ushort Zone;

            protected override void Exec(WorldState ws)
            {
                ws.CurrentZone = Zone;
                ws.CurrentZoneChanged?.Invoke(ws, this);
            }

            public override string Str(WorldState? ws) => $"ZONE|{Zone}";
        }

        // global events
        public event EventHandler<OpDirectorUpdate>? DirectorUpdate;
        public class OpDirectorUpdate : Operation
        {
            public uint DirectorID;
            public uint UpdateID;
            public uint Param1;
            public uint Param2;
            public uint Param3;
            public uint Param4;

            protected override void Exec(WorldState ws)
            {
                ws.DirectorUpdate?.Invoke(ws, this);
            }

            public override string Str(WorldState? ws) => $"DIRU|{DirectorID:X8}|{UpdateID:X8}|{Param1:X8}|{Param2:X8}|{Param3:X8}|{Param4:X8}";
        }

        public event EventHandler<OpEnvControl>? EnvControl;
        public class OpEnvControl : Operation
        {
            public uint DirectorID;
            public byte Index;
            public uint State;

            protected override void Exec(WorldState ws)
            {
                ws.EnvControl?.Invoke(ws, this);
            }

            public override string Str(WorldState? ws) => $"ENVC|{DirectorID:X8}|{Index:X2}|{State:X8}";
        }
    }
}
