using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.P4S1
{
    // state related to elegant evisceration mechanic (dual hit tankbuster)
    // TODO: consider showing some tank swap / invul hint...
    public class ElegantEvisceration : CommonComponents.CastCounter
    {
        public ElegantEvisceration() : base(ActionID.MakeSpell(AID.ElegantEviscerationSecond)) { }
    }

    public class P4S1 : BossModule
    {
        public P4S1(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            StateMachine.State? s;
            s = Decollation(ref InitialState, 9.3f);
            s = BloodrakeBelone(ref s.Next, 4.2f);
            s = Decollation(ref s.Next, 3.4f);
            s = ElegantEvisceration(ref s.Next, 4.2f);
            s = Pinax(ref s.Next, 11.3f, true);
            s = ElegantEvisceration(ref s.Next, 4.4f);
            s = VengefulElementalBelone(ref s.Next, 4.2f);
            s = BeloneCoils(ref s.Next, 8.2f);
            s = Decollation(ref s.Next, 3.4f);
            s = ElegantEvisceration(ref s.Next, 4.2f);
            s = Pinax(ref s.Next, 11.3f, false);
            s = Decollation(ref s.Next, 0); // note: cast starts ~0.2s before pinax resolve, whatever...
            s = Decollation(ref s.Next, 4.2f);
            s = Decollation(ref s.Next, 4.2f);
            s = CommonStates.Targetable(ref s.Next, this, false, 10, "Enrage"); // checkpoint is triggered by boss becoming untargetable...
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }

        private StateMachine.State Decollation(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, this, AID.Decollation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State ElegantEvisceration(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, this, AID.ElegantEvisceration, delay, 5, "Tankbuster");
            cast.Exit.Add(() => ActivateComponent(new ElegantEvisceration()));
            cast.EndHint |= StateMachine.StateHint.Tankbuster | StateMachine.StateHint.GroupWithNext;

            var second = CommonStates.ComponentCondition<ElegantEvisceration>(ref cast.Next, 3.2f, this, comp => comp.NumCasts > 0, "Tankbuster");
            second.Exit.Add(DeactivateComponent<ElegantEvisceration>);
            second.EndHint |= StateMachine.StateHint.Tankbuster;
            return second;
        }

        private StateMachine.State InversiveChlamys(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, this, AID.InversiveChlamys, delay, 7);
            var resolve = CommonStates.ComponentCondition<InversiveChlamys>(ref cast.Next, 0.8f, this, comp => !comp.TethersActive, "Chlamys");
            return resolve;
        }

        private StateMachine.State BloodrakeBelone(ref StateMachine.State? link, float delay)
        {
            // note: just before (~0.1s) every bloodrake cast start, its targets are tethered to boss
            // targets of first bloodrake will be killed if they are targets of chlamys tethers later
            var bloodrake1 = CommonStates.Cast(ref link, this, AID.Bloodrake, delay, 4, "Bloodrake 1");
            bloodrake1.Enter.Add(() => ActivateComponent(new InversiveChlamys(this, true)));
            bloodrake1.EndHint |= StateMachine.StateHint.GroupWithNext;

            // this cast is pure flavour and does nothing (replaces status 2799 'Aethersucker' with status 2800 'Casting Chlamys' on boss)
            var aetheric = CommonStates.Cast(ref bloodrake1.Next, this, AID.AethericChlamys, 3.2f, 4);

            // targets of second bloodrake will be killed if they are targets of 'Cursed Casting' (which targets players with 'Role Call')
            var bloodrake2 = CommonStates.Cast(ref aetheric.Next, this, AID.Bloodrake, 4.2f, 4, "Bloodrake 2");
            bloodrake2.Enter.Add(() => ActivateComponent(new DirectorsBelone(this, true)));
            bloodrake2.EndHint |= StateMachine.StateHint.GroupWithNext;

            // this cast removes status 2799 'Aethersucker' from boss
            // right after it ends, instant cast 27111 applies 'Role Call' debuffs - corresponding component handles that
            var beloneStart = CommonStates.CastStart(ref bloodrake2.Next, this, AID.DirectorsBelone, 4.2f);
            beloneStart.EndHint |= StateMachine.StateHint.PositioningStart;

            var beloneEnd = CommonStates.CastEnd(ref beloneStart.Next, this, 5);

            // Cursed Casting happens right before (0.5s) chlamys resolve
            var inv = InversiveChlamys(ref beloneEnd.Next, 9.2f);
            inv.Exit.Add(DeactivateComponent<InversiveChlamys>);
            inv.Exit.Add(DeactivateComponent<DirectorsBelone>);
            inv.EndHint |= StateMachine.StateHint.PositioningEnd;
            return inv;
        }

        private StateMachine.State Pinax(ref StateMachine.State? link, float delay, bool keepScene)
        {
            var setting = CommonStates.Cast(ref link, this, AID.SettingTheScene, delay, 4, "Scene");
            setting.Exit.Add(() => ActivateComponent(new SettingTheScene(this)));
            setting.Exit.Add(() => ActivateComponent(new PinaxUptime(this)));
            setting.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            // ~1s after cast end, we get a bunch of env controls

            var pinaxStart = CommonStates.CastStart(ref setting.Next, this, AID.Pinax, 8.2f);
            pinaxStart.Exit.Add(DeactivateComponent<PinaxUptime>);
            pinaxStart.EndHint |= StateMachine.StateHint.PositioningEnd;

            var pinaxEnd = CommonStates.CastEnd(ref pinaxStart.Next, this, 5, "Pinax");
            pinaxEnd.Exit.Add(() => ActivateComponent(new Pinax(this)));
            pinaxEnd.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            // timeline:
            //  0.0s pinax cast end
            //  1.0s square 1 activation: env control (.10 = 00800040), helper starts casting 27095
            //  4.0s square 2 activation: env control (.15 = 00800040), helper starts casting 27092
            //  7.0s square 1 env control (.10 = 02000001)
            // 10.0s square 2 env control (.15 = 02000001)
            //       square 1 cast finish (+ instant 27091)
            // 13.0s square 2 cast finish (+ instant 27088)
            // 14.0s square 3 activation: env control (.20 = 00800040), helper starts casting 27094
            // 20.0s square 3 env control (.20 = 02000001)
            // 23.0s square 3 cast finish (+ instant 27090)
            // 25.0s square 4 activation: env control (.05 = 00800040), helper starts casting 27093
            // 31.0s square 4 env control (.05 = 02000001)
            // 34.0s square 4 cast finish (+ instant 27089)

            var p1 = CommonStates.ComponentCondition<Pinax>(ref pinaxEnd.Next, 10, this, comp => comp.NumFinished == 1, "Corner1");
            p1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var p2 = CommonStates.ComponentCondition<Pinax>(ref p1.Next, 3, this, comp => comp.NumFinished == 2, "Corner2");
            p2.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningEnd;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.NortherlyShiftCloak] = new(null, () => { });
            dispatch[AID.SoutherlyShiftCloak] = new(null, () => { });
            dispatch[AID.EasterlyShiftCloak] = new(null, () => { });
            dispatch[AID.WesterlyShiftCloak] = new(null, () => { });
            dispatch[AID.NortherlyShiftSword] = new(null, () => { });
            dispatch[AID.SoutherlyShiftSword] = new(null, () => { });
            dispatch[AID.EasterlyShiftSword] = new(null, () => { });
            dispatch[AID.WesterlyShiftSword] = new(null, () => { });
            var shiftStart = CommonStates.CastStart(ref p2.Next, this, dispatch, 3.6f);
            shiftStart.Exit.Add(() => ActivateComponent(new Shift(this))); // together with this, one of the helpers starts casting 27142 or 27137
            shiftStart.EndHint |= StateMachine.StateHint.PositioningStart;

            var p3 = CommonStates.ComponentCondition<Pinax>(ref shiftStart.Next, 6.4f, this, comp => comp.NumFinished == 3, "Corner3");
            p3.EndHint |= StateMachine.StateHint.GroupWithNext;

            var shiftEnd = CommonStates.CastEnd(ref p3.Next, this, 1.6f, "Shift");
            shiftEnd.EndHint |= StateMachine.StateHint.GroupWithNext;

            var p4 = CommonStates.ComponentCondition<Pinax>(ref shiftEnd.Next, 9.4f, this, comp => comp.NumFinished == 4, "Pinax resolve");
            if (!keepScene)
                p4.Exit.Add(DeactivateComponent<SettingTheScene>);
            p4.Exit.Add(DeactivateComponent<Pinax>);
            p4.Exit.Add(DeactivateComponent<Shift>);
            p4.EndHint |= StateMachine.StateHint.PositioningEnd;
            return p4;
        }

        private StateMachine.State VengefulElementalBelone(ref StateMachine.State? link, float delay)
        {
            // all other bloodrakes target all players
            // third bloodrake in addition 'targets' three of the four corner helpers - untethered one is safe during later mechanic
            var bloodrake3 = CommonStates.Cast(ref link, this, AID.Bloodrake, delay, 4, "Bloodrake 3");
            bloodrake3.Enter.Add(() => ActivateComponent(new ElementalBelone(this)));
            bloodrake3.Exit.Add(DeactivateComponent<SettingTheScene>);
            bloodrake3.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var setting = CommonStates.Cast(ref bloodrake3.Next, this, AID.SettingTheScene, 7.2f, 4, "Scene");
            setting.Exit.Add(() => ActivateComponent(new SettingTheScene(this)));
            setting.Exit.Add(() => FindComponent<ElementalBelone>()!.Visible = true);
            setting.EndHint |= StateMachine.StateHint.GroupWithNext;

            var vengeful = CommonStates.Cast(ref setting.Next, this, AID.VengefulBelone, 8.2f, 4, "Roles"); // acting X applied after cast end
            vengeful.Exit.Add(() => ActivateComponent(new VengefulBelone(this)));
            vengeful.EndHint |= StateMachine.StateHint.GroupWithNext;

            var elemental = CommonStates.Cast(ref vengeful.Next, this, AID.ElementalBelone, 4.2f, 4); // 'elemental resistance down' applied after cast end

            var bloodrake4 = CommonStates.Cast(ref elemental.Next, this, AID.Bloodrake, 4.2f, 4, "Bloodrake 4");
            bloodrake4.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var bursts = CommonStates.Cast(ref bloodrake4.Next, this, AID.BeloneBursts, 4.2f, 5, "Orbs"); // orbs appear at cast start, tether and start moving at cast end
            bursts.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var periaktoi = CommonStates.Cast(ref bursts.Next, this, AID.Periaktoi, 9.2f, 5, "Square explode");
            periaktoi.Exit.Add(DeactivateComponent<SettingTheScene>);
            periaktoi.Exit.Add(DeactivateComponent<ElementalBelone>);
            periaktoi.Exit.Add(DeactivateComponent<VengefulBelone>); // TODO: reconsider deactivation time, debuffs fade ~12s later, but I think vengeful needs to be handled before explosion?
            periaktoi.EndHint |= StateMachine.StateHint.PositioningEnd;
            return periaktoi;
        }

        private StateMachine.State BeloneCoils(ref StateMachine.State? link, float delay)
        {
            var bloodrake5 = CommonStates.Cast(ref link, this, AID.Bloodrake, delay, 4, "Bloodrake 5");
            bloodrake5.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var coils1 = CommonStates.Cast(ref bloodrake5.Next, this, AID.BeloneCoils, 3.2f, 4, "Coils 1");
            coils1.Exit.Add(() => ActivateComponent(new BeloneCoils(this)));
            coils1.Exit.Add(() => ActivateComponent(new InversiveChlamys(this, false)));
            coils1.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var inv1 = InversiveChlamys(ref coils1.Next, 3.2f);
            inv1.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningEnd;

            var aetheric = CommonStates.Cast(ref inv1.Next, this, AID.AethericChlamys, 2.4f, 4);

            var bloodrake6 = CommonStates.Cast(ref aetheric.Next, this, AID.Bloodrake, 4.2f, 4, "Bloodrake 6");
            bloodrake6.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var coils2 = CommonStates.Cast(ref bloodrake6.Next, this, AID.BeloneCoils, 4.2f, 4, "Coils 2");
            coils2.Exit.Add(() => ActivateComponent(new DirectorsBelone(this, false)));
            coils2.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var belone = CommonStates.Cast(ref coils2.Next, this, AID.DirectorsBelone, 9.2f, 5);

            var inv2 = InversiveChlamys(ref belone.Next, 9.2f);
            inv2.Exit.Add(DeactivateComponent<BeloneCoils>);
            inv2.Exit.Add(DeactivateComponent<InversiveChlamys>);
            inv2.Exit.Add(DeactivateComponent<DirectorsBelone>);
            inv2.EndHint |= StateMachine.StateHint.PositioningEnd;
            return inv2;
        }
    }
}
