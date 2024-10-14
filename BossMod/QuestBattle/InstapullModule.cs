namespace BossMod.QuestBattle;

public class InstapullModule(WorldState ws, Actor primary, WPos center, ArenaBounds bounds) : BossModule(ws, primary, center, bounds)
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.OID == 0 && !actor.InCombat)
            hints.PrioritizeAll();
    }

    protected sealed override bool CheckPull() => true;
}
