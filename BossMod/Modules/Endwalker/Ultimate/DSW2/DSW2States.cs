namespace BossMod.Endwalker.Ultimate.DSW2
{
    class DSW2States : StateMachineBuilder
    {
        private DSW2 _module;

        public DSW2States(DSW2 module) : base(module)
        {
            _module = module;
            DeathPhase(1, Phase2Thordan);
        }

        private void Phase2Thordan(uint id)
        {
            P2AscalonMercyMight(id, 8.4f);
            P2StrengthOfTheWard(id + 0x10000, 6.7f);

            SimpleState(id + 0x20000, 100, "???");
        }

        private void P2AscalonMercyMight(uint id, float delay)
        {
            Cast(id, AID.AscalonsMercyConcealed, delay, 3);
            ComponentCondition<P2AscalonMercy>(id + 2, 1.5f, comp => comp.NumCasts > 0, "Dropped cones")
                .ActivateOnEnter<P2AscalonMercy>()
                .DeactivateOnExit<P2AscalonMercy>();
            ComponentCondition<P2AscalonMight>(id + 0x1000, 5.2f, comp => comp.NumCasts > 2, "3x tankbuster cones")
                .ActivateOnEnter<P2AscalonMight>()
                .DeactivateOnExit<P2AscalonMight>();
        }

        private void P2StrengthOfTheWard(uint id, float delay)
        {
            Cast(id, AID.StrengthOfTheWard, delay, 4);
            Targetable(id + 0x10, false, 3, "Trio 1");
            CastStart(id + 0x20, AID.LightningStorm, 3.6f)
                .ActivateOnEnter<P2StrengthOfTheWard>();
            CastEnd(id + 0x21, 5.7f);
            ComponentCondition<P2StrengthOfTheWard>(id + 0x30, 0.3f, comp => comp.NumImpactHits > 0, "Charges + Hit 1");
            ComponentCondition<P2StrengthOfTheWard>(id + 0x40, 1.9f, comp => comp.NumImpactHits > 1, "Hit 2");
            ComponentCondition<P2StrengthOfTheWard>(id + 0x60, 1.9f, comp => comp.NumImpactHits > 2, "Hit 3");
            ComponentCondition<P2StrengthOfTheWard>(id + 0x80, 1.9f, comp => comp.NumImpactHits > 3, "Hit 4")
                .ActivateOnEnter<P2AscalonMercy>()
                .DeactivateOnExit<P2StrengthOfTheWard>();
            ComponentCondition<P2AscalonMercy>(id + 0x90, 0.4f, comp => comp.NumCasts > 0, "Dropped cones")
                .DeactivateOnExit<P2AscalonMercy>();
        }
    }
}
