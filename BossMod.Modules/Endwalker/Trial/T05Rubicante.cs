namespace BossMod.Endwalker.Trial.T05Rubicante;

public enum OID : uint
{
    Boss = 0x3AEE, // R10.000, x1
    Helper = 0x233C, // Helper type
    RubicanteMirage = 0x3D85, // x8, Scalding Fleet source
    CircleOfPurgatory = 0x3D86, // x8, outer ring triangles
    CircleOfPurgatoryInner = 0x3D95, // x1
    CircleOfPurgatoryMiddle = 0x3D96, // x1
    CircleOfPurgatoryOuter = 0x3D97, // x1
    CircleOfPurgatoryFireball = 0x3D98, // x2
    GreaterFlamesent = 0x3D88, // spawn during fight
    FlamesentHeat = 0x3D89, // spawn during fight, Shattering Heat
    FlamesentWind = 0x3D8A, // spawn during fight, Ghastly Wind
    FlamesentFlame = 0x3D8B, // spawn during fight, Ghastly Flame
}

public enum AID : uint
{
    Inferno1 = 31978, // Boss->self, 5.0s cast, range 60 circle, raidwide
    ShatteringHeat1 = 31979, // Boss->player, 5.0s cast, range 4 circle, tankbuster
    HopeAbandonYe = 32550, // Boss->self, no cast, single-target, visual
    ArcaneRevelation = 31934, // Boss->self, no cast, single-target, visual
    NinthCircle = 32802, // Boss->self, no cast, single-target, visual
    OrdealOfPurgation1 = 31940, // Boss->self, 10.0s cast, single-target, players are shackled as it ends
    OrdealOfPurgation2 = 33000, // Boss->self, 10.0s cast, single-target, players are shackled as it ends
    FieryExpiation = 31941, // CircleOfPurgatory->self, 2.0s cast, range 40 60-degree cone, from the edge
    FieryExpiationRect = 31942, // CircleOfPurgatory->self, 2.0s cast, range 20 width 40 rect, not seen
    ArchInferno = 31943, // Boss->self, 6.0s cast, range 5 circle
    ArchInfernoTick = 31944, // Helper->self, no cast, range 5 circle, ticks until Radial Flagration
    InfernoDevilFirst = 31945, // Helper->self, 6.0s cast, range 10 circle
    InfernoDevilRest = 31946, // Helper->self, 5.0s cast, range 10 circle
    InfernalSlaughter = 31950, // Boss->self, no cast, single-target, visual
    Hellscathe = 31951, // Helper->location, 4.0s cast, range 5 circle
    Conflagration = 31947, // Boss->self, 5.0s cast, range 40 width 10 rect, from the edge
    RadialFlagration = 31948, // Boss->self, 5.0s cast, single-target, visual
    RadialFlagrationAOE = 31949, // Helper->self, 6.0s cast, range 21 30-degree cone, x8
    GhastlyTorch = 31956, // GreaterFlamesent->self, 4.0s cast, range 60 circle, raidwide
    GhastlyFlame = 31959, // FlamesentFlame->self, 4.0s cast, single-target, visual
    GhastlyFlameAOE = 31960, // Helper->location, 4.0s cast, range 5 circle
    GhastlyWind = 31958, // FlamesentWind->self, 5.0s cast, range 40 60-degree cone
    ShatteringHeat2 = 31957, // FlamesentHeat->player, 5.0s cast, range 3 circle, tankbuster
    BlazingRapture = 31954, // Helper->self, 13.7s cast, range 60 circle, raidwide
    Inferno2 = 31962, // Helper->player, 5.0s cast, range 5 circle, spread
    ExplosivePyre = 31963, // Boss->player, no cast, range 2 circle, auto-attack
    Flamerake = 31965, // Boss->self, 6.0s cast, single-target, visual
    Flamerake1 = 31967, // Helper->self, 1.8s cast, range 40 width 12 rect, from the edge
    Flamerake2 = 31968, // Helper->self, 2.8s cast, range 8 width 40 rect
    Flamerake3 = 31969, // Helper->self, 3.8s cast, range 8 width 40 rect
    Soulscald = 31974, // Boss->self, 5.0s cast, single-target, visual
    ScaldingFleetMark = 31939, // Helper->player, no cast, marks every player for Scalding Fleet
    ScaldingFleetFirst = 31976, // RubicanteMirage->location, no cast, range 40 width 6 rect, from the boss through each player
    ScaldingFleetSecond = 31977, // RubicanteMirage->location, 3.0s cast, range 60 width 6 rect
    Dualfire = 32149, // Boss->self, 5.0s cast, single-target, visual
    DualfireAOE = 32150, // Helper->self, no cast, range 60 cone, on both tanks
    SweepingImmolationSpread = 31970, // Boss->self, 5.0s cast, range 20 180-degree cone
    SweepingImmolationStack = 31971, // Boss->self, 5.0s cast, range 20 180-degree cone, not seen
    PartialImmolation = 31972, // Helper->player, 5.5s cast, range 5 circle, spread
    TotalImmolation = 31973, // Helper->player, 5.5s cast, range 6 circle, stack, not seen
}

