using BossMod.Interfaces;
using System.IO;

namespace BossMod;

internal class WorldStateFactory : IWorldStateFactory
{
    public unsafe WorldState Create()
    {
        var qpf = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->PerformanceCounterFrequency;
        var gamever = File.ReadAllText("ffxivgame.ver");
        return new((ulong)qpf, gamever);
    }
}
