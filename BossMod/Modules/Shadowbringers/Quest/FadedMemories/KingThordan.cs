namespace BossMod.Shadowbringers.Quest.FadedMemories;

class DragonsGaze(BossModule module) : Components.CastGaze(module, AID.TheDragonsGaze);

class KingThordanStates : StateMachineBuilder
{
    public KingThordanStates(BossModule module) : base(module)
    {
        TrivialPhase().ActivateOnEnter<DragonsGaze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 3632, PrimaryActorOID = (uint)OID.KingThordan)]
public class KingThordan(WorldState ws, Actor primary) : BossModule(ws, primary, new(-247, 321), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }
}
