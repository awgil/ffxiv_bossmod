using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace BossMod
{
    public class WindowManager
    {
        public class Window
        {
            private Action _onDraw;
            private Action _onClose;
            private bool _wantClose;

            public bool Closed { get; private set; } = false;
            public Vector2 SizeHint = new(300, 300);
            public Vector2 MinSize = new(50, 50);
            public ImGuiWindowFlags Flags = ImGuiWindowFlags.None;
            public string Name = "";

            internal Window(string name, Action draw, Action close)
            {
                Name = name;
                _onDraw = draw;
                _onClose = close;
            }

            internal bool Draw()
            {
                var visible = !_wantClose;
                ImGui.SetNextWindowSize(SizeHint, ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSizeConstraints(MinSize, new Vector2(float.MaxValue, float.MaxValue));
                if (ImGui.Begin(Name, ref visible, Flags))
                {
                    _onDraw();
                }
                ImGui.End();

                if (!visible)
                {
                    _onClose();
                    Closed = true;
                }
                return visible;
            }

            internal void Reset()
            {
                _onClose();
            }

            public void Close()
            {
                _wantClose = true;
            }
        }
        private static List<Window> _windows = new();
        private static int _nextUniqueID = 0;

        public static Window CreateWindow(string name, Action draw, Action close, bool unique = true)
        {
            var w = unique ? _windows.Find(x => x.Name == name) : null;
            if (w == null)
            {
                w = new Window(unique ? name : $"{name}##{_nextUniqueID++}", draw, close);
                _windows.Add(w);
            }
            else
            {
                close(); // window was never created, so clean up any resources...
            }
            ImGui.SetWindowFocus(w.Name);
            return w;
        }

        public static void DrawAll()
        {
            for (int i = 0; i < _windows.Count; ++i)
            {
                if (!_windows[i].Draw())
                {
                    _windows.RemoveAt(i--);
                }
            }
        }

        public static void Reset()
        {
            foreach (var w in _windows)
                w.Reset();
            _windows.Clear();
            _nextUniqueID = 0;
        }
    }
}
