namespace BossMod;

// state of the party/alliance/trust that player is part of; part of the world state structure
// solo player is considered to be in party of size 1
// after joining the party, member's slot never changes until leaving the party; this means that there could be intermediate gaps
// note that player could be in party without having actor in world (e.g. if he is in different zone)
// if player does not exist in world, party is always empty; otherwise player is always in slot 0
// in alliance, two 'other' groups use slots 8-15 and 16-23; alliance members don't have content-ID, but always have actor-ID
// in trust, buddies are considered party members with content-id 0 (but non-zero actor id, they are always in world)
// slots 24-63 are occupied by friendly NPCs, i.e. actors with type = Enemy who have the IsAlly and IsTargetable flags set
// certain modules need to treat NPCs as regular party members for the purpose of mechanic resolution
// we limit to 64 slots to facilitate a bitmask for the entire "party" state fitting inside one ulong
// party slot is considered 'empty' if both ids are 0
public sealed class PartyState
{
    public const int PlayerSlot = 0;
    public const int MaxPartySize = 8;
    public const int MaxAllianceSize = 24;
    public const int MaxAllies = 64;

    public record struct Member(ulong ContentId, ulong InstanceId, bool InCutscene, string Name)
    {
        // note that a valid member can have 0 contentid (eg buddy) or 0 instanceid (eg player in a different zone)
        public readonly bool IsValid() => ContentId != 0 || InstanceId != 0;
    }
    public static readonly Member EmptySlot = new(0, 0, false, "");

    public readonly Member[] Members = Utils.MakeArray(MaxAllies, EmptySlot);
    private readonly Actor?[] _actors = new Actor?[MaxAllies]; // transient

    public Actor? this[int slot] => (slot >= 0 && slot < _actors.Length) ? _actors[slot] : null; // bounds-checking accessor
    public Actor? Player() => this[PlayerSlot];

    public int LimitBreakCur;
    public int LimitBreakMax = 10000;

    public int LimitBreakLevel => LimitBreakMax > 0 ? LimitBreakCur / LimitBreakMax : 0;

    public PartyState(ActorState actorState)
    {
        void assign(ulong instanceID, Actor? actor)
        {
            if (TryFindSlot(instanceID, out var slot))
                _actors[slot] = actor;
        }
        actorState.Added.Subscribe(actor => assign(actor.InstanceID, actor));
        actorState.Removed.Subscribe(actor => assign(actor.InstanceID, null));
    }

    // select non-null and optionally alive raid members
    public IEnumerable<Actor> WithoutSlot(bool includeDead = false, bool excludeAlliance = false, bool excludeNPCs = false)
    {
        for (int i = 0; i < MaxAllies; ++i)
        {
            if (excludeNPCs && i >= MaxAllianceSize)
                break;
            if (excludeAlliance && i is >= MaxPartySize and < MaxAllianceSize)
                continue;
            var player = _actors[i];
            if (player == null)
                continue;
            if (player.IsDead && !includeDead)
                continue;
            yield return player;
        }
    }

    public IEnumerable<(int, Actor)> WithSlot(bool includeDead = false, bool excludeAlliance = false)
    {
        for (int i = 0; i < MaxAllies; ++i)
        {
            if (excludeAlliance && i is >= MaxPartySize and < MaxAllianceSize)
                continue;
            var player = _actors[i];
            if (player == null)
                continue;
            if (player.IsDead && !includeDead)
                continue;
            yield return (i, player);
        }
    }

    // find a slot index containing specified player (by instance ID); returns -1 if not found
    public int FindSlot(ulong instanceID) => instanceID != 0 ? Array.FindIndex(Members, m => m.InstanceId == instanceID) : -1;

    // find a slot index containing specified player (by name); returns -1 if not found
    public int FindSlot(ReadOnlySpan<char> name, StringComparison cmp = StringComparison.CurrentCultureIgnoreCase)
    {
        for (int i = 0; i < Members.Length; ++i)
            if (name.Equals(Members[i].Name, cmp))
                return i;
        return -1;
    }

    public bool TryFindSlot(ulong instanceID, out int slot)
    {
        slot = FindSlot(instanceID);
        return slot >= 0;
    }

    public bool TryFindSlot(Actor actor, out int slot) => TryFindSlot(actor.InstanceID, out slot);

    public bool TryFindSlot(ReadOnlySpan<char> name, out int slot, StringComparison cmp = StringComparison.CurrentCultureIgnoreCase)
    {
        slot = FindSlot(name, cmp);
        return slot >= 0;
    }

    public IEnumerable<WorldState.Operation> CompareToInitial()
    {
        for (int i = 0; i < Members.Length; ++i)
            if (Members[i].IsValid())
                yield return new OpModify(i, Members[i]);
        if (LimitBreakCur != 0 || LimitBreakMax != 10000)
            yield return new OpLimitBreakChange(LimitBreakCur, LimitBreakMax);
    }

    // implementation of operations
    public Event<OpModify> Modified = new();
    public sealed record class OpModify(int Slot, Member Member) : WorldState.Operation
    {
        protected override void Exec(WorldState ws)
        {
            if (Slot >= 0 && Slot < ws.Party.Members.Length)
            {
                ws.Party.Members[Slot] = Member;
                ws.Party._actors[Slot] = ws.Actors.Find(Member.InstanceId);
                ws.Party.Modified.Fire(this);
            }
            else
            {
                Service.Log($"[PartyState] Out-of-bounds slot {Slot}");
            }
        }
        public override void Write(ReplayRecorder.Output output) => output.EmitFourCC("PAR "u8).Emit(Slot).Emit(Member.ContentId, "X").Emit(Member.InstanceId, "X8").Emit(Member.InCutscene).Emit(Member.Name);
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
