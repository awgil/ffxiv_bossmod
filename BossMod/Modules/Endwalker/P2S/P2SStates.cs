namespace BossMod.Endwalker.P2S
{
    class P2SStates : StateMachineBuilder
    {
        public P2SStates(BossModule module) : base(module)
        {
            MurkyDepths(0x00000000, 10);
            DoubledImpact(0x00010000, 5.2f);

            // deluge 1
            SewageDeluge(0x00100000, 7.8f);
            Cataract(0x00110000, 14.9f);
            Coherence(0x00120000, 8.1f);
            MurkyDepths(0x00130000, 8.2f);
            OminousBubblingShockwave(0x00140000, 3.7f);
            AvariceCataract(0x00150000, 10.9f);
            // note: deluge 1 ends here...

            Flow1(0x00200000, 8.6f);
            DoubledImpact(0x00210000, 8.2f);
            MurkyDepths(0x00220000, 5.2f);

            // deluge 2
            SewageDeluge(0x00300000, 11.7f);
            Shockwave(0x00310000, 9.6f);
            KampeosHarma(0x00320000, 4.4f);
            DoubledImpact(0x00330000, 9.9f);
            MurkyDepths(0x00340000, 4.2f);
            Flow2(0x00350000, 8.6f);
            Cataract(0x00360000, 1.2f);
            // note: deluge 2 ends here...

            AvariceDissociationCataract(0x00400000, 15.2f);
            DissociationEruptionFloodCoherence(0x00410000, 9.8f);
            DoubledImpact(0x00420000, 7.2f);
            MurkyDepths(0x00430000, 3.2f);

            // deluge 3
            SewageDeluge(0x00500000, 12.8f);
            Flow3(0x00510000, 11.7f);
            DissociationEruption(0x00520000, 8.3f);
            OminousBubblingShockwave(0x00530000, 3.4f);
            DoubledImpact(0x00540000, 5.4f);
            MurkyDepths(0x00550000, 7.2f);
            MurkyDepths(0x00560000, 6.2f);

            Cast(0x00600000, AID.Enrage, 5.3f, 10, "Enrage");
        }

        private void MurkyDepths(uint id, float delay)
        {
            var s = Cast(id, AID.MurkyDepths, delay, 5, "MurkyDepths");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void DoubledImpact(uint id, float delay)
        {
            var s = Cast(id, AID.DoubledImpact, delay, 5, "DoubledImpact");
            s.Enter.Add(Module.ActivateComponent<DoubledImpact>);
            s.Exit.Add(Module.DeactivateComponent<DoubledImpact>);
            s.EndHint |= StateMachine.StateHint.Tankbuster;
        }

        private void SewageDeluge(uint id, float delay)
        {
            var s = Cast(id, AID.SewageDeluge, delay, 5, "Deluge");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private StateMachine.State Cataract(uint id, float delay)
        {
            var start = CastStartMulti(id, new AID[] { AID.SpokenCataract, AID.WingedCataract }, delay);
            start.Exit.Add(Module.ActivateComponent<Cataract>);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CastEnd(id + 1, 8, "Cataract");
            end.Exit.Add(Module.DeactivateComponent<Cataract>);
            end.EndHint |= StateMachine.StateHint.PositioningEnd;
            return end;
        }

        private StateMachine.State Coherence(uint id, float delay)
        {
            var cast = Cast(id, AID.Coherence, delay, 12);
            cast.Enter.Add(Module.ActivateComponent<Coherence>);

            var resolve = ComponentCondition<Coherence>(id + 2, 3.1f, comp => comp.NumCasts > 0, "Coherence");
            resolve.Exit.Add(Module.DeactivateComponent<Coherence>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
            return resolve;
        }

        private void Shockwave(uint id, float delay)
        {
            // TODO: some component (knockback? or just make sure autorot uses arms length?)
            var s = Cast(id, AID.Shockwave, delay, 8, "Shockwave");
            s.EndHint |= StateMachine.StateHint.Knockback;
        }

        // note: this activates component, which has to be deactivated later manually
        private void PredatoryAvarice(uint id, float delay)
        {
            var s = Cast(id, AID.PredatoryAvarice, delay, 4, "Avarice");
            s.Exit.Add(Module.ActivateComponent<PredatoryAvarice>);
        }

        // note: this activates component, which has to be deactivated later manually
        private void Dissociation(uint id, float delay)
        {
            var s = Cast(id, AID.Dissociation, delay, 4, "Dissociation");
            s.Exit.Add(Module.ActivateComponent<Dissociation>);
        }

        private void AvariceCataract(uint id, float delay)
        {
            PredatoryAvarice(id, delay);

            var cataract = Cataract(id + 0x1000, 9.8f);
            cataract.EndHint &= ~StateMachine.StateHint.PositioningEnd;

            var resolve = ComponentCondition<PredatoryAvarice>(id + 0x2000, 6.2f, comp => !comp.Active, "Avarice resolve");
            resolve.Exit.Add(Module.DeactivateComponent<PredatoryAvarice>);
            resolve.EndHint = StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.Raidwide;
        }

        private void AvariceDissociationCataract(uint id, float delay)
        {
            PredatoryAvarice(id, delay);
            Dissociation(id + 0x1000, 2.4f);

            // note: we don't create separate avarice/dissociation resolve states here, since they all resolve at almost the same time
            var cataract = Cataract(id + 0x2000, 10.1f);
            cataract.Exit.Add(Module.DeactivateComponent<PredatoryAvarice>);
            cataract.Exit.Add(Module.DeactivateComponent<Dissociation>);
            cataract.EndHint |= StateMachine.StateHint.Raidwide; // avarice resolve
        }

        private void DissociationEruptionFloodCoherence(uint id, float delay)
        {
            Dissociation(id, delay);

            var eruption = Cast(id + 0x1000, AID.SewageEruption, 8.2f, 5, "Eruption");
            eruption.EndHint |= StateMachine.StateHint.PositioningStart;

            var flood = Cast(id + 0x2000, AID.TaintedFlood, 2.3f, 3, "Flood");
            flood.Exit.Add(Module.DeactivateComponent<Dissociation>);

            var coherence = Coherence(id + 0x3000, 4.7f);
            coherence.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void DissociationEruption(uint id, float delay)
        {
            // TODO: get rid of timeout!
            Dissociation(id, delay);

            var eruption = Cast(id + 0x1000, AID.SewageEruption, 8.3f, 5, "Eruption");
            eruption.EndHint |= StateMachine.StateHint.PositioningStart;

            var resolve = Timeout(id + 0x2000, 5, "Resolve"); // should be last eruption...
            resolve.Exit.Add(Module.DeactivateComponent<Dissociation>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void Flow1(uint id, float delay)
        {
            // TODO: get rid of timeouts!
            var cast = Cast(id, AID.ChannelingFlow, delay, 5, "Flow 1");
            cast.Exit.Add(Module.ActivateComponent<ChannelingFlow>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            var stuns = Timeout(id + 0x1000, 14);
            stuns.EndHint |= StateMachine.StateHint.PositioningEnd | StateMachine.StateHint.DowntimeStart;

            var resolve = Timeout(id + 0x2000, 3, "Flow resolve");
            resolve.Exit.Add(Module.DeactivateComponent<ChannelingFlow>);
            resolve.EndHint |= StateMachine.StateHint.DowntimeEnd | StateMachine.StateHint.Raidwide;
        }

        private void Flow2(uint id, float delay)
        {
            // TODO: get rid of timeouts!
            // flow 2: same statuses as first flow, different durations
            var cast = Cast(id, AID.ChannelingOverflow, delay, 5, "Flow 2");
            cast.Exit.Add(Module.ActivateComponent<ChannelingFlow>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            Cast(id + 0x1000, AID.TaintedFlood, 4.2f, 3, "Flood 1");

            var hit1 = Timeout(id + 0x2000, 9, "Hit 1");
            hit1.EndHint |= StateMachine.StateHint.Raidwide;

            Cast(id + 0x3000, AID.TaintedFlood, 3.4f, 3, "Flood 2");

            var hit2 = Timeout(id + 0x4000, 9, "Hit 2");
            hit2.Exit.Add(Module.DeactivateComponent<ChannelingFlow>);
            hit2.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
        }

        private void Flow3(uint id, float delay)
        {
            // TODO: get rid of timeouts!
            // flow 3: same as flow 2, but with coherence instead of floods
            var cast = Cast(id, AID.ChannelingOverflow, delay, 5, "Flow 3");
            cast.Exit.Add(Module.ActivateComponent<ChannelingFlow>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            Coherence(id + 0x1000, 5.5f); // first hit is around coherence cast end

            var resolve = Timeout(id + 0x3000, 10, "Flow resolve"); // second hit
            resolve.Exit.Add(Module.DeactivateComponent<ChannelingFlow>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void OminousBubblingShockwave(uint id, float delay)
        {
            // note: can determine bubbling targets by watching 233Cs cast OminousBubblingAOE on two targets
            // TODO: some component...
            var bubbling = Cast(id, AID.OminousBubbling, delay, 3, "TwoStacks");
            bubbling.Exit.Add(Module.ActivateComponent<OminousBubbling>);
            bubbling.EndHint |= StateMachine.StateHint.PositioningStart;

            Shockwave(id + 0x1000, 2.8f);

            var resolve = ComponentCondition<OminousBubbling>(id + 0x2000, 3.8f, comp => comp.NumCasts > 0, "AOE resolve");
            resolve.Exit.Add(Module.DeactivateComponent<OminousBubbling>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
        }

        private void KampeosHarma(uint id, float delay)
        {
            var start = CastStart(id, AID.KampeosHarma, delay);
            start.Enter.Add(Module.ActivateComponent<KampeosHarma>); // note: icons appear right before harma cast start...
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            CastEnd(id + 1, 8.4f, "Harma");
            Targetable(id + 2, false, 0); // usually happens together with cast end

            var resolve = Targetable(id + 3, true, 7.5f, "Harma resolve");
            resolve.Exit.Add(Module.DeactivateComponent<KampeosHarma>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }
    }
}
