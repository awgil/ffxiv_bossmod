namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D043ZanderSnakeskinner;

public enum OID : uint
{
    BossP1 = 0x411E, // R2.100, x1
    BossP2 = 0x41BA, // R2.500, x1
    Helper = 0x233C, // R0.500, x12, Helper type
}

public enum AID : uint
{
    AutoAttack = 870, // BossP1/BossP2->player, no cast, single-target
    Electrothermia = 36594, // BossP1->self, 5.0s cast, range 60 circle, visual
    SoulbaneSaber = 36574, // BossP1->self, 3.0s cast, range 20 width 4 rect, visual
    SoulbaneSaberBurst = 36575, // Helper->self, 10.0s cast, range 20 width 40 rect
    SoulbaneSaberVisual = 39240, // Helper->self, 10.5s cast, range 20 width 40 rect
    SaberRush = 36595, // BossP1->player, 5.0s cast, single-target, tankbuster
    SoulbaneShock = 37922, // Helper->player, 5.0s cast, range 5 circle spread

    Intermission1 = 36576, // BossP1->self, no cast, single-target, visual
    Intermission2 = 36577, // BossP1->self, no cast, single-target, visual
    Intermission3 = 36578, // BossP1->self, no cast, single-target, visual

    Screech = 36596, // BossP2->self, 5.0s cast, range 60 circle, raidwide
    ShadeShot = 36597, // BossP2->player, 5.0s cast, single-target, tankbuster
    Syntheslean = 37198, // BossP2->self, 4.0s cast, range 19 90-degree cone
    Syntheslither1 = 36579, // BossP2->location, 4.0s cast, single-target
    Syntheslither1AOE1 = 36580, // Helper->self, 5.0s cast, range 19 90-degree cone
    Syntheslither1AOE2 = 36581, // Helper->self, 5.6s cast, range 19 90-degree cone
    Syntheslither1AOE3 = 36582, // Helper->self, 6.2s cast, range 19 90-degree cone
    Syntheslither1AOE4 = 36583, // Helper->self, 6.8s cast, range 19 90-degree cone
    Syntheslither2 = 36584, // BossP2->location, 4.0s cast, single-target
    Syntheslither2AOE1 = 36585, // Helper->self, 5.0s cast, range 19 90-degree cone
    Syntheslither2AOE2 = 36586, // Helper->self, 5.6s cast, range 19 90-degree cone
    Syntheslither2AOE3 = 36587, // Helper->self, 6.2s cast, range 19 90-degree cone
    Syntheslither2AOE4 = 36588, // Helper->self, 6.8s cast, range 19 90-degree cone
    SlitherbaneForeguard = 36589, // BossP2->self, 4.0s cast, range 20 width 4 rect
    SlitherbaneForeguardAOE = 36592, // Helper->self, 4.5s cast, range 20 180-degree cone
    SlitherbaneRearguard = 36590, // BossP2->self, 4.0s cast, range 20 width 4 rect
    SlitherbaneRearguardAOE = 36593, // Helper->self, 4.5s cast, range 20 180-degree cone
    SlitherbaneBurst = 36591, // Helper->self, 11.0s cast, range 20 width 40 rect
    SlitherbaneVisual = 39241, // Helper->self, 11.5s cast, range 20 width 40 rect
}

public enum IconID : uint
{
    SaberRush = 218, // player
    SoulbaneShock = 376, // player
}

class Electrothermia(BossModule module) : Components.RaidwideCast(module, AID.Electrothermia);
class SoulbaneSaber(BossModule module) : Components.StandardAOEs(module, AID.SoulbaneSaber, new AOEShapeRect(20, 2));
class SoulbaneSaberBurst(BossModule module) : Components.StandardAOEs(module, AID.SoulbaneSaberBurst, new AOEShapeRect(20, 20));
class SaberRush(BossModule module) : Components.SingleTargetCast(module, AID.SaberRush);
class SoulbaneShock(BossModule module) : Components.SpreadFromCastTargets(module, AID.SoulbaneShock, 5);
class BossP2(BossModule module) : Components.Adds(module, (uint)OID.BossP2);
class Screech(BossModule module) : Components.RaidwideCast(module, AID.Screech);
class ShadeShot(BossModule module) : Components.SingleTargetCast(module, AID.ShadeShot);

class SynthesleanSlither(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(19, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Syntheslean or AID.Syntheslither1AOE1 or AID.Syntheslither1AOE2 or AID.Syntheslither1AOE3 or AID.Syntheslither1AOE4 or AID.Syntheslither2AOE1 or AID.Syntheslither2AOE2 or AID.Syntheslither2AOE3 or AID.Syntheslither2AOE4)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Syntheslean or AID.Syntheslither1AOE1 or AID.Syntheslither1AOE2 or AID.Syntheslither1AOE3 or AID.Syntheslither1AOE4 or AID.Syntheslither2AOE1 or AID.Syntheslither2AOE2 or AID.Syntheslither2AOE3 or AID.Syntheslither2AOE4
            && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class SlitherbaneForeguard(BossModule module) : Components.StandardAOEs(module, AID.SlitherbaneForeguard, new AOEShapeRect(20, 2));
class SlitherbaneForeguardAoE(BossModule module) : Components.StandardAOEs(module, AID.SlitherbaneForeguardAOE, new AOEShapeCone(20, 90.Degrees()));
class SlitherbaneRearguard(BossModule module) : Components.StandardAOEs(module, AID.SlitherbaneRearguard, new AOEShapeRect(20, 2));
class SlitherbaneRearguardAoE(BossModule module) : Components.StandardAOEs(module, AID.SlitherbaneRearguardAOE, new AOEShapeCone(20, 90.Degrees()));
class SlitherbaneBurst(BossModule module) : Components.StandardAOEs(module, AID.SlitherbaneBurst, new AOEShapeRect(20, 20))
{
    private readonly SlitherbaneForeguardAoE? _foreguard = module.FindComponent<SlitherbaneForeguardAoE>();
    private readonly SlitherbaneRearguardAoE? _rearguard = module.FindComponent<SlitherbaneRearguardAoE>();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // ignore casters corresponding to incomplete fore/rearguard cleaves; note that cleave can start casting slightly earlier
        var numPendingBursts = (_foreguard?.NumCasts ?? 0) + (_rearguard?.NumCasts ?? 0) - NumCasts;
        return Casters.Take(numPendingBursts).Select(c => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo), Color, Risky));
    }
}

class D043ZanderSnakeskinnerStates : StateMachineBuilder
{
    public D043ZanderSnakeskinnerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrothermia>()
            .ActivateOnEnter<SoulbaneSaber>()
            .ActivateOnEnter<SoulbaneSaberBurst>()
            .ActivateOnEnter<SaberRush>()
            .ActivateOnEnter<SoulbaneShock>()
            .ActivateOnEnter<BossP2>()
            .ActivateOnEnter<Screech>()
            .ActivateOnEnter<ShadeShot>()
            .ActivateOnEnter<SynthesleanSlither>()
            .ActivateOnEnter<SlitherbaneForeguardAoE>()
            .ActivateOnEnter<SlitherbaneRearguardAoE>()
            .ActivateOnEnter<SlitherbaneForeguard>()
            .ActivateOnEnter<SlitherbaneRearguard>()
            .ActivateOnEnter<SlitherbaneBurst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12752)]
public class D043ZanderSnakeskinner(WorldState ws, Actor primary) : BossModule(ws, primary, new(90, -430), new ArenaBoundsCircle(17));
