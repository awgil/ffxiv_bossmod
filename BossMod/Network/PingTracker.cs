using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Application.Network;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Network;

public sealed partial class PingTracker(IPluginLog logger) : IDisposable
{
    public float LastRTT { get; private set; }

    private readonly CancellationTokenSource Cts = new();

    public void Run()
    {
        Task.Run(Loop);
    }

    private async Task Loop()
    {
        var token = Cts.Token;

        while (!token.IsCancellationRequested)
        {
            unsafe
            {
                Fetch();
            }

            await Task.Delay(3000, token).ConfigureAwait(true);
        }
    }

    private unsafe void Fetch()
    {
        var fw = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->NetworkModuleProxy->NetworkModule;
        var zc = *(ZoneClient**)((nint)fw + 0xA70);
        if (zc == null)
        {
            logger.Verbose("ZoneClient is missing");
            return;
        }

        var addr = IPAddress.Parse(zc->Host.AsSpan());
        if (addr == IPAddress.Loopback)
        {
            logger.Debug("not connected");
            return;
        }

        var rtt = GetAddressLastRTT(addr);
        var err = Marshal.GetLastWin32Error();

        if (err != 0)
            logger.Warning($"Ping failure: {err}");
        else
            LastRTT = rtt * 0.001f;
    }

    public void Dispose()
    {
        logger.Info("Shutting down");
        Cts.Cancel();
        Cts.Dispose();
    }

    private static ulong GetAddressLastRTT(IPAddress addr)
    {
        var addressBytes = addr.GetAddressBytes();
        var raw = BitConverter.ToUInt32(addressBytes);

        var hopcount = 0u;
        var rtt = 0u;

        return GetRTTAndHopCount(raw, ref hopcount, 51, ref rtt) == 1 ? rtt : 0;
    }

    [LibraryImport("Iphlpapi.dll", EntryPoint = "GetRTTAndHopCount", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int GetRTTAndHopCount(uint address, ref uint hopCount, uint maxHops, ref uint rtt);
}
