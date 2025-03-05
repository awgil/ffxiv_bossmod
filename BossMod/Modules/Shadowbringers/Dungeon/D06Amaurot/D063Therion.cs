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
    DeathlyRayVisualTherion1 = 17107, // Helper->self, 5.0s cast, range 80 width 6 rect
    DeathlyRayVisualTherion2 = 15582, // Boss->self, 3.0s cast, single-target
    DeathlyRayVisualTherion3 = 16785, // Boss->self, no cast, single-target

    DeathlyRayFacesFirst = 15580, // TheFaceOfTheBeast->self, no cast, range 60 width 6 rect
    DeathlyRayFacesRest = 15581, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayTherionFirst = 15583, // Helper->self, no cast, range 60 width 6 rect
    DeathlyRayTherionRest = 15585, // Helper->self, no cast, range 60 width 6 rect
    Misfortune = 15586, // Helper->location, 3.0s cast, range 6 circle
}

class ShadowWreck(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShadowWreck));
class Misfortune(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Misfortune), 6);

class Apokalypsis(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private static readonly AOEShapeRect _rect = new(76, 10);
    private Border? BorderComponent;
    private List<WPos> UnsafePlatforms = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
        {
            yield return new(_rect, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation);
            foreach (var pos in UnsafePlatforms)
                yield return new(new AOEShapeRect(2, 2, 2), pos, default, _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ApokalypsisFirst)
        {
            _activation = Module.CastFinishAt(spell);
            BorderComponent ??= Module.FindComponent<Border>();
            UnsafePlatforms = BorderComponent?.UnsafePlatformPositions.ToList() ?? [];
        }
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
                    UnsafePlatforms.Clear();
                    NumCasts = 0;
                }
                break;
        }
    }
}

class TherionCharge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShape _rect = new AOEShapeRect(35, 20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TherionCharge)
            _aoe = new(_rect, caster.Position.Rounded(), default, Module.CastFinishAt(spell));
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

class DeathlyRayTherion(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeathlyRayVisualTherion1)
            _aoe = new(new AOEShapeRect(60, 3), caster.Position, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DeathlyRayTherionFirst:
            case AID.DeathlyRayTherionRest:
                if (++NumCasts >= 5)
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
    private readonly List<Actor> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters)
            yield return new(_rect, c.Position, c.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DeathlyRayFacesFirst)
        {
            Casters.Add(caster);
        }
        if ((AID)spell.Action.ID is AID.DeathlyRayFacesRest)
        {
            if (++NumCasts >= Casters.Count * 4)
            {
                Casters.Clear();
                NumCasts = 0;
            }
        }
    }
}

class Border : Components.GenericAOEs
{
    private static readonly List<WDir> SidePlatforms = [new(-12, -8), new(12, -8), new(-12, 12), new(12, 12), new(-12, 32), new(12, 32), new(-12, 46), new(12, 46)];
    private static readonly int[] PlatformHalfLen = [48, 30, 20, 10];

    public static readonly WPos OriginalCenter = new(0, -63);

    public int Stage { get; private set; }

    private BitMask MissingPlatforms;
    private BitMask UnsafePlatforms;

    public IEnumerable<WPos> UnsafePlatformPositions => SidePlatforms.Where((_, i) => UnsafePlatforms[i]).Select(p => OriginalCenter + p);

    public Border(BossModule module) : base(module)
    {
        WorldState.Actors.EventObjectAnimation.Subscribe(OnEventObjectAnimation);
    }

    // TODO this really should be in a separate component lol
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Stage == 0 && Module.PrimaryActor.CastInfo is { Action.ID: 15587 } ci)
            yield return new AOEInstance(new AOEShapeRect(36, 20), new(0, -111), default, Module.CastFinishAt(ci));
    }

    public static (WPos Center, ArenaBoundsCustom Bounds) BuildBounds(PolygonClipper clipper, int stage = 0, BitMask missingPlatforms = default)
    {
        var platLength = PlatformHalfLen[stage];

        WDir centerOffset = new(0, 48 - platLength);
        var mainPlatContour = CurveApprox.Rect(new(10, 0), new(0, platLength));
        var mainplat = new PolygonClipper.Operand(mainPlatContour);
        foreach (var (center, i) in SidePlatforms.Select((p, i) => (p, i)))
        {
            if (missingPlatforms[i])
                continue;

            mainplat.AddContour(CurveApprox.Rect(center - centerOffset, new WDir(2, 0), new(0, 2)));
        }

        return (OriginalCenter + centerOffset, new(MathF.Max(14, platLength), clipper.Simplify(mainplat)));
    }

    private void UpdateBounds()
    {
        var b = BuildBounds(Module.Arena.Bounds.Clipper, Stage, MissingPlatforms);
        Module.Arena.Bounds = b.Bounds;
        Module.Arena.Center = b.Center;
    }

    private void Advance(int expectedStage = -1)
    {
        if (expectedStage >= 0 && expectedStage != Stage)
        {
            Module.ReportError(this, $"expected bounds stage {expectedStage}, but got {Stage} - doing nothing");
        }
        else
        {
            Stage++;
            UpdateBounds();
        }
    }

    private int FindPlatform(Actor act) => SidePlatforms.FindIndex(p => (OriginalCenter + p).AlmostEqual(act.Position, 1));

    private void OnEventObjectAnimation(Actor act, ushort p1, ushort p2)
    {
        if (act.OID == 0x1EA1A1)
        {
            switch ((p1, p2))
            {
                case (1, 2):
                    if (Stage == 0)
                        Advance(0);
                    break;
                case (16, 32):
                    var tile = FindPlatform(act);
                    if (tile < 0)
                        Module.ReportError(this, $"unmatched tile for {act} @ {act.Position}");
                    else
                        UnsafePlatforms.Set(tile);
                    break;
                case (4, 8):
                    var tile2 = FindPlatform(act);
                    if (tile2 < 0)
                        Module.ReportError(this, $"unmatched tile for {act} @ {act.Position}");
                    else
                    {
                        UnsafePlatforms.Clear(tile2);
                        MissingPlatforms.Set(tile2);
                        UpdateBounds();
                    }
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TherionCharge)
        {
            switch (Stage)
            {
                case 1:
                    Advance(1);
                    break;
                case 2:
                    Advance(2);
                    break;
                default:
                    Module.ReportError(this, "unexpected third charge from Therion, doing nothing");
                    break;
            }
        }
    }
}

class D063TherionStates : StateMachineBuilder
{
    public D063TherionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Misfortune>()
            .ActivateOnEnter<ShadowWreck>()
            .ActivateOnEnter<Apokalypsis>()
            .ActivateOnEnter<TherionCharge>()
            .ActivateOnEnter<DeathlyRayFaces>()
            .ActivateOnEnter<DeathlyRayTherion>()
            .ActivateOnEnter<Border>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 652, NameID = 8210)]
public class D063Therion(WorldState ws, Actor primary) : BossModule(ws, primary, Border.OriginalCenter, Border.BuildBounds(new()).Bounds);
