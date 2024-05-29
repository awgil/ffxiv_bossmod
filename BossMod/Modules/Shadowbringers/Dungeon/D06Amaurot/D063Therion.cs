namespace BossMod.Shadowbringers.Dungeon.D06Amaurot.D063Therion;

public enum OID : uint
{
    Boss = 0x27C1, // R=25.84
    TheFaceOfTheBeast = 0x27C3, // R=2.1
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 15574, // Boss->player, no cast, single-target
    ShadowWreck = 15587, // Boss->self, 4.0s cast, range 100 circle
    ApokalypsisFirst = 15575, // Boss->self, 6.0s cast, range 76 width 20 rect
    ApokalypsisRest = 15577, // Helper->self, no cast, range 76 width 20 rect
    TherionCharge = 15578, // Boss->location, 7.0s cast, range 100 circle, damage fall off AOE

    DeathlyRayVisualFaces1 = 15579, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualFaces2 = 16786, // Boss->self, no cast, single-target
    DeathlyRayVisualThereion1 = 17107, // Helper->self, 5.0s cast, range 80 width 6 rect
    DeathlyRayVisualThereion2 = 15582, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualThereion3 = 16785, // Boss->self, no cast, single-target

    DeathlyRayFacesFirst = 15580, // TheFaceOfTheBeast->self, no cast, range 60 width 6 rect
    DeathlyRayFacesRest = 15581, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayThereionFirst = 15583, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayThereionRest = 15585, // Helper->self, no cast, range 60 width 6 rect
    Misfortune = 15586, // Helper->location, 3.0s cast, range 6 circle
}

class ShadowWreck(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowWreck));
class Misfortune(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Misfortune), 6);

class Border(BossModule module) : Components.GenericAOEs(module, warningText: "Platform will be removed during next Apokalypsis!")
{
    private ArenaBounds? arena;
    private const int SquareHalfWidth = 2;
    private const float RectangleHalfWidth = 10.1f;
    private const int MaxError = 5;
    private const float PathfindingOffset = -0.5f;
    private static readonly AOEShapeRect _square = new(2, 2, 2);

    public readonly List<AOEInstance> BreakingPlatforms = [];

    public static readonly List<WPos> positions = [new(-12, -71), new(12, -71), new(-12, -51),
    new(12, -51), new(-12, -31), new(12, -31), new(-12, -17), new(12, -17), new(0, -65), new(0, -45)];

    private static readonly List<Shape> shapes = [new Square(positions[0], SquareHalfWidth),
    new Square(positions[1], SquareHalfWidth), new Square(positions[2], SquareHalfWidth),
    new Square(positions[3], SquareHalfWidth), new Square(positions[4], SquareHalfWidth),
    new Square(positions[5], SquareHalfWidth), new Square(positions[6], SquareHalfWidth),
    new Square(positions[7], SquareHalfWidth), new Square(positions[8], RectangleHalfWidth),
    new Square(positions[9], RectangleHalfWidth)];

    private static readonly List<Shape> rect = [new Rectangle(new WPos(0, -45), 10, 30)];
    public readonly List<Shape> unionRefresh = new(rect.Concat(shapes.Take(8)));
    private readonly List<Shape> difference = [];
    public static readonly ArenaBounds arenaDefault = new ArenaBoundsComplex(rect.Concat(shapes.Take(8)), Offset: PathfindingOffset);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in BreakingPlatforms)
            yield return new(_square, p.Origin, Color: ArenaColor.FutureVulnerable, Risky: Module.FindComponent<Apokalypsis>()!.NumCasts == 0);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
        {
            for (var i = 0; i < 8; i++)
            {
                if (actor.Position.AlmostEqual(positions[i], MaxError))
                {
                    if (unionRefresh.Remove(shapes[i]))
                    {
                        if (unionRefresh.Count == 7)
                            difference.Add(shapes[8]);
                        if (unionRefresh.Count == 5)
                            difference.Add(shapes[9]);
                        arena = new ArenaBoundsComplex(unionRefresh, difference, Offset: PathfindingOffset);
                        Module.Arena.Bounds = arena;
                        Module.Arena.Center = arena.Center;
                    }
                    BreakingPlatforms.Remove(new(_square, positions[i], Color: ArenaColor.FutureVulnerable));
                }
            }
        }
        if (state == 0x00100020)
        {
            for (var i = 0; i < 8; i++)
            {
                if (actor.Position.AlmostEqual(positions[i], MaxError))
                    BreakingPlatforms.Add(new(_square, positions[i], Color: ArenaColor.FutureVulnerable));
            }
        }
    }
}

