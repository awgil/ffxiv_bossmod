namespace BossMod.QuestBattle.Stormblood.MSQ;

public class AutoAlphi(WorldState ws) : UnmanagedRotation(ws, 25)
{
    private Actor? Carby => World.Actors.FirstOrDefault(x => x.OID == 0x2343);

    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget?.Type == ActorType.Enemy)
            UseAction(Roleplay.AID.RuinIII, primaryTarget);

        if (Carby?.CastInfo is { Action.ID: 9396 } cinfo)
            Hints.AddForbiddenZone(new AOEShapeDonut(3, 100), Carby.Position, activation: World.FutureTime(cinfo.NPCRemainingTime));
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 582)]
public class EmissaryOfTheDawn(WorldState ws) : QuestBattle(ws)
{
    private readonly AutoAlphi _ai = new(ws);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-5.44f, 0.00f, 1.25f))
            .Hints((player, hints) => {
                hints.PrioritizeTargetsByOID(0x234C);

                if (ws.Actors.FirstOrDefault(x => x.OID == 0x2344 && x.IsTargetable) is Actor popularis)
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.Physick), popularis, ActionQueue.Priority.High);
            })
            .WithInteract(0x1EA9D9)
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints) => _ai.Execute(player, hints);
}
