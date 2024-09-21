namespace BossMod;

[ConfigDisplay(Name = "Automatic out-of-combat utility actions", Parent = typeof(ActionTweaksConfig))]
class OutOfCombatActionsConfig : ConfigNode
{
    [PropertyDisplay("Enable the feature")]
    public bool Enabled = true;

    [PropertyDisplay("Auto use Peloton when moving out of combat")]
    public bool AutoPeloton = true;
}

// Tweak to automatically use out-of-combat convenience actions (peloton, pet summoning, etc).
public sealed class OutOfCombatActionsTweak
{
    private readonly OutOfCombatActionsConfig _config = Service.Config.Get<OutOfCombatActionsConfig>();

    public void FillActions(Actor player, AIHints hints)
    {
        if (!_config.Enabled)
            return;

        if (_config.AutoPeloton && player.ClassCategory == ClassCategory.PhysRanged && !player.InCombat && player.Position != player.PrevPosition && player.FindStatus(BRD.SID.Peloton) == null)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Peloton), player, ActionQueue.Priority.VeryLow);

        // TODO: other things
    }
}
