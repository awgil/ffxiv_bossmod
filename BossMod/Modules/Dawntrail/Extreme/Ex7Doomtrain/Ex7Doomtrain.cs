using System.ComponentModel;
using System.Runtime.InteropServices;

namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

static class Shapes
{
    public static IEnumerable<WDir> Fence(float x, float z) => CurveApprox.TruncatedRect(new(x, z), new WDir(2.5f, 0), new(0, 2.5f), 0.5f);
    public static IEnumerable<WDir> Crate(float x, float z) => CurveApprox.TruncatedRect(new(x, z), new WDir(2.9f, 0), new(0, 2.9f), 0.5f);
}

class DeadMansExpress(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DeadMansExpress, 30, kind: Kind.DirForward, stopAtWall: true);
class DeadMansWindpipe(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DeadMansWindpipeBoss, 30, kind: Kind.DirForward, stopAtWall: true)
{
    public override IEnumerable<Source> Sources(int slot, Actor actor) => base.Sources(slot, actor).Select(src => src with { Direction = src.Direction + 180.Degrees() });
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => pos.InRect(Arena.Center - new WDir(0, 15), new WDir(0, 10), 10);
}

class PushPullCounter(BossModule module) : Components.CastCounterMulti(module, [AID.DeadMansExpress, AID.DeadMansWindpipe]);

class Plasma(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts { get; private set; }
    string? next;

    public void Reset() { NumCasts = 0; }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (next != null)
            hints.Add($"Next: {next}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DeadMansOverdraughtSpread:
                next = "Spread";
                break;
            case AID.DeadMansOverdraughtStack:
                next = "Stack";
                break;
            case AID.DeadMansExpress:
            case AID.DeadMansWindpipeBoss:
                EnableHints = false;
                Activate();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_Plasma:
                Spreads.Clear();
                NumCasts++;
                next = null;
                break;
            case AID._Ability_HyperexplosivePlasma:
                Stacks.Clear();
                NumCasts++;
                next = null;
                break;
            case AID.DeadMansExpress:
            case AID.DeadMansWindpipe:
                EnableHints = true;
                break;
        }
    }

    void Activate()
    {
        switch (next)
        {
            case "Spread":
                foreach (var player in Raid.WithoutSlot())
                    Spreads.Add(new(player, 5));
                break;
            case "Stack":
                foreach (var player in Raid.WithoutSlot().OrderByDescending(r => r.Class.IsSupport()).Take(4))
                    Stacks.Add(new(player, 5, maxSize: 2));
                break;
        }
    }
}

class DeadMansBlastpipe(BossModule module) : Components.GenericAOEs(module, AID.DeadMansBlastpipe)
{
    private Actor? Caster;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Caster != null)
            yield return new(new AOEShapeRect(10, 10), Arena.Center - new WDir(0, 15), default);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeadMansWindpipe)
            Caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Caster = null;
        }
    }
}

class LevinSignal(BossModule module) : Components.GenericAOEs(module)
{
    // 7 second delay
    private readonly List<(Actor Caster, bool Ground, DateTime Activation, float MaxLen)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Where(c => c.Ground).Select(c => new AOEInstance(new AOEShapeRect(c.MaxLen, 2.5f), c.Caster.Position, default, c.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_LevinSignal)
        {
            var car = Module.FindComponent<CarCounter>();
            var maxLen = 30;

            if (car?.Car == 2)
            {
                if (MathF.Abs(actor.Position.X - 92.5f) < 3)
                    maxLen = 20;

                if (MathF.Abs(actor.Position.X - 107.5f) < 3)
                    maxLen = 10;
            }

            _casters.Add((actor, actor.PosRot.Y < 2, WorldState.FutureTime(7), maxLen));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PlasmaBeamGround or AID.PlasmaBeamOverhead or AID._Ability_PlasmaBeam2 or AID._Ability_PlasmaBeam3)
        {
            _casters.RemoveAll(c => c.Caster == caster);
            NumCasts++;
        }
    }
}

class UnlimitedExpress1(BossModule module) : Components.RaidwideCast(module, AID._Ability_UnlimitedExpress1);

class Electray(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, float Length)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, l) in _casters)
            yield return new AOEInstance(new AOEShapeRect(l, 2.5f), c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = (AID)spell.Action.ID switch
        {
            AID._Ability_Electray => 25,
            AID._Ability_Electray2 => 20,
            AID._Ability_Electray1 => 5,
            _ => 0
        };
        if (len > 0)
            _casters.Add((caster, len));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Ability_Electray:
            case AID._Ability_Electray2:
            case AID._Ability_Electray1:
                NumCasts++;
                _casters.RemoveAll(c => c.Caster == caster);
                break;
        }
    }
}

