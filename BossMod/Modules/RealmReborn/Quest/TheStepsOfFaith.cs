namespace BossMod.RealmReborn.Quest.TheStepsOfFaith;

public enum OID : uint
{
    Boss = 0x3A5F, // R30.000, x1
}

public enum AID : uint
{
    FlameBreathCast = 30185, // Vishap->self, 5.0s cast, range 1 width 2 rect
    FlameBreathChannel = 30884, // Vishap->self, no cast, range 40 width 20 rect
    Cauterize = 30878, // Boss->self, 30.5+4.5s cast, single-target
    Touchdown = 26408, // Vishap->self, 6.0s cast, range 80 circle
    Fireball = 30875, // Vishap->players/3A71/3A6F/3A6C/3A69/3A68/3A62/3A61/3A60/3A72/3A70/3A6B/3A6A/3A64/3A63, 6.0s cast, range 6 circle
    BodySlam = 26401, // Vishap->self, 6.0s cast, range 80 width 44 rect
    Flamisphere = 30883, // Vishap->location, 8.0s cast, range 10 circle
    FlameBreath2Cast = 26411, // Boss->self, 3.8+1.2s cast, range 60 width 20 rect
    RipperClaw = 31262, // 3ABD->self, 3.7s cast, range 9 ?-degree cone
    EarthshakerAOE = 30880, // Boss->self, 4.5s cast, range 31 circle
    Earthshaker = 30887, // Vishap->self, 6.5s cast, range 80 30-degree cone
    EarthrisingAOE = 26410, // Boss->self, 4.5s cast, range 31 circle
    EarthrisingCast = 30888, // Vishap->self, 7.0s cast, range 8 circle
    EarthrisingRepeat = 26412, // Vishap->self, no cast, range 8 circle
    SidewiseSlice = 30879, // Boss->self, 8.0s cast, range 50 120-degree cone
    ScorchingBreath = 29785, // Boss->self, 15.0+5.0s cast, single-target

}

class RipperClaw(BossModule module) : Components.StandardAOEs(module, AID.RipperClaw, new AOEShapeCone(9, 45.Degrees()));

class EarthShakerAOE(BossModule module) : Components.StandardAOEs(module, AID.EarthshakerAOE, new AOEShapeCircle(31));
class Earthshaker(BossModule module) : Components.StandardAOEs(module, AID.Earthshaker, new AOEShapeCone(80, 15.Degrees()), maxCasts: 2);

class EarthrisingAOE(BossModule module) : Components.StandardAOEs(module, AID.EarthrisingAOE, new AOEShapeCircle(31));
class Earthrising(BossModule module) : Components.Exaflare(module, 8)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EarthrisingCast)
        {
            Lines.Add(new() { Next = caster.Position, Advance = new(0, -7.5f), NextExplosion = Module.CastFinishAt(spell), TimeToMove = 1, ExplosionsLeft = 5, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.EarthrisingRepeat or AID.EarthrisingCast)
        {
            foreach (var l in Lines.Where(l => l.Next.AlmostEqual(caster.Position, 1)))
                AdvanceLine(l, caster.Position);
            ++NumCasts;
        }
    }
}

class SidewiseSlice(BossModule module) : Components.StandardAOEs(module, AID.SidewiseSlice, new AOEShapeCone(50, 60.Degrees()));

class FireballSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.Fireball, 6);

class Flamisphere(BossModule module) : Components.StandardAOEs(module, AID.Flamisphere, new AOEShapeCircle(10));

class BodySlam(BossModule module) : Components.KnockbackFromCastTarget(module, AID.BodySlam, 20, kind: Kind.DirForward, stopAtWall: true);

class FlameBreath(BossModule module) : Components.GenericAOEs(module, AID.FlameBreathChannel)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlameBreathCast)
            _aoe = new(new AOEShapeRect(500, 10), Module.PrimaryActor.Position, 180.Degrees(), Module.CastFinishAt(spell).AddSeconds(1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (NumCasts >= 35)
        {
            _aoe = null;
            NumCasts = 0;
        }
    }
}

