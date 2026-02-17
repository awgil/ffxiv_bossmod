namespace BossMod;

public sealed class Replay
{
    public record struct TimeRange(DateTime Start = default, DateTime End = default)
    {
        public readonly float Duration => (float)(End - Start).TotalSeconds;

        public override readonly string ToString() => $"{Duration:f2}";
        public readonly bool Contains(DateTime t) => t >= Start && t <= End;
        public readonly TimeSpan Distance(DateTime t) => t < Start ? Start - t : t > End ? t - End : default;
    }

    // note: if target is non-null, Location corresponds to target's position at cast start
    public sealed record class Cast(ActionID ID, float ExpectedCastTime, Participant? Target, Vector3 Location, Angle Rotation, bool Interruptible)
    {
        public TimeRange Time;
        public ClientAction? ClientAction;
    }

    public sealed record class ActionTarget(Participant Target, ActionEffects Effects)
    {
        public DateTime ConfirmationSource;
        public DateTime ConfirmationTarget;
    }

    // note: if main target is non-null, TargetPos corresponds to main target's position at cast event
    public sealed record class Action(ActionID ID, DateTime Timestamp, Participant Source, Participant? MainTarget, Vector3 TargetPos, float AnimationLock, uint SourceSequence, uint GlobalSequence, Angle Rotation)
    {
        public readonly List<ActionTarget> Targets = [];
        public ClientAction? ClientAction;
    }

    // note: actors are sometimes destroyed and recreated (e.g. due to object size limits); we combine them into single participant structure with multiple 'instances'
    // sometimes we could even have participants that never exist as actors (but are referenced by other actions/events/etc.)
    // on the other hand, sometimes instance ids are reused for clearly different actors (e.g. different OIDs) - we try to separate them into different participants
    // we use a heuristic to distinguish actor destruction/recreation vs instance id reuse
    public sealed record class Participant(ulong InstanceID)
    {
        public uint OID;
        public ActorType Type;
        public ulong OwnerID;
        public uint ZoneID;
        public uint CFCID;
        public TimeRange EffectiveExistence;
        public readonly List<TimeRange> WorldExistence = []; // sorted by time, non-overlapping ranges
        public readonly SortedList<DateTime, (string name, uint id)> NameHistory = [];
        public readonly SortedList<DateTime, bool> TargetableHistory = [];
        public readonly SortedList<DateTime, bool> DeadHistory = [];
        public readonly SortedList<DateTime, bool> AllyHistory = [];
        public readonly SortedList<DateTime, Vector4> PosRotHistory = [];
        public readonly SortedList<DateTime, ActorHPMP> HPMPHistory = [];
        public readonly List<Cast> Casts = [];
        public readonly SortedList<DateTime, uint> EventObjectAnimation = [];
        public readonly SortedList<DateTime, byte> EventState = [];
        public readonly SortedList<DateTime, ushort> ActionTimeline = [];
        public float MinRadius = float.MaxValue;
        public float MaxRadius = float.MinValue;
        public bool HasAnyActions;
        public bool HasAnyStatuses;
        public bool IsTargetOfAnyActions;

        public bool ExistsInWorldAt(DateTime t) => WorldExistence.Any(r => r.Contains(t));
        public (string? name, uint id) NameAt(DateTime t) => HistoryEntryAt(NameHistory, t);
        public bool TargetableAt(DateTime t) => HistoryEntryAt(TargetableHistory, t);
        public bool AllyAt(DateTime t) => HistoryEntryAt(AllyHistory, t);
        public bool DeadAt(DateTime t) => HistoryEntryAt(DeadHistory, t);
        public Vector4 PosRotAt(DateTime t) => HistoryEntryAt(PosRotHistory, t);
        public ActorHPMP HPMPAt(DateTime t) => HistoryEntryAt(HPMPHistory, t);

        public bool WasAlly => AllyHistory.Any(p => p.Value);

