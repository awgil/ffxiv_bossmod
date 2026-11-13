namespace BossMod.Endwalker.Trial.T04Barbariccia;

public enum OID : uint
{
    Boss = 0x39D1, // Teamcraft index
    BarbaricciaShadow = 0x39D2, // source of Brutal Gust and Winding Gale charges
    StiffBreeze = 0x39D3, // moves outward from the centre at ~1.45y/s, stops once it has hit someone
    Tangle = 0x3973, // x4, 12y out on the cardinals
    Helper = 0x233C, // Helper type
}

public enum AID : uint
{
    AutoAttack = 30072, // Boss->player, no cast, single-target
    Teleport = 29569, // Boss->location, no cast, single-target
    TeleportP2 = 30082, // Boss->location, no cast, single-target
    VoidAeroIV = 30134, // Boss->self, 5.0s cast, range 40 circle, raidwide
    VoidAeroIII = 30135, // Boss->player, 5.0s cast, range 5 circle, shared tankbuster

    SavageBarberyLineVisual = 29838, // Boss->self, 7.0s cast, single-target
    SavageBarberyLine = 30144, // Helper->location, 8.0s cast, range 40 width 12 rect
    SavageBarberyLineSword = 30145, // Helper->self, 10.1s cast, range 20 circle
    SavageBarberyDonutVisual = 29801, // Boss->self, 7.0s cast, single-target
    SavageBarberyDonut = 30138, // Helper->self, 8.0s cast, range 6-20 donut
    SavageBarberyDonutSword = 30139, // Helper->self, 10.1s cast, range 20 circle
    HairRaid = 30146, // Boss->location, 6.0s cast, single-target
    HairRaidAOE = 30147, // Helper->self, 8.0s cast, range 40 120-degree cone
    HairSpray = 30171, // Helper->player, 8.0s cast, range 4 circle
    DeadlyTwist = 30172, // Helper->player, 5.0s cast, range 6 circle

    TeasingTangles1 = 30174, // Boss->self, 4.0s cast, single-target
    TeasingTangles2 = 30175, // Boss->self, no cast, single-target
    TangleAOE = 30176, // Helper->self, 4.6s cast, range 6 circle
    Fetters = 30128, // Helper->player, no cast, single-target
    TanglePull = 30124, // Helper->player, no cast, attract to the tethered tangle
    TangleFail = 29568, // Helper->player, no cast, attract + Sustained Damage for leaving the leash
    SecretBreeze1 = 30148, // Boss->self, 3.0s cast, single-target
    SecretBreeze2 = 30149, // Helper->self, 4.0s cast, range 40 30-degree cone
    BrushWithDeath1 = 30115, // Boss->self, no cast, single-target
    BrushWithDeath2 = 30116, // Boss->self, no cast, single-target
    BrushWithDeath3 = 30117, // Boss->self, no cast, single-target

    CurlingIron = 30130, // Boss->self, 5.0s cast, single-target
    Voidstrom = 30141, // Helper->self, no cast, range 6 circle, 3 pulses
    CurlingIronAOE = 30178, // Helper->self, no cast, range 40 circle, knockback 4 + stun
    CatabasisVisual1 = 30179, // Boss->self, no cast, single-target
    CatabasisVisual2 = 29832, // Boss->self, no cast, single-target
    Catabasis = 30140, // Helper->self, 8.0s cast, range 20 circle, raidwide

    BrutalRush = 30150, // Boss->player, no cast, single-target, rushes 1-3
    BrutalRushLast = 30151, // Boss->player, no cast, single-target, rush 4
    BrutalGust = 30152, // BarbaricciaShadow->self, 2.0s cast, range 40 width 4 rect

    WarningGaleVisual = 30086, // Boss->self, no cast, single-target
    WarningGale = 30153, // Helper->self, 5.0s cast, range 6 circle
    WindingGaleTeleport = 30154, // BarbaricciaShadow->location, no cast, single-target
    WindingGaleArc = 29831, // Helper->location, 5.0s cast, half donut around the location
    WindingGaleCharge = 30155, // Helper->location, 2.5s cast, width 4 rect charge
    WindingGaleChargeVisual = 30156, // BarbaricciaShadow->location, no cast, width 4 rect charge
    WindingGaleReturn = 30157, // BarbaricciaShadow->location, no cast, single-target

    BoulderVisual = 30108, // Boss->self, no cast, single-target
    Boulder = 30167, // Helper->location, 4.0s cast, range 10 circle
    BoulderBreakVisual = 29570, // Boss->self, no cast, single-target
    BoulderBreak = 29647, // Helper->player, 5.0s cast, range 5 circle, tankbuster on both tanks

    KnuckleDrumVisual = 30103, // Boss->self, no cast, single-target
    KnuckleDrum = 30164, // Helper->self, no cast, range 40 circle, 10 hits
    KnuckleDrumLast = 30165, // Helper->self, no cast, range 40 circle

