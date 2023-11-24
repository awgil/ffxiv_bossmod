using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    public class Replay
    {
        public record struct TimeRange(DateTime Start = default, DateTime End = default)
        {
            public float Duration => (float)(End - Start).TotalSeconds;

            public override string ToString() => $"{Duration:f2}";
            public bool Contains(DateTime t) => t >= Start && t <= End;
        }

        // note: if target is non-null, Location corresponds to target's position at cast start
        public record class Cast(ActionID ID, float ExpectedCastTime, Participant? Target, Vector3 Location, Angle Rotation, bool Interruptible)
        {
            public TimeRange Time;
            public ClientAction? ClientAction;
        }

        public record class ActionTarget(Participant Target, ActionEffects Effects)
        {
            public DateTime ConfirmationSource;
            public DateTime ConfirmationTarget;
        }

        // note: if main target is non-null, TargetPos corresponds to main target's position at cast event
        public record class Action(ActionID ID, DateTime Timestamp, Participant Source, Participant? MainTarget, Vector3 TargetPos, float AnimationLock, uint GlobalSequence)
        {
            public List<ActionTarget> Targets = new();
            public ClientAction? ClientAction;
        }

        // note: actors are sometimes destroyed and recreated (e.g. due to object size limits); we combine them into single participant structure with multiple 'instances'
        // sometimes we could even have participants that never exist as actors (but are referenced by other actions/events/etc.)
        // on the other hand, sometimes instance ids are reused for clearly different actors (e.g. different OIDs) - we try to separate them into different participants
        // we use a heuristic to distinguish actor destruction/recreation vs instance id reuse
        public record class Participant(ulong InstanceID)
        {
            public uint OID;
            public ActorType Type;
            public ulong OwnerID;
            public TimeRange EffectiveExistence;
            public List<TimeRange> WorldExistence = new(); // sorted by time, non-overlapping ranges
            public SortedList<DateTime, string> NameHistory = new();
            public SortedList<DateTime, bool> TargetableHistory = new();
            public SortedList<DateTime, bool> DeadHistory = new();
            public SortedList<DateTime, Vector4> PosRotHistory = new();
            public SortedList<DateTime, (ActorHP hp, uint curMP)> HPMPHistory = new();
            public List<Cast> Casts = new();
            public float MinRadius = float.MaxValue;
            public float MaxRadius = float.MinValue;
            public bool HasAnyActions;
            public bool HasAnyStatuses;
            public bool IsTargetOfAnyActions;

            public bool ExistsInWorldAt(DateTime t) => WorldExistence.Any(r => r.Contains(t));
            public string NameAt(DateTime t) => HistoryEntryAt(NameHistory, t) ?? "";
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

        public record class Status(uint ID, int Index, Participant Target, Participant? Source, float InitialDuration, ushort StartingExtra)
        {
            public TimeRange Time;
        }

        public record class Tether(uint ID, Participant Source, Participant Target)
        {
            public TimeRange Time;
        }

        public record class Icon(uint ID, Participant Target, DateTime Timestamp);

        public record class DirectorUpdate(uint DirectorID, uint UpdateID, uint Param1, uint Param2, uint Param3, uint Param4, DateTime Timestamp);

        public record class EnvControl(byte Index, uint State, DateTime Timestamp);

        public record class ClientAction(ActionID ID, uint SourceSequence, Participant? Target, Vector3 TargetPos, DateTime Requested)
        {
            public DateTime Rejected;
            public Cast? Cast;
            public Action? Action;
        }

        public record class EncounterPhase(int ID, uint LastStateID, DateTime Exit);

        public record class EncounterState(uint ID, string Name, string Comment, float ExpectedDuration, DateTime Exit)
        {
            public string FullName => $"{ID:X} '{Name}' ({Comment})";
        }

        public record class EncounterError(DateTime Timestamp, Type? CompType, string Message);

        public record class Encounter(ulong InstanceID, uint OID, ushort Zone)
        {
            public float CountdownOnPull = 10000;
            public TimeRange Time = new(); // pull to deactivation
            public List<EncounterPhase> Phases = new();
            public List<EncounterState> States = new();
            public List<EncounterError> Errors = new();
            public Dictionary<uint, List<Participant>> ParticipantsByOID = new(); // key = oid
            public List<(Participant, Class)> PartyMembers = new();
            public int FirstAction;
            public int FirstStatus;
            public int FirstTether;
            public int FirstIcon;
            public int FirstDirectorUpdate;
            public int FirstEnvControl;
        }

        public string Path = "";
        public ulong QPF = TimeSpan.TicksPerSecond;
        public string GameVersion = "";
        public List<WorldState.Operation> Ops = new();
        public List<Participant> Participants = new();
        public List<Action> Actions = new();
        public List<Status> Statuses = new();
        public List<Tether> Tethers = new();
        public List<Icon> Icons = new();
        public List<DirectorUpdate> DirectorUpdates = new();
        public List<EnvControl> EnvControls = new();
        public List<ClientAction> ClientActions = new();
        public List<Encounter> Encounters = new();
        public SortedList<DateTime, string> UserMarkers = new();

        public IEnumerable<Action> EncounterActions(Encounter e) => Actions.Skip(e.FirstAction).TakeWhile(a => a.Timestamp <= e.Time.End);
        public IEnumerable<Status> EncounterStatuses(Encounter e) => Statuses.Skip(e.FirstStatus).TakeWhile(s => s.Time.Start <= e.Time.End);
        public IEnumerable<Tether> EncounterTethers(Encounter e) => Tethers.Skip(e.FirstTether).TakeWhile(t => t.Time.Start <= e.Time.End);
        public IEnumerable<Icon> EncounterIcons(Encounter e) => Icons.Skip(e.FirstIcon).TakeWhile(i => i.Timestamp <= e.Time.End);
        public IEnumerable<DirectorUpdate> EncounterDirectorUpdates(Encounter e) => DirectorUpdates.Skip(e.FirstDirectorUpdate).TakeWhile(du => du.Timestamp <= e.Time.End);
        public IEnumerable<EnvControl> EncounterEnvControls(Encounter e) => EnvControls.Skip(e.FirstEnvControl).TakeWhile(ec => ec.Timestamp <= e.Time.End);

        public Participant? FindParticipant(ulong instanceID, DateTime t) => Participants.FirstOrDefault(p => p.InstanceID == instanceID && p.EffectiveExistence.Contains(t));
    }
}
