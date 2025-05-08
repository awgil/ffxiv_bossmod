namespace BossMod.Shadowbringers.Quest.FadedMemories;

class FlamingTizona(BossModule module) : Components.StandardAOEs(module, AID.FlamingTizona, 6);

class FlameGeneralAldynnStates : StateMachineBuilder
{
    public FlameGeneralAldynnStates(BossModule module) : base(module)
    {
        TrivialPhase().ActivateOnEnter<FlamingTizona>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69311, NameID = 4739, PrimaryActorOID = (uint)OID.FlameGeneralAldynn)]
public class FlameGeneralAldynn(WorldState ws, Actor primary) : BossModule(ws, primary, new(-143, 357), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
