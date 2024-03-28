namespace BossMod.Endwalker.HuntS.KerShroud
{
    public enum OID : uint
    {
        Boss = 0x3672, // R2.500, x1
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        AccursedPox = 27725, // Boss->location, 5.0s cast, range 8 circle
        EntropicFlame = 27724, // Boss->self, 4.0s cast, range 60 width 8 rect
    };

    class AccursedPox : Components.LocationTargetedAOEs
    {
        public AccursedPox() : base(ActionID.MakeSpell(AID.AccursedPox), 8) { }
    }

    class EntropicFlame : Components.SelfTargetedAOEs
    {
        public EntropicFlame() : base(ActionID.MakeSpell(AID.EntropicFlame), new AOEShapeRect(60, 4)) { }
    }

    class KerShroudStates : StateMachineBuilder
    {
        public KerShroudStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<AccursedPox>()
                .ActivateOnEnter<EntropicFlame>();
        }
    }

    [ModuleInfo(NotoriousMonsterID = 177)]
    public class KerShroud : SimpleBossModule
    {
        public KerShroud(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
