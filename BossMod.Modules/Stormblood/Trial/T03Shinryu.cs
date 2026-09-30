namespace BossMod.Stormblood.Trial.T03Shinryu;

public enum OID : uint
{
    Boss = 0x1983, // R22.000
    Platform = 0x1EA1A1, // R2.000, EventObj, destructible arena tiles (9)
    RightWing = 0x1B1A, // R15.000, Part
    LeftWing = 0x1B19, // R15.000, Part
    WaterSpout = 0x1E8536, // R2.000, EventObj
    WaterPuddles = 0x1E950D, // R0.500, EventObj
    Icicle = 0x1B16, // R2.500
    EyeOfTheStorm = 0x1B17, // R1.000, pulsing knockback
    Cocoon1 = 0x1B13, // R3.000
    MassiveCocoon = 0x1C86, // R6.000
    Ginryu = 0x1B14, // R1.800
    Hakkinryu = 0x1C83, // R3.600
    Fetters = 0x1B15, // R1.000, fetters + button mash
    Tail = 0x1B12, // R17.940, Helper
    Helper = 0x18D6, // R0.500
}

public enum AID : uint
{
    AutoAttack = 8105, // RightWing/LeftWing->player, no cast, single-target
    TidalWave = 8075, // Helper->self, 10.0s cast, range 80+R width 60 rect
    TidalWaveCast = 8106, // Boss->self, 10.0s cast, single-target, visual
    Levinbolt = 8092, // Helper->player, no cast, range 5 circle
    LevinboltVisual = 8091, // RightWing->self, 6.0s cast, single-target, visual
    AkhMornVisual = 8100, // Boss->players, 4.0s cast, single-target, shared tankbuster visual
    AkhMorn1 = 8101, // Boss->players, no cast, single-target, shared tankbuster hit
    SummonIcicle = 8095, // LeftWing->self, 4.0s cast, single-target
    IcicleImpact = 8096, // Icicle->self, no cast, range 6 circle
    Spikesicle = 8097, // Icicle->self, 2.5s cast, range 62+R width 10 rect
    Hellfire = 8076, // Helper->self, 10.0s cast, range 60 circle
    HellfireVisual = 8107, // Boss->self, 10.0s cast, single-target
    MeteorImpact = 9291, // Helper->self, 5.0s cast, range 60 circle
    MeteorImpactCocoon = 8086, // Cocoon1/MassiveCocoon->self, 4.0s cast, range 60 circle
    Collapse = 8728, // Hakkinryu->self, no cast, range 8+R cone
    Protostar = 8085, // Boss->self, 6.0s cast, range 80 circle
    DarkMatter = 8088, // Boss->self, 3.0s cast, range 60 circle
    GyreCharge = 8104, // Boss->self, no cast, range 100+R width 60 rect
    GyreChargeVisual = 8180, // Helper->self, 6.3s cast, range 100+R width 60 rect
    TailSlap = 8083, // Tail->self, 3.0s cast, range 40 width 20 rect
    TailSlapAlt = 9130, // Tail->self, 3.0s cast, range 40 width 20 rect
    IceStorm = 8098, // LeftWing->self, 6.0s cast, single-target
    BurningChains = 8144, // Helper->self, no cast, single-target
    IceStormRaidwide = 8099, // Helper->self, no cast, range 60 circle
    Dragonfist = 9455, // Boss->self, no cast, single-target
    DragonfistVisual = 9456, // Helper->self, 4.0s cast, range 16 circle
    DiamondDust = 8078, // Helper->self, 10.0s cast, range 60 circle
    Fireball = 8732, // Ginryu->location, 2.5s cast, range 4 circle
    DeathSentence = 8731, // Hakkinryu->player, 4.0s cast, single-target
    SpikedTail = 8729, // Ginryu->player, 1.0s cast, single-target
    JudgmentBolt = 8077, // Helper->self, 10.0s cast, range 60 circle
    EarthenFury = 8079, // Helper->self, 10.0s cast, range 60 circle
    EarthenFurySmash = 9146, // Helper->location, range 20 width 20 rect
    AkhRhai = 8102, // Helper->location, no cast, range 4 circle
    AkhRhaiRepeat = 8103, // Helper->location, no cast, range 4 circle
    HypernovaCast = 8089, // RightWing->self, 6.0s cast, single-target
    Hypernova = 8090, // Helper->players, no cast, range 8 circle
    SuperCyclone = 8984, // Helper->self, 0.5s cast, range 90 circle
    AerialBlast = 8080, // Helper->self, 10.0s cast, range 60 circle
    BlazingTrail = 8730, // Ginryu->self, 3.0s cast, range 15+R width 11 rect
    EarthBreath = 8093, // Boss->self, 9.0s cast, range 6+R cone
    EarthBreathAOE = 8094, // Helper->self, 4.5s cast, range 80+R 60-degree cone
}

