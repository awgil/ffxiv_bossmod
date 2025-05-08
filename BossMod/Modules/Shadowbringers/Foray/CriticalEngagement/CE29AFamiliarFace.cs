namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE29AFamiliarFace;

public enum OID : uint
{
    Boss = 0x2DD2, // R9.450, x1
    PhantomHashmal = 0x3321, // R9.450, x1
    Helper = 0x233C, // R0.500, x24
    ArenaFeatures = 0x1EA1A1, // R2.000, x9, EventObj type
    Tower = 0x1EB17E, // R0.500, EventObj type, spawn during fight
    FallingTower = 0x1EB17D, // R0.500, EventObj type, spawn during fight, rotation at spawn determines fall direction?..
    Hammer = 0x1EB17F, // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    TectonicEruption = 23826, // Helper->location, 4.0s cast, range 6 circle puddle
    RockCutter = 23827, // Boss->player, 5.0s cast, single-target, tankbuster
    AncientQuake = 23828, // Boss->self, 5.0s cast, single-target, visual
    AncientQuakeAOE = 23829, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Sanction = 23817, // Boss->self, no cast, single-target, visual (light raidwide)
    SanctionAOE = 23832, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Roxxor = 23823, // Helper->players, 5.0s cast, range 6 circle spread

    ControlTowerAppear = 23830, // Helper->self, 4.0s cast, range 6 circle aoe around appearing towers
    TowerRound = 23831, // Boss->self, 4.0s cast, single-target, visual (spawns 2 towers + light raidwide)
    TowerRoundAOE = 23834, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    ControlTower = 23816, // Boss->self, 4.0s cast, single-target, visual (spawns 3 towers + light raidwide)
    ControlTowerAOE = 23833, // Helper->self, no cast, ??? (x2 ~0.8s after visual)
    Towerfall = 23818, // Helper->self, 7.0s cast, range 40 width 10 rect aoe

    PhantomOrder = 24702, // Boss->self, 4.0s cast, single-target, visual
    ExtremeEdgeR = 23821, // PhantomHashmal->self, 8.0s cast, range 60 width 36 rect aoe offset to the right
    ExtremeEdgeL = 23822, // PhantomHashmal->self, 8.0s cast, range 60 width 36 rect aoe offset to the left

    IntractableLand = 24576, // Boss->self, 5.0s cast, single-target, visual (double exaflares)
    IntractableLandFirst = 23819, // Helper->self, 5.3s cast, range 8 circle
    IntractableLandRest = 23820, // Helper->location, no cast, range 8 circle

    HammerRound = 23824, // Boss->self, 5.0s cast, single-target, visual
    Hammerfall = 23825, // Helper->self, 8.0s cast, range 37 circle aoe
}

class TectonicEruption(BossModule module) : Components.StandardAOEs(module, AID.TectonicEruption, 6);
class RockCutter(BossModule module) : Components.SingleTargetDelayableCast(module, AID.RockCutter);
class AncientQuake(BossModule module) : Components.RaidwideCast(module, AID.AncientQuake);
class Roxxor(BossModule module) : Components.SpreadFromCastTargets(module, AID.Roxxor, 6);
class ControlTowerAppear(BossModule module) : Components.StandardAOEs(module, AID.ControlTowerAppear, new AOEShapeCircle(6));

// note: we could predict aoes way in advance, when FallingTower actors are created - they immediately have correct rotation
// if previous cast was TowerRound, delay is ~24.4s; otherwise if previous cast was ControlTower, delay is ~9.6s; otherwise it is ~13s
// however, just watching casts normally gives more than enough time to avoid aoes and does not interfere with mechanics that resolve earlier
class Towerfall(BossModule module) : Components.StandardAOEs(module, AID.Towerfall, new AOEShapeRect(40, 5));

class ExtremeEdge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor caster, float offset)> _casters = [];

    private static readonly AOEShapeRect _shape = new(60, 18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
            yield return new(_shape, c.caster.Position + c.offset * c.caster.CastInfo!.Rotation.ToDirection().OrthoL(), c.caster.CastInfo.Rotation, Module.CastFinishAt(c.caster.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var offset = (AID)spell.Action.ID switch
        {
            AID.ExtremeEdgeL => 12,
            AID.ExtremeEdgeR => -12,
            _ => 0
        };
        if (offset != 0)
            _casters.Add((caster, offset));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ExtremeEdgeL or AID.ExtremeEdgeR)
            _casters.RemoveAll(c => c.caster == caster);
    }
}

class IntractableLand(BossModule module) : Components.Exaflare(module, 8)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.IntractableLandFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 8 * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 0.8f, ExplosionsLeft = 8, MaxShownExplosions = 4 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.IntractableLandFirst or AID.IntractableLandRest)
        {
            var pos = (AID)spell.Action.ID == AID.IntractableLandFirst ? caster.Position : spell.TargetXZ;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(pos, 1));
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], pos);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class Hammerfall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(37);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Hammer)
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(12.6f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hammerfall && _aoes.Count > 0)
            _aoes.RemoveAt(0);
    }
}

class FourthMakeHashmalStates : StateMachineBuilder
{
    public FourthMakeHashmalStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TectonicEruption>()
            .ActivateOnEnter<RockCutter>()
            .ActivateOnEnter<AncientQuake>()
            .ActivateOnEnter<Roxxor>()
            .ActivateOnEnter<ControlTowerAppear>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<ExtremeEdge>()
            .ActivateOnEnter<IntractableLand>()
            .ActivateOnEnter<Hammerfall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 29)] // bnpcname=9693
public class FourthMakeHashmal(WorldState ws, Actor primary) : BossModule(ws, primary, new(330, 390), new ArenaBoundsCircle(30));
