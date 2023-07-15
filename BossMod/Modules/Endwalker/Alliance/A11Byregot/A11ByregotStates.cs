//namespace BossMod.Endwalker.Alliance.A11Byregot
//{
//    class A11ByregotStates : StateMachineBuilder
//    {
//        public A11ByregotStates(BossModule module) : base(module)
//        {
//            OrdealOfThunder(0x00000000, 6.5f);
//            ByregotStrike(0x00010000, 5.9f);
//            ByregotWard(0x00020000, 8.2f);
//            BuilderBuildByregotStrike(0x00030000, 5.2f);

//            HammerPhase(0x00100000, 15.1f);
//        }

//        private void OrdealOfThunder(uint id, float delay)
//        {
//            Cast(id, AID.OrdealOfThunder, delay, 5, "Raidwide")
//                .SetHint(StateMachine.StateHint.Raidwide);
//        }

//        private void ByregotWard(uint id, float delay)
//        {
//            Cast(id, AID.ByregotWard, delay, 5, "Tankbuster")
//                .SetHint(StateMachine.StateHint.Tankbuster);
//        }

//        private void ByregotStrike(uint id, float delay)
//        {
//            Cast(id, AID.ByregotStrike, delay, 6)
//                .ActivateOnEnter<ByregotStrike>();
//            ComponentCondition<ByregotStrike>(id + 2, 1, comp => comp.Done, "Knockback")
//                .DeactivateOnExit<ByregotStrike>();
//        }

//        private void BuilderBuildByregotStrike(uint id, float delay)
//        {
//            Cast(id, AID.BuilderBuild, delay, 3);
//            Cast(id + 0x10, AID.ByregotStrikeWithCone, 2.8f, 6)
//                .ActivateOnEnter<ByregotStrike>();
//            ComponentCondition<ByregotStrike>(id + 2, 1, comp => comp.Done, "Cones + Knockback")
//                .DeactivateOnExit<ByregotStrike>();
//        }

//        private void HammerPhase(uint id, float delay)
//        {
//            ComponentCondition<Hammers>(id, delay, comp => comp.Active, "Destroy side tiles")
//                .ActivateOnEnter<Hammers>();
//        }
//    }
//}
