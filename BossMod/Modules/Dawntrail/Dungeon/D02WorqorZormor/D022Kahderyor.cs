namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D022Kahderyor;

public enum OID : uint
{
    Boss = 0x415D, // R7.0
    CrystallineDebris = 0x415E, // R1.4
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WindUnbound = 36282, // Boss->self, 5.0s cast, range 60 circle

    CrystallineCrushVisual = 36285, // Boss->location, 5.0+1.0s cast, single-target
    CrystallineCrush = 36153, // Helper->self, 6.3s cast, range 6 circle, tower

    WindShotVisual1 = 36284, // Boss->self, 5.5s cast, single-target
    WindshotVisual2 = 36300, // Helper->player, no cast, single-target
    WindShot = 36296, // Helper->players, 6.0s cast, range 5-10 donut, stack

    EarthenShotVisual1 = 36283, // Boss->self, 5.0+0.5s cast, single-target
    EarthenShotVisual2 = 36299, // Helper->player, no cast, single-target
    EarthenShot = 36295, // Helper->player, 6.0s cast, range 6 circle, spread

    CrystallineStormVisual = 36286, // Boss->self, 3.0+1.0s cast, single-target
    CrystallineStorm = 36290, // Helper->self, 4.0s cast, range 50 width 2 rect

    SeedCrystalsVisual = 36291, // Boss->self, 4.5+0.5s cast, single-target
    SeedCrystals = 36298, // Helper->player, 5.0s cast, range 6 circle, spread

    SharpenedSights = 36287, // Boss->self, 3.0s cast, single-target
    EyeOfTheFierce = 36297, // Helper->self, 5.0s cast, range 60 circle

    StalagmiteCircleVisual = 36288, // Boss->self, 5.0s cast, single-target
    StalagmiteCircle = 36293, // Helper->self, 5.0s cast, range 15 circle

    CyclonicRingVisual = 36289, // Boss->self, 5.0s cast, single-target
    CyclonicRing = 36294 // Helper->self, 5.0s cast, range 8-40 donut
}

public enum IconID : uint
{
    Stackmarker = 62, // Helper
    WindShot = 511, // player
    EarthenShot = 169, // player
    SeedCrystals = 311 // player
}

class WindEarthShot(BossModule module) : Components.GenericAOEs(module)
{
    private const string risk2Hint = "Walk into a crystal line!";
    private const string stayHint = "Stay inside crystal line!";
    private static readonly AOEShapeDonut donut = new(8, 50);
    private static readonly AOEShapeCircle circle = new(15);
    private static readonly AOEShapeCustom ENVC21Inverted = new([new Rectangle(new(-63, -57), 1, 50, -29.996f.Degrees()),
    new Rectangle(new(-53, -47), 1, 50, -119.997f.Degrees()),
    new Rectangle(new(-53, -67), 1, 50, 80.001f.Degrees())], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom ENVC21 = new([new Rectangle(new(-63, -57), 7, 50, -29.996f.Degrees()),
    new Rectangle(new(-53, -47), 7, 50, -119.997f.Degrees()),
    new Rectangle(new(-53, -67), 7, 50, 80.001f.Degrees())]);
    private static readonly AOEShapeCustom ENVC20Inverted = new([new Rectangle(new(-43, -57), 1, 50, -29.996f.Degrees()),
    new Rectangle(new(-53, -47), 1, 50, -99.996f.Degrees()),
    new Rectangle(new(-53, -67), 1, 50, -119.997f.Degrees())], InvertForbiddenZone: true);
    private static readonly AOEShapeCustom ENVC20 = new([new Rectangle(new(-43, -57), 7, 50, -29.996f.Degrees()),
    new Rectangle(new(-53, -47), 7, 50, -99.996f.Degrees()),
    new Rectangle(new(-53, -67), 7, 50, -119.997f.Degrees())]);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        var activation = Module.WorldState.FutureTime(5.9f);
        if (state == 0x00800040)
        {
            if (index == 0x1E)
                _aoe = new(donut, new(-43, -57), default, activation);
            else if (index == 0x1F)
                _aoe = new(donut, new(-63, -57), default, activation);
            else if (index == 0x20)
                _aoe = new(ENVC20Inverted, Module.Center, default, activation, ArenaColor.SafeFromAOE);
            else if (index == 0x21)
                _aoe = new(ENVC21Inverted, Module.Center, default, activation, ArenaColor.SafeFromAOE);
        }
        else if (state == 0x00200010)
        {
            if (index == 0x1E)
                _aoe = new(circle, new(-43, -57), default, activation);
            else if (index == 0x1F)
                _aoe = new(circle, new(-63, -57), default, activation);
            else if (index == 0x20)
                _aoe = new(ENVC20, Module.Center, default, activation);
            else if (index == 0x21)
                _aoe = new(ENVC21, Module.Center, default, activation);
        }
        else if (state is 0x02000001 or 0x04000004 or 0x00800040 or 0x08000004 or 0x01000001)
            _aoe = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveAOEs(slot, actor).Any(c => !(c.Shape == ENVC20Inverted || c.Shape == ENVC21Inverted)))
            base.AddHints(slot, actor, hints);
        else if (ActiveAOEs(slot, actor).Any(c => (c.Shape == ENVC20Inverted || c.Shape == ENVC21Inverted) && !c.Check(actor.Position)))
            hints.Add(risk2Hint);
        else if (ActiveAOEs(slot, actor).Any(c => (c.Shape == ENVC20Inverted || c.Shape == ENVC21Inverted) && c.Check(actor.Position)))
            hints.Add(stayHint, false);
    }
}