public enum IconID : uint
{
    Levinbolt = 24, // player
    HypernovaStack = 62, // player
    EarthBreath = 23, // player
}

public enum SID : uint
{
    FireResistanceUp = 520,
    LightningResistanceDownII = 1260,
    Paralysis = 17,
    Fetters = 667,
    Affixed = 1267,
    BurningChains = 769,
    ThinIce = 911
}

public enum TetherID : uint
{
    BurningTether = 9
}

class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWave, 35, kind: Components.Knockback.Kind.DirForward);
class Levinbolt(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Levinbolt, AID.Levinbolt, 5, 6);
class EarthBreathBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 30.Degrees()), (uint)IconID.EarthBreath, AID.EarthBreathAOE);
class EarthBreathAOE(BossModule module) : Components.StandardAOEs(module, AID.EarthBreathAOE, new AOEShapeCone(60, 30.Degrees()));
class IcicleAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Icicle]);
class AkhMornStack(BossModule module) : Components.CastSharedTankbuster(module, AID.AkhMornVisual, 5);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningTether, AID.BurningChains, 30);
class MeteorImpact(BossModule module) : Components.ProximityAOEs(module, AID.MeteorImpact, 14);
class Cocoons(BossModule module) : Components.AddsMulti(module, [(uint)OID.Cocoon1, (uint)OID.MassiveCocoon]);
class DragonAdds(BossModule module) : Components.AddsMulti(module, [(uint)OID.Ginryu, (uint)OID.Hakkinryu], priority: 1);
class Fireball(BossModule module) : Components.StandardAOEs(module, AID.Fireball, 4);
class BlazingTrail(BossModule module) : Components.StandardAOEs(module, AID.BlazingTrail, new AOEShapeRect(16.8f, 5.5f));
class GyreCharge(BossModule module) : Components.StandardAOEs(module, AID.GyreChargeVisual, new AOEShapeRect(100, 30));
class TailSlap(BossModule module) : Components.GroupedAOEs(module, [AID.TailSlap, AID.TailSlapAlt], new AOEShapeRect(40, 10));
class Dragonfist(BossModule module) : Components.StandardAOEs(module, AID.DragonfistVisual, 16);
class EarthenFurySmash(BossModule module) : Components.StandardAOEs(module, AID.EarthenFurySmash, new AOEShapeRect(20, 10));
class SuperCyclone(BossModule module) : Components.KnockbackFromCastTarget(module, AID.SuperCyclone, 5);
class AerialBlast(BossModule module) : Components.KnockbackFromCastTarget(module, AID.AerialBlast, 15);
class Hypernova(BossModule module) : Components.StackWithIcon(module, (uint)IconID.HypernovaStack, AID.Hypernova, 7, 6);
class Spikesicle(BossModule module) : Components.StandardAOEs(module, AID.Spikesicle, new AOEShapeRect(64.5f, 5), maxCasts: 3);
class SpikesicleKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Spikesicle, 10, maxCasts: 3, shape: new AOEShapeRect(64.5f, 5), kind: Components.Knockback.Kind.DirForward);
class DeathSentence(BossModule module) : Components.SingleTargetCast(module, AID.DeathSentence);
class SpikedTail(BossModule module) : Components.SingleTargetCast(module, AID.SpikedTail);
class Hellfire(BossModule module) : Components.RaidwideCast(module, AID.Hellfire);
class DiamondDust(BossModule module) : Components.RaidwideCast(module, AID.DiamondDust);
class JudgmentBolt(BossModule module) : Components.RaidwideCast(module, AID.JudgmentBolt);
class IceStormRaidwide(BossModule module) : Components.RaidwideInstant(module, AID.IceStormRaidwide, 0);
class Protostar(BossModule module) : Components.RaidwideCast(module, AID.Protostar);
class DarkMatter(BossModule module) : Components.RaidwideCast(module, AID.DarkMatter);
class EarthenFury(BossModule module) : Components.RaidwideCast(module, AID.EarthenFury);
class MeteorImpactCocoon(BossModule module) : Components.RaidwideCast(module, AID.MeteorImpactCocoon);
class AkhRhai(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(4);
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.AkhRhai or AID.AkhRhaiRepeat))
            return;
        var pos = spell.TargetXZ;
        if (pos == default)
            pos = caster.Position;
        _aoes.Add(new(_shape, pos, default, WorldState.FutureTime(6)));
        ++NumCasts;
    }

    public override void Update() => _aoes.RemoveAll(a => a.Activation < WorldState.CurrentTime);
}

