namespace BossMod.Shadowbringers.Dungeon.D12MatoyasRelict.D122Nixie;

public enum OID : uint
{
    Boss = 0x307F, // R2.4
    Icicle = 0x3081, // R1.0
    UnfinishedNixie = 0x3080, // R1.2
    Geyser = 0x1EB0C7,
    CloudPlatform = 0x1EA1A1, // R0.5s-2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 22932, // Boss->player, no cast, single-target
    Teleport = 22933, // Boss->location, no cast, single-target

    CrashSmash = 22927, // Boss->self, 3.0s cast, single-target
    CrackVisual = 23481, // Icicle->self, 5.0s cast, single-target
    Crack = 22928, // Icicle->self, no cast, range 80 width 3 rect

    ShowerPower = 22929, // Boss->self, 3.0s cast, single-target
    Gurgle = 22930, // Helper->self, no cast, range 60 width 10 rect

    PitterPatter = 22920, // Boss->self, 3.0s cast, single-target
    Sploosh = 22926, // Helper->self, no cast, range 6 circle, launches player
    FallDamage = 22934, // Helper->player, no cast, single-target, physical damage bonk when hitting ground from geyser

    SinginInTheRain = 22921, // UnfinishedNixie->self, 40.0s cast, single-target
    SeaShantyVisual = 22922, // Boss->self, no cast, single-target
    SeaShanty = 22924, // Helper->self, no cast, ???
    SeaShantyEnrage = 22923, // Helper->self, no cast, ???

    SplishSplash = 22925, // Boss->self, 3.0s cast, single-target
    Sputter = 22931 // Helper->player, 5.0s cast, range 6 circle, spread
}

public enum TetherID : uint
{
    Tankbuster = 8, // Icicle->player
    Gurgle = 3 // Boss->Helper
}

public enum SID : uint
{
    HeadInTheClouds = 2472, // none->player, extra=0x0
}

class Crack(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(80, 1.5f), (uint)TetherID.Tankbuster)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Crack)
            CurrentBaits.Clear();
    }
}

class Gurgle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    private static readonly (WPos, Angle)[] aoePositions = [
        (new(-20, -165), 90.Degrees()),
        (new(-20, -155), 90.Degrees()),
        (new(-20, -145), 90.Degrees()),
        (new(-20, -135), 90.Degrees()),
        (new(20, -165), -90.Degrees()),
        (new(20, -155), -90.Degrees()),
        (new(20, -145), -90.Degrees()),
        (new(20, -135), -90.Degrees()),
    ];

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001 && index is >= 0x13 and <= 0x1A)
        {
            var pos = aoePositions[index - 0x13];
            aoes.Add(new AOEInstance(new AOEShapeRect(60, 5), pos.Item1, pos.Item2, WorldState.FutureTime(9)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Gurgle)
            aoes.Clear();
    }
}

class Sputter(BossModule module) : Components.SpreadFromCastTargets(module, AID.Sputter, 6);

class SeaShanty(BossModule module) : Components.CastCounter(module, AID.SeaShanty);

class GeyserDanger(BossModule module) : Components.GenericAOEs(module, AID.Sploosh)
{
    public readonly List<(Actor, DateTime)> Geysers = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Geyser)
            Geysers.Add((actor, WorldState.FutureTime(4.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Geysers.RemoveAll(g => g.Item1.Position.AlmostEqual(caster.Position, 1));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Geysers.Select(g => new AOEInstance(new AOEShapeCircle(6), g.Item1.Position, Activation: g.Item2));
}

class Bounds(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x12)
        {
            if (state == 0x00020001)
                Arena.Bounds = Nixie.CombinedBounds;
            else
            {
                Arena.Center = Nixie.GroundCenter;
                Arena.Bounds = Nixie.DefaultBounds;
            }
        }
    }
}

class GeyserSafe(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> Geysers = [];
    private DateTime SplooshTime;
    private BitMask floaters;

    private Actor? BestGeyser => Geysers.Count == 0 ? null : Geysers.MinBy(g => g.Position.Z);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Geyser)
        {
            Geysers.Add(actor);
            if (SplooshTime == default)
                SplooshTime = WorldState.FutureTime(4.8f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Sploosh)
            Geysers.RemoveAll(g => g.Position.AlmostEqual(caster.Position, 1));
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.HeadInTheClouds)
        {
            floaters.Set(Raid.FindSlot(actor.InstanceID));
            if (floaters.NumSetBits() == Raid.WithoutSlot().Count())
            {
                Arena.Center = Nixie.CloudCenter;
                Arena.Bounds = Nixie.CloudBounds;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (floaters[pcSlot])
            Arena.ZoneRect(Nixie.GroundCenter, default(Angle), 19.5f, 19.5f, 19.5f, ArenaColor.AOE);
        else if (BestGeyser is Actor g)
            Arena.ZoneCircle(g.Position, 6, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (floaters[slot])
            return;

        if (BestGeyser is Actor g)
            hints.Add("Go to geyser!", !actor.Position.InCircle(g.Position, 6));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (floaters[slot])
            hints.AddForbiddenZone(new AOEShapeRect(19.5f, 19.5f, 19.5f), Nixie.GroundCenter);
        else if (BestGeyser is Actor g)
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(g.Position, 6), SplooshTime);
    }
}

class NixieStates : StateMachineBuilder
{
    public NixieStates(BossModule module) : base(module)
    {
        DeathPhase(0, SimplePhase);
    }

    private void SimplePhase(uint id)
    {
        Targetable(id, false, 37.4f, "Boss disappears")
            .ActivateOnEnter<GeyserSafe>()
            .ActivateOnEnter<Crack>()
            .ActivateOnEnter<Sputter>()
            .ActivateOnEnter<Gurgle>()
            .ActivateOnEnter<Bounds>()
            .ActivateOnEnter<SeaShanty>()
            .ClearHint(StateMachine.StateHint.DowntimeStart); // not worth the hassle, DowntimeIn would be 0 until the boss becomes targetable again, but we should be attacking adds asap

        ComponentCondition<SeaShanty>(id + 0x10, 51, t => t.NumCasts > 0, "Death zone activate")
            .DeactivateOnExit<GeyserSafe>();

        Targetable(id + 0x20, true, 12.1f, "Boss reappears");

        Timeout(id + 0x30, 9999, "Enrage")
            .ActivateOnEnter<GeyserDanger>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 746, NameID = 9738)]
public class Nixie(WorldState ws, Actor primary) : BossModule(ws, primary, GroundCenter, DefaultBounds)
{
    private static ArenaBoundsCustom MakeCloud()
    {
        var clipper = new PolygonClipper();
        var ground = new PolygonClipper.Operand(CurveApprox.Rect(new(19.5f, 0), new(0, 19.5f)));
        var cloud = new PolygonClipper.Operand(CurveApprox.Rect(new(9.5f, 0), new(0, 5.5f)).Select(d => d - new WDir(0, 25)));
        return new ArenaBoundsCustom(25f, clipper.Union(ground, cloud));
    }

    public static readonly ArenaBoundsCustom DefaultBounds = new(25f, new(CurveApprox.Rect(new(19.5f, 0), new(0, 19.5f))));
    public static readonly ArenaBoundsRect CloudBounds = new(9.5f, 5.5f);
    public static readonly ArenaBoundsCustom CombinedBounds = MakeCloud();

    public static readonly WPos CloudCenter = new(0, -175);
    public static readonly WPos GroundCenter = new(0, -150);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.UnfinishedNixie), ArenaColor.Enemy);
    }
}
