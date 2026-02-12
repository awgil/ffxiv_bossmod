using BossMod.Interfaces;

namespace BossMod.Mocks;

internal class MockWorldStateFactory : IWorldStateFactory
{
    public WorldState Create() => new(0, "unknown");
}
