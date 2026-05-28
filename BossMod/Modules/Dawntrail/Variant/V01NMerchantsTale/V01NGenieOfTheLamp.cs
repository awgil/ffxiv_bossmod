namespace BossMod.Dawntrail.Variant.V01NMerchantsTale.V01NGenieOfTheLamp;

public enum OID : uint
{
    Boss = 0x4867, // R3.750, x?    
    Airship = 0x4869, // R3.750, x?
    Lever = 0x486A, // R1.100, x0 (spawn during fight)
    DousingSpirit = 0x4868, // R2.400, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x?, Helper type
}
public enum AID : uint
{
    FabulousFirecrackersVisual1 = 43352, // 4867->self, 6.0+1.0s cast, single-target
    FabulousFirecrackersVisual2 = 43351, // 4867->self, 6.0+1.0s cast, single-target
    FabulousFirecrackersAOE1 = 43353, // 233C->self, 7.0s cast, range 30 180-degree cone
    FabulousFirecrackersAOE2 = 43354, // 233C->self, 7.0s cast, range 30 90-degree cone

    ParadeOfWonders = 43345, // 4867->self, 5.0s cast, range 60 circle
    SpectacularSparksVisual = 43348, // 4867->self, 4.0s cast, single-target
    SpectacularSparks = 43349, // 233C->self, 6.5s cast, range 36 width 6 rect
    SpectacularSparks2 = 43347, // 4867->self, 4.0s cast, single-target

    ASailorsTale = 43356, // 4867->self, 4.5s cast, single-target
    ChartCourse = 43359, // 4867->self, 7.0s cast, single-target
    Voyage = 47042, // 4869->self, 8.3s cast, range 8 width 12 rect
    Voyage1 = 43360, // 233C->self, no cast, range 12 width 12 rect
    Voyage2 = 43361, // 233C->self, 0.5s cast, range 12 width 12 rect
    Voyage3 = 43362, // 233C->self, 0.5s cast, range 12 width 12 rect
    Voyage4 = 43363, // 233C->self, no cast, range 8 width 12 rect
    Voyage5 = 43364, // 233C->self, 0.5s cast, range 22 width 12 rect
    Voyage6 = 43646, // 233C->self, 0.5s cast, range 21 width 12 rect
    Voyage7 = 43674, // 233C->self, 0.5s cast, range 20 width 12 rect
    Voyage8 = 43679, // 233C->self, 0.5s cast, range 22 width 12 rect
    Voyage9 = 43729, // 233C->self, 0.6s cast, range 20 width 12 rect

    ExplosiveEnding = 43346, // 4867->self, 5.0s cast, range 60 circle
    PyromagicksVisual = 44260, // 4867->self, 4.0+1.0s cast, single-target
    Pyromagicks1 = 44261, // 233C->self, 5.0s cast, range 6 circle
    Pyromagicks2 = 44265, // 233C->self, no cast, range 6 circle

    LampLighting = 45586, // 4867->self, 3.5s cast, range 60 width 8 rect
    FanningFlameVisual = 44253, // 4867->self, 3.5+1.0s cast, single-target
    FanningFlame1 = 44254, // 233C->self, 4.5s cast, range 30 45-degree cone
    FanningFlame2 = 44255, // 233C->self, 6.5s cast, range 30 45-degree cone
    SupernaturalSurprise = 43343, // 4867->self, 5.0s cast, range 60 circle

    RubBurn = 43344, // 4867->player, 5.0s cast, single-target
    RainbowRoad = 44663, // 4867->self, 4.0+1.0s cast, single-target
    RainbowRoadShort = 44771, // 233C->self, 5.0s cast, range 15 circle
    RainbowRoadLong = 44821, // 233C->self, 12.0s cast, range 15 circle

    LampOil = 44249, // 4867->self, 7.5+1.0s cast, single-target
    LampOil1 = 44252, // 233C->self, 8.5s cast, range 8 circle

    AetherialBlizzard = 44344, // 4868->self, 8.0s cast, range 36 width 4 rect

}
public enum IconID : uint
{
    TankLockOn = 218, // player->self
}

public enum TetherID : uint
{
    AirshipTether = 102, // Airship->Boss
    LeverTether = 86, // 486A->Boss
}

