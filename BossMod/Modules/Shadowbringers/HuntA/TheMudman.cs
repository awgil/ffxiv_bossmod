// CONTRIB: made by malediktus, not checked
namespace BossMod.Shadowbringers.HuntA.TheMudman
{
    public enum OID : uint
    {
        Boss = 0x281F, // R=4.2
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        FeculentFlood = 16828, // Boss->self, 3,0s cast, range 40 60-degree cone
        RoyalFlush = 16826, // Boss->self, 3,0s cast, range 8 circle
        BogBequest = 16827, // Boss->self, 5,0s cast, range 5-20 donut
        GravityForce = 16829, // Boss->player, 5,0s cast, range 6 circle, interruptible, applies heavy
    };


    class BogBequest : Components.SelfTargetedAOEs
    {
        public BogBequest() : base(ActionID.MakeSpell(AID.BogBequest), new AOEShapeDonut(5, 20)) { }
    }

    class FeculentFlood : Components.SelfTargetedAOEs
    {
        public FeculentFlood() : base(ActionID.MakeSpell(AID.FeculentFlood), new AOEShapeCone(40, 30.Degrees())) { }
    }

    class RoyalFlush : Components.SelfTargetedAOEs
    {
        public RoyalFlush() : base(ActionID.MakeSpell(AID.RoyalFlush), new AOEShapeCircle(8)) { }
    }

    class GravityForce : Components.SpreadFromCastTargets
    {
        public GravityForce() : base(ActionID.MakeSpell(AID.GravityForce), 6) { }
    }

    class GravityForceHint : Components.CastInterruptHint
    {
        public GravityForceHint() : base(ActionID.MakeSpell(AID.GravityForce)) { }
    }

    class TheMudmanStates : StateMachineBuilder
    {
        public TheMudmanStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<BogBequest>()
                .ActivateOnEnter<FeculentFlood>()
                .ActivateOnEnter<RoyalFlush>()
                .ActivateOnEnter<GravityForce>()
                .ActivateOnEnter<GravityForceHint>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 129)]
    public class TheMudman(WorldState ws, Actor primary) : SimpleBossModule(ws, primary) {}
}
