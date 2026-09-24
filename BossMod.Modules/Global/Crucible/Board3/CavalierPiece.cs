#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.CavalierPiece;

public enum OID : uint
{
    Boss = 0x4C8E, // R2.520, x1
    Helper = 0x233C, // R0.500, x6, Helper type
    _Gen_FeintedCavalierPiece = 0x4C8F, // R2.520, x5
    _Gen_BoneBishop = 0x4C90, // R0.750, x0 (spawn during fight)
    _Gen_BoneFragment = 0x4C92, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 49680, // Boss->player, no cast, single-target
    _Spell_Blizzard = 48621, // 4C90->player, no cast, single-target
    _Weaponskill_Steelripper = 48472, // Boss->self, 6.0+1.0s cast, single-target
    _Weaponskill_Steelripper1 = 48473, // Helper->self, 7.0s cast, range 60 130-degree cone
    _Weaponskill_ = 50551, // 4C92->Boss, no cast, single-target
    _Weaponskill_1 = 50552, // 4C8F->Boss, no cast, single-target
    _Weaponskill_Doubling = 48460, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Menace = 48463, // 4C8F->self, 5.4+0.6s cast, single-target
    _Weaponskill_Menace1 = 48464, // Helper->self, 6.0s cast, range 20 circle
    _Weaponskill_CrushingBlade = 48471, // Boss->player, 5.0s cast, single-target
    _Weaponskill_Valfodr = 48461, // _Gen_FeintedCavalierPiece->self, 5.6+0.4s cast, single-target
    _Weaponskill_Valfodr1 = 48462, // Helper->self, 6.0s cast, range 60 width 8 rect
}

public enum IconID : uint
{
    _Gen_Icon_m0982trg_a0c = 633, // Boss->player
}

public enum TetherID : uint
{
    _Gen_Tether_chn_m0237_yami_x2 = 398, // 4C92->Boss
}

class BoneBishop(BossModule module) : Components.Adds(module, (uint)OID._Gen_BoneBishop);
class Steelripper(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Steelripper1, new AOEShapeCone(60, 65.Degrees()));
class Menace(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Menace1, 20);
class CrushingBlade(BossModule module) : Components.Knockback(module, AID._Weaponskill_CrushingBlade)
{
    readonly List<Actor> Casters = [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casters.Count > 0)
            hints.Add("Tankbuster");
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
            yield return new(c.Position, 15, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            var origin = src.Origin;
            var ctr = Arena.Center;
            hints.AddForbiddenZone(Sdf.Discrete(p =>
            {
                var dir = (p - origin).Normalized() * 15;
                return !(p + dir).AlmostEqual(ctr, 20);
            }), src.Activation);
        }
    }
}

class Valfodr(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Valfodr1, new AOEShapeRect(60, 4));

class CavalierPieceStates : StateMachineBuilder
{
    public CavalierPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BoneBishop>()
            .ActivateOnEnter<Steelripper>()
            .ActivateOnEnter<Menace>()
            .ActivateOnEnter<CrushingBlade>()
            .ActivateOnEnter<Valfodr>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14564)]
public class CavalierPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

