using BossMod.Interfaces;
using System.IO;

namespace BossMod;

internal class WorldStateFactory : IWorldStateFactory
{
    public unsafe RealWorld Create() => new(
        (ulong)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency,
        File.ReadAllText("ffxivgame.ver")
    );
}
