#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.TreantPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x7, Helper type
    Boss = 0x4CCD, // R10.000, x1
    _Gen_SlugPiece = 0x4CCE, // R0.800, x0 (spawn during fight)
    _Gen_SaplingPiece = 0x4CCF, // R0.600-0.840, x0 (spawn during fight)

    BossZone = 0x1EABFA,
}

public enum AID : uint
{
    _Spell_Aero = 48624, // Boss->player, no cast, single-target
    _Weaponskill_RustlingBreeze = 48776, // Boss->self, 6.1+0.9s cast, single-target
    _Weaponskill_RustlingBreeze1 = 48778, // Helper->self, 7.0s cast, range 60 90-degree cone
    _Weaponskill_FallingLeaves = 48769, // Boss->self, 3.0s cast, single-target
    _AutoAttack_ = 49682, // 4CCE->player, no cast, single-target
    _Weaponskill_RustlingBreeze2 = 48777, // Boss->self, 6.2+0.8s cast, single-target
    _Weaponskill_RustlingBreeze3 = 48779, // Helper->self, 7.0s cast, range 60 150-degree cone
    _Weaponskill_RustlingBreeze4 = 48780, // Helper->self, 7.0s cast, range 60 150-degree cone
    _Weaponskill_AqueousDischarge = 48783, // 4CCE->self, 2.5s cast, range 5 circle
    _Spell_Stone = 48625, // 4CCF->player, no cast, single-target
    _Ability_GrabAndGrow = 48786, // 4CCF->Boss, no cast, single-target
    _Weaponskill_ArborealStorm = 48770, // Boss->self, 4.3+0.7s cast, single-target
    _Weaponskill_ArborealStorm5 = 48771, // Helper->self, 5.0s cast, range 12 circle
    _Weaponskill_ArborealStorm4 = 48772, // Helper->self, 7.0s cast, range 12-18 donut
    _Weaponskill_ArborealStorm3 = 48773, // Helper->self, 9.0s cast, range 18-24 donut
    _Weaponskill_ArborealStorm1 = 48774, // Helper->self, 11.0s cast, range 24-30 donut
    _Weaponskill_ArborealStorm2 = 48775, // Helper->self, 13.0s cast, range 30-36 donut
}

public enum TetherID : uint
{
    _Gen_Tether_chn_arrow01f = 57, // 4CCF->Boss
}

class Earth(BossModule module) : Components.Voidzone(module, 10, OID.BossZone);
class RustlingBreeze1(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_RustlingBreeze1, new AOEShapeCone(60, 45.Degrees()));
class RustlingBreeze2(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_RustlingBreeze3, AID._Weaponskill_RustlingBreeze4], new AOEShapeCone(60, 75.Degrees()));
class Adds(BossModule module) : ProximityAdds(module, [OID._Gen_SlugPiece, OID._Gen_SaplingPiece]);
class AqueousDischarge(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_AqueousDischarge, 5);

class ArborealStorm(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(12), new AOEShapeDonut(12, 18), new AOEShapeDonut(18, 24), new AOEShapeDonut(24, 30), new AOEShapeDonut(30, 36)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ArborealStorm5)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var seq = (AID)spell.Action.ID switch
        {
            AID._Weaponskill_ArborealStorm5 => 0,
            AID._Weaponskill_ArborealStorm4 => 1,
            AID._Weaponskill_ArborealStorm3 => 2,
            AID._Weaponskill_ArborealStorm1 => 3,
            AID._Weaponskill_ArborealStorm2 => 4,
            _ => -1
        };

        if (seq >= 0)
            AdvanceSequence(seq, caster.Position, WorldState.FutureTime(2));
    }
}

class TreantPieceStates : StateMachineBuilder
{
    public TreantPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Earth>()
            .ActivateOnEnter<RustlingBreeze1>()
            .ActivateOnEnter<RustlingBreeze2>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<AqueousDischarge>()
            .ActivateOnEnter<ArborealStorm>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14618)]
public class TreantPiece(ModuleInit init) : BossModule(init, new(120, -420), new ArenaBoundsCircle(20));

