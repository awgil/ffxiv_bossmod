// CONTRIB: made by malediktus, not checked
namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D051ForgivenCruelty
{
    public enum OID : uint
    {
        Boss = 0x27CA, //R=6.89
        Helper = 0x233C, //R=0.5
        Helper2 = 0x27CB, //R=0.5
    }

    public enum AID : uint
    {
        AutoAttack = 872, // 27CA->player, no cast, single-target
        Rake = 15611, // 27CA->player, 3,0s cast, single-target
        LumenInfinitum = 16818, // 27CA->self, 3,7s cast, range 40 width 5 rect
        TyphoonWingA = 15615, // 27CA->self, 5,0s cast, single-target
        TyphoonWingB = 15614, // 27CA->self, 5,0s cast, single-target
        TyphoonWingC = 15617, // 27CA->self, 7,0s cast, single-target
        TyphoonWingD = 15618, // 27CA->self, 7,0s cast, single-target
        TyphoonWing2 = 15616, // 233C->self, 5,0s cast, range 25 60-degree cone
        TyphoonWing3 = 17153, // 233C->self, 7,0s cast, range 25 60-degree cone
        TyphoonWing4 = 17153, // 233C->self, 7,0s cast, range 25 60-degree cone
        CycloneWing = 15612, // 27CA->self, 3,0s cast, single-target
        CycloneWing2 = 15613, // 233C->self, 4,0s cast, range 40 circle
        HurricaneWing = 15619, // 233C->self, 5,0s cast, range 10 circle
    };

    class Rake : Components.SingleTargetCast
    {
        public Rake() : base(ActionID.MakeSpell(AID.Rake)) { }
    }

    class CycloneWing : Components.RaidwideCast
    {
        public CycloneWing() : base(ActionID.MakeSpell(AID.CycloneWing2)) { }
    }

    class LumenInfinitum : Components.SelfTargetedAOEs
    {
        public LumenInfinitum() : base(ActionID.MakeSpell(AID.LumenInfinitum), new AOEShapeRect(40, 2.5f)) { }
    }

    class HurricaneWing : Components.SelfTargetedAOEs
    {
        public HurricaneWing() : base(ActionID.MakeSpell(AID.HurricaneWing), new AOEShapeCircle(10)) { }
    }

    class TyphoonWing2 : Components.SelfTargetedAOEs
    {
        public TyphoonWing2() : base(ActionID.MakeSpell(AID.TyphoonWing2), new AOEShapeCone(25, 30.Degrees())) { }
    }

    class TyphoonWing3 : Components.SelfTargetedAOEs
    {
        public TyphoonWing3() : base(ActionID.MakeSpell(AID.TyphoonWing3), new AOEShapeCone(25, 30.Degrees())) { }
    }

    class TyphoonWing4 : Components.SelfTargetedAOEs
    {
        public TyphoonWing4() : base(ActionID.MakeSpell(AID.TyphoonWing4), new AOEShapeCone(25, 30.Degrees())) { }
    }

    class D051ForgivenCrueltyStates : StateMachineBuilder
    {
        public D051ForgivenCrueltyStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<Rake>()
                .ActivateOnEnter<HurricaneWing>()
                .ActivateOnEnter<TyphoonWing2>()
                .ActivateOnEnter<TyphoonWing3>()
                .ActivateOnEnter<TyphoonWing4>()
                .ActivateOnEnter<CycloneWing>()
                .ActivateOnEnter<LumenInfinitum>();
        }
    }

    [ModuleInfo(CFCID = 659, NameID = 8260)]
    public class D051ForgivenCruelty : BossModule
    {
        public D051ForgivenCruelty(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(188, -170), 20)) { }
    }
}
