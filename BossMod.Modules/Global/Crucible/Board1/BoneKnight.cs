namespace BossMod.Global.Crucible.BoneKnight;

public enum OID : uint
{
    BoneKnight = 0x4B86,
    BoneBishop = 0x4B87,
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 50784, // BoneKnight->player, no cast, single-target
    AutoBlizzard = 50788, // BoneBishop->player, no cast, single-target
    DeathSpiralCast = 46867, // BoneBishop->self, 5.0s cast, single-target
    DeathSpiral = 46868, // Helper->self, 6.0s cast, range 4-40 donut
    Tumulus = 46866, // BoneKnight->self, 5.0s cast, range 6 circle
    BlackEruptionCast = 46873, // BoneBishop->self, 5.0+1.0s cast, single-target
    BlackEruptionFirst = 46874, // Helper->location, 6.0s cast, range 5 circle
    BlackEruptionRest = 46900, // Helper->location, 1.5s cast, range 5 circle
    Ossify = 46871, // BoneKnight->self, 8.0s cast, single-target
    ForwardGuard = 46864, // BoneKnight->self, 5.0s cast, single-target
    ForwardGuardDisable = 46865, // BoneKnight->self, no cast, single-target
}

class DeathSpiral(BossModule module) : Components.StandardAOEs(module, AID.DeathSpiral, new AOEShapeDonut(4, 40));
class Tumulus(BossModule module) : Components.StandardAOEs(module, AID.Tumulus, 6);
class BlackEruption(BossModule module) : Components.GroupedAOEs(module, [AID.BlackEruptionFirst, AID.BlackEruptionRest], new AOEShapeCircle(5));

class ForwardGuard(BossModule module) : Components.DirectionalParry(module, (uint)OID.BoneKnight)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ForwardGuard)
            PredictParrySide(caster.InstanceID, Side.Front);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var (id, targetState) in _actorStates)
        {
            if (targetState != 0 && hints.FindEnemy(WorldState.Actors.Find(id)) is { } e && e.Actor.TargetID == actor.InstanceID)
                e.PreferShirking = true;
        }
    }
}

class BoneKnightStates : StateMachineBuilder
{
    public BoneKnightStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeathSpiral>()
            .ActivateOnEnter<Tumulus>()
            .ActivateOnEnter<BlackEruption>()
            .ActivateOnEnter<ForwardGuard>()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && ((BoneKnight)module).BoneBishop is { IsDeadOrDestroyed: true };
    }
}

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.BoneKnight, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14531)]
public class BoneKnight(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20))
{
    public Actor? BoneBishop { get; private set; }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);

        Arena.Actor(BoneBishop, ArenaColor.Enemy);
    }

    protected override void UpdateModule()
    {
        BoneBishop ??= Enemies(OID.BoneBishop).FirstOrDefault();
    }
}
