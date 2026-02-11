using Dalamud.Game.Config;
using Dalamud.Plugin.Services;

namespace BossMod.Mock;

internal class MockGameConfig : IGameConfig
{
    public GameConfigSection System => throw new NotImplementedException();

    public GameConfigSection UiConfig => throw new NotImplementedException();

    public GameConfigSection UiControl => throw new NotImplementedException();

    public event EventHandler<ConfigChangeEvent> Changed = delegate { };
    public event EventHandler<ConfigChangeEvent> SystemChanged = delegate { };
    public event EventHandler<ConfigChangeEvent> UiConfigChanged = delegate { };
    public event EventHandler<ConfigChangeEvent> UiControlChanged = delegate { };

    public void Set(SystemConfigOption option, bool value) => throw new NotImplementedException();
    public void Set(SystemConfigOption option, uint value) => throw new NotImplementedException();
    public void Set(SystemConfigOption option, float value) => throw new NotImplementedException();
    public void Set(SystemConfigOption option, string value) => throw new NotImplementedException();
    public void Set(UiConfigOption option, bool value) => throw new NotImplementedException();
    public void Set(UiConfigOption option, uint value) => throw new NotImplementedException();
    public void Set(UiConfigOption option, float value) => throw new NotImplementedException();
    public void Set(UiConfigOption option, string value) => throw new NotImplementedException();
    public void Set(UiControlOption option, bool value) => throw new NotImplementedException();
    public void Set(UiControlOption option, uint value) => throw new NotImplementedException();
    public void Set(UiControlOption option, float value) => throw new NotImplementedException();
    public void Set(UiControlOption option, string value) => throw new NotImplementedException();
    public bool TryGet(SystemConfigOption option, out bool value) => throw new NotImplementedException();
    public bool TryGet(SystemConfigOption option, out uint value) => throw new NotImplementedException();
    public bool TryGet(SystemConfigOption option, out float value) => throw new NotImplementedException();
    public bool TryGet(SystemConfigOption option, out string value) => throw new NotImplementedException();
    public bool TryGet(SystemConfigOption option, out UIntConfigProperties? properties) => throw new NotImplementedException();
    public bool TryGet(SystemConfigOption option, out FloatConfigProperties? properties) => throw new NotImplementedException();
    public bool TryGet(SystemConfigOption option, out PadButtonValue value) => throw new NotImplementedException();
    public bool TryGet(SystemConfigOption option, out StringConfigProperties? properties) => throw new NotImplementedException();
    public bool TryGet(UiConfigOption option, out bool value) => throw new NotImplementedException();
    public bool TryGet(UiConfigOption option, out uint value) => throw new NotImplementedException();
    public bool TryGet(UiConfigOption option, out float value) => throw new NotImplementedException();
    public bool TryGet(UiConfigOption option, out string value) => throw new NotImplementedException();
    public bool TryGet(UiConfigOption option, out UIntConfigProperties? properties) => throw new NotImplementedException();
    public bool TryGet(UiConfigOption option, out FloatConfigProperties? properties) => throw new NotImplementedException();
    public bool TryGet(UiConfigOption option, out StringConfigProperties? properties) => throw new NotImplementedException();
    public bool TryGet(UiControlOption option, out bool value) => throw new NotImplementedException();
    public bool TryGet(UiControlOption option, out uint value) => throw new NotImplementedException();
    public bool TryGet(UiControlOption option, out float value) => throw new NotImplementedException();
    public bool TryGet(UiControlOption option, out string value) => throw new NotImplementedException();
    public bool TryGet(UiControlOption option, out UIntConfigProperties? properties) => throw new NotImplementedException();
    public bool TryGet(UiControlOption option, out FloatConfigProperties? properties) => throw new NotImplementedException();
    public bool TryGet(UiControlOption option, out StringConfigProperties? properties) => throw new NotImplementedException();
}
