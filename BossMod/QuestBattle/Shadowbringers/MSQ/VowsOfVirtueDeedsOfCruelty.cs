namespace BossMod.QuestBattle.Shadowbringers.MSQ;

class AutoEstinien(WorldState ws) : UnmanagedRotation(ws, 10)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (primaryTarget == null)
            return;

        var gcd = ComboAction switch
        {
            Roleplay.AID.SonicThrust => Roleplay.AID.CoerthanTorment,
            Roleplay.AID.DoomSpike => Roleplay.AID.SonicThrust,
            _ => Roleplay.AID.DoomSpike
        };

        UseAction(gcd, primaryTarget);
        if (Player.HPMP.CurHP * 2 < Player.HPMP.MaxHP)
            UseAction(Roleplay.AID.AquaVitae, Player, -10);

        UseAction(Roleplay.AID.SkydragonDive, primaryTarget, -10);
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 702)]
public class VowsOfVirtueDeedsOfCruelty(WorldState ws) : QuestBattle(ws)
{
    private readonly AutoEstinien _ai = new(ws);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(134, 0, 400))
            .With(obj => {
                obj.OnConditionChange += (flag, val) => obj.CompleteIf(flag == Dalamud.Game.ClientState.Conditions.ConditionFlag.Jumping61 && !val);
            })
            .Hints((player, hints) => {
                hints.PathfindMapCenter = player.Position with { Z = 400 };
                hints.PathfindMapBounds = new ArenaBoundsRect(20, 14);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(240, -40, 287))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints) => _ai.Execute(player, hints);
}
