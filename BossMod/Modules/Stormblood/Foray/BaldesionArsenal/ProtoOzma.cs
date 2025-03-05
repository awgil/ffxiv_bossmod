namespace BossMod.Stormblood.Foray.BaldesionArsenal.ProtoOzma;

public enum OID : uint
{
    Boss = 0x25E8, // R13.500, x1
    Helper = 0x2629, // R0.500, x12, mixed types
    Shadow = 0x25E9, // R13.500, x0 (spawn during fight)
    ArsenalUrolith = 0x25EB, // R3.000, x0 (spawn during fight)
    Button = 0x1EA1A1
}

public enum AID : uint
{
    AAStar = 14262, // Helper->players, no cast, range 6 circle, shared damage on random target
    AASphere = 14251, // Helper->player, no cast, single-target
    AAPyramid = 14253, // Helper->players, no cast, range 4 circle, bleed autos, should not be stacked

    TransfigurationStar = 14258, // Boss/Shadow->self, no cast, single-target
    TransfigurationUnStar = 14259, // Boss->self, no cast, single-target
    TransfigurationPyramid = 14244, // Shadow/Boss->self, no cast, single-target
    TransfigurationUnPyramid = 14245, // Boss->self, no cast, single-target
    TransfigurationCube = 14238, // Shadow/Boss->self, no cast, single-target
    MourningStar = 14260, // Boss/Shadow->self, no cast, single-target
    MourningStar1 = 14261, // Helper->self, no cast, range 27 circle
    ShootingStar = 14263, // Boss->self, 5.0s cast, single-target
    ShootingStar1 = 14264, // Helper->self, 5.0s cast, range 26 circle
    BlackHole = 14237, // Boss->self, no cast, range 40 circle
    Execration = 14246, // Shadow/Boss->self, no cast, single-target
    Execration1 = 14247, // Helper->self, no cast, range 40+R width 11 rect
    FlareStar = 14240, // Shadow->self, no cast, single-target
    FlareStar1 = 14241, // Helper->self, no cast, range 38+R circle
    AccelerationBomb = 14250, // Boss->self, no cast, ???
    MeteorImpact = 14256, // ArsenalUrolith->self, 4.0s cast, range 20 circle
    Meteor = 14248, // Helper->location, no cast, range 10 circle

    // haven't seen these in replays yet, they're guessed based on existing data and action ID order
    TransfigurationUnCube = 14238, // Boss->self, no cast, single-target
    AACube = 14252, // Helper->players, no cast, range 40 width 4 rect, cleaving auto, move away from tank
}

public enum SID : uint
{
    Cube = 1070, // 25E9->25E9, extra=0x0
    Pyramid = 1071, // 25E9/Boss->25E9/Boss, extra=0x0
    AccelerationBomb = 1072, // none->player, extra=0x0
    Stellation = 1744, // Boss/25E9->Boss/25E9, extra=0x0
    BlackHoleBuffer = 1745, // none->player, extra=0x0
}

public enum IconID : uint
{
    AccelerationBomb = 75, // player->self
    Meteor = 57, // player->self
    _Gen_Icon_62 = 62, // player->self
}

class MourningStar(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.MourningStar1))
{
    private readonly List<(Actor Source, DateTime Activation)> Casts = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casts.Select(c => new AOEInstance(new AOEShapeCircle(27), c.Source.Position, default, c.Activation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TransfigurationStar)
            Casts.Add((caster, WorldState.FutureTime(8.15f)));
        else if (spell.Action == WatchedAction)
            Casts.Clear();
    }
}

class Execration(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Execration1))
{
    private readonly List<(WPos Origin, Angle Rotation, DateTime Activation)> Casts = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casts.Select(c => new AOEInstance(new AOEShapeRect(40, 5.5f), c.Origin, c.Rotation, c.Activation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TransfigurationPyramid)
        {
            if ((OID)caster.OID == OID.Boss)
            {
                Casts.Add((caster.Position, default, WorldState.FutureTime(8.8f)));
                Casts.Add((caster.Position, 120.Degrees(), WorldState.FutureTime(8.8f)));
                Casts.Add((caster.Position, -120.Degrees(), WorldState.FutureTime(8.8f)));
            }
            else
            {
                Casts.Add((caster.Position, caster.Rotation, WorldState.FutureTime(8.8f)));
            }
        }
        else if (spell.Action == WatchedAction)
            Casts.Clear();
    }
}

class FlareStar(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.FlareStar1))
{
    private readonly List<(Actor Source, DateTime Activation)> Casts = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casts.Select(c => new AOEInstance(new AOEShapeDonut(17.5f, 38.5f), c.Source.Position, c.Source.Rotation, c.Activation));

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TransfigurationCube)
            Casts.Add((caster, WorldState.FutureTime(8.6f)));
        else if (spell.Action == WatchedAction)
            Casts.Clear();
    }
}

class ShootingStar(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ShootingStar1), 8, shape: new AOEShapeCircle(26))
{
    private static readonly Angle ToCorner = Angle.FromDirection(new WDir(5, 12));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor).Where(s => s.Shape!.Check(actor.Position, s.Origin, s.Direction)))
        {
            var plat = ProtoOzma.GuessPlatform(s.Origin);
            if (plat != null)
            {
                // this is overly conservative but i'm an idiot
                var i = ShapeDistance.Intersection([ShapeDistance.InvertedCone(s.Origin, 4, plat.DirectionToBoss, ToCorner), ShapeDistance.InvertedCone(s.Origin, 4, plat.DirectionToBoss + 180.Degrees(), ToCorner)]);
                hints.AddForbiddenZone(i, s.Activation);
            }
            break;
        }
    }
}

class StarAutos(BossModule module) : Components.GenericStackSpread(module)
{
    private bool Enabled;