class CarCounter(BossModule module) : BossComponent(module)
{
    public int Car
    {
        get => field;
        set
        {
            field = value;
            switch (value)
            {
                case 2:
                    Car2();
                    break;
                case 3:
                    Car3();
                    break;
            }
        }
    } = 1;

    void Car2()
    {
        var clipper = Arena.Bounds.Clipper;

        var rect = CurveApprox.Rect(new WDir(10, 0), new WDir(0, 14.5f));

        var poly = clipper.Difference(new(rect), new(Shapes.Fence(2.5f, 7.5f)));
        poly = clipper.Difference(new(poly), new(Shapes.Fence(-2.5f, -2.5f)));

        // crates
        poly = clipper.Difference(new(poly), new(Shapes.Crate(7.575f, -2.425f)));
        poly = clipper.Difference(new(poly), new(Shapes.Crate(-7.425f, 7.425f)));

        Arena.Bounds = new ArenaBoundsCustom(14.5f, poly);
        Arena.Center = new(100, 150);
    }

    void Car3()
    {
        Arena.Bounds = new ArenaBoundsRect(10, 14.5f);
        Arena.Center = new(100, 200);
    }
}

// 175.73
// 168.09

class LightningBurst : Components.BaitAwayIcon
{
    public LightningBurst(BossModule module) : base(module, new AOEShapeCircle(5), (uint)IconID.LightningBurst, AID._Ability_LightningBurst1, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
    {
        EnableHints = false;
    }
}

// tankbuster happens ~9.7 seconds after Horn icon (icon has no visual, just SFX)
// train distance determined by SID 4541 on Ghost Train actor
class AetherialRay(BossModule module) : Components.GenericBaitAway(module, AID._Ability_AetherialRay, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private Angle _nextRotation;
    private Actor? _nextSource;
    private DateTime _nextActivation;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID._Gen_Distance)
        {
            _nextRotation = status.Extra switch
            {
                0x578 => 170.Degrees(),
                0x106 => 106.Degrees(),
                _ => default
            };
            if (_nextRotation == default)
                ReportError("Unrecognized status on Ghost Train, don't know where to predict");
        }

        if ((SID)status.ID == SID._Gen_Stop)
        {
            foreach (ref var bait in CollectionsMarshal.AsSpan(CurrentBaits))
                bait.Source = actor;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Horn)
        {
            var gh = Module.Enemies(OID._Gen_GhostTrain)[0];
            var g = (gh.Position - Arena.Center).ToAngle() + _nextRotation;
            var offset = g.ToDirection() * 25;
            _nextSource = new Actor(1, 0, 819, 0, "fake actor", 0, ActorType.Enemy, Class.ACN, 1, new Vector4(Arena.Center.X + offset.X, gh.PosRot.Y, Arena.Center.Z + offset.Z, 0));
            _nextActivation = WorldState.FutureTime(9.7f);
        }

        if ((IconID)iconID == IconID.AetherialRay && _nextSource != null)
            CurrentBaits.Add(new(_nextSource, actor, new AOEShapeCone(50, 17.5f.Degrees()), _nextActivation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class MultiToot(BossModule module) : Components.GenericStackSpread(module)
{
    public int NumCasts { get; private set; }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.DoubleToot:
                foreach (var player in Raid.WithoutSlot().OrderByDescending(r => r.Class.GetRole2() == Role2.Healer).Take(2))
                    Stacks.Add(new(player, 6, minSize: 2, maxSize: 3, WorldState.FutureTime(10)));
                break;
            case IconID.TripleToot:
                foreach (var player in Raid.WithoutSlot().Where(r => r.Class.GetRole2() != Role2.Tank))
                    Spreads.Add(new(player, 6, WorldState.FutureTime(10)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID._Ability_Aetherochar or AID._Ability_Aetherosote)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1077, NameID = 14284, DevOnly = true)]
public class Ex7Doomtrain(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), Car1Bounds)
{
    public static readonly ArenaBoundsCustom Car1Bounds = MakeCar1Bounds();
    static ArenaBoundsCustom MakeCar1Bounds()
    {
        var b = CurveApprox.Rect(new WDir(10, 0), new WDir(0, 14.5f));

        var clipper = new PolygonClipper();
        var poly = clipper.Difference(new(b), new(Shapes.Fence(-2.5f, 7.5f)));
        poly = clipper.Difference(new(poly), new(Shapes.Fence(2.5f, -2.5f)));

        return new(14.5f, poly);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.IsTargetable)
            Arena.ActorInsideBounds(Arena.Center - new WDir(0, 14.5f), PrimaryActor.Rotation, ArenaColor.Enemy);

        Arena.Actors(Enemies(OID._Gen_Aether), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID._Gen_GhostTrain), ArenaColor.Object, true);
    }
}
