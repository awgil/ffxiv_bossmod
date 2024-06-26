namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

class DAL2CuchulainnStates : StateMachineBuilder
{
    public DAL2CuchulainnStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FellFlow1>()
            .ActivateOnEnter<AmbientPulsationAOE>()
            .ActivateOnEnter<NecroticBillowAOE>()
            .ActivateOnEnter<BurgeoningDread>()
            .ActivateOnEnter<MightOfMalice>()
            .ActivateOnEnter<PutrifiedSoul2>()
            .ActivateOnEnter<PutrifiedSoul1>();
    }
}
