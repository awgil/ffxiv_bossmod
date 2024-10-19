namespace BossMod.QuestBattle.Shadowbringers.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 743)]
public class FadedMemories(WorldState ws) : QuestBattle(ws)
{
    enum SID : uint
    {
        Invincibility = 671
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        QuestObjective.Combat(ws, new Vector3(-96.48f, -281.11f, 226.96f)).Named("Scions"),

        new QuestObjective(ws)
            .Named("Dialogue 1")
            .WithInteract(0x2F0D)
            .With(obj => {
                obj.OnDirectorUpdate += (diru) => obj.CompleteIf(diru.Param1 == 0x801202 && diru.Param2 == 0);
            }),

        QuestObjective.Combat(ws, new Vector3(-121.18f, -281.11f, 176.96f)).Named("Garleans"),
        QuestObjective.Combat(ws, new Vector3(-183.93f, -281.11f, 198.60f)).Named("Crystal Braves"),

        new QuestObjective(ws)
            .Named("Dialogue 2")
            .WithConnection(new Vector3(-185.77f, -281.11f, 254.00f))
            .WithInteract(0x2F0D)
            .With(obj => {
                // wall disappears
                obj.OnEventObjectAnimation += (act, p1, p2) => obj.CompleteIf(act.OID == 0x1EA1A1 && p1 == 4 && p2 == 8);
            }),

        QuestObjective.Combat(ws, new Vector3(-250.62f, -266.00f, 270.28f), new Vector3(-252.78f, -266.00f, 304.66f))
            .Named("Heavens' Ward")
            .Hints((player, hints) => {
                foreach(var h in hints.PotentialTargets)
                {
                    h.Priority = h.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;

                    // The Dragon's Gaze
                    if (h.Actor.CastInfo is {Action.ID: 21090} cinfo)
                        hints.ForbiddenDirections.Add((player.AngleTo(h.Actor), 45.Degrees(), World.FutureTime(cinfo.NPCRemainingTime)));
                }
            }),

        QuestObjective.Combat(ws, new Vector3(-159.19f, -266.00f, 351.25f)).Named("Commanders"),

        QuestObjective.Combat(ws, new Vector3(-223.25f, -266.00f, 440.12f)).Named("Nidhogg"),

        new QuestObjective(ws)
            .WithConnection(new Vector3(-293.15f, -244.74f, 506.86f))
            .WithConnection(new Vector3(-282.51f, -244.74f, 523.28f))
            .WithInteract(0x2F0D)
            .With(obj => {
                // dunno which wall this is, it triggers right after you interact with elidibert
                obj.OnEventObjectStateChanged += (act, state) => obj.CompleteIf(act.OID == 0x1EA1A1 && state == 2);
            })
            .Named("Dialogue 3"),

        QuestObjective.Combat(ws, new Vector3(-324.27f, -229.19f, 594.67f)).Named("Best girl"),
        QuestObjective.Combat(ws, new Vector3(-304.79f, -229.19f, 605.35f)).Named("Other best girl"),
        QuestObjective.Combat(ws, new Vector3(-318.37f, -229.19f, 613.27f)).Named("Zenos"),

        new QuestObjective(ws)
            .Named("Dialogue 4")
            .WithConnection(new Vector3(-322.08f, -229.19f, 622.23f))
            .WithInteract(0x2F0D)
    ];
}
