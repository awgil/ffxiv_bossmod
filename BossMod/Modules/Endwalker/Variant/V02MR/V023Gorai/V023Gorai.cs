namespace BossMod.Endwalker.Variant.V02MR.V023Gorai;

class UnenlightenmentAOE(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.UnenlightenmentAOE));
class RatTower(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.RatTower), 6);
class DramaticBurst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DramaticBurst));
class SpikeOfFlameAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SpikeOfFlameAOE), 5);

class ImpurePurgation(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ImpurePurgation))
{
    private readonly List<Actor> _castersPurgationFirst = [];
    private readonly List<Actor> _castersPurgationNext = [];

    private static readonly AOEShape _shapePurgationFirst = new AOEShapeCone(60, 22.5f.Degrees());
    private static readonly AOEShape _shapePurgationNext = new AOEShapeCone(60, 22.5f.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersPurgationFirst.Count > 0
            ? _castersPurgationFirst.Select(c => new AOEInstance(_shapePurgationFirst, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersPurgationNext.Select(c => new AOEInstance(_shapePurgationNext, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.ImpurePurgationFirst => _castersPurgationFirst,
        AID.ImpurePurgationSecond => _castersPurgationNext,
        _ => null
    };
}

class StringSnap(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.StringSnapVisual))
{
    private readonly List<Actor> _castersSnapFirst = [];
    private readonly List<Actor> _castersSnapSecond = [];
    private readonly List<Actor> _castersSnapThird = [];

    private static readonly AOEShape _shapeSnapFirst = new AOEShapeCircle(10);
    private static readonly AOEShape _shapeSnapSecond = new AOEShapeDonut(10, 20);
    private static readonly AOEShape _shapeSnapThird = new AOEShapeDonut(20, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersSnapFirst.Count > 0
            ? _castersSnapFirst.Select(c => new AOEInstance(_shapeSnapFirst, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersSnapSecond.Count > 0
            ? _castersSnapSecond.Select(c => new AOEInstance(_shapeSnapSecond, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersSnapThird.Select(c => new AOEInstance(_shapeSnapThird, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.StringSnapDonut1 => _castersSnapFirst,
        AID.StringSnapDonut2 => _castersSnapSecond,
        AID.StringSnapDonut3 => _castersSnapThird,
        _ => null
    };
}

class FlameAndSulphur(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeFlameExpand = new(46, 5);
    private static readonly AOEShapeRect _shapeFlameSplit = new(46, 2.5f);
    private static readonly AOEShapeCircle _shapeRockExpand = new(11);
    private static readonly AOEShapeDonut _shapeRockSplit = new(5.5f, 16);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BrazenBalladSplitting:
                foreach (var a in Module.Enemies(OID.FlameAndSulphurFlame))
                    _aoes.Add(new(_shapeFlameExpand, a.Position, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                foreach (var a in Module.Enemies(OID.FlameAndSulphurRock))
                    _aoes.Add(new(_shapeRockExpand, a.Position, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                break;
            case AID.BrazenBalladExpanding:
                foreach (var a in Module.Enemies(OID.FlameAndSulphurFlame))
                {
                    var offset = a.Rotation.ToDirection().OrthoL() * 7.5f;
                    _aoes.Add(new(_shapeFlameSplit, a.Position + offset, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                    _aoes.Add(new(_shapeFlameSplit, a.Position - offset, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                }
                foreach (var a in Module.Enemies(OID.FlameAndSulphurRock))
                    _aoes.Add(new(_shapeRockSplit, a.Position, a.Rotation, spell.NPCFinishAt.AddSeconds(3.1f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FireSpreadExpand or AID.FireSpreadSplit or AID.FallingRockExpand or AID.FallingRockSplit)
        {
            ++NumCasts;
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class TorchingTorment(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(6);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TorchingTormentVisual && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TorchingTormentAOE)
        {
            CurrentBaits.Clear();
            ++NumCasts;
        }
    }
}

//Route 5
class PureShock(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PureShock));

//Route 6
class Thundercall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _orbs = [];
    private Actor? _safeOrb;
    private Actor? _miniTarget;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeSmall = new(8);
    private static readonly AOEShapeCircle _shapeLarge = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_orbs, ArenaColor.Object, true);
        if (_miniTarget != null)
            Arena.AddCircle(_miniTarget.Position, 3, ArenaColor.Danger);
        if (_safeOrb != null)
            Arena.AddCircle(_safeOrb.Position, 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.HumbleHammerAOE)
        {
            _orbs.AddRange(Module.Enemies(OID.BallOfLevin));
            WDir center = new();
            foreach (var o in _orbs)
                center += o.Position - Module.Center;
            _safeOrb = _orbs.Farthest(Module.Center + center);
            _miniTarget = WorldState.Actors.Find(spell.TargetID);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HumbleHammerAOE:
                _miniTarget = null;
                _safeOrb = null;
                foreach (var o in _orbs)
                    _aoes.Add(new(spell.Targets.Any(t => t.ID == o.InstanceID) ? _shapeSmall : _shapeLarge, o.Position, default, WorldState.FutureTime(4.2f)));
                break;
            case AID.ShockSmall:
            case AID.ShockLarge:
                _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
                ++NumCasts;
                break;
        }
    }
}
class ShockAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ShockAOE), new AOEShapeCircle(20));
class HumbleHammerAOE(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HumbleHammerAOE), 3);

//Route 7
class WorldlyPursuit(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ImpurePurgation))
{
    private readonly List<Actor> _castersPursuitFirst = [];
    private readonly List<Actor> _castersPursuitRest = [];

    private static readonly AOEShape _shapePursuitFirst = new AOEShapeCross(60, 10);
    private static readonly AOEShape _shapePursuitRest = new AOEShapeCross(60, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersPursuitFirst.Count > 0)
            return _castersPursuitFirst.Select(c => new AOEInstance(_shapePursuitFirst, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
        else
            return _castersPursuitRest.Select(c => new AOEInstance(_shapePursuitRest, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.WorldlyPursuitFirst => _castersPursuitFirst,
        AID.WorldlyPursuitRest => _castersPursuitRest,
        _ => null
    };
}
class FightingSpiritsRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FightingSpiritsRaidwide));
class BiwaBreakerFirst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BiwaBreakerFirst));
class BiwaBreakerRest(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.BiwaBreakerRest));

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, Category = BossModuleInfo.Category.Criterion, GroupID = 945, NameID = 12373)]
public class V023Gorai(WorldState ws, Actor primary) : BossModule(ws, primary, new(741, -190), new ArenaBoundsSquare(20));
