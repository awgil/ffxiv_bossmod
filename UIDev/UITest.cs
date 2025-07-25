﻿using BossMod;
using Dalamud.Interface.Utility;
using ImGuiNET;
using ImGuiScene;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using static SDL2.SDL;

namespace UIDev;

class UITest
{
    public static void Main(string[] args)
    {
        var windowInfo = new WindowCreateInfo()
        {
            Title = "UI Test",
            XPos = -10,
            //YPos = 20,
            Fullscreen = true,
            TransparentColor = [0, 0, 0],
        };

        if (args.Length > 0 && args[0] == "-w" && false)
        {
            // windowed mode
            windowInfo.XPos = 100;
            windowInfo.YPos = -2000;
            windowInfo.Width = 1200;
            windowInfo.Height = 800;
            windowInfo.Fullscreen = false;
            windowInfo.TransparentColor = null;

            if (SDL_Init(SDL_INIT_VIDEO) == 0 && SDL_GetDesktopDisplayMode(0, out var mode) >= 0)
            {
                windowInfo.Width = mode.w - 200;
                windowInfo.Height = mode.h - 200;
                SDL_Quit();
            }
        }

        using var scene = new SimpleImGuiScene(RendererFactory.RendererBackend.DirectX11, windowInfo);
        scene.Renderer.ClearColor = new Vector4(0, 0, 0, 0);

        InitDalamud();

        Service.LogHandlerDebug = msg => Debug.WriteLine(msg);
        Service.LogHandlerVerbose = msg => Debug.WriteLine(msg);
        Service.LuminaGameData = new(FindGameDataPath());
        Service.LuminaGameData.Options.PanicOnSheetChecksumMismatch = false; // TODO: remove - temporary workaround until lumina is updated
        Service.LuminaGameData.Options.RsvResolver = Service.LuminaRSV.TryGetValue;
        Service.WindowSystem = new("uitest");
        typeof(Service).GetProperty("Texture")!.SetValue(null, new OfflineTextureProvider(scene.Renderer));
        //Service.Device = (SharpDX.Direct3D11.Device?)scene.Renderer.GetType().GetField("_device", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(scene.Renderer);

        // esc should close focused window
        bool escDown = false;
        scene.Window.OnSDLEvent += (ref SDL_Event sdlEvent) =>
        {
            if (sdlEvent.type == SDL_EventType.SDL_KEYDOWN && sdlEvent.key.keysym.scancode == SDL_Scancode.SDL_SCANCODE_ESCAPE && !escDown)
            {
                escDown = true;
                var focusWindow = Service.WindowSystem.HasAnyFocus ? Service.WindowSystem.Windows.FirstOrDefault(w => w.IsFocused && w.RespectCloseHotkey) : null;
                if (focusWindow != null)
                    focusWindow.IsOpen = false;
            }
            else if (sdlEvent.type == SDL_EventType.SDL_KEYUP && sdlEvent.key.keysym.scancode == SDL_Scancode.SDL_SCANCODE_ESCAPE)
            {
                escDown = false;
            }
        };

        var newFrame = (Action)Delegate.CreateDelegate(typeof(Action), typeof(ImGuiHelpers).GetMethod("NewFrame", BindingFlags.Static | BindingFlags.NonPublic)!);
        scene.OnBuildUI += () =>
        {
            // this hack is needed to ensure we use correct global scale
            newFrame.Invoke();

            // dalamud trying to draw a fadeout effect causes a deadlock in uidev as the TextureManager service isn't present
            foreach (var w in Service.WindowSystem.Windows)
                w.DisableFadeInFadeOut = true;

            Service.WindowSystem.Draw();
        };

        var pluginConfigs = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "pluginConfigs");
        var configPath = Path.Join(pluginConfigs, "BossMod.json");
        var rotationRoot = Path.Join(pluginConfigs, "BossMod", "autorot");
        var mainWindow = new UITestWindow(scene, configPath, rotationRoot)
        {
            IsOpen = true
        };
        scene.Run();
        mainWindow.Dispose();
        ActionDefinitions.Instance.Dispose();
    }

    private static unsafe void InitDalamud()
    {
        // we have to manually provide a singleton instance of ServiceContainer, otherwise WindowSystem.Draw() will deadlock
        var dala = Assembly.GetAssembly(typeof(Dalamud.Game.ActionKind))!;
        var wrapper = dala.GetType("Dalamud.Service`1[Dalamud.IoC.Internal.ServiceContainer]")!;
        var cont = dala.GetType("Dalamud.IoC.Internal.ServiceContainer")!;

        var provideFn = wrapper.GetMethod("Provide", BindingFlags.Static | BindingFlags.Public);
        provideFn!.Invoke(null, [Activator.CreateInstance(cont)]);
        // provideFn!.Invoke(null, [Activator.CreateInstance(texManager)]);

        // all of this is taken straight from dalamud
        ImFontConfigPtr fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
        fontConfig.MergeMode = true;
        fontConfig.PixelSnapH = true;

        var fontPathJp = "NotoSansCJKjp-Medium.otf";
        ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPathJp, 17.0f, null, ImGui.GetIO().Fonts.GetGlyphRangesJapanese());

        var fontPathGame = "gamesym.ttf";
        var rangeHandle = GCHandle.Alloc(new ushort[] { 0xE020, 0xE0DB, 0 }, GCHandleType.Pinned);
        ImGui.GetIO().Fonts.AddFontFromFileTTF(fontPathGame, 17.0f, fontConfig, rangeHandle.AddrOfPinnedObject());

        ImGui.GetIO().Fonts.Build();

        fontConfig.Destroy();
        rangeHandle.Free();

        ImGui.GetStyle().GrabRounding = 3f;
        ImGui.GetStyle().FrameRounding = 4f;
        ImGui.GetStyle().WindowRounding = 4f;
        ImGui.GetStyle().WindowBorderSize = 0f;
        ImGui.GetStyle().WindowMenuButtonPosition = ImGuiDir.Right;
        ImGui.GetStyle().ScrollbarSize = 16f;

        ImGui.GetStyle().Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.06f, 0.06f, 0.06f, 0.87f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.29f, 0.29f, 0.29f, 0.54f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.54f, 0.54f, 0.54f, 0.40f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.64f, 0.64f, 0.64f, 0.67f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.29f, 0.29f, 0.29f, 1.00f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.86f, 0.86f, 0.86f, 1.00f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.54f, 0.54f, 0.54f, 1.00f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.67f, 0.67f, 0.67f, 1.00f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.Button] = new Vector4(0.71f, 0.71f, 0.71f, 0.40f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.47f, 0.47f, 0.47f, 1.00f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.74f, 0.74f, 0.74f, 1.00f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.Header] = new Vector4(0.59f, 0.59f, 0.59f, 0.31f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.50f, 0.50f, 0.50f, 0.80f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.60f, 0.60f, 0.60f, 1.00f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.79f, 0.79f, 0.79f, 0.25f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.78f, 0.78f, 0.78f, 0.67f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.88f, 0.88f, 0.88f, 0.95f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.Tab] = new Vector4(0.23f, 0.23f, 0.23f, 0.86f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.71f, 0.71f, 0.71f, 0.80f);
        ImGui.GetStyle().Colors[(int)ImGuiCol.TabActive] = new Vector4(0.36f, 0.36f, 0.36f, 1.00f);
        // end dalamud copy
    }

    private static string FindGameDataPath()
    {
        if (Environment.GetEnvironmentVariable("FFXIV_GAME_FOLDER") is string gameFolderOverride)
            return Path.Join(gameFolderOverride, "game", "sqpack");

        // stolen from FFXIVLauncher/src/XIVLauncher/AppUtil.cs
        foreach (var registryView in new RegistryView[] { RegistryView.Registry32, RegistryView.Registry64 })
        {
            using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                // Should return "C:\Program Files (x86)\SquareEnix\FINAL FANTASY XIV - A Realm Reborn\boot\ffxivboot.exe" if installed with default options.
                using (var subkey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{2B41E132-07DF-4925-A3D3-F2D1765CCDFE}"))
                {
                    if (subkey != null && subkey.GetValue("DisplayIcon", null) is string path)
                    {
                        // DisplayIcon includes "boot\ffxivboot.exe", need to remove it
                        var basePath = Directory.GetParent(path)?.Parent?.FullName;
                        if (basePath != null)
                        {
                            var dataPath = Path.Join(basePath, "game", "sqpack");
                            if (Directory.Exists(dataPath))
                            {
                                return dataPath;
                            }
                        }
                    }
                }

                // Should return "C:\Program Files (x86)\Steam\steamapps\common\game Online" if installed with default options.
                Span<int> validSteamAppIds = [39210, 312060];
                foreach (var steamAppId in validSteamAppIds)
                {
                    using (var subkey = hklm.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App {steamAppId}"))
                    {
                        if (subkey != null && subkey.GetValue("InstallLocation", null) is string path)
                        {
                            // InstallLocation is the root path of the game (the one containing boot and game) itself
                            var dataPath = Path.Join(path, "game", "sqpack");
                            if (Directory.Exists(dataPath))
                            {
                                return dataPath;
                            }
                        }
                    }
                }
            }
        }

        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "SquareEnix\\FINAL FANTASY XIV - A Realm Reborn\\game\\sqpack");
    }
}
