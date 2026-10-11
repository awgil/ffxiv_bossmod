#pragma warning disable CA1707 // Identifiers should not contain underscores

namespace BossMod.Global.Crucible.CorpseFlowerPiece;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x3, Helper type
    Boss = 0x4CBD, // R3.200, x1
    _Gen_SaplingPiece = 0x4CBE, // R0.750, x0 (spawn during fight)
    _Gen_QueenHawkPiece = 0x4CBF, // R0.720, x0 (spawn during fight)

    BuddingThorns = 0x1EC0E2
}

public enum AID : uint
{
    _AutoAttack_ = 49681, // Boss->player, no cast, single-target
    _Weaponskill_BuddingThorns = 48687, // Boss->self, 3.0s cast, single-target
    _Weaponskill_FloralTrap = 48683, // Boss->self, 5.0s cast, range 80 circle
    _Weaponskill_FloralTrap1 = 48684, // Boss->self, no cast, range 45 ?-degree cone, dist 50 attract
    _Weaponskill_Devour = 48685, // Boss->self, no cast, range 8 ?-degree cone
    _Weaponskill_Spit = 48686, // Boss->self, 4.0s cast, range 0 ???, 10 unit player kb in the boss's facing direction
    _Weaponskill_RottenStench = 48690, // Boss->self, 5.0+1.0s cast, single-target
    _Weaponskill_RottenStench1 = 48691, // Boss->self, no cast, range 45 width 12 rect
    _AutoAttack_1 = 48688, // 4CBF->player, no cast, single-target
    _Weaponskill_AcidRain = 48692, // Boss->self, 3.0s cast, single-target
    _Weaponskill_AcidRain1 = 48693, // Helper->location, 2.0s cast, range 6 circle
    _Weaponskill_AcidRain2 = 48694, // Helper->location, no cast, range 6 circle
    _Weaponskill_FinalSting = 48689, // 4CBF->player, 5.0s cast, single-target
}

public enum SID : uint
{
    _Gen_Bind = 2518, // Boss->player, extra=0x0
    _Gen_Stun = 2656, // Boss->player/4CBF, extra=0x0
    _Gen_DamageUp = 2550, // none->Boss, extra=0x1/0x2/0x3
    _Gen_Devoured = 421, // Boss->player, extra=0x0
    _Gen_Briar = 5176, // none->player, extra=0x32
}

public enum IconID : uint
{
    _Gen_Icon_m0005_loc_x2 = 703, // player->self
    _Gen_Icon_share_laser_5sec_0t = 525, // Boss->player
    _Gen_Icon_tracking_lockon01i = 197, // player->self
}

class BuddingThorns(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(Actor, float)> Zones = [];

    public ArcList ForbiddenArcs { get; private set; } = new(default, 20);
    public DateTime Deadline { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Zones.Select(b => new AOEInstance(new AOEShapeCircle(b.Item2), b.Item1.Position, b.Item1.Rotation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_FloralTrap)
        {
            Deadline = Module.CastFinishAt(spell);
            ForbiddenArcs = new(spell.LocXZ, 20);
            foreach (var (z, _) in Zones)
            {
                var dir = z.Position - ForbiddenArcs.Center;

                var v = dir.Length();
                if (v > 10)
                    ForbiddenArcs.ForbidArcByLength(dir.ToAngle(), Angle.Asin(10 / v));
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.BuddingThorns)
            Zones.Add((actor, 5));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
            foreach (ref var z in Zones.AsSpan())
                if (z.Item1 == actor)
                    z.Item2 = 10;
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 4 && Zones.RemoveAll(z => z.Item1 == actor) > 0)
        {
            Deadline = default;
            ForbiddenArcs.Forbidden.Clear();
        }
    }
}

class FloralTrap(BossModule module) : Components.CastCounter(module, AID._Weaponskill_FloralTrap)
{
    public static readonly Angle InhaleHalfAngle = 15.Degrees(); // blind guess

    readonly BuddingThorns Thorns = module.FindComponent<BuddingThorns>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var (from, to) in Thorns.ForbiddenArcs.Forbidden.Segments)
            hints.AddForbiddenZone(ShapeDistance.Cone(Thorns.ForbiddenArcs.Center, 50, ((to + from) * 0.5f).Radians(), ((to - from) * 0.5f).Radians()), Thorns.Deadline, 0x12345678);

        if (Thorns.Deadline != default)
            foreach (var w in Module.Enemies(OID._Gen_QueenHawkPiece))
                hints.AddForbiddenZone(ShapeDistance.InvertedCone(Thorns.ForbiddenArcs.Center, 50, Module.PrimaryActor.AngleTo(w), InhaleHalfAngle), Thorns.Deadline, 0x12345678);
    }
}

class QueenHawkPiece(BossModule module) : Components.AddsPointless(module, (uint)OID._Gen_QueenHawkPiece)
{
    readonly BuddingThorns Thorns = module.FindComponent<BuddingThorns>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var queen in ActiveActors)
        {
            if (hints.FindEnemy(queen) is { } q)
            {
                q.Priority = AIHints.Enemy.PriorityPointless;
                if (queen.TargetID != actor.InstanceID)
                    q.PreferProvoking = true;
            }
        }

        if (Thorns.Deadline == default)
            return;

        List<Angle> safe = [.. Thorns.ForbiddenArcs.Allowed(default).Select(mm => (mm.min + mm.max) * 0.5f)];

        foreach (var add in ActiveActors)
        {
            if (safe.Any(s => add.Position.InCone(Thorns.ForbiddenArcs.Center, s, FloralTrap.InhaleHalfAngle)))
                continue;

            foreach (var center in safe)
            {
                hints.ForbiddenZones.RemoveAll(z => z.tag == 0x12345678);
                hints.GoalZones.Add(hints.PullTargetToLocation(add, Thorns.ForbiddenArcs.Center + center.ToDirection() * 3, actor, 0, 1, false));
            }
        }
    }
}

class RottenStench(BossModule module) : Components.IconLineStack(module, 6, 45, (uint)IconID._Gen_Icon_share_laser_5sec_0t, AID._Weaponskill_RottenStench1, 5.4f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        hints.FindEnemy(Source)?.CanMove = false;
    }
}

class AcidRainPre(BossModule module) : BossComponent(module)
{
    DateTime _deadline;
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_tracking_lockon01i)
            _deadline = WorldState.FutureTime(5.1f);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_AcidRain1)
            _deadline = default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_deadline != default)
            hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 18), _deadline);
    }
}
class AcidRain(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(6), AID._Weaponskill_AcidRain1, AID._Weaponskill_AcidRain2, 5, 1.1f, 8)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (Chasers.Count == 0 && hints.FindEnemy(Module.PrimaryActor) is { } p)
            p.DesiredPosition = Arena.Center;
    }
}

class CorpseFlowerPieceStates : StateMachineBuilder
{
    public CorpseFlowerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BuddingThorns>()
            .ActivateOnEnter<FloralTrap>()
            .ActivateOnEnter<RottenStench>()
            .ActivateOnEnter<QueenHawkPiece>()
            .ActivateOnEnter<AcidRainPre>()
            .ActivateOnEnter<AcidRain>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14603)]
public class CorpseFlowerPiece(ModuleInit init) : BossModule(init, new(120, -420), new ArenaBoundsCircle(20));
