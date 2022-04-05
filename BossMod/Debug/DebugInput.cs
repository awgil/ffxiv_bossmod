using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BossMod
{
    class DebugInput
    {
        private delegate byte ConvertVirtualKeyDelegate(int vkCode);
        private ConvertVirtualKeyDelegate _convertVirtualKey;

        private delegate ref int GetRefValueDelegate(int vkCode);
        private GetRefValueDelegate _getKeyRef;

        public DebugInput()
        {
            _convertVirtualKey = Service.KeyState.GetType().GetMethod("ConvertVirtualKey", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<ConvertVirtualKeyDelegate>(Service.KeyState);
            _getKeyRef = Service.KeyState.GetType().GetMethod("GetRefValue", BindingFlags.NonPublic | BindingFlags.Instance)!.CreateDelegate<GetRefValueDelegate>(Service.KeyState);
        }

        public void Draw()
        {
            foreach (var vk in Service.KeyState.GetValidVirtualKeys())
            {
                ImGui.Text($"{vk} ({(int)vk}): internal code={_convertVirtualKey((int)vk)}, state={_getKeyRef((int)vk)}");
            }
        }
    }
}
