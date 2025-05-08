namespace BossMod.Endwalker.Dungeon.D04KtisisHyperboreia.D043Hermes;

public enum OID : uint
{
    Boss = 0x348A, // R4.200, x?
    Meteor = 0x348C, // R2.400, x?
}

public enum AID : uint
{
    Trismegistos = 25886, // Boss->self, 5.0s cast, range 40 circle
    TrueAeroIV = 25889, // Karukeion->self, 4.0s cast, range 50 width 10 rect
    TrueTornado = 25902, // Boss->self, 5.0s cast, single-target
    CosmicKiss = 25891, // Meteor->self, 5.0s cast, range 40 circle
    TrueAeroIV1 = 27836, // Karukeion->self, 4.0s cast, range 50 width 10 rect
    TrueAero = 25899, // Boss->self, 5.0s cast, single-target
    TrueAero2 = 25901, // Hermes->self, 2.5s cast, range 40 width 6 rect
    TrueAeroIV2 = 27837, // Karukeion->self, 10.0s cast, range 50 width 10 rect
    TrueAeroII = 25897, // Hermes->player, 5.0s cast, range 6 circle
    TrueAeroII2 = 25898, // 233C->location, 3.5s cast, range 6 circle
    TrueTornado2 = 25906, // Hermes->location, 2.5s cast, range 4 circle
    TrueBravery = 25907, // Boss->self, 5.0s cast, single-target
}

class Trismegistos(BossModule module) : Components.RaidwideCast(module, AID.Trismegistos);
class TrueTornado(BossModule module) : Components.SingleTargetCast(module, AID.TrueTornado);
class TrueTornado2(BossModule module) : Components.StandardAOEs(module, AID.TrueTornado2, 4);
class CosmicKiss(BossModule module) : Components.StandardAOEs(module, AID.CosmicKiss, new AOEShapeCircle(10));

class TrueAeroIV(BossModule module) : Components.StandardAOEs(module, AID.TrueAeroIV, new AOEShapeRect(50, 5));
class TrueAeroIV2(BossModule module) : Components.StandardAOEs(module, AID.TrueAeroIV1, new AOEShapeRect(50, 5), maxCasts: 4);
class TrueAeroIV3(BossModule module) : Components.StandardAOEs(module, AID.TrueAeroIV2, new AOEShapeRect(50, 5), maxCasts: 4);
class WindSafe(BossModule module) : Components.GenericAOEs(module)
{
    private IEnumerable<Actor> Meteors => Module.Enemies(OID.Meteor);
    private readonly List<(Actor source, AOEInstance aoe)> SafeZones = [];

    // naively, just the same width as meteor hitbox - seemed accurate during my testing, needs double-check though
    private readonly float SafeZoneWidth = 2.4f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => SafeZones.Take(4).Select(x => x.aoe);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var shapes = SafeZones.Take(4).Select(s => s.aoe.Shape.CheckFn(s.aoe.Origin, s.aoe.Rotation)).ToList();
        if (shapes.Count == 0)
            return;

        hints.AddForbiddenZone(p => !shapes.Any(e => e(p)), SafeZones[0].aoe.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TrueAeroIV1)
        {
            NumCasts++;
            var meteorBlocking = Meteors.FirstOrDefault(x => x.Position.InRect(caster.Position, spell.Rotation, 50, 0, 5) && (NumCasts <= 4 || !WillBreak(x)));
            if (meteorBlocking != null)
                SafeZones.Add((caster, new AOEInstance(new AOEShapeRect(50, SafeZoneWidth), meteorBlocking.Position, spell.Rotation, Module.CastFinishAt(spell), ArenaColor.SafeFromAOE, false)));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var meteor in Module.Enemies(OID.Meteor))
        {
            if (WillBreak(meteor))
                Arena.ActorOutsideBounds(meteor.Position, meteor.Rotation, ArenaColor.Object);
            else
                Arena.Actor(meteor.Position, meteor.Rotation, ArenaColor.Object);
        }
    }

    private bool WillBreak(Actor meteor) => meteor.ModelState.AnimState2 == 1;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TrueAeroIV1)
            SafeZones.RemoveAll(x => x.source == caster);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        // reset cast counter for next iteration of mechanic
        if (!Meteors.Any())
            NumCasts = 0;
    }
}

class TrueAero(BossModule module) : Components.GenericBaitAway(module, AID.TrueAero)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            foreach (var player in WorldState.Party.WithoutSlot())
                CurrentBaits.Add(new(caster, player, new AOEShapeRect(40, 3), Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.Clear();
    }
}

class TrueAero2(BossModule module) : Components.StandardAOEs(module, AID.TrueAero2, new AOEShapeRect(40, 3));
class TrueBravery(BossModule module) : Components.CastInterruptHint(module, AID.TrueBravery);
class TrueAeroII(BossModule module) : Components.SpreadFromCastTargets(module, AID.TrueAeroII, 6);
class TrueAeroII2(BossModule module) : Components.StandardAOEs(module, AID.TrueAeroII2, 6);

class D043HermesStates : StateMachineBuilder
{
    public D043HermesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Trismegistos>()
            .ActivateOnEnter<TrueTornado>()
            .ActivateOnEnter<TrueTornado2>()
            .ActivateOnEnter<CosmicKiss>()
            .ActivateOnEnter<TrueAeroIV>()
            .ActivateOnEnter<TrueAeroIV3>()
            .ActivateOnEnter<WindSafe>()
            .ActivateOnEnter<TrueAero>()
            .ActivateOnEnter<TrueAero2>()
            .ActivateOnEnter<TrueBravery>()
            .ActivateOnEnter<TrueAeroII>()
            .ActivateOnEnter<TrueAeroII2>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10399)]
public class D043Hermes(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -50), new ArenaBoundsCircle(20));
