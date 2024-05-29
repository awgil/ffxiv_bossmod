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
    private const int RectangleHalfWidth = 10;
    private const int MaxError = 5;
    private static readonly AOEShapeRect _square = new(2, 2, 2);

    private readonly List<AOEInstance> _aoes = [];

    private static readonly List<(WPos Position, Shape Shape)> shapes =
    [
        (new(-12, -71), new Square(new(-12, -71), SquareHalfWidth)),
        (new(12, -71), new Square(new(12, -71), SquareHalfWidth)),
        (new(-12, -51), new Square(new(-12, -51), SquareHalfWidth)),
        (new(12, -51), new Square(new(12, -51), SquareHalfWidth)),
        (new(-12, -31), new Square(new(-12, -31), SquareHalfWidth)),
        (new(12, -31), new Square(new(12, -31), SquareHalfWidth)),
        (new(-12, -17), new Square(new(-12, -17), SquareHalfWidth)),
        (new(12, -17), new Square(new(12, -17), SquareHalfWidth)),
        (new(0, -65), new Square(new(0, -65), RectangleHalfWidth)),
        (new(0, -55), new Square(new(0, -45), RectangleHalfWidth))
    ];
    private static readonly List<Shape> rect = [new Rectangle(new WPos(0, -45), 10, 30)];
    private readonly List<Shape> unionRefresh = new(rect.Concat(shapes.Take(8).Select(s => s.Shape)));
    private readonly List<Shape> difference = [];
    public static readonly ArenaBounds arenaDefault = new ArenaBoundsComplex(rect.Concat(shapes.Take(8).Select(s => s.Shape)));

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
        {
            for (var i = 0; i < 8; i++)
            {
                if (actor.Position.AlmostEqual(shapes[i].Position, MaxError))
                {
                    if (unionRefresh.Remove(shapes[i].Shape))
                    {
                        if (unionRefresh.Count == 7)
                            difference.Add(shapes[8].Shape);
                        if (unionRefresh.Count == 5)
                            difference.Add(shapes[9].Shape);
                        arena = new ArenaBoundsComplex(unionRefresh, difference);
                        Module.Arena.Bounds = arena;
                        Module.Arena.Center = arena.Center;
                    }
                }
            }
        }
        if (state == 0x00100020)
        {
            for (var i = 0; i < 8; i++)
            {
                if (actor.Position.AlmostEqual(shapes[i].Position, MaxError))
                    _aoes.Add(new(_square, shapes[i].Position, Color: ArenaColor.FutureVulnerable));
            }
        }
    }
}

class Apokalypsis(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ApokalypsisFirst)
            _aoe = new(new AOEShapeRect(76, 10), caster.Position, spell.Rotation, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ApokalypsisFirst:
            case AID.ApokalypsisRest:
                if (++NumCasts == 5)
                {
                    _aoe = null;
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
            _aoe = new(_rect, NumCasts == 0 ? new(0, -65) : new(0, -45), default, spell.NPCFinishAt);
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
