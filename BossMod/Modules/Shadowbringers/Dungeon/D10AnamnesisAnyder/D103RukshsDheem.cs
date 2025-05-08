namespace BossMod.Shadowbringers.Dungeon.D10AnamnesisAnyder.D103RukshsDheem;

public enum OID : uint
{
    Boss = 0x2CFF, // R4.0
    QueensHarpooner = 0x2D01, // R1.56
    DepthGrip = 0x2D00, // R5.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/QueensHarpooner->player, no cast, single-target
    SwiftShift = 19331, // Boss->location, no cast, single-target, teleport

    Bonebreaker = 19340, // Boss->player, 4.0s cast, single-target, tankbuster

    SeabedCeremonyVisual = 19323, // Boss->self, 4.0s cast, single-target
    SeabedCeremony = 19324, // Helper->self, 4.0s cast, range 60 circle

    DepthGrip = 19332, // Boss->self, 4.0s cast, single-target
    Arise = 19333, // DepthGrip->self, no cast, single-target
    WavebreakerVisual1 = 19334, // DepthGrip->self, no cast, single-target
    WavebreakerVisual2 = 19335, // DepthGrip->self, no cast, single-target
    Wavebreaker1 = 13268, // Helper->self, no cast, range 36 width 8 rect
    Wavebreaker2 = 13269, // Helper->self, no cast, range 21 width 10 rect

    FallingWaterVisual = 19325, // Boss->self, 5.0s cast, single-target, spread
    FallingWater = 19326, // Helper->player, 5.0s cast, range 8 circle

    RisingTide = 19339, // Boss->self, 3.0s cast, range 50 width 6 cross

    Meatshield = 19338, // QueensHarpooner->Boss, no cast, single-target
    CoralTrident = 19337, // QueensHarpooner->self, 5.0s cast, range 6 90-degree cone
    Seafoam = 19336, // QueensHarpooner->self, 7.0s cast, range 60 circle

    FlyingFountVisual = 19327, // Boss->self, 5.0s cast, single-target, stack
    FlyingFount = 19328, // Helper->player, 5.0s cast, range 6 circle

    CommandCurrentVisual = 19329, // Boss->self, 4.9s cast, single-target
    CommandCurrent = 19330 // Helper->self, 5.0s cast, range 40 30-degree cone
}

public enum SID : uint
{
    MeatShield = 2267
}

class SeabedCeremony(BossModule module) : Components.RaidwideCast(module, AID.SeabedCeremony);
class Seafoam(BossModule module) : Components.RaidwideCast(module, AID.Seafoam);
class Bonebreaker(BossModule module) : Components.SingleTargetDelayableCast(module, AID.Bonebreaker);
class FallingWater(BossModule module) : Components.SpreadFromCastTargets(module, AID.FallingWater, 8);
class FlyingFount(BossModule module) : Components.StackWithCastTargets(module, AID.FlyingFount, 6);
class CommandCurrent(BossModule module) : Components.StandardAOEs(module, AID.CommandCurrent, new AOEShapeCone(40, 15.Degrees()));
class CoralTrident(BossModule module) : Components.StandardAOEs(module, AID.CoralTrident, new AOEShapeCone(6, 45.Degrees()));
class RisingTide(BossModule module) : Components.StandardAOEs(module, AID.RisingTide, new AOEShapeCross(50, 3));

class Voidzones(BossModule module) : BossComponent(module)
{
    public enum Configuration
    {
        Default,
        Split,
        Narrow
    }

    public Configuration Current { get; private set; }

    public override void OnEventEnvControl(byte index, uint state)
    {
        bool activate;
        if (state == 0x00020001)
            activate = true;
        else if (state == 0x00080004)
            activate = false;
        else
            return;

        if (index == 0x17)
            Current = activate ? Configuration.Narrow : Configuration.Default;
        else if (index == 0x18)
            Current = activate ? Configuration.Split : Configuration.Default;

        Arena.Bounds = Current switch
        {
            Configuration.Split => RukshsDheem.SplitBounds,
            Configuration.Narrow => RukshsDheem.NarrowBounds,
            _ => RukshsDheem.DefaultBounds,
        };
    }
}

class Wavebreaker(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<List<AOEInstance>> aoes = [];
    private Voidzones? vz;
    private bool drawExas;

    public List<Actor> Casters = [];

    private static readonly AOEShapeRect rectNarrow = new(36, 4);
    private static readonly AOEShapeRect rectWide = new(21, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (aoes.Count == 0)
            yield break;

        if (drawExas)
        {
            var highlight = true;
            foreach (var aoe in aoes[0])
            {
                yield return aoe with { Color = highlight ? ArenaColor.Danger : aoe.Color };
                highlight = false;
            }
        }
        else
            foreach (var aoe in aoes[0])
                yield return aoe;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Arise:
                Casters.Add(caster);
                AddAOEs(caster);
                break;
            case AID.Wavebreaker1:
            case AID.Wavebreaker2:
                Casters.RemoveAt(0);
                if (aoes.Count > 0)
                {
                    aoes[0].RemoveAt(0);
                    if (aoes[0].Count == 0)
                        aoes.RemoveAt(0);
                }
                break;
        }
    }

    private void AddAOEs(Actor caster)
    {
        vz ??= Module.FindComponent<Voidzones>();
        if (vz == null)
            return;

        if (aoes.Count == 0)
            aoes.Add([]);
        if (aoes[0].Count > 3)
            aoes.Add([]);

        drawExas = false;

        DateTime activation;
        var shape = rectNarrow;
        if (vz.Current == Voidzones.Configuration.Narrow)
        {
            activation = WorldState.FutureTime(aoes[0].Count > 3 ? 11.6f : 8);
            drawExas = true;
        }
        else if (vz.Current == Voidzones.Configuration.Split)
            activation = WorldState.FutureTime(7.6f);
        else
        {
            activation = WorldState.FutureTime(9.8f);
            shape = rectWide;
        }
        var toAdd = aoes[0].Count > 3 ? aoes[1] : aoes[0];
        toAdd.Add(new(shape, caster.Position, caster.Rotation, activation));
    }
}

