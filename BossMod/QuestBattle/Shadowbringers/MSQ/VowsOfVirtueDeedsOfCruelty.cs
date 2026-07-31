namespace BossMod.QuestBattle.Shadowbringers.MSQ;

class AutoEstinien(WorldState ws) : UnmanagedRotation(ws, 3)
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
        if (Player.HPRatio < 0.5f)
            UseAction(Roleplay.AID.AquaVitae, Player, -10);

        UseAction(Roleplay.AID.SkydragonDive, primaryTarget, -10);
    }
}

[ZoneModuleInfo(702)]
public class VowsOfVirtueDeedsOfCruelty(WorldState ws) : QuestBattle(ws)
{
    private readonly AutoEstinien _ai = new(ws);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(134, 0, 400))
            .With(obj => {
                obj.OnConditionChange += (flag, val) => obj.CompleteIf(flag == Dalamud.Game.ClientState.Conditions.ConditionFlag.Jumping61 && !val);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(240, -40, 287))
    ];

    public override void AddQuestAIHints(Actor player, AIHints hints) => _ai.Execute(player, hints);
}
