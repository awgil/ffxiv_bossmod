namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D150ScholaMarkIIColossus;

public enum OID : uint
{
    Boss = 0x2528, //R=3.2
    ScholaColossusRubricatus = 0x2527, //R=3.4
    KanESenna = 0x2632, // R0.5
    Helper = 0x233C,

    ScholaAvenger = 0x2520, // R2.200, x?
    ScholaCenturion = 0x26EB, // R0.500, x?
    ScholaLaquearius = 0x26EC, // R0.500, x?
    ScholaEques = 0x26EE // R0.500, x?
}

public enum AID : uint
{
    AutoAttack = 870, // Boss/ScholaColossusRubricatus->player, no cast, single-target

    ElementalBlessing = 14472, // KanESenna->self, no cast, ???
    Exhaust = 14966, // Boss->self, 2.5s cast, range 40+R width 10 rect
    GrandSword = 14967, // ScholaColossusRubricatus->self, 3.0s cast, range 15+R 120-degree cone
    MagitekMissile = 15042, // Helper->location, 5.0s cast, range 30 circle, damage fall off aoe
    MagitekMissileVisual = 15041, // Boss->self, 3.0s cast, single-target
    SelfDetonate = 14574, // ScholaColossusRubricatus->self, 35.0s cast, range 30 circle, enrage
    UnbreakableCermetBlade = 14470, // ScholaColossusRubricatus->self, 9.0s cast, range 30 circle
}

class Adds(BossModule module) : Components.AddsMulti(module, [OID.ScholaAvenger, OID.ScholaCenturion, OID.ScholaEques, OID.ScholaLaquearius]);
class Exhaust(BossModule module) : Components.StandardAOEs(module, AID.Exhaust, new AOEShapeRect(43.2f, 5));
class GrandSword(BossModule module) : Components.StandardAOEs(module, AID.GrandSword, new AOEShapeCone(18.4f, 60.Degrees()));
class MagitekMissile(BossModule module) : Components.StandardAOEs(module, AID.MagitekMissile, 15);
class SelfDetonate(BossModule module) : Components.CastHint(module, AID.SelfDetonate, "Enrage!", true);

class UnbreakableCermetBlade(BossModule module) : Components.GenericAOEs(module) // Mel
{
    private AOEInstance? _aoe;
    private const string RiskHint = "Go under shield!";
    private const string StayHint = "Wait under shield!";
    private static readonly AOEShapeCircle circle = new(5.5f); // adjusted this because it was technically right, but also smaller than the actual size was. 

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ElementalBlessing)
            _aoe = new(circle, caster.Position, default, default, ArenaColor.SafeFromAOE);
        else if ((AID)spell.Action.ID == AID.UnbreakableCermetBlade)
            _aoe = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var activeAOEs = ActiveAOEs(slot, actor).ToList();
        if (activeAOEs.Any(c => !c.Check(actor.Position)))
            hints.Add(RiskHint);
        else if (activeAOEs.Any(c => c.Check(actor.Position)))
            hints.Add(StayHint, false);
    }
}

class D150ScholaMarkIIColossusStates : StateMachineBuilder
{
    public D150ScholaMarkIIColossusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<Exhaust>()
            .ActivateOnEnter<GrandSword>()
            .ActivateOnEnter<MagitekMissile>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<UnbreakableCermetBlade>()
            .Raw.Update = () => module.Enemies(OID.ScholaColossusRubricatus).All(e => e.IsDeadOrDestroyed) && module.PrimaryActor.IsDestroyed;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LegendofIceman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7888)]
public class D150ScholaMarkIIColossus(WorldState ws, Actor primary) : BossModule(ws, primary, new(367.21f, -138.99f), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ScholaColossusRubricatus), ArenaColor.Enemy);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Boss => 2,
                OID.ScholaColossusRubricatus => 1,
                _ => 0
            };
        }
    }
}
