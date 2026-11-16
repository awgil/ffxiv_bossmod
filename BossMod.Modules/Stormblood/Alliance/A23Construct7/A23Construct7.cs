namespace BossMod.Stormblood.Alliance.A23Construct7;

public enum OID : uint
{
    Boss = 0x225D, // R4.200, x1
    Helper = 0x18D6, // R0.500, x1-24
    LooseCog = 0x225E, // R8.700, x0 (spawn during fight)
    UnkB84 = 0xB84, // R0.500, x0 (spawn during fight)
    Missile = 0x225F, // R1.300, x0 (spawn during fight)
    Construct711 = 0x22A2, // R3.150, x0 (spawn during fight)
    Construct712 = 0x22A3, // R3.150, x0 (spawn during fight)
    Construct713 = 0x2260, // R3.150, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Destroy = 11354, // Boss->player, 5.0s cast, single-target
    IgniteSpread = 11367, // 18D6/player->players, no cast, range 10 circle
    Accelerate = 11365, // Boss->players, 8.0s cast, range 6 circle
    PulverizeBoss = 11361, // Boss->self, 7.0s cast, range 15 circle
    PulverizeBaited = 11362, // 18D6/player->location, 4.0s cast, range 12 circle
    Compress = 11356, // Boss->self, 2.5s cast, range 100+R width 7 rect
    Triboelectricity = 11373, // 225E->self, no cast, range 8 circle
    IgniteRect = 11366, // Boss->self, 3.0s cast, range 100+R width 20 rect
    LithobrakeVisual = 11368, // Boss->self, 5.0s cast, range 100 circle, "raidwide", doesn't apply any actioneffects
    Lithobrake = 11550, // 18D6->self, 5.0s cast, range 100 circle, proximity
    DisposeFirst = 11359, // Boss->self, 7.0s cast, range 100+R 60-degree cone
    DisposeRest = 11360, // Boss->self, no cast, range 100+R 60-degree cone
    IncinerateFast = 11364, // Boss->self, 5.0s cast, range 100 circle
    ComputationMode = 11351, // Boss->self, 5.0s cast, single-target
    Subtract = 11372, // Boss->self, 5.0s cast, range 100 circle
    DivideByThree = 11466, // Boss->self, 10.0s cast, range 100 circle
    DivideByFour = 11468, // Boss->self, 10.0s cast, range 100 circle
    DivideByFive = 11469, // Boss->self, 10.0s cast, range 100 circle
    Indivisible = 11470, // Boss->self, 10.0s cast, range 100 circle
    IncinerateSlow = 11464, // Boss->self, 10.0s cast, range 100 circle
    TartarusMode = 11352, // Boss->self, 5.0s cast, single-target
    Tartarus = 11363, // Boss->18D6, no cast, range 100 circle
    AccelerationBomb = 11556, // 22A3->self, 6.0s cast, single-target
    Ferrofluid = 11555, // 2260->self, 6.0s cast, single-target
    BallisticMissile = 11442, // 22A2->self, 6.0s cast, single-target
    Magnetism = 11557, // _Gen_Construct7->self, no cast, range 100 circle
    Explosion = 11375, // 225F->self, no cast, range 3 circle
    AnnihilationMode = 11353, // Boss->self, 10.0s cast, single-target
    EmpoweredDestroy = 11377, // Boss->player, 5.0s cast, single-target
    EmpoweredCompress = 11357, // Boss->self, 2.5s cast, range 100 width 7 cross
    EmpoweredIncinerateFast = 11612, // Boss->self, 5.0s cast, range 100 circle
    EmpoweredIncinerateSlow = 11613, // Boss->self, 10.0s cast, range 100 circle
}

public enum SID : uint
{
    Hover = 1515, // none->_Gen_LooseCog, extra=0x1F4
    ComputationMode = 1562, // Boss->_Gen_Construct7/Boss/player, extra=0x0
    HPPenalty = 1557, // Boss->player, extra=0x9
    HPBoost1 = 1558, // none->player, extra=0x0
    HPBoost2 = 1559, // none->player, extra=0x0
    HPBoost3 = 1560, // none->player, extra=0x0
    HPBoost4 = 1561, // none->player, extra=0x0
    ComputationBoost = 1565, // none->player, extra=0x1/0x2/0x3/0x4
    ComputationError = 1566, // none->player, extra=0x1/0x2
    TartarusMode = 1563, // Boss->Boss/_Gen_Construct7, extra=0x0
    DownForTheCount = 783, // Boss->player, extra=0xEC7
    Invincibility = 1570, // none->player, extra=0x0
    PositiveCharge = 698, // none->player, extra=0x0
    NegativeCharge = 699, // none->player, extra=0x0
    Stun = 149, // none->player, extra=0x0
    AccelerationBomb = 1384, // none->player, extra=0x0
    AnnihilationMode = 1564, // Boss->player/Boss, extra=0x0
    Minimum = 438, // Boss->player, extra=0xF
}

