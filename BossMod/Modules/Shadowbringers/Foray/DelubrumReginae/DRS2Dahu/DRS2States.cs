namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2Dahu
{
    class DRS2States : StateMachineBuilder
    {
        public DRS2States(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            ReverberatingRoarHotChargeFirebreathe(id, 10.3f);
            HeadDownSpitFlameShockwave(id + 0x10000, 14.6f);
            FeralHowl(id + 0x20000, 1.5f);
            FirebreatheRotating(id + 0x30000, 1.6f);
            // TODO: adds (no need for a state?) -> roar + head down -> shockwave -> hysteric assault -> rotating firebreathe -> hot charge + firebreathe -> ...
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void ReverberatingRoarHotChargeFirebreathe(uint id, float delay)
        {
            ComponentCondition<FallingRock>(id, delay, comp => comp.Casters.Count > 0, "Rocks 1 bait")
                .ActivateOnEnter<FallingRock>();
            Cast(id + 0x10, AID.HotCharge, 9.5f, 3, "Charge 1")
                .ActivateOnEnter<HotCharge>()
                .DeactivateOnExit<HotCharge>();
            Cast(id + 0x20, AID.HotCharge, 1.9f, 3, "Charge 2")
                .ActivateOnEnter<HotCharge>()
                .DeactivateOnExit<HotCharge>()
                .DeactivateOnExit<FallingRock>(); // last rock ends ~1.1s before cast start
            Cast(id + 0x30, AID.Firebreathe, 1.9f, 5, "Cone")
                .ActivateOnEnter<Firebreathe>()
                .DeactivateOnExit<Firebreathe>();
        }

        private void HeadDownSpitFlameShockwave(uint id, float delay)
        {
            ComponentCondition<HeadDown>(id, delay, comp => comp.Casters.Count > 0, "Add charges begin")
                .ActivateOnEnter<HeadDown>();
            CastStart(id + 0x10, AID.SpitFlame, 4.8f)
                .ActivateOnEnter<SpitFlame>(); // first icon appears right before cast start
            CastEnd(id + 0x11, 8);
            ComponentCondition<SpitFlame>(id + 0x20, 3.7f, comp => !comp.Active, "Spits resolve")
                .DeactivateOnExit<SpitFlame>();
            CastMulti(id + 0x30, new[] { AID.LeftSidedShockwaveFirst, AID.RightSidedShockwaveFirst }, 3.2f, 3, "Shockwave 1")
                .ActivateOnEnter<Shockwave>();
            CastMulti(id + 0x40, new[] { AID.LeftSidedShockwaveSecond, AID.RightSidedShockwaveSecond }, 1.6f, 1, "Shockwave 2")
                .DeactivateOnExit<Shockwave>();
            ComponentCondition<HeadDown>(id + 0x50, 1.1f, comp => comp.Casters.Count == 0, "Add charges resolve")
                .DeactivateOnExit<HeadDown>();
        }

        private void FeralHowl(uint id, float delay)
        {
            Cast(id, AID.FeralHowl, delay, 5)
                .ActivateOnEnter<HuntersClaw>()
                .ActivateOnEnter<FeralHowl>();
            ComponentCondition<FeralHowl>(id + 2, 2.1f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<FeralHowl>();
            ComponentCondition<HuntersClaw>(id + 3, 1.5f, comp => comp.NumCasts > 0, "Add aoes")
                .DeactivateOnExit<HuntersClaw>();
        }

        private void FirebreatheRotating(uint id, float delay)
        {
            CastStart(id, AID.FirebreatheRotating, delay)
                .ActivateOnEnter<FirebreatheRotating>(); // icon appears just before cast start
            CastEnd(id + 1, 5);
            ComponentCondition<FirebreatheRotating>(id + 0x10, 0.7f, comp => comp.NumCasts > 0, "Cone 1");
            ComponentCondition<FirebreatheRotating>(id + 0x20, 8, comp => comp.NumCasts >= 5, "Cone 5")
                .DeactivateOnExit<FirebreatheRotating>();
        }
    }
}
