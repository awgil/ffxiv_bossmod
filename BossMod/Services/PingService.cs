using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Application.Network;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Services;

internal sealed partial class PingService(IPluginLog logger) : IHostedService
{
    public float LastRTT { get; private set; }

    private CancellationTokenSource Source = new();
    private Task UpdateTask = Task.CompletedTask;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Source = new();

        var token = Source.Token;
        UpdateTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                unsafe
                {
                    Fetch();
                }

                await Task.Delay(3000, token);
            }
        }, token);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Source.Cancel();

        // task is intentionally not awaited, GetRTTAndHopCount blocks for a pretty long time, and unlike config saver service we don't care about race conditions
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
