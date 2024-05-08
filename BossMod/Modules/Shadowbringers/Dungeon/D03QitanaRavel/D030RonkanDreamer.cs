namespace BossMod.Shadowbringers.Dungeon.D03QitanaRavel.D030RonkanDreamer;

public enum OID : uint
{
    Boss = 0x2A40, //R=1.8
    Helper = 0x2E8, //R=0.5
    //trash that can be pulled into miniboss room
    RonkanVessel = 0x28DD, //R=3.0
    RonkanIdol = 0x28DC, //R=2.04
    RonkanThorn = 0x28E3, //R=2.4
}

public enum AID : uint
{
    WrathOfTheRonka = 17223, // 2A40->self, 6.0s cast, single-target
    WrathOfTheRonkaLong = 15918, // 28E8->self, no cast, range 35 width 8 rect
    WrathOfTheRonkaShort = 15916, // 28E8->self, no cast, range 12 width 8 rect
    WrathOfTheRonkaMedium = 15917, // 28E8->self, no cast, range 22 width 8 rect
    RonkanFire = 17433, // 2A40->player, 1.0s cast, single-target
    RonkanAbyss = 17387, // 2A40->location, 3.0s cast, range 6 circle
    AutoAttack = 872, // 28DD/28DC->player, no cast, single-target
    AutoAttack2 = 17949, // 28E3->player, no cast, single-target
    BurningBeam = 15923, // 28E3->self, 3.0s cast, range 15 width 4 rect
}

public enum TetherID : uint
{
    StatueActivate = 37, // 28E8->Boss
}

class RonkanFire(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.RonkanFire));
class RonkanAbyss(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.RonkanAbyss), 6);

class WrathOfTheRonka(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];
    private static readonly AOEShapeRect RectShort = new(12, 4);
    private static readonly AOEShapeRect RectMedium = new(22, 4);
    private static readonly AOEShapeRect RectLong = new(35, 4);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
        {
            if (Module.PrimaryActor.Position.AlmostEqual(new(0, 634), 1))
            {
                if (c.Position.AlmostEqual(new(-17, 657), 1) || c.Position.AlmostEqual(new(-17, 650), 1) || c.Position.AlmostEqual(new(-17, 635), 1) || c.Position.AlmostEqual(new(-17, 620), 1) || c.Position.AlmostEqual(new(17, 657), 1) || c.Position.AlmostEqual(new(17, 650), 1) || c.Position.AlmostEqual(new(17, 635), 1) || c.Position.AlmostEqual(new(17, 620), 1))
                    yield return new(RectLong, c.Position, c.Rotation, _activation);
                if (c.Position.AlmostEqual(new(-17, 627), 1) || c.Position.AlmostEqual(new(17, 642), 1))
                    yield return new(RectMedium, c.Position, c.Rotation, _activation);
                if (c.Position.AlmostEqual(new(-17, 642), 1) || c.Position.AlmostEqual(new(17, 627), 1))
                    yield return new(RectShort, c.Position, c.Rotation, _activation);
            }
            if (Module.PrimaryActor.Position.AlmostEqual(new(0, 428), 1))
            {
                if (c.Position.AlmostEqual(new(17, 451), 1) || c.Position.AlmostEqual(new(17, 444), 1) || c.Position.AlmostEqual(new(17, 414), 1) || c.Position.AlmostEqual(new(17, 429), 1) || c.Position.AlmostEqual(new(-17, 451), 1) || c.Position.AlmostEqual(new(-17, 444), 1) || c.Position.AlmostEqual(new(-17, 414), 1) || c.Position.AlmostEqual(new(-17, 429), 1))
                    yield return new(RectLong, c.Position, c.Rotation, _activation);
                if (c.Position.AlmostEqual(new(-17, 436), 1) || c.Position.AlmostEqual(new(17, 421), 1))
                    yield return new(RectMedium, c.Position, c.Rotation, _activation);
                if (c.Position.AlmostEqual(new(17, 436), 1) || c.Position.AlmostEqual(new(-17, 421), 1))
                    yield return new(RectShort, c.Position, c.Rotation, _activation);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.StatueActivate)
        {
            _casters.Add(source);
            _activation = WorldState.FutureTime(6);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WrathOfTheRonkaShort or AID.WrathOfTheRonkaMedium or AID.WrathOfTheRonkaLong)
        {
            ++NumCasts;
            if (NumCasts == 6)
            {
                NumCasts = 0;
                _casters.Clear();
            }
        }
    }
}

public class Layout(BossModule module) : BossComponent(module)
{
    private static IEnumerable<WPos> Wall1()
    {
        yield return new WPos(-4, 646.4f);
        yield return new WPos(-6, 646.4f);
        yield return new WPos(-6, 639);
        yield return new WPos(-4, 639);
    }

    private static IEnumerable<WPos> Wall2()
    {
        yield return new WPos(4, 631.4f);
        yield return new WPos(6, 631.4f);
        yield return new WPos(6, 624);
        yield return new WPos(4, 624);
    }

    private static IEnumerable<WPos> Wall3()
    {
        yield return new WPos(4, 440);
        yield return new WPos(6, 440);
        yield return new WPos(6, 433);
        yield return new WPos(4, 433);
    }

    private static IEnumerable<WPos> Wall4()
    {
        yield return new WPos(-4, 425);
        yield return new WPos(-6, 425);
        yield return new WPos(-6, 418);
        yield return new WPos(-4, 418);
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Module.PrimaryActor.Position.AlmostEqual(new(0, 634), 1))
        {
            Arena.AddPolygon(Wall1(), ArenaColor.Border, 2);
            Arena.AddPolygon(Wall2(), ArenaColor.Border, 2);
        }
        if (Module.PrimaryActor.Position.AlmostEqual(new(0, 428), 1))
        {
            Arena.AddPolygon(Wall3(), ArenaColor.Border, 2);
            Arena.AddPolygon(Wall4(), ArenaColor.Border, 2);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    { //Note: this isn't looking natural because the AI is trying to dodge the lasers and the wall at the same time, consider not activating the AI in partyfinder until the AI is improved, for multiboxing it should do ok
        base.AddAIHints(slot, actor, assignment, hints);
        if (Module.PrimaryActor.Position.AlmostEqual(new(0, 634), 1))
        {
            hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Wall1(), true));
            hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Wall2(), false));
        }
        if (Module.PrimaryActor.Position.AlmostEqual(new(0, 428), 1))
        {
            hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Wall3(), false));
            hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Wall4(), true));
        }
    }
}

class BurningBeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningBeam), new AOEShapeRect(15, 2));

class D030RonkanDreamerStates : StateMachineBuilder
{
    public D030RonkanDreamerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Layout>()
            .ActivateOnEnter<RonkanFire>()
            .ActivateOnEnter<RonkanAbyss>()
            .ActivateOnEnter<WrathOfTheRonka>()
            .ActivateOnEnter<BurningBeam>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8639)]
public class D030RonkanDreamer(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, primary.Position.Z > 550 ? 640 : 434), new ArenaBoundsRect(17.5f, 24))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.RonkanVessel))
            Arena.Actor(s, ArenaColor.Object);
        foreach (var e in Enemies(OID.RonkanThorn))
            Arena.Actor(e, ArenaColor.Object);
        foreach (var e in Enemies(OID.RonkanIdol))
            Arena.Actor(e, ArenaColor.Object);
    }
}
