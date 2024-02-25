// CONTRIB: made by malediktus, not checked
namespace BossMod.Stormblood.HuntA.Angada
{
    public enum OID : uint
    {
        Boss = 0x1AC0, // R=5.4
    };

    public enum AID : uint
    {
        AutoAttack = 872, // 1AC0->player, no cast, single-target
        ScytheTail = 8190, // 1AC0->self, 3,0s cast, range 4+R circle
        RockThrow = 8193, // 1AC0->location, 3,0s cast, range 6 circle
        Butcher = 8191, // 1AC0->self, 3,0s cast, range 6+R 120-degree cone
        Rip = 8192, // 1AC0->self, no cast, range 6+R 120-degree cone, always happens directly after Butcher
    };

    class ScytheTail : Components.SelfTargetedAOEs
    {
        public ScytheTail() : base(ActionID.MakeSpell(AID.ScytheTail), new AOEShapeCircle(9.4f)) { }
    }

    class Butcher : Components.SelfTargetedAOEs
    {
        public Butcher() : base(ActionID.MakeSpell(AID.Butcher), new AOEShapeCone(11.4f, 60.Degrees())) { }
    }

    class RockThrow : Components.LocationTargetedAOEs
    {
        public RockThrow() : base(ActionID.MakeSpell(AID.RockThrow), 6) { }
    }

    class AngadaStates : StateMachineBuilder
    {
        public AngadaStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<ScytheTail>()
                .ActivateOnEnter<Butcher>()
                .ActivateOnEnter<RockThrow>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 90)]
    public class Angada : SimpleBossModule
    {
        public Angada(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
