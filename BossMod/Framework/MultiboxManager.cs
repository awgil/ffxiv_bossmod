using BossMod.Autorotation;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace BossMod;

internal sealed class MultiboxManager(RotationModuleManager rotations, WorldState ws, IChatGui chat) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        chat.ChatMessage += OnChatMessage;
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        chat.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (type == XivChatType.Echo && message.TextValue == "test")
        {
            var leaderId = ws.Party.Members[0].ContentId;

            foreach (var p in rotations.Presets)
            {
                var md = p.Modules.FirstOrDefault(m => m.Type == typeof(Autorotation.MiscAI.Multibox));
                if (md != null)
                    md.TransientSettings.Add(new Preset.ModuleSetting(default, 0, new StrategyValueInt() { Value = (long)leaderId }));
                else
                    Service.Log($"no matching module in {p.Name}");
            }
        }
    }
}
