namespace BossMod.Stormblood.Raid.O2NCatastrophe;

public enum OID : uint
{
    Boss = 0x1E5D, // R4.000
    Helper = 0x18D6, // R0.500
    FleshyMember = 0x1F4C, // R1.000
    Petrosphere = 0x1E5F, // R1.000, purple, hits anyone on the ground
    PotentPetrosphere = 0x1E5E, // R1.000, gold, hits anyone levitating
}

public enum AID : uint
{
    AutoAttack = 9448, // Boss->player, no cast, single-target
    Tremblor = 9489, // Boss->self, no cast, single-target
    TremblorAOE = 9492, // Helper->self, no cast, range 50 circle
    Earthquake = 9490, // Boss->self, 5.0s cast, single-target
    EarthquakeAOE = 9493, // Helper->self, no cast, range 50 circle
    HundredGs = 9467, // Boss->self, 3.0s cast, single-target
    HundredGsHit = 9468, // Helper->player, no cast, single-target
    Evilsphere = 9487, // Boss->player, 5.0s cast, single-target
    GravitationalDistortion = 9482, // Boss->self, no cast, single-target
    GravitationalManipulation = 9476, // Boss->self, 5.0s cast, single-target
    GravitationalManipulationHit = 9477, // Helper->player, no cast, single-target
    GravitationalExplosion = 9478, // Helper->player, no cast, range 7 circle
    ParanormalWave = 9486, // Boss->self, no cast, range 6+R cone
    ManiacalProbe = 9615, // Boss->self, 5.0s cast, single-target
    Erosion = 9616, // FleshyMember->self, 3.0s cast, range 4 circle
    Epicenter = 9378, // Boss->self, 3.0s cast, single-target
    GravitationalWave = 9488, // Boss->self, 3.0s cast, range 50 circle
    MainQuake = 9381, // Helper->self, no cast, range 14 circle
    Antilight = 9474, // Boss->self, 3.0s cast, single-target
    DemonEye = 9485, // Boss->self, 5.0s cast, range 50 circle
    DemonEyeAOE = 9494, // Helper->self, no cast, range 100 circle
    Explosion = 9491, // Petrosphere->self, no cast, range 15 circle
    PotentExplosion = 9475, // PotentPetrosphere->self, no cast, range 15 circle
    Minus100Gs = 9471, // Boss->self, 8.0s cast, single-target
    Minus100GsLift = 9472, // Helper->player, no cast, single-target
    Minus100GsLand = 9473, // Helper->player, no cast, single-target
    AntiGravityGimbal = 9483, // duty action, grants Levitation
}

public enum SID : uint
{
    Levitation = 1366, // Anti-gravity Gimbal
    SixFulmsUnder = 567, // sink; levitate or die
}

public enum IconID : uint
{
    GravitationalExplosion = 113,
}

class GravitationalWave(BossModule module) : Components.RaidwideCast(module, AID.GravitationalWave);
class HundredGs(BossModule module) : Components.CastHint(module, AID.HundredGs, "Levitation ends");
class Minus100Gs(BossModule module) : Components.CastHint(module, AID.Minus100Gs, "Go north or south!");
class Evilsphere(BossModule module) : Components.SingleTargetCast(module, AID.Evilsphere);
class DemonEye(BossModule module) : Components.CastGaze(module, AID.DemonEye);
class Erosion(BossModule module) : Components.StandardAOEs(module, AID.Erosion, 4);
class ParanormalWave(BossModule module) : Components.Cleave(module, AID.ParanormalWave, new AOEShapeCone(10, 60.Degrees())); // 6 + boss radius
class GravitationalExplosion(BossModule module) : Components.StackWithIcon(module, (uint)IconID.GravitationalExplosion, AID.GravitationalExplosion, 7, 5.2f);

class AntiGravityGimbal(BossModule module) : BossComponent(module)
{
    private bool Sinking(Actor actor) => actor.FindStatus(SID.SixFulmsUnder) != null;
    private bool Levitating(Actor actor) => actor.FindStatus(SID.Levitation) != null;
    private bool EarthquakeCasting => Module.PrimaryActor.CastInfo?.IsSpell(AID.Earthquake) ?? false;
    private bool Stacking => Module.FindComponent<GravitationalExplosion>()?.ActiveStacks.Any() ?? false;
    private Petrospheres? Orbs => Module.FindComponent<Petrospheres>();
    private bool PurpleOrbs => Orbs?.Active(OID.Petrosphere) ?? false;
    private bool GoldOrbs => Orbs?.Active(OID.PotentPetrosphere) ?? false;

    // Gold orbs petrify anyone who is levitating. Six Fulms Under can tick during that set. Wait until the orbs explode, then levitate.
    private bool MustLevitate(Actor actor) => !Levitating(actor) && !GoldOrbs && (Sinking(actor) || EarthquakeCasting || Stacking || PurpleOrbs);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (MustLevitate(actor))
            hints.Add("Levitate!");
        else if (!Levitating(actor) && GoldOrbs)
            hints.Add("Don't levitate", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var gimbal = WorldState.Client.DutyActions.FirstOrDefault(a => a.Action.ID == (uint)AID.AntiGravityGimbal);
        if (actor != Raid.Player() || gimbal.Action == default || !MustLevitate(actor))
            return;
        hints.ActionsToExecute.Push(gimbal.Action, Module.PrimaryActor, ActionQueue.Priority.VeryHigh);
    }
}

