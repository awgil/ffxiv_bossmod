namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D041VanguardCommander;

public enum OID : uint
{
    Boss = 0x411D, // R3.240, x1
    Helper = 0x233C, // R0.500, x7, Helper type
    VanguardSentryR8 = 0x41BC, // R3.240, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 36403, // Boss->player, no cast, single-target
    Electrowave = 36571, // Boss->self, 5.0s cast, range 60 circle, raidwide
    EnhancedMobilityROut = 36559, // Boss->location, 10.0s cast, range 14 width 6 rect, teleport (right hand + move so that out is safe)
    EnhancedMobilityRIn = 36560, // Boss->location, 10.0s cast, range 14 width 6 rect, teleport (right hand + move so that in is safe)
    EnhancedMobilityLIn = 39140, // Boss->location, 10.0s cast, range 14 width 6 rect (right hand + move so that in is safe)
    EnhancedMobilityLOut = 39141, // Boss->location, 10.0s cast, range 14 width 6 rect (left hand + move so that in is safe)
    EnhancedMobilityAOEROut = 36563, // Helper->self, 10.5s cast, range 10 width 14 rect
    EnhancedMobilityAOELOut = 36564, // Helper->self, 10.5s cast, range 10 width 14 rect
    EnhancedMobilityAOERIn = 37184, // Helper->self, 10.5s cast, range 20 width 14 rect
    EnhancedMobilityAOELIn = 37191, // Helper->self, 10.5s cast, range 20 width 14 rect
    RapidRotaryROut = 36561, // Boss->self, no cast, single-target, visual
    RapidRotaryRIn = 36562, // Boss->self, no cast, single-target, visual
    RapidRotaryLIn = 39142, // Boss->self, no cast, single-target, visual
    RapidRotaryLOut = 39143, // Boss->self, no cast, single-target, visual
    RapidRotaryAOE = 36565, // Helper->self, no cast, range 11-17 donut 120-degree cone (common to in & out)
    RapidRotaryAOEOut = 36566, // Helper->self, no cast, range 14 120-degree cone
    RapidRotaryAOEIn = 36567, // Helper->self, no cast, range 14-28 donut 120-degree cone
    Dispatch = 36568, // Boss->self, 4.0s cast, single-target, visual (spawn sentries)
    Rush = 36569, // VanguardSentryR8->location, 6.0s cast, width 5 rect charge
    AerialOffensive = 36570, // VanguardSentryR8->location, 9.0s cast, range 4+10 circle puddle
    Electrosurge = 36572, // Boss->self, 4.0+1.0s cast, single-target, visual (spread)
    ElectrosurgeAOE = 36573, // Helper->player, 5.0s cast, range 5 circle spread
}

public enum IconID : uint
{
    Electrosurge = 315, // player
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));

class EnhancedMobility(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shapeSideOut = new(10, 7);
    private static readonly AOEShapeRect _shapeSideIn = new(20, 7);
    private static readonly AOEShape _shapeOut = new AOEShapeCone(17, 60.Degrees());
    private static readonly AOEShape _shapeIn = new AOEShapeDonutSector(11, 28, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        (AOEShape? side, AOEShape? main, float offset, Angle rotation) = (AID)spell.Action.ID switch
        {
            AID.EnhancedMobilityAOEROut => (_shapeSideOut, _shapeOut, +7, -60.Degrees()),
            AID.EnhancedMobilityAOELOut => (_shapeSideOut, _shapeOut, -7, 60.Degrees()),
            AID.EnhancedMobilityAOERIn => (_shapeSideIn, _shapeIn, +7, 60.Degrees()),
            AID.EnhancedMobilityAOELIn => (_shapeSideIn, _shapeIn, -7, -60.Degrees()),
            _ => (null, null, 0, default)
        };
        if (side != null && main != null)
        {
            var activationSide = Module.CastFinishAt(spell);
            var activationMain = activationSide.AddSeconds(1.3f); // note: the main cones are staggered by 0.3s, but it's not very good for ai...
            _aoes.Add(new(side, caster.Position + offset * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, activationSide));
            _aoes.Add(new(main, caster.Position, spell.Rotation + rotation, activationMain));
            _aoes.Add(new(main, caster.Position, spell.Rotation + rotation * 3, activationMain));
            _aoes.Add(new(main, caster.Position, spell.Rotation + rotation * 5, activationMain));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EnhancedMobilityAOEROut or AID.EnhancedMobilityAOELOut or AID.EnhancedMobilityAOERIn or AID.EnhancedMobilityAOELIn or AID.RapidRotaryAOEOut or AID.RapidRotaryAOEIn && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class Rush(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Rush), 2.5f);
class AerialOffensive(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AerialOffensive), 14, maxCasts: 4);
class Electrosurge(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ElectrosurgeAOE), 5);

class D041VanguardCommanderStates : StateMachineBuilder
{
    public D041VanguardCommanderStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<EnhancedMobility>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<AerialOffensive>()
            .ActivateOnEnter<Electrosurge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12750)]
public class D041VanguardCommander(WorldState ws, Actor primary) : BossModule(ws, primary, new(-100, 207), new ArenaBoundsSquare(17));
