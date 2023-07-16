using System.Linq;

namespace BossMod.Endwalker.Alliance.A24Menphina
{
    class A24MenphinaStates : StateMachineBuilder
    {
        public A24MenphinaStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
        }

        private void SinglePhase(uint id)
        {
            BlueMoon(id, 7.2f, false);
            LovesLightNormalOne(id + 0x10000, 3.8f);
            MidnightFrost(id + 0x20000, 4.7f);
            LunarKiss(id + 0x30000, 5.4f, false);
            SilverMirrorNormalMoonsetWinterHalo(id + 0x40000, 3.8f);
            LovesLightNormalFourMidnightFrost(id + 0x50000, 5.5f);
            SelenainMysteria(id + 0x60000, 7.3f);
            MidnightFrostWaxingClaw(id + 0x100000, 5.4f);
            PlayfulOrbitMidnightFrostWaxingClaw(id + 0x110000, 6.2f);
            BlueMoon(id + 0x120000, 8.1f, true);
            KeenMoonbeamMidnightFrostWaxingClaw(id + 0x130000, 3.9f);
            CrateringChill(id + 0x140000, 3.9f);
            MoonsetRays(id + 0x150000, 6.0f);
            LovesLightMountedFour(id + 0x160000, 5.7f);
            SilverMirrorMounted(id + 0x170000, 2.0f);
            LovesLightMountedOneMidnightFrost(id + 0x180000, 7.2f);
            MoonsetRays(id + 0x190000, 6.0f);
            LunarKiss(id + 0x1A0000, 7.6f, true);
            LovesLightMountedFourMidnightFrost(id + 0x1B0000, 3.9f);
            BlueMoon(id + 0x1C0000, 0.7f, true);
            LunarKiss(id + 0x1D0000, 2.8f, true);
            KeenMoonbeamMidnightFrostWaxingClaw(id + 0x1E0000, 3.0f);
            //CrateringChill(id + 0x1F0000, 3.9f); // not sure, didn't see beyond first visual cast...
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void BlueMoon(uint id, float delay, bool mounted)
        {
            Cast(id, mounted ? AID.BlueMoonMounted : AID.BlueMoonNormal, delay, 5);
            ComponentCondition<BlueMoon>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<BlueMoon>()
                .DeactivateOnExit<BlueMoon>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void LunarKiss(uint id, float delay, bool mounted)
        {
            CastStart(id, mounted ? AID.LunarKissMounted : AID.LunarKissNormal, delay)
                .ActivateOnEnter<LunarKiss>();
            CastEnd(id + 1, 7);
            ComponentCondition<LunarKiss>(id + 2, 0.9f, comp => comp.NumCasts > 0, "Tankbusters")
                .DeactivateOnExit<LunarKiss>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void MoonsetRays(uint id, float delay)
        {
            Cast(id, AID.MoonsetRays, delay, 5, "Stack")
                .ActivateOnEnter<MoonsetRays>()
                .DeactivateOnExit<MoonsetRays>();
        }

        private void MidnightFrost(uint id, float delay)
        {
            CastMulti(id, new[] { AID.MidnightFrostShortNormalFront, AID.MidnightFrostShortNormalBack }, delay, 6)
                .ActivateOnEnter<MidnightFrostWaxingClaw>();
            ComponentCondition<MidnightFrostWaxingClaw>(id + 2, 0.2f, comp => comp.NumCasts > 0, "Half-arena cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>();
        }

        private void MidnightFrostWaxingClaw(uint id, float delay)
        {
            CastMulti(id, new[] { AID.MidnightFrostLongMountedFrontRight, AID.MidnightFrostLongMountedFrontLeft, AID.MidnightFrostLongMountedBackRight, AID.MidnightFrostLongMountedBackLeft }, delay, 8)
                .ActivateOnEnter<MidnightFrostWaxingClaw>();
            ComponentCondition<MidnightFrostWaxingClaw>(id + 2, 0.2f, comp => comp.NumCasts > 0, "Double cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>();
        }

        private void PlayfulOrbitMidnightFrostWaxingClaw(uint id, float delay)
        {
            CastMulti(id, new[] { AID.PlayfulOrbit1, AID.PlayfulOrbit2 }, delay, 2.6f);
            CastMulti(id + 0x10, new[] { AID.MidnightFrostLongDismounted1FrontRight, AID.MidnightFrostLongDismounted1FrontLeft, AID.MidnightFrostLongDismounted1BackRight, AID.MidnightFrostLongDismounted1BackLeft, AID.MidnightFrostLongDismounted2FrontRight, AID.MidnightFrostLongDismounted2FrontLeft, AID.MidnightFrostLongDismounted2BackRight, AID.MidnightFrostLongDismounted2BackLeft }, 2.2f, 8)
                .ActivateOnEnter<MidnightFrostWaxingClaw>();
            ComponentCondition<MidnightFrostWaxingClaw>(id + 0x12, 0.2f, comp => comp.NumCasts > 0, "Double cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>();
        }

        private void KeenMoonbeamMidnightFrostWaxingClaw(uint id, float delay)
        {
            Cast(id, AID.KeenMoonbeam, delay, 3);
            ComponentCondition<KeenMoonbeam>(id + 2, 1.5f, comp => comp.Active)
                .ActivateOnEnter<KeenMoonbeam>();
            CastStartMulti(id + 0x10, new[] { AID.MidnightFrostLongMountedFrontRight, AID.MidnightFrostLongMountedFrontLeft, AID.MidnightFrostLongMountedBackRight, AID.MidnightFrostLongMountedBackLeft }, 2.9f);
            ComponentCondition<KeenMoonbeam>(id + 0x11, 2.1f, comp => !comp.Active, "Spreads")
                .ActivateOnEnter<MidnightFrostWaxingClaw>()
                .DeactivateOnExit<KeenMoonbeam>();
            CastEnd(id + 0x12, 5.9f);
            ComponentCondition<MidnightFrostWaxingClaw>(id + 0x13, 0.2f, comp => comp.NumCasts > 0, "Double cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>();
        }

        private void CrateringChill(uint id, float delay)
        {
            Cast(id, AID.CrateringChill, delay, 3);
            ComponentCondition<CrateringChill>(id + 2, 1.5f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<CrateringChill>();
            Cast(id + 0x10, AID.WinterSolstice, 2.7f, 3);
            ComponentCondition<CrateringChill>(id + 0x20, 0.3f, comp => comp.NumCasts > 0, "Proximity + icy floor")
                .DeactivateOnExit<CrateringChill>();
            CastMulti(id + 0x30, new[] { AID.PlayfulOrbit1, AID.PlayfulOrbit2 }, 1.6f, 2.6f);
            CastMulti(id + 0x40, new[] { AID.WinterHaloLongDismounted1Right, AID.WinterHaloLongDismounted1Left, AID.WinterHaloLongDismounted2Right, AID.WinterHaloLongDismounted2Left }, 2.2f, 8)
                .ActivateOnEnter<MidnightFrostWaxingClaw>()
                .ActivateOnEnter<WinterHalo>();
            ComponentCondition<WinterHalo>(id + 0x42, 0.2f, comp => comp.NumCasts > 0, "Donut + half-arena cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>()
                .DeactivateOnExit<WinterHalo>();
        }

        private void LovesLightNormalOne(uint id, float delay)
        {
            Cast(id, AID.LovesLightNormalOne, delay, 3);
            // +0.9s: envcontrol .0E=00020001 = N moon
            Cast(id + 0x10, AID.FullBrightNormal, 2.7f, 3);
            ComponentCondition<FirstBlush>(id + 0x20, 0.9f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<FirstBlush>();
            ComponentCondition<FirstBlush>(id + 0x21, 10.5f, comp => comp.NumCasts > 0, "Line through center")
                .DeactivateOnExit<FirstBlush>();
        }

        private void LovesLightMountedOneMidnightFrost(uint id, float delay)
        {
            Cast(id, AID.LovesLightMountedOne, delay, 3);
            // +0.8s: envcontrol .11=00020001 = SW moon
            Cast(id + 0x10, AID.FullBrightMounted, 1.6f, 3);
            ComponentCondition<FirstBlush>(id + 0x20, 0.8f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<FirstBlush>();
            CastMulti(id + 0x30, new[] { AID.PlayfulOrbit1, AID.PlayfulOrbit2 }, 1.9f, 2.6f);
            CastStartMulti(id + 0x40, new[] { AID.MidnightFrostLongDismounted1FrontRight, AID.MidnightFrostLongDismounted1FrontLeft, AID.MidnightFrostLongDismounted1BackRight, AID.MidnightFrostLongDismounted1BackLeft, AID.MidnightFrostLongDismounted2FrontRight, AID.MidnightFrostLongDismounted2FrontLeft, AID.MidnightFrostLongDismounted2BackRight, AID.MidnightFrostLongDismounted2BackLeft }, 2.2f);
            ComponentCondition<FirstBlush>(id + 0x50, 3.8f, comp => comp.NumCasts > 0, "Line through center")
                .ActivateOnEnter<MidnightFrostWaxingClaw>()
                .DeactivateOnExit<FirstBlush>();
            CastEnd(id + 0x60, 4.2f);
            ComponentCondition<MidnightFrostWaxingClaw>(id + 0x70, 0.2f, comp => comp.NumCasts > 0, "Double cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>();
        }

        private void LovesLightNormalFourMidnightFrost(uint id, float delay)
        {
            Cast(id, AID.LovesLightNormalFour, delay, 3);
            // +0.9s: envcontrol .17/19/1A/1C=00020001 = N/S -> E/W moons
            Cast(id + 0x10, AID.FullBrightNormal, 2.7f, 3);
            ComponentCondition<LoversBridgeShort>(id + 0x20, 0.9f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<LoversBridgeShort>();
            ComponentCondition<LoversBridgeShort>(id + 0x21, 6, comp => comp.NumCasts > 0, "Moons 1")
                .DeactivateOnExit<LoversBridgeShort>();
            CastStartMulti(id + 0x30, new[] { AID.MidnightFrostShortNormalFront, AID.MidnightFrostShortNormalBack }, 5)
                .ActivateOnEnter<LoversBridgeLong>();
            ComponentCondition<LoversBridgeLong>(id + 0x31, 1, comp => comp.NumCasts > 0, "Moons 2")
                .ActivateOnEnter<MidnightFrostWaxingClaw>()
                .DeactivateOnExit<LoversBridgeLong>();
            CastEnd(id + 0x32, 5);
            ComponentCondition<MidnightFrostWaxingClaw>(id + 0x33, 0.2f, comp => comp.NumCasts > 0, "Half-arena cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>();
        }

        private void LovesLightMountedFour(uint id, float delay)
        {
            Cast(id, AID.LovesLightMountedFour, delay, 3);
            // +0.7s: envcontrol .16/18/1B/1D=00020001 = E/W -> N/S moons
            Cast(id + 0x10, AID.FullBrightMounted, 1.6f, 3);
            ComponentCondition<LoversBridgeShort>(id + 0x20, 0.8f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<LoversBridgeShort>();
            ComponentCondition<LoversBridgeShort>(id + 0x21, 6, comp => comp.NumCasts > 0, "Moons 1")
                .DeactivateOnExit<LoversBridgeShort>();
            ComponentCondition<LoversBridgeLong>(id + 0x22, 6, comp => comp.NumCasts > 0, "Moons 2")
                .ActivateOnEnter<LoversBridgeLong>()
                .DeactivateOnExit<LoversBridgeLong>();
        }

        private void LovesLightMountedFourMidnightFrost(uint id, float delay)
        {
            Cast(id, AID.LovesLightMountedFour, delay, 3);
            // +0.7s: envcontrol .17/19/1A/1C=00020001 = N/S -> E/W moons
            Cast(id + 0x10, AID.FullBrightMounted, 1.6f, 3);
            ComponentCondition<LoversBridgeShort>(id + 0x20, 0.8f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<LoversBridgeShort>();
            CastStartMulti(id + 0x30, new[] { AID.MidnightFrostShortMountedFront, AID.MidnightFrostShortMountedBack }, 3.1f);
            ComponentCondition<LoversBridgeShort>(id + 0x40, 2.9f, comp => comp.NumCasts > 0, "Moons 1")
                .ActivateOnEnter<MidnightFrostWaxingClaw>()
                .DeactivateOnExit<LoversBridgeShort>();
            CastEnd(id + 0x50, 3.1f)
                .ActivateOnEnter<LoversBridgeLong>();
            ComponentCondition<MidnightFrostWaxingClaw>(id + 0x51, 0.2f, comp => comp.NumCasts > 0, "Half-arena cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>();
            ComponentCondition<LoversBridgeLong>(id + 0x60, 2.7f, comp => comp.NumCasts > 0, "Moons 2")
                .DeactivateOnExit<LoversBridgeLong>();
        }

        private void SilverMirrorNormalMoonsetWinterHalo(uint id, float delay)
        {
            Cast(id, AID.SilverMirrorNormal, delay, 4, "Puddles")
                .ActivateOnEnter<SilverMirror>();
            Cast(id + 0x10, AID.Moonset, 2.7f, 4)
                .ActivateOnEnter<Moonset>()
                .DeactivateOnExit<SilverMirror>(); // last puddle ends ~1.3s into cast
            ComponentCondition<Moonset>(id + 0x20, 1, comp => comp.NumCasts >= 1, "Jump 1");
            ComponentCondition<Moonset>(id + 0x21, 2.2f, comp => comp.NumCasts >= 2, "Jump 2");
            ComponentCondition<Moonset>(id + 0x22, 2.2f, comp => comp.NumCasts >= 3, "Jump 3")
                .DeactivateOnExit<Moonset>();
            Cast(id + 0x30, AID.WinterHaloShort, 1.5f, 5)
                .ActivateOnEnter<WinterHalo>();
            ComponentCondition<WinterHalo>(id + 0x32, 0.3f, comp => comp.NumCasts > 0, "Donut")
                .DeactivateOnExit<WinterHalo>();
        }

        private void SilverMirrorMounted(uint id, float delay)
        {
            Cast(id, AID.SilverMirrorMounted, delay, 4, "Puddles")
                .ActivateOnEnter<SilverMirror>();
            CastMulti(id + 0x10, new[] { AID.MidnightFrostLongMountedFrontRight, AID.MidnightFrostLongMountedFrontLeft, AID.MidnightFrostLongMountedBackRight, AID.MidnightFrostLongMountedBackLeft, AID.WinterHaloLongMountedRight, AID.WinterHaloLongMountedLeft }, 1.7f, 8)
                .ActivateOnEnter<MidnightFrostWaxingClaw>()
                .ActivateOnEnter<WinterHalo>()
                .DeactivateOnExit<SilverMirror>(); // last puddle ends ~2.3s into cast
            ComponentCondition<MidnightFrostWaxingClaw>(id + 0x12, 0.2f, comp => comp.NumCasts > 0, "Donut + half-arena cleave / double cleave")
                .DeactivateOnExit<MidnightFrostWaxingClaw>()
                .DeactivateOnExit<WinterHalo>();
        }

        private void SelenainMysteria(uint id, float delay)
        {
            Cast(id, AID.SelenainMysteria, delay, 3, "Boss disappears")
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<CeremonialPillar>(id + 0x10, 4.5f, comp => comp.ActiveActors.Any(), "Adds appear")
                .ActivateOnEnter<CeremonialPillar>()
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            ComponentCondition<CeremonialPillar>(id + 0x100, 100, comp => !comp.ActiveActors.Any(), "Adds enrage")
                .ActivateOnEnter<AncientBlizzard>()
                .ActivateOnEnter<KeenMoonbeam>()
                .DeactivateOnExit<AncientBlizzard>()
                .DeactivateOnExit<KeenMoonbeam>()
                .DeactivateOnExit<CeremonialPillar>()
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<RiseOfTheTwinMoons>(id + 0x110, 11.1f, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<RiseOfTheTwinMoons>()
                .DeactivateOnExit<RiseOfTheTwinMoons>()
                .SetHint(StateMachine.StateHint.Raidwide);
            Targetable(id + 0x120, true, 3.3f, "Boss reappears");
        }
    }
}
