namespace BossMod.Shadowbringers.Foray.CLL.CLL2Brionac;

public enum OID : uint
{
    Boss = 0x2ECC, // R20.000, x1
    Helper = 0x233C, // R0.500, x36, Helper type
    Brionac = 0x2ED6, // R0.500, x1
    MagitekCore = 0x2F9A, // R10.000, x0 (spawn during fight), Part type
    Lightsphere = 0x2ECD, // R1.000, x0 (spawn during fight)
    Shadowsphere = 0x2ECE, // R1.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 21446, // Boss->player, no cast, single-target
    ElectricAnvilBoss = 20956, // Boss->self, 4.0s cast, single-target
    ElectricAnvil = 20957, // Helper->player, 5.0s cast, single-target
    FalseThunder1 = 20942, // Boss->self, 8.0s cast, range 47 ?-degree cone
    FalseThunder2 = 20943, // Boss->self, 8.0s cast, range 47 ?-degree cone
    AntiWarmachinaWeaponry = 20941, // Boss->self, 5.0s cast, single-target
    MagitekThunder = 20993, // Brionac->2ED7, no cast, single-target
    LightningShowerBoss = 21444, // Boss->self, 4.0s cast, single-target
    LightningShower = 21445, // Helper->self, 5.0s cast, range 60 circle
    EnergyGeneration = 20944, // Boss->self, 3.0s cast, single-target
    Lightburst = 20945, // Lightsphere->self, 2.0s cast, range 5-20 donut
    InfraredBlast = 20974, // Helper->player, no cast, single-target
    ShadowBurst = 20946, // Shadowsphere->self, 2.0s cast, range 12 circle
    VoltstreamBoss = 20954, // Boss->self, 3.0s cast, single-target
    Voltstream = 20955, // Helper->self, 6.0s cast, range 40 width 10 rect
    PoleShiftBoss = 20947, // Boss->self, 8.0s cast, single-target
    PoleShift = 20948, // Helper->Lightsphere/Shadowsphere, no cast, single-target

    MagitekMagnetism = 20949, // Boss->self, 6.0s cast, single-target
    MagneticJolt = 20951, // Helper->self, no cast, ???
    PolarMagnetism = 20953, // Boss->self, 6.0s cast, single-target

    PolarMagnetismAttract = 20950, // Helper->self, no cast, ???
    PolarMagnetismKnockback = 20952, // Helper->self, no cast, ???
}

public enum TetherID : uint
{
    PoleShift = 21, // Shadowsphere->Lightsphere
    PolarMagnetism = 124, // player->Lightsphere
}

public enum IconID : uint
{
    MagnetPlus = 231, // player->self
    MagnetMinus = 232, // player->self
    BallPlus = 162,
    BallMinus = 163,
}

class ElectricAnvil(BossModule module) : Components.SingleTargetCast(module, AID.ElectricAnvil);
class FalseThunder(BossModule module) : Components.GroupedAOEs(module, [AID.FalseThunder2, AID.FalseThunder1], new AOEShapeCone(47, 65.Degrees()));
class LightningShower(BossModule module) : Components.RaidwideCast(module, AID.LightningShower);

