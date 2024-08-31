using BossMod.Autorotation;

namespace BossMod;

public class ColumnPlannerTrackStrategy(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, StrategyConfig config, int level, ModuleRegistry.Info? moduleInfo)
    : ColumnPlannerTrack(timeline, tree, phaseBranches, config.UIName)
{
    protected override StrategyValue GetDefaultValue()
    {
        var res = new StrategyValue();
        for (int i = 1; i < config.Options.Count; ++i)
        {
            if (level >= config.Options[i].MinLevel && level <= config.Options[i].MaxLevel)
            {
                res.Option = i;
                break;
            }
        }
        return res;
    }

    protected override void RefreshElement(Element e)
    {
        var opt = config.Options[e.Value.Option];
        e.Window.Color = e.Value.Option > 0 && e.Value.Option <= Timeline.Colors.PlannerWindow.Length ? Timeline.Colors.PlannerWindow[e.Value.Option - 1] : Timeline.Colors.PlannerFallback;
        e.CooldownLength = opt.Cooldown;
        e.EffectLength = opt.Effect;
    }

    protected override List<string> DescribeElement(Element e) => UIStrategyValue.Preview(ref e.Value, config, moduleInfo);
    protected override bool EditElement(Element e) => UIStrategyValue.DrawEditor(ref e.Value, config, moduleInfo, level) | EditElementWindow(e);
}
