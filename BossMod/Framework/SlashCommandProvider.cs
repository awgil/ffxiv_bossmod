using Dalamud.Game.Command;
using Dalamud.Plugin.Services;

namespace BossMod;

public class SlashCommandHandler
{
    public delegate bool ComplexHandlerDelegate(ReadOnlySpan<char> args);

    public string SimpleDescription { get; private set; } = "";
    public Action? SimpleHandler { get; private set; }
    public readonly Dictionary<string, SlashCommandHandler> Subcommands = [];
    public string ComplexArgsHint { get; private set; } = "";
    public string ComplexDescription { get; private set; } = "";
    public ComplexHandlerDelegate? ComplexHandler { get; private set; }

    public bool Execute(ReadOnlySpan<char> args)
    {
        Span<Range> ranges = stackalloc Range[2];
        var numRanges = args.Split(ranges, ' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (numRanges == 0)
        {
            if (SimpleHandler == null)
                return false;
            SimpleHandler();
            return true;
        }

        // TODO: C#13 - GetAlternateLookup
        var sub = args[ranges[0]];
        foreach (var (k, v) in Subcommands)
            if (sub.Equals(k, StringComparison.CurrentCultureIgnoreCase))
                return v.Execute(numRanges > 1 ? args[ranges[1]] : "");

        return ComplexHandler?.Invoke(args) ?? false;
    }

    public void SetSimpleHandler(string description, Action action)
    {
        SimpleDescription = description;
        SimpleHandler = action;
    }

    public SlashCommandHandler AddSubcommand(string name) => Subcommands[name] = new();

    public void SetComplexHandler(string argsHint, string description, ComplexHandlerDelegate action)
    {
        ComplexArgsHint = argsHint;
        ComplexDescription = description;
        ComplexHandler = action;
    }

    public void AddHelp(string prefix, List<string> result, bool forceSimpleWithoutPrefix)
    {
        if (forceSimpleWithoutPrefix)
            result.Add(SimpleDescription);
        else if (SimpleDescription.Length > 0)
            result.Add($"{prefix} → {SimpleDescription}");

        if (ComplexDescription.Length > 0)
            result.Add($"{prefix} {ComplexArgsHint} → {ComplexDescription}");
        foreach (var sub in Subcommands)
            sub.Value.AddHelp($"{prefix} {sub.Key}", result, false);
    }
}

public sealed class SlashCommandProvider(ICommandManager commandManager, string rootCommand) : SlashCommandHandler, IDisposable
{
    private readonly List<string> _aliases = [];

    public void Dispose()
    {
        commandManager.RemoveHandler(rootCommand);
        foreach (var alias in _aliases)
            commandManager.RemoveHandler(alias);
    }

    public void Register()
    {
        commandManager.AddHandler(rootCommand, new CommandInfo(OnCommand) { HelpMessage = string.Join("\n", BuildHelp(true)) });
    }

    public void RegisterAlias(string alias, string prefix)
    {
        var handler = Subcommands[prefix];
        commandManager.AddHandler(alias, new CommandInfo((cmd, args) => handler.Execute(args)) { HelpMessage = $"alias for {rootCommand} {prefix}" });
        _aliases.Add(alias);
    }

    private void OnCommand(string cmd, string args)
    {
        Service.Log($"OnCommand: {cmd} {args}");
        if (!Execute(args))
        {
            Service.ChatGui.PrintError($"Unrecognized slash command: {cmd} {args}");
            foreach (var h in BuildHelp(false))
                Service.ChatGui.Print($"* {h}");
        }
    }

    private List<string> BuildHelp(bool forceSimpleWithoutPrefix)
    {
        List<string> help = [];
        AddHelp(rootCommand, help, forceSimpleWithoutPrefix);
        return help;
    }
}
