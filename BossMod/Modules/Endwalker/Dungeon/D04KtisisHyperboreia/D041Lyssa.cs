namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D041Lyssa;

public enum OID : uint
{
    Boss = 0x3323, // R4.000, x1
}

public enum AID : uint
{
    SkullDasher = 25182, // Boss->none, 5.0s cast, single-target
    PunishingSlice = 25177, // _Gen_Lyssa->self, 2.0s cast, range 50 width 50 rect
    IcePillar = 25179, // _Gen_IcePillar->self, 3.0s cast, range 4 circle
    FrigidStomp = 25181, // Boss->self, 5.0s cast, range 50 circle
    PillarPierce = 25375, // _Gen_IcePillar->self, 5.0s cast, range 80 width 4 rect
    HeavySmash = 25180, // Boss->none, 5.0s cast, range 6 circle
}

class SkullDasher(BossModule module) : Components.SingleTargetCast(module, AID.SkullDasher);

class PunishingSlice(BossModule module) : Components.GenericAOEs(module, AID.PunishingSlice)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect shape = new(50, 25);

    private static readonly Dictionary<(byte index, uint state), Angle> aoeSources = new()
    {
        {(0x00, 0x00020001), default},
        {(0x00, 0x00200010), 60.Degrees()},
        {(0x00, 0x01000080), 120.Degrees()},
        {(0x01, 0x00200010), 180.Degrees()},
        {(0x01, 0x01000080), 240.Degrees()},
        {(0x01, 0x00020001), 300.Degrees()}
    };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => Utils.ZeroOrOne(_aoe);

    public override void OnMapEffect(byte index, uint state)
    {
        if (aoeSources.TryGetValue((index, state), out var rotation))
        {
            var activation = NumCasts == 0 ? Module.WorldState.FutureTime(13) : Module.WorldState.FutureTime(16);
            var aoeSource = Arena.Center - rotation.ToDirection() * 12.5f;
            _aoe = new AOEInstance(shape, aoeSource, rotation, activation);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}

class FrigidStomp(BossModule module) : Components.RaidwideCast(module, AID.FrigidStomp);
class IcePillar(BossModule module) : Components.StandardAOEs(module, AID.IcePillar, new AOEShapeCircle(4));
class PillarPierce(BossModule module) : Components.StandardAOEs(module, AID.PillarPierce, new AOEShapeRect(80, 2));
class HeavySmash(BossModule module) : Components.StackWithCastTargets(module, AID.HeavySmash, 6, 4);

class D041LyssaStates : StateMachineBuilder
{
    public D041LyssaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SkullDasher>()
            .ActivateOnEnter<PunishingSlice>()
            .ActivateOnEnter<FrigidStomp>()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<HeavySmash>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10396)]
public class D041Lyssa(WorldState ws, Actor primary) : BossModule(ws, primary, new(-144, 49), new ArenaBoundsCircle(19.5f));
