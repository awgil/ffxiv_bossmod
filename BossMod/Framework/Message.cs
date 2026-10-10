using DalaMock.Host.Mediator;

namespace BossMod;

public sealed record OpenConfigMessage(string Tab = "") : MessageBase;
public sealed record OpenPresetEditorMessage(string Name) : MessageBase;
