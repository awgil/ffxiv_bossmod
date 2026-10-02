namespace BossMod.Heavensward.Trial.T03Thordan;

public enum OID : uint
{
    Boss = 0x1073, // R3.800, King Thordan
    SerZephirin = 0x1074, // R2.200
    SerAdelphel = 0x1075, // R2.200
    SerJanlenoux = 0x1076, // R2.200
    SerVellguine = 0x1077, // R2.200
    SerPaulecrain = 0x1078, // R2.200
    SerIgnasse = 0x1079, // R2.200
    SerGrinnaux = 0x107A, // R2.200
    SerHermenost = 0x107B, // R2.200
    SerGuerrique = 0x107C, // R2.200
    SerCharibert = 0x107D, // R2.200
    SerHaumeric = 0x107E, // R2.200
    SerNoudenet = 0x107F, // R2.200
    Ascalon = 0x1080, // R3.800
    Brightsphere = 0x1081, // R1.000, left behind by Shining Blade, explodes ~4.8s after spawning
    MeteorCircle = 0x1082, // R1.000
    CometCircle = 0x1083, // R1.000
    Helper = 0x1084, // R0.500
    IcePuddle = 0x1E9729, // R2.000, EventObj, left by Hiemal Storm, slows + damages from ~3s after spawning, lasts 18s
}

public enum AID : uint
{
    AutoAttackKnight = 870, // SerAdelphel/Janlenoux->player, no cast, single-target
    KnightVisual1 = 4120, // Knights->self, no cast, single-target
    KnightVisual2 = 4121, // Knights->self, no cast, single-target
    KnightVisual3 = 4185, // Knights->self, no cast, single-target
    BossVisual1 = 4186, // Boss->self, no cast, single-target
    LightOfAscalonStart = 4187, // Boss->self, no cast, single-target, first Light of Ascalon ~10.8s later
    Attack = 4190, // Boss->self, no cast, range 8+R 90-degree cone
    AscalonsMight = 4191, // Boss->self, no cast, range 8+R cone
    AscalonsMercyBoss = 4192, // Boss->self, 3.0s cast, range 31+R cone
    AscalonsMercy = 4193, // Helper->self, 3.0s cast, range 34+R cone
    LightningStormVisual = 4194, // Boss->self, 3.5s cast, single-target
    LightningStorm = 4195, // Helper->player, 4.0s cast, range 5 circle spread
    MeteorainVisual = 4196, // Boss->self, 2.7s cast, single-target
    Meteorain = 4197, // Helper->location, 3.0s cast, range 6 circle, placed on players
    AncientQuaga = 4198, // Boss->self, 3.0s cast, range 80 circle
    KnightsOfTheRound = 4199, // Boss->self, 3.0s cast, single-target
    TheDragonsEye = 4200, // Boss->self, 4.0s cast, single-target
    TheDragonsGaze = 4201, // Boss->self, 5.0s cast, range 80+R circle gaze
    UltimateEnd = 4202, // Boss->self, no cast, range 80 circle, right after the last Light of Ascalon
    TheLightOfAscalon = 4203, // Helper->self, no cast, range 80 circle, 3y knockback x7 ~1.4s apart
    BroadSwing = 4204, // Boss->location, no cast, width 10 rect
    SacredCross = 4205, // SerZephirin->self, 15.0s cast, range 80+R circle
    ShiningBlade = 4210, // SerAdelphel/SerJanlenoux->location, no cast, width 6 charge, 3 chained dashes with no telegraph
    BrightFlare = 4211, // Brightsphere->self, no cast, range 5+R circle
    DimensionalCollapseVisual = 4212, // SerGrinnaux->self, 5.5s cast, single-target
    DimensionalCollapse = 4213, // Helper->location, 6.0s cast, range 3 circle, grows to ~10 before it resolves
    ConvictionVisual = 4214, // SerHermenost->self, 5.2s cast, single-target
    Conviction = 4215, // Helper->location, 8.0s cast, range 2 circle tower
    EternalConviction = 4216, // Helper->self, no cast, range 80 circle, unsoaked tower
    HeavyImpactVisual = 4217, // SerGuerrique->self, no cast, single-target
    HeavyImpact1 = 4218, // Helper->self, 3.0s cast, range 6 270-degree cone, gap behind
    HeavyImpact2 = 4219, // Helper->self, 3.0s cast, range 6-12 270-degree ring, ~1.9s after the previous ring
    HeavyImpact3 = 4220, // Helper->self, 3.0s cast, range 12-18 270-degree ring
    HeavyImpact4 = 4221, // Helper->self, 3.0s cast, range 18-27 270-degree ring
    SpiralThrust = 4222, // SerVellguine->self, 3.0s cast, range 52+R width 12 rect
    SpiralPierce = 4223, // SerPaulecrain->player, 4.0s cast, width 12 charge
    SkywardLeapVisual = 4225, // Helper->self, 6.0s cast, range 80 circle, marks where Ser Ignasse lands
    SkywardLeap = 4226, // SerIgnasse->self, no cast, range 80 circle, damage falls off with distance
    HeavensflameVisual = 4227, // SerCharibert->self, 2.5s cast, single-target
    Heavensflame1 = 4228, // Helper->location, 3.0s cast, range 3 circle
    Heavensflame2 = 4229, // Helper->location, 3.0s cast, range 4 circle
    Heavensflame3 = 4230, // Helper->location, 3.0s cast, range 5 circle
    Heavensflame4 = 4231, // Helper->location, 3.0s cast, range 6 circle
    HolyChain = 4232, // Helper->self, no cast, damage while the chain holds
    HiemalStormVisual = 4233, // SerHaumeric->self, 2.5s cast, single-target
    HiemalStorm = 4234, // Helper->location, no cast, range 6 circle, on icon targets
    HolyMeteor = 4235, // SerNoudenet->self, 3.0s cast, single-target
    CometImpact = 4236, // CometCircle->self, no cast, range 80 circle
    MeteorImpact = 4237, // MeteorCircle->self, no cast, range 80 circle
}