class Drains(BossModule module) : BossComponent(module)
{
    enum DrainState
    {
        Inactive,
        Covered,
        Active
    }
    private readonly DrainState[] DrainStates = Utils.MakeArray(8, DrainState.Inactive);
    private static readonly WPos[] Positions = new WPos[8];
    private int NumActiveDrains => DrainStates.Count(d => d != DrainState.Inactive);
    private DateTime activation;

    private Wavebreaker? wavebreaker;

    static Drains()
    {
        int[] xPositions = [-11, 11];
        var zStart = -465;
        var zStep = 10;
        var index = 0;
        for (var i = 0; i < 2; i++)
            for (var j = 0; j < 4; j++)
                Positions[index++] = new(xPositions[i], zStart + j * zStep);
    }

    private void Reset()
    {
        activation = default;
        Array.Fill(DrainStates, DrainState.Inactive);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is not (>= 0x0F and <= 0x16))
            return;

        var ix = index - 0x0F;
        var prev = DrainStates[ix];
        DrainStates[ix] = state switch
        {
            0x00020001 => DrainState.Active,
            0x00080004 => DrainState.Covered,
            _ => DrainState.Inactive,
        };
        if (prev == DrainState.Inactive)
            activation = WorldState.FutureTime(NumActiveDrains > 4 ? 16 : 12);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SeabedCeremony)
            Reset();
    }

    private IEnumerable<(WPos Position, bool Blocked)> GetActiveDrains()
    {
        wavebreaker ??= Module.FindComponent<Wavebreaker>();

        List<Actor> handBlockers = [];
        if (NumActiveDrains > 4)
        {
            handBlockers = wavebreaker?.Casters ?? [];
            if (handBlockers.Count == 0)
                yield break;
        }

        for (var i = 0; i < 8; i++)
        {
            var st = DrainStates[i];
            if (st == DrainState.Inactive)
                continue;

            var pos = Positions[i];

            if (handBlockers.Any(b => pos.InRect(b.Position, b.Rotation, 21, 0, 5)))
                continue;

            yield return (pos, st == DrainState.Covered);
        }
    }

    private IEnumerable<WPos> GetUnblockedDrains() => GetActiveDrains().Where(d => !d.Blocked).Select(d => d.Position);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (pos, blocked) in GetActiveDrains())
            Arena.AddRect(pos, new WDir(1, 0), 1.25f, 1.25f, 1.25f, blocked ? ArenaColor.PlayerGeneric : ArenaColor.Safe);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (GetUnblockedDrains().Any())
            hints.Add("Block drains!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        List<Func<WPos, bool>> zones = [];
        foreach (var drain in GetActiveDrains())
        {
            bool inDrain(WPos p) => p.AlmostEqual(drain.Position, 1.25f);
            var numBlockers = Raid.WithoutSlot().Count(p => inDrain(p.Position));
            if (numBlockers == 0 || numBlockers == 1 && inDrain(actor.Position))
                zones.Add(ShapeContains.Rect(drain.Position, default(Angle), 1.25f, 1.25f, 1.25f));
        }
        if (zones.Count == 0)
            return;

        var zunion = ShapeContains.Union(zones);
        hints.AddForbiddenZone(p => !zunion(p), activation);
    }
}

class RukshsDheemStates : StateMachineBuilder
{
    public RukshsDheemStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SeabedCeremony>()
            .ActivateOnEnter<Seafoam>()
            .ActivateOnEnter<Bonebreaker>()
            .ActivateOnEnter<FallingWater>()
            .ActivateOnEnter<FlyingFount>()
            .ActivateOnEnter<CommandCurrent>()
            .ActivateOnEnter<CoralTrident>()
            .ActivateOnEnter<RisingTide>()
            .ActivateOnEnter<Voidzones>()
            .ActivateOnEnter<Wavebreaker>()
            .ActivateOnEnter<Drains>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 714, NameID = 9264)]
public class RukshsDheem(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -450), DefaultBounds)
{
    private static ArenaBoundsCustom MakeSplitBounds()
    {
        var rect1 = CurveApprox.Rect(new WDir(0, -1), 15.5f, 19.5f);
        var split = CurveApprox.Rect(new WDir(0, -1), 20, 4);
        var clipper = new PolygonClipper();
        var output = clipper.Difference(new(rect1), new(split));
        return new ArenaBoundsCustom(19.5f, output);
    }

    private const float X = 15.5f;
    private const float Z = 19.5f;
    public static readonly ArenaBoundsRect DefaultBounds = new(X, Z);
    public static readonly ArenaBoundsRect NarrowBounds = new(X - 7.5f, Z);
    public static readonly ArenaBoundsCustom SplitBounds = MakeSplitBounds();

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.QueensHarpooner), ArenaColor.Enemy);
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID.MeatShield) == null ? 1 : 0;
    }
}
