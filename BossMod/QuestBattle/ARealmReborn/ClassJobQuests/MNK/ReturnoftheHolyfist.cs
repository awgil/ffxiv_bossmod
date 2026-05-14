namespace BossMod.QuestBattle.ARealmReborn.ClassJobQuests.MNK;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 322, 261)]
internal class ReturnOfTheHolyfist(WorldState ws) : QuestBattle(ws)
{
    private int _snapPunchCount;
    private int _comboStep; // 0=Bootshine, 1=TrueStrike, 2=SnapPunch

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .With(obj => {
                obj.OnEventCast += (act, spell) => {
                    if (act.OID != 0) return;
                    var id = spell.Action;
                    if (_comboStep == 0 && id == ActionID.MakeSpell(BossMod.MNK.AID.Bootshine))
                        _comboStep = 1;
                    else if (_comboStep == 1 && id == ActionID.MakeSpell(BossMod.MNK.AID.TrueStrike))
                        _comboStep = 2;
                    else if (_comboStep == 2 && id == ActionID.MakeSpell(BossMod.MNK.AID.SnapPunch))
                    {
                        _snapPunchCount++;
                        _comboStep = 0;
                    }
                };
                // no completion condition — objective stays active so Hints and OnEventCast keep firing
            })
            .Hints((player, hints) => {
                if (_snapPunchCount >= 3) return;
                var target = hints.PotentialTargets.FirstOrDefault()?.Actor;
                if (target == null) return;
                hints.ForcedTarget = target;
                var next = _comboStep switch {
                    0 => BossMod.MNK.AID.Bootshine,
                    1 => BossMod.MNK.AID.TrueStrike,
                    _ => BossMod.MNK.AID.SnapPunch,
                };
                hints.ActionsToExecute.Push(ActionID.MakeSpell(next), target, ActionQueue.Priority.VeryHigh);
            })
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        // ForcePriority bypasses the PriorityPointless setter guard so the mob appears in PotentialTargets
        foreach (var e in hints.PotentialTargets)
            e.ForcePriority(0);
    }
}