public enum IconID : uint
{
    Tankbuster = 218, // player->self
    Spread = 138, // player->self
    Stack = 100, // player->self
    Checkmark = 136, // player->self
    X = 137, // player->self
    Positive = 48, // player->self
    Negative = 49, // player->self
    AccelerationBomb = 75, // player->self
}

public enum TetherID : uint
{
    Magnet = 38, // player->player
}

class Destroy(BossModule module) : Components.SingleTargetCast(module, AID.Destroy);
class Ignite(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, AID.IgniteSpread, 10, 5.1f);
class Accelerate(BossModule module) : Components.StackWithCastTargets(module, AID.Accelerate, 6);
class Pulverize1(BossModule module) : Components.StandardAOEs(module, AID.PulverizeBoss, 15);
class Pulverize2(BossModule module) : Components.StandardAOEs(module, AID.PulverizeBaited, 12);
class Compress(BossModule module) : Components.StandardAOEs(module, AID.Compress, new AOEShapeRect(100, 3.5f));
class IgnitePredict(BossModule module) : Components.GenericAOEs(module, AID.IgniteRect)
{
    AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Boss && id == 0x1E43)
            _predicted = new(new AOEShapeRect(100, 10), actor.Position, actor.Rotation, WorldState.FutureTime(5.6f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _predicted = null;
    }
}
class Ignite2(BossModule module) : Components.StandardAOEs(module, AID.IgniteRect, new AOEShapeRect(100, 10));
class Lithobrake(BossModule module) : Components.ProximityAOEs(module, AID.Lithobrake, 24);
class Triboelectricity(BossModule module) : Components.GenericAOEs(module, AID.Triboelectricity)
{
    readonly List<(Actor Caster, int NumLeft, DateTime Next)> Cogs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Cogs.Select(c => new AOEInstance(new AOEShapeCircle(8), c.Caster.Position, default, c.Next));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.LooseCog)
            Cogs.Add((actor, 4, WorldState.FutureTime(5)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            var ix = Cogs.FindIndex(c => c.Caster == caster);
            if (ix >= 0)
            {
                if (Cogs[ix].NumLeft == 1)
                    Cogs.RemoveAt(ix);
                else
                    Cogs.Ref(ix).NumLeft--;
            }
        }
    }
}

class Dispose(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DisposeFirst)
        {
            // rotation angle is unknown until second cast
            Sequences.Add(new()
            {
                Shape = new AOEShapeCone(100, 30.Degrees()),
                Origin = spell.LocXZ,
                Rotation = spell.Rotation,
                Increment = 60.Degrees(),
                NextActivation = Module.CastFinishAt(spell),
                SecondsBetweenActivations = 1.2f,
                NumRemainingCasts = 6,
                MaxShownAOEs = 3
            });
            Sequences.Add(new()
            {
                Shape = new AOEShapeCone(100, 30.Degrees()),
                Origin = spell.LocXZ,
                Rotation = spell.Rotation,
                Increment = -60.Degrees(),
                NextActivation = Module.CastFinishAt(spell),
                SecondsBetweenActivations = 1.2f,
                NumRemainingCasts = 6,
                MaxShownAOEs = 3
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DisposeFirst:
                for (var i = 0; i < Sequences.Count; i++)
                    AdvanceSequence(i, WorldState.CurrentTime);
                break;

            case AID.DisposeRest:
                var index = Sequences.FindIndex(s => s.Origin.AlmostEqual(caster.Position, 1) && s.Rotation.AlmostEqual(spell.Rotation, 0.05f));
                if (index >= 0)
                {
                    AdvanceSequence(index, WorldState.CurrentTime);
                    if (Sequences.Count == 2)
                    {
                        var seq = Sequences[index];
                        Sequences.Clear();
                        Sequences.Add(seq);
                    }
                }
                break;
        }
    }
}

class IncinerateFast(BossModule module) : Components.RaidwideCast(module, AID.IncinerateFast);
class IncinerateSlow(BossModule module) : Components.RaidwideCast(module, AID.IncinerateSlow);

class TartarusMode(BossModule module) : Components.CastCounter(module, AID.TartarusMode);
class Tartarus(BossModule module) : Components.CastCounter(module, AID.Tartarus);

class IntermissionAdds(BossModule module) : Components.AddsMulti(module, [OID.Construct712, OID.Construct711, OID.Construct713]);

