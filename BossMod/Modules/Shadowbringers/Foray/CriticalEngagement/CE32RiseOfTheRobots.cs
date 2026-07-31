namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE32RiseOfTheRobots;

public enum OID : uint
{
    Boss = 0x2E8C, // R4.200, x1
    MathTowers = 0x1EB036, // R0.5
    Deathwall = 0x1EB032, // R0.5
    DeathwallHelper = 0x2EE8, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    ElectricEntrapment = 20688, // DeathwallHelper->self, no cast, range 18-30 donut

    BootCampMode = 20671, // Boss->self, 5.0s cast, single-target

    BasicTrainingFFirst = 20672, // Boss->self, 8.0s cast, range 100 60-degree cone, front
    BasicTrainingBFirst = 20674, // Helper->self, 8.0s cast, range 100 60-degree cone, back
    BasicTrainingBRest = 20675, // Helper->self, no cast, range 100 60-degree cone, back
    BasicTrainingFRest = 20673, // Boss->self, no cast, range 100 60-degree cone, front
    AnnihilationMode = 20682, // Boss->self, 5.0s cast, single-target
    TrainAnnhilationModeBFirst = 20685, // Helper->self, 8.0s cast, range 100 60-degree cone, back
    TrainAnnhilationModeFFirst = 20683, // Boss->self, 8.0s cast, range 100 60-degree cone, front
    TrainAnnhilationModeBRest = 20686, // Helper->self, no cast, range 100 60-degree cone, back
    TrainAnnhilationModeFRest = 20684, // Boss->self, no cast, range 100 60-degree cone, front

    Order = 20676, // Boss->self, 5.0s cast, range 100 circle
    Indivisible = 20680, // Boss->self, 16.0s cast, range 100 circle
    DivideByThree = 20677, // Boss->self, 16.0s cast, range 100 circle
    DivideByFour = 20678, // Boss->self, 16.0s cast, range 100 circle
    DivideByFive = 20679, // Boss->self, 16.0s cast, range 100 circle
    Incinerate1 = 20687, // Boss->self, 5.0s cast, range 100 circle
    Incinerate2 = 20681, // Boss->self, 10.0s cast, range 100 circle, 2+3 happen at the same time
    Incinerate3 = 21030 // Helper->self, 10.0s cast, range 100 circle
}

public enum IconID : uint
{
    RotateCW = 235, // Boss->self
    RotateCCW = 236 // Boss->self
}

public enum SID : uint
{
    LeftFace = 1295, // none->player, extra=0x0
    ForwardMarch = 1293, // none->player, extra=0x0
    AboutFace = 1294, // none->player, extra=0x0
    RightFace = 1296, // none->player, extra=0x0
    ForcedMarch = 1257 // none->player, extra=0x1/0x4/0x2/0x8
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BootCampMode)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 4.3d))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Deathwall)
        {
            Arena.Bounds = new ArenaBoundsCircle(25f); // default arena got no extra collision, just a donut aoe
            Arena.Center = Arena.Center.Quantized();
            _aoe = [];
        }
    }
}

sealed class Train(BossModule module) : Components.GenericRotatingAOE(module)
{
    private Angle _increment;
    private DateTime _activation;
    private int casts;
    private readonly List<Angle> _rotation = [with(2)];

    private static readonly AOEShapeCone _shape = new(100f, 30f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        _increment = iconID switch
        {
            (uint)IconID.RotateCW => -60f.Degrees(),
            (uint)IconID.RotateCCW => 60f.Degrees(),
            _ => default
        };
        _activation = WorldState.FutureTime(8d);
        InitIfReady();
    }

    private void InitIfReady()
    {
        if (_rotation.Count == 2 && _increment != default)
        {
            var pos = Arena.Center;
            for (var i = 0; i < 2; ++i)
            {
                Sequences.Add(new(_shape, pos, _rotation[i], _increment, _activation, 1.2d, casts));
            }
            _rotation.Clear();
            _increment = default;
            casts = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BasicTrainingBFirst:
            case (uint)AID.BasicTrainingFFirst:
            case (uint)AID.TrainAnnhilationModeFFirst:
            case (uint)AID.TrainAnnhilationModeBFirst:
                _rotation.Add(spell.Rotation);
                casts = spell.Action.ID >= (uint)AID.TrainAnnhilationModeFFirst ? 12 : 6;
                InitIfReady();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BasicTrainingFFirst:
            case (uint)AID.BasicTrainingFRest:
            case (uint)AID.TrainAnnhilationModeFFirst:
            case (uint)AID.TrainAnnhilationModeFRest:
                var count = Sequences.Count - 1;
                var time = WorldState.CurrentTime;
                for (var i = count; i >= 0; --i)
                {
                    AdvanceSequence(i, time);
                }
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Sequences.Count != 0)
        {
            base.AddAIHints(slot, actor, assignment, hints);
            var sequences = CollectionsMarshal.AsSpan(Sequences);
            ref readonly var sequence = ref sequences[0];
            hints.AddForbiddenZone(new SDInvertedCircle(CE32RiseOfTheRobots.ArenaCenter, 3f), sequence.NextActivation);
        }
    }
}