class WindShotStack(BossModule module) : Components.UniformStackSpread(module, 2, 0, 4)
{
    // this is a donut targeted on each player, it is best solved by stacking
    private static readonly AOEShapeDonut donut = new(5, 10);
    private DateTime activation;
    private readonly List<Actor> actors = [];

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.WindShot)
        {
            activation = Module.WorldState.FutureTime(6);
            actors.Add(actor);
        }
    }

    public override void Update()
    {
        var player = Module.Raid.Player();
        Stacks.Clear();
        if (actors.Count > 0 && player != null)
        {
            var closestTarget = Raid.WithoutSlot().Exclude(player).Closest(player.Position);
            if (closestTarget != default)
                AddStack(closestTarget, activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WindShot)
            actors.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count > 0)
        {
            var closestTarget = Raid.WithoutSlot().Exclude(actor).Closest(actor.Position);
            if (closestTarget != null)
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(closestTarget.Position, 2), ActiveStacks.First().Activation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var c in actors)
            donut.Draw(Arena, c.Position, default, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) { }
}

class WindUnbound(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WindUnbound));
class CrystallineCrush(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.CrystallineCrush), 6, 4, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Towers[0].Position, 6), Towers[0].Activation);
            hints.PredictedDamage.Add((Raid.WithSlot().Mask(), Towers[0].Activation));
        }
    }
}

class EarthenShot(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EarthenShot), 6);
class StalagmiteCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StalagmiteCircle), new AOEShapeCircle(15));
class CrystallineStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrystallineStorm), new AOEShapeRect(25, 1, 25));
class CyclonicRing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CyclonicRing), new AOEShapeDonut(8, 40));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFierce));
class SeedCrystals(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SeedCrystals), 6);

class D022KahderyorStates : StateMachineBuilder
{
    public D022KahderyorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindShotStack>()
            .ActivateOnEnter<WindEarthShot>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<CrystallineStorm>()
            .ActivateOnEnter<CrystallineCrush>()
            .ActivateOnEnter<EarthenShot>()
            .ActivateOnEnter<StalagmiteCircle>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<SeedCrystals>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12703)]
public class D022Kahderyor(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultBounds.Center, DefaultBounds)
{
    private static readonly List<Shape> union = [new Circle(new(-53, -57), 19.5f)];
    private static readonly List<Shape> difference = [new Rectangle(new(-72.5f, -57), 20, 0.75f, 90.Degrees()), new Rectangle(new(-53, -37), 20, 1.5f)];
    public static readonly ArenaBoundsComplex DefaultBounds = new(union, difference);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.CrystallineDebris))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
