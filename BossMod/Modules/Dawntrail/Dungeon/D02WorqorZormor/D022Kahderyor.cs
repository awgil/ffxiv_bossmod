
namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D022Kahderyor;

public enum OID : uint
{
    Boss = 0x415D, // R7.000, x1
    Helper = 0x233C, // R0.500, x20, 523 type
    CrystallineDebris = 0x415E, // R1.400, x0 (spawn during fight)
}

public enum AID : uint
{
    WindUnbound = 36282, // Boss->self, 5.0s cast, range 60 circle
    CrystallineCrush = 36153, // 233C->self, 6.3s cast, range 6 circle
    WindShot = 36284, // Boss->self, 5.5s cast, single-target
    WindShotHelper = 36296, // 233C->player, 6.0s cast, range ?-10 donut
    EarthenShot = 36283, // Boss->self, 5.0+0.5s cast, single-target
    EarthenShotHelper = 36295, // 233C->player, 6.0s cast, range 6 circle
    CrystallineStorm = 36290, // 233C->self, 4.0s cast, range 50 width 2 rect
    SeedCrystals = 36298, // 233C->player, 5.0s cast, range 6 circle
    CyclonicRing = 36294, // 233C->self, 5.0s cast, range ?-40 donut
    EyeOfTheFierce = 36297, // 233C->self, 5.0s cast, range 60 circle
    StalagmiteCircle = 36293, // 233C->self, 5.0s cast, range 15 circle
}

class CrystalInout(BossModule module) : Components.GenericAOEs(module)
{
    private enum Active
    {
        None = 0,
        In = 1,
        Out = 2
    }

    private record struct Inout(AOEShape InShape, AOEShape OutShape, WPos Center, Angle Rotation);

    private DateTime _finishAt;
    private readonly List<Inout> _aoes = [];
    private Active _active = Active.None;
    private byte _castsWhileActive;

    private void Reset()
    {
        _castsWhileActive = 0;
        _aoes.Clear();
        _finishAt = default;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active == Active.None)
            yield break;

        foreach (var x in _aoes)
        {
            if (_active == Active.In)
                yield return new AOEInstance(x.InShape, x.Center, x.Rotation, _finishAt, ArenaColor.SafeFromAOE, Risky: false);
            else
                yield return new AOEInstance(x.OutShape, x.Center, x.Rotation, _finishAt);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_active == Active.In && ActiveAOEs(slot, actor).All(c => !c.Check(actor.Position)))
            hints.Add("Get to safe zone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var shapes = aoes.Select(s => s.Shape.Distance(s.Origin, s.Rotation)).ToList();
        if (shapes.Count == 0)
            return;

        float distance(WPos p)
        {
            var dist = shapes.Select(s => s(p)).Min();
            return _active == Active.Out ? dist : -dist;
        }
        hints.AddForbiddenZone(distance, _finishAt);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EarthenShotHelper)
        {
            _active = Active.Out;
            _finishAt = Module.CastFinishAt(spell);
        }
        if ((AID)spell.Action.ID == AID.WindShotHelper)
        {
            _active = Active.In;
            _finishAt = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EarthenShot or AID.WindShot)
            _castsWhileActive++;

        if ((AID)spell.Action.ID is AID.EarthenShotHelper or AID.WindShotHelper)
        {
            _active = Active.None;
            if (_castsWhileActive >= 2)
                Reset();
        }

        if ((AID)spell.Action.ID == AID.CrystallineCrush)
            _aoes.Add(new Inout(
                new AOEShapeCircle(8),
                new AOEShapeCircle(15),
                caster.Position,
                spell.Rotation
            ));

        if ((AID)spell.Action.ID == AID.CrystallineStorm)
            _aoes.Add(new Inout(
                new AOEShapeRect(25, 1, 25),
                new AOEShapeRect(25, 7, 25),
                caster.Position,
                spell.Rotation
            ));
    }
}

class WindUnbound(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WindUnbound));
class WindShot(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.WindShotHelper), new AOEShapeDonut(5, 10), true);
class EarthenShot(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EarthenShotHelper), 6, true);
class CrystallineCrush(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.CrystallineCrush), 6, maxSoakers: 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
            hints.AddForbiddenZone(new AOEShapeDonut(6, 40), Towers[0].Position);
    }
}

class CrystallineStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrystallineStorm), new AOEShapeRect(25, 1, 25));
class CrystallineDebris(BossModule module) : Components.Adds(module, (uint)OID.CrystallineDebris)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in Actors.Where(x => !x.IsDead))
            Arena.AddCircle(c.Position, 1.4f, ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Actors.Any(x => !x.IsDead))
            hints.Add("Break debris!");
    }
}
class SeedCrystals(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SeedCrystals), 6);
class CyclonicRing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CyclonicRing), new AOEShapeDonut(7.5f, 40));
class StalagmiteCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StalagmiteCircle), new AOEShapeCircle(15));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFierce));

class D022KahderyorStates : StateMachineBuilder
{
    public D022KahderyorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CrystallineCrush>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<WindShot>()
            .ActivateOnEnter<EarthenShot>()
            .ActivateOnEnter<CrystallineStorm>()
            .ActivateOnEnter<CrystallineDebris>()
            .ActivateOnEnter<CrystalInout>()
            .ActivateOnEnter<SeedCrystals>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<StalagmiteCircle>()
            .ActivateOnEnter<EyeOfTheFierce>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12703)]
public class D022Kahderyor(WorldState ws, Actor primary) : BossModule(ws, primary, new(-53, -57), new ArenaBoundsCircle(20));
