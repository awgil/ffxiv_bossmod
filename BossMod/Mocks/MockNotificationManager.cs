using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;

namespace BossMod.Mocks;

internal class MockNotificationManager : INotificationManager
{
    public IActiveNotification AddNotification(Notification notification) => throw new NotImplementedException();
}