class FabulousFirecrackersCleave(BossModule module) : Components.StandardAOEs(module, AID.FabulousFirecrackersAOE1, new AOEShapeCone(30f, 90.Degrees()));
class FabulousFirecrackersCone(BossModule module) : Components.StandardAOEs(module, AID.FabulousFirecrackersAOE2, new AOEShapeCone(30f, 45.Degrees()));
class SpectacularSparks(BossModule module) : Components.StandardAOEs(module, AID.SpectacularSparks, new AOEShapeRect(36f, 3f), 3);
class ChartCourse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Actor> _airships = [];
    private static readonly AOEShapeRect rectHalfRoom = new(23f, 70f, 23f);
    private static readonly AOEShapeRect rectAngle = new(14f, 14f, 14f);
    private static readonly AOEShapeRect rectShip = new(17.5f, 6f, 17.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.LeverTether)
        {
            if (source?.OID == (uint)OID.Lever)
            {
                _aoes.Add(new(rectHalfRoom, source.Position));
            }
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Airship && !actor.Position.AlmostEqual(Module.Center, 5))
            _airships.Add(actor);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChartCourse)
        {
            _aoes.Add(new(rectAngle, Module.PrimaryActor.Position, 45.Degrees()));
            foreach (var a in _airships)
                _aoes.Add(new(rectShip, a.Position, a.Rotation));
        }
        if ((AID)spell.Action.ID == AID.ExplosiveEnding)
        {
            _aoes.Clear();
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)(spell.Action.ID) == AID.FanningFlameVisual)
        {
            _aoes.Clear();
        }
    }
}
class Pyromagicks(BossModule module) : Components.Exaflare(module, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Pyromagicks1)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 8.5f * spell.Rotation.ToDirection(), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1.1f, ExplosionsLeft = 5, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Pyromagicks1 or AID.Pyromagicks2)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}
class LampLighting(BossModule module) : Components.StandardAOEs(module, AID.LampLighting, new AOEShapeRect(60f, 4f));
class FanningFlame(BossModule module) : Components.GroupedAOEs(module, [AID.FanningFlame1, AID.FanningFlame2], new AOEShapeCone(30f, 22.5f.Degrees()), 4, true);
class ParadeOfWonders(BossModule module) : Components.RaidwideCast(module, AID.ParadeOfWonders);
class ExplosiveEnding(BossModule module) : Components.RaidwideCast(module, AID.ExplosiveEnding);
class SupernaturalSurprise(BossModule module) : Components.RaidwideCast(module, AID.SupernaturalSurprise);
class RubBurn(BossModule module) : Components.SingleTargetCast(module, AID.RubBurn);
class RainbowRoad(BossModule module) : Components.GroupedAOEs(module, [AID.RainbowRoadShort, AID.RainbowRoadLong], new AOEShapeCircle(15f), 2);
class LampOil(BossModule module) : Components.GroupedAOEs(module, [AID.LampOil, AID.LampOil1], new AOEShapeCircle(8f));
class AetherialBlizzard(BossModule module) : Components.StandardAOEs(module, AID.AetherialBlizzard, new AOEShapeRect(36, 2));

class V01NGenieOfTheLampStates : StateMachineBuilder
{
    public V01NGenieOfTheLampStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FabulousFirecrackersCleave>()
            .ActivateOnEnter<FabulousFirecrackersCone>()
            .ActivateOnEnter<SpectacularSparks>()
            .ActivateOnEnter<ChartCourse>()
            .ActivateOnEnter<Pyromagicks>()
            .ActivateOnEnter<LampLighting>()
            .ActivateOnEnter<FanningFlame>()
            .ActivateOnEnter<ParadeOfWonders>()
            .ActivateOnEnter<ExplosiveEnding>()
            .ActivateOnEnter<SupernaturalSurprise>()
            .ActivateOnEnter<RubBurn>()
            .ActivateOnEnter<RainbowRoad>()
            .ActivateOnEnter<LampOil>()
            .ActivateOnEnter<AetherialBlizzard>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1066, NameID = 14396)]
public class V01NGenieOfTheLamp(WorldState ws, Actor primary) : BossModule(ws, primary, new(primary.Position.X, primary.Position.Z + 6), new ArenaBoundsSquare(17.5f));
