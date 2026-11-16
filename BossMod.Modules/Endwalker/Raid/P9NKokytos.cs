namespace BossMod.Endwalker.Raid.P9NKokytos;

public enum OID : uint
{
    Boss = 0x3ED5, // R9.500, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    Comet = 0x3ED6, // R2.400, x3
    FireSphere = 0x3FCD, // R1.500, x4
    IceSphere = 0x3FCE, // R0.700, x4
    Exit = 0x1E850B, // R0.500, x1, EventObj type
    Actor1EA1A1 = 0x1EA1A1, // R2.000, x1, EventObj type
    CharybdisPuddle = 0x1EB881, // R0.500, x4, EventObj type, left under each charybdis target for 20s
}

public enum AID : uint
{
    AutoAttack = 33045, // Boss->player, no cast, single-target
    GluttonysAugur = 33046, // Boss->self, 5.0s cast, single-target
    GluttonysAugurAOE = 33047, // Helper->self, no cast, range 60 circle
    RaveningMage = 33048, // Boss->self, 4.0s cast, single-target
    RaveningMartialist = 33049, // Boss->self, 4.0s cast, single-target
    RaveningBeast = 33050, // Boss->self, 3.0+1.0s cast, single-target
    AutoAttack1 = 33052, // Boss->player, no cast, single-target
    Dualspell = 33053, // Boss->self, 5.0s cast, single-target
    FireIIISmall = 33054, // Helper->players, 7.0s cast, range 6 circle spread
    BlizzardIIIOut = 33055, // Helper->self, 7.0s cast, range 14-30 donut
    FireIIIBig = 33056, // Helper->players, 7.0s cast, range 12 circle spread
    BlizzardIIIIn = 33057, // Helper->self, 7.0s cast, range 8-30 donut
    Ability33058 = 33058, // Helper->self, no cast, single-target
    Ability33059 = 33059, // Helper->self, no cast, single-target
    Disgorge = 33060, // Boss->self, no cast, single-target
    Disgorge1 = 33061, // Boss->self, no cast, single-target
    Disgorge2 = 33062, // Boss->self, no cast, single-target
    AutoAttack2 = 33063, // Boss->player, no cast, single-target
    ArchaicRockbreaker = 33064, // Boss->location, 5.0s cast, range 6 circle
    Shockwave = 33065, // Helper->self, no cast, range 60 circle, knockback 10 from the centre
    ArchaicRockbreakerAOE = 33066, // Helper->location, 7.0s cast, range 8 circle, two waves 5s apart
    InsideRoundhouse = 33067, // Boss->self, no cast, range 12 circle
    OutsideRoundhouse = 33068, // Boss->self, no cast, range 8-20 donut
    SwingingKickFront = 33069, // Boss->self, 1.0s cast, range 40 180-degree cone
    SwingingKickRear = 33070, // Boss->self, 1.0s cast, range 40 180-degree cone
    ArchaicDemolish = 33071, // Boss->self, 7.5+1.0s cast, single-target
    ArchaicDemolishAOE = 33072, // Helper->players, 8.5s cast, range 6 circle, healer stacks
    AscendantFist = 33073, // Boss->player, 5.0s cast, single-target
    Charybdis = 33074, // Boss->self, 7.0+1.0s cast, single-target
    CharybdisAOE = 33075, // Helper->players, 8.0s cast, range 6 circle spread, leaves a puddle
    Comet = 33076, // Boss->self, 8.0+1.0s cast, single-target
    CometImpact = 33077, // Comet->self, 8.0s cast, range 60 circle, proximity
    CometBurst = 33078, // Comet->self, 6.0s cast, range 10 circle
    Touchdown = 33079, // Boss->location, no cast, range 60 circle, proximity
    BeastlyRoar = 33080, // Boss->self, 4.0s cast, range 60 circle
    GluttonousRampage = 33081, // Boss->self, 8.0s cast, single-target
    RavenousBite = 33082, // Boss->player, no cast, single-target
    EclipticMeteor = 33083, // Boss->self, 7.0+2.0s cast, single-target
    EclipticMeteorVisual = 33084, // Helper->self, no cast, single-target
    BeastlyBile = 33085, // Boss->self, no cast, single-target
    BeastlyBileAOE = 33086, // Helper->player, 7.0s cast, range 6 circle spread
    PulverizingPounce = 33087, // Boss->players, 5.0s cast, range 6 circle stack
    AutoAttack3 = 33088, // Boss->player, no cast, single-target
    GlobalSpell = 33089, // Boss->self, 6.0s cast, single-target
    GlobalSpellAOE = 33090, // Helper->self, no cast, range 60 circle
    CometBurstInstant = 33091, // Comet->self, no cast, single-target
    Ability33092 = 33092, // Boss->location, no cast, single-target
    TouchdownMarker = 33094, // Helper->self, 13.0s cast, range 60 circle, proximity from the centre
    FrontCombinationIn = 33095, // Boss->self, 5.0s cast, range 12 circle
    FrontCombinationOut = 33096, // Boss->self, 5.0s cast, range 8-20 donut
    RearCombinationIn = 33097, // Boss->self, 5.0s cast, range 12 circle
    RearCombinationOut = 33098, // Boss->self, 5.0s cast, range 8-20 donut
    FireIII = 33102, // Boss->self, 5.0s cast, single-target
    BlizzardIII = 33103, // Boss->self, 5.0s cast, single-target
    EclipticMeteorAOE = 33160, // Helper->self, 1.0s cast, range 60 circle, hide behind the comet that stays
    IceflameSummoning = 34529, // Boss->self, 5.0s cast, single-target
    FireSphereSmall = 34530, // FireSphere->self, 7.0s cast, range 8 circle
    FireSphereBig = 34531, // FireSphere->self, 7.0s cast, range 16 circle
    IceSphereSmall = 34532, // IceSphere->self, 7.0s cast, range 8 circle
    IceSphereBig = 34533, // IceSphere->self, 7.0s cast, range 16 circle
    SphereVisual1 = 34599, // Helper->self, 7.0s cast, range 8 circle
    SphereVisual2 = 34600, // Helper->self, 7.0s cast, range 8 circle
    SphereVisual3 = 34601, // Helper->self, 7.0s cast, range 8 circle
    SphereVisual4 = 34602, // Helper->self, 7.0s cast, range 8 circle
}

