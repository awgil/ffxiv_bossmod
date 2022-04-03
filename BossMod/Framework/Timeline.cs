using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    // utility for drawing time-related data
    public class Timeline
    {
        private float _pixelsPerSecond = 10;
        private float _tScroll = 0;
        private float _height = 0;
        private float _tickFrequency = 5;
        private float _topMargin = 20;
        private float _timeAxisWidth = 35;

        public float Height => _height;
        public float PixelsPerSecond => _pixelsPerSecond;

        // has to be called just before Begin()/End()
        public void DrawHeader(float hOffsetCenter, string text, uint color)
        {
            var cursor = ImGui.GetCursorScreenPos();
            var s = ImGui.CalcTextSize(text);
            ImGui.GetWindowDrawList().AddText(cursor + new Vector2(_timeAxisWidth + hOffsetCenter - s.X / 2, 0), color, text);
        }

        // returns top-left corner of the client area
        public Vector2 Begin(float clientAreaWidth, float bottomMargin, float maxTime, float? currentTime = null)
        {
            var wsize = ImGui.GetWindowSize();
            var cursor = ImGui.GetCursorScreenPos();
            float w = _timeAxisWidth + clientAreaWidth;
            _height = MathF.Max(10, wsize.Y - cursor.Y - _topMargin - bottomMargin - 8);
            ImGui.SetCursorPosY(cursor.Y + _topMargin);
            ImGui.InvisibleButton("canvas", new(w, _height), ImGuiButtonFlags.MouseButtonLeft | ImGuiButtonFlags.MouseButtonRight);

            cursor += new Vector2(_timeAxisWidth, _topMargin);

            // scroll by wheel, zoom by shift+wheel
            if (ImGui.IsItemHovered() && ImGui.GetIO().MouseWheel != 0)
            {
                if (ImGui.GetIO().KeyShift)
                {
                    var cursorT = TimeFromScreen(cursor, ImGui.GetMousePos());
                    _pixelsPerSecond *= MathF.Pow(1.05f, ImGui.GetIO().MouseWheel);
                    _tScroll += cursorT - TimeFromScreen(cursor, ImGui.GetMousePos());

                    _tickFrequency = 5;
                    while (_tickFrequency < 60 && _pixelsPerSecond * _tickFrequency < 30)
                        _tickFrequency *= 2;
                    while (_tickFrequency > 1 && _pixelsPerSecond * _tickFrequency > 55)
                        _tickFrequency = MathF.Floor(_tickFrequency * 0.5f);
                    while (_tickFrequency > 0.1f && _pixelsPerSecond * _tickFrequency > 55)
                        _tickFrequency = MathF.Floor(_tickFrequency * 5) * 0.1f;
                }
                else
                {
                    _tScroll -= 10 * ImGui.GetIO().MouseWheel;
                }
            }

            _tScroll = MathF.Min(_tScroll, maxTime - _height / _pixelsPerSecond);
            _tScroll = MathF.Max(_tScroll, 0);
            DrawTimeAxis(cursor, maxTime, currentTime);
            ImGui.PushClipRect(cursor, cursor + new Vector2(clientAreaWidth, _height), true);
            return cursor;
        }

        public void End()
        {
            ImGui.PopClipRect();
        }

        public Vector2 ToScreenCoords(Vector2 screenTL, float hOffset, float t)
        {
            return screenTL + new Vector2(hOffset, (t - _tScroll) * _pixelsPerSecond);
        }

        public float TimeFromScreen(Vector2 screenTL, Vector2 screenCoord)
        {
            return (screenCoord.Y - screenTL.Y) / _pixelsPerSecond + _tScroll;
        }

        private void DrawTimeAxis(Vector2 screenZero, float maxTime, float? currentTime)
        {
            var maxT = Math.Min(maxTime, _tScroll + _height / _pixelsPerSecond);
            var drawlist = ImGui.GetWindowDrawList();
            drawlist.AddLine(screenZero, ToScreenCoords(screenZero, 0, maxT), 0xffffffff);
            for (float t = MathF.Ceiling(_tScroll / _tickFrequency) * _tickFrequency; t <= maxT; t += _tickFrequency)
            {
                string tickText = $"{t:f1}";
                var tickTextSize = ImGui.CalcTextSize(tickText);

                var p = ToScreenCoords(screenZero, 0, t);
                drawlist.AddLine(p, p - new Vector2(3, 0), 0xffffffff);
                drawlist.AddText(p - new Vector2(tickTextSize.X + 5, tickTextSize.Y / 2), 0xffffffff, tickText);
            }

            if (currentTime != null && currentTime.Value >= _tScroll && currentTime.Value <= maxT)
            {
                // draw timeline mark
                var p = ToScreenCoords(screenZero, 0, currentTime.Value);
                drawlist.AddTriangleFilled(p, p - new Vector2(4, 2), p - new Vector2(4, -2), 0xffffffff);
            }
        }

    }
}
