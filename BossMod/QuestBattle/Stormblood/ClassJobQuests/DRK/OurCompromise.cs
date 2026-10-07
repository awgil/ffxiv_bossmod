namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.DRK;

public enum OID : uint
{
    YstrideMelee = 0x1C5F,
    Myste = 0x1C62,
    ImpenetrableVeil = 0x1C63,
    YstrideHealer = 0x1C64,
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 272, 716)]
internal class OurCompromise(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-617.188f, 59.021f, -465.867f))
            .With(obj => obj.OnActorCombatChanged += act => obj.CompleteIf(act.OID == 0 && !act.InCombat))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            var priority = (OID)e.Actor.OID switch
            {
                OID.YstrideHealer    => 3, // kill healer first
                OID.YstrideMelee     => 2, // then melee adds
                OID.Myste            => 1, // BossMod combo should eventually use Souleater
                OID.ImpenetrableVeil => AIHints.Enemy.PriorityPointless, // ignore shield object
                _                    => 0
            };
            // ForcePriority bypasses the PriorityPointless setter guard (L?? enemies appear PendingDead)
            e.ForcePriority(priority);
        }
    }
}
