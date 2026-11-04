namespace BossMod.Endwalker.Variant.V01Sildihn.V013GladiatorOfSildih;

public enum OID : uint
{
    Boss = 0x399F, // R6.500, x1
    Helper = 0x233C, // R0.500, x18, Helper type
    AntiqueBoulder = 0x39A3, // R1.800, x0 (spawn during fight)
    Regret = 0x39A1, // R1.000, x0 (spawn during fight)
    HatefulVisage = 0x39A0, // R2.250, x0 (spawn during fight)
    WhirlwindUpdraft = 0x39A2, // R2.000, x0 (spawn during fight)
    WhirlwindSmall = 0x3AEC, // R1.330, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    FlashOfSteel1 = 30294, // Boss->self, 5.0s cast, range 60 circle
    Unk1 = 30265, // Boss->location, no cast, single-target
    Unk2 = 30655, // Helper->self, 10.0s cast, range 18 circle
    SculptorsPassion = 30282, // Boss->self, 5.0s cast, range 60 width 8 rect
    RingOfMightInnerSmall = 30271, // Boss->self, 10.0s cast, range 8 circle
    RingOfMightInnerMedium = 30272, // Boss->self, 10.0s cast, range 13 circle
    RingOfMightInnerLarge = 30273, // Boss->self, 10.0s cast, range 18 circle
    RingOfMightOuterSmall = 30274, // Helper->self, 12.0s cast, range 8-50 donut
    RingOfMightOuterMedium = 30275, // Helper->self, 12.0s cast, range 13-50 donut
    RingOfMightOuterLarge = 30276, // Helper->self, 12.0s cast, range 18-50 donut
    FlashOfSteel2 = 30287, // Boss->self, 5.0s cast, range 60 circle
    Landing = 30288, // AntiqueBoulder->self, 7.0s cast, range 50 circle
    ShatteringSteel = 30283, // Boss->self, 12.0s cast, range 60 circle
    MightySmite = 30295, // Boss->player, 5.0s cast, single-target
    RushOfMightShort = 30266, // Boss->location, 10.0s cast, range 25 width 3 rect
    RushOfMightMedium = 30267, // Boss->location, 10.0s cast, range 25 width 3 rect
    RushOfMightLong = 30268, // Boss->location, 10.0s cast, range 25 width 3 rect
    RushOfMightForward = 30269, // Helper->self, 10.5s cast, range 60 180-degree cone
    RushOfMightBackward = 30270, // Helper->self, 12.5s cast, range 60 180-degree cone
    WrathOfRuin = 30277, // Boss->self, 3.0s cast, single-target
    RackAndRuin = 30278, // Regret->location, 4.0s cast, range 40 width 5 rect
    SunderedRemainsCast = 30280, // Boss->self, 3.0s cast, single-target
    SunderedRemains = 30281, // Helper->self, 9.0s cast, range 10 circle
    BitingWindUpdraft = 30285, // Helper->self, 4.0s cast, range 6 circle
    BitingWindKnockup = 30286, // Helper->player, no cast, single-target
    BitingWindAOE = 31222, // Helper->self, 4.0s cast, range 4 circle
    BitingWind = 31223, // Helper->player, no cast, single-target, knockback 5 if walked into a small whirlwind
    HatefulVisageCast = 30289, // Boss->self, 3.0s cast, single-target
    GoldenFlame = 30290, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect
    SilverFlameFirstCW = 30291, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect
    SilverFlameFirstCCW = 30292, // HatefulVisage->self, 8.0s cast, range 60 width 10 rect
    SilverFlameRest = 30293, // HatefulVisage->self, no cast, range 60 width 10 rect
}

class ShatteringSteel(BossModule module) : Components.CastLineOfSightAOE(module, AID.ShatteringSteel, 60, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(0x39A3).Where(a => a.ModelState.AnimState2 == 0);
}
class SculptorsPassion(BossModule module) : Components.StandardAOEs(module, AID.SculptorsPassion, new AOEShapeRect(60, 4));
class FlashOfSteel1(BossModule module) : Components.RaidwideCast(module, AID.FlashOfSteel1);
class FlashOfSteel2(BossModule module) : Components.RaidwideCast(module, AID.FlashOfSteel2);
class Landing(BossModule module) : Components.StandardAOEs(module, AID.Landing, 16);
class MightySmite(BossModule module) : Components.SingleTargetCast(module, AID.MightySmite);

