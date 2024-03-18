namespace BossMod.Modules.Endwalker.SoloDuty.Endwalker;

class EndwalkerP2States : StateMachineBuilder
{
    public EndwalkerP2States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AetherialRayHint>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<SilveredEdge>()
            .ActivateOnEnter<VeilAsunder>()
            .ActivateOnEnter<SwiftAsShadow>()
            .ActivateOnEnter<CandlewickPointBlank>()
            .ActivateOnEnter<CandlewickDonut>()
            // zero cast time raidwide
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<Extinguishment>()
            .ActivateOnEnter<WyrmsTongue>()
            .ActivateOnEnter<UnmovingDvenadkatik>()
            .ActivateOnEnter<TheEdgeUnbound2>();
    }
}

[ModuleInfo(PrimaryActorOID = (uint)OID.Phase2Zenos, QuestID = 70000, NameID = 10393)]
public class EndwalkerP2 : BossModule
{
    public EndwalkerP2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
}
