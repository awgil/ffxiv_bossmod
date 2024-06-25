namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D041Lyssa;

public enum OID : uint
{
    Boss = 0x3323, // R=4.0
    Helper = 0x233C,
    IcePillar = 0x3324, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    FrigidStomp = 25181, // Boss->self, 5.0s cast, range 50 circle, raidwide
    FrostbiteAndSeek1 = 28304, // Helper->self, no cast, single-target
    FrostbiteAndSeek2 = 25175, // Boss->self, 3.0s cast, single-target
    HeavySmash = 25180, // Boss->players, 5.0s cast, range 6 circle, stack
    IcePillar = 25179, // IcePillar->self, 3.0s cast, range 4 circle
    Icicall = 25178, // Boss->self, 3.0s cast, single-target
    PillarPierceAOE = 25375, // IcePillar->self, 5.0s cast, range 80 width 4 rect
    PunishingSliceVisual = 25176, // Boss->self, no cast, single-target
    PunishingSliceAOE = 25177, // Helper->self, 2.0s cast, range 50 width 50 rect
    SkullDasher = 25182, // Boss->player, 5.0s cast, single-target, tankbuster

}

class PillarPierceAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarPierceAOE), new AOEShapeRect(40, 2));

class PunishingSlice(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(50, 25);

    private static readonly Dictionary<(byte index, uint state), (WPos origin, Angle rotation)> aoeSources = new()
    {
        {(0x00, 0x00200010), (new WPos(-154.825f, 42.75f), 60.Degrees())},
        {(0x00, 0x01000080), (new WPos(-154.825f, 55.25f), 119.997f.Degrees())},
        {(0x00, 0x00020001), (new WPos(-144, 36.5f), -0.003f.Degrees())},
        {(0x01, 0x00200010), (new WPos(-144, 61.5f), -180.Degrees())},
        {(0x01, 0x01000080), (new WPos(-133.175f, 55.25f), -120.003f.Degrees())},
        {(0x01, 0x00020001), (new WPos(-133.175f, 42.75f), -60.005f.Degrees())}
    };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (aoeSources.TryGetValue((index, state), out var source))
        {
            var activation = NumCasts == 0 ? Module.WorldState.FutureTime(13) : Module.WorldState.FutureTime(16);
            _aoe = new AOEInstance(rect, source.origin, source.rotation, activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.PunishingSliceAOE)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}

class IcePillar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IcePillar), new AOEShapeCircle(4));
class HeavySmash(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HeavySmash), 6, 4);
class SkullDasher(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.SkullDasher));
class FrigidStomp(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FrigidStomp));

class D041LyssaStates : StateMachineBuilder
{
    public D041LyssaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PillarPierceAOE>()
            .ActivateOnEnter<PunishingSlice>()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<HeavySmash>()
            .ActivateOnEnter<SkullDasher>()
            .ActivateOnEnter<FrigidStomp>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10396)]
public class D041Lyssa(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly List<Shape> union = [new Circle(new(-144, 49), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(-144, 28), 20, 2), new Rectangle(new(-144, 70), 20, 2)];
    public static readonly ArenaBoundsComplex DefaultBounds = new(union, difference);
}
