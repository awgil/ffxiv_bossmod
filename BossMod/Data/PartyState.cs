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

        private ActorState _actorState;
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
            _actorState = actorState;
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

        public event EventHandler<(int slot, ulong contentID, ulong instanceID, ulong prevContentID, ulong prevInstanceID)>? Modified;

        public void Modify(int slot, ulong contentID, ulong instanceID)
        {
            if (slot < 0 || slot >= MaxAllianceSize)
            {
                Service.Log($"[PartyState] Out-of-bounds slot {slot}");
                return;
            }
            if (slot >= MaxPartySize && contentID != 0)
            {
                Service.Log($"[PartyState] Unexpected non-zero content ID {contentID:X}:{instanceID:X} for alliance slot #{slot}");
                return;
            }

            var prevContentID = slot < MaxPartySize ? _contentIDs[slot] : 0;
            var prevInstanceID = _actorIDs[slot];
            if (contentID == prevContentID && instanceID == prevInstanceID)
                return; // nothing to do

            if (slot < MaxPartySize)
                _contentIDs[slot] = contentID;
            _actorIDs[slot] = instanceID;
            _actors[slot] = _actorState.Find(instanceID);
            Modified?.Invoke(this, (slot, contentID, instanceID, prevContentID, prevInstanceID));
        }
    }
}