public enum IconID : uint
{
    ArchaicDemolish = 100, // player
    CharybdisAOE = 101, // player
    PulverizingPounce = 161, // player
    AscendantFist = 218, // player
    GluttonousRampage = 314, // player
    BeastlyBile = 375, // player
    FireIIISmall = 452, // player
    FireIIIBig = 453, // player
}

public enum TetherID : uint
{
    RampageStretched = 1, // player->Boss, far enough (seen from ~13y)
    RampageTooClose = 57, // player->Boss, the tether starts like this; stretch it
}

class GluttonysAugur(BossModule module) : Components.RaidwideCast(module, AID.GluttonysAugur);
class GlobalSpell(BossModule module) : Components.RaidwideCast(module, AID.GlobalSpell);
class BeastlyRoar(BossModule module) : Components.RaidwideCast(module, AID.BeastlyRoar);
class BeastlyRoarKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.BeastlyRoar, 10);
class GluttonousRampage(BossModule module) : Components.CastHint(module, AID.GluttonousRampage, "Tankbusters on tethered tanks");

class GluttonousRampageTethers(BossModule module) : BossComponent(module)
{
    private const float StretchRadius = 15;
    private readonly Dictionary<ulong, (Actor target, bool stretched)> _tethers = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_tethers.TryGetValue(actor.InstanceID, out var t))
            hints.Add(t.stretched ? "Tankbuster on you (tether)" : "Stretch the tether!", !t.stretched);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_tethers.Count > 0)
            hints.Add($"Tankbuster on {string.Join(", ", _tethers.Values.Select(t => t.target.Name))}");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_tethers.ContainsKey(actor.InstanceID))
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.PrimaryActor.Position, StretchRadius));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (target, stretched) in _tethers.Values)
        {
            Arena.AddLine(Module.PrimaryActor.Position, target.Position, stretched ? ArenaColor.Safe : ArenaColor.Danger);
            Arena.AddCircle(target.Position, 3, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.RampageStretched or (uint)TetherID.RampageTooClose)
            _tethers[source.InstanceID] = (source, tether.ID == (uint)TetherID.RampageStretched);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.RampageStretched or (uint)TetherID.RampageTooClose && _tethers.TryGetValue(source.InstanceID, out var t) && t.stretched == (tether.ID == (uint)TetherID.RampageStretched))
            _tethers.Remove(source.InstanceID);
    }
}
class FireIIISmall(BossModule module) : Components.SpreadFromCastTargets(module, AID.FireIIISmall, 6);
class FireIIIBig(BossModule module) : Components.SpreadFromCastTargets(module, AID.FireIIIBig, 12);
class BlizzardIIIIn(BossModule module) : Components.StandardAOEs(module, AID.BlizzardIIIIn, new AOEShapeDonut(8, 30));
class BlizzardIIIOut(BossModule module) : Components.StandardAOEs(module, AID.BlizzardIIIOut, new AOEShapeDonut(14, 30));
class ArchaicRockbreaker(BossModule module) : Components.StandardAOEs(module, AID.ArchaicRockbreaker, 6);
class Shockwave(BossModule module) : Components.Knockback(module, AID.Shockwave)
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Module.Center, 10, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArchaicRockbreaker)
            _activation = Module.CastFinishAt(spell, 1.85f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Shockwave)
        {
            ++NumCasts;
            _activation = default;
        }
    }
}

