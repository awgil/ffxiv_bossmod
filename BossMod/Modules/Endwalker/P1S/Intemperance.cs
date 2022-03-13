using ImGuiNET;
using System;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.P1S
{
    using static BossModule;

    // state related to intemperance mechanic
    // TODO: improve, it's now not really providing any useful hints...
    class Intemperance : Component
    {
        public enum State { Unknown, TopToBottom, BottomToTop }
        public enum Cube { None, R, B, P }

        public State CurState = State.Unknown;
        public int NumExplosions { get; private set; } = 0;
        private P1S _module;
        private Cube[] _cubes = new Cube[24]; // [3*i+j] corresponds to cell i [NW N NE E SE S SW W], cube j [bottom center top]

        private static float _cellHalfSize = 6;

        private static Cube[] _patternSymm = {
            Cube.R, Cube.P, Cube.R,
            Cube.B, Cube.R, Cube.B,
            Cube.R, Cube.P, Cube.R,
            Cube.R, Cube.P, Cube.B,
            Cube.R, Cube.P, Cube.R,
            Cube.B, Cube.B, Cube.B,
            Cube.R, Cube.P, Cube.R,
            Cube.R, Cube.P, Cube.B,
        };
        private static Cube[] _patternAsymm = {
            Cube.B, Cube.P, Cube.R,
            Cube.R, Cube.R, Cube.B,
            Cube.B, Cube.P, Cube.R,
            Cube.R, Cube.P, Cube.R,
            Cube.B, Cube.P, Cube.R,
            Cube.R, Cube.B, Cube.B,
            Cube.B, Cube.P, Cube.R,
            Cube.R, Cube.P, Cube.R,
        };
        private static Vector2[] _offsets = { new(-1, -1), new(0, -1), new(1, -1), new(1, 0), new(1, 1), new(0, 1), new(-1, 1), new(-1, 0) };

        public Intemperance(P1S module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var pat = _cubes.SequenceEqual(_patternSymm) ? "symmetrical" : (_cubes.SequenceEqual(_patternAsymm) ? "asymmetrical" : "unknown");
            hints.Add($"Order: {CurState}, pattern: {pat}.", false);
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            // draw cell delimiters
            Vector3 v1 = new(_cellHalfSize + 1, 0, arena.WorldHalfSize);
            Vector3 v2 = new(-_cellHalfSize, 0, arena.WorldHalfSize);
            arena.ZoneRect(arena.WorldCenter - v1, arena.WorldCenter + v2, arena.ColorAOE);
            arena.ZoneRect(arena.WorldCenter - v2, arena.WorldCenter + v1, arena.ColorAOE);

            v1 = new(v1.Z, 0, v1.X);
            v2 = new(v2.Z, 0, v2.X);
            arena.ZoneRect(arena.WorldCenter - v1, arena.WorldCenter + v2, arena.ColorAOE);
            arena.ZoneRect(arena.WorldCenter - v2, arena.WorldCenter + v1, arena.ColorAOE);

            // draw cubes on margins
            var drawlist = ImGui.GetWindowDrawList();
            var marginOffset = arena.ScreenHalfSize + arena.ScreenMarginSize / 2;
            for (int i = 0; i < 8; ++i)
            {
                var center = arena.ScreenCenter + arena.RotatedCoords(_offsets[i] * marginOffset);
                DrawTinyCube(drawlist, center + new Vector2(-3,  5), _cubes[3 * i + 0]);
                DrawTinyCube(drawlist, center + new Vector2(-3,  0), _cubes[3 * i + 1]);
                DrawTinyCube(drawlist, center + new Vector2(-3, -5), _cubes[3 * i + 2]);

                drawlist.AddLine(center + new Vector2(4, -7), center + new Vector2(4, 7), 0xffffffff);
                if (CurState != State.Unknown)
                {
                    float lineDir = CurState == State.BottomToTop ? -1 : 1;
                    drawlist.AddLine(center + new Vector2(4, 7 * lineDir), center + new Vector2(2, 5 * lineDir), 0xffffffff);
                    drawlist.AddLine(center + new Vector2(4, 7 * lineDir), center + new Vector2(6, 5 * lineDir), 0xffffffff);
                }
            }
        }

        public override void OnEventCast(CastEvent info)
        {
            if (info.IsSpell(AID.PainfulFlux)) // this is convenient to rely on, since exactly 1 cast happens right after every explosion
                ++NumExplosions;
        }

        public override void OnEventEnvControl(uint featureID, byte index, uint state)
        {
            // we get the following env-control messages:
            // 1. ~2.8s after 26142 cast, we get 25 EnvControl messages with featureID 800375A0
            // 2. first 24 correspond to cubes, in groups of three (bottom->top), in order: NW N NE E SE S SW W
            //    the last one (index 26) can be ignored, probably corresponds to oneshot border
            //    state corresponds to cube type (00020001 for red, 00800040 for blue, 20001000 for purple)
            //    so asymmetrical pattern is: BPR RRB BPR RPR BPR RBB BPR RPR
            //    and symmetrical pattern is: RPR BRB RPR RPB RPR BBB RPR RPB
            // 3. on each explosion, we get 8 191s, with type 00080004 for exploded red, 04000004 for exploded blue, 08000004 for exploded purple
            // 4. 3 sec before second & third explosion, we get 8 191s, with type 00200020 for preparing red, 02000200 for preparing blue, 80008000 for preparing purple
            if (featureID == 0x800375A0 && index < 24)
            {
                switch (state)
                {
                    case 0x00020001:
                        _cubes[index] = Cube.R;
                        break;
                    case 0x00800040:
                        _cubes[index] = Cube.B;
                        break;
                    case 0x20001000:
                        _cubes[index] = Cube.P;
                        break;
                }
            }
        }

        private void DrawTinyCube(ImDrawListPtr drawlist, Vector2 center, Cube type)
        {
            Vector2 off = new(3);
            if (type != Cube.None)
            {
                uint col = type == Cube.R ? 0xff0000ff : (type == Cube.B ? 0xffff0000 : 0xffff00ff);
                drawlist.AddRectFilled(center - off, center + off, col);
            }
            drawlist.AddRect(center - off, center + off, 0xffffffff);
        }
    }
}
