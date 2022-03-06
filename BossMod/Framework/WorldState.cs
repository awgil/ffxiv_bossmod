using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    // this class represents parts of a world state that are interesting to boss modules
    // it does not know anything about dalamud, so it can be used for UI test - there is a separate utility that updates it based on game state every frame
    public class WorldState
    {
        public DateTime CurrentTime;
        public WaymarkState Waymarks { get; init; } = new();
        public ActorState Actors { get; init; } = new();
        public PartyState Party { get; init; } = new();
        public WorldEvents Events { get; init; } = new();

        private ushort _currentZone;
        public event EventHandler<ushort>? CurrentZoneChanged;
        public ushort CurrentZone
        {
            get => _currentZone;
            set
            {
                if (_currentZone != value)
                {
                    _currentZone = value;
                    CurrentZoneChanged?.Invoke(this, value);
                }
            }
        }

        // TODO: redesign this, it should be a per-actor flag
        private bool _playerInCombat;
        public event EventHandler<bool>? PlayerInCombatChanged;
        public bool PlayerInCombat
        {
            get => _playerInCombat;
            set
            {
                if (_playerInCombat != value)
                {
                    _playerInCombat = value;
                    PlayerInCombatChanged?.Invoke(this, value);
                }
            }
        }
    }
}
