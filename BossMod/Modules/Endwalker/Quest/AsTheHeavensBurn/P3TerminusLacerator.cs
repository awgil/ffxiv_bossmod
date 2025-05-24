namespace BossMod.Endwalker.Quest.AsTheHeavensBurn.P3TerminusLacerator;

public enum OID : uint
{
    Boss = 0x35EE,
    Helper = 0x233C,
    Vanquisher = 0x35EF,
}

public enum AID : uint
{
    BlackStar = 27012, // Helper->location, 6.0s cast, range 40 circle
    DeadlyImpact = 27014, // Helper->location, 7.0s cast, range 10 circle
    ForcefulImpactAOE = 26239, // Vanquisher->location, 5.0s cast, range 7 circle
    ForcefulImpactKB = 27030, // Helper->self, 5.6s cast, range 20 circle
    MutableLawsBig = 27041, // Helper->location, 10.0s cast, range 6 circle
    MutableLawsSmall = 27040, // Helper->location, 10.0s cast, range 6 circle
    AccursedTongue = 27038, // Helper->35F5/35FA/35F9/35F7, 5.0s cast, range 6 circle
    Shock = 27035, // 35F0->self, 5.0s cast, range 10 circle
    Depress = 27036, // 35EF->35FA, 5.0s cast, range 7 circle
    ForcefulImpact = 27029, // 35EF->location, 5.0s cast, range 7 circle
}

class DeadlyImpact(BossModule module) : Components.StandardAOEs(module, AID.DeadlyImpact, 10, maxCasts: 6);
class BlackStar(BossModule module) : Components.RaidwideCast(module, AID.BlackStar);

class ForcefulImpact(BossModule module) : Components.StandardAOEs(module, AID.ForcefulImpactAOE, 7);
class ForcefulImpactKB(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ForcefulImpactKB, 10, stopAtWall: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.FirstOrDefault() is Actor c)
            hints.AddPredictedDamage(WorldState.Party.WithSlot().Mask(), Module.CastFinishAt(c.CastInfo));
    }
}
class MutableLaws1(BossModule module) : Components.StandardAOEs(module, AID.MutableLawsBig, 15);
class MutableLaws2(BossModule module) : Components.StandardAOEs(module, AID.MutableLawsSmall, 6);
class AccursedTongue(BossModule module) : Components.SpreadFromCastTargets(module, AID.AccursedTongue, 6);
class ForcefulImpact2(BossModule module) : Components.StandardAOEs(module, AID.ForcefulImpact, 7);
class Shock(BossModule module) : Components.StandardAOEs(module, AID.Shock, new AOEShapeCircle(10), maxCasts: 6);
class Depress(BossModule module) : Components.StackWithCastTargets(module, AID.Depress, 7);

class TerminusLaceratorStates : StateMachineBuilder
{
    private readonly TerminusLacerator _module;

    public TerminusLaceratorStates(TerminusLacerator module) : base(module)
    {
        _module = module;

        TrivialPhase()
            .ActivateOnEnter<DeadlyImpact>()
            .ActivateOnEnter<BlackStar>();
        TrivialPhase(1)
            .ActivateOnEnter<ForcefulImpact>()
            .ActivateOnEnter<ForcefulImpactKB>()
            .ActivateOnEnter<MutableLaws1>()
            .ActivateOnEnter<MutableLaws2>()
            .ActivateOnEnter<AccursedTongue>()
            .ActivateOnEnter<ForcefulImpact2>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<Depress>()
            .Raw.Update = () => _module.BossP2?.IsDeadOrDestroyed ?? false;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, NameID = 10933)]
public class TerminusLacerator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-260.28f, 80.75f), new ArenaBoundsCircle(19.5f))
{
    public Actor? BossP2 => Enemies(OID.Vanquisher).FirstOrDefault();

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(BossP2, ArenaColor.Enemy);
    }
}
