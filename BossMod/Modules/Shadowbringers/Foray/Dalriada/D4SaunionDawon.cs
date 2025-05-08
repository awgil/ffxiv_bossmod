namespace BossMod.Shadowbringers.Foray.Dalriada.D4SaunionDawon;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x16 (spawn during fight), Helper type
    Boss = 0x31DD, // R7.875, x1
    DawonTheYounger = 0x31DE, // R6.900, x0 (spawn during fight)
    LyonTheBeastKing = 0x31DF, // R0.500, x0 (spawn during fight)
    VerdantPlume = 0x31E1, // R0.500, x0 (spawn during fight)
    VermilionFlame = 0x31E0, // R0.750, x0 (spawn during fight)
    MeneniusSasLanatus = 0x31E2, // R1.000, x1
    MeneniusSasLanatus1 = 0x33DE, // R0.500, x0 (spawn during fight)
    PathZigzag = 0x1EB217,
    PathSpiral = 0x1EB216,
    CircleMarker = 0x1EB1E7,
    CrossMarker = 0x1EB1E8
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    MagitekHalo = 23989, // Boss->self, 5.0s cast, range 9-60 donut
    HighPoweredMagitekRay = 24005, // Boss->player, 5.0s cast, range 5 circle
    MagitekCrossray = 23991, // Boss->self, 5.0s cast, range 60 width 19 cross
    MobileCrossrayFront = 23997, // Boss->self, 7.0s cast, single-target
    MobileCrossrayRight = 23998, // Boss->self, 7.0s cast, single-target
    MobileCrossrayLeft = 23999, // Boss->self, 7.0s cast, single-target
    MobileCrossrayBack = 24000, // Boss->self, 7.0s cast, single-target
    MobileCrossray = 23992, // Boss->self, no cast, range 60 width 19 cross
    MissileCommand = 24001, // Boss->self, 4.0s cast, single-target
    SurfaceMissile = 24004, // Helper->location, 5.0s cast, range 6 circle
    AntiPersonnelMissile = 24002, // Helper->players, 5.0s cast, range 6 circle
    MissileSalvo = 24003, // Helper->players, 5.0s cast, range 6 circle
    MobileHaloFront = 23993, // Boss->self, 7.0s cast, single-target
    MobileHaloRight = 23994, // Boss->self, 7.0s cast, single-target
    MobileHaloLeft = 23995, // Boss->self, 7.0s cast, single-target
    MobileHaloBack = 23996, // Boss->self, 7.0s cast, single-target
    MobileHalo = 23990, // Boss->self, no cast, range 9-60 donut
    Touchdown = 24006, // DawonTheYounger->self, 6.0s cast, ???
    Touchdown1 = 24912, // Helper->self, 6.0s cast, range 60 circle
    Touchdown2 = 24007, // Helper->self, no cast, ???
    AetherialDistribution = 25061, // DawonTheYounger->Boss, no cast, single-target
    AutoAttack1 = 6497, // DawonTheYounger->player, no cast, single-target
    WildfireWinds = 24013, // DawonTheYounger->self, 5.0s cast, ???
    WildfireWinds1 = 24772, // Helper->self, no cast, ???
    MobileCrossray1 = 24000, // Boss->self, 7.0s cast, single-target
    Explosion = 24015, // VerdantPlume->self, 1.5s cast, range 3-12 donut
    RawHeat = 24014, // VermilionFlame->self, 1.5s cast, range 10 circle
    SwoopingFrenzy = 24016, // DawonTheYounger->location, 4.0s cast, range 12 circle
    FrigidPulse = 24701, // DawonTheYounger->self, 5.0s cast, range 12-60 donut, inner range reported as 4 ingame
    SpiralScourgeVisual = 23986, // Boss->self, 7.0s cast, single-target
    SpiralScourge = 23987, // Helper->location, no cast, range 13 circle
    Obey = 24009, // DawonTheYounger->self, 11.0s cast, single-target
    SwoopingFrenzyInstant = 24010, // DawonTheYounger->location, no cast, range 12 circle
    FrigidPulseInstant = 24011, // DawonTheYounger->self, no cast, range 12-60 donut
    FireBrand = 24012, // DawonTheYounger->self, no cast, range 50 width 14 cross
    Pentagust = 24017, // DawonTheYounger->self, 5.0s cast, single-target
    Pentagust1 = 24018, // Helper->self, 5.0s cast, range 50 20-degree cone
    ToothAndTalon = 24020, // DawonTheYounger->player, 5.0s cast, single-target
    FireBrand1 = 24019, // DawonTheYounger->self, 5.0s cast, range 50 width 14 cross
}

