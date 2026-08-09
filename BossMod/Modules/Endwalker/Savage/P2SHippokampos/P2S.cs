namespace BossMod.Endwalker.Savage.P2SHippokampos;

sealed class DoubledImpact(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.DoubledImpact, 6f);
sealed class SewageEruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SewageEruptionAOE, 6f);
// state related to sewage deluge mechanic
sealed class SewageDeluge(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly Shape[] shapes = [new Square(new(90.5f, 90.5f), 4.5f), new Square(new(109.5f, 90.5f), 4.5f), new Square(new(90.5f, 109.5f), 4.5f), new Square(new(109.5f, 109.5f), 4.5f),
    new Rectangle(new(100f, 90.5f), 5f, 2f), new Rectangle(new(100f, 109.5f), 5f, 2f), new Rectangle(new(90.5f, 100f), 2f, 5f), new Rectangle(new(109.5f, 100f), 2f, 5f)];
    private int seenEvents;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        // 800375A2: we typically get two events for index=0 (global) and index=N (corner)
        // state 00200010 - "prepare" (show aoe that is still harmless)
        // state 00020001 - "active" (dot in center/borders, oneshot in corner)
        // state 00080004 - "finish" (reset)

        switch (state)
        {
            case 0x00200010u:
                if (index is > 0 and < 5)
                {
                    if (shapes[index - 1] is Square sq)
                    {
                        sq.HalfHeight = sq.HalfWidth = default;
                        ++seenEvents;
                    }
                }
                else if (index == 0)
                {
                    ++seenEvents;
                }
                if (seenEvents > 1)
                {
                    var center = Arena.Center;
                    var shape = new AOEShapeCustom(center, [new Rectangle(new(100f, 100f), 17.5f, 22.5f)], shapes);
                    _aoe = [new(shape, center, activation: WorldState.FutureTime(7.9d), shapeDistance: shape.Distance(center, default))];
                }
                break;
            case 0x0020001u:
                _aoe = [];
                Arena.Bounds = new ArenaBoundsCustom(shapes);
                break;
            case 0x00080004u:
                Arena.Bounds = new ArenaBoundsRect(17.5f, 22.5f);
                if (index is > 0 and < 5)
                {
                    if (shapes[index - 1] is Square sq)
                    {
                        seenEvents = 0;
                        sq.HalfHeight = sq.HalfWidth = 4.5f;
                    }
                }
                break;
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 811u, NameID = 10348u, PlanLevel = 90)]
public sealed class P2S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsRect(17.5f, 22.5f));
