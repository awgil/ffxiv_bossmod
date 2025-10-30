using BossMod.Autorotation;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace BossMod;

internal sealed class MultiboxManager : IDisposable
{
    private readonly RotationModuleManager _rotations;
    private readonly WorldState _ws;

    public MultiboxManager(RotationModuleManager mgr, WorldState ws)
    {
        _rotations = mgr;
        _ws = ws;
        Service.ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        Service.ChatGui.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Service.IsDev && type == XivChatType.Echo && message.TextValue == "test")
        {
            var leaderId = _ws.Party.Members[0].ContentId;

            foreach (var p in _rotations.Presets)
            {
#if DEBUG
                var md = p.Modules.FirstOrDefault(m => m.Type == typeof(Autorotation.MiscAI.Multibox));
                if (md != null)
                    md.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueInt() { Value = (long)leaderId }));
                else
                    Service.Log($"no matching module in {p.Name}");
#endif
            }
        }
    }
}
