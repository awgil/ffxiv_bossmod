using System.Linq;

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    class Ex4BarbaricciaStates : StateMachineBuilder
    {
        public Ex4BarbaricciaStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase)
                .ActivateOnEnter<StiffBreeze>(); // note: while it is really only active during two mechanics, it lingers for quite some time, so it's simpler to keep it always active
        }

        private void SinglePhase(uint id)
        {
            VoidAeroRaidwide(id, 9.2f);
            RagingStorm(id + 0x10000, 7.2f);
            SavageBarberyHairRaid(id + 0x20000, 3.3f);
            RagingStorm(id + 0x30000, 1.9f);
            SavageBarberyHairRaid(id + 0x40000, 3.3f);
            VoidAeroRaidwide(id + 0x50000, 2.9f);
            VoidAeroTankbuster(id + 0x60000, 2.1f);
            RagingStorm(id + 0x70000, 7.2f);
            TeasingTangles1(id + 0x80000, 3.1f);
            VoidAeroRaidwide(id + 0x90000, 0.9f);
            RagingStorm(id + 0xA0000, 7.2f);
            CurlingIron(id + 0xB0000, 3.4f);
            Catabasis(id + 0xC0000, 6.2f);

            BrutalRush(id + 0x100000, 3f);
            WindingGaleBoulderBreak(id + 0x110000, 1.6f);
            BrutalRush(id + 0x120000, 1.7f);
            KnuckleDrum(id + 0x130000, 3);
            BlowAwayImpactBoldBoulderTrample(id + 0x140000, 2.1f);
            TeasingTangles2(id + 0x150000, 4.7f);
            KnuckleDrum(id + 0x160000, 3.3f);

            IronOut(id + 0x200000, 11.7f);
            RagingStorm(id + 0x210000, 6.1f);
            EntanglementSecretBreeze(id + 0x220000, 3.1f);
            SavageBarberyHairRaid(id + 0x230000, 3.4f);
            VoidAeroRaidwide(id + 0x240000, 3.9f);
            VoidAeroTankbuster(id + 0x250000, 2.2f);
            RagingStorm(id + 0x260000, 7.2f);
            EntanglementUpbraid(id + 0x270000, 3.1f);
            SavageBarberyHairRaid(id + 0x280000, 1.6f, true);
            VoidAeroRaidwide(id + 0x290000, 4.3f);
            RagingStorm(id + 0x2A0000, 7.2f);
            CurlingIron(id + 0x2B0000, 3.2f);

            BrutalRush(id + 0x300000, 4);
            KnuckleDrum(id + 0x310000, 3);
            BlowAwayBoulders(id + 0x320000, 2.1f);
            TornadoChainImpactHairSpray(id + 0x330000, 1.5f);
            BrutalRushDryBlowsBoulderBreakWindingGale(id + 0x340000, 0.6f);
            KnuckleDrum(id + 0x350000, 6.2f);

            IronOut(id + 0x400000, 10.5f);
            RagingStorm(id + 0x410000, 6.1f);
            EntanglementSecretBreeze(id + 0x420000, 3.1f);
            SavageBarberyHairRaid(id + 0x430000, 3.3f);
            VoidAeroRaidwide(id + 0x440000, 3.9f);
            RagingStorm(id + 0x450000, 2.1f);
            Cast(id + 0x460000, AID.Maelstrom, 3.4f, 9, "Enrage");
        }

        private void VoidAeroTankbuster(uint id, float delay)
        {
            Cast(id, AID.VoidAeroTankbuster, delay, 5, "Tankbuster")
                .ActivateOnEnter<VoidAeroTankbuster>()
                .DeactivateOnExit<VoidAeroTankbuster>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void VoidAeroRaidwide(uint id, float delay)
        {
            Cast(id, AID.VoidAeroRaidwide, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void RagingStorm(uint id, float delay)
        {
            ComponentCondition<RagingStorm>(id, delay, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<RagingStorm>()
                .DeactivateOnExit<RagingStorm>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void CurlingIron(uint id, float delay)
        {
            Cast(id, AID.CurlingIron, delay, 5);
            ComponentCondition<CurlingIron>(id + 0x10, 8.2f, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<CurlingIron>()
                .DeactivateOnExit<CurlingIron>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void IronOut(uint id, float delay)
        {
            ComponentCondition<IronOut>(id, delay, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<IronOut>()
                .DeactivateOnExit<IronOut>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Catabasis(uint id, float delay)
        {
            Targetable(id, false, delay, "Disappear");
            ComponentCondition<Catabasis>(id + 1, 11, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<Catabasis>()
                .DeactivateOnExit<Catabasis>()
                .SetHint(StateMachine.StateHint.Raidwide);
            Targetable(id + 2, true, 0.1f, "Reappear");
        }

        private void KnuckleDrum(uint id, float delay)
        {
            ComponentCondition<KnuckleDrum>(id, delay, comp => comp.NumCasts > 0, "Raidwide first hit")
                .ActivateOnEnter<KnuckleDrum>()
                .DeactivateOnExit<KnuckleDrum>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<KnuckleDrumLast>(id + 0x100, 7.7f, comp => comp.NumCasts > 0, "Raidwide last hit")
                .ActivateOnEnter<KnuckleDrumLast>()
                .DeactivateOnExit<KnuckleDrumLast>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void BrutalRush(uint id, float delay)
        {
            ComponentCondition<BrutalRush>(id, delay, comp => comp.HavePendingRushes)
                .ActivateOnEnter<BrutalRush>();
            ComponentCondition<BrutalRush>(id + 1, 3.4f, comp => comp.NumCasts >= 1, "Charge 1");
            ComponentCondition<BrutalRush>(id + 2, 1.7f, comp => comp.NumCasts >= 2, "Charge 2");
            ComponentCondition<BrutalRush>(id + 3, 1.7f, comp => comp.NumCasts >= 3, "Charge 3");
            ComponentCondition<BrutalRush>(id + 4, 1.7f, comp => comp.NumCasts >= 4, "Charge 4")
                .DeactivateOnExit<BrutalRush>();
        }

        private void SavageBarberyHairRaid(uint id, float delay, bool fast = false)
        {
            CastMulti(id, new AID[] { AID.SavageBarberyDonut1, AID.SavageBarberyDonut2, AID.SavageBarberyDonut3, AID.SavageBarberyDonut4, AID.SavageBarberyRect1, AID.SavageBarberyRect2 }, delay, 6)
                .ActivateOnEnter<SavageBarbery>();
            ComponentCondition<SavageBarbery>(id + 0x10, 1, comp => comp.NumActiveCasts < 2, "Donut/rect");
            ComponentCondition<SavageBarbery>(id + 0x20, 2.1f, comp => comp.NumActiveCasts == 0, "Sword")
                .DeactivateOnExit<SavageBarbery>();

            CastMulti(id + 0x1000, new AID[] { AID.HairRaidCone, AID.HairRaidDonut }, fast ? 1 : 4.2f, 6)
                .ActivateOnEnter<HairRaid>()
                .ActivateOnEnter<HairSprayDeadlyTwist>();
            ComponentCondition<HairRaid>(id + 0x1010, 2, comp => comp.NumActiveCasts == 0, "Donut/cone")
                .DeactivateOnExit<HairRaid>();
            ComponentCondition<HairSprayDeadlyTwist>(id + 0x1020, fast ? 1.9f : 2.3f, comp => !comp.Active, "Stack/spread")
                .DeactivateOnExit<HairSprayDeadlyTwist>();
        }

        private void WindingGaleBoulderBreak(uint id, float delay)
        {
            ComponentCondition<WarningGale>(id, delay, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<WarningGale>();
            ComponentCondition<BoulderBreak>(id + 1, 0.8f, comp => comp.Active)
                .ActivateOnEnter<BoulderBreak>();
            ComponentCondition<WarningGale>(id + 2, 4.2f, comp => comp.NumCasts > 0, "Spirals")
                .ActivateOnEnter<WindingGale>()
                .DeactivateOnExit<WindingGale>()
                .DeactivateOnExit<WarningGale>();
            ComponentCondition<BoulderBreak>(id + 3, 0.8f, comp => comp.NumCasts > 0, "Shared tankbuster")
                .DeactivateOnExit<BoulderBreak>();

            ComponentCondition<WindingGaleCharge>(id + 0x10, 2.8f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<WindingGaleCharge>();
            ComponentCondition<WindingGaleCharge>(id + 0x11, 2, comp => comp.Casters.Count > 6);
            ComponentCondition<WindingGaleCharge>(id + 0x12, 0.5f, comp => comp.Casters.Count <= 6);
            ComponentCondition<WindingGaleCharge>(id + 0x13, 2, comp => comp.Casters.Count == 0)
                .DeactivateOnExit<WindingGaleCharge>();

            ComponentCondition<WarningGale>(id + 0x20, 1.5f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<WarningGale>();
            ComponentCondition<Boulder>(id + 0x21, 1.1f, comp => comp.Casters.Count > 0, "Bait")
                .ActivateOnEnter<WindingGale>()
                .ActivateOnEnter<Boulder>();
            ComponentCondition<BrittleBoulder>(id + 0x22, 3, comp => comp.NumFinishedSpreads > 0, "Spread")
                .ActivateOnEnter<BrittleBoulder>()
                .DeactivateOnExit<BrittleBoulder>();

            ComponentCondition<HairFlayUpbraid>(id + 0x30, 0.2f, comp => comp.Active)
                .ActivateOnEnter<HairFlayUpbraid>();
            ComponentCondition<TornadoChainInner>(id + 0x31, 0.4f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<TornadoChainInner>();
            ComponentCondition<WarningGale>(id + 0x32, 0.2f, comp => comp.NumCasts > 0, "Spirals")
                .DeactivateOnExit<WarningGale>()
                .DeactivateOnExit<WindingGale>();
            ComponentCondition<Boulder>(id + 0x33, 0.2f, comp => comp.NumCasts > 0)
                .DeactivateOnExit<Boulder>();

            ComponentCondition<TornadoChainInner>(id + 0x40, 3.6f, comp => comp.NumCasts > 0, "Out")
                .DeactivateOnExit<TornadoChainInner>();
            ComponentCondition<TornadoChainOuter>(id + 0x41, 2.5f, comp => comp.NumCasts > 0, "In")
                .ActivateOnEnter<TornadoChainOuter>()
                .DeactivateOnExit<TornadoChainOuter>();

            ComponentCondition<HairFlayUpbraid>(id + 0x50, 1.1f, comp => !comp.Active, "Stack in pairs")
                .DeactivateOnExit<HairFlayUpbraid>();
        }

        private void BlowAwayImpactBoldBoulderTrample(uint id, float delay)
        {
            ComponentCondition<BlowAwayRaidwide>(id, delay, comp => comp.NumCasts > 0)
                .ActivateOnEnter<BlowAwayRaidwide>()
                .DeactivateOnExit<BlowAwayRaidwide>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<BlowAwayPuddle>(id + 1, 1.7f, comp => comp.ActiveCasters.Any(), "Bait 1")
                .ActivateOnEnter<BlowAwayPuddle>();
            ComponentCondition<BrutalRush>(id + 2, 0.7f, comp => comp.HavePendingRushes)
                .ActivateOnEnter<BrutalRush>();
            // +1.3s: puddles 2 bait
            // +1.6s: rush 1 start
            // +3.2s: rush 2 start
            // +3.3s: puddles 1 finish + 3 bait
            // +3.6s: rush 1 finish
            // +4.8s: rush 3 start
            // +5.2s: rush 2 finish
            // +5.3s: puddles 2 finish + 4 bait
            // +6.4s: rush 4 start
            // +6.8s: rush 3 finish
            // +7.3s: puddles 3 finish
            // +8.4s: rush 4 finish
            // +9.3s: puddles 4 finish

            ComponentCondition<ImpactAOE>(id + 0x100, 7.7f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<ImpactAOE>();
            ComponentCondition<BrutalRush>(id + 0x110, 0.7f, comp => comp.NumCasts >= 4, "Charges")
                .ActivateOnEnter<ImpactKnockback>()
                .DeactivateOnExit<BrutalRush>();
            ComponentCondition<BoldBoulderTrample>(id + 0x120, 0.3f, comp => comp.Stacks.Count > 0)
                .ActivateOnEnter<BoldBoulderTrample>();
            ComponentCondition<BlowAwayPuddle>(id + 0x130, 0.6f, comp => !comp.ActiveCasters.Any())
                .DeactivateOnExit<BlowAwayPuddle>();
            ComponentCondition<ImpactAOE>(id + 0x200, 4.7f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<ImpactAOE>()
                .DeactivateOnExit<ImpactKnockback>();
            ComponentCondition<BoldBoulderTrample>(id + 0x201, 1.8f, comp => comp.Spreads.Count == 0, "Flare");
            ComponentCondition<BoldBoulderTrample>(id + 0x202, 0.3f, comp => comp.Stacks.Count == 0, "Stack")
                .DeactivateOnExit<BoldBoulderTrample>();
        }

        private void BlowAwayBoulders(uint id, float delay)
        {
            ComponentCondition<BlowAwayRaidwide>(id, delay, comp => comp.NumCasts > 0)
                .ActivateOnEnter<BlowAwayRaidwide>()
                .DeactivateOnExit<BlowAwayRaidwide>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<BlowAwayPuddle>(id + 1, 1.7f, comp => comp.ActiveCasters.Any(), "Bait 1")
                .ActivateOnEnter<BlowAwayPuddle>();
            // +2.0s: puddles 2 bait
            // +4.0s: puddles 1 finish + 3 bait
            // +4.3s: icons (for puddles that are to be baited into center)
            // +6.0s: puddles 2 finish + 4 bait
            // +8.0s: puddles 3 finish
            // +10.0s: puddles 4 finish

            ComponentCondition<BrutalRush>(id + 0x100, 8.4f, comp => comp.HavePendingRushes)
                .ActivateOnEnter<BrutalRush>();
            ComponentCondition<Boulder>(id + 0x110, 1.1f, comp => comp.Casters.Count > 0, "Bait center")
                .ActivateOnEnter<Boulder>();
            ComponentCondition<BrutalRush>(id + 0x120, 2.5f, comp => comp.NumCasts >= 1, "Charge 1")
                .ActivateOnEnter<BrittleBoulder>()
                .DeactivateOnExit<BlowAwayPuddle>();
            ComponentCondition<BrittleBoulder>(id + 0x130, 0.5f, comp => !comp.Active, "Spread")
                .DeactivateOnExit<BrittleBoulder>();
            ComponentCondition<Boulder>(id + 0x140, 1, comp => comp.NumCasts > 0)
                .DeactivateOnExit<Boulder>();
            ComponentCondition<BrutalRush>(id + 0x150, 3.6f, comp => comp.NumCasts >= 4, "Charge 4")
                .DeactivateOnExit<BrutalRush>();
        }

        private void TeasingTangles1(uint id, float delay)
        {
            Cast(id, AID.TeasingTangles1, delay, 4)
                .ActivateOnEnter<Tangle>();
            ComponentCondition<Tangle>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Tangles 1 start");
            ComponentCondition<Tangle>(id + 0x10, 0.6f, comp => comp.NumTethers > 0);
            ComponentCondition<HairFlayUpbraid>(id + 0x20, 2.8f, comp => comp.Active)
                .ActivateOnEnter<HairFlayUpbraid>();
            Cast(id + 0x30, AID.SecretBreeze, 4.5f, 3)
                .ActivateOnEnter<SecretBreezeCones>();
            ComponentCondition<HairFlayUpbraid>(id + 0x40, 0.5f, comp => !comp.Active, "Stack/spread")
                .DeactivateOnExit<HairFlayUpbraid>();
            ComponentCondition<SecretBreezeCones>(id + 0x50, 0.5f, comp => comp.NumCasts > 0, "Cones")
                .DeactivateOnExit<SecretBreezeCones>();
            ComponentCondition<SecretBreezeProteans>(id + 0x60, 2, comp => comp.NumCasts > 0, "Proteans")
                .ActivateOnEnter<SecretBreezeProteans>()
                .DeactivateOnExit<SecretBreezeProteans>();
            ComponentCondition<Tangle>(id + 0x70, 3.2f, comp => comp.NumTethers == 0, "Tangles 1 end")
                .DeactivateOnExit<Tangle>();
        }

        private void TeasingTangles2(uint id, float delay)
        {
            ComponentCondition<BrutalRush>(id, delay, comp => comp.HavePendingRushes)
                .ActivateOnEnter<BrutalRush>();
            ComponentCondition<BrutalRush>(id + 1, 3.4f, comp => comp.NumCasts >= 1, "Charge 1");
            ComponentCondition<BrutalRush>(id + 2, 1.7f, comp => comp.NumCasts >= 2);
            ComponentCondition<BrutalRush>(id + 3, 1.7f, comp => comp.NumCasts >= 3)
                .ActivateOnEnter<Tangle>(); // activates ~0.1s after second charge
            ComponentCondition<BrutalRush>(id + 4, 1.7f, comp => comp.NumCasts >= 4, "Charge 4")
                .DeactivateOnExit<BrutalRush>();

            ComponentCondition<BlusteryRuler>(id + 0x10, 0.4f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<BlusteryRuler>();
            ComponentCondition<Tangle>(id + 0x20, 0.8f, comp => comp.NumCasts > 0, "Tangles 2 start");
            ComponentCondition<Tangle>(id + 0x21, 0.5f, comp => comp.NumTethers > 0);
            ComponentCondition<BlusteryRuler>(id + 0x30, 3.7f, comp => !comp.ActiveCasters.Any())
                .DeactivateOnExit<BlusteryRuler>();

            ComponentCondition<DryBlowsRaidwide>(id + 0x40, 2.8f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<DryBlowsRaidwide>()
                .DeactivateOnExit<DryBlowsRaidwide>()
                .SetHint(StateMachine.StateHint.Raidwide);

            ComponentCondition<TornadoChainInner>(id + 0x50, 7.6f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<TornadoChainInner>()
                .ActivateOnEnter<DryBlowsPuddle>();
            ComponentCondition<HairFlayUpbraid>(id + 0x51, 0.3f, comp => comp.Active)
                .ActivateOnEnter<HairFlayUpbraid>();
            ComponentCondition<TornadoChainInner>(id + 0x52, 3.7f, comp => comp.NumCasts > 0, "Out")
                .DeactivateOnExit<TornadoChainInner>();
            ComponentCondition<TornadoChainOuter>(id + 0x53, 2.5f, comp => comp.NumCasts > 0, "In")
                .ActivateOnEnter<TornadoChainOuter>()
                .DeactivateOnExit<TornadoChainOuter>();
            ComponentCondition<HairFlayUpbraid>(id + 0x54, 1.8f, comp => !comp.Active, "Stack in pairs")
                .DeactivateOnExit<HairFlayUpbraid>()
                .DeactivateOnExit<Tangle>()
                .DeactivateOnExit<DryBlowsPuddle>();
        }

        private void EntanglementSecretBreeze(uint id, float delay)
        {
            // TODO: component?..
            Cast(id, AID.Entanglement, delay, 4, "Playstation");
            // +1.2s: tethers appear

            Cast(id + 0x10, AID.SecretBreeze, 6.5f, 3)
                .ActivateOnEnter<SecretBreezeCones>();
            ComponentCondition<SecretBreezeCones>(id + 0x12, 1, comp => comp.NumCasts > 0, "Cones")
                .DeactivateOnExit<SecretBreezeCones>();
            ComponentCondition<SecretBreezeProteans>(id + 0x13, 2, comp => comp.NumCasts > 0, "Proteans")
                .ActivateOnEnter<SecretBreezeProteans>()
                .DeactivateOnExit<SecretBreezeProteans>();
        }

        private void EntanglementUpbraid(uint id, float delay)
        {
            // TODO: component?..
            Cast(id, AID.Entanglement, delay, 4, "Playstation");
            // +1.2s: tethers appear

            ComponentCondition<HairFlayUpbraid>(id + 0x10, 6, comp => comp.Active)
                .ActivateOnEnter<HairFlayUpbraid>();
            ComponentCondition<HairFlayUpbraid>(id + 0x11, 8, comp => !comp.Active, "Stack in pairs")
                .DeactivateOnExit<HairFlayUpbraid>();
        }

        private void TornadoChainImpactHairSpray(uint id, float delay)
        {
            ComponentCondition<TornadoChainInner>(id, delay, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<TornadoChainInner>();
            ComponentCondition<HairSprayDeadlyTwist>(id + 1, 3.9f, comp => comp.Active)
                .ActivateOnEnter<HairSprayDeadlyTwist>();
            ComponentCondition<TornadoChainInner>(id + 2, 0.1f, comp => comp.NumCasts > 0, "Out")
                .DeactivateOnExit<TornadoChainInner>();
            ComponentCondition<TornadoChainOuter>(id + 0x10, 2.5f, comp => comp.NumCasts > 0, "In")
                .ActivateOnEnter<TornadoChainOuter>()
                .ActivateOnEnter<ImpactAOE>() // starts ~0.2s after out finishes
                .ActivateOnEnter<ImpactKnockback>()
                .DeactivateOnExit<TornadoChainOuter>();
            ComponentCondition<ImpactAOE>(id + 0x20, 3.9f, comp => comp.NumCasts > 0, "Knockback")
                .DeactivateOnExit<ImpactAOE>()
                .DeactivateOnExit<ImpactKnockback>();
            ComponentCondition<HairSprayDeadlyTwist>(id + 0x21, 1.5f, comp => !comp.Active, "Spread")
                .DeactivateOnExit<HairSprayDeadlyTwist>();
        }

        private void BrutalRushDryBlowsBoulderBreakWindingGale(uint id, float delay)
        {
            ComponentCondition<BrutalRush>(id, delay, comp => comp.HavePendingRushes)
                .ActivateOnEnter<BrutalRush>();
            ComponentCondition<BrutalRush>(id + 1, 3.4f, comp => comp.NumCasts >= 1, "Charge 1");
            ComponentCondition<BrutalRush>(id + 2, 1.7f, comp => comp.NumCasts >= 2)
                .ActivateOnEnter<BlusteryRuler>(); // activates ~1.2s after first charge
            ComponentCondition<BrutalRush>(id + 3, 1.7f, comp => comp.NumCasts >= 3);
            ComponentCondition<BrutalRush>(id + 4, 1.7f, comp => comp.NumCasts >= 4, "Charge 4")
                .DeactivateOnExit<BrutalRush>();
            ComponentCondition<BlusteryRuler>(id + 5, 1, comp => comp.NumCasts > 0)
                .DeactivateOnExit<BlusteryRuler>();

            ComponentCondition<DryBlowsRaidwide>(id + 0x10, 3.1f, comp => comp.NumCasts > 0)
                .ActivateOnEnter<DryBlowsRaidwide>()
                .DeactivateOnExit<DryBlowsRaidwide>()
                .SetHint(StateMachine.StateHint.Raidwide);

            ComponentCondition<TornadoChainInner>(id + 0x20, 5.6f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<TornadoChainInner>()
                .ActivateOnEnter<DryBlowsPuddle>();
            ComponentCondition<TornadoChainInner>(id + 0x21, 4, comp => comp.NumCasts > 0, "Out")
                .DeactivateOnExit<TornadoChainInner>();
            ComponentCondition<TornadoChainOuter>(id + 0x22, 2.5f, comp => comp.NumCasts > 0, "In")
                .ActivateOnEnter<BoulderBreak>() // <0.1s after out
                .ActivateOnEnter<TornadoChainOuter>()
                .DeactivateOnExit<TornadoChainOuter>();
            ComponentCondition<BoulderBreak>(id + 0x23, 2.5f, comp => comp.NumCasts > 0, "Shared tankbuster")
                .DeactivateOnExit<BoulderBreak>()
                .DeactivateOnExit<DryBlowsPuddle>();

            ComponentCondition<WarningGale>(id + 0x30, 0.3f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<WarningGale>();
            ComponentCondition<WarningGale>(id + 0x31, 5, comp => comp.NumCasts > 0, "Spirals")
                .ActivateOnEnter<WindingGale>()
                .DeactivateOnExit<WindingGale>()
                .DeactivateOnExit<WarningGale>();

            ComponentCondition<WindingGaleCharge>(id + 0x40, 3.7f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<WindingGaleCharge>(); // tornado chain starts at the same time
            ComponentCondition<WindingGaleCharge>(id + 0x41, 2, comp => comp.Casters.Count > 6)
                .ActivateOnEnter<TornadoChainInner>();
            ComponentCondition<WindingGaleCharge>(id + 0x42, 0.5f, comp => comp.Casters.Count <= 6);
            ComponentCondition<TornadoChainInner>(id + 0x43, 1.5f, comp => comp.NumCasts > 0, "Out")
                .DeactivateOnExit<TornadoChainInner>();
            ComponentCondition<WindingGaleCharge>(id + 0x44, 0.5f, comp => comp.Casters.Count == 0)
                .ActivateOnEnter<TornadoChainOuter>()
                .DeactivateOnExit<WindingGaleCharge>();

            ComponentCondition<WarningGale>(id + 0x50, 1.8f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<WarningGale>();
            ComponentCondition<TornadoChainOuter>(id + 0x51, 0.2f, comp => comp.NumCasts > 0, "In")
                .ActivateOnEnter<WindingGale>()
                .DeactivateOnExit<TornadoChainOuter>();
            ComponentCondition<BoldBoulderTrample>(id + 0x52, 1.6f, comp => comp.Stacks.Count > 0)
                .ActivateOnEnter<BoldBoulderTrample>();
            ComponentCondition<WarningGale>(id + 0x53, 3.2f, comp => comp.NumCasts > 0, "Spirals")
                .DeactivateOnExit<WarningGale>()
                .DeactivateOnExit<WindingGale>();
            ComponentCondition<BoldBoulderTrample>(id + 0x54, 2.7f, comp => comp.Stacks.Count == 0, "Stack");
            ComponentCondition<BoldBoulderTrample>(id + 0x55, 1, comp => comp.Spreads.Count == 0, "Flare")
                .DeactivateOnExit<BoldBoulderTrample>();
        }
    }
}