public enum TetherID : uint
{
    OneMind = 12, // Boss->DawonTheYounger
}

class SpiralScourge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    private enum Path
    {
        None,
        Zigzag,
        Spiral
    }

    private enum Rotation
    {
        None,
        R90,
        R180,
        R270
    }

    private Path NextPath;
    private Rotation NextPathRotation;

    // separate rotations for the two patterns
    // not only do they differ by 2-3 ingame units (so we can't take one list and rotate it around the origin without hints becoming semi-worthless), but they ALSO have an inconsistent number of total hits
    public static readonly WDir[][] Zigzag = [
        [new(-20.52f, -20.01f), new(-20.52f, -15.22f), new(-20.49f, -10.00f), new(-20.49f, -4.78f), new(-20.49f, 0.41f), new(-20.49f, 5.26f), new(-20.43f, 10.14f), new(-20.40f, 14.96f), new(-18.99f, 18.38f), new(-15.73f, 20.94f), new(-11.27f, 21.80f), new(-7.37f, 21.62f), new(-2.94f, 19.94f), new(-0.50f, 16.67f), new(-0.04f, 12.34f), new(0.05f, 7.70f), new(0.05f, 2.91f), new(-0.01f, -1.91f), new(-0.07f, -6.74f), new(-0.01f, -11.92f), new(0.51f, -16.44f), new(2.95f, -19.95f), new(7.04f, -21.51f), new(11.62f, -21.60f), new(15.98f, -20.74f), new(19.25f, -17.66f), new(20.41f, -13.54f), new(20.53f, -8.72f), new(20.53f, -3.90f), new(20.50f, 0.92f), new(20.47f, 5.50f), new(20.47f, 10.69f), new(20.47f, 15.91f), new(20.44f, 20.15f)],
        [new(-22.50f, 20.49f), new(-17.68f, 20.49f), new(-12.46f, 20.46f), new(-7.24f, 20.46f), new(-2.03f, 20.46f), new(2.77f, 20.46f), new(7.62f, 20.43f), new(12.44f, 20.46f), new(15.98f, 20.00f), new(19.89f, 17.19f), new(21.60f, 13.01f), new(21.75f, 9.32f), new(21.02f, 4.59f), new(18.06f, 1.26f), new(13.75f, 0.01f), new(9.14f, -0.08f), new(4.41f, -0.08f), new(-0.44f, -0.05f), new(-5.20f, -0.02f), new(-10.02f, -0.02f), new(-15.24f, -0.42f), new(-18.96f, -1.85f), new(-21.28f, -5.61f), new(-21.74f, -10.31f), new(-21.28f, -14.82f), new(-19.27f, -18.27f), new(-14.97f, -20.41f), new(-10.78f, -20.53f), new(-5.60f, -20.50f), new(-0.77f, -20.50f), new(3.80f, -20.50f), new(9.02f, -20.50f), new(14.21f, -20.53f), new(18.54f, -20.53f)],
        [new(20.50f, 20.97f), new(20.50f, 16.58f), new(20.50f, 12.43f), new(20.53f, 7.18f), new(20.50f, 2.30f), new(20.44f, -2.92f), new(20.44f, -7.50f), new(20.44f, -12.32f), new(19.79f, -16.62f), new(17.17f, -20.01f), new(12.87f, -21.42f), new(8.01f, -21.60f), new(4.11f, -20.62f), new(1.03f, -17.39f), new(-0.01f, -13.39f), new(-0.07f, -8.17f), new(-0.04f, -3.50f), new(0.02f, 1.29f), new(0.05f, 6.48f), new(-0.07f, 10.90f), new(-0.16f, 15.54f), new(-2.00f, 19.27f), new(-5.38f, 21.34f), new(-9.32f, 21.77f), new(-13.44f, 21.58f), new(-17.71f, 19.66f), new(-20.06f, 16.24f), new(-20.52f, 12.61f), new(-20.49f, 7.73f), new(-20.49f, 2.85f), new(-20.49f, -2.37f), new(-20.49f, -7.59f), new(-20.49f, -12.78f), new(-20.52f, -18.00f)],
        [new(22.48f, -20.35f), new(17.84f, -20.44f), new(13.11f, -20.50f), new(8.56f, -20.50f), new(3.35f, -20.50f), new(-1.42f, -20.50f), new(-5.99f, -20.53f), new(-10.78f, -20.56f), new(-15.61f, -20.19f), new(-19.21f, -18.24f), new(-21.25f, -14.88f), new(-21.71f, -10.06f), new(-21.22f, -5.55f), new(-19.05f, -1.88f), new(-15.33f, -0.11f), new(-10.11f, 0.04f), new(-5.20f, -0.02f), new(-0.41f, -0.05f), new(4.38f, -0.05f), new(9.20f, -0.02f), new(13.87f, 0.01f), new(17.99f, 1.23f), new(20.98f, 4.56f), new(21.72f, 9.32f), new(21.63f, 13.01f), new(19.70f, 17.31f), new(16.07f, 19.97f), new(12.44f, 20.46f), new(8.11f, 20.43f), new(3.22f, 20.43f), new(-1.60f, 20.46f), new(-6.85f, 20.46f), new(-12.07f, 20.46f), new(-17.25f, 20.49f), new(-22.50f, 20.49f)]
    ];

    public static readonly WDir[][] Spiral = [
        [new(0.48f, -26.51f), new(4.90f, -26.05f), new(9.57f, -24.99f), new(13.87f, -23.12f), new(17.72f, -20.22f), new(20.98f, -16.75f), new(23.46f, -12.99f), new(25.44f, -8.63f), new(26.39f, -3.96f), new(26.42f, -0.54f), new(26.29f, 4.34f), new(25.14f, 9.04f), new(23.00f, 13.10f), new(20.01f, 16.82f), new(15.98f, 20.09f), new(11.77f, 21.83f), new(7.47f, 23.54f), new(2.77f, 23.84f), new(-1.93f, 23.17f), new(-6.63f, 20.91f), new(-10.57f, 18.35f), new(-13.99f, 14.47f), new(-15.61f, 9.93f), new(-16.92f, 5.47f), new(-16.28f, 0.68f), new(-14.51f, -3.65f), new(-11.27f, -6.92f), new(-6.54f, -9.12f), new(-2.51f, -10.06f), new(1.76f, -8.60f), new(4.96f, -5.15f), new(5.88f, -1.21f), new(3.62f, 1.93f)],
        [],
        [new(0.48f, 26.50f), new(-3.95f, 26.35f), new(-8.59f, 25.40f), new(-12.95f, 23.48f), new(-16.86f, 20.88f), new(-20.15f, 17.62f), new(-23.05f, 14.02f), new(-25.25f, 9.38f), new(-26.32f, 5.11f), new(-26.50f, 1.17f), new(-26.47f, -3.50f), new(-25.37f, -8.23f), new(-23.36f, -12.26f), new(-20.67f, -16.04f), new(-17.10f, -19.19f), new(-13.01f, -21.48f), new(-8.10f, -23.19f), new(-3.31f, -23.98f), new(1.39f, -23.43f), new(5.63f, -21.35f), new(10.15f, -18.73f), new(13.20f, -14.98f), new(15.46f, -10.89f), new(16.93f, -6.31f), new(16.62f, -1.52f), new(15.09f, 3.00f), new(11.95f, 6.26f), new(7.40f, 8.80f), new(3.47f, 10.02f), new(-1.14f, 8.80f), new(-4.41f, 5.75f), new(-5.90f, 1.99f), new(-4.35f, -1.76f), new(-0.50f, -0.21f)],
        [new(26.48f, -0.51f), new(26.39f, 3.55f), new(25.59f, 8.19f), new(23.76f, 12.55f), new(21.08f, 16.49f), new(18.02f, 20.03f), new(14.06f, 22.90f), new(9.39f, 25.22f), new(5.12f, 26.32f), new(1.18f, 26.50f), new(-3.49f, 26.25f), new(-8.28f, 25.37f), new(-12.31f, 23.57f), new(-16.16f, 20.64f), new(-19.39f, 16.55f), new(-21.53f, 12.61f), new(-23.33f, 7.73f), new(-24.00f, 3.27f), new(-23.45f, -1.40f), new(-21.37f, -5.67f), new(-18.81f, -10.21f), new(-15.27f, -13.11f), new(-11.27f, -15.46f), new(-6.66f, -16.81f), new(-1.90f, -16.65f), new(2.67f, -15.31f), new(6.67f, -12.02f), new(8.78f, -7.83f), new(10.03f, -3.50f), new(9.05f, 0.80f), new(6.06f, 4.31f), new(1.97f, 5.96f), new(-1.66f, 3.98f), new(-0.29f, 0.34f)]
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes.Take(16);

    private DateTime _lastSpiralScourge;
    private readonly List<(Path, Rotation, WPos)> _recorded = [];

    public override void Update()
    {
        if (_recorded.Count > 0 && _lastSpiralScourge != default && (WorldState.CurrentTime - _lastSpiralScourge).TotalSeconds > 10)
        {
            var ty = _recorded[0].Item1;
            var rot = _recorded[0].Item2;
            var dir = _recorded.Select(r => r.Item3 - Arena.Center).Select(d => $"new({d.X:f2}f, {d.Z:f2}f)");
            Service.Log($"spiral scourge: {ty} {rot} x{_recorded.Count}, {string.Join(", ", dir)}");
            _recorded.Clear();
            if (aoes.Count > 0)
            {
                ReportError($"too few hits for spiral scourge {ty} {rot}, clearing old ones");
                aoes.Clear();
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpiralScourgeVisual)
        {
            var path = NextPath switch
            {
                Path.Spiral => Spiral,
                Path.Zigzag => Zigzag,
                _ => []
            };
            var finish = Module.CastFinishAt(spell, 0.8f);
            foreach (var z in path[(int)NextPathRotation])
            {
                // AOE size is 13, but there is some small variance in actual cast locations (usually less than 0.2 units) which is maybe based on server load or something
                aoes.Add(new AOEInstance(new AOEShapeCircle(13.1f), Arena.Center + z, default, finish));
                finish = finish.AddSeconds(0.58f);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.PathSpiral:
                NextPath = Path.Spiral;
                NextPathRotation = GuessRotation(actor);
                break;
            case OID.PathZigzag:
                NextPath = Path.Zigzag;
                NextPathRotation = GuessRotation(actor);
                break;
        }
    }

    private static readonly (Rotation, Angle)[] Rotations = [(Rotation.None, 0.Degrees()), (Rotation.R90, 90.Degrees()), (Rotation.R180, 180.Degrees()), (Rotation.R270, 270.Degrees())];

    private Rotation GuessRotation(Actor a)
    {
        foreach (var (r, g) in Rotations)
            if (a.Rotation.AlmostEqual(g, 0.1f))
                return r;

        ReportError($"can't guess direction from {a} with {a.Rotation}, returning none");
        return Rotation.None;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpiralScourge)
        {
            _lastSpiralScourge = WorldState.CurrentTime;
            _recorded.Add((NextPath, NextPathRotation, spell.TargetXZ));
            if (aoes.Count > 0)
                aoes.RemoveAt(0);
            else
                ReportError($"too many spiral scourge casts, this is number {_recorded}");
        }
    }
}

class MagitekHalo(BossModule module) : Components.StandardAOEs(module, AID.MagitekHalo, new AOEShapeDonut(9, 60));
class MagitekCrossray(BossModule module) : Components.StandardAOEs(module, AID.MagitekCrossray, new AOEShapeCross(60, 9.5f));
class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID.SurfaceMissile, 6);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID.AntiPersonnelMissile, 6);
class MissileSalvo(BossModule module) : Components.StackWithCastTargets(module, AID.MissileSalvo, 6);

