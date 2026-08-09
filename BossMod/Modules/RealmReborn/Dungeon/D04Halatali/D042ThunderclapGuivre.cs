namespace BossMod.RealmReborn.Dungeon.D04Halatali.D042ThunderclapGuivre;

public enum OID : uint
{
    Boss = 0x4644,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    Electrify = 40595, // Boss->location, 4.0s cast, range 6 circle

    HydroelectricShockVisual = 40593, // Boss->self, 9.0+1,0s cast, single-target
    HydroelectricShock = 41113, // Helper->self, 10.0s cast, ???

    Levinfang = 40594 // Boss->player, 5.0s cast, single-target
}

sealed class Levinfang(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Levinfang);
sealed class Electrify(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Electrify, 6f);
sealed class HydroelectricShock(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydroelectricShock)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, D042ThunderclapGuivre.GetArenaPoly(),
            [new PolygonCustom([new(-188.248f, -101.035f), new(-187.247f, -107.031f), new(-186.281f, -111.518f),
            new(-184.045f, -113.744f), new(-178.523f, -112.543f), new(-173.155f, -107.977f), new(-175.178f, -97.181f)]),
            new PolygonCustom([new(-177.532f, -166.762f), new(-177.432f, -162.877f), new(-175.846f, -159.154f),
            new(-174.737f, -155.21f), new(-173.449f, -154.881f), new(-167.214f, -153.974f), new(-167.644f, -172.626f)])]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HydroelectricShock)
        {
            _aoe = [];
        }
    }
}

sealed class D042ThunderclapGuivreStates : StateMachineBuilder
{
    public D042ThunderclapGuivreStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Levinfang>()
            .ActivateOnEnter<Electrify>()
            .ActivateOnEnter<HydroelectricShock>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 7u, NameID = 1196u)]
public sealed class D042ThunderclapGuivre : BossModule
{
    public D042ThunderclapGuivre(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D042ThunderclapGuivre(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static PolygonCustom[] GetArenaPoly()
    => [new([new(-176.42f, -166.47f), new(-175.16f, -165.74f), new(-174.56f, -165.61f), new(-171.93f, -165.35f), new(-172.02f, -154.56f),
    new(-171.71f, -153.93f), new(-167.56f, -149.39f), new(-167.15f, -149.07f), new(-166.72f, -148.80f), new(-166.26f, -148.57f),
    new(-165.11f, -148.51f), new(-164.51f, -148.19f), new(-164.01f, -147.81f), new(-163.59f, -147.36f), new(-162.50f, -145.84f),
    new(-162.25f, -145.36f), new(-162.06f, -144.90f), new(-160.87f, -140.24f), new(-160.33f, -139.90f), new(-159.34f, -139.61f),
    new(-158.82f, -139.40f), new(-158.30f, -138.48f), new(-157.96f, -138.12f), new(-157.38f, -137.84f), new(-156.28f, -138.09f),
    new(-155.58f, -138.15f), new(-155.05f, -137.80f), new(-154.66f, -137.41f), new(-154.35f, -136.96f), new(-154.08f, -136.26f),
    new(-153.92f, -135.61f), new(-153.81f, -135.00f), new(-153.81f, -134.35f), new(-154.01f, -133.71f), new(-154.41f, -133.14f),
    new(-154.92f, -132.68f), new(-155.35f, -132.41f), new(-156.07f, -132.19f), new(-156.62f, -132.15f), new(-157.35f, -132.17f),
    new(-157.86f, -132.09f), new(-158.55f, -131.31f), new(-158.85f, -130.89f), new(-159.11f, -130.44f), new(-159.31f, -129.97f),
    new(-159.60f, -128.97f), new(-159.62f, -128.30f), new(-161.07f, -122.73f), new(-161.32f, -122.11f), new(-161.50f, -121.51f),
    new(-161.73f, -120.95f), new(-162.36f, -118.56f), new(-164.02f, -115.32f), new(-165.81f, -113.24f), new(-166.31f, -112.86f),
    new(-166.90f, -112.63f), new(-167.58f, -112.53f), new(-171.24f, -113.31f), new(-176.57f, -113.39f), new(-177.17f, -113.49f),
    new(-178.81f, -114.31f), new(-179.46f, -114.52f), new(-180.09f, -114.80f), new(-180.51f, -115.30f), new(-181.01f, -115.60f),
    new(-181.59f, -115.67f), new(-182.20f, -115.64f), new(-182.66f, -115.33f), new(-182.83f, -114.72f), new(-180.98f, -110.77f),
    new(-180.64f, -110.24f), new(-177.35f, -108.46f), new(-174.83f, -107.50f), new(-174.44f, -106.99f), new(-176.20f, -99.92f),
    new(-176.51f, -99.44f), new(-178.18f, -100.23f), new(-182.03f, -101.58f), new(-185.18f, -103.04f), new(-185.82f, -103.20f),
    new(-186.53f, -103.02f), new(-187.17f, -103.21f), new(-191.06f, -105.86f), new(-191.42f, -106.26f), new(-192.24f, -107.60f),
    new(-193.13f, -108.41f), new(-193.41f, -108.85f), new(-194.39f, -109.85f), new(-194.83f, -110.16f), new(-195.43f, -110.22f),
    new(-196.04f, -110.23f), new(-197.35f, -110.04f), new(-198.06f, -110.05f), new(-198.62f, -110.47f), new(-199.10f, -110.92f),
    new(-199.57f, -111.50f), new(-200.26f, -112.53f), new(-200.46f, -113.02f), new(-200.82f, -113.98f), new(-204.44f, -122.82f),
    new(-204.58f, -123.51f), new(-204.48f, -124.02f), new(-204.18f, -124.70f), new(-203.98f, -125.37f), new(-203.08f, -130.67f),
    new(-202.82f, -131.36f), new(-202.34f, -131.92f), new(-200.27f, -133.92f), new(-199.85f, -134.50f), new(-199.49f, -135.14f),
    new(-199.52f, -135.78f), new(-198.40f, -138.02f), new(-198.65f, -138.68f), new(-199.02f, -139.32f), new(-199.95f, -140.55f),
    new(-200.19f, -141.04f), new(-200.09f, -141.57f), new(-197.46f, -148.56f), new(-197.16f, -149.17f), new(-196.84f, -149.62f),
    new(-196.46f, -149.98f), new(-192.39f, -152.30f), new(-191.85f, -152.80f), new(-191.54f, -153.21f), new(-187.54f, -160.57f),
    new(-187.21f, -161.09f), new(-186.84f, -161.56f), new(-186.53f, -162.21f), new(-185.97f, -162.58f), new(-184.71f, -163.00f),
    new(-184.36f, -163.42f), new(-183.75f, -163.80f), new(-183.25f, -163.72f), new(-182.87f, -163.36f), new(-182.40f, -162.81f),
    new(-181.87f, -162.35f), new(-181.23f, -162.01f), new(-180.55f, -161.73f), new(-179.85f, -161.66f), new(-178.65f, -161.43f),
    new(-178.12f, -161.82f), new(-177.76f, -162.17f), new(-177.33f, -162.71f), new(-177.00f, -166.15f), new(-176.69f, -166.59f)])];

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom(GetArenaPoly());
        return (arena.Center, arena);
    }
}
