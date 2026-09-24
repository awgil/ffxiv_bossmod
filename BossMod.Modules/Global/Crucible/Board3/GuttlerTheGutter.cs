#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.GuttlerTheGutter;

public enum OID : uint
{
    Boss = 0x4CAA, // R4.000, x1
    Helper = 0x233C, // R0.500, x18, Helper type
    _Gen_ = 0x4E5D, // R1.400, x0 (spawn during fight)
    _Gen_CombustingBlade = 0x4CAB, // R1.000, x6
    _Gen_MoltenBlade = 0x4CAC, // R1.000, x0 (spawn during fight)
    _Gen_ThanatosPiece = 0x4CAD, // R2.000, x0 (spawn during fight)

    DeadlyDemesne = 0x1EC0C3
}

public enum AID : uint
{
    _AutoAttack_ = 49714, // Boss->players, no cast, range 9 120-degree cone
    _Ability_ = 48590, // Boss->location, no cast, single-target
    _Weaponskill_BeastlyAura = 48606, // Boss->self, 6.0s cast, single-target
    _Weaponskill_BeastlyAura1 = 48607, // Helper->self, 7.0s cast, range 80 width 80 rect
    _Weaponskill_CombustingBlades = 48598, // Boss->self, no cast, single-target
    _Weaponskill_CombustingBlades1 = 48592, // 4CAB->Boss, no cast, single-target
    _Weaponskill_CombustingBlades2 = 48591, // 4CAB->Boss, no cast, single-target
    _Weaponskill_CombustingBlades3 = 48593, // 4CAB->Boss, no cast, single-target
    _Weaponskill_CombustingBlades4 = 48594, // Helper->self, 0.5s cast, range 2 circle
    _Weaponskill_CombustingBlades5 = 48595, // Helper->self, 0.7s cast, range 2 circle
    _Weaponskill_CombustingBlades6 = 48596, // Helper->self, 0.9s cast, range 2 circle
    _Weaponskill_MagicalCombustion = 48597, // 4CAB->self, 5.0s cast, range 8 circle
    _Weaponskill_GluttonousGutting = 48599, // Boss->self, 6.0+0.6s cast, single-target
    _Weaponskill_GluttonousGutting1 = 48600, // Helper->self, 11.6s cast, range 50 width 40 rect
    _Weaponskill_Gyrocleave = 48604, // Boss->self, 6.0s cast, single-target
    _Weaponskill_Gyrocleave1 = 48605, // Helper->self, 7.0s cast, range 80 width 20 rect
    _Weaponskill_Thunderbolt = 48620, // Boss->self/player, 5.0s cast, range 50 width 6 rect
    _Weaponskill_OverpoweringPoint = 48612, // Boss->self, 4.9+3.5s cast, single-target
    _Weaponskill_OverpoweringPoint1 = 48613, // Boss->self, no cast, single-target
    _Weaponskill_OverpoweringPoint2 = 48614, // Helper->self, 3.5s cast, range 60 width 6 rect
    _Weaponskill_DeadlyDemesne = 48608, // Boss->self, 3.0s cast, single-target
    _Weaponskill_Fetters = 48609, // Helper->self, no cast, range 10 width 10 rect
    _Weaponskill_ = 48610, // Helper->self, no cast, range 100 circle
    _Weaponskill_LifeClaim = 48611, // Helper->self, 3.0s cast, range 15 width 10 cross
    _Weaponskill_MoltenMetal = 48615, // Boss->self, 6.2+2.1s cast, single-target
    _Weaponskill_MoltenMetal1 = 48616, // Boss->self, no cast, single-target
    _Weaponskill_MoltenMetal2 = 48617, // 4CAC->Boss, 2.5s cast, single-target
    _Weaponskill_MoltenMetal3 = 48618, // Helper->self, 3.0s cast, range 6 circle
    _Spell_BeastlyFlare = 48619, // 4CAC->self, 8.0s cast, range 80 circle
    _Weaponskill_CombustingBlades7 = 48601, // Boss->self, no cast, single-target
    _Weaponskill_GluttonousGoring = 48602, // Boss->self, 6.0+0.6s cast, single-target
    _Weaponskill_GluttonousGoring1 = 48603, // Helper->self, 11.6s cast, range 40 circle
    _AutoAttack_Attack = 870, // 4CAD->player, no cast, single-target
    _Weaponskill_InfernalPain = 50540, // 4CAD->self, 8.0s cast, range 40 circle
}

