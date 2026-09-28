#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.MorbolPiece;

public enum OID : uint
{
    Boss = 0x4CB9, // R6.000, x1
    _Gen_SeedlingPiece = 0x4CBA, // R0.900, x0 (spawn during fight)
    _Gen_OchuPiece = 0x4CBB, // R2.400, x0 (spawn during fight)
    _Gen_CarrionBroth = 0x4CBC, // R2.500, x3 (spawn during fight)
    Helper = 0x233C, // R0.500, x3, Helper type

    PoisonPuddle = 0x1E9F40
}

public enum AID : uint
{
    _AutoAttack_ = 50937, // Boss->player, no cast, single-target
    _AutoAttack_Attack = 50750, // 4CBA->player, no cast, single-target
    _Weaponskill_ExtremelyBadBreath = 48671, // Boss->self, 4.5+0.5s cast, single-target
    _Weaponskill_ExtremelyBadBreath1 = 48673, // Helper->self, 5.0s cast, range 50 90-degree cone
    _Weaponskill_ExtremelyBadBreath2 = 48674, // Boss->self, no cast, single-target
    _Weaponskill_ExtremelyBadBreath3 = 48675, // Helper->self, 0.5s cast, range 50 90-degree cone
    _Weaponskill_StickySpit = 48680, // 4CBC->player, 2.0s cast, single-target
    _AutoAttack_Attack1 = 50544, // 4CBC->player, no cast, single-target
    _Weaponskill_ExtremelyBadBreath4 = 48672, // Boss->self, 4.5+0.5s cast, single-target
    _AutoAttack_1 = 50396, // 4CBB->player, no cast, single-target
    _Weaponskill_AcidMist = 48679, // 4CBB->self, 4.0s cast, range 6 circle
    _Weaponskill_Tremblor = 50757, // Boss->self, 4.5+0.5s cast, single-target
    _Weaponskill_Tremblor1 = 50758, // Helper->self, 5.0s cast, range 50 circle
    _Weaponskill_VineProbe = 48682, // Helper->self, 5.0s cast, range 13 width 8 rect
    _Weaponskill_VineProbe1 = 48681, // Boss->self, 4.0+1.0s cast, single-target
}

public enum SID : uint
{
    _Gen_Poison = 5141, // 4CBA->player, extra=0x2
    _Gen_VulnerabilityUp = 1789, // 4CBC->player, extra=0x1
    _Gen_Heavy = 2551, // 4CBC->player, extra=0x1E
    _Gen_Paralysis = 5382, // 4CBC->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_m037_turning_right01x2 = 684, // Boss->self
    _Gen_Icon_m037_turning_left01x2 = 685, // Boss->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_tergetfix1f = 17, // 4CBA->player
    _Gen_Tether_chn_m0070_1c = 44, // 4CBC->player
}

class PoisonPuddle(BossModule module) : Components.Voidzone(module, 6, OID.PoisonPuddle);

class ExtremelyBadBreath(BossModule module) : Components.GenericRotatingAOE(module)
{
    Angle _rotation;
    Actor? _caster;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_ExtremelyBadBreath1)
        {
            _caster = caster;
            Init();
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID._Gen_Icon_m037_turning_left01x2:
                _rotation = 45.Degrees();
                Init();
                break;
            case IconID._Gen_Icon_m037_turning_right01x2:
                _rotation = -45.Degrees();
                Init();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_ExtremelyBadBreath1 or AID._Weaponskill_ExtremelyBadBreath3)
        {
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
            if (Sequences.Count == 0)
            {
                _rotation = default;
                _caster = null;
            }
        }
    }

    void Init()
    {
        if (Sequences.Count > 0 || _rotation == default || _caster == null)
            return;

        Sequences.Add(new()
        {
            Shape = new AOEShapeCone(50, 45.Degrees()),
            Origin = _caster.CastInfo!.LocXZ,
            Rotation = _caster.CastInfo!.Rotation,
            Increment = _rotation,
            NextActivation = Module.CastFinishAt(_caster.CastInfo),
            SecondsBetweenActivations = 2.1f,
            NumRemainingCasts = 5,
            MaxShownAOEs = 3
        });
    }
}

class AcidMist(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_AcidMist, 6);
class Tremblor(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_Tremblor1, 10)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 10), src.Activation);
    }
}
class VineProbe(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_VineProbe, new AOEShapeRect(13, 4));

class Adds(BossModule module) : Components.AddsMulti(module, [OID._Gen_SeedlingPiece, OID._Gen_OchuPiece, OID._Gen_CarrionBroth]);

class MorbolPieceStates : StateMachineBuilder
{
    public MorbolPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PoisonPuddle>()
            .ActivateOnEnter<ExtremelyBadBreath>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<AcidMist>()
            .ActivateOnEnter<Tremblor>()
            .ActivateOnEnter<VineProbe>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14599)]
public class MorbolPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