class MobileCrossray(BossModule module) : Components.GenericAOEs(module, AID.MobileCrossray)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var offset = (AID)spell.Action.ID switch
        {
            AID.MobileCrossrayBack => new WDir(0, -18),
            AID.MobileCrossrayRight => new(-18, 0),
            AID.MobileCrossrayLeft => new(18, 0),
            AID.MobileCrossrayFront => new(0, 18),
            _ => default
        };

        if (offset == default)
            return;

        aoes.Add(new AOEInstance(new AOEShapeCross(60, 9.5f), caster.Position + offset, caster.Rotation, Module.CastFinishAt(spell, 2.03f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            aoes.Clear();
    }
}

class MobileHalo(BossModule module) : Components.GenericAOEs(module, AID.MobileHalo)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var offset = (AID)spell.Action.ID switch
        {
            AID.MobileHaloBack => new WDir(0, -18),
            AID.MobileHaloRight => new(-18, 0),
            AID.MobileHaloLeft => new(18, 0),
            AID.MobileHaloFront => new(0, 18),
            _ => default
        };

        if (offset == default)
            return;

        aoes.Add(new AOEInstance(new AOEShapeDonut(9, 60), caster.Position + offset, caster.Rotation, Module.CastFinishAt(spell, 2.03f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
            aoes.Clear();
    }
}
class HighPoweredMagitekRay(BossModule module) : Components.BaitAwayCast(module, AID.HighPoweredMagitekRay, new AOEShapeCircle(5), centerAtTarget: true, endsOnCastEvent: true);

class Tether(BossModule module) : BossComponent(module)
{
    private Actor? Dawon => Module.Enemies(OID.DawonTheYounger).FirstOrDefault();
    private bool tethered;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.OneMind)
            tethered = true;
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.OneMind)
            tethered = false;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (tethered)
            Arena.AddLine(Module.PrimaryActor.Position, Dawon!.Position, ArenaColor.Danger);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (tethered)
        {
            hints.SetPriority(Module.PrimaryActor, AIHints.Enemy.PriorityInvincible);
            hints.SetPriority(Dawon!, AIHints.Enemy.PriorityInvincible);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (tethered && (Dawon?.TargetID == actor.InstanceID || Module.PrimaryActor.TargetID == actor.InstanceID))
            hints.Add("Separate bosses!");
    }
}

