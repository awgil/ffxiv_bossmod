namespace BossMod.Endwalker.Dungeon.D02TowerOfBabil.D023Anima;

public enum OID : uint
{
    Boss = 0x33FD, // R=18.7
    LowerAnima = 0x3400, // R=18.7
    IronNail = 0x3401, // R=1.0
    LunarNail = 0x33FE, // R=1.0
    MegaGraviton = 0x33FF, // R=1.0
    Actor1eb239 = 0x1EB239, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 25341, // Boss->player, no cast, single-target

    AetherialPull = 25345, // MegaGraviton->player, 8.0s cast, single-target, pull 30 between centers

    BoundlessPainPull = 26229, // Helper->self, no cast, range 60 circle, pull 60 between centers
    BoundlessPainVisual = 25347, // Boss->self, 8.0s cast, single-target, creates expanding AOE
    BoundlessPainFirst = 25348, // Helper->location, no cast, range 6 circle
    BoundlessPainRest = 25349, // Helper->location, no cast, range 6 circle

    CharnelClaw = 25357, // IronNail->self, 6.0s cast, range 40 width 5 rect

    CoffinScratchFirst = 25358, // Helper->location, 3.5s cast, range 3 circle
    CoffinScratchRest = 21239, // Helper->location, no cast, range 3 circle

    Imperatum = 25353, // Boss->self, 5.0s cast, range 60 circle, phase change
    ImperatumPull = 23929, // Helper->player, no cast, single-target, pull 60 between centers

    LunarNail = 25342, // Boss->self, 3.0s cast, single-target

    ObliviatingClaw1 = 25354, // LowerAnima->self, 3.0s cast, single-target
    ObliviatingClaw2 = 25355, // LowerAnima->self, 3.0s cast, single-target
    ObliviatingClawSpawnAOE = 25356, // IronNail->self, 6.0s cast, range 3 circle

    OblivionVisual = 25359, // LowerAnima->self, 6.0s cast, single-target
    OblivionStart = 23697, // Helper->location, no cast, range 60 circle
    OblivionLast = 23872, // Helper->location, no cast, range 60 circle

    MegaGraviton = 25344, // Boss->self, 5.0s cast, range 60 circle, tether mechanic
    GravitonSpark = 25346, // MegaGraviton->player, no cast, single-target, on touching the graviton

    PaterPatriaeAOE = 24168, // Helper->self, 3.5s cast, range 60 width 8 rect
    PaterPatriae2 = 25350, // Boss->self, 3.5s cast, single-target

    PhantomPain1 = 21182, // Boss->self, 7.0s cast, single-target
    PhantomPain2 = 25343, // Helper->self, 7.0s cast, range 20 width 20 rect

    VisualModelChange = 27228, // LowerAnima->self, no cast, single-target

    EruptingPainVisual = 25351, // Boss->self, 5.0s cast, single-target
    EruptingPain = 25352 // Helper->player, 5.0s cast, range 6 circle
}

public enum TetherID : uint
{
    PhantomPain = 162, // Helper->Helper
    AetherialPullBad = 57, // MegaGraviton->player
    AetherialPullGood = 17, // MegaGraviton->player
    AnimaDrawsPower = 22 // Helper->Boss
}

public enum IconID : uint
{
    ChasingAOE = 197 // player
}

class ArenaChange(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x03)
        {
            if (state == 0x00020001)
                Arena.Center = D023Anima.LowerArenaCenter;
            if (state == 0x00080004)
                Arena.Center = D023Anima.UpperArenaCenter;
        }
    }
}

class BoundlessPain(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCircle circle = new(18);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BoundlessPainPull:
                _aoe = new(circle, Arena.Center);
                break;
            case AID.BoundlessPainFirst:
            case AID.BoundlessPainRest:
                if (++NumCasts == 20)
                {
                    _aoe = null;
                    NumCasts = 0;
                }
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveAOEs(slot, actor).Any())
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center, Arena.Center + new WDir(0, 20), 20));
    }
}

