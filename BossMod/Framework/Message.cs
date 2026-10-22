using DalaMock.Host.Mediator;

namespace BossMod;

public sealed record OpenSettingsWindow(string Tab = "") : MessageBase;
public sealed record OpenPresetEditor(string Name) : MessageBase;
public sealed record OpenWindow(Type WindowType) : MessageBase;
public sealed record ToggleWindow(Type WindowType) : MessageBase;
public sealed record ShowConfigNode(Type NodeType) : MessageBase;
