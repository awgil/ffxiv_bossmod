﻿namespace BossMod.RealmReborn.Dungeon.D13CastrumMeridianum.D133Livia;

public enum OID : uint
{
    Boss = 0x38CE, // x1
    Helper = 0x233C, // x50
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 28788, // Boss->location, no cast, single-target
    AglaeaClimb = 28798, // Boss->player, 5.0s cast, single-target, tankbuster
    ArtificialPlasma = 28033, // Boss->self, 5.0s cast, range 40 circle, raidwide

    Roundhouse = 28786, // Boss->self, 4.4s cast, single-target, visual
    RoundhouseAOE = 29163, // Helper->self, 5.0s cast, range 10 circle aoe (center)
    RoundhouseDischarge = 28787, // Helper->self, 7.0s cast, range 8 circle aoe (sides)

    InfiniteReach = 28791, // Boss->self, 4.9s cast, single-target, visual
    InfiniteReachSecondaryVisual = 28792, // Boss->self, no cast, single-target, visual
    InfiniteReachDischarge = 28794, // Helper->self, 7.2s cast, range 8 circle aoe
    InfiniteReachAOE = 28793, // Helper->self, 7.2s cast, range 40 width 4 rect aoe
    InfiniteReachAngrySalamander = 28796, // Boss->self, no cast, single-target

    ThermobaricStrike = 29357, // Boss->self, 4.0s cast, single-target, visual
    StunningSweep = 28789, // Boss->self, 3.3s cast, single-target, visual
    StunningSweepDischarge = 28790, // Helper->self, 4.0s cast, range 8 circle aoe
    StunningSweepAOE = 29164, // Helper->self, 4.0s cast, range 8 circle aoe
    ThermobaricCharge = 29356, // Helper->self, 7.0s cast, range 40 circle aoe with 14 (?) falloff
    AngrySalamander = 28795, // Boss->self, 1.5s cast, single-target, visual
    AngrySalamanderAOE = 28797, // Helper->self, 2.5s cast, range 20 width 4 cross

    ArtificialBoost = 29354, // Boss->self, 4.0s cast, single-target, visual (buff)
    ArtificialPlasmaBoostFirst = 29352, // Boss->self, 5.0s cast, raidwide
    ArtificialPlasmaBoostRest = 29353, // Boss->self, no cast, raidwide
}

class AglaeaClimb(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.AglaeaClimb));
class ArtificialPlasma(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ArtificialPlasma));

class Roundhouse(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Roundhouse))
{
    private readonly List<Actor> _castersRoundhouse = [];
    private readonly List<Actor> _castersDischarge = [];

    private static readonly AOEShape _shapeRoundhouse = new AOEShapeCircle(10);
    private static readonly AOEShape _shapeDischarge = new AOEShapeCircle(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersRoundhouse.Count > 0)
            return _castersRoundhouse.Select(c => new AOEInstance(_shapeRoundhouse, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!)));
        else
            return _castersDischarge.Select(c => new AOEInstance(_shapeDischarge, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!)));
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
        AID.RoundhouseAOE => _castersRoundhouse,
        AID.RoundhouseDischarge => _castersDischarge,
        _ => null
    };
}

// note: casts are staggered in a slightly complicated way, so we don't remove actors immediately when cast finishes
// a 'set' is always considered finished when 6th discharge finishes
class InfiniteReach(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.InfiniteReachDischarge))
{
    private readonly List<Actor?> _castersRect = [];
    private readonly List<Actor?> _castersDischarge = [];
    private readonly List<Actor?> _castersSalamander = [];

    private static readonly AOEShapeRect _shapeRect = new(40, 2);
    private static readonly AOEShapeCircle _shapeDischarge = new(8);
    private static readonly AOEShapeCross _shapeSalamander = new(20, 2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        bool haveSets = false;
        int currentSet = NumCasts / 6;
        foreach (var c in _castersRect.Skip(currentSet).Take(1).OfType<Actor>())
        {
            haveSets = true;
            yield return new(_shapeRect, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!));
        }
        foreach (var c in _castersDischarge.Skip(currentSet * 6).Take(6).OfType<Actor>())
        {
            haveSets = true;
            yield return new(_shapeDischarge, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!));
        }
        if (!haveSets)
        {
            foreach (var c in _castersSalamander.OfType<Actor>())
            {
                yield return new(_shapeSalamander, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action, false)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var list = CastersForSpell(spell.Action, true);
        if (list != null)
        {
            var index = list.IndexOf(caster);
            if (index >= 0)
                list[index] = null;
        }
    }

    private List<Actor?>? CastersForSpell(ActionID spell, bool forRemove) => (AID)spell.ID switch
    {
        AID.InfiniteReachAOE => _castersRect,
        AID.InfiniteReachDischarge => _castersDischarge,
        AID.AngrySalamanderAOE => forRemove || !Module.PrimaryActor.IsTargetable ? _castersSalamander : null, // note: this component cares only about cast when boss is untargetable
        _ => null
    };
}

class StunningSweep(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.StunningSweep))
{
    private readonly List<Actor> _castersSweepDischarge = [];
    private readonly List<Actor> _castersThermobaric = [];

    private static readonly AOEShape _shapeSweepDischarge = new AOEShapeCircle(8);
    private static readonly AOEShape _shapeThermobaric = new AOEShapeCircle(13); // TODO: verify falloff

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersSweepDischarge.Count > 0)
            return _castersSweepDischarge.Select(c => new AOEInstance(_shapeSweepDischarge, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!)));
        else
            return _castersThermobaric.Select(c => new AOEInstance(_shapeThermobaric, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action, false)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action, true)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell, bool forRemove) => (AID)spell.ID switch
    {
        AID.StunningSweepAOE or AID.StunningSweepDischarge => _castersSweepDischarge,
        AID.ThermobaricCharge => forRemove || Module.PrimaryActor.CastInfo != null ? _castersThermobaric : null, // note: this component cares about cast while boss is casting
        _ => null
    };
}

class AngrySalamander(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.AngrySalamander))
{
    private readonly List<Actor> _castersSalamander = [];
    private readonly List<Actor> _castersThermobaric = [];

    private static readonly AOEShape _shapeSalamander = new AOEShapeCross(20, 2);
    private static readonly AOEShape _shapeThermobaric = new AOEShapeCircle(13); // TODO: verify falloff

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _castersSalamander)
            yield return new(_shapeSalamander, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!));
        foreach (var c in _castersThermobaric)
            yield return new(_shapeThermobaric, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo!));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action, false)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action, true)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell, bool forRemove) => (AID)spell.ID switch
    {
        AID.AngrySalamanderAOE => forRemove || Module.PrimaryActor.IsTargetable ? _castersSalamander : null, // note: this component cares only about cast when boss is targetable
        AID.ThermobaricCharge => forRemove || Module.PrimaryActor.CastInfo == null ? _castersThermobaric : null, // note: this component cares about thermobarics while boss is not casting
        _ => null
    };
}

class D133LiviaStates : StateMachineBuilder
{
    public D133LiviaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AglaeaClimb>()
            .ActivateOnEnter<ArtificialPlasma>()
            .ActivateOnEnter<Roundhouse>()
            .ActivateOnEnter<InfiniteReach>()
            .ActivateOnEnter<StunningSweep>()
            .ActivateOnEnter<AngrySalamander>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 15, NameID = 2118)]
public class D133Livia(WorldState ws, Actor primary) : BossModule(ws, primary, new(-98, -33), new ArenaBoundsCircle(20));