class Apokalypsis(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private static readonly AOEShapeRect _rect = new(76, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(_rect, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation, Risky: (Module.FindComponent<Border>()!.unionRefresh.Count - Module.FindComponent<Border>()!.BreakingPlatforms.Count) > 1);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ApokalypsisFirst)
            _activation = spell.NPCFinishAt;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ApokalypsisFirst:
            case AID.ApokalypsisRest:
                if (++NumCasts == 5)
                {
                    _activation = default;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class ThereionCharge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(10, 20, 100);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TherionCharge)
            _aoe = new(_rect, NumCasts == 0 ? Border.positions[8] : Border.positions[9], default, spell.NPCFinishAt);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TherionCharge)
        {
            ++NumCasts;
            _aoe = null;
        }
    }
}

class DeathlyRayThereion(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeathlyRayVisualThereion1)
            _aoe = new(new AOEShapeRect(60, 3), caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DeathlyRayThereionFirst:
            case AID.DeathlyRayThereionRest:
                if (++NumCasts == 5)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }
}

class DeathlyRayFaces(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(60, 3);
    private readonly List<AOEInstance> _aoesFirst = [];
    private readonly List<AOEInstance> _aoesRest = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var a in _aoesFirst)
            yield return new(a.Shape, a.Origin, a.Rotation, default, ArenaColor.Danger);
        foreach (var a in _aoesRest)
            yield return new(a.Shape, a.Origin, a.Rotation, a.Activation, _aoesFirst.Count > 0 ? ArenaColor.AOE : ArenaColor.Danger, _aoesFirst.Count == 0);

    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DeathlyRayFacesFirst && _aoesFirst.Count == 0 && _aoesRest.Count == 0)
        {
            foreach (var c in Module.Enemies(OID.TheFaceOfTheBeast).Where(x => x.Rotation.AlmostEqual(caster.Rotation, Helpers.RadianConversion)))
                _aoesFirst.Add(new(_rect, c.Position, c.Rotation));
            foreach (var c in Module.Enemies(OID.TheFaceOfTheBeast).Where(x => !x.Rotation.AlmostEqual(caster.Rotation, Helpers.RadianConversion)))
                _aoesRest.Add(new(_rect, c.Position, c.Rotation, Module.WorldState.FutureTime(8.5f)));
        }
        if ((AID)spell.Action.ID is AID.DeathlyRayFacesFirst or AID.DeathlyRayFacesRest)
        {
            ++NumCasts;
            if (NumCasts == 5 * _aoesFirst.Count)
            {
                _aoesFirst.Clear();
                NumCasts = 0;
            }
            if (_aoesFirst.Count == 0 && NumCasts == 5 * _aoesRest.Count)
            {
                _aoesRest.Clear();
                NumCasts = 0;
            }
        }
    }
}

class MeleeRange(BossModule module) : BossComponent(module) // force melee range for melee rotation solver users
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Service.Config.Get<AutorotationConfig>().Enabled)
        {
            if (!Module.InBounds(actor.Position)) // return into module bounds if accidently walked into fire to prevent death by doom
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center, 3));
            else if (!Module.FindComponent<DeathlyRayFaces>()!.ActiveAOEs(slot, actor).Any() && !Module.FindComponent<Apokalypsis>()!.ActiveAOEs(slot, actor).Any() &&
            !Module.FindComponent<ThereionCharge>()!.ActiveAOEs(slot, actor).Any())
                if (actor.Role is Role.Melee or Role.Tank)
                    hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.PrimaryActor.Position, Module.PrimaryActor.HitboxRadius + 3));
        }
    }
}

class D063TherionStates : StateMachineBuilder
{
    public D063TherionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MeleeRange>()
            .ActivateOnEnter<ThereionCharge>()
            .ActivateOnEnter<Misfortune>()
            .ActivateOnEnter<ShadowWreck>()
            .ActivateOnEnter<Apokalypsis>()
            .ActivateOnEnter<DeathlyRayFaces>()
            .ActivateOnEnter<DeathlyRayThereion>()
            .ActivateOnEnter<Border>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 341, NameID = 8210)]
public class D063Therion(WorldState ws, Actor primary) : BossModule(ws, primary, Border.arenaDefault.Center, Border.arenaDefault);
