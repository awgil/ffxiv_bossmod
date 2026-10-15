using DalaMock.Host.Mediator;

namespace BossMod;

public sealed record OpenSettingsWindowMessage(string Tab = "") : MessageBase;
public sealed record OpenPresetEditorMessage(string Name) : MessageBase;
public sealed record OpenWindowMessage(Type WindowType) : MessageBase;
public sealed record ToggleWindowMessage(Type WindowType) : MessageBase;
public sealed record ShowConfigNodeMessage(Type NodeType) : MessageBase;
