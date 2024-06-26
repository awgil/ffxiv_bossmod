namespace BossMod.RealmReborn.Dungeon.D06Haukke.D063LadyAmandine;

public enum OID : uint
{
    Boss = 0x38A8, // R=1.8
    Sentry = 0x38A9, // R=1.35
    Handmaiden = 0x38AA, // R=1.0
}

public enum AID : uint
{
    AutoAttackBoss = 28647, // Boss->player, no cast
    Teleport = 28644, // Boss->location, no cast
    VoidCall = 28640, // Boss->self, 4.0s cast, visual (summons add)
    DarkMist = 28646, // Boss->self, 4.0s cast, range 9 circle
    BeguilingMist = 28649, // Boss->self, 3.9s cast, visual
    BeguilingMistAOE = 28643, // Boss->self, no cast, unavoidable
    VoidThunder3 = 28645, // Boss->player, 5.0s cast, interruptible tankbuster

    PetrifyingEye = 28648, // Sentry->self, 5.0s cast, gaze

    AutoAttackAdd = 870, // Handmaiden->player, no cast
    ColdCaress = 28642, // Handmaiden->player, no cast
    Stoneskin = 28641, // Handmaiden->Boss, 5.0s cast, buff target
}

class DarkMist(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkMist), new AOEShapeCircle(9));
class BeguilingMist(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.BeguilingMist), "Forced movement towards boss");
class VoidThunder(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.VoidThunder3));
class VoidThunderInterruptHint(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.VoidThunder3));
class PetrifyingEye(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.PetrifyingEye));

class D063LadyAmandineStates : StateMachineBuilder
{
    public D063LadyAmandineStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DarkMist>()
            .ActivateOnEnter<BeguilingMist>()
            .ActivateOnEnter<VoidThunder>()
            .ActivateOnEnter<VoidThunderInterruptHint>()
            .ActivateOnEnter<PetrifyingEye>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 6, NameID = 422)]
public class D063LadyAmandine(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly List<Shape> shape = [new PolygonCustom([new(20.5f, -3.4f), new(17.4f, -11.9f), new(-15.9f, -11.1f), new(-22.3f, -10.5f),
    new(-20.8f, 19.2f), new(-15, 18.2f), new(-13.1f, 23.4f), new(8.1f, 23.4f),
    new(8.5f, 21.4f), new(17.2f, 20.5f), new(16.7f, 8.7f), new(20.6f, 3.2f)])];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(shape);

    public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.CalculateAIHints(slot, actor, assignment, hints);
        foreach (var e in hints.PotentialTargets)
        {
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.Handmaiden => 2,
                OID.Boss => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Handmaiden))
            Arena.Actor(s, ArenaColor.Enemy);
    }
}
