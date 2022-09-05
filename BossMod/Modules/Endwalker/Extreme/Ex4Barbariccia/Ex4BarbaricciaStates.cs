using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia
{
    class Ex4BarbaricciaStates : StateMachineBuilder
    {
        public Ex4BarbaricciaStates(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase);
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

            BrutalRushes(id + 0x100000, 6.4f);
            WindingGaleBoulderBreak(id + 0x110000, 1.5f);
            BrutalRushes(id + 0x120000, 5.2f);

            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void VoidAeroRaidwide(uint id, float delay)
        {
            Cast(id, AID.VoidAeroRaidwide, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void VoidAeroTankbuster(uint id, float delay)
        {
            Cast(id, AID.VoidAeroTankbuster, delay, 5, "Tankbuster")
                .ActivateOnEnter<VoidAeroTankbuster>()
                .DeactivateOnExit<VoidAeroTankbuster>()
                .SetHint(StateMachine.StateHint.Tankbuster);
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

        private void SavageBarberyHairRaid(uint id, float delay)
        {
            CastMulti(id, new AID[] { AID.SavageBarberyDonut, AID.SavageBarberyRect }, delay, 6)
                .ActivateOnEnter<SavageBarbery>();
            ComponentCondition<SavageBarbery>(id + 0x10, 1, comp => comp.NumActiveCasts < 2, "Donut/rect");
            ComponentCondition<SavageBarbery>(id + 0x20, 2.1f, comp => comp.NumActiveCasts == 0, "Sword")
                .DeactivateOnExit<SavageBarbery>();

            CastMulti(id + 0x1000, new AID[] { AID.HairRaidCone, AID.HairRaidDonut }, 4.2f, 6)
                .ActivateOnEnter<HairRaid>()
                .ActivateOnEnter<HairSpray>()
                .ActivateOnEnter<DeadlyTwist>();
            ComponentCondition<HairRaid>(id + 0x1010, 2, comp => comp.NumActiveCasts == 0, "Donut/cone")
                .DeactivateOnExit<HairRaid>();
            Condition(id + 0x1020, 2.3f, () => Module.FindComponent<HairSpray>()!.NumCasts + Module.FindComponent<DeadlyTwist>()!.NumCasts > 0, "Stack/spread")
                .DeactivateOnExit<HairSpray>()
                .DeactivateOnExit<DeadlyTwist>();
        }

        private void TeasingTangles1(uint id, float delay)
        {
            Cast(id, AID.TeasingTangles, delay, 4)
                .ActivateOnEnter<Tangle>();
            ComponentCondition<Tangle>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Tangles 1 start");
            ComponentCondition<Tangle>(id + 0x10, 0.7f, comp => comp.NumTethers > 0);
            ComponentCondition<HairFlayUpbraid>(id + 0x20, 2.7f, comp => comp.NumActiveMechanics > 0)
                .ActivateOnEnter<HairFlayUpbraid>();
            Cast(id + 0x30, AID.SecretBreeze, 4.5f, 3)
                .ActivateOnEnter<SecretBreezeCones>();
            ComponentCondition<HairFlayUpbraid>(id + 0x40, 0.5f, comp => comp.NumActiveMechanics == 0, "Stack/spread")
                .DeactivateOnExit<HairFlayUpbraid>();
            ComponentCondition<SecretBreezeCones>(id + 0x50, 0.5f, comp => comp.NumCasts > 0, "Cones")
                .DeactivateOnExit<SecretBreezeCones>();
            ComponentCondition<SecretBreezeProteans>(id + 0x60, 2, comp => comp.NumCasts > 0, "Proteans")
                .ActivateOnEnter<SecretBreezeProteans>()
                .DeactivateOnExit<SecretBreezeProteans>();
            ComponentCondition<Tangle>(id + 0x70, 3.2f, comp => comp.NumTethers == 0, "Tangles 1 end")
                .DeactivateOnExit<Tangle>();
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

        private void BrutalRushes(uint id, float delay)
        {
            ComponentCondition<BrutalGust>(id, delay, comp => comp.NumCasts >= 1, "Charge 1")
                .ActivateOnEnter<BrutalGust>();
            ComponentCondition<BrutalGust>(id + 1, 1.7f, comp => comp.NumCasts >= 2, "Charge 2");
            ComponentCondition<BrutalGust>(id + 2, 1.7f, comp => comp.NumCasts >= 3, "Charge 3");
            ComponentCondition<BrutalGust>(id + 3, 1.7f, comp => comp.NumCasts >= 4, "Charge 4")
                .DeactivateOnExit<BrutalGust>();
        }

        private void WindingGaleBoulderBreak(uint id, float delay)
        {
            ComponentCondition<WarningGale>(id, delay, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<WarningGale>();
            ComponentCondition<BoulderBreak>(id + 1, 0.8f, comp => comp.CastActive)
                .ActivateOnEnter<BoulderBreak>();
            ComponentCondition<WarningGale>(id + 2, 5, comp => comp.NumCasts > 0, "Spirals")
                .ActivateOnEnter<WindingGale>()
                .DeactivateOnExit<WindingGale>()
                .DeactivateOnExit<WarningGale>();
            ComponentCondition<BoulderBreak>(id + 3, 0.8f, comp => comp.NumCasts > 0, "Shared tankbuster")
                .DeactivateOnExit<BoulderBreak>();

            ComponentCondition<WindingGaleCharge>(id + 0x10, 2.8f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<WindingGaleCharge>();
            ComponentCondition<WindingGaleCharge>(id + 0x11, 2, comp => comp.Casters.Count > 6);
            ComponentCondition<WindingGaleCharge>(id + 0x12, 0.5f, comp => comp.Casters.Count <= 6, "Charge 1");
            ComponentCondition<WindingGaleCharge>(id + 0x13, 2, comp => comp.Casters.Count == 0, "Charge 2")
                .DeactivateOnExit<WindingGaleCharge>();

            ComponentCondition<WarningGale>(id + 0x20, 1.5f, comp => comp.ActiveCasters.Any())
                .ActivateOnEnter<WarningGale>();
            ComponentCondition<Boulder>(id + 0x21, 1.1f, comp => comp.Casters.Count > 0, "Bait")
                .ActivateOnEnter<WindingGale>()
                .ActivateOnEnter<Boulder>();
            ComponentCondition<BrittleBoulder>(id + 0x22, 3, comp => comp.NumCasts > 0, "Spread")
                .ActivateOnEnter<BrittleBoulder>()
                .DeactivateOnExit<BrittleBoulder>();

            ComponentCondition<HairFlayUpbraid>(id + 0x30, 0.2f, comp => comp.NumActiveMechanics > 0)
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

            ComponentCondition<HairFlayUpbraid>(id + 0x50, 1.1f, comp => comp.NumActiveMechanics == 0, "Stack in pairs")
                .DeactivateOnExit<HairFlayUpbraid>();
        }
    }
}
