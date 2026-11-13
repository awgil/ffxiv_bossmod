namespace BossMod.Endwalker.Variant.V02MountRokkon.V021Yozakura;

public enum OID : uint
{
    Boss = 0x3EDB, // R3.450, x1
    MirroredYozakura = 0x3EDC, // R3.450, x0 (spawn during fight)
    MudBubble = 0x3EDD, // R4.000, x0 (spawn during fight)
    Shibamaru = 0x3EDE, // R0.400, x0 (spawn during fight)
    Kuromaru = 0x3EDF, // R0.400, x0 (spawn during fight)
    Shiromaru = 0x3EE0, // R0.400, x0 (spawn during fight)
    AccursedSeedling = 0x3EE1, // R0.750, x0 (spawn during fight)
    LivingGaol = 0x3EE2, // R3.700, x0 (spawn during fight)
    AutumnalTempest = 0x3EE3, // R0.800, x0 (spawn during fight)
    ShishuYamabiko = 0x3F00, // R0.800
    Helper2 = 0x3F53, // R0.500
    Fire = 0x1EB88B, // R0.500, EventObj type
    Wind = 0x1EB88C, // R0.500, EventObj type
    Water = 0x1EB88D, // R0.500, EventObj type
    Thunder = 0x1EB88E, // R0.500, EventObj type
    MudVoidzone = 0x1EB907, // R0.500, EventObj type
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 34322, // Boss->location, no cast, single-target
    GloryNeverlasting = 33705, // Boss->player, 5.0s cast, single-target, tankbuster
    ArtOfTheFireblossom = 33640, // Boss->self, 3.0s cast, range 9 circle
    ArtOfTheWindblossom = 33641, // Boss->self, 5.0s cast, range 5-60 donut
    KugeRantsui = 33706, // Boss->self, 5.0s cast, range 60 circle, raidwide
    OkaRanman = 33646, // Boss->self, 5.0s cast, range 60 circle, raidwide
    SealOfRiotousBloom = 33652, // Boss->self, 5.0s cast, single-target
    SealOfTheFireblossomVisual = 33658, // Boss->location, no cast, single-target
    SealOfTheWindblossomVisual = 33659, // Boss->location, no cast, single-target
    SealOfTheLevinblossomVisual = 33660, // Boss->location, no cast, single-target
    SealOfTheRainblossomVisual = 33661, // Boss->location, no cast, single-target
    SealOfTheFireblossom = 33653, // Helper->self, 2.0s cast, range 9 circle
    SealOfTheWindblossom = 33654, // Helper->self, 2.0s cast, range 5-60 donut
    SealOfTheRainblossom = 33655, // Helper->self, 2.0s cast, range 70 45-degree cone, intercardinals
    SealOfTheLevinblossom = 33656, // Helper->self, 1.8s cast, range 70 45-degree cone, cardinals
    SealOfTheFleeting = 33657, // Boss->self, 3.0s cast, single-target, tethers the petal piles
    SeasonsOfTheFleetingVisual1 = 33665, // Boss->self, 10.0s cast, single-target, telegraphs four sequential AoEs
    SeasonsOfTheFleetingVisual2 = 33666, // Boss->self, no cast, single-target
    FireAndWaterTelegraph = 33667, // Helper->self, 2.0s cast, range 46 width 5 rect
    EarthAndLightningTelegraph = 33668, // Helper->self, 2.0s cast, range 60 45-degree cone
    SeasonOfFire = 33669, // Helper->self, 0.8s cast, range 46 width 5 rect
    SeasonOfWater = 33670, // Helper->self, 0.8s cast, range 46 width 5 rect
    SeasonOfLightning = 33671, // Helper->self, 0.8s cast, range 70 45-degree cone
    SeasonOfEarth = 33672, // Helper->self, 0.8s cast, range 70 45-degree cone
    WindblossomWhirlVisual = 33679, // Boss->self, 3.0s cast, single-target, left windy route
    WindblossomWhirl1 = 33680, // Helper->self, 5.0s cast, range 5-60 donut
    WindblossomWhirl2 = 34544, // Helper->self, 3.0s cast, range 5-60 donut
    LevinblossomStrikeVisual = 33681, // Boss->self, 2.3s cast, single-target
    LevinblossomStrike = 33682, // Helper->location, 3.0s cast, range 3 circle
    DriftingPetals = 33683, // Boss->self, 5.0s cast, range 60 circle, knockback 15
    Bunshin = 33662, // Boss->self, 5.0s cast, single-target, left rainy route
    ShadowflightVisual = 33663, // Boss->self, 3.0s cast, single-target
    Shadowflight = 33664, // MirroredYozakura->self, 2.5s cast, range 10 width 6 rect
    MudrainVisual = 33673, // Boss->self, 3.0s cast, single-target
    Mudrain = 33674, // Helper->location, 3.8s cast, range 5 circle
    IcebloomVisual = 33675, // Boss->self, 3.0s cast, single-target
    Icebloom = 33676, // Helper->location, 3.0s cast, range 6 circle
    MudPieVisual = 33677, // Boss->self, 3.0s cast, single-target
    MudPie = 33678, // MudBubble->self, 4.0s cast, range 60 width 6 rect
    ArtOfTheFluff1 = 33693, // Shibamaru/Kuromaru->self, 6.5s cast, range 60 circle, gaze, middle rope pulled route
    ArtOfTheFluff2 = 33694, // Shiromaru->self, 6.5s cast, range 60 circle, gaze
    FireblossomFlareVisual = 33695, // Boss->self, 3.0s cast, single-target
    FireblossomFlare = 33696, // Helper->location, 3.0s cast, range 6 circle
    DondenGaeshi = 33692, // Boss->self, 3.0s cast, single-target, indicates platforms
    SilentWhistle = 33691, // Boss->self, 3.0s cast, single-target, summons dogs
    LevinblossomLanceCW = 33687, // Boss->self, 5.0s cast, single-target, middle rope unpulled route
    LevinblossomLanceCCW = 33688, // Boss->self, 5.0s cast, single-target
    LevinblossomLanceFirst = 33689, // Helper->self, 5.8s cast, range 60 width 7 rect
    LevinblossomLanceRest = 33690, // Helper->self, no cast, range 60 width 7 rect
    TatamiTrap = 33684, // Boss->self, 3.0s cast, single-target
    TatamiGaeshiVisual = 33685, // Boss->self, 3.0s cast, single-target
    TatamiGaeshi = 33686, // Helper->self, 3.8s cast, range 40 width 10 rect
    Mebuki = 33697, // Boss->self, 3.0s cast, single-target, right route without dogu
    RockRootArrangementVisual = 33700, // Boss->self, 5.0s cast, single-target
    RockRootArrangementFirst = 33701, // Helper->location, 3.0s cast, range 4 circle
    RockRootArrangementRest = 33702, // Helper->location, no cast, range 4 circle
    BehindBarbs = 33698, // AccursedSeedling->player, no cast, single-target
    Explosion = 33699, // LivingGaol->player, 15.0s cast, single-target, gaol explodes if not killed
    Witherwind = 33703, // Boss->self, 3.0s cast, single-target, right route with dogu
    CuttingLeaves = 33704, // Helper->player, no cast, single-target, touched whirlwind
}

