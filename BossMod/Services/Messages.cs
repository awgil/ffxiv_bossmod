using DalaMock.Host.Mediator;
using Dalamud.Interface.Windowing;

namespace BossMod.Services;

public record class CreateWindowMessage(Window Window, bool Detached) : MessageBase;
public record class DestroyWindowMessage(string WindowName) : MessageBase;
