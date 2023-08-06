using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BossMod
{
    // state of the party/alliance that player is part of; part of the world state structure
    // solo player is considered to be in party of size 1
    // after joining the party, member's slot never changes until leaving the party; this means that there could be intermediate gaps
    // note that player could be in party without having actor in world (e.g. if he is in different zone)
    // if player does not exist in world, party is always empty; otherwise player is always in slot 0
    // in alliance, two 'other' groups use slots 8-15 and 16-23; alliance members don't have content-ID, but always have actor-ID
    public class PartyState
    {
        public static int PlayerSlot { get; } = 0;
        public static int MaxPartySize { get; } = 8;
        public static int MaxAllianceSize { get; } = 24;

        private ulong[] _contentIDs = new ulong[MaxPartySize]; // non-alliance slots: empty slots contain 0's, alliance slots: n/a (FF always reports 0)
        private ulong[] _actorIDs = new ulong[MaxAllianceSize]; // non-alliance slots: empty slots or slots corresponding to players not in world contain 0's, alliance slots: empty slots contains 0's
        private Actor?[] _actors = new Actor?[MaxAllianceSize];

        public ReadOnlySpan<ulong> ContentIDs => _contentIDs;
        public ReadOnlySpan<ulong> ActorIDs => _actorIDs;
        public ReadOnlyCollection<Actor?> Members => Array.AsReadOnly(_actors);

        public Actor? this[int slot] => (slot >= 0 && slot < _actors.Length) ? _actors[slot] : null; // bounds-checking accessor
        public Actor? Player() => this[PlayerSlot];

        public PartyState(ActorState actorState)
        {
            actorState.Added += (_, actor) =>
            {
                var slot = FindSlot(actor.InstanceID);
                if (slot >= 0)
                    _actors[slot] = actor;
            };
            actorState.Removed += (_, actor) =>
            {
                var slot = FindSlot(actor.InstanceID);
                if (slot >= 0)
                    _actors[slot] = null;
            };
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
                    yield return new OpModify() { Slot = i, ContentID = i < MaxPartySize ? _contentIDs[i] : 0, InstanceID = _actorIDs[i] };
        }

        // implementation of operations
        public event EventHandler<OpModify>? Modified;
        public class OpModify : WorldState.Operation
        {
            public int Slot;
            public ulong ContentID;
            public ulong InstanceID;

            protected override void Exec(WorldState ws)
            {
                if (Slot < 0 || Slot >= MaxAllianceSize)
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
                ws.Party.Modified?.Invoke(ws, this);
            }

            public override void Write(ReplayRecorder.Output output) => WriteTag(output, "PAR ").Emit(Slot).Emit(ContentID, "X").Emit(InstanceID, "X8");
        }
    }
}