public enum IconID : uint
{
    RootArrangement = 197, // player
}

class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GloryNeverlasting && Arena.Bounds.Radius > 20)
            _aoe = new(new AOEShapeRect(20, 20, 20), Arena.Center, default, Module.CastFinishAt(spell, 3.7f), Inverted: true);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if ((index is 0x35 or 0x36) && state == 0x00020001)
        {
            _aoe = null;
            Arena.Bounds = new ArenaBoundsSquare(20);
        }
    }
}

class SealOfRiotousBloom(BossModule module) : Components.GenericAOEs(module)
{
    private enum Element { Fire, Water, Thunder, Wind }

    private static readonly AOEShapeCircle _circle = new(9);
    private static readonly AOEShapeDonut _donut = new(5, 60);
    private static readonly AOEShapeCone _cone = new(70, 22.5f.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Element> _elements = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(5);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        Element? element = (OID)actor.OID switch
        {
            OID.Fire => Element.Fire,
            OID.Water => Element.Water,
            OID.Wind => Element.Wind,
            OID.Thunder => Element.Thunder,
            _ => null
        };
        if (element is not Element e)
            return;
        if (state == 0x00100020)
            _elements.Add(e);
        else if (state == 0x00400080 && _elements.Contains(e))
            Activate(e, WorldState.FutureTime(8.1f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count > 4 && (AID)spell.Action.ID is AID.SealOfTheFireblossom or AID.SealOfTheWindblossom)
            _aoes.RemoveRange(0, 5);
    }

    private void Activate(Element element, DateTime activation)
    {
        switch (element)
        {
            case Element.Fire:
            case Element.Wind:
                _aoes.Add(new(element == Element.Fire ? _circle : _donut, Arena.Center, default, activation));
                break;
            case Element.Thunder:
            case Element.Water:
                var offset = element == Element.Thunder ? 0 : 45;
                for (var i = 0; i < 4; ++i)
                    _aoes.Add(new(_cone, Arena.Center, (offset + i * 90).Degrees(), activation));
                break;
        }
        _elements.Remove(element);
        if (_aoes.Count == 5 && _elements.Count > 0)
        {
            var next = WorldState.FutureTime(16.3f);
            for (var i = _elements.Count - 1; i >= 0; --i)
                Activate(_elements[i], next);
        }
    }
}

class SeasonsOfTheFleeting(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        for (var i = 0; i < Math.Min(_aoes.Count, 8); ++i)
            yield return _aoes.Count > 4 && i < 4 ? _aoes[i] with { Color = ArenaColor.Danger } : _aoes[i];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FireAndWaterTelegraph:
                _aoes.Add(new(new AOEShapeRect(46, 2.5f), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 7.4f)));
                break;
            case AID.EarthAndLightningTelegraph:
                _aoes.Add(new(new AOEShapeCone(70, 22.5f.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 7.4f)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.SeasonOfFire or AID.SeasonOfWater or AID.SeasonOfLightning or AID.SeasonOfEarth && _aoes.Count > 0)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}

class WindblossomWhirl(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private int _remaining;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WindblossomWhirlVisual)
        {
            _aoe = new(new AOEShapeDonut(5, 60), Arena.Center, default, Module.CastFinishAt(spell, 6.3f));
            _remaining = 5;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.WindblossomWhirl1 or AID.WindblossomWhirl2 && _aoe != null)
        {
            ++NumCasts;
            if (--_remaining == 0)
                _aoe = null;
        }
    }
}

