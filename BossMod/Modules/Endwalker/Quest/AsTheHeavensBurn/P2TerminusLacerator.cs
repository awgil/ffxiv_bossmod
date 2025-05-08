using BossMod.QuestBattle.Endwalker.MSQ;

namespace BossMod.Endwalker.Quest.AsTheHeavensBurn.P2TerminusLacerator;

public enum OID : uint
{
    Boss = 0x35EC,
    Helper = 0x233C,
    Meteorite = 0x35ED
}

public enum AID : uint
{
    BlackStar = 27012, // Helper->location, 6.0s cast, range 40 circle
    DeadlyImpact = 27014, // Helper->location, 7.0s cast, range 10 circle
    Burst = 27021, // Helper->location, 7.5s cast, range 5 circle
    DeadlyImpactMeteorite = 27025, // 35ED->self, 5.0s cast, range 20 circle
    DeadlyImpactMeteorite2 = 27023, // Boss->self, 6.0s cast, single-target
    DeadlyImpactHelper = 27024, // Helper->location, 6.0s cast, range 20 circle
    Explosion = 27026, // 35ED->self, 3.0s cast, range 6 circle
}

class Burst(BossModule module) : Components.CastTowers(module, AID.Burst, 5);
class DeadlyImpact(BossModule module) : Components.StandardAOEs(module, AID.DeadlyImpact, 10, maxCasts: 6);
class BlackStar(BossModule module) : Components.RaidwideCast(module, AID.BlackStar);
class DeadlyImpactProximity(BossModule module) : Components.StandardAOEs(module, AID.DeadlyImpactMeteorite, new AOEShapeCircle(8));
class DeadlyImpactProximity2(BossModule module) : Components.StandardAOEs(module, AID.DeadlyImpactMeteorite2, new AOEShapeCircle(10));
class MeteorExplosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCircle(6));
class Meteor(BossModule module) : Components.GenericLineOfSightAOE(module, default, 100, false)
{
    public record MeteorObj(Actor Actor, DateTime Explosion);

    private readonly List<MeteorObj> Meteors = [];

    private void Refresh()
    {
        var meteor = Meteors.FirstOrDefault();
        Modify(meteor?.Actor.Position, Module.Enemies(0x35ED).Where(m => !m.IsDead && m.ModelState.AnimState1 != 1).Select(m => (m.Position, m.HitboxRadius)), meteor?.Explosion ?? default);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EB291)
        {
            Meteors.Add(new(actor, WorldState.FutureTime(11.9f)));
            Refresh();
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (Meteors.RemoveAll(x => x.Actor == actor) > 0)
            Refresh();
    }
}

class AutoAlisaie(BossModule module) : QuestBattle.RotationModule<AlisaieAI>(module);

class TerminusLaceratorStates : StateMachineBuilder
{
    public TerminusLaceratorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DeadlyImpact>()
            .ActivateOnEnter<BlackStar>()
            .ActivateOnEnter<DeadlyImpactProximity>()
            .ActivateOnEnter<DeadlyImpactProximity2>()
            .ActivateOnEnter<MeteorExplosion>()
            .ActivateOnEnter<Meteor>()
            .ActivateOnEnter<AutoAlisaie>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 804, NameID = 10933)]
public class TerminusLacerator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-260.28f, 80.75f), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
}
