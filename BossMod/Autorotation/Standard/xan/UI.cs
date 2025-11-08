using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Autorotation.xan;

public class TargetingRenderer : DefaultStrategyRenderer
{
    public override void DrawLabel(StrategyConfigTrack t)
    {
        ImGui.Text("Targeting");
        ImGui.SameLine();
        UIMisc.HelpMarker("These settings only affect what the rotation module chooses to use actions on. Regardless of which one you choose, this module will not change your in-game target ('hard target').\n\nFor a module that will automatically change your hard target, use AI -> Automatic targeting.");
    }

    public override bool Draw(StrategyConfigTrack _track, ref StrategyValue value)
    {
        var ix = ((StrategyValueTrack)value).Option;
        var modified = false;
        var opt = (Targeting)ix;

        var forcepri = opt == Targeting.AutoPrimary;
        var trypri = forcepri || opt == Targeting.AutoTryPri;

        if (ImGui.RadioButton("Use player's target", opt == Targeting.Manual))
        {
            value = new StrategyValueTrack() { Option = 0 };
            modified = true;
        }
        if (ImGui.RadioButton("Automatically pick best target", opt != Targeting.Manual))
        {
            if (opt != Targeting.Auto)
            {
                value = new StrategyValueTrack() { Option = 1 };
                modified = true;
            }
        }
        using (ImRaii.Disabled(opt == Targeting.Manual))
        {
            ImGui.Indent();
            if (ImGui.Checkbox("Make sure player's target is hit", ref trypri))
            {
                value = new StrategyValueTrack() { Option = trypri ? 3 : 1 };
                modified = true;
            }
            using (ImRaii.Disabled(!trypri))
            {
                if (ImGui.Checkbox("Do nothing if the player doesn't have a target", ref forcepri))
                {
                    value = new StrategyValueTrack() { Option = forcepri ? 2 : 3 };
                    modified = true;
                }
            }
            ImGui.Unindent();
        }

        return modified;
    }
}

public class AOERenderer : DefaultStrategyRenderer
{
    public override bool Draw(StrategyConfigTrack config, ref StrategyValue value)
    {
        var opt = ((StrategyValueTrack)value).Option;

        if (UICombo.Radio(config.OptionEnum, ref opt, false, ix => config.Options[ix].DisplayName.Length > 0 ? config.Options[ix].DisplayName : UICombo.EnumString((Enum)config.OptionEnum.GetEnumValues().GetValue(ix)!)))
        {
            value = new StrategyValueTrack() { Option = opt };
            return true;
        }

        return false;
    }
}

public class OffensiveStrategyRenderer : DefaultStrategyRenderer
{
    private static readonly List<string> optionNames = ["Automatic", "Disabled", "Forced"];

    public override bool Draw(StrategyConfigTrack config, ref StrategyValue value)
    {
        var opt = ((StrategyValueTrack)value).Option;

        if (UICombo.Radio(typeof(OffensiveStrategy), ref opt, true, i => optionNames.BoundSafeAt(i, "")!))
        {
            value = new StrategyValueTrack() { Option = opt };
            return true;
        }

        return false;
    }
}

public class CheckboxRenderer : DefaultStrategyRenderer
{
    public override bool Draw(StrategyConfigTrack config, ref StrategyValue currentValue)
    {
        var opt = ((StrategyValueTrack)currentValue).Option == 1;

        if (ImGui.Checkbox("Enabled", ref opt))
        {
            currentValue = new StrategyValueTrack() { Option = opt ? 1 : 0 };
            return true;
        }

        return false;
    }
}
