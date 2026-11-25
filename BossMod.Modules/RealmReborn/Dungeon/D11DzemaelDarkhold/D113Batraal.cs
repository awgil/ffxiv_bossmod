namespace BossMod.RealmReborn.Dungeon.D11DzemaelDarkhold.D113Batraal;

public enum OID : uint
{
    Boss = 0x4A8D, // R4.600
    CorruptedCrystal = 0x4A8E, // R1.000
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    GrimCleaver = 45577, // Boss->player, no cast, single-target
    GrimFate = 45578, // Boss->player, no cast, single-target
    GrimHalo = 45579, // Boss->self, 4.7s cast, range 12 circle
    Hellssend = 45580, // Boss->self, 2.7s cast, single-target visual
    CorruptedCrystals = 45581, // Boss->CorruptedCrystal, no cast, single-target
    Desolation = 45582, // Boss->self, 3.0s cast, range 60 width 6 rect
}

class GrimHalo(BossModule module) : Components.StandardAOEs(module, AID.GrimHalo, 12);
class Desolation(BossModule module) : Components.StandardAOEs(module, AID.Desolation, new AOEShapeRect(60, 3));
class Crystals(BossModule module) : Components.Adds(module, (uint)OID.CorruptedCrystal, 2);

class D113BatraalStates : StateMachineBuilder
{
    public D113BatraalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrimHalo>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<Crystals>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 13, NameID = 1396)]
public class D113Batraal(ModuleInit init) : BossModule(init, new(80.5f, -175.5f), CaveBounds)
{
    private static readonly ArenaBoundsCustom CaveBounds = new(30, new([new RelPolygonWithHoles([
        new(-6.36f, -28.31f), new(-3.82f, -29.08f), new(-2.72f, -27.7f), new(-1.5f, -27.5f), new(-0.23f, -27.71f), new(0.59f, -28.54f), new(1.84f, -28.29f), new(2.62f, -29.08f),
        new(5.96f, -27.86f), new(6.26f, -25.15f), new(5, -22.86f), new(5, -21.12f), new(6, -19.38f), new(6, -13.91f), new(3.28f, -12.82f), new(2.75f, -12.3f),
        new(2.75f, -10.16f), new(4.17f, -8.03f), new(5.53f, -6.66f), new(7.89f, -5.25f), new(11.11f, -5.25f), new(12.44f, -6.05f), new(13.79f, -5.59f), new(14.23f, -6.25f),
        new(15.77f, -6.25f), new(16.86f, -4.61f), new(17.64f, -4.09f), new(18.55f, -2.27f), new(18.01f, -0.94f), new(18.23f, 0.78f), new(19.99f, 2.54f), new(21.07f, 2.82f),
        new(21.99f, 4.34f), new(22.27f, 7.14f), new(21.5f, 8.16f), new(21.5f, 9.4f), new(22.18f, 11.23f), new(23.7f, 12.74f), new(24.07f, 14.22f), new(23.25f, 14.77f),
        new(23.25f, 15.34f), new(24.37f, 17.03f), new(23, 18.67f), new(23, 19.54f), new(24.66f, 21.2f), new(21.45f, 25.57f), new(18.47f, 24.75f), new(16.1f, 25.02f),
        new(13.75f, 23.19f), new(12.14f, 22.5f), new(10.19f, 22.5f), new(9.86f, 22), new(9.58f, 22.85f), new(8.02f, 22.46f), new(6.31f, 21.04f), new(5.55f, 18.5f),
        new(4.29f, 17.23f), new(2.07f, 17.01f), new(0.32f, 17.66f), new(-0.7f, 19.1f), new(0.01f, 21.7f), new(-0.26f, 25.16f), new(-1.87f, 27.85f), new(-3.84f, 29.25f),
        new(-6.8f, 29.25f), new(-9.2f, 28.72f), new(-12.4f, 26.5f), new(-13.61f, 26.5f), new(-14.86f, 27.25f), new(-16.61f, 27.25f), new(-18.91f, 26.22f), new(-20.24f, 26),
        new(-20.8f, 26.1f), new(-22.17f, 24.68f), new(-25.11f, 20.95f), new(-23.67f, 17.46f), new(-21.2f, 15.24f), new(-20.77f, 13.31f), new(-21.77f, 11.32f), new(-21.46f, 9.77f),
        new(-19.5f, 7.56f), new(-19.5f, 6.16f), new(-18.18f, 5.63f), new(-17.96f, 4.5f), new(-17.18f, 3.72f), new(-15.67f, 3.93f), new(-15.43f, 2.25f), new(-14.09f, 2.25f),
        new(-13.58f, 0.97f), new(-10.29f, 1.24f), new(-8.85f, 0.83f), new(-7.23f, -1.2f), new(-7.01f, -3.91f), new(-7.67f, -5.47f), new(-10.54f, -8.82f), new(-12.82f, -9.58f),
        new(-13.77f, -11), new(-14.21f, -11), new(-15.96f, -12.76f), new(-18.95f, -13.02f), new(-20.69f, -14.5f), new(-22.71f, -14.5f), new(-23.5f, -15.29f), new(-23.5f, -16.79f),
        new(-24.75f, -18.04f), new(-24.75f, -19.67f), new(-23.25f, -21.66f), new(-23.25f, -22.43f), new(-20.96f, -24.96f), new(-19.44f, -26.37f), new(-18.61f, -26.29f), new(-18.31f, -27.35f),
        new(-17.23f, -28.19f), new(-16.25f, -28.53f), new(-14.01f, -27.77f), new(-10.51f, -29.03f), new(-8.34f, -28.3f), new(-7.2f, -29.16f)
    ])]));
}
