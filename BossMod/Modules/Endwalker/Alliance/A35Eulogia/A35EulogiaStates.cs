using System.Linq;

namespace BossMod.Endwalker.Alliance.A35Eulogia
{
    class SoaringMinuet : Components.SelfTargetedAOEs
    {
        public SoaringMinuet() : base(ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees())) { }
    }
    class LightningBolt : Components.SelfTargetedAOEs
    {
        public LightningBolt() : base(ActionID.MakeSpell(AID.LightningBolt), new AOEShapeCircle(18)) { }
    }
    /////////
    class FirstBlush1 : Components.SelfTargetedAOEs
    {
        public FirstBlush1() : base(ActionID.MakeSpell(AID.FirstBlush1), new AOEShapeRect(120, 12.5f)) { }
    }
    class FirstBlush2 : Components.SelfTargetedAOEs
    {
        public FirstBlush2() : base(ActionID.MakeSpell(AID.FirstBlush2), new AOEShapeRect(120, 12.5f)) { }
    }
    class FirstBlush3 : Components.SelfTargetedAOEs
    {
        public FirstBlush3() : base(ActionID.MakeSpell(AID.FirstBlush3), new AOEShapeRect(120, 12.5f)) { }
    }
    class FirstBlush4 : Components.SelfTargetedAOEs
    {
        public FirstBlush4() : base(ActionID.MakeSpell(AID.FirstBlush4), new AOEShapeRect(120, 12.5f)) { }
    }
    /////////
    class ClimbingShotKnockback : Components.KnockbackFromCastTarget
    {
        public ClimbingShotKnockback() : base(ActionID.MakeSpell(AID.ClimbingShot1), 20) { }
    }
    
    class A35EulogiaStates : StateMachineBuilder
    {           
        public A35EulogiaStates(BossModule module) : base(module)
        {
            SimplePhase(0, id => { SimpleState(id, 10000, "Enrage"); }, "Single phase")
                .ActivateOnEnter<SunbeamSelf>()
                .ActivateOnEnter<ByregotStrikeJump>()
                .ActivateOnEnter<ByregotStrikeKnockback>()
                .ActivateOnEnter<ByregotStrikeCone>()
                .ActivateOnEnter<Hydrostasis>()
                .ActivateOnEnter<DestructiveBoltStack>()
                .ActivateOnEnter<Hieroglyphika>()
                .ActivateOnEnter<HandOfTheDestroyer>()
                .ActivateOnEnter<AsAboveSoBelow>()
                .ActivateOnEnter<MatronsBreath>()
                .ActivateOnEnter<SoaringMinuet>()
                .ActivateOnEnter<LightningBolt>()
                .ActivateOnEnter<FirstBlush1>()
                .ActivateOnEnter<FirstBlush2>()
                .ActivateOnEnter<FirstBlush3>()
                .ActivateOnEnter<FirstBlush4>()
                .ActivateOnEnter<ThousandfoldThrust>()
                .ActivateOnEnter<SolarFans>()
                .ActivateOnEnter<ClimbingShotKnockback>()
                .ActivateOnEnter<Quintessence>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed;
        }
    }
}
