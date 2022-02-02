using Dalamud.Hooking;
using ImGuiNET;
using System;

namespace BossMod
{
    class Autorotation : IDisposable
    {
        private GeneralConfig _config;
        private WindowManager.Window? _ui;

        private delegate ulong GetAdjustedActionIdDelegate(byte param1, uint param2);
        private Hook<GetAdjustedActionIdDelegate> _getAdjustedActionIdHook;
        private unsafe float* _comboTimeLeft = null;
        private unsafe WARRotation.AID* _comboLastMove = null;

        public unsafe float ComboTimeLeft => *_comboTimeLeft;
        public unsafe WARRotation.AID ComboLastMove => *_comboLastMove;

        public WARActions WarActions { get; init; } = new();

        public unsafe Autorotation(GeneralConfig config)
        {
            _config = config;

            IntPtr comboPtr = Service.SigScanner.GetStaticAddressFromSig("E8 ?? ?? ?? ?? 80 7E 21 00", 0x178);
            _comboTimeLeft = (float*)comboPtr;
            _comboLastMove = (WARRotation.AID*)(comboPtr + 0x4);

            var getAdjustedActionIdAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 8B F8 3B DF");
            _getAdjustedActionIdHook = new(getAdjustedActionIdAddress, new GetAdjustedActionIdDelegate(GetAdjustedActionIdDetour));
            _getAdjustedActionIdHook.Enable();
        }

        public void Dispose()
        {
            _getAdjustedActionIdHook.Dispose();
        }

        public void Update()
        {
            bool enabled = false;
            if (_config.AutorotationEnabled)
            {
                enabled = (Service.ClientState.LocalPlayer?.ClassJob.Id ?? 0) == 21; // 21 is WAR
            }

            if (enabled)
            {
                if (!_getAdjustedActionIdHook.IsEnabled)
                    _getAdjustedActionIdHook.Enable();
                WarActions.Update(ComboLastMove, ComboTimeLeft);
            }
            else
            {
                if (_getAdjustedActionIdHook.IsEnabled)
                    _getAdjustedActionIdHook.Disable();
            }

            bool showUI = enabled && _config.AutorotationShowUI;
            if (showUI && _ui == null)
            {
                _ui = WindowManager.CreateWindow("Autorotation", () => WarActions.DrawActionHint(false), () => { });
                _ui.SizeHint = new(100, 100);
                _ui.MinSize = new(100, 100);
                _ui.Flags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;
            }
            else if (!showUI && _ui != null)
            {
                _ui?.Close();
                _ui = null;
            }
        }

        private ulong GetAdjustedActionIdDetour(byte self, uint actionID)
        {
            if (Service.ClientState.LocalPlayer == null)
                return _getAdjustedActionIdHook.Original(self, actionID);

            switch (actionID)
            {
                case (uint)WARRotation.AID.HeavySwing:
                    return (uint)WarActions.NextBestAction;
                case (uint)WARRotation.AID.StormEye:
                    return (uint)WARRotation.GetNextStormEyeComboAction(WarActions.State);
                case (uint)WARRotation.AID.StormPath:
                    return (uint)WARRotation.GetNextStormPathComboAction(WarActions.State);
                case (uint)WARRotation.AID.MythrilTempest:
                    return (uint)WARRotation.GetNextAOEComboAction(WarActions.State);
                default:
                    return _getAdjustedActionIdHook.Original(self, actionID);
            }
        }
    }
}
