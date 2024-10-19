namespace BossMod.QuestBattle.Shadowbringers.SideQuests;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 668)]
internal class HiredGunblades(WorldState ws) : QuestBattle(ws)
{
    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .WithConnection(new Vector3(-34.54f, 11.95f, -254.85f))
            .Hints((player, hints) => {
                uint[] townspeople = [0x1EAE12, 0x1EAE13, 0x1EAE14, 0x1EAE15, 0x1EAE16];

                hints.InteractWithTarget = World.Actors.FirstOrDefault(a => townspeople.Contains(a.OID) && a.IsTargetable);
            })
            .With(obj => {
                obj.OnActorCombatChanged += (act) => obj.CompleteIf(act.OID == 0 && !act.InCombat);
            }),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-50.79f, 10.73f, -226.50f))
            .CompleteOnKilled(0x29F3),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-53.44f, 6.73f, -130.41f))
    ];
}