class WaterPuddles(BossModule module) : BossComponent(module)
{
    // TODO: puddle radius and Levinbolt/Hellfire timing still unverified; consider forbidden zones instead of hints
    private bool _levinCasting;
    private bool _fireCasting;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LevinboltVisual)
            _levinCasting = true;
        else if ((AID)spell.Action.ID == AID.HellfireVisual)
            _fireCasting = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LevinboltVisual)
            _levinCasting = false;
        else if ((AID)spell.Action.ID == AID.HellfireVisual)
            _fireCasting = false;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!Module.Enemies((uint)OID.WaterPuddles).Any(z => !z.IsDead))
            return;
        if (_levinCasting)
            hints.Add("Avoid puddles during lightning bolts.");
        else if (_fireCasting)
            hints.Add("Stand in puddles during hellfire.");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in Module.Enemies((uint)OID.WaterPuddles))
        {
            if (orb.IsDead)
                continue;
            if (_levinCasting)
                Arena.ZoneCircle(orb.Position, 5, ArenaColor.Danger);
            else if (_fireCasting)
                Arena.ZoneCircle(orb.Position, 5, ArenaColor.SafeFromAOE);
            else
                Arena.AddCircle(orb.Position, 5, ArenaColor.Object);
        }
    }
}

class Fetters(BossModule module) : BossComponent(module)
{
    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (status.ID == (uint)SID.Fetters && Raid.FindSlot(actor.InstanceID) >= 0)
            Arena.Bounds = new ArenaBoundsSquare(30);
    }
}

class TailSlapArena(BossModule module) : BossComponent(module)
{
    private static readonly ArenaBoundsSquare Phase3Base = new(30);
    private PolygonClipper.Operand _arena = new([.. CurveApprox.Rect(new WDir(0, 1), 30, 30)]);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00400080u || actor.OID != (uint)OID.Platform || actor.Position.AlmostEqual(default, 1f))
            return;
        var offset = actor.Position.Rounded() - Arena.Center;
        var tile = new PolygonClipper.Operand([.. CurveApprox.Rect(new WDir(0, 1), 10, 10).Select(c => c + offset)]);
        _arena = new(Phase3Base.Clipper.Difference(_arena, tile));
        Arena.Bounds = new ArenaBoundsCustom(30, Phase3Base.Clipper.Simplify(_arena));
    }
}

class T03ShinryuStates : StateMachineBuilder
{
    public T03ShinryuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<WaterPuddles>()
            .ActivateOnEnter<Levinbolt>()
            .ActivateOnEnter<EarthBreathBait>()
            .ActivateOnEnter<EarthBreathAOE>()
            .ActivateOnEnter<AkhMornStack>()
            .ActivateOnEnter<IcicleAdds>()
            .ActivateOnEnter<Spikesicle>()
            .ActivateOnEnter<SpikesicleKnockback>()
            .ActivateOnEnter<AkhRhai>()
            .ActivateOnEnter<BurningChains>()
            .ActivateOnEnter<Cocoons>()
            .ActivateOnEnter<DragonAdds>()
            .ActivateOnEnter<MeteorImpact>()
            .ActivateOnEnter<MeteorImpactCocoon>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<BlazingTrail>()
            .ActivateOnEnter<Fetters>()
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<IceStormRaidwide>()
            .ActivateOnEnter<Protostar>()
            .ActivateOnEnter<DarkMatter>()
            .ActivateOnEnter<JudgmentBolt>()
            .ActivateOnEnter<EarthenFury>()
            .ActivateOnEnter<DeathSentence>()
            .ActivateOnEnter<SpikedTail>()
            .ActivateOnEnter<GyreCharge>()
            .ActivateOnEnter<TailSlap>()
            .ActivateOnEnter<TailSlapArena>()
            .ActivateOnEnter<Dragonfist>()
            .ActivateOnEnter<DiamondDust>()
            .ActivateOnEnter<EarthenFurySmash>()
            .ActivateOnEnter<SuperCyclone>()
            .ActivateOnEnter<AerialBlast>()
            .ActivateOnEnter<Hypernova>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 239, NameID = 5640)] // TODO: clear Incomplete after puddle/arena/Fetters verify
public class T03Shinryu(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsSquare(20, MapResolution: 0.3f));