public enum IconID : uint
{
    LightningStorm = 21, // player
    HiemalStorm = 29, // player
}

public enum TetherID : uint
{
    HolyChain = 9, // player->player
}

class Attack(BossModule module) : Components.Cleave(module, AID.Attack, new AOEShapeCone(11.8f, 45.Degrees()));
class AscalonsMercyBoss(BossModule module) : Components.StandardAOEs(module, AID.AscalonsMercyBoss, new AOEShapeCone(34.8f, 10.Degrees()));
class AscalonsMercy(BossModule module) : Components.StandardAOEs(module, AID.AscalonsMercy, new AOEShapeCone(34.5f, 10.Degrees()));
class Meteorain(BossModule module) : Components.StandardAOEs(module, AID.Meteorain, 6);
class AncientQuaga(BossModule module) : Components.RaidwideCast(module, AID.AncientQuaga);
class TheDragonsGaze(BossModule module) : Components.CastGaze(module, AID.TheDragonsGaze);
class SacredCross(BossModule module) : Components.RaidwideCast(module, AID.SacredCross);
class DimensionalCollapse(BossModule module) : Components.StandardAOEs(module, AID.DimensionalCollapse, 10);
class Conviction(BossModule module) : Components.CastTowers(module, AID.Conviction, 2);
class HeavyImpact(BossModule module) : Components.ConcentricAOEs(module, [
    new AOEShapeCone(6, 135.Degrees()),
    new AOEShapeDonutSector(6, 12, 135.Degrees()),
    new AOEShapeDonutSector(12, 18, 135.Degrees()),
    new AOEShapeDonutSector(18, 27, 135.Degrees())])
{
    public override void Update()
    {
        Sequences.RemoveAll(s => s.NextActivation != default && s.NextActivation < WorldState.CurrentTime.AddSeconds(-1));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HeavyImpact1)
            AddSequence(caster.Position, Module.CastFinishAt(spell), spell.Rotation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.HeavyImpact1 => 0,
            AID.HeavyImpact2 => 1,
            AID.HeavyImpact3 => 2,
            AID.HeavyImpact4 => 3,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(1.9f), spell.Rotation);
    }
}
class SpiralThrust(BossModule module) : Components.StandardAOEs(module, AID.SpiralThrust, new AOEShapeRect(54.2f, 6));
class SpiralPierce(BossModule module) : Components.BaitAwayChargeCast(module, AID.SpiralPierce, 6);
class SkywardLeap(BossModule module) : Components.StandardAOEs(module, AID.SkywardLeapVisual, new AOEShapeCircle(38), warningText: "Get far away from the marker!");
class Heavensflame1(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame1, 3);
class Heavensflame2(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame2, 4);
class Heavensflame3(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame3, 5);
class Heavensflame4(BossModule module) : Components.StandardAOEs(module, AID.Heavensflame4, 6);
class HolyChain(BossModule module) : Components.Chains(module, (uint)TetherID.HolyChain, AID.HolyChain);
class HiemalStorm(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.HiemalStorm, AID.HiemalStorm, 6, 2.8f);
class IcePuddle(BossModule module) : Components.Voidzone(module, 6.5f, (uint)OID.IcePuddle);
class LightningStorm(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.LightningStorm, AID.LightningStorm, 5, 4.1f);

