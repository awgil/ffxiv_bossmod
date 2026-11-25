namespace BossMod.QuestBattle.Stormblood.ClassJobQuests.RDM.WithHeartAndSteel;

public enum OID : uint
{
    MhachiBhoot = 0x1AF0,
    MhachiGhost = 0x1AEF,
    Parthenope = 0x1AF1,
    Lilith = 0x1AD9,
}

[ZoneModuleInfo(245, territoryID: 707)]
internal class WithHeartAndSteel(WorldState ws) : QuestBattle(ws)
{
    private static Waypoint W(float x, float y, float z) => new(new(x, y, z), false);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [
        QuestObjective.Combat(ws)
            .Named("First ghosts")
            .WithConnections(W(-258.7f, -114.66f, -228.9f), W(-269f, -113.17f, -231f), W(-279.2f, -110.88f, -233f), W(-289.4f, -111.77f, -235f), W(-299.2f, -112.35f, -238.4f)),

        QuestObjective.Combat(ws)
            .Named("Parthenope")
            .WithConnections(W(-309.5f, -113.62f, -240.7f), W(-319.5f, -114.5f, -243.6f), W(-329.3f, -115.46f, -247.3f), W(-343.4f, -116f, -253.6f))
            .Hints((player, hints) => hints.PrioritizeTargetsByOID((uint)OID.Parthenope, 0)),

        new QuestObjective(ws)
            .Named("Climb to the circle")
            .WithConnections(W(-353f, -116f, -256.8f), W(-353.6f, -116f, -272.2f), W(-353.2f, -116.4f, -282.6f), W(-350.3f, -117.05f, -292.3f), W(-345.5f, -117.55f, -292.5f),
                W(-335.6f, -119.22f, -294.9f), W(-325.5f, -120.64f, -297.3f), W(-315.6f, -122f, -298.4f), W(-305.9f, -123.4f, -301.8f), W(-302.9f, -123f, -311.8f),
                W(-301.5f, -121.73f, -322.1f), W(-300.1f, -122.54f, -332.5f), W(-297.8f, -121.81f, -337f), W(-300f, -116.94f, -344.6f), W(-302.3f, -115.38f, -342.4f), W(-303.8f, -114.3f, -340.2f))
            .With(obj => obj.Update += () => obj.CompleteIf(ws.Party.Player()?.PosRot.Y > -90)),

        new QuestObjective(ws)
            .Named("Lilith")
            .WithConnections(W(-300.1f, -74.95f, -365.5f), W(-300.1f, -74.11f, -397.3f))
            .Hints((player, hints) => hints.PrioritizeTargetsByOID((uint)OID.Lilith, 0))
            .CompleteOnKilled((uint)OID.Lilith)
    ];
}
