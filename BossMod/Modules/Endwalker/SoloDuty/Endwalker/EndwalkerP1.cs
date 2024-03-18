namespace BossMod.Modules.Endwalker.SoloDuty.Endwalker;

class EndwalkerP1States : StateMachineBuilder
{
    public EndwalkerP1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<Megaflare>()
            .ActivateOnEnter<JudgementBolt>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<AkhMorn>()
            .ActivateOnEnter<StarBeyondStars>()
            .ActivateOnEnter<TheEdgeUnbound>()
            .ActivateOnEnter<WyrmsTongue>()
            .ActivateOnEnter<NineNightsAvatar>()
            .ActivateOnEnter<NineNightsHelpers>()
            .ActivateOnEnter<VeilAsunder>()
            .ActivateOnEnter<Exaflare>()
            .ActivateOnEnter<MortalCoil>()
            .ActivateOnEnter<TidalWave2>();
    }
}

[ModuleInfo(PrimaryActorOID = (uint)OID.Phase1Zenos, QuestID = 70000, NameID = 10393)]
public class EndwalkerP1 : BossModule
{
    public EndwalkerP1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(100, 100), 20)) { }
}