class BrightFlare(BossModule module) : Components.GenericAOEs(module, AID.BrightFlare)
{
    private readonly List<(Actor Sphere, DateTime Activation)> _spheres = [];
    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _spheres.Select(s => new AOEInstance(_shape, s.Sphere.Position, default, s.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Brightsphere)
            _spheres.Add((actor, WorldState.FutureTime(4.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _spheres.RemoveAll(s => s.Sphere == caster);
        }
    }
}

class LightOfAscalon(BossModule module) : Components.Knockback(module, AID.TheLightOfAscalon)
{
    private const int NumHits = 7;
    private DateTime _next;
    private WPos? _origin;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_next == default || NumCasts >= NumHits)
            yield break;
        var origin = _origin ?? Module.Enemies(OID.Ascalon).FirstOrDefault()?.Position ?? new(0, -19);
        yield return new(origin, 3 * (NumHits - NumCasts), _next);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightOfAscalonStart)
        {
            _next = WorldState.FutureTime(10.8f);
            NumCasts = 0;
            _origin = null;
        }
        else if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _origin = caster.Position;
            _next = WorldState.FutureTime(1.4f);
        }
    }
}

class Knights(BossModule module) : Components.AddsMulti(module, [
    OID.SerZephirin, OID.SerAdelphel, OID.SerJanlenoux, OID.SerVellguine, OID.SerPaulecrain, OID.SerIgnasse,
    OID.SerGrinnaux, OID.SerHermenost, OID.SerGuerrique, OID.SerCharibert, OID.SerHaumeric, OID.SerNoudenet
], 1);
class Meteors(BossModule module) : Components.AddsMulti(module, [OID.MeteorCircle, OID.CometCircle], 2);

class T03ThordanStates : StateMachineBuilder
{
    public T03ThordanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Attack>()
            .ActivateOnEnter<AscalonsMercyBoss>()
            .ActivateOnEnter<AscalonsMercy>()
            .ActivateOnEnter<Meteorain>()
            .ActivateOnEnter<AncientQuaga>()
            .ActivateOnEnter<TheDragonsGaze>()
            .ActivateOnEnter<SacredCross>()
            .ActivateOnEnter<DimensionalCollapse>()
            .ActivateOnEnter<Conviction>()
            .ActivateOnEnter<HeavyImpact>()
            .ActivateOnEnter<SpiralThrust>()
            .ActivateOnEnter<SpiralPierce>()
            .ActivateOnEnter<SkywardLeap>()
            .ActivateOnEnter<Heavensflame1>()
            .ActivateOnEnter<Heavensflame2>()
            .ActivateOnEnter<Heavensflame3>()
            .ActivateOnEnter<Heavensflame4>()
            .ActivateOnEnter<HolyChain>()
            .ActivateOnEnter<HiemalStorm>()
            .ActivateOnEnter<IcePuddle>()
            .ActivateOnEnter<LightningStorm>()
            .ActivateOnEnter<BrightFlare>()
            .ActivateOnEnter<LightOfAscalon>()
            .ActivateOnEnter<Knights>()
            .ActivateOnEnter<Meteors>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 90, NameID = 3632)]
public class T03Thordan(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(24));