    BoldBoulderVisual = 30106, // Boss->self, no cast, single-target
    BoldBoulder = 30166, // Helper->player, 8.0s cast, range 40 circle, damage falls off with distance
    Trample = 30170, // Boss->player, no cast, range 6 circle, stack
    BlusteryRuler = 30160, // Helper->self, 5.0s cast, range 6 circle
    BlusteryRulerVisual = 30097, // Boss->self, no cast, single-target
    Tousle = 30161, // StiffBreeze->self, no cast, range 2 circle
    TornadoChainVisual1 = 30094, // Boss->self, no cast, single-target
    TornadoChainVisual2 = 30095, // Boss->self, no cast, single-target
    TornadoChainInner = 30158, // Helper->self, 4.0s cast, range 11 circle
    TornadoChainOuter = 30159, // Helper->self, 6.5s cast, range 11-20 donut

    BlowAwayRaidwide = 30101, // Boss->self, no cast, range 20 circle
    BlowAwayPuddle = 30163, // Helper->self, 4.0s cast, range 6 circle, drops under two players every 2s
    Touchdown = 29805, // Helper->location, 6.2s cast, range 40 circle, knockback 7
    TouchdownVisual = 29804, // Boss->location, no cast, single-target

    VoidAero1 = 29802, // Boss->self, 3.0s cast, single-target
    VoidAero2 = 29803, // Helper->location, 4.0s cast, range 4 circle
    Impact = 30168, // Helper->self, 6.2s cast, range 6 circle
    ImpactKnockback = 30169, // Helper->self, 6.2s cast, range 20 circle
    DryBlows = 30099, // Boss->self, no cast, range 20 circle
    DryBlowsPuddle = 30162, // Helper->location, 3.0s cast, range 3 circle
}

public enum IconID : uint
{
    DeadlyTwist = 62, // player
    Trample = 100, // player
    BoulderBreak = 343, // player
    BoldBoulder = 346, // player
    Boulder = 352, // player
    HairSpray = 373, // player
}

public enum TetherID : uint
{
    Tangle = 199, // player->Tangle
}

class VoidAeroIV(BossModule module) : Components.RaidwideCast(module, AID.VoidAeroIV);
class VoidAeroIII(BossModule module) : Components.CastSharedTankbuster(module, AID.VoidAeroIII, 5);

class SavageBarberyLine(BossModule module) : Components.StandardAOEs(module, AID.SavageBarberyLine, new AOEShapeRect(40, 6));
class SavageBarberyLineSword(BossModule module) : Components.StandardAOEs(module, AID.SavageBarberyLineSword, 20);
class SavageBarberyDonut(BossModule module) : Components.StandardAOEs(module, AID.SavageBarberyDonut, new AOEShapeDonut(6, 20));
class SavageBarberyDonutSword(BossModule module) : Components.StandardAOEs(module, AID.SavageBarberyDonutSword, 20);
class HairRaid(BossModule module) : Components.StandardAOEs(module, AID.HairRaidAOE, new AOEShapeCone(40, 60.Degrees()));
class HairSpray(BossModule module) : Components.SpreadFromCastTargets(module, AID.HairSpray, 4);
class DeadlyTwist(BossModule module) : Components.StackWithCastTargets(module, AID.DeadlyTwist, 6);

class TangleAOE(BossModule module) : Components.StandardAOEs(module, AID.TangleAOE, 6);

class TangleLeash(BossModule module) : BossComponent(module)
{
    public const float Radius = 8;
    private readonly Actor?[] _tangles = new Actor?[PartyState.MaxPartySize];

    public Actor? TangleFor(int slot) => slot >= 0 && slot < _tangles.Length ? _tangles[slot] : null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (TangleFor(slot) is { } tangle)
            hints.Add("Stay near tangle!", !actor.Position.InCircle(tangle.Position, Radius));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (TangleFor(slot) is { } tangle)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(tangle.Position, Radius - 0.5f));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (TangleFor(pcSlot) is { } tangle)
            Arena.AddCircle(tangle.Position, Radius, ArenaColor.Safe);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tangle && Raid.TryFindSlot(source, out var slot) && slot < _tangles.Length)
            _tangles[slot] = WorldState.Actors.Find(tether.Target);
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Tangle && Raid.TryFindSlot(source, out var slot) && slot < _tangles.Length)
            _tangles[slot] = null;
    }
}

class SecretBreeze(BossModule module) : Components.StandardAOEs(module, AID.SecretBreeze2, new AOEShapeCone(40, 15.Degrees()), 8);

class Voidstrom(BossModule module) : Components.GenericAOEs(module, AID.Voidstrom)
{
    private static readonly AOEShapeCircle _shape = new(6);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(_shape, Arena.Center, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CurlingIron)
        {
            NumCasts = 0;
            _activation = Module.CastFinishAt(spell, 6.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && ++NumCasts >= 3)
            _activation = default;
    }
}

class CurlingIronKnockback(BossModule module) : Components.Knockback(module, AID.CurlingIronAOE, true)
{
    private DateTime _activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(Arena.Center, 4, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CurlingIron)
            _activation = Module.CastFinishAt(spell, 8.5f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            _activation = default;
    }
}

