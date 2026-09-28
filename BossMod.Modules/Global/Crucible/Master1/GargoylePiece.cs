#pragma warning disable CA1707 // Identifiers should not contain underscores
namespace BossMod.Global.Crucible.GargoylePiece;

public enum OID : uint
{
    Boss = 0x4CC2, // R4.600, x1
    Helper = 0x233C,
    _Gen_Malady = 0x4CC3, // R1.000, x10
}

public enum AID : uint
{
    _AutoAttack_ = 50396, // Boss->player, no cast, single-target
    _Weaponskill_RipplingEvisceration = 48719, // Boss->self, 5.4+0.6s cast, single-target
    _Weaponskill_RipplingEvisceration1 = 48720, // Helper->self, 6.0s cast, range 13 circle
    _Weaponskill_RipplingEvisceration2 = 48721, // Helper->self, 8.0s cast, range ?-30 donut
    _Weaponskill_SweepingEvisceration = 48717, // Boss->self, 7.9s cast, single-target
    _Weaponskill_SweepingEvisceration1 = 50932, // Boss->location, no cast, single-target
    _Weaponskill_SweepingEvisceration2 = 50933, // Helper->player, no cast, single-target
    _Weaponskill_SweepingEvisceration3 = 48718, // Helper->self, no cast, range 60 180-degree cone
    _Weaponskill_Malady = 48713, // Boss->self, 4.0+1.0s cast, single-target
    _Weaponskill_Malady1 = 48714, // 4CC3->Boss, 4.7s cast, single-target
    _Weaponskill_Malady2 = 48715, // Helper->location, 5.0s cast, range 6 circle
    _Weaponskill_Burst = 48716, // _Gen_Malady->self, no cast, range 6 circle
    _Ability_ = 48732, // Boss->location, no cast, single-target
    _Weaponskill_FivefoldFallout = 50695, // Boss->self, 2.5+0.5s cast, single-target
    _Weaponskill_FivefoldFallout1 = 50696, // Helper->self, 3.0s cast, range 60 circle
    _Weaponskill_FivefoldFallout2 = 48722, // Boss->self, no cast, single-target
    _Weaponskill_FivefoldFallout3 = 48723, // Helper->self, 0.5s cast, range 60 circle
    _Weaponskill_FivefoldFallout4 = 48724, // Boss->self, 2.5+0.5s cast, single-target
    _Weaponskill_FivefoldFallout5 = 48725, // Helper->self, 3.0s cast, range 60 circle
    _Weaponskill_Desolation = 48726, // Boss->self, 2.2+1.3s cast, single-target
    _Weaponskill_Desolation1 = 48727, // Helper->self, 3.5s cast, range 60 width 7 rect
    _Weaponskill_GrimFate = 48730, // Boss->player, 5.0s cast, single-target
    _Weaponskill_GrimFate1 = 48731, // Helper->player, no cast, single-target
    _Weaponskill_SeaOfPitch = 48728, // Boss->self, 3.0s cast, single-target
    _Weaponskill_SeaOfPitch1 = 48729, // Helper->location, 3.0s cast, range 6 circle
}

// tether stretch is probably 20 units
public enum TetherID : uint
{
    _Gen_Tether_chn_arrow01f = 57, // Boss->player
    _Gen_Tether_chn_dark001f = 1, // Boss->player
    _Gen_Tether_chn_magic_supply_01y2 = 426, // _Gen_Malady->Boss
}

public enum SID : uint
{
    _Gen_GrowingDread = 5178, // Helper->player, extra=0x1
    _Gen_DamageUp = 2550, // none->Boss, extra=0xA
}