        private T? HistoryEntryAt<T>(SortedList<DateTime, T> history, DateTime t)
        {
            var next = history.UpperBound(t);
            return next == 0 ? default : history.Values[next - 1];
        }
    }

    public sealed record class Status(uint ID, int Index, Participant Target, Participant? Source, float InitialDuration, ushort StartingExtra)
    {
        public TimeRange Time;
    }

    public sealed record class Tether(uint ID, Participant Source, Participant Target)
    {
        public TimeRange Time;
    }

    public sealed record class Icon(uint ID, Participant Source, Participant? Target, DateTime Timestamp);

    public sealed record class DirectorUpdate(uint DirectorID, uint UpdateID, uint Param1, uint Param2, uint Param3, uint Param4, DateTime Timestamp);

    public sealed record class MapEffect(byte Index, uint State, DateTime Timestamp);

    public sealed record class ClientAction(ActionID ID, uint SourceSequence, Participant? Target, Vector3 TargetPos, DateTime Requested)
    {
        public DateTime Rejected;
        public Cast? Cast;
        public Action? Action;
    }

    public sealed record class EncounterPhase(int ID, uint LastStateID, DateTime Exit);

    public sealed record class EncounterState(uint ID, string Name, string Comment, float ExpectedDuration, List<uint> ExpectedSuccessors, DateTime Exit)
    {
        public string FullName => $"{ID:X} '{Name}' ({Comment})";
    }

    public sealed record class EncounterError(DateTime Timestamp, Type? CompType, string Message);

    public sealed record class Encounter(ulong InstanceID, uint OID, ushort Zone)
    {
        public float CountdownOnPull = 10000;
        public TimeRange Time; // pull to deactivation
        public readonly List<EncounterPhase> Phases = [];
        public readonly List<EncounterState> States = [];
        public readonly List<EncounterError> Errors = [];
        public readonly Dictionary<uint, List<Participant>> ParticipantsByOID = []; // key = oid
        public readonly List<(Participant p, Class cls, int level)> PartyMembers = [];
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
    public readonly List<WorldState.Operation> Ops = [];
    public readonly List<Participant> Participants = [];
    public readonly List<Action> Actions = [];
    public readonly List<Status> Statuses = [];
    public readonly List<Tether> Tethers = [];
    public readonly List<Icon> Icons = [];
    public readonly List<DirectorUpdate> DirectorUpdates = [];
    public readonly List<MapEffect> MapEffects = [];
    public readonly List<ClientAction> ClientActions = [];
    public readonly List<Encounter> Encounters = [];
    public readonly SortedList<DateTime, string> UserMarkers = [];

    public IEnumerable<Action> EncounterActions(Encounter e) => Actions.Skip(e.FirstAction).TakeWhile(a => a.Timestamp <= e.Time.End);
    public IEnumerable<Status> EncounterStatuses(Encounter e) => Statuses.Skip(e.FirstStatus).TakeWhile(s => s.Time.Start <= e.Time.End);
    public IEnumerable<Tether> EncounterTethers(Encounter e) => Tethers.Skip(e.FirstTether).TakeWhile(t => t.Time.Start <= e.Time.End);
    public IEnumerable<Icon> EncounterIcons(Encounter e) => Icons.Skip(e.FirstIcon).TakeWhile(i => i.Timestamp <= e.Time.End);
    public IEnumerable<DirectorUpdate> EncounterDirectorUpdates(Encounter e) => DirectorUpdates.Skip(e.FirstDirectorUpdate).TakeWhile(du => du.Timestamp <= e.Time.End);
    public IEnumerable<MapEffect> EncounterMapEffects(Encounter e) => MapEffects.Skip(e.FirstEnvControl).TakeWhile(ec => ec.Timestamp <= e.Time.End);

    public Participant? FindParticipant(ulong instanceID, DateTime t) => Participants.FirstOrDefault(p => p.InstanceID == instanceID && p.EffectiveExistence.Contains(t));
}
