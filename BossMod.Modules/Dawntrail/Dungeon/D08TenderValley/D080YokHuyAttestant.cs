namespace BossMod.Dawntrail.Dungeon.D08TenderValley.D080YokHuyAttestant;

public enum OID : uint
{
    Boss = 0x4253,
    YokHuyOrb = 0x424F,
    YokHuyAltar = 0x4251,
    YokHuyAltar2 = 0x448D,
    BlackenedStatue = 0x4254, // R0.500, x6, fire Ancient Wrath across the room, teleport between the west and east wall
}

public enum AID : uint
{
    AutoAttack = 872,

    SunToss = 38539,
    BoulderToss = 38540,
    TectonicShift = 39221,
    AncientWrath = 38538, // Boss->self, 6.0s cast, single-target, visual (statue lines)
    AncientWrathShort = 39823, // BlackenedStatue->self, no cast, range 12 width 8 rect
    AncientWrathMedium = 39824, // BlackenedStatue->self, no cast, range 22 width 8 rect
    AncientWrathLong = 39825, // BlackenedStatue->self, no cast, range 35 width 8 rect
}

class SunToss(BossModule module) : Components.StandardAOEs(module, AID.SunToss, 6);
class BoulderToss(BossModule module) : Components.SingleTargetDelayableCast(module, AID.BoulderToss);
class TectonicShift(BossModule module) : Components.StandardAOEs(module, AID.TectonicShift, 8);

class AncientWrath(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;

    private static readonly AOEShapeRect _long = new(35, 4);
    private static readonly AOEShapeRect _medium = new(22, 4);
    private static readonly AOEShapeRect _short = new(12, 4);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;
        foreach (var s in Module.Enemies(OID.BlackenedStatue))
            yield return new(Shape(s.Position), s.Position, s.Rotation, _activation);
    }

    private static AOEShapeRect Shape(WPos statue)
    {
        var west = statue.X < -130;
        return statue.Z switch
        {
            > -474 and < -469 => west ? _medium : _short,
            > -489 and < -484 => west ? _short : _medium,
            _ => _long
        };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AncientWrath)
            _activation = Module.CastFinishAt(spell, 2.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AncientWrathShort or AID.AncientWrathMedium or AID.AncientWrathLong)
        {
            ++NumCasts;
            _activation = default;
        }
    }
}

class D080YokHuyAttestantStates : StateMachineBuilder
{
    public D080YokHuyAttestantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SunToss>()
            .ActivateOnEnter<BoulderToss>()
            .ActivateOnEnter<TectonicShift>()
            .ActivateOnEnter<AncientWrath>();
    }
}

[ModuleInfo(Contributors = "CerQ, Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 834, NameID = 12801, SortOrder = -1)]
public class D080YokHuyAttestant(ModuleInit init) : BossModule(init, new(-130, -475.25f), new ArenaBoundsRect(17.5f, 24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.YokHuyOrb), ArenaColor.Object);
        Arena.Actors(Enemies(OID.YokHuyAltar), ArenaColor.Object);
        Arena.Actors(Enemies(OID.YokHuyAltar2), ArenaColor.Object);
    }
}
