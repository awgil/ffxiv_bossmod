using DalaMock.Host.Mediator;

namespace BossMod.Services;

public record class CreateWindowMessage(UIWindow Window) : MessageBase;
