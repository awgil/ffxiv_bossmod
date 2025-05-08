namespace BossMod.Endwalker.Quest.LifeEphemeralPathEternal;

class ElectrogeneticForce(BossModule module) : Components.CastTowers(module, AID.ElectrogeneticForce, 6);
class RawRockbreaker(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RawRockbreaker)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var idx = (AID)spell.Action.ID switch
        {
            AID.RawRockbreaker1 => 0,
            AID.RawRockbreaker2 => 1,
            _ => -1
        };
        AdvanceSequence(idx, caster.Position, WorldState.FutureTime(2));
    }

    public override void Update()
    {
        if (!Module.PrimaryActor.IsTargetable)
            Sequences.Clear();
    }
}
class ChiBlast(BossModule module) : Components.RaidwideCast(module, AID.ChiBlast1);
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCircle(6));
class ArmOfTheScholar(BossModule module) : Components.StandardAOEs(module, AID.ArmOfTheScholar, new AOEShapeCircle(5));

class ClassicalFire(BossModule module) : Components.StackWithCastTargets(module, AID.ClassicalFire, 6);
class ClassicalThunder(BossModule module) : Components.SpreadFromCastTargets(module, AID.ClassicalThunder, 6);
class ClassicalBlizzard(BossModule module) : Components.StandardAOEs(module, AID.ClassicalBlizzard, 6);
class ClassicalStone(BossModule module) : Components.StandardAOEs(module, AID.ClassicalStone, new AOEShapeCircle(15));

class AncelRockfistStates : StateMachineBuilder
{
    public AncelRockfistStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ChiBlast>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<ArmOfTheScholar>()
            .ActivateOnEnter<RawRockbreaker>()
            .ActivateOnEnter<ElectrogeneticForce>()
            .ActivateOnEnter<ClassicalFire>()
            .ActivateOnEnter<ClassicalThunder>()
            .ActivateOnEnter<ClassicalBlizzard>()
            .ActivateOnEnter<ClassicalStone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69608, NameID = 10732)]
public class AncelRockfist(WorldState ws, Actor primary) : BossModule(ws, primary, new(224.8f, -855.8f), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}
