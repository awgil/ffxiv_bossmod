

namespace BossMod.Global.Crucible.MedusaPiece;

public enum OID : uint
{
    Boss = 0x4CF9,
    Helper = 0x233C,
    _Gen_LamiaPiece = 0x4CFA, // R1.400, x0 (spawn during fight)
    _Gen_CyclopsPiece = 0x4CFB, // R2.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _AutoAttack_ = 50935, // Boss->player, no cast, single-target
    _Weaponskill_Summon = 49283, // Boss->self, 3.0s cast, single-target
    _Spell_Fire = 48622, // 4CFA->player, no cast, single-target
    _Weaponskill_PetrifyingRegard = 49277, // Boss->self, 3.0s cast, single-target
    _Weaponskill_PetrifyingRegard1 = 49278, // Helper->self, 3.0s cast, range 60 45-degree cone
    _Spell_CircleOfFlames = 49284, // 4CFA->location, 4.0s cast, range 5 circle
    _Weaponskill_RipplingEvisceration = 49288, // Boss->self, 6.5+0.5s cast, single-target
    _Weaponskill_Shockwave = 49296, // Helper->self, 7.0s cast, range 60 ?-degree cone
    _Weaponskill_CirclingBlade = 49289, // Helper->self, 7.0s cast, range 6 circle
    _Weaponskill_RingingBlade = 49290, // Boss->self, no cast, single-target
    _Weaponskill_RingingBlade1 = 49291, // Helper->self, 9.0s cast, range 5-60 donut
    _Spell_Raise = 49287, // 4CFA->4CFA, 10.0s cast, single-target
    _Weaponskill_1000TonzeSwing = 49297, // 4CFB->self, 10.0s cast, range 20 circle
    _Ability_ = 49285, // 4CFA->location, no cast, single-target
    _AutoAttack_1 = 50937, // 4CFB->player, no cast, single-target
    _Weaponskill_TerrorizingMiasma = 49286, // 4CFA->self, 4.0s cast, range 6 circle
    _Ability_Impassion = 49298, // Boss->self, 8.0s cast, single-target
    _Weaponskill_Glower = 49370, // 4CFB->self, 4.0s cast, range 40 width 3 rect
    _Weaponskill_PetrifyingPassion = 49279, // Boss->self, 3.0s cast, single-target
    _Weaponskill_PetrifyingPassion1 = 49281, // Helper->self, 3.0s cast, range 60 45-degree cone
}

public enum SID : uint
{
    _Gen_DamageUp = 2550, // Helper->4CFA, extra=0x1
    _Gen_StoneCurse = 437, // Helper->4CFB, extra=0x0
    _Gen_DamageUp1 = 3129, // Boss->Boss, extra=0x0
    _Gen_ = 2056, // Boss->Boss, extra=0x1D
    _Gen_Hysteria = 4167, // 4CFA->player, extra=0x0
    _Gen_Bleeding = 3077, // none->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_lockon5_t0h = 23, // player->self
    _Gen_Icon_lockon8_t0w = 244, // player->self
    _Gen_Icon_m0489_turning_left01f = 236, // Boss->self
}

public enum TetherID : uint
{
    _Gen_Tether_chn_dark001f = 1, // Boss->player
}

class PetrifyingRegard(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 22.5f.Degrees()), (uint)IconID._Gen_Icon_lockon5_t0h, AID._Weaponskill_PetrifyingRegard1, 5.8f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurrentBaits.Count > 0)
            foreach (var enemy in Module.Enemies(OID._Gen_LamiaPiece).Where(l => !l.IsDead))
                Arena.AddCircle(enemy.Position, enemy.HitboxRadius, ArenaColor.Danger);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var bait in ActiveBaitsOn(actor))
        {
            foreach (var enemy in Module.Enemies(OID._Gen_LamiaPiece).Where(l => !l.IsDead))
            {
                var dir = enemy.Position - bait.Source.Position;
                var width = MathF.Atan2(enemy.HitboxRadius, dir.Length()).Radians();
                hints.AddForbiddenZone(ShapeDistance.Cone(bait.Source.Position, 100, dir.ToAngle(), 22.5f.Degrees() + width), bait.Activation);
            }

            var cyclops = Module.Enemies(OID._Gen_CyclopsPiece).Where(p => p.CastInfo?.IsSpell(AID._Weaponskill_1000TonzeSwing) == true).Select(p =>
            {
                var dir = p.Position - bait.Source.Position;
                var width = MathF.Atan2(p.HitboxRadius, dir.Length()).Radians();
                return ShapeDistance.InvertedCone(bait.Source.Position, 50, dir.ToAngle(), 22.5f.Degrees() + width);
            }).ToList();

            if (cyclops.Count > 0)
                hints.AddForbiddenZone(ShapeDistance.Intersection(cyclops), bait.Activation);
        }
    }
}
class PetrifyingRegardAOE(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_PetrifyingRegard1, new AOEShapeCone(60, 22.5f.Degrees()));
class LamiaPiece(BossModule module) : Components.Adds(module, (uint)OID._Gen_LamiaPiece);

class Shockwave(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Shockwave, new AOEShapeCone(60, 22.5f.Degrees()), 4);
class Blade(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_CirclingBlade:
                _predicted.Add(new(new AOEShapeCircle(6), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.SortBy(p => p.Activation);
                break;
            case AID._Weaponskill_RingingBlade1:
                _predicted.Add(new(new AOEShapeDonut(5, 60), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.SortBy(p => p.Activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_CirclingBlade or AID._Weaponskill_RingingBlade1)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class CyclopsPiece(BossModule module) : Components.Adds(module, (uint)OID._Gen_CyclopsPiece);

class C1000TonzeSwing(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_1000TonzeSwing, 20)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Count == 4 ? [] : base.ActiveAOEs(slot, actor);
}

class TerrorizingMiasma(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_TerrorizingMiasma, 6);
class Glower(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Glower, new AOEShapeRect(40, 1.5f));

class MedusaPieceStates : StateMachineBuilder
{
    public MedusaPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PetrifyingRegard>()
            .ActivateOnEnter<PetrifyingRegardAOE>()
            .ActivateOnEnter<LamiaPiece>()
            .ActivateOnEnter<Blade>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<CyclopsPiece>()
            .ActivateOnEnter<C1000TonzeSwing>()
            .ActivateOnEnter<TerrorizingMiasma>()
            .ActivateOnEnter<Glower>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14660)]
public class MedusaPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsRect(20, 15));
