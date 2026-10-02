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
    _Weaponskill_FloralTrap1 = 48684, // Boss->self, no cast, range 45 ?-degree cone
    _Weaponskill_Devour = 48685, // Boss->self, no cast, range 8 ?-degree cone
    _Weaponskill_Spit = 48686, // Boss->self, 4.0s cast, range 0 ???
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

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Zones.Select(b => new AOEInstance(new AOEShapeCircle(b.Item2), b.Item1.Position, b.Item1.Rotation));

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
        if (state == 4)
            Zones.RemoveAll(z => z.Item1 == actor);
    }
}

class FloralTrap(BossModule module) : Components.CastCounter(module, AID._Weaponskill_FloralTrap)
{
    readonly List<Actor> Casters = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            Casters.Remove(caster);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Casters)
        {
            var src = c.CastInfo!.LocXZ;
            foreach (var zone in Module.Enemies(OID.BuddingThorns))
            {
                var oo = zone.Position - src;
                var v = oo.Length();

                // what do...
                if (v < 10)
                    continue;

                hints.AddForbiddenZone(ShapeDistance.Cone(src, 60, oo.ToAngle(), Angle.Asin(10 / v)), Module.CastFinishAt(c.CastInfo));
            }
        }
    }
}

// TODO: Devour instakills the wasp, but we need to ensure we pull it into the right location first...
// at d0 the boss will probably die before she casts, and even if she doesn't it's not that much damage
class QueenHawkPiece(BossModule module) : Components.AddsPointless(module, (uint)OID._Gen_QueenHawkPiece);

class RottenStench(BossModule module) : Components.IconLineStack(module, 6, 45, (uint)IconID._Gen_Icon_share_laser_5sec_0t, AID._Weaponskill_RottenStench1, 5.4f);

class CorpseFlowerPieceStates : StateMachineBuilder
{
    public CorpseFlowerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BuddingThorns>()
            .ActivateOnEnter<FloralTrap>()
            .ActivateOnEnter<RottenStench>()
            .ActivateOnEnter<QueenHawkPiece>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14603)]
public class CorpseFlowerPiece(ModuleInit init) : BossModule(init, new(120, -420), new ArenaBoundsCircle(20))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (hints.FindEnemy(PrimaryActor) is { } p)
            p.DesiredPosition = Arena.Center;
    }
}

