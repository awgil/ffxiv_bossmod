namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

class DRS7States : StateMachineBuilder
{
    public DRS7States(BossModule module) : base(module)
    {
        SimplePhase(0, PhaseBeforeAdds, "Before adds")
            .ActivateOnEnter<RapidBoltsAOE>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
        SimplePhase(1, PhaseAdds, "Adds")
            .ActivateOnEnter<AddPhaseArena>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsTargetable;
        DeathPhase(2, PhaseAfterAdds)
            .ActivateOnEnter<RapidBoltsAOE>();
    }

    void PhaseBeforeAdds(uint id)
    {
        FoeSplitter(id, 8.2f);
        ViciousSwipe(id + 0x10000, 8.2f);
        Whack(id + 0x20000, 2.4f);
        ThousandTonzeSwing(id + 0x30000, 4.7f);
        // TODO: rapid bolts x2 > repeat?
        SimpleState(id + 0xFF0000, 100, "???");
    }

    void PhaseAdds(uint id)
    {
        MemoryOfTheLabyrinth(id, 2); // note: large variance
        LabyrinthineFateFatefulWords(id + 0x10000, 24.4f);
        DevastatingBolt(id + 0x20000, 2.6f);
        RendingBolt(id + 0x30000, 2.8f);
        LabyrinthineFateDevastatingBoltRendingBoltFatefulWords(id + 0x40000, 7.3f);
        RendingBoltDevastatingBolt(id + 0x50000, 4.6f);
        // TODO: repeat?
        SimpleState(id + 0xFF0000, 100, "???");
    }

    void PhaseAfterAdds(uint id)
    {
        ThunderousDischarge(id, 8.1f);
        RapidBolts(id + 0x10000, 9.4f);
        ThousandTonzeSwing(id + 0x20000, 6.1f);
        RapidBolts(id + 0x30000, 6.1f);
        RapidBolts(id + 0x40000, 2.1f);
        CrushingHoof(id + 0x50000, 2.1f);
        Whack(id + 0x60000, 2.3f);
        FoeSplitter(id + 0x70000, 11.8f);
        ViciousSwipe(id + 0x80000, 5.2f);
        Whack(id + 0x90000, 2.3f);
        // TODO: repeat?
        SimpleState(id + 0xFF0000, 100, "???");
    }

    private void FoeSplitter(uint id, float delay)
    {
        Cast(id, AID.FoeSplitter, delay, 5, "Tankbuster")
            .ActivateOnEnter<FoeSplitter>()
            .DeactivateOnExit<FoeSplitter>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void ViciousSwipe(uint id, float delay)
    {
        ComponentCondition<ViciousSwipe>(id, delay, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<ViciousSwipe>()
            .DeactivateOnExit<ViciousSwipe>();
    }

    private void ThunderousDischarge(uint id, float delay)
    {
        Cast(id, AID.ThunderousDischarge, delay, 5);
        ComponentCondition<ThunderousDischarge>(id + 2, 0.7f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<ThunderousDischarge>()
            .DeactivateOnExit<ThunderousDischarge>();
    }

    private void RapidBolts(uint id, float delay)
    {
        CastStart(id, AID.RapidBolts, delay)
            .ActivateOnEnter<RapidBoltsBait>();
        CastEnd(id + 1, 5, "Baited puddles")
            .DeactivateOnExit<RapidBoltsBait>();
    }

    private void ThousandTonzeSwing(uint id, float delay)
    {
        Cast(id, AID.ThousandTonzeSwing, delay, 6, "Center aoe")
            .ActivateOnEnter<ThousandTonzeSwing>()
            .DeactivateOnExit<ThousandTonzeSwing>();
    }

    private void CrushingHoof(uint id, float delay)
    {
        Cast(id, AID.CrushingHoof, delay, 5)
            .ActivateOnEnter<CrushingHoof>();
        ComponentCondition<CrushingHoof>(id + 2, 1, comp => comp.NumCasts > 0, "Baited proximity")
            .DeactivateOnExit<CrushingHoof>();
    }

    private void Whack(uint id, float delay)
    {
        Cast(id, AID.Whack, delay, 3, "Cone 1")
            .ActivateOnEnter<Whack>();
        ComponentCondition<Whack>(id + 2, 2, comp => comp.NumCasts >= 2, "Cone 2");
        ComponentCondition<Whack>(id + 3, 2, comp => comp.NumCasts >= 3, "Cone 3")
            .DeactivateOnExit<Whack>();
    }

    private void MemoryOfTheLabyrinth(uint id, float delay)
    {
        Cast(id, AID.MemoryOfTheLabyrinth, delay, 3);
        Condition(id + 0x10, 0.9f, () => Module.Enemies(OID.StygimolochMonk).Any(a => a.IsTargetable), "Adds appear");
    }

    private State FatefulWords(uint id, float delay)
    {
        Cast(id, AID.FatefulWords, delay, 5)
            .ActivateOnEnter<FatefulWords>();
        return ComponentCondition<FatefulWords>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Knockback/attract")
            .DeactivateOnExit<FatefulWords>();
    }

    private void LabyrinthineFateFatefulWords(uint id, float delay)
    {
        Cast(id, AID.LabyrinthineFate, delay, 3);
        FatefulWords(id + 0x10, 3.3f);
    }

    private State DevastatingBolt(uint id, float delay)
    {
        Cast(id, AID.DevastatingBolt, delay, 3);
        return ComponentCondition<DevastatingBoltInner>(id + 0x10, 4.5f, comp => comp.NumCasts > 0, "Alcoves")
            .ActivateOnEnter<DevastatingBoltOuter>()
            .ActivateOnEnter<DevastatingBoltInner>()
            .DeactivateOnExit<DevastatingBoltOuter>()
            .DeactivateOnExit<DevastatingBoltInner>();
    }

    private void RendingBolt(uint id, float delay)
    {
        Cast(id, AID.RendingBolt, delay, 3, "Puddles first")
            .ActivateOnEnter<Electrocution>();
        ComponentCondition<Electrocution>(id + 0x10, 4, comp => comp.Casters.Count == 0, "Puddles last")
            .DeactivateOnExit<Electrocution>();
    }

    private void LabyrinthineFateDevastatingBoltRendingBoltFatefulWords(uint id, float delay)
    {
        Cast(id, AID.LabyrinthineFate, delay, 3);
        DevastatingBolt(id + 0x100, 3.3f);

        Cast(id + 0x200, AID.RendingBolt, 2.8f, 3, "Puddles first")
            .ActivateOnEnter<Electrocution>();
        FatefulWords(id + 0x210, 3.3f)
            .DeactivateOnExit<Electrocution>(); // last puddles resolve ~0.7s into cast
    }

    private void RendingBoltDevastatingBolt(uint id, float delay)
    {
        Cast(id, AID.RendingBolt, delay, 3, "Puddles first")
            .ActivateOnEnter<Electrocution>();
        DevastatingBolt(id + 0x100, 1.3f)
            .DeactivateOnExit<Electrocution>(); // last puddles resolve ~0.3s before cast end
    }
}
