using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Autorotation.xan;

public class TargetingRenderer : TrackRenderer
{
    public override void DrawLabel(StrategyContext context, StrategyConfig config)
    {
        ImGui.Text("Targeting");
        ImGui.SameLine();
        UIMisc.HelpMarker("These settings only affect what the rotation module chooses to use actions on. Regardless of which one you choose, this module will not change your in-game target ('hard target').\n\nFor a module that will automatically change your hard target, use AI -> Automatic targeting.");
    }

    public override bool DrawValue(StrategyContext context, StrategyConfigTrack config, ref StrategyValueTrack value)
    {
        var ix = value.Option;
        var modified = false;
        var opt = (Targeting)ix;

        var forcepri = opt == Targeting.AutoPrimary;
        var trypri = forcepri || opt == Targeting.AutoTryPri;

        if (ImGui.RadioButton("Use player's target", opt == Targeting.Manual))
        {
            value.Option = 0;
            modified = true;
        }
        if (ImGui.RadioButton("Automatically pick best target", opt != Targeting.Manual))
        {
            if (opt != Targeting.Auto)
            {
                value.Option = 1;
                modified = true;
            }
        }
        using (ImRaii.Disabled(opt == Targeting.Manual))
        {
            ImGui.Indent();
            if (ImGui.Checkbox("Make sure player's target is hit", ref trypri))
            {
                value.Option = trypri ? 3 : 1;
                modified = true;
            }
            using (ImRaii.Disabled(!trypri))
            {
                if (ImGui.Checkbox("Do nothing if the player doesn't have a target", ref forcepri))
                {
                    value.Option = forcepri ? 2 : 3;
                    modified = true;
                }
            }
            ImGui.Unindent();
        }

        return modified;
    }
}

public class OffensiveStrategyRenderer : TrackRenderer
{
    private static readonly List<string> optionNames = ["Automatic", "Disabled", "Forced"];

    public override bool DrawValue(StrategyContext context, StrategyConfigTrack config, ref StrategyValueTrack value) => UICombo.Radio(typeof(OffensiveStrategy), ref value.Option, true, i => optionNames.BoundSafeAt(i, "")!);
}

public class DefaultOnRenderer : TrackRenderer
{
    public override bool DrawValue(StrategyContext context, StrategyConfigTrack config, ref StrategyValueTrack value)
    {
        var enabled = value.Option == 0;

        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            value.Option = enabled ? 0 : 1;
            return true;
        }

        return false;
    }
}

public class DefaultOffRenderer : TrackRenderer
{
    public override bool DrawValue(StrategyContext context, StrategyConfigTrack config, ref StrategyValueTrack value)
    {
        var enabled = value.Option == 1;

        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            value.Option = enabled ? 1 : 0;
            return true;
        }

        return false;
    }
}
