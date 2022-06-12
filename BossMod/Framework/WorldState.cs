using System;

namespace BossMod
{
    // this class represents parts of a world state that are interesting to boss modules
    // it does not know anything about dalamud, so it can be used for UI test - there is a separate utility that updates it based on game state every frame
    public class WorldState
    {
        public DateTime CurrentTime;
        public WaymarkState Waymarks { get; init; } = new();
        public ActorState Actors { get; init; } = new();
        public PartyState Party { get; init; }
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

        public WorldState()
        {
            Party = new(Actors);
        }
    }
}
