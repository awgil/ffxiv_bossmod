using ImGuiNET;
using ImGuiScene;
using BossMod;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace UIDev
{
    class UITest : IPluginUIMock
    {
        public static void Main(string[] args)
        {
            UIBootstrap.Inititalize(new UITest());
        }

        private SimpleImGuiScene? _scene;
        private List<Type> _testTypes = new();
        private string _path = "";
        private string _configPath = "";
        private bool _configModified;

        public void Initialize(SimpleImGuiScene scene)
        {
            _configPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "XIVLauncher", "pluginConfigs", "BossMod.json");

            Service.LogHandler = msg => Debug.WriteLine(msg);
            Service.LuminaGameData = new(FindGameDataPath());
            Service.LuminaGameData.Options.PanicOnSheetChecksumMismatch = false; // TODO: remove - temporary workaround until lumina is updated
            Service.Config.Initialize();
            Service.Config.LoadFromFile(new(_configPath));
            Service.Config.Modified += (_, _) => _configModified = true;
            //Service.Device = (SharpDX.Direct3D11.Device?)scene.Renderer.GetType().GetField("_device", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(scene.Renderer);

            // scene is a little different from what you have access to in dalamud
            // but it can accomplish the same things, and is really only used for initial setup here

            scene.OnBuildUI += WindowManager.DrawAll;

            // saving this only so we can kill the test application by closing the window
            // (instead of just by hitting escape)
            _scene = scene;

            var testType = typeof(ITest);
            foreach (var t in testType.Assembly.GetTypes())
            {
                if (t != testType && testType.IsAssignableFrom(t))
                {
                    _testTypes.Add(t);
                }
            }

            WindowManager.CreateWindow("Boss mod UI development", DrawMainWindow, () => scene.ShouldQuit = true, () => true);
        }

        public void Dispose()
        {
        }

        private void DrawMainWindow()
        {
            ImGui.InputText("Config", ref _configPath, 500);
            ImGui.SameLine();
            if (ImGui.Button("Reload"))
            {
                Service.Config.LoadFromFile(new(_configPath));
                _configModified = false;
            }
            ImGui.SameLine();
            if (ImGui.Button(_configModified ? "Save (modified)" : "Save (no changes)"))
            {
                Service.Config.SaveToFile(new(_configPath));
                _configModified = false;
            }

            ImGui.Separator();

            ImGui.InputText("Path", ref _path, 500);
            if (ImGui.Button("Open native log..."))
            {
                var data = ReplayParserLog.Parse(_path);
                if (data.Ops.Count > 0)
                {
                    var visu = new ReplayVisualizer(data);
                    var w = WindowManager.CreateWindow($"Native log: {_path}", visu.Draw, visu.Dispose, () => true);
                    w.SizeHint = new(1000, 1000);
                }
            }
            //ImGui.SameLine();
            //if (ImGui.Button("Open ACT log..."))
            //{
            //    var data = ReplayParserAct.Parse(_path, 0);
            //    if (data.Ops.Count > 0)
            //    {
            //        var visu = new ReplayVisualizer(data);
            //        WindowManager.CreateWindow($"ACT log: {_path}", visu.Draw, visu.Dispose, () => true);
            //    }
            //}
            ImGui.SameLine();
            if (ImGui.Button("Analyze all logs..."))
            {
                var a = new AnalysisManager(_path);
                WindowManager.CreateWindow($"Multiple logs: {_path}", a.Draw, a.Dispose, () => true);
            }

            foreach (var t in _testTypes)
            {
                if (ImGui.Button($"Show {t}"))
                {
                    var inst = (ITest?)Activator.CreateInstance(t);
                    if (inst != null)
                    {
                        var window = WindowManager.CreateWindow(t.ToString(), inst.Draw, inst.Dispose, () => true);
                        window.Flags = inst.WindowFlags();
                    }
                }
            }
        }

        private string FindGameDataPath()
        {
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
                }
            }
            return "D:\\installed\\SquareEnix\\FINAL FANTASY XIV - A Realm Reborn\\game\\sqpack";
        }
    }
}