class EmpoweredDestroy(BossModule module) : Components.SingleTargetCast(module, AID.EmpoweredDestroy);
class EmpoweredCompress(BossModule module) : Components.StandardAOEs(module, AID.EmpoweredCompress, new AOEShapeRect(100, 3.5f));
class EmpoweredIncinerateFast(BossModule module) : Components.RaidwideCast(module, AID.EmpoweredIncinerateFast);
class EmpoweredIncinerateSlow(BossModule module) : Components.RaidwideCast(module, AID.EmpoweredIncinerateSlow);

class A23Construct7States : StateMachineBuilder
{
    public A23Construct7States(BossModule module) : base(module)
    {
        SimplePhase(0, P1, "P1")
            .ActivateOnEnter<Tartarus>()
            .ActivateOnEnter<Destroy>()
            .ActivateOnEnter<Ignite>()
            .ActivateOnEnter<Ignite2>()
            .ActivateOnEnter<IgnitePredict>()
            .ActivateOnEnter<Accelerate>()
            .ActivateOnEnter<Pulverize1>()
            .ActivateOnEnter<Pulverize2>()
            .ActivateOnEnter<Compress>()
            .ActivateOnEnter<Lithobrake>()
            .ActivateOnEnter<Triboelectricity>()
            .ActivateOnEnter<Dispose>()
            .ActivateOnEnter<IncinerateFast>()
            .ActivateOnEnter<IncinerateSlow>()
            .ActivateOnEnter<ComputationMode>()
            .Raw.Update = () => Module.FindComponent<Tartarus>() is { NumCasts: 1 } && Module.PrimaryActor.IsDestroyed // standard phasing
                || Module.PrimaryActor.IsDead; // unsync intermission skip

        SimplePhase(1, P2, "Intermission")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .ActivateOnEnter<IntermissionAdds>()
            .Raw.Update = () => module.PrimaryActor.IsDead || Module.Enemies(OID.Boss).Any();

        DeathPhase(2, P3)
            .ActivateOnEnter<Ignite>()
            .ActivateOnEnter<Accelerate>()
            .ActivateOnEnter<Pulverize1>()
            .ActivateOnEnter<Pulverize2>()
            .ActivateOnEnter<EmpoweredDestroy>()
            .ActivateOnEnter<EmpoweredCompress>()
            .ActivateOnEnter<EmpoweredIncinerateFast>()
            .ActivateOnEnter<EmpoweredIncinerateSlow>()
            .ActivateOnEnter<ComputationMode>()
            .SetHint(StateMachine.PhaseHint.StartWithDowntime);
    }

    private void P1(uint id)
    {
        ComponentCondition<TartarusMode>(id, 180, t => t.NumCasts > 0, maxOverdue: 1000)
            .ActivateOnEnter<TartarusMode>();
        Targetable(id + 0x10, false, 2.1f, "Boss disappears");
        Condition(id + 0x11, 18.9f, () => Module.PrimaryActor.IsDestroyed, "Intermission start");
    }

    private void P2(uint id)
    {
        ComponentCondition<IntermissionAdds>(id, 5.4f, i => i.ActiveActors.Any(), "Minibots appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd)
            .OnEnter(() =>
            {
                Module.Arena.Center = A23Construct7.AddsArenaCenter;
                Module.Arena.Bounds = A23Construct7.AddsArena;
            });
        Timeout(id + 0xFF0000, 10000, "???");
    }

    private void P3(uint id)
    {
        Targetable(id, true, 10.3f, "Boss reappears")
            .OnEnter(() =>
            {
                Module.Arena.Center = new(800, -141);
                Module.Arena.Bounds = new ArenaBoundsSquare(29.5f);
            });

        Timeout(id + 0xFF0000, 10000, "???");
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7237)]
public class A23Construct7(ModuleInit init) : BossModule(init, new(800, -141), new ArenaBoundsSquare(29.5f))
{
    public static readonly ArenaBoundsCustom AddsArena = MakeAddsArena();
    public static readonly WPos AddsArenaCenter = new(800, 793.5f);

    private static ArenaBoundsCustom MakeAddsArena()
    {
        var platform = CurveApprox.Circle(14.5f, 0.05f);

        var n = platform.Select(p => p - new WDir(0, 19.5f));
        var e = platform.Select(p => p + new WDir(22.51666f, 19.5f));
        var w = platform.Select(p => p + new WDir(-22.51666f, 19.5f));

        var poly = new RelSimplifiedComplexPolygon([new RelPolygonWithHoles([.. n]), new([.. e]), new([.. w])]);
        return new(37.1f, poly);
    }
}

