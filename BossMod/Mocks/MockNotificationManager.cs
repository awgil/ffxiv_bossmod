using DalaMock.Core.Mocks;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;

namespace BossMod.Mocks;

internal class MockNotificationManager : INotificationManager, IMockService
{
    public string ServiceName => "MockNotificationManager";

    public IActiveNotification AddNotification(Notification notification) => throw new NotImplementedException();
}
