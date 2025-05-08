namespace BossMod.Endwalker.Dungeon.D02TowerOfBabil.D022Lugae;

public enum OID : uint
{
    Boss = 0x33FA, // R=3.9
    MagitekChakram = 0x33FB, // R=3.0
    MagitekExplosive = 0x33FC, // R=2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Downpour = 25333, // Boss->self, 5.0s cast, single-target
    Explosion = 25337, // MagitekExplosive->self, 7.0s cast, range 40 width 8 cross
    MagitekChakram = 25331, // Boss->self, 5.0s cast, single-target
    MagitekExplosive = 25336, // Boss->self, 5.0s cast, single-target
    MagitekMissile = 25334, // Boss->self, 3.0s cast, single-target
    MagitekRay = 25340, // Boss->self, 3.0s cast, range 50 width 6 rect
    MightyBlow = 25332, // MagitekChakram->self, 7.0s cast, range 40 width 8 rect
    SurfaceMissile = 25335, // Helper->location, 3.5s cast, range 6 circle
    ThermalSuppression = 25338 // Boss->self, 5.0s cast, range 60 circle
}

public enum SID : uint
{
    Minimum = 2504, // none->player, extra=0x14
    Breathless = 2672, // none->player, extra=0x1/0x2/0x3/0x4/0x5/0x6
    Heavy = 2391, // none->player, extra=0x32
    Toad = 2671 // none->player, extra=0x1B1
}

class DownpourMagitekChakram(BossModule module) : Components.GenericAOEs(module)
{
    private enum Mechanic { None, Downpour, Chakram }
    private Mechanic CurrentMechanic { get; set; }
    private static readonly AOEShapeRect square = new(4, 4, 4);
    private static readonly WPos toad = new(213, 306);
    private static readonly WPos mini = new(229, 306);
    private const string toadHint = "Walk onto green square!";
    private const string miniHint = "Walk onto purple square!";
    private bool avoidSquares;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (CurrentMechanic == Mechanic.Downpour)
        {
            var breathless = actor.FindStatus(SID.Breathless) != null;
            yield return new(breathless ? square : square, toad, Color: breathless ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
            yield return new(square, mini);
        }
        else if (CurrentMechanic == Mechanic.Chakram)
        {
            var minimum = !avoidSquares && actor.FindStatus(SID.Minimum) == null;
            yield return new(minimum ? square : square, mini, Color: minimum ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
            yield return new(square, toad);
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is 0x01 or 0x02 && state == 0x00080004)
        {
            avoidSquares = false;
            CurrentMechanic = Mechanic.None;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Downpour)
            CurrentMechanic = Mechanic.Downpour;
        else if ((AID)spell.Action.ID == AID.MagitekChakram)
            CurrentMechanic = Mechanic.Chakram;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThermalSuppression && CurrentMechanic != Mechanic.None)
            avoidSquares = true;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentMechanic == Mechanic.Chakram && actor.FindStatus(SID.Minimum) == null && !avoidSquares)
            hints.Add(miniHint);
        else if (CurrentMechanic == Mechanic.Downpour && actor.FindStatus(SID.Toad) == null)
            hints.Add(toadHint);
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentMechanic == Mechanic.Chakram && actor.FindStatus(SID.Minimum) == null && !avoidSquares)
        {
            hints.AddForbiddenZone(ShapeContains.Rect(toad, new Angle(), square.LengthFront, square.LengthBack, square.HalfWidth));
            hints.AddForbiddenZone(ShapeContains.InvertedRect(mini, new Angle(), square.LengthFront, square.LengthBack, square.HalfWidth));
        }
        else if (CurrentMechanic == Mechanic.Downpour && actor.FindStatus(SID.Toad) == null)
        {
            hints.AddForbiddenZone(ShapeContains.Rect(mini, new Angle(), square.LengthFront, square.LengthBack, square.HalfWidth));
            hints.AddForbiddenZone(ShapeContains.InvertedRect(toad, new Angle(), square.LengthFront, square.LengthBack, square.HalfWidth));
        }
        else if (avoidSquares)
        {
            hints.AddForbiddenZone(ShapeContains.Rect(toad, new Angle(), square.LengthFront, square.LengthBack, square.HalfWidth));
            hints.AddForbiddenZone(ShapeContains.Rect(mini, new Angle(), square.LengthFront, square.LengthBack, square.HalfWidth));
        }
    }
}

class ThermalSuppression(BossModule module) : Components.RaidwideCast(module, AID.ThermalSuppression);
class MightyRay(BossModule module) : Components.StandardAOEs(module, AID.MagitekRay, new AOEShapeRect(50, 3));
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCross(40, 4));
class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID.SurfaceMissile, 6);

class D022LugaeStates : StateMachineBuilder
{
    public D022LugaeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThermalSuppression>()
            .ActivateOnEnter<DownpourMagitekChakram>()
            .ActivateOnEnter<MightyRay>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<SurfaceMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus, LTS), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 785, NameID = 10281)]
public class D022Lugae(WorldState ws, Actor primary) : BossModule(ws, primary, new(221, 306), new ArenaBoundsSquare(19.5f));
