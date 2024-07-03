namespace BossMod.Dawntrail.Quest.MSQ.TakingAStand;

public enum OID : uint
{
    Boss = 0x4211, // R=3.5
    BakoolJaJaShade = 0x4215, // R3.500, x3
    Deathwall = 0x1EBA1F,
    MagickedStandardOrange = 0x4212, // R1.0
    MagickedStandardGreen = 0x4213, // R1.0
    GrowingAOE = 0x421D, // R1.0-3.58
    Garrote = 0x4214, // R1.0
    HiredThug1 = 0x4219, // R0.5
    HiredThug2 = 0x421A, // R0.5
    HiredThug3 = 0x421B, // R0.5
    HoobigoGuardian = 0x4216, // R1.0
    HoobigoLancer = 0x4217, // R1.0
    DopproIllusionist = 0x4218, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 871, // HoobigoLancer->player, no cast, single-target
    AutoAttack3 = 873, // HiredThug3->player, no cast, single-target
    Teleport = 37089, // Boss->location, no cast, single-target
    Roar1 = 37090, // Boss->self, 5.0s cast, range 45 circle, spawns death wall
    Roar2 = 37091, // Boss->self, 5.0s cast, range 45 circle
    LethalSwipe = 37092, // Boss->self, 6.0s cast, range 45 180-degree cone
    MagickedStandard = 37098, // Boss->self, 4.0s cast, single-target
    Kickdown = 37101, // Boss->player, 5.0s cast, single-target, knockback 18, away from source
    ArcaneActivationCircle = 37099, // MagickedStandardOrange->self, 4.0s cast, range 10 circle
    ArcaneActivationDonut = 37100, // MagickedStandardGreen->self, 4.0s cast, range 3-10 donut
    FireshowerVisual = 37751, // Boss->self, 3.0s cast, single-target
    Fireshower = 37802, // Helper->location, 4.0s cast, range 6 circle
    GreatLeapVisual = 37093, // Boss->self, 8.5s cast, single-target
    GreatLeap = 37094, // Boss->location, no cast, range 18 circle
    ShadowSiblings = 37103, // Boss->self, 4.0s cast, single-target
    LethalSwipeShade = 37104, // BakoolJaJaShade->self, 5.9+0,1s cast, range 10 circle
    SelfSacrifice = 37105, // BakoolJaJaShade->self, no cast, range 10 circle

    RiotousRampageVisual1 = 37095, // Boss->location, 5.9s cast, single-target
    RiotousRampageVisual2 = 37096, // Boss->location, no cast, single-target
    RiotousRampage = 36416, // Helper->self, 7.2s cast, range 4 circle
    HeavyImpact = 36444, // Helper->self, no cast, range 40 circle, tower fail
    SupremeStandard = 37106, // Boss->self, 4.0s cast, single-target
    SupremeStandardPull = 37107, // Helper->self, no cast, range 50 circle
    SupremeStandardModelChange = 37108, // Boss->self, 1.5s cast, single-target
    MightAndMagicVisual = 35333, // Boss->self, no cast, single-target, QTE, nothing of this can be dodged
    MightAndMagic = 15552, // Helper->self, 1.4s cast, range 50 width 10 rect
    MightAndMagic2 = 13270, // Helper->self, 8.1s cast, range 50 width 10 rect
    MightAndMagic3 = 13271, // Helper->self, 8.8s cast, range 50 width 10 rect
    MightAndMagicEnrage = 15554, // Helper->self, 2.3s cast, range 50 width 10 rect, QTE failed

    TeleportAdds = 34873, // HoobigoGuardian/HoobigoLancer->location, no cast, single-target
    RunThrough1 = 37111, // HoobigoLancer->self, 4.0s cast, range 45 width 5 rect
    RunThrough2 = 37110, // HoobigoGuardian->self, 4.0s cast, range 45 width 5 rect
    FirefloodVisual = 37112, // DopproIllusionist->self, 5.0s cast, single-target
    Fireflood = 37113, // Helper->location, 5.0s cast, range 40 circle, distances based AOE, guessed optimal range 18
    TuraliStoneIIIVisual = 37114, // DopproIllusionist->self, 4.0s cast, single-target
    TuraliStoneIII = 37115, // Helper->location, 3.0s cast, range 4 circle
    Desperation = 37119, // Boss->self, no cast, range 10 90-degree cone
    TuraliQuakeVisual = 37116, // DopproIllusionist->self, 4.0s cast, single-target
    TuraliQuake = 37117, // Helper->location, 5.0s cast, range 9 circle
    Fire = 966 // DopproIllusionist->player, 1.0s cast, single-target
}

class RoarArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(20, 25);
    private static readonly ArenaBoundsCircle smallerBounds = new(20);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Deathwall)
        {
            _aoe = null;
            Module.Arena.Bounds = smallerBounds;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Roar1)
            _aoe = new(donut, Module.Arena.Center, default, spell.NPCFinishAt.AddSeconds(0.9f));
    }
}

class MagickedStandard(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(10);
    private static readonly AOEShapeDonut donut = new(3, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnActorCreated(Actor actor)
    {
        var activation = Module.WorldState.FutureTime(12.6f);
        if ((OID)actor.OID == OID.MagickedStandardGreen)
            _aoes.Add(new(donut, actor.Position, default, activation));
        else if ((OID)actor.OID == OID.MagickedStandardOrange)
            _aoes.Add(new(circle, actor.Position, default, activation));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcaneActivationCircle or AID.ArcaneActivationDonut)
            _aoes.Clear();
    }
}

class GreatLeap(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime activation;
    private static readonly AOEShapeCircle circle = new(18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (activation != default)
            yield return new(circle, Module.Enemies(OID.GrowingAOE).FirstOrDefault()!.Position, default, activation);
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GreatLeapVisual)
            activation = spell.NPCFinishAt.AddSeconds(0.2f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.GreatLeap)
            activation = default;
    }
}

class SelfSacrifice(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle circle = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LethalSwipeShade)
            _aoes.Add(new(circle, caster.Position, default, spell.NPCFinishAt.AddSeconds(0.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SelfSacrifice)
            _aoes.Clear();
    }
}

class Kickdown(BossModule module) : Components.Knockback(module)
{
    private DateTime activation;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (activation != default)
            yield return new(Module.PrimaryActor.Position, 18, activation);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => (Module.FindComponent<MagickedStandard>()?.ActiveAOEs(slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)) ?? false) || !Module.InBounds(pos);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Kickdown)
            activation = spell.NPCFinishAt;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Kickdown)
            activation = default;
    }
}

class RiotousRampage(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RiotousRampage)
            Towers.Add(new(caster.Position, 4, activation: spell.NPCFinishAt));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RiotousRampage && Towers.Count > 0)
            Towers.RemoveAt(0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Towers[0].Position, 4));
    }
}

class Roar1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Roar1));
class Roar2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Roar2));
class LethalSwipe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LethalSwipe), new AOEShapeCone(45, 90.Degrees()));
class Fireshower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Fireshower), new AOEShapeCircle(6));
class RunThrough1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RunThrough1), new AOEShapeRect(45, 2.5f));
class RunThrough2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RunThrough2), new AOEShapeRect(45, 2.5f));
class Fireflood(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Fireflood), 18);
class TuraliStoneIII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TuraliStoneIII), 4);
class TuraliQuake(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.TuraliQuake), 9, maxCasts: 5);
class StayInBounds(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Module.InBounds(actor.Position))
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Module.Center, 3));
    }
}

class TakingAStandStates : StateMachineBuilder
{
    public TakingAStandStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StayInBounds>()
            .ActivateOnEnter<RoarArenaChange>()
            .ActivateOnEnter<Roar1>()
            .ActivateOnEnter<Roar2>()
            .ActivateOnEnter<MagickedStandard>()
            .ActivateOnEnter<Kickdown>()
            .ActivateOnEnter<RiotousRampage>()
            .ActivateOnEnter<LethalSwipe>()
            .ActivateOnEnter<Fireshower>()
            .ActivateOnEnter<GreatLeap>()
            .ActivateOnEnter<SelfSacrifice>()
            .ActivateOnEnter<TuraliQuake>()
            .ActivateOnEnter<TuraliStoneIII>()
            .ActivateOnEnter<Fireflood>()
            .ActivateOnEnter<RunThrough1>()
            .ActivateOnEnter<RunThrough2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70438, NameID = 12677)]
public class TakingAStand(WorldState ws, Actor primary) : BossModule(ws, primary, new(500, -175), new ArenaBoundsCircle(24.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.HiredThug1))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.HiredThug2))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.HiredThug3))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.HoobigoGuardian))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.HoobigoLancer))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.DopproIllusionist))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
