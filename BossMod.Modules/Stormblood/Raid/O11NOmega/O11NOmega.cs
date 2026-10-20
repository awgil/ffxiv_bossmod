namespace BossMod.Stormblood.Raid.O11NOmega;

public enum OID : uint
{
    Boss = 0x2477, // R6.750
    Helper = 0x233C, // R0.500
    RocketPunch = 0x2597, // R2.500
    RocketPunchBig = 0x2596, // R8.000
    LevelChecker = 0x2479, // R3.990
    ConductiveFocus = 0x25A6, // R2.490
    LoopSpot = 0x1EAA09, // R0.500
    BurnPuddle = 0x1E8D9B, // R0.500
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AtomicRay = 12934, // Boss->self, 4.0s cast, range 80 circle
    MustardBomb = 12935, // Boss->player, 5.0s cast, range 5 circle
    StarboardWaveCannon = 12929, // Boss->self, 5.0s cast, range 60+R 210-degree cone
    StarboardWaveCannon2 = 12930, // Boss->self, 3.0s cast, range 60+R 210-degree cone
    LarboardWaveCannon = 12931, // Boss->self, 5.0s cast, range 60+R 210-degree cone
    LarboardWaveCannon2 = 12932, // Boss->self, 3.0s cast, range 60+R 210-degree cone
    BallisticMissile = 12926, // Boss->self, no cast, single-target
    BallisticImpact = 12927, // Helper->player, no cast, range 6 circle
    FlameThrower = 12925, // Helper->self, 4.0s cast, range 60+R 30-degree cone
    FlameThrowerVisual = 12924, // Boss->self, 4.0s cast, single-target
    PeripheralSynthesis = 12912, // Boss->self, 3.0s cast, single-target
    RushSmall = 13830, // Helper->self, no cast, range 8 width 5 rect
    ProgramLoop = 12915, // Boss->self, 4.0s cast, single-target
    Executable = 13736, // LevelChecker->self, 5.0s cast, single-target
    CriticalStorageViolation = 12921, // Helper->self, no cast, range 80 circle
    StorageViolation = 12920, // Helper->self, no cast, range 3 circle
    Reset = 13738, // LevelChecker->self, 3.0s cast, range 12 circle
    Reformat = 13737, // LevelChecker->self, 3.0s cast, range 10-20 donut
    DeltaAttack = 12923, // Boss->self, 30.0s cast, range 100+R width 100 rect
    ForceQuit = 12922, // LevelChecker->self, 24.0s cast, range 80 circle
    Rush = 13724, // RocketPunchBig->self, 7.0s cast, range 49+R width 15 rect
    ElectricSlide = 12933, // Boss->player, 5.0s cast, range 6 circle
    Blaster = 12928, // Boss->player, no cast, range 15 circle
}

public enum IconID : uint
{
    BallisticMissile = 101, // player
    Stack = 62, // player
}

public enum TetherID : uint
{
    Blaster = 84,
}

class AtomicRay(BossModule module) : Components.RaidwideCast(module, AID.AtomicRay);
class MustardBomb(BossModule module) : Components.SpreadFromCastTargets(module, AID.MustardBomb, 5);
class WaveCannon(BossModule module) : Components.GroupedAOEs(module, [AID.StarboardWaveCannon, AID.StarboardWaveCannon2, AID.LarboardWaveCannon, AID.LarboardWaveCannon2], new AOEShapeCone(66.75f, 105.Degrees()));
class FlameThrower(BossModule module) : Components.StandardAOEs(module, AID.FlameThrower, new AOEShapeCone(60.5f, 15.Degrees()));
class RocketPunchRush(BossModule module) : Components.StandardAOEs(module, AID.Rush, new AOEShapeRect(57, 7.5f));
class Reset(BossModule module) : Components.StandardAOEs(module, AID.Reset, 12);
class Reformat(BossModule module) : Components.StandardAOEs(module, AID.Reformat, new AOEShapeDonut(10, 20));
class ForceQuit(BossModule module) : Components.RaidwideCast(module, AID.ForceQuit);
class DeltaAttack(BossModule module) : Components.RaidwideCast(module, AID.DeltaAttack);
class ElectricSlide(BossModule module) : Components.StackWithCastTargets(module, AID.ElectricSlide, 6);
class Blaster(BossModule module) : Components.TankbusterTether(module, AID.Blaster, (uint)TetherID.Blaster, 15, 8.1f);

