using ImGuiNET;
using ImGuiScene;
using BossMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace UIDev
{
    class UITestWindow : UIWindow
    {
        private SimpleImGuiScene _scene;
        private List<Type> _testTypes;
        private string _path = "";
        private string _configPath;

        // don't allow closing window by esc while there are any config modifications
        private bool _configModified
        {
            get => !RespectCloseHotkey;
            set => RespectCloseHotkey = !value;
        }

        public UITestWindow(SimpleImGuiScene scene, string configPath) : base("Boss mod UI development", false, new(600, 600))
        {
            _scene = scene;
            _testTypes = Utils.GetDerivedTypes<TestWindow>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract).ToList();
            _configPath = configPath;

            Service.Config.Initialize();
            Service.Config.LoadFromFile(new(configPath));
            Service.Config.Modified += (_, _) => _configModified = true;
        }

        public override void OnClose()
        {
            _scene.ShouldQuit = true;
        }

        public override void Draw()
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
                    new ReplayWindow(data);
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
                new UISimpleWindow($"Multiple logs: {_path}", new AnalysisManager(_path).Draw, true, new(1200, 800));
            }
            ImGui.SameLine();
            if (ImGui.Button("Convert to verbose"))
            {
                ConvertLog(_path, ReplayRecorderConfig.LogFormat.TextVerbose);
            }
            ImGui.SameLine();
            if (ImGui.Button("Convert to short text"))
            {
                ConvertLog(_path, ReplayRecorderConfig.LogFormat.TextCondensed);
            }
            ImGui.SameLine();
            if (ImGui.Button("Convert to uncompressed binary"))
            {
                ConvertLog(_path, ReplayRecorderConfig.LogFormat.BinaryUncompressed);
            }
            ImGui.SameLine();
            if (ImGui.Button("Convert to compressed binary"))
            {
                ConvertLog(_path, ReplayRecorderConfig.LogFormat.BinaryCompressed);
            }
            //ImGui.SameLine();
            //if (ImGui.Button("Raw decompress"))
            //{
            //    Stream inStream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
            //    inStream.Seek(4, SeekOrigin.Begin);
            //    inStream = new BrotliStream(inStream, CompressionMode.Decompress, false);
            //    var outStream = new FileStream($"{_path}.decompressed", FileMode.Create, FileAccess.Write, FileShare.Read);
            //    outStream.WriteByte((byte)'B');
            //    outStream.WriteByte((byte)'L');
            //    outStream.WriteByte((byte)'O');
            //    outStream.WriteByte((byte)'G');
            //    var buffer = new byte[65536];
            //    while (true)
            //    {
            //        var numRead = inStream.Read(buffer, 0, buffer.Length);
            //        if (numRead == 0)
            //            break;
            //        outStream.Write(buffer, 0, numRead);
            //    }
            //    inStream.Dispose();
            //    outStream.Dispose();
            //}

            foreach (var t in _testTypes)
            {
                if (ImGui.Button($"Show {t}"))
                {
                    Activator.CreateInstance(t);
                }
            }
        }

        private void ConvertLog(string input, ReplayRecorderConfig.LogFormat format)
        {
            var config = new ReplayRecorderConfig()
            {
                WorldLogFormat = format,
                DumpServerPackets = true,
                DumpClientPackets = true,
                TargetDirectory = new DirectoryInfo(input).Parent,
                LogPrefix = format.ToString(),
            };
            ReplayParserLog.Parse(_path, config);
        }
    }
}
