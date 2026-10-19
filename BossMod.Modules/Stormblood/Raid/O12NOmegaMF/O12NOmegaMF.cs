namespace BossMod.Stormblood.Raid.O12NOmegaMF;

public enum OID : uint
{
    Boss = 0x247B, // R3.000, Omega-M
    OmegaF = 0x247C, // R3.000, Omega-F
    Helper = 0x233C, // R0.500
    OpticalUnit = 0x18D6, // R0.500
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 13094, // Boss/Omega->player, no cast, single-target

    SolarRay = 13071, // Boss->player, 5.0s cast, single-target
    SolarRayF = 13072, // Boss/Omega->player, 5.0s cast, single-target
    ProgramAlpha = 13064, // Boss->self, 3.0s cast, single-target
    Floodlight = 13065, // Helper->location, 3.0s cast, range 6 circle
    Spotlight = 13066, // Helper->player, 5.0s cast, range 6 circle
    EfficientBladework = 13055, // Boss->self, 4.0s cast, range 10 circle

    SubjectSimulationF = 13041, // Boss->self, 2.0s cast, single-target
    Discharger = 13046, // Boss/Omega->self, no cast, knockback

    OptimizedFireIII = 13069, // Boss/Omega->self, 3.0s cast, single-target
    OptimizedFireIIISpread = 13070, // Helper->player, 5.0s cast, range 5 circle
    OptimizedBlizzardIII = 13059, // Boss/Omega->self, 3.0s cast, range 40 width 10 cross
    LaserShowerF = 13074, // Boss/Omega->self, 4.0s cast, range 100 circle
    LaserShowerM = 13073, // Boss->self, 4.0s cast, range 100 circle

    SyntheticBlades = 13057, // Boss/Omega->self, 2.0s cast, single-target
    SuperliminalSteel = 13061, // Boss/Omega->self, 3.0s cast, single-target
    SuperliminalSteel1 = 13063, // Helper->self, 3.0s cast, range 80 width 36 rect
    SuperliminalSteel2 = 13062, // Helper->self, 3.0s cast, range 80 width 36 rect

    SubjectSimulationM = 13044, // Boss->self, 2.0s cast, single-target
    EfficientBladeworkInstant = 13043, // Boss->self, no cast, range 10 circle
    SyntheticShield = 13053, // Boss->self, 2.0s cast, single-target
    BeyondStrength = 13056, // Boss->self, 4.0s cast, range 5-40 donut

    GroundZero = 13076, // Boss->location, no cast, range 6 circle
    ElectricSlide = 13075, // Omega->location, no cast, range 6 circle
    OptimizedPassageOfArms = 13078, // Boss->self, no cast, single-target
    LaserShowerEnrage = 13079, // Omega->self, 55.0s cast, range 100 circle

    FirewallM = 13202, // Boss->self, 3.0s cast, range 100 circle
    FirewallF = 13203, // Omega->self, 3.0s cast, range 100 circle
    SpotlightDuo = 13082, // Helper->player, 5.0s cast, range 6 circle

    CosmoMemoryM = 13083, // Boss->self, 5.0s cast, single-target
    CosmoMemoryF = 13084, // Omega->self, 5.0s cast, single-target
    CosmoMemory = 13085, // OpticalUnit->self, 1.2s cast, range 100 circle
    ResonanceM = 13204, // Boss->self, 3.0s cast, single-target
    ResonanceF = 13205, // Omega->self, 3.0s cast, single-target
    SuppressionM = 13086, // Boss->self, 3.0s cast, single-target
    SuppressionF = 13087, // Omega->self, 3.0s cast, single-target
    OpticalLaser = 13088, // OpticalUnit->self, 5.0s cast, range 100 width 16 rect

    OptimizedMeteor = 13093, // Helper->player, 5.0s cast, range 100 circle
    OptimizedSagittariusArrow = 13091, // Boss->self, 5.0s cast, range 100 width 10 rect
    OptimizedMeteorVisual = 13092, // Omega->self, 5.0s cast, single-target
    OptimizedBladedanceM = 13089, // Boss->player, 5.0s cast, single-target
    OptimizedBladedanceF = 13090, // Omega->player, 5.0s cast, single-target
}

public enum IconID : uint
{
    Stack = 62, // spotlight and electric slide
    FireSpread = 96,
    GroundZero = 139,
    Meteor = 87,
}

public enum SID : uint
{
    PacketFilterM = 1660, // attack Omega-F
    PacketFilterF = 1661, // attack Omega-M
    LocalResonance = 1662,
}

class Tankbusters(BossModule module) : Components.CastHint(module, AID.SolarRay, "Tankbuster")
{
    private static readonly ActionID[] Aids = [ActionID.MakeSpell(AID.SolarRay), ActionID.MakeSpell(AID.SolarRayF), ActionID.MakeSpell(AID.OptimizedBladedanceM), ActionID.MakeSpell(AID.OptimizedBladedanceF)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Aids.Contains(spell.Action))
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Aids.Contains(spell.Action))
            Casters.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            if (c.CastInfo != null)
            {
                var target = c.CastInfo.TargetID != c.InstanceID ? c.CastInfo.TargetID : c.TargetID;
                hints.AddPredictedDamage(new BitMask().WithBit(Raid.FindSlot(target)), Module.CastFinishAt(c.CastInfo), AIHints.PredictedDamageType.Tankbuster);
            }
        }
    }
}

