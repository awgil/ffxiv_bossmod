namespace BossMod.Stormblood.Dungeon.D03BardamsMettle.D033Yol;

public enum OID : uint
{
    Boss = 0x1AA6, // R2.8
    YolFeather = 0x1AA8, // R0.5
    CorpsecleanerEagle = 0x1AA7, // R2.52
    LeftWing = 0x1C0B, // R0.8
    RightWing = 0x1C0A, // R0.8
    Helper = 0x19A
}

public enum AID : uint
{
    AutoAttack = 871, // Boss->player, no cast, single-target
    AutoAttack2 = 870, // CorpsecleanerEagle->player, no cast, single-target
    Feathercut = 7945, // Boss->player, no cast, single-target

    WindUnbound = 7946, // Boss->self, 3.0s cast, range 40+R circle, raidwide
    Pinion = 7953, // YolFeather->self, 2.5s cast, range 40+R width 2 rect
    FlutterfallSpreadVisual = 7947, // Boss->self, 3.5s cast, single-target
    FlutterfallSpread = 7948, // Helper->player, no cast, range 6 circle, spread
    FlutterfallVisual = 7952, // Boss->self, 3.0s cast, single-target
    Flutterfall = 7954, // Helper->location, 2.5s cast, range 6 circle
    EyeOfTheFierce = 7949, // Boss->self, 4.5s cast, range 40+R circle, gaze
    FeatherSquall = 7950, // Boss->self, no cast, range 40+R width 6 rect, charge during add phase
    Wingbeat = 7951 // Boss->self, no cast, range 40+R 90-degree cone, bait away icon
}

public enum IconID : uint
{
    Flutterfall = 23, // player
    Wingbeat = 16 // player
}

class Flutterfall(BossModule module) : Components.StandardAOEs(module, AID.Flutterfall, 6);
class Pinion(BossModule module) : Components.StandardAOEs(module, AID.Pinion, new AOEShapeRect(40.5f, 1));
class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, AID.EyeOfTheFierce);
class WindUnbound(BossModule module) : Components.RaidwideCast(module, AID.WindUnbound);
class Wingbeat(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(42.8f, 45.Degrees()), (uint)IconID.Wingbeat, AID.Wingbeat);
class FlutterfallSpread(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Flutterfall, AID.FlutterfallSpread, 6, 5.4f);

class FeatherSquall(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeRect rect = new(42.8f, 3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor == Module.PrimaryActor && !actor.Position.AlmostEqual(new(24, -475.5f), 1))
            _aoe = new(rect, actor.Position, actor.Rotation, WorldState.FutureTime(6.7f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FeatherSquall)
            _aoe = null;
    }
}

class D033YolStates : StateMachineBuilder
{
    public D033YolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flutterfall>()
            .ActivateOnEnter<Pinion>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<Wingbeat>()
            .ActivateOnEnter<FlutterfallSpread>()
            .ActivateOnEnter<FeatherSquall>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 240, NameID = 6155)]
public class D033Yol(WorldState ws, Actor primary) : BossModule(ws, primary, new(24.18f, -475.12f), new ArenaBoundsCircle(19.5f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.LeftWing or OID.RightWing or OID.CorpsecleanerEagle => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LeftWing), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.RightWing), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.CorpsecleanerEagle), ArenaColor.Enemy);
    }
}
