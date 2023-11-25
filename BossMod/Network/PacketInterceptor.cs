using Dalamud.Hooking;
using System;
using System.Runtime.InteropServices;

namespace BossMod.Network
{
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

    public class PacketInterceptor : IDisposable
    {
        public delegate void ServerIPCReceivedDelegate(DateTime sendTimestamp, uint sourceServerActor, uint targetServerActor, ushort opcode, uint epoch, Span<byte> payload);
        public event ServerIPCReceivedDelegate? ServerIPCReceived;

        private unsafe delegate bool FetchReceivedPacketDelegate(void* self, ReceivedPacket* outData);
        private Hook<FetchReceivedPacketDelegate> _fetchHook;

        public bool Active
        {
            get => _fetchHook.IsEnabled;
            set
            {
                if (value)
                    _fetchHook.Enable();
                else
                    _fetchHook.Disable();
            }
        }

        public unsafe PacketInterceptor()
        {
            // alternative signature: E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 48 8D 35 - seem to be changing from build to build
            _fetchHook = Service.Hook.HookFromSignature<FetchReceivedPacketDelegate>("E8 ?? ?? ?? ?? 84 C0 0F 85 ?? ?? ?? ?? 44 0F B6 64 24", FetchReceivedPacketDetour);
            Service.Log($"[NPI] FetchReceivedPacket address = 0x{_fetchHook.Address:X}");

            // potentially useful sigs from dalamud:
            // server ipc handler: 40 53 56 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 44 24 ?? 8B F2 --- void(void* self, uint targetId, void* dataPtr)
            // client ipc handler: 48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC 70 8B 81 ?? ?? ?? ?? --- byte(void* self, void* dataPtr, void* a3, byte a4)
        }

        public void Dispose()
        {
            _fetchHook.Dispose();
        }

        private unsafe bool FetchReceivedPacketDetour(void* self, ReceivedPacket* outData)
        {
            var res = _fetchHook.Original(self, outData);
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
    }
}
