namespace BossMod.Endwalker.Alliance.A10RhalgrEmissary;

class A10RhalgrEmissaryStates : StateMachineBuilder
{
    public A10RhalgrEmissaryStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        DestructiveStatic(id, 5.4f);
        DestructiveChargeLightningBolt(id + 0x10000, 14.4f);
        BoltsFromTheBlue(id + 0x20000, 2.2f);
        DestructiveChargeDestructiveStatic(id + 0x30000, 12.4f);
        Boltloop(id + 0x40000, 6.2f);
        DestructiveStrike(id + 0x50000, 4.1f);
        BoltsFromTheBlue(id + 0x60000, 6.2f);
        DestructiveChargeLightningBolt(id + 0x70000, 13.5f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void DestructiveStatic(uint id, float delay)
    {
        Cast(id, AID.DestructiveStatic, delay, 8, "Frontal cleave")
            .ActivateOnEnter<DestructiveStatic>()
            .DeactivateOnExit<DestructiveStatic>();
    }

    private void DestructiveChargeLightningBolt(uint id, float delay)
    {
        ComponentCondition<DestructiveCharge>(id, delay, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<DestructiveCharge>();
        CastStart(id + 0x10, AID.LightningBolt, 6.7f);
        ComponentCondition<DestructiveCharge>(id + 0x20, 2.4f, comp => comp.NumCasts > 0, "Diagonal cleaves")
            .DeactivateOnExit<DestructiveCharge>();
        CastEnd(id + 0x30, 0.6f);
        ComponentCondition<LightningBolt>(id + 0x40, 1.0f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<LightningBolt>(); // 3x3, 3sec cast each, 2s between sets
        ComponentCondition<LightningBolt>(id + 0x50, 7.0f, comp => comp.Casters.Count == 0, "Puddles resolve")
            .DeactivateOnExit<LightningBolt>();
    }

    private void DestructiveChargeDestructiveStatic(uint id, float delay)
    {
        ComponentCondition<DestructiveCharge>(id, delay, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<DestructiveCharge>();
        CastStart(id + 0x10, AID.DestructiveStatic, 3.9f);
        ComponentCondition<DestructiveCharge>(id + 0x20, 5.2f, comp => comp.NumCasts > 0, "Diagonal cleaves")
            .ActivateOnEnter<DestructiveStatic>()
            .DeactivateOnExit<DestructiveCharge>();
        CastEnd(id + 0x30, 2.8f, "Frontal cleave")
            .DeactivateOnExit<DestructiveStatic>();
    }

    private void BoltsFromTheBlue(uint id, float delay)
    {
        Cast(id, AID.BoltsFromTheBlue, delay, 5);
        ComponentCondition<BoltsFromTheBlue>(id + 2, 1, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<BoltsFromTheBlue>()
            .DeactivateOnExit<BoltsFromTheBlue>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Boltloop(uint id, float delay)
    {
        Cast(id, AID.Boltloop, delay, 2)
            .ActivateOnEnter<Boltloop>();
        ComponentCondition<Boltloop>(id + 0x10, 1.1f, comp => comp.NumCasts >= 2, "Expanding aoe 1");
        ComponentCondition<Boltloop>(id + 0x20, 2.0f, comp => comp.NumCasts >= 4, "Expanding aoe 2");
        ComponentCondition<Boltloop>(id + 0x30, 2.0f, comp => comp.NumCasts >= 6, "Expanding aoe 3")
            .DeactivateOnExit<Boltloop>();
    }

    private void DestructiveStrike(uint id, float delay)
    {
        Cast(id, AID.DestructiveStrike, delay, 5, "Tankbuster")
            .ActivateOnEnter<DestructiveStrike>();
        ComponentCondition<DestructiveStrike>(id + 2, 0.1f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<DestructiveStrike>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }
}
