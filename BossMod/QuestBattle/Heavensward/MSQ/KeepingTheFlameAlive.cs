namespace BossMod.QuestBattle.Heavensward.MSQ;

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 400)]
public class KeepingTheFlameAlive(WorldState ws) : QuestBattle(ws)
{
    enum OID : uint
    {
        HummingAtomizer = 0xF88,
        IronCell = 0xF89,
        IdentificationKey = 0x1E9A2A
    }

    private static readonly uint[] CrystalBraves = [0xF70, 0xF71, 0xF72];

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        new QuestObjective(ws)
            .Named("Trigger cutscene")
            .WithConnections(
                new Waypoint(new(-30.25f, 0.14f, -132.16f)),
                // drop off bridge
                new Waypoint(new(-42.44f, -10.85f, -122.70f), false),
                new Waypoint(new(-78.03f, -10.18f, -98.29f))
            )
            .CompleteOnCreated((uint)OID.IronCell),

        new QuestObjective(ws)
            .Named("Open cell")
            .Hints((act, hints) => hints.PrioritizeTargetsByOID(OID.IronCell))
            .CompleteOnKilled((uint)OID.IronCell),

        new QuestObjective(ws)
            .Named("Destroy generator")
            .WithConnection(V3(163.35f, 6.26f, -65.16f))
            .Hints((player, hints) =>
            {
                if (player.PosRot.Y >= 6)
                    hints.PrioritizeTargetsByOID(OID.HummingAtomizer);
            })
            .CompleteOnKilled((uint)OID.HummingAtomizer),

        new QuestObjective(ws)
            .Named("Find key")
            .WithConnection(V3(117.31f, -3.71f, 36.29f))
            .Hints(FindkeyHints)
            .CompleteOnDestroyed((uint)OID.IdentificationKey),

        new QuestObjective(ws)
            .Named("Free Raubahn")
            .WithConnections(
                new Waypoint(new(-30.25f, 0.14f, -132.16f)),
                new Waypoint(new(-42.44f, -10.85f, -122.70f), false),
                new Waypoint(new(-86.78f, -10.18f, -96.53f))
            )
            .WithInteract(0x1E9D3C)
    ];

    private void FindkeyHints(Actor player, AIHints hints)
    {
        var inCombat = false;
        foreach (var h in hints.PotentialTargets)
        {
            if (CrystalBraves.Contains(h.Actor.OID))
            {
                h.Priority = 0;
                inCombat = true;
            }
        }

        if (!inCombat)
            hints.InteractWithOID(World, OID.IdentificationKey);
    }
}
