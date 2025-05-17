namespace BossMod.Heavensward.Dungeon.D17BaelsarsWall.D173TheGriffin;
public enum OID : uint
{
    Boss = 0x193D, // R1.500, x?
    ThirdCohortHoplomachus = 0x1940, // R0.500, x?
    RestraintCollar = 0x193E, // R1.000, x?
    BladeOfTheGriffin = 0x193F, // R2.000, x?
}

public enum AID : uint
{
    Attack = 870, // 193D->player, no cast, single-target
    DullBlade = 7361, // 193D->player, no cast, single-target
    BeakOfTheGriffin = 7363, // 193D->self, 3.0s cast, range 80+R circle
    FlashPowder = 7364, // 193D->self, 3.0s cast, range 80+R circle
    SanguineBlade = 7365, // 193D->self, 4.5s cast, range 40+R 180-degree cone
    ClawOfTheGriffin = 7362, // 193D->player, 4.0s cast, single-target
    GullDive = 7371, // 193F->self, no cast, range 80+R circle
    Lionshead = 7370, // 193D->location, 9.0s cast, range 80 circle
    BigBoot = 7367, // 193D->player, 3.0s cast, single-target
    Corrosion = 7372, // 193F->self, 20.0s cast, range 9 circle
    RestraintCollar = 7368, // 193D->player, 3.0s cast, single-target
    RestraintCollar2 = 7369, // 193E->self, 15.0s cast, ???

    A = 7366, // 193D->location, no cast, single-target
}
public enum IconID : uint
{
    RestraintCollar = 1,
    BigBoot = 22,
    Tankbuster = 198,
};
class BeakOfTheGriffin(BossModule module) : Components.RaidwideCast(module, AID.BeakOfTheGriffin);
class FlashPowder(BossModule module) : Components.CastGaze(module, AID.FlashPowder);
class SanguineBlade(BossModule module) : Components.StandardAOEs(module, AID.SanguineBlade, new AOEShapeCone(41.5f, 90.Degrees()));
class ClawOfTheGriffin(BossModule module) : Components.SingleTargetCast(module, AID.ClawOfTheGriffin);
class GullDive(BossModule module) : Components.StandardAOEs(module, AID.GullDive, new AOEShapeCircle(80));
class BigBoot(BossModule module) : Components.Knockback(module, AID.BigBoot, stopAtWall: true)
{
    private Actor? _kbTarget;
    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new Source(Module.PrimaryActor.Position, 20, Kind: Kind.AwayFromOrigin);
    }
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.BigBoot)
        {
            _kbTarget = actor;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BigBoot)
        {
            _kbTarget = null;
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == _kbTarget)
            if (CalculateMovements(slot, actor).Any(e => DestinationUnsafe(slot, actor, e.to)))
                hints.Add("About to be knocked into danger!");
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pc == _kbTarget)
            foreach (var e in CalculateMovements(pcSlot, pc))
                DrawKnockback(e.from, e.to, pc.Rotation, Arena);
    }
};
class Corrosion(BossModule module) : Components.StandardAOEs(module, AID.Corrosion, new AOEShapeCircle(9));
class RestraintCollar(BossModule module) : BossComponent(module)
{
    private Actor? _fetterTarget;
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.RestraintCollar)
        {
            _fetterTarget = actor;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RestraintCollar)
        {
            _fetterTarget = null;
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor == _fetterTarget)
        {
            hints.AddForbiddenZone(new AOEShapeDonut(6, 60), Module.PrimaryActor.Position);
        }
    }
};
class AddsModule(BossModule module) : Components.AddsMulti(module, [OID.RestraintCollar, OID.BladeOfTheGriffin])
{
    private readonly List<Actor> _blades = [];
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.BladeOfTheGriffin)
            _blades.Add(actor);
        if (OIDs.Contains(actor.OID))
            Actors.Add(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.BladeOfTheGriffin)
            _blades.Remove(actor);
        Actors.Remove(actor);
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        Actor? lowestBlade = null;
        var lowestBladeHP = float.MaxValue;
        foreach (var t in _blades)
        {
            var currentPercent = (float)t.HPMP.CurHP / t.HPMP.MaxHP;
            if (currentPercent < lowestBladeHP)
            {
                lowestBlade = t;
                lowestBladeHP = currentPercent;
            }
        }
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.RestraintCollar => 4,
                _ when e.Actor == lowestBlade => 3,
                OID.BladeOfTheGriffin => 2,
                OID.Boss => 1,
                _ => 0
            };
    }
}

class D173TheGriffinStates : StateMachineBuilder
{
    public D173TheGriffinStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BeakOfTheGriffin>()
            .ActivateOnEnter<FlashPowder>()
            .ActivateOnEnter<SanguineBlade>()
            .ActivateOnEnter<ClawOfTheGriffin>()
            .ActivateOnEnter<GullDive>()
            .ActivateOnEnter<BigBoot>()
            .ActivateOnEnter<Corrosion>()
            .ActivateOnEnter<RestraintCollar>()
            .ActivateOnEnter<AddsModule>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 219, NameID = 5564)]
public class D173TheGriffin(WorldState ws, Actor primary) : BossModule(ws, primary, new(352f, 391.98f), new ArenaBoundsCircle(19));
