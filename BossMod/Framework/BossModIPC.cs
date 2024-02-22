using BossMod;

namespace BossMod
{
    internal class BossModIPC
    {
        internal static void Initialize()
        {
            Service.PluginInterface.GetIpcProvider<bool>("BossMod.IsMoving").RegisterFunc(IsMoving);
        }

        internal static void Dispose()
        {
            Service.PluginInterface.GetIpcProvider<bool>("BossMod.IsMoving").UnregisterFunc();
        }

        private static bool IsMoving()
        => ActionManagerEx.Instance!.InputOverride.IsMoving();
    }
}
