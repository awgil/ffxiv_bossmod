using ImGuiNET;
using ImGuiScene;
using System.Numerics;
using BossMod;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

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
        private DateTime _startTime = DateTime.Now;
        private string _path = "";

        public void Initialize(SimpleImGuiScene scene)
        {
            Service.LogHandler = (string msg) => Debug.WriteLine(msg);
            Service.LuminaGameData = new("E:\\installed\\SquareEnix\\FINAL FANTASY XIV - A Realm Reborn\\game\\sqpack"); // TODO: unhardcode!!!
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
            ImGui.InputText("Path", ref _path, 500);
            if (ImGui.Button("Open native log..."))
            {
                var data = ReplayParserLog.Parse(_path);
                if (data.Ops.Count > 0)
                {
                    var visu = new ReplayVisualizer(data);
                    WindowManager.CreateWindow($"Native log: {_path}", visu.Draw, visu.Dispose, () => true);
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Open ACT log..."))
            {
                var data = ReplayParserAct.Parse(_path, 0);
                if (data.Ops.Count > 0)
                {
                    var visu = new ReplayVisualizer(data);
                    WindowManager.CreateWindow($"ACT log: {_path}", visu.Draw, visu.Dispose, () => true);
                }
            }
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
    }
}