class Gravitons(BossModule module) : Components.PersistentVoidzone(module, 1, m => m.Enemies(OID.MegaGraviton).Where(x => !x.IsDead));
class AetherialPull(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.AetherialPull))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor != WorldState.Party.Player())
            return;

        foreach (var b in ActiveBaits)
        {
            if (b.Target == actor)
            {
                hints.AddForbiddenZone(b.Shape, BaitOrigin(b), b.Rotation, b.Activation);
            }
        }
    }

    public override void Update()
    {
        foreach (ref var b in CurrentBaits.AsSpan())
        {
            if (b.Shape is AOEShapeRect shape)
            {
                var length = (b.Target.Position - b.Source.Position).Length();
                if (shape.LengthFront != length)
                {
                    b.Shape = shape with { LengthFront = length };
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && WorldState.Actors.Find(spell.TargetID) is var target && target != null && target == WorldState.Party.Player())
            CurrentBaits.Add(new(caster, target, new AOEShapeRect(0, 0.5f), Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            CurrentBaits.RemoveAll(b => b.Source == caster);
    }
}

class CoffinScratch(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(3), ActionID.MakeSpell(AID.CoffinScratchFirst), ActionID.MakeSpell(AID.CoffinScratchRest), 6, 2, 5)
{
    private readonly bool ResetExcludedTargets = true;
    private readonly List<Actor> Actors = []; // to keep track of the icon before mechanic starts for handling custom forbidden zones
    private DateTime Activation;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Chasers.Count > 0)
        {
            AddForbiddenZones(actor, hints, true);
            AddForbiddenZones(actor, hints, false);
        }
        if (Actors.Contains(actor))
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center + new WDir(19, 0), Arena.Center + new WDir(-19, 0), 20), Activation);
        else if (Chasers.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(actor.Position, 90.Degrees(), 40, 40, 3));
    }
    private void AddForbiddenZones(Actor actor, AIHints hints, bool isTarget)
    {
        // sort of a hack to prevent the AI from getting "stuck" inside the AOE because all paths to safety have equal distance
        foreach (var c in Chasers.Where(c => c.Target == actor == isTarget))
        {
            var circle = (AOEShapeCircle)c.Shape;
            var radius = isTarget ? MoveDistance + circle.Radius : circle.Radius + 1;
            var position = isTarget ? c.PredictedPosition() - circle.Radius * actor.Rotation.ToDirection() : c.PredictedPosition();
            hints.AddForbiddenZone(ShapeDistance.Circle(position, radius), c.NextActivation);
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == ActionFirst)
        {
            var pos = spell.TargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
            var (slot, target) = Raid.WithSlot().ExcludedFromMask(ExcludedTargets).MinBy(ip => (ip.Item2.Position - pos).LengthSq());
            if (target != null)
            {
                Actors.Remove(target);
                Chasers.Add(new(Shape, target, pos, 0, MaxCasts, Module.CastFinishAt(spell), SecondsBetweenActivations)); // initial cast does not move anywhere
                ExcludedTargets.Set(slot);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == ActionFirst || spell.Action == ActionRest)
        {
            var pos = spell.MainTargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.MainTargetID)?.Position ?? spell.TargetXZ;
            Advance(pos, MoveDistance, WorldState.CurrentTime);
            if (Chasers.Count == 0 && ResetExcludedTargets)
            {
                ExcludedTargets.Reset();
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.ChasingAOE)
        {
            Activation = WorldState.FutureTime(5.1f);
            Actors.Add(actor);
        }
    }
}
class PhantomPain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhantomPain2), new AOEShapeRect(10, 10, 10));
class PaterPatriaeAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PaterPatriaeAOE), new AOEShapeRect(60, 4));
class CharnelClaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CharnelClaw), new AOEShapeRect(40, 2.5f), 5);
class ErruptingPain(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EruptingPain), 6);
class ObliviatingClawSpawnAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ObliviatingClawSpawnAOE), new AOEShapeCircle(3));
class Oblivion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.OblivionVisual), "Raidwide x16");
class MegaGraviton(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MegaGraviton));

class D023AnimaStates : StateMachineBuilder
{
    public D023AnimaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Gravitons>()
            .ActivateOnEnter<BoundlessPain>()
            .ActivateOnEnter<CoffinScratch>()
            .ActivateOnEnter<PhantomPain>()
            .ActivateOnEnter<AetherialPull>()
            .ActivateOnEnter<PaterPatriaeAOE>()
            .ActivateOnEnter<CharnelClaw>()
            .ActivateOnEnter<ErruptingPain>()
            .ActivateOnEnter<ObliviatingClawSpawnAOE>()
            .ActivateOnEnter<Oblivion>()
            .ActivateOnEnter<MegaGraviton>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team (Malediktus, LTS), Ported by Herculezz", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 785, NameID = 10285)]
public class D023Anima(WorldState ws, Actor primary) : BossModule(ws, primary, UpperArenaCenter, new ArenaBoundsSquare(19.5f))
{
    public static readonly WPos UpperArenaCenter = new(0, -180);
    public static readonly WPos LowerArenaCenter = new(0, -400);
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LowerAnima), ArenaColor.Enemy);
    }
}