class Floodlight(BossModule module) : Components.StandardAOEs(module, AID.Floodlight, 6);
class Spotlight(BossModule module) : Components.StackWithCastTargets(module, AID.Spotlight, 6);
class SpotlightDuo(BossModule module) : Components.StackWithCastTargets(module, AID.SpotlightDuo, 6);
class EfficientBladework(BossModule module) : Components.StandardAOEs(module, AID.EfficientBladework, 10);
class OptimizedFireIII(BossModule module) : Components.SpreadFromCastTargets(module, AID.OptimizedFireIIISpread, 5);
class OptimizedBlizzardIII(BossModule module) : Components.StandardAOEs(module, AID.OptimizedBlizzardIII, new AOEShapeCross(40, 5));
class SuperliminalSteel(BossModule module) : Components.GroupedAOEs(module, [AID.SuperliminalSteel1, AID.SuperliminalSteel2], new AOEShapeRect(80, 18));
class BeyondStrength(BossModule module) : Components.StandardAOEs(module, AID.BeyondStrength, new AOEShapeDonut(5, 40));
class OpticalLaser(BossModule module) : Components.StandardAOEs(module, AID.OpticalLaser, new AOEShapeRect(100, 8));
class OptimizedSagittariusArrow(BossModule module) : Components.StandardAOEs(module, AID.OptimizedSagittariusArrow, new AOEShapeRect(100, 5));

class Raidwides(BossModule module) : Components.CastHint(module, AID.LaserShowerM, "Raidwide")
{
    private static readonly ActionID[] Aids = [ActionID.MakeSpell(AID.LaserShowerM), ActionID.MakeSpell(AID.LaserShowerF), ActionID.MakeSpell(AID.LaserShowerEnrage), ActionID.MakeSpell(AID.FirewallM), ActionID.MakeSpell(AID.FirewallF), ActionID.MakeSpell(AID.CosmoMemoryM), ActionID.MakeSpell(AID.CosmoMemoryF), ActionID.MakeSpell(AID.CosmoMemory)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Aids.Contains(spell.Action))
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Aids.Contains(spell.Action))
            Casters.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo));
    }
}

class Passage(BossModule module) : Components.CastHint(module, AID.LaserShowerEnrage, "Kill Omega-M!");

class SpawnBladework(BossModule module) : Components.GenericAOEs(module, AID.EfficientBladeworkInstant)
{
    private readonly AOEShapeCircle _shape = new(10);
    private Actor? _follow;
    private DateTime _followAt;
    private readonly List<(WPos Pos, DateTime At)> _fixed = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_follow != null)
            yield return new(_shape, _follow.Position, Activation: _followAt);
        foreach (var f in _fixed)
            yield return new(_shape, f.Pos, Activation: f.At);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SubjectSimulationM)
        {
            _follow = caster;
            _followAt = WorldState.FutureTime(14.3f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GroundZero)
            _fixed.Add((spell.TargetPos.ToWPos(), WorldState.FutureTime(8.3f)));
        else if (spell.Action.ID == (uint)AID.EfficientBladeworkInstant)
        {
            _follow = null;
            _fixed.Clear();
            NumCasts++;
        }
    }

    public override void Update()
    {
        if (_follow != null && _followAt.AddSeconds(1) < WorldState.CurrentTime)
            _follow = null;
        _fixed.RemoveAll(f => f.At.AddSeconds(1) < WorldState.CurrentTime);
    }
}

class Discharger(BossModule module) : Components.Knockback(module, AID.Discharger)
{
    private const float Distance = 15;
    private Actor? _follow;
    private DateTime _followAt;
    private readonly List<(WPos Pos, DateTime At)> _fixed = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_follow != null)
            yield return new(_follow.Position, Distance, _followAt);
        foreach (var f in _fixed)
            yield return new(f.Pos, Distance, f.At);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SubjectSimulationF)
        {
            _follow = caster;
            _followAt = WorldState.FutureTime(14.6f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ElectricSlide)
            _fixed.Add((spell.TargetPos.ToWPos(), WorldState.FutureTime(8.3f)));
        else if (spell.Action.ID == (uint)AID.Discharger)
        {
            _follow = null;
            _fixed.Clear();
            NumCasts++;
        }
    }

    public override void Update()
    {
        if (_follow != null && _followAt.AddSeconds(1) < WorldState.CurrentTime)
            _follow = null;
        _fixed.RemoveAll(f => f.At.AddSeconds(1) < WorldState.CurrentTime);
    }
}

