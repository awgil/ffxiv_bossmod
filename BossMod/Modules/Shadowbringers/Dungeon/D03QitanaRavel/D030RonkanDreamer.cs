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

class BurningBeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BurningBeam), new AOEShapeRect(15, 2));

class D030RonkanDreamerStates : StateMachineBuilder
{
    public D030RonkanDreamerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RonkanFire>()
            .ActivateOnEnter<RonkanAbyss>()
            .ActivateOnEnter<WrathOfTheRonka>()
            .ActivateOnEnter<BurningBeam>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 651, NameID = 8639)]
public class D030RonkanDreamer(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position.Z > 550 ? arena1.Center : arena2.Center, primary.Position.Z > 550 ? arena1 : arena2)
{
    private static readonly List<Shape> union1 = [new Rectangle(new(0, 640), 17.5f, 23)];
    private static readonly List<Shape> difference1 = [new Rectangle(new(-5.2f, 642.7f), 1.15f, 3.5f), new Rectangle(new(5.1f, 627.6f), 1.15f, 3.5f)];
    private static readonly ArenaBounds arena1 = new ArenaBoundsComplex(union1, difference1);
    private static readonly List<Shape> union2 = [new Rectangle(new(0, 434.5f), 17.5f, 24f)];
    private static readonly List<Shape> difference2 = [new Rectangle(new(-5.1f, 421.7f), 1.15f, 3.5f), new Rectangle(new(5.1f, 436.6f), 1.15f, 3.5f)];
    private static readonly ArenaBounds arena2 = new ArenaBoundsComplex(union2, difference2);

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