class RingOfMight(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<(Actor Caster, AOEShape Shape)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Take(1).Select(c => new AOEInstance(c.Shape, c.Caster.Position, default, Module.CastFinishAt(c.Caster.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.RingOfMightInnerSmall => new AOEShapeCircle(8),
            AID.RingOfMightInnerMedium => new AOEShapeCircle(13),
            AID.RingOfMightInnerLarge => new AOEShapeCircle(18),
            AID.RingOfMightOuterSmall => new AOEShapeDonut(8, 60),
            AID.RingOfMightOuterMedium => new AOEShapeDonut(13, 60),
            AID.RingOfMightOuterLarge => new AOEShapeDonut(18, 60),
            _ => null
        };

        if (shape != null)
        {
            Casters.Add((caster, shape));
            Casters.SortBy(c => c.Caster.CastInfo!.NPCTotalTime);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RingOfMightInnerSmall or AID.RingOfMightInnerMedium or AID.RingOfMightInnerLarge or AID.RingOfMightOuterSmall or AID.RingOfMightOuterMedium or AID.RingOfMightOuterLarge)
        {
            NumCasts++;
            Casters.RemoveAll(c => c.Caster == caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count > 1)
            hints.AddForbiddenZone(ShapeDistance.Donut(Casters[0].Caster.Position, ((AOEShapeCircle)Casters[0].Shape).Radius + 2, 60), Module.CastFinishAt(Casters[0].Caster.CastInfo));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RingOfMightInnerSmall or AID.RingOfMightInnerMedium or AID.RingOfMightInnerLarge or AID.RingOfMightOuterSmall or AID.RingOfMightOuterMedium or AID.RingOfMightOuterLarge)
            Casters.RemoveAll(c => c.Caster == caster);
    }
}

class RushOfMight(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Casters.FirstOrDefault() is { } c)
        {
            yield return new AOEInstance(new AOEShapeCone(60, 90.Degrees()), c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));

            // the charge AOE is a regular rectangle but we don't get to see what the target position is until the moment we receive the cast event, so we just predict it here
            if (c.CastInfo!.Action.ID == (uint)AID.RushOfMightForward)
            {
                var bossPos = Module.PrimaryActor.Position;
                var srcPos = c.CastInfo!.LocXZ;
                var len = (srcPos - bossPos).Length();
                yield return new AOEInstance(new AOEShapeRect(len, 1.5f), bossPos, Angle.FromDirection(srcPos - bossPos), Module.CastFinishAt(c.CastInfo, -0.5f));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RushOfMightForward or AID.RushOfMightBackward)
        {
            Casters.Add(caster);
            Casters.SortBy(c => c.CastInfo!.NPCTotalTime);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RushOfMightForward or AID.RushOfMightBackward)
        {
            NumCasts++;
            Casters.Remove(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RushOfMightForward or AID.RushOfMightBackward)
            Casters.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Casters.Count > 1)
        {
            var c0 = Casters[0].CastInfo!;
            hints.AddForbiddenZone(ShapeDistance.Cone(c0.LocXZ - c0.Rotation.ToDirection() * 2, 60, c0.Rotation + 180.Degrees(), 90.Degrees()), Module.CastFinishAt(c0));
        }
    }
}

class RackAndRuin(BossModule module) : Components.StandardAOEs(module, AID.RackAndRuin, new AOEShapeRect(40, 2.5f), maxCasts: 8);

class SunderedRemains(BossModule module) : Components.StandardAOEs(module, AID.SunderedRemains, 10, 6);

class GoldenFlame(BossModule module) : Components.StandardAOEs(module, AID.GoldenFlame, new AOEShapeRect(60, 5));
class BitingWindUpdraft(BossModule module) : Components.StandardAOEs(module, AID.BitingWindUpdraft, new AOEShapeCircle(6));
class BitingWindSmall(BossModule module) : Components.VoidzoneAtCastTarget(module, 4, AID.BitingWindAOE, OID.WhirlwindSmall, 0.3f);

class BitingWindUpdraftVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(6);
    private WPos? _origin;
    private DateTime _activation = DateTime.MaxValue;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_origin is not WPos origin)
            yield break;
        if (WorldState.CurrentTime > _activation)
            yield return new(_circle, origin, default, _activation, ArenaColor.SafeFromAOE, Inverted: true);
        else
            yield return new(_circle, origin);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_origin != null && _activation != DateTime.MaxValue)
            hints.Add(WorldState.CurrentTime < _activation ? "Prepare to walk into updraft!" : "Walk into updraft!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_origin is not WPos origin)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
        if (_activation != DateTime.MaxValue && WorldState.CurrentTime < _activation)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(origin, 12), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_origin != null && (AID)spell.Action.ID == AID.ShatteringSteel)
            _activation = Module.CastFinishAt(spell, -5);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BitingWindUpdraft)
            _origin = spell.LocXZ;
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.WhirlwindUpdraft)
        {
            _origin = null;
            _activation = DateTime.MaxValue;
        }
    }
}

class SilverFlame(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (aid is AID.SilverFlameFirstCW or AID.SilverFlameFirstCCW)
            Sequences.Add(new(new AOEShapeRect(60, 5), caster.Position, spell.Rotation, aid == AID.SilverFlameFirstCW ? -10.Degrees() : 10.Degrees(), Module.CastFinishAt(spell), 2, 5, 5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SilverFlameFirstCW or AID.SilverFlameFirstCCW or AID.SilverFlameRest)
            AdvanceSequence(caster.Position, spell.Rotation, WorldState.CurrentTime);
    }
}

class V013GladiatorOfSildihStates : StateMachineBuilder
{
    public V013GladiatorOfSildihStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlashOfSteel1>()
            .ActivateOnEnter<FlashOfSteel2>()
            .ActivateOnEnter<MightySmite>()
            .ActivateOnEnter<SculptorsPassion>()
            .ActivateOnEnter<RingOfMight>()
            .ActivateOnEnter<RushOfMight>()
            .ActivateOnEnter<ShatteringSteel>()
            .ActivateOnEnter<Landing>()
            .ActivateOnEnter<RackAndRuin>()
            .ActivateOnEnter<SunderedRemains>()
            .ActivateOnEnter<GoldenFlame>()
            .ActivateOnEnter<SilverFlame>()
            .ActivateOnEnter<BitingWindSmall>()
            .ActivateOnEnter<BitingWindUpdraft>()
            .ActivateOnEnter<BitingWindUpdraftVoidzone>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "xan, Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 868, NameID = 11387, SortOrder = 4)]
public class V013GladiatorOfSildih(ModuleInit init) : BossModule(init, new(-35, -271), new ArenaBoundsSquare(19.5f));

