namespace BossMod.QuestBattle.Dawntrail.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 998)]
public class TheProtectorAndTheDestroyer(WorldState ws) : QuestBattle(ws)
{
    public unsafe override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        if (ws.Party.Player()?.PosRot.Y > 50)
            return [];

        return [
        new QuestObjective(ws)
            .WithConnection(new Waypoint(new Vector3(-0.57f, -6.05f, 209.93f), false))
            .WithConnection(new Waypoint(new Vector3(0.01f, 0.00f, 114.44f), false))
            .WithConnection(new Vector3(7.51f, 7.89f, 21.07f))
            .With(obj => {
                List<uint> sadCitizens = [];

                obj.OnDirectorUpdate += (diru) => {
                    if (diru.UpdateID == 0x80000000)
                    {
                        var localID = diru.Param1;
                        if (diru.Param2 == 0xF119)
                            sadCitizens.Add(diru.Param1);
                        else
                        {
                            sadCitizens.Remove(diru.Param1);
                            obj.CompleteIf(sadCitizens.Count == 0);
                        }
                    }
                };

                obj.AddAIHints += (player, hints) => {
                    if (!player.InCombat)
                        hints.InteractWithTarget = null; // World.Actors.FirstOrDefault(x => sadCitizens.Contains(Utils.GameObjectInternal(Service.ObjectTable[x.SpawnIndex])->LayoutId) && x.IsTargetable);
                };
            }),

        // wait for wall to disappear
        new QuestObjective(ws)
            .With(obj => {
                obj.OnEventObjectAnimation += (act, param1, param2) => {
                    obj.CompleteIf(act.OID == 0x1E8FB8 && param1 == 4 && param2 == 8);
                };
            }),

        // trigger zone transition
        new QuestObjective(ws)
            .WithConnection(new Vector3(115.72f, 0.00f, 0.36f))
            .CompleteOnState7(0x1EBAE1)
    ];
    }
}