class FlameBreath2(BossModule module) : Components.GenericAOEs(module, AID.FlameBreathChannel)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FlameBreath2Cast)
        {
            NumCasts = 0;

            _aoe = new(new AOEShapeRect(60, 10), caster.Position, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (NumCasts >= 14)
        {
            _aoe = null;
        }
    }
}

class Cauterize(BossModule module) : Components.GenericAOEs(module, AID.Cauterize)
{
    private Actor? Source;

    private static readonly AOEShapeRect MoveIt = new(40, 22, 38);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Source == null)
            yield break;

        if (Arena.Center.Z > 218)
            yield return new AOEInstance(MoveIt, Arena.Center);
        else
            yield return new AOEInstance(new AOEShapeRect(160, 22), Source.Position, 180.Degrees(), Module.CastFinishAt(Source.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Source = Module.PrimaryActor;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Source = null;
    }
}

class Touchdown(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Touchdown, 10, stopAtWall: true);

class ScorchingBreath(BossModule module) : Components.GenericAOEs(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ScorchingBreath)
            NumCasts++;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts > 0)
            yield return new AOEInstance(new AOEShapeRect(100, 10, 100), Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, Module.CastFinishAt(Module.PrimaryActor.CastInfo));
    }
}

class ScrollingBounds(BossModule module) : BossComponent(module)
{
    public const float HalfHeight = 40;
    public const float HalfWidth = 22;

    public static readonly ArenaBoundsRect Bounds = new(HalfWidth, HalfHeight);

    private int Phase = 1;
    private (float Min, float Max) ZBounds = (120, 300);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 3 && state == 0x20001)
        {
            ZBounds = (120, 200);
            Phase = 2;
        }

        if (index == 0 && state == 0x800040)
        {
            ZBounds = (-40, 200);
            Phase = 3;
        }

        if (index == 4 && state == 0x20001)
        {
            ZBounds = (-40, 40);
            Phase = 4;
        }

        if (index == 1 && state == 0x800040)
        {
            ZBounds = (-200, 40);
            Phase = 5;
        }

        if (index == 6 && state == 0x20001)
        {
            ZBounds = (-200, -120);
            Phase = 6;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // force player to walk south to aggro vishap (status 1268 = In Event, not actionable)
        if (Phase == 1 && !actor.InCombat && actor.FindStatus(1268) == null)
            hints.AddForbiddenZone(new AOEShapeRect(38, 22, 40), Arena.Center);

        // subsequent state transitions don't trigger until player moves into the area
        if (Phase == 3 && actor.Position.Z > 25)
            hints.AddForbiddenZone(new AOEShapeRect(40, 22, 38), Arena.Center);

        if (Phase == 5 && actor.Position.Z > -135)
            hints.AddForbiddenZone(new AOEShapeRect(40, 22, 38), Arena.Center);
    }

    public override void Update()
    {
        base.Update();
        if (WorldState.Party.Player() is not Actor p)
            return;

        Arena.Center = new(0, Math.Clamp(p.Position.Z, ZBounds.Min + HalfHeight, ZBounds.Max - HalfHeight));
    }
}

class VishapStates : StateMachineBuilder
{
    public VishapStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlameBreath>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<FireballSpread>()
            .ActivateOnEnter<Flamisphere>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<SidewiseSlice>()
            .ActivateOnEnter<ScrollingBounds>()
            .ActivateOnEnter<FlameBreath2>()
            .ActivateOnEnter<EarthShakerAOE>()
            .ActivateOnEnter<Earthshaker>()
            .ActivateOnEnter<EarthrisingAOE>()
            .ActivateOnEnter<Earthrising>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<ScorchingBreath>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70127, NameID = 3330)]
public class Vishap(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 245), ScrollingBounds.Bounds)
{
    // vishap doesn't start targetable
    protected override bool CheckPull() => PrimaryActor.InCombat;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
        Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
    }
}