class Feathers(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnActorCreated(Actor actor)
    {
        // 11.4
        switch ((OID)actor.OID)
        {
            case OID.VermilionFlame:
                aoes.Add(new AOEInstance(new AOEShapeCircle(10), actor.Position, default, Activation: WorldState.FutureTime(11.4f)));
                break;
            case OID.VerdantPlume:
                aoes.Add(new AOEInstance(new AOEShapeDonut(3, 12), actor.Position, default, Activation: WorldState.FutureTime(11.4f)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Explosion or AID.RawHeat)
            aoes.RemoveAt(0);
    }
}

class SwoopingFrenzy(BossModule module) : Components.StandardAOEs(module, AID.SwoopingFrenzy, 12);
class FrigidPulse(BossModule module) : Components.StandardAOEs(module, AID.FrigidPulse, new AOEShapeDonut(12, 180));

class Obey(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes.Take(1);

    public override void Update()
    {
        if (aoes.Count > 0 && Module.Enemies(OID.DawonTheYounger).FirstOrDefault()?.IsDeadOrDestroyed == true)
            aoes.Clear();
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID is OID.CircleMarker or OID.CrossMarker)
        {
            var circle = actor.OID == (uint)OID.CircleMarker;
            switch (state)
            {
                case 0x00010002:
                case 0x00100020:
                case 0x00400080:
                    var prevPos = aoes.Count > 0 ? aoes[^1].Origin : Module.Enemies(OID.DawonTheYounger)[0].Position;
                    var angle = Angle.FromDirection(actor.Position - prevPos);

                    var activation = aoes.Count > 0 ? aoes[^1].Activation.AddSeconds(6.5f) : WorldState.FutureTime(16.16f);
                    aoes.Add(new(circle ? new AOEShapeDonut(12, 60) : new AOEShapeCross(60, 7), actor.Position, angle, activation));
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FireBrand or AID.FrigidPulseInstant)
            aoes.RemoveAt(0);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (aoes.Count == 0)
            return;

        hints.Add(string.Join(" -> ", aoes.Select(a => a.Shape is AOEShapeDonut ? "Donut" : "Cross")));
    }
}

class Pentagust(BossModule module) : Components.StandardAOEs(module, AID.Pentagust1, new AOEShapeCone(50, 10.Degrees()));
class ToothAndTalon(BossModule module) : Components.SingleTargetCast(module, AID.ToothAndTalon);
class FireBrand(BossModule module) : Components.StandardAOEs(module, AID.FireBrand1, new AOEShapeCross(50, 7));

class SaunionStates : StateMachineBuilder
{
    public SaunionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HighPoweredMagitekRay>()
            .ActivateOnEnter<MagitekHalo>()
            .ActivateOnEnter<MagitekCrossray>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<AntiPersonnelMissile>()
            .ActivateOnEnter<MissileSalvo>()
            .ActivateOnEnter<MobileCrossray>()
            .ActivateOnEnter<MobileHalo>()
            .ActivateOnEnter<SpiralScourge>()
            .ActivateOnEnter<Tether>()
            .ActivateOnEnter<Feathers>()
            .ActivateOnEnter<SwoopingFrenzy>()
            .ActivateOnEnter<FrigidPulse>()
            .ActivateOnEnter<Obey>()
            .ActivateOnEnter<Pentagust>()
            .ActivateOnEnter<ToothAndTalon>()
            .ActivateOnEnter<FireBrand>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.DawonTheYounger).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10192)]
public class Saunion(WorldState ws, Actor primary) : BossModule(ws, primary, new(650, -659), new ArenaBoundsRect(26.5f, 26.5f))
{
    public override bool DrawAllPlayers => true;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.DawonTheYounger), ArenaColor.Enemy);
    }
}
