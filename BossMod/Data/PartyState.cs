using System.Collections.ObjectModel;

namespace BossMod;

// state of the party/alliance/trust that player is part of; part of the world state structure
// solo player is considered to be in party of size 1
// after joining the party, member's slot never changes until leaving the party; this means that there could be intermediate gaps
// note that player could be in party without having actor in world (e.g. if he is in different zone)
// if player does not exist in world, party is always empty; otherwise player is always in slot 0
// in alliance, two 'other' groups use slots 8-15 and 16-23; alliance members don't have content-ID, but always have actor-ID
// in trust, buddies are considered party members with content-id 0 (but non-zero actor id, they are always in world)
// party slot is considered 'empty' if both ids are 0
public sealed class PartyState
{
    public const int PlayerSlot = 0;
    public const int MaxPartySize = 8;
    public const int MaxAllianceSize = 24;

    private readonly ulong[] _contentIDs = new ulong[MaxPartySize]; // non-alliance slots: empty slots or buddy slots contain 0's, alliance slots: n/a (FF always reports 0)
    private readonly ulong[] _actorIDs = new ulong[MaxAllianceSize]; // non-alliance slots: empty slots or slots corresponding to players not in world contain 0's, alliance slots: empty slots contains 0's
    private readonly Actor?[] _actors = new Actor?[MaxAllianceSize];

    public ReadOnlySpan<ulong> ContentIDs => _contentIDs;
    public ReadOnlySpan<ulong> ActorIDs => _actorIDs;
    public ReadOnlyCollection<Actor?> Members => Array.AsReadOnly(_actors);

    public Actor? this[int slot] => (slot >= 0 && slot < _actors.Length) ? _actors[slot] : null; // bounds-checking accessor
    public Actor? Player() => this[PlayerSlot];

    public int LimitBreakCur;
    public int LimitBreakMax = 10000;

    public PartyState(ActorState actorState)
    {
        void assign(ulong instanceID, Actor? actor)
        {
            var slot = FindSlot(instanceID);
            if (slot >= 0)
                _actors[slot] = actor;
        }
        actorState.Added.Subscribe(actor => assign(actor.InstanceID, actor));
        actorState.Removed.Subscribe(actor => assign(actor.InstanceID, null));
    }

    // select non-null and optionally alive raid members
    public IEnumerable<Actor> WithoutSlot(bool includeDead = false)
    {
        for (int i = 0; i < _actors.Length; ++i)
        {
            var player = _actors[i];
            if (player == null)
                continue;
            if (player.IsDead && !includeDead)
                continue;
            yield return player;
        }
    }

    public IEnumerable<(int, Actor)> WithSlot(bool includeDead = false)
    {
        for (int i = 0; i < _actors.Length; ++i)
        {
            var player = _actors[i];
            if (player == null)
                continue;
            if (player.IsDead && !includeDead)
                continue;
            yield return (i, player);
        }
    }

    // find a slot index containing specified player (by instance ID); returns -1 if not found
    public int FindSlot(ulong instanceID) => instanceID != 0 ? Array.IndexOf(_actorIDs, instanceID) : -1;

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        for (int i = 0; i < MaxAllianceSize; ++i)
            if (i < MaxPartySize && _contentIDs[i] != 0 || _actorIDs[i] != 0)
                yield return new OpModify(i, i < MaxPartySize ? _contentIDs[i] : 0, _actorIDs[i]);
        if (LimitBreakCur != 0 || LimitBreakMax != 10000)
            yield return new OpLimitBreakChange(LimitBreakCur, LimitBreakMax);
    }

    // implementation of operations
    public Event<OpModify> Modified = new();
    public sealed record class OpModify(int Slot, ulong ContentID, ulong InstanceID) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            if (Slot is < 0 or >= MaxAllianceSize)
            {
                Service.Log($"[PartyState] Out-of-bounds slot {Slot}");
                return;
            }
            if (Slot >= MaxPartySize && ContentID != 0)
            {
                Service.Log($"[PartyState] Unexpected non-zero content ID {ContentID:X}:{InstanceID:X} for alliance slot #{Slot}");
                return;
            }

            if (Slot < MaxPartySize)
                ws.Party._contentIDs[Slot] = ContentID;
            ws.Party._actorIDs[Slot] = InstanceID;
            ws.Party._actors[Slot] = ws.Actors.Find(InstanceID);
            ws.Party.Modified.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("PAR "u8).Emit(Slot).Emit(ContentID, "X").Emit(InstanceID, "X8");
    }

    public Event<OpLimitBreakChange> LimitBreakChanged = new();
    public sealed record class OpLimitBreakChange(int Cur, int Max) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            ws.Party.LimitBreakCur = Cur;
            ws.Party.LimitBreakMax = Max;
            ws.Party.LimitBreakChanged.Fire(this);
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("LB  "u8).Emit(Cur).Emit(Max);
    }
}
