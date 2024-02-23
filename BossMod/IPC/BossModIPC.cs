using Dalamud.Plugin;

namespace BossMod.IPC;
internal class BossModIPC
{
    public bool Moving;
    private Autorotation _autorot;

    public BossModIPC(DalamudPluginInterface pluginInterface, Autorotation autorotation)
    {
        _autorot = autorotation;

        Service.PluginInterface.GetIpcProvider<bool>($"{nameof(BossMod)}.IsAIMoving").RegisterFunc(CheckIsAIMoving);
        Service.PluginInterface.GetIpcProvider<object>($"{nameof(BossMod)}.InitiateCombat").RegisterAction(InitiateCombat);
        Service.PluginInterface.GetIpcProvider<bool, object>($"{nameof(BossMod)}.ToggleAutoRotation").RegisterAction(ToggleAutoRotation);
    }

    private bool CheckIsAIMoving() => Moving;
    private void InitiateCombat() => _autorot.ClassActions?.UpdateAutoAction(CommonActions.AutoActionAIFight, float.MaxValue, false);
    private void ToggleAutoRotation(bool state) => Service.Config.Get<AutorotationConfig>().Enabled = state;

    public void Dispose()
    {
        Service.PluginInterface.GetIpcProvider<bool>($"{nameof(BossMod)}.IsAIMoving").UnregisterFunc();
        Service.PluginInterface.GetIpcProvider<object>($"{nameof(BossMod)}.InitiateCombat").UnregisterAction();
        Service.PluginInterface.GetIpcProvider<bool, object>($"{nameof(BossMod)}.ToggleAutoRotation").UnregisterAction();
    }
}
