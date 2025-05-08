namespace BossMod.Endwalker.Quest.WorthyOfHisBack;

public enum OID : uint
{
    Boss = 0x342C,
    DeathWall = 0x1EB27A,
    Helper = 0x233C, // R0.500, x22, Helper type
    TrueAeroIV = 0x342E, // R0.700, x0 (spawn during fight)
    Thelema = 0x342F, // R1.000, x0 (spawn during fight)
    ThelemaAgape = 0x3864, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 28383, // Boss->player, no cast, single-target
    AutoAttackDancer = 28384, // Boss->player, no cast, single-target
    TrueStone = 28385, // Boss->player, no cast, single-target, whm form autoattack
    TrueBlink = 25600, // Boss->self, 3.0s cast, single-target, boss dash to some location

    MousasMantle = 25592, // Boss->self, 6.5s cast, single-target, switch jobs to dancer
    MagosMantle = 25593, // Boss->self, 6.5s cast, single-target, switch jobs to whm

    Kleos = 25597, // Boss->self, 3.0s cast, range 40 circle, raidwide

    CircumzenithalArcVisual = 25598, // Boss->self, 5.0+2.2s cast, single-target
    CircumzenithalArcFirst = 28466, // 233C->self, 7.3s cast, range 40 180-degree cone
    CircumzenithalArcSecond = 28376, // 233C->self, 7.3s cast, range 40 180-degree cone

    CrepuscularRay = 25595, // 342D->location, 5.0s cast, width 8 rect charge

    CircleOfBrilliance = 25602, // Boss->self, 5.0s cast, range 5 circle

    EnomotosFirst = 25603, // 233C->self, 5.0s cast, range 6 circle
    EnomotosRest = 25604, // 233C->self, no cast, range 6 circle

    EnomotosSmall = 28392, // 233C->location, 3.0s cast, range 4 circle

    EpeaPteroentaFirst = 25605, // Boss->self, 7.0s cast, range 20 120-degree cone
    EpeaPteroentaSecond = 25607, // 233C->self, 8.0s cast, range 20 120-degree cone
    EpeaPteroentaIDK = 25606, // Boss->self, no cast, range 20 ?-degree cone

    ParhelionVisual = 25608, // Boss->self, 5.0s cast, single-target
    ParhelionVisual2 = 25609, // Boss->self, 5.0s cast, single-target
    ParhelionConeFirst = 25610, // 233C->self, 5.0s cast, range 20 45-degree cone
    ParhelionConeRest = 25611, // 233C->self, 1.5s cast, range 20 45-degree cone

    ParhelionDonut1 = 25612, // 233C->self, 4.0s cast, range 10 circle
    ParhelionDonut2 = 25613, // 233C->self, 4.0s cast, range 10-15 donut
    ParhelionDonut3 = 25614, // 233C->self, 4.0s cast, range 15-20 donut

    TrueAeroIV = 25615, // Boss->self, 3.0s cast, range 40 circle
    TrueHoly = 25619, // Boss->self, 5.0s cast, range 40 circle

    AfflatusAzemFirst = 25617, // Boss->location, 4.0s cast, range 5 circle
    AfflatusAzemChase = 25618, // Boss->location, 1.0s cast, range 5 circle

    Windage = 25616, // 342E->self, 2.5s cast, range 5 circle
    WindageSlow = 28116, // 342E->self, 9.0s cast, range 5 circle

    TrueStoneIVVisual = 25620, // Boss->self, 3.0s cast, single-target
    TrueStoneIV = 25621, // 233C->location, 6.0s cast, range 10 circle
}

class Kleos(BossModule module) : Components.RaidwideCast(module, AID.Kleos, "Raidwide + death wall spawn");

class ParhelionCone(BossModule module) : Components.GenericRotatingAOE(module)
{
    enum Direction
    {
        Unknown,
        CW,
        CCW
    }

    private Direction NextDirection;

    private Angle GetAngle(Direction d) => d switch
    {
        Direction.CCW => 45.Degrees(),
        Direction.CW => -45.Degrees(),
        _ => default
    };

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ParhelionConeFirst)
            Sequences.Add(new(new AOEShapeCone(20, 22.5f.Degrees()), caster.Position, caster.Rotation, GetAngle(NextDirection), Module.CastFinishAt(spell), 2.6f, 9));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ParhelionConeFirst or AID.ParhelionConeRest)
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == 168)
            NextDirection = Direction.CCW;
        if (iconID == 167)
            NextDirection = Direction.CW;

        for (var i = 0; i < Sequences.Count; i++)
            Sequences[i] = Sequences[i] with { Increment = GetAngle(NextDirection) };
    }
}
class ParhelionDonut(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 15), new AOEShapeDonut(15, 20)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ParhelionDonut1)
            AddSequence(Module.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.ParhelionDonut1 => 0,
            AID.ParhelionDonut2 => 1,
            AID.ParhelionDonut3 => 2,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2f)))
            ReportError($"unexpected order {order}");
    }
}

