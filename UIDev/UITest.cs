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

        public void Initialize(SimpleImGuiScene scene)
        {
            Service.LogHandler = (string msg) => Debug.WriteLine(msg);
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

            WindowManager.CreateWindow("Boss mod UI development", DrawMainWindow, () => scene.ShouldQuit = true);
        }

        public void Dispose()
        {
        }

        private void DrawMainWindow()
        {
            ImGui.Text($"Running time: {DateTime.Now - _startTime}");

            foreach (var t in _testTypes)
            {
                if (ImGui.Button($"Show {t}"))
                {
                    var inst = (ITest?)Activator.CreateInstance(t);
                    if (inst != null)
                    {
                        var window = WindowManager.CreateWindow(t.ToString(), inst.Draw, inst.Dispose);
                        window.Flags = inst.WindowFlags();
                    }
                }
            }
        }

        //public void DrawSettingsWindow()
        //{
        //    if (!SettingsVisible)
        //    {
        //        return;
        //    }

        //    ImGui.SetNextWindowSize(new Vector2(232, 75), ImGuiCond.Always);
        //    if (ImGui.Begin("A Wonderful Configuration Window", ref this.settingsVisible,
        //        ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
        //    {
        //        if (ImGui.Checkbox("Random Config Bool", ref this.fakeConfigBool))
        //        {
        //            // nothing to do in a fake ui!
        //        }
        //    }
        //    ImGui.End();
        //}
    }
}
