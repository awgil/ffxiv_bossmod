namespace BossMod.QuestBattle.Stormblood.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 467)]
public sealed class TheKeyToVictory(WorldState ws) : QuestBattle(ws)
{
    enum OID : uint
    {
        Wiscar = 0x1E82,
        Soblyn = 0x1E83,
        QueerDevice = 0x1EA757,
        TatteredDiary = 0x1EA752,
        Colossus = 0x1E7F
    }

    public override List<QuestObjective> DefineObjectives(WorldState ws)
    {
        return [
            new QuestObjective(ws)
                .WithConnection(new Vector3(-396.38f, 4.94f, 122.21f))
                .WithConnection(new Vector3(-285.98f, 11.18f, 223.66f))
                .Hints((player, hints) =>
                {
                    // eventobj doesn't spawn until all the npcs are out of combat - way faster to kill all the soblyns than to wait
                    hints.PrioritizeTargetsByOID(OID.Soblyn);
                })
                .WithInteract(OID.QueerDevice)
                .CompleteOnTargetable((uint)OID.QueerDevice, false),

            new QuestObjective(ws)
                .WithConnection(new Vector3(-278.78f, 11.18f, 158.27f))
                .PauseForCombat(false)
                .Hints((player, hints) => hints.PrioritizeAll())
                .CompleteOnKilled((uint)OID.Colossus),

            new QuestObjective(ws)
                .WithInteract(OID.TatteredDiary)
                .CompleteOnDestroyed((uint)OID.TatteredDiary),

            new QuestObjective(ws)
                .WithConnection(new Vector3(-100.29f, 3.63f, 527.66f))
                .WithConnection(new Vector3(43.84f, 37.55f, 699.77f))
                .Hints((player, hints) => {
                    hints.PrioritizeTargetsByOID(0x1EB3, 1);
                    hints.PrioritizeTargetsByOID(0x1EB0, 0);
                })
                .CompleteOnKilled(0x1EB0),

            new QuestObjective(ws)
                .WithConnection(new Vector3(50.57f, 42.00f, 724.45f))
                .WithInteract(0x1EA771)
        ];
    }
}