class EpeaPteroenta(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var imminent = true;
        foreach (var c in Casters.Take(2))
        {
            yield return new AOEInstance(new AOEShapeCone(20, 60.Degrees()), c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo), imminent ? ArenaColor.Danger : ArenaColor.AOE);
            imminent = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EpeaPteroentaFirst or AID.EpeaPteroentaSecond)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EpeaPteroentaFirst or AID.EpeaPteroentaSecond)
            Casters.Remove(caster);
    }
}

class CrepuscularRay(BossModule module) : Components.ChargeAOEs(module, AID.CrepuscularRay, 4);
class CircumzenithalArc(BossModule module) : Components.StandardAOEs(module, AID.CircumzenithalArcFirst, new AOEShapeCone(40, 90.Degrees()));
class CircumzenithalArcSecond(BossModule module) : Components.StandardAOEs(module, AID.CircumzenithalArcSecond, new AOEShapeCone(40, 90.Degrees()))
{
    private CrepuscularRay? ray;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        ray ??= Module.FindComponent<CrepuscularRay>();
        // skip forbidden zone creation if charges are still active, AI doesn't handle it well
        // TODO does this work better with new pathfinder?
        if (ray?.Casters.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}
class CircleOfBrilliance(BossModule module) : Components.StandardAOEs(module, AID.CircleOfBrilliance, new AOEShapeCircle(5));
class Enomotos(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(6), AID.EnomotosFirst)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            Lines.Add(new Line()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 5,
                Rotation = default,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = 9,
                MaxShownExplosions = 3
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EnomotosFirst or AID.EnomotosRest)
        {
            var line = Lines.FirstOrDefault(x => x.Next.AlmostEqual(caster.Position, 1));
            if (line != null)
                AdvanceLine(line, caster.Position);
        }
    }
}

class DeathWall(BossModule module) : Components.GenericAOEs(module, AID.Kleos)
{
    private DateTime? ExpectedActivation;
    private readonly AOEShape Voidzone = new AOEShapeDonut(19.5f, 100);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (ExpectedActivation != null)
            yield return new AOEInstance(Voidzone, Arena.Center, Activation: ExpectedActivation.Value);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            ExpectedActivation = WorldState.FutureTime(4.1f);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.DeathWall)
        {
            ExpectedActivation = null;
            Arena.Bounds = new ArenaBoundsCircle(19.5f);
        }
    }
}

class Windage(BossModule module) : Components.StandardAOEs(module, AID.Windage, new AOEShapeCircle(5));
class AfflatusAzem(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(5), AID.AfflatusAzemFirst, AID.AfflatusAzemChase, 5, 2.1f, 5);
class WindageSlow(BossModule module) : Components.StandardAOEs(module, AID.WindageSlow, new AOEShapeCircle(5));
class TrueHoly(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TrueHoly, 20)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var action = actor.Class.GetClassCategory() is ClassCategory.Healer or ClassCategory.Caster ? ActionID.MakeSpell(ClassShared.AID.Surecast) : ActionID.MakeSpell(ClassShared.AID.ArmsLength);
        if (Casters.FirstOrDefault()?.CastInfo?.NPCRemainingTime is var t && t < 5)
            hints.ActionsToExecute.Push(action, actor, ActionQueue.Priority.Medium);
    }
}
class TrueStoneIV(BossModule module) : Components.StandardAOEs(module, AID.TrueStoneIV, 10, maxCasts: 7);
class EnomotosSmall(BossModule module) : Components.StandardAOEs(module, AID.EnomotosSmall, 4);
class Adds(BossModule module) : Components.AddsMulti(module, [OID.Thelema, OID.ThelemaAgape], 1);

public class VenatStates : StateMachineBuilder
{
    public VenatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeathWall>()
            .ActivateOnEnter<Kleos>()
            .ActivateOnEnter<CircumzenithalArc>()
            .ActivateOnEnter<CircumzenithalArcSecond>()
            .ActivateOnEnter<CrepuscularRay>()
            .ActivateOnEnter<CircleOfBrilliance>()
            .ActivateOnEnter<Enomotos>()
            .ActivateOnEnter<EpeaPteroenta>()
            .ActivateOnEnter<ParhelionDonut>()
            .ActivateOnEnter<ParhelionCone>()
            .ActivateOnEnter<Windage>()
            .ActivateOnEnter<AfflatusAzem>()
            .ActivateOnEnter<WindageSlow>()
            .ActivateOnEnter<TrueHoly>()
            .ActivateOnEnter<TrueStoneIV>()
            .ActivateOnEnter<EnomotosSmall>()
            .ActivateOnEnter<Adds>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69968, NameID = 10586)]
public class Venat(WorldState ws, Actor primary) : BossModule(ws, primary, new(-630, 72), new ArenaBoundsCircle(24.5f));