class LevinblossomLance(BossModule module) : Components.GenericRotatingAOE(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;
        if (aid is AID.LevinblossomLanceCW or AID.LevinblossomLanceCCW)
            Sequences.Add(new(new AOEShapeRect(30, 3.5f, 30), spell.LocXZ, spell.Rotation, aid == AID.LevinblossomLanceCW ? -28.Degrees() : 28.Degrees(), Module.CastFinishAt(spell, 0.8f), 1, 5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.LevinblossomLanceFirst or AID.LevinblossomLanceRest && Sequences.Count > 0)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

class DriftingPetals(BossModule module) : Components.KnockbackFromCastTarget(module, AID.DriftingPetals, 15, ignoreImmunes: true)
{
    private readonly Mudrain? _mud = module.FindComponent<Mudrain>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var src = Sources(slot, actor).FirstOrDefault();
        if (src.Distance == 0)
            return;
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(src.Origin, 5), src.Activation);
        if (_mud != null)
            foreach (var aoe in _mud.ActiveAOEs(slot, actor))
                hints.AddForbiddenZone(ShapeDistance.Cone(src.Origin, 20, Angle.FromDirection(aoe.Origin - src.Origin), 20.Degrees()), src.Activation);
    }
}

class RootArrangement(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4), AID.RockRootArrangementFirst, AID.RockRootArrangementRest, 4, 1, 4);