class Petrospheres(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(15);
    private readonly Dictionary<ulong, DateTime> _activation = [];
    private DateTime _antilightExplosion;
    private DateTime _minusExplosion;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var levitating = actor.FindStatus(SID.Levitation) != null;
        var gold = Active(OID.PotentPetrosphere);
        var purple = Active(OID.Petrosphere);
        var dropping = Module.PrimaryActor.CastInfo?.IsSpell(AID.HundredGs) ?? false;
        var showPurple = purple && (!levitating || gold || dropping);
        var showGold = gold && levitating && !purple;
        if (!showPurple && !showGold)
            yield break;
        var oid = showGold ? OID.PotentPetrosphere : OID.Petrosphere;
        foreach (var orb in Module.Enemies(oid))
            if (_activation.TryGetValue(orb.InstanceID, out var activation))
                yield return new(Shape, orb.Position, Activation: activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.IsSpell(AID.Antilight))
            _antilightExplosion = Module.CastFinishAt(spell, 13.1f);
        else if (spell.IsSpell(AID.Minus100Gs))
            _minusExplosion = Module.CastFinishAt(spell, 7.5f);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is not (uint)OID.Petrosphere and not (uint)OID.PotentPetrosphere)
            return;
        var now = WorldState.CurrentTime;
        var activation = _minusExplosion > now ? _minusExplosion : _antilightExplosion > now ? _antilightExplosion : WorldState.FutureTime(8);
        _activation[actor.InstanceID] = activation;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Explosion or (uint)AID.PotentExplosion)
        {
            _activation.Remove(caster.InstanceID);
            ++NumCasts;
        }
    }

    public override void OnActorDestroyed(Actor actor) => _activation.Remove(actor.InstanceID);

    public bool Active(OID oid) => Module.Enemies(oid).Any(a => _activation.ContainsKey(a.InstanceID));
}

class MainQuake(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle Shape = new(14);
    private readonly Dictionary<ulong, WPos> _marked = [];
    private readonly List<WPos> _staleHelpers = [];
    private bool _resolved;
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation == default || !_resolved)
            yield break;
        foreach (var pos in _marked.Values)
        {
            var center = RealCenter(pos) ?? InwardCenter(pos);
            if (center != null)
                yield return new(Shape, center.Value, Activation: _activation);
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_activation != default && !_resolved)
            hints.Add("Watch the tentacles");
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID != (uint)OID.FleshyMember)
            return;
        if (modelState == 4)
        {
            if (_activation == default)
            {
                _activation = WorldState.FutureTime(16.1f);
                _staleHelpers.Clear();
                foreach (var helper in Module.Enemies(OID.Helper))
                    _staleHelpers.Add(helper.Position);
            }
            _marked[actor.InstanceID] = actor.Position;
        }
        else if (modelState == 0 && _marked.Remove(actor.InstanceID))
            _resolved = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.MainQuake)
            return;
        ++NumCasts;
        _marked.Clear();
        _staleHelpers.Clear();
        _resolved = false;
        _activation = default;
    }

    private WPos? InwardCenter(WPos tentacle)
    {
        var off = tentacle - Arena.Center;
        var dist = off.Length();
        if (dist < 1)
            return null;
        return Arena.Center + off * ((2 * dist - 17) / dist);
    }

    private WPos? RealCenter(WPos tentacle)
    {
        var axis = tentacle - Arena.Center;
        foreach (var helper in Module.Enemies(OID.Helper))
        {
            var off = helper.Position - Arena.Center;
            if (off.LengthSq() < 36 || Math.Abs(axis.Cross(off)) > 4 || axis.Dot(off) <= 0)
                continue;
            if (_staleHelpers.Any(p => p.AlmostEqual(helper.Position, 1)))
                continue;
            return helper.Position;
        }
        return null;
    }
}

class O2NCatastropheStates : StateMachineBuilder
{
    public O2NCatastropheStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GravitationalWave>()
            .ActivateOnEnter<HundredGs>()
            .ActivateOnEnter<Minus100Gs>()
            .ActivateOnEnter<Evilsphere>()
            .ActivateOnEnter<DemonEye>()
            .ActivateOnEnter<Erosion>()
            .ActivateOnEnter<ParanormalWave>()
            .ActivateOnEnter<GravitationalExplosion>()
            .ActivateOnEnter<AntiGravityGimbal>()
            .ActivateOnEnter<Petrospheres>()
            .ActivateOnEnter<MainQuake>();
    }
}

[ModuleInfo(Contributors = "croizat", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 253, NameID = 5631)]
public class O2NCatastrophe(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));