sealed class OrderTowers(BossModule module) : Components.GenericAOEs(module)
{
    public readonly AOEInstance[][] AOEs = new AOEInstance[8][];
    public static readonly WPos[] TowerPositions = GetTowerPositions();
    public List<uint>[] Numbers = new List<uint>[8];

    private static WPos[] GetTowerPositions()
    {
        var positions = new WPos[8];
        var north = new WDir(default, -20f);
        var angle = -90f.Degrees();
        var center = CE32RiseOfTheRobots.ArenaCenter;
        for (var i = 0; i < 4; ++i)
        {
            positions[i] = north.Rotate(angle * i) + center;
        }
        return positions;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (slot > 7 || AOEs[slot] == default) // no support for NPCs that might be around
        {
            return [];
        }
        return AOEs[slot];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var divisor = spell.Action.ID switch
        {
            (uint)AID.DivideByThree => 3u,
            (uint)AID.DivideByFour => 4u,
            (uint)AID.DivideByFive => 5u,
            (uint)AID.Indivisible => 0u,
            _ => 6u
        };
        if (divisor < 6u)
        {
            for (var i = 0; i < 8; ++i)
            {
                Numbers[i] = [];
            }

            var party = Module.Raid.WithSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                var maxHP = p.Item2.HPMP.MaxHP;
                if (maxHP > 9) // ignore players not taking part in the mechanic
                {
                    continue;
                }
                var outsideSafe = false;
                var shapes = new List<Polygon>();

                for (var j = 0u; j < 5u; ++j)
                {
                    var isDivisible = divisor == default ? (maxHP + j).IsPrime() : (maxHP + j).IsDivisible(divisor);
                    if (!outsideSafe && isDivisible || outsideSafe && !isDivisible)
                    {
                        if (j == 0u)
                        {
                            outsideSafe = true;
                            Numbers[p.Item1].Add(j);
                            continue;
                        }
                        Numbers[p.Item1].Add(j);
                        shapes.Add(new Polygon(TowerPositions[j - 1], 5f, 20));
                    }
                }
                AOEs[p.Item1] = [new(new AOEShapeCustom(shapes, invertForbiddenZone: !outsideSafe), Arena.Center, default, Module.CastFinishAt(spell), !outsideSafe ? Colors.SafeFromAOE : default)];
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Indivisible:
            case (uint)AID.DivideByThree:
            case (uint)AID.DivideByFour:
            case (uint)AID.DivideByFive:
                Array.Clear(AOEs);
                Array.Clear(Numbers);
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot > 7)
        {
            return;
        }
        ref readonly var aoes = ref AOEs[slot];
        if (aoes != default)
        {
            ref readonly var aoe = ref aoes[0];
            var isInside = aoe.Check(actor.Position);
            hints.Add(Numbers[slot][0] == default ? ("Avoid marked towers!", isInside) : ("Move into a marked tower!", !isInside));
        }
    }
}

sealed class OrderForcedMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    private readonly OrderTowers _math = module.FindComponent<OrderTowers>()!;
    private static readonly Random random = new();
    private readonly float randomOdd = random.Next(1, 51) * 2 - 1; // used as pseudo randomisation for default case
    private static readonly Angle a175 = 175f.Degrees(), a45 = 45f.Degrees(), am90 = -90f.Degrees(), a225 = 22.5f.Degrees(), a180 = 180f.Degrees();

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        ref readonly var aoes = ref _math.AOEs[slot];
        if (aoes != default)
        {
            ref readonly var aoe = ref aoes[0];
            var isInside = aoe.Check(pos);
            return _math.Numbers[slot][0] == default ? isInside : !isInside;
        }
        return !Arena.InBounds(pos);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
            return;
        if (_math.Numbers[slot] is var num && num != default)
        {
            var move0 = state.PendingMoves[0];
            var act = move0.activation;
            var isDefault = num[0] == default;
            var angleToTower = isDefault ? default : (num[0] - 1) * am90;
            hints.AddForbiddenZone(isDefault ? new SDInvertedCone(CE32RiseOfTheRobots.ArenaCenter, 7f, a45 * randomOdd, a225) : new SDInvertedCone(CE32RiseOfTheRobots.ArenaCenter, 7f, a180 + angleToTower, a225), act);
            hints.ForbiddenDirections.Add(isDefault ? (a45 * randomOdd, a175, act) : (angleToTower - move0.dir, a175, act));
        }
    }
}

sealed class Incinerate(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Incinerate1, (uint)AID.Incinerate2]);

sealed class CE32RiseOfTheRobotsStates : StateMachineBuilder
{
    public CE32RiseOfTheRobotsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<OrderTowers>()
            .ActivateOnEnter<OrderForcedMarch>()
            .ActivateOnEnter<Train>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 735, NameID = 14)]
public sealed class CE32RiseOfTheRobots(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, startingArena)
{
    public static readonly WPos ArenaCenter = new(104f, 237f);
    private static readonly ArenaBoundsCustom startingArena = new([new Polygon(ArenaCenter, 29.5f, 32)]);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
