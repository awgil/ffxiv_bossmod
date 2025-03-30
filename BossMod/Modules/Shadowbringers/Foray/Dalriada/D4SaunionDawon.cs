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

    private Path NextPath;
    private Angle NextPathRotation;

    public static readonly List<WDir> Zigzag = [
        new(-20.52f, -20.01f), new(-20.52f, -15.22f), new(-20.49f, -10.00f), new(-20.49f, -4.78f), new(-20.49f, 0.41f), new(-20.49f, 5.26f), new(-20.43f, 10.14f), new(-20.40f, 14.96f), new(-18.99f, 18.38f), new(-15.73f, 20.94f), new(-11.27f, 21.80f), new(-7.37f, 21.62f), new(-2.94f, 19.94f), new(-0.50f, 16.67f), new(-0.04f, 12.34f), new(0.05f, 7.70f), new(0.05f, 2.91f), new(-0.01f, -1.91f), new(-0.07f, -6.74f), new(-0.01f, -11.92f), new(0.51f, -16.44f), new(2.95f, -19.95f), new(7.04f, -21.51f), new(11.62f, -21.60f), new(15.98f, -20.74f), new(19.25f, -17.66f), new(20.41f, -13.54f), new(20.53f, -8.72f), new(20.53f, -3.90f), new(20.50f, 0.92f), new(20.47f, 5.50f), new(20.47f, 10.69f), new(20.47f, 15.91f), new(20.44f, 20.15f)
    ];

    public static readonly List<WDir> Spiral = [
        new WDir(3.95f, -26.35f), new WDir(8.59f, -25.40f), new WDir(12.95f, -23.48f), new WDir(16.86f, -20.88f), new WDir(20.15f, -17.62f), new WDir(23.05f, -14.02f), new WDir(25.25f, -9.38f), new WDir(26.32f, -5.11f), new WDir(26.50f, -1.17f), new WDir(26.47f, 3.50f), new WDir(25.37f, 8.23f), new WDir(23.36f, 12.26f), new WDir(20.67f, 16.04f), new WDir(17.10f, 19.19f), new WDir(13.01f, 21.48f), new WDir(8.10f, 23.19f), new WDir(3.31f, 23.98f), new WDir(-1.39f, 23.43f), new WDir(-5.63f, 21.35f), new WDir(-10.15f, 18.73f), new WDir(-13.20f, 14.98f), new WDir(-15.46f, 10.89f), new WDir(-16.93f, 6.31f), new WDir(-16.62f, 1.52f), new WDir(-15.09f, -3.00f), new WDir(-11.95f, -6.26f), new WDir(-7.40f, -8.80f), new WDir(-3.47f, -10.02f), new WDir(1.14f, -8.80f), new WDir(4.41f, -5.75f), new WDir(5.90f, -1.99f), new WDir(4.35f, 1.76f), new WDir(0.50f, 0.21f)
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes.Take(16);

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
            foreach (var z in path)
            {
                aoes.Add(new AOEInstance(new AOEShapeCircle(13), Arena.Center + z.Rotate(NextPathRotation), default, finish));
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
                NextPathRotation = actor.Rotation;
                break;
            case OID.PathZigzag:
                NextPath = Path.Zigzag;
                NextPathRotation = actor.Rotation;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpiralScourge)
        {
            aoes.RemoveAt(0);
            if (aoes.Count == 0)
            {
                NextPath = Path.None;
                NextPathRotation = default;
            }
        }
    }
}

class MagitekHalo(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.MagitekHalo), new AOEShapeDonut(9, 60));
class MagitekCrossray(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.MagitekCrossray), new AOEShapeCross(60, 9.5f));
class SurfaceMissile(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SurfaceMissile), 6);
class AntiPersonnelMissile(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.AntiPersonnelMissile), 6);
class MissileSalvo(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.MissileSalvo), 6);

class MobileCrossray(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.MobileCrossray))
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

class MobileHalo(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.MobileHalo))
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
class HighPoweredMagitekRay(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.HighPoweredMagitekRay), new AOEShapeCircle(5), centerAtTarget: true, endsOnCastEvent: true);

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

class SwoopingFrenzy(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SwoopingFrenzy), 12);
class FrigidPulse(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.FrigidPulse), new AOEShapeDonut(12, 180));

class Obey(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes.Take(1);

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

class Pentagust(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.Pentagust1), new AOEShapeCone(50, 10.Degrees()));
class ToothAndTalon(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ToothAndTalon));
class FireBrand(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID.FireBrand1), new AOEShapeCross(50, 7));

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

