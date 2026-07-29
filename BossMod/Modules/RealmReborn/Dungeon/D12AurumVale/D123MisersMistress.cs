namespace BossMod.RealmReborn.Dungeon.D12AurumVale.D123MisersMistress;

public enum OID : uint
{
    MisersMistress = 0x4B0E, // R3.85
    MorbolFruit1 = 0x1E8EE3, // R0.5
    MorbolFruit2 = 0x1E8EE1, // R0.5
    MorbolFruit3 = 0x1E8EDE, // R0.5
    MorbolFruit4 = 0x1E8EDF, // R0.5
    MorbolFruit5 = 0x1E8EE0, // R0.5
    MorbolFruit6 = 0x1E8EE2, // R0.5
    MorbolFruit = 0x4B10, // R0.6-1.8
    MorbolSeedling = 0x4B0F, // R0.9
}

public enum AID : uint
{
    AutoAttack1 = 1350, // MisersMistress->player, no cast, single-target
    AutoAttack2 = 1041, // MorbolSeedling->player, no cast, single-target

    VineProbe = 45997, // MisersMistress->player, 5.0s cast, single-target
    BadBreath = 45996, // MisersMistress->self, 4.0s cast, range 12+R 120-degree cone
    BurrBurrow = 45998, // MisersMistress->self, 3.0s cast, ???
    HookedBurrs = 45999, // MisersMistress->player, no cast, single-target
    Sow = 46000, // MisersMistress->self, 3.0s cast, single-target
    Germinate = 46001 // MorbolFruit->self, 15.0s cast, single-target
}

public enum SID : uint
{
    Burrs = 303 // Boss->player, extra=0x1/0x2/0x3
}

[SkipLocalsInit]
sealed class BurrBurrow(BossModule module) : Components.RaidwideCast(module, (uint)AID.BurrBurrow);

[SkipLocalsInit]
sealed class VineProbe(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.VineProbe);

[SkipLocalsInit]
sealed class BadBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BadBreath, new AOEShapeCone(15.85f, 60f.Degrees()));

[SkipLocalsInit]
sealed class MorbolFruit(BossModule module) : BossComponent(module)
{
    private BitMask burrs;
    private List<Actor> _fruits = [with(6)];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Burrs && status.Extra > 0x02)
        {
            burrs.Set(Raid.FindSlot(actor.InstanceID));
            if (_fruits.Count != 6)
            {
                _fruits = Module.Enemies([(uint)OID.MorbolFruit1, (uint)OID.MorbolFruit2, (uint)OID.MorbolFruit3, (uint)OID.MorbolFruit4,
                (uint)OID.MorbolFruit5, (uint)OID.MorbolFruit6]);
            }
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Burrs && status.Extra > 0x02)
        {
            burrs.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!burrs[slot])
        {
            return;
        }

        Actor? closest = null;
        var minDistSq = float.MaxValue;

        var count = _fruits.Count;
        for (var i = 0; i < count; ++i)
        {
            var fruit = _fruits[i];
            if (fruit.IsTargetable)
            {
                hints.GoalZones.Add(AIHints.GoalSingleTarget(fruit, 2f, 5f));
                var distSq = (actor.Position - fruit.Position).LengthSq();
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closest = fruit;
                }
            }
        }
        hints.InteractWithTarget = closest;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!burrs[slot])
        {
            return;
        }

        var count = _fruits.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_fruits[i].IsTargetable)
            {
                hints.Add("Eat a fruit to cleanse debuff!");
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!burrs[pcSlot])
        {
            return;
        }

        var count = _fruits.Count;
        for (var i = 0; i < count; ++i)
        {
            var fruit = _fruits[i];
            if (fruit.IsTargetable)
            {
                Arena.ZoneCircleOutline(fruit.Position, 3f, Colors.Safe);
            }
        }
    }
}

