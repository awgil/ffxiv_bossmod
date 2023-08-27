using Dalamud.Game.ClientState.Keys;
using ImGuiNET;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BossMod
{
    [StructLayout(LayoutKind.Explicit, Size = 0xB)]
    public unsafe struct InputDataKeybind
    {
        [FieldOffset(0)] public fixed ushort Bindings[5];
        [FieldOffset(10)] public byte Terminator;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x9C8)]
    public unsafe struct InputData
    {
        [FieldOffset(0)] public void** Vtbl;

        [FieldOffset(0x9AC)] public int KeybindCount;
        [FieldOffset(0x9B0)] public InputDataKeybind* Keybinds;
        [FieldOffset(0x9B8)] public fixed byte GamepadAxisRemap[7];
    }

    unsafe class DebugInput : IDisposable
    {
        private delegate byte ConvertVirtualKeyDelegate(int vkCode);
        private ConvertVirtualKeyDelegate _convertVirtualKey;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        private UITree _tree = new();
        private WorldState _ws;
        private AI.AIController _navi;
        private Vector2 _dest;
        private WPos _prevPos;
        private bool _jump;
        private DateTime _prevFrame;
        private bool _gamepadAxisOverrideEnable;
        private float _gamepadAxisOverrideAngle;
        private int _gamepadButtonOverride = -1;
        private int _gamepadButtonValue;
        private bool _gamepadNavigate;

        public DebugInput(Autorotation autorot)
        {
            _convertVirtualKey = Service.KeyState.GetType().GetMethod("ConvertVirtualKey", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<ConvertVirtualKeyDelegate>(Service.KeyState);
            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
            _ws = autorot.WorldState;
            _navi = new();
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
            var curFrame = DateTime.Now;
            var dt = (curFrame - _prevFrame).TotalSeconds;
            _prevFrame = curFrame;

            var player = _ws.Party.Player();
            var curPos = player?.Position ?? new();
            ImGui.TextUnformatted($"Speed: {(curPos - _prevPos).Length() / dt:f2}");
            _prevPos = curPos;

            ImGui.Checkbox("Jump!", ref _jump);
            ImGui.InputFloat2("Destination", ref _dest);
            ImGui.SameLine();
            if (ImGui.Button("Move!"))
            {
                _navi.NaviTargetPos = new(_dest);

                //var toTarget = _navi.NaviTargetPos.Value - curPos;
                //_navi.NaviTargetRot = toTarget.Normalized();

                //var cameraFacing = _navi.CameraFacing;
                //var dot = cameraFacing.Dot(_navi.NaviTargetRot.Value);
                //if (dot < -0.707107f)
                //    _navi.NaviTargetRot = -_navi.NaviTargetRot.Value;
                //else if (dot < 0.707107f)
                //    _navi.NaviTargetRot = cameraFacing.OrthoL().Dot(_navi.NaviTargetRot.Value) > 0 ? _navi.NaviTargetRot.Value.OrthoR() : _navi.NaviTargetRot.Value.OrthoL();

                _navi.WantJump = _jump;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel move"))
            {
                _navi.Clear();
            }
            _navi.Update(player);

            DrawGamepad();

            foreach (var n in _tree.Node("Input data"))
            {
                var idata = GetInputData();
                foreach (var n2 in _tree.Node($"Keybinds ({idata->KeybindCount} total)"))
                {
                    var mapping = new VirtualKey[256];
                    foreach (var vk in Service.KeyState.GetValidVirtualKeys())
                        mapping[_convertVirtualKey((int)vk)] = vk;

                    Func<byte, string> bindString = v => v switch
                    {
                        0 => "---",
                        < 0xA0 => mapping[v].ToString(),
                        < 0xA7 => $"mouse{v - 0xA0}",
                        _ => $"gamepad{v-0xA7}"
                    };
                    Func<ushort, string> printBinding = v => $"{((v & 0x100) != 0 ? "shift+" : "")}{((v & 0x200) != 0 ? "ctrl+" : "")}{((v & 0x400) != 0 ? "alt+" : "")}{((v & 0xF800) != 0 ? "?+" : "")}{bindString((byte)v)} ({v:X4})";
                    for (int i = 0; i < idata->KeybindCount; ++i)
                    {
                        _tree.LeafNode($"{i} = {string.Join(", ", Enumerable.Range(0, 5).Select(j => printBinding(idata->Keybinds[i].Bindings[j])))}");
                    }
                }
            }

            foreach (var n in _tree.Node("Virtual keys"))
            {
                _tree.LeafNodes(Service.KeyState.GetValidVirtualKeys(), vk => $"{vk} ({(int)vk}): internal code={_convertVirtualKey((int)vk)}, state={_getKeyRef((int)vk)}");
            }
        }

        private void DrawGamepad()
        {
            ImGui.SliderFloat("Gamepad angle", ref _gamepadAxisOverrideAngle, -180, 180);
            ImGui.SameLine();
            ImGui.Checkbox("Gamepad move", ref _gamepadAxisOverrideEnable);

            ImGui.SliderInt("Gamepad value", ref _gamepadButtonValue, -100, 100);
            ImGui.SameLine();
            ImGui.InputInt("Gamepad button", ref _gamepadButtonOverride);

            ImGui.Checkbox("Gamepad navigate", ref _gamepadNavigate);

            var input = ActionManagerEx.Instance!.InputOverride;
            Array.Fill(input.GamepadOverrides, 0);
            if (_gamepadButtonOverride >= 0 && _gamepadButtonOverride < input.GamepadOverrides.Length)
            {
                input.GamepadOverridesEnabled = true;
                input.GamepadOverrides[_gamepadButtonOverride] = _gamepadButtonValue;
            }
            else if (_gamepadAxisOverrideEnable)
            {
                input.GamepadOverridesEnabled = true;
                input.GamepadOverrides[3] = (int)(100 * _gamepadAxisOverrideAngle.Degrees().Sin());
                input.GamepadOverrides[4] = (int)(100 * _gamepadAxisOverrideAngle.Degrees().Cos());
            }
            else if (_gamepadNavigate)
            {
                var dest = new WPos(_dest);
                var dir = (Camera.Instance?.CameraAzimuth ?? 0).Radians() - Angle.FromDirection(dest - (_ws.Party.Player()?.Position ?? dest)) + 180.Degrees();
                input.GamepadOverridesEnabled = true;
                input.GamepadOverrides[3] = (int)(100 * dir.Sin());
                input.GamepadOverrides[4] = (int)(100 * dir.Cos());
            }
            else
            {
                input.GamepadOverridesEnabled = false;
            }
        }

        private InputData* GetInputData() => (InputData*)FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetUIInputData();
    }
}