class MagnetTethers(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private readonly Actor?[] tetheredTo = new Actor?[PartyState.MaxPartySize];
    private readonly DateTime[] activations = new DateTime[PartyState.MaxPartySize];

    enum Charge
    {
        None,
        Plus,
        Minus
    }

    private readonly Dictionary<ulong, WPos> swaps = [];
    private readonly Dictionary<ulong, Charge> charges = [];

    private Balls? balls;

    public override void Update()
    {
        balls ??= Module.FindComponent<Balls>();
    }

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (slot <= PartyState.MaxPartySize && tetheredTo[slot] is { } src)
        {
            var c1 = GetCharge(actor);
            var c2 = GetCharge(src);
            if (c1 == Charge.None || c2 == Charge.None)
                yield break;

            yield return new(ActualSource(src), 30, Activation: activations[slot], Kind: c1 == c2 ? Kind.AwayFromOrigin : Kind.TowardsOrigin);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => balls?.ActiveAOEs(slot, actor).Any(a => a.Shape.Check(pos, a.Origin, a.Rotation)) == true;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        for (var slot = 0; slot < PartyState.MaxPartySize; slot++)
        {
            var actor = Raid[slot];
            if (actor == null)
                continue;

            if (tetheredTo[slot] is { } src)
                Arena.AddLine(actor.Position, ActualSource(src), ArenaColor.PlayerGeneric);
        }
    }

    private WPos ActualSource(Actor s) => swaps.TryGetValue(s.InstanceID, out var p) ? p : s.Position;
    private Charge GetCharge(Actor c) => charges.TryGetValue(c.InstanceID, out var c2) ? c2 : Charge.None;

    // delay 8.2f
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PolarMagnetism && Raid.TryFindSlot(source.InstanceID, out var slot) && WorldState.Actors.Find(tether.Target) is { } target)
        {
            tetheredTo[slot] = target;
            activations[slot] = WorldState.FutureTime(8.2f);
        }

        if (tether.ID == (uint)TetherID.PoleShift && WorldState.Actors.Find(tether.Target) is { } t)
        {
            swaps[source.InstanceID] = t.Position;
            swaps[t.InstanceID] = source.Position;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.MagnetPlus:
            case IconID.BallPlus:
                charges[actor.InstanceID] = Charge.Plus;
                break;
            case IconID.MagnetMinus:
            case IconID.BallMinus:
                charges[actor.InstanceID] = Charge.Minus;
                break;
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PolarMagnetism && Raid.TryFindSlot(source.InstanceID, out var slot))
            tetheredTo[slot] = null;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PolarMagnetismAttract or AID.PolarMagnetismKnockback)
        {
            Array.Fill(tetheredTo, null);
            charges.Clear();
            swaps.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
        {
            if (src.Kind == Kind.AwayFromOrigin && balls?.CurrentPattern == Balls.Pattern.DonutOut)
            {
                WDir dirToCenter = src.Origin.X < 80 ? new(4, 0) : new(-4, 0);
                hints.AddForbiddenZone(ShapeContains.InvertedRect(src.Origin, src.Origin + dirToCenter, 0.5f), src.Activation);
            }
            break;
        }
    }
}

class Balls(BossModule module) : Components.GenericAOEs(module)
{
    class Ball(Actor caster, DateTime creation, DateTime activation, WPos destination)
    {
        public Actor Caster = caster;
        public DateTime Creation = creation;
        public DateTime Activation = activation;
        public WPos Destination = destination;
    }

    public enum Pattern
    {
        None,
        DonutOut,
        DonutIn
    }

    public Pattern CurrentPattern { get; private set; }

    private readonly List<Ball> Casters = [];

    private static readonly AOEShape Circle = new AOEShapeCircle(12);
    private static readonly AOEShape Donut = new AOEShapeDonut(5, 20);

    public override void Update()
    {
        foreach (var c in Casters)
        {
            if (c.Destination.X < 65)
            {
                CurrentPattern = c.Caster.OID == (uint)OID.Lightsphere ? Pattern.DonutOut : Pattern.DonutIn;
                return;
            }
        }

        CurrentPattern = Pattern.None;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in Casters)
        {
            if ((b.Activation - WorldState.CurrentTime).TotalSeconds < 6)
                yield return new AOEInstance(b.Caster.OID == (uint)OID.Lightsphere ? Donut : Circle, b.Destination, Activation: b.Activation);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Lightsphere or OID.Shadowsphere)
            Casters.Add(new(actor, WorldState.CurrentTime, WorldState.FutureTime(10.7f), actor.Position));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Lightburst or AID.ShadowBurst)
        {
            if (Casters.Find(c => c.Caster == caster) is { } b)
                b.Activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Lightburst or AID.ShadowBurst)
            Casters.RemoveAll(c => c.Caster == caster);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 21 && WorldState.Actors.Find(tether.Target) is { } target)
        {
            foreach (var b in Casters)
            {
                if (b.Caster == source)
                    b.Destination = target.Position;
                else if (b.Caster == target)
                    b.Destination = source.Position;

                b.Activation = WorldState.FutureTime(12.1f);
            }
        }
    }
}

class MagitekCore(BossModule module) : Components.Adds(module, (uint)OID.MagitekCore, 1);
class Voltstream(BossModule module) : Components.StandardAOEs(module, AID.Voltstream, new AOEShapeRect(40, 5), maxCasts: 3);

class BrionacStates : StateMachineBuilder
{
    public BrionacStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ElectricAnvil>()
            .ActivateOnEnter<FalseThunder>()
            .ActivateOnEnter<LightningShower>()
            .ActivateOnEnter<Balls>()
            .ActivateOnEnter<MagitekCore>()
            .ActivateOnEnter<Voltstream>()
            .ActivateOnEnter<MagnetTethers>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 735, NameID = 9436)]
public class Brionac(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, -222), new ArenaBoundsRect(29.5f, 14.5f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat && WorldState.Party.Player() is { } player && Bounds.Contains(player.Position - Arena.Center);
}