class ArchaicDemolish(BossModule module) : Components.StackWithCastTargets(module, AID.ArchaicDemolishAOE, 6, 2);
class AscendantFist(BossModule module) : Components.SingleTargetCast(module, AID.AscendantFist);
class Charybdis(BossModule module) : Components.SpreadFromCastTargets(module, AID.CharybdisAOE, 6);
class CharybdisPuddle(BossModule module) : Components.Voidzone(module, 6, (uint)OID.CharybdisPuddle);
class BeastlyBile(BossModule module) : Components.SpreadFromCastTargets(module, AID.BeastlyBileAOE, 6);
class PulverizingPounce(BossModule module) : Components.StackWithCastTargets(module, AID.PulverizingPounce, 6);
class TouchdownMarker(BossModule module) : Components.ProximityAOEs(module, AID.TouchdownMarker, 15)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).Where(a => a.Activation <= WorldState.FutureTime(5));
}
class CometImpact(BossModule module) : Components.ProximityAOEs(module, AID.CometImpact, 15);
class CometBurst(BossModule module) : Components.StandardAOEs(module, AID.CometBurst, 10);
class FireSphereSmall(BossModule module) : Components.StandardAOEs(module, AID.FireSphereSmall, 8);
class FireSphereBig(BossModule module) : Components.StandardAOEs(module, AID.FireSphereBig, 16);
class IceSphereSmall(BossModule module) : Components.StandardAOEs(module, AID.IceSphereSmall, 8);
class IceSphereBig(BossModule module) : Components.StandardAOEs(module, AID.IceSphereBig, 16);

class ArchaicRockbreakerWaves(BossModule module) : Components.StandardAOEs(module, AID.ArchaicRockbreakerAOE, 8)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var all = base.ActiveAOEs(slot, actor).ToList();
        if (all.Count == 0)
            return all;
        var first = all.Min(a => a.Activation);
        return all.Where(a => a.Activation < first.AddSeconds(1));
    }
}

class Combination(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _in = new(12);
    private static readonly AOEShapeDonut _out = new(8, 20);
    private static readonly AOEShapeCone _kick = new(40, 90.Degrees());
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        (AOEShape? first, Angle kick) = (AID)spell.Action.ID switch
        {
            AID.FrontCombinationIn => (_in, default),
            AID.FrontCombinationOut => (_out, default),
            AID.RearCombinationIn => (_in, 180.Degrees()),
            AID.RearCombinationOut => (_out, 180.Degrees()),
            _ => ((AOEShape?)null, default(Angle))
        };
        if (first != null)
        {
            _aoes.Add(new(first, caster.Position, default, Module.CastFinishAt(spell)));
            _aoes.Add(new(_kick, caster.Position, spell.Rotation + kick, Module.CastFinishAt(spell, 2.8f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.InsideRoundhouse or AID.OutsideRoundhouse or AID.SwingingKickFront or AID.SwingingKickRear && _aoes.Count > 0)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}

class EclipticMeteor(BossModule module) : Components.GenericLineOfSightAOE(module, AID.EclipticMeteorAOE, 60, false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EclipticMeteor)
            Modify(Module.Center, Module.Enemies(OID.Comet).Where(c => !c.IsDead && c.ModelState.AnimState2 != 1).Select(c => (c.Position, c.HitboxRadius)), Module.CastFinishAt(spell, 2.6f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.EclipticMeteorAOE)
            Modify(null, []);
    }
}

class P9NKokytosStates : StateMachineBuilder
{
    public P9NKokytosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GluttonysAugur>()
            .ActivateOnEnter<GlobalSpell>()
            .ActivateOnEnter<BeastlyRoar>()
            .ActivateOnEnter<BeastlyRoarKnockback>()
            .ActivateOnEnter<GluttonousRampage>()
            .ActivateOnEnter<GluttonousRampageTethers>()
            .ActivateOnEnter<FireIIISmall>()
            .ActivateOnEnter<FireIIIBig>()
            .ActivateOnEnter<BlizzardIIIIn>()
            .ActivateOnEnter<BlizzardIIIOut>()
            .ActivateOnEnter<ArchaicRockbreaker>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<ArchaicRockbreakerWaves>()
            .ActivateOnEnter<ArchaicDemolish>()
            .ActivateOnEnter<AscendantFist>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<CharybdisPuddle>()
            .ActivateOnEnter<BeastlyBile>()
            .ActivateOnEnter<PulverizingPounce>()
            .ActivateOnEnter<TouchdownMarker>()
            .ActivateOnEnter<CometImpact>()
            .ActivateOnEnter<CometBurst>()
            .ActivateOnEnter<EclipticMeteor>()
            .ActivateOnEnter<Combination>()
            .ActivateOnEnter<FireSphereSmall>()
            .ActivateOnEnter<FireSphereBig>()
            .ActivateOnEnter<IceSphereSmall>()
            .ActivateOnEnter<IceSphereBig>();
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 936, NameID = 12369)]
public class P9NKokytos(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