class BallisticImpact(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.BallisticMissile, AID.BallisticImpact, 8.1f, true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (ActiveBaitsOn(actor).Any() && (actor.Position - Arena.Center).LengthSq() < 16 * 16)
            hints.Add("Drop fire at the edge!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 16), b.Activation);
    }
}

class BallisticPuddle(BossModule module) : Components.Voidzone(module, 6, OID.BurnPuddle);

class RocketPunch(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var punch in Module.Enemies(OID.RocketPunch))
            if (!punch.IsDead)
                yield return new(Shape, punch.Position, default, WorldState.CurrentTime);
    }
}

class ElectricFloor(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Inner = new(4);
    private static readonly AOEShapeDonut Outer = new(18, 30);
    private bool _active;
    private bool _seenChecker;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active)
            yield break;
        var checker = Module.Enemies(OID.LevelChecker).FirstOrDefault(a => !a.IsDead);
        var pos = checker?.Position ?? Arena.Center;
        yield return new(Inner, pos, default, WorldState.CurrentTime);
        yield return new(Outer, pos, default, WorldState.CurrentTime);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ProgramLoop)
        {
            _active = true;
            _seenChecker = false;
        }
    }

    public override void Update()
    {
        if (!_active)
            return;
        if (Module.Enemies(OID.LevelChecker).Any(a => !a.IsDead))
            _seenChecker = true;
        else if (_seenChecker)
            _active = false;
    }
}

class Loop(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LoopSpot)
            Towers.Add(new(actor.Position, 3, activation: WorldState.FutureTime(8)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.StorageViolation or AID.CriticalStorageViolation)
            Towers.Clear();
    }

    public override void Update()
    {
        var max = Towers.Count > 4 ? 1 : 2;
        for (var i = 0; i < Towers.Count; ++i)
        {
            var tower = Towers[i];
            tower.MaxSoakers = max;
            Towers[i] = tower;
        }

        EnableHints = !Module.Enemies(OID.LevelChecker).Any(a => a.CastInfo != null && (AID)a.CastInfo.Action.ID is AID.Reset or AID.Reformat);
    }
}

class OmegaJammer(BossModule module) : BossComponent(module)
{
    private int _induction = -1; // director 80037577

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x8000000C && param1 == 0x1B)
            _induction = param2 == uint.MaxValue ? 10000 : (int)param2;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Focus() != null)
            hints.Add($"Jammer the focus!");
        else if (Punch(actor) != null)
            hints.Add("Jammer the fists!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var target = Focus() ?? Punch(actor);
        if (target == null)
            return;

        var reach = 20 + actor.HitboxRadius + target.HitboxRadius;
        hints.ForcedTarget = target;
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(target.Position, reach - 1), WorldState.CurrentTime);
        if ((actor.Position - target.Position).LengthSq() <= reach * reach)
        {
            var jammer = ActionID.MakeSpell(ClassShared.AID.OmegaJammer);
            var duty = WorldState.Client.DutyActions.FirstOrDefault(a => a.Action.ID == jammer.ID);
            hints.ActionsToExecute.Push(duty.Action != default ? duty.Action : jammer, target, ActionQueue.Priority.VeryHigh, forced: true);
        }
    }

    // don't care about hitting the focus when the induction bar is full
    private Actor? Focus() => _induction >= 10000 ? null : Module.Enemies(OID.ConductiveFocus).FirstOrDefault(a => !a.IsDead);
    private Actor? Punch(Actor actor) => Closest(OID.RocketPunch, actor);
    private Actor? Closest(OID oid, Actor actor) => Module.Enemies(oid).Where(a => !a.IsDead).MinBy(a => (a.Position - actor.Position).LengthSq());
}

class OmegaStates : StateMachineBuilder
{
    public OmegaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<MustardBomb>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnEnter<BallisticImpact>()
            .ActivateOnEnter<BallisticPuddle>()
            .ActivateOnEnter<FlameThrower>()
            .ActivateOnEnter<RocketPunch>()
            .ActivateOnEnter<RocketPunchRush>()
            .ActivateOnEnter<ElectricFloor>()
            .ActivateOnEnter<Loop>()
            .ActivateOnEnter<Reset>()
            .ActivateOnEnter<Reformat>()
            .ActivateOnEnter<ForceQuit>()
            .ActivateOnEnter<DeltaAttack>()
            .ActivateOnEnter<ElectricSlide>()
            .ActivateOnEnter<Blaster>()
            .ActivateOnEnter<OmegaJammer>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 589, NameID = 7695)]
public class Omega(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsSquare(20));