class GroundZero(BossModule module) : Components.UniformStackSpread(module, 6, 6, alwaysShowSpreads: true)
{
    private readonly List<Actor> _pendingStacks = [];
    private DateTime _pendingUntil;
    private DateTime _gooUntil;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.GroundZero)
        {
            _gooUntil = WorldState.FutureTime(2);
            AddSpread(actor, WorldState.FutureTime(5.2f));
            foreach (var stack in _pendingStacks)
                AddStack(stack, WorldState.FutureTime(5.2f));
            _pendingStacks.Clear();
        }
        else if (iconID == (uint)IconID.Stack)
        {
            if (WorldState.CurrentTime < _gooUntil)
                AddStack(actor, WorldState.FutureTime(5.2f));
            else
            {
                _pendingStacks.Add(actor);
                _pendingUntil = WorldState.FutureTime(1);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.GroundZero)
            Spreads.Clear();
        else if (spell.Action.ID == (uint)AID.ElectricSlide)
            Stacks.Clear();
    }

    public override void Update()
    {
        if (_pendingStacks.Count > 0 && WorldState.CurrentTime > _pendingUntil)
            _pendingStacks.Clear();
    }
}

class OptimizedMeteor(BossModule module) : Components.BaitAwayCast(module, AID.OptimizedMeteor, new AOEShapeCircle(10), true)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Any())
        {
            if ((actor.Position - Arena.Center).LengthSq() < 16 * 16)
                hints.Add("Bait meteor to the edge!");
        }
        else
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        DamageType = AIHints.PredictedDamageType.Raidwide;
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var b in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 16), b.Activation);
    }
}

class PacketFilter(BossModule module) : BossComponent(module)
{
    private enum Attack { None, M, F }
    private Attack _attack;

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (actor != Raid.Player())
            return;
        if (status.ID == (uint)SID.PacketFilterF)
            _attack = Attack.M;
        else if (status.ID == (uint)SID.PacketFilterM)
            _attack = Attack.F;
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (actor == Raid.Player() && status.ID is (uint)SID.PacketFilterF or (uint)SID.PacketFilterM)
            _attack = Attack.None;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_attack == Attack.M)
            hints.Add("Attack Omega-M!");
        else if (_attack == Attack.F)
            hints.Add("Attack Omega-F!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var oid = _attack == Attack.M ? OID.Boss : _attack == Attack.F ? OID.OmegaF : 0;
        if (oid != 0)
            hints.ForcedTarget = Module.Enemies(oid).FirstOrDefault(a => !a.IsDead);
    }
}

class LocalResonance(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Active)
            hints.Add("Move bosses apart!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;
        var player = Raid.Player();
        if (player == null)
            return;
        var avoid = player.FindStatus(SID.PacketFilterM) != null ? Alive(OID.Boss) : player.FindStatus(SID.PacketFilterF) != null ? Alive(OID.OmegaF) : null;
        if (avoid != null)
            hints.AddForbiddenZone(ShapeDistance.Circle(avoid.Position, 15), WorldState.CurrentTime);
    }

    private bool Active => Alive(OID.Boss)?.FindStatus(SID.LocalResonance) != null || Alive(OID.OmegaF)?.FindStatus(SID.LocalResonance) != null;
    private Actor? Alive(OID oid) => Module.Enemies(oid).FirstOrDefault(a => !a.IsDead);
}

class OmegaMStates : StateMachineBuilder
{
    public OmegaMStates(BossModule module) : base(module)
    {
        SimplePhase(0, SinglePhase, "Boss death")
            .Raw.Update = () => Dead(module.PrimaryActor) && module.Enemies(OID.OmegaF).All(Dead);
    }

    private static bool Dead(Actor actor) => actor.IsDeadOrDestroyed || actor.HPMP.CurHP == 0;

    private void SinglePhase(uint id)
    {
        SimpleState(id + 0xFF0000, 10000, "Omega")
            .ActivateOnEnter<Tankbusters>()
            .ActivateOnEnter<Floodlight>()
            .ActivateOnEnter<Spotlight>()
            .ActivateOnEnter<SpotlightDuo>()
            .ActivateOnEnter<EfficientBladework>()
            .ActivateOnEnter<SpawnBladework>()
            .ActivateOnEnter<Discharger>()
            .ActivateOnEnter<OptimizedFireIII>()
            .ActivateOnEnter<OptimizedBlizzardIII>()
            .ActivateOnEnter<SuperliminalSteel>()
            .ActivateOnEnter<BeyondStrength>()
            .ActivateOnEnter<GroundZero>()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<Passage>()
            .ActivateOnEnter<PacketFilter>()
            .ActivateOnEnter<LocalResonance>()
            .ActivateOnEnter<OpticalLaser>()
            .ActivateOnEnter<OptimizedSagittariusArrow>()
            .ActivateOnEnter<OptimizedMeteor>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 590, NameID = 7633)]
public class OmegaM(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.OmegaF).Where(a => !a.IsDead && (a.IsTargetable || a.CastInfo != null)), ArenaColor.Enemy, true);
    }
}
