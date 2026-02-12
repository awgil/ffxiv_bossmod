using BossMod.Interfaces;

namespace BossMod.Mocks;

internal class MockWorldStateFactory : IWorldStateFactory
{
    public RealWorld Create() => new(0, "unknown");
}
