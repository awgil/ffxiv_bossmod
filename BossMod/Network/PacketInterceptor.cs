using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod.Network;

[StructLayout(LayoutKind.Explicit, Pack = 1)]
unsafe struct ReceivedIPCPacket
{
    [FieldOffset(0x20)] public uint SourceActor;
    [FieldOffset(0x24)] public uint TargetActor;
    [FieldOffset(0x30)] public ulong PacketSize;
    [FieldOffset(0x38)] public ServerIPC.IPCHeader* PacketData;
}

[StructLayout(LayoutKind.Explicit, Pack = 1)]
unsafe struct ReceivedPacket
{
    [FieldOffset(0x10)] public ReceivedIPCPacket* IPC;
    [FieldOffset(0x18)] public long SendTimestamp;
}

[StructLayout(LayoutKind.Explicit, Size = 0x10)]
unsafe struct SentIPCHeader
{
    [FieldOffset(0x00)] public uint Opcode;
    [FieldOffset(0x08)] public ulong PayloadSize; // 0x10 (payload header) + actual data size
}

internal sealed unsafe class PacketInterceptor(ISigScanner scanner, IPluginLog logger, HookProvider hook) : IDisposable, IHostedService
{
    public delegate void ServerIPCReceivedDelegate(DateTime sendTimestamp, uint sourceServerActor, uint targetServerActor, ushort opcode, uint epoch, Span<byte> payload);
    public event ServerIPCReceivedDelegate? ServerIPCReceived;

    public delegate void ClientIPCSentDelegate(uint opcode, Span<byte> payload);
    public event ClientIPCSentDelegate? ClientIPCSent;

    private delegate bool FetchReceivedPacketDelegate(void* self, ReceivedPacket* outData);
    private Hook<FetchReceivedPacketDelegate>? _fetchHook;

    private delegate byte SendPacketDelegate(void* self, SentIPCHeader* packet, int* a3, byte a4);
    private Hook<SendPacketDelegate>? _sendHook;

    public bool ActiveRecv
    {
        get => _fetchHook?.IsEnabled ?? false;
        set
        {
            if (_fetchHook == null)
            {
                logger.Warning($"[NPI] Recv hook not found!");
                return;
            }

            if (value)
                _fetchHook.Enable();
            else
                _fetchHook.Disable();
        }
    }

    public bool ActiveSend
    {
        get => _sendHook?.IsEnabled ?? false;
        set
        {
            if (_sendHook == null)
            {
                logger.Warning("[NPI] Send hook not found!");
                return;
            }

            if (value)
                _sendHook.Enable();
            else
                _sendHook.Disable();
        }
    }

    public async Task StartAsync(CancellationToken token)
    {
        // alternative signatures - seem to be changing from build to build:
        // - E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 48 8D 35
        // - E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 44 0F B6 64 24
        var foundFetchAddress = scanner.TryScanText("E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 48 8D 4C 24 ?? FF 15", out var fetchAddress)
            || scanner.TryScanText("E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 44 0F B6 64 24", out fetchAddress);
        if (foundFetchAddress)
            _fetchHook = hook.FromAddress<FetchReceivedPacketDelegate>(fetchAddress, FetchReceivedPacketDetour, false);

        _sendHook = hook.FromSignature<SendPacketDelegate>("48 89 5C 24 ?? 48 89 74 24 ?? 4C 89 64 24 ?? 55 41 56 41 57 48 8B EC 48 83 EC 70", SendPacketDetour, false);

        // potentially useful sigs from dalamud:
        // server ipc handler: 40 53 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 8B F2 --- void(void* self, uint targetId, void* dataPtr)
    }

    public Task StopAsync(CancellationToken token) => Task.CompletedTask;

    public void Dispose()
    {
        _fetchHook?.Dispose();
        _sendHook?.Dispose();
    }

    private bool FetchReceivedPacketDetour(void* self, ReceivedPacket* outData)
    {
        var res = _fetchHook!.Original(self, outData);
        if (outData->IPC != null)
        {
            ServerIPCReceived?.Invoke(
                DateTimeOffset.FromUnixTimeMilliseconds(outData->SendTimestamp).DateTime,
                outData->IPC->SourceActor,
                outData->IPC->TargetActor,
                outData->IPC->PacketData->MessageType,
                outData->IPC->PacketData->Epoch,
                new(outData->IPC->PacketData + 1, (int)outData->IPC->PacketSize - sizeof(ServerIPC.IPCHeader)));
        }
        return res;
    }

    private byte SendPacketDetour(void* self, SentIPCHeader* packet, int* a3, byte a4)
    {
        ClientIPCSent?.Invoke(packet->Opcode, new((byte*)packet + 0x20, (int)packet->PayloadSize - 0x10));
        return _sendHook!.Original(self, packet, a3, a4);
    }
}