public enum SID : uint
{
    _Gen_ = 2552, // none->Boss, extra=0x477/0x483/0x482/0x478
    _Gen_Paralysis = 5388, // Boss->player, extra=0x0
    _Gen_Bind = 3625, // Helper->player, extra=0x0
    _Gen_Fetters = 1614, // none->player, extra=0xEC4
    _Gen_DamageDown = 4874, // 4CAB->player, extra=0x1
}

public enum IconID : uint
{
    _Gen_Icon_tank_laser_5sec_lockon_c0a1 = 471, // player->self
    _Gen_Icon_suteloc6s6m_1k1 = 669, // player->self
}

class AutoAttack(BossModule module) : Components.Cleave(module, AID._AutoAttack_, new AOEShapeCone(9, 60.Degrees()), (uint)OID.Boss, activeWhileCasting: false)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID && Module.PrimaryActor.CastInfo == null && Module.PrimaryActor.IsTargetable && WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID) is { } pet)
        {
            var dir = Module.PrimaryActor.AngleTo(pet);
            hints.AddForbiddenZone(ShapeDistance.Cone(Module.PrimaryActor.Position, 9, dir, 60.Degrees()), DateTime.MaxValue);
        }
    }
}

class BeastlyAura(BossModule module) : Components.Knockback(module, AID._Weaponskill_BeastlyAura1)
{
    readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            var ci = c.CastInfo!;
            yield return new(ci.LocXZ, 20, Module.CastFinishAt(ci), new AOEShapeRect(80, 80), ci.Rotation + 90.Degrees(), Kind.DirForward);
            yield return new(ci.LocXZ, 20, Module.CastFinishAt(ci), new AOEShapeRect(80, 80), ci.Rotation - 90.Degrees(), Kind.DirForward);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, default(Angle), 5, 5, 2.5f), Module.CastFinishAt(Casters[0].CastInfo));
    }
}

class MagicalCombustion(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MagicalCombustion, 8);
class GluttonousGutting(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GluttonousGutting1, new AOEShapeRect(50, 20));
class GluttonousGoring(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_GluttonousGoring1, 40);
class Gyrocleave(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Gyrocleave1, new AOEShapeRect(80, 10));
class Thunderbolt(BossModule module) : Components.BaitAwayCast(module, AID._Weaponskill_Thunderbolt, new AOEShapeRect(50, 3));
class OverpoweringPoint(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_OverpoweringPoint2, new AOEShapeRect(60, 3));
class DeadlyDemesne(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor, DateTime)> _cages = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _cages.Select(c => new AOEInstance(new AOEShapeRect(5, 5, 5), c.Item1.Position, c.Item1.Rotation, c.Item2));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.DeadlyDemesne)
        {
            switch (state)
            {
                case 0x00010002:
                case 0x00010010:
                    _cages.Add((actor, WorldState.FutureTime(3.7f)));
                    break;
                case 0x00040008:
                    _cages.RemoveAll(c => c.Item1 == actor);
                    break;

            }
        }
    }
}
class LifeClaim(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_LifeClaim, new AOEShapeCross(15, 5));

class MoltenMetal(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_MoltenMetal3, 6);
class BeastlyFlare(BossModule module) : Components.ProximityAOEs(module, AID._Spell_BeastlyFlare, 20);

class GuttlerTheGutterStates : StateMachineBuilder
{
    public GuttlerTheGutterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<BeastlyAura>()
            .ActivateOnEnter<MagicalCombustion>()
            .ActivateOnEnter<GluttonousGutting>()
            .ActivateOnEnter<GluttonousGoring>()
            .ActivateOnEnter<Gyrocleave>()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<OverpoweringPoint>()
            .ActivateOnEnter<DeadlyDemesne>()
            .ActivateOnEnter<LifeClaim>()
            .ActivateOnEnter<MoltenMetal>()
            .ActivateOnEnter<BeastlyFlare>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14592)]
public class GuttlerTheGutter(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, -420), CustomBounds)
{
    public static readonly ArenaBoundsCustom CustomBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds()
    {
        var c = new PolygonClipper();

        static PolygonClipper.Operand P(float x, float z) => new(CurveApprox.Rect(new WDir(x, z), new WDir(2.5f, 0), new WDir(0, 2.5f)));

        return new(25, c.UnionAll(new(CurveApprox.Rect(new(10, 0), new(0, 20))), P(0, 22.5f), P(0, -22.5f), P(12.5f, 2.5f), P(-12.5f, -2.5f), P(12.5f, -7.5f), P(-12.5f, 7.5f)));
    }
}
