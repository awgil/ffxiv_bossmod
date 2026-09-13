namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoardOfTheUnbroken.Battle04;

public enum OID : uint
{
    Boss = 0x4B8D,
    WispPiece = 0x4B8E,
    GreatWispPiece = 0x4DD4,
    BallOfFire = 0x4B8F,
    Helper = 0x233C,
    FlamePuddleVoidzone = 0x1EC025,
}
public enum AID : uint
{
    AutoAttack = 49682, // Boss->none, no cast, single-target
    A = 46914, // Boss->location, no cast, single-target
    ScorchingSmiteVisual = 46913, // Boss->self, 5.0+1.0s cast, single-target
    ScorchingSmite = 46912, // Helper->self, 6.0s cast, range 40 120-degree cone
    Allfire = 46915, // Boss->self, 4.0s cast, range 40 circle
    MagmaSmall = 46916, // Helper->location, 3.0s cast, range 3 circle
    MagmaBig = 46917, // Helper->location, 3.0s cast, range 5 circle
    ScorchingSmite2 = 46910, // Boss->self, 6.0s cast, single-target
    ScorchingSmite3 = 46911, // Boss->self, no cast, single-target
    ScorchingSmiteFakeout = 49688, // Helper->self, 9.3s cast, range 40 120-degree cone
    BurningWard = 46918, // Boss->self, 3.0s cast, single-target
    FireCall = 46921, // Boss->self, 4.0s cast, single-target
    ArmOfPurgatory = 46922, // 4B8F->self, 1.0s cast, range 10 circle

}
class MagmaSmall(BossModule module) : Components.StandardAOEs(module, AID.MagmaSmall, 3f);
class MagmaBig(BossModule module) : Components.StandardAOEs(module, AID.MagmaBig, 5f);
class ScorchingSmite(BossModule module) : Components.StandardAOEs(module, AID.ScorchingSmite, new AOEShapeCone(40f, 60.Degrees()));
class ScorchingSmiteFakeout(BossModule module) : Components.StandardAOEs(module, AID.ScorchingSmiteFakeout, new AOEShapeCone(40f, 60.Degrees()));
class FlamePuddleVoidZone(BossModule module) : Components.Voidzone(module, 5f, OID.FlamePuddleVoidzone);
class BallOfFire(BossModule module) : Components.Voidzone(module, 10f, uint.MaxValue, moveHintLength: 15)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.BallOfFire && !actor.Position.AlmostEqual(Module.Center, 2))
            AddSource(actor);
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.BallOfFire)
            RemoveSource(actor);
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArmOfPurgatory)
        {
            RemoveSource(caster);
        }
    }
}
class ArmOfPurgatory(BossModule module) : Components.StandardAOEs(module, AID.ArmOfPurgatory, new AOEShapeCircle(10f));

class AddsMulti(BossModule module) : Components.AddsMulti(module, [(uint)OID.WispPiece, (uint)OID.GreatWispPiece])
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
            e.Priority = (OID)e.Actor.OID switch
            {
                OID.GreatWispPiece => 2,
                OID.WispPiece => 1,
                OID.Boss => -1,
                _ => 0
            };
    }
}
class Battle04States : StateMachineBuilder
{
    public Battle04States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScorchingSmite>()
            .ActivateOnEnter<ScorchingSmiteFakeout>()
            .ActivateOnEnter<MagmaSmall>()
            .ActivateOnEnter<MagmaBig>()
            .ActivateOnEnter<FlamePuddleVoidZone>()
            .ActivateOnEnter<BallOfFire>()
            .ActivateOnEnter<ArmOfPurgatory>()
            .ActivateOnEnter<AddsMulti>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1088, NameID = 14538)]
public class Battle04(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));