class GloryNeverlasting(BossModule module) : Components.SingleTargetDelayableCast(module, AID.GloryNeverlasting);
class ArtOfTheFireblossom(BossModule module) : Components.StandardAOEs(module, AID.ArtOfTheFireblossom, new AOEShapeCircle(9));
class ArtOfTheWindblossom(BossModule module) : Components.StandardAOEs(module, AID.ArtOfTheWindblossom, new AOEShapeDonut(5, 60));
class KugeRantsui(BossModule module) : Components.RaidwideCast(module, AID.KugeRantsui);
class OkaRanman(BossModule module) : Components.RaidwideCast(module, AID.OkaRanman);
class LevinblossomStrike(BossModule module) : Components.StandardAOEs(module, AID.LevinblossomStrike, new AOEShapeCircle(3));
class Mudrain(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, AID.Mudrain, OID.MudVoidzone, 0.7f, a => a.EventState == 7);
class Icebloom(BossModule module) : Components.StandardAOEs(module, AID.Icebloom, new AOEShapeCircle(6));
class Shadowflight(BossModule module) : Components.StandardAOEs(module, AID.Shadowflight, new AOEShapeRect(10, 3));
class MudPie(BossModule module) : Components.StandardAOEs(module, AID.MudPie, new AOEShapeRect(60, 3));
class FireblossomFlare(BossModule module) : Components.StandardAOEs(module, AID.FireblossomFlare, new AOEShapeCircle(6));
class ArtOfTheFluff1(BossModule module) : Components.CastGaze(module, AID.ArtOfTheFluff1);
class ArtOfTheFluff2(BossModule module) : Components.CastGaze(module, AID.ArtOfTheFluff2);
class TatamiGaeshi(BossModule module) : Components.StandardAOEs(module, AID.TatamiGaeshi, new AOEShapeRect(40, 5));
class AccursedSeedling(BossModule module) : Components.Voidzone(module, 4, OID.AccursedSeedling);
class Witherwind(BossModule module) : Components.Voidzone(module, 3, OID.AutumnalTempest, moveHintLength: 20);

class V021YozakuraStates : StateMachineBuilder
{
    public V021YozakuraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<GloryNeverlasting>()
            .ActivateOnEnter<KugeRantsui>()
            .ActivateOnEnter<OkaRanman>()
            .ActivateOnEnter<ArtOfTheFireblossom>()
            .ActivateOnEnter<ArtOfTheWindblossom>()
            .ActivateOnEnter<SealOfRiotousBloom>()
            .ActivateOnEnter<SeasonsOfTheFleeting>()
            .ActivateOnEnter<RootArrangement>()
            .ActivateOnEnter<AccursedSeedling>()
            .ActivateOnEnter<Witherwind>()
            .ActivateOnEnter<WindblossomWhirl>()
            .ActivateOnEnter<LevinblossomStrike>()
            .ActivateOnEnter<Mudrain>()
            .ActivateOnEnter<DriftingPetals>()
            .ActivateOnEnter<Icebloom>()
            .ActivateOnEnter<Shadowflight>()
            .ActivateOnEnter<MudPie>()
            .ActivateOnEnter<FireblossomFlare>()
            .ActivateOnEnter<ArtOfTheFluff1>()
            .ActivateOnEnter<ArtOfTheFluff2>()
            .ActivateOnEnter<LevinblossomLance>()
            .ActivateOnEnter<TatamiGaeshi>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12325, SortOrder = 2)]
public class V021Yozakura(ModuleInit init) : BossModule(init, ArenaCenter(init.Primary), new ArenaBoundsSquare(init.Primary.Position.X > 700 ? 19.5f : 22.5f))
{
    private static WPos ArenaCenter(Actor primary) => primary.Position.X < -700 ? new(-775, 16) : primary.Position.X > 700 ? new(737, 220) : new(47, 93);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LivingGaol), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.PrioritizeTargetsByOID(OID.LivingGaol, 1);
    }
}