class RipplingEvisceration(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_RipplingEvisceration1:
                _predicted.Add(new(new AOEShapeCircle(13), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.SortBy(p => p.Activation);
                break;
            case AID._Weaponskill_RipplingEvisceration2:
                _predicted.Add(new(new AOEShapeDonut(13, 30), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
                _predicted.SortBy(p => p.Activation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_RipplingEvisceration1 or AID._Weaponskill_RipplingEvisceration2)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class SweepingEviscerationTether(BossModule module) : BossComponent(module)
{
    readonly ResistHelper Resists = module.FindComponent<ResistHelper>()!;

    Actor? _target;
    DateTime _deadline;
    bool _disabled;

    bool IsImmune => Resists[ResistHelper.Resistance.Physical] > _deadline;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == _target && !IsImmune)
            hints.Add("Stretch tether!", actor.DistanceToPoint(Module.PrimaryActor.Position) < 20);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target is { } t)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(Module.PrimaryActor.Position, t.Position, 0xFF000000, 2);

            var safe = IsImmune || (t.Position - Module.PrimaryActor.Position).LengthSq() >= 400;

            if (!IsImmune)
                Arena.AddCircle(Module.PrimaryActor.Position, 20, ArenaColor.Object);

            Arena.AddLine(Module.PrimaryActor.Position, t.Position, safe ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Weaponskill_SweepingEvisceration1)
        {
            _target = null;
            _disabled = true; // boss retethers while jumping, we want to ignore it
        }

        if ((AID)spell.Action.ID is AID._Weaponskill_SweepingEvisceration2)
            _disabled = false;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (!_disabled && (TetherID)tether.ID is TetherID._Gen_Tether_chn_arrow01f or TetherID._Gen_Tether_chn_dark001f)
            _target = WorldState.Actors.Find(tether.Target);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_target == actor && !IsImmune)
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, 20), _deadline);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_SweepingEvisceration)
            _deadline = Module.CastFinishAt(spell, 0.2f);
    }
}

class SweepingEvisceration(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_SweepingEvisceration3)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_SweepingEvisceration1)
        {
            _predicted.Add(new(new AOEShapeCone(60, 90.Degrees()), spell.TargetXZ, spell.Rotation, WorldState.FutureTime(2.8f)));
            _predicted.Add(new(new AOEShapeCone(60, 90.Degrees()), spell.TargetXZ, spell.Rotation + 180.Degrees(), WorldState.FutureTime(4.8f)));
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_SweepingEvisceration3)
        {
            NumCasts++;
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class Malady(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Malady2, 6);
class MaladyOrb(BossModule module) : BossComponent(module)
{
    int _stacks;
    readonly List<Actor> _orbs = [];

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_GrowingDread)
            _stacks = status.Extra;
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_GrowingDread)
            _stacks = 0;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_magic_supply_01y2)
            _orbs.Add(source);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID._Gen_Tether_chn_magic_supply_01y2)
            _orbs.Remove(source);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_stacks < 4 && _orbs.Count > 0)
            hints.Add("Pick up orbs!", false);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var o in _orbs)
        {
            if (_stacks < 4)
                Arena.AddCircle(o.Position, 1, ArenaColor.Safe);
            else
                Arena.ZoneCircle(o.Position, 1, ArenaColor.AOE);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_stacks < 4)
        {
            foreach (var o in _orbs)
                hints.GoalZones.Add(AIHints.GoalSingleTarget(o.Position, 1, 2));
        }
        else
        {
            foreach (var o in _orbs)
                hints.AddForbiddenZone(ShapeDistance.Circle(o.Position, 1));
        }
    }
}

class FivefoldFallout(BossModule module) : Components.RaidwideCastDelay(module, AID._Weaponskill_FivefoldFallout1, AID._Weaponskill_FivefoldFallout5, 8.8f, "Raidwide 5x");
class FivefoldFalloutKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID._Weaponskill_FivefoldFallout5, 20)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            var ctr = Arena.Center;
            var orig = src.Origin;
            hints.AddForbiddenZone(Sdf.Discrete(p =>
            {
                var dir = (p - orig).Normalized() * 20;
                return !(p + dir).AlmostEqual(ctr, 20);
            }), src.Activation);
        }
    }
}
class Desolation(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_Desolation1, new AOEShapeRect(60, 3.5f));
class SeaOfPitch(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_SeaOfPitch1, 6);

class GargoylePieceStates : StateMachineBuilder
{
    public GargoylePieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ResistHelper>()
            .ActivateOnEnter<RipplingEvisceration>()
            .ActivateOnEnter<SweepingEviscerationTether>()
            .ActivateOnEnter<SweepingEvisceration>()
            .ActivateOnEnter<Malady>()
            .ActivateOnEnter<MaladyOrb>()
            .ActivateOnEnter<FivefoldFallout>()
            .ActivateOnEnter<FivefoldFalloutKnockback>()
            .ActivateOnEnter<Desolation>()
            .ActivateOnEnter<SeaOfPitch>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14608)]
public class GargoylePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

