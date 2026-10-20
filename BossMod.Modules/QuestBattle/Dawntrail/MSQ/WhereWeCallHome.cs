using BossMod.Roleplay;

namespace BossMod.QuestBattle.Dawntrail.MSQ;

class AutoKrile(WorldState ws) : UnmanagedRotation(ws, 25)
{
    protected override void Exec(Actor? primaryTarget)
    {
        if (Player.HPRatio < 0.6f)
            UseAction(AID.AquaVitaeKrile, Player, -50);

        if (primaryTarget is not { IsAlly: false })
            return;

        var numAOETargets = Hints.NumPriorityTargetsInAOECircle(primaryTarget.Position, 5);

        if (numAOETargets >= 2)
            UseAction(ComboAction switch
            {
                AID.Aero2InGreen => AID.Water2InBlue,
                AID.Fire2InRed => AID.Aero2InGreen,
                _ => AID.Fire2InRed
            }, primaryTarget);
        else
            UseAction(ComboAction switch
            {
                AID.AeroInGreen => AID.WaterInBlue,
                AID.FireInRed => AID.AeroInGreen,
                _ => AID.FireInRed
            }, primaryTarget);

        var butterflyIn = ActionDefinitions.Instance[ActionID.MakeSpell(AID.ButterflyMotif)]!.ReadyIn(World.Client.Cooldowns, World.Client.DutyActions);

        if (butterflyIn < 5 || Player.FindStatus(SID.ButterflyMotif) != null)
            Hints.GoalZones.Add(Hints.GoalAOECircle(10));

        var targetsAny = false;
        foreach (var p in Hints.PriorityTargets)
        {
            targetsAny = true;
            if (!AIHints.TargetInAOECircle(p.Actor, Player.Position, 10))
                return;
        }

        if (targetsAny && World.Client.Cooldowns[ActionDefinitions.GCDGroup].Remaining > 0)
            UseAction(AID.ButterflyMotif, Player, 100);
    }
}

[ZoneModuleInfo(1085)]
public class WhereWeCallHome(WorldState ws) : QuestBattle(ws)
{
    private readonly AutoKrile _ai = new(ws);

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        if (player.FindStatus(SID.RolePlaying) == null)
            return;

        _ai.Execute(player, hints);
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Pack 1")
            .Hints((player, hints) => hints.PrioritizeAll())
            .With(obj => {
                var numCs = 0;

                obj.OnStatusGain += (act, st) => obj.CompleteIf(st.ID == 1268 && ++numCs == 2);
            }),

        QuestObjective.StandardInteract(ws, 0x1EBF37, new Vector3(334.33f, -117.50f, -490.46f))
            .Named("Citizens 1"),

        QuestObjective.StandardInteract(ws, 0x4B84, new Vector3(284.04f, -115.98f, -416.33f))
            .Named("Citizen 2")
            .CompleteOnTargetable(0x4B84, false),

        QuestObjective.StandardInteract(ws, 0x1EBF38, new Vector3(205.43f, -115.99f, -398.92f))
            .Named("Fire"),

        QuestObjective.StandardInteract(ws, 0x4B36)
            .Named("Citizen 3")
            .CompleteOnTargetable(0x4B36, false),
        QuestObjective.StandardInteract(ws, 0x4B38)
            .Named("Citizen 4")
            .CompleteOnTargetable(0x4B38, false),

        QuestObjective.Combat(ws, new Vector3(133.17f, -114.00f, -430.79f))
            .Named("Pack 5")
            .Hints((player, hints) => hints.PrioritizeAll())
    ];
}
