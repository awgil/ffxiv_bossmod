using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.P1S
{
    public class P1S : BossModule
    {
        public static float InnerCircleRadius { get; } = 12; // this determines in/out flails and cells boundary

        private List<Actor> _boss;
        public Actor? Boss() => _boss.FirstOrDefault();

        public P1S(WorldState ws)
            : base(ws)
        {
            _boss = Enemies(OID.Boss);

            StateMachine.State? s;
            s = HeavyHand(ref InitialState, 8);
            s = AetherialShackles(ref s.Next, 6, false);
            s = GaolerFlail(ref s.Next, 4.5f);
            s = Knockback(ref s.Next, 5.7f);
            s = GaolerFlail(ref s.Next, 3.3f);
            s = WarderWrath(ref s.Next, 5.6f);
            s = IntemperancePhase(ref s.Next, 11.2f, true);
            s = Knockback(ref s.Next, 5.3f);

            s = ShiningCells(ref s.Next, 9.1f);
            s = Aetherflail(ref s.Next, 8.1f);
            s = Knockback(ref s.Next, 7.6f);
            s = Aetherflail(ref s.Next, 3);
            s = ShacklesOfTime(ref s.Next, 4.6f, false);
            s = SlamShut(ref s.Next, 1.6f);

            s = FourfoldShackles(ref s.Next, 13);
            s = WarderWrath(ref s.Next, 5.4f);
            s = IntemperancePhase(ref s.Next, 11.2f, false);
            s = WarderWrath(ref s.Next, 3.7f);

            s = ShiningCells(ref s.Next, 11.2f);

            // subsequences
            StateMachine.State? fork1 = null, fork2 = null;
            Fork1(ref fork1);
            Fork2(ref fork2);

            Dictionary<AID, (StateMachine.State?, Action)> forkDispatch = new();
            forkDispatch[AID.AetherialShackles] = new(fork1, () => { });
            forkDispatch[AID.ShacklesOfTime] = new(fork2, () => { });
            var fork = CommonStates.CastStart(ref s.Next, Boss, forkDispatch, 6, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback"); // first branch delay = 7.8
        }

        protected override void ResetModule()
        {
            Arena.IsCircle = false; // reset could happen during cells
        }

        protected override void DrawArenaForegroundPost()
        {
            if (Arena.IsCircle)
            {
                // cells mode
                float diag = Arena.WorldHalfSize / 1.414214f;
                Arena.AddCircle(Arena.WorldCenter, InnerCircleRadius, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldE, Arena.WorldW, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldN, Arena.WorldS, Arena.ColorBorder);
                Arena.AddLine(Arena.WorldCenter + new Vector3(diag, 0, diag), Arena.WorldCenter - new Vector3(diag, 0, diag), Arena.ColorBorder);
                Arena.AddLine(Arena.WorldCenter + new Vector3(diag, 0, -diag), Arena.WorldCenter - new Vector3(diag, 0, -diag), Arena.ColorBorder);
            }

            Arena.Actor(Boss(), Arena.ColorEnemy);
            Arena.Actor(Player(), Arena.ColorPC);
        }

        private StateMachine.State HeavyHand(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.HeavyHand, delay, 5, "HeavyHand");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
            return s;
        }

        private StateMachine.State WarderWrath(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.WarderWrath, delay, 5, "Wrath");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State Aetherchain(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.Aetherchain, delay, 5, "Aetherchain");
            s.Enter.Add(() => ActivateComponent(new AetherExplosion(this)));
            s.Exit.Add(DeactivateComponent<AetherExplosion>);
            return s;
        }

        private StateMachine.State DoubleAetherchain(ref StateMachine.State? link, float delayFirst, float delaySecond)
        {
            var first = Aetherchain(ref link, delayFirst);
            first.EndHint |= StateMachine.StateHint.GroupWithNext;
            return Aetherchain(ref first.Next, delaySecond);
        }

        // if delay is >0, build cast-start + cast-end states, otherwise build only cast-end state (used for first cast after fork)
        private StateMachine.State CastMaybeOmitStart(ref StateMachine.State? link, AID aid, float delay, float castTime, string name)
        {
            if (delay > 0)
                return CommonStates.Cast(ref link, Boss, aid, delay, castTime, name);
            else
                return CommonStates.CastEnd(ref link, Boss, castTime, name);
        }

        // aetherial shackles is paired either with wrath (first time) or two aetherchains (second time)
        private StateMachine.State AetherialShackles(ref StateMachine.State? link, float delay, bool withAetherchains)
        {
            var cast = CastMaybeOmitStart(ref link, AID.AetherialShackles, delay, 3, "Shackles");
            cast.EndHint |= StateMachine.StateHint.PositioningStart | StateMachine.StateHint.GroupWithNext;
            cast.Exit.Add(() => ActivateComponent(new Shackles(this)));

            var overlap = withAetherchains ? DoubleAetherchain(ref cast.Next, 6.1f, 3.2f) : WarderWrath(ref cast.Next, 4.1f);
            overlap.EndHint |= StateMachine.StateHint.GroupWithNext;

            // ~19sec after cast end
            // technically, resolve happens ~0.4sec before second aetherchain cast end, but that's irrelevant
            var resolve = CommonStates.ComponentCondition<Shackles>(ref overlap.Next, withAetherchains ? 0 : 9.8f, this, comp => comp.NumDebuffs() == 0, "Shackles resolve");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit.Add(DeactivateComponent<Shackles>);
            return resolve;
        }

        private StateMachine.State FourfoldShackles(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, Boss, AID.FourShackles, delay, 3, "FourShackles");
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;
            cast.Exit.Add(() => ActivateComponent(new Shackles(this)));

            // note that it takes almost a second for debuffs to be applied
            var hit1 = CommonStates.ComponentCondition<Shackles>(ref cast.Next, 9, this, comp => comp.NumDebuffs() <= 6, "Hit1", 1, 3);
            hit1.EndHint |= StateMachine.StateHint.GroupWithNext;
            var hit2 = CommonStates.ComponentCondition<Shackles>(ref hit1.Next, 5, this, comp => comp.NumDebuffs() <= 4, "Hit2");
            hit2.EndHint |= StateMachine.StateHint.GroupWithNext;
            var hit3 = CommonStates.ComponentCondition<Shackles>(ref hit2.Next, 5, this, comp => comp.NumDebuffs() <= 2, "Hit3");
            hit3.EndHint |= StateMachine.StateHint.GroupWithNext;
            var hit4 = CommonStates.ComponentCondition<Shackles>(ref hit3.Next, 5, this, comp => comp.NumDebuffs() == 0, "Hit4");
            hit4.EndHint |= StateMachine.StateHint.PositioningEnd;
            hit4.Exit.Add(DeactivateComponent<Shackles>);
            return hit4;
        }

        // shackles of time is paired either with heavy hand or knockback mechanics; also cast-start sometimes is omitted if delay is 0, since it is used to determine fork path
        private StateMachine.State ShacklesOfTime(ref StateMachine.State? link, float delay, bool withKnockback)
        {
            var cast = CastMaybeOmitStart(ref link, AID.ShacklesOfTime, delay, 4, "ShacklesOfTime");
            cast.EndHint |= StateMachine.StateHint.PositioningStart | StateMachine.StateHint.GroupWithNext;
            cast.Exit.Add(() => ActivateComponent(new AetherExplosion(this)));

            var overlap = withKnockback ? Knockback(ref cast.Next, 2.2f, false) : HeavyHand(ref cast.Next, 5.2f);
            overlap.EndHint |= StateMachine.StateHint.GroupWithNext;

            // ~15s from cast end
            var resolve = CommonStates.ComponentCondition<AetherExplosion>(ref overlap.Next, withKnockback ? 3.4f : 4.8f, this, comp => !comp.SOTActive, "Shackles resolve");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit.Add(DeactivateComponent<AetherExplosion>);
            return resolve;
        }

        private StateMachine.State GaolerFlailStart(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.GaolerFlailRL] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Right, Flails.Zone.Left)));
            dispatch[AID.GaolerFlailLR] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Left, Flails.Zone.Right)));
            dispatch[AID.GaolerFlailIO1] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Inner, Flails.Zone.Outer)));
            dispatch[AID.GaolerFlailIO2] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Inner, Flails.Zone.Outer)));
            dispatch[AID.GaolerFlailOI1] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Outer, Flails.Zone.Inner)));
            dispatch[AID.GaolerFlailOI2] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Outer, Flails.Zone.Inner)));
            return CommonStates.CastStart(ref link, Boss, dispatch, delay);
        }

        private StateMachine.State GaolerFlailEnd(ref StateMachine.State? link, float castTimeLeft, string name)
        {
            var end = CommonStates.CastEnd(ref link, Boss, castTimeLeft);

            var resolve = CommonStates.ComponentCondition<Flails>(ref end.Next, 3.6f, this, comp => comp.NumCasts == 2, name);
            resolve.Exit.Add(DeactivateComponent<Flails>);
            return resolve;
        }

        private StateMachine.State GaolerFlail(ref StateMachine.State? link, float delay)
        {
            var start = GaolerFlailStart(ref link, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var resolve = GaolerFlailEnd(ref start.Next, 11.5f, "Flails");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State Aetherflail(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.AetherflailRX] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Right, Flails.Zone.UnknownCircle)));
            dispatch[AID.AetherflailLX] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Left, Flails.Zone.UnknownCircle)));
            dispatch[AID.AetherflailIL] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Inner, Flails.Zone.Left)));
            dispatch[AID.AetherflailIR] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Inner, Flails.Zone.Right)));
            dispatch[AID.AetherflailOL] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Outer, Flails.Zone.Left)));
            dispatch[AID.AetherflailOR] = new(null, () => ActivateComponent(new Flails(this, Flails.Zone.Outer, Flails.Zone.Right)));
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;
            start.Exit.Add(() => ActivateComponent(new AetherExplosion(this)));

            var resolve = GaolerFlailEnd(ref start.Next, 11.5f, "Aetherflail");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit.Add(DeactivateComponent<AetherExplosion>);
            return resolve;
        }

        private StateMachine.State Knockback(ref StateMachine.State? link, float delay, bool positioningHints = true)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.KnockbackGrace] = new(null, () => ActivateComponent(new Knockback(this, Boss()?.CastInfo?.TargetID ?? 0, false)));
            dispatch[AID.KnockbackPurge] = new(null, () => ActivateComponent(new Knockback(this, Boss()?.CastInfo?.TargetID ?? 0, true)));
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            if (positioningHints)
                start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Knockback");
            end.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Tankbuster;

            var resolve = CommonStates.ComponentCondition<Knockback>(ref end.Next, 4.4f, this, comp => comp.AOEDone, "Explode");
            if (positioningHints)
                resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            resolve.Exit.Add(DeactivateComponent<Knockback>);
            return resolve;
        }

        // full intemperance phases (overlap either with 2 wraths or with flails)
        private StateMachine.State IntemperancePhase(ref StateMachine.State? link, float delay, bool withWraths)
        {
            var intemp = CommonStates.Cast(ref link, Boss, AID.Intemperance, delay, 2, "Intemperance");
            intemp.Exit.Add(() => ActivateComponent(new Intemperance(this)));
            intemp.EndHint |= StateMachine.StateHint.GroupWithNext;

            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.IntemperateTormentUp] = new(null, () => FindComponent<Intemperance>()!.CurState = Intemperance.State.BottomToTop);
            dispatch[AID.IntemperateTormentDown] = new(null, () => FindComponent<Intemperance>()!.CurState = Intemperance.State.TopToBottom);
            var explosionStart = CommonStates.CastStart(ref intemp.Next, Boss, dispatch, 5.8f);
            var explosionEnd = CommonStates.CastEnd(ref explosionStart.Next, Boss, 10);

            var cube1 = CommonStates.ComponentCondition<Intemperance>(ref explosionEnd.Next, 1.2f, this, comp => comp.NumExplosions > 0, "Cube1", 0.2f);
            cube1.EndHint |= StateMachine.StateHint.GroupWithNext;

            if (withWraths)
            {
                var wrath1 = WarderWrath(ref cube1.Next, 1);
                wrath1.EndHint |= StateMachine.StateHint.GroupWithNext;

                var cube2 = CommonStates.ComponentCondition<Intemperance>(ref wrath1.Next, 5, this, comp => comp.NumExplosions > 1, "Cube2", 0.2f);
                cube2.EndHint |= StateMachine.StateHint.GroupWithNext;

                var wrath2 = WarderWrath(ref cube2.Next, 0.2f);
                wrath2.EndHint |= StateMachine.StateHint.GroupWithNext;

                var cube3 = CommonStates.ComponentCondition<Intemperance>(ref wrath2.Next, 5.8f, this, comp => comp.NumExplosions > 2, "Cube3");
                cube3.Exit.Add(DeactivateComponent<Intemperance>);
                return cube3;
            }
            else
            {
                var flailStart = GaolerFlailStart(ref cube1.Next, 3);
                flailStart.EndHint |= StateMachine.StateHint.PositioningStart;

                var cube2 = CommonStates.ComponentCondition<Intemperance>(ref flailStart.Next, 8, this, comp => comp.NumExplosions > 1, "Cube2");
                cube2.EndHint |= StateMachine.StateHint.GroupWithNext;

                var flailResolve = GaolerFlailEnd(ref cube2.Next, 3.5f, "Flails");
                flailResolve.EndHint |= StateMachine.StateHint.GroupWithNext;

                var cube3 = CommonStates.ComponentCondition<Intemperance>(ref flailResolve.Next, 3.9f, this, comp => comp.NumExplosions > 2, "Cube3");
                cube3.EndHint |= StateMachine.StateHint.PositioningEnd;
                cube3.Exit.Add(DeactivateComponent<Intemperance>);
                return cube3;
            }
        }

        private StateMachine.State ShiningCells(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.ShiningCells, delay, 7, "Cells");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit.Add(() => Arena.IsCircle = true);
            return s;
        }

        private StateMachine.State SlamShut(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.SlamShut, delay, 7, "SlamShut");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit.Add(() => Arena.IsCircle = false);
            return s;
        }

        // there are two possible orderings for last mechanics of the fight
        private StateMachine.State ForkMerge(ref StateMachine.State? link)
        {
            StateMachine.State? s;
            s = Aetherflail(ref link, 9);
            s = Aetherflail(ref s.Next, 2.7f);
            s = Aetherflail(ref s.Next, 2.7f);
            s = WarderWrath(ref s.Next, 13); // not sure about timings below...
            s = CommonStates.Simple(ref s.Next, 2, "?????");
            return s;
        }

        private StateMachine.State Fork1(ref StateMachine.State? link)
        {
            StateMachine.State? s;
            s = AetherialShackles(ref link, 0, true);
            s = WarderWrath(ref s.Next, 5.2f);
            s = ShacklesOfTime(ref s.Next, 7.2f, true);
            s = WarderWrath(ref s.Next, 5.9f);
            return ForkMerge(ref s.Next);
        }

        private StateMachine.State Fork2(ref StateMachine.State? link)
        {
            StateMachine.State? s;
            s = ShacklesOfTime(ref link, 0, true);
            s = WarderWrath(ref s.Next, 3);
            s = AetherialShackles(ref s.Next, 9, true);
            s = WarderWrath(ref s.Next, 7);
            return ForkMerge(ref s.Next);
        }
    }
}