[SkipLocalsInit]
sealed class D123MisersMistressStates : StateMachineBuilder
{
    public D123MisersMistressStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MorbolFruit>()
            .ActivateOnEnter<BurrBurrow>()
            .ActivateOnEnter<VineProbe>()
            .ActivateOnEnter<BadBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport,
StatesType = typeof(D123MisersMistressStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = null,
PrimaryActorOID = (uint)OID.MisersMistress,
Contributors = "The Combat Reborn Team (Malediktus)",
Expansion = BossModuleInfo.Expansion.RealmReborn,
Category = BossModuleInfo.Category.Dungeon,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 5u,
NameID = 1532u,
SortOrder = 3,
PlanLevel = 0)]
[SkipLocalsInit]
public sealed class D123MisersMistress : BossModule
{
    public D123MisersMistress(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D123MisersMistress(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices =
        [
            new(-390.6f, -142.52f), new(-390.05f, -142.12f), new(-389.51f, -141.84f), new(-388.26f, -141.72f), new(-387.03f, -141.22f),
            new(-386.41f, -141.16f), new(-385.89f, -140.77f), new(-385.31f, -140.56f), new(-384.72f, -140.55f), new(-383.84f, -140.67f),
            new(-383.32f, -140.25f), new(-382.26f, -139.66f), new(-381.04f, -139.47f), new(-379.84f, -139.63f), new(-378.07f, -140.6f),
            new(-370.65f, -137.12f), new(-369.7f, -136.22f), new(-369.82f, -135.61f), new(-370.69f, -134.65f), new(-370.98f, -134.05f),
            new(-371.18f, -133.4f), new(-371.47f, -132.8f), new(-372.21f, -131.71f), new(-372.03f, -131.13f), new(-371.75f, -130.51f),
            new(-371.98f, -129.82f), new(-372.41f, -129.26f), new(-372.56f, -128.68f), new(-372.47f, -127.96f), new(-372.19f, -127.4f),
            new(-372.47f, -126.91f), new(-376.16812f, -125.205475f), new(-376.40543f, -125.7156f), new(-377f, -126.00012f), new(-377.5708f, -125.63572f),
            new(-377.9187f, -125.50009f), new(-378.14816f, -125.00005f), new(-378.38162f, -124.29889f), new(-379f, -124.00009f), new(-379.42154f, -123.50002f),
            new(-379.50006f, -123.00006f), new(-379.3999f, -122.50011f), new(-379.50006f, -122.00003f), new(-380f, -121.50021f), new(-380.50006f, -121.572f),
            new(-381f, -121.50021f), new(-381.50006f, -121.34563f), new(-382.0001f, -121.00026f), new(-382.36124f, -120.97006f), new(-382.59027f, -120.61028f),
            new(-382.62512f, -120.20206f), new(-382.43494f, -119.65919f), new(-382.25778f, -119.09669f), new(-382.00006f, -118.78616f), new(-381.50006f, -118.50008f),
            new(-381.06393f, -118.06393f), new(-381.19f, -117.85f), new(-381.34f, -117.33f), new(-381.54f, -116.85f), new(-383.29f, -115.8f),
            new(-383.78f, -115.32f), new(-384.48f, -115.36f), new(-385.07f, -115.63f), new(-385.53f, -116.17f), new(-385.48f, -116.83f),
            new(-385.66f, -117.37f), new(-386.88f, -118.89f), new(-387.42f, -119.23f), new(-387.97f, -119.47f), new(-389.22f, -119.13f),
            new(-390.12f, -118.26f), new(-390.58f, -117.04f), new(-391.03f, -116.56f), new(-391.33f, -116.02f), new(-391.58f, -115.43f),
            new(-392.06f, -114.98f), new(-392.58f, -114.85f), new(-392.95f, -114.38f), new(-393.22f, -113.77f), new(-393.78f, -113.31f),
            new(-394.38f, -113.15f), new(-394.8163f, -112.84793f), new(-395.00006f, -113.00005f), new(-396.00018f, -113.00005f), new(-397.0001f, -113.5827f),
            new(-398.00003f, -114.00008f), new(-399f, -113.56828f), new(-400f, -111.99994f), new(-402.00006f, -109.99999f), new(-403.86694f, -109.27551f),
            new(-404.34796f, -108.46222f), new(-405.00003f, -108.14293f), new(-406f, -107.77205f), new(-407f, -107.84482f), new(-407.72943f, -108.15604f),
            new(-408.00003f, -108.18188f), new(-408.24945f, -108.2699f), new(-409.06046f, -109.00005f), new(-410.00006f, -109.3978f), new(-412f, -109.86716f),
            new(-414.00012f, -109.87968f), new(-415f, -109.87964f), new(-416f, -110f), new(-417.00006f, -110.23241f), new(-417.8684f, -110.75179f),
            new(-418.30035f, -111.50354f), new(-419f, -112f), new(-420f, -112.24489f), new(-421.00003f, -112.00015f), new(-422f, -112.00015f),
            new(-422.6829f, -112.19093f), new(-423.15103f, -112.41593f), new(-423.62204f, -112.0659f), new(-423.9813f, -111.67542f), new(-425.0496f, -110.63678f),
            new(-425.6011f, -110.19748f), new(-425.7f, -110.28f), new(-426.15f, -110.82f), new(-426.23f, -111.48f), new(-426.46f, -112.09f),
            new(-426.79f, -112.69f), new(-426.42f, -113.9f), new(-426.67f, -114.48f), new(-426.88f, -115.18f), new(-426.88f, -115.7f),
            new(-427.45f, -116.8f), new(-427.91f, -117.1f), new(-428.55f, -117.32f), new(-428.95f, -117.62f), new(-429.41f, -118.13f),
            new(-429.77f, -118.69f), new(-429.97f, -119.33f), new(-429.6f, -119.69f), new(-429.35f, -120.27f), new(-428.91f, -120.85f),
            new(-428f, -121.65f), new(-427.73f, -122.24f), new(-427.53f, -122.87f), new(-427.71f, -123.41f), new(-428.20166f, -124f),
            new(-428f, -124f), new(-427.00006f, -123.76939f), new(-426.04962f, -123.00018f), new(-425.62384f, -123f), new(-425.3312f, -122.99972f),
            new(-425.08398f, -122.49966f), new(-424.8366f, -122f), new(-424.90833f, -121.28485f), new(-424.53674f, -120.86774f), new(-424.00018f, -120.59009f),
            new(-423.32547f, -120.5816f), new(-422.6507f, -120.65009f), new(-422.00003f, -121.12601f), new(-421.39584f, -121.3885f), new(-421f, -122.00005f),
            new(-420.70148f, -123f), new(-421f, -124.00008f), new(-421.5678f, -124.41571f), new(-422.00003f, -125.00002f), new(-422.6972f, -125.23663f),
            new(-423.39456f, -125.33611f), new(-424.00018f, -125.87007f), new(-424.79144f, -126.00014f), new(-425.2715f, -125.49867f), new(-425.46405f, -124.99983f),
            new(-425.7389f, -125f), new(-426.07205f, -125.00015f), new(-427f, -125f), new(-427.99985f, -125.774025f), new(-427.89f, -126.11f),
            new(-428.47f, -127.25f), new(-428.68f, -128.32f), new(-428.16f, -128.76f), new(-427.67f, -129.25f), new(-427.01f, -129.37f),
            new(-426.43f, -129.62f), new(-425.89f, -129.92f), new(-425.26f, -130.13f), new(-424.83f, -130.53f), new(-424.14f, -130.72f),
            new(-423.85986f, -130.85469f), new(-423.59464f, -130.28458f), new(-423.00006f, -130.00003f), new(-422.4293f, -130.36441f), new(-422.0813f, -130.5f),
            new(-421.85193f, -131f), new(-421.61838f, -131.70111f), new(-421.00006f, -132f), new(-420.57855f, -132.50003f), new(-420.5f, -133f),
            new(-420.6001f, -133.50002f), new(-420.50003f, -134.00003f), new(-420f, -134.49985f), new(-419.50006f, -134.42809f), new(-419.00003f, -134.49991f),
            new(-418.50006f, -134.65442f), new(-418.00003f, -134.9998f), new(-417.89236f, -135.49991f), new(-418f, -136.00006f), new(-418.50003f, -136.50003f),
            new(-418.62662f, -137.00002f), new(-418.50003f, -137.50006f), new(-419f, -138f), new(-419.28796f, -138.48657f), new(-419.4225f, -138.9732f),
            new(-419.34567f, -139.4879f), new(-419.3049f, -140.00264f), new(-419.11935f, -140.66727f), new(-417.97f, -140.26f), new(-416.6f, -140.05f),
            new(-416.14f, -139.6f), new(-415.63f, -139.27f), new(-413.86f, -138.71f), new(-413.25f, -138.77f), new(-412.66f, -138.58f),
            new(-412.12f, -138.82f), new(-411.64f, -139.14f), new(-411.16f, -139.65f), new(-410.65f, -139.89f), new(-410.12f, -140.34f),
            new(-409.43f, -140.34f), new(-409.21f, -141.03f), new(-408.07f, -141.58f), new(-407.37f, -141.49f), new(-406.52f, -138.39f),
            new(-406.06f, -138.07f), new(-405.43f, -137.9f), new(-404.78f, -137.83f), new(-404.09f, -137.92f), new(-403.58f, -137.93f),
            new(-403.03f, -137.86f), new(-402.46f, -138.13f), new(-400.99f, -140.5f), new(-400.77f, -141.06f), new(-400.67f, -141.67f),
            new(-400.34f, -142.29f), new(-399.77f, -142.76f), new(-399.14f, -143.04f), new(-398.71f, -143.54f), new(-398.07f, -143.6f),
            new(-397.61f, -143.12f), new(-396.57f, -141.51f), new(-396.50568f, -141.44095f), new(-396.88287f, -140.7056f), new(-396.99234f, -140.00006f),
            new(-396.76154f, -139.09293f), new(-396.5696f, -138f), new(-396.30475f, -137.00008f), new(-395.96588f, -136.71196f), new(-395.53174f, -136.55923f),
            new(-395.0764f, -135.99971f), new(-394.54355f, -135.80574f), new(-394.0107f, -136.00015f), new(-393.53793f, -137.11462f), new(-393.4109f, -138.00005f),
            new(-393.22437f, -138.5f), new(-392.851f, -139f), new(-392.83902f, -140f), new(-393.3309f, -141.16708f), new(-393.78757f, -141.23047f),
            new(-393.27f, -141.99f), new(-393.1f, -142.76f), new(-392.83f, -143.4f), new(-392.4f, -143.93f), new(-391.8f, -144.32f),
        ];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.MorbolFruit or (uint)OID.MorbolSeedling => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.MorbolFruit));
        Arena.Actors(Enemies((uint)OID.MorbolSeedling));
    }
}