public enum SID : uint
{
    Pattern = 2056, // none->CircleOfPurgatoryInner/CircleOfPurgatoryMiddle/CircleOfPurgatoryOuter, extra=0x21E/0x221/0x222
}

public enum IconID : uint
{
    Dualfire = 230, // player
}

class Inferno1(BossModule module) : Components.RaidwideCast(module, AID.Inferno1);
class ShatteringHeat1(BossModule module) : Components.SpreadFromCastTargets(module, AID.ShatteringHeat1, 4);

class OrdealOfPurgation(BossModule module) : Components.GenericAOEs(module, AID.FieryExpiation)
{
    private static readonly AOEShapeCone _shape = new(40, 30.Degrees());

    private int _dirInner;
    private int _dirInnerExtra;
    private int _midIncrement;
    private int _midDecrement;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default)
            yield break;
        yield return AOEFromDirection(_dirInner);
        if (_dirInnerExtra != _dirInner)
            yield return AOEFromDirection(_dirInnerExtra);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID != SID.Pattern)
            return;
        switch ((OID)actor.OID)
        {
            case OID.CircleOfPurgatoryInner:
                _dirInner = AngleToDirectionIndex(actor.Rotation);
                _dirInnerExtra = status.Extra switch
                {
                    0x21F => AngleToDirectionIndex(actor.Rotation - 90.Degrees()),
                    0x220 => AngleToDirectionIndex(actor.Rotation + 180.Degrees()),
                    _ => _dirInner
                };
                break;
            case OID.CircleOfPurgatoryMiddle:
                _midIncrement = AngleToDirectionIndex(actor.Rotation - 135.Degrees());
                _midDecrement = AngleToDirectionIndex(actor.Rotation + 135.Degrees());
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.OrdealOfPurgation1:
            case AID.OrdealOfPurgation2:
                _activation = Module.CastFinishAt(spell);
                break;
            case AID.FieryExpiation:
                if (!ActiveAOEs(0, caster).Any(a => a.Origin.AlmostEqual(spell.LocXZ, 1)))
                    ReportError($"Predicted fireball exit does not match the cast at {spell.LocXZ}");
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _activation = default;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var dir = state switch
        {
            0x00020001 => -1,
            0x00200010 => +1,
            _ => 0
        };
        if (dir == 0)
            return;
        switch (index)
        {
            case 1:
                _dirInner = NormalizeDirectionIndex(_dirInner + dir);
                _dirInnerExtra = NormalizeDirectionIndex(_dirInnerExtra + dir);
                break;
            case 2:
                _midIncrement = NormalizeDirectionIndex(_midIncrement + dir);
                _midDecrement = NormalizeDirectionIndex(_midDecrement + dir);
                break;
        }
    }

    private static int NormalizeDirectionIndex(int index) => index & 7;
    private static int AngleToDirectionIndex(Angle rotation) => NormalizeDirectionIndex((int)(Math.Round(rotation.Deg / 45) + 4));
    private static Angle DirectionIndexToAngle(int index) => (index - 4) * 45.Degrees();

    private int TransformByMiddle(int index)
    {
        if (index == _midIncrement)
            return NormalizeDirectionIndex(index + 1);
        if (index == _midDecrement)
            return NormalizeDirectionIndex(index - 1);
        return index;
    }

    private AOEInstance AOEFromDirection(int index)
    {
        var dir = DirectionIndexToAngle(TransformByMiddle(index));
        return new(_shape, Module.Center + Module.Bounds.Radius * dir.ToDirection(), dir + 180.Degrees(), _activation);
    }
}

class FieryExpiationRect(BossModule module) : Components.StandardAOEs(module, AID.FieryExpiationRect, new AOEShapeRect(20, 20));

class ArchInferno(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(5);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArchInferno)
            _aoe = new(_shape, caster.Position, default, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RadialFlagrationAOE)
            _aoe = null;
    }
}

class InfernoDevilFirst(BossModule module) : Components.StandardAOEs(module, AID.InfernoDevilFirst, 10);
class InfernoDevilRest(BossModule module) : Components.StandardAOEs(module, AID.InfernoDevilRest, 10);
class Hellscathe(BossModule module) : Components.StandardAOEs(module, AID.Hellscathe, 5);
class Conflagration(BossModule module) : Components.StandardAOEs(module, AID.Conflagration, new AOEShapeRect(40, 5));
class RadialFlagration(BossModule module) : Components.StandardAOEs(module, AID.RadialFlagrationAOE, new AOEShapeCone(21, 15.Degrees()));

