
namespace BossMod
{
    internal class BossModIPC
    {
        private static Autorotation _auto;

        internal unsafe static void Initialize(Autorotation autorotation)
        {
            Service.PluginInterface.GetIpcProvider<bool>("BossMod.IsMoving").RegisterFunc(IsMoving);
            Service.PluginInterface.GetIpcProvider<int>("BossMod.ForbiddenZonesCount").RegisterFunc(ForbiddenZonesCount);

            _auto = autorotation;
        }

        internal static void Dispose()
        {
            Service.PluginInterface.GetIpcProvider<bool>("BossMod.IsMoving").UnregisterFunc();
        }

        private static bool IsMoving()
        => ActionManagerEx.Instance!.InputOverride.IsMoving();

        private static int ForbiddenZonesCount()
        => _auto.Hints.ForbiddenZones.Count;
    }
}
