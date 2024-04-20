using System.Runtime.InteropServices;
using System.Text;

namespace BossMod;

// utility to remove anti-multibox check
public static partial class MultiboxUnlock
{
    public unsafe static void Exec()
    {
        foreach (var handle in EnumHandles())
        {
            // there's a weird bug in winapi - sometimes name query can hang; apparently it happens on some file objects
            // to avoid that, try to get names only for mutexes
            if (ObjectNameOrTypeName(handle, true) == "Mutant")
            {
                var name = ObjectNameOrTypeName(handle, false);
                if (name.Contains("6AA83AB5-BAC4-4a36-9F66-A309770760CB_ffxiv_game0", StringComparison.Ordinal))
                {
                    Service.Log($"[Multibox] Closing handle {handle:X} '{name}'");
                    CloseHandle(handle);
                }
            }
        }
    }

    private unsafe static List<ulong> EnumHandles()
    {
        List<ulong> ret = [];
        uint bufferSize = 0x8000;
        while (true)
        {
            var buffer = new byte[bufferSize];
            fixed (byte* pbuf = &buffer[0])
            {
                var psnap = (PROCESS_HANDLE_SNAPSHOT_INFORMATION*)pbuf;
                psnap->NumberOfHandles = 0;
                // ProcessHandleInformation == 51
                uint retSize = 0;
                var status = NtQueryInformationProcess(ulong.MaxValue, 51, pbuf, bufferSize, &retSize);
                if ((uint)status == 0xC0000004) // STATUS_INFO_LENGTH_MISMATCH
                {
                    bufferSize = retSize;
                    continue;
                }

                if (status >= 0)
                {
                    var handles = (PROCESS_HANDLE_TABLE_ENTRY_INFO*)(psnap + 1);
                    for (ulong i = 0; i < psnap->NumberOfHandles; ++i)
                        ret.Add(handles[i].HandleValue);
                }
                break;
            }
        }
        return ret;
    }

    // both OBJECT_NAME_INFORMATION and OBJECT_TYPE_INFORMATION have UNICODE_STRING as first member, so same thing works for both
    private unsafe static string ObjectNameOrTypeName(ulong handle, bool typeName)
    {
        uint bufferSize = 1024;
        var buffer = new byte[bufferSize];
        fixed (byte* pbuf = &buffer[0])
        {
            uint retSize = 0;
            var status = NtQueryObject(handle, typeName ? 2 : 1, pbuf, bufferSize, &retSize);
            if (status >= 0)
            {
                var name = (UNICODE_STRING*)pbuf;
                if (name->Buffer != null)
                    return Encoding.Unicode.GetString(name->Buffer, name->Length);
            }
        }
        return "";
    }

#pragma warning disable 0649
    private struct PROCESS_HANDLE_TABLE_ENTRY_INFO
    {
        public ulong HandleValue;
        public ulong HandleCount;
        public ulong PointerCount;
        public uint GrantedAccess;
        public uint ObjectTypeIndex;
        public uint HandleAttributes;
        public uint Reserved;
    }

    private struct PROCESS_HANDLE_SNAPSHOT_INFORMATION
    {
        public ulong NumberOfHandles;
        public ulong Reserved;
        //PROCESS_HANDLE_TABLE_ENTRY_INFO Handles;
    }

    private unsafe struct UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        public byte* Buffer;
    }
#pragma warning restore 0649

    [LibraryImport("ntdll.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static unsafe partial int NtQueryInformationProcess(ulong ProcessHandle, int ProcessInformationClass, void* ProcessInformation, uint ProcessInformationLength, uint* ReturnLength);

    [LibraryImport("ntdll.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static unsafe partial int NtQueryObject(ulong Handle, int ObjectInformationClass, void* ObjectInformation, uint ObjectInformationLength, uint* ReturnLength);

    [LibraryImport("kernel32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool CloseHandle(ulong Handle);
}
