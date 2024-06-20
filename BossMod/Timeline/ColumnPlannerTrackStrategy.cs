using BossMod.Autorotation;

namespace BossMod;

public class ColumnPlannerTrackStrategy(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, StrategyConfig config, int level, ModuleRegistry.Info? moduleInfo)
    : ColumnPlannerTrack(timeline, tree, phaseBranches, config.UIName)
{
    public class OverrideElement(Entry window) : Element(window)
    {
        public StrategyValue Value = new();
    }

    public void AddElement(StateMachineTree.Node attachNode, float delay, float windowLength, StrategyValue value)
    {
        var elem = (OverrideElement)AddElement(attachNode, delay, windowLength);
        elem.Value = value;
        UpdateProperties(elem);
    }

    protected override Element CreateElement(Entry window)
    {
        var res = new OverrideElement(window);
        for (int i = 1; i < config.Options.Count; ++i)
        {
            if (level >= config.Options[i].MinLevel && level <= config.Options[i].MaxLevel)
            {
                res.Value.Option = i;
                break;
            }
        }
        UpdateProperties(res);
        return res;
    }

    protected override List<string> DescribeElement(Element e)
    {
        var cast = (OverrideElement)e;
        return UIStrategyValue.Preview(ref cast.Value, config, moduleInfo);
    }

    protected override void EditElement(Element e)
    {
        var cast = (OverrideElement)e;
        if (UIStrategyValue.DrawEditor(ref cast.Value, config, moduleInfo, level))
        {
            UpdateProperties(cast);
            NotifyModified();
        }
    }

    private void UpdateProperties(OverrideElement e)
    {
        var opt = config.Options[e.Value.Option];
        e.Window.Color = e.Value.Option > 0 && e.Value.Option <= Timeline.Colors.PlannerWindow.Length ? Timeline.Colors.PlannerWindow[e.Value.Option - 1] : Timeline.Colors.PlannerFallback;
        e.CooldownLength = opt.Cooldown;
        e.EffectLength = opt.Effect;
    }
}
