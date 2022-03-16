using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod
{
    public class WindowManager
    {
        public class Window
        {
            private Action _draw;
            private Action _close;
            private Func<bool> _userClose;

            public Vector2 SizeHint = new(300, 300);
            public Vector2 MinSize = new(50, 50);
            public ImGuiWindowFlags Flags = ImGuiWindowFlags.None;
            public string Name;
            public string? Title;

            internal Window(string name, Action draw, Action close, Func<bool> userClose)
            {
                Name = name;
                _draw = draw;
                _close = close;
                _userClose = userClose;
            }

            internal bool Draw()
            {
                var visible = true;
                ImGui.SetNextWindowSize(SizeHint, ImGuiCond.FirstUseEver);
                ImGui.SetNextWindowSizeConstraints(MinSize, new Vector2(float.MaxValue, float.MaxValue));
                if (ImGui.Begin(Title != null ? $"{Title}###{Name}" : Name, ref visible, Flags))
                {
                    _draw();
                }
                ImGui.End();

                if (!visible)
                    visible = !_userClose();
                return visible;
            }

            internal void Reset()
            {
                _close();
            }
        }
        private static List<Window> _windows = new();
        private static int _nextDrawIndex = -1; // this is used to allow closing window during draw

        public static Window CreateWindow(string name, Action draw, Action close, Func<bool> userClose)
        {
            var w = _windows.Find(x => x.Name == name);
            if (w == null)
            {
                w = new Window(name, draw, close, userClose);
                _windows.Add(w);
            }
            else
            {
                close(); // window was never created, so clean up any resources...
            }
            ImGui.SetWindowFocus(w.Name);
            return w;
        }

        public static void CloseWindow(Window w)
        {
            int index = _windows.IndexOf(w);
            if (index < 0)
                return; // window not found

            w.Reset();
            if (index < _nextDrawIndex)
                --_nextDrawIndex;
            _windows.RemoveAt(index);
        }

        public static void DrawAll()
        {
            _nextDrawIndex = 0;
            while (_nextDrawIndex < _windows.Count)
            {
                var w = _windows[_nextDrawIndex++];
                if (!w.Draw())
                {
                    CloseWindow(w);
                }
            }
            _nextDrawIndex = -1;
        }

        public static void Reset()
        {
            // note that window's close can potentially close another window...
            while (_windows.Count > 0)
                CloseWindow(_windows.First());
        }
    }
}
