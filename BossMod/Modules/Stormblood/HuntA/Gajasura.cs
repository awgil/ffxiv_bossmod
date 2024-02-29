﻿// CONTRIB: made by malediktus, not checked
namespace BossMod.Stormblood.HuntA.Gajasura
{
    public enum OID : uint
    {
        Boss = 0x1ABF, // R=3.23
    };

    public enum AID : uint
    {
        AutoAttack = 872, // 1ABF->player, no cast, single-target
        Spin = 8188, // 1ABF->self, 3,0s cast, range 5+R 120-degree cone
        Hurl = 8187, // 1ABF->location, 3,0s cast, range 6 circle
        Buffet = 8189, // 1ABF->none, 3,0s cast, single-target, randomly hits a target that isn't tanking, only happens when at least 2 actors are in combat with Gajasura (chocobos count)
    };

    class Spin : Components.SelfTargetedAOEs
    {
        public Spin() : base(ActionID.MakeSpell(AID.Spin), new AOEShapeCircle(8.23f)) { }
    }

    class Hurl : Components.SelfTargetedAOEs
    {
        public Hurl() : base(ActionID.MakeSpell(AID.Hurl), new AOEShapeCircle(6)) { }
    }

    class Buffet : Components.SingleTargetCast
    {
        public Buffet() : base(ActionID.MakeSpell(AID.Buffet), "Heavy damage on random target (except tank)") { }
    }

    class GajasuraStates : StateMachineBuilder
    {
        public GajasuraStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Spin>()
                .ActivateOnEnter<Hurl>()
                .ActivateOnEnter<Buffet>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 89)]
    public class Gajasura : SimpleBossModule
    {
        public Gajasura(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
