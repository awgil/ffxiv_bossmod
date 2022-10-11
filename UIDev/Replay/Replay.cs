using BossMod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UIDev
{
    public class Replay
    {
        public class TimeRange
        {
            public DateTime Start;
            public DateTime End;

            public TimeRange(DateTime start = new(), DateTime end = new()) { Start = start; End = end; }
            public override string ToString() => $"{(End - Start).TotalSeconds:f2}";
            public bool Contains(DateTime t) => t >= Start && t <= End;
        }

        public class Cast
        {
            public ActionID ID;
            public float ExpectedCastTime;
            public TimeRange Time = new();
            public Participant? Target;
            public Vector3 Location; // if target is non-null, corresponds to target's position at cast start
            public Angle Rotation;
            public bool Interruptible;
        }

        public class ActionTarget
        {
            public Participant? Target;
            public ActionEffects Effects;
        }

        public class Action
        {
            public ActionID ID;
            public DateTime Timestamp;
            public Participant? Source;
            public Participant? MainTarget;
            public Vector3 TargetPos;
            public uint GlobalSequence;
            public List<ActionTarget> Targets = new();
        }

        public class Participant
        {
            public ulong InstanceID;
            public uint OID;
            public ActorType Type;
            public ulong OwnerID;
            public string Name = "";
            public TimeRange Existence = new();
            public SortedList<DateTime, bool> TargetableHistory = new();
            public SortedList<DateTime, bool> DeadHistory = new();
            public SortedList<DateTime, Vector4> PosRotHistory = new();
            public SortedList<DateTime, (ActorHP hp, uint curMP)> HPMPHistory = new();
            public List<Cast> Casts = new();
            public bool HasAnyActions;
            public bool HasAnyStatuses;
            public bool IsTargetOfAnyActions;

            public bool TargetableAt(DateTime t) => HistoryEntryAt(TargetableHistory, t);
            public bool DeadAt(DateTime t) => HistoryEntryAt(DeadHistory, t);
            public Vector4 PosRotAt(DateTime t) => HistoryEntryAt(PosRotHistory, t);
            public (ActorHP hp, uint curMP) HPMPAt(DateTime t) => HistoryEntryAt(HPMPHistory, t);
            public ActorHP HPAt(DateTime t) => HPMPAt(t).hp;
            public uint MPAt(DateTime t) => HPMPAt(t).curMP;

            private T? HistoryEntryAt<T>(SortedList<DateTime, T> history, DateTime t)
            {
                int next = history.UpperBound(t);
                return next == 0 ? default : history.Values[next - 1];
            }
        }

        public class Status
        {
            public uint ID;
            public int Index;
            public Participant? Target;
            public Participant? Source;
            public float InitialDuration;
            public TimeRange Time = new();
            public ushort StartingExtra;
        }

        public class Tether
        {
            public uint ID;
            public Participant? Source;
            public Participant? Target;
            public TimeRange Time = new();
        }

        public class Icon
        {
            public uint ID;
            public Participant? Target;
            public DateTime Timestamp;
        }

        public class DirectorUpdate
        {
            public uint DirectorID;
            public uint UpdateID;
            public uint Param1;
            public uint Param2;
            public uint Param3;
            public uint Param4;
            public DateTime Timestamp;
        }

        public class EnvControl
        {
            public uint DirectorID;
            public byte Index;
            public uint State;
            public DateTime Timestamp;
        }

        public class EncounterState
        {
            public uint ID;
            public string Name = "";
            public string Comment = "";
            public float ExpectedDuration;
            public DateTime Exit;

            public string FullName => $"{ID:X} '{Name}' ({Comment})";
        }

        public class EncounterError
        {
            public DateTime Timestamp;
            public Type? CompType;
            public string Message = "";
        }

        public class Encounter
        {
            public ulong InstanceID;
            public uint OID;
            public ushort Zone;
            public TimeRange Time = new(); // pull to deactivation
            public List<EncounterState> States = new();
            public List<EncounterError> Errors = new();
            public Dictionary<uint, List<Participant>> Participants = new(); // key = oid
            public List<(Participant, Class)> PartyMembers = new();
            public int FirstAction;
            public int FirstStatus;
            public int FirstTether;
            public int FirstIcon;
            public int FirstDirectorUpdate;
            public int FirstEnvControl;
        }

        public string Path = "";
        public List<WorldState.Operation> Ops = new();
        public List<Participant> Participants = new();
        public List<Action> Actions = new();
        public List<Status> Statuses = new();
        public List<Tether> Tethers = new();
        public List<Icon> Icons = new();
        public List<DirectorUpdate> DirectorUpdates = new();
        public List<EnvControl> EnvControls = new();
        public List<Encounter> Encounters = new();

        public IEnumerable<Action> EncounterActions(Encounter e) => Actions.Skip(e.FirstAction).TakeWhile(a => a.Timestamp <= e.Time.End);
        public IEnumerable<Status> EncounterStatuses(Encounter e) => Statuses.Skip(e.FirstStatus).TakeWhile(s => s.Time.Start <= e.Time.End);
        public IEnumerable<Tether> EncounterTethers(Encounter e) => Tethers.Skip(e.FirstTether).TakeWhile(t => t.Time.Start <= e.Time.End);
        public IEnumerable<Icon> EncounterIcons(Encounter e) => Icons.Skip(e.FirstIcon).TakeWhile(i => i.Timestamp <= e.Time.End);
        public IEnumerable<DirectorUpdate> EncounterDirectorUpdates(Encounter e) => DirectorUpdates.Skip(e.FirstDirectorUpdate).TakeWhile(du => du.Timestamp <= e.Time.End);
        public IEnumerable<EnvControl> EncounterEnvControls(Encounter e) => EnvControls.Skip(e.FirstEnvControl).TakeWhile(ec => ec.Timestamp <= e.Time.End);
    }
}