class Catabasis(BossModule module) : Components.RaidwideCast(module, AID.Catabasis, "Raidwide, mitigate!")
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        // TODO: wind border radius is a guess: nobody touched it in the 11-07 run, the furthest anyone stood after the cutscene was 15y
        if (spell.Action == WatchedAction)
            Arena.Bounds = new ArenaBoundsCircle(15);
    }
}

class BrutalGust(BossModule module) : Components.StandardAOEs(module, AID.BrutalGust, new AOEShapeRect(40, 2));
class WarningGale(BossModule module) : Components.StandardAOEs(module, AID.WarningGale, 6);
class WindingGaleArc(BossModule module) : Components.StandardAOEs(module, AID.WindingGaleArc, new AOEShapeDonutSector(9, 11, 90.Degrees()));
class WindingGaleCharge(BossModule module) : Components.ChargeAOEs(module, AID.WindingGaleCharge, 2);
class Boulder(BossModule module) : Components.StandardAOEs(module, AID.Boulder, 10);
class BoulderBreak(BossModule module) : Components.BaitAwayCast(module, AID.BoulderBreak, new AOEShapeCircle(5), true);

class KnuckleDrum(BossModule module) : Components.CastCounter(module, AID.KnuckleDrumLast)
{
    private DateTime _firstHit;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_firstHit != default)
            hints.Add("Raidwide x11");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_firstHit != default)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), _firstHit, AIHints.PredictedDamageType.Raidwide, 11, _firstHit.AddSeconds(7.7f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.KnuckleDrumVisual)
            _firstHit = WorldState.FutureTime(2.2f);
        else if (spell.Action == WatchedAction)
            _firstHit = default;
    }
}

class BoldBoulderTrample(BossModule module) : Components.UniformStackSpread(module, 6, 14, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BoldBoulder && WorldState.Actors.Find(spell.TargetID) is { } target)
            AddSpread(target, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BoldBoulder)
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Trample)
            AddStack(actor, WorldState.FutureTime(7));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Trample)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

class BlusteryRuler(BossModule module) : Components.StandardAOEs(module, AID.BlusteryRuler, 6);

class StiffBreeze(BossModule module) : Components.Voidzone(module, 2, OID.StiffBreeze, moveHintLength: 3)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Tousle)
            RemoveSource(caster);
    }
}

class TornadoChain(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(11), new AOEShapeDonut(11, 20)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TornadoChainInner)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.TornadoChainInner => 0,
            AID.TornadoChainOuter => 1,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2.5f));
    }
}

class BlowAwayPuddle(BossModule module) : Components.StandardAOEs(module, AID.BlowAwayPuddle, 6);

class Touchdown(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Touchdown, 7)
{
    public override PointTest DestinationUnsafe(int slot, Actor actor)
        => Module.FindComponent<TangleLeash>()?.TangleFor(slot) is { } tangle
            ? base.DestinationUnsafe(slot, actor) | ShapeDistance.InvertedCircle(tangle.Position, TangleLeash.Radius).Discretize()
            : base.DestinationUnsafe(slot, actor);
}

// TODO: not reached on the 11-07 run (boss died first): shapes from the Action sheet, knockback distance from Ex4, timings unverified
class VoidAero(BossModule module) : Components.StandardAOEs(module, AID.VoidAero2, 4);
class Impact(BossModule module) : Components.StandardAOEs(module, AID.Impact, 6);
class ImpactKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ImpactKnockback, 6);
class DryBlowsPuddle(BossModule module) : Components.StandardAOEs(module, AID.DryBlowsPuddle, 3);

class T04BarbaricciaStates : StateMachineBuilder
{
    public T04BarbaricciaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidAeroIV>()
            .ActivateOnEnter<VoidAeroIII>()
            .ActivateOnEnter<SavageBarberyLine>()
            .ActivateOnEnter<SavageBarberyLineSword>()
            .ActivateOnEnter<SavageBarberyDonut>()
            .ActivateOnEnter<SavageBarberyDonutSword>()
            .ActivateOnEnter<HairRaid>()
            .ActivateOnEnter<HairSpray>()
            .ActivateOnEnter<DeadlyTwist>()
            .ActivateOnEnter<TangleAOE>()
            .ActivateOnEnter<TangleLeash>()
            .ActivateOnEnter<SecretBreeze>()
            .ActivateOnEnter<Voidstrom>()
            .ActivateOnEnter<CurlingIronKnockback>()
            .ActivateOnEnter<Catabasis>()
            .ActivateOnEnter<BrutalGust>()
            .ActivateOnEnter<WarningGale>()
            .ActivateOnEnter<WindingGaleArc>()
            .ActivateOnEnter<WindingGaleCharge>()
            .ActivateOnEnter<Boulder>()
            .ActivateOnEnter<BoulderBreak>()
            .ActivateOnEnter<KnuckleDrum>()
            .ActivateOnEnter<BoldBoulderTrample>()
            .ActivateOnEnter<BlusteryRuler>()
            .ActivateOnEnter<StiffBreeze>()
            .ActivateOnEnter<TornadoChain>()
            .ActivateOnEnter<BlowAwayPuddle>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<VoidAero>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<ImpactKnockback>()
            .ActivateOnEnter<DryBlowsPuddle>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 870, NameID = 10298)]
public class T04Barbariccia(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
