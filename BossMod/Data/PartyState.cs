using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BossMod
{
    // state of the party that player is part of; part of the world state structure
    // does not include alliance members
    // solo player is considered to be in party of size 1
    // after joining the party, member's slot never changes until leaving the party; this means that there could be intermediate gaps
    // note that player could be in party without having actor in world (e.g. if he is in different zone)
    // if player does not exist in world, party is always empty; otherwise player is always in slot 0
    public class PartyState
    {
        public static int PlayerSlot { get; } = 0;
        public static int MaxSize { get; } = 8;

        private ulong[] _contentIDs = new ulong[MaxSize]; // empty slots contain 0's
        private Actor?[] _actors = new Actor?[MaxSize];

        public ReadOnlySpan<ulong> ContentIDs => _contentIDs;
        public ReadOnlyCollection<Actor?> Members => Array.AsReadOnly(_actors);

        public Actor? this[int slot] => (slot >= 0 && slot < _actors.Length) ? _actors[slot] : null; // bounds-checking accessor
        public Actor? Player() => this[PlayerSlot];

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
        public int FindSlot(uint instanceID)
        {
            return instanceID != 0 ? Array.FindIndex(_actors, x => x?.InstanceID == instanceID) : -1;
        }

        public event EventHandler<(int, ulong, Actor?)>? Joined;
        public event EventHandler<(int, ulong, Actor?)>? Left;
        public event EventHandler<(int, ulong, Actor?)>? Reassigned; // actor representation changed for same player (usually to/from null)

        public int Add(ulong contentID, Actor? actor, bool isPlayer)
        {
            int slot = Array.IndexOf(_contentIDs, 0ul);
            if (slot == -1)
            {
                Service.Log($"[PartyState] Too many raid members: {_actors.Length} already exist; skipping new member {contentID:X}");
            }
            else if (isPlayer != (slot == 0))
            {
                Service.Log($"[PartyState] Unexpected is-player={isPlayer}, slot={slot}");
                slot = -1;
            }
            else
            {
                _contentIDs[slot] = contentID;
                _actors[slot] = actor;
                Joined?.Invoke(this, (slot, contentID, actor));
            }
            return slot;
        }

        public void Remove(int slot)
        {
            if (_contentIDs[slot] == 0)
            {
                Service.Log($"[PartyState] Trying to remove non-existent member from slot {slot}");
            }
            else
            {
                Left?.Invoke(this, (slot, _contentIDs[slot], _actors[slot]));
                _contentIDs[slot] = 0;
                _actors[slot] = null;
            }
        }

        public void AssignActor(int slot, ulong contentID, Actor? actor)
        {
            if (_contentIDs[slot] != contentID || contentID == 0)
            {
                Service.Log($"[PartyState] Trying to assign actor to non-existent or incorrect slot #{slot}: contains {_contentIDs[slot]:X}, got {contentID:X}");
            }
            else if (_actors[slot] != actor)
            {
                _actors[slot] = actor;
                Reassigned?.Invoke(this, (slot, contentID, actor));
            }
        }
    }
}