    public override void Update()
    {
        Stacks.Clear();
        if (Enabled && Module.PrimaryActor.CastInfo == null)
        {
            var target = Raid.WithoutSlot().Closest(Module.PrimaryActor.Position);
            if (target != null)
                Stacks.Add(new(target, 6, activation: DateTime.MaxValue));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MourningStar:
                Enabled = true;
                break;
            case AID.TransfigurationUnStar:
                Enabled = false;
                break;
        }
    }
}

class AccelerationBomb(BossModule module) : Components.StayMove(module)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            PlayerStates[slot] = default;
    }
}

class BlackHole(BossModule module) : BossComponent(module)
{
    private DateTime Activation;

    public static readonly List<WDir> TowerLocations = [
        // A towers
        new WDir(0, 24.5f),
        new WDir(0, -22),
        // B towers
        new WDir(0, 24.5f).Rotate(-120.Degrees()),
        new WDir(0, -22).Rotate(-120.Degrees()),
        // C towers
        new WDir(0, 24.5f).Rotate(120.Degrees()),
        new WDir(0, -22).Rotate(120.Degrees()),
    ];

    private readonly List<WPos> Buttons = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (caster.OID == (uint)OID.Boss && (AID)spell.Action.ID is AID.TransfigurationUnCube or AID.TransfigurationUnPyramid or AID.TransfigurationUnStar)
        {
            Activation = WorldState.FutureTime(9.2f);
            Buttons.AddRange(TowerLocations.Select(t => Arena.Center + t));
        }
        if ((AID)spell.Action.ID == AID.BlackHole)
        {
            Activation = default;
            Buttons.Clear();
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Activation == default)
            return;

        foreach (var t in Buttons)
            Arena.AddCircle(t, 2, ArenaColor.Safe);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Activation == default)
            return;

        hints.Add("Stand on a button!", !Buttons.Any(b => actor.Position.InCircle(b, 2)));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Activation == default)
            return;

        var towers = ShapeDistance.Intersection(Buttons.Select(b => ShapeDistance.Donut(b, 2, 100)).ToList());
        hints.AddForbiddenZone(towers, Activation);
    }
}

class MeteorBait(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Meteor, default, 15, 9)
{
    public static readonly List<WDir> MeteorDropLocations = [
        // A plat
        new WDir(-4, 35),
        new WDir(0, -22),
        // B plat
        new WDir(-4, 35).Rotate(-120.Degrees()),
        new WDir(0, -22).Rotate(-120.Degrees()),
        // C plat
        new WDir(-4, 35).Rotate(120.Degrees()),
        new WDir(0, -22).Rotate(120.Degrees()),
    ];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorImpact)
            Spreads.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Spreads.Any(s => s.Target.InstanceID == actor.InstanceID))
        {
            var drops = MeteorDropLocations.Select(m => ShapeDistance.InvertedCircle(Arena.Center + m, 1)).ToList();
            hints.AddForbiddenZone(ShapeDistance.Intersection(drops), Spreads[0].Activation);
            var otherSpreads = ActiveSpreadTargets.Exclude(actor).Select(t => ShapeDistance.Circle(t.Position, 15)).ToList();
            hints.AddForbiddenZone(ShapeDistance.Union(otherSpreads), DateTime.MaxValue);
        }
    }
}
class MeteorImpact(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.MeteorImpact), 15);
class Urolith(BossModule module) : Components.Adds(module, (uint)OID.ArsenalUrolith, 1);

class ProtoOzmaStates : StateMachineBuilder
{
    public ProtoOzmaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StarAutos>()
            .ActivateOnEnter<MourningStar>()
            .ActivateOnEnter<ShootingStar>()
            .ActivateOnEnter<Execration>()
            .ActivateOnEnter<FlareStar>()
            .ActivateOnEnter<BlackHole>()
            .ActivateOnEnter<MeteorBait>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<AccelerationBomb>()
            .ActivateOnEnter<Urolith>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7981)]
public class ProtoOzma(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, OzmaBounds)
{
    public static readonly WPos ArenaCenter = new(-17, 29);
    public static readonly ArenaBoundsCustom OzmaBounds = MakeBounds();

    public record class Platform(WPos Center, Angle DirectionToBoss, string Marker)
    {
        public static readonly AOEShapeRect Shape = new(12, 5, 12);
        public bool Contains(WPos position) => Shape.Check(position, Center, DirectionToBoss);
    }

    private static ArenaBoundsCustom MakeBounds()
    {
        var clipper = new PolygonClipper();

        var ring = CurveApprox.Donut(18.75f, 25f, 1 / 90f);

        var platform = CurveApprox.Rect(new(5, 0), new(0, 12)).Select(off => new WDir(0, 24.5f) + off);
        var platforms = new PolygonClipper.Operand(platform);
        platforms.AddContour(platform.Select(d => d.Rotate(120.Degrees())));
        platforms.AddContour(platform.Select(d => d.Rotate(240.Degrees())));

        return new(36.5f, clipper.Union(new(ring), platforms));
    }

    public static IEnumerable<Platform> Platforms()
    {
        yield return new(ArenaCenter + new WDir(0, 24.5f), 180.Degrees(), "A");
        yield return new(ArenaCenter + new WDir(0, 24.5f).Rotate(-120.Degrees()), 60.Degrees(), "B");
        yield return new(ArenaCenter + new WDir(0, 24.5f).Rotate(120.Degrees()), -60.Degrees(), "C");
    }

    public static Platform? GuessPlatform(WPos position) => Platforms().FirstOrDefault(p => p.Contains(position));

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorInsideBounds(Center, PrimaryActor.Rotation, ArenaColor.Enemy);
    }
}

