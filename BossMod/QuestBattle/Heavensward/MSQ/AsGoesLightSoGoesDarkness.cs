namespace BossMod.QuestBattle.Heavensward.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 441)]
public sealed class AsGoesLightSoGoesDarkness(WorldState ws) : QuestBattle(ws)
{
    enum OID : uint
    {
        Boss = 0x148A,
        Helper = 0x233C,
        VaultDoor1 = 0x1E9ED7,
        VaultDoor2 = 0x1E9ED8,
        ArenaDoor = 0x1E9ED9,
        VaultDoor3 = 0x1E9EDA,
        ArenaWall = 0x1E9EDD,

        Refugee1 = 0x14BE,
        Refugee2 = 0x14BF,
        Refugee3 = 0x14C0,
        Refugee4 = 0x14C1,
        Refugee5 = 0x14C2,
        Bonds = 0x1E9EE0,
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        QuestObjective Refugee(OID oid)
        {
            var obj = new QuestObjective(ws);
            obj.OnStatusLose += (act, stat) => obj.CompleteIf(act.OID == (uint)oid && stat.ID == 990);
            return obj;
        }

        QuestObjective EventState(OID oid)
        {
            var obj = new QuestObjective(ws);
            obj.OnActorEventStateChanged = (act) => obj.CompleteIf(act.OID == (uint)oid && act.EventState == 7);
            return obj;
        }

        return [
            Refugee(OID.Refugee1).Named("Refugee 1").WithConnection(V3(0, -300, 75)).StopOnCombat(),

            EventState(OID.VaultDoor1).Named("Pack 1").WithConnection(V3(16, -300, 30)),
            EventState(OID.VaultDoor2).Named("Refugee 2").WithConnection(V3(52, -300, -30)),
            EventState(OID.ArenaDoor).Named("Pack 2").WithConnection(V3(-30, -300, -75)),
            EventState(OID.VaultDoor3).Named("Cutscene").WithConnection(V3(-17.5f, -292, -100)),

            Refugee(OID.Refugee3)
                .Named("Refugee 3+4")
                .WithInteract((uint)OID.Bonds)
                .WithConnection(V3(-52, -300, -30)),

            Refugee(OID.Refugee5)
                .Named("Refugee 5")
                .WithInteract((uint)OID.Bonds)
                .WithConnection(V3(55, -300, -68)),

            EventState(OID.ArenaWall).Named("Help Aymeric").WithConnection(V3(0, -292, -100)),

            new QuestObjective(ws).Named("Refugee 6").WithConnection(V3(2, -282.35f, -151))
        ];
    }
}
