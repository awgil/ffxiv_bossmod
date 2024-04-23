using Dalamud.Game.ClientState.Keys;
using System.Runtime.InteropServices;

namespace BossMod.AI;

internal sealed partial class Broadcast
{
    private readonly AIConfig _config = Service.Config.Get<AIConfig>();
    private readonly List<(VirtualKey, bool)> _broadcasts = [
        (VirtualKey.F10, false), // target focus
        (VirtualKey.T, false), // target target's target
        (VirtualKey.G, false), // mount
        (VirtualKey.SPACE, false), // jump
        (VirtualKey.NUMPAD0, false), // interact
        (VirtualKey.NUMPAD2, false), // menu navigation
        (VirtualKey.NUMPAD4, false), // menu navigation
        (VirtualKey.NUMPAD6, false), // menu navigation
        (VirtualKey.NUMPAD8, false), // menu navigation
    ];

    public void Update()
    {
        if (!_config.BroadcastToSlaves)
            return;

        for (int i = 0; i < _broadcasts.Count; ++i)
        {
            var vk = _broadcasts[i].Item1;
            bool pressed = (GetKeyState((int)vk) & 0x8000) == 0x8000;
            if (pressed != _broadcasts[i].Item2)
            {
                foreach (var w in EnumerateSlaves())
                {
                    Service.Log($"Broadcast: {vk} to {w}");
                    PostMessageW(w, pressed ? 0x0100u : 0x0101u, (ulong)vk, 0);
                }
                _broadcasts[i] = (vk, pressed);
            }
        }
    }

    public static void BroadcastKeypress(VirtualKey vk)
    {
        foreach (var w in EnumerateSlaves())
            SendKeypress(w, vk);
    }

    private static void SendKeypress(IntPtr hwnd, VirtualKey vk)
    {
        PostMessageW(hwnd, 0x0100, (ulong)vk, 0);
        PostMessageW(hwnd, 0x0101, (ulong)vk, 0);
    }

    private static List<IntPtr> EnumerateSlaves()
    {
        List<IntPtr> res = [];
        var active = GetActiveWindow();
        var name = WindowName(active);
        if (name.Length == 0)
            return res;
        _ = EnumWindows((hwnd, lparam) =>
        {
            if (hwnd != active && !IsIconic(hwnd) && WindowName(hwnd) == name)
                res.Add(hwnd);
            return true;
        }, IntPtr.Zero);
        return res;
    }

    private unsafe static string WindowName(IntPtr hwnd)
    {
        int size = GetWindowTextLengthW(hwnd);
        if (size <= 0)
            return "";

        var buffer = new char[size + 1];
        fixed (char* pbuf = &buffer[0])
        {
            size = GetWindowTextW(hwnd, pbuf, size + 1);
            return new(pbuf);
        }
    }

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [LibraryImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [LibraryImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(IntPtr hWnd);

    [LibraryImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static unsafe partial int GetWindowTextW(IntPtr hWnd, char* strText, int maxCount);

    [LibraryImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial int GetWindowTextLengthW(IntPtr hWnd);

    [LibraryImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial IntPtr GetActiveWindow();

    [LibraryImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool PostMessageW(IntPtr hWnd, uint msg, ulong wparam, ulong lparam);

    [LibraryImport("user32.dll"), DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static partial short GetKeyState(int keyCode);
}
