using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.P4S
{
    // state related to elegant evisceration mechanic (dual hit tankbuster)
    // TODO: consider showing some tank swap / invul hint...
    public class ElegantEvisceration : CommonComponents.CastCounter
    {
        public ElegantEvisceration() : base(ActionID.MakeSpell(AID.ElegantEviscerationSecond)) { }
    }

    // state related to demigod double mechanic (shared tankbuster)
    class DemigodDouble : CommonComponents.SharedTankbuster
    {
        public DemigodDouble(P4S module) : base(module, module.Enemies(OID.Boss2), 6) { }
    }

    // state related to heart stake mechanic (dual hit tankbuster with bleed)
    // TODO: consider showing some tank swap / invul hint...
    class HeartStake : CommonComponents.CastCounter
    {
        public HeartStake() : base(ActionID.MakeSpell(AID.HeartStakeSecond)) { }
    }

    public class P4S : BossModule
    {
        // common wreath of thorns constants
        public static float WreathAOERadius = 20;
        public static float WreathTowerRadius = 4;

        private List<WorldState.Actor> _boss1;
        private List<WorldState.Actor> _boss2;
        public WorldState.Actor? Boss1() => _boss1.FirstOrDefault();
        public WorldState.Actor? Boss2() => _boss2.FirstOrDefault();

        public P4S(WorldState ws)
            : base(ws, 8)
        {
            _boss1 = Enemies(OID.Boss1);
            _boss2 = Enemies(OID.Boss2);

            StateMachine.State? p1 = null, p2 = null;
            Phase1(ref p1);
            Phase2(ref p2);
            p1!.Enter.Add(() => Arena.IsCircle = false);
            p2!.Enter.Add(() => Arena.IsCircle = true);

            var fork = CommonStates.Simple(ref InitialState, 0);
            fork.Update = (_) => Boss1() != null ? p1 : p2;
            fork.PotentialSuccessors = new[] { p1!, p2! };
        }

        protected override void DrawArenaForegroundPost()
        {
            Arena.Actor(Boss1() ?? Boss2(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
        }

        private StateMachine.State Phase1(ref StateMachine.State? link)
        {
            StateMachine.State? s;
            s = Decollation(ref link, 9.3f);
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
            s = CommonStates.Targetable(ref s.Next, Boss1, false, 10, "Enrage"); // checkpoint is triggered by boss becoming untargetable...
            return s;
        }

        private StateMachine.State Decollation(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss1, AID.Decollation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State ElegantEvisceration(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, Boss1, AID.ElegantEvisceration, delay, 5, "Tankbuster");
            cast.Exit.Add(() => ActivateComponent(new ElegantEvisceration()));
            cast.EndHint |= StateMachine.StateHint.Tankbuster | StateMachine.StateHint.GroupWithNext;

            var second = CommonStates.ComponentCondition<ElegantEvisceration>(ref cast.Next, 3.2f, this, comp => comp.NumCasts > 0, "Tankbuster");
            second.Exit.Add(DeactivateComponent<ElegantEvisceration>);
            second.EndHint |= StateMachine.StateHint.Tankbuster;
            return second;
        }

        private StateMachine.State InversiveChlamys(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, Boss1, AID.InversiveChlamys, delay, 7);
            var resolve = CommonStates.ComponentCondition<InversiveChlamys>(ref cast.Next, 0.8f, this, comp => !comp.TethersActive, "Chlamys");
            return resolve;
        }

        private StateMachine.State BloodrakeBelone(ref StateMachine.State? link, float delay)
        {
            // note: just before (~0.1s) every bloodrake cast start, its targets are tethered to boss
            // targets of first bloodrake will be killed if they are targets of chlamys tethers later
            var bloodrake1 = CommonStates.Cast(ref link, Boss1, AID.Bloodrake, delay, 4, "Bloodrake 1");
            bloodrake1.Enter.Add(() => ActivateComponent(new InversiveChlamys(this, true)));
            bloodrake1.EndHint |= StateMachine.StateHint.GroupWithNext;

            // this cast is pure flavour and does nothing (replaces status 2799 'Aethersucker' with status 2800 'Casting Chlamys' on boss)
            var aetheric = CommonStates.Cast(ref bloodrake1.Next, Boss1, AID.AethericChlamys, 3.2f, 4);

            // targets of second bloodrake will be killed if they are targets of 'Cursed Casting' (which targets players with 'Role Call')
            var bloodrake2 = CommonStates.Cast(ref aetheric.Next, Boss1, AID.Bloodrake, 4.2f, 4, "Bloodrake 2");
            bloodrake2.Enter.Add(() => ActivateComponent(new DirectorsBelone(this, true)));
            bloodrake2.EndHint |= StateMachine.StateHint.GroupWithNext;

            // this cast removes status 2799 'Aethersucker' from boss
            // right after it ends, instant cast 27111 applies 'Role Call' debuffs - corresponding component handles that
            var beloneStart = CommonStates.CastStart(ref bloodrake2.Next, Boss1, AID.DirectorsBelone, 4.2f);
            beloneStart.EndHint |= StateMachine.StateHint.PositioningStart;

            var beloneEnd = CommonStates.CastEnd(ref beloneStart.Next, Boss1, 5);

            // Cursed Casting happens right before (0.5s) chlamys resolve
            var inv = InversiveChlamys(ref beloneEnd.Next, 9.2f);
            inv.Exit.Add(DeactivateComponent<InversiveChlamys>);
            inv.Exit.Add(DeactivateComponent<DirectorsBelone>);
            inv.EndHint |= StateMachine.StateHint.PositioningEnd;
            return inv;
        }

        private StateMachine.State Pinax(ref StateMachine.State? link, float delay, bool keepScene)
        {
            var setting = CommonStates.Cast(ref link, Boss1, AID.SettingTheScene, delay, 4, "Scene");
            setting.Exit.Add(() => ActivateComponent(new SettingTheScene(this)));
            setting.Exit.Add(() => ActivateComponent(new PinaxUptime(this)));
            setting.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            // ~1s after cast end, we get a bunch of env controls

            var pinaxStart = CommonStates.CastStart(ref setting.Next, Boss1, AID.Pinax, 8.2f);
            pinaxStart.Exit.Add(DeactivateComponent<PinaxUptime>);
            pinaxStart.EndHint |= StateMachine.StateHint.PositioningEnd;

            var pinaxEnd = CommonStates.CastEnd(ref pinaxStart.Next, Boss1, 5, "Pinax");
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
            var shiftStart = CommonStates.CastStart(ref p2.Next, Boss1, dispatch, 3.6f);
            shiftStart.Exit.Add(() => ActivateComponent(new Shift(this))); // together with this, one of the helpers starts casting 27142 or 27137
            shiftStart.EndHint |= StateMachine.StateHint.PositioningStart;

            var p3 = CommonStates.ComponentCondition<Pinax>(ref shiftStart.Next, 6.4f, this, comp => comp.NumFinished == 3, "Corner3");
            p3.EndHint |= StateMachine.StateHint.GroupWithNext;

            var shiftEnd = CommonStates.CastEnd(ref p3.Next, Boss1, 1.6f, "Shift");
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
            var bloodrake3 = CommonStates.Cast(ref link, Boss1, AID.Bloodrake, delay, 4, "Bloodrake 3");
            bloodrake3.Enter.Add(() => ActivateComponent(new ElementalBelone(this)));
            bloodrake3.Exit.Add(DeactivateComponent<SettingTheScene>);
            bloodrake3.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var setting = CommonStates.Cast(ref bloodrake3.Next, Boss1, AID.SettingTheScene, 7.2f, 4, "Scene");
            setting.Exit.Add(() => ActivateComponent(new SettingTheScene(this)));
            setting.Exit.Add(() => FindComponent<ElementalBelone>()!.Visible = true);
            setting.EndHint |= StateMachine.StateHint.GroupWithNext;

            var vengeful = CommonStates.Cast(ref setting.Next, Boss1, AID.VengefulBelone, 8.2f, 4, "Roles"); // acting X applied after cast end
            vengeful.Exit.Add(() => ActivateComponent(new VengefulBelone(this)));
            vengeful.EndHint |= StateMachine.StateHint.GroupWithNext;

            var elemental = CommonStates.Cast(ref vengeful.Next, Boss1, AID.ElementalBelone, 4.2f, 4); // 'elemental resistance down' applied after cast end

            var bloodrake4 = CommonStates.Cast(ref elemental.Next, Boss1, AID.Bloodrake, 4.2f, 4, "Bloodrake 4");
            bloodrake4.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var bursts = CommonStates.Cast(ref bloodrake4.Next, Boss1, AID.BeloneBursts, 4.2f, 5, "Orbs"); // orbs appear at cast start, tether and start moving at cast end
            bursts.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var periaktoi = CommonStates.Cast(ref bursts.Next, Boss1, AID.Periaktoi, 9.2f, 5, "Square explode");
            periaktoi.Exit.Add(DeactivateComponent<SettingTheScene>);
            periaktoi.Exit.Add(DeactivateComponent<ElementalBelone>);
            periaktoi.Exit.Add(DeactivateComponent<VengefulBelone>); // TODO: reconsider deactivation time, debuffs fade ~12s later, but I think vengeful needs to be handled before explosion?
            periaktoi.EndHint |= StateMachine.StateHint.PositioningEnd;
            return periaktoi;
        }

        private StateMachine.State BeloneCoils(ref StateMachine.State? link, float delay)
        {
            var bloodrake5 = CommonStates.Cast(ref link, Boss1, AID.Bloodrake, delay, 4, "Bloodrake 5");
            bloodrake5.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var coils1 = CommonStates.Cast(ref bloodrake5.Next, Boss1, AID.BeloneCoils, 3.2f, 4, "Coils 1");
            coils1.Exit.Add(() => ActivateComponent(new BeloneCoils(this)));
            coils1.Exit.Add(() => ActivateComponent(new InversiveChlamys(this, false)));
            coils1.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var inv1 = InversiveChlamys(ref coils1.Next, 3.2f);
            inv1.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningEnd;

            var aetheric = CommonStates.Cast(ref inv1.Next, Boss1, AID.AethericChlamys, 2.4f, 4);

            var bloodrake6 = CommonStates.Cast(ref aetheric.Next, Boss1, AID.Bloodrake, 4.2f, 4, "Bloodrake 6");
            bloodrake6.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Raidwide;

            var coils2 = CommonStates.Cast(ref bloodrake6.Next, Boss1, AID.BeloneCoils, 4.2f, 4, "Coils 2");
            coils2.Exit.Add(() => ActivateComponent(new DirectorsBelone(this, false)));
            coils2.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var belone = CommonStates.Cast(ref coils2.Next, Boss1, AID.DirectorsBelone, 9.2f, 5);

            var inv2 = InversiveChlamys(ref belone.Next, 9.2f);
            inv2.Exit.Add(DeactivateComponent<BeloneCoils>);
            inv2.Exit.Add(DeactivateComponent<InversiveChlamys>);
            inv2.Exit.Add(DeactivateComponent<DirectorsBelone>);
            inv2.EndHint |= StateMachine.StateHint.PositioningEnd;
            return inv2;
        }

        private StateMachine.State Phase2(ref StateMachine.State? link)
        {
            StateMachine.State? s;
            s = SearingStream(ref link, 10);
            s = AkanthaiAct1(ref s.Next, 10.2f);
            s = FarNearSight(ref s.Next, 1);
            s = AkanthaiAct2(ref s.Next, 7.1f);
            s = AkanthaiAct3(ref s.Next, 8.2f);
            s = FarNearSight(ref s.Next, 4.1f);
            s = HeartStake(ref s.Next, 9.2f);
            s = AkanthaiAct4(ref s.Next, 4.2f);
            s = SearingStream(ref s.Next, 9.3f);
            s = AkanthaiAct5(ref s.Next, 4.2f);
            s = SearingStream(ref s.Next, 7.2f);
            s = DemigodDouble(ref s.Next, 4.2f);
            s = AkanthaiAct6(ref s.Next, 8.2f);
            s = CommonStates.Cast(ref s.Next, Boss2, AID.Enrage, 4.4f, 10, "Enrage"); // not sure whether it's really an enrage, but it's unique aid with 10 sec cast...
            return s;
        }

        private StateMachine.State SearingStream(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss2, AID.SearingStream, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State UltimateImpulse(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss2, AID.UltimateImpulse, delay, 7, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State FarNearSight(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.Nearsight] = new(null, () => ActivateComponent(new NearFarSight(this, NearFarSight.State.Near)));
            dispatch[AID.Farsight] = new(null, () => ActivateComponent(new NearFarSight(this, NearFarSight.State.Far)));
            var start = CommonStates.CastStart(ref link, Boss2, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss2, 5);

            var resolve = CommonStates.ComponentCondition<NearFarSight>(ref end.Next, 1.1f, this, comp => comp.CurState == NearFarSight.State.Done, "Far/nearsight");
            resolve.Exit.Add(DeactivateComponent<NearFarSight>);
            resolve.EndHint |= StateMachine.StateHint.Tankbuster | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State DemigodDouble(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss2, AID.DemigodDouble, delay, 5, "SharedTankbuster");
            s.Enter.Add(() => ActivateComponent(new DemigodDouble(this)));
            s.Exit.Add(DeactivateComponent<DemigodDouble>);
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State HeartStake(ref StateMachine.State? link, float delay)
        {
            var comp = FindComponent<HeartStake>()!;
            var cast = CommonStates.Cast(ref link, Boss2, AID.HeartStake, delay, 5, "Tankbuster");
            cast.Exit.Add(() => ActivateComponent(new HeartStake()));
            cast.EndHint |= StateMachine.StateHint.Tankbuster | StateMachine.StateHint.GroupWithNext;

            var second = CommonStates.ComponentCondition<HeartStake>(ref cast.Next, 3.1f, this, comp => comp.NumCasts > 0, "Tankbuster");
            second.Exit.Add(DeactivateComponent<HeartStake>);
            second.EndHint |= StateMachine.StateHint.Tankbuster;
            return second;
        }

        private StateMachine.State HellSting(ref StateMachine.State? link, float delay)
        {
            // timeline:
            // 0.0s: cast start (boss visual + helpers real)
            // 2.4s: visual cast end
            // 3.0s: first aoes (helpers cast end)
            // 5.5s: boss visual instant cast + helpers start cast
            // 6.1s: second aoes (helpers cast end)
            var cast = CommonStates.Cast(ref link, Boss2, AID.HellsSting, delay, 2.4f);
            cast.Enter.Add(() => ActivateComponent(new HellsSting(this)));

            var hit1 = CommonStates.ComponentCondition<HellsSting>(ref cast.Next, 0.6f, this, comp => comp.NumCasts > 0, "Cone");
            hit1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var hit2 = CommonStates.ComponentCondition<HellsSting>(ref hit1.Next, 3.1f, this, comp => comp.NumCasts > 8, "Cone");
            hit2.Exit.Add(DeactivateComponent<HellsSting>);
            return hit2;
        }

        private StateMachine.State AkanthaiAct1(ref StateMachine.State? link, float delay)
        {
            // 'act 1' is 4 aoes (N/S/E/W) and 8 towers; explosion order is 2 opposite aoes -> all towers -> remaining aoes
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
            // aoes are at (82/118, 100) and (100, 82/118), towers are at (95.05/104.95, 95.05/104.95) and (88.69/111.31, 88.69/111.31)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiAct1, delay, 5, "Act1");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe = SearingStream(ref intro.Next, 4.2f);
            aoe.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            // -0.1s: first 2 aoes tethered
            //  0.0s: wreath cast start ==> component determines order and starts showing first aoe pair
            //  3.0s: towers tethered
            //  6.0s: last 2 aoes tethered
            //  8.0s: wreath cast end
            // 10.0s: first 2 aoes start cast 27149
            // 11.0s: first 2 aoes finish cast ==> component starts showing towers
            // 13.0s: towers start cast 27150
            // 14.0s: towers finish cast ==> component starts showing second aoe pair
            // 16.0s: last 2 aoes start cast 27149
            // 17.0s: last 2 aoes finish cast ==> component is reset
            // 18.0s: boss starts casting far/nearsight
            var wreath = CommonStates.Cast(ref aoe.Next, Boss2, AID.WreathOfThorns1, 6.2f, 8, "Wreath1");
            wreath.Enter.Add(() => ActivateComponent(new WreathOfThorns1(this)));
            wreath.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var aoe1 = CommonStates.ComponentCondition<WreathOfThorns1>(ref wreath.Next, 3, this, comp => comp.CurState != WreathOfThorns1.State.FirstAOEs, "AOE 1");
            aoe1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe2 = CommonStates.ComponentCondition<WreathOfThorns1>(ref aoe1.Next, 3, this, comp => comp.CurState != WreathOfThorns1.State.Towers, "Towers");
            aoe2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe3 = CommonStates.ComponentCondition<WreathOfThorns1>(ref aoe2.Next, 3, this, comp => comp.CurState != WreathOfThorns1.State.LastAOEs, "AOE 2");
            aoe3.Exit.Add(DeactivateComponent<WreathOfThorns1>);
            aoe3.EndHint |= StateMachine.StateHint.PositioningEnd;
            return aoe3;
        }

        private StateMachine.State AkanthaiAct2(ref StateMachine.State? link, float delay)
        {
            // 'act 2' is 4 aoes and 4 towers + player pairwise tethers
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
            // towers are at (96,82), (118,96), (104,118) and (82,104); aoes are at (104,82), (118,104), (96,118) and (82,96)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiAct2, delay, 5, "Act2");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            var dd = DemigodDouble(ref intro.Next, 4.2f);
            dd.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            // -0.1s: two towers and two aoes tethered
            //  0.0s: wreath cast start ==> component determines order and starts showing first set
            //  3.0s: remaining tethers
            //  6.0s: wreath cast end
            //  6.8s: icons + tethers appear (1 dd pair and 1 tank-healer pair with fire, 1 dd pair with wind, 1 tank-healer pair with dark on healer) ==> component starts showing 'break' hint on dark pair and 'stack in center' hint on everyone else
            //  9.2s: dark design cast start
            // 11.8s: 'thornpricked' debuffs
            // 14.2s: dark design cast end ==> component starts showing 'gtfo from aoe/soak tower' hint + 'break' hint for next pair
            // 18.1s: first 2 aoes and towers start cast 27149/27150
            // 19.1s: first 2 aoes and towers finish cast => component starts showing second set
            // 25.1s: last 2 aoes and towers start cast 27149/27150
            // 26.1s: last 2 aoes and towers finish cast => component is reset
            // 26.4s: boss starts casting aoe
            // 27.8s: wind pair expires if not broken
            // 33.4s: boss finishes casting aoe
            var wreath = CommonStates.Cast(ref dd.Next, Boss2, AID.WreathOfThorns2, 4.2f, 6, "Wreath2");
            wreath.Enter.Add(() => ActivateComponent(new WreathOfThorns2(this)));
            wreath.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var darkDesign = CommonStates.Cast(ref wreath.Next, Boss2, AID.DarkDesign, 3.2f, 5, "DarkDesign");
            darkDesign.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve1 = CommonStates.ComponentCondition<WreathOfThorns2>(ref darkDesign.Next, 4.9f, this, comp => comp.CurState != WreathOfThorns2.State.FirstSet, "Resolve 1");
            resolve1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve2 = CommonStates.ComponentCondition<WreathOfThorns2>(ref resolve1.Next, 7, this, comp => comp.CurState != WreathOfThorns2.State.SecondSet, "Resolve 2");
            resolve2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe = UltimateImpulse(ref resolve2.Next, 0.3f);
            aoe.Exit.Add(DeactivateComponent<WreathOfThorns2>);
            aoe.EndHint |= StateMachine.StateHint.PositioningEnd;
            return aoe;
        }

        private StateMachine.State AkanthaiAct3(ref StateMachine.State? link, float delay)
        {
            // 'act 3' is two sets of 4 towers + jumps and knockback from center
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and knockback
            // towers are at (82.61/117.39, 104.66/95.34) and (87.27/112.73, 87.27/112.73)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiAct3, delay, 5, "Act3");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            // -0.1s: four towers (E/W) tethered
            //  0.0s: wreath cast start ==> component should determine order and show spots for everyone (rdd/healers to soak, some tank to bait jump)
            //  3.0s: center tether
            //  6.0s: remaining tethers
            //  8.0s: wreath cast end
            // 11.2s: kick cast start
            // 16.1s: kick cast end
            // 16.3s: first jump ==> component should switch from jump to cone mode
            // 20.0s: first towers start cast 27150
            // 20.2s: cones 1 go off ==> component should switch to second jump mode
            // 21.0s: first towers finish cast => component should show second towers
            // 22.0s: central tower starts cast 27152
            // 23.0s: central tower finishes cast
            // 26.0s: second towers start cast 27150
            // 26.4s: second jump ==> component should switch to second cone mode
            // 27.0s: second towers finish cast
            // 30.4s: second cones
            var wreath = CommonStates.Cast(ref intro.Next, Boss2, AID.WreathOfThorns3, 4.2f, 8, "Wreath3");
            wreath.Enter.Add(() => ActivateComponent(new WreathOfThorns3(this)));
            wreath.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var jump1 = CommonStates.Cast(ref wreath.Next, Boss2, AID.KothornosKock, 3.2f, 4.9f, "Jump1");
            jump1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var cones1 = CommonStates.ComponentCondition<WreathOfThorns3>(ref jump1.Next, 4.9f, this, comp => comp.NumCones > 0, "Cones1");
            cones1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var towers1 = CommonStates.ComponentCondition<WreathOfThorns3>(ref cones1.Next, 0.8f, this, comp => comp.CurState != WreathOfThorns3.State.RangedTowers, "Towers1");
            towers1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var knockback = CommonStates.ComponentCondition<WreathOfThorns3>(ref towers1.Next, 2, this, comp => comp.CurState != WreathOfThorns3.State.Knockback, "Knockback");
            knockback.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Knockback;

            var jump2 = CommonStates.ComponentCondition<WreathOfThorns3>(ref knockback.Next, 3.4f, this, comp => comp.NumJumps > 1, "Jump2");
            jump2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var towers2 = CommonStates.ComponentCondition<WreathOfThorns3>(ref jump2.Next, 0.6f, this, comp => comp.CurState != WreathOfThorns3.State.MeleeTowers, "Towers2");
            towers2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var cones2 = CommonStates.ComponentCondition<WreathOfThorns3>(ref towers2.Next, 3.4f, this, comp => comp.NumCones > 1, "Cones2");
            cones2.Exit.Add(DeactivateComponent<WreathOfThorns3>);
            cones2.EndHint |= StateMachine.StateHint.PositioningEnd;
            return cones2;
        }

        private StateMachine.State AkanthaiAct4(ref StateMachine.State? link, float delay)
        {
            // 'act 4' is 4 towers + 4 aoes, tethered to players
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers and aoes
            // towers are at (82/118, 100) and (100, 82/118), aoes are at (87.27/112.73, 87.27/112.73)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiAct4, delay, 5, "Act4");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe1 = SearingStream(ref intro.Next, 4.2f);
            aoe1.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            //  0.0s: wreath cast ends
            //  0.8s: icons and tethers appear
            //  3.2s: searing stream cast start
            //  5.8s: 'thornpricked' debuffs
            //  8.2s: searing stream cast end
            // .....: blow up tethers
            // 36.4s: ultimate impulse cast start
            var wreath = CommonStates.Cast(ref aoe1.Next, Boss2, AID.WreathOfThorns4, 4.2f, 5, "Wreath4");
            wreath.Exit.Add(() => ActivateComponent(new WreathOfThorns4(this)));
            wreath.EndHint |= StateMachine.StateHint.GroupWithNext;

            var aoe2 = SearingStream(ref wreath.Next, 3.2f);
            aoe2.Exit.Add(() => FindComponent<WreathOfThorns4>()!.ReadyToBreak = true);
            aoe2.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var aoe3 = UltimateImpulse(ref aoe2.Next, 28.2f);
            aoe3.Exit.Add(DeactivateComponent<WreathOfThorns4>);
            aoe3.EndHint |= StateMachine.StateHint.PositioningEnd;
            return aoe3;
        }

        private StateMachine.State AkanthaiAct5(ref StateMachine.State? link, float delay)
        {
            // 'act 5' ('finale') is 8 staggered towers that should be soaked in correct order
            // 'intro' cast is pure flavour, it is cast together with 'visual' casts by towers
            // towers are at (88/112, 100), (100, 88/112), (91.5/108.5, 91.5/108.5)
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiFinale, delay, 5, "Act5");
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            // timeline:
            //  0.0s: wreath cast ends
            //  0.8s: icons and tethers appear
            //  3.2s: fleeting impulse cast start
            //  5.8s: 'thornpricked' debuffs
            //  8.1s: fleeting impulse cast ends
            //  8.4s: impulse hit 1 (~0.3 from cast end)
            //  9.9s: impulse hit 2 (~1.3 from prev for each next)
            // 11.2s: impulse hit 3
            // 12.5s: impulse hit 4
            // 13.9s: impulse hit 5
            // 15.2s: impulse hit 6
            // 16.6s: impulse hit 7
            // 17.9s: impulse hit 8
            // 18.8s: 'thornpricked' disappear and some sort of instant cast happens, that does nothing if there are no fails
            // 21.6s: first tether for wreath 6; tethers switch every ~0.5s
            // 21.7s: wreath 6 cast start
            // 27.7s: wreath 6 cast end
            // 29.8s: first tower starts 27150 cast
            // 30.8s: first tower finishes cast
            // ... towers are staggered by ~1.3s
            // 38.8s: near/farsight cast start
            // 39.1s: last tower finishes cast
            var wreath5 = CommonStates.Cast(ref intro.Next, Boss2, AID.WreathOfThorns5, 4.2f, 5, "Wreath5");
            wreath5.Exit.Add(() => ActivateComponent(new WreathOfThorns5(this)));
            wreath5.EndHint |= StateMachine.StateHint.GroupWithNext;

            var fleeting = CommonStates.Cast(ref wreath5.Next, Boss2, AID.FleetingImpulse, 3.2f, 4.9f, "Impulse");
            fleeting.EndHint |= StateMachine.StateHint.GroupWithNext;

            var wreath6 = CommonStates.Cast(ref fleeting.Next, Boss2, AID.WreathOfThorns6, 13.6f, 6, "Wreath6");
            wreath6.EndHint |= StateMachine.StateHint.GroupWithNext;

            var tb = FarNearSight(ref wreath6.Next, 11.2f);
            tb.Exit.Add(DeactivateComponent<WreathOfThorns5>);
            return tb;
        }

        private StateMachine.State AkanthaiAct6(ref StateMachine.State? link, float delay)
        {
            // timeline:
            //  0.0s: curtain call cast ends
            //  0.8s: icons and tethers appear
            //  5.8s: 'thornpricked' debuffs with 12/22/32/42 duration
            // 10.2s: hell sting 1 sequence start
            // 16.3s: hell sting 1 sequence end
            // 30.5s: hell sting 2 sequence start
            // 36.6s: hell sting 2 sequence end
            // 45.7s: aoe start
            // 52.7s: aoe end
            // 55.7s: 'thornpricked' debuffs
            // 59.9s: hell sting 3 sequence start
            // 66.0s: hell sting 3 sequence end
            // 80.2s: hell sting 4 sequence start
            // 86.3s: hell sting 4 sequence end
            // 95.4s: aoe start
            var intro = CommonStates.Cast(ref link, Boss2, AID.AkanthaiCurtainCall, delay, 5, "Act6");
            intro.Exit.Add(() => ActivateComponent(new CurtainCall(this)));
            intro.EndHint |= StateMachine.StateHint.GroupWithNext;

            var sting1 = HellSting(ref intro.Next, 10.2f);
            sting1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var sting2 = HellSting(ref sting1.Next, 14.2f);
            sting2.EndHint |= StateMachine.StateHint.GroupWithNext;

            var impulse1 = UltimateImpulse(ref sting2.Next, 9.1f);
            impulse1.Exit.Add(DeactivateComponent<CurtainCall>);
            impulse1.Exit.Add(() => ActivateComponent(new CurtainCall(this)));
            impulse1.EndHint |= StateMachine.StateHint.GroupWithNext;

            var sting3 = HellSting(ref impulse1.Next, 7.2f);
            sting3.EndHint |= StateMachine.StateHint.GroupWithNext;

            var sting4 = HellSting(ref sting3.Next, 14.2f);
            sting4.EndHint |= StateMachine.StateHint.GroupWithNext;

            var impulse2 = UltimateImpulse(ref sting4.Next, 9.1f);
            impulse2.Exit.Add(DeactivateComponent<CurtainCall>);
            return impulse2;
        }
    }
}
