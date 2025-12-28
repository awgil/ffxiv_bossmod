namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

class LevinSignal(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, bool Ground, DateTime Activation, float MaxLen, float Offset)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Where(c => c.Ground).Select(c => new AOEInstance(new AOEShapeRect(c.MaxLen, 2.5f), c.Caster.Position + new WDir(0, c.Offset), default, c.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LevinSignal)
        {
            var ground = actor.PosRot.Y < 2;

            var car = Module.FindComponent<CarGeometry>();
            var maxLen = 30;
            var offset = 0;

            if (car?.Car == 2)
            {
                if (MathF.Abs(actor.Position.X - 92.5f) < 3)
                    maxLen = 20;

                if (MathF.Abs(actor.Position.X - 107.5f) < 3)
                    maxLen = 10;
            }

            if (car?.Car == 6 && actor.Position.X < 95)
            {
                if (ground)
                {
                    maxLen = 10;
                }
                else
                {
                    maxLen = 20;
                    offset = 10;
                }
            }

            _casters.Add((actor, ground, WorldState.FutureTime(7), maxLen, offset));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PlasmaBeamGround or AID.PlasmaBeamOverhead or AID.PlasmaBeamMedium or AID.PlasmaBeamLong)
        {
            _casters.RemoveAll(c => c.Caster == caster);
            NumCasts++;
        }
    }
}

class UnlimitedExpress1(BossModule module) : Components.RaidwideCast(module, AID.UnlimitedExpress);

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
            AID.Electray1 => 25,
            AID.ElectrayMedium => 20,
            AID.Electray2 => 10,
            AID.ElectrayShort => 5,
            _ => 0
        };
        if (len > 0)
            _casters.Add((caster, len));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Electray1:
            case AID.Electray2:
            case AID.ElectrayMedium:
            case AID.ElectrayShort:
                NumCasts++;
                _casters.RemoveAll(c => c.Caster == caster);
                break;
        }
    }
}

class LightningBurst : Components.BaitAwayIcon
{
    public LightningBurst(BossModule module) : base(module, new AOEShapeCircle(5), (uint)IconID.LightningBurst, AID.LightningBurst, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
    {
        EnableHints = false;
    }
}

class Shockwave(BossModule module) : Components.RaidwideInstant(module, AID.Shockwave, 5.7f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.ShockwaveVisual)
            Activation = WorldState.FutureTime(Delay);
    }
}

class DerailmentSiege(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DerailmentSiegeFinal1 or AID.DerailmentSiegeFinal2 or AID.DerailmentSiegeFinal3 or AID.DerailmentSiegeFinal4)
            Towers.Add(new(caster.Position, 5, maxSoakers: 8, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DerailmentSiegeFinal1 or AID.DerailmentSiegeFinal2 or AID.DerailmentSiegeFinal3 or AID.DerailmentSiegeFinal4)
        {
            NumCasts++;
            Towers.Clear();
        }

        if ((AID)spell.Action.ID == AID.DerailmentSiegeHit)
            NumCasts++;
    }
}

class Derail(BossModule module) : Components.CastCounter(module, AID.Derail);

class Launchpad(BossModule module) : BossComponent(module)
{
    private WPos? _pad;
    const float Radius = 2;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0A && state == 0x00020001)
            _pad = new(100, 212.5f);
        if (index == 0x0B && state == 0x00020001)
            _pad = new(100, 262.5f);
        if (index == 0x0C && state == 0x00020001)
            _pad = new(100, 312.5f);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_pad != null && pc.Position.Z < _pad.Value.Z + Radius)
            Arena.ZoneCircle(_pad.Value, Radius, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_pad != null && actor.Position.Z < _pad.Value.Z + Radius)
            hints.Add("Get to launchpad!", !actor.Position.InCircle(_pad.Value, Radius));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_pad != null && actor.Position.Z < _pad.Value.Z + Radius)
            hints.AddForbiddenZone(ShapeContains.Donut(_pad.Value, Radius, 100));
    }
}

class ThirdRail(BossModule module) : Components.StandardAOEs(module, AID.ThirdRailPuddle, 4)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Casters)
        {
            var height = c.CastInfo!.Location.Y;
            var sameHeight = (height > 4) == (actor.PosRot.Y > 4);
            yield return new(new AOEShapeCircle(4), c.CastInfo!.LocXZ, default, Module.CastFinishAt(c.CastInfo), Risky: sameHeight);
        }
    }
}
class ThirdRailBait(BossModule module) : BossComponent(module)
{
    public bool Active = true;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Active)
            Arena.AddCircle(pc.Position, 4, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThirdRailPuddle)
            Active = false;
    }
}

class ZoomCounter(BossModule module) : Components.CastCounter(module, AID.Zoom);

class Breathlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly CarGeometry _car = module.FindComponent<CarGeometry>()!;

    private readonly List<(Level, DateTime)> _casts = [];

    public bool Draw = true;

    enum Level
    {
        Ground,
        Air
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Draw ? _casts.Take(1).Select(c => new AOEInstance(c.Item1 == Level.Ground ? _car.GroundShape : _car.AirShape, Arena.Center, Activation: c.Item2)) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ThunderousBreathFirst:
                _casts.Add((Level.Ground, Module.CastFinishAt(spell)));
                _casts.Add((Level.Air, Module.CastFinishAt(spell, 2.5f)));
                break;
            case AID.HeadlightFirst:
                _casts.Add((Level.Air, Module.CastFinishAt(spell)));
                _casts.Add((Level.Ground, Module.CastFinishAt(spell, 2.5f)));
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Draw && _casts is [(var l, _), ..])
            hints.Add(l == Level.Ground ? "Next: platform safe" : "Next: ground safe");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_casts is [(Level.Air, var activate), ..])
            foreach (var p in _car.Portals)
                hints.AddForbiddenZone(ShapeContains.Circle(p, 1.5f), activate);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Headlight or AID.ThunderousBreath)
        {
            NumCasts++;
            if (_casts.Count > 0)
                _casts.RemoveAt(0);
        }
    }
}

class Plummet(BossModule module) : Components.SpreadFromCastTargets(module, AID.Plummet, 8);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1077, NameID = 14284, PlanLevel = 100)]
public class Ex7Doomtrain(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), Car1Bounds)
{
    public static readonly ArenaBoundsCustom Car1Bounds = MakeCar1Bounds();
    static ArenaBoundsCustom MakeCar1Bounds()
    {
        var b = CurveApprox.Rect(new WDir(10, 0), new WDir(0, CarGeometry.CarHeight));

        var clipper = new PolygonClipper();
        var poly = clipper.Difference(new(b), new(Shapes.Fence(-2.5f, 7.5f)));
        poly = clipper.Difference(new(poly), new(Shapes.Fence(2.5f, -2.5f)));

        return new(CarGeometry.CarHeight, poly);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.IsTargetable)
        {
            var height = Arena.Bounds.Radius;
            if (PrimaryActor.Position.InRect(Arena.Center, default(Angle), height + 12, height + 12, 10))
                Arena.ActorInsideBounds(Arena.Center - new WDir(0, height), PrimaryActor.Rotation, ArenaColor.Enemy);
            else
                Arena.ActorOutsideBounds(Arena.Center - new WDir(0, height), PrimaryActor.Rotation, ArenaColor.Enemy);
        }

        Arena.Actors(Enemies(OID.AetherIntermission), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GhostTrain), ArenaColor.Object, true);
        foreach (var obj in Enemies(OID.ArcaneRevelation))
            Arena.ActorInsideBounds(obj.Position, obj.Rotation, ArenaColor.Object);
    }
}
