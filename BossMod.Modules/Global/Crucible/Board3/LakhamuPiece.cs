#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.LakhamuPiece;

public enum OID : uint
{
    Boss = 0x4C9E, // R3.400, x1
    Helper = 0x233C, // R0.500, x9 (spawn during fight), Helper type
    _Gen_GolemPiece = 0x4C9F, // R2.200, x0 (spawn during fight)
    _Gen_SandSphere = 0x4CA0, // R1.800, x0 (spawn during fight)
}

public enum AID : uint
{
    _Spell_Stone = 50792, // Boss->player, no cast, single-target
    _Ability_Landslip = 48553, // Boss->self, 7.5+1.0s cast, single-target
    _Ability_Landslip1 = 48556, // Helper->self, 8.0s cast, range 45 width 10 rect
    _Weaponskill_Rockslide = 48554, // 4C9F->self, 6.0s cast, single-target
    _Weaponskill_Rockslide1 = 48555, // Helper->self, 7.0s cast, range 45 width 10 rect
    _AutoAttack_ = 50398, // 4C9F->player, no cast, single-target
    _Spell_SandTempest = 48561, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_Burst = 48562, // 4CA0->self, 3.0s cast, range 12 circle
    _Spell_EarthShaker = 48557, // Boss->self, 4.0+0.2s cast, single-target
    _Spell_EarthShaker1 = 48558, // Helper->self, no cast, range 60 90?-degree cone
    _Spell_Earthrender = 48559, // Boss->self, 4.0s cast, single-target
    _Spell_Earthrender1 = 48560, // Helper->location, 3.0s cast, range 6 circle
}

public enum SID : uint
{
    _Gen_Blind = 5389, // Boss->player, extra=0x0
    _Gen_EarthResistanceDown = 5025, // Helper->player, extra=0x1
}

public enum IconID : uint
{
    _Gen_Icon_m0117_earth_shake_01s = 40, // player/49E8->self
}

class Landslip(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Ability_Landslip1, 20, ignoreImmunes: true, shape: new AOEShapeRect(45, 5), kind: Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            var rect = ShapeDistance.Rect(src.Origin, src.Direction, 45, 0, 5);
            var dir = src.Direction.ToDirection().Abs();
            var fromCenter = dir.X > dir.Z ? src.Origin with { X = Arena.Center.X } : src.Origin with { Z = Arena.Center.Z };
            var toWall = ShapeDistance.HalfPlane(fromCenter, -src.Direction.ToDirection());
            hints.AddForbiddenZone(ShapeDistance.Intersection([rect, toWall]), src.Activation);

            foreach (var c in Module.FindComponent<Rockslide>()?.Casters ?? [])
            {
                var castRect = ShapeDistance.Rect(c.CastInfo!.LocXZ - src.Direction.ToDirection() * 20, c.CastInfo.Rotation, 45, 0, 5);
                hints.AddForbiddenZone(ShapeDistance.Intersection([rect, castRect]), src.Activation);
            }
        }
    }
}
class Rockslide(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Rockslide1, new AOEShapeRect(45, 5))
{
    bool _knockbackActive;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_knockbackActive)
            base.AddHints(slot, actor, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID._Ability_Landslip1)
            _knockbackActive = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID._Ability_Landslip1)
            _knockbackActive = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_knockbackActive)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class GolemPiece(BossModule module) : Components.Adds(module, (uint)OID._Gen_GolemPiece);

class SandTempest(BossModule module) : Components.RaidwideCast(module, AID._Spell_SandTempest, "Raidwide + blind");
class Burst(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Burst)
{
    readonly List<(Actor Orb, DateTime Activation)> Orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Orbs.Select(o => new AOEInstance(new AOEShapeCircle(12), o.Orb.Position, default, o.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_SandSphere)
            Orbs.Add((actor, WorldState.FutureTime(5.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            Orbs.RemoveAll(o => o.Orb == caster);
    }
}

class EarthShaker(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 45.Degrees()), (uint)IconID._Gen_Icon_m0117_earth_shake_01s, AID._Spell_EarthShaker1, 3.3f);
class Earthrender(BossModule module) : Components.StandardAOEs(module, AID._Spell_Earthrender1, 6);

class LakhamuPieceStates : StateMachineBuilder
{
    public LakhamuPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rockslide>()
            .ActivateOnEnter<Landslip>()
            .ActivateOnEnter<GolemPiece>()
            .ActivateOnEnter<SandTempest>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<EarthShaker>()
            .ActivateOnEnter<Earthrender>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1090, NameID = 14580)]
public class LakhamuPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