class GreaterFlamesent(BossModule module) : Components.Adds(module, (uint)OID.GreaterFlamesent);
class FlamesentHeat(BossModule module) : Components.Adds(module, (uint)OID.FlamesentHeat);
class FlamesentWind(BossModule module) : Components.Adds(module, (uint)OID.FlamesentWind);
class FlamesentFlame(BossModule module) : Components.Adds(module, (uint)OID.FlamesentFlame);
class GhastlyTorch(BossModule module) : Components.RaidwideCast(module, AID.GhastlyTorch);
class GhastlyFlame(BossModule module) : Components.StandardAOEs(module, AID.GhastlyFlameAOE, 5);
class GhastlyWind(BossModule module) : Components.StandardAOEs(module, AID.GhastlyWind, new AOEShapeCone(40, 30.Degrees()));
class ShatteringHeat2(BossModule module) : Components.SpreadFromCastTargets(module, AID.ShatteringHeat2, 3);
class BlazingRapture(BossModule module) : Components.RaidwideCast(module, AID.BlazingRapture);

class Inferno2(BossModule module) : Components.SpreadFromCastTargets(module, AID.Inferno2, 5);
class Flamerake1(BossModule module) : Components.StandardAOEs(module, AID.Flamerake1, new AOEShapeRect(40, 6));
class Flamerake2(BossModule module) : Components.StandardAOEs(module, AID.Flamerake2, new AOEShapeRect(8, 20));
class Flamerake3(BossModule module) : Components.StandardAOEs(module, AID.Flamerake3, new AOEShapeRect(8, 20));

class ScaldingFleetFirst(BossModule module) : Components.GenericBaitAway(module, AID.ScaldingFleetFirst)
{
    private static readonly AOEShapeRect _shape = new(40, 3);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ScaldingFleetMark && WorldState.Actors.Find(spell.MainTargetID) is { } target)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, target, _shape, WorldState.FutureTime(5.1f)));
        }
        else if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

class ScaldingFleetSecond(BossModule module) : Components.StandardAOEs(module, AID.ScaldingFleetSecond, new AOEShapeRect(60, 3));

class Dualfire(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 60.Degrees()), (uint)IconID.Dualfire, AID.DualfireAOE, 6, damageType: AIHints.PredictedDamageType.None);

class SweepingImmolationSpread(BossModule module) : Components.StandardAOEs(module, AID.SweepingImmolationSpread, new AOEShapeCone(20, 90.Degrees()));
class SweepingImmolationStack(BossModule module) : Components.StandardAOEs(module, AID.SweepingImmolationStack, new AOEShapeCone(20, 90.Degrees()));
class PartialTotalImmolation(BossModule module) : Components.CastStackSpread(module, AID.TotalImmolation, AID.PartialImmolation, 6, 5, 8, 8, true);

class T05RubicanteStates : StateMachineBuilder
{
    public T05RubicanteStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Inferno1>()
            .ActivateOnEnter<ShatteringHeat1>()
            .ActivateOnEnter<OrdealOfPurgation>()
            .ActivateOnEnter<FieryExpiationRect>()
            .ActivateOnEnter<ArchInferno>()
            .ActivateOnEnter<InfernoDevilFirst>()
            .ActivateOnEnter<InfernoDevilRest>()
            .ActivateOnEnter<Hellscathe>()
            .ActivateOnEnter<Conflagration>()
            .ActivateOnEnter<RadialFlagration>()
            .ActivateOnEnter<GreaterFlamesent>()
            .ActivateOnEnter<FlamesentHeat>()
            .ActivateOnEnter<FlamesentWind>()
            .ActivateOnEnter<FlamesentFlame>()
            .ActivateOnEnter<GhastlyTorch>()
            .ActivateOnEnter<GhastlyFlame>()
            .ActivateOnEnter<GhastlyWind>()
            .ActivateOnEnter<ShatteringHeat2>()
            .ActivateOnEnter<BlazingRapture>()
            .ActivateOnEnter<Inferno2>()
            .ActivateOnEnter<Flamerake1>()
            .ActivateOnEnter<Flamerake2>()
            .ActivateOnEnter<Flamerake3>()
            .ActivateOnEnter<ScaldingFleetFirst>()
            .ActivateOnEnter<ScaldingFleetSecond>()
            .ActivateOnEnter<Dualfire>()
            .ActivateOnEnter<SweepingImmolationSpread>()
            .ActivateOnEnter<SweepingImmolationStack>()
            .ActivateOnEnter<PartialTotalImmolation>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 886, NameID = 12054)]
public class T05Rubicante(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